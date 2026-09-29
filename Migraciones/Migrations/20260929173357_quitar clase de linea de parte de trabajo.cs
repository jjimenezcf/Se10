using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class quitarclasedelineadepartedetrabajo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CLASE",
                schema: "VENTA",
                table: "PARTE_TR_LINEA");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CLASE",
                schema: "VENTA",
                table: "PARTE_TR_LINEA",
                type: "VARCHAR(30)",
                nullable: true);
        }
    }
}
