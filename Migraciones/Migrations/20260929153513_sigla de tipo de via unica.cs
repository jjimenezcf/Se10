using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class sigladetipodeviaunica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "I_TIPO_VIA_SIGLA",
                schema: "CALLEJERO",
                table: "TIPO_VIA");

            migrationBuilder.CreateIndex(
                name: "I_TIPO_VIA_SIGLA",
                schema: "CALLEJERO",
                table: "TIPO_VIA",
                column: "SIGLA",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "I_TIPO_VIA_SIGLA",
                schema: "CALLEJERO",
                table: "TIPO_VIA");

            migrationBuilder.CreateIndex(
                name: "I_TIPO_VIA_SIGLA",
                schema: "CALLEJERO",
                table: "TIPO_VIA",
                column: "SIGLA");
        }
    }
}
