using System.Collections.Generic;

namespace api.coleta.Models.DTOs
{
    public class RelatorioDTO
    {
        public Guid? Id { get; set; }
        public IFormFile? Arquivo { get; set; }
        public string? ArquivoJson { get; set; }
        public string? ColetaId { get; set; }
    }

    public class RelatorioOuputDTO
    {
        public Guid Id { get; set; }
        public string ColetaId { get; set; }
        public string LinkBackup { get; set; }
        public DateTime DataInclusao { get; set; }
        public string NomeColeta { get; set; }
        public string Talhao { get; set; }
        public string TipoColeta { get; set; }
        public string Fazenda { get; set; }
        public string NomeCliente { get; set; }
        public string Safra { get; set; }
        public string Funcionario { get; set; }
        public string Observacao { get; set; }
        public string Profundidade { get; set; }
        public List<string> TiposAnalise { get; set; } = [];
        public string? JsonRelatorio { get; set; }
        public bool IsRelatorio { get; set; }
    }

    public class AtualizarJsonRelatorioDTO
    {
        public Guid ColetaId { get; set; }
        public Guid RelatorioId { get; set; }
        public string JsonRelatorio { get; set; } = string.Empty;
    }

    public class RelatorioCompletoOutputDTO
    {
        public Guid Id { get; set; }
        public string ColetaId { get; set; }
        public string LinkBackup { get; set; }
        public DateTime DataInclusao { get; set; }
        public string NomeColeta { get; set; }
        public string Talhao { get; set; }
        public string TipoColeta { get; set; }
        public string Fazenda { get; set; }
        public string NomeCliente { get; set; }
        public string Safra { get; set; }
        public string Funcionario { get; set; }
        public string Observacao { get; set; }
        public string Profundidade { get; set; }
        public List<string> TiposAnalise { get; set; } = [];
        public string? JsonRelatorio { get; set; }
        public bool IsRelatorio { get; set; }
        
        // Dados da coleta (mapa, grid, pontos)
        public ColetaDadosDTO? DadosColeta { get; set; }
        
        // Classificações dos nutrientes por objeto
        public List<object>? NutrientesClassificados { get; set; }
        
        /// <summary>
        /// Estatísticas de cada atributo (para gráficos mobile: histograma, min, média, max)
        /// </summary>
        public Dictionary<string, EstatisticaAtributoDTO> EstatisticasAtributos { get; set; } = new Dictionary<string, EstatisticaAtributoDTO>();
    }

    public class ColetaDadosDTO
    {
        public Guid ColetaId { get; set; }
        public object? Geojson { get; set; }
        public object? Talhao { get; set; }
        public object? UsuarioResp { get; set; }
        public Guid GeoJsonID { get; set; }
        public Guid UsuarioRespID { get; set; }
        public int Zonas { get; set; }
        public decimal? AreaHa { get; set; }
    }

    /// <summary>
    /// DTO para indicador individual com valor médio, classificação e cor
    /// </summary>
    public class IndicadorDTO
    {
        public double ValorMedio { get; set; }
        public string Classificacao { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para dados estatísticos de um atributo (para histograma e análise estatística)
    /// </summary>
    public class EstatisticaAtributoDTO
    {
        /// <summary>
        /// Nome do atributo
        /// </summary>
        public string Nome { get; set; } = string.Empty;
        
        /// <summary>
        /// Array de todos os valores do atributo para cada ponto (para histograma)
        /// </summary>
        public List<double> Valores { get; set; } = new List<double>();
        
        /// <summary>
        /// Menor valor encontrado
        /// </summary>
        public double Minimo { get; set; }
        
        /// <summary>
        /// Valor médio
        /// </summary>
        public double Media { get; set; }
        
        /// <summary>
        /// Maior valor encontrado
        /// </summary>
        public double Maximo { get; set; }
        
        /// <summary>
        /// Classificação baseada na média
        /// </summary>
        public string Classificacao { get; set; } = string.Empty;
        
        /// <summary>
        /// Cor baseada na classificação
        /// </summary>
        public string Cor { get; set; } = string.Empty;
        
        /// <summary>
        /// Quantidade de pontos válidos
        /// </summary>
        public int QuantidadePontos { get; set; }
    }

    /// <summary>
    /// DTO para indicadores de acidez do solo (pH)
    /// </summary>
    public class AcidezDTO
    {
        public IndicadorDTO pH { get; set; } = new IndicadorDTO();
    }

    /// <summary>
    /// DTO para indicadores de saturação (m% e V%)
    /// </summary>
    public class SaturacaoDTO
    {
        [System.Text.Json.Serialization.JsonPropertyName("m%")]
        public IndicadorDTO SaturacaoAluminio { get; set; } = new IndicadorDTO();
        
        [System.Text.Json.Serialization.JsonPropertyName("V%")]
        public IndicadorDTO SaturacaoBases { get; set; } = new IndicadorDTO();
    }

    /// <summary>
    /// DTO para equilíbrio de bases (relações Ca/Mg, Ca/K, Mg/K)
    /// </summary>
    public class EquilibrioBasesDTO
    {
        [System.Text.Json.Serialization.JsonPropertyName("Ca/Mg")]
        public IndicadorDTO CaMg { get; set; } = new IndicadorDTO();
        
        [System.Text.Json.Serialization.JsonPropertyName("Ca/K")]
        public IndicadorDTO CaK { get; set; } = new IndicadorDTO();
        
        [System.Text.Json.Serialization.JsonPropertyName("Mg/K")]
        public IndicadorDTO MgK { get; set; } = new IndicadorDTO();
    }

    /// <summary>
    /// DTO para participação na CTC (%)
    /// </summary>
    public class ParticipacaoCTCDTO
    {
        [System.Text.Json.Serialization.JsonPropertyName("Ca/CTC")]
        public IndicadorDTO CaCTC { get; set; } = new IndicadorDTO();
        
        [System.Text.Json.Serialization.JsonPropertyName("Mg/CTC")]
        public IndicadorDTO MgCTC { get; set; } = new IndicadorDTO();
        
        [System.Text.Json.Serialization.JsonPropertyName("K/CTC")]
        public IndicadorDTO KCTC { get; set; } = new IndicadorDTO();
        
        [System.Text.Json.Serialization.JsonPropertyName("H+Al/CTC")]
        public IndicadorDTO HAlCTC { get; set; } = new IndicadorDTO();
        
        [System.Text.Json.Serialization.JsonPropertyName("Al/CTC")]
        public IndicadorDTO AlCTC { get; set; } = new IndicadorDTO();
    }

    /// <summary>
    /// DTO para todos os indicadores de gráficos
    /// </summary>
    public class IndicadoresGraficosDTO
    {
        public AcidezDTO Acidez { get; set; } = new AcidezDTO();
        public SaturacaoDTO Saturacao { get; set; } = new SaturacaoDTO();
        public EquilibrioBasesDTO EquilibrioBases { get; set; } = new EquilibrioBasesDTO();
        public ParticipacaoCTCDTO ParticipacaoCTC { get; set; } = new ParticipacaoCTCDTO();
    }

    /// <summary>
    /// DTO para a resposta completa com indicadores para gráficos
    /// </summary>
    public class ResumoTalhaoDTO
    {
        public Guid RelatorioId { get; set; }
        public Guid ColetaId { get; set; }
        public string NomeTalhao { get; set; } = string.Empty;
        public string NomeFazenda { get; set; } = string.Empty;
        public string NomeSafra { get; set; } = string.Empty;
        public int TotalPontos { get; set; }
        public IndicadoresGraficosDTO IndicadoresGraficos { get; set; } = new IndicadoresGraficosDTO();
        
        /// <summary>
        /// Estatísticas detalhadas de todos os atributos do relatório (para histogramas e análise estatística)
        /// </summary>
        public Dictionary<string, EstatisticaAtributoDTO> EstatisticasAtributos { get; set; } = new Dictionary<string, EstatisticaAtributoDTO>();
    }

}
