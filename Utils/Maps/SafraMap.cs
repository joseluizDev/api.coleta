using System;
using System.Collections.Generic;
using System.Linq;
using api.coleta.Models.Entidades;
using api.safra.Models.DTOs;

namespace api.coleta.Utils.Maps
{
    public static class SafraMap
    {
        public static SafraResponseDTO? ToResponseDto(this Safra? safra)
        {
            if (safra == null)
            {
                return null;
            }

            return new SafraResponseDTO
            {
                Id = safra.Id,
                Observacao = safra.Observacao,
                DataInicio = safra.DataInicio,
                DataFim = safra.DataFim,
                FazendaID = safra.FazendaID,
                ClienteID = safra.ClienteID,
                Fazenda = safra.Fazenda?.ToResponseDto() ?? null!,
                Cliente = safra.Cliente?.ToResponseDto() ?? null!
            };
        }

        public static Safra? ToEntity(this SafraRequestDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new Safra
            {
                Observacao = dto.Observacao,
                DataInicio = dto.DataInicio,
                DataFim = dto.DataFim,
                FazendaID = dto.FazendaID,
                ClienteID = dto.ClienteID
            };
        }

        public static Safra? ToEntity(this SafraResponseDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new Safra
            {
                Observacao = dto.Observacao,
                DataInicio = dto.DataInicio,
                DataFim = dto.DataFim,
                FazendaID = dto.FazendaID,
                ClienteID = dto.ClienteID
            };
        }

        public static List<SafraResponseDTO> ToResponseDtoList(this IEnumerable<Safra?>? safras)
        {
            if (safras == null)
            {
                return new List<SafraResponseDTO>();
            }

            return safras
                .Select(s => s.ToResponseDto())
                .Where(dto => dto is not null)
                .Select(dto => dto!)
                .ToList();
        }
    }
}
