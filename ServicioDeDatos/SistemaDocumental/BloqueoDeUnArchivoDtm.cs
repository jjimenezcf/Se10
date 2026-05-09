using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.SistemaDocumental
{


    [Table(Tablas.ARCHIVO + "_" + Sufijo.BLOQUEO, Schema = Esquemas.SISDOC)]
    public class BloqueoDeUnArchivoDtm : RegistroDtm
    {
        public int IdArchivo { get; set; }
        public ArchivoDtm Archivo { get; set; }
        public bool Bloqueado { get; set; }

    }

    public static partial class ModeloDocumental
    {
        public static void BloqueoDeUnArchivo(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCampoArchivo<BloqueoDeUnArchivoDtm>(modelBuilder, obligatorio: true, unico: true);
            modelBuilder.Entity<BloqueoDeUnArchivoDtm>().Property(b => b.Bloqueado).HasColumnName(ICampos.BLOQUEO).HasColumnType(IDominio.BIT).IsRequired(true).HasDefaultValue(false);
        }

    }


}
