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
}
