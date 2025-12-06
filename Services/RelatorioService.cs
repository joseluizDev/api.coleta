using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Utils;
using api.coleta.Utils.Maps;
using api.minionStorage.Services;
using api.coleta.Services.Relatorio;
using RelatorioEntity = api.coleta.Models.Entidades.Relatorio;

namespace api.coleta.Services
{
    public class RelatorioService : ServiceBase
    {
        private readonly RelatorioRepository _relatorioRepository;
        private readonly IMinioStorage _minioStorage;
        private readonly NutrientConfigRepository _nutrientConfigRepository;
        private readonly NutrientClassificationService _classificationService;
        private readonly GeoJsonProcessorService _geoJsonProcessorService;
        private readonly AttributeStatisticsService _statisticsService;
        private readonly SoilIndicatorService _indicatorService;

        public RelatorioService(
            RelatorioRepository relatorioRepository,
            IMinioStorage minioStorage,
            NutrientConfigRepository nutrientConfigRepository,
            NutrientClassificationService classificationService,
            GeoJsonProcessorService geoJsonProcessorService,
            AttributeStatisticsService statisticsService,
            SoilIndicatorService indicatorService,
            IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _relatorioRepository = relatorioRepository;
            _minioStorage = minioStorage;
            _nutrientConfigRepository = nutrientConfigRepository;
            _classificationService = classificationService;
            _geoJsonProcessorService = geoJsonProcessorService;
            _statisticsService = statisticsService;
            _indicatorService = indicatorService;
        }

        public async Task<string?> SalvarRelatorio(RelatorioDTO arquivo, Guid userId)
        {
            string bucketName = "coleta";
            var file = arquivo.Arquivo;

            string fileExtension = Path.GetExtension(file.FileName).TrimStart('.');
            string contentType = file.ContentType;
            string objectName = $"{Guid.NewGuid()}.{fileExtension}";

            using var stream = file.OpenReadStream();
            string url = await _minioStorage.UploadFileAsync(bucketName, objectName, stream, contentType);

            if (url != null)
            {
                RelatorioEntity map = RelatorioMapDto.MapRelatorio(arquivo);
                map.LinkBackup = url;
                map.UsuarioId = userId;

                _relatorioRepository.Adicionar(map);
                UnitOfWork.Commit();
                return "Opa";

            }

            return url;
        }

        public async Task<RelatorioOuputDTO?> GetRelario(Guid id, Guid userId)
        {
            var relatorio = await _relatorioRepository.ObterPorId(id, userId);
            if (relatorio != null)
            {
                return RelatorioMapDto.MapRelatorio(relatorio);
            }
            return null;

        }

        public async Task<List<RelatorioOuputDTO>> ListarRelatoriosPorUploadAsync(Guid userId)
        {

            var relatorios = await _relatorioRepository.ListarRelatoriosPorUploadAsync(userId);
            if (relatorios == null || relatorios.Count == 0)
            {
                return [];
            }

            return relatorios.MapRelatorioSemJson(); // Excludes JsonRelatorio for performance
        }

        public async Task<bool> AtualizarJsonRelatorioAsync(Guid coletaId, Guid relatorioId, Guid userId, string jsonRelatorio)
        {
            var relatorio = await _relatorioRepository.ObterPorIdColetaRelatorio(coletaId, relatorioId, userId);
            if (relatorio == null)
            {
                return false;
            }

            relatorio.JsonRelatorio = jsonRelatorio;

            _relatorioRepository.Atualizar(relatorio);
            UnitOfWork.Commit();

            return true;
        }

        public async Task<RelatorioCompletoOutputDTO?> GetRelatorioCompletoAsync(Guid relatorioOuColetaId, Guid userId)
        {
            // Tentar buscar primeiro por RelatorioId, depois por ColetaId
            var relatorio = await _relatorioRepository.ObterPorRelatorioId(relatorioOuColetaId, userId);
            if (relatorio == null)
            {
                // Fallback: buscar por ColetaId (comportamento antigo)
                relatorio = await _relatorioRepository.ObterPorId(relatorioOuColetaId, userId);
            }
            
            if (relatorio == null)
            {
                return null;
            }

            var relatorioDto = RelatorioMapDto.MapRelatorio(relatorio);
            
            // Carregar configurações personalizadas do usuário ANTES de processar nutrientes
            // Assim evitamos consultar a lógica padrão/dependente para atributos que já têm config personalizada
            var configsPersonalizadasList = _nutrientConfigRepository.ListarNutrientConfigsComFallback(userId);
            var configsPersonalizadas = configsPersonalizadasList
                .Where(c => !string.IsNullOrEmpty(c.NutrientName))
                .ToDictionary(c => c.NutrientName!, c => c);
            
            // Processar classificações dos nutrientes para cada objeto (otimizado)
            var classificacoesPorObjeto = new List<object>();
            if (!string.IsNullOrEmpty(relatorio.JsonRelatorio))
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var jsonData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(relatorio.JsonRelatorio, options);
                var pontos = jsonData;
                if (pontos.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    // Processar TODOS os atributos disponíveis no JSON (sem filtro)
                    
                    // Calcular médias de CTC e Argila (necessário para dependências)
                    double sumCtc = 0, sumArgila = 0;
                    int countCtc = 0, countArgila = 0;
                    
                    foreach (var ponto in pontos.EnumerateArray())
                    {
                        if (ponto.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            if (ponto.TryGetProperty("CTC", out var ctcProp) && ctcProp.ValueKind == System.Text.Json.JsonValueKind.Number)
                            {
                                sumCtc += ctcProp.GetDouble();
                                countCtc++;
                            }
                            if (ponto.TryGetProperty("Argila", out var argilaProp) && argilaProp.ValueKind == System.Text.Json.JsonValueKind.Number)
                            {
                                sumArgila += argilaProp.GetDouble();
                                countArgila++;
                            }
                            else if (ponto.TryGetProperty("Mat. Org.", out var matOrgProp) && matOrgProp.ValueKind == System.Text.Json.JsonValueKind.Number)
                            {
                                sumArgila += matOrgProp.GetDouble();
                                countArgila++;
                            }
                        }
                    }
                    
                    double mediaCtc = countCtc > 0 ? sumCtc / countCtc : 0;
                    double mediaArgila = countArgila > 0 ? sumArgila / countArgila : 0;
                    
                    // Processar cada objeto individualmente
                    foreach (var ponto in pontos.EnumerateArray())
                    {
                        if (ponto.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            int? objetoId = null;
                            if (ponto.TryGetProperty("ID", out var idProp) && idProp.ValueKind == System.Text.Json.JsonValueKind.Number)
                            {
                                objetoId = idProp.GetInt32();
                            }
                            
                            if (!objetoId.HasValue) continue;
                            
                            var nutrientesClassificados = new Dictionary<string, object>();
                            
                            // Processar TODOS os atributos do objeto JSON
                            foreach (var propriedade in ponto.EnumerateObject())
                            {
                                string atributo = propriedade.Name;
                                var prop = propriedade.Value;
                                
                                // Pular ID e prof.
                                if (atributo == "ID" || atributo == "prof.") continue;
                                
                                if (prop.ValueKind != System.Text.Json.JsonValueKind.Number) continue;
                                if (!prop.TryGetDouble(out double valor)) continue;
                                
                                // PRIMEIRO: Verificar se existe configuração personalizada para este atributo
                                var resultPersonalizado = _classificationService.ClassificarComConfigPersonalizada(atributo, valor, configsPersonalizadas);
                                if (resultPersonalizado != null)
                                {
                                    // Usar configuração personalizada - ignora lógica padrão/dependente
                                    nutrientesClassificados[atributo] = resultPersonalizado;
                                    continue; // Pular para o próximo atributo
                                }
                                
                                // SEGUNDO: Usar lógica padrão (config default ou dependente)
                                // Determinar referência e valor de referência
                                string? referencia = null;
                                double valorReferencia = 0;
                                
                                if (atributo.Contains("Ca") || atributo.Contains("Mg") || atributo.Contains("K") || 
                                    atributo.Contains("Al") || atributo.Contains("H+Al") || atributo == "Mg/CTC" || atributo == "Ca + Mg" || atributo == "Ca+Mg")
                                {
                                    referencia = "CTC";
                                    valorReferencia = mediaCtc;
                                }
                                else if ((atributo.Contains("P") || atributo.Contains("Fósforo")) && !atributo.Contains("Resina"))
                                {
                                    referencia = "Argila";
                                    valorReferencia = mediaArgila;
                                }
                                
                                var result = NutrienteConfig.GetNutrientClassification(atributo, valor, valorReferencia, referencia);
                                if (result != null)
                                {
                                    nutrientesClassificados[atributo] = new
                                    {
                                        valor = valor,
                                        classificacao = result.Classificacao,
                                        cor = result.Cor,
                                        intervalos = result.Intervalos // Retornar todos os intervalos possíveis
                                    };
                                }
                            }
                            
                            if (nutrientesClassificados.Count > 0)
                            {
                                classificacoesPorObjeto.Add(new
                                {
                                    id = objetoId.Value,
                                    nutrientes = nutrientesClassificados
                                });
                            }
                        }
                    }
                }
            }
            
            // Montar dados da coleta com geojson processado
            ColetaDadosDTO? dadosColeta = null;
            
            if (relatorio.Coleta != null)
            {
                var coleta = relatorio.Coleta;
                var talhaoJson = coleta.Talhao;
                
                // Processar GeoJSON para extrair grid e pontos usando o serviço especializado
                var geoJsonProcessado = _geoJsonProcessorService.ProcessarGeoJson(coleta.GeojsonID, out int zonas);
                
                dadosColeta = new ColetaDadosDTO
                {
                    ColetaId = coleta.Id,
                    Geojson = geoJsonProcessado,
                    Talhao = talhaoJson != null ? new
                    {
                        id = talhaoJson.Id,
                        nome = talhaoJson.Nome,
                        area = talhaoJson.Area,
                        observacao = talhaoJson.Observacao,
                        coordenadas = talhaoJson.Coordenadas
                    } : null,
                    UsuarioResp = coleta.UsuarioResp != null ? new
                    {
                        id = coleta.UsuarioResp.Id,
                        nomeCompleto = coleta.UsuarioResp.NomeCompleto,
                        cpf = coleta.UsuarioResp.CPF,
                        email = coleta.UsuarioResp.Email,
                        telefone = coleta.UsuarioResp.Telefone
                    } : null,
                    GeoJsonID = coleta.GeojsonID,
                    UsuarioRespID = coleta.UsuarioRespID,
                    Zonas = zonas,
                    AreaHa = !string.IsNullOrEmpty(talhaoJson?.Area) && decimal.TryParse(talhaoJson.Area, out var area) ? area : (decimal?)null
                };
            }
            
            // Calcular estatísticas de todos os atributos para gráficos mobile
            var estatisticasAtributos = _statisticsService.CalcularEstatisticasAtributos(relatorio.JsonRelatorio, configsPersonalizadas);
            
            return new RelatorioCompletoOutputDTO
            {
                Id = relatorioDto.Id,
                ColetaId = relatorioDto.ColetaId,
                LinkBackup = relatorioDto.LinkBackup,
                DataInclusao = relatorioDto.DataInclusao,
                NomeColeta = relatorioDto.NomeColeta,
                Talhao = relatorioDto.Talhao,
                TipoColeta = relatorioDto.TipoColeta,
                Fazenda = relatorioDto.Fazenda,
                NomeCliente = relatorioDto.NomeCliente,
                Safra = relatorioDto.Safra,
                Funcionario = relatorioDto.Funcionario,
                Observacao = relatorioDto.Observacao,
                Profundidade = relatorioDto.Profundidade,
                TiposAnalise = relatorioDto.TiposAnalise,
                JsonRelatorio = relatorioDto.JsonRelatorio,
                IsRelatorio = relatorioDto.IsRelatorio,
                DadosColeta = dadosColeta,
                NutrientesClassificados = classificacoesPorObjeto,
                EstatisticasAtributos = estatisticasAtributos
            };
        }

        /// <summary>
        /// Obtém o resumo dos indicadores para gráficos do talhão
        /// Retorna indicadores de acidez, saturação, equilíbrio de bases e participação na CTC
        /// </summary>
        public async Task<ResumoTalhaoDTO?> GetResumoAcidezSoloAsync(Guid relatorioOuColetaId, Guid userId)
        {
            // Tentar buscar primeiro por RelatorioId, depois por ColetaId
            var relatorio = await _relatorioRepository.ObterPorRelatorioId(relatorioOuColetaId, userId);
            if (relatorio == null)
            {
                // Fallback: buscar por ColetaId (comportamento antigo)
                relatorio = await _relatorioRepository.ObterPorId(relatorioOuColetaId, userId);
            }
            
            if (relatorio == null || string.IsNullOrEmpty(relatorio.JsonRelatorio))
            {
                return null;
            }

            var resumo = new ResumoTalhaoDTO
            {
                RelatorioId = relatorio.Id,
                ColetaId = relatorio.ColetaId,
                NomeTalhao = relatorio.Coleta?.Talhao?.Nome ?? string.Empty,
                NomeFazenda = relatorio.Coleta?.Fazenda?.Nome ?? string.Empty,
                NomeSafra = relatorio.Coleta?.Safra?.Observacao ?? string.Empty
            };

            // Carregar configurações personalizadas do usuário
            var configsPersonalizadasList = _nutrientConfigRepository.ListarNutrientConfigsComFallback(userId);
            var configsPersonalizadas = configsPersonalizadasList
                .Where(c => !string.IsNullOrEmpty(c.NutrientName))
                .ToDictionary(c => c.NutrientName!, c => c);

            // Processar o JSON para calcular médias
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var jsonData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(relatorio.JsonRelatorio, options);
            
            if (jsonData.ValueKind != System.Text.Json.JsonValueKind.Array)
            {
                return resumo;
            }

            // Dicionário para armazenar todos os valores de cada atributo
            var valoresPorAtributo = new Dictionary<string, List<double>>();
            
            // Variáveis para acumular valores dos indicadores principais
            double sumPH = 0, sumM = 0, sumV = 0;
            int countPH = 0, countM = 0, countV = 0;
            double sumCaMg = 0, sumCaK = 0, sumMgK = 0;
            int countCaMg = 0, countCaK = 0, countMgK = 0;
            double sumCaCTC = 0, sumMgCTC = 0, sumKCTC = 0, sumHAlCTC = 0, sumAlCTC = 0;
            int countCaCTC = 0, countMgCTC = 0, countKCTC = 0, countHAlCTC = 0, countAlCTC = 0;
            
            // Variáveis para Macronutrientes
            double sumCa = 0, sumMg = 0, sumK = 0, sumCaMgMacro = 0, sumHAl = 0, sumAl = 0;
            int countCa = 0, countMg = 0, countK = 0, countCaMgMacro = 0, countHAl = 0, countAl = 0;
            double sumP = 0, sumCTCMacro = 0, sumSB = 0, sumMO = 0;
            int countP = 0, countCTCMacro = 0, countSB = 0, countMO = 0;
            
            // Variáveis para Micronutrientes
            double sumFe = 0, sumCu = 0, sumMn = 0, sumB = 0, sumZn = 0, sumS = 0;
            int countFe = 0, countCu = 0, countMn = 0, countB = 0, countZn = 0, countS = 0;
            
            // Variáveis para calcular CTC e Argila médias (referências para classificação)
            double sumCTCRef = 0, sumArgilaRef = 0;
            int countCTCRef = 0, countArgilaRef = 0;
            var chavesCTCRef = new[] { "CTC", "CTC a pH 7 (T)", "CTC (T)" };
            var chavesArgilaRef = new[] { "Argila", "Argila (%)", "Mat. Org.", "Matéria Orgânica" };
            
            int totalPontos = 0;

            // Chaves possíveis para cada indicador (ordem de prioridade)
            var chavesPH = new[] { "pH (CaCl2)", "pH CaCl2", "pH", "pH (H2O)", "pH H2O" };
            var chavesM = new[] { "m%", "m", "Saturação por Alumínio", "saturação por Alumínio - m% (Al)" };
            var chavesV = new[] { "V%", "V", "Saturação por bases", "saturação por bases  - V%" };
            var chavesCaMg = new[] { "Ca/Mg", "Relação Ca/Mg", "Relação de Ca/Mg" };
            var chavesCaK = new[] { "Ca/K", "Relação Ca/K", "Relação de Ca/K" };
            var chavesMgK = new[] { "Mg/K", "Relação Mg/K", "Relação de Mg/K" };
            var chavesCaCTC = new[] { "Ca/CTC (%)", "Ca/CTC", "Participação de Ca/CTC (%)" };
            var chavesMgCTC = new[] { "Mg/CTC (%)", "Mg/CTC", "Participação de Mg/CTC (%)" };
            var chavesKCTC = new[] { "K/CTC (%)", "K/CTC", "Participação de K/CTC (%)" };
            var chavesHAlCTC = new[] { "H+Al/CTC (%)", "H+Al/CTC", "Participação de H+Al/CTC (%)" };
            var chavesAlCTC = new[] { "Al/CTC (%)", "Al/CTC", "Participação de Al/CTC (%)" };
            
            // Chaves para Macronutrientes
            var chavesCa = new[] { "Ca", "Cálcio", "Cálcio - Ca (cmolc/dm³)" };
            var chavesMg = new[] { "Mg", "Magnésio", "Magnésio - Mg (cmolc/dm³)" };
            var chavesK = new[] { "K", "Potássio", "Potássio - K (cmolc/dm³)" };
            var chavesCaMgMacro = new[] { "Ca+Mg", "Ca + Mg", "Ca+Mg (cmolc/dm³)" };
            var chavesHAl = new[] { "H+Al", "H + Al", "Acidez Potencial (H+Al) (cmolc/dm³)" };
            var chavesAl = new[] { "Al", "Alumínio", "Alumínio - Al (cmolc/dm³)" };
            var chavesP = new[] { "PMELICH 1", "P", "Fósforo", "Fósforo - P Mehlich-1 (mg/dm³)" };
            var chavesCTCMacro = new[] { "CTC", "CTC a pH 7 (T)", "CTC (T)" };
            var chavesSB = new[] { "SB", "Soma de bases", "SB (cmolc/dm³)" };
            var chavesMO = new[] { "Mat. Org.", "MO", "Matéria Orgânica", "Matéria Orgânica - MO (g/dm³)" };
            
            // Chaves para Micronutrientes
            var chavesFe = new[] { "Fe", "Ferro", "Ferro - Fe (mg/dm³)" };
            var chavesCu = new[] { "Cu", "Cobre", "Cobre - Cu (mg/dm³)" };
            var chavesMn = new[] { "Mn", "Manganês", "Manganês - Mn (mg/dm³)" };
            var chavesB = new[] { "B", "Boro", "Boro - B (mg/dm³)" };
            var chavesZn = new[] { "Zn", "Zinco", "Zinco - Zn (mg/dm³)" };
            var chavesS = new[] { "S", "Enxofre", "Enxofre - S (mg/dm³)", "S-SO4" };

            // Atributos a ignorar na coleta de estatísticas
            var atributosIgnorados = new HashSet<string> { "ID", "id", "prof.", "profundidade", "Profundidade" };

            foreach (var ponto in jsonData.EnumerateArray())
            {
                if (ponto.ValueKind != System.Text.Json.JsonValueKind.Object) continue;
                totalPontos++;

                // Coletar TODOS os valores numéricos de cada atributo para estatísticas
                foreach (var propriedade in ponto.EnumerateObject())
                {
                    string nomeAtributo = propriedade.Name;
                    
                    // Ignorar atributos não relevantes
                    if (atributosIgnorados.Contains(nomeAtributo)) continue;
                    
                    if (propriedade.Value.ValueKind == System.Text.Json.JsonValueKind.Number &&
                        propriedade.Value.TryGetDouble(out double valor))
                    {
                        if (!valoresPorAtributo.ContainsKey(nomeAtributo))
                        {
                            valoresPorAtributo[nomeAtributo] = new List<double>();
                        }
                        valoresPorAtributo[nomeAtributo].Add(valor);
                        
                        // Acumular CTC para referência
                        if (chavesCTCRef.Contains(nomeAtributo))
                        {
                            sumCTCRef += valor;
                            countCTCRef++;
                        }
                        // Acumular Argila para referência
                        if (chavesArgilaRef.Contains(nomeAtributo))
                        {
                            sumArgilaRef += valor;
                            countArgilaRef++;
                        }
                    }
                }

                // Buscar indicadores principais
                _indicatorService.BuscarEAcumularValor(ponto, chavesPH, ref sumPH, ref countPH);
                _indicatorService.BuscarEAcumularValor(ponto, chavesM, ref sumM, ref countM);
                _indicatorService.BuscarEAcumularValor(ponto, chavesV, ref sumV, ref countV);
                _indicatorService.BuscarEAcumularValor(ponto, chavesCaMg, ref sumCaMg, ref countCaMg);
                _indicatorService.BuscarEAcumularValor(ponto, chavesCaK, ref sumCaK, ref countCaK);
                _indicatorService.BuscarEAcumularValor(ponto, chavesMgK, ref sumMgK, ref countMgK);
                _indicatorService.BuscarEAcumularValor(ponto, chavesCaCTC, ref sumCaCTC, ref countCaCTC);
                _indicatorService.BuscarEAcumularValor(ponto, chavesMgCTC, ref sumMgCTC, ref countMgCTC);
                _indicatorService.BuscarEAcumularValor(ponto, chavesKCTC, ref sumKCTC, ref countKCTC);
                _indicatorService.BuscarEAcumularValor(ponto, chavesHAlCTC, ref sumHAlCTC, ref countHAlCTC);
                _indicatorService.BuscarEAcumularValor(ponto, chavesAlCTC, ref sumAlCTC, ref countAlCTC);

                // Macronutrientes
                _indicatorService.BuscarEAcumularValor(ponto, chavesCa, ref sumCa, ref countCa);
                _indicatorService.BuscarEAcumularValor(ponto, chavesMg, ref sumMg, ref countMg);
                _indicatorService.BuscarEAcumularValor(ponto, chavesK, ref sumK, ref countK);
                _indicatorService.BuscarEAcumularValor(ponto, chavesCaMgMacro, ref sumCaMgMacro, ref countCaMgMacro);
                _indicatorService.BuscarEAcumularValor(ponto, chavesHAl, ref sumHAl, ref countHAl);
                _indicatorService.BuscarEAcumularValor(ponto, chavesAl, ref sumAl, ref countAl);
                _indicatorService.BuscarEAcumularValor(ponto, chavesP, ref sumP, ref countP);
                _indicatorService.BuscarEAcumularValor(ponto, chavesCTCMacro, ref sumCTCMacro, ref countCTCMacro);
                _indicatorService.BuscarEAcumularValor(ponto, chavesSB, ref sumSB, ref countSB);
                _indicatorService.BuscarEAcumularValor(ponto, chavesMO, ref sumMO, ref countMO);

                // Micronutrientes
                _indicatorService.BuscarEAcumularValor(ponto, chavesFe, ref sumFe, ref countFe);
                _indicatorService.BuscarEAcumularValor(ponto, chavesCu, ref sumCu, ref countCu);
                _indicatorService.BuscarEAcumularValor(ponto, chavesMn, ref sumMn, ref countMn);
                _indicatorService.BuscarEAcumularValor(ponto, chavesB, ref sumB, ref countB);
                _indicatorService.BuscarEAcumularValor(ponto, chavesZn, ref sumZn, ref countZn);
                _indicatorService.BuscarEAcumularValor(ponto, chavesS, ref sumS, ref countS);
            }

            resumo.TotalPontos = totalPontos;

            // Calcular médias de referência
            double mediaCTCRef = countCTCRef > 0 ? sumCTCRef / countCTCRef : 0;
            double mediaArgilaRef = countArgilaRef > 0 ? sumArgilaRef / countArgilaRef : 0;

            // Calcular estatísticas para cada atributo
            foreach (var kvp in valoresPorAtributo)
            {
                var nomeAtributo = kvp.Key;
                var valores = kvp.Value;

                if (valores.Count == 0) continue;

                var estatistica = _statisticsService.CalcularEstatisticaAtributo(nomeAtributo, valores, configsPersonalizadas, mediaCTCRef, mediaArgilaRef);
                resumo.EstatisticasAtributos[nomeAtributo] = estatistica;
            }

            // Calcular indicadores principais
            // 1. Acidez - pH
            resumo.IndicadoresGraficos.Acidez.pH = _indicatorService.CalcularIndicador("pH", sumPH, countPH, configsPersonalizadas);

            // 2. Saturação
            resumo.IndicadoresGraficos.Saturacao.SaturacaoAluminio = _indicatorService.CalcularIndicador("m%", sumM, countM, configsPersonalizadas);
            resumo.IndicadoresGraficos.Saturacao.SaturacaoBases = _indicatorService.CalcularIndicador("V%", sumV, countV, configsPersonalizadas);

            // 3. Equilíbrio de Bases
            resumo.IndicadoresGraficos.EquilibrioBases.CaMg = _indicatorService.CalcularIndicador("Ca/Mg", sumCaMg, countCaMg, configsPersonalizadas);
            resumo.IndicadoresGraficos.EquilibrioBases.CaK = _indicatorService.CalcularIndicador("Ca/K", sumCaK, countCaK, configsPersonalizadas);
            resumo.IndicadoresGraficos.EquilibrioBases.MgK = _indicatorService.CalcularIndicador("Mg/K", sumMgK, countMgK, configsPersonalizadas);

            // 4. Participação na CTC
            resumo.IndicadoresGraficos.ParticipacaoCTC.CaCTC = _indicatorService.CalcularIndicador("Ca/CTC (%)", sumCaCTC, countCaCTC, configsPersonalizadas);
            resumo.IndicadoresGraficos.ParticipacaoCTC.MgCTC = _indicatorService.CalcularIndicador("Mg/CTC (%)", sumMgCTC, countMgCTC, configsPersonalizadas);
            resumo.IndicadoresGraficos.ParticipacaoCTC.KCTC = _indicatorService.CalcularIndicador("K/CTC (%)", sumKCTC, countKCTC, configsPersonalizadas);
            resumo.IndicadoresGraficos.ParticipacaoCTC.HAlCTC = _indicatorService.CalcularIndicador("H+Al/CTC (%)", sumHAlCTC, countHAlCTC, configsPersonalizadas);
            resumo.IndicadoresGraficos.ParticipacaoCTC.AlCTC = _indicatorService.CalcularIndicador("Al/CTC (%)", sumAlCTC, countAlCTC, configsPersonalizadas);

            // 5. Macronutrientes (com referência CTC ou Argila)
            resumo.IndicadoresGraficos.Macronutrientes.Ca = _indicatorService.CalcularIndicadorComReferencia("Ca", sumCa, countCa, configsPersonalizadas, mediaCTCRef, "CTC");
            resumo.IndicadoresGraficos.Macronutrientes.Mg = _indicatorService.CalcularIndicadorComReferencia("Mg", sumMg, countMg, configsPersonalizadas, mediaCTCRef, "CTC");
            resumo.IndicadoresGraficos.Macronutrientes.K = _indicatorService.CalcularIndicadorComReferencia("K", sumK, countK, configsPersonalizadas, mediaCTCRef, "CTC");
            resumo.IndicadoresGraficos.Macronutrientes.CaMg = _indicatorService.CalcularIndicadorComReferencia("Ca+Mg", sumCaMgMacro, countCaMgMacro, configsPersonalizadas, mediaCTCRef, "CTC");
            resumo.IndicadoresGraficos.Macronutrientes.HAl = _indicatorService.CalcularIndicadorComReferencia("H+Al", sumHAl, countHAl, configsPersonalizadas, mediaCTCRef, "CTC");
            resumo.IndicadoresGraficos.Macronutrientes.Al = _indicatorService.CalcularIndicadorComReferencia("Al", sumAl, countAl, configsPersonalizadas, mediaCTCRef, "CTC");
            resumo.IndicadoresGraficos.Macronutrientes.Fosforo = _indicatorService.CalcularIndicadorComReferencia("PMELICH 1", sumP, countP, configsPersonalizadas, mediaArgilaRef, "Argila");
            resumo.IndicadoresGraficos.Macronutrientes.CTC = _indicatorService.CalcularIndicador("CTC", sumCTCMacro, countCTCMacro, configsPersonalizadas);
            resumo.IndicadoresGraficos.Macronutrientes.SB = _indicatorService.CalcularIndicador("SB", sumSB, countSB, configsPersonalizadas);
            resumo.IndicadoresGraficos.Macronutrientes.MateriaOrganica = _indicatorService.CalcularIndicador("Mat. Org.", sumMO, countMO, configsPersonalizadas);

            // 6. Micronutrientes
            resumo.IndicadoresGraficos.Micronutrientes.Fe = _indicatorService.CalcularIndicador("Fe", sumFe, countFe, configsPersonalizadas);
            resumo.IndicadoresGraficos.Micronutrientes.Cu = _indicatorService.CalcularIndicador("Cu", sumCu, countCu, configsPersonalizadas);
            resumo.IndicadoresGraficos.Micronutrientes.Mn = _indicatorService.CalcularIndicador("Mn", sumMn, countMn, configsPersonalizadas);
            resumo.IndicadoresGraficos.Micronutrientes.B = _indicatorService.CalcularIndicador("B", sumB, countB, configsPersonalizadas);
            resumo.IndicadoresGraficos.Micronutrientes.Zn = _indicatorService.CalcularIndicador("Zn", sumZn, countZn, configsPersonalizadas);
            resumo.IndicadoresGraficos.Micronutrientes.S = _indicatorService.CalcularIndicador("S", sumS, countS, configsPersonalizadas);

            // 7. Resumo Visual (para o gráfico de barras horizontal - Interpretação Visual da Análise de Solo)
            resumo.IndicadoresGraficos.ResumoVisual.M = _indicatorService.CalcularIndicador("m%", sumM, countM, configsPersonalizadas);
            resumo.IndicadoresGraficos.ResumoVisual.Al = _indicatorService.CalcularIndicadorComReferencia("Al", sumAl, countAl, configsPersonalizadas, mediaCTCRef, "CTC");
            resumo.IndicadoresGraficos.ResumoVisual.V = _indicatorService.CalcularIndicador("V%", sumV, countV, configsPersonalizadas);
            resumo.IndicadoresGraficos.ResumoVisual.CTC = _indicatorService.CalcularIndicador("CTC", sumCTCMacro, countCTCMacro, configsPersonalizadas);
            resumo.IndicadoresGraficos.ResumoVisual.Fe = _indicatorService.CalcularIndicador("Fe", sumFe, countFe, configsPersonalizadas);
            resumo.IndicadoresGraficos.ResumoVisual.Cu = _indicatorService.CalcularIndicador("Cu", sumCu, countCu, configsPersonalizadas);
            resumo.IndicadoresGraficos.ResumoVisual.Mn = _indicatorService.CalcularIndicador("Mn", sumMn, countMn, configsPersonalizadas);
            resumo.IndicadoresGraficos.ResumoVisual.B = _indicatorService.CalcularIndicador("B", sumB, countB, configsPersonalizadas);
            resumo.IndicadoresGraficos.ResumoVisual.Zn = _indicatorService.CalcularIndicador("Zn", sumZn, countZn, configsPersonalizadas);
            resumo.IndicadoresGraficos.ResumoVisual.S = _indicatorService.CalcularIndicador("S", sumS, countS, configsPersonalizadas);
            resumo.IndicadoresGraficos.ResumoVisual.Mg = _indicatorService.CalcularIndicadorComReferencia("Mg", sumMg, countMg, configsPersonalizadas, mediaCTCRef, "CTC");
            resumo.IndicadoresGraficos.ResumoVisual.Ca = _indicatorService.CalcularIndicadorComReferencia("Ca", sumCa, countCa, configsPersonalizadas, mediaCTCRef, "CTC");
            resumo.IndicadoresGraficos.ResumoVisual.K = _indicatorService.CalcularIndicadorComReferencia("K", sumK, countK, configsPersonalizadas, mediaCTCRef, "CTC");

            return resumo;
        }
    }
}
