using api.coleta.Data.Repositories;
using api.coleta.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Repositories
{
    public class NutrientConfigRepository : GenericRepository<NutrientConfig>
    {
        public NutrientConfigRepository(ApplicationDbContext context) : base(context)
        { }

        public void SalvarNutrientConfig(NutrientConfig config)
        {
            Adicionar(config);
        }

        public NutrientConfig? BuscarNutrientConfigPorId(Guid id)
        {
            return ObterPorId(id);
        }

        public void AtualizarNutrientConfig(NutrientConfig config)
        {
            Atualizar(config);
        }

        public void DeletarNutrientConfig(NutrientConfig config)
        {
            Deletar(config);
        }

        public List<NutrientConfig> ListarNutrientConfigsPorUsuario(Guid? usuarioId)
        {
            return Context.NutrientConfigs
                .Where(c => c.UserId == usuarioId || (usuarioId == null && c.IsGlobal))
                .ToList();
        }

        public List<NutrientConfig> ListarNutrientConfigsComFallback(Guid? usuarioId)
        {
            // Pegar personalizadas do usuário
            var personalizadas = Context.NutrientConfigs.Where(c => c.UserId == usuarioId).ToList();
            var nutrientNames = personalizadas.Select(c => c.NutrientName).ToHashSet();

            // Pegar globais que não têm personalizada correspondente
            var globais = Context.NutrientConfigs.Where(c => c.IsGlobal && !nutrientNames.Contains(c.NutrientName)).ToList();

            return personalizadas.Concat(globais).ToList();
        }

        public NutrientConfig? BuscarNutrientConfigPorNomeEUsuario(string nutrientName, Guid? usuarioId)
        {
            return Context.NutrientConfigs
                .FirstOrDefault(c => c.NutrientName == nutrientName && (c.UserId == usuarioId || (usuarioId == null && c.IsGlobal)));
        }

        // Para fallback: personalizada ou global
        public NutrientConfig? BuscarNutrientConfigComFallback(string nutrientName, Guid? usuarioId)
        {
            var config = Context.NutrientConfigs
                .Where(c => c.NutrientName == nutrientName && c.UserId == usuarioId)
                .FirstOrDefault();
            if (config == null)
            {
                config = Context.NutrientConfigs
                    .Where(c => c.NutrientName == nutrientName && c.IsGlobal)
                    .FirstOrDefault();
            }
            return config;
        }

        public NutrientConfig? BuscarNutrientConfigPersonalizadaPorNomeEUsuario(string nutrientName, Guid userId)
        {
            return Context.NutrientConfigs
                .FirstOrDefault(c => c.NutrientName == nutrientName && c.UserId == userId && !c.IsGlobal);
        }

        public List<NutrientConfig> ListarGlobais()
        {
            return Context.NutrientConfigs.Where(c => c.IsGlobal).ToList();
        }

        public List<NutrientConfig> ListarPersonalizadas(Guid userId)
        {
            return Context.NutrientConfigs.Where(c => !c.IsGlobal && c.UserId == userId).ToList();
        }
    }
}