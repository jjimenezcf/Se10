using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.MaestrosTecnico
{
    [Table(Tablas.UNITARIO + "_" + nameof(Sufijo.TARIFA), Schema = Esquemas.MT)]
    public class TarifaDtm : RegistroDtm, IDetalle
    {
        public int IdElemento { get; set; }
        public UnitarioDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int IdProveedor { get; set; }
        public ProveedorDtm Proveedor {get; set;}
        public string Referencia { get; set; }
        public decimal Tarifa { get; set; }
        public enumNegocio Negocio => enumNegocio.Unitario;
    }

    public static partial class ModeloDeMt
    {
        internal static void TarifasDeUnUnitario(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TarifaDtm>().Ignore(x => x.Negocio);
            //modelBuilder.Entity<TarifaDtm>().Property(nameof(TarifaDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired();
            //ApiDeRegistroDtm.DefinirFk<TarifaDtm, UnitarioDtm>(modelBuilder, nameof(TarifaDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<TarifaDtm>(modelBuilder, nameof(TarifaDtm.Elemento), nameof(TarifaDtm.IdElemento), ICampos.ID_ELEMENTO, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<TarifaDtm>(modelBuilder, nameof(TarifaDtm.Proveedor), nameof(TarifaDtm.IdProveedor), ICampos.ID_PROVEEDOR, requerida: true, unico: false);
            modelBuilder.Entity<TarifaDtm>().Property(nameof(TarifaDtm.Tarifa)).HasColumnName(ICampos.VALOR).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<TarifaDtm>().Property(nameof(TarifaDtm.Referencia)).HasColumnName(ICampos.REFERENCIA).HasColumnType(IDominio.VARCHAR_50).IsRequired(true);
            var tabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TarifaDtm)); 
            modelBuilder.Entity<TarifaDtm>().HasAlternateKey(p => new { p.IdElemento, p.IdProveedor }).HasName($"AK_{tabla}_{ICampos.ID_ELEMENTO}_{ICampos.ID_PROVEEDOR}");
        }
    }
}
