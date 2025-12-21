using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class AddColheitaFieldsToImagemNdvi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ColheitaMax",
                table: "ImagensNdvi",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ColheitaMedia",
                table: "ImagensNdvi",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ColheitaMin",
                table: "ImagensNdvi",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataImagemColheita",
                table: "ImagensNdvi",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColheitaMax",
                table: "ImagensNdvi");

            migrationBuilder.DropColumn(
                name: "ColheitaMedia",
                table: "ImagensNdvi");

            migrationBuilder.DropColumn(
                name: "ColheitaMin",
                table: "ImagensNdvi");

            migrationBuilder.DropColumn(
                name: "DataImagemColheita",
                table: "ImagensNdvi");
        }
    }
}
