using System.Text;
using api.cliente.Interfaces;
using api.minionStorage.Services;
using BackAppPromo.Infrastructure.Authentication;

namespace api.coleta.Configuration
{
    public static class AuthConfig
    {
        public static IServiceCollection AddAuthConfiguration(this IServiceCollection services)
        {
            var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? throw new InvalidOperationException("JWT_SECRET_KEY não configurado no .env");
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                ?? throw new InvalidOperationException("JWT_ISSUER não configurado no .env");
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                ?? throw new InvalidOperationException("JWT_AUDIENCE não configurado no .env");

            services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSecretKey)
                        ),
                        ValidateIssuer = true,
                        ValidIssuer = jwtIssuer,
                        ValidateAudience = true,
                        ValidAudience = jwtAudience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddScoped<IJwtToken, JwtTokenService>();

            return services;
        }

        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowAnyOrigin", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            return services;
        }

        public static IServiceCollection AddMinioConfiguration(this IServiceCollection services)
        {
            var minioEndpoint = Environment.GetEnvironmentVariable("MINIO_ENDPOINT")
                ?? throw new InvalidOperationException("MINIO_ENDPOINT não configurado no .env");
            var minioAccessKey = Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY")
                ?? throw new InvalidOperationException("MINIO_ACCESS_KEY não configurado no .env");
            var minioSecretKey = Environment.GetEnvironmentVariable("MINIO_SECRET_KEY")
                ?? throw new InvalidOperationException("MINIO_SECRET_KEY não configurado no .env");
            var minioPublicUrl = Environment.GetEnvironmentVariable("MINIO_PUBLIC_URL")
                ?? throw new InvalidOperationException("MINIO_PUBLIC_URL não configurado no .env");

            services.AddSingleton<IMinioStorage>(provider =>
                new MinioStorage(
                    endpoint: minioEndpoint,
                    accessKey: minioAccessKey,
                    secretKey: minioSecretKey,
                    publicUrl: minioPublicUrl
                )
            );

            return services;
        }
    }
}
