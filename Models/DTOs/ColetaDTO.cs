using System.Collections.Generic;

namespace api.coleta.models
{
    public class ColetasRequestDTO
    {
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string reference { get; set; }
        public string Place_Id { get; set; }
    }

    public class ColetasResponseDTO
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
