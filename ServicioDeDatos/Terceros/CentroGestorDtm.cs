using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;
namespace ServicioDeDatos.Terceros
{
    public enum enumTipoPermiso
    {
        [Description("Gestor")]
        Gestor,
        [Description("Consultor")]
        Consultor
    }


    public interface ICamposCG
    {
        public int IdCg { get; set; }
        public abstract CentroGestorDtm CentroGestor { get; }
        public const string ID_CG = nameof(ID_CG);
        public const string ID_CG_PADRE = nameof(ID_CG_PADRE);
    }

    [Table(Tablas.CENTRO_GESTOR, Schema = Esquemas.TERCEROS)]
    public class CentroGestorDtm : ElementoDtm, IUsaBaja, IUsaArchivo, IPuedeUsarResponsable
    {
        public int IdSociedad { get; set; }
        public int? IdCgPadre { get; set; }
        public string Codigo { get; set; }
        public string eMail { get; set; }
        public int? IdResponsable { get; set; }
        public int? IdArchivo { get; set; }
        public bool Baja { get; set; }
        public string Sigla { get; set; }
        public int IdGestor { get; set; }
        public int IdConsultor { get; set; }
        public PermisoDtm Consultor { get; set; }
        public PermisoDtm Gestor { get; set; }
        public SociedadDtm Sociedad { get; set; }
        public CentroGestorDtm CgPadre { get; set; }
        public UsuarioDtm Responsable { get; set; }
        public ArchivoDtm Archivo { get; set; }
        public override string Expresion => $"({Codigo}) {base.Expresion}";
        public string NombreDelPermisoDe(enumModoDeAccesoDeDatos modo) => $"CG: ({modo.Nombre()}) {Expresion}";
    }

    [Table(Tablas.CENTRO_GESTOR + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.TERCEROS)]
    public class AuditoriaDeUnCentroGestorDtm : AuditoriaDtm
    {
    }

    public static partial class ModeloDeTerceros
    {
        internal static string TablaCgs => $"{ApiDeRegistroDtm.EsquemaDeTabla(typeof(CentroGestorDtm))}.{ApiDeRegistroDtm.NombreDeTabla(typeof(CentroGestorDtm))}";

        public static void CentroGestor(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<CentroGestorDtm>(modelBuilder);

            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(CentroGestorDtm));
            
            modelBuilder.Entity<CentroGestorDtm>().Property(p => p.IdSociedad).HasColumnName(ICampos.ID_SOCIEDAD).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<CentroGestorDtm>().Property(p => p.IdCgPadre).HasColumnName(ICamposCG.ID_CG_PADRE).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<CentroGestorDtm>().Property(p => p.Codigo).HasColumnName(ICampos.CODIGO).HasColumnType(IDominio.VARCHAR_50).IsRequired();
            modelBuilder.Entity<CentroGestorDtm>().Property(p => p.eMail).HasColumnName(ICampos.EMAIL).HasColumnType(IDominio.VARCHAR_50).IsRequired();
            modelBuilder.Entity<CentroGestorDtm>().Property(p => p.Sigla).HasColumnName(ICampos.SIGLA).HasColumnType(IDominio.VARCHAR_50).IsRequired(false);

            modelBuilder.Entity<CentroGestorDtm>().HasIndex(p => new { p.IdSociedad, p.Nombre }).HasDatabaseName($"I_{nombreDeTabla}_{ICampos.ID_SOCIEDAD}_{ICampos.NOMBRE}").IsUnique();
            modelBuilder.Entity<CentroGestorDtm>().HasIndex(p => new { p.IdSociedad, p.Codigo }).HasDatabaseName($"I_{nombreDeTabla}_{ICampos.ID_SOCIEDAD}_{ICampos.CODIGO}").IsUnique();

            ApiDeRegistroDtm.DefinirFk<CentroGestorDtm>(modelBuilder, nameof(CentroGestorDtm.Sociedad), nameof(CentroGestorDtm.IdSociedad), ICampos.ID_SOCIEDAD, unico: false);
            ApiDeRegistroDtm.DefinirFk<CentroGestorDtm>(modelBuilder, nameof(CentroGestorDtm.CgPadre), nameof(CentroGestorDtm.IdCgPadre), ICamposCG.ID_CG_PADRE, unico: false);
            ApiDeUsuarioDtm.DefinirResponsable<CentroGestorDtm>(modelBuilder, true);
            ApiDeElementoDtm.DefinirCampoArchivo<CentroGestorDtm>(modelBuilder);
        }

        public static void PermisosDeUnCg(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoFk<CentroGestorDtm>(modelBuilder, nameof(CentroGestorDtm.Consultor), nameof(CentroGestorDtm.IdConsultor), ICampos.ID_CONSULTOR, requerida: true, unico: true);
            ApiDeRegistroDtm.DefinirCampoFk<CentroGestorDtm>(modelBuilder, nameof(CentroGestorDtm.Gestor), nameof(CentroGestorDtm.IdGestor), ICampos.ID_GESTOR, requerida: true, unico: true);
        }

        public static void CentroGestorAudt(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnCentroGestorDtm>(modelBuilder);
        }

        internal static void DefinirCampoCG<TEntity>(ModelBuilder modelBuilder) where TEntity : RegistroDtm
        {
            modelBuilder.Entity<TEntity>().Property(p => ((ICamposCG)p).IdCg).HasColumnName(ICamposCG.ID_CG).HasColumnType(IDominio.INT).IsRequired();
        }
    }


    public class CentroGestorSql
    {
        public static class Parametros
        {
            public static string ID_USUARIO = nameof(ID_USUARIO);
            public static string ID_PUESTO = nameof(ID_PUESTO);
            public static string ID_ROL = nameof(ID_ROL);
            public static string NOMBRE = nameof(NOMBRE);
            public static string ID_SOCIEDAD = nameof(ID_SOCIEDAD);
            public static string ID_NEGOCIO = nameof(ID_NEGOCIO);
        }

        public static class Filtro
        {
            public static string PorRol = $"{nameof(PermisosDeUnRolDtm.IdRol)}".ToLower();
            public static string PorPuesto = $"{nameof(RolesDeUnPuestoDtm.IdPuesto)}".ToLower();
            public static string PorUsuario = $"{nameof(PuestosDeUnUsuarioDtm.IdUsuario)}".ToLower();
            public static string TiposNoActivos = $"{nameof(TipoDeElementoDtm.Activo)}".ToLower();
            public static string PorNegocio = $"{nameof(NegociosDeUnCgDtm.IdNegocio)}".ToLower();
            public static string PorSociedad = $"{nameof(CentroGestorDtm.IdSociedad)}".ToLower();
            public static string PorTipoPermiso = $"{nameof(enumTipoPermiso)}".ToLower();
        }

        private static string _cgsDeBaja = $@"AND T1.{ICampos.BAJA} = 1";

        private static string _filtroPorIdSociedad = $@"AND EXISTS (select top(1) 1 from {ModeloDeTerceros.TablaCgs}
                                                                     where  {ICampos.ID} = t1.{ICampos.ID} 
                                                                     and {ICampos.ID_SOCIEDAD} = @{Parametros.ID_SOCIEDAD})";

        private static string _filtroPorNombre = $"AND T1.{ICampos.NOMBRE} LIKE '%'+@{Parametros.NOMBRE}+'%'";

        private static string _Consultor = $"tr.{ICampos.IDPERMISO} = NG.{ICampos.ID_CONSULTOR}";
        private static string _Gestor = $"tr.{ICampos.IDPERMISO} = NG.{ICampos.ID_GESTOR}";

        private static string _filtroPorUsuario = $@"
             AND EXISTS(SELECT top(1) 1 
                    FROM {ModeloDeSeguridad.TablaPtsDeUnUsuario}
                    WHERE EXISTS (SELECT top(1) 1 
                                   FROM {ModeloDeSeguridad.TablaRolesDeUnPt} trp
                                   WHERE EXISTS ((SELECT top(1) 1 
                                                  FROM {ModeloDeSeguridad.TablaPermisosDeUnRol} tr
                                                  WHERE [{Filtro.PorTipoPermiso}]
                                                   AND tr.{ICampos.IDROL} = trp.{ICampos.IDROL})
                                    )
                                    AND {ICampos.IDPUESTO} = trp.{ICampos.IDPUESTO})
                      AND IDUSUA = @{Parametros.ID_USUARIO}
                      )
             ";

        private static string _filtroPorPt = $@"
             AND EXISTS(SELECT top(1) 1 
                         FROM {ModeloDeSeguridad.TablaRolesDeUnPt} trp
                         WHERE EXISTS ((SELECT top(1) 1 
                                        FROM {ModeloDeSeguridad.TablaPermisosDeUnRol} tr
                                        WHERE (tr.{ICampos.IDPERMISO} = NG.{ICampos.ID_CONSULTOR} or tr.IDPERMISO = NG.{ICampos.ID_GESTOR} ) 
                                         AND tr.{ICampos.IDROL} = trp.{ICampos.IDROL})
                          )
                          AND trp.{ICampos.IDPUESTO} = @{Parametros.ID_PUESTO}
                      )
             ";

        private static string _filtroPorRol = $@"
             AND EXISTS(SELECT top(1) 1 
                        FROM {ModeloDeSeguridad.TablaPermisosDeUnRol} tr
                        WHERE (tr.{ICampos.IDPERMISO} = NG.{ICampos.ID_CONSULTOR} or tr.IDPERMISO = NG.{ICampos.ID_GESTOR} ) 
                        AND tr.{ICampos.IDROL} =  @{Parametros.ID_ROL}
                       )
             ";

        private static string _filtroPorNegocio = $@"AND NG.{ICampos.ID_NEGOCIO} = @{Parametros.ID_NEGOCIO}";

        private static string AplicarFiltros(string sentenciaSql, int idSociedad, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            sentenciaSql = filtrarPorSociedad(sentenciaSql, idSociedad, parametrosSql);

            sentenciaSql = filtrarPorNombre(sentenciaSql, filtros, parametrosSql);
            sentenciaSql = filtrarPorUsuario(sentenciaSql, filtros, parametrosSql);
            sentenciaSql = filtrarPorPt(sentenciaSql, filtros, parametrosSql);
            sentenciaSql = filtrarPorRol(sentenciaSql, filtros, parametrosSql);
            sentenciaSql = filtrarPorNegocioAccedido(sentenciaSql, filtros, parametrosSql);

            sentenciaSql = filtrarPorCgsDeBaja(sentenciaSql, filtros);


            return sentenciaSql;
        }

        private static string filtrarPorNegocioAccedido(string sentenciaSql, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            if (!filtros.ContainsKey(Filtro.PorNegocio) || filtros[Filtro.PorNegocio].ToString().Entero() <= 0)
                return sentenciaSql.Replace($"[{nameof(_filtroPorNegocio)}]", "");

            sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorNegocio)}]", _filtroPorNegocio);
            parametrosSql.Add($"@{Parametros.ID_NEGOCIO}", filtros[Filtro.PorNegocio]);
            return sentenciaSql;
        }

        private static string filtrarPorRol(string sentenciaSql, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            if (!filtros.ContainsKey(Filtro.PorRol))
                return sentenciaSql.Replace($"[{nameof(_filtroPorRol)}]", "");


            sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorRol)}]", _filtroPorRol);
            parametrosSql.Add($"@{Parametros.ID_ROL}", filtros[Filtro.PorRol]);
            return sentenciaSql;
        }

        private static string filtrarPorPt(string sentenciaSql, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            if (!filtros.ContainsKey(Filtro.PorPuesto))
                return sentenciaSql.Replace($"[{nameof(_filtroPorPt)}]", "");


            sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorPt)}]", _filtroPorPt);
            parametrosSql.Add($"@{Parametros.ID_PUESTO}", filtros[Filtro.PorPuesto]);
            return sentenciaSql;
        }

        private static string filtrarPorUsuario(string sentenciaSql, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            if (!filtros.ContainsKey(Filtro.PorUsuario))
                return sentenciaSql.Replace($"[{nameof(_filtroPorUsuario)}]", "");


            string tipoPermiso = $"({_Consultor} or {_Gestor})";

            if (filtros[Filtro.PorTipoPermiso].ToString().Equals(enumTipoPermiso.Gestor.ToString()))
                tipoPermiso = $"{_Gestor}";
            if (filtros[Filtro.PorTipoPermiso].ToString().Equals(enumTipoPermiso.Consultor.ToString()))
                tipoPermiso = $"{_Consultor}";

            var filtro = _filtroPorUsuario.Replace($"[{Filtro.PorTipoPermiso}]", tipoPermiso);
            sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorUsuario)}]", filtro);
            parametrosSql.Add($"@{Parametros.ID_USUARIO}", filtros[Filtro.PorUsuario]);
            return sentenciaSql;
        }

        private static string filtrarPorSociedad(string sentenciaSql, int idSociedad, Dictionary<string, object> parametrosSql)
        {
            sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorIdSociedad)}]", $"{_filtroPorIdSociedad}");
            parametrosSql.Add($"@{Parametros.ID_SOCIEDAD}", idSociedad);
            return sentenciaSql;
        }

        private static string filtrarPorNombre(string sentenciaSql, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorNombre)}]", !filtros[nameof(INombre.Nombre).ToLower()].ToString().IsNullOrEmpty() ? $"{_filtroPorNombre}" : "");
            if (!filtros[nameof(INombre.Nombre).ToLower()].ToString().IsNullOrEmpty())
                parametrosSql.Add($"@{Parametros.NOMBRE}", filtros[nameof(INombre.Nombre).ToLower()].ToString());
            return sentenciaSql;
        }

        private static string filtrarPorCgsDeBaja(string sentenciaSql, Dictionary<string, object> filtros)
        {
            var filtro = Filtro.TiposNoActivos.ToLower();
            if ((bool)filtros[filtro])
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_cgsDeBaja)}]", $"{_cgsDeBaja}");
            else
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_cgsDeBaja)}]", $"");
            return sentenciaSql;
        }

        public static List<NodoDtm> LeerJerarquiaDeCgsPorSociedad(ContextoSe contexto, int idSociedad, Dictionary<string, object> filtros)
        {
            var JerarquiaDeCgsPorSociedad = $@"
                              WITH CGS 
                              AS
                              (
                                  SELECT T1.{ICampos.ID} AS id
                              		   ,T1.{ICampos.BAJA}                  
                              		   ,T1.{ICampos.NOMBRE}                  
                              		   ,T1.{ICamposCG.ID_CG_PADRE}                    
                                       ,T1.{ICampos.CODIGO}   
                                  FROM  {ModeloDeTerceros.TablaCgs} T1 WITH(NOLOCK)
                                  UNION ALL
                                  --RECURSIVIDAD
                                  SELECT T2.{ICampos.ID} AS id
                              		   ,T2.{ICampos.BAJA}                     
                              		   ,T2.{ICampos.NOMBRE}                   
                              		   ,T2.{ICamposCG.ID_CG_PADRE}                   
                                       ,T2.{ICampos.CODIGO}   
                                  FROM  {ModeloDeTerceros.TablaCgs} AS T2 WITH(NOLOCK)
                              	JOIN CGS AS TP ON T2.{ICamposCG.ID_CG_PADRE} = TP.id
                              )    
                              SELECT DISTINCT
                                    T2.{ICampos.NOMBRE}                   AS Padre
                                   ,T1.{ICampos.ID}                       AS Id
                              	   ,case when T1.{ICampos.BAJA} = 1                
                                         then 0                          
                                         else 1 end                      AS Activo
                              	   ,'(' + T1.{ICampos.CODIGO}
                                   + ') ' + T1.{ICampos.NOMBRE}   
                                                                         AS {nameof(CentroGestorDtm.Nombre)}
								   ,T1.{ICamposCG.ID_CG_PADRE}           AS IdPadre
                              	   ,'{typeof(CentroGestorDtm).FullName}' AS TipoDtm
                              FROM CGS T1
                              LEFT JOIN {ModeloDeTerceros.TablaCgs} T2 WITH(NOLOCK) ON T2.ID = T1.{ICamposCG.ID_CG_PADRE}
                              LEFT JOIN {ModeloDeTerceros.TablaNegociosDeUnCg} NG WITH(NOLOCK) ON NG.{ICamposCG.ID_CG} = T1.{ICampos.ID}
                              WHERE 1=1 
                              [{nameof(_cgsDeBaja)}]
                              [{nameof(_filtroPorIdSociedad)}]
                              [{nameof(_filtroPorNombre)}]
                              [{nameof(_filtroPorUsuario)}]
                              [{nameof(_filtroPorPt)}]
                              [{nameof(_filtroPorRol)}]
                              [{nameof(_filtroPorNegocio)}]
							  order by Padre, {nameof(CentroGestorDtm.Nombre)}
                              ";

            var parametrosSql = new Dictionary<string, object>();
            var sentenciaSql = AplicarFiltros(JerarquiaDeCgsPorSociedad, idSociedad, filtros, parametrosSql);

            var consulta = new ConsultaSql<NodoDtm>(contexto, sentenciaSql);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
            return registros;
        }

        public static CentroGestorDtm LeerCgPorId(ContextoSe contexto, int id)
        {
            var _leerPorId = $@"
                     select {ICampos.ID} as {nameof(CentroGestorDtm.Id)}
                          , {ICampos.NOMBRE} as {nameof(CentroGestorDtm.Nombre)}
                          , {ICampos.ID_SOCIEDAD} as {nameof(CentroGestorDtm.IdSociedad)}
                          , {ICamposCG.ID_CG_PADRE} as {nameof(CentroGestorDtm.IdCgPadre)}
                          , {ICampos.SIGLA} as {nameof(CentroGestorDtm.Sigla)}
                          , {ICampos.CODIGO} as {nameof(CentroGestorDtm.Codigo)}
                          , {ICampos.EMAIL} as {nameof(CentroGestorDtm.eMail)}
                          , '(' + {ICampos.CODIGO} + ')' + {ICampos.NOMBRE} as {nameof(CentroGestorDtm.Expresion)}
                          , {ICampos.ID_ARCHIVO} as {nameof(CentroGestorDtm.IdArchivo)}
                          , {ICampos.ID_RESPONSABLE} as {nameof(CentroGestorDtm.IdResponsable)}
                          , {ICampos.ID_CREADOR} as {nameof(CentroGestorDtm.IdUsuaCrea)}
                          , {ICampos.ID_MODIFICADOR} as {nameof(CentroGestorDtm.IdUsuaModi)}
                          , {ICampos.FECCRE} as {nameof(CentroGestorDtm.FechaCreacion)}
                          , {ICampos.FECMOD} as {nameof(CentroGestorDtm.FechaModificacion)}
                          , {ICampos.BAJA} as {nameof(CentroGestorDtm.Baja)}
                     from {ModeloDeTerceros.TablaCgs}
                     where {ICampos.ID} = @{ICampos.ID}
                     ";

            var cache = ServicioDeCaches.Obtener(nameof(LeerCgPorId));
            if (!cache.ContainsKey(id.ToString()))
            {
                var parametrosSql = new Dictionary<string, object>();
                parametrosSql.Add($"@{ICampos.ID}", id);

                var consulta = new ConsultaSql<CentroGestorDtm>(contexto, _leerPorId);
                cache[id.ToString()] = consulta.LanzarConsulta(new DynamicParameters(parametrosSql))[0];
            }
            return (CentroGestorDtm)cache[id.ToString()];
        }

        public static List<RegistroDtm> CentrosGestoresDependientes(ContextoSe contexto, int idCg)
        {
            string _leer = $@"
            WITH centrosGestores ({ICampos.ID})
            AS
            (
                SELECT {ICampos.ID}
                FROM TERCEROS.CENTRO_GESTOR
                WHERE {ICampos.ID}  = @{ICampos.ID}
                UNION ALL
                --RECURSIVIDAD
                SELECT e.{ICampos.ID}
                FROM TERCEROS.CENTRO_GESTOR AS e 
                JOIN centrosGestores AS m ON e.{ICamposCG.ID_CG_PADRE} = m.{ICampos.ID}
                )
            SELECT {ICampos.ID} as {nameof(RegistroDtm.Id)} 
            FROM centrosGestores
            where {ICampos.ID} <> @{ICampos.ID}
            ";

            var cache = ServicioDeCaches.Obtener(nameof(CentrosGestoresDependientes));
            if (!cache.ContainsKey(idCg.ToString()))
            {
                var parametrosSql = new Dictionary<string, object>();
                parametrosSql.Add($"@{ICampos.ID}", idCg);
                var sentencia = new ConsultaSql<RegistroDtm>(contexto, _leer);
                cache[idCg.ToString()] = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql));
            }
            return (List<RegistroDtm>)cache[idCg.ToString()];
        }

    }
}
