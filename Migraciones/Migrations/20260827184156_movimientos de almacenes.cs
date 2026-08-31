using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class movimientosdealmacenes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ALMACEN_TIPO_MOVIMIENTO",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CLASE = table.Column<string>(type: "VARCHAR(20)", nullable: false),
                    NOMBRE = table.Column<string>(type: "VARCHAR(250)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ALMACEN_TIPO_MOVIMIENTO", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ALMACEN_MOVIMIENTO",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ALMACEN = table.Column<int>(type: "INT", nullable: false),
                    ID_UNITARIO = table.Column<int>(type: "INT", nullable: false),
                    ID_TIPO_MOVIMIENTO = table.Column<int>(type: "INT", nullable: false),
                    CANTIDAD = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: false),
                    STOCK = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: false),
                    PRECIO = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: false),
                    VALOR = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: false),
                    REALIZADO_EL = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    ID_MOVIMIENTO = table.Column<int>(type: "INT", nullable: true),
                    ID_LINEA_ALBARAN = table.Column<int>(type: "INT", nullable: true),
                    ID_LINEA_DEVOLUCION = table.Column<int>(type: "INT", nullable: true),
                    ID_LINEA_INVENTARIO = table.Column<int>(type: "INT", nullable: true),
                    ID_PREASIENTO = table.Column<int>(type: "INT", nullable: true),
                    ID_CREADOR = table.Column<int>(type: "INT", nullable: false),
                    ID_MODIFICADOR = table.Column<int>(type: "INT", nullable: true),
                    FECCRE = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    FECMOD = table.Column<DateTime>(type: "DATETIME2(7)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ALMACEN_MOVIMIENTO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ALMACEN_MOVIMIENTO_ID_ALMACEN",
                        column: x => x.ID_ALMACEN,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_MOVIMIENTO_ID_CREADOR",
                        column: x => x.ID_CREADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_MOVIMIENTO_ID_MODIFICADOR",
                        column: x => x.ID_MODIFICADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_MOVIMIENTO_ID_PREASIENTO",
                        column: x => x.ID_PREASIENTO,
                        principalSchema: "CONTABILIDAD",
                        principalTable: "PREASIENTO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_MOVIMIENTO_ID_TIPO_MOVIMIENTO",
                        column: x => x.ID_TIPO_MOVIMIENTO,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN_TIPO_MOVIMIENTO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_MOVIMIENTO_ID_UNITARIO",
                        column: x => x.ID_UNITARIO,
                        principalSchema: "MT",
                        principalTable: "UNITARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_MOVIMIENTO_ID_ALMACEN",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                column: "ID_ALMACEN");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_MOVIMIENTO_ID_CREADOR",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                column: "ID_CREADOR");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_MOVIMIENTO_ID_MODIFICADOR",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                column: "ID_MODIFICADOR");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_MOVIMIENTO_ID_PREASIENTO",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                column: "ID_PREASIENTO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_MOVIMIENTO_ID_TIPO_MOVIMIENTO",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                column: "ID_TIPO_MOVIMIENTO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_MOVIMIENTO_ID_UNITARIO",
                schema: "LOGISTICA",
                table: "ALMACEN_MOVIMIENTO",
                column: "ID_UNITARIO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_TIPO_MOVIMIENTO_NOMBRE",
                schema: "LOGISTICA",
                table: "ALMACEN_TIPO_MOVIMIENTO",
                column: "NOMBRE",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ALMACEN_MOVIMIENTO",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "ALMACEN_TIPO_MOVIMIENTO",
                schema: "LOGISTICA");
        }
    }
}
