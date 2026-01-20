using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using api.fazenda.Models.Entidades;

namespace api.coleta.Models.Entidades
{
    public class Coleta : Entity
    {
        public Guid TalhaoID { get; set; }
        public virtual TalhaoJson? Talhao { get; set; }
        public Guid GeojsonID { get; set; }
        public virtual Geojson? Geojson { get; set; }
        public Guid UsuarioRespID { get; set; }
        public virtual Usuario? UsuarioResp { get; set; }
        [MaxLength(255)]
        public string? Observacao { get; set; }
        [MaxLength(255)]
        public string? NomeColeta { get; set; }
        public TipoColeta TipoColeta { get; set; }
        public List<TipoAnalise> TipoAnalise { get; set; }
        public Profundidade Profundidade { get; set; }
        public Guid UsuarioID { get; set; }
        public virtual Usuario? Usuario { get; set; }
        public Guid? SafraID { get; set; }
        public virtual Safra? Safra { get; set; }
        public Guid? FazendaID { get; set; }
        public virtual Fazenda? Fazenda { get; set; }
        public virtual ICollection<Relatorio>? Relatorios { get; set; }

        public Coleta()
        {
            TipoAnalise = new List<TipoAnalise>();
        }
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
        // Profundidades iniciando em 0
        ZeroADez,
        ZeroAVinte,
        ZeroATrinta,
        ZeroAQuarenta,
        ZeroACinquenta,
        ZeroASetenta,

        // Profundidades iniciando em 10
        DezAVinte,
        DezATrinta,
        DezAQuarenta,
        DezACinquenta,
        DezASetenta,

        // Profundidades iniciando em 20
        VinteATrinta,
        VinteAQuarenta,
        VinteACinquenta,
        VinteASetenta,

        // Profundidades iniciando em 30
        TrintaAQuarenta,
        TrintaACinquenta,
        TrintaASetenta,

        // Profundidades iniciando em 40
        QuarentaACinquenta,
        QuarentaASetenta,

        // Profundidades iniciando em 50
        CinquentaASetenta
    }
}
