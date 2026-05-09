using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class incluirmodosdepagopordefectoenunproveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ID_CUENTA_CARGO",
                schema: "TERCEROS",
                table: "PROVEEDOR",
                type: "INT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_TARJETA",
                schema: "TERCEROS",
                table: "PROVEEDOR",
                type: "INT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MODO",
                schema: "TERCEROS",
                table: "PROVEEDOR",
                type: "VARCHAR(30)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "I_PROVEEDOR_ID_CUENTA_CARGO",
                schema: "TERCEROS",
                table: "PROVEEDOR",
                column: "ID_CUENTA_CARGO");

            migrationBuilder.CreateIndex(
                name: "I_PROVEEDOR_ID_TARJETA",
                schema: "TERCEROS",
                table: "PROVEEDOR",
                column: "ID_TARJETA");

            migrationBuilder.AddForeignKey(
                name: "FK_PROVEEDOR_ID_CUENTA_CARGO",
                schema: "TERCEROS",
                table: "PROVEEDOR",
                column: "ID_CUENTA_CARGO",
                principalSchema: "TERCEROS",
                principalTable: "SOCIEDAD_CUENTA",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PROVEEDOR_ID_TARJETA",
                schema: "TERCEROS",
                table: "PROVEEDOR",
                column: "ID_TARJETA",
                principalSchema: "TERCEROS",
                principalTable: "SOCIEDAD_TARJETA",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PROVEEDOR_ID_CUENTA_CARGO",
                schema: "TERCEROS",
                table: "PROVEEDOR");

            migrationBuilder.DropForeignKey(
                name: "FK_PROVEEDOR_ID_TARJETA",
                schema: "TERCEROS",
                table: "PROVEEDOR");

            migrationBuilder.DropIndex(
                name: "I_PROVEEDOR_ID_CUENTA_CARGO",
                schema: "TERCEROS",
                table: "PROVEEDOR");

            migrationBuilder.DropIndex(
                name: "I_PROVEEDOR_ID_TARJETA",
                schema: "TERCEROS",
                table: "PROVEEDOR");

            migrationBuilder.DropColumn(
                name: "ID_CUENTA_CARGO",
                schema: "TERCEROS",
                table: "PROVEEDOR");

            migrationBuilder.DropColumn(
                name: "ID_TARJETA",
                schema: "TERCEROS",
                table: "PROVEEDOR");

            migrationBuilder.DropColumn(
                name: "MODO",
                schema: "TERCEROS",
                table: "PROVEEDOR");
        }
    }
}
