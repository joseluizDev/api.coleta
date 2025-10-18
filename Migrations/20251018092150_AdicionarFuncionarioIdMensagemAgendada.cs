using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarFuncionarioIdMensagemAgendada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FuncionarioId",
                table: "MensagensAgendadas",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_MensagensAgendadas_FuncionarioId",
                table: "MensagensAgendadas",
                column: "FuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MensagensAgendadas_UsuarioId",
                table: "MensagensAgendadas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_MensagensAgendadas_Usuarios_FuncionarioId",
                table: "MensagensAgendadas",
                column: "FuncionarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MensagensAgendadas_Usuarios_UsuarioId",
                table: "MensagensAgendadas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MensagensAgendadas_Usuarios_FuncionarioId",
                table: "MensagensAgendadas");

            migrationBuilder.DropForeignKey(
                name: "FK_MensagensAgendadas_Usuarios_UsuarioId",
                table: "MensagensAgendadas");

            migrationBuilder.DropIndex(
                name: "IX_MensagensAgendadas_FuncionarioId",
                table: "MensagensAgendadas");

            migrationBuilder.DropIndex(
                name: "IX_MensagensAgendadas_UsuarioId",
                table: "MensagensAgendadas");

            migrationBuilder.DropColumn(
                name: "FuncionarioId",
                table: "MensagensAgendadas");
        }
    }
}
