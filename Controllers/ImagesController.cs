using api.cliente.Interfaces;
using api.coleta.Models.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImagesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMinioStorage _minioStorage;
        private readonly IJwtToken _jwtToken;

        public ImagesController(
            INotificador notificador,
            ApplicationDbContext context,
            IMinioStorage minioStorage,
            IJwtToken jwtToken) : base(notificador)
        {
            _context = context;
            _minioStorage = minioStorage;
            _jwtToken = jwtToken;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile arquivo)
        {
            try
            {
                var token = ObterIDDoToken();
                var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userIdNullable == null)
                    return BadRequest(new { message = "Token inválido" });

                Guid userId = (Guid)userIdNullable;

                if (arquivo == null || arquivo.Length == 0)
                    return BadRequest(new { message = "Arquivo não enviado" });

                string bucketName = "coleta";
                string fileExtension = Path.GetExtension(arquivo.FileName).TrimStart('.');
                string objectName = $"images/{userId}/{Guid.NewGuid()}.{fileExtension}";
                string contentType = arquivo.ContentType;

                using var stream = arquivo.OpenReadStream();
                string url = await _minioStorage.UploadFileAsync(bucketName, objectName, stream, contentType);

                if (string.IsNullOrEmpty(url))
                    return BadRequest(new { message = "Erro ao fazer upload do arquivo" });

                var imagem = new Imagem
                {
                    Url = url,
                    ObjectName = objectName,
                    BucketName = bucketName,
                    ContentType = contentType,
                    NomeOriginal = arquivo.FileName,
                    TamanhoBytes = arquivo.Length,
                    UsuarioId = userId
                };

                await _context.Imagens.AddAsync(imagem);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    id = imagem.Id,
                    url = imagem.Url,
                    nomeOriginal = imagem.NomeOriginal,
                    contentType = imagem.ContentType,
                    tamanhoBytes = imagem.TamanhoBytes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao processar upload", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var imagem = await _context.Imagens.FindAsync(id);
            if (imagem == null)
                return NotFound(new { message = "Imagem não encontrada" });

            return Ok(new
            {
                id = imagem.Id,
                url = imagem.Url,
                nomeOriginal = imagem.NomeOriginal,
                contentType = imagem.ContentType,
                tamanhoBytes = imagem.TamanhoBytes,
                dataInclusao = imagem.DataInclusao
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var token = ObterIDDoToken();
                var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);
                if (userIdNullable == null)
                    return BadRequest(new { message = "Token inválido" });

                var imagem = await _context.Imagens.FindAsync(id);
                if (imagem == null)
                    return NotFound(new { message = "Imagem não encontrada" });

                // Deletar do MinIO
                await _minioStorage.DeleteFileAsync(imagem.BucketName, imagem.ObjectName);

                // Deletar do banco
                _context.Imagens.Remove(imagem);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Imagem deletada com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao deletar imagem", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListByUser()
        {
            var token = ObterIDDoToken();
            var userIdNullable = _jwtToken.ObterUsuarioIdDoToken(token);
            if (userIdNullable == null)
                return BadRequest(new { message = "Token inválido" });

            Guid userId = (Guid)userIdNullable;

            var imagens = await _context.Imagens
                .Where(i => i.UsuarioId == userId)
                .OrderByDescending(i => i.DataInclusao)
                .Select(i => new
                {
                    id = i.Id,
                    url = i.Url,
                    nomeOriginal = i.NomeOriginal,
                    contentType = i.ContentType,
                    tamanhoBytes = i.TamanhoBytes,
                    dataInclusao = i.DataInclusao
                })
                .ToListAsync();

            return Ok(imagens);
        }
    }
}
