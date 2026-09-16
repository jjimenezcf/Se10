using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class clasedenaturalezaobligatoriayquitarclasedeunitario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CLASE",
                schema: "MT",
                table: "UNITARIO");

            // Sin defaultValue a propósito: si queda alguna naturaleza sin CLASE tras el backfill,
            // que el ALTER falle aquí (dato pendiente de revisar) en vez de rellenarla con un valor inventado
            // que no sería un enumClaseUnitario válido.
            migrationBuilder.AlterColumn<string>(
                name: "CLASE",
                schema: "MT",
                table: "MT_NATURALEZA",
                type: "VARCHAR(30)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(30)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // El valor real por unitario se pierde al hacer Up(); "Material" es solo un valor válido
            // de relleno para no dejar la columna con datos que enumClaseUnitario no pueda deserializar.
            migrationBuilder.AddColumn<string>(
                name: "CLASE",
                schema: "MT",
                table: "UNITARIO",
                type: "VARCHAR(30)",
                nullable: false,
                defaultValue: "Material");

            migrationBuilder.AlterColumn<string>(
                name: "CLASE",
                schema: "MT",
                table: "MT_NATURALEZA",
                type: "VARCHAR(30)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(30)");
        }
    }
}
