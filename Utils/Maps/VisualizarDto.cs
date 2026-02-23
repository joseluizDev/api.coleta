using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;

namespace api.coleta.Utils.Maps;

public static class VisualizarDto
{
    public static Coleta MapVisualizar(this VisualizarMapInputDto map)
    {
        if (!map.FuncionarioID.HasValue)
            throw new ArgumentException("FuncionarioID é obrigatório para criação.");

        if (!map.TalhaoID.HasValue)
            throw new ArgumentException("TalhaoID é obrigatório para criação.");

        if (string.IsNullOrEmpty(map.TipoColeta))
            throw new ArgumentException("TipoColeta é obrigatório para criação.");

        if (map.TipoAnalise == null || !map.TipoAnalise.Any())
            throw new ArgumentException("TipoAnalise é obrigatório para criação.");

        if (string.IsNullOrEmpty(map.Profundidade))
            throw new ArgumentException("Profundidade é obrigatória para criação.");

        return new Coleta
        {
            UsuarioRespID = map.FuncionarioID.Value,
            TipoAnalise = map.TipoAnalise
                .Select(x => Enum.Parse<TipoAnalise>(x.Trim()))
                .ToList(),

            TipoColeta = Enum.Parse<TipoColeta>(map.TipoColeta),
            TalhaoID = map.TalhaoID.Value,
            FazendaID = map.FazendaID,
            SafraID = map.SafraID,
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
