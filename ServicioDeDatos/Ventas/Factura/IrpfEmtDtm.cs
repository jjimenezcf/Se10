using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Ventas
{

    [Table(Tablas.FACTURA_EMT + "_" + nameof(Sufijo.IRPF), Schema = Esquemas.VENTA)]
    public class IrpfEmtDtm : Ampliacion<FacturaEmtDtm>
    {
        public override enumNegocio Negocio => enumNegocio.FacturaEmitida;

        //public new FacturaEmtDtm Elemento;
        public decimal? BiSujeta { get; set; }
        public int? IdIrpf { get; set; }
        public IrpfDtm TipoIrpf { get; set; }
        public decimal? Irpf { get; set; }
        public decimal? Importe => BiSujeta * (Irpf is null ? 0 : (decimal)Irpf / 100);
    }


    public static partial class ModeloDeFacturaEmt
    {
        internal static void IrpfEmt(ModelBuilder modelBuilder)
        {
            ModeloDeAmpliaciones.DefinirAmpliacion<FacturaEmtDtm, IrpfEmtDtm>(modelBuilder, nameof(FacturaEmtDtm.IrpfEmt));

            modelBuilder.Entity<IrpfEmtDtm>().Property(nameof(IrpfEmtDtm.BiSujeta)).HasColumnName(ICampos.BI).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<IrpfEmtDtm>(modelBuilder, nameof(IrpfEmtDtm.TipoIrpf), nameof(IrpfEmtDtm.IdIrpf), ICampos.ID_IRPF, requerida: false, unico: false);
            modelBuilder.Entity<IrpfEmtDtm>().Property(nameof(IrpfEmtDtm.Irpf)).HasColumnName(ICampos.IRPF).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);
        }

    }
}
