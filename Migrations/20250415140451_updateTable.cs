using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class updateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ColetaId",
                table: "Relatorios",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Relatorios_ColetaId",
                table: "Relatorios",
                column: "ColetaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Relatorios_Coletas_ColetaId",
                table: "Relatorios",
                column: "ColetaId",
                principalTable: "Coletas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Relatorios_Coletas_ColetaId",
                table: "Relatorios");

            migrationBuilder.DropIndex(
                name: "IX_Relatorios_ColetaId",
                table: "Relatorios");

            migrationBuilder.DropColumn(
                name: "ColetaId",
                table: "Relatorios");
        }
    }
}
