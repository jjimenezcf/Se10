using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Terceros
{
    [Table(Tablas.SOCIEDAD + "_" + nameof(Sufijo.PARAMETRO), Schema = Esquemas.TERCEROS)]
    public class ParametrosDeMiSociedadDtm : Ampliacion<SociedadDtm>
    {
        public override enumNegocio Negocio => enumNegocio.Sociedad;

        //public new SociedadDtm Elemento;

        public string PieDePresupuesto { get; set; }
        public string PieDeFactura { get; set; }
        public string InscritoEn { get; set; }
        public int Libro => 171;
        public string LocalizadoEn => "Murcia";
        public int Hoja => 0;
        public int Folio => 1;
        public string Seccion => "3ª";
        public string Volumen => Literal.Cero;
        public string Adicional => "ref.3657,Inscip.1ª";
    }

    public static partial class ModeloDeTerceros
    {
        public static void ParametrosDeMiSociedad(ModelBuilder modelBuilder)
        {

            ModeloDeAmpliaciones.DefinirAmpliacion<SociedadDtm, ParametrosDeMiSociedadDtm>(modelBuilder, nameof(SociedadDtm.Parametros));
            modelBuilder.Entity<ParametrosDeMiSociedadDtm>().Property(p => p.PieDePresupuesto).HasColumnName(ICampos.PIE_DE_PPT).HasColumnType(IDominio.VARCHAR_2000).IsRequired(true);
            modelBuilder.Entity<ParametrosDeMiSociedadDtm>().Property(p => p.PieDeFactura).HasColumnName(ICampos.PIE_DE_FACTURA).HasColumnType(IDominio.VARCHAR_2000).IsRequired(true);
            modelBuilder.Entity<ParametrosDeMiSociedadDtm>().Property(p => p.InscritoEn).HasColumnName(ICampos.INSCRITO_EN).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
        }
    }

}

