using System.ComponentModel.DataAnnotations;
using api.coleta.models;

namespace api.coleta.Models.Entidades
{
    public class Coleta : Entity
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

        public Coleta()
        {
        }

        public Coleta(ColetasRequestDTO coleta)
        {
            Nome = coleta.Nome;
            Lat = coleta.location.Lat;
            Lng = coleta.location.Lng;
        }
    }
}

