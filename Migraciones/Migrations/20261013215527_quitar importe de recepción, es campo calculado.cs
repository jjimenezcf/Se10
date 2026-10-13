using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class quitarimportederecepciónescampocalculado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IMPORTE",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "IMPORTE",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                type: "DECIMAL(18,6)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
