using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class devolucionesdelasentregasdelospedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PEDIDO_LINEA_DEVOLUCION",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_LINEA_RECEPCION = table.Column<int>(type: "INT", nullable: false),
                    DEVUELTA_EL = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    CANTIDAD = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: false),
                    MOTIVO = table.Column<string>(type: "VARCHAR(2000)", nullable: false),
                    ID_CREADOR = table.Column<int>(type: "INT", nullable: false),
                    FECCRE = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    ID_MODIFICADOR = table.Column<int>(type: "INT", nullable: true),
                    FECMOD = table.Column<DateTime>(type: "DATETIME2(7)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PEDIDO_LINEA_DEVOLUCION", x => x.ID);
                    table.CheckConstraint("CK_PEDIDO_LINEA_DEVOLUCION_CANTIDAD", "[CANTIDAD] > 0");
                    table.ForeignKey(
                        name: "FK_PEDIDO_LINEA_DEVOLUCION_ID_CREADOR",
                        column: x => x.ID_CREADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PEDIDO_LINEA_DEVOLUCION_ID_LINEA_RECEPCION",
                        column: x => x.ID_LINEA_RECEPCION,
                        principalSchema: "LOGISTICA",
                        principalTable: "PEDIDO_LINEA_RECEPCION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PEDIDO_LINEA_DEVOLUCION_ID_MODIFICADOR",
                        column: x => x.ID_MODIFICADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_MOVIMIENTO_ID_LINEA_DEVOLUCION",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                column: "ID_LINEA_DEVOLUCION");

            migrationBuilder.CreateIndex(
                name: "I_PEDIDO_LINEA_DEVOLUCION_ID_CREADOR",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_DEVOLUCION",
                column: "ID_CREADOR");

            migrationBuilder.CreateIndex(
                name: "I_PEDIDO_LINEA_DEVOLUCION_ID_LINEA_RECEPCION",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_DEVOLUCION",
                column: "ID_LINEA_RECEPCION");

            migrationBuilder.CreateIndex(
                name: "I_PEDIDO_LINEA_DEVOLUCION_ID_MODIFICADOR",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_DEVOLUCION",
                column: "ID_MODIFICADOR");

            migrationBuilder.AddForeignKey(
                name: "FK_ALMACEN_MOVIMIENTO_ID_LINEA_DEVOLUCION",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                column: "ID_LINEA_DEVOLUCION",
                principalSchema: "LOGISTICA",
                principalTable: "PEDIDO_LINEA_DEVOLUCION",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ALMACEN_MOVIMIENTO_ID_LINEA_DEVOLUCION",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO");

            migrationBuilder.DropTable(
                name: "PEDIDO_LINEA_DEVOLUCION",
                schema: "LOGISTICA");

            migrationBuilder.DropIndex(
                name: "I_ALMACEN_MOVIMIENTO_ID_LINEA_DEVOLUCION",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO");
        }
    }
}
