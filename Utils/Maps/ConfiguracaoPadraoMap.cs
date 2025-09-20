using System.Collections.Generic;
using System.Linq;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;

namespace api.coleta.Utils.Maps
{
    public static class ConfiguracaoPadraoMap
    {
        public static ConfiguracaoPadraoResponseDTO? ToResponseDto(this ConfiguracaoPadrao? configuracao)
        {
            if (configuracao == null)
            {
                return null;
            }

            return new ConfiguracaoPadraoResponseDTO
            {
                Id = configuracao.Id,
                Nome = configuracao.Nome,
                Limite = configuracao.Limite,
                CorHex = configuracao.CorHex
            };
        }

        public static List<ConfiguracaoPadraoResponseDTO> ToResponseDtoList(this IEnumerable<ConfiguracaoPadrao?>? configuracoes)
        {
            if (configuracoes == null)
            {
                return new List<ConfiguracaoPadraoResponseDTO>();
            }

            return configuracoes
                .Select(c => c.ToResponseDto())
                .Where(dto => dto is not null)
                .Select(dto => dto!)
                .ToList();
        }
    }
}
