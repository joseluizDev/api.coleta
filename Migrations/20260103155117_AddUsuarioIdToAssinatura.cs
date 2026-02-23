using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioIdToAssinatura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assinaturas_Clientes_ClienteId",
                table: "Assinaturas");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClienteId",
                table: "Assinaturas",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioId",
                table: "Assinaturas",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_UsuarioId",
                table: "Assinaturas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assinaturas_Clientes_ClienteId",
                table: "Assinaturas",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assinaturas_Usuarios_UsuarioId",
                table: "Assinaturas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assinaturas_Clientes_ClienteId",
                table: "Assinaturas");

            migrationBuilder.DropForeignKey(
                name: "FK_Assinaturas_Usuarios_UsuarioId",
                table: "Assinaturas");

            migrationBuilder.DropIndex(
                name: "IX_Assinaturas_UsuarioId",
                table: "Assinaturas");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Assinaturas");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClienteId",
                table: "Assinaturas",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_Assinaturas_Clientes_ClienteId",
                table: "Assinaturas",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
