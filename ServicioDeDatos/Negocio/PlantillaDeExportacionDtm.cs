using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;

namespace ServicioDeDatos.Negocio
{

    [Table(Tablas.PLANTILLA_EXPORTACION , Schema = Esquemas.NEGOCIO)]
    public class PlantillaDeExportacionDtm : RegistroConNombreDtm, IRegistroDeParametrizacion
    {
        public int IdNegocio { get; set; }
        public virtual NegocioDtm Negocio { get; set; }
        public string Dll { get; set; }
        public string Clase { get; set; }
        public string Metodo { get; set; }
        public int IdPermiso { get; set; }
        public PermisoDtm Permiso { get; }

    }

    public static partial class ModeloDeNegocio
    {
        public static void PlantillasDeExportacion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlantillaDeExportacionDtm>().Property(p => p.Nombre).HasColumnName(ICampos.NOMBRE).HasColumnType(IDominio.VARCHAR_250).IsRequired();

            ApiDeRegistroDtm.DefinirCampoFk<PlantillaDeExportacionDtm>(modelBuilder, nameof(PlantillaDeExportacionDtm.Negocio), nameof(PlantillaDeExportacionDtm.IdNegocio), ICampos.ID_NEGOCIO, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<PlantillaDeExportacionDtm>(modelBuilder, nameof(PlantillaDeExportacionDtm.Permiso), nameof(PlantillaDeExportacionDtm.IdPermiso), ICampos.ID_PERMISO, requerida: true, unico: true);
            modelBuilder.Entity<PlantillaDeExportacionDtm>().Property(p => p.Dll).HasColumnName(ICampos.DLL).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<PlantillaDeExportacionDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<PlantillaDeExportacionDtm>().Property(p => p.Metodo).HasColumnName(ICampos.METODO).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);

            modelBuilder.Entity<PlantillaDeExportacionDtm>()
               .HasIndex(p => new {p.Dll, p.Clase, p.Metodo })
               .IsUnique(true)
               .HasDatabaseName($"I_{Tablas.PLANTILLA_EXPORTACION}_{ICampos.DLL}_{ICampos.CLASE}_{ICampos.METODO}");
        }
    }

}
