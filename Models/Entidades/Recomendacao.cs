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
        /// Descrição da recomendação
        /// </summary>
        public string Descricao { get; set; } = string.Empty;
    }
}
