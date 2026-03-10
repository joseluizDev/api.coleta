using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.cliente.Interfaces;
using api.coleta.Models.DTOs;
using api.coleta.Models.Entidades;
using api.coleta.Services;
using Microsoft.IdentityModel.Tokens;

namespace BackAppPromo.Infrastructure.Authentication
{
    public class JwtTokenService : IJwtToken
    {
        private readonly string _secretKey;
        private readonly string _refreshSecretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly RefreshTokenStore _refreshTokenStore;

        private const int AccessTokenMinutes = 20;
        private const int RefreshTokenDays = 7;

        public JwtTokenService(IConfiguration configuration, RefreshTokenStore refreshTokenStore)
        {
            _secretKey = configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey não configurado");
            _refreshSecretKey = configuration["Jwt:RefreshSecretKey"]
                ?? throw new InvalidOperationException("Jwt:RefreshSecretKey não configurado");
            _issuer = configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer não configurado");
            _audience = configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience não configurado");
            _refreshTokenStore = refreshTokenStore;
        }

        // ========== ACCESS TOKEN ==========

        public string GerarToken(Usuario usuario)
        {
            return CriarToken(usuario, "access", TimeSpan.FromMinutes(AccessTokenMinutes), _secretKey);
        }

        public bool ValidarToken(string token)
        {
            return ValidarComChave(token, _secretKey, "access");
        }

        public Guid? ObterUsuarioIdDoToken(string token)
        {
            return ExtrairUserId(token);
        }

        // ========== REFRESH TOKEN ==========

        public string GerarRefreshToken(Usuario usuario)
        {
            var token = CriarToken(usuario, "refresh", TimeSpan.FromDays(RefreshTokenDays), _refreshSecretKey);
            _refreshTokenStore.Adicionar(token, usuario.Id, DateTime.UtcNow.AddDays(RefreshTokenDays));
            return token;
        }

        public bool ValidarRefreshToken(string refreshToken)
        {
            // 1. Valida assinatura + expiração + tipo do JWT
            if (!ValidarComChave(refreshToken, _refreshSecretKey, "refresh"))
                return false;

            // 2. Valida presença no store em memória
            return _refreshTokenStore.Validar(refreshToken);
        }

        public Guid? ObterUsuarioIdDoRefreshToken(string refreshToken)
        {
            if (!ValidarRefreshToken(refreshToken))
                return null;
            return ExtrairUserId(refreshToken);
        }

        // ========== PAR DE TOKENS ==========

        public TokenPairDTO GerarParDeTokens(Usuario usuario)
        {
            return new TokenPairDTO
            {
                AccessToken = GerarToken(usuario),
                RefreshToken = GerarRefreshToken(usuario),
                ExpiresIn = AccessTokenMinutes * 60
            };
        }

        // ========== HELPERS PRIVADOS ==========

        private string CriarToken(Usuario usuario, string tokenType, TimeSpan duracao, string secret)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, usuario.Id.ToString()),
                    new Claim("token_type", tokenType),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.Add(duracao),
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _issuer,
                Audience = _audience
            };
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        private bool ValidarComChave(string token, string secret, string tipoEsperado)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwt = (JwtSecurityToken)validatedToken;
                var tipoClaim = jwt.Claims.FirstOrDefault(c => c.Type == "token_type")?.Value;
                return tipoClaim == tipoEsperado;
            }
            catch
            {
                return false;
            }
        }

        private Guid? ExtrairUserId(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(token)) return null;
                var jwt = handler.ReadToken(token) as JwtSecurityToken;
                var claim = jwt?.Claims.FirstOrDefault(c => c.Type == "unique_name");
                return Guid.TryParse(claim?.Value, out var id) ? id : null;
            }
            catch { return null; }
        }
    }
}
