using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class updatetablefuncionario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Funcionario",
                table: "VisualizarMapas");

            migrationBuilder.AddColumn<Guid>(
                name: "FuncionarioID",
                table: "VisualizarMapas",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioID",
                table: "Funcionarios",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_VisualizarMapas_FuncionarioID",
                table: "VisualizarMapas",
                column: "FuncionarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_UsuarioID",
                table: "Funcionarios",
                column: "UsuarioID");

            migrationBuilder.AddForeignKey(
                name: "FK_Funcionarios_Usuarios_UsuarioID",
                table: "Funcionarios",
                column: "UsuarioID",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VisualizarMapas_Funcionarios_FuncionarioID",
                table: "VisualizarMapas",
                column: "FuncionarioID",
                principalTable: "Funcionarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Funcionarios_Usuarios_UsuarioID",
                table: "Funcionarios");

            migrationBuilder.DropForeignKey(
                name: "FK_VisualizarMapas_Funcionarios_FuncionarioID",
                table: "VisualizarMapas");

            migrationBuilder.DropIndex(
                name: "IX_VisualizarMapas_FuncionarioID",
                table: "VisualizarMapas");

            migrationBuilder.DropIndex(
                name: "IX_Funcionarios_UsuarioID",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "FuncionarioID",
                table: "VisualizarMapas");

            migrationBuilder.DropColumn(
                name: "UsuarioID",
                table: "Funcionarios");

            migrationBuilder.AddColumn<string>(
                name: "Funcionario",
                table: "VisualizarMapas",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
