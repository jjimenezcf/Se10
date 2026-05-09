using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.SistemaDocumental
{

    [Table(Tablas.CIRCUITO_DOC + "_" + Sufijo.ACTIVIDAD_FORMATIVA, Schema = Esquemas.SISDOC)]
    public class DatosDeActividadFormativaDtm : Ampliacion<CircuitoDocDtm>, IPuedeUsarResponsable
    {
        public override enumNegocio Negocio => enumNegocio.CircuitoDoc;
        public new CircuitoDocDtm Elemento;

        public int? IdResponsable { get; set; }
        public UsuarioDtm Responsable { get; set; }
        public DateTime? Inicio { get; set; }
        public DateTime? Fin { get; set; }
        public decimal? Coste { get; set; }

    }


    public static partial class ModeloDeCircuitoDoc
    {
        internal static void DatosDeActividadFormativa(ModelBuilder modelBuilder)
        {
            ModeloDeAmpliaciones.DefinirAmpliacion<CircuitoDocDtm, DatosDeActividadFormativaDtm>(modelBuilder, nameof(CircuitoDocDtm.DatosActividadFormativa));

            ApiDeUsuarioDtm.DefinirResponsable<DatosDeActividadFormativaDtm>(modelBuilder, false);
            modelBuilder.Entity<DatosDeActividadFormativaDtm>().Property(p => p.Coste).HasColumnName(ICampos.COSTE).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<DatosDeActividadFormativaDtm>().Property(nameof(DatosDeActividadFormativaDtm.Inicio)).HasColumnName(ICampos.INICIO).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<DatosDeActividadFormativaDtm>().Property(nameof(DatosDeActividadFormativaDtm.Fin)).HasColumnName(ICampos.FIN).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            ApiDeUsuarioDtm.DefinirResponsable<DatosDeActividadFormativaDtm>(modelBuilder, false);
        }
    }

}
