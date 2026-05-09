using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Callejero
{
    [Table(Tablas.TIPO_VIA, Schema = Esquemas.CALLEJERO)]
    public class TipoDeViaDtm : RegistroConNombreDtm
    {
        public string Sigla { get; set; }
    }

    public static partial class ModeloDeCallejero
    {
        public static void TipoDeVia(ModelBuilder modelBuilder)
        {
            ApiDeNombreDtm.DefinirCampoNombreDtm<TipoDeViaDtm>(modelBuilder);
            
            modelBuilder.Entity<TipoDeViaDtm>().Property(v => v.Sigla)
                .HasColumnName(ICampos.SIGLA)
                .HasColumnType(IDominio.VARCHAR_10)
                .IsRequired(true);

            modelBuilder.Entity<TipoDeViaDtm>().HasIndex(p => p.Sigla).HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(TipoDeViaDtm))}_{ICampos.SIGLA}").IsUnique(false);
        }

    }

}
