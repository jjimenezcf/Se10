using System;
using System.Collections.Generic;
using Dapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Seguridad;

namespace ServicioDeDatos.Elemento
{

    public class PermisosDelElementoDtm: RegistroDtm, IPermisosDelElementoDtm
    {
        public int IdElemento { get; set; }
        public int IdGestor { get; set; }
        public int IdConsultor { get; set; }
        public int IdAdministrador { get; set; }

        public string Administrador { get; }
        public string Gestor { get; }
        public string Consultor { get; }
        public string Elemento { get; }
    }

    public class PermisosDelElementoSql
    {
        public static PermisosDelElementoDtm LeerPorIdElemento(ContextoSe contexto, string esquemaTabla, Type ElementoDtm, int idElemento, bool errorSiNoHay = true)
        {
            var _leerPorId = $@"
                     select {ICampos.ID} as {nameof(PermisosDelElementoDtm.Id)}
                          , {ICampos.ID_ELEMENTO} as {nameof(PermisosDelElementoDtm.IdElemento)}
                          , {ICampos.ID_ADM} as {nameof(PermisosDelElementoDtm.IdAdministrador)}
                          , {ICampos.ID_GESTOR} as {nameof(PermisosDelElementoDtm.IdGestor)}
                          , {ICampos.ID_CONSULTOR} as {nameof(PermisosDelElementoDtm.IdConsultor)}
                          , (select {ICampos.NOMBRE} from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisoDtm))}  where {ICampos.ID} = t1.{ICampos.ID_ADM}) as {nameof(PermisosDelElementoDtm.Administrador)}
                          , (select {ICampos.NOMBRE} from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisoDtm))}  where {ICampos.ID} = t1.{ICampos.ID_GESTOR}) as {nameof(PermisosDelElementoDtm.Gestor)}
                          , (select {ICampos.NOMBRE} from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisoDtm))}  where {ICampos.ID} = t1.{ICampos.ID_CONSULTOR}) as {nameof(PermisosDelElementoDtm.Consultor)}
                          , (select {ICampos.NOMBRE} from {ApiDeRegistroDtm.EsquemaTabla(ElementoDtm)} where {ICampos.ID} = t1.{ICampos.ID_ELEMENTO}) as {nameof(PermisosDelElementoDtm.Elemento)}
                     from {esquemaTabla} T1 
                     where {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
                     ";

            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO}", idElemento);

            var consulta = new ConsultaSql<PermisosDelElementoDtm>(contexto, _leerPorId);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            if (registros.Count == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No se ha localizado el permiso por elemento con Id: {idElemento} en la tabla {esquemaTabla}");

            return registros.Count == 0 ? null : registros[0];
        }


        public static PermisosDelElementoDtm LeerPermisoPorIdPermiso(ContextoSe contexto, string esquemaTabla,  int idPermiso, bool errorSiNoHay = true)
        {
            var _leerPorId = $@"
                     select {ICampos.ID} as {nameof(PermisosDelElementoDtm.Id)}
                          , {ICampos.ID_ELEMENTO} as {nameof(PermisosDelElementoDtm.IdElemento)}
                          , {ICampos.ID_ADM} as {nameof(PermisosDelElementoDtm.IdAdministrador)}
                          , {ICampos.ID_GESTOR} as {nameof(PermisosDelElementoDtm.IdGestor)}
                          , {ICampos.ID_CONSULTOR} as {nameof(PermisosDelElementoDtm.IdConsultor)}
                     from {esquemaTabla} T1 
                     where {ICampos.ID_ADM} = @{ICampos.ID_PERMISO} or {ICampos.ID_GESTOR} = @{ICampos.ID_PERMISO} or {ICampos.ID_CONSULTOR} = @{ICampos.ID_PERMISO} 
                     ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_PERMISO}", idPermiso }
            };

            var consulta = new ConsultaSql<PermisosDelElementoDtm>(contexto, _leerPorId);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            if (registros.Count == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No se ha localizado el elemento para el permiso  con Id: {idPermiso} en la tabla {esquemaTabla}");

            return registros.Count == 0 ? null : registros[0];
        }

        public static PermisosDelElementoDtm LeerElementoPorIdPermiso(ContextoSe contexto, string esquemaTabla, Type ElementoDtm, int idPermiso, bool errorSiNoHay = true)
        {
            var _leerPorId = $@"
                     select {ICampos.ID} as {nameof(PermisosDelElementoDtm.Id)}
                          , {ICampos.ID_ELEMENTO} as {nameof(PermisosDelElementoDtm.IdElemento)}
                          , {ICampos.ID_ADM} as {nameof(PermisosDelElementoDtm.IdAdministrador)}
                          , {ICampos.ID_GESTOR} as {nameof(PermisosDelElementoDtm.IdGestor)}
                          , {ICampos.ID_CONSULTOR} as {nameof(PermisosDelElementoDtm.IdConsultor)}
                          , (select {ICampos.NOMBRE} from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisoDtm))}  where {ICampos.ID} = t1.{ICampos.ID_ADM}) as {nameof(PermisosDelElementoDtm.Administrador)}
                          , (select {ICampos.NOMBRE} from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisoDtm))}  where {ICampos.ID} = t1.{ICampos.ID_GESTOR}) as {nameof(PermisosDelElementoDtm.Gestor)}
                          , (select {ICampos.NOMBRE} from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisoDtm))}  where {ICampos.ID} = t1.{ICampos.ID_CONSULTOR}) as {nameof(PermisosDelElementoDtm.Consultor)}
                          , (select {ICampos.NOMBRE} from {ApiDeRegistroDtm.EsquemaTabla(ElementoDtm)} where {ICampos.ID} = t1.{ICampos.ID_ELEMENTO}) as {nameof(PermisosDelElementoDtm.Elemento)}
                     from {esquemaTabla} T1 
                     where {ICampos.ID_ADM} = @{ICampos.ID_PERMISO} or {ICampos.ID_GESTOR} = @{ICampos.ID_PERMISO} or {ICampos.ID_CONSULTOR} = @{ICampos.ID_PERMISO} 
                     ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_PERMISO}", idPermiso }
            };

            var consulta = new ConsultaSql<PermisosDelElementoDtm>(contexto, _leerPorId);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            if (registros.Count == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No se ha localizado el elemento para el permiso  con Id: {idPermiso} en la tabla {esquemaTabla}");

            return registros.Count == 0 ? null : registros[0];
        }


        public static RegistroDtm Insertar(ContextoSe contexto, string esquemaTabla, int idElemento, int idAdministrador, int idGestor, int idConsultor)
        {
            var sentencia = $@"Insert into {esquemaTabla} (
                                        {ICampos.ID_ELEMENTO}
                                      , {ICampos.ID_ADM}
                                      , {ICampos.ID_GESTOR}
                                      , {ICampos.ID_CONSULTOR})
                               values ( @{ICampos.ID_ELEMENTO}
                                      , @{ICampos.ID_ADM}
                                      , @{ICampos.ID_GESTOR}
                                      , @{ICampos.ID_CONSULTOR})
                               SELECT SCOPE_IDENTITY() as {nameof(RegistroDtm.Id)}
             ";

            /*Evitar duplicados
              select  @ID_ELEMENTO
                     , @ID_ADM
                     , @ID_GESTOR
                     , @ID_CONSULTOR
               from {esquemaTabla} t1
               where Not Exists (select 1 from {esquemaTabla} where ID_ELEMENTO = t1.ID_ELEMENTO)
               SELECT SCOPE_IDENTITY() as Id
             * */

            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO}", idElemento);
            parametrosSql.Add($"@{ICampos.ID_ADM}", idAdministrador);
            parametrosSql.Add($"@{ICampos.ID_GESTOR}", idGestor);
            parametrosSql.Add($"@{ICampos.ID_CONSULTOR}", idConsultor);

            var consulta = new ConsultaSql<RegistroDtm>(contexto, sentencia);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql))[0];
            return registros;
        }

        public static List<PermisosDelElementoDtm> LeerElementosConSeguridad(ContextoSe contexto, string esquemaTabla)
        {
            var _leerElementos = $@"
                     select {ICampos.ID} as {nameof(PermisosDelElementoDtm.Id)}
                          , {ICampos.ID_ELEMENTO} as {nameof(PermisosDelElementoDtm.IdElemento)}
                          , {ICampos.ID_ADM} as {nameof(PermisosDelElementoDtm.IdAdministrador)}
                          , {ICampos.ID_GESTOR} as {nameof(PermisosDelElementoDtm.IdGestor)}
                          , {ICampos.ID_CONSULTOR} as {nameof(PermisosDelElementoDtm.IdConsultor)}
                     from {esquemaTabla} T1 
                     ";

            var consulta = new ConsultaSql<PermisosDelElementoDtm>(contexto, _leerElementos);
            var registros = consulta.LanzarConsulta(new DynamicParameters());

            return registros;
        }
    }

    public static class ApiPermisosDelElemento
    {
        internal static void DefinirCampos<TEntity, TPadre>(ModelBuilder modelBuilder)
            where TEntity : PermisosDelElementoDtm
            where TPadre : ElementoDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            ApiDeRegistroDtm.DefinirCampoIdDtm<TEntity>(modelBuilder);

            modelBuilder.Entity<TEntity>().Property(x => x.IdElemento).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ELEMENTO).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.IdGestor).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_GESTOR).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.IdConsultor).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_CONSULTOR).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.IdAdministrador).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ADM).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TEntity, PermisoDtm>(modelBuilder, nameof(PermisosDelElementoDtm.IdGestor), ICampos.ID_GESTOR, true);
            ApiDeRegistroDtm.DefinirFk<TEntity, PermisoDtm>(modelBuilder, nameof(PermisosDelElementoDtm.IdConsultor), ICampos.ID_CONSULTOR, true);
            ApiDeRegistroDtm.DefinirFk<TEntity, PermisoDtm>(modelBuilder, nameof(PermisosDelElementoDtm.IdAdministrador), ICampos.ID_ADM, true);

            ApiDeRegistroDtm.DefinirFk<TEntity, TPadre>(modelBuilder, nameof(PermisosDelElementoDtm.IdElemento), ICampos.ID_ELEMENTO, true);
        }
    }
}
