using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class AgregarGuidsDeConsultaAlFacturador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GUID_PDF",
                schema: "VENTA",
                table: "PETICION_DE_FACTURA_EMT",
                type: "UNIQUEIDENTIFIER",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GUID_XML",
                schema: "VENTA",
                table: "PETICION_DE_FACTURA_EMT",
                type: "UNIQUEIDENTIFIER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GUID_PDF",
                schema: "VENTA",
                table: "PETICION_DE_FACTURA_EMT");

            migrationBuilder.DropColumn(
                name: "GUID_XML",
                schema: "VENTA",
                table: "PETICION_DE_FACTURA_EMT");
        }
    }
}
