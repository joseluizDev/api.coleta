using System.Collections.Generic;

namespace api.coleta.Models.DTOs
{
    /// <summary>
    /// DTO para query de relatórios mobile com filtros
    /// </summary>
    public class QueryRelatorioMobile
    {
        public string? Fazenda { get; set; }
        public string? Talhao { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
    }

    /// <summary>
    /// DTO de resposta para listagem de relatórios mobile
    /// </summary>
    public class RelatorioMobileResponseDTO
    {
        public bool Success { get; set; }
        public List<RelatorioMobileItemDTO> Data { get; set; } = new();
        public PaginationDTO Pagination { get; set; } = new();
    }

    /// <summary>
    /// DTO de item individual de relatório
    /// </summary>
    public class RelatorioMobileItemDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Fazenda { get; set; } = string.Empty;
        public string Talhao { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public int PontosColetados { get; set; }
        public int TotalPontos { get; set; }
        public string Profundidade { get; set; } = string.Empty;
        public string Grid { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public List<PontoColetaMobileDTO> Pontos { get; set; } = new();
    }

    /// <summary>
    /// DTO de ponto de coleta para mobile
    /// </summary>
    public class PontoColetaMobileDTO
    {
        public string Id { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DadosAmostraDTO? DadosAmostra { get; set; }
        public bool Coletado { get; set; }
    }

    /// <summary>
    /// DTO de dados de amostra de solo (futuro)
    /// </summary>
    public class DadosAmostraDTO
    {
        public double? pH { get; set; }
        public double? MateriaOrganica { get; set; }
        public double? Fosforo { get; set; }
        public double? Potassio { get; set; }
    }

    /// <summary>
    /// DTO de paginação
    /// </summary>
    public class PaginationDTO
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
    }

    /// <summary>
    /// DTO de resposta de erro
    /// </summary>
    public class ErrorResponseDTO
    {
        public bool Success { get; set; } = false;
        public ErrorDetailDTO Error { get; set; } = new();
    }

    /// <summary>
    /// DTO de detalhes do erro
    /// </summary>
    public class ErrorDetailDTO
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

