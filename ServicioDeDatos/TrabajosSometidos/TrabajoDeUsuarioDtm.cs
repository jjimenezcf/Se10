using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;



namespace ServicioDeDatos.TrabajosSometidos
{
    public enum enumEstadosDeUnTrabajo {Pendiente, iniciado, Terminado, conErrores, Error, Bloqueado}

    public static class TrabajoSometido { 
        public static string ToDtm(this enumEstadosDeUnTrabajo estado)
        {
            switch(estado)
            {
                case enumEstadosDeUnTrabajo.conErrores: return "TR";
                case enumEstadosDeUnTrabajo.Pendiente: return "PT";
                case enumEstadosDeUnTrabajo.Bloqueado: return "BL";
                case enumEstadosDeUnTrabajo.iniciado: return "EJ";
                case enumEstadosDeUnTrabajo.Terminado: return "OK";
                case enumEstadosDeUnTrabajo.Error: return "ER";
            }

            throw new Exception($"El estado de un trabajo {estado} no está definido en la BD");
        }
        public static string ToDto(this enumEstadosDeUnTrabajo estado)
        {
            switch (estado)
            {
                case enumEstadosDeUnTrabajo.conErrores: return "Con errores" ;
                case enumEstadosDeUnTrabajo.Pendiente:  return "Pendiente";
                case enumEstadosDeUnTrabajo.Bloqueado:  return "Bloqueado";
                case enumEstadosDeUnTrabajo.iniciado:   return "Iniciado";
                case enumEstadosDeUnTrabajo.Terminado:  return "Terminado";
                case enumEstadosDeUnTrabajo.Error:      return "Erroneo";
            }

            throw new Exception($"El estado de un trabajo {estado} no está definido en la BD");
        }
        public static string ToDto(string estadoDtm)
        {
            switch (estadoDtm)
            {
                case "TR": return "Con errores" ;
                case "PT": return "Pendiente";
                case "BL": return "Bloqueado";
                case "EJ": return "Iniciado";
                case "OK": return "Terminado";
                case "ER": return "Erroneo";
            }

            throw new Exception($"El estado de un trabajo {estadoDtm} no está definido en la BD");
        }

        public static string ToDtm(string estadoDto)
        {
            switch (estadoDto)
            {
                case "Con errores" : return "TR";
                case "Pendiente"   : return "PT";
                case "Bloqueado"   : return "BL";
                case "Iniciado"    : return "EJ";
                case "Terminado"   : return "OK";
                case "Erroneo"     : return "ER";
            }

            throw new Exception($"El estado de un trabajo '{estadoDto}' no es válido");
        }

        public static string EnumeradoToDtm(string estado)
        {
            switch (estado)
            {
                case nameof(enumEstadosDeUnTrabajo.conErrores): return "TR";
                case nameof(enumEstadosDeUnTrabajo.Pendiente): return "PT";
                case nameof(enumEstadosDeUnTrabajo.Bloqueado): return "BL";
                case nameof(enumEstadosDeUnTrabajo.iniciado): return "EJ";
                case nameof(enumEstadosDeUnTrabajo.Terminado): return "OK";
                case nameof(enumEstadosDeUnTrabajo.Error): return "ER";
            }

            throw new Exception($"El estado de un trabajo '{estado}' no es válido");
        }

    }


    [Table("USUARIO", Schema = "TRABAJO")]
    public class TrabajoDeUsuarioDtm : RegistroDtm
    {
        public int IdTrabajo { get; set; }
        public int IdEjecutor { get; set; }
        public int IdSometedor { get; set; }
        public DateTime Encolado { get; set; }
        public DateTime Planificado { get; set; }
        public DateTime? Iniciado { get; set; }
        public DateTime? Terminado { get; set; }

        public string Estado { get; set; }
        public string Parametros { get; set; }
        public int Periodicidad { get; set; }
        public virtual TrabajoSometidoDtm Trabajo { get; set; }
        public virtual UsuarioDtm Sometedor { get; set; }
        public virtual UsuarioDtm Ejecutor { get; set; }
    }

    public static class TablaTrabajoDeUsuario
    {
        public static string Tabla => ApiDeRegistroDtm.EsquemaTabla(typeof(TrabajoDeUsuarioDtm));
        public static void Definir(ModelBuilder mb)
        {
            mb.Entity<TrabajoDeUsuarioDtm>().Property(p => p.IdTrabajo).HasColumnName(ICampos.ID_TRABAJO).IsRequired(true).HasColumnType(IDominio.INT);
            mb.Entity<TrabajoDeUsuarioDtm>().Property(p => p.IdEjecutor).HasColumnName(ICampos.ID_EJECUTOR).IsRequired(true).HasColumnType(IDominio.INT);
            mb.Entity<TrabajoDeUsuarioDtm>().Property(p => p.IdSometedor).HasColumnName(ICampos.ID_SOMETEDOR).IsRequired(true).HasColumnType(IDominio.INT);
            mb.Entity<TrabajoDeUsuarioDtm>().Property(p => p.Encolado).HasColumnName(ICampos.ENTRADA).IsRequired(true).HasColumnType(IDominio.DATETIME_2);
            mb.Entity<TrabajoDeUsuarioDtm>().Property(p => p.Planificado).HasColumnName(ICampos.PLANIFICADO).IsRequired(true).HasColumnType(IDominio.DATETIME_2);
            mb.Entity<TrabajoDeUsuarioDtm>().Property(p => p.Iniciado).HasColumnName(ICampos.INICIADO).IsRequired(false).HasColumnType(IDominio.DATETIME_2);
            mb.Entity<TrabajoDeUsuarioDtm>().Property(p => p.Terminado).HasColumnName(ICampos.TERMINADO).IsRequired(false).HasColumnType(IDominio.DATETIME_2);
            mb.Entity<TrabajoDeUsuarioDtm>().Property(p => p.Estado).HasColumnName(ICampos.ESTADO).IsRequired(true).HasColumnType(IDominio.CHAR_2);
            mb.Entity<TrabajoDeUsuarioDtm>().Property(p => p.Parametros).HasColumnName(ICampos.PARAMETROS).IsRequired(false).HasColumnType(IDominio.VARCHAR_2000);
            mb.Entity<TrabajoDeUsuarioDtm>().Property(p => p.Periodicidad).HasColumnName(ICampos.PERIODICIDAD).IsRequired(true).HasColumnType(IDominio.INT);

            mb.Entity<TrabajoDeUsuarioDtm>().HasOne(x => x.Trabajo).WithMany().HasForeignKey(x=>x.IdTrabajo).HasConstraintName("FK_TRABAJO_DE_USUARIO_ID_TRABAJO").OnDelete(DeleteBehavior.Restrict);
            mb.Entity<TrabajoDeUsuarioDtm>().HasOne(x => x.Ejecutor).WithMany().HasForeignKey(x => x.IdEjecutor).HasConstraintName("FK_TRABAJO_DE_USUARIO_ID_EJECUTOR").OnDelete(DeleteBehavior.Restrict);
            mb.Entity<TrabajoDeUsuarioDtm>().HasOne(x => x.Sometedor).WithMany().HasForeignKey(x => x.IdSometedor).HasConstraintName("FK_TRABAJO_DE_USUARIO_ID_SOMETEDOR").OnDelete(DeleteBehavior.Restrict);
        }
    }
    public class TrabajoDeUsuarioDapper: IRegistro
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

    }

    public static class TrabajosDeUsuarioSql
    {
        public static string LeerTrabajoPendiente = $@"
          SELECT top(1) 
            T1.{ICampos.ID} as {nameof(TrabajoDeUsuarioDapper.Id)}
          , T2.{ICampos.NOMBRE} as {nameof(TrabajoDeUsuarioDapper.Nombre)}
          FROM {TablaTrabajoDeUsuario.Tabla} T1 with(nolock)
          INNER JOIN {TablaTrabajo.Tabla} T2 with(nolock) ON T2.{ICampos.ID} = T1.{ICampos.ID_TRABAJO}
          WHERE T1.{ICampos.ESTADO} = 'PT'
           AND (T1.{ICampos.INICIADO} = '0001-01-01T00:00:00.0000000' OR T1.{ICampos.INICIADO} IS NULL)
           and T1.{ICampos.ID} not in (select T1.{ICampos.ID_TRABAJO} FROM {TablaSemaforoDeTrabajos.Tabla} with(nolock))
           and T1.{ICampos.PLANIFICADO} < GETDATE()
           ORDER BY T1.{ICampos.PLANIFICADO}
          ";
    }
}
