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
        /// Descrição da recomendação
        /// </summary>
        public string Descricao { get; set; } = string.Empty;
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
        /// Descrição da recomendação
        /// </summary>
        public string Descricao { get; set; } = string.Empty;
        
        /// <summary>
        /// Data de inclusão da recomendação
        /// </summary>
        public DateTime DataInclusao { get; set; }
    }
}
