using System.Data.Common;
using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using api.coleta.Utils;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Repositories;

public class GeoJsonRepository : GenericRepository<Geojson>
{
    public GeoJsonRepository(ApplicationDbContext context) : base(context) {}
    
    public Geojson Adicionar(Geojson entity)
    {
         Context.Add(entity);
        return Context.SaveChanges()  > 0 ? entity : null;
    }

    public void Atualizar(Entity entity)
    {
        Context.Update(entity);
    }

    public Geojson? ObterPorId(Guid id)
    {
        return DbSet.FirstOrDefault(x => x.Id == id);
    }

    public void Deletar(Entity entity)
    {
        Context.Remove(entity);
    }
    
    public Task<Geojson?> ListaGeojson(Guid userId, int page)
    {

        Task<Geojson?> geojson = Context.Geojson.FirstOrDefaultAsync();
        return geojson;
    }
}