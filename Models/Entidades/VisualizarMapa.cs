using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.coleta.Models.Entidades
{
    public class VisualizarMapa : Entity
    {
        public Guid TalhaoID { get; set; }
        public virtual TalhaoJson Talhao { get; set; }
        public Guid GeojsonID { get; set; }
        public virtual Geojson Geojson { get; set; }
        public Guid FuncionarioID { get; set; }
        public virtual Funcionario Funcionario { get; set; }
        [MaxLength(255)]
        public string? Observacao { get; set; }
        public TipoColeta TipoColeta { get; set; }
        public TipoAnalise TipoAnalise { get; set; }
        public Profundidade Profundidade { get; set; }
        public Guid UsuarioID { get; set; }
        public virtual Usuario Usuario { get; set; }
    }

    public enum TipoColeta
    {
        Hexagonal,
        Retangular,
        PontosAmostrais
    }

    public enum TipoAnalise
    {
        Macronutrientes,
        Micronutrientes,
        Textura,
        Microbiologica,
        BioAs,
        Compactacao,
        Outros
    }

    public enum Profundidade
    {
        ZeroADez,
        ZeroAVinte,
        ZeroATrinta,
        ZeroAQuarenta,
        ZeroACinquenta,
        ZeroASetenta,
        DezAVinte,
        VinteATrinta,
        TrintaAQuarenta,
        QuarentaACinquenta,
        CinquentaASetenta
    }
}
