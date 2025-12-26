namespace api.coleta.Models.Entidades
{
    /// <summary>
    /// Representa uma recomendação associada a um relatório
    /// </summary>
    public class Recomendacao : Entity
    {
        /// <summary>
        /// ID do relatório ao qual a recomendação pertence
        /// </summary>
        public Guid RelatorioId { get; set; }
        
        /// <summary>
        /// Navegação para o relatório relacionado
        /// </summary>
        public virtual Relatorio Relatorio { get; set; } = null!;
        
        /// <summary>
        /// ID da coleta associada
        /// </summary>
        public Guid? ColetaId { get; set; }
        
        /// <summary>
        /// Navegação para a coleta relacionada
        /// </summary>
        public virtual Coleta? Coleta { get; set; }
        
        /// <summary>
        /// Nome da coluna da recomendação
        /// </summary>
        public string NomeColuna { get; set; } = string.Empty;
        
        /// <summary>
        /// Unidade de medida
        /// </summary>
        public string UnidadeMedida { get; set; } = string.Empty;
    }
}
