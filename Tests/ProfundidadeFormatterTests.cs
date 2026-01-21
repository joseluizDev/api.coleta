using api.coleta.Models.Entidades;
using api.coleta.Utils;
using Xunit;

namespace api.coleta.Tests
{
    public class ProfundidadeFormatterTests
    {
        [Theory]
        [InlineData(Profundidade.ZeroADez, "0-10")]
        [InlineData(Profundidade.ZeroAVinte, "0-20")]
        [InlineData(Profundidade.ZeroATrinta, "0-30")]
        [InlineData(Profundidade.ZeroAQuarenta, "0-40")]
        [InlineData(Profundidade.ZeroACinquenta, "0-50")]
        [InlineData(Profundidade.ZeroASetenta, "0-70")]
        [InlineData(Profundidade.DezAVinte, "10-20")]
        [InlineData(Profundidade.DezATrinta, "10-30")]
        [InlineData(Profundidade.DezAQuarenta, "10-40")]
        [InlineData(Profundidade.DezACinquenta, "10-50")]
        [InlineData(Profundidade.DezASetenta, "10-70")]
        [InlineData(Profundidade.VinteATrinta, "20-30")]
        [InlineData(Profundidade.VinteAQuarenta, "20-40")]
        [InlineData(Profundidade.VinteACinquenta, "20-50")]
        [InlineData(Profundidade.VinteASetenta, "20-70")]
        [InlineData(Profundidade.TrintaAQuarenta, "30-40")]
        [InlineData(Profundidade.TrintaACinquenta, "30-50")]
        [InlineData(Profundidade.TrintaASetenta, "30-70")]
        [InlineData(Profundidade.QuarentaACinquenta, "40-50")]
        [InlineData(Profundidade.QuarentaASetenta, "40-70")]
        [InlineData(Profundidade.CinquentaASetenta, "50-70")]
        public void Formatar_deve_retornar_nomeclatura_para_todos_os_valores(Profundidade profundidade, string esperado)
        {
            Assert.Equal(esperado, ProfundidadeFormatter.Formatar(profundidade));
            Assert.Equal(esperado, ProfundidadeFormatter.Formatar(profundidade.ToString()));
        }
    }
}
