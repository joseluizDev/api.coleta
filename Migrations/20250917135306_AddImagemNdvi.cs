using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class AddImagemNdvi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImagensNdvi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LinkImagem = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataImagem = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PercentualNuvens = table.Column<double>(type: "double", nullable: false),
                    NdviMax = table.Column<double>(type: "double", nullable: false),
                    NdviMin = table.Column<double>(type: "double", nullable: false),
                    TalhaoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FazendaId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DataInclusao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagensNdvi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImagensNdvi_Fazendas_FazendaId",
                        column: x => x.FazendaId,
                        principalTable: "Fazendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImagensNdvi_Talhoes_TalhaoId",
                        column: x => x.TalhaoId,
                        principalTable: "Talhoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImagensNdvi_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ImagensNdvi_FazendaId",
                table: "ImagensNdvi",
                column: "FazendaId");

            migrationBuilder.CreateIndex(
                name: "IX_ImagensNdvi_TalhaoId",
                table: "ImagensNdvi",
                column: "TalhaoId");

            migrationBuilder.CreateIndex(
                name: "IX_ImagensNdvi_UsuarioId",
                table: "ImagensNdvi",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImagensNdvi");
        }
    }
}
