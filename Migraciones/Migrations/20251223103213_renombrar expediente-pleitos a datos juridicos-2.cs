using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class renombrarexpedientepleitosadatosjuridicos2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EXPEDIENTE_PLEITO",
                schema: "EXPEDIENTE");

            migrationBuilder.AddColumn<bool>(
                name: "USA_DATOS_JURIDICOS",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_TIPO",
                type: "BIT",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EXPEDIENTE_DATOS_JURIDICOS",
                schema: "EXPEDIENTE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_PROCURADOR = table.Column<int>(type: "INT", nullable: true),
                    ID_ABOGADO = table.Column<int>(type: "INT", nullable: true),
                    ID_JUZGADO = table.Column<int>(type: "INT", nullable: true),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXPEDIENTE_DATOS_JURIDICOS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EXPEDIENTE_DATOS_JURIDICOS_ID_ABOGADO",
                        column: x => x.ID_ABOGADO,
                        principalSchema: "TERCEROS",
                        principalTable: "ABOGADO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EXPEDIENTE_DATOS_JURIDICOS_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "EXPEDIENTE",
                        principalTable: "EXPEDIENTE",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EXPEDIENTE_DATOS_JURIDICOS_ID_JUZGADO",
                        column: x => x.ID_JUZGADO,
                        principalSchema: "TERCEROS",
                        principalTable: "JUZGADO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EXPEDIENTE_DATOS_JURIDICOS_ID_PROCURADOR",
                        column: x => x.ID_PROCURADOR,
                        principalSchema: "TERCEROS",
                        principalTable: "PROCURADOR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_EXPEDIENTE_DATOS_JURIDICOS_ID_ABOGADO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS",
                column: "ID_ABOGADO");

            migrationBuilder.CreateIndex(
                name: "I_EXPEDIENTE_DATOS_JURIDICOS_ID_ELEMENTO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS",
                column: "ID_ELEMENTO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_EXPEDIENTE_DATOS_JURIDICOS_ID_JUZGADO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS",
                column: "ID_JUZGADO");

            migrationBuilder.CreateIndex(
                name: "I_EXPEDIENTE_DATOS_JURIDICOS_ID_PROCURADOR",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_DATOS_JURIDICOS",
                column: "ID_PROCURADOR");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EXPEDIENTE_DATOS_JURIDICOS",
                schema: "EXPEDIENTE");

            migrationBuilder.DropColumn(
                name: "USA_DATOS_JURIDICOS",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_TIPO");

            migrationBuilder.CreateTable(
                name: "EXPEDIENTE_PLEITO",
                schema: "EXPEDIENTE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ABOGADO = table.Column<int>(type: "INT", nullable: true),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false),
                    ID_JUZGADO = table.Column<int>(type: "INT", nullable: true),
                    ID_PROCURADOR = table.Column<int>(type: "INT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXPEDIENTE_PLEITO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EXPEDIENTE_PLEITO_ID_ABOGADO",
                        column: x => x.ID_ABOGADO,
                        principalSchema: "TERCEROS",
                        principalTable: "ABOGADO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EXPEDIENTE_PLEITO_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "EXPEDIENTE",
                        principalTable: "EXPEDIENTE",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EXPEDIENTE_PLEITO_ID_JUZGADO",
                        column: x => x.ID_JUZGADO,
                        principalSchema: "TERCEROS",
                        principalTable: "JUZGADO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EXPEDIENTE_PLEITO_ID_PROCURADOR",
                        column: x => x.ID_PROCURADOR,
                        principalSchema: "TERCEROS",
                        principalTable: "PROCURADOR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_EXPEDIENTE_PLEITO_ID_ABOGADO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_PLEITO",
                column: "ID_ABOGADO");

            migrationBuilder.CreateIndex(
                name: "I_EXPEDIENTE_PLEITO_ID_ELEMENTO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_PLEITO",
                column: "ID_ELEMENTO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_EXPEDIENTE_PLEITO_ID_JUZGADO",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_PLEITO",
                column: "ID_JUZGADO");

            migrationBuilder.CreateIndex(
                name: "I_EXPEDIENTE_PLEITO_ID_PROCURADOR",
                schema: "EXPEDIENTE",
                table: "EXPEDIENTE_PLEITO",
                column: "ID_PROCURADOR");
        }
    }
}
