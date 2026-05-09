using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Juridico
{

    [Table(Tablas.PLEITO + "_" + nameof(Sufijo.RECOBRO), Schema = Esquemas.JURIDICO)]
    public class RecobroDtm : Ampliacion<PleitoDtm>
    {
        public override enumNegocio Negocio => enumNegocio.Pleito;
        //public new PleitoDtm Elemento;

        public decimal Principal { get; set; }  
        public decimal ? Intereses { get; set; }
        public DateTime FechaDeDeuda { get; set; }
        public string Notacion { get; set; }
    }


    public static partial class ModeloDePleito
    {
        internal static void DatosDeRecobro(ModelBuilder modelBuilder)
        {

            ModeloDeAmpliaciones.DefinirAmpliacion<PleitoDtm, RecobroDtm>(modelBuilder, nameof(PleitoDtm.Recobro));


            modelBuilder.Entity<RecobroDtm>().Property(nameof(RecobroDtm.Principal)).HasColumnName(ICampos.PRINCIPAL).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<RecobroDtm>().Property(nameof(RecobroDtm.Intereses)).HasColumnName(ICampos.INTERESES).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<RecobroDtm>().Property(nameof(RecobroDtm.FechaDeDeuda)).HasColumnName(ICampos.FECHA).HasColumnType(IDominio.DATETIME_2).IsRequired(true);
            modelBuilder.Entity<RecobroDtm>().Property(nameof(RecobroDtm.Notacion)).HasColumnName(ICampos.ANOTACION).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);
        }
    }
}
