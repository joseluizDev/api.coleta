using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.coleta.Migrations
{
    /// <inheritdoc />
    public partial class AddAltimetriaSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "PercentualNuvens",
                table: "ImagensNdvi",
                type: "double",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "NdviMin",
                table: "ImagensNdvi",
                type: "double",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<double>(
                name: "NdviMax",
                table: "ImagensNdvi",
                type: "double",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double");

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
                nullable: false,
                defaultValue: "ndvi")
                .Annotation("MySql:CharSet", "utf8mb4");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AlterColumn<double>(
                name: "PercentualNuvens",
                table: "ImagensNdvi",
                type: "double",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "NdviMin",
                table: "ImagensNdvi",
                type: "double",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "NdviMax",
                table: "ImagensNdvi",
                type: "double",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);
        }
    }
}
