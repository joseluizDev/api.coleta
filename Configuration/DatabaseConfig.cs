using api.coleta.Data;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Configuration
{
    public static class DatabaseConfig
    {
        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services)
        {
            var dbServer = Environment.GetEnvironmentVariable("DB_SERVER")
                ?? throw new InvalidOperationException("DB_SERVER não configurado no .env");
            var dbPort = Environment.GetEnvironmentVariable("DB_PORT")
                ?? throw new InvalidOperationException("DB_PORT não configurado no .env");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME")
                ?? throw new InvalidOperationException("DB_NAME não configurado no .env");
            var dbUser = Environment.GetEnvironmentVariable("DB_USER")
                ?? throw new InvalidOperationException("DB_USER não configurado no .env");
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
                ?? throw new InvalidOperationException("DB_PASSWORD não configurado no .env");

            var connectionString = $"server={dbServer};port={dbPort};database={dbName};user={dbUser};password={dbPassword};";

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var versao = ServerVersion.AutoDetect(connectionString);
                options.UseMySql(connectionString, versao);
            });

            services.AddScoped<ApplicationDbContext>();

            return services;
        }

        public static WebApplication ApplyMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            // Pular migrations para InMemory database (testes)
            if (db.Database.IsInMemory())
            {
                logger.LogInformation("InMemory database detectado. Pulando migrations.");
                return app;
            }

            try
            {
                ApplyMigrationBaseline(db, logger);

                var pendingMigrations = db.Database.GetPendingMigrations().ToList();
                if (pendingMigrations.Count > 0)
                {
                    logger.LogInformation("Aplicando {Count} migration(s) pendente(s): {Migrations}",
                        pendingMigrations.Count,
                        string.Join(", ", pendingMigrations));

                    db.Database.Migrate();
                    logger.LogInformation("Migrations aplicadas com sucesso!");
                }
                else
                {
                    logger.LogInformation("Banco de dados já está atualizado. Nenhuma migration pendente.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao aplicar migrations.");
                throw;
            }

            return app;
        }

        public static WebApplication EnsureLicensingTables(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (db.Database.IsInMemory())
                return app;

            try
            {
                var needsRecreate = false;
                try
                {
                    db.Database.ExecuteSqlRaw("SELECT ValorAnual FROM Planos LIMIT 1");
                    db.Database.ExecuteSqlRaw("SELECT Ativa, AutoRenovar, StatusPagamento FROM Assinaturas LIMIT 1");
                }
                catch
                {
                    needsRecreate = true;
                }

                if (needsRecreate)
                {
                    RecreateLicensingTables(db);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Note: Could not setup licensing tables: {ex.Message}");
            }

            return app;
        }

        private static void ApplyMigrationBaseline(ApplicationDbContext db, ILogger logger)
        {
            var appliedMigrations = db.Database.GetAppliedMigrations().ToList();
            if (appliedMigrations.Count > 0) return;

            var tabelaExiste = false;
            try
            {
                var conn = db.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ConfiguracaoPadraos'";
                var result = cmd.ExecuteScalar();
                tabelaExiste = Convert.ToInt64(result) > 0;
            }
            catch (Exception exCheck)
            {
                logger.LogWarning(exCheck, "Falha ao verificar existência de tabelas no banco.");
            }

            if (!tabelaExiste) return;

            logger.LogInformation("Banco existente sem histórico de migrations. Aplicando baseline...");

            var migrationsBaseline = new[]
            {
                "20251226144718_InitialCreate",
                "20251227162801_SyncLicensingSystem",
                "20251227192938_AddPaymentMethodFields",
                "20251227200159_AddEfiPayPlanIdIntColumn"
            };

            foreach (var migration in migrationsBaseline)
            {
                db.Database.ExecuteSqlRaw(
                    "INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) VALUES ({0}, {1})",
                    migration, "8.0.2");
                logger.LogInformation("Baseline: registrada migration {Migration}", migration);
            }
        }

        private static void RecreateLicensingTables(ApplicationDbContext db)
        {
            Console.WriteLine("Recreating licensing tables with correct schema...");

            db.Database.ExecuteSqlRaw(@"
                SET FOREIGN_KEY_CHECKS = 0;
                DROP TABLE IF EXISTS `HistoricosPagamento`;
                DROP TABLE IF EXISTS `Assinaturas`;
                DROP TABLE IF EXISTS `Planos`;
                SET FOREIGN_KEY_CHECKS = 1;
            ");

            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE `Planos` (
                    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
                    `Nome` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                    `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL,
                    `ValorAnual` decimal(12,4) NOT NULL,
                    `LimiteHectares` decimal(12,4) NOT NULL,
                    `Ativo` tinyint(1) NOT NULL DEFAULT 1,
                    `RequereContato` tinyint(1) NOT NULL DEFAULT 0,
                    `EfiPayPlanId` varchar(50) CHARACTER SET utf8mb4 NULL,
                    `DataInclusao` datetime(6) NOT NULL,
                    CONSTRAINT `PK_Planos` PRIMARY KEY (`Id`)
                ) CHARACTER SET=utf8mb4
            ");

            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE `Assinaturas` (
                    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
                    `ClienteId` char(36) COLLATE ascii_general_ci NOT NULL,
                    `PlanoId` char(36) COLLATE ascii_general_ci NOT NULL,
                    `DataInicio` datetime(6) NOT NULL,
                    `DataFim` datetime(6) NOT NULL,
                    `Ativa` tinyint(1) NOT NULL DEFAULT 0,
                    `AutoRenovar` tinyint(1) NOT NULL DEFAULT 0,
                    `Observacao` varchar(500) CHARACTER SET utf8mb4 NULL,
                    `DeletadoEm` datetime(6) NULL,
                    `EfiPaySubscriptionId` varchar(100) CHARACTER SET utf8mb4 NULL,
                    `EfiPayPlanId` varchar(50) CHARACTER SET utf8mb4 NULL,
                    `StatusPagamento` varchar(50) CHARACTER SET utf8mb4 NULL,
                    `DataUltimoPagamento` datetime(6) NULL,
                    `DataInclusao` datetime(6) NOT NULL,
                    CONSTRAINT `PK_Assinaturas` PRIMARY KEY (`Id`),
                    CONSTRAINT `FK_Assinaturas_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE CASCADE,
                    CONSTRAINT `FK_Assinaturas_Planos_PlanoId` FOREIGN KEY (`PlanoId`) REFERENCES `Planos` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4
            ");

            db.Database.ExecuteSqlRaw(@"
                CREATE TABLE `HistoricosPagamento` (
                    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
                    `AssinaturaId` char(36) COLLATE ascii_general_ci NOT NULL,
                    `Valor` decimal(12,4) NOT NULL,
                    `DataPagamento` datetime(6) NOT NULL,
                    `MetodoPagamento` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
                    `Status` int NOT NULL DEFAULT 0,
                    `TransacaoId` varchar(255) CHARACTER SET utf8mb4 NULL,
                    `Observacao` longtext CHARACTER SET utf8mb4 NULL,
                    `DeletadoEm` datetime(6) NULL,
                    `EfiPayChargeId` varchar(100) CHARACTER SET utf8mb4 NULL,
                    `EfiPayStatus` varchar(50) CHARACTER SET utf8mb4 NULL,
                    `PixQrCode` longtext CHARACTER SET utf8mb4 NULL,
                    `PixQrCodeBase64` longtext CHARACTER SET utf8mb4 NULL,
                    `PixTxId` varchar(100) CHARACTER SET utf8mb4 NULL,
                    `DataExpiracao` datetime(6) NULL,
                    `DataInclusao` datetime(6) NOT NULL,
                    CONSTRAINT `PK_HistoricosPagamento` PRIMARY KEY (`Id`),
                    CONSTRAINT `FK_HistoricosPagamento_Assinaturas_AssinaturaId` FOREIGN KEY (`AssinaturaId`) REFERENCES `Assinaturas` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4
            ");

            db.Database.ExecuteSqlRaw(@"
                INSERT INTO `Planos` (`Id`, `Nome`, `Descricao`, `ValorAnual`, `LimiteHectares`, `Ativo`, `RequereContato`, `DataInclusao`)
                VALUES
                (UUID(), 'Básico', 'Plano ideal para pequenos produtores. Inclui todas as funcionalidades essenciais para até 1.000 hectares.', 3598.00, 1000, 1, 0, NOW()),
                (UUID(), 'Premium', 'Plano completo para produtores de médio porte. Todas as funcionalidades para até 2.000 hectares.', 6599.80, 2000, 1, 0, NOW()),
                (UUID(), 'Gold', 'Plano personalizado para grandes produtores. Hectares e funcionalidades sob medida para sua operação.', 0, 999999, 1, 1, NOW())
            ");

            Console.WriteLine("Licensing tables recreated successfully with default plans!");
        }
    }
}
