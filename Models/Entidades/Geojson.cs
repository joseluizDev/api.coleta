using System.ComponentModel.DataAnnotations.Schema;

namespace api.coleta.Models.Entidades
{
    public class Geojson : Entity
    {
        [Column(TypeName = "JSON")]
        public string Pontos { get;set; }
        [Column(TypeName = "JSON")]
        public string Grid { get; set; }
    }
}