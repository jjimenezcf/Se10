using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Juridico
{
    public enum enumAvaceOperacion { PlanificadoVariado, EliminarPlanificacion, RealizarPartePlanificado, RealizarParteDeContrato, AnularRealizacionDeParte, AnularRealizacionDeContrato, FacturarPlanificado, FacturarRealizado, FacturarContrato, CobrarFactura, AnularCobro }

    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.AVANCE), Schema = Esquemas.JURIDICO)]
    public class AvanceDtm : Ampliacion<ContratoDtm>
    {
        public override enumNegocio Negocio => enumNegocio.Contrato;
        //public new ContratoDtm Elemento;

        public decimal Planificado { get; set; }
        public decimal Realizado { get; set; }
        public decimal Facturado { get; set; }
        public decimal Cobrado { get; set; }
    }



    public static partial class ModeloDeContrato
    {
        internal static void DatosDeAvance(ModelBuilder modelBuilder)
        {
            ModeloDeAmpliaciones.DefinirAmpliacion<ContratoDtm, AvanceDtm>(modelBuilder, nameof(ContratoDtm.Avance));

            modelBuilder.Entity<AvanceDtm>().Property(nameof(AvanceDtm.Planificado)).HasColumnName(ICampos.PLANIFICADO).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<AvanceDtm>().Property(nameof(AvanceDtm.Realizado)).HasColumnName(ICampos.REALIZADO).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<AvanceDtm>().Property(nameof(AvanceDtm.Facturado)).HasColumnName(ICampos.FACTURADO).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<AvanceDtm>().Property(nameof(AvanceDtm.Cobrado)).HasColumnName(ICampos.COBRADO).HasColumnType(IDominio.DECIMAL).IsRequired(true);
        }
    }
}
