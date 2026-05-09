using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class tabladepeticionesparafacturaraterceros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PETICION_DE_FACTURA_EMT",
                schema: "VENTA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GUID = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    CREADO_EL = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    PETICION = table.Column<string>(type: "VARCHAR(50)", nullable: false),
                    ID_FACTURADOR = table.Column<int>(type: "INT", nullable: false),
                    FACTURA_JSON = table.Column<string>(type: "VARCHAR(MAX)", nullable: true),
                    ID_FACTURA_EMT = table.Column<int>(type: "INT", nullable: true),
                    ERROR = table.Column<string>(type: "VARCHAR(MAX)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PETICION_DE_FACTURA_EMT", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PETICION_DE_FACTURA_EMT_ID_FACTURADOR",
                        column: x => x.ID_FACTURADOR,
                        principalSchema: "TERCEROS",
                        principalTable: "FACTURADOR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PETICION_DE_FACTURA_EMT_ID_FACTURA_EMT",
                        column: x => x.ID_FACTURA_EMT,
                        principalSchema: "VENTA",
                        principalTable: "FACTURA_EMT",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_PETICION_DE_FACTURA_EMT_ID_FACTURA_EMT",
                schema: "VENTA",
                table: "PETICION_DE_FACTURA_EMT",
                column: "ID_FACTURA_EMT",
                unique: true,
                filter: "[ID_FACTURA_EMT] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "I_PETICION_DE_FACTURA_EMT_ID_FACTURADOR",
                schema: "VENTA",
                table: "PETICION_DE_FACTURA_EMT",
                column: "ID_FACTURADOR");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PETICION_DE_FACTURA_EMT",
                schema: "VENTA");
        }
    }
}
