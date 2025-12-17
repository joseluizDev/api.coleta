using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoImagemAndAltimetriaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ColetaId",
                table: "Recomendacoes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<double>(
                name: "AltimetriaMax",
                table: "ImagensNdvi",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AltimetriaMin",
                table: "ImagensNdvi",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AltimetriaVariacao",
                table: "ImagensNdvi",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoImagem",
                table: "ImagensNdvi",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Recomendacoes_ColetaId",
                table: "Recomendacoes",
                column: "ColetaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recomendacoes_Coletas_ColetaId",
                table: "Recomendacoes",
                column: "ColetaId",
                principalTable: "Coletas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recomendacoes_Coletas_ColetaId",
                table: "Recomendacoes");

            migrationBuilder.DropIndex(
                name: "IX_Recomendacoes_ColetaId",
                table: "Recomendacoes");

            migrationBuilder.DropColumn(
                name: "ColetaId",
                table: "Recomendacoes");

            migrationBuilder.DropColumn(
                name: "AltimetriaMax",
                table: "ImagensNdvi");

            migrationBuilder.DropColumn(
                name: "AltimetriaMin",
                table: "ImagensNdvi");

            migrationBuilder.DropColumn(
                name: "AltimetriaVariacao",
                table: "ImagensNdvi");

            migrationBuilder.DropColumn(
                name: "TipoImagem",
                table: "ImagensNdvi");
        }
    }
}
