using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Repositories
{
    public class ImagemNdviRepository : GenericRepository<ImagemNdvi>
    {
        public ImagemNdviRepository(ApplicationDbContext context) : base(context) { }

        public void Adicionar(ImagemNdvi entity) => Context.Add(entity);
        public Task<ImagemNdvi?> ObterPorId(Guid id) => Context.ImagensNdvi.FirstOrDefaultAsync(x => x.Id == id);
        public Task<List<ImagemNdvi>> ListarPorTalhao(Guid talhaoId) => Context.ImagensNdvi.Where(x => x.TalhaoId == talhaoId).OrderByDescending(x => x.DataImagem).ToListAsync();
    }
}
