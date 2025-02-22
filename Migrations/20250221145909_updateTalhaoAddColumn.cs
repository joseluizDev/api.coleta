using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class updateTalhaoAddColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClienteID",
                table: "Talhoes",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Talhoes_ClienteID",
                table: "Talhoes",
                column: "ClienteID");

            migrationBuilder.AddForeignKey(
                name: "FK_Talhoes_Clientes_ClienteID",
                table: "Talhoes",
                column: "ClienteID",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Talhoes_Clientes_ClienteID",
                table: "Talhoes");

            migrationBuilder.DropIndex(
                name: "IX_Talhoes_ClienteID",
                table: "Talhoes");

            migrationBuilder.DropColumn(
                name: "ClienteID",
                table: "Talhoes");
        }
    }
}
