using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class añadirtabladeconsultaconguid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CONSULTA_CON_GUID",
                schema: "NEGOCIO",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_NEGOCIO = table.Column<int>(type: "INT", nullable: false),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false),
                    GUID = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    ID_USUARIO = table.Column<int>(type: "INT", nullable: false),
                    CREADO_EL = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    DESCARGADO_EL = table.Column<DateTime>(type: "DATETIME2(7)", nullable: true),
                    CADUCA_EL = table.Column<DateTime>(type: "DATETIME2(7)", nullable: true),
                    MAXIMO_DESCARGAS = table.Column<int>(type: "INT", nullable: true),
                    NUMERO = table.Column<int>(type: "INT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTA_CON_GUID", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CONSULTA_CON_GUID_ID_NEGOCIO",
                        column: x => x.ID_NEGOCIO,
                        principalSchema: "NEGOCIO",
                        principalTable: "NEGOCIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CONSULTA_CON_GUID_ID_USUARIO",
                        column: x => x.ID_USUARIO,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_CONSULTA_CON_GUID_ID_NEGOCIO",
                schema: "NEGOCIO",
                table: "CONSULTA_CON_GUID",
                column: "ID_NEGOCIO");

            migrationBuilder.CreateIndex(
                name: "I_CONSULTA_CON_GUID_ID_USUARIO",
                schema: "NEGOCIO",
                table: "CONSULTA_CON_GUID",
                column: "ID_USUARIO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONSULTA_CON_GUID",
                schema: "NEGOCIO");
        }
    }
}
