using System.ComponentModel.DataAnnotations;

namespace api.coleta.Models.Entidades
{
    public class ConfiguracaoPersonalizada : Entity
    {
        public Guid UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; }

        [MaxLength(100)]
        public string Nome { get; set; }

        public decimal LimiteInferior { get; set; }
        public decimal LimiteSuperior { get; set; }

        [MaxLength(20)]
        public string CorHex { get; set; }
    }
}
