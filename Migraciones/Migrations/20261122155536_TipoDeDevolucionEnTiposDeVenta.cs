using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class TipoDeDevolucionEnTiposDeVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ID_TIPO_DEVOLUCION",
                schema: "LOGISTICA",
                table: "VENTA_TIPO",
                type: "INT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "I_VENTA_TIPO_ID_TIPO_DEVOLUCION",
                schema: "LOGISTICA",
                table: "VENTA_TIPO",
                column: "ID_TIPO_DEVOLUCION");

            migrationBuilder.AddForeignKey(
                name: "FK_VENTA_TIPO_ID_TIPO_DEVOLUCION",
                schema: "LOGISTICA",
                table: "VENTA_TIPO",
                column: "ID_TIPO_DEVOLUCION",
                principalSchema: "LOGISTICA",
                principalTable: "DEVOLUCIONVENTA_TIPO",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VENTA_TIPO_ID_TIPO_DEVOLUCION",
                schema: "LOGISTICA",
                table: "VENTA_TIPO");

            migrationBuilder.DropIndex(
                name: "I_VENTA_TIPO_ID_TIPO_DEVOLUCION",
                schema: "LOGISTICA",
                table: "VENTA_TIPO");

            migrationBuilder.DropColumn(
                name: "ID_TIPO_DEVOLUCION",
                schema: "LOGISTICA",
                table: "VENTA_TIPO");
        }
    }
}
