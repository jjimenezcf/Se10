using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;

namespace ServicioDeDatos.TrabajosSometidos
{
    [Table(Tablas.TRABAJO, Schema = Esquemas.TRABAJO)]
    public class TrabajoSometidoDtm : RegistroConNombreDtm
    {

        public bool EsDll { get; set; }
        public string Dll { get; set; }
        public string Clase { get; set; }

        public string Metodo { get; set; }

        public string Esquema { get; set; }

        public string Pa { get; set; }

        public bool ComunicarFin { get; set; }

        public bool ComunicarError { get; set; }
        public int? IdEjecutor { get; set; }
        public virtual UsuarioDtm Ejecutor { get; set; }

        public int? IdInformarA { get; set; }

        public virtual PuestoDtm InformarA { get; set; }
    }


    public static class TablaTrabajo
    {
        public static string Tabla => ApiDeRegistroDtm.EsquemaTabla(typeof(TrabajoSometidoDtm));
        public static void Definir(ModelBuilder modelBuilder)
        {

            ApiDeNombreDtm.DefinirCampoNombreDtm<TrabajoSometidoDtm>(modelBuilder);
            modelBuilder.Entity<TrabajoSometidoDtm>().Property(p => p.EsDll).HasColumnName(ICampos.ES_DLL).HasColumnType(IDominio.BIT);
            modelBuilder.Entity<TrabajoSometidoDtm>().Property(p => p.Dll).HasColumnName(ICampos.DLL).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<TrabajoSometidoDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<TrabajoSometidoDtm>().Property(p => p.Metodo).HasColumnName(ICampos.METODO).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<TrabajoSometidoDtm>().Property(p => p.Esquema).HasColumnName(ICampos.ESQUEMA).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<TrabajoSometidoDtm>().Property(p => p.Pa).HasColumnName(ICampos.PA).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);

            modelBuilder.Entity<TrabajoSometidoDtm>().Property(p => p.IdEjecutor).HasColumnName(ICampos.ID_EJECUTOR).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<TrabajoSometidoDtm>().Property(p => p.IdInformarA).HasColumnName(ICampos.ID_INFORMAR_A).HasColumnType(IDominio.INT).IsRequired(false);

            modelBuilder.Entity<TrabajoSometidoDtm>().Property(p => p.ComunicarFin).HasColumnName(ICampos.COMUNICAR_FIN).HasColumnType(IDominio.BIT).IsRequired();
            modelBuilder.Entity<TrabajoSometidoDtm>().Property(p => p.ComunicarError).HasColumnName(ICampos.COMUNICAR_ERROR).HasColumnType(IDominio.BIT).IsRequired();

            modelBuilder.Entity<TrabajoSometidoDtm>().HasOne(x => x.Ejecutor).WithMany().HasForeignKey(x => x.IdEjecutor).HasConstraintName("FK_TRABAJO_ID_EJECUTOR").OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TrabajoSometidoDtm>().HasOne(x => x.InformarA).WithMany().HasForeignKey(x => x.IdInformarA).HasConstraintName("FK_TRABAJO_ID_INFORMAR_A").OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TrabajoSometidoDtm>().HasIndex(x => new { x.Nombre, x.Dll, x.Clase, x.Metodo }).IsUnique(true).HasDatabaseName("IX_TRABAJO_METODO");
            modelBuilder.Entity<TrabajoSometidoDtm>().HasIndex(x => new { x.Nombre, x.Esquema, x.Pa }).IsUnique(true).HasDatabaseName("IX_TRABAJO_PA");
        }
    }

}
