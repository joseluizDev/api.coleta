using System.ComponentModel.DataAnnotations;

namespace api.coleta.Models.Entidades
{
    public class Minerais : Entity
    {
        [MaxLength(50)]
        public string Nome { get; set; }
    }
}