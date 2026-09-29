using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class quitarclasedelineadeplanificaciondeventa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CLASE",
                schema: "VENTA",
                table: "PLANIFICACION_VENTA_LINEA");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CLASE",
                schema: "VENTA",
                table: "PLANIFICACION_VENTA_LINEA",
                type: "VARCHAR(30)",
                nullable: true);
        }
    }
}
