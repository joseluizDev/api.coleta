using System.Collections.Generic;
using System.Linq;
using api.coleta.models;
using api.coleta.Models.Entidades;

namespace api.coleta.Utils.Maps
{
    public static class MColetaMap
    {
        public static ColetaResponseDTO? ToResponseDto(this MColeta? coleta)
        {
            if (coleta == null)
            {
                return null;
            }

            return new ColetaResponseDTO
            {
                Id = coleta.Id,
                Nome = coleta.Nome,
                Endereco = coleta.Endereco,
                Lat = coleta.Lat,
                Lng = coleta.Lng,
                reference = coleta.Reference,
                Place_Id = coleta.Place_Id
            };
        }

        public static MColeta? ToEntity(this ColetaRequestDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new MColeta
            {
                Nome = dto.Nome,
                Endereco = dto.Endereco,
                Lat = dto.Lat,
                Lng = dto.Lng,
                Reference = dto.reference,
                Place_Id = dto.Place_Id
            };
        }

        public static List<ColetaResponseDTO> ToResponseDtoList(this IEnumerable<MColeta?>? coletas)
        {
            if (coletas == null)
            {
                return new List<ColetaResponseDTO>();
            }

            return coletas
                .Select(c => c.ToResponseDto())
                .Where(dto => dto is not null)
                .Select(dto => dto!)
                .ToList();
        }
    }
}
