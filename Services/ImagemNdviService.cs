using System.Collections.Generic;
using System.Linq;
using api.coleta.Data;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.coleta.Utils.Maps;
using api.minionStorage.Services;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Services
{
    public class ImagemNdviService : ServiceBase
    {
        private readonly ImagemNdviRepository _imagemNdviRepository;
        private readonly IMinioStorage _minioStorage;
        private readonly ApplicationDbContext _context;

        public ImagemNdviService(ImagemNdviRepository imagemNdviRepository, IMinioStorage minioStorage, IUnitOfWork unitOfWork, ApplicationDbContext context) : base(unitOfWork)
        {
            _imagemNdviRepository = imagemNdviRepository;
            _minioStorage = minioStorage;
            _context = context;
        }

        public async Task<ImagemNdviOutputDTO?> SalvarImagemAsync(ImagemNdviUploadDTO dto, Guid usuarioId)
        {
            if (dto.Arquivo == null || dto.Arquivo.Length == 0) return null;

            // Buscar Talhao para obter FazendaId
            var talhao = await _context.Talhoes.FindAsync(dto.TalhaoId);
            if (talhao == null) throw new KeyNotFoundException("TalhaoId inválido: talhão não encontrado.");

            // Determinar o tipo de imagem (ndvi ou altimetria)
            string tipoImagem = dto.TipoImagem?.ToLower() ?? "ndvi";

            string bucketName = "coleta";
            var file = dto.Arquivo;
            string fileExtension = Path.GetExtension(file.FileName).TrimStart('.');
            string contentType = file.ContentType;
            string objectName = $"{tipoImagem}/{talhao.FazendaID}/{dto.TalhaoId}/{Guid.NewGuid()}.{fileExtension}";

            using var stream = file.OpenReadStream();
            string url = await _minioStorage.UploadFileAsync(bucketName, objectName, stream, contentType);
            if (string.IsNullOrEmpty(url)) return null;

            var entidade = new ImagemNdvi
            {
                LinkImagem = url,
                DataImagem = dto.DataImagem,
                TipoImagem = tipoImagem,
                // Campos NDVI
                PercentualNuvens = dto.PercentualNuvens,
                NdviMax = dto.NdviMax,
                NdviMin = dto.NdviMin,
                // Campos Altimetria
                AltimetriaMin = dto.AltimetriaMin,
                AltimetriaMax = dto.AltimetriaMax,
                AltimetriaVariacao = dto.AltimetriaVariacao,
                // IDs
                TalhaoId = dto.TalhaoId,
                FazendaId = talhao.FazendaID,
                UsuarioId = usuarioId
            };

            _imagemNdviRepository.Adicionar(entidade);
            UnitOfWork.Commit();

            return new ImagemNdviOutputDTO
            {
                Id = entidade.Id,
                LinkImagem = entidade.LinkImagem,
                DataImagem = entidade.DataImagem,
                TipoImagem = entidade.TipoImagem,
                PercentualNuvens = entidade.PercentualNuvens,
                NdviMax = entidade.NdviMax,
                NdviMin = entidade.NdviMin,
                AltimetriaMin = entidade.AltimetriaMin,
                AltimetriaMax = entidade.AltimetriaMax,
                AltimetriaVariacao = entidade.AltimetriaVariacao,
                TalhaoId = entidade.TalhaoId,
                FazendaId = entidade.FazendaId,
                DataInclusao = entidade.DataInclusao
            };
        }

        public async Task<IEnumerable<ImagemNdviOutputDTO>> GetByTalhaoIdAsync(Guid talhaoId)
        {
            var imagens = await _context.ImagensNdvi.Where(i => i.TalhaoId == talhaoId).ToListAsync();
            return imagens.ToOutputDtoList();
        }
    }
}
