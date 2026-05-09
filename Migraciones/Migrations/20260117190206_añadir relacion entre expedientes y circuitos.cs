using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class añadirrelacionentreexpedientesycircuitos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EXPEDIENTE_CIRCUITO_DOC",
                schema: "EXPEDIENTE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ELEMENTO1 = table.Column<int>(type: "INT", nullable: false),
                    ID_ELEMENTO2 = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXPEDIENTE_CIRCUITO_DOC", x => x.ID);
                    table.UniqueConstraint("AK_EXPEDIENTE_CIRCUITO_DOC", x => new { x.ID_ELEMENTO1, x.ID_ELEMENTO2 });
                    table.ForeignKey(
                        name: "FK_EXPEDIENTE_CIRCUITO_DOC_ID_ELEMENTO1",
                        column: x => x.ID_ELEMENTO1,
                        principalSchema: "EXPEDIENTE",
                        principalTable: "EXPEDIENTE",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EXPEDIENTE_CIRCUITO_DOC_ID_ELEMENTO2",
                        column: x => x.ID_ELEMENTO2,
                        principalSchema: "SISDOC",
                        principalTable: "CIRCUITO_DOC",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_EXPEDIENTE_CIRCUITO_DOC_ID_ELEMENTO1",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_CIRCUITO_DOC",
                column: "ID_ELEMENTO1");

            migrationBuilder.CreateIndex(
                name: "I_EXPEDIENTE_CIRCUITO_DOC_ID_ELEMENTO2",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_CIRCUITO_DOC",
                column: "ID_ELEMENTO2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EXPEDIENTE_CIRCUITO_DOC",
                schema: "EXPEDIENTE");
        }
    }
}
