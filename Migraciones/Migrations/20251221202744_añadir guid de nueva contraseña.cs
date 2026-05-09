using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class añadirguiddenuevacontraseña : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GUID",
                schema: "ENTORNO",
                table: "USUARIO",
                type: "UNIQUEIDENTIFIER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SOLICITADO_EL",
                schema: "ENTORNO",
                table: "USUARIO",
                type: "DATETIME2(7)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GUID",
                schema: "ENTORNO",
                table: "USUARIO");

            migrationBuilder.DropColumn(
                name: "SOLICITADO_EL",
                schema: "ENTORNO",
                table: "USUARIO");
        }
    }
}
