using System.Text.Json;

namespace api.coleta.Models.DTOs
{
    public class RelatorioDTO
    {
        public Guid? Id { get; set; }
        public IFormFile Arquivo { get; set; }
        public string ArquivoJson { get; set; }
        public string ColetaId { get; set; }
    }

    public class RelatorioOuputDTO
    {
        public Guid Id { get; set; }
        public string ColetaId { get; set; }
        public string JsonRelatorio { get; set; }
        public string LinkBackup { get; set; }
        public DateTime DataInclusao { get; set; }
    }
}
