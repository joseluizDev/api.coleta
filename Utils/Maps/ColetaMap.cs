using System.Collections.Generic;
using System.Linq;
using api.coleta.models;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;

namespace api.coleta.Utils.Maps
{
    public static class ColetaMap
    {
        public static ColetaPorUsuarioDto? ToUsuarioDto(this Coleta? coleta)
        {
            if (coleta == null)
            {
                return null;
            }

            return new ColetaPorUsuarioDto
            {
                Id = coleta.Id,
                Nome = coleta.NomeColeta,
                TipoAnalise = coleta.TipoAnalise?.Select(x => x.ToString()).ToList(),
                Safra = coleta.Safra != null ? coleta.Safra.ToResponseDto()! : null!,
                Talhao = coleta.Talhao != null ? coleta.Talhao.ToTalhaoResponseDto()! : null!,
                FazendaID = coleta.FazendaID
            };
        }

        public static VisualizarMapOutputDto? ToVisualizarDto(this Coleta? coleta)
        {
            if (coleta == null)
            {
                return null;
            }

            return new VisualizarMapOutputDto
            {
                Id = coleta.Id,
                TalhaoID = coleta.TalhaoID,
                Talhao = coleta.Talhao?.ToTalhoes() ?? null!,
                FazendaID = coleta.FazendaID,
                ClienteID = coleta.Talhao?.Talhao?.ClienteID,
                Safra = coleta.Safra,
                SafraID = coleta.SafraID,
                Geojson = coleta.Geojson,
                GeoJsonID = coleta.GeojsonID,
                UsuarioResp = coleta.UsuarioResp?.ToResponseDto(),
                UsuarioRespID = coleta.UsuarioRespID,
                Observacao = coleta.Observacao,
                TipoColeta = coleta.TipoColeta.ToString(),
                TipoAnalise = coleta.TipoAnalise?.Select(x => x.ToString()).ToList(),
                Profundidade = ProfundidadeFormatter.Formatar(coleta.Profundidade.ToString()),
                NomeColeta = coleta.NomeColeta
            };
        }

        public static List<ColetaPorUsuarioDto> ToUsuarioDtoList(this IEnumerable<Coleta?>? coletas)
        {
            if (coletas == null)
            {
                return new List<ColetaPorUsuarioDto>();
            }

            return coletas
                .Select(c => c.ToUsuarioDto())
                .Where(dto => dto is not null)
                .Select(dto => dto!)
                .ToList();
        }

        public static List<VisualizarMapOutputDto?> ToVisualizarDtoNullableList(this IEnumerable<Coleta?>? coletas)
        {
            if (coletas == null)
            {
                return new List<VisualizarMapOutputDto?>();
            }

            return coletas
                .Select(c => c.ToVisualizarDto())
                .ToList();
        }

        public static List<VisualizarMapOutputDto> ToVisualizarDtoList(this IEnumerable<Coleta?>? coletas)
        {
            if (coletas == null)
            {
                return new List<VisualizarMapOutputDto>();
            }

            return coletas
                .Select(c => c.ToVisualizarDto())
                .Where(dto => dto is not null)
                .Select(dto => dto!)
                .ToList();
        }
    }
}
