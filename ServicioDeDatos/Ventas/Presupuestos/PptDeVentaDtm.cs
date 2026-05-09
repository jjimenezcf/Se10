using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Juridico;
using Utilidades;

namespace ServicioDeDatos.Presupuesto
{

    [Table(Tablas.PRESUPUESTO + "_" + nameof(Sufijo.VENTA), Schema = Esquemas.PRESUPUESTO)]
    public class PptDeVentaDtm : Ampliacion<PresupuestoDtm>
    {
        public override enumNegocio Negocio => enumNegocio.Presupuesto;
        //public new PresupuestoDtm Elemento;

        public int? IdIvaR { get; set; }
        public IvaRepercutidoDtm IvaRepercutido { get; set; }
        public decimal ? Iva { get; set; }  
        public decimal ? Descuento { get; set; }
    }


    public static partial class ModeloDePresupuesto
    {
        internal static void PptDeVenta(ModelBuilder modelBuilder)
        {


            ModeloDeAmpliaciones.DefinirAmpliacion<PresupuestoDtm, PptDeVentaDtm>(modelBuilder, nameof(PresupuestoDtm.DatosPropuestos));

            ApiDeRegistroDtm.DefinirCampoFk<PptDeVentaDtm>(modelBuilder, nameof(PptDeVentaDtm.IvaRepercutido), nameof(PptDeVentaDtm.IdIvaR), ICampos.ID_IVA_R, requerida: false, unico: false);

            modelBuilder.Entity<PptDeVentaDtm>().Property(nameof(PptDeVentaDtm.Iva)).HasColumnName(ICampos.IVA).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);
            modelBuilder.Entity<PptDeVentaDtm>().Property(nameof(PptDeVentaDtm.Descuento)).HasColumnName(ICampos.DESCUENTO).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);
        }
    }
}
