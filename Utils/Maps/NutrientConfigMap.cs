using System;
using System.Collections.Generic;
using System.Linq;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;

namespace api.coleta.Utils.Maps
{
    public static class NutrientConfigMap
    {
        public static NutrientConfigResponseDTO? ToResponseDto(this NutrientConfig? config)
        {
            if (config == null)
            {
                return null;
            }

            var data = config.GetConfigData();
            return new NutrientConfigResponseDTO
            {
                Id = config.Id,
                UserId = config.UserId,
                NutrientName = config.NutrientName,
                Ranges = data?.Ranges ?? new List<List<object>>(),
                IsGlobal = config.IsGlobal,
                DataInclusao = config.DataInclusao
            };
        }

        public static List<NutrientConfigResponseDTO> ToResponseDtoList(this IEnumerable<NutrientConfig?>? configs)
        {
            return configs?.Select(c => c.ToResponseDto()).Where(dto => dto != null).Cast<NutrientConfigResponseDTO>().ToList() ?? new List<NutrientConfigResponseDTO>();
        }
    }
}