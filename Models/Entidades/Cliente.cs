using System.ComponentModel.DataAnnotations;

namespace api.cliente.Models.Entidades
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
      public Cliente()
      {
      }
   }
}