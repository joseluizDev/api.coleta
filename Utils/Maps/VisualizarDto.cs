using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;

namespace api.coleta.Utils.Maps;

public static class VisualizarDto
{
    public static VisualizarMapa MapVisualizar(this VisualizarMapInputDto map)
    {
        return new VisualizarMapa
        {
          FuncionarioID =  map.FuncionarioID,
          TipoAnalise = Enum.Parse<TipoAnalise>(map.TipoAnalise),
          TipoColeta = Enum.Parse<TipoColeta>(map.TipoColeta),
          TalhaoID = map.TalhaoID,
          Profundidade = Enum.Parse<Profundidade>(map.Profundidade),
          Observacao =  map.Observacao,
          GeojsonID = (Guid)map.GeojsonId,
        };
    }

    public static List<VisualizarMapa> MapVisualizar(this List<VisualizarMapInputDto> map)
    {
        return map.Select(x => x.MapVisualizar()).ToList();
    }
}