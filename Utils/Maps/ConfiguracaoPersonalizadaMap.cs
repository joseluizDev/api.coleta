using System;
using System.Collections.Generic;
using System.Linq;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;

namespace api.coleta.Utils.Maps
{
    public static class ConfiguracaoPersonalizadaMap
    {
        public static ConfiguracaoPersonalizadaResponseDTO? ToResponseDto(this ConfiguracaoPersonalizada? configuracao)
        {
            if (configuracao == null)
            {
                return null;
            }

            return new ConfiguracaoPersonalizadaResponseDTO
            {
                Id = configuracao.Id,
                UsuarioId = configuracao.UsuarioId,
                Nome = configuracao.Nome,
                LimiteInferior = configuracao.LimiteInferior,
                LimiteSuperior = configuracao.LimiteSuperior,
                CorHex = configuracao.CorHex
            };
        }

        public static List<ConfiguracaoPersonalizadaResponseDTO> ToResponseDtoList(this IEnumerable<ConfiguracaoPersonalizada?>? configuracoes)
        {
            if (configuracoes == null)
            {
                return new List<ConfiguracaoPersonalizadaResponseDTO>();
            }

            return configuracoes
                .Select(c => c.ToResponseDto())
                .Where(dto => dto is not null)
                .Select(dto => dto!)
                .ToList();
        }
    }
}
