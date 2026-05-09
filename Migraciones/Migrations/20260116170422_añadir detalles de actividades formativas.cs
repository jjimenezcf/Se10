using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class añadirdetallesdeactividadesformativas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CIRCUITO_DOC_ACTIVIDAD_FORMATIVA",
                schema: "SISDOC",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_RESPONSABLE = table.Column<int>(type: "INT", nullable: true),
                    INICIO = table.Column<DateTime>(type: "DATETIME2(7)", nullable: true),
                    FIN = table.Column<DateTime>(type: "DATETIME2(7)", nullable: true),
                    COSTE = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: true),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CIRCUITO_DOC_ACTIVIDAD_FORMATIVA", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CIRCUITO_DOC_ACTIVIDAD_FORMATIVA_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "SISDOC",
                        principalTable: "CIRCUITO_DOC",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CIRCUITO_DOC_ACTIVIDAD_FORMATIVA_ID_RESPONSABLE",
                        column: x => x.ID_RESPONSABLE,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CIRCUITO_DOC_INSCRITO",
                schema: "SISDOC",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false),
                    ID_INSCRITO = table.Column<int>(type: "INT", nullable: false),
                    ASISTIO = table.Column<bool>(type: "BIT", nullable: false),
                    IMPORTE = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: true),
                    ID_ARCHIVO = table.Column<int>(type: "INT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CIRCUITO_DOC_INSCRITO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CIRCUITO_DOC_INSCRITO_ID_ARCHIVO",
                        column: x => x.ID_ARCHIVO,
                        principalSchema: "SISDOC",
                        principalTable: "ARCHIVO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CIRCUITO_DOC_INSCRITO_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "SISDOC",
                        principalTable: "CIRCUITO_DOC",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CIRCUITO_DOC_INSCRITO_ID_INSCRITO",
                        column: x => x.ID_INSCRITO,
                        principalSchema: "TERCEROS",
                        principalTable: "INTERLOCUTOR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CIRCUITO_DOC_VOLUNTARIO",
                schema: "SISDOC",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false),
                    ID_INSCRITO = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CIRCUITO_DOC_VOLUNTARIO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CIRCUITO_DOC_VOLUNTARIO_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "SISDOC",
                        principalTable: "CIRCUITO_DOC",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CIRCUITO_DOC_VOLUNTARIO_ID_INSCRITO",
                        column: x => x.ID_INSCRITO,
                        principalSchema: "TERCEROS",
                        principalTable: "INTERLOCUTOR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_CIRCUITO_DOC_ACTIVIDAD_FORMATIVA_ID_ELEMENTO",
                schema: "SISDOC",
                table: "CIRCUITO_DOC_ACTIVIDAD_FORMATIVA",
                column: "ID_ELEMENTO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_CIRCUITO_DOC_ACTIVIDAD_FORMATIVA_ID_RESPONSABLE",
                schema: "SISDOC",
                table: "CIRCUITO_DOC_ACTIVIDAD_FORMATIVA",
                column: "ID_RESPONSABLE");

            migrationBuilder.CreateIndex(
                name: "I_CIRCUITO_DOC_INSCRITO_ID_ARCHIVO",
                schema: "SISDOC",
                table: "CIRCUITO_DOC_INSCRITO",
                column: "ID_ARCHIVO");

            migrationBuilder.CreateIndex(
                name: "I_CIRCUITO_DOC_INSCRITO_ID_ELEMENTO",
                schema: "SISDOC",
                table: "CIRCUITO_DOC_INSCRITO",
                column: "ID_ELEMENTO");

            migrationBuilder.CreateIndex(
                name: "I_CIRCUITO_DOC_INSCRITO_ID_INSCRITO",
                schema: "SISDOC",
                table: "CIRCUITO_DOC_INSCRITO",
                column: "ID_INSCRITO");

            migrationBuilder.CreateIndex(
                name: "I_CIRCUITO_DOC_VOLUNTARIO_ID_ELEMENTO",
                schema: "SISDOC",
                table: "CIRCUITO_DOC_VOLUNTARIO",
                column: "ID_ELEMENTO");

            migrationBuilder.CreateIndex(
                name: "I_CIRCUITO_DOC_VOLUNTARIO_ID_INSCRITO",
                schema: "SISDOC",
                table: "CIRCUITO_DOC_VOLUNTARIO",
                column: "ID_INSCRITO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CIRCUITO_DOC_ACTIVIDAD_FORMATIVA",
                schema: "SISDOC");

            migrationBuilder.DropTable(
                name: "CIRCUITO_DOC_INSCRITO",
                schema: "SISDOC");

            migrationBuilder.DropTable(
                name: "CIRCUITO_DOC_VOLUNTARIO",
                schema: "SISDOC");
        }
    }
}
