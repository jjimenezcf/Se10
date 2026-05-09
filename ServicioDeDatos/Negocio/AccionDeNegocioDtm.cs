
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;

namespace ServicioDeDatos.Negocio
{
    public enum enumMomentoDeAccion
    {
        [Description("Antes de crear")]
        AC,
        [Description("Después de crear")]
        DC,
        [Description("Antes de modificar")]
        AM,
        [Description("Después de modificar")]
        DM,
        [Description("Antes de eliminar")]
        AE,
        [Description("Después de eliminar")]
        DE
    }

    [Table(Tablas.ACCION, Schema = Esquemas.NEGOCIO)]
    public class AccionDeNegocioDtm : RegistroDtm
    {
        public int IdAccion { get; set; }
        public int IdNegocio { get; set; }
        public AccionDtm Accion { get; }
        public NegocioDtm Negocio { get; }
        public enumMomentoDeAccion Momento { get; set; }
        public int Orden { get; set; }
        public string Parametros { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }
    }

    public static partial class ModeloDeNegocio
    {

        internal static void AccionDeNegocio(ModelBuilder modelBuilder)
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(AccionDeNegocioDtm));

            modelBuilder.Entity<AccionDeNegocioDtm>().Property(x => x.IdNegocio).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_NEGOCIO).IsRequired();
            modelBuilder.Entity<AccionDeNegocioDtm>().Property(x => x.IdAccion).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ACCION).IsRequired();
            modelBuilder.Entity<AccionDeNegocioDtm>().Property(p => p.Momento).HasColumnName(ICampos.MOMENTO).HasColumnType(IDominio.VARCHAR_2).IsRequired();
            modelBuilder.Entity<AccionDeNegocioDtm>().Property(p => p.Orden).HasColumnType(IDominio.INT).HasColumnName(ICampos.ORDEN).IsRequired();
            modelBuilder.Entity<AccionDeNegocioDtm>().Property(x => x.Parametros).HasColumnName(ICampos.PARAMETROS).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);
            modelBuilder.Entity<AccionDeNegocioDtm>().Property(p => p.Descripcion).HasColumnName(ICampos.DESCRIPCION).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);
            modelBuilder.Entity<AccionDeNegocioDtm>().Property(p => p.Activo).HasColumnName(ICampos.ACTIVO).HasColumnType(IDominio.BIT).IsRequired();

            ApiDeRegistroDtm.DefinirFk<AccionDeNegocioDtm>(modelBuilder, nameof(AccionDeNegocioDtm.Negocio), nameof(AccionDeNegocioDtm.IdNegocio), ICampos.ID_NEGOCIO, unico: false);
            ApiDeRegistroDtm.DefinirFk<AccionDeNegocioDtm>(modelBuilder, nameof(AccionDeNegocioDtm.Accion), nameof(AccionDeNegocioDtm.IdAccion), ICampos.ID_ACCION, unico: false);


            modelBuilder.Entity<AccionDeNegocioDtm>().HasIndex(x => new { x.IdNegocio, x.IdAccion, x.Momento, x.Orden }).HasDatabaseName($"I_{nombreDeTabla}_AK").IsUnique(true);

        }
    }


}
