using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class informacioncontabledealmacenes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ID_CUENTA_COMPRA_DIRECTA",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_TIPO",
                type: "INT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_CUENTA_CONSUMO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_TIPO",
                type: "INT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_CUENTA_COMPRAS",
                schema: "LOGISTICA",
                table: "ALMACEN",
                type: "INT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_CUENTA_DEVOLUCIONES",
                schema: "LOGISTICA",
                table: "ALMACEN",
                type: "INT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_CUENTA_EXISTENCIAS",
                schema: "LOGISTICA",
                table: "ALMACEN",
                type: "INT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_CUENTA_VARIACION",
                schema: "LOGISTICA",
                table: "ALMACEN",
                type: "INT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "USA_VARIACION_EXISTENCIAS",
                schema: "LOGISTICA",
                table: "ALMACEN",
                type: "BIT",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "I_EXPEDIENTE_TIPO_ID_CUENTA_COMPRA_DIRECTA",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_TIPO",
                column: "ID_CUENTA_COMPRA_DIRECTA");

            migrationBuilder.CreateIndex(
                name: "I_EXPEDIENTE_TIPO_ID_CUENTA_CONSUMO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_TIPO",
                column: "ID_CUENTA_CONSUMO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ID_CUENTA_COMPRAS",
                schema: "LOGISTICA",
                table: "ALMACEN",
                column: "ID_CUENTA_COMPRAS");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ID_CUENTA_DEVOLUCIONES",
                schema: "LOGISTICA",
                table: "ALMACEN",
                column: "ID_CUENTA_DEVOLUCIONES");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ID_CUENTA_EXISTENCIAS",
                schema: "LOGISTICA",
                table: "ALMACEN",
                column: "ID_CUENTA_EXISTENCIAS");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ID_CUENTA_VARIACION",
                schema: "LOGISTICA",
                table: "ALMACEN",
                column: "ID_CUENTA_VARIACION");

            migrationBuilder.AddForeignKey(
                name: "FK_ALMACEN_ID_CUENTA_COMPRAS",
                schema: "LOGISTICA",
                table: "ALMACEN",
                column: "ID_CUENTA_COMPRAS",
                principalSchema: "CONTABILIDAD",
                principalTable: "CUENTA",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ALMACEN_ID_CUENTA_DEVOLUCIONES",
                schema: "LOGISTICA",
                table: "ALMACEN",
                column: "ID_CUENTA_DEVOLUCIONES",
                principalSchema: "CONTABILIDAD",
                principalTable: "CUENTA",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ALMACEN_ID_CUENTA_EXISTENCIAS",
                schema: "LOGISTICA",
                table: "ALMACEN",
                column: "ID_CUENTA_EXISTENCIAS",
                principalSchema: "CONTABILIDAD",
                principalTable: "CUENTA",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ALMACEN_ID_CUENTA_VARIACION",
                schema: "LOGISTICA",
                table: "ALMACEN",
                column: "ID_CUENTA_VARIACION",
                principalSchema: "CONTABILIDAD",
                principalTable: "CUENTA",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EXPEDIENTE_TIPO_ID_CUENTA_COMPRA_DIRECTA",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_TIPO",
                column: "ID_CUENTA_COMPRA_DIRECTA",
                principalSchema: "CONTABILIDAD",
                principalTable: "CUENTA",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EXPEDIENTE_TIPO_ID_CUENTA_CONSUMO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_TIPO",
                column: "ID_CUENTA_CONSUMO",
                principalSchema: "CONTABILIDAD",
                principalTable: "CUENTA",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ALMACEN_ID_CUENTA_COMPRAS",
                schema: "LOGISTICA",
                table: "ALMACEN");

            migrationBuilder.DropForeignKey(
                name: "FK_ALMACEN_ID_CUENTA_DEVOLUCIONES",
                schema: "LOGISTICA",
                table: "ALMACEN");

            migrationBuilder.DropForeignKey(
                name: "FK_ALMACEN_ID_CUENTA_EXISTENCIAS",
                schema: "LOGISTICA",
                table: "ALMACEN");

            migrationBuilder.DropForeignKey(
                name: "FK_ALMACEN_ID_CUENTA_VARIACION",
                schema: "LOGISTICA",
                table: "ALMACEN");

            migrationBuilder.DropForeignKey(
                name: "FK_EXPEDIENTE_TIPO_ID_CUENTA_COMPRA_DIRECTA",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_TIPO");

            migrationBuilder.DropForeignKey(
                name: "FK_EXPEDIENTE_TIPO_ID_CUENTA_CONSUMO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_TIPO");

            migrationBuilder.DropIndex(
                name: "I_EXPEDIENTE_TIPO_ID_CUENTA_COMPRA_DIRECTA",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_TIPO");

            migrationBuilder.DropIndex(
                name: "I_EXPEDIENTE_TIPO_ID_CUENTA_CONSUMO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_TIPO");

            migrationBuilder.DropIndex(
                name: "I_ALMACEN_ID_CUENTA_COMPRAS",
                schema: "LOGISTICA",
                table: "ALMACEN");

            migrationBuilder.DropIndex(
                name: "I_ALMACEN_ID_CUENTA_DEVOLUCIONES",
                schema: "LOGISTICA",
                table: "ALMACEN");

            migrationBuilder.DropIndex(
                name: "I_ALMACEN_ID_CUENTA_EXISTENCIAS",
                schema: "LOGISTICA",
                table: "ALMACEN");

            migrationBuilder.DropIndex(
                name: "I_ALMACEN_ID_CUENTA_VARIACION",
                schema: "LOGISTICA",
                table: "ALMACEN");

            migrationBuilder.DropColumn(
                name: "ID_CUENTA_COMPRA_DIRECTA",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_TIPO");

            migrationBuilder.DropColumn(
                name: "ID_CUENTA_CONSUMO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_TIPO");

            migrationBuilder.DropColumn(
                name: "ID_CUENTA_COMPRAS",
                schema: "LOGISTICA",
                table: "ALMACEN");

            migrationBuilder.DropColumn(
                name: "ID_CUENTA_DEVOLUCIONES",
                schema: "LOGISTICA",
                table: "ALMACEN");

            migrationBuilder.DropColumn(
                name: "ID_CUENTA_EXISTENCIAS",
                schema: "LOGISTICA",
                table: "ALMACEN");

            migrationBuilder.DropColumn(
                name: "ID_CUENTA_VARIACION",
                schema: "LOGISTICA",
                table: "ALMACEN");

            migrationBuilder.DropColumn(
                name: "USA_VARIACION_EXISTENCIAS",
                schema: "LOGISTICA",
                table: "ALMACEN");
        }
    }
}
