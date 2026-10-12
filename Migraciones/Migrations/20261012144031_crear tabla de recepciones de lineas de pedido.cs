using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class creartabladerecepcionesdelineasdepedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PEDIDO_LINEA_RECEPCION",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_LINEA = table.Column<int>(type: "INT", nullable: false),
                    CANTIDAD = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: false),
                    PRECIO = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: false),
                    DESCUENTO = table.Column<decimal>(type: "DECIMAL(4,2)", nullable: true),
                    IMPORTE = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: false),
                    ID_EXPEDIENTE = table.Column<int>(type: "INT", nullable: true),
                    ID_ALMACEN = table.Column<int>(type: "INT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PEDIDO_LINEA_RECEPCION", x => x.ID);
                    table.CheckConstraint("CK_PEDIDO_LINEA_RECEPCION_DESTINO", "([ID_ALMACEN] IS NOT NULL AND [ID_EXPEDIENTE] IS NULL) OR ([ID_ALMACEN] IS NULL AND [ID_EXPEDIENTE] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_PEDIDO_LINEA_RECEPCION_ID_ALMACEN",
                        column: x => x.ID_ALMACEN,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PEDIDO_LINEA_RECEPCION_ID_EXPEDIENTE",
                        column: x => x.ID_EXPEDIENTE,
                        principalSchema: "EXPEDIENTE",
                        principalTable: "EXPEDIENTE",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PEDIDO_LINEA_RECEPCION_ID_LINEA",
                        column: x => x.ID_LINEA,
                        principalSchema: "LOGISTICA",
                        principalTable: "PEDIDO_LINEA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_PEDIDO_LINEA_RECEPCION_ID_ALMACEN",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                column: "ID_ALMACEN");

            migrationBuilder.CreateIndex(
                name: "I_PEDIDO_LINEA_RECEPCION_ID_EXPEDIENTE",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                column: "ID_EXPEDIENTE");

            migrationBuilder.CreateIndex(
                name: "I_PEDIDO_LINEA_RECEPCION_ID_LINEA",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                column: "ID_LINEA");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PEDIDO_LINEA_RECEPCION",
                schema: "LOGISTICA");
        }
    }
}
