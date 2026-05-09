using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Guarderias
{

    [Table(Tablas.CURSO_INFANTE, Schema = Esquemas.GUARDERIA)]
    public class InfanteDeUnCursoDtm : RelacionDtm, IDetalle
    {
        public int IdElemento { get; set; }
        public CursoDeGuarderiaDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int IdInfante { get; set; }
        public InfanteDtm Infante { get; set; }
        public enumNegocio Negocio => enumNegocio.CursoDeGuarderia;

        public InfanteDeUnCursoDtm()
        {
            PropiedadDelIdElemento1 = nameof(IdElemento);
            PropiedadDelIdElemento2 = nameof(IdInfante);
        }

    }

    public static partial class ModeloDeGuarderias
    {
        public static void InfanteDeUnCurso(ModelBuilder modelBuilder)
        {
            {
                modelBuilder.Entity<InfanteDeUnCursoDtm>().Ignore(x => x.Negocio);
                modelBuilder.Entity<InfanteDeUnCursoDtm>().Ignore(x => x.Elemento);
                
                modelBuilder.Entity<InfanteDeUnCursoDtm>().Property(nameof(InfanteDeUnCursoDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
                ApiDeRegistroDtm.DefinirFk<InfanteDeUnCursoDtm, CursoDeGuarderiaDtm>(modelBuilder, nameof(InfanteDeUnCursoDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);

                modelBuilder.Entity<InfanteDeUnCursoDtm>().Property(nameof(InfanteDeUnCursoDtm.IdInfante)).HasColumnName(ICampos.ID_INFANTE).HasColumnType(IDominio.INT).IsRequired(true);
                ApiDeRegistroDtm.DefinirFk<InfanteDeUnCursoDtm>(modelBuilder, nameof(InfanteDeUnCursoDtm.Infante), nameof(InfanteDeUnCursoDtm.IdInfante), ICampos.ID_INFANTE, unico: false);

                modelBuilder.Entity<InfanteDeUnCursoDtm>().HasAlternateKey(x => new { x.IdElemento, x.IdInfante }).HasName($"AK_{Tablas.CURSO}_{Tablas.INFANTE}_{ICampos.ID_ELEMENTO}_{ICampos.ID_INFANTE}");
            }
        }
    }
}
