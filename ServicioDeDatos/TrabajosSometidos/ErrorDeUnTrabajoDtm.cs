
using System;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.TrabajosSometidos
{
    public class ErrorDeUnTrabajoDtm : RegistroDtm
    {
        public int IdTrabajoDeUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public string Error { get; set; }
        public string Detalle { get; set; }
        public virtual TrabajoDeUsuarioDtm TrabajoDeUsuario { get; set; }
    }

    public static class TablaDeErroresDeUnTrabajo
    {
        public static void Definir(ModelBuilder mb)
        {
            mb.Entity<ErrorDeUnTrabajoDtm>().ToTable("ERROR", "TRABAJO");
            mb.Entity<ErrorDeUnTrabajoDtm>().Property(p => p.IdTrabajoDeUsuario).HasColumnName("ID_TRABAJO_USUARIO").IsRequired(true).HasColumnType(IDominio.INT);
            mb.Entity<ErrorDeUnTrabajoDtm>().Property(p => p.Fecha).HasColumnName("FECHA").IsRequired(true).HasColumnType(IDominio.DATETIME_2);
            mb.Entity<ErrorDeUnTrabajoDtm>().Property(p => p.Error).HasColumnName("ERROR").IsRequired(true).HasColumnType(IDominio.VARCHAR_2000);
            mb.Entity<ErrorDeUnTrabajoDtm>().Property(p => p.Detalle).HasColumnName("DETALLE").IsRequired(false).HasColumnType(IDominio.VARCHAR_MAX);
            mb.Entity<ErrorDeUnTrabajoDtm>().HasOne(x => x.TrabajoDeUsuario).WithMany().HasForeignKey(x => x.IdTrabajoDeUsuario).HasConstraintName("FK_ERROR_DE_TRABAJO_ID_TRABAJO_USUARIO").OnDelete(DeleteBehavior.Restrict);
        }
    }


}