
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
}
