using System.Text.Json;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;

namespace api.coleta.Services.Relatorio
{
    /// <summary>
    /// Serviço responsável pelo cálculo de indicadores do solo.
    /// Calcula indicadores de acidez, saturação, equilíbrio de bases e participação na CTC.
    /// </summary>
    public class SoilIndicatorService
    {
        private readonly NutrientClassificationService _classificationService;

        public SoilIndicatorService(NutrientClassificationService classificationService)
        {
            _classificationService = classificationService;
        }

        /// <summary>
        /// Busca valor em um ponto JSON usando múltiplas chaves possíveis e acumula
        /// </summary>
        /// <param name="ponto">Elemento JSON do ponto</param>
        /// <param name="chaves">Array de chaves possíveis (ordem de prioridade)</param>
        /// <param name="soma">Referência para acumular soma</param>
        /// <param name="contador">Referência para acumular contagem</param>
        public void BuscarEAcumularValor(
            JsonElement ponto,
            string[] chaves,
            ref double soma,
            ref int contador)
        {
            foreach (var chave in chaves)
            {
                if (ponto.TryGetProperty(chave, out var prop) &&
                    prop.ValueKind == JsonValueKind.Number &&
                    prop.TryGetDouble(out double valor))
                {
                    soma += valor;
                    contador++;
                    break; // Encontrou, não precisa continuar
                }
            }
        }

        /// <summary>
        /// Calcula o indicador com média, classificação e cor
        /// </summary>
        /// <param name="nomeAtributo">Nome do atributo/nutriente</param>
        /// <param name="soma">Soma dos valores</param>
        /// <param name="contador">Quantidade de valores</param>
        /// <param name="configsPersonalizadas">Configurações personalizadas do usuário</param>
        /// <returns>IndicadorDTO com valor médio, classificação, cor e intervalo adequado</returns>
        public IndicadorDTO CalcularIndicador(
            string nomeAtributo,
            double soma,
            int contador,
            Dictionary<string, NutrientConfig> configsPersonalizadas)
        {
            if (contador == 0)
            {
                return CriarIndicadorSemDados();
            }

            double media = soma / contador;
            IntervaloClassificacaoDTO? intervaloAdequado = null;

            // Tentar classificação personalizada primeiro
            var resultPersonalizado = _classificationService.ClassificarComConfigPersonalizada(
                nomeAtributo, media, configsPersonalizadas);

            if (resultPersonalizado != null)
            {
                var resultObj = (dynamic)resultPersonalizado;

                // Tentar obter intervalo adequado da config personalizada
                intervaloAdequado = ExtrairIntervaloAdequado(nomeAtributo, configsPersonalizadas);

                return new IndicadorDTO
                {
                    ValorMedio = Math.Round(media, 1),
                    Classificacao = resultObj.classificacao?.ToString() ?? "Não classificado",
                    Cor = resultObj.cor?.ToString() ?? "#CCCCCC",
                    IntervaloAdequado = intervaloAdequado
                };
            }

            // Usar classificação padrão (sem referência)
            var classResult = NutrienteConfig.GetNutrientClassification(nomeAtributo, media, 0, null);

            // Buscar intervalo adequado nos intervalos retornados
            if (classResult?.Intervalos != null)
            {
                var intervalo = classResult.Intervalos.FirstOrDefault(i =>
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

            return new IndicadorDTO
            {
                ValorMedio = Math.Round(media, 1),
                Classificacao = classResult?.Classificacao ?? "Não classificado",
                Cor = classResult?.Cor ?? "#CCCCCC",
                IntervaloAdequado = intervaloAdequado
            };
        }

        /// <summary>
        /// Calcula um indicador com referência CTC ou Argila para classificação
        /// </summary>
        /// <param name="nomeAtributo">Nome do atributo/nutriente</param>
        /// <param name="soma">Soma dos valores</param>
        /// <param name="contador">Quantidade de valores</param>
        /// <param name="configsPersonalizadas">Configurações personalizadas do usuário</param>
        /// <param name="valorReferencia">Valor de referência (média de CTC ou Argila)</param>
        /// <param name="tipoReferencia">Tipo de referência ("CTC" ou "Argila")</param>
        /// <returns>IndicadorDTO com valor médio, classificação, cor e intervalo adequado</returns>
        public IndicadorDTO CalcularIndicadorComReferencia(
            string nomeAtributo,
            double soma,
            int contador,
            Dictionary<string, NutrientConfig> configsPersonalizadas,
            double valorReferencia,
            string tipoReferencia)
        {
            if (contador == 0)
            {
                return CriarIndicadorSemDados();
            }

            double media = soma / contador;
            IntervaloClassificacaoDTO? intervaloAdequado = null;

            // Tentar classificação personalizada primeiro
            var resultPersonalizado = _classificationService.ClassificarComConfigPersonalizada(
                nomeAtributo, media, configsPersonalizadas);

            if (resultPersonalizado != null)
            {
                var resultObj = (dynamic)resultPersonalizado;

                // Tentar obter intervalo adequado da config personalizada
                intervaloAdequado = ExtrairIntervaloAdequado(nomeAtributo, configsPersonalizadas);

                return new IndicadorDTO
                {
                    ValorMedio = Math.Round(media, 2),
                    Classificacao = resultObj.classificacao?.ToString() ?? "Não classificado",
                    Cor = resultObj.cor?.ToString() ?? "#CCCCCC",
                    IntervaloAdequado = intervaloAdequado
                };
            }

            // Usar classificação padrão com referência CTC/Argila
            var classResult = NutrienteConfig.GetNutrientClassification(
                nomeAtributo, media, valorReferencia, tipoReferencia);

            // Buscar intervalo adequado nos intervalos retornados
            if (classResult?.Intervalos != null)
            {
                var intervalo = classResult.Intervalos.FirstOrDefault(i =>
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

            return new IndicadorDTO
            {
                ValorMedio = Math.Round(media, 2),
                Classificacao = classResult?.Classificacao ?? "Não classificado",
                Cor = classResult?.Cor ?? "#CCCCCC",
                IntervaloAdequado = intervaloAdequado
            };
        }

        /// <summary>
        /// Cria indicador padrão para quando não há dados
        /// </summary>
        private IndicadorDTO CriarIndicadorSemDados()
        {
            return new IndicadorDTO
            {
                ValorMedio = 0,
                Classificacao = "Sem dados",
                Cor = "#CCCCCC",
                IntervaloAdequado = null
            };
        }

        /// <summary>
        /// Extrai intervalo adequado de uma configuração personalizada
        /// </summary>
        private IntervaloClassificacaoDTO? ExtrairIntervaloAdequado(
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
