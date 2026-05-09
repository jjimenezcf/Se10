using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;


namespace ServicioDeDatos.MaestrosTecnico
{
    public enum enumClaseUnitario
    {
        [Description("Material")]
        Material,
        [Description("Mano de obra")]
        ManoDeObra,
        [Description("Servicios")]
        Servicio
    }


    public static class ExtensorClaseUnitario
    {
        public static string Prefijo(this enumClaseUnitario clase) => clase.ToString().Substring(0, 3);
    }

    [Table(Tablas.UNITARIO, Schema = Esquemas.MT)]
    public class UnitarioDtm : ElementoDtm, IUsaDescripcion, IUsaReferencia, IUsaBaja
    {
        public enumClaseUnitario Clase { get; set; }
        public int IdUnidad { get; set; }
        public int IdNaturaleza { get; set; }
        public string Descripcion { get; set; }


        public UnidadDtm Unidad { get; set; }
        public NaturalezaDtm Naturaleza { get; set; }
        public string Referencia { get; set; }

        public decimal Coste { get; set; }
        public decimal Venta { get; set; }

        public override string Expresion => $"({Referencia}) {Nombre}";

        public bool Baja { get; set; }
    }


    [Table(Tablas.UNITARIO + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.MT)]
    public class AuditoriaDeUnUnitarioDtm : AuditoriaDtm
    {
    }

    public static partial class ModeloDeMt
    {
        public static void Unitarios(ModelBuilder modelBuilder)
        {

            ApiDeElementoDtm.DefinirCamposDelElementoDtm<UnitarioDtm>(modelBuilder, indiceUnicoPorNombre: true);
            modelBuilder.Entity<UnitarioDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<UnitarioDtm>(modelBuilder, nameof(UnitarioDtm.Unidad), nameof(UnitarioDtm.IdUnidad), ICampos.ID_UNIDAD, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<UnitarioDtm>(modelBuilder, nameof(UnitarioDtm.Naturaleza), nameof(UnitarioDtm.IdNaturaleza), ICampos.ID_NATURALEZA, requerida: true, unico: false);

            modelBuilder.Entity<UnitarioDtm>().Property(nameof(UnitarioDtm.Coste)).HasColumnName(ICampos.COSTE).HasColumnType(IDominio.DECIMAL).IsRequired(true).HasDefaultValue(0);
            modelBuilder.Entity<UnitarioDtm>().Property(nameof(UnitarioDtm.Venta)).HasColumnName(ICampos.VALOR).HasColumnType(IDominio.DECIMAL).IsRequired(true).HasDefaultValue(0);
        }

        public static void UnitarioAudt(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnUnitarioDtm>(modelBuilder);
        }
    }
}
