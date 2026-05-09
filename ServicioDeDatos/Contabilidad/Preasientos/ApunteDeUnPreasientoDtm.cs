using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

public enum enumPosicionContable { Debe, Haber }

public enum enumClaseDeApunte 
{
    Far_Gasto, Far_Iva, Far_Irpf, Far_Proveedor,
    Pag_Acreedor, Pag_Pago, Pag_Gasto,
    Fae_Iva, Fae_Cliente, Fae_Ingreso, Fae_Irpf,
    Cob_Deudor, Cob_Cobro
}
public static class ltrTipoDeApunte 
{ 
    public const string Gasto = nameof(Gasto);
    public const string Pago = nameof(Pago);
    public const string Ingreso = nameof(Ingreso);
    public const string Cobro = nameof(Cobro);
}

namespace ServicioDeDatos.Contabilidad
{
    [Table(Tablas.PREASIENTO + "_" + nameof(Sufijo.APUNTE), Schema = Esquemas.CONTABILIDAD)]
    public class ApunteDeUnPreasientoDtm : RegistroDtm, IDetalle
    {
        public int IdElemento { get; set; }
        public PreasientoDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public enumPosicionContable Posicion { get; set; }

        public enumClaseDeApunte Clase {  get; set; }
        public string Tipo { get; set; }

        public int Orden { get; set; }

        public string Cuenta { get; set; }

        public decimal Importe {  get; set; }
        public string Concepto { get; set; }
        public decimal? BaseDelImporte { get; set; }
        public decimal? IvaDelImporte { get; set; }
        public int? IdIva { get; set; }
        public enumNegocio Negocio => enumNegocio.Preasiento;
    }


    public class Saldo
    {
        public string Cuenta {  set; get; }
        public decimal Debe { set; get; }
        public decimal Haber { set; get; }
    }

    public static partial class ModeloDePreasiento
    {
        internal static void ApuntesDeUnPreasiento(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApunteDeUnPreasientoDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<ApunteDeUnPreasientoDtm>().Ignore(x => x.Elemento);

            ApiDeRegistroDtm.DefinirCampoFk<ApunteDeUnPreasientoDtm>(modelBuilder, nameof(ApunteDeUnPreasientoDtm.Elemento), nameof(ApunteDeUnPreasientoDtm.IdElemento), ICampos.ID_ELEMENTO, requerida: true, unico: false);

            modelBuilder.Entity<ApunteDeUnPreasientoDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired(true);
            modelBuilder.Entity<ApunteDeUnPreasientoDtm>().Property(p => p.Posicion).HasColumnName(ICampos.POSICION).HasColumnType(IDominio.VARCHAR_5).IsRequired(true);
            modelBuilder.Entity<ApunteDeUnPreasientoDtm>().Property(p => p.Tipo).HasColumnName(ICampos.DETALLE).HasColumnType(IDominio.VARCHAR_20).IsRequired(false);

            modelBuilder.Entity<ApunteDeUnPreasientoDtm>().Property(p => p.Orden).HasColumnName(ICampos.ORDEN).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<ApunteDeUnPreasientoDtm>().Property(p => p.Cuenta).HasColumnName(ICampos.CODIGO_CUENTA).HasColumnType(IDominio.CUENTA_CONTABLE).IsRequired(true);
            modelBuilder.Entity<ApunteDeUnPreasientoDtm>().Property(p => p.Importe).HasColumnName(ICampos.IMPORTE).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<ApunteDeUnPreasientoDtm>().Property(p => p.BaseDelImporte).HasColumnName(ICampos.BI).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<ApunteDeUnPreasientoDtm>().Property(p => p.IvaDelImporte).HasColumnName(ICampos.IVA).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<ApunteDeUnPreasientoDtm>().Property(p => p.IdIva).HasColumnName(ICampos.ID_IVA_S).HasColumnType(IDominio.INT).IsRequired(false);

            modelBuilder.Entity<ApunteDeUnPreasientoDtm>().Property(nameof(ApunteDeUnPreasientoDtm.Concepto)).HasColumnName(ICampos.CONCEPTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);

        }

    }
}
