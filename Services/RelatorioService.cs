using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Utils.Maps;
using api.minionStorage.Services;
using static System.Net.Mime.MediaTypeNames;
using api.coleta.Utils;

namespace api.coleta.Services
{
    public class RelatorioService : ServiceBase
    {
        private readonly RelatorioRepository _relatorioRepository;
        private readonly IMinioStorage _minioStorage;
        private readonly GeoJsonRepository _geoJsonRepository;
        private readonly NutrientConfigRepository _nutrientConfigRepository;

        public RelatorioService(RelatorioRepository relatorioRepository, IMinioStorage minioStorage, GeoJsonRepository geoJsonRepository, NutrientConfigRepository nutrientConfigRepository, IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _relatorioRepository = relatorioRepository;
            _minioStorage = minioStorage;
            _geoJsonRepository = geoJsonRepository;
            _nutrientConfigRepository = nutrientConfigRepository;
        }

        /// <summary>
        /// Classifica um valor usando configuração personalizada.
        /// Retorna null se não houver configuração personalizada para o atributo.
        /// </summary>
        private object? ClassificarComConfigPersonalizada(string atributo, double valor, Dictionary<string, NutrientConfig> configsPersonalizadas)
        {
            // Tentar buscar pelo nome exato do atributo
            if (!configsPersonalizadas.TryGetValue(atributo, out var config))
            {
                // Tentar buscar pelo mapeamento de chaves curtas
                if (NutrienteConfig.NutrientKeyMapping.TryGetValue(atributo, out var fullKey))
                {
                    configsPersonalizadas.TryGetValue(fullKey, out config);
                }
            }

            if (config == null) return null;

            var configData = config.GetConfigData();
            if (configData?.Ranges == null || configData.Ranges.Count == 0) return null;

            // Processar os intervalos personalizados
            var intervalos = new List<NutrienteConfig.IntervaloInfo>();
            string? classificacao = null;
            string? cor = null;

            foreach (var range in configData.Ranges)
            {
                if (range.Count < 3) continue;

                double? min = null;
                double? max = null;
                string? rangeCor = null;
                string? rangeClassificacao = null;

                // Parse min
                if (range[0] != null)
                {
                    if (range[0] is System.Text.Json.JsonElement jsonMin)
                        min = jsonMin.ValueKind == System.Text.Json.JsonValueKind.Number ? jsonMin.GetDouble() : null;
                    else if (double.TryParse(range[0].ToString(), out double parsedMin))
                        min = parsedMin;
                }

                // Parse max
                if (range[1] != null)
                {
                    if (range[1] is System.Text.Json.JsonElement jsonMax)
                        max = jsonMax.ValueKind == System.Text.Json.JsonValueKind.Number ? jsonMax.GetDouble() : null;
                    else if (double.TryParse(range[1].ToString(), out double parsedMax))
                        max = parsedMax;
                }

                // Parse cor
                if (range[2] != null)
                {
                    rangeCor = range[2] is System.Text.Json.JsonElement jsonCor 
                        ? jsonCor.GetString() 
                        : range[2].ToString();
                }

                // Parse classificação (opcional, posição 3)
                if (range.Count > 3 && range[3] != null)
                {
                    rangeClassificacao = range[3] is System.Text.Json.JsonElement jsonClass 
                        ? jsonClass.GetString() 
                        : range[3].ToString();
                }

                intervalos.Add(new NutrienteConfig.IntervaloInfo
                {
                    Min = min,
                    Max = max,
                    Cor = rangeCor ?? "#CCCCCC",
                    Classificacao = rangeClassificacao ?? $"Faixa {min}-{max}"
                });

                // Verificar se o valor está neste intervalo
                bool dentroDoIntervalo = (min == null || valor >= min) && (max == null || valor < max);
                if (dentroDoIntervalo && classificacao == null)
                {
                    classificacao = rangeClassificacao ?? $"Faixa {min}-{max}";
                    cor = rangeCor;
                }
            }

            // Se o valor não está em nenhum intervalo personalizado, retornar null
            // para que o sistema use a configuração global do NutrienteConfig
            if (classificacao == null)
            {
                return null;
            }

            return new
            {
                valor = valor,
                classificacao = classificacao,
                cor = cor ?? "#CCCCCC",
                intervalos = intervalos,
                configPersonalizada = true
            };
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
                Relatorio map = RelatorioMapDto.MapRelatorio(arquivo);
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

        public async Task<RelatorioCompletoOutputDTO?> GetRelatorioCompletoAsync(Guid relatorioId, Guid userId)
        {
            var relatorio = await _relatorioRepository.ObterPorRelatorioId(relatorioId, userId);
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
                    
                    // Processar cada objeto individualmente (limitado aos primeiros 20 para performance)
                    int objetosProcessados = 0;
                    int maxObjetos = 20;
                    
                    foreach (var ponto in pontos.EnumerateArray())
                    {
                        if (objetosProcessados >= maxObjetos) break;
                        
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
                                var resultPersonalizado = ClassificarComConfigPersonalizada(atributo, valor, configsPersonalizadas);
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
                                objetosProcessados++;
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
                
                // Buscar geojson pelo ID da coleta
                var geojson = _geoJsonRepository.ObterPorId(coleta.GeojsonID);
                
                // Processar GeoJSON para extrair grid e pontos
                object? geoJsonProcessado = null;
                int zonas = 0;
                
                if (geojson != null && !string.IsNullOrEmpty(geojson.Pontos))
                {
                    try
                    {
                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var pontos = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(geojson.Pontos, options);
                        
                        var gridList = new List<object>();
                        var pointsList = new List<object>();
                        
                        // Extrair pontos
                        if (pontos.TryGetProperty("points", out var pointsElement))
                        {
                            foreach (var point in pointsElement.EnumerateArray())
                            {
                                if (point.TryGetProperty("geometry", out var geometry) &&
                                    geometry.TryGetProperty("coordinates", out var coordinates) &&
                                    point.TryGetProperty("properties", out var properties))
                                {
                                    pointsList.Add(new
                                    {
                                        dados = new
                                        {
                                            id = properties.TryGetProperty("id", out var id) ? id.GetInt32() : 1,
                                            hexagonId = properties.TryGetProperty("hexagonId", out var hexId) ? hexId.GetInt32() : 1,
                                            coletado = properties.TryGetProperty("coletado", out var coletado) ? coletado.GetBoolean() : false
                                        },
                                        cordenadas = new[] { coordinates[0].GetDouble(), coordinates[1].GetDouble() }
                                    });
                                }
                            }
                        }
                        
                        // Extrair grid (polígonos)
                        if (pontos.TryGetProperty("features", out var featuresElement))
                        {
                            foreach (var feature in featuresElement.EnumerateArray())
                            {
                                if (feature.TryGetProperty("geometry", out var geometry) &&
                                    geometry.TryGetProperty("type", out var geoType) &&
                                    geoType.GetString() == "Polygon" &&
                                    geometry.TryGetProperty("coordinates", out var coordinates))
                                {
                                    try
                                    {
                                        var coords = System.Text.Json.JsonSerializer.Deserialize<List<List<double[]>>>(coordinates.GetRawText(), options);
                                        if (coords != null && coords.Count > 0 && coords[0].Count > 0)
                                        {
                                            gridList.Add(new { cordenadas = coords[0] });
                                            zonas++;
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                        
                        geoJsonProcessado = new
                        {
                            grid = gridList,
                            points = pointsList
                        };
                    }
                    catch { }
                }
                
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
                NutrientesClassificados = classificacoesPorObjeto
            };
        }
    }
}
