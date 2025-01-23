using api.fazenda.Models.Entidades;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.coleta.Models.Entidades
{
    public class Cliente : Entity
    {
        [MaxLength(255)]
        public string Nome { get; set; }
        [MaxLength(11)]
        public string CPF { get; set; }
        [MaxLength(100)]
        public string Email { get; set; }
        [MaxLength(11)]
        public string Telefone { get; set; }
        [ForeignKey("Usuario")]
        public Guid UsuarioID { get; set; }
        public virtual Usuario Usuario { get; set; }
        public Cliente()
        {
        }
    }
}