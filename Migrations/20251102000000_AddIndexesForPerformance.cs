using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
  /// <inheritdoc />
  public partial class AddIndexesForPerformance : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      // Índice composto para a query principal do mobile
      // Cobre: UsuarioRespID (filtro principal)
      migrationBuilder.CreateIndex(
          name: "IX_Coletas_UsuarioRespID_GeojsonID_TalhaoID",
          table: "Coletas",
          columns: new[] { "UsuarioRespID", "GeojsonID", "TalhaoID" });

      // Índice para verificação de relatórios
      migrationBuilder.CreateIndex(
          name: "IX_Relatorios_ColetaId",
          table: "Relatorios",
          column: "ColetaId");

      // Índice para pontos coletados por coleta
      migrationBuilder.CreateIndex(
          name: "IX_PontoColetados_ColetaID_DataColeta",
          table: "PontoColetados",
          columns: new[] { "ColetaID", "DataColeta" });

      // Índice para TalhaoJson.TalhaoID (usado em joins)
      migrationBuilder.CreateIndex(
          name: "IX_TalhaoJson_TalhaoID",
          table: "TalhaoJson",
          column: "TalhaoID");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(
          name: "IX_Coletas_UsuarioRespID_GeojsonID_TalhaoID",
          table: "Coletas");

      migrationBuilder.DropIndex(
          name: "IX_Relatorios_ColetaId",
          table: "Relatorios");

      migrationBuilder.DropIndex(
          name: "IX_PontoColetados_ColetaID_DataColeta",
          table: "PontoColetados");

      migrationBuilder.DropIndex(
          name: "IX_TalhaoJson_TalhaoID",
          table: "TalhaoJson");
    }
  }
}
