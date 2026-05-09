using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Guarderias
{
    public static class ltrDeAulasDeGuarderia
    {
        public static readonly string Aula = nameof(Aula);
        public const string SelectorParaCurso = nameof(SelectorParaCurso);
    }


    [Table(Tablas.AULA, Schema = Esquemas.GUARDERIA)]
    public class AulaDeGuarderiaDtm : RegistroConNombreDtm, IUsaCg
    {
        public int IdCg { get; set; }

        public CentroGestorDtm Cg { get; set; }
    }


    public static partial class ModeloDeGuarderias
    {
        public static void AulaDeGuarderia(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<AulaDeGuarderiaDtm>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<AulaDeGuarderiaDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<AulaDeGuarderiaDtm>(modelBuilder, nameof(AulaDeGuarderiaDtm.Cg));


            modelBuilder.Entity<AulaDeGuarderiaDtm>()
                        .HasIndex(p => new { p.IdCg, p.Nombre })
                        .IsUnique(true)
                        .HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(AulaDeGuarderiaDtm))}_{ICampos.ID_CG}_{ICampos.NOMBRE}");

        }
    }
}
