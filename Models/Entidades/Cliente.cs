using api.fazenda.Models.Entidades;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public enum TipoDocumento
{
    CPF,
    CNPJ
}


namespace api.coleta.Models.Entidades
{
    public class Cliente : Entity
    {
        [MaxLength(255)]
        public string Nome { get; set; }
        [MaxLength(14)]
        public string? CPF { get; set; }
        [MaxLength(100)]
        public string Email { get; set; }
        [MaxLength(11)]
        public TipoDocumento? TipoDocumento { get; set; }
        [MaxLength(14)]
        public string Documento { get; set; }
        [MaxLength(14)]

        public string Telefone { get; set; }
        [ForeignKey("Usuario")]
        public Guid UsuarioID { get; set; }
        public virtual Usuario Usuario { get; set; }
        [MaxLength(8)]
        public string Cep { get; set; }
        [MaxLength(255)]
        public string Endereco { get; set; }
        [MaxLength(100)]
        public string Cidade { get; set; }
        [MaxLength(2)]
        public string Estado { get; set; }
        public Cliente()
        {
        }
    }
}