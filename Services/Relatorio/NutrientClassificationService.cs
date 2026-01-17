using api.coleta.Models.Entidades;

namespace api.coleta.Services.Relatorio
{
    /// <summary>
    /// Serviço responsável pela classificação de nutrientes usando configurações personalizadas.
    /// Extraído do RelatorioService para melhor separação de responsabilidades.
    /// </summary>
    public class NutrientClassificationService
    {
        /// <summary>
        /// Classifica um valor usando configuração personalizada.
        /// Retorna null se não houver configuração personalizada para o atributo.
        /// </summary>
        /// <param name="atributo">Nome do atributo/nutriente</param>
        /// <param name="valor">Valor a ser classificado</param>
        /// <param name="configsPersonalizadas">Dicionário de configurações personalizadas do usuário</param>
        /// <returns>Objeto com classificação, cor e intervalos, ou null se não houver config personalizada</returns>
        public object? ClassificarComConfigPersonalizada(
            string atributo,
            double valor,
            Dictionary<string, NutrientConfig> configsPersonalizadas)
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
            var intervalos = ExtrairIntervalos(configData);
            var (classificacao, cor) = EncontrarClassificacao(valor, configData);

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

        /// <summary>
        /// Extrai a lista de intervalos de uma configuração de nutriente
        /// </summary>
        private List<NutrienteConfig.IntervaloInfo> ExtrairIntervalos(NutrientConfigData configData)
        {
            var intervalos = new List<NutrienteConfig.IntervaloInfo>();

            if (configData?.Ranges == null) return intervalos;

            foreach (var range in configData.Ranges)
            {
                if (range.Count < 3) continue;

                double? min = ParseDoubleFromRange(range[0]);
                double? max = ParseDoubleFromRange(range[1]);
                string? rangeCor = ParseStringFromRange(range[2]);
                string? rangeClassificacao = range.Count > 3 ? ParseStringFromRange(range[3]) : null;

                intervalos.Add(new NutrienteConfig.IntervaloInfo
                {
                    Min = min,
                    Max = max,
                    Cor = rangeCor ?? "#CCCCCC",
                    Classificacao = rangeClassificacao ?? $"Faixa {min}-{max}"
                });
            }

            return intervalos;
        }

        /// <summary>
        /// Encontra a classificação e cor para um valor dentro dos ranges configurados
        /// </summary>
        private (string? classificacao, string? cor) EncontrarClassificacao(double valor, NutrientConfigData configData)
        {
            if (configData?.Ranges == null) return (null, null);

            foreach (var range in configData.Ranges)
            {
                if (range.Count < 3) continue;

                double? min = ParseDoubleFromRange(range[0]);
                double? max = ParseDoubleFromRange(range[1]);
                string? rangeCor = ParseStringFromRange(range[2]);
                string? rangeClassificacao = range.Count > 3 ? ParseStringFromRange(range[3]) : null;

                // Verificar se o valor está neste intervalo
                bool dentroDoIntervalo = (min == null || valor >= min) && (max == null || valor < max);

                if (dentroDoIntervalo)
                {
                    return (rangeClassificacao ?? $"Faixa {min}-{max}", rangeCor);
                }
            }

            return (null, null);
        }

        /// <summary>
        /// Parse de valor double de um objeto de range (pode ser JsonElement ou double)
        /// </summary>
        private double? ParseDoubleFromRange(object? value)
        {
            if (value == null) return null;

            if (value is System.Text.Json.JsonElement jsonElement)
            {
                return jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number
                    ? jsonElement.GetDouble()
                    : null;
            }

            if (double.TryParse(value.ToString(), out double parsed))
            {
                return parsed;
            }

            return null;
        }

        /// <summary>
        /// Parse de valor string de um objeto de range (pode ser JsonElement ou string)
        /// </summary>
        private string? ParseStringFromRange(object? value)
        {
            if (value == null) return null;

            if (value is System.Text.Json.JsonElement jsonElement)
            {
                return jsonElement.GetString();
            }

            return value.ToString();
        }
    }
}
