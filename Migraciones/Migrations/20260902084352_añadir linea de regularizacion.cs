using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class añadirlineaderegularizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "REGULARIZACION_LINEA",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false),
                    ORDEN = table.Column<int>(type: "INT", nullable: false),
                    ID_UNITARIO = table.Column<int>(type: "INT", nullable: false),
                    CANTIDAD = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: false),
                    PRECIO = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: false),
                    ID_CREADOR = table.Column<int>(type: "INT", nullable: false),
                    FECCRE = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    ID_MODIFICADOR = table.Column<int>(type: "INT", nullable: true),
                    FECMOD = table.Column<DateTime>(type: "DATETIME2(7)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGULARIZACION_LINEA", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_LINEA_ID_CREADOR",
                        column: x => x.ID_CREADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_LINEA_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_LINEA_ID_MODIFICADOR",
                        column: x => x.ID_MODIFICADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_LINEA_ID_UNITARIO",
                        column: x => x.ID_UNITARIO,
                        principalSchema: "MT",
                        principalTable: "UNITARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_LINEA_ID_CREADOR",
                schema: "LOGISTICA",
                table: "REGULARIZACION_LINEA",
                column: "ID_CREADOR");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_LINEA_ID_ELEMENTO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_LINEA",
                column: "ID_ELEMENTO");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_LINEA_ID_MODIFICADOR",
                schema: "LOGISTICA",
                table: "REGULARIZACION_LINEA",
                column: "ID_MODIFICADOR");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_LINEA_ID_UNITARIO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_LINEA",
                column: "ID_UNITARIO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "REGULARIZACION_LINEA",
                schema: "LOGISTICA");
        }
    }
}
