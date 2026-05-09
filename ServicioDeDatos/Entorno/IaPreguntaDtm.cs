using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Entorno
{
    [Table("IA_PREGUNTA", Schema = Esquemas.ENTORNO)]
    public class IaPreguntaDtm : RegistroDtm
    {
        public string Guid { get; set; }
        public int IdUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public string Pregunta { get; set; }
        public string Respuesta { get; set; }

        public UsuarioDtm Usuario { get; set; }
    }

    public static partial class ModeloDeEntorno
    {
        public static void IaPreguntas(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IaPreguntaDtm>()
                .Property(p => p.Guid)
                .HasColumnName(ICampos.GUID)
                .HasColumnType(IDominio.VARCHAR_50)
                .IsRequired(true);

            modelBuilder.Entity<IaPreguntaDtm>()
                .Property(p => p.Fecha)
                .HasColumnName(ICampos.FECHA)
                .HasColumnType(IDominio.DATETIME_2)
                .IsRequired(true);

            modelBuilder.Entity<IaPreguntaDtm>()
                .Property(p => p.Pregunta)
                .HasColumnName(ICampos.PREGUNTA)
                .HasColumnType(IDominio.VARCHAR_2000)
                .IsRequired(true);

            modelBuilder.Entity<IaPreguntaDtm>()
                .Property(p => p.Respuesta)
                .HasColumnName(ICampos.RESPUESTA)
                .HasColumnType(IDominio.VARCHAR_MAX)
                .IsRequired(false);

            ApiDeRegistroDtm.DefinirCampoFk<IaPreguntaDtm>(
                modelBuilder,
                nameof(IaPreguntaDtm.Usuario),
                nameof(IaPreguntaDtm.IdUsuario),
                ICampos.ID_USUARIO,
                requerida: true,
                unico: false);

            modelBuilder.Entity<IaPreguntaDtm>()
                .HasIndex(p => p.Guid)
                .IsUnique(false)
                .HasDatabaseName($"I_IA_PREGUNTA_{ICampos.GUID}");

            modelBuilder.Entity<IaPreguntaDtm>()
                .HasIndex(p => p.Pregunta)
                .IsUnique(false)
                .HasDatabaseName($"I_IA_PREGUNTA_{ICampos.PREGUNTA}");
        }
    }
}
