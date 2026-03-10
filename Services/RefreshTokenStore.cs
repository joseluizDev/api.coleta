using System.Collections.Concurrent;

namespace api.coleta.Services
{
    /// <summary>
    /// Armazena refresh tokens em memória com limpeza automática de tokens expirados.
    /// Singleton registrado no DI.
    /// </summary>
    public class RefreshTokenStore : IDisposable
    {
        private readonly ConcurrentDictionary<string, RefreshTokenEntry> _tokens = new();
        private readonly Timer _cleanupTimer;

        private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);

        public RefreshTokenStore()
        {
            // Limpeza automática a cada 1 hora
            _cleanupTimer = new Timer(LimparTokensExpirados, null, CleanupInterval, CleanupInterval);
        }

        /// <summary>
        /// Adiciona um refresh token ao store.
        /// </summary>
        public void Adicionar(string refreshToken, Guid userId, DateTime expiresAt)
        {
            _tokens[refreshToken] = new RefreshTokenEntry
            {
                UserId = userId,
                ExpiresAt = expiresAt,
                CriadoEm = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Valida se o refresh token existe no store e não expirou.
        /// </summary>
        public bool Validar(string refreshToken)
        {
            if (!_tokens.TryGetValue(refreshToken, out var entry))
                return false;

            if (entry.ExpiresAt < DateTime.UtcNow)
            {
                _tokens.TryRemove(refreshToken, out _);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Remove um refresh token (usado na rotação e logout).
        /// </summary>
        public void Revogar(string refreshToken)
        {
            _tokens.TryRemove(refreshToken, out _);
        }

        /// <summary>
        /// Revoga todos os refresh tokens de um usuário (logout completo).
        /// </summary>
        public void RevogarTodosDoUsuario(Guid userId)
        {
            var tokensDoUsuario = _tokens.Where(t => t.Value.UserId == userId).Select(t => t.Key).ToList();
            foreach (var token in tokensDoUsuario)
            {
                _tokens.TryRemove(token, out _);
            }
        }

        /// <summary>
        /// Retorna quantidade de tokens ativos (para debug/monitoramento).
        /// </summary>
        public int ContarAtivos() => _tokens.Count(t => t.Value.ExpiresAt >= DateTime.UtcNow);

        private void LimparTokensExpirados(object? state)
        {
            var agora = DateTime.UtcNow;
            var expirados = _tokens.Where(t => t.Value.ExpiresAt < agora).Select(t => t.Key).ToList();
            foreach (var token in expirados)
            {
                _tokens.TryRemove(token, out _);
            }
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
        }
    }

    public class RefreshTokenEntry
    {
        public Guid UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
