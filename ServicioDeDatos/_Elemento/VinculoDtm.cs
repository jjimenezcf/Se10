using Dapper;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.SistemaDocumental;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
namespace ServicioDeDatos.Elemento
{
    public class VinculoDtm : RegistroDtm, IVinculoDtm
    {
        public int idElemento1 { get; set; }
        public int idElemento2 { get; set; }
    }

    public class CualquierVinculo
    {
        public int Id { get; set; }
        public enumNegocio Negocio1 { get; set; }
        public int idElemento1 { get; set; }
        public INombre elemento1 { get; set; }
        public enumNegocio Negocio2 { get; set; }
        public int idElemento2 { get; set; }
        public IElementoDtm elemento2 { get; set; }
    }


    public class ltrDeVinculos
    {
        public const string EstadosDelVinculadoDiferentes = nameof(EstadosDelVinculadoDiferentes);
        public const string EstadosDelPrincipalDiferentes = nameof(EstadosDelPrincipalDiferentes);
        public const string TiposDelVinculadoDiferentes = nameof(TiposDelVinculadoDiferentes);
        public const string TiposDelPrincipalDiferentes = nameof(TiposDelPrincipalDiferentes);
        public const string CgsDelVinculadoDiferentes = nameof(CgsDelVinculadoDiferentes);
        public const string CgsDelPrincipalDiferentes = nameof(CgsDelPrincipalDiferentes);

        public const string EstadosDelVinculadoIguales = nameof(EstadosDelVinculadoIguales);
        public const string EstadosDelPrincipalIguales = nameof(EstadosDelPrincipalIguales);
        public const string TiposDelVinculadoIguales = nameof(TiposDelVinculadoIguales);
        public const string TiposDelPrincipalIguales = nameof(TiposDelPrincipalIguales);
        public const string CgsDelVinculadoIguales = nameof(CgsDelVinculadoIguales);
        public const string CgsDelPrincipalIguales = nameof(CgsDelPrincipalIguales);
    }
    public static class ApiDeVinculos
    {
        internal static void DefinirCampos<TEntity>(ModelBuilder modelBuilder, string propiedadReferenciada1, string propiedadReferenciada2, bool hijoUnico = false) where TEntity : VinculoDtm
        {
            ApiDeRegistroDtm.DefinirCampoFk<TEntity>(modelBuilder, propiedadReferenciada1, nameof(VinculoDtm.idElemento1), ICampos.ID_ELEMENTO1, requerida: true, false);
            ApiDeRegistroDtm.DefinirCampoFk<TEntity>(modelBuilder, propiedadReferenciada2, nameof(VinculoDtm.idElemento2), ICampos.ID_ELEMENTO2, requerida: true, hijoUnico);


            modelBuilder.Entity<TEntity>()
                        .HasAlternateKey(new string[] { nameof(VinculoDtm.idElemento1), nameof(VinculoDtm.idElemento2) })
                        .HasName($"AK_{ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity))}");

        }

        public static string Tabla(Type tipoDtm1, Type tipoDtm2)
        {
            return ApiDeRegistroDtm.EsquemaTabla(tipoDtm1) + "_" + ApiDeRegistroDtm.NombreDeTabla(tipoDtm2);
        }

        public static string TablaDeVinculacion(Type tipoDtm, enumNegocio negocio, bool errorSiNoExiste = true)
        {
            switch (negocio)
            {
                case enumNegocio.Archivos: return $"{ApiDeElementoDtm.TablaDeArchivosVinculados(tipoDtm)}";
                case enumNegocio.Archivador: return $"{ApiDeElementoDtm.TablaDeArchivadoresVinculados(tipoDtm)}";
                case enumNegocio.Interlocutor: return $"{ApiDeElementoDtm.TablaDeInterlocutoresVinculados(tipoDtm)}";
                case enumNegocio.Tarea: return $"{ApiDeElementoDtm.TablaDeTareasVinculadas(tipoDtm)}";
                case enumNegocio.Registro: return $"{ApiDeElementoDtm.TablaDeRegistrosEsVinculados(tipoDtm)}";
                case enumNegocio.EventoDeAgenda: return $"{ApiDeElementoDtm.TablaDeEventosVinculados(tipoDtm)}";
                case enumNegocio.Certificado: return $"{ApiDeElementoDtm.TablaDeCertificados(tipoDtm)}";
                case enumNegocio.Pago: return $"{ApiDeElementoDtm.TablaDePagos(tipoDtm)}";
                case enumNegocio.Expediente: return $"{ApiDeElementoDtm.TablaDeExpedintes(tipoDtm)}";
                case enumNegocio.CircuitoDoc: return $"{ApiDeElementoDtm.TablaDeCircuitosDoc(tipoDtm)}";
            }
            if (errorSiNoExiste) throw new Exception($"Debe definir el nombre de la tabla para la relación entre un objeto del tipo {tipoDtm} con el negocio {negocio}");
            return null;
        }
    }

    public static class VinculoSql
    {

        private static string _quitarVinculo = $@"
        delete from [esquema].[tabla] 
        where {ICampos.ID_ELEMENTO1} = @{ICampos.ID_ELEMENTO1}
          and {ICampos.ID_ELEMENTO2} = @{ICampos.ID_ELEMENTO2}
        ";

        private static string _quitarVinculoAl = $@"
        delete from [esquema].[tabla] 
        where {ICampos.ID_ELEMENTO2} = @{ICampos.ID_ELEMENTO2}
        ";

        private static string _quitarVinculoAlExcepto = $@"
        delete from [esquema].[tabla] 
        where {ICampos.ID_ELEMENTO2} = @{ICampos.ID_ELEMENTO2} and {ICampos.ID_ELEMENTO1} <> @{ICampos.ID_ELEMENTO1}
        ";

        private static string _leerVinculo = @$"  
        select {ICampos.ID} as {nameof(RegistroDtm.Id)}, {ICampos.ID_ELEMENTO1} as {nameof(VinculoDtm.idElemento1)}, {ICampos.ID_ELEMENTO2} as {nameof(VinculoDtm.idElemento2)}
        from tabla 
        where {ICampos.ID_ELEMENTO1} = @{ICampos.ID_ELEMENTO1}
          and {ICampos.ID_ELEMENTO2} = @{ICampos.ID_ELEMENTO2}";

        public static VinculoDtm CrearVinculo(ContextoSe contexto, Type tipoDtm, enumNegocio negocio, int idElemento1, int idElemento2, bool vincularSiNoLoEsta = false)
        {

            var tabla = ApiDeVinculos.TablaDeVinculacion(tipoDtm, negocio);

            string _crearVinculo = $@"
                 begin
                   insert into {tabla} ({ICampos.ID_ELEMENTO1}, {ICampos.ID_ELEMENTO2}) 
                   values(@{ICampos.ID_ELEMENTO1},@{ICampos.ID_ELEMENTO2})
                   {_leerVinculo}
                 end
                 ";

            if (vincularSiNoLoEsta) _crearVinculo = $@"
                 begin 
                   select @cantidad = count(*)
                   from  {tabla}
                   where ID_ELEMENTO1 = @{ICampos.ID_ELEMENTO1} and ID_ELEMENTO2 = @{ICampos.ID_ELEMENTO2}
                   
                   if @cantidad = 0 begin
                   insert into {tabla} ({ICampos.ID_ELEMENTO1}, {ICampos.ID_ELEMENTO2}) 
                   values(@{ICampos.ID_ELEMENTO1},@{ICampos.ID_ELEMENTO2})
                   end
                   {_leerVinculo}
                 end
                 ";

            var parametrosSql = new Dictionary<string, object>();
            var sentenciaSql = _crearVinculo.Replace("tabla", tabla);

            parametrosSql.Add($"@cantidad", 0);
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO1}", idElemento1);
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO2}", idElemento2);

            var sentencia = new ConsultaSql<VinculoDtm>(contexto, sentenciaSql);
            try
            {
                var vinculos = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql));
                return vinculos[0];
            }
            finally
            {
                VaciarCache(contexto, tipoDtm, negocio, idElemento1, idElemento2);
            }
        }

        public static void VaciarCache(ContextoSe contexto, Type tipoDtm, enumNegocio vinculado, int idElemento1, int idElemento2)
        {
            var tabla = ApiDeVinculos.TablaDeVinculacion(tipoDtm, vinculado);
            ServicioDeCaches.EliminarElemento(CacheDe.LeerVinculosCon, $"{tabla}-{idElemento1}");
            ServicioDeCaches.EliminarElemento(CacheDe.LeerVinculosAl, $"{tabla}-{idElemento2}");
            ServicioDeCaches.EliminarElemento(CacheDe.ExisteElVinculo, $"{tabla}-{idElemento1}-{idElemento2}");
            ServicioDeCaches.EliminarElemento(CacheDe.ContarVinculosCon, $"{tabla}-{idElemento1}");
            BlanquearCacheDeAnexados(contexto, tipoDtm, idElemento1);
        }

        public static void QuitarVinculo(ContextoSe contexto, Type tipoDtm, enumNegocio negocio, int idElemento1, int idElemento2)
        {
            var parametrosSql = new Dictionary<string, object>();

            var tabla = ApiDeVinculos.TablaDeVinculacion(tipoDtm, negocio);
            var sentenciaSql = _quitarVinculo.Replace("[esquema].[tabla]", $"{tabla}");

            parametrosSql.Add($"@{ICampos.ID_ELEMENTO1}", idElemento1);
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO2}", idElemento2);

            var consulta = new ConsultaSql<VinculoDtm>(contexto, sentenciaSql);
            try
            {
                consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
            }
            finally
            {
                ServicioDeCaches.EliminarElemento(CacheDe.LeerVinculosCon, $"{tabla}-{idElemento1}");
                ServicioDeCaches.EliminarElemento(CacheDe.LeerVinculosAl, $"{tabla}-{idElemento2}");
                ServicioDeCaches.EliminarElemento(CacheDe.ExisteElVinculo, $"{tabla}-{idElemento1}-{idElemento2}");
                BlanquearCacheDeAnexados(contexto, tipoDtm, idElemento1);
            }
        }

        public static void BlanquearCacheDeAnexados()
        {
            ServicioDeCaches.EliminarCache(CacheDe.Arc_AnexadosDeUnArchivadorVinculado);
            ServicioDeCaches.EliminarCache(CacheDe.Arc_AnexadosDto_PorGuid);
            ServicioDeCaches.EliminarCache(CacheDe.Arc_AnexadosDto);
            ServicioDeCaches.EliminarCache(CacheDe.Arc_AnexadosDtm);
            ServicioDeCaches.EliminarCache(CacheDe.Arc_Anexados);
            ServicioDeCaches.EliminarCache(CacheDe.Arc_CantidadDeArchivos);
            ServicioDeCaches.EliminarCache(CacheDe.Arc_ListaDeArchivosExt); 
        }

        public static void BlanquearCacheDeAnexados(ContextoSe contexto, Type tipo, int idElemento)
        {
            var clave = $"{tipo}-{idElemento}";
            ServicioDeCaches.EliminarElemento(CacheDe.Arc_AnexadosDeUnArchivadorVinculado, clave);
            ServicioDeCaches.EliminarElemento(CacheDe.Arc_AnexadosDto, clave);
            ServicioDeCaches.EliminarElemento(CacheDe.Arc_AnexadosDtm, clave);
            ServicioDeCaches.EliminarElemento(CacheDe.Arc_Anexados, clave);
            ServicioDeCaches.EliminarElementos(CacheDe.Arc_AnexadosDto_PorGuid,$"-{idElemento}");
            if (tipo == typeof(ArchivadorDtm))
            {
                ServicioDeCaches.EliminarElemento(CacheDe.Arc_CantidadDeArchivos, idElemento.ToString());
                ServicioDeCaches.EliminarElemento(CacheDe.Arc_ListaDeArchivosExt, idElemento.ToString());
            }
            if (tipo == typeof(CarpetaDtm))
            {
                var idarchivador = contexto.Set<CarpetaDtm>().First(x => x.Id == idElemento).IdArchivador;
                ServicioDeCaches.EliminarElemento(CacheDe.Arc_CantidadDeArchivos, idarchivador.ToString());
                ServicioDeCaches.EliminarElemento(CacheDe.Arc_ListaDeArchivosExt, idarchivador.ToString());
            }
        }

        public static void QuitarVinculosAl(ContextoSe contexto, Type tipoDtm, enumNegocio negocio, int idElemento2)
        {
            var parametrosSql = new Dictionary<string, object>();

            var tabla = ApiDeVinculos.TablaDeVinculacion(tipoDtm, negocio);
            var sentenciaSql = _quitarVinculoAl.Replace("[esquema].[tabla]", $"{tabla}");

            parametrosSql.Add($"@{ICampos.ID_ELEMENTO2}", idElemento2);

            var consulta = new ConsultaSql<VinculoDtm>(contexto, sentenciaSql);
            consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
            ServicioDeCaches.EliminarElemento(CacheDe.LeerVinculosAl, $"{tabla}-{idElemento2}");
            ServicioDeCaches.EliminarElementos(CacheDe.LeerVinculosCon, $"{tabla}");
            ServicioDeCaches.EliminarElementos(CacheDe.ExisteElVinculo, $"{tabla}");
            ServicioDeCaches.EliminarElementos(CacheDe.ContarVinculosCon, $"{tabla}");
        }
        public static void QuitarVinculosAlExcepto(ContextoSe contexto, Type tipoDtm, enumNegocio negocio, int idElemento2, int idElemento1)
        {
            var parametrosSql = new Dictionary<string, object>();

            var tabla = ApiDeVinculos.TablaDeVinculacion(tipoDtm, negocio);
            var sentenciaSql = _quitarVinculoAlExcepto.Replace("[esquema].[tabla]", $"{tabla}");

            parametrosSql.Add($"@{ICampos.ID_ELEMENTO2}", idElemento2);
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO1}", idElemento1);

            var consulta = new ConsultaSql<VinculoDtm>(contexto, sentenciaSql);
            consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
            ServicioDeCaches.EliminarElemento(CacheDe.LeerVinculosAl, $"{tabla}-{idElemento2}");
            ServicioDeCaches.EliminarElementos(CacheDe.LeerVinculosCon, $"{tabla}");
            ServicioDeCaches.EliminarElementos(CacheDe.ExisteElVinculo, $"{tabla}");
            ServicioDeCaches.EliminarElementos(CacheDe.ContarVinculosCon, $"{tabla}");
        }

        public static int ContarVinculosCon(ContextoSe contexto, Type tipoDtm1, Type tipoDtm2, int idElemento)
        {
            var tabla = ApiDeVinculos.Tabla(tipoDtm1, tipoDtm2);

            var indice = $"{tabla}-{idElemento}";
            var cache = ServicioDeCaches.Obtener(CacheDe.ContarVinculosCon);
            if (cache.ContainsKey(indice))
                return (int)cache[indice];

            GestorDeMetadatos.ValidarExisteTabla(tabla);

            var consultaSql = $"Select count(*) cantidad from {tabla} where {ICampos.ID_ELEMENTO1} = @{ICampos.ID_ELEMENTO1}";
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_ELEMENTO1}", idElemento }
            };

            var consulta = new ConsultaSql<RegistrosAfectados>(contexto, consultaSql);

            cache[indice] = consulta.LanzarConsulta(new DynamicParameters(parametrosSql))[0].cantidad;
            return (int)cache[indice];
        }

        public static int ExisteElVinculo(ContextoSe contexto, Type tipoDtm1, Type tipoDtm2, int idElemento1, int idElemento2)
        {
            var tabla = ApiDeVinculos.Tabla(tipoDtm1, tipoDtm2);

            var indice = $"{tabla}-{idElemento1}-{idElemento2}";
            var cache = ServicioDeCaches.Obtener(CacheDe.ExisteElVinculo);
            if (cache.ContainsKey(indice))
                return (int)cache[indice];

            GestorDeMetadatos.ValidarExisteTabla(tabla);

            var parametrosSql = new Dictionary<string, object>();
            var consultaSql = $"Select count(*) cantidad from {tabla} where {ICampos.ID_ELEMENTO1} = @{ICampos.ID_ELEMENTO1} and  {ICampos.ID_ELEMENTO2} = @{ICampos.ID_ELEMENTO2}";

            parametrosSql.Add($"@{ICampos.ID_ELEMENTO1}", idElemento1);
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO2}", idElemento2);

            var consulta = new ConsultaSql<RegistrosAfectados>(contexto, consultaSql);

            cache[indice] = consulta.LanzarConsulta(new DynamicParameters(parametrosSql))[0].cantidad;
            return (int)cache[indice];
        }

        public static List<VinculoDtm> LeerVinculosCon(ContextoSe contexto, Type tipoDtm1, Type tipoDtm2, int idElemento, Dictionary<string, object> filtros = null)
        {
            var tabla = ApiDeVinculos.Tabla(tipoDtm1, tipoDtm2);
            GestorDeMetadatos.ValidarExisteTabla(tabla);
            return LeerVinculosCon(contexto, tabla, idElemento, ApiDeRegistroDtm.EsquemaTabla(tipoDtm1), ApiDeRegistroDtm.EsquemaTabla(tipoDtm2), filtros);
        }

        public static List<VinculoDtm> LeerVinculosCon(ContextoSe contexto, Type tipoDtm, enumNegocio negocioVinculado, string tablaDelVinculado, int idElemento, Dictionary<string, object> filtros = null)
        {
            var tabla = ApiDeVinculos.TablaDeVinculacion(tipoDtm, negocioVinculado);

            return LeerVinculosCon(contexto, tabla, idElemento, ApiDeRegistroDtm.EsquemaTabla(tipoDtm), tablaDelVinculado, filtros);
        }

        private static List<VinculoDtm> LeerVinculosCon(ContextoSe contexto, string esquema_tabla, int idElemento, string tabla1, string tabla2, Dictionary<string, object> filtros)
        {

            var indice = $"{esquema_tabla}-{idElemento}";
            var cache = ServicioDeCaches.Obtener(CacheDe.LeerVinculosCon);
            if (filtros is null || filtros.Count == 0)
            {
                if (cache.ContainsKey(indice))
                    return (List<VinculoDtm>)cache[indice];
            }

            string _leerVinculosCon = $@"
                   select t1.{ICampos.ID_ELEMENTO1} as {nameof(VinculoDtm.idElemento1)}
                        , t1.{ICampos.ID_ELEMENTO2} as {nameof(VinculoDtm.idElemento2)}
                   from {esquema_tabla} t1{Environment.NewLine}";

            string _join = filtros == null ? "" :
                         $"inner join {tabla1} t2 on t2.{ICampos.ID} = t1.{ICampos.ID_ELEMENTO1}" + Environment.NewLine +
                         $"inner join {tabla2} t3 on t3.{ICampos.ID} = t1.{ICampos.ID_ELEMENTO2}" + Environment.NewLine;

            string _where = $"where t1.{ICampos.ID_ELEMENTO1} = @{ICampos.ID_ELEMENTO1}";
            if (filtros != null) _where = IncluirFiltros(filtros, _where);

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_ELEMENTO1}", idElemento }
            };
            var consulta = new ConsultaSql<VinculoDtm>(contexto, _leerVinculosCon + _join + _where);
            var resultado = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
            if (filtros is null || filtros.Count == 0)
                cache[indice] = resultado;
            return resultado;
        }

        public static List<VinculoDtm> LeerVinculosAl(ContextoSe contexto, Type tipoDtm, enumNegocio negocioVinculado, Type tipoVinculadoDtm, int idElemento2, Dictionary<string, object> filtros)
        {
            var tablaDeVinculacion = ApiDeVinculos.TablaDeVinculacion(tipoDtm, negocioVinculado);
            var indice = $"{tablaDeVinculacion}-{idElemento2}";
            var cache = ServicioDeCaches.Obtener(CacheDe.LeerVinculosAl);
            if (filtros is null || filtros.Count == 0)
            {
                if (cache.ContainsKey(indice))
                    return (List<VinculoDtm>)cache[indice];
            }

            string _leerVinculosAl = $@"
                   select  t1.{ICampos.ID} as {nameof(VinculoDtm.Id)}
                         , t1.{ICampos.ID_ELEMENTO1} as {nameof(VinculoDtm.idElemento1)}
                         , t1.{ICampos.ID_ELEMENTO2} as {nameof(VinculoDtm.idElemento2)}
                   from {tablaDeVinculacion} t1{Environment.NewLine}";

            string _join = filtros == null ? "" :
                         $"inner join {ApiDeRegistroDtm.EsquemaTabla(tipoDtm)} t2 on t2.{ICampos.ID} = t1.{ICampos.ID_ELEMENTO1}" + Environment.NewLine +
                         $"inner join {ApiDeRegistroDtm.EsquemaTabla(tipoVinculadoDtm)}  t3 on t3.{ICampos.ID} = t1.{ICampos.ID_ELEMENTO2}" + Environment.NewLine;

            string _where = $"where t1.{ICampos.ID_ELEMENTO2} = @{ICampos.ID_ELEMENTO2}";
            if (filtros != null) _where = IncluirFiltros(filtros, _where);
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_ELEMENTO2}", idElemento2 }
            };
            var consulta = new ConsultaSql<VinculoDtm>(contexto, _leerVinculosAl + _join + _where);
            var resultado = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
            if (filtros is null || filtros.Count == 0)
                cache[indice] = resultado;
            return resultado;
        }


        public static int ContarVinculosCon(ContextoSe contexto, Type tipoDtm, enumNegocio negocio, int idElemento)
        {
            var tabla = ApiDeVinculos.TablaDeVinculacion(tipoDtm, negocio);
            string _contarVinculosCon = $@"
                   select count(*) as cantidad
                   from {tabla} 
                   where {ICampos.ID_ELEMENTO1} = @{ICampos.ID_ELEMENTO1}
                   ";
            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO1}", idElemento);

            var consulta = new ConsultaSql<RegistrosAfectados>(contexto, _contarVinculosCon);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
            return registros[0].cantidad;
        }

        public static int ContarVinculosConElTipo(ContextoSe contexto, Type tipoDtm, enumNegocio negocio, int idTipo)
        {
            var tablaDeVinculo = ApiDeVinculos.TablaDeVinculacion(tipoDtm, negocio);
            var tablaDeElementos = ApiDeRegistroDtm.EsquemaTabla(tipoDtm);
            string _contarVinculosCon = $@"
                   select count(*) as cantidad
                   from {tablaDeVinculo} t1
                   inner join {tablaDeElementos} t2 on t2.id = t1.{ICampos.ID_ELEMENTO1}
                   where t2.{ICampos.ID_TIPO} = @{ICampos.ID_TIPO}
                   ";
            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID_TIPO}", idTipo);

            var consulta = new ConsultaSql<RegistrosAfectados>(contexto, _contarVinculosCon);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
            return registros[0].cantidad;
        }

        public static int ContarVinculosAl(ContextoSe contexto, Type tipoDtm, enumNegocio vinculado, int idElemento2)
        {
            var tabla = ApiDeVinculos.TablaDeVinculacion(tipoDtm, vinculado);
            string _contarVinculosAl = $@"
                   select count(*) as cantidad
                   from {tabla} 
                   where {ICampos.ID_ELEMENTO2} = @{ICampos.ID_ELEMENTO2}
                   ";
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_ELEMENTO2}", idElemento2 }
            };

            var consulta = new ConsultaSql<RegistrosAfectados>(contexto, _contarVinculosAl);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
            return registros[0].cantidad;
        }

        public static VinculoDtm LeerVinculo(ContextoSe contexto, Type tipoDtm, enumNegocio negocio, int idElemento1, int idElemento2, bool erroSiNoHay = true)
        {
            var tabla = ApiDeVinculos.TablaDeVinculacion(tipoDtm, negocio);

            var parametrosSql = new Dictionary<string, object>();

            parametrosSql.Add($"@{ICampos.ID_ELEMENTO1}", idElemento1);
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO2}", idElemento2);

            var consulta = new ConsultaSql<VinculoDtm>(contexto, _leerVinculo.Replace("tabla", tabla));
            var vinculos = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
            if (vinculos.Count == 0 && erroSiNoHay)
                throw new Exception($"No hay vinculos en la tabla {tabla} para los ids solicitados {idElemento1}, {idElemento2}");

            return vinculos.Count == 0 ? null : vinculos[0];
        }

        public static List<RegistroDtm> RegistrosSinVinculosAl<T>(this ContextoSe contexto, enumNegocio negocio) where T : ElementoDtm
        {
            var tablaDeVinculos = ApiDeVinculos.TablaDeVinculacion(typeof(T), negocio);
            return RegistrosSinVinculosAl(contexto, ApiDeRegistroDtm.EsquemaTabla(typeof(T)), tablaDeVinculos);
        }

        private static List<RegistroDtm> RegistrosSinVinculosAl(ContextoSe contexto, string tablaDeElementos, string tablaDeVinculos)
        {
            string _leerVinculosCon = $@"
                   select {ICampos.ID}
                   from {tablaDeElementos} t1
                   where not exists (select top(1) 1 from {tablaDeVinculos} where {ICampos.ID_ELEMENTO1} = t1.{ICampos.ID})
                   ";

            var parametrosSql = new Dictionary<string, object>();
            var consulta = new ConsultaSql<RegistroDtm>(contexto, _leerVinculosCon);
            return consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
        }


        private static string IncluirFiltros(Dictionary<string, object> filtros, string where)
        {
            foreach (var filtro in filtros)
            {
                where = IncluirFiltrosPorEstado(where, filtro);
                where = IncluirFiltrosPorTipo(where, filtro);
                where = IncluirFiltrosPorCg(where, filtro);
            }

            return where;
        }

        private static string IncluirFiltrosPorCg(string where, KeyValuePair<string, object> filtro)
        {
            if (filtro.Key == ltrDeVinculos.CgsDelVinculadoDiferentes)
                where = where + $"{Environment.NewLine} and t3.{ICampos.ID_CG} not in ({filtro.Value}){Environment.NewLine}";
            if (filtro.Key == ltrDeVinculos.CgsDelVinculadoIguales)
                where = where + $"{Environment.NewLine} and t3.{ICampos.ID_CG} in ({filtro.Value}){Environment.NewLine}";


            if (filtro.Key == ltrDeVinculos.CgsDelPrincipalDiferentes)
                where = where + $"{Environment.NewLine} and t2.{ICampos.ID_CG} not in ({filtro.Value}){Environment.NewLine}";
            if (filtro.Key == ltrDeVinculos.CgsDelPrincipalIguales)
                where = where + $"{Environment.NewLine} and t2.{ICampos.ID_CG} in ({filtro.Value}){Environment.NewLine}";
            return where;
        }

        private static string IncluirFiltrosPorTipo(string where, KeyValuePair<string, object> filtro)
        {
            if (filtro.Key == ltrDeVinculos.TiposDelVinculadoDiferentes)
                where = where + $"{Environment.NewLine} and t3.{ICampos.ID_TIPO} not in ({filtro.Value}){Environment.NewLine}";
            if (filtro.Key == ltrDeVinculos.TiposDelVinculadoIguales)
                where = where + $"{Environment.NewLine} and t3.{ICampos.ID_TIPO} in ({filtro.Value}){Environment.NewLine}";
            if (filtro.Key == ltrDeVinculos.TiposDelPrincipalDiferentes)
                where = where + $"{Environment.NewLine} and t2.{ICampos.ID_TIPO} not in ({filtro.Value}){Environment.NewLine}";
            if (filtro.Key == ltrDeVinculos.TiposDelPrincipalIguales)
                where = where + $"{Environment.NewLine} and t2.{ICampos.ID_TIPO} in ({filtro.Value}){Environment.NewLine}";
            return where;
        }

        private static string IncluirFiltrosPorEstado(string where, KeyValuePair<string, object> filtro)
        {
            if (filtro.Key == ltrDeVinculos.EstadosDelVinculadoDiferentes)
                where = where + $"{Environment.NewLine} and t3.{ICampos.ID_ESTADO} not in ({filtro.Value}){Environment.NewLine}";
            if (filtro.Key == ltrDeVinculos.EstadosDelVinculadoIguales)
                where = where + $"{Environment.NewLine} and t3.{ICampos.ID_ESTADO} in ({filtro.Value}){Environment.NewLine}";
            if (filtro.Key == ltrDeVinculos.EstadosDelPrincipalDiferentes)
                where = where + $"{Environment.NewLine} and t2.{ICampos.ID_ESTADO} not in ({filtro.Value}){Environment.NewLine}";
            if (filtro.Key == ltrDeVinculos.EstadosDelPrincipalIguales)
                where = where + $"{Environment.NewLine} and t2.{ICampos.ID_ESTADO} in ({filtro.Value}){Environment.NewLine}";
            return where;
        }
    }
}


