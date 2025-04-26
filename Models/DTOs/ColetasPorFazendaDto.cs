using api.coleta.Models.DTOs;
using api.cliente.Models.DTOs;
using api.fazenda.models;

namespace api.coleta.Models.DTOs
{
    public class ColetasPorFazendaDto
    {
        public Guid FazendaId { get; set; }
        public string NomeFazenda { get; set; }
        public FazendaResponseDTO Fazenda { get; set; }
        public ClienteResponseDTO Cliente { get; set; }
        public List<VisualizarMapOutputDto> Coletas { get; set; }

        public ColetasPorFazendaDto()
        {
            Coletas = new List<VisualizarMapOutputDto>();
        }
    }
}
