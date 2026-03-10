namespace api.coleta.Models.DTOs
{
    public class TokenPairDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }

    public class RefreshTokenRequestDTO
    {
        public string? RefreshToken { get; set; }
    }
}
