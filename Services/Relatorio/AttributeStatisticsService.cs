using System.Text.Json;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;

namespace api.coleta.Services.Relatorio
{
    /// <summary>
    /// Serviço responsável pelo cálculo de estatísticas de atributos do solo.
    /// Calcula min, max, média e classificação para cada atributo.
    /// </summary>
    public class AttributeStatisticsService
    {
        private readonly NutrientClassificationService _classificationService;

        /// <summary>
        /// Atributos que devem ser ignorados nas estatísticas
        /// </summary>
        private static readonly HashSet<string> AtributosIgnorados = new()
        {
            "ID", "id", "prof.", "profundidade", "Profundidade"
        };

        /// <summary>
        /// Chaves possíveis para CTC (referência)
        /// </summary>
        private static readonly string[] ChavesCTC = { "CTC", "CTC a pH 7 (T)", "CTC (T)" };

        /// <summary>
        /// Chaves possíveis para Argila (referência)
        /// </summary>
        private static readonly string[] ChavesArgila = { "Argila", "Argila (%)", "Mat. Org.", "Matéria Orgânica" };

        /// <summary>
        /// Atributos que dependem de CTC para classificação
        /// </summary>
        private static readonly string[] AtributosCTC =
        {
            "Ca", "Mg", "K", "Al", "H+Al", "Ca+Mg", "Ca + Mg",
            "Cálcio", "Magnésio", "Potássio", "Alumínio"
        };

        /// <summary>
        /// Atributos que dependem de Argila para classificação
        /// </summary>
        private static readonly string[] AtributosArgila =
        {
            "PMELICH 1", "P Mehlich", "Fósforo", "P"
        };

        public AttributeStatisticsService(NutrientClassificationService classificationService)
        {
            _classificationService = classificationService;
        }

        /// <summary>
        /// Calcula estatísticas completas de um atributo (para histograma e análise estatística)
        /// </summary>
        public EstatisticaAtributoDTO CalcularEstatisticaAtributo(
            string nomeAtributo,
            List<double> valores,
            Dictionary<string, NutrientConfig> configsPersonalizadas,
            double mediaCTC,
            double mediaArgila)
        {
            var minimo = valores.Min();
            var maximo = valores.Max();
            var media = valores.Average();

            // Obter classificação e cor baseada na média
            var (classificacao, cor, intervaloAdequado) = ObterClassificacao(
                nomeAtributo, media, configsPersonalizadas, mediaCTC, mediaArgila);

            return new EstatisticaAtributoDTO
            {
                Nome = nomeAtributo,
                Valores = valores,
                Minimo = Math.Round(minimo, 2),
                Media = Math.Round(media, 2),
                Maximo = Math.Round(maximo, 2),
                Classificacao = classificacao,
                Cor = cor,
                QuantidadePontos = valores.Count,
                IntervaloAdequado = intervaloAdequado
            };
        }

        /// <summary>
        /// Calcula estatísticas de todos os atributos do JSON do relatório
        /// </summary>
        public Dictionary<string, EstatisticaAtributoDTO> CalcularEstatisticasAtributos(
            string? jsonRelatorio,
            Dictionary<string, NutrientConfig> configsPersonalizadas)
        {
            var resultado = new Dictionary<string, EstatisticaAtributoDTO>();

            if (string.IsNullOrEmpty(jsonRelatorio))
            {
                return resultado;
            }

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var jsonData = JsonSerializer.Deserialize<JsonElement>(jsonRelatorio, options);

                if (jsonData.ValueKind != JsonValueKind.Array)
                {
                    return resultado;
                }

                // Coletar valores por atributo e calcular referências
                var valoresPorAtributo = new Dictionary<string, List<double>>();
                var (mediaCTC, mediaArgila) = ColetarValoresECalcularReferencias(
                    jsonData, valoresPorAtributo);

                // Calcular estatísticas para cada atributo
                foreach (var kvp in valoresPorAtributo)
                {
                    if (kvp.Value.Count == 0) continue;

                    resultado[kvp.Key] = CalcularEstatisticaAtributo(
                        kvp.Key, kvp.Value, configsPersonalizadas, mediaCTC, mediaArgila);
                }
            }
            catch
            {
                // Silently ignore parsing errors
            }

            return resultado;
        }

        /// <summary>
        /// Coleta todos os valores numéricos do JSON e calcula médias de CTC e Argila
        /// </summary>
        private (double mediaCTC, double mediaArgila) ColetarValoresECalcularReferencias(
            JsonElement jsonData,
            Dictionary<string, List<double>> valoresPorAtributo)
        {
            double sumCTC = 0, sumArgila = 0;
            int countCTC = 0, countArgila = 0;

            foreach (var ponto in jsonData.EnumerateArray())
            {
                if (ponto.ValueKind != JsonValueKind.Object) continue;

                foreach (var propriedade in ponto.EnumerateObject())
                {
                    string nomeAtributo = propriedade.Name;

                    if (AtributosIgnorados.Contains(nomeAtributo)) continue;

                    if (propriedade.Value.ValueKind == JsonValueKind.Number &&
                        propriedade.Value.TryGetDouble(out double valor))
                    {
                        if (!valoresPorAtributo.ContainsKey(nomeAtributo))
                        {
                            valoresPorAtributo[nomeAtributo] = new List<double>();
                        }
                        valoresPorAtributo[nomeAtributo].Add(valor);

                        // Acumular CTC
                        if (ChavesCTC.Contains(nomeAtributo))
                        {
                            sumCTC += valor;
                            countCTC++;
                        }

                        // Acumular Argila
                        if (ChavesArgila.Contains(nomeAtributo))
                        {
                            sumArgila += valor;
                            countArgila++;
                        }
                    }
                }
            }

            double mediaCTC = countCTC > 0 ? sumCTC / countCTC : 0;
            double mediaArgila = countArgila > 0 ? sumArgila / countArgila : 0;

            return (mediaCTC, mediaArgila);
        }

        /// <summary>
        /// Obtém classificação, cor e intervalo adequado para um valor médio
        /// </summary>
        private (string classificacao, string cor, IntervaloClassificacaoDTO? intervaloAdequado) ObterClassificacao(
            string nomeAtributo,
            double media,
            Dictionary<string, NutrientConfig> configsPersonalizadas,
            double mediaCTC,
            double mediaArgila)
        {
            string classificacao = "Não classificado";
            string cor = "#CCCCCC";
            IntervaloClassificacaoDTO? intervaloAdequado = null;

            // Primeiro: tentar classificação personalizada
            var resultPersonalizado = _classificationService.ClassificarComConfigPersonalizada(
                nomeAtributo, media, configsPersonalizadas);

            if (resultPersonalizado != null)
            {
                var resultObj = (dynamic)resultPersonalizado;
                classificacao = resultObj.classificacao?.ToString() ?? "Não classificado";
                cor = resultObj.cor?.ToString() ?? "#CCCCCC";

                // Tentar obter intervalo adequado da config personalizada
                intervaloAdequado = ExtrairIntervaloAdequadoPersonalizado(nomeAtributo, configsPersonalizadas);
            }
            else
            {
                // Segundo: usar classificação padrão (NutrienteConfig)
                var (referencia, valorReferencia) = DeterminarReferencia(nomeAtributo, mediaCTC, mediaArgila);

                var classResult = NutrienteConfig.GetNutrientClassification(
                    nomeAtributo, media, valorReferencia, referencia);

                if (classResult != null && !string.IsNullOrEmpty(classResult.Classificacao))
                {
                    classificacao = classResult.Classificacao;
                    cor = classResult.Cor ?? "#CCCCCC";

                    // Buscar intervalo adequado nos intervalos retornados
                    var intervalo = classResult.Intervalos?.FirstOrDefault(i =>
                        i.Classificacao == "Adequado");

                    if (intervalo != null)
                    {
                        intervaloAdequado = new IntervaloClassificacaoDTO
                        {
                            Min = intervalo.Min,
                            Max = intervalo.Max
                        };
                    }
                }
            }

            return (classificacao, cor, intervaloAdequado);
        }

        /// <summary>
        /// Determina o tipo de referência (CTC ou Argila) e valor para um atributo
        /// </summary>
        public (string? referencia, double valorReferencia) DeterminarReferencia(
            string atributo,
            double mediaCTC,
            double mediaArgila)
        {
            // Atributos que dependem de CTC
            if (AtributosCTC.Any(a => atributo.Contains(a, StringComparison.OrdinalIgnoreCase)))
            {
                return ("CTC", mediaCTC);
            }

            // Atributos que dependem de Argila
            if (AtributosArgila.Any(a => atributo.Contains(a, StringComparison.OrdinalIgnoreCase)))
            {
                return ("Argila", mediaArgila);
            }

            return (null, 0);
        }

        /// <summary>
        /// Extrai intervalo adequado de uma configuração personalizada
        /// </summary>
        private IntervaloClassificacaoDTO? ExtrairIntervaloAdequadoPersonalizado(
            string nomeAtributo,
            Dictionary<string, NutrientConfig> configsPersonalizadas)
        {
            if (!configsPersonalizadas.TryGetValue(nomeAtributo, out var config))
            {
                return null;
            }

            var configData = config.GetConfigData();
            if (configData?.Ranges == null) return null;

            foreach (var range in configData.Ranges)
            {
                if (range.Count >= 4)
                {
                    var classif = range[3]?.ToString();
                    if (classif == "Adequado")
                    {
                        double.TryParse(range[0]?.ToString(), out double min);
                        double.TryParse(range[1]?.ToString(), out double max);

                        return new IntervaloClassificacaoDTO
                        {
                            Min = min,
                            Max = max > 0 ? max : null
                        };
                    }
                }
            }

            return null;
        }
    }
}
