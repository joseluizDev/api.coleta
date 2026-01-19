using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.coleta.Controllers
{
    /// <summary>
    /// Controller temporario para aplicar migrations manuais.
    /// REMOVER APOS USO!
    /// </summary>
    [ApiController]
    [Route("api/migration")]
    public class MigrationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MigrationController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Aplica migration para adicionar UsuarioId na tabela Assinaturas.
        /// Endpoint temporario - remover apos uso.
        /// </summary>
        [HttpPost("apply-usuarioid")]
        [AllowAnonymous]
        public async Task<IActionResult> ApplyUsuarioIdMigration([FromQuery] string key)
        {
            // Chave simples de seguranca
            if (key != "agrosyste2024migrate")
            {
                return Unauthorized("Chave invalida");
            }

            var results = new List<string>();

            try
            {
                // Verificar se coluna ja existe
                var checkColumnSql = @"
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME = 'Assinaturas'
                    AND COLUMN_NAME = 'UsuarioId'";

                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = checkColumnSql;
                var columnExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

                if (columnExists)
                {
                    results.Add("Coluna UsuarioId ja existe na tabela Assinaturas");
                    return Ok(new { success = true, message = "Migration ja aplicada", results });
                }

                // Aplicar alteracoes
                var sqlStatements = new[]
                {
                    "ALTER TABLE `Assinaturas` ADD `UsuarioId` char(36) COLLATE ascii_general_ci NULL",
                    "CREATE INDEX `IX_Assinaturas_UsuarioId` ON `Assinaturas` (`UsuarioId`)",
                    "ALTER TABLE `Assinaturas` ADD CONSTRAINT `FK_Assinaturas_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `Usuarios` (`Id`)"
                };

                foreach (var sql in sqlStatements)
                {
                    try
                    {
                        using var cmd = connection.CreateCommand();
                        cmd.CommandText = sql;
                        await cmd.ExecuteNonQueryAsync();
                        results.Add($"OK: {sql.Substring(0, Math.Min(60, sql.Length))}...");
                    }
                    catch (Exception ex)
                    {
                        results.Add($"ERRO: {sql.Substring(0, Math.Min(40, sql.Length))}... - {ex.Message}");
                    }
                }

                return Ok(new { success = true, message = "Migration aplicada com sucesso", results });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message, results });
            }
        }

        /// <summary>
        /// Verifica estrutura da tabela Assinaturas
        /// </summary>
        [HttpGet("check-assinaturas")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckAssinaturas([FromQuery] string key)
        {
            if (key != "agrosyste2024migrate")
            {
                return Unauthorized("Chave invalida");
            }

            try
            {
                var sql = "DESCRIBE Assinaturas";
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = sql;

                var columns = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(new
                    {
                        Field = reader.GetString(0),
                        Type = reader.GetString(1),
                        Null = reader.GetString(2),
                        Key = reader.GetString(3)
                    });
                }

                return Ok(new { columns });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
