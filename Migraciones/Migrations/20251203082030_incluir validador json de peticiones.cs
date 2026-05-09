using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class incluirvalidadorjsondepeticiones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VALIDADOR_JSON",
                schema: "VENTA",
                table: "PETICION_DE_FACTURA_EMT",
                type: "VARCHAR(MAX)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VALIDADOR_JSON",
                schema: "VENTA",
                table: "PETICION_DE_FACTURA_EMT");
        }
    }
}
