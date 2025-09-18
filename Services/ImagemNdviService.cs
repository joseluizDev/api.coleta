using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Repositories;
using api.minionStorage.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Services
{
    public class ImagemNdviService : ServiceBase
    {
        private readonly ImagemNdviRepository _imagemNdviRepository;
        private readonly MinioStorage _minioStorage;
        private readonly ApplicationDbContext _context;

        public ImagemNdviService(ImagemNdviRepository imagemNdviRepository, MinioStorage minioStorage, IUnitOfWork unitOfWork, IMapper mapper, ApplicationDbContext context) : base(unitOfWork, mapper)
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
            if (talhao == null) throw new Exception("TalhaoId inválido: talhão não encontrado.");

            string bucketName = "coleta"; // Pode externalizar
            var file = dto.Arquivo;
            string fileExtension = Path.GetExtension(file.FileName).TrimStart('.');
            string contentType = file.ContentType;
            string objectName = $"ndvi/{talhao.FazendaID}/{dto.TalhaoId}/{Guid.NewGuid()}.{fileExtension}";

            using var stream = file.OpenReadStream();
            string url = await _minioStorage.UploadFileAsync(bucketName, objectName, stream, contentType);
            if (string.IsNullOrEmpty(url)) return null;

            var entidade = new ImagemNdvi
            {
                LinkImagem = url,
                DataImagem = dto.DataImagem,
                PercentualNuvens = dto.PercentualNuvens,
                NdviMax = dto.NdviMax,
                NdviMin = dto.NdviMin,
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
                PercentualNuvens = entidade.PercentualNuvens,
                NdviMax = entidade.NdviMax,
                NdviMin = entidade.NdviMin,
                TalhaoId = entidade.TalhaoId,
                FazendaId = entidade.FazendaId,
                DataInclusao = entidade.DataInclusao
            };
        }

        public async Task<IEnumerable<ImagemNdviOutputDTO>> GetByTalhaoIdAsync(Guid talhaoId)
        {
            var imagens = await _context.ImagensNdvi.Where(i => i.TalhaoId == talhaoId).ToListAsync();
            return _mapper.Map<IEnumerable<ImagemNdviOutputDTO>>(imagens);
        }
    }
}
