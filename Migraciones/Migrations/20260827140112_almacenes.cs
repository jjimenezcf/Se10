using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migraciones.Migrations
{
    /// <inheritdoc />
    public partial class almacenes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ALMACEN_AUDITORIA",
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
                    table.PrimaryKey("PK_ALMACEN_AUDITORIA", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ALMACEN_ESTADO",
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
                    table.PrimaryKey("PK_ALMACEN_ESTADO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ALMACEN_ESTADO_ID_PERMISO",
                        column: x => x.ID_PERMISO,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ALMACEN_TIPO",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_INTERVENTOR = table.Column<int>(type: "INT", nullable: false),
                    CALCULO = table.Column<string>(type: "VARCHAR(20)", nullable: false),
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
                    table.PrimaryKey("PK_ALMACEN_TIPO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ALMACEN_TIPO_ID_ADM",
                        column: x => x.ID_ADM,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_TIPO_ID_CONSULTOR",
                        column: x => x.ID_CONSULTOR,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_TIPO_ID_ESTADO",
                        column: x => x.ID_ESTADO,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN_ESTADO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_TIPO_ID_GESTOR",
                        column: x => x.ID_GESTOR,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_TIPO_ID_INTERVENTOR",
                        column: x => x.ID_INTERVENTOR,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_TIPO_ID_PADRE",
                        column: x => x.ID_PADRE,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN_TIPO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ALMACEN_TRANSICION",
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
                    table.PrimaryKey("PK_ALMACEN_TRANSICION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ALMACEN_TRANSICION_ID_DESTINO",
                        column: x => x.ID_DESTINO,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN_ESTADO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_TRANSICION_ID_ORIGEN",
                        column: x => x.ID_ORIGEN,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN_ESTADO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_TRANSICION_ID_PERMISO",
                        column: x => x.ID_PERMISO,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ALMACEN",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    table.PrimaryKey("PK_ALMACEN", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ALMACEN_ID_CG",
                        column: x => x.ID_CG,
                        principalSchema: "TERCEROS",
                        principalTable: "CENTRO_GESTOR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_ID_CREADOR",
                        column: x => x.ID_CREADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_ID_ESTADO",
                        column: x => x.ID_ESTADO,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN_ESTADO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_ID_MODIFICADOR",
                        column: x => x.ID_MODIFICADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_ID_TIPO",
                        column: x => x.ID_TIPO,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN_TIPO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ALMACEN_ACCION",
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
                    table.PrimaryKey("PK_ALMACEN_ACCION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ALMACEN_ACCION_ID_ACCION",
                        column: x => x.ID_ACCION,
                        principalSchema: "ENTORNO",
                        principalTable: "ACCION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_ACCION_ID_TRANSICION",
                        column: x => x.ID_TRANSICION,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN_TRANSICION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ALMACEN_ARCHIVADOR",
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
                    table.PrimaryKey("PK_ALMACEN_ARCHIVADOR", x => x.ID);
                    table.UniqueConstraint("AK_ALMACEN_ARCHIVADOR", x => new { x.ID_ELEMENTO1, x.ID_ELEMENTO2 });
                    table.ForeignKey(
                        name: "FK_ALMACEN_ARCHIVADOR_ID_ELEMENTO1",
                        column: x => x.ID_ELEMENTO1,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_ARCHIVADOR_ID_ELEMENTO2",
                        column: x => x.ID_ELEMENTO2,
                        principalSchema: "SISDOC",
                        principalTable: "ARCHIVADOR",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ALMACEN_ARCHIVO",
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
                    table.PrimaryKey("PK_ALMACEN_ARCHIVO", x => x.ID);
                    table.UniqueConstraint("AK_ALMACEN_ARCHIVO", x => new { x.ID_ELEMENTO1, x.ID_ELEMENTO2 });
                    table.ForeignKey(
                        name: "FK_ALMACEN_ARCHIVO_ID_ELEMENTO1",
                        column: x => x.ID_ELEMENTO1,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_ARCHIVO_ID_ELEMENTO2",
                        column: x => x.ID_ELEMENTO2,
                        principalSchema: "SISDOC",
                        principalTable: "ARCHIVO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ALMACEN_DIRECCION",
                schema: "LOGISTICA",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID_ELEMENTO = table.Column<int>(type: "INT", nullable: false),
                    CALIFICADOR = table.Column<string>(type: "VARCHAR(20)", nullable: false),
                    ID_PAIS = table.Column<int>(type: "INT", nullable: false),
                    ID_PROVINCIA = table.Column<int>(type: "INT", nullable: false),
                    ID_MUNICIPIO = table.Column<int>(type: "INT", nullable: false),
                    ID_CALLE = table.Column<int>(type: "INT", nullable: false),
                    ID_ZONA = table.Column<int>(type: "INT", nullable: true),
                    ID_BARRIO = table.Column<int>(type: "INT", nullable: true),
                    ID_CP = table.Column<int>(type: "INT", nullable: true),
                    NUMERO = table.Column<int>(type: "INT", nullable: true),
                    ESCALERA = table.Column<string>(type: "VARCHAR(4)", nullable: true),
                    PISO = table.Column<string>(type: "VARCHAR(4)", nullable: true),
                    PUERTA = table.Column<string>(type: "VARCHAR(15)", nullable: true),
                    OTROS = table.Column<string>(type: "VARCHAR(2000)", nullable: true),
                    URL = table.Column<string>(type: "VARCHAR(2000)", nullable: true),
                    ACTIVO = table.Column<bool>(type: "BIT", nullable: false),
                    ID_CREADOR = table.Column<int>(type: "INT", nullable: false),
                    CREADO_EL = table.Column<DateTime>(type: "DATETIME2(7)", nullable: false),
                    CREADOR = table.Column<string>(type: "VARCHAR(255)", nullable: true, computedColumnSql: "ENTORNO.CC_USUARIO_EXPRESION(ID_CREADOR)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ALMACEN_DIRECCION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ALMACEN_DIRECCION_ID_BARRIO",
                        column: x => x.ID_BARRIO,
                        principalSchema: "CALLEJERO",
                        principalTable: "BARRIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_DIRECCION_ID_CALLE",
                        column: x => x.ID_CALLE,
                        principalSchema: "CALLEJERO",
                        principalTable: "CALLE",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_DIRECCION_ID_CP",
                        column: x => x.ID_CP,
                        principalSchema: "CALLEJERO",
                        principalTable: "CODIGO_POSTAL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_DIRECCION_ID_CREADOR",
                        column: x => x.ID_CREADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_DIRECCION_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_DIRECCION_ID_MUNICIPIO",
                        column: x => x.ID_MUNICIPIO,
                        principalSchema: "CALLEJERO",
                        principalTable: "MUNICIPIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_DIRECCION_ID_PAIS",
                        column: x => x.ID_PAIS,
                        principalSchema: "CALLEJERO",
                        principalTable: "PAIS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_DIRECCION_ID_PROVINCIA",
                        column: x => x.ID_PROVINCIA,
                        principalSchema: "CALLEJERO",
                        principalTable: "PROVINCIA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_DIRECCION_ID_ZONA",
                        column: x => x.ID_ZONA,
                        principalSchema: "CALLEJERO",
                        principalTable: "ZONA",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(@"
CREATE FUNCTION [LOGISTICA].[CC_ALMACEN_NOMBRE] (@id_elemento int)
RETURNS VarChar(250)
AS
begin
  declare @resultado VARCHAR(250)

  select @resultado = NOMBRE from LOGISTICA.ALMACEN where id = @id_elemento
  return @resultado
END
");

            migrationBuilder.CreateTable(
                name: "ALMACEN_OBSERVACION",
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
                    ELEMENTO = table.Column<string>(type: "VARCHAR(255)", nullable: true, computedColumnSql: "LOGISTICA.CC_ALMACEN_NOMBRE(ID_ELEMENTO)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ALMACEN_OBSERVACION", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ALMACEN_OBSERVACION_ID_CREADOR",
                        column: x => x.ID_CREADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_OBSERVACION_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ALMACEN_PERMISO",
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
                    table.PrimaryKey("PK_ALMACEN_PERMISO", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ALMACEN_PERMISO_ID_ADM",
                        column: x => x.ID_ADM,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_PERMISO_ID_CONSULTOR",
                        column: x => x.ID_CONSULTOR,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_PERMISO_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_PERMISO_ID_GESTOR",
                        column: x => x.ID_GESTOR,
                        principalSchema: "SEGURIDAD",
                        principalTable: "PERMISO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ALMACEN_TRAZA",
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
                    table.PrimaryKey("PK_ALMACEN_TRAZA", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ALMACEN_TRAZA_ID_CREADOR",
                        column: x => x.ID_CREADOR,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_TRAZA_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ALMACEN_HISTORIA",
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
                    table.PrimaryKey("PK_ALMACEN_HISTORIA", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ALMACEN_HISTORIA_ID_ELEMENTO",
                        column: x => x.ID_ELEMENTO,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_HISTORIA_ID_ESTADO",
                        column: x => x.ID_ESTADO,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN_ESTADO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_HISTORIA_ID_OBSERVACION",
                        column: x => x.ID_OBSERVACION,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN_OBSERVACION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_HISTORIA_ID_TRANSICION",
                        column: x => x.ID_TRANSICION,
                        principalSchema: "LOGISTICA",
                        principalTable: "ALMACEN_TRANSICION",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ALMACEN_HISTORIA_ID_USUARIO",
                        column: x => x.ID_USUARIO,
                        principalSchema: "ENTORNO",
                        principalTable: "USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ID_CG",
                schema: "LOGISTICA",
                table: "ALMACEN",
                column: "ID_CG");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ID_CREADOR",
                schema: "LOGISTICA",
                table: "ALMACEN",
                column: "ID_CREADOR");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ID_ESTADO",
                schema: "LOGISTICA",
                table: "ALMACEN",
                column: "ID_ESTADO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ID_MODIFICADOR",
                schema: "LOGISTICA",
                table: "ALMACEN",
                column: "ID_MODIFICADOR");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ID_TIPO",
                schema: "LOGISTICA",
                table: "ALMACEN",
                column: "ID_TIPO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_NOMBRE",
                schema: "LOGISTICA",
                table: "ALMACEN",
                column: "NOMBRE");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_REFERENCIA",
                schema: "LOGISTICA",
                table: "ALMACEN",
                column: "REFERENCIA",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ACCION_POR_ORDEN",
                schema: "LOGISTICA",
                table: "ALMACEN_ACCION",
                columns: new[] { "ID_TRANSICION", "MOMENTO", "ORDEN" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ACCION_ID_ACCION",
                schema: "LOGISTICA",
                table: "ALMACEN_ACCION",
                column: "ID_ACCION");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ACCION_ID_TRANSICION",
                schema: "LOGISTICA",
                table: "ALMACEN_ACCION",
                column: "ID_TRANSICION");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ARCHIVADOR_ID_ELEMENTO1",
                schema: "LOGISTICA",
                table: "ALMACEN_ARCHIVADOR",
                column: "ID_ELEMENTO1");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ARCHIVADOR_ID_ELEMENTO2",
                schema: "LOGISTICA",
                table: "ALMACEN_ARCHIVADOR",
                column: "ID_ELEMENTO2");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ARCHIVO_ID_ELEMENTO1",
                schema: "LOGISTICA",
                table: "ALMACEN_ARCHIVO",
                column: "ID_ELEMENTO1");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ARCHIVO_ID_ELEMENTO2",
                schema: "LOGISTICA",
                table: "ALMACEN_ARCHIVO",
                column: "ID_ELEMENTO2");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_AUDITORIA_ID_ELEMENTO",
                schema: "LOGISTICA",
                table: "ALMACEN_AUDITORIA",
                column: "ID_ELEMENTO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_AUDITORIA_ID_USUARIO",
                schema: "LOGISTICA",
                table: "ALMACEN_AUDITORIA",
                column: "ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "AK_ALMACEN_DIRECCION",
                schema: "LOGISTICA",
                table: "ALMACEN_DIRECCION",
                columns: new[] { "CALIFICADOR", "ID_ELEMENTO", "ID_PAIS", "ID_PROVINCIA", "ID_MUNICIPIO", "ID_CALLE", "NUMERO", "PUERTA", "ESCALERA", "PISO" },
                unique: true,
                filter: "[NUMERO] IS NOT NULL AND [PUERTA] IS NOT NULL AND [ESCALERA] IS NOT NULL AND [PISO] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_DIRECCION_ID_BARRIO",
                schema: "LOGISTICA",
                table: "ALMACEN_DIRECCION",
                column: "ID_BARRIO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_DIRECCION_ID_CALLE",
                schema: "LOGISTICA",
                table: "ALMACEN_DIRECCION",
                column: "ID_CALLE");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_DIRECCION_ID_CP",
                schema: "LOGISTICA",
                table: "ALMACEN_DIRECCION",
                column: "ID_CP");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_DIRECCION_ID_CREADOR",
                schema: "LOGISTICA",
                table: "ALMACEN_DIRECCION",
                column: "ID_CREADOR");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_DIRECCION_ID_ELEMENTO",
                schema: "LOGISTICA",
                table: "ALMACEN_DIRECCION",
                column: "ID_ELEMENTO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_DIRECCION_ID_MUNICIPIO",
                schema: "LOGISTICA",
                table: "ALMACEN_DIRECCION",
                column: "ID_MUNICIPIO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_DIRECCION_ID_PAIS",
                schema: "LOGISTICA",
                table: "ALMACEN_DIRECCION",
                column: "ID_PAIS");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_DIRECCION_ID_PROVINCIA",
                schema: "LOGISTICA",
                table: "ALMACEN_DIRECCION",
                column: "ID_PROVINCIA");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_DIRECCION_ID_ZONA",
                schema: "LOGISTICA",
                table: "ALMACEN_DIRECCION",
                column: "ID_ZONA");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ESTADO_ID_PERMISO",
                schema: "LOGISTICA",
                table: "ALMACEN_ESTADO",
                column: "ID_PERMISO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_ESTADO_NOMBRE",
                schema: "LOGISTICA",
                table: "ALMACEN_ESTADO",
                column: "NOMBRE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_HISTORIA_ID_ELEMENTO",
                schema: "LOGISTICA",
                table: "ALMACEN_HISTORIA",
                column: "ID_ELEMENTO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_HISTORIA_ID_ESTADO",
                schema: "LOGISTICA",
                table: "ALMACEN_HISTORIA",
                column: "ID_ESTADO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_HISTORIA_ID_OBSERVACION",
                schema: "LOGISTICA",
                table: "ALMACEN_HISTORIA",
                column: "ID_OBSERVACION");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_HISTORIA_ID_TRANSICION",
                schema: "LOGISTICA",
                table: "ALMACEN_HISTORIA",
                column: "ID_TRANSICION");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_HISTORIA_ID_USUARIO",
                schema: "LOGISTICA",
                table: "ALMACEN_HISTORIA",
                column: "ID_USUARIO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_OBSERVACION_ID_CREADOR",
                schema: "LOGISTICA",
                table: "ALMACEN_OBSERVACION",
                column: "ID_CREADOR");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_OBSERVACION_ID_ELEMENTO",
                schema: "LOGISTICA",
                table: "ALMACEN_OBSERVACION",
                column: "ID_ELEMENTO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_PERMISO_ID_ADM",
                schema: "LOGISTICA",
                table: "ALMACEN_PERMISO",
                column: "ID_ADM",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_PERMISO_ID_CONSULTOR",
                schema: "LOGISTICA",
                table: "ALMACEN_PERMISO",
                column: "ID_CONSULTOR",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_PERMISO_ID_ELEMENTO",
                schema: "LOGISTICA",
                table: "ALMACEN_PERMISO",
                column: "ID_ELEMENTO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_PERMISO_ID_GESTOR",
                schema: "LOGISTICA",
                table: "ALMACEN_PERMISO",
                column: "ID_GESTOR",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_TIPO_ID_ADM",
                schema: "LOGISTICA",
                table: "ALMACEN_TIPO",
                column: "ID_ADM",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_TIPO_ID_CONSULTOR",
                schema: "LOGISTICA",
                table: "ALMACEN_TIPO",
                column: "ID_CONSULTOR",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_TIPO_ID_ESTADO",
                schema: "LOGISTICA",
                table: "ALMACEN_TIPO",
                column: "ID_ESTADO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_TIPO_ID_GESTOR",
                schema: "LOGISTICA",
                table: "ALMACEN_TIPO",
                column: "ID_GESTOR",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_TIPO_ID_INTERVENTOR",
                schema: "LOGISTICA",
                table: "ALMACEN_TIPO",
                column: "ID_INTERVENTOR",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_TIPO_ID_PADRE",
                schema: "LOGISTICA",
                table: "ALMACEN_TIPO",
                column: "ID_PADRE");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_TIPO_NOMBRE",
                schema: "LOGISTICA",
                table: "ALMACEN_TIPO",
                column: "NOMBRE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_TRANSICION_ID_DESTINO",
                schema: "LOGISTICA",
                table: "ALMACEN_TRANSICION",
                column: "ID_DESTINO");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_TRANSICION_ID_ORIGEN",
                schema: "LOGISTICA",
                table: "ALMACEN_TRANSICION",
                column: "ID_ORIGEN");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_TRANSICION_ID_PERMISO",
                schema: "LOGISTICA",
                table: "ALMACEN_TRANSICION",
                column: "ID_PERMISO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_TRANSICION_NOMBRE",
                schema: "LOGISTICA",
                table: "ALMACEN_TRANSICION",
                column: "NOMBRE");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_TRANSICION_NOMBRE_ID_ORIGEN_ID_DESTINO",
                schema: "LOGISTICA",
                table: "ALMACEN_TRANSICION",
                columns: new[] { "NOMBRE", "ID_ORIGEN", "ID_DESTINO" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_TRAZA_ID_CREADOR",
                schema: "LOGISTICA",
                table: "ALMACEN_TRAZA",
                column: "ID_CREADOR");

            migrationBuilder.CreateIndex(
                name: "I_ALMACEN_TRAZA_ID_ELEMENTO",
                schema: "LOGISTICA",
                table: "ALMACEN_TRAZA",
                column: "ID_ELEMENTO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ALMACEN_ACCION",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "ALMACEN_ARCHIVADOR",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "ALMACEN_ARCHIVO",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "ALMACEN_AUDITORIA",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "ALMACEN_DIRECCION",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "ALMACEN_HISTORIA",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "ALMACEN_PERMISO",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "ALMACEN_TRAZA",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "ALMACEN_OBSERVACION",
                schema: "LOGISTICA");

            migrationBuilder.Sql("DROP FUNCTION [LOGISTICA].[CC_ALMACEN_NOMBRE];");

            migrationBuilder.DropTable(
                name: "ALMACEN_TRANSICION",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "ALMACEN",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "ALMACEN_TIPO",
                schema: "LOGISTICA");

            migrationBuilder.DropTable(
                name: "ALMACEN_ESTADO",
                schema: "LOGISTICA");
        }
    }
}
