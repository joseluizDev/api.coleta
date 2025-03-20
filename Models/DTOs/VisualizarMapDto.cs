using api.coleta.Models.Entidades;

namespace api.coleta.Models.DTOs
{
    public class VisualizarMapInputDto
    {
        public Guid? Id { get; set; }
        public Guid TalhaoID { get; set; }
        public string Geojson { get; set; }
        public string Funcionario { get; set; }
        public string Observacao { get; set; }
        public TipoColeta TipoColeta { get; set; }
        public TipoAnalise TipoAnalise { get; set; }
        public Profundidade Profundidade { get; set; }
    }

    public class VisualizarMapOutputDto
    {
        public Guid? Id { get; set; }
        public Guid TalhaoID { get; set; }
        public string Geojson { get; set; }
        public string Funcionario { get; set; }
        public string Observacao { get; set; }
        public string TipoColeta { get; set; }
        public string TipoAnalise { get; set; }
        public string Profundidade { get; set; }
    }
}
