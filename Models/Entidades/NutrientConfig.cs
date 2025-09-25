using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace api.coleta.Models.Entidades
{
    public class NutrientConfig : Entity
    {
        public Guid? UserId { get; set; }

        [MaxLength(255)]
        public string? NutrientName { get; set; }

        public string? ConfigData { get; set; }  // JSON string

        public bool IsGlobal { get; set; }

        // Helper para desserializar ConfigData
        public NutrientConfigData? GetConfigData()
        {
            return ConfigData != null ? JsonSerializer.Deserialize<NutrientConfigData>(ConfigData) : null;
        }

        // Helper para serializar
        public void SetConfigData(NutrientConfigData data)
        {
            ConfigData = JsonSerializer.Serialize(data);
        }
    }

    public class NutrientConfigData
    {
        public List<List<object>>? Ranges { get; set; }  // [[min, max, color], ...]
    }
}