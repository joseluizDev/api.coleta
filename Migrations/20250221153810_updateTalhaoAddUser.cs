using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class updateTalhaoAddUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioID",
                table: "Talhoes",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Talhoes_UsuarioID",
                table: "Talhoes",
                column: "UsuarioID");

            migrationBuilder.AddForeignKey(
                name: "FK_Talhoes_Usuarios_UsuarioID",
                table: "Talhoes",
                column: "UsuarioID",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Talhoes_Usuarios_UsuarioID",
                table: "Talhoes");

            migrationBuilder.DropIndex(
                name: "IX_Talhoes_UsuarioID",
                table: "Talhoes");

            migrationBuilder.DropColumn(
                name: "UsuarioID",
                table: "Talhoes");
        }
    }
}
