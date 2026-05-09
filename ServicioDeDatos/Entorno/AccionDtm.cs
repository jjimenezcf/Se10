using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Entorno
{
    public enum enumClaseDeAccion
    {
        [Description("Programa")]
        DLL,
        [Description("Bloque Sql")]
        SQL,
        [Description("Procedimiento almacenado")]
        PA
    }


    [Table(Tablas.ACCION, Schema = Esquemas.ENTORNO)]
    public class AccionDtm : RegistroConNombreDtm
    {
        public string ClaseDeAccion { get; set; }
        public string Dll { get; set; }
        public string Clase { get; set; }
        public string Metodo { get; set; }

        public string Esquema { get; set; }
        public string Pa { get; set; }
        public string Sql { get; set; }

        public string Descripcion { get; set; }

    }

    public static partial class ModeloDeEntorno
    {
        internal static string TablaAccion => $"{ApiDeRegistroDtm.EsquemaDeTabla(typeof(AccionDtm))}.{ApiDeRegistroDtm.NombreDeTabla(typeof(AccionDtm))}";

        public static void Acciones(ModelBuilder modelBuilder)
        {
            ApiDeNombreDtm.DefinirCampoNombreDtm<AccionDtm>(modelBuilder);

            modelBuilder.Entity<AccionDtm>().Property(p => p.ClaseDeAccion).HasColumnName(ICampos.CLASE_DE_ACCION).HasColumnType(IDominio.VARCHAR_4).IsRequired();

            modelBuilder.Entity<AccionDtm>().Property(p => p.Dll).HasColumnName(ICampos.DLL).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<AccionDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<AccionDtm>().Property(p => p.Metodo).HasColumnName(ICampos.METODO).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<AccionDtm>().Property(p => p.Esquema).HasColumnName(ICampos.ESQUEMA).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<AccionDtm>().Property(p => p.Pa).HasColumnName(ICampos.PA).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<AccionDtm>().Property(p => p.Sql).HasColumnName(ICampos.SQL).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);
            modelBuilder.Entity<AccionDtm>().Property(p => p.Descripcion).HasColumnName(ICampos.DESCRIPCION).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);

            modelBuilder.Entity<AccionDtm>().HasIndex(x => new { x.Dll, x.Clase, x.Metodo, x.Esquema, x.Pa, x.Sql }).HasDatabaseName($"I_{Tablas.ACCION}_POR_ACCION").IsUnique();           
        }
    }


}
