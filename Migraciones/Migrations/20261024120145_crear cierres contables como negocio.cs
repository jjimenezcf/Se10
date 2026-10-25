using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class crearcierrescontablescomonegocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CIERRE_CONTABLE_AUDITORIA",
                schema: "CONTABILIDAD",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false),
                    ID_USUARIO = table.Column<int>(type: "INT", nullable: false),
                    OPERACION = table.Column<string>(type: "CHAR(1)", nullable: false),
                    REGISTRO = table.Column<string>(type: "VARCHAR(MAX)", nullable: false),
                    AUDITADO_EL = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CIERRE_CONTABLE_AUDITORIA", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "I_CIERRE_CONTABLE_AUDITORIA_ID_ELEMENTO",
                schema: "CONTABILIDAD",
                table: "CIERRE_CONTABLE_AUDITORIA",
                column: "ID_ELEMENTO");

            migrationBuilder.CreateIndex(
                name: "I_CIERRE_CONTABLE_AUDITORIA_ID_USUARIO",
                schema: "CONTABILIDAD",
                table: "CIERRE_CONTABLE_AUDITORIA",
                column: "ID_USUARIO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CIERRE_CONTABLE_AUDITORIA",
                schema: "CONTABILIDAD");
        }
    }
}
