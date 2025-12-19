using System.Collections.Generic;
using System.Linq;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;

namespace api.coleta.Utils.Maps
{
    public static class ImagemNdviMap
    {
        public static ImagemNdviOutputDTO? ToOutputDto(this ImagemNdvi? imagem)
        {
            if (imagem == null)
            {
                return null;
            }

            return new ImagemNdviOutputDTO
            {
                Id = imagem.Id,
                LinkImagem = imagem.LinkImagem,
                DataImagem = imagem.DataImagem,
                TipoImagem = imagem.TipoImagem,
                // Campos NDVI
                PercentualNuvens = imagem.PercentualNuvens,
                NdviMax = imagem.NdviMax,
                NdviMin = imagem.NdviMin,
                // Campos Altimetria
                AltimetriaMin = imagem.AltimetriaMin,
                AltimetriaMax = imagem.AltimetriaMax,
                AltimetriaVariacao = imagem.AltimetriaVariacao,
                // IDs
                TalhaoId = imagem.TalhaoId,
                FazendaId = imagem.FazendaId,
                DataInclusao = imagem.DataInclusao
            };
        }

        public static List<ImagemNdviOutputDTO> ToOutputDtoList(this IEnumerable<ImagemNdvi?>? imagens)
        {
            if (imagens == null)
            {
                return new List<ImagemNdviOutputDTO>();
            }

            return imagens
                .Select(i => i.ToOutputDto())
                .Where(dto => dto is not null)
                .Select(dto => dto!)
                .ToList();
        }
    }
}
