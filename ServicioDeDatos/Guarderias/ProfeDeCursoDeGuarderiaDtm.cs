using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Guarderias
{
    [Table(Tablas.CURSO + "_" + nameof(Sufijo.PROFE), Schema = Esquemas.GUARDERIA)]
    public class ProfeDeCursoDeGuarderiaDtm : RegistroDtm, IDetalle, IUsaTrabajador
    {
        public int IdElemento { get; set; }
        public CursoDeGuarderiaDtm Elemento { get; set; }
        public enumNegocio Negocio => enumNegocio.CursoDeGuarderia;
        public int IdTrabajador { get; set; }
        public TrabajadorDtm Trabajador { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;
    }

    public static partial class ModeloDeGuarderias
    {
        internal static void ProfesoresDeUnCurso(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProfeDeCursoDeGuarderiaDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<ProfeDeCursoDeGuarderiaDtm>().Ignore(x => x.Elemento);

            modelBuilder.Entity<ProfeDeCursoDeGuarderiaDtm>().Property(nameof(ProfeDeCursoDeGuarderiaDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<ProfeDeCursoDeGuarderiaDtm, CursoDeGuarderiaDtm>(modelBuilder, nameof(ProfeDeCursoDeGuarderiaDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);

            ApiDeRegistroDtm.DefinirTrabajador<ProfeDeCursoDeGuarderiaDtm>(modelBuilder);

            modelBuilder.Entity<ProfeDeCursoDeGuarderiaDtm>().HasAlternateKey(x => new { x.IdElemento, x.IdTrabajador }).HasName($"AK_{Tablas.CURSO}_{Tablas.TRABAJADOR}_{ICampos.ID_ELEMENTO}_{ICampos.ID_TRABAJADOR}");

        }
    }
}
