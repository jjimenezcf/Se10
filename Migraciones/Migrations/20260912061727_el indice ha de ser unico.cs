using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class elindicehadeserunico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "I_REGULARIZACION_LINEA_ID_ELEMENTO_ID_UNITARIO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_LINEA");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_LINEA_ID_ELEMENTO_ID_UNITARIO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_LINEA",
                columns: new[] { "ID_ELEMENTO", "ID_UNITARIO" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "I_REGULARIZACION_LINEA_ID_ELEMENTO_ID_UNITARIO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_LINEA");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_LINEA_ID_ELEMENTO_ID_UNITARIO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_LINEA",
                columns: new[] { "ID_ELEMENTO", "ID_UNITARIO" });
        }
    }
}
