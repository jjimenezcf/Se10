
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Terceros;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;
using static Dapper.SqlMapper;

namespace ServicioDeDatos.Ventas
{
    public class ltrDeUnCobro
    {
    }

    /*
     01: Efectivo

02: Cheque

03: Pagaré

04: Transferencia

05: Domiciliación bancaria

06: Giro

07: Tarjeta de crédito/débito

08: Adeudo directo

09: Confirming

10: Tarjeta de débito/crédito

11: Otro medio

12: Recibo

13: Transferencia o ingreso en cuenta

14: Letra de cambio

15: Pago en especie

16: Compensación

17: Cheque bancario

18: Cheque nominativo

19: Cheque al portador
     */

    public enum enumClaseDeCobro
    {
        [Description("Pago contado")]
        Contado,
        [Description("Pago por transferencia")]
        Transferencia,
        [Description("Carta de pago")]
        CartaDePago,
        [Description("Remesa bancaria")]
        Remesa,
    }

    [Table(Tablas.FACTURA_EMT + "_" + Sufijo.COBRO, Schema = Esquemas.VENTA)]
    public class CobroDeFaeDtm : RegistroDtm, IDetalle, IAuditoria, IUsaPreasiento
    {
        public int IdElemento { get; set; }
        public FacturaEmtDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public DateTime CobradoEl { get; set; }
        public decimal Cobrado { get; set; }
        public enumClaseDeCobro Clase { get; set; }

        public int? IdCuentaDeIngreso { get; set; }
        public CuentaDeMiSociedadDtm CuentaDeIngreso { get;  }

        public int? IdCuentaDeCargo { get; set; }
        public CuentaDeClienteDtm CuentaDeCargo { get;  }

        public int? IdFacturaRemesada { get; set; }
        public FacturaEmtDeUnaRemesaDtm FacturaRemesada { get; }

        public enumNegocio Negocio => enumNegocio.FacturaEmitida;

        public int IdUsuaCrea { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }

        public int? IdUsuaModi { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

        public string Referencia { get; set; }
        public int? IdPreasiento { get; set; }
        public PreasientoDtm Preasiento { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }


    public static partial class ModeloDeFacturaEmt
    {
        internal static void CobroDeFae(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CobroDeFaeDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<CobroDeFaeDtm>().Ignore(x => x.Referencia);
            modelBuilder.Entity<CobroDeFaeDtm>().Ignore(x => x.CuentaDeCargo);
            modelBuilder.Entity<CobroDeFaeDtm>().Ignore(x => x.CuentaDeIngreso);
            modelBuilder.Entity<CobroDeFaeDtm>().Ignore(x => x.FacturaRemesada);

            ApiDeRegistroDtm.DefinirCampoFk<CobroDeFaeDtm>(modelBuilder, nameof(CobroDeFaeDtm.Elemento), nameof(CobroDeFaeDtm.IdElemento), ICampos.ID_ELEMENTO, requerida: true, unico: false);

            modelBuilder.Entity<CobroDeFaeDtm>().Property(p => p.CobradoEl).HasColumnName(ICampos.COBRADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);
            modelBuilder.Entity<CobroDeFaeDtm>().Property(p => p.Cobrado).HasColumnName(ICampos.IMPORTE).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<CobroDeFaeDtm>().Property(nameof(CobroDeFaeDtm.Clase)).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<CobroDeFaeDtm>(modelBuilder, nameof(CobroDeFaeDtm.CuentaDeCargo), nameof(CobroDeFaeDtm.IdCuentaDeCargo), ICampos.ID_CUENTA_CARGO, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<CobroDeFaeDtm>(modelBuilder, nameof(CobroDeFaeDtm.CuentaDeIngreso), nameof(CobroDeFaeDtm.IdCuentaDeIngreso), ICampos.ID_CUENTA_INGRESO, requerida: false, unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<CobroDeFaeDtm>(modelBuilder, nameof(CobroDeFaeDtm.FacturaRemesada), nameof(CobroDeFaeDtm.IdFacturaRemesada), ICampos.ID_FACTURA_REMESADA, requerida: false, unico: true);


            ApiDeElementoDtm.DefinirCamposDeAuditoria<CobroDeFaeDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirPreasiento<CobroDeFaeDtm>(modelBuilder, obligatorio: false, unico: true);
        }

    }
}
