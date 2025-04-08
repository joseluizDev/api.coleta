using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;

namespace api.coleta.Utils.Maps
{
    public static class MineralMapDto
    {
        public static MineralDTO MapMineral(this Minerais mineral)
        {
            return new MineralDTO
            {
                Id = mineral.Id,
                Nome = mineral.Nome,
                DataInclusao = mineral.DataInclusao.ToString("dd/MM/yyyy")
            };
        }
        public static List<MineralDTO> MapMineral(this List<Minerais> minerals)
        {
            return minerals.Select(x => x.MapMineral()).ToList();
        }
    }
}
