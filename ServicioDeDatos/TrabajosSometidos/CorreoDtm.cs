using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;

namespace ServicioDeDatos.TrabajosSometidos
{
    [Table("CORREO", Schema = "TRABAJO")]

    public class CorreoDtm: RegistroDtm
    {
        public string Emisor { get; set; }

        public string Receptores { get; set; }

        public string Asunto { get; set; }

        public string Cuerpo { get; set; }

        public string Elementos { get; set; }

        public string Archivos { get; set; }

        public int IdUsuario { get; set; }

        public DateTime Creado { get; set; }

        public DateTime Enviado { get; set; }
        public virtual UsuarioDtm Usuario { get; set; }

    }

    public static class TablaDeCorreos
    {
        public static void Definir(ModelBuilder mb)
        {
            mb.Entity<CorreoDtm>().ToTable("CORREO", "TRABAJO");
            mb.Entity<CorreoDtm>().Property(p => p.Emisor).HasColumnName("EMISOR").IsRequired(true).HasColumnType("VARCHAR(250)");
            mb.Entity<CorreoDtm>().Property(p => p.Receptores).HasColumnName("RECEPTORES").IsRequired(true).HasColumnType("VARCHAR(2000)");
            mb.Entity<CorreoDtm>().Property(p => p.Asunto).HasColumnName("ASUNTO").IsRequired(true).HasColumnType("VARCHAR(500)");
            mb.Entity<CorreoDtm>().Property(p => p.Cuerpo).HasColumnName("CUERPO").IsRequired(true).HasColumnType("VARCHAR(MAX)");
            mb.Entity<CorreoDtm>().Property(p => p.Elementos).HasColumnName("ELEMENTOS").IsRequired(false).HasColumnType("VARCHAR(2000)");
            mb.Entity<CorreoDtm>().Property(p => p.Archivos).HasColumnName("ARCHIVOS").IsRequired(false).HasColumnType("VARCHAR(MAX)");
            mb.Entity<CorreoDtm>().Property(p => p.IdUsuario).HasColumnName("ID_USUARIO").IsRequired(true).HasColumnType("INT");
            mb.Entity<CorreoDtm>().Property(p => p.Creado).HasColumnName("CREADO").IsRequired(true).HasColumnType("DATETIME2(7)");
            mb.Entity<CorreoDtm>().Property(p => p.Enviado).HasColumnName("ENVIADO").IsRequired(true).HasColumnType("DATETIME2(7)");
            mb.Entity<CorreoDtm>().HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).HasConstraintName("FK_TRABAJO_DE_USUARIO_ID_USUARIO").OnDelete(DeleteBehavior.Restrict);
        }
    }


    public static class CorreoSql
    {
        public static string ActualizarFechaDeEnvio = "UPDATE TRABAJO.CORREO SET ENVIADO = @Enviado WHERE ID = @Id";

    }
}
