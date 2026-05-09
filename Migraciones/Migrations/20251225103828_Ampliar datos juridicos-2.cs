using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class Ampliardatosjuridicos2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "COSTAS",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS",
                type: "DECIMAL(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LITIGADO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS",
                type: "DECIMAL(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NIG",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS",
                type: "VARCHAR(25)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "REFERENCIA",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS",
                type: "VARCHAR(50)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SENTENCIADO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS",
                type: "DECIMAL(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SENTENCIADO_EL",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS",
                type: "DATETIME2(7)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "COSTAS",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS");

            migrationBuilder.DropColumn(
                name: "LITIGADO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS");

            migrationBuilder.DropColumn(
                name: "NIG",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS");

            migrationBuilder.DropColumn(
                name: "REFERENCIA",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS");

            migrationBuilder.DropColumn(
                name: "SENTENCIADO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS");

            migrationBuilder.DropColumn(
                name: "SENTENCIADO_EL",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS");
        }
    }
}
