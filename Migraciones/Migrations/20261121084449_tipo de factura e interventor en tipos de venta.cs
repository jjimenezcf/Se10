using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class tipodefacturaeinterventorentiposdeventa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // los tipos de venta que ya existen necesitan su permiso de interventor (lo crea GestorDeTiposDeElemento al insertar un tipo):
            // se añade la columna admitiendo nulos, se crea un permiso por tipo con el nombre y la clase de su permiso de gestor
            // ("VENTA.TIPO (Gestor): X" -> "VENTA.TIPO (Interventor): X") y el tipo de permiso de interventor, se asigna y se hace obligatoria
            migrationBuilder.AddColumn<int>(
                name: "ID_INTERVENTOR",
                schema: "LOGISTICA",
                table: "VENTA_TIPO",
                type: "INT",
                nullable: true);

            migrationBuilder.Sql(@"
INSERT INTO SEGURIDAD.PERMISO (NOMBRE, IDCLASE, IDTIPO)
SELECT REPLACE(G.NOMBRE, '(Gestor)', '(Interventor)'), G.IDCLASE, TP.ID
FROM LOGISTICA.VENTA_TIPO T
JOIN SEGURIDAD.PERMISO G ON G.ID = T.ID_GESTOR
CROSS JOIN SEGURIDAD.TIPO_PERMISO TP
WHERE TP.NOMBRE = 'Interventor'
  AND G.NOMBRE LIKE '%(Gestor)%'
  AND NOT EXISTS (SELECT 1 FROM SEGURIDAD.PERMISO P WHERE P.NOMBRE = REPLACE(G.NOMBRE, '(Gestor)', '(Interventor)'));

UPDATE T SET ID_INTERVENTOR = P.ID
FROM LOGISTICA.VENTA_TIPO T
JOIN SEGURIDAD.PERMISO G ON G.ID = T.ID_GESTOR
JOIN SEGURIDAD.PERMISO P ON P.NOMBRE = REPLACE(G.NOMBRE, '(Gestor)', '(Interventor)')
WHERE G.NOMBRE LIKE '%(Gestor)%';
");

            migrationBuilder.AlterColumn<int>(
                name: "ID_INTERVENTOR",
                schema: "LOGISTICA",
                table: "VENTA_TIPO",
                type: "INT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INT",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_TIPO_FACTURA",
                schema: "LOGISTICA",
                table: "VENTA_TIPO",
                type: "INT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "I_VENTA_TIPO_ID_INTERVENTOR",
                schema: "LOGISTICA",
                table: "VENTA_TIPO",
                column: "ID_INTERVENTOR",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_VENTA_TIPO_ID_TIPO_FACTURA",
                schema: "LOGISTICA",
                table: "VENTA_TIPO",
                column: "ID_TIPO_FACTURA");

            migrationBuilder.AddForeignKey(
                name: "FK_VENTA_TIPO_ID_INTERVENTOR",
                schema: "LOGISTICA",
                table: "VENTA_TIPO",
                column: "ID_INTERVENTOR",
                principalSchema: "SEGURIDAD",
                principalTable: "PERMISO",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VENTA_TIPO_ID_TIPO_FACTURA",
                schema: "LOGISTICA",
                table: "VENTA_TIPO",
                column: "ID_TIPO_FACTURA",
                principalSchema: "VENTA",
                principalTable: "FACTURA_EMT_TIPO",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VENTA_TIPO_ID_INTERVENTOR",
                schema: "LOGISTICA",
                table: "VENTA_TIPO");

            migrationBuilder.DropForeignKey(
                name: "FK_VENTA_TIPO_ID_TIPO_FACTURA",
                schema: "LOGISTICA",
                table: "VENTA_TIPO");

            migrationBuilder.DropIndex(
                name: "I_VENTA_TIPO_ID_INTERVENTOR",
                schema: "LOGISTICA",
                table: "VENTA_TIPO");

            migrationBuilder.DropIndex(
                name: "I_VENTA_TIPO_ID_TIPO_FACTURA",
                schema: "LOGISTICA",
                table: "VENTA_TIPO");

            migrationBuilder.DropColumn(
                name: "ID_INTERVENTOR",
                schema: "LOGISTICA",
                table: "VENTA_TIPO");

            migrationBuilder.DropColumn(
                name: "ID_TIPO_FACTURA",
                schema: "LOGISTICA",
                table: "VENTA_TIPO");
        }
    }
}
