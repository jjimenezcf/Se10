using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ServicioDeDatos.Elemento
{
    public enum enumClaseDeElementoPorTipo
    {
        [Description("Sin clase")]
        Sin_Clase
    }
    

    public enum enumClaseDeLibro
    {
        //[Description("Sin libro")]
        //SIN_LIBRO,
        [Description("Por tipo")]
        POR_TIPO,
        [Description("Por centro de gestión")]
        POR_CG,
        [Description("Por centro de gestión y tipo")]
        POR_CG_TIPO
    }

    public class TipoDeElementoDtm : RegistroConNombreDtm,  ITipoDeElementoDtm
    {

        public int? IdPadre { get; set; }

        public int IdPermisoDeGestor { get; set; }
        public int IdPermisoDeConsultor { get; set; }
        public int IdPermisoDeAdministrador { get; set; }


        public bool Activo { get; set; }
        public string TipoDtm { get; set; }
        public string TipoDto { get; set; }

        public enumClaseDeLibro ClaseDeLibro { get; set; }
        public string Sigla { get; set; }
        
        public string Mascara { get; set; }
        public string Marcador { get; set; }
        public bool NombreModificable { get; set; }
        public bool PermiteCrear { get; set; }
        public bool EditarTrasCrear { get; set; }

        public PermisoDtm PermisoDeGestor { get; set; }
        public PermisoDtm PermisoDeConsultor { get; set; }
        public PermisoDtm PermisoDeAdministrador { get; set; }
        public static enumNegocio Negocio => enumNegocio.No_Definido;

        public static bool EsSinClase(System.Enum clase)
        {
            var valor = ApiDeEnsamblados.ToEnumerado<enumClaseDeElementoPorTipo>(clase.ToString(), errorSiNoEsValido: false);
            return valor != null && valor.Value == enumClaseDeElementoPorTipo.Sin_Clase;
        }
    }

    public class TipoConFlujoDtm : TipoDeElementoDtm, IUsaEstado
    {
        public int IdEstado { get ; set ; }

        public virtual IEstado iEstado { get; }

        public IEstado Estado => iEstado;
    }

    public class TipoDeElementoSql
    {
        public static class Parametros
        {
            public static string ID_USUARIO = nameof(ID_USUARIO);
            public static string ID_PUESTO = nameof(ID_PUESTO);
            public static string ID_ROL = nameof(ID_ROL);
            public static string NOMBRE = nameof(NOMBRE);
            public static string ID_PADRE = nameof(ID_PADRE);
        }

        public static class Filtro
        {
            public static string PorRol = $"{nameof(PermisosDeUnRolDtm.IdRol)}";
            public static string PorPuesto = $"{nameof(RolesDeUnPuestoDtm.IdPuesto)}";
            public static string PorUsuario = $"{nameof(PuestosDeUnUsuarioDtm.IdUsuario)}";
            public static string TiposNoActivos = $"{nameof(TipoDeElementoDtm.Activo)}";
            public static string ModoDeAcceso = $"{nameof(ModoDeAcceso)}".ToLower();

        }

        public static string JerarquiaDeTipos = $@"
                                WITH TIPOS 
                                AS
                                (
                                    SELECT  T1.{ICampos.ID} AS {ICampos.ID}
		                                   ,T1.{ICampos.ACTIVO}                  
		                                   ,T1.{ICampos.NOMBRE}                 
		                                   ,T1.{ICampos.ID_PADRE}                
		                                   ,T1.{ICampos.ID_GESTOR}       
		                                   ,T1.{ICampos.ID_CONSULTOR}    
		                                   ,T1.{ICampos.ID_ADM}
		                                   ,T1.{ICampos.TIPO_DTM}                
		                                   ,T1.{ICampos.TIPO_DTO}       
		                                   ,T1.{ICampos.CLASE_DE_LIBRO}                 
		                                   ,T1.{ICampos.SIGLA}                           
		                                   ,T1.{ICampos.MARCADOR}                           
		                                   ,T1.{ICampos.MASCARA}                            
		                                   ,T1.{ICampos.NOMBRE_MODIFICABLE}  
                                    FROM  [Esquema].[Tabla] T1 WITH(NOLOCK)
                                    UNION ALL
                                    --RECURSIVIDAD
                                    SELECT  T2.{ICampos.ID} AS {ICampos.ID}
		                                   ,T2.{ICampos.ACTIVO}              
		                                   ,T2.{ICampos.NOMBRE}                    
		                                   ,T2.{ICampos.ID_PADRE}                 
		                                   ,T2.{ICampos.ID_GESTOR}        
		                                   ,T2.{ICampos.ID_CONSULTOR}     
		                                   ,T2.{ICampos.ID_ADM} 
		                                   ,T2.{ICampos.TIPO_DTM}                 
		                                   ,T2.{ICampos.TIPO_DTO}   
		                                   ,T2.{ICampos.CLASE_DE_LIBRO}                 
		                                   ,T2.{ICampos.SIGLA}                                         
		                                   ,T2.{ICampos.MARCADOR}                           
		                                   ,T2.{ICampos.MASCARA}                                 
		                                   ,T2.{ICampos.NOMBRE_MODIFICABLE}                 
                                    FROM  [Esquema].[Tabla] AS T2 WITH(NOLOCK)
	                                JOIN TIPOS AS TP ON T2.{ICampos.ID_PADRE} = TP.id
                                )
                                SELECT DISTINCT
                                        T2.{ICampos.NOMBRE}              AS Padre
                                       ,T1.{ICampos.ID}                  AS Id
	                                   ,T1.{ICampos.ACTIVO}              AS Activo
	                                   ,T1.{ICampos.NOMBRE}              AS Nombre
	                                   ,T1.{ICampos.ID_PADRE}            AS IdPadre
	                                   ,T1.{ICampos.TIPO_DTM}            AS TipoDtm
                                FROM TIPOS T1
                                JOIN  {ModeloDeSeguridad.TablaDePermiso} PA WITH(NOLOCK) ON PA.ID = T1.{ICampos.ID_ADM}
                                JOIN  {ModeloDeSeguridad.TablaDePermiso} PG WITH(NOLOCK) ON PG.ID = T1.{ICampos.ID_GESTOR}
                                JOIN  {ModeloDeSeguridad.TablaDePermiso} PC WITH(NOLOCK) ON PC.ID = T1.{ICampos.ID_CONSULTOR}
                                LEFT JOIN [Esquema].[Tabla] T2 WITH(NOLOCK) ON T2.ID = T1.{ICampos.ID_PADRE}
                                WHERE 1=1 
[{nameof(_tiposAccedidosPorElRol)}]
[{nameof(_tiposAccedidosPorElPuesto)}]
[{nameof(_tiposAccedidosPorElUsuario)}]
[{nameof(_tiposNoActivos)}]
[{nameof(_filtroPorIdPadre)}]
[{nameof(_filtroPorNombre)}]
order by T2.{ICampos.NOMBRE}, T1.{ICampos.NOMBRE}
";
        private static string _modoDeAccesoAdministrador = $"tr.{ICampos.IDPERMISO} = T1.{ICampos.ID_ADM}";
        private static string _modoDeAccesoGestor = $"tr.{ICampos.IDPERMISO} = T1.{ICampos.ID_GESTOR}";
        private static string _modoDeAccesoConsultor = $"tr.{ICampos.IDPERMISO} = T1.{ICampos.ID_CONSULTOR}";

        private static string _clausulaModoDeAcceso = nameof(_clausulaModoDeAcceso);

        private static string _modoDeAccesoCualquiera = $"{_modoDeAccesoAdministrador} OR {_modoDeAccesoConsultor} OR {_modoDeAccesoGestor}";

        private static string _tiposAccedidosPorElRol = $@"
          AND EXISTS (SELECT 1 
                FROM {ModeloDeSeguridad.TablaPermisosDeUnRol} tr
                WHERE ([{_clausulaModoDeAcceso}]) 
                AND tr.{ICampos.IDROL} =  @{Parametros.ID_ROL})
         ";

        private static string _tiposAccedidosPorElPuesto = $@"
          AND EXISTS (SELECT 1 
                      FROM {ModeloDeSeguridad.TablaRolesDeUnPt} trp
                      WHERE EXISTS ((SELECT 1 
                                     FROM {ModeloDeSeguridad.TablaPermisosDeUnRol} tr
                                     WHERE ([{_clausulaModoDeAcceso}]) 
                                     AND tr.{ICampos.IDROL} = trp.{ICampos.IDROL})
                      )
                      AND {ICampos.IDPUESTO} = @{Parametros.ID_PUESTO})
         ";

        private static string _tiposAccedidosPorElUsuario = $@"
          AND EXISTS (SELECT 1 
                      FROM {ModeloDeSeguridad.TablaPtsDeUnUsuario}
                      WHERE Exists (select 1 
                                    FROM {ModeloDeSeguridad.TablaRolesDeUnPt} trp
                                    WHERE EXISTS ((SELECT 1 
                                                   FROM {ModeloDeSeguridad.TablaPermisosDeUnRol} tr
                                                   WHERE ([{_clausulaModoDeAcceso}]) 
                                                   AND tr.{ICampos.IDROL} = trp.{ICampos.IDROL})
                                    )
                                    AND {ICampos.IDPUESTO} = trp.{ICampos.IDPUESTO})
                      AND IDUSUA = @{Parametros.ID_USUARIO}
                      )
         ";

        private static string _tiposNoActivos = $@"AND T1.{ICampos.ACTIVO} = 0";

        private static string _filtroPorIdPadre = $@"AND T1.{ICampos.ID_PADRE} = @{Parametros.ID_PADRE}";

        private static string _filtroPorNombre = $"AND T1.{ICampos.NOMBRE} LIKE '%'+@{Parametros.NOMBRE}+'%'";

        public static string AplicarFiltros(string sentenciaSql, int? idPadre, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            sentenciaSql = filtrarPorPadre(sentenciaSql, idPadre, filtros, parametrosSql);

            sentenciaSql = filtrarPorNombre(sentenciaSql, filtros, parametrosSql);

            sentenciaSql = filtrarPorRol(sentenciaSql, filtros, parametrosSql);

            sentenciaSql = filtrarPorPuesto(sentenciaSql, filtros, parametrosSql);

            sentenciaSql = filtrarPorUsuario(sentenciaSql, filtros, parametrosSql);

            sentenciaSql = filtrarPorTiposNoActivos(sentenciaSql, filtros);

            return sentenciaSql;
        }

        private static string filtrarPorPadre(string sentenciaSql, int? idPadre, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorIdPadre)}]", (int)idPadre > 0 ? $"{_filtroPorIdPadre}" : "");
            if ((int)idPadre > 0)
                parametrosSql.Add($"@{Parametros.ID_PADRE}", (int)filtros[nameof(Parametros.ID_PADRE).ToLower()]);
            return sentenciaSql;
        }

        private static string filtrarPorNombre(string sentenciaSql, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            sentenciaSql = sentenciaSql.Replace($"[{nameof(_filtroPorNombre)}]", !filtros[nameof(INombre.Nombre).ToLower()].ToString().IsNullOrEmpty() ? $"{_filtroPorNombre}" : "");
            if (!filtros[nameof(INombre.Nombre).ToLower()].ToString().IsNullOrEmpty())
                parametrosSql.Add($"@{Parametros.NOMBRE}", filtros[nameof(INombre.Nombre).ToLower()].ToString());
            return sentenciaSql;
        }

        private static string filtrarPorTiposNoActivos(string sentenciaSql, Dictionary<string, object> filtros)
        {
            var filtro = Filtro.TiposNoActivos.ToLower();
            if ((bool)filtros[filtro])
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_tiposNoActivos)}]", $"{_tiposNoActivos}");
            else
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_tiposNoActivos)}]", $"");
            return sentenciaSql;
        }

        private static string filtrarPorRol(string sentenciaSql, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            var filtro = Filtro.PorRol.ToLower();
            if (filtros.ContainsKey(filtro) && filtros[filtro].ToString().Entero() > 0)
            {
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_tiposAccedidosPorElRol)}]", $"{_tiposAccedidosPorElRol}");
                parametrosSql.Add($"@{Parametros.ID_ROL}", filtros[filtro].ToString().Entero());
                sentenciaSql = FiltrarPorModoDeAcceso(sentenciaSql, filtros);
            }
            else
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_tiposAccedidosPorElRol)}]", "");

            return sentenciaSql;
        }

        private static string FiltrarPorModoDeAcceso(string sentenciaSql, Dictionary<string, object> filtros)
        {
            switch (filtros[Filtro.ModoDeAcceso])
            {
                case "-1": sentenciaSql = sentenciaSql.Replace($"[{_clausulaModoDeAcceso}]", _modoDeAccesoCualquiera); break;
                case "adm": sentenciaSql = sentenciaSql.Replace($"[{_clausulaModoDeAcceso}]", _modoDeAccesoAdministrador); break;
                case "con": sentenciaSql = sentenciaSql.Replace($"[{_clausulaModoDeAcceso}]", _modoDeAccesoConsultor); break;
                case "ges": sentenciaSql = sentenciaSql.Replace($"[{_clausulaModoDeAcceso}]", _modoDeAccesoGestor); break;
            }

            return sentenciaSql;
        }

        private static string filtrarPorPuesto(string sentenciaSql, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            var filtro = Filtro.PorPuesto.ToLower();
            if (filtros.ContainsKey(filtro) && filtros[filtro].ToString().Entero() > 0)
            {
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_tiposAccedidosPorElPuesto)}]", $"{_tiposAccedidosPorElPuesto}");
                parametrosSql.Add($"@{Parametros.ID_PUESTO}", filtros[filtro].ToString().Entero());
                sentenciaSql = FiltrarPorModoDeAcceso(sentenciaSql, filtros);
            }
            else
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_tiposAccedidosPorElPuesto)}]", "");
            return sentenciaSql;
        }

        private static string filtrarPorUsuario(string sentenciaSql, Dictionary<string, object> filtros, Dictionary<string, object> parametrosSql)
        {
            var filtro = Filtro.PorUsuario.ToLower();
            if (filtros.ContainsKey(filtro) && filtros[filtro].ToString().Entero() > 0)
            {
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_tiposAccedidosPorElUsuario)}]", $"{_tiposAccedidosPorElUsuario}");
                parametrosSql.Add($"@{Parametros.ID_USUARIO}", filtros[filtro].ToString().Entero());
                sentenciaSql = FiltrarPorModoDeAcceso(sentenciaSql, filtros);
            }
            else
                sentenciaSql = sentenciaSql.Replace($"[{nameof(_tiposAccedidosPorElUsuario)}]", "");
            return sentenciaSql;
        }

        public static TipoDeElementoDtm LeerTipoPorId(ContextoSe contexto, Type tipoDtm, int id)
        {

            var esquema = ApiDeRegistroDtm.EsquemaDeTabla(tipoDtm);
            var tabla = ApiDeRegistroDtm.NombreDeTabla(tipoDtm);

            var _leerPorId = $@"
                select {ICampos.ID} as {nameof(TipoDeElementoDtm.Id)}
                     , {ICampos.NOMBRE} as {nameof(TipoDeElementoDtm.Nombre)}
                     , {ICampos.ID_PADRE} as {nameof(TipoDeElementoDtm.IdPadre)}
                     , {ICampos.ID_CONSULTOR} as {nameof(TipoDeElementoDtm.IdPermisoDeConsultor)}
                     , {ICampos.ID_GESTOR} as {nameof(TipoDeElementoDtm.IdPermisoDeGestor)}
                     , {ICampos.ID_ADM} as {nameof(TipoDeElementoDtm.IdPermisoDeAdministrador)}
                     , {ICampos.ACTIVO} as {nameof(TipoDeElementoDtm.Activo)}
                     , {ICampos.TIPO_DTM} as {nameof(TipoDeElementoDtm.TipoDtm)}
                     , {ICampos.TIPO_DTO} as {nameof(TipoDeElementoDtm.TipoDto)}
                     , {ICampos.CLASE_DE_LIBRO} as {nameof(TipoDeElementoDtm.ClaseDeLibro)}
                     , {ICampos.SIGLA} as {nameof(TipoDeElementoDtm.Sigla)}
                     , {ICampos.MASCARA} as {nameof(TipoDeElementoDtm.Mascara)}
                     , {ICampos.MARCADOR} as {nameof(TipoDeElementoDtm.Marcador)}
                     , {ICampos.NOMBRE_MODIFICABLE} as {nameof(TipoDeElementoDtm.NombreModificable)}
                     , {ICampos.PERMITE_CREAR} as {nameof(TipoDeElementoDtm.PermiteCrear)}
                from {esquema}.{tabla}
                where {ICampos.ID} = @{ICampos.ID}
                ";

            var cache = ServicioDeCaches.Obtener($"{esquema}.{tabla}.{nameof(LeerTipoPorId)}");
            if (!cache.ContainsKey(id.ToString()))
            {
                var parametrosSql = new Dictionary<string, object>
                {
                    { $"@{ICampos.ID}", id }
                };
                var consulta = new ConsultaSql<TipoDeElementoDtm>(contexto, _leerPorId);
                var tipo = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
                if (tipo.Count != 1)
                    GestorDeErrores.Emitir($"El id del tipo solicitado no se encuentra en el sistema");
                cache[id.ToString()] = tipo[0];
            }

            return (TipoDeElementoDtm)cache[id.ToString()];
        }

        public static List<RegistroDtm> TiposDependientes(ContextoSe contexto, Type tipoDtm, int idTipo)
        {
            var esquema = ApiDeRegistroDtm.EsquemaDeTabla(tipoDtm);
            var tabla = ApiDeRegistroDtm.NombreDeTabla(tipoDtm);
            string _leer = $@"
            WITH tipos ({ICampos.ID})
            AS
            (
                SELECT {ICampos.ID}
                FROM {esquema}.{tabla}
                WHERE {ICampos.ID}  = @{ICampos.ID}
                UNION ALL
                --RECURSIVIDAD
                SELECT e.{ICampos.ID}
                FROM {esquema}.{tabla} AS e 
                JOIN tipos AS m ON e.{ICampos.ID_PADRE} = m.{ICampos.ID}
                )
            SELECT {ICampos.ID} as {nameof(RegistroDtm.Id)} 
            FROM tipos
            where {ICampos.ID} <> @{ICampos.ID}
            ";

            var cache = ServicioDeCaches.Obtener($"{esquema}.{tabla}.{nameof(TiposDependientes)}");
            if (!cache.ContainsKey(idTipo.ToString()))
            {
                var parametrosSql = new Dictionary<string, object>
                {
                    { $"@{ICampos.ID}", idTipo }
                };
                var sentencia = new ConsultaSql<RegistroDtm>(contexto, _leer);
                cache[idTipo.ToString()] = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql));
            }
            return (List<RegistroDtm>)cache[idTipo.ToString()];
        }

    }

    public class PlantillaPorTipoDtm : RegistroConNombreDtm, IPlantillaPorTipo
    {
        public int IdAccion { get; set; }

        public int IdPermiso { get; set; }

        public int IdTipo { get; set; }

        public enumNegocio Negocio { get; set; }

        public AccionDtm Accion { get; set; }
        public PermisoDtm Permiso { get; set; }

        public ITipoDtm Tipo { get; set; }

        public string NombrePa =>  $"Plt_Por_Tipo_{Nombre.NormalizarFichero()}";
        public string fichero => $"Plt_{Nombre}.{enumExtensiones.docx}".NormalizarFichero();

        public int? IdArchivo { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    public class ClaseDelTipoDtm : RegistroDtm, IClaseDelTipoDtm
    {
        public int IdTipo { get; set; }
        public int IdClase { get; set; }
        public ClaseDelNegocioDtm Clase { get; set; }
        public static enumNegocio Negocio => enumNegocio.Tarea;

        public ITipoDtm Tipo => null;
    }

    public static class ApiTipoDeElementoDtm
    {
        internal static void DefinirCamposDelTipoElementoDtm<TEntity>(ModelBuilder modelBuilder) where TEntity : TipoDeElementoDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            ApiDeRegistroDtm.DefinirCampoIdDtm<TEntity>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<TEntity>(modelBuilder, 250, "", true);

            modelBuilder.Entity<TEntity>().Property(x => x.IdPadre).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_PADRE).IsRequired(false);

            modelBuilder.Entity<TEntity>().Property(x => x.IdPermisoDeGestor).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_GESTOR).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.IdPermisoDeConsultor).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_CONSULTOR).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.IdPermisoDeAdministrador).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ADM).IsRequired();

            modelBuilder.Entity<TEntity>().Property(p => p.Activo).HasColumnName(ICampos.ACTIVO).HasColumnType(IDominio.BIT).IsRequired();
            modelBuilder.Entity<TEntity>().Property(p => p.TipoDtm).HasColumnName(ICampos.TIPO_DTM).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<TEntity>().Property(p => p.TipoDto).HasColumnName(ICampos.TIPO_DTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);

            ApiDeRegistroDtm.DefinirFk<TEntity>(modelBuilder, nameof(TipoDeElementoDtm.PermisoDeConsultor), nameof(TipoDeElementoDtm.IdPermisoDeConsultor), ICampos.ID_CONSULTOR, true);
            ApiDeRegistroDtm.DefinirFk<TEntity>(modelBuilder, nameof(TipoDeElementoDtm.PermisoDeGestor), nameof(TipoDeElementoDtm.IdPermisoDeGestor), ICampos.ID_GESTOR, true);
            ApiDeRegistroDtm.DefinirFk<TEntity>(modelBuilder, nameof(TipoDeElementoDtm.PermisoDeAdministrador), nameof(TipoDeElementoDtm.IdPermisoDeAdministrador), ICampos.ID_ADM, true);

            modelBuilder.Entity<TEntity>().Property(nameof(ILibroDeRegistro.ClaseDeLibro))
                .HasColumnName(ICampos.CLASE_DE_LIBRO)
                .HasColumnType(IDominio.VARCHAR_20)
                .IsRequired(true);

            modelBuilder.Entity<TEntity>().Property(nameof(ILibroDeRegistro.Sigla)).HasColumnName(ICampos.SIGLA).HasColumnType(IDominio.VARCHAR_5).IsRequired();
            modelBuilder.Entity<TEntity>().Property(nameof(TipoDeElementoDtm.Mascara)).HasColumnName(ICampos.MASCARA).HasColumnType(IDominio.VARCHAR_255).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(nameof(TipoDeElementoDtm.Marcador)).HasColumnName(ICampos.MARCADOR).HasColumnType(IDominio.VARCHAR_255).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(p => p.NombreModificable).HasColumnName(ICampos.NOMBRE_MODIFICABLE).HasColumnType(IDominio.BIT).IsRequired().HasDefaultValue(true);
            modelBuilder.Entity<TEntity>().Property(p => p.PermiteCrear).HasColumnName(ICampos.PERMITE_CREAR).HasColumnType(IDominio.BIT).IsRequired().HasDefaultValue(true);
            modelBuilder.Entity<TEntity>().Property(p => p.EditarTrasCrear).HasColumnName(ICampos.EDITAR_TRAS_CREAR).HasColumnType(IDominio.BIT).IsRequired().HasDefaultValue(true);

            if (ApiDeInterfaceDtm.ImplementaPermisosDeInterventor(typeof(TEntity)))
            {
                modelBuilder.Entity<TEntity>().Property(x => ((IPermisoDeInterventor)x).IdPermisoInterventor).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_INTERVENTOR).IsRequired();
                ApiDeRegistroDtm.DefinirFk<TEntity>(modelBuilder, nameof(IPermisoDeInterventor.PermisoDeInterventor), nameof(IPermisoDeInterventor.IdPermisoInterventor), ICampos.ID_INTERVENTOR, true);
            }
        }


        public static enumNegocio Negocio(Type tipoDtm)
        {
            if (!ApiDeEnsamblados.HeredaDe(tipoDtm, typeof(TipoDeElementoDtm)))
                GestorDeErrores.Emitir($"El tipo {tipoDtm.Name} no hereda de {nameof(TipoDeElementoDtm)}");

            var tablaDeTipo = ApiDeRegistroDtm.NombreDeTabla(tipoDtm);
            var tablaDeNegocio = tablaDeTipo.Replace("_" + nameof(Sufijo.TIPO), "");

            return ApiDeEnsamblados.ToEnumerado<enumNegocio>(tablaDeNegocio);
        }

    }


    public static class PlantillaPorTipo
    {
        internal static void DefinirCamposDePlantillaPorTipoDtm<TEntity>(ModelBuilder modelBuilder)
        where TEntity : RegistroConNombreDtm
        {
            if (!typeof(TEntity).ImplementaPlantillasPorTipo())
                throw new Exception($"La entidad {typeof(TEntity).Name} que está intentando definir no implementa {nameof(IPlantillaPorTipo)}");

            modelBuilder.Entity<TEntity>().Ignore(p => ((IPlantillaPorTipo) p).Negocio);
            modelBuilder.Entity<TEntity>().Ignore(nameof(PlantillaPorTipoDtm.NombrePa));
            modelBuilder.Entity<TEntity>().Ignore(nameof(PlantillaPorTipoDtm.fichero));

            ApiDeRegistroDtm.DefinirCampoIdDtm<TEntity>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<TEntity>(modelBuilder, unico: true);

            ApiDeElementoDtm.DefinirCampoArchivo<TEntity>(modelBuilder, obligatorio: true, unico: true);

            modelBuilder.Entity<TEntity>().Property(x => ((IPlantillaPorTipo)x).IdPermiso).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_PERMISO).IsRequired();
            ApiDeRegistroDtm.DefinirCampoFk<TEntity>(modelBuilder, nameof(IPlantillaPorTipo.Permiso), nameof(IPlantillaPorTipo.IdPermiso), ICampos.ID_PERMISO, requerida: true, unico: true);
            ApiDeRegistroDtm.DefinirCampoFk<TEntity>(modelBuilder, nameof(IPlantillaPorTipo.Accion), nameof(IPlantillaPorTipo.IdAccion), ICampos.ID_ACCION, requerida: true, unico: false);
            ApiDeElementoDtm.DefinirCampoTipo<TEntity>(modelBuilder, nameof(PlantillaPorTipoDtm.Tipo));
        }
    }

}

