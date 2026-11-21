using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class movimientodeventadematerial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ID_LINEA_VENTA",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                type: "INT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_MOVIMIENTO_ID_LINEA_VENTA",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                column: "ID_LINEA_VENTA");

            migrationBuilder.AddForeignKey(
                name: "FK_ALMACEN_MOVIMIENTO_ID_LINEA_VENTA",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                column: "ID_LINEA_VENTA",
                principalSchema: "LOGISTICA",
                principalTable: "VENTA_LINEA",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ALMACEN_MOVIMIENTO_ID_LINEA_VENTA",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO");

            migrationBuilder.DropIndex(
                name: "I_ALMACEN_MOVIMIENTO_ID_LINEA_VENTA",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO");

            migrationBuilder.DropColumn(
                name: "ID_LINEA_VENTA",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO");
        }
    }
}
