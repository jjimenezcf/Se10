using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ServicioDeDatos.Entorno
{
    [Table("MENU", Schema = "ENTORNO")]
    public class MenuDtm : RegistroConNombreDtm
    {
        public string Descripcion { get; set; }

        public string Icono { get; set; }

        public int Orden { get; set; }

        public bool Activo { get; set; }

        public int? IdPadre { get; set; }

        public MenuDtm Padre { get; set; }

        public List<MenuDtm> Submenus { get; set; }

        public int? IdVistaMvc { get; set; }

        public VistaMvcDtm VistaMvc { get; set; }
        public string Parametros { get; set; }
    }

    public static partial class ModeloDeEntorno
    {
        internal static string TablaMenus => $"{ApiDeRegistroDtm.EsquemaDeTabla(typeof(MenuDtm))}.{ApiDeRegistroDtm.NombreDeTabla(typeof(MenuDtm))}";

        public static void Menus(ModelBuilder modelBuilder)
        {
            ApiDeNombreDtm.DefinirCampoNombreDtm<MenuDtm>(modelBuilder,250,"",true, conIndice: false);
            ApiDeElementoDtm.DefinirCampoDescripcion<MenuDtm>(modelBuilder);

            modelBuilder.Entity<MenuDtm>().Property(menu => menu.IdPadre).HasColumnName(ICampos.IDPADRE).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<MenuDtm>().Property(menu => menu.IdVistaMvc).HasColumnName(ICampos.IDVISTA_MVC).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<MenuDtm>().Property(menu => menu.Icono).HasColumnName(ICampos.ICONO).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<MenuDtm>().Property(menu => menu.Orden).HasColumnName(ICampos.ORDEN).HasColumnType(IDominio.INT).IsRequired(true).HasDefaultValue(0);
            modelBuilder.Entity<MenuDtm>().Property(menu => menu.Activo).HasColumnName(ICampos.ACTIVO).HasColumnType(IDominio.BIT).IsRequired(true);
            modelBuilder.Entity<MenuDtm>().Property(menu => menu.Parametros).HasColumnName(ICampos.PARAMETROS).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);

            modelBuilder.Entity<MenuDtm>()
                .HasOne(menu => menu.Padre)
                .WithMany()
                .IsRequired(false)
                .HasForeignKey(menu => menu.IdPadre)
                .HasConstraintName($"FK_{ApiDeRegistroDtm.NombreDeTabla(typeof(MenuDtm))}_{ICampos.IDPADRE}");

            modelBuilder.Entity<MenuDtm>()
                        .HasOne(menu => menu.VistaMvc)
                        .WithMany(vista => vista.Menus)
                        .IsRequired(false)
                        .HasForeignKey(menu => menu.IdVistaMvc)
                        .HasConstraintName($"FK_{ApiDeRegistroDtm.NombreDeTabla(typeof(MenuDtm))}_{ICampos.IDVISTA_MVC}");

            modelBuilder.Entity<MenuDtm>()
                .HasMany(menu => menu.Submenus)
                .WithOne(m => m.Padre)
                .IsRequired(false);

            modelBuilder.Entity<MenuDtm>().HasIndex(x => new { x.IdPadre, x.Nombre }).IsUnique().HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(MenuDtm))}_{ICampos.ID_PADRE}_{ICampos.NOMBRE}");
        }
    }

    public static class MenuSql
    {

        public static class Parametros
        {
            public static string ID_USUARIO = ICampos.IDUSUA;
            public static string ID_PUESTO = ICampos.IDPUESTO;
            public static string ID_ROL = ICampos.IDROL;
            public static string ID_VISTA = ICampos.IDVISTA_MVC;
            public static string NOMBRE =ICampos.NOMBRE;
        }
        public static class Filtro
        {
            public static string PorRol = $"{nameof(PermisosDeUnRolDtm.IdRol)}".ToLower();
            public static string PorPuesto = $"{nameof(RolesDeUnPuestoDtm.IdPuesto)}".ToLower();
            public static string PorUsuario = $"{nameof(PuestosDeUnUsuarioDtm.IdUsuario)}".ToLower();
            public static string NoActivos = $"{nameof(MenuDtm.Activo)}".ToLower();
            public static string PorVista = $"{nameof(MenuDtm.IdVistaMvc)}".ToLower();
            public static string PorNombre = $"{nameof(MenuDtm.Nombre)}".ToLower();
        }
        private static string _noActivo = $@"AND T1.{ICampos.ACTIVO} = 0";
        private static string _filtroPorNombre = $"AND (T1.{ICampos.NOMBRE} LIKE '%'+@{Parametros.NOMBRE}+'%' or T2.{ICampos.NOMBRE} LIKE '%'+@{Parametros.NOMBRE}+'%')";
        private static string _filtroPorUsuario = $@"
        and exists (
                select top(1) 1 
                from {ModeloDeEntorno.TablaVistasMvc} v
                where exists (
                      select top (1) 1 from {ModeloDeSeguridad.TablaDePermiso} p
                      where p.id = v.{ICampos.IDPERMISO}
                      and Exists (
                            select top(1) 1 from {ModeloDeSeguridad.TablaPermisosDeUnRol} pr
                            where pr.{ICampos.IDPERMISO} = p.{ICampos.ID}
                      	    and Exists (
                      	         select top(1) 1 from {ModeloDeSeguridad.TablaRolesDeUnPt} rp
                      		     where rp.{ICampos.IDROL} = pr.{ICampos.IDROL}
                      		     and exists (
                      		          select top(1) 1 from {ModeloDeSeguridad.TablaPtsDeUnUsuario} up
                      		          where up.{ICampos.IDPUESTO} = rp.{ICampos.IDPUESTO}
                      		          and up.{ICampos.IDUSUA} = @{Parametros.ID_USUARIO}
                      	         )
                            )
                       )
                    )
                and v.id = t1.{ICampos.IDVISTA_MVC} 
            )
        ";
        private static string _filtroPorPuestoDeTrabajo = $@"
        and exists (
                select top(1) 1 
                from {ModeloDeEntorno.TablaVistasMvc} v
                where exists (
                      select top (1) 1 from {ModeloDeSeguridad.TablaDePermiso} p
                      where p.id = v.{ICampos.IDPERMISO}
                      and Exists (
                            select top(1) 1 from {ModeloDeSeguridad.TablaPermisosDeUnRol} pr
                            where pr.{ICampos.IDPERMISO} = p.{ICampos.ID}
                      	    and Exists (
                      	         select top(1) 1 from {ModeloDeSeguridad.TablaRolesDeUnPt} rp
                      		     where rp.{ICampos.IDROL} = pr.{ICampos.IDROL}
                      		     and rp.{ICampos.IDPUESTO} = @{Parametros.ID_PUESTO}
                            )
                       )
                    )
                and v.id = t1.{ICampos.IDVISTA_MVC} 
            )
        ";
        private static string _filtroPorRol = $@"
        and exists (
                select top(1) 1 
                from {ModeloDeEntorno.TablaVistasMvc} v
                where exists (
                      select top (1) 1 from {ModeloDeSeguridad.TablaDePermiso} p
                      where p.id = v.{ICampos.IDPERMISO}
                      and Exists (
                            select top(1) 1 from {ModeloDeSeguridad.TablaPermisosDeUnRol} pr
                            where pr.{ICampos.IDPERMISO} = p.{ICampos.ID}
                      	    and pr.{ICampos.IDROL}= @{Parametros.ID_ROL}
                       )
                    )
                and v.id = t1.IDVISTA_MVC 
            )
        ";
        private static string _filtroPorVista = $@"and t1.{ICampos.IDVISTA_MVC} = @{Parametros.ID_VISTA}";

        public static string JerarquiaDeMenus = $@"
                                WITH MENUS 
                                AS
                                (
                                    SELECT  T1.{ICampos.ID} AS {ICampos.ID}
		                                   ,T1.{ICampos.ACTIVO}                  
		                                   ,T1.{ICampos.NOMBRE}                 
		                                   ,T1.{ICampos.IDPADRE}                  
		                                   ,T1.{ICampos.ORDEN}                     
		                                   ,T1.{ICampos.PARAMETROS}             
                                    FROM   {ModeloDeEntorno.TablaMenus}  T1 WITH(NOLOCK)       
                                    -- Aquí he de meter los filtros de permisos
                                    where 1=1 
                                    [{nameof(_filtroPorUsuario)}]
                                    [{nameof(_filtroPorPuestoDeTrabajo)}]
                                    [{nameof(_filtroPorRol)}]
                                    [{nameof(_filtroPorVista)}]
                                    UNION ALL
                                    --RECURSIVIDAD
                                    SELECT  T2.{ICampos.ID} AS {ICampos.ID}
		                                   ,T2.{ICampos.ACTIVO}              
		                                   ,T2.{ICampos.NOMBRE}                    
		                                   ,T2.{ICampos.IDPADRE}                  
		                                   ,T2.{ICampos.ORDEN}                   
		                                   ,T2.{ICampos.PARAMETROS}                          
                                    FROM  {ModeloDeEntorno.TablaMenus} AS T2 WITH(NOLOCK)
	                                JOIN MENUS AS TP ON [restriccionConPermisos]
                                    -- si no se filtra por permisos --> T2.{ICampos.IDPADRE} = TP.{ICampos.ID}
                                    -- si hay filtros de permisos   --> T2.{ICampos.ID} = TP.{ICampos.IDPADRE}
                                )
                                SELECT DISTINCT
                                        T2.{ICampos.NOMBRE}              AS {nameof(MenuDtm.Padre)} 
                                       ,T2.{ICampos.ORDEN}               AS OrdenPadre 
                                       ,T1.{ICampos.ORDEN}               AS {nameof(MenuDtm.Orden)} 
                                       ,T1.{ICampos.ID}                  AS {nameof(MenuDtm.Id)} 
	                                   ,T1.{ICampos.ACTIVO}              AS {nameof(MenuDtm.Activo)} 
	                                   ,[Padre]T1.{ICampos.NOMBRE}       AS {nameof(MenuDtm.Nombre)} 
	                                   ,T1.{ICampos.IDPADRE}             AS {nameof(MenuDtm.IdPadre)} 
	                                   ,T1.{ICampos.PARAMETROS}          AS {nameof(MenuDtm.Parametros)}
                              	       ,'{typeof(MenuDtm).FullName}'     AS TipoDtm
                                FROM MENUS T1
                                LEFT JOIN {ModeloDeEntorno.TablaMenus} T2 WITH(NOLOCK) ON T2.ID = T1.{ICampos.IDPADRE}
                                WHERE 1=1 
                                [{nameof(_noActivo)}]
                                [{nameof(_filtroPorNombre)}]
                                order by T2.{ICampos.ORDEN}, T1.{ICampos.ORDEN}
                                ";

        public static bool HayFiltrosConSoloPermisos(Dictionary<string, object> filtros)
        {
            if (_HayFiltrosConPermisos(filtros))
            {
                return !(bool)filtros[Filtro.NoActivos] && ((string)filtros[Filtro.PorNombre]).IsNullOrEmpty();
            }
            return false;
        }

        private static bool _HayFiltrosConPermisos(Dictionary<string, object> filtros)
        {
            return filtros.ContainsKey(Filtro.PorUsuario) || filtros.ContainsKey(Filtro.PorPuesto) || filtros.ContainsKey(Filtro.PorRol) || filtros.ContainsKey(Filtro.PorVista);
        }

        public static string AplicarFiltros(string sentenciaSql, int? idPadre, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            sentenciaSql = filtrarPorNombre(sentenciaSql, filtros, parametrosSql);
            sentenciaSql = filtrarPorUsuario(sentenciaSql, filtros, parametrosSql);
            sentenciaSql = filtrarPorPt(sentenciaSql, filtros, parametrosSql);
            sentenciaSql = filtrarPorRol(sentenciaSql, filtros, parametrosSql);
            sentenciaSql = filtrarPorVista(sentenciaSql, filtros, parametrosSql);
            sentenciaSql = filtrarPorNoActivo(sentenciaSql, filtros);
            sentenciaSql = sentenciaSql.Replace("[restriccionConPermisos]", _HayFiltrosConPermisos(filtros) 
                ? $"T2.{ICampos.ID} = TP.{ICampos.IDPADRE}" 
                : $"T2.{ICampos.IDPADRE} = TP.{ICampos.ID}");

            return sentenciaSql;
        }

        private static string filtrarPorNombre(string sentenciaSql, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorNombre)}]", !filtros[Filtro.PorNombre].ToString().IsNullOrEmpty() ? $"{_filtroPorNombre}" : "");
            if (!filtros[Filtro.PorNombre].ToString().IsNullOrEmpty())
                parametrosSql.Add($"@{Parametros.NOMBRE}", filtros[Filtro.PorNombre].ToString());
            return sentenciaSql;
        }

        private static string filtrarPorUsuario(string sentenciaSql, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            if (filtros.ContainsKey(Filtro.PorUsuario))
            {
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorUsuario)}]", $"{_filtroPorUsuario}");                
                parametrosSql.Add($"@{Parametros.ID_USUARIO}", (int)filtros[Filtro.PorUsuario]);
            }
            else
            {
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorUsuario)}]", "");
            }
            return sentenciaSql;
        }

        private static string filtrarPorPt(string sentenciaSql, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            if (filtros.ContainsKey(Filtro.PorPuesto))
            {
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorPuestoDeTrabajo)}]", $"{_filtroPorPuestoDeTrabajo}");
                parametrosSql.Add($"@{Parametros.ID_PUESTO}", (int)filtros[Filtro.PorPuesto]);
            }
            else
            {
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorPuestoDeTrabajo)}]", "");
            }
            return sentenciaSql;
        }

        private static string filtrarPorRol(string sentenciaSql, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            if (filtros.ContainsKey(Filtro.PorRol))
            {
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorRol)}]", $"{_filtroPorRol}");
                parametrosSql.Add($"@{Parametros.ID_ROL}",(int) filtros[Filtro.PorRol]);
            }
            else
            {
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorRol)}]", "");
            }
            return sentenciaSql;
        }

        private static string filtrarPorVista(string sentenciaSql, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            if (filtros.ContainsKey(Filtro.PorVista))
            {
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorVista)}]", $"{_filtroPorVista}");
                parametrosSql.Add($"@{Parametros.ID_VISTA}", (int) filtros[Filtro.PorVista]);
            }
            else
            {
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorVista)}]", "");
            }
            return sentenciaSql;
        }


        private static string filtrarPorNoActivo(string sentenciaSql, Dictionary<string, object> filtros)
        {
            var filtro = Filtro.NoActivos.ToLower();
            if (filtros.ContainsKey(filtro) && (bool)filtros[filtro])
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_noActivo)}]", $"{_noActivo}");
            else
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_noActivo)}]", $"");
            return sentenciaSql;
        }

    }
}
