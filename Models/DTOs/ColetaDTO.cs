
using api.coleta.Models.Entidades;
using api.safra.Models.DTOs;
using api.talhao.Models.DTOs;

namespace api.coleta.models
{
    public class ColetaRequestDTO
    {
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string reference { get; set; }
        public string Place_Id { get; set; }
    }

    public class ColetaResponseDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string reference { get; set; }
        public string Place_Id { get; set; }
    }

    public class ColetaPorUsuarioDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }

        public List<string> TipoAnalise { get; set; }

        public SafraResponseDTO Safra { get; set; }
        
        public TalhaoResponseDTO Talhao { get; set; }
        

        // Dados resumidos de Cliente



    }

    

    public class ItemColetaDto
    {
        public int Id { get; set; }
        public string Produto { get; set; }
        public decimal Quantidade { get; set; }
    }
}
