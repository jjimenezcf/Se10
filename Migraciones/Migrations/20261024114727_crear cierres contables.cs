using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class crearcierrescontables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CIERRE_CONTABLE",
                schema: "CONTABILIDAD",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_SOCIEDAD = table.Column<int>(type: "INT", nullable: false),
                    ID_LOTE = table.Column<int>(type: "INT", nullable: false),
                    CERRADA_EL = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    NOMBRE = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    FECCRE = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    ID_CREADOR = table.Column<int>(type: "INT", nullable: false),
                    FECMOD = table.Column<DateTime>(type: "DATETIME2(7)", nullable: true),
                    ID_MODIFICADOR = table.Column<int>(type: "INT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CIERRE_CONTABLE", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CIERRE_CONTABLE_ID_CREADOR",
                        column: x => x.ID_CREADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CIERRE_CONTABLE_ID_LOTE",
                        column: x => x.ID_LOTE,
                        principalSchema: "SISDOC",
                        principalTable: "CIRCUITO_DOC",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CIERRE_CONTABLE_ID_MODIFICADOR",
                        column: x => x.ID_MODIFICADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CIERRE_CONTABLE_ID_SOCIEDAD",
                        column: x => x.ID_SOCIEDAD,
                        principalSchema: "TERCEROS",
                        principalTable: "SOCIEDAD",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_CIERRE_CONTABLE_ID_CREADOR",
                schema: "CONTABILIDAD",
                table: "CIERRE_CONTABLE",
                column: "ID_CREADOR");

            migrationBuilder.CreateIndex(
                name: "I_CIERRE_CONTABLE_ID_LOTE",
                schema: "CONTABILIDAD",
                table: "CIERRE_CONTABLE",
                column: "ID_LOTE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_CIERRE_CONTABLE_ID_MODIFICADOR",
                schema: "CONTABILIDAD",
                table: "CIERRE_CONTABLE",
                column: "ID_MODIFICADOR");

            migrationBuilder.CreateIndex(
                name: "I_CIERRE_CONTABLE_ID_SOCIEDAD",
                schema: "CONTABILIDAD",
                table: "CIERRE_CONTABLE",
                column: "ID_SOCIEDAD");

            migrationBuilder.CreateIndex(
                name: "I_CIERRE_CONTABLE_NOMBRE",
                schema: "CONTABILIDAD",
                table: "CIERRE_CONTABLE",
                column: "NOMBRE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CIERRE_CONTABLE",
                schema: "CONTABILIDAD");
        }
    }
}
