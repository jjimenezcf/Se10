using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class tabladefacturadordesociedades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FACTURADOR",
                schema: "TERCEROS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false),
                    ID_TIPO_FACTURA = table.Column<int>(type: "INT", nullable: false),
                    ID_CG = table.Column<int>(type: "INT", nullable: false),
                    APIKEY = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    MAPEOS_JSON = table.Column<string>(type: "VARCHAR(MAX)", nullable: false),
                    ACTIVA = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FACTURADOR", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FACTURADOR_ID_CG",
                        column: x => x.ID_CG,
                        principalSchema: "TERCEROS",
                        principalTable: "CENTRO_GESTOR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FACTURADOR_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "TERCEROS",
                        principalTable: "SOCIEDAD",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FACTURADOR_ID_TIPO_FACTURA",
                        column: x => x.ID_TIPO_FACTURA,
                        principalSchema: "VENTA",
                        principalTable: "FACTURA_EMT_TIPO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_FACTURADOR_ID_CG",
                schema: "TERCEROS",
                table: "FACTURADOR",
                column: "ID_CG");

            migrationBuilder.CreateIndex(
                name: "I_FACTURADOR_ID_ELEMENTO",
                schema: "TERCEROS",
                table: "FACTURADOR",
                column: "ID_ELEMENTO");

            migrationBuilder.CreateIndex(
                name: "I_FACTURADOR_ID_ELEMENTO_APIKEY",
                schema: "TERCEROS",
                table: "FACTURADOR",
                columns: new[] { "ID_ELEMENTO", "APIKEY" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_FACTURADOR_ID_ELEMENTO_ID_CG_ID_TIPO_FACTURA",
                schema: "TERCEROS",
                table: "FACTURADOR",
                columns: new[] { "ID_ELEMENTO", "ID_CG", "ID_TIPO_FACTURA" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_FACTURADOR_ID_TIPO_FACTURA",
                schema: "TERCEROS",
                table: "FACTURADOR",
                column: "ID_TIPO_FACTURA");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FACTURADOR",
                schema: "TERCEROS");
        }
    }
}
