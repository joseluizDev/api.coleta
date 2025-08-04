using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;

namespace api.coleta.Utils.Maps;

public static class VisualizarDto
{
    public static Coleta MapVisualizar(this VisualizarMapInputDto map)
    {
        return new Coleta
        {
            UsuarioRespID = map.FuncionarioID,
            TipoAnalise = map.TipoAnalise
                .Select(x => Enum.Parse<TipoAnalise>(x.Trim()))
                .ToList(),

            TipoColeta = Enum.Parse<TipoColeta>(map.TipoColeta),
            TalhaoID = map.TalhaoID,
            Profundidade = Enum.Parse<Profundidade>(map.Profundidade),
            Observacao = map.Observacao,
            NomeColeta = map.NomeColeta,
            
            GeojsonID = (Guid)map.GeojsonId,
        };
    }

    public static List<Coleta> MapVisualizar(this List<VisualizarMapInputDto> map)
    {
        return map.Select(x => x.MapVisualizar()).ToList();
    }
}
