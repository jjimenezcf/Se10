using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Ventas
{

    [Table(Tablas.FACTURA_EMT + "_" + nameof(Sufijo.PERIODO), Schema = Esquemas.VENTA)]
    public class PeriodoEmtDtm : Ampliacion<FacturaEmtDtm>
    {
        public override enumNegocio Negocio => enumNegocio.FacturaEmitida;        
        
        //public new FacturaEmtDtm Elemento;
        public DateTime? Inicio { get; set; }  
        public DateTime? Fin { get; set; }
        public string Notacion { get; set; }
    }


    public static partial class ModeloDeFacturaEmt
    {
        internal static void PeriodoEmt(ModelBuilder modelBuilder)
        {

            ModeloDeAmpliaciones.DefinirAmpliacion<FacturaEmtDtm, PeriodoEmtDtm>(modelBuilder, nameof(FacturaEmtDtm.Periodo));

            modelBuilder.Entity<PeriodoEmtDtm>().Property(nameof(PeriodoEmtDtm.Inicio)).HasColumnName(ICampos.INICIO).HasColumnType(IDominio.DATE).IsRequired(false);
            modelBuilder.Entity<PeriodoEmtDtm>().Property(nameof(PeriodoEmtDtm.Fin)).HasColumnName(ICampos.FIN).HasColumnType(IDominio.DATE).IsRequired(false);
            modelBuilder.Entity<PeriodoEmtDtm>().Property(nameof(PeriodoEmtDtm.Notacion)).HasColumnName(ICampos.ANOTACION).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);
        }
    }
}
