using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class AddFazendaIdToColeta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FazendaID",
                table: "Coletas",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Coletas_FazendaID",
                table: "Coletas",
                column: "FazendaID");

            migrationBuilder.AddForeignKey(
                name: "FK_Coletas_Fazendas_FazendaID",
                table: "Coletas",
                column: "FazendaID",
                principalTable: "Fazendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coletas_Fazendas_FazendaID",
                table: "Coletas");

            migrationBuilder.DropIndex(
                name: "IX_Coletas_FazendaID",
                table: "Coletas");

            migrationBuilder.DropColumn(
                name: "FazendaID",
                table: "Coletas");
        }
    }
}
