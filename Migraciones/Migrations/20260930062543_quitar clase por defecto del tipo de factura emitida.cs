using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class quitarclasepordefectodeltipodefacturaemitida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CLASE",
                schema: "VENTA",
                table: "FACTURA_EMT_TIPO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CLASE",
                schema: "VENTA",
                table: "FACTURA_EMT_TIPO",
                type: "VARCHAR(30)",
                nullable: true);
        }
    }
}
