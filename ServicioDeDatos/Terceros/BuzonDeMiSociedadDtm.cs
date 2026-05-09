using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ServicioDeDatos.Terceros
{
    public static class ltrDeBuzonesDeMiSociedad
    {
        public const string PermisosDeBuzones = nameof(PermisosDeBuzones);
    }


    [Table(Tablas.SOCIEDAD + "_" + nameof(Sufijo.BUZON), Schema = Esquemas.TERCEROS)]
    public class BuzonDeMiSociedadDtm : RegistroDtm, IDetalle
    {
        public int IdElemento { get; set; }
        public SociedadDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;
        public string eMail { get; set; }

        public string Buzon { get; set; }

        public int Orden {  get; set; }

        public int IdPermiso { get; set; }
        public PermisoDtm Permiso { get; set; }

        public enumNegocio Negocio => enumNegocio.Sociedad;
    }

    public static partial class ModeloDeTerceros
    {
        public static void BuzonDeMiSociedad(ModelBuilder modelBuilder)
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(BuzonDeMiSociedadDtm));

            modelBuilder.Entity<BuzonDeMiSociedadDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<BuzonDeMiSociedadDtm>().Ignore(x => x.Elemento);

            modelBuilder.Entity<BuzonDeMiSociedadDtm>().Property(nameof(BuzonDeMiSociedadDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<BuzonDeMiSociedadDtm, SociedadDtm>(modelBuilder, nameof(BuzonDeMiSociedadDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);
            modelBuilder.Entity<BuzonDeMiSociedadDtm>().Property(p => p.eMail).HasColumnName(ICampos.EMAIL).HasColumnType(IDominio.VARCHAR_250).IsRequired();
            modelBuilder.Entity<BuzonDeMiSociedadDtm>().Property(p => p.Buzon).HasColumnName(ICampos.BUZON).HasColumnType(IDominio.VARCHAR_250).IsRequired();
            modelBuilder.Entity<BuzonDeMiSociedadDtm>().Property(p => p.Orden).HasColumnName(ICampos.ORDEN).HasColumnType(IDominio.INT).IsRequired().HasDefaultValue(0);

            ApiDeRegistroDtm.DefinirDependencia<BuzonDeMiSociedadDtm>(modelBuilder, nameof(BuzonDeMiSociedadDtm.Permiso), nameof(BuzonDeMiSociedadDtm.IdPermiso), ICampos.ID_PERMISO, unico: true);

            modelBuilder.Entity<BuzonDeMiSociedadDtm>().HasIndex(p => new { p.eMail, p.Buzon }).HasDatabaseName($"I_{nombreDeTabla}_{ICampos.EMAIL}_{ICampos.BUZON}").IsUnique(true);
        }
    }

}

