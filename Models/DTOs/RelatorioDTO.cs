using System.Collections.Generic;

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
        public string LinkBackup { get; set; }
        public DateTime DataInclusao { get; set; }
        public string NomeColeta { get; set; }
        public string Talhao { get; set; }
        public string TipoColeta { get; set; }
        public string Fazenda { get; set; }
        public string Safra { get; set; }
        public string Funcionario { get; set; }
        public string Observacao { get; set; }
        public string Profundidade { get; set; }
        public List<string> TiposAnalise { get; set; } = [];
        public string? JsonRelatorio { get; set; }
        public bool IsRelatorio { get; set; }
    }
}
