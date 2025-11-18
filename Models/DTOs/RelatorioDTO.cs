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
        public string NomeCliente { get; set; }
        public string Safra { get; set; }
        public string Funcionario { get; set; }
        public string Observacao { get; set; }
        public string Profundidade { get; set; }
        public List<string> TiposAnalise { get; set; } = [];
        public string? JsonRelatorio { get; set; }
        public bool IsRelatorio { get; set; }
    }

    public class AtualizarJsonRelatorioDTO
    {
        public Guid ColetaId { get; set; }
        public Guid RelatorioId { get; set; }
        public string JsonRelatorio { get; set; } = string.Empty;
    }

    public class RelatorioCompletoOutputDTO
    {
        public Guid Id { get; set; }
        public string ColetaId { get; set; }
        public string LinkBackup { get; set; }
        public DateTime DataInclusao { get; set; }
        public string NomeColeta { get; set; }
        public string Talhao { get; set; }
        public string TipoColeta { get; set; }
        public string Fazenda { get; set; }
        public string NomeCliente { get; set; }
        public string Safra { get; set; }
        public string Funcionario { get; set; }
        public string Observacao { get; set; }
        public string Profundidade { get; set; }
        public List<string> TiposAnalise { get; set; } = [];
        public string? JsonRelatorio { get; set; }
        public bool IsRelatorio { get; set; }
        
        // Dados da coleta (mapa, grid, pontos)
        public ColetaDadosDTO? DadosColeta { get; set; }
    }

    public class ColetaDadosDTO
    {
        public Guid ColetaId { get; set; }
        public object? Geojson { get; set; }
        public object? Talhao { get; set; }
        public object? UsuarioResp { get; set; }
        public Guid GeoJsonID { get; set; }
        public Guid UsuarioRespID { get; set; }
        public int Zonas { get; set; }
        public decimal? AreaHa { get; set; }
    }
}
