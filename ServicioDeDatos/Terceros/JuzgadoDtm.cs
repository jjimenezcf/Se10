using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Terceros
{

    [Table(Tablas.JUZGADO_CLASE, Schema = Esquemas.TERCEROS)]
    public class ClaseDeJuzgadoDtm : RegistroConNombreDtm, IRegistroDeParametrizacion
    {
    }

    [Table(Tablas.JUZGADO, Schema = Esquemas.TERCEROS)]
    public class JuzgadoDtm : RegistroConNombreDtm
    {
        public int IdClase { get; set; }
        public ClaseDeJuzgadoDtm Clase { get; set; }

        public int IdMunicipio { get; set; }
        public MunicipioDtm Municipio;

        public string Calificador { get; set; }
    }


    public static partial class ModeloDeTerceros
    {
        public static void ClaseDeJuzgado(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<ClaseDeJuzgadoDtm>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<ClaseDeJuzgadoDtm>(modelBuilder, unico: true);
        }

        public static void Juzgado(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<JuzgadoDtm>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<JuzgadoDtm>(modelBuilder, unico: true);

            modelBuilder.Entity<JuzgadoDtm>().Property(x => x.IdClase).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_CLASE).IsRequired();
            ApiDeRegistroDtm.DefinirFk<JuzgadoDtm>(modelBuilder, nameof(JuzgadoDtm.Clase), nameof(JuzgadoDtm.IdClase), ICampos.ID_CLASE, unico: false);

            modelBuilder.Entity<JuzgadoDtm>().Property(x => x.IdMunicipio).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_MUNICIPIO).IsRequired();
            ApiDeRegistroDtm.DefinirFk<JuzgadoDtm>(modelBuilder, nameof(JuzgadoDtm.Municipio), nameof(JuzgadoDtm.IdMunicipio), ICampos.ID_MUNICIPIO, unico: false);

            modelBuilder.Entity<JuzgadoDtm>().Property(x => x.Calificador).HasColumnType(IDominio.VARCHAR_20).HasColumnName(ICampos.CALIFICADOR).IsRequired();

        }
    }
}
