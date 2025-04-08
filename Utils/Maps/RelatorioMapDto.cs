using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;

namespace api.coleta.Utils.Maps
{
    public static class RelatorioMapDto
    {
        public static Relatorio MapRelatorio(this RelatorioDTO relatorio)
        {
            return new Relatorio
            {
                JsonRelatorio = relatorio.ArquivoJson,
                ColetaId = Guid.Parse(relatorio.ColetaId),
            };
        }

        public static RelatorioOuputDTO MapRelatorio(this Relatorio relatorio)
        {
            return new RelatorioOuputDTO
            {
                Id = relatorio.Id,
                JsonRelatorio = relatorio.JsonRelatorio,
                ColetaId = relatorio.ColetaId.ToString(),
                LinkBackup = relatorio.LinkBackup,
            };
        }

        // Fix: Change the parameter type from Relatorio to IEnumerable<Relatorio>
        public static List<RelatorioOuputDTO> MapRelatorio(this IEnumerable<Relatorio> relatorios)
        {
            return relatorios.Select(x => x.MapRelatorio()).ToList();
        }
    }
}
