using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class renombrarID_LINEA_ALBARANaID_LINEA_RECEPCIONenlosmovimientosdealmacen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ALMACEN_MOVIMIENTO_ID_LINEA_ALBARAN",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO");

            migrationBuilder.RenameColumn(
                name: "ID_LINEA_ALBARAN",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                newName: "ID_LINEA_RECEPCION");

            migrationBuilder.RenameIndex(
                name: "I_ALMACEN_MOVIMIENTO_ID_LINEA_ALBARAN",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                newName: "I_ALMACEN_MOVIMIENTO_ID_LINEA_RECEPCION");

            migrationBuilder.AddForeignKey(
                name: "FK_ALMACEN_MOVIMIENTO_ID_LINEA_RECEPCION",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                column: "ID_LINEA_RECEPCION",
                principalSchema: "LOGISTICA",
                principalTable: "PEDIDO_LINEA_RECEPCION",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ALMACEN_MOVIMIENTO_ID_LINEA_RECEPCION",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO");

            migrationBuilder.RenameColumn(
                name: "ID_LINEA_RECEPCION",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                newName: "ID_LINEA_ALBARAN");

            migrationBuilder.RenameIndex(
                name: "I_ALMACEN_MOVIMIENTO_ID_LINEA_RECEPCION",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                newName: "I_ALMACEN_MOVIMIENTO_ID_LINEA_ALBARAN");

            migrationBuilder.AddForeignKey(
                name: "FK_ALMACEN_MOVIMIENTO_ID_LINEA_ALBARAN",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                column: "ID_LINEA_ALBARAN",
                principalSchema: "LOGISTICA",
                principalTable: "PEDIDO_LINEA_RECEPCION",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
