using System.ComponentModel.DataAnnotations;
namespace api.coleta.Models.Entidades
{
   public class Funcionario : Entity
   {
      [MaxLength(100)]
      public string Nome { get; set; }
      [MaxLength(11)]
      public string CPF { get; set; }
      [MaxLength(100)]
      public string Email { get; set; }
      [MaxLength(11)]
      public string Telefone { get; set; }
      [MaxLength(100)]
      public string Senha { get; set; }
      public Guid UsuarioID { get; set; }
      public virtual Usuario Usuario { get; set; }
      [MaxLength(255)]
      public string Observacao { get; set; }

      public bool Ativo { get; set; }

      public Funcionario()
      {
      }
   }
}