using System.Text.Json;
using api.coleta.Models.Entidades;

namespace api.coleta.Models.DTOs
{
    public class VisualizarMapInputDto
    {
        public Guid? Id { get; set; }
        public Guid TalhaoID { get; set; }
        public JsonElement Geojson { get; set; }
        public Guid FuncionarioID { get; set; }
        public string Observacao { get; set; }
        public string TipoColeta { get; set; }
        public string TipoAnalise { get; set; }
        public string Profundidade { get; set; }
        public Guid? GeojsonId { get; set; }
    }

    public class VisualizarMapOutputDto
    {
        public Guid? Id { get; set; }
        public Guid TalhaoID { get; set; }
        public string Geojson { get; set; }
        public string FuncionarioID { get; set; }
        public string Observacao { get; set; }
        public string TipoColeta { get; set; }
        public string TipoAnalise { get; set; }
        public string Profundidade { get; set; }
    }
}
