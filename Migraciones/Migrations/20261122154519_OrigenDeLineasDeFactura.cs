using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class OrigenDeLineasDeFactura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ID_LINEA_DEVOLUCION_VENTA",
                schema: "VENTA",
                table: "FACTURA_EMT_LINEA",
                type: "INT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_LINEA_VENTA",
                schema: "VENTA",
                table: "FACTURA_EMT_LINEA",
                type: "INT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "I_FACTURA_EMT_LINEA_ID_LINEA_DEVOLUCION_VENTA",
                schema: "VENTA",
                table: "FACTURA_EMT_LINEA",
                column: "ID_LINEA_DEVOLUCION_VENTA");

            migrationBuilder.CreateIndex(
                name: "I_FACTURA_EMT_LINEA_ID_LINEA_VENTA",
                schema: "VENTA",
                table: "FACTURA_EMT_LINEA",
                column: "ID_LINEA_VENTA");

            migrationBuilder.AddForeignKey(
                name: "FK_FACTURA_EMT_LINEA_ID_LINEA_DEVOLUCION_VENTA",
                schema: "VENTA",
                table: "FACTURA_EMT_LINEA",
                column: "ID_LINEA_DEVOLUCION_VENTA",
                principalSchema: "LOGISTICA",
                principalTable: "DEVOLUCIONVENTA_LINEA",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FACTURA_EMT_LINEA_ID_LINEA_VENTA",
                schema: "VENTA",
                table: "FACTURA_EMT_LINEA",
                column: "ID_LINEA_VENTA",
                principalSchema: "LOGISTICA",
                principalTable: "VENTA_LINEA",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            // Relleno de las facturas ya creadas desde un documento de venta: la prefactura tiene una línea alzada (sin unitario) por cada
            // línea vendida con su mismo orden (ExtensorDeVentas.CrearPrefactura). Sólo si la factura es de un único documento de venta, para
            // que el orden no sea ambiguo. Las rectificativas y sustitutas anteriores no tienen documento de venta que apunte a ellas: se
            // rellenan las que sí (la sustituta, a la que pasa a apuntar el documento de venta)
            migrationBuilder.Sql(@"
UPDATE fl
   SET fl.ID_LINEA_VENTA = vl.ID
  FROM VENTA.FACTURA_EMT_LINEA fl
  JOIN LOGISTICA.VENTA v ON v.ID_FACTURA_EMT = fl.ID_ELEMENTO
  JOIN LOGISTICA.VENTA_LINEA vl ON vl.ID_ELEMENTO = v.ID AND vl.ORDEN = fl.ORDEN
 WHERE fl.ID_LINEA_VENTA IS NULL
   AND fl.ID_UNITARIO IS NULL
   AND (SELECT COUNT(*) FROM LOGISTICA.VENTA v2 WHERE v2.ID_FACTURA_EMT = fl.ID_ELEMENTO) = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FACTURA_EMT_LINEA_ID_LINEA_DEVOLUCION_VENTA",
                schema: "VENTA",
                table: "FACTURA_EMT_LINEA");

            migrationBuilder.DropForeignKey(
                name: "FK_FACTURA_EMT_LINEA_ID_LINEA_VENTA",
                schema: "VENTA",
                table: "FACTURA_EMT_LINEA");

            migrationBuilder.DropIndex(
                name: "I_FACTURA_EMT_LINEA_ID_LINEA_DEVOLUCION_VENTA",
                schema: "VENTA",
                table: "FACTURA_EMT_LINEA");

            migrationBuilder.DropIndex(
                name: "I_FACTURA_EMT_LINEA_ID_LINEA_VENTA",
                schema: "VENTA",
                table: "FACTURA_EMT_LINEA");

            migrationBuilder.DropColumn(
                name: "ID_LINEA_DEVOLUCION_VENTA",
                schema: "VENTA",
                table: "FACTURA_EMT_LINEA");

            migrationBuilder.DropColumn(
                name: "ID_LINEA_VENTA",
                schema: "VENTA",
                table: "FACTURA_EMT_LINEA");
        }
    }
}
