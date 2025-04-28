namespace api.coleta.Models.Entidades
{
    public class PontoColetado : Entity
    {
        public Guid PontoID { get; set; }
        public Guid ColetaID { get; set; }
        public Guid FuncionarioID { get; set; }
        public Guid HexagonID { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime DataColeta { get; set; }
    }
}