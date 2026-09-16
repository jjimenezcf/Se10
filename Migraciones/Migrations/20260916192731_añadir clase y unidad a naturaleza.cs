using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class añadirclaseyunidadanaturaleza : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CLASE",
                schema: "MT",
                table: "MT_NATURALEZA",
                type: "VARCHAR(30)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_UNIDAD",
                schema: "MT",
                table: "MT_NATURALEZA",
                type: "INT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "I_MT_NATURALEZA_ID_UNIDAD",
                schema: "MT",
                table: "MT_NATURALEZA",
                column: "ID_UNIDAD");

            migrationBuilder.AddForeignKey(
                name: "FK_MT_NATURALEZA_ID_UNIDAD",
                schema: "MT",
                table: "MT_NATURALEZA",
                column: "ID_UNIDAD",
                principalSchema: "MT",
                principalTable: "MT_UNIDAD",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MT_NATURALEZA_ID_UNIDAD",
                schema: "MT",
                table: "MT_NATURALEZA");

            migrationBuilder.DropIndex(
                name: "I_MT_NATURALEZA_ID_UNIDAD",
                schema: "MT",
                table: "MT_NATURALEZA");

            migrationBuilder.DropColumn(
                name: "CLASE",
                schema: "MT",
                table: "MT_NATURALEZA");

            migrationBuilder.DropColumn(
                name: "ID_UNIDAD",
                schema: "MT",
                table: "MT_NATURALEZA");
        }
    }
}
