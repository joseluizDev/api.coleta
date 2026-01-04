using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Sync migration for Licensing System tables (Planos, Assinaturas, HistoricosPagamento)
    /// Tables already exist in database, this migration only syncs the model snapshot.
    /// </summary>
    public partial class SyncLicensingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tables already exist in database - no SQL needed
            // This migration syncs the EF Core model with the existing schema
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricosPagamento");

            migrationBuilder.DropTable(
                name: "Assinaturas");

            migrationBuilder.DropTable(
                name: "Planos");
        }
    }
}
