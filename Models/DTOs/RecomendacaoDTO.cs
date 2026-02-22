namespace api.coleta.Models.DTOs
{
    /// <summary>
    /// DTO para criação e atualização de recomendações
    /// </summary>
    public class RecomendacaoDTO
    {
        /// <summary>
        /// ID do relatório ao qual a recomendação pertence
        /// </summary>
        public Guid RelatorioId { get; set; }
        
        /// <summary>
        /// ID da coleta associada
        /// </summary>
        public Guid? ColetaId { get; set; }
        
        /// <summary>
        /// Nome da coluna da recomendação
        /// </summary>
        public string NomeColuna { get; set; } = string.Empty;
        
        /// <summary>
        /// Unidade de medida
        /// </summary>
        public string UnidadeMedida { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para retorno de recomendações
    /// </summary>
    public class RecomendacaoOutputDTO
    {
        /// <summary>
        /// ID da recomendação
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// ID do relatório
        /// </summary>
        public Guid RelatorioId { get; set; }
        
        /// <summary>
        /// ID da coleta associada
        /// </summary>
        public Guid? ColetaId { get; set; }
        
        /// <summary>
        /// Nome da coluna da recomendação
        /// </summary>
        public string NomeColuna { get; set; } = string.Empty;
        
        /// <summary>
        /// Unidade de medida
        /// </summary>
        public string UnidadeMedida { get; set; } = string.Empty;
        
        /// <summary>
        /// Data de inclusão da recomendação
        /// </summary>
        public DateTime DataInclusao { get; set; }
    }

    /// <summary>
    /// Intervalo de cor calculado para a recomendação (usado no mapa mobile)
    /// </summary>
    public class RecomendacaoIntervaloDTO
    {
        /// <summary>Valor mínimo do intervalo (null = sem limite inferior)</summary>
        public double? Minimo { get; set; }

        /// <summary>Valor máximo do intervalo (null = sem limite superior)</summary>
        public double? Maximo { get; set; }

        /// <summary>Cor hexadecimal associada ao intervalo</summary>
        public string Cor { get; set; } = string.Empty;

        /// <summary>Rótulo descritivo do intervalo (ex: "50", "50 - 100")</summary>
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Valor de recomendação por ponto de amostra
    /// </summary>
    public class RecomendacaoPontoDTO
    {
        /// <summary>ID do ponto de amostra (corresponde ao campo ID no JsonRelatorio)</summary>
        public int Id { get; set; }

        /// <summary>Valor da recomendação para este ponto</summary>
        public double Valor { get; set; }

        /// <summary>Cor do intervalo correspondente</summary>
        public string Cor { get; set; } = string.Empty;

        /// <summary>Rótulo do intervalo correspondente</summary>
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Recomendação com dados completos (valores por ponto e intervalos de cor) — para uso mobile
    /// </summary>
    public class RecomendacaoComDadosDTO : RecomendacaoOutputDTO
    {
        /// <summary>
        /// Intervalos de cor calculados a partir da distribuição de valores 
        /// (mesma lógica do web: até 5 valores únicos → um intervalo por valor; mais → 5 faixas)
        /// </summary>
        public List<RecomendacaoIntervaloDTO> Intervalos { get; set; } = new();

        /// <summary>
        /// Valores da recomendação por ponto, com cor e rótulo já resolvidos
        /// </summary>
        public List<RecomendacaoPontoDTO> ValoresPorPonto { get; set; } = new();
    }
}
