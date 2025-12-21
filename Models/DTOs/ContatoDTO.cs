using System.ComponentModel.DataAnnotations;

namespace api.coleta.Models.DTOs
{
    public class ContatoRequestDTO
    {
        [Required(ErrorMessage = "Nome completo é obrigatório")]
        [StringLength(255, ErrorMessage = "Nome não pode exceder 255 caracteres")]
        public string NomeCompleto { get; set; }

        [Required(ErrorMessage = "Cidade é obrigatória")]
        [StringLength(100, ErrorMessage = "Cidade não pode exceder 100 caracteres")]
        public string Cidade { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100, ErrorMessage = "Email não pode exceder 100 caracteres")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Número de telefone é obrigatório")]
        [Phone(ErrorMessage = "Telefone inválido")]
        [StringLength(20, ErrorMessage = "Telefone não pode exceder 20 caracteres")]
        public string NumeroTelefone { get; set; }
    }

    public class ContatoResponseDTO
    {
        public Guid Id { get; set; }
        public string NomeCompleto { get; set; }
        public string Cidade { get; set; }
        public string Email { get; set; }
        public string NumeroTelefone { get; set; }
        public bool EmailEnviado { get; set; }
        public DateTime DataInclusao { get; set; }
    }
}
