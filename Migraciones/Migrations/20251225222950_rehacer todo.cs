using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class rehacertodo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "COSTAS",
                schema: "JURIDICO",
                table: "PLEITO");

            migrationBuilder.DropColumn(
                name: "LITIGADO",
                schema: "JURIDICO",
                table: "PLEITO");

            migrationBuilder.DropColumn(
                name: "NIG",
                schema: "JURIDICO",
                table: "PLEITO");

            migrationBuilder.DropColumn(
                name: "REF_EXTERNA",
                schema: "JURIDICO",
                table: "PLEITO");

            migrationBuilder.DropColumn(
                name: "SENTENCIADO",
                schema: "JURIDICO",
                table: "PLEITO");

            migrationBuilder.DropColumn(
                name: "SENTENCIADO_EL",
                schema: "JURIDICO",
                table: "PLEITO");

            migrationBuilder.CreateTable(
                name: "PLEITO_RECOBRO",
                schema: "JURIDICO",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PRINCIPAL = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: false),
                    INTERESES = table.Column<decimal>(type: "DECIMAL(18,6)", nullable: true),
                    FECHA = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    ANOTACION = table.Column<string>(type: "VARCHAR(2000)", nullable: true),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PLEITO_RECOBRO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PLEITO_RECOBRO_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "JURIDICO",
                        principalTable: "PLEITO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_PLEITO_RECOBRO_ID_ELEMENTO",
                schema: "JURIDICO",
                table: "PLEITO_RECOBRO",
                column: "ID_ELEMENTO",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PLEITO_RECOBRO",
                schema: "JURIDICO");

            migrationBuilder.AddColumn<decimal>(
                name: "COSTAS",
                schema: "JURIDICO",
                table: "PLEITO",
                type: "DECIMAL(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LITIGADO",
                schema: "JURIDICO",
                table: "PLEITO",
                type: "DECIMAL(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NIG",
                schema: "JURIDICO",
                table: "PLEITO",
                type: "VARCHAR(25)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "REF_EXTERNA",
                schema: "JURIDICO",
                table: "PLEITO",
                type: "VARCHAR(50)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SENTENCIADO",
                schema: "JURIDICO",
                table: "PLEITO",
                type: "DECIMAL(18,6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SENTENCIADO_EL",
                schema: "JURIDICO",
                table: "PLEITO",
                type: "DATETIME2(7)",
                nullable: true);
        }
    }
}
