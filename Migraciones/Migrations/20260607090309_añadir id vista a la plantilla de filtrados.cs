using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class añadiridvistaalaplantilladefiltrados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ID_VISTA",
                schema: "NEGOCIO",
                table: "PLANTILLA_FILTRADO",
                type: "INT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "I_PLANTILLA_FILTRADO_ID_VISTA",
                schema: "NEGOCIO",
                table: "PLANTILLA_FILTRADO",
                column: "ID_VISTA");

            migrationBuilder.AddForeignKey(
                name: "FK_PLANTILLA_FILTRADO_ID_VISTA",
                schema: "NEGOCIO",
                table: "PLANTILLA_FILTRADO",
                column: "ID_VISTA",
                principalSchema: "ENTORNO",
                principalTable: "VISTA_MVC",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PLANTILLA_FILTRADO_ID_VISTA",
                schema: "NEGOCIO",
                table: "PLANTILLA_FILTRADO");

            migrationBuilder.DropIndex(
                name: "I_PLANTILLA_FILTRADO_ID_VISTA",
                schema: "NEGOCIO",
                table: "PLANTILLA_FILTRADO");

            migrationBuilder.DropColumn(
                name: "ID_VISTA",
                schema: "NEGOCIO",
                table: "PLANTILLA_FILTRADO");
        }
    }
}
