using api.coleta.Models.Entidades;
using api.vinculoClienteFazenda.Models.DTOs;

namespace api.coleta.Utils.Maps
{
    public static class VinculoMap
    {
        public static VinculoResponseDTO? ToResponseDto(this VinculoClienteFazenda? vinculo)
        {
            if (vinculo == null)
            {
                return null;
            }

            return new VinculoResponseDTO
            {
                Id = vinculo.Id,
                ClienteId = vinculo.ClienteId,
                FazendaId = vinculo.FazendaId,
                Ativo = vinculo.Ativo
            };
        }

        public static VinculoClienteFazenda? ToEntity(this VinculoRequestDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new VinculoClienteFazenda
            {
                ClienteId = dto.ClienteId,
                FazendaId = dto.FazendaId,
                Ativo = dto.Ativo
            };
        }
    }
}
