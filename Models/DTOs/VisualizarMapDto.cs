using System.Text.Json;
using System.Text.Json.Serialization;
using api.coleta.Models.Entidades;
using api.funcionario.Models.DTOs;
using api.talhao.Models.DTOs;

namespace api.coleta.Models.DTOs
{
    public class VisualizarMapInputDto
    {
        public Guid? Id { get; set; }
        public Guid TalhaoID { get; set; }
        public Guid? FazendaID { get; set; }
        public Guid? SafraID { get; set; }
        
        [JsonPropertyName("geoJson")]
        public JsonElement Geojson { get; set; }
        
        public Guid FuncionarioID { get; set; }
        public string Observacao { get; set; }
        public string TipoColeta { get; set; }
        public List<string> TipoAnalise { get; set; }
        public string Profundidade { get; set; }
        public Guid? GeojsonId { get; set; }
        public string? NomeColeta { get; set; }
    }

    public class QueryVisualizarMap
    {
        public int? Page { get; set; }
        public Guid? FuncionarioID { get; set; }
        public string? TipoColeta { get; set; }
        public string? TipoAnalise { get; set; }
        public Guid? ClienteID { get; set; }
        public Guid? FazendaID { get; set; }
        public Guid? TalhaoID { get; set; }
        public Guid? SafraID { get; set; }
        public string? NomeColeta { get; set; }
    }

    public class VisualizarMapOutputDto
    {
        public Guid? Id { get; set; }
        public Talhoes Talhao { get; set; }
        public Guid TalhaoID { get; set; }
    // Adicionado: expor IDs de fazenda e cliente para facilitar edição no frontend
    public Guid? FazendaID { get; set; }
    public Guid? ClienteID { get; set; }
        public Safra? Safra { get; set; }
        public Guid? SafraID { get; set; }
        public Geojson Geojson { get; set; }
        public Guid GeoJsonID { get; set; }
        public UsuarioResponseDTO UsuarioResp { get; set; }
        public Guid UsuarioRespID { get; set; }
        public string Observacao { get; set; }
        public string TipoColeta { get; set; }
        public List<string> TipoAnalise { get; set; }
        public string Profundidade { get; set; }
        public string? NomeColeta { get; set; }
    }
}
