using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.SistemaDocumental
{

    [Table(Tablas.CIRCUITO_DOC + "_" + Sufijo.INSCRITO, Schema = Esquemas.SISDOC)]
    public class InscritosEnActividadDtm : RegistroDtm, IDetalle, IUsaArchivo, IEsInterlocutor
    {
        public int IdElemento { get; set; }
        public CircuitoDocDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int IdInterlocutor { get; set; }
        public InterlocutorDtm Interlocutor { get; set; }

        public bool Asistio { get; set; }

        public decimal? Pagado { get; set; }

        public int? IdArchivo { get; set; }
        public ArchivoDtm Archivo { get; set; }

        public enumNegocio Negocio => enumNegocio.CircuitoDoc;
    }

    public static partial class ModeloDeCircuitoDoc
    {
        internal static void InscritosEnActividad(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InscritosEnActividadDtm>().Ignore(x => x.Elemento);
            modelBuilder.Entity<InscritosEnActividadDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<InscritosEnActividadDtm>().Property(nameof(InscritosEnActividadDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired();
            ApiDeRegistroDtm.DefinirFk<InscritosEnActividadDtm, CircuitoDocDtm>(modelBuilder, nameof(InscritosEnActividadDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);

            ApiDeRegistroDtm.DefinirDependencia<InscritosEnActividadDtm>(modelBuilder, nameof(InscritosEnActividadDtm.Interlocutor), nameof(InscritosEnActividadDtm.IdInterlocutor), ICampos.ID_INSCRITO, requerido: true, unico: false);

            modelBuilder.Entity<InscritosEnActividadDtm>().Property(p => p.Pagado).HasColumnName(ICampos.IMPORTE).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<InscritosEnActividadDtm>().Property(p => p.Asistio).HasColumnName(ICampos.ASISTIO).HasColumnType(IDominio.BIT).IsRequired();
            ApiDeElementoDtm.DefinirCampoArchivo<InscritosEnActividadDtm>(modelBuilder);
        }
    }
}
