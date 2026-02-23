using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class AddEfiPayPlanIdIntColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EfiPayPlanIdInt",
                table: "Planos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BoletoCodigoBarras",
                table: "HistoricosPagamento",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BoletoLinhaDigitavel",
                table: "HistoricosPagamento",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BoletoLink",
                table: "HistoricosPagamento",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BoletoPdfUrl",
                table: "HistoricosPagamento",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "BoletoVencimento",
                table: "HistoricosPagamento",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CartaoBandeira",
                table: "HistoricosPagamento",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CartaoParcelas",
                table: "HistoricosPagamento",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CartaoUltimos4Digitos",
                table: "HistoricosPagamento",
                type: "varchar(4)",
                maxLength: 4,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "CartaoValorParcela",
                table: "HistoricosPagamento",
                type: "decimal(12,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EfiPaySubscriptionId",
                table: "HistoricosPagamento",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "RecorrenciaParcela",
                table: "HistoricosPagamento",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecorrenciaTotalParcelas",
                table: "HistoricosPagamento",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EfiPayPlanIdInt",
                table: "Planos");

            migrationBuilder.DropColumn(
                name: "BoletoCodigoBarras",
                table: "HistoricosPagamento");

            migrationBuilder.DropColumn(
                name: "BoletoLinhaDigitavel",
                table: "HistoricosPagamento");

            migrationBuilder.DropColumn(
                name: "BoletoLink",
                table: "HistoricosPagamento");

            migrationBuilder.DropColumn(
                name: "BoletoPdfUrl",
                table: "HistoricosPagamento");

            migrationBuilder.DropColumn(
                name: "BoletoVencimento",
                table: "HistoricosPagamento");

            migrationBuilder.DropColumn(
                name: "CartaoBandeira",
                table: "HistoricosPagamento");

            migrationBuilder.DropColumn(
                name: "CartaoParcelas",
                table: "HistoricosPagamento");

            migrationBuilder.DropColumn(
                name: "CartaoUltimos4Digitos",
                table: "HistoricosPagamento");

            migrationBuilder.DropColumn(
                name: "CartaoValorParcela",
                table: "HistoricosPagamento");

            migrationBuilder.DropColumn(
                name: "EfiPaySubscriptionId",
                table: "HistoricosPagamento");

            migrationBuilder.DropColumn(
                name: "RecorrenciaParcela",
                table: "HistoricosPagamento");

            migrationBuilder.DropColumn(
                name: "RecorrenciaTotalParcelas",
                table: "HistoricosPagamento");
        }
    }
}
