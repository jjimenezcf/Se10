using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class añadirintentosfallidosdeconexión : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BLOQUEADO_EL",
                schema: "ENTORNO",
                table: "USUARIO",
                type: "DATETIME2(7)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FALLIDOS",
                schema: "ENTORNO",
                table: "USUARIO",
                type: "INT",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BLOQUEADO_EL",
                schema: "ENTORNO",
                table: "USUARIO");

            migrationBuilder.DropColumn(
                name: "FALLIDOS",
                schema: "ENTORNO",
                table: "USUARIO");
        }
    }
}
