using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableRelatorio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioId",
                table: "Relatorios",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Relatorios_UsuarioId",
                table: "Relatorios",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Relatorios_Usuarios_UsuarioId",
                table: "Relatorios",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Relatorios_Usuarios_UsuarioId",
                table: "Relatorios");

            migrationBuilder.DropIndex(
                name: "IX_Relatorios_UsuarioId",
                table: "Relatorios");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Relatorios");
        }
    }
}
