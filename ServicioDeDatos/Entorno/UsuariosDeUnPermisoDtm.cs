using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
namespace ServicioDeDatos.Entorno
{

    public class UsuariosDeUnPermisoDtm : RegistroDtm
    {
        public int IdUsuario { get; set; }

        public int IdPermiso { get; set; }

        public string Origen { get; set; }

        public virtual UsuarioDtm Usuario { get; set; }

        public virtual PermisoDtm Permiso { get; set; }
    }

    public static partial class ModeloDeEntorno
    {
        public static void CrearVistaDePermisosPorUsuario(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsuariosDeUnPermisoDtm>()
                .ToView(Vistas.USUARIO_PERMISO, Esquemas.ENTORNO)
                .HasKey(x => new { x.Id });

            modelBuilder.Entity<UsuariosDeUnPermisoDtm>().Property(p => p.Id).HasColumnName(ICampos.ID)
            .HasColumnType(IDominio.INT)
            .HasComputedColumnSql($"CAST(ROW_NUMBER() OVER(ORDER BY t2.{ICampos.IDUSUA} ASC) as {IDominio.INT})");

            modelBuilder.Entity<UsuariosDeUnPermisoDtm>().Property(nameof(UsuariosDeUnPermisoDtm.IdUsuario)).HasColumnName(ICampos.IDUSUA).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<UsuariosDeUnPermisoDtm>().Property(nameof(UsuariosDeUnPermisoDtm.IdPermiso)).HasColumnName(ICampos.IDPERMISO).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<UsuariosDeUnPermisoDtm>().Property(p => p.Origen).HasColumnName(ICampos.ORIGEN).HasColumnType(IDominio.VARCHAR_MAX).HasComputedColumnSql($"SEGURIDAD.OBTENER_ORIGEN({ICampos.IDUSUA},{ICampos.IDPERMISO})");

            modelBuilder.Entity<UsuariosDeUnPermisoDtm>()
                .HasOne(x => x.Usuario)
                .WithMany(x => x.Permisos)
                .HasForeignKey(x => x.IdUsuario);

            modelBuilder.Entity<UsuariosDeUnPermisoDtm>()
                .HasOne(x => x.Permiso)
                .WithMany(x => x.Usuarios)
                .HasForeignKey(x => x.IdUsuario);
        }

        public static void ModificarVistaDePermisos(this MigrationBuilder migration)
        {
            migration.Sql($@"
               CREATE OR ALTER   VIEW[ENTORNO].[USUARIO_PERMISO]
                 AS  
                 select CAST(ROW_NUMBER() OVER(ORDER BY IDUSUA ASC) as int) AS ID, IDUSUA, IDPERMISO, SEGURIDAD.OBTENER_ORIGEN (IDUSUA, IDPERMISO) as ORIGEN
                 from (
                     select  t2.IDUSUA as IDUSUA, t4.IDPERMISO as IDPERMISO 
                     from SEGURIDAD.USU_PUESTO t2
                     inner join SEGURIDAD.ROL_PUESTO T3 ON T3.IDPUESTO = T2.IDPUESTO
                     INNER JOIN SEGURIDAD.ROL_PERMISO T4 ON T4.IDROL = T3.IDROL
                  
                     union all
                    
                     select t2.IDUSUA as IDUSUA, t1.IDPERMISO as IDPERMISO
                     from SEGURIDAD.USU_PUESTO t2
                     inner join SEGURIDAD.PERMISO_DIRECTOS t1 on t1.IDPUESTO = t2.IDPUESTO
                    
                     union all
                    
                     select T2.ID_RESPONSABLE, t1.ID_INTERVENTOR
                     from entorno.AGENDA t1
                     inner join JURIDICO.CONTRATO T2 ON T2.ID_AGENDA = T1.ID
                     
                     union all
                     
                     select t2.ID, t1.ID_CONSULTOR
                     from entorno.AGENDA t1
                     inner join ENTORNO.USUARIO t2 on t2.ID_AGENDA = t1.ID
                    
                     union all
                    
                     select t2.ID, t1.ID_GESTOR
                     from entorno.CERTIFICADO t1
                     inner join ENTORNO.USUARIO t2 on t2.ID_CERTIFICADO = t1.ID
                     
                 
                  ) Permisos
                  group by IDUSUA, IDPERMISO
               GO
               ");
        }

        public static void ModificarVistaDePermisosTrasPonerAgendaAlContrato(this MigrationBuilder migration)
        {
            migration.Sql($@"
               CREATE OR ALTER   VIEW[ENTORNO].[USUARIO_PERMISO]
                 AS  
                 select CAST(ROW_NUMBER() OVER(ORDER BY IDUSUA ASC) as int) AS ID, IDUSUA, IDPERMISO, SEGURIDAD.OBTENER_ORIGEN (IDUSUA, IDPERMISO) as ORIGEN
                 from (
                     select  t2.IDUSUA as IDUSUA, t4.IDPERMISO as IDPERMISO 
                     from SEGURIDAD.USU_PUESTO t2
                     inner join SEGURIDAD.ROL_PUESTO T3 ON T3.IDPUESTO = T2.IDPUESTO
                     INNER JOIN SEGURIDAD.ROL_PERMISO T4 ON T4.IDROL = T3.IDROL
                  
                  union all
                  
                   select t2.IDUSUA as IDUSUA, t1.IDPERMISO as IDPERMISO
                   from SEGURIDAD.USU_PUESTO t2
                   inner join SEGURIDAD.PERMISO_DIRECTOS t1 on t1.IDPUESTO = t2.IDPUESTO
               
                   union all
                 
                   select t2.ID, t1.ID_GESTOR
                   from entorno.AGENDA t1
                   inner join ENTORNO.USUARIO t2 on t2.ID_AGENDA = t1.ID
                   
                   union all
                 
                   select T2.ID_RESPONSABLE, t1.ID_INTERVENTOR
                   from entorno.AGENDA t1
                   inner join JURIDICO.CONTRATO T2 ON T2.ID_AGENDA = T1.ID
                   
                   union all
                   
                   select t2.ID, t1.ID_CONSULTOR
                   from entorno.AGENDA t1
                   inner join ENTORNO.USUARIO t2 on t2.ID_AGENDA = t1.ID
               
                   union all
                 
                   select t2.ID, t1.ID_GESTOR
                   from entorno.CERTIFICADO t1
                   inner join ENTORNO.USUARIO t2 on t2.ID_CERTIFICADO = t1.ID
                   
                 
                  ) Permisos
                  group by IDUSUA, IDPERMISO
               GO
               ");
        }
    }
}
