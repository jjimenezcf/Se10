using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Terceros;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Juridico
{
    [Table(Tablas.CONTRATO + "_" + Sufijo.MATRICULA_GUARDERIA, Schema = Esquemas.JURIDICO)]
    public class MatriculaDeGuarderiaDtm : Ampliacion<ContratoDtm>, IPuedeUsarCliente
    {
        public override enumNegocio Negocio => enumNegocio.Contrato;
        //public new ContratoDtm Elemento;

        public int? IdInfante { get; set; }
        public InfanteDtm Infante { get; set; }

        public int? IdCliente { get; set; }
        public ClienteDtm Cliente { get; set; }

        public int? IdCurso { get; set; }
        public CursoDeGuarderiaDtm Curso { get; set; }

        public string Contacto { get; set; }
        public string eMail { get; set; }
        public string Telefono { get; set; }
    }
    public static partial class ModeloDeContrato
    {
        internal static void DatosDeMatriculaDeGuarderia(ModelBuilder modelBuilder)
        {
            ModeloDeAmpliaciones.DefinirAmpliacion<ContratoDtm, MatriculaDeGuarderiaDtm>(modelBuilder, nameof(ContratoDtm.MatriculaDeGuarderia));

            ApiDeRegistroDtm.DefinirCliente<MatriculaDeGuarderiaDtm>(modelBuilder);

            modelBuilder.Entity<MatriculaDeGuarderiaDtm>().Property(nameof(MatriculaDeGuarderiaDtm.IdInfante)).HasColumnName(ICampos.ID_INFANTE).HasColumnType(IDominio.INT).IsRequired(false);
            ApiDeRegistroDtm.DefinirFk<MatriculaDeGuarderiaDtm>(modelBuilder, nameof(MatriculaDeGuarderiaDtm.Infante), nameof(MatriculaDeGuarderiaDtm.IdInfante), ICampos.ID_INFANTE, unico: false);

            modelBuilder.Entity<MatriculaDeGuarderiaDtm>().Property(nameof(MatriculaDeGuarderiaDtm.IdCurso)).HasColumnName(ICampos.ID_CURSO).HasColumnType(IDominio.INT).IsRequired(false);
            ApiDeRegistroDtm.DefinirFk<MatriculaDeGuarderiaDtm>(modelBuilder, nameof(MatriculaDeGuarderiaDtm.Curso), nameof(MatriculaDeGuarderiaDtm.IdCurso), ICampos.ID_CURSO, unico: false);

            //modelBuilder.Entity<MatriculaDeGuarderiaDtm>()
            //.HasIndex(x => new { x.IdInfante, x.IdCurso })
            //.IsUnique(true)
            //.HasFilter($"[{ICampos.ID_INFANTE}] IS NOT NULL AND [{ICampos.ID_CURSO}]] IS NOT NULL")
            //.HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(MatriculaDeGuarderiaDtm))}_{ICampos.ID_INFANTE}_{ICampos.ID_CURSO}");
        }
    }
}
