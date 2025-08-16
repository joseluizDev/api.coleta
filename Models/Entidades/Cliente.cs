using api.fazenda.Models.Entidades;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;





namespace api.coleta.Models.Entidades
{
    public class Cliente : Entity
    {
        [Required]
        [MaxLength(255)]
        public string Nome { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(14)]
        public string Documento { get; set; }

        [Required]
        [MaxLength(14)]
        public string Telefone { get; set; }

        [ForeignKey("Usuario")]
        public Guid UsuarioID { get; set; }
        public virtual Usuario Usuario { get; set; }

        [Required]
        [MaxLength(8)]
        public string Cep { get; set; }

        [Required]
        [MaxLength(255)]
        public string Endereco { get; set; }

        [Required]
        [MaxLength(100)]
        public string Cidade { get; set; }

        [Required]
        [MaxLength(2)]
        public string Estado { get; set; }

        public Cliente()
        {
        }
    }
}