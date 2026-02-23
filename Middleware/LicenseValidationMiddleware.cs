using api.cliente.Interfaces;
using api.cliente.Repositories;
using api.coleta.Repositories;
using System.Text.Json;

namespace api.coleta.Middleware
{
    public class LicenseValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LicenseValidationMiddleware> _logger;

        // Routes that don't require license validation
        private static readonly HashSet<string> PublicRoutes = new(StringComparer.OrdinalIgnoreCase)
        {
            "/api/plano",
            "/api/usuario/login",
            "/api/usuario/cadastrar",
            "/api/usuario/recuperar-senha",
            "/api/webhook",
            "/api/assinatura/criar-com-pix",
            "/api/assinatura/status",
            "/swagger",
            "/health"
        };

        public LicenseValidationMiddleware(
            RequestDelegate next,
            ILogger<LicenseValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            AssinaturaRepository assinaturaRepo,
            ClienteRepository clienteRepo,
            IJwtToken jwtToken)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Skip license check for public routes
            if (IsPublicRoute(path))
            {
                await _next(context);
                return;
            }

            // Skip if no authorization header (let normal auth handle it)
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                await _next(context);
                return;
            }

            try
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var userId = jwtToken.ObterUsuarioIdDoToken(token);

                if (userId == null)
                {
                    await _next(context);
                    return;
                }

                // Find cliente by user
                var clientes = clienteRepo.ListarTodosClientes(userId.Value);
                var cliente = clientes.FirstOrDefault();

                if (cliente == null)
                {
                    // No cliente linked to user - allow access (might be admin)
                    await _next(context);
                    return;
                }

                // Check for active subscription
                var assinatura = await assinaturaRepo.ObterAssinaturaAtivaDoClienteAsync(cliente.Id);

                if (assinatura == null || !assinatura.EstaVigente())
                {
                    _logger.LogWarning("License validation failed for cliente {ClienteId} - no active subscription",
                        cliente.Id);

                    context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                    context.Response.ContentType = "application/json";

                    var response = new
                    {
                        error = "Licença inválida ou expirada",
                        message = "Sua licença expirou ou não foi encontrada. Por favor, renove sua assinatura para continuar usando o sistema.",
                        code = "LICENSE_EXPIRED",
                        redirectTo = "/assinatura/planos"
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    return;
                }

                // License is valid, continue
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during license validation");
                // On error, allow request to continue (fail-open for now)
                await _next(context);
            }
        }

        private static bool IsPublicRoute(string path)
        {
            foreach (var route in PublicRoutes)
            {
                if (path.StartsWith(route, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }

    // Extension method for easy registration
    public static class LicenseValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseLicenseValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LicenseValidationMiddleware>();
        }
    }
}
