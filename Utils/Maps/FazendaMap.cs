using System.Collections.Generic;
using System.Linq;
using api.fazenda.models;
using api.fazenda.Models.Entidades;

namespace api.coleta.Utils.Maps
{
    public static class FazendaMap
    {
        public static FazendaResponseDTO? ToResponseDto(this Fazenda? fazenda)
        {
            if (fazenda == null)
            {
                return null;
            }

            return new FazendaResponseDTO
            {
                Id = fazenda.Id,
                Nome = fazenda.Nome,
                Endereco = fazenda.Endereco,
                Lat = fazenda.Lat,
                Lng = fazenda.Lng,
                ClienteID = fazenda.ClienteID
            };
        }

        public static Fazenda? ToEntity(this FazendaRequestDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new Fazenda
            {
                Nome = dto.Nome,
                Endereco = dto.Endereco,
                Lat = dto.Lat,
                Lng = dto.Lng,
                ClienteID = dto.ClienteID
            };
        }

        public static List<FazendaResponseDTO> ToResponseDtoList(this IEnumerable<Fazenda?>? fazendas)
        {
            if (fazendas == null)
            {
                return new List<FazendaResponseDTO>();
            }

            return fazendas
                .Select(f => f.ToResponseDto())
                .Where(dto => dto is not null)
                .Select(dto => dto!)
                .ToList();
        }
    }
}
