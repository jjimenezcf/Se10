using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class preasientosdeentregasenobra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ID_PREASIENTO",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                type: "INT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_PREASIENTO",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_DEVOLUCION",
                type: "INT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "I_PEDIDO_LINEA_RECEPCION_ID_PREASIENTO",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                column: "ID_PREASIENTO");

            migrationBuilder.CreateIndex(
                name: "I_PEDIDO_LINEA_DEVOLUCION_ID_PREASIENTO",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_DEVOLUCION",
                column: "ID_PREASIENTO");

            migrationBuilder.AddForeignKey(
                name: "FK_PEDIDO_LINEA_DEVOLUCION_ID_PREASIENTO",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_DEVOLUCION",
                column: "ID_PREASIENTO",
                principalSchema: "CONTABILIDAD",
                principalTable: "PREASIENTO",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PEDIDO_LINEA_RECEPCION_ID_PREASIENTO",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                column: "ID_PREASIENTO",
                principalSchema: "CONTABILIDAD",
                principalTable: "PREASIENTO",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PEDIDO_LINEA_DEVOLUCION_ID_PREASIENTO",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_DEVOLUCION");

            migrationBuilder.DropForeignKey(
                name: "FK_PEDIDO_LINEA_RECEPCION_ID_PREASIENTO",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION");

            migrationBuilder.DropIndex(
                name: "I_PEDIDO_LINEA_RECEPCION_ID_PREASIENTO",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION");

            migrationBuilder.DropIndex(
                name: "I_PEDIDO_LINEA_DEVOLUCION_ID_PREASIENTO",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_DEVOLUCION");

            migrationBuilder.DropColumn(
                name: "ID_PREASIENTO",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION");

            migrationBuilder.DropColumn(
                name: "ID_PREASIENTO",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_DEVOLUCION");
        }
    }
}
