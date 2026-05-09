using System;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.TrabajosSometidos
{
    public class TrazaDeUnTrabajoDtm : RegistroDtm
    {
        public int IdTrabajoDeUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public string Traza { get; set; }
        public virtual TrabajoDeUsuarioDtm TrabajoDeUsuario { get; set; }
    }

    public static class TablaDeLatrazaDeUnTrabajo
    {
        public static void Definir(ModelBuilder mb)
        {
            mb.Entity<TrazaDeUnTrabajoDtm>().ToTable("TRAZA", "TRABAJO");
            mb.Entity<TrazaDeUnTrabajoDtm>().Property(p => p.IdTrabajoDeUsuario).HasColumnName("ID_TRABAJO_USUARIO").IsRequired(true).HasColumnType("INT");
            mb.Entity<TrazaDeUnTrabajoDtm>().Property(p => p.Fecha).HasColumnName("FECHA").IsRequired(true).HasColumnType("DATETIME2(7)");
            mb.Entity<TrazaDeUnTrabajoDtm>().Property(p => p.Traza).HasColumnName("LOG").IsRequired(true).HasColumnType("VARCHAR(MAX)");
            mb.Entity<TrazaDeUnTrabajoDtm>().HasOne(x => x.TrabajoDeUsuario).WithMany().HasForeignKey(x => x.IdTrabajoDeUsuario).HasConstraintName("FK_LOG_DE_TRABAJO_ID_TRABAJO_USUARIO").OnDelete(DeleteBehavior.Restrict);
        }
    }
}