using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCPFAndCNPJFromCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Proteção: só tenta dropar se as colunas existirem
            migrationBuilder.Sql(@"SET @existCPF = (
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'CPF');
                SET @stmtCPF = IF(@existCPF > 0, 'ALTER TABLE `Clientes` DROP COLUMN `CPF`', 'SELECT 1');
                PREPARE dropStmtCPF FROM @stmtCPF; EXECUTE dropStmtCPF; DEALLOCATE PREPARE dropStmtCPF;");

            migrationBuilder.Sql(@"SET @existCNPJ = (
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'CNPJ');
                SET @stmtCNPJ = IF(@existCNPJ > 0, 'ALTER TABLE `Clientes` DROP COLUMN `CNPJ`', 'SELECT 1');
                PREPARE dropStmtCNPJ FROM @stmtCNPJ; EXECUTE dropStmtCNPJ; DEALLOCATE PREPARE dropStmtCNPJ;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CPF",
                table: "Clientes",
                type: "varchar(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CNPJ",
                table: "Clientes",
                type: "varchar(18)",
                maxLength: 18,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
