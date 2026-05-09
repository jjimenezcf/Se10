using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class añadirtablasdepreguntas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IA_PREGUNTA",
                schema: "ENTORNO",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GUID = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    ID_USUARIO = table.Column<int>(type: "INT", nullable: false),
                    FECHA = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    PREGUNTA = table.Column<string>(type: "VARCHAR(2000)", nullable: false),
                    RESPUESTA = table.Column<string>(type: "VARCHAR(MAX)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IA_PREGUNTA", x => x.ID);
                    table.ForeignKey(
                        name: "FK_IA_PREGUNTA_ID_USUARIO",
                        column: x => x.ID_USUARIO,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_IA_PREGUNTA_GUID",
                schema: "ENTORNO",
                table: "IA_PREGUNTA",
                column: "GUID");

            migrationBuilder.CreateIndex(
                name: "I_IA_PREGUNTA_ID_USUARIO",
                schema: "ENTORNO",
                table: "IA_PREGUNTA",
                column: "ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "I_IA_PREGUNTA_PREGUNTA",
                schema: "ENTORNO",
                table: "IA_PREGUNTA",
                column: "PREGUNTA");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IA_PREGUNTA",
                schema: "ENTORNO");
        }
    }
}
