using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class regularizacióndealmacen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "REGULARIZACION_AUDITORIA",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false),
                    ID_USUARIO = table.Column<int>(type: "INT", nullable: false),
                    OPERACION = table.Column<string>(type: "CHAR(1)", nullable: false),
                    REGISTRO = table.Column<string>(type: "VARCHAR(MAX)", nullable: false),
                    AUDITADO_EL = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGULARIZACION_AUDITORIA", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "REGULARIZACION_ESTADO",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOMBRE = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    ID_PERMISO = table.Column<int>(type: "INT", nullable: false),
                    INICIAL = table.Column<bool>(type: "BIT", nullable: false),
                    TERMINADO = table.Column<bool>(type: "BIT", nullable: false),
                    CANCELADO = table.Column<bool>(type: "BIT", nullable: false),
                    ORDEN = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGULARIZACION_ESTADO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_ESTADO_ID_PERMISO",
                        column: x => x.ID_PERMISO,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "REGULARIZACION_TIPO",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CLASE_REGULARIZACION = table.Column<string>(type: "VARCHAR(20)", nullable: false),
                    NOMBRE = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    ID_PADRE = table.Column<int>(type: "INT", nullable: true),
                    ID_GESTOR = table.Column<int>(type: "INT", nullable: false),
                    ID_CONSULTOR = table.Column<int>(type: "INT", nullable: false),
                    ID_ADM = table.Column<int>(type: "INT", nullable: false),
                    ACTIVO = table.Column<bool>(type: "BIT", nullable: false),
                    TIPO_DTM = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    TIPO_DTO = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    CLASE_DE_LIBRO = table.Column<string>(type: "VARCHAR(20)", nullable: false),
                    SIGLA = table.Column<string>(type: "VARCHAR(5)", nullable: false),
                    MASCARA = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    MARCADOR = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    NOMBRE_MODIFICABLE = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true),
                    PERMITE_CREAR = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true),
                    EDITAR_TRAS_CREAR = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true),
                    ID_ESTADO = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGULARIZACION_TIPO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_TIPO_ID_ADM",
                        column: x => x.ID_ADM,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_TIPO_ID_CONSULTOR",
                        column: x => x.ID_CONSULTOR,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_TIPO_ID_ESTADO",
                        column: x => x.ID_ESTADO,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION_ESTADO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_TIPO_ID_GESTOR",
                        column: x => x.ID_GESTOR,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_TIPO_ID_PADRE",
                        column: x => x.ID_PADRE,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION_TIPO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "REGULARIZACION_TRANSICION",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOMBRE = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    ID_ORIGEN = table.Column<int>(type: "INT", nullable: false),
                    ID_DESTINO = table.Column<int>(type: "INT", nullable: false),
                    DEL_SISTEMA = table.Column<bool>(type: "BIT", nullable: false),
                    CON_OBSERVACION = table.Column<bool>(type: "BIT", nullable: false),
                    POR_DEFECCTO = table.Column<bool>(type: "BIT", nullable: false, defaultValue: false),
                    ASUNTO = table.Column<string>(type: "VARCHAR(250)", nullable: true),
                    ID_PERMISO = table.Column<int>(type: "INT", nullable: false),
                    ACTIVO = table.Column<bool>(type: "BIT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGULARIZACION_TRANSICION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_TRANSICION_ID_DESTINO",
                        column: x => x.ID_DESTINO,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION_ESTADO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_TRANSICION_ID_ORIGEN",
                        column: x => x.ID_ORIGEN,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION_ESTADO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_TRANSICION_ID_PERMISO",
                        column: x => x.ID_PERMISO,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "REGULARIZACION",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ALMACEN = table.Column<int>(type: "INT", nullable: false),
                    NOMBRE = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    FECCRE = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    ID_CREADOR = table.Column<int>(type: "INT", nullable: false),
                    FECMOD = table.Column<DateTime>(type: "DATETIME2(7)", nullable: true),
                    ID_MODIFICADOR = table.Column<int>(type: "INT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    ID_CG = table.Column<int>(type: "INT", nullable: false),
                    ID_TIPO = table.Column<int>(type: "INT", nullable: false),
                    DESCRIPCION = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true),
                    REFERENCIA = table.Column<string>(type: "VARCHAR(20)", maxLength: 20, nullable: false),
                    ID_ESTADO = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGULARIZACION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_ID_ALMACEN",
                        column: x => x.ID_ALMACEN,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_ID_CG",
                        column: x => x.ID_CG,
                        principalSchema: "TERCEROS",
                        principalTable: "CENTRO_GESTOR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_ID_CREADOR",
                        column: x => x.ID_CREADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_ID_ESTADO",
                        column: x => x.ID_ESTADO,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION_ESTADO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_ID_MODIFICADOR",
                        column: x => x.ID_MODIFICADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_ID_TIPO",
                        column: x => x.ID_TIPO,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION_TIPO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "REGULARIZACION_ACCION",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_TRANSICION = table.Column<int>(type: "INT", nullable: false),
                    ID_ACCION = table.Column<int>(type: "INT", nullable: false),
                    PARAMETROS = table.Column<string>(type: "VARCHAR(2000)", nullable: true),
                    DESCRIPCION = table.Column<string>(type: "VARCHAR(2000)", nullable: true),
                    MOMENTO = table.Column<string>(type: "VARCHAR(1)", nullable: false),
                    ORDEN = table.Column<int>(type: "INT", nullable: false),
                    ACTIVO = table.Column<bool>(type: "BIT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGULARIZACION_ACCION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_ACCION_ID_ACCION",
                        column: x => x.ID_ACCION,
                        principalSchema: "ENTORNO",
                        principalTable: "ACCION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_ACCION_ID_TRANSICION",
                        column: x => x.ID_TRANSICION,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION_TRANSICION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "REGULARIZACION_ARCHIVADOR",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ELEMENTO1 = table.Column<int>(type: "INT", nullable: false),
                    ID_ELEMENTO2 = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGULARIZACION_ARCHIVADOR", x => x.ID);
                    table.UniqueConstraint("AK_REGULARIZACION_ARCHIVADOR", x => new { x.ID_ELEMENTO1, x.ID_ELEMENTO2 });
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_ARCHIVADOR_ID_ELEMENTO1",
                        column: x => x.ID_ELEMENTO1,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_ARCHIVADOR_ID_ELEMENTO2",
                        column: x => x.ID_ELEMENTO2,
                        principalSchema: "SISDOC",
                        principalTable: "ARCHIVADOR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "REGULARIZACION_ARCHIVO",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ELEMENTO1 = table.Column<int>(type: "INT", nullable: false),
                    ID_ELEMENTO2 = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGULARIZACION_ARCHIVO", x => x.ID);
                    table.UniqueConstraint("AK_REGULARIZACION_ARCHIVO", x => new { x.ID_ELEMENTO1, x.ID_ELEMENTO2 });
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_ARCHIVO_ID_ELEMENTO1",
                        column: x => x.ID_ELEMENTO1,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_ARCHIVO_ID_ELEMENTO2",
                        column: x => x.ID_ELEMENTO2,
                        principalSchema: "SISDOC",
                        principalTable: "ARCHIVO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(@"
            CREATE FUNCTION [LOGISTICA].[CC_REGULARIZACION_NOMBRE] (@id_elemento int)
            RETURNS VarChar(250)
            AS
            begin
              declare @resultado VARCHAR(250)

              select @resultado = NOMBRE from LOGISTICA.REGULARIZACION where id = @id_elemento
              return @resultado
            END
            ");

            migrationBuilder.CreateTable(
                name: "REGULARIZACION_OBSERVACION",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOMBRE = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false),
                    DESCRIPCION = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true),
                    ID_CREADOR = table.Column<int>(type: "INT", nullable: false),
                    CREADO_EL = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    CREADOR = table.Column<string>(type: "VARCHAR(255)", nullable: true, computedColumnSql: "ENTORNO.CC_USUARIO_EXPRESION(ID_CREADOR)"),
                    ELEMENTO = table.Column<string>(type: "VARCHAR(255)", nullable: true, computedColumnSql: "LOGISTICA.CC_REGULARIZACION_NOMBRE(ID_ELEMENTO)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGULARIZACION_OBSERVACION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_OBSERVACION_ID_CREADOR",
                        column: x => x.ID_CREADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_OBSERVACION_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "REGULARIZACION_PERMISO",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false),
                    ID_GESTOR = table.Column<int>(type: "INT", nullable: false),
                    ID_CONSULTOR = table.Column<int>(type: "INT", nullable: false),
                    ID_ADM = table.Column<int>(type: "INT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGULARIZACION_PERMISO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_PERMISO_ID_ADM",
                        column: x => x.ID_ADM,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_PERMISO_ID_CONSULTOR",
                        column: x => x.ID_CONSULTOR,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_PERMISO_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_PERMISO_ID_GESTOR",
                        column: x => x.ID_GESTOR,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "REGULARIZACION_TRAZA",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NOMBRE = table.Column<string>(type: "VARCHAR(250)", nullable: false),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false),
                    DESCRIPCION = table.Column<string>(type: "VARCHAR(2000)", maxLength: 2000, nullable: true),
                    ID_CREADOR = table.Column<int>(type: "INT", nullable: false),
                    CREADO_EL = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    CREADOR = table.Column<string>(type: "VARCHAR(255)", nullable: true, computedColumnSql: "ENTORNO.CC_USUARIO_EXPRESION(ID_CREADOR)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGULARIZACION_TRAZA", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_TRAZA_ID_CREADOR",
                        column: x => x.ID_CREADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_TRAZA_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "REGULARIZACION_HISTORIA",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false),
                    FECHA = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    ID_USUARIO = table.Column<int>(type: "INT", nullable: false),
                    ID_ESTADO = table.Column<int>(type: "INT", nullable: false),
                    TIEMPO = table.Column<long>(type: "BIGINT", nullable: true),
                    ID_TRANSICION = table.Column<int>(type: "INT", nullable: true),
                    ID_OBSERVACION = table.Column<int>(type: "INT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGULARIZACION_HISTORIA", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_HISTORIA_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_HISTORIA_ID_ESTADO",
                        column: x => x.ID_ESTADO,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION_ESTADO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_HISTORIA_ID_OBSERVACION",
                        column: x => x.ID_OBSERVACION,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION_OBSERVACION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_HISTORIA_ID_TRANSICION",
                        column: x => x.ID_TRANSICION,
                        principalSchema: "LOGISTICA",
                        principalTable: "REGULARIZACION_TRANSICION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_REGULARIZACION_HISTORIA_ID_USUARIO",
                        column: x => x.ID_USUARIO,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_ID_ALMACEN",
                schema: "LOGISTICA",
                table: "REGULARIZACION",
                column: "ID_ALMACEN");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_ID_CG",
                schema: "LOGISTICA",
                table: "REGULARIZACION",
                column: "ID_CG");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_ID_CREADOR",
                schema: "LOGISTICA",
                table: "REGULARIZACION",
                column: "ID_CREADOR");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_ID_ESTADO",
                schema: "LOGISTICA",
                table: "REGULARIZACION",
                column: "ID_ESTADO");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_ID_MODIFICADOR",
                schema: "LOGISTICA",
                table: "REGULARIZACION",
                column: "ID_MODIFICADOR");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_ID_TIPO",
                schema: "LOGISTICA",
                table: "REGULARIZACION",
                column: "ID_TIPO");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_NOMBRE",
                schema: "LOGISTICA",
                table: "REGULARIZACION",
                column: "NOMBRE");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_REFERENCIA",
                schema: "LOGISTICA",
                table: "REGULARIZACION",
                column: "REFERENCIA",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ACCION_POR_ORDEN",
                schema: "LOGISTICA",
                table: "REGULARIZACION_ACCION",
                columns: new[] { "ID_TRANSICION", "MOMENTO", "ORDEN" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_ACCION_ID_ACCION",
                schema: "LOGISTICA",
                table: "REGULARIZACION_ACCION",
                column: "ID_ACCION");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_ACCION_ID_TRANSICION",
                schema: "LOGISTICA",
                table: "REGULARIZACION_ACCION",
                column: "ID_TRANSICION");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_ARCHIVADOR_ID_ELEMENTO1",
                schema: "LOGISTICA",
                table: "REGULARIZACION_ARCHIVADOR",
                column: "ID_ELEMENTO1");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_ARCHIVADOR_ID_ELEMENTO2",
                schema: "LOGISTICA",
                table: "REGULARIZACION_ARCHIVADOR",
                column: "ID_ELEMENTO2");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_ARCHIVO_ID_ELEMENTO1",
                schema: "LOGISTICA",
                table: "REGULARIZACION_ARCHIVO",
                column: "ID_ELEMENTO1");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_ARCHIVO_ID_ELEMENTO2",
                schema: "LOGISTICA",
                table: "REGULARIZACION_ARCHIVO",
                column: "ID_ELEMENTO2");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_AUDITORIA_ID_ELEMENTO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_AUDITORIA",
                column: "ID_ELEMENTO");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_AUDITORIA_ID_USUARIO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_AUDITORIA",
                column: "ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_ESTADO_ID_PERMISO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_ESTADO",
                column: "ID_PERMISO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_ESTADO_NOMBRE",
                schema: "LOGISTICA",
                table: "REGULARIZACION_ESTADO",
                column: "NOMBRE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_HISTORIA_ID_ELEMENTO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_HISTORIA",
                column: "ID_ELEMENTO");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_HISTORIA_ID_ESTADO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_HISTORIA",
                column: "ID_ESTADO");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_HISTORIA_ID_OBSERVACION",
                schema: "LOGISTICA",
                table: "REGULARIZACION_HISTORIA",
                column: "ID_OBSERVACION");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_HISTORIA_ID_TRANSICION",
                schema: "LOGISTICA",
                table: "REGULARIZACION_HISTORIA",
                column: "ID_TRANSICION");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_HISTORIA_ID_USUARIO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_HISTORIA",
                column: "ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_OBSERVACION_ID_CREADOR",
                schema: "LOGISTICA",
                table: "REGULARIZACION_OBSERVACION",
                column: "ID_CREADOR");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_OBSERVACION_ID_ELEMENTO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_OBSERVACION",
                column: "ID_ELEMENTO");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_PERMISO_ID_ADM",
                schema: "LOGISTICA",
                table: "REGULARIZACION_PERMISO",
                column: "ID_ADM",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_PERMISO_ID_CONSULTOR",
                schema: "LOGISTICA",
                table: "REGULARIZACION_PERMISO",
                column: "ID_CONSULTOR",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_PERMISO_ID_ELEMENTO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_PERMISO",
                column: "ID_ELEMENTO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_PERMISO_ID_GESTOR",
                schema: "LOGISTICA",
                table: "REGULARIZACION_PERMISO",
                column: "ID_GESTOR",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_TIPO_ID_ADM",
                schema: "LOGISTICA",
                table: "REGULARIZACION_TIPO",
                column: "ID_ADM",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_TIPO_ID_CONSULTOR",
                schema: "LOGISTICA",
                table: "REGULARIZACION_TIPO",
                column: "ID_CONSULTOR",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_TIPO_ID_ESTADO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_TIPO",
                column: "ID_ESTADO");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_TIPO_ID_GESTOR",
                schema: "LOGISTICA",
                table: "REGULARIZACION_TIPO",
                column: "ID_GESTOR",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_TIPO_ID_PADRE",
                schema: "LOGISTICA",
                table: "REGULARIZACION_TIPO",
                column: "ID_PADRE");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_TIPO_NOMBRE",
                schema: "LOGISTICA",
                table: "REGULARIZACION_TIPO",
                column: "NOMBRE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_TRANSICION_ID_DESTINO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_TRANSICION",
                column: "ID_DESTINO");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_TRANSICION_ID_ORIGEN",
                schema: "LOGISTICA",
                table: "REGULARIZACION_TRANSICION",
                column: "ID_ORIGEN");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_TRANSICION_ID_PERMISO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_TRANSICION",
                column: "ID_PERMISO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_TRANSICION_NOMBRE",
                schema: "LOGISTICA",
                table: "REGULARIZACION_TRANSICION",
                column: "NOMBRE");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_TRANSICION_NOMBRE_ID_ORIGEN_ID_DESTINO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_TRANSICION",
                columns: new[] { "NOMBRE", "ID_ORIGEN", "ID_DESTINO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_TRAZA_ID_CREADOR",
                schema: "LOGISTICA",
                table: "REGULARIZACION_TRAZA",
                column: "ID_CREADOR");

            migrationBuilder.CreateIndex(
                name: "I_REGULARIZACION_TRAZA_ID_ELEMENTO",
                schema: "LOGISTICA",
                table: "REGULARIZACION_TRAZA",
                column: "ID_ELEMENTO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "REGULARIZACION_ACCION",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "REGULARIZACION_ARCHIVADOR",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "REGULARIZACION_ARCHIVO",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "REGULARIZACION_AUDITORIA",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "REGULARIZACION_HISTORIA",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "REGULARIZACION_PERMISO",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "REGULARIZACION_TRAZA",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "REGULARIZACION_OBSERVACION",
                schema: "LOGISTICA");

            migrationBuilder.Sql("DROP FUNCTION [LOGISTICA].[CC_REGULARIZACION_NOMBRE];");

            migrationBuilder.DropTable(
                name: "REGULARIZACION_TRANSICION",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "REGULARIZACION",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "REGULARIZACION_TIPO",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "REGULARIZACION_ESTADO",
                schema: "LOGISTICA");
        }
    }
}
