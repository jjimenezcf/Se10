using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class fechaderecepcionenrecepcionesdelineasdepedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RECEPCIONADA_EL",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                type: "DATETIME2(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // las recepciones registradas antes de existir la fecha toman como fecha de recepción la de su creación
            migrationBuilder.Sql("UPDATE LOGISTICA.PEDIDO_LINEA_RECEPCION SET RECEPCIONADA_EL = CAST(FECCRE AS DATE)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RECEPCIONADA_EL",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION");
        }
    }
}
