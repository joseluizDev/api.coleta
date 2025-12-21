using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.coleta.Models.Entidades
{
    public class Imagem : Entity
    {
        [Required]
        [MaxLength(500)]
        public string Url { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string ObjectName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string BucketName { get; set; } = "coleta";

        [MaxLength(100)]
        public string? ContentType { get; set; }

        [MaxLength(255)]
        public string? NomeOriginal { get; set; }

        public long? TamanhoBytes { get; set; }

        [ForeignKey("Usuario")]
        public Guid UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; }
    }
}
