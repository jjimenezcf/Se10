using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class auditoriaenrecepcionesdelineasdepedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FECCRE",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                type: "DATETIME2(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FECMOD",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                type: "DATETIME2(7)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ID_CREADOR",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                type: "INT",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ID_MODIFICADOR",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                type: "INT",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "I_PEDIDO_LINEA_RECEPCION_ID_CREADOR",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                column: "ID_CREADOR");

            migrationBuilder.CreateIndex(
                name: "I_PEDIDO_LINEA_RECEPCION_ID_MODIFICADOR",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                column: "ID_MODIFICADOR");

            migrationBuilder.AddForeignKey(
                name: "FK_PEDIDO_LINEA_RECEPCION_ID_CREADOR",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                column: "ID_CREADOR",
                principalSchema: "ENTORNO",
                principalTable: "USUARIO",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PEDIDO_LINEA_RECEPCION_ID_MODIFICADOR",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION",
                column: "ID_MODIFICADOR",
                principalSchema: "ENTORNO",
                principalTable: "USUARIO",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PEDIDO_LINEA_RECEPCION_ID_CREADOR",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION");

            migrationBuilder.DropForeignKey(
                name: "FK_PEDIDO_LINEA_RECEPCION_ID_MODIFICADOR",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION");

            migrationBuilder.DropIndex(
                name: "I_PEDIDO_LINEA_RECEPCION_ID_CREADOR",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION");

            migrationBuilder.DropIndex(
                name: "I_PEDIDO_LINEA_RECEPCION_ID_MODIFICADOR",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION");

            migrationBuilder.DropColumn(
                name: "FECCRE",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION");

            migrationBuilder.DropColumn(
                name: "FECMOD",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION");

            migrationBuilder.DropColumn(
                name: "ID_CREADOR",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION");

            migrationBuilder.DropColumn(
                name: "ID_MODIFICADOR",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "LOGISTICA",
                table: "PEDIDO_LINEA_RECEPCION");
        }
    }
}
