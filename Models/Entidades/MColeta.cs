using System.ComponentModel.DataAnnotations;

namespace api.coleta.Models.Entidades
{
    public class MColeta : Entity
    {
        [MaxLength]
        public string Nome { get; set; }
        [MaxLength(255)]
        public string Endereco { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        [MaxLength(255)]
        public string Reference { get; set; }
        [MaxLength(255)]
        public string Place_Id { get; set; }

        public MColeta()
        {
        }
    }
}

