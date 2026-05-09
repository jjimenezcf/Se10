using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.SistemaDocumental
{

    [Table(Tablas.CIRCUITO_DOC + "_" + Sufijo.VOLUNTARIO, Schema = Esquemas.SISDOC)]
    public class VoluntarioDeActividadDtm : RegistroDtm, IDetalle, IEsInterlocutor
    {
        public int IdElemento { get; set; }
        public CircuitoDocDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int IdInterlocutor { get; set; }
        public InterlocutorDtm Interlocutor { get; set; }
        public enumNegocio Negocio => enumNegocio.CircuitoDoc;
    }

    public static partial class ModeloDeCircuitoDoc
    {
        internal static void VoluntarioDeActivida(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VoluntarioDeActividadDtm>().Ignore(x => x.Elemento);
            modelBuilder.Entity<VoluntarioDeActividadDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<VoluntarioDeActividadDtm>().Property(nameof(VoluntarioDeActividadDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired();
            ApiDeRegistroDtm.DefinirFk<VoluntarioDeActividadDtm, CircuitoDocDtm>(modelBuilder, nameof(VoluntarioDeActividadDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);

            ApiDeRegistroDtm.DefinirDependencia<VoluntarioDeActividadDtm>(modelBuilder, nameof(VoluntarioDeActividadDtm.Interlocutor), nameof(VoluntarioDeActividadDtm.IdInterlocutor), ICampos.ID_INSCRITO, requerido: true, unico: false);
        }
    }
}
