using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class preasientosdeapuntesdeexpediente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ID_PREASIENTO",
                schema: "EXPEDIENTE",
                table: "APUNTE",
                type: "INT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "I_APUNTE_ID_PREASIENTO",
                schema: "EXPEDIENTE",
                table: "APUNTE",
                column: "ID_PREASIENTO");

            migrationBuilder.AddForeignKey(
                name: "FK_APUNTE_ID_PREASIENTO",
                schema: "EXPEDIENTE",
                table: "APUNTE",
                column: "ID_PREASIENTO",
                principalSchema: "CONTABILIDAD",
                principalTable: "PREASIENTO",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_APUNTE_ID_PREASIENTO",
                schema: "EXPEDIENTE",
                table: "APUNTE");

            migrationBuilder.DropIndex(
                name: "I_APUNTE_ID_PREASIENTO",
                schema: "EXPEDIENTE",
                table: "APUNTE");

            migrationBuilder.DropColumn(
                name: "ID_PREASIENTO",
                schema: "EXPEDIENTE",
                table: "APUNTE");
        }
    }
}
