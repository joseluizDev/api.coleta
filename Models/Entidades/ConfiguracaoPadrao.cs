using System.ComponentModel.DataAnnotations;

namespace api.coleta.Models.Entidades
{
    public class ConfiguracaoPadrao : Entity
    {
        [MaxLength(100)]
        public string Nome { get; set; }
        public decimal Limite { get; set; }
        [MaxLength(20)]
        public string CorHex { get; set; }
    }
}
