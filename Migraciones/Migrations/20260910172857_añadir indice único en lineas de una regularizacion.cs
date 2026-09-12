using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class añadirindiceúnicoenlineasdeunaregularizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_LINEA_ID_ELEMENTO_ID_UNITARIO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_LINEA",
                columns: new[] { "ID_ELEMENTO", "ID_UNITARIO" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "I_REGULARIZACION_LINEA_ID_ELEMENTO_ID_UNITARIO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_LINEA");
        }
    }
}
