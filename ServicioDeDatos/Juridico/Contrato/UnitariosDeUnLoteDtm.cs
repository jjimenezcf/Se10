using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.MaestrosTecnico;

namespace ServicioDeDatos.Juridico
{
    public class ltrUnitariosDeUnLote
    {
        public static readonly string JoinConUnitarios = nameof(JoinConUnitarios);
        public static readonly string JoinConLotes = nameof(JoinConLotes);
    }

    [Table(Tablas.LOTE + "_" + Sufijo.UNITARIO, Schema = Esquemas.JURIDICO)]
    public class UnitariosDeUnLoteDtm : RelacionDtm
    {
        public int IdUnitario { get; set; }
        public UnitarioDtm Unitario { get; set; }
        public int IdLote { get; set; }
        public LoteDeUnContratoDtm Lote { get; set; }
        public decimal Coste { get; set; }
        public decimal Venta { get; set; }

        public UnitariosDeUnLoteDtm()
        {
            PropiedadDelIdElemento1 = nameof(IdLote);
            PropiedadDelIdElemento2 = nameof(IdUnitario);
        }
    }
    
    public static partial class ModeloDeContrato
    {
        public static void UnitariosDeUnLote(ModelBuilder modelBuilder)
        {
            ApiDeRelaciones.DefinirRelacion<UnitariosDeUnLoteDtm>(modelBuilder, nameof(UnitariosDeUnLoteDtm.Lote), nameof(UnitariosDeUnLoteDtm.Unitario), ICampos.ID_LOTE, ICampos.ID_UNITARIO);
            modelBuilder.Entity<UnitariosDeUnLoteDtm>().Property(nameof(UnitariosDeUnLoteDtm.Coste)).HasColumnName(ICampos.COSTE).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<UnitariosDeUnLoteDtm>().Property(nameof(UnitariosDeUnLoteDtm.Venta)).HasColumnName(ICampos.VENTA).HasColumnType(IDominio.DECIMAL).IsRequired(true);
        }
    }

}
