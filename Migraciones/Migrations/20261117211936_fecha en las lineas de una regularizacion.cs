using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class fechaenlaslineasdeunaregularizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FECHA",
                schema: "LOGISTICA",
                table: "REGULARIZACION_LINEA",
                type: "DATE",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // las líneas que ya existen toman como fecha el día en que se crearon
            migrationBuilder.Sql("UPDATE LOGISTICA.REGULARIZACION_LINEA SET FECHA = CAST(FECCRE AS DATE)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FECHA",
                schema: "LOGISTICA",
                table: "REGULARIZACION_LINEA");
        }
    }
}
