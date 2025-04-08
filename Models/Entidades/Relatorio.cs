using System.ComponentModel.DataAnnotations.Schema;

namespace api.coleta.Models.Entidades
{
    public class Relatorio : Entity
    {
        public string LinkBackup { get; set; }
        [Column(TypeName = "JSON")]
        public string JsonRelatorio { get; set; }
        public Guid UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; }
        public Guid ColetaId { get; set; }
        public virtual Coleta Coleta { get; set; }
    }
}