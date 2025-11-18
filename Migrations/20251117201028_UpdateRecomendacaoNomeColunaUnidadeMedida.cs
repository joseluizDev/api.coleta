using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRecomendacaoNomeColunaUnidadeMedida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Descricao",
                table: "Recomendacoes",
                newName: "UnidadeMedida");

            migrationBuilder.AddColumn<string>(
                name: "NomeColuna",
                table: "Recomendacoes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomeColuna",
                table: "Recomendacoes");

            migrationBuilder.RenameColumn(
                name: "UnidadeMedida",
                table: "Recomendacoes",
                newName: "Descricao");
        }
    }
}
