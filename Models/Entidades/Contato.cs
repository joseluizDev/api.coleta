using System.ComponentModel.DataAnnotations;

namespace api.coleta.Models.Entidades
{
    public class Contato : Entity
    {
        [Required]
        [MaxLength(255)]
        public string NomeCompleto { get; set; }

        [Required]
        [MaxLength(100)]
        public string Cidade { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(20)]
        public string NumeroTelefone { get; set; }

        public bool EmailUsuarioEnviado { get; set; } = false;
        public bool EmailAdminsEnviado { get; set; } = false;
        public DateTime? DataEnvioEmail { get; set; }

        public Contato() { }
    }
}
