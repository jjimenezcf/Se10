
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ServicioDeDatos.Entorno
{
    [Table(Tablas.AGENDA, Schema = Esquemas.ENTORNO)]
    public class AgendaDtm : RegistroConNombreDtm
    {
        public int IdConsultor { get; set; }
        public int IdGestor { get; set; }
        public int IdInterventor { get; set; }
        public PermisoDtm Consultor { get; }
        public PermisoDtm Gestor { get; }
        public PermisoDtm Interventor { get; }

        public string Uri { get; set; }
        public string Ics { get; set; }
        public string RutaIcs => Path.Combine(enumRutas.RutaDeAgendas, Ics + ".ics");

        public string NombreIcs(int longitud)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new string(Enumerable.Repeat(chars, longitud)
              .Select(s => s[random.Next(s.Length)]).ToArray());
            return result;
        }
    }

    public static partial class ModeloDeEntorno
    {
        internal static string TablaAgenda => $"{ApiDeRegistroDtm.EsquemaDeTabla(typeof(AgendaDtm))}.{ApiDeRegistroDtm.NombreDeTabla(typeof(AgendaDtm))}";

        public static void Agendas(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AgendaDtm>().Ignore(p => p.RutaIcs);

            ApiDeNombreDtm.DefinirCampoNombreDtm<AgendaDtm>(modelBuilder, unico: true);
            ApiDeRegistroDtm.DefinirCampoFk<AgendaDtm>(modelBuilder, nameof(AgendaDtm.Consultor), nameof(AgendaDtm.IdConsultor), ICampos.ID_CONSULTOR, requerida: true, unico: true);
            ApiDeRegistroDtm.DefinirCampoFk<AgendaDtm>(modelBuilder, nameof(AgendaDtm.Gestor), nameof(AgendaDtm.IdGestor), ICampos.ID_GESTOR, requerida: true, unico: true);
            ApiDeRegistroDtm.DefinirCampoFk<AgendaDtm>(modelBuilder, nameof(AgendaDtm.Interventor), nameof(AgendaDtm.IdInterventor), ICampos.ID_INTERVENTOR, requerida: true, unico: true);
            modelBuilder.Entity<AgendaDtm>().Property(p => p.Uri).HasColumnName(ICampos.URL).HasColumnType(IDominio.URL).IsRequired(false);
            modelBuilder.Entity<AgendaDtm>().Property(p => p.Ics).HasColumnName(ICampos.ICS).HasColumnType(IDominio.VARCHAR_255).IsRequired(false);
        }


        public static void CrearPermisosDeIntervencion(this MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                   declare @nombre varchar(50) = 'AGENDA (Interventor): '
                   insert into SEGURIDAD.PERMISO (NOMBRE, IDCLASE, IDTIPO)
                   select @nombre + t1.NOMBRE, 1, 5
                   from entorno.AGENDA t1
                   where not exists (
                     select 1
                     from SEGURIDAD.PERMISO
                     where nombre like @nombre + t1.NOMBRE
                   )
                   go
                ");
        }

        public static void AsociarPermisosDeIntervencion(this MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                   UPDATE
                      Table_A
                   SET
                       Table_A.ID_INTERVENTOR = Table_B.ID
                   FROM
                       entorno.AGENDA AS Table_A
                       INNER JOIN SEGURIDAD.PERMISO AS Table_B ON 'AGENDA (Interventor): ' + Table_A.NOMBRE = Table_B.NOMBRE
                   go
                ");
        }
    }
}
