namespace api.coleta.Models.DTOs
{
    public class NutrientConfigRequestDTO
    {
        public Guid? UserId { get; set; }  // null for global
        public required string NutrientName { get; set; }
        public required List<List<object>> Ranges { get; set; }  // [[min, max, color], ...]
    }

    public class NutrientConfigResponseDTO
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public required string NutrientName { get; set; }
        public required List<List<object>> Ranges { get; set; }
        public bool IsGlobal { get; set; }
        public DateTime DataInclusao { get; set; }
    }
}