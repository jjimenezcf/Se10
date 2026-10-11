using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class anadiralmacenalpedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ID_ALMACEN",
                schema: "LOGISTICA",
                table: "PEDIDO",
                type: "INT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "I_PEDIDO_ID_ALMACEN",
                schema: "LOGISTICA",
                table: "PEDIDO",
                column: "ID_ALMACEN");

            migrationBuilder.AddForeignKey(
                name: "FK_PEDIDO_ID_ALMACEN",
                schema: "LOGISTICA",
                table: "PEDIDO",
                column: "ID_ALMACEN",
                principalSchema: "LOGISTICA",
                principalTable: "ALMACEN",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PEDIDO_ID_ALMACEN",
                schema: "LOGISTICA",
                table: "PEDIDO");

            migrationBuilder.DropIndex(
                name: "I_PEDIDO_ID_ALMACEN",
                schema: "LOGISTICA",
                table: "PEDIDO");

            migrationBuilder.DropColumn(
                name: "ID_ALMACEN",
                schema: "LOGISTICA",
                table: "PEDIDO");
        }
    }
}
