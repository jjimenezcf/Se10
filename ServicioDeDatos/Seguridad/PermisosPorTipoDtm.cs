using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using Utilidades;

namespace ServicioDeDatos.Seguridad
{
    [Table(Tablas.PERMISO_POR_TIPO, Schema = Esquemas.SEGURIDAD)]
    public class PermisosPorTipoDtm : RegistroDtm, IPermisoOtorgado
    {
        public int IdNegocio { get; set; }
        public int IdTipo { get; set; }
        public int IdUsuario { get; set; }
        public int IdPermiso { get; set; }
        public bool Calculado { get; set; }

        public NegocioDtm Negocio { get; }
        public UsuarioDtm Usuario { get; }
        public PermisoDtm Permiso { get; }

    }

    public class PermisosPorTipoSql
    {
        private static readonly string _LeerTabla = $@"
            select {ICampos.ID}
              , {ICampos.ID_NEGOCIO} as {nameof(PermisosPorTipoDtm.IdNegocio)}
              , {ICampos.ID_TIPO} as {nameof(PermisosPorTipoDtm.IdTipo)}
              , {ICampos.ID_USUARIO} as {nameof(PermisosPorTipoDtm.IdUsuario)}
              , {ICampos.ID_PERMISO}  as {nameof(PermisosPorTipoDtm.IdPermiso)}
              , {ICampos.CALCULADO}  as {nameof(PermisosPorTipoDtm.Calculado)}
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTipoDtm))} ";

        public static List<PermisosPorTipoDtm> PermisosDeUsuario(ContextoSe contexto, int idNegocio, int idTipo)
        {
            string _leer = $@"{_LeerTabla}
            where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
              and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
            ";

            if (idTipo > 0) _leer = $"{_leer}{Environment.NewLine}and {ICampos.ID_TIPO} = @{ICampos.ID_TIPO}";

            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_PorTipo);
            var indice = $"{idNegocio}.{contexto.DatosDeConexion.IdUsuario}.{idTipo}";
            if (!cache.ContainsKey(indice))
            {
                var parametrosSql = new Dictionary<string, object>
                {
                    { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                    { $"@{ICampos.ID_USUARIO}", contexto.DatosDeConexion.IdUsuario }
                };
                if (idTipo > 0) parametrosSql.Add($"@{ICampos.ID_TIPO}", idTipo);

                var sentencia = new ConsultaSql<PermisosPorTipoDtm>(contexto, _leer);
                cache[indice] = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql));
            }
            return (List<PermisosPorTipoDtm>)cache[indice];
        }

        public static bool EstaElPermisoOtorgado(ContextoSe contexto, PermisosPorTipoDtm registro)
        {
            string _leer = $@"select {ICampos.ID}
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTipoDtm))} 
            where {ICampos.ID_PERMISO} = @{ICampos.ID_PERMISO}
              and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
            ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_PERMISO}", registro.IdPermiso },
                { $"@{ICampos.ID_USUARIO}", registro.IdUsuario }
            };

            var sentencia = new ConsultaSql<RegistroDtm>(contexto, _leer);
            var r = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql));

            return r.Count() > 0;
        }

        public static bool UsuarioConAlgunPermiso(ContextoSe contexto, List<int> permisos)
        =>
        UsuarioConAlgunPermiso(contexto, contexto.DatosDeConexion.IdUsuario, permisos);

        public static bool UsuarioConAlgunPermiso(ContextoSe contexto, int IdUsuario, List<int> permisos)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_PorTipo);
            var indice = $"{contexto.DatosDeConexion.IdUsuario}.{permisos.ToString(Simbolos.Guion)}";
            if (!cache.ContainsKey(indice))
            {
                string _leer = $@"
                                select top(1) 1
                                from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTipoDtm))} 
                                where {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
                                  and {ICampos.ID_PERMISO} in ({permisos.ToString(Simbolos.Coma)})
                                ";
                var parametrosSql = new Dictionary<string, object> { { $"@{ICampos.ID_USUARIO}", IdUsuario } };
                var sentencia = new ConsultaSql<PermisosPorTipoDtm>(contexto, _leer);
                cache[indice] = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql)).Count() == 1;
            }
            return (bool)cache[indice];
        }


        public static void Otorgar(ContextoSe contexto, int idNegocio, Type tipoDeElemento, int idTipo, int idUsuario, int idPermiso, bool calculado)
        {
            string _crear = $@"
            insert into {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTipoDtm))} (
                {ICampos.ID_NEGOCIO}
              , {ICampos.ID_TIPO}
              , {ICampos.ID_USUARIO}
              , {ICampos.ID_PERMISO}
              , {ICampos.CALCULADO}
            ) 
            select @{ICampos.ID_NEGOCIO}, @{ICampos.ID_TIPO}, @{ICampos.ID_USUARIO}, @{ICampos.ID_PERMISO}, {(calculado ? 1 : 0)}
            where Not Exists (select 1 
               from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTipoDtm))} 
               where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
                 and {ICampos.ID_TIPO} = @{ICampos.ID_TIPO}
                 and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
                 and {ICampos.ID_PERMISO} = @{ICampos.ID_PERMISO}
            )
            ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                { $"@{ICampos.ID_TIPO}", idTipo },
                { $"@{ICampos.ID_USUARIO}", idUsuario },
                { $"@{ICampos.ID_PERMISO}", idPermiso }
            };

            var sentencia = new ConsultaSql<PermisosPorTipoDtm>(contexto, _crear);
            sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));

            var dependientes = TipoDeElementoSql.TiposDependientes(contexto, tipoDeElemento, idTipo);
            foreach (var hijo in dependientes)
            {
                parametrosSql[$"@{ICampos.ID_TIPO}"] = hijo.Id;
                sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
            }
        }

        public static void QuitarPermisos(ContextoSe contexto, int idUsuario, bool? calculado)
        {
            var esCalculado = calculado == null ? "" : $" and {ICampos.CALCULADO} = {((bool)calculado ? 1 : 0)}";

            string _crear = $@"
            delete from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTipoDtm))} 
               where  {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}{esCalculado}";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_USUARIO}", idUsuario }
            };

            var sentencia = new ConsultaSql<PermisosPorTipoDtm>(contexto, _crear);
            sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));

            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorTipo);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_TiposConGestion);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_TiposConsultor);
        }

        public static int EliminarTodos(ContextoSe contexto)
        {
            string _quitar = $@"
            delete 
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTipoDtm))}
            where {ICampos.CALCULADO} = 1 
            ";
            var parametrosSql = new Dictionary<string, object>();

            var sentencia = new ConsultaSql<PermisosPorTipoDtm>(contexto, _quitar);
            var i = sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorTipo);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_TiposConGestion);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_TiposConsultor);
            return i;
        }

    }


    public static partial class ModeloDeSeguridad
    {
        internal static string TablaDePermisoPorTipo => $"{ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTipoDtm))}";

        public static void PermisoPorTipo(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<PermisosPorTipoDtm>(modelBuilder);
            modelBuilder.Entity<PermisosPorTipoDtm>().Property(p => p.IdNegocio).HasColumnName(ICampos.ID_NEGOCIO).HasColumnType("INT").IsRequired();
            modelBuilder.Entity<PermisosPorTipoDtm>().Property(p => p.IdTipo).HasColumnName(ICampos.ID_TIPO).HasColumnType("INT").IsRequired();
            modelBuilder.Entity<PermisosPorTipoDtm>().Property(p => p.IdPermiso).HasColumnName(ICampos.ID_PERMISO).HasColumnType("INT").IsRequired();
            modelBuilder.Entity<PermisosPorTipoDtm>().Property(p => p.IdUsuario).HasColumnName(ICampos.ID_USUARIO).HasColumnType("INT").IsRequired();
            modelBuilder.Entity<PermisosPorTipoDtm>().Property(p => p.Calculado).HasColumnName(ICampos.CALCULADO).HasColumnType(IDominio.BIT).IsRequired();

            modelBuilder.Entity<PermisosPorTipoDtm>()
                        .HasAlternateKey(x => new { x.IdNegocio, x.IdTipo, x.IdUsuario, x.IdPermiso })
                        .HasName($"AK_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorTipoDtm))}");


            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorTipoDtm>(modelBuilder
                , nameof(PermisosPorTipoDtm.Negocio)
                , nameof(PermisosPorTipoDtm.IdNegocio)
                , ICampos.ID_NEGOCIO
                , requerida: true
                , unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorTipoDtm>(modelBuilder
                , nameof(PermisosPorTipoDtm.Usuario)
                , nameof(PermisosPorTipoDtm.IdUsuario)
                , ICampos.ID_USUARIO
                , requerida: true
                , unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorTipoDtm>(modelBuilder
                , nameof(PermisosPorTipoDtm.Permiso)
                , nameof(PermisosPorTipoDtm.IdPermiso)
                , ICampos.ID_PERMISO
                , requerida: true
                , unico: false);


            modelBuilder.Entity<PermisosPorTipoDtm>()
              .HasIndex(x => new { x.IdNegocio, x.IdTipo })
              .IsUnique(false)
              .HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorTipoDtm))}_{ICampos.ID_NEGOCIO}_{ICampos.ID_TIPO}");

            modelBuilder.Entity<PermisosPorTipoDtm>()
              .HasIndex(x => new { x.IdNegocio, x.IdUsuario, x.IdPermiso })
              .IsUnique(false)
              .HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorTipoDtm))}_{ICampos.ID_NEGOCIO}_{ICampos.ID_USUARIO}_{ICampos.ID_PERMISO}");
        }
    }
}
