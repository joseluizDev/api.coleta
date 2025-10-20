using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using System.Linq;

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
            var coleta = relatorio.Coleta;
            var talhaoJson = coleta?.Talhao;
            var talhaoEntity = talhaoJson?.Talhao;
            var fazenda = talhaoEntity?.Fazenda ?? coleta?.Safra?.Fazenda;
            var cliente = fazenda?.Cliente;
            var tiposAnalise = coleta?.TipoAnalise?
                .Select(x => x.ToString())
                .ToList() ?? [];

            return new RelatorioOuputDTO
            {
                Id = relatorio.Id,
                ColetaId = relatorio.ColetaId.ToString(),
                LinkBackup = relatorio.LinkBackup,
                DataInclusao = relatorio.DataInclusao,
                NomeColeta = !string.IsNullOrWhiteSpace(coleta?.NomeColeta) ? coleta.NomeColeta : "N/A",
                Talhao = !string.IsNullOrWhiteSpace(talhaoJson?.Nome) ? talhaoJson.Nome : "N/A",
                TipoColeta = coleta != null ? coleta.TipoColeta.ToString() : "N/A",
                Fazenda = !string.IsNullOrWhiteSpace(fazenda?.Nome) ? fazenda.Nome : "N/A",
                NomeCliente = !string.IsNullOrWhiteSpace(cliente?.Nome) ? cliente.Nome : "N/A",
                Safra = coleta?.Safra != null && !string.IsNullOrWhiteSpace(coleta.Safra.Observacao)
                    ? coleta.Safra.Observacao
                    : coleta?.Safra != null
                        ? coleta.Safra.DataInicio.ToString("dd/MM/yyyy")
                        : "N/A",
                Funcionario = !string.IsNullOrWhiteSpace(coleta?.UsuarioResp?.NomeCompleto) ? coleta.UsuarioResp.NomeCompleto : "N/A",
                Observacao = !string.IsNullOrWhiteSpace(coleta?.Observacao) ? coleta.Observacao : "N/A",
                Profundidade = coleta != null ? ProfundidadeFormatter.Formatar(coleta.Profundidade.ToString()) : "N/A",
                TiposAnalise = tiposAnalise,
                JsonRelatorio = relatorio.JsonRelatorio,
                IsRelatorio = !string.IsNullOrWhiteSpace(relatorio.JsonRelatorio)
            };
        }

        // Fix: Change the parameter type from Relatorio to IEnumerable<Relatorio>
        public static List<RelatorioOuputDTO> MapRelatorio(this IEnumerable<Relatorio> relatorios)
        {
            return relatorios.Select(x => x.MapRelatorio()).ToList();
        }
    }
}
