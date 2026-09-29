using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class quitarclasedelineadepedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CLASE",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CLASE",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA",
                type: "VARCHAR(30)",
                nullable: true);
        }
    }
}
