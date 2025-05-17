using Microsoft.EntityFrameworkCore.Migrations;

namespace api.coleta.Migrations
{
    public partial class UpdateConfiguracaoPersonalizadaLimites : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adicionar novas colunas
            migrationBuilder.AddColumn<decimal>(
                name: "LimiteInferior",
                table: "ConfiguracaoPersonalizadas",
                type: "decimal(12,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LimiteSuperior",
                table: "ConfiguracaoPersonalizadas",
                type: "decimal(12,4)",
                nullable: false,
                defaultValue: 1m);

            // Copiar dados da coluna Limite para LimiteInferior e LimiteSuperior
            migrationBuilder.Sql(
                "UPDATE ConfiguracaoPersonalizadas " +
                "SET LimiteInferior = 0, " +
                "LimiteSuperior = Limite");

            // Remover a coluna antiga
            migrationBuilder.DropColumn(
                name: "Limite",
                table: "ConfiguracaoPersonalizadas");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Adicionar a coluna antiga
            migrationBuilder.AddColumn<decimal>(
                name: "Limite",
                table: "ConfiguracaoPersonalizadas",
                type: "decimal(12,4)",
                nullable: false,
                defaultValue: 0m);

            // Copiar dados de LimiteSuperior para Limite
            migrationBuilder.Sql(
                "UPDATE ConfiguracaoPersonalizadas " +
                "SET Limite = LimiteSuperior");

            // Remover as novas colunas
            migrationBuilder.DropColumn(
                name: "LimiteInferior",
                table: "ConfiguracaoPersonalizadas");

            migrationBuilder.DropColumn(
                name: "LimiteSuperior",
                table: "ConfiguracaoPersonalizadas");
        }
    }
}
