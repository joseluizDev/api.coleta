using api.cliente.Models.DTOs;
using api.fazenda.models;
using System.Collections.Generic;

namespace api.talhao.Models.DTOs
{
    public class QueryTalhao
    {
        public int? Page { get; set; }
        public Guid? FazendaID { get; set; }
    }
    public class TalhaoRequestDTO
    {
        public Guid? Id { get; set; }
        public Guid FazendaID { get; set; }
        public Guid ClienteID { get; set; }
        public List<Talhoes> Talhoes { get; set; }
    }

    public class TalhaoResponseDTO
    {
        public Guid Id { get; set; }
        public Guid FazendaID { get; set; }
        public FazendaResponseDTO Fazenda { get; set; }
        public ClienteResponseDTO Cliente { get; set; }
        public Guid ClienteID { get; set; }
        public List<Talhoes> Talhoes { get; set; }
    }

    public class Talhoes
    {
        public Guid? Id { get; set; }
        public double Area { get; set; }
        public string Nome { get; set; }
        public string? observacao { get; set; }
        public Guid TalhaoID { get; set; }

        public List<Coordenada> Coordenadas { get; set; }
    }

    public class Coordenada
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    // DTO para listagem de talhões agrupados por fazenda
    public class TalhaoAgrupadoPorFazendaResponseDTO
    {
        public Guid FazendaID { get; set; }
        public string NomeFazenda { get; set; }
        public List<TalhaoResumoDTO> Talhoes { get; set; }
    }

    public class TalhaoResumoDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public double Area { get; set; }
        public string? Observacao { get; set; }
    }

    public class AtualizarNomeTalhaoJsonDTO
    {
        public string Nome { get; set; }
        public string? Observacao { get; set; }
    }
}
