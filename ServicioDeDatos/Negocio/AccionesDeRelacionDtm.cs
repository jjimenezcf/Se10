using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;

namespace ServicioDeDatos.Negocio
{
    public enum enumMomentoDeRelacion
    {
        [Description("Antes de relacionar")]
        AR,
        [Description("Después de relacionar")]
        DR,
        [Description("Antes de eliminar relacionar")]
        AB,
        [Description("Después de eliminar relacionar")]
        DB
    }

    [Table(Tablas.ACCION_RELACION, Schema = Esquemas.NEGOCIO)]
    public class AccionesDeRelacionDtm : RegistroDtm, IAccionDeRelacion
    {
        public int IdAccion { get; set; }
        public int IdNegocio { get; set; }
        public int IdVinculado { get; set; }
        public string Parametros { get; set; }
        public string Descripcion { get; set; }
        public string Momento { get; set; }
        public AccionDtm Accion { get; }
        public NegocioDtm Negocio { get; }
        public NegocioDtm Vinculado { get; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
    }

    public static partial class ModeloDeNegocio
    {

        internal static void AccionesDeRelacion(ModelBuilder modelBuilder)
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(AccionesDeRelacionDtm));

            //ApiDeRegistroDtm.DefinirCampoIdDtm<AccionesDeRelacionDtm>(modelBuilder);

            modelBuilder.Entity<AccionesDeRelacionDtm>().Property(x => x.IdNegocio).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_NEGOCIO).IsRequired();
            modelBuilder.Entity<AccionesDeRelacionDtm>().Property(x => x.IdVinculado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_VINCULADO).IsRequired();
            modelBuilder.Entity<AccionesDeRelacionDtm>().Property(x => x.IdAccion).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ACCION).IsRequired();
            modelBuilder.Entity<AccionesDeRelacionDtm>().Property(x => x.Parametros).HasColumnName(ICampos.PARAMETROS).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);
            modelBuilder.Entity<AccionesDeRelacionDtm>().Property(p => p.Descripcion).HasColumnName(ICampos.DESCRIPCION).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);
            modelBuilder.Entity<AccionesDeRelacionDtm>().Property(p => p.Momento).HasColumnName(ICampos.MOMENTO).HasColumnType(IDominio.VARCHAR_2).IsRequired();
            modelBuilder.Entity<AccionesDeRelacionDtm>().Property(p => p.Orden).HasColumnType(IDominio.INT).HasColumnName(ICampos.ORDEN).IsRequired();
            modelBuilder.Entity<AccionesDeRelacionDtm>().Property(p => p.Activo).HasColumnName(ICampos.ACTIVO).HasColumnType(IDominio.BIT).IsRequired();

            ApiDeRegistroDtm.DefinirFk<AccionesDeRelacionDtm>(modelBuilder, nameof(AccionesDeRelacionDtm.Negocio), nameof(AccionesDeRelacionDtm.IdNegocio), ICampos.ID_NEGOCIO, unico: false);
            ApiDeRegistroDtm.DefinirFk<AccionesDeRelacionDtm>(modelBuilder, nameof(AccionesDeRelacionDtm.Vinculado), nameof(AccionesDeRelacionDtm.IdVinculado), ICampos.ID_VINCULADO, unico: false);
            ApiDeRegistroDtm.DefinirFk<AccionesDeRelacionDtm>(modelBuilder, nameof(AccionesDeRelacionDtm.Accion), nameof(AccionesDeRelacionDtm.IdAccion), ICampos.ID_ACCION, unico: false);


            modelBuilder.Entity<AccionesDeRelacionDtm>().HasAlternateKey(x => new { x.IdNegocio, x.IdVinculado, x.IdAccion, x.Orden }).HasName($"AK_{Tablas.ACCION_RELACION}");

        }
    }


}
