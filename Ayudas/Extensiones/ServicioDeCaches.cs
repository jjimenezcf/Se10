using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Utilidades
{
    public enum ModoDescriptor { Mantenimiento, Consulta, ModalDeConsulta, SeleccionarParaFiltrar, Relacion, ParaSeleccionar, Imputar, ConsultarElemento}
    public static class CacheDe
    {
        public static readonly string Fija = nameof(Fija)+"_";

        public static readonly string ValidarJson = nameof(ValidarJson);
        public static readonly string GestorDeRelaciones = nameof(GestorDeRelaciones);
        public static readonly string GestorDePlantillasPorTipo = nameof(GestorDePlantillasPorTipo);
        public static readonly string GestorDeClasesDelTipo = nameof(GestorDeClasesDelTipo);
        public static readonly string DescriptoresJson = nameof(DescriptoresJson);
        public static readonly string LeerVinculosAl = nameof(LeerVinculosAl);
        public static readonly string LeerVinculosCon = nameof(LeerVinculosCon);
        public static readonly string ExisteElVinculo = nameof(ExisteElVinculo);
        public static readonly string ContarVinculosCon = nameof(ContarVinculosCon);
        public static readonly string RenderCrud = nameof(RenderCrud);
        public static readonly string RenderConsulta = nameof(RenderConsulta);
        public static readonly string DescriptorDeCrud = nameof(DescriptorDeCrud);
        public static readonly string ArbolDeMenu = nameof(ArbolDeMenu);

        public static readonly string Fija_TiposReferenciados = nameof(Fija) + nameof(Fija_TiposReferenciados).Replace(nameof(Fija),"");
        public static readonly string Fija_InterfacesUsada = nameof(Fija) + nameof(Fija_InterfacesUsada).Replace(nameof(Fija), "");
        public static readonly string Fija_Metodos = nameof(Fija) + nameof(Fija_Metodos).Replace(nameof(Fija),"");

        public static readonly string Ia_Filtros =  nameof(Ia_Filtros);

        public static readonly string Prefijo_Negocio = "Negocio";
        public static readonly string Negocio_TiposDeAmpliaciones = nameof(Negocio_TiposDeAmpliaciones);
        public static readonly string Negocios_TiposDeDetalles = nameof(Negocios_TiposDeDetalles);
        public static readonly string Negocio_UsaLaAmpliacion = nameof(Negocio_UsaLaAmpliacion);
        public static readonly string Negocio_HayClasesDelNegocio = nameof(Negocio_HayClasesDelNegocio);
        public static readonly string Negocio_HayClasesDelTipo = nameof(Negocio_HayClasesDelTipo);
        public static readonly string Negocio_ParametrosDeNegocio = nameof(Negocio_ParametrosDeNegocio);
        public static readonly string Negocio_ParametrosDeUsuario = nameof(Negocio_ParametrosDeUsuario);
        public static readonly string Negocio_IdsDelTipoConHijos = nameof(Negocio_IdsDelTipoConHijos);
        public static readonly string Negocios_EnumeradoDeUnDtmDto = nameof(Negocios_EnumeradoDeUnDtmDto);
        public static readonly string Negocio_Transiciones = nameof(Negocio_Transiciones);
        public static readonly string Negocio_TransicionesDisponibles = nameof(Negocio_TransicionesDisponibles);
        public static readonly string Negocio_TransicionesHasta = nameof(Negocio_TransicionesHasta);


        public static readonly string Permisos_PorTipo = "PermisosPorTipoSql.PermisosDeUsuario";
        public static readonly string Permisos_PorNegocio = "PermisosPorNegocioSql.PermisosDeUsuario";
        public static readonly string Permisos_PorElemento = "PermisosPorElementoSql.PermisosDeUsuario";
        public static readonly string Permisos_AlgunElemento = "PermisosPorElementoSql.AlgunElemento";
        public static readonly string Permisos_PermisoOtorgado = "PermisosPorElementoSql.PermisoOtorgado";
        public static readonly string Permisos_PorEstado = "PermisosPorEstadoSql.PermisosDeUsuario";
        public static readonly string Permisos_PorTransicion = "PermisosPorTransicionSql.PermisosDeUsuario";
        public static readonly string Permisos_PorCg = "PermisosPorCgSql.PermisosDeUsuario";
        public static readonly string Permisos_TiposConGestion = nameof(Permisos_TiposConGestion);
        public static readonly string Permisos_TieneAlgunPermiso = nameof(Permisos_TieneAlgunPermiso);
        public static readonly string Permisos_TiposConsultor = nameof(Permisos_TiposConsultor);        
        public static readonly string Permisos_CgsConGestion = nameof(Permisos_CgsConGestion);
        public static readonly string Permisos_CgPorNegocio = nameof(Permisos_CgPorNegocio);
        public static readonly string Permisos_CgPorPermisos = nameof(Permisos_CgPorPermisos);

        public static readonly string Ent_PuestosDeUnUsuario = nameof(Ent_PuestosDeUnUsuario);
        public static readonly string Ent_UsuariosDeUnPuesto = nameof(Ent_UsuariosDeUnPuesto);
        public static readonly string Ent_FlujodeAutorizacion = nameof(Ent_FlujodeAutorizacion);
        public static readonly string Ent_UsuariosPorLogin = nameof(Ent_UsuariosPorLogin);

        public static readonly string Ent_MisCorreos_Credenciales = nameof(Ent_MisCorreos_Credenciales);
        public static readonly string Ent_MisCorreos_filtrados = nameof(Ent_MisCorreos_filtrados);
        public static readonly string Ent_MisCorreos_Cantidad = nameof(Ent_MisCorreos_Cantidad);
        public static readonly string Ent_MisCorreos_Del_Buzon = nameof(Ent_MisCorreos_Del_Buzon);
        public static readonly string Ent_MisCorreos_Procesados = nameof(Ent_MisCorreos_Procesados);
        public static readonly string Ent_MisCorreos_Todos = nameof(Ent_MisCorreos_Todos);
        public static readonly string Ent_ReferenciasA = nameof(Ent_ReferenciasA);

        public static readonly string Ent_Bloqueadores = nameof(Ent_Bloqueadores);
        public static readonly string Ent_Desbloqueadores = nameof(Ent_Desbloqueadores);
        public static readonly string Ent_AccesosRecientes = nameof(Ent_AccesosRecientes);
        public static readonly string Ent_SeleccionarPorPropiedad = nameof(Ent_SeleccionarPorPropiedad);        


        public static readonly string ModoAcceso_AlNegocio = $"GestorDeElementos.LeerModoDeAccesoAlNegocio";

        public static readonly string VisorDeAgenda = nameof(VisorDeAgenda);
        public static readonly string EventosMapeado = nameof(EventosMapeado);
        public static readonly string Dtm_ExisteTabla = nameof(Dtm_ExisteTabla);
        public static readonly string ExistePa = nameof(ExistePa);
        public static readonly string Valores = nameof(Valores);
        public static readonly string Estados = nameof(Estados);
        public static readonly string Tipos = nameof(Tipos);
        public static readonly string Estado = nameof(Estado);
        public static readonly string Tipo = nameof(Tipo);
        public static readonly string EstadosSiguientes = nameof(EstadosSiguientes);
        public static readonly string AmpliacionesSinJoin = nameof(AmpliacionesSinJoin);
        public static readonly string AmpliacionesConJoin = nameof(AmpliacionesConJoin);
        public static readonly string Detalle = nameof(Detalle);
        public static readonly string HayDetalle = nameof(HayDetalle);
        public static readonly string VariableDtm = ApiDeEnsamblados.ClaseVariableDtm;
        public static readonly string EsquemaDeTabla = nameof(EsquemaDeTabla);
        public static readonly string NombreDeTabla = nameof(NombreDeTabla);
        public static readonly string PropiedadesDelTipo = nameof(PropiedadesDelTipo);
        public static readonly string Dtm_HayRegistros = nameof(Dtm_HayRegistros);
        

        public static readonly string HeredaDe = nameof(HeredaDe);

        public static readonly string Arc_BloqueoDeArchivos = nameof(Arc_BloqueoDeArchivos);
        public static readonly string Arc_HayCarpetas = nameof(Arc_HayCarpetas);
        public static readonly string Arc_AuditoriaDeArchivo = nameof(Arc_AuditoriaDeArchivo);
        public static readonly string Arc_AnexadosDeUnArchivadorVinculado = nameof(Arc_AnexadosDeUnArchivadorVinculado);
        public static readonly string Arc_Anexados = nameof(Arc_Anexados);
        public static readonly string Arc_AnexadosDto = nameof(Arc_AnexadosDto);
        public static readonly string Arc_AnexadosDtm = nameof(Arc_AnexadosDtm);
        public static readonly string Arc_AnexadosDto_PorGuid = nameof(Arc_AnexadosDto_PorGuid);
        public static readonly string Arc_CantidadDeArchivos = nameof(Arc_CantidadDeArchivos);
        public static readonly string Arc_ListaDeArchivosExt = nameof(Arc_ListaDeArchivosExt);
        public static readonly string Arc_EsCorreo = nameof(Arc_EsCorreo);
        

        public static readonly string ListasDeNegocios = nameof(ListasDeNegocios);
        public static readonly string NegociosQuePermitenSubirDocumentacionDesdeElMovil = nameof(NegociosQuePermitenSubirDocumentacionDesdeElMovil);

        public static readonly string Pldor_TotalSinIva = nameof(Pldor_TotalSinIva);
        public static readonly string Pldor_TotalConIva = nameof(Pldor_TotalConIva);

        public static readonly string Plv_TotalSinIva = nameof(Plv_TotalSinIva);
        public static readonly string Plv_TotalConIva = nameof(Plv_TotalConIva);
        public static readonly string PlvDeUnParteTr = nameof(PlvDeUnParteTr);
        public static readonly string PlvDeUnFactura = nameof(PlvDeUnFactura);

        public static readonly string Fae_BiSinDto = nameof(Fae_BiSinDto);
        public static readonly string Fae_Bi = nameof(Fae_Bi);
        public static readonly string Fae_BiConIva = nameof(Fae_BiConIva);
        public static readonly string Fae_Apagar = nameof(Fae_Apagar);
        public static readonly string Fae_Cobrado = nameof(Fae_Cobrado);
        public static readonly string Fae_TotalDeIva = nameof(Fae_TotalDeIva);
        public static readonly string Fae_TotalDeIrpf = nameof(Fae_TotalDeIrpf);
        public static readonly string Fae_TotalDeDescuento = nameof(Fae_TotalDeDescuento);
        public static readonly string Fae_Anterior = nameof(Fae_Anterior);
        public static readonly string Fae_Abonado = nameof(Fae_Abonado);
        public static readonly string Fae_RectificaA = nameof(Fae_RectificaA);
        public static readonly string Fae_RectificadaPor = nameof(Fae_RectificadaPor);
        


        public static readonly string Far_Iva = nameof(Far_Iva);
        public static readonly string Far_Base = nameof(Far_Base);
        public static readonly string Far_Irpf = nameof(Far_Irpf);
        public static readonly string Far_Incorporadas = nameof(Far_Incorporadas);
        public static readonly string Far_TotalPagado = nameof(Far_TotalPagado);
        public static readonly string Far_PagosNoCancelados = nameof(Far_PagosNoCancelados);
        public static readonly string Far_PagosEnCurso = nameof(Far_PagosEnCurso);
        public static readonly string Far_PagosContadosEnCurso = nameof(Far_PagosContadosEnCurso);
        public static readonly string Fae_FacturaPorNumero = nameof(Fae_FacturaPorNumero);
        public static readonly string Far_QuienMeRectifica = nameof(Far_QuienMeRectifica);
        public static readonly string Far_Impuestos = nameof(Far_Impuestos);
        public static readonly string Far_Naturalezas = nameof(Far_Naturalezas);


        public static readonly string Ter_Bancos = nameof(Ter_Bancos);
        public static readonly string Ter_UsuarioDeCliente = nameof(Ter_UsuarioDeCliente);
        public static readonly string Ter_PuestoDeCliente = nameof(Ter_PuestoDeCliente);
        public static readonly string Ter_EsClienteWeb = nameof(Ter_EsClienteWeb);
        public static readonly string Ter_ExpedientesDeClientes = nameof(Ter_ExpedientesDeClientes);
        public static readonly string Ter_NifDeProveedor = nameof(Ter_NifDeProveedor);
        public static readonly string Ter_NifDeCliente = nameof(Ter_NifDeCliente);
        public static readonly string Ter_Interlocutor = nameof(Ter_Interlocutor);


        public static readonly string Int_Persona = nameof(Int_Persona);
        public static readonly string Int_Sociedad = nameof(Int_Sociedad);
        public static readonly string Int_Proveedor = nameof(Int_Proveedor);  
        public static readonly string Int_Cliente = nameof(Int_Cliente);
        public static readonly string Int_Trabajador = nameof(Int_Trabajador);

        public static readonly string Rem_Fae_Total = nameof(Rem_Fae_Total);
        public static readonly string Rem_Fae_Cobrado = nameof(Rem_Fae_Cobrado);
        public static readonly string Rem_Pag_Total = nameof(Rem_Pag_Total);
        public static readonly string Rem_Pag_Pagado = nameof(Rem_Pag_Pagado);
        public static readonly string Rem_Pag_Hay_Pagos = nameof(Rem_Pag_Hay_Pagos);

        public static readonly string Pag_DatosDelPagoDto = nameof(Pag_DatosDelPagoDto);        

        public static readonly string Ppt_TotalSinIva = nameof(Ppt_TotalSinIva);
        public static readonly string Ppt_TotalConIva = nameof(Ppt_TotalConIva);
        public static readonly string Ppt_Facturado = nameof(Ppt_Facturado);
        public static readonly string Ppt_Ejecutando = nameof(Ppt_Ejecutando);
        public static readonly string Ppt_Ejecutado = nameof(Ppt_Ejecutado);
        public static readonly string Ppt_Prefacturado = nameof(Ppt_Prefacturado);
        public static readonly string Ppt_TieneFacturas = nameof(Ppt_TieneFacturas);
        public static readonly string Ppt_TienePartesTr = nameof(Ppt_TienePartesTr);

        public static readonly string Ptr_TotalSinIva = nameof(Ptr_TotalSinIva);
        public static readonly string Ptr_TotalConIva = nameof(Ptr_TotalConIva);

        public static readonly string Spr_Referenciado = nameof(Spr_Referenciado);       


        public static readonly string Pedido_Total = nameof(Pedido_Total);


        public static readonly string Exp_TienePresupuestos = nameof(Exp_TienePresupuestos);
        public static readonly string Exp_Ingresos = nameof(Exp_Ingresos);
        public static readonly string Exp_Gastos = nameof(Exp_Gastos);
        public static readonly string Exp_Presupuestado = nameof(Exp_Presupuestado);
        public static readonly string Exp_Pagos = nameof(Exp_Pagos);
        public static readonly string Exp_Cobros = nameof(Exp_Cobros);

        public static readonly string Exp_Tareas = nameof(Exp_Tareas);

        public static readonly string epLeerElementos = nameof(epLeerElementos);
        public static readonly string elemento_HitoAnterior_AlActual = nameof(elemento_HitoAnterior_AlActual);
        public static readonly string elemento_HitoAnterior_AlPrimero = nameof(elemento_HitoAnterior_AlPrimero);
        

        public static readonly string Callejero_ZonasDeUnaCalle = nameof(Callejero_ZonasDeUnaCalle);
        public static readonly string Callejero_CpsDeUnaCalle = nameof(Callejero_CpsDeUnaCalle);
        public static readonly string Callejero_BarriosDeUnaCalle = nameof(Callejero_BarriosDeUnaCalle);
        public static readonly string Callejero_Direcciones = nameof(Callejero_Direcciones);
        public static readonly string Callejero_ContarDirecciones = nameof(Callejero_ContarDirecciones);
        
    }

    public static class ServicioDeCaches
    {
        private static Type _clase;
        private static Type Clase()
        {
            if (_clase == null)
                _clase = ApiDeEnsamblados.ObtenerType(ApiDeEnsamblados.DllDelServicioDeDatos, ApiDeEnsamblados.ClaseCacheDeVariable);
            return _clase;
        }

        private static bool? _usaCache;

        //para evitar la recursividad, ya que si se monta un sistema por primera vez, no existe la tabla de variables
        private static bool _estoyConsultandoLaPropiedad;

        public static bool UsaCache
        {
            get
            {
                if (_estoyConsultandoLaPropiedad) _usaCache = false;
                if (_usaCache == null)
                {
                    _estoyConsultandoLaPropiedad = true;



                    PropertyInfo propiedad = Clase().GetProperty(ApiDeEnsamblados.CFG_Usar_Cache);

                    try
                    {
                        _usaCache = (bool)propiedad.GetValue(null);
                    }
                    catch (Exception e)
                    {
                        if (e.InnerException is not null && e.InnerException.Message != "Invalid object name 'ENTORNO.VARIABLE'.")
                            throw;
                        _usaCache = false;
                    }
                    finally
                    {
                        _estoyConsultandoLaPropiedad = false;
                    }

                }
                return _usaCache.Value;
            }
            set
            {
                _usaCache = value;
            }
        }

        public static void InicializarUsaCache()
        {
            _usaCache = null;
            _estoyConsultandoLaPropiedad = false;
        }


        private static bool? _usaCacheParaRenderizar;
        public static bool UsaCacheParaRenderizar
        {
            get
            {
                if (!UsaCache) return false;
                if (_usaCacheParaRenderizar == null)
                {
                    PropertyInfo propiedad = Clase().GetProperty(ApiDeEnsamblados.Cfg_Usar_Cache_Descriptores_Json);
                    _usaCacheParaRenderizar = (bool)propiedad.GetValue(null);
                }
                return _usaCacheParaRenderizar.Value;
            }
            set { _usaCacheParaRenderizar = value; }
        }

        public enum enumTipoCache { Ak, PorPropiedad, PorNombre, PorId }
        public enum enumConJoin { si, no }

        private static ConcurrentDictionary<string, ConcurrentDictionary<string, object>> CachesDeSe = new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();

        public static ConcurrentDictionary<string, object> Obtener(string nombreCache)
        {
            if (!nombreCache.StartsWith(CacheDe.VariableDtm))
                ValidarUsoDeCache(nombreCache);

            if (!CachesDeSe.ContainsKey(nombreCache))
                CachesDeSe[nombreCache] = new ConcurrentDictionary<string, object>();

            return CachesDeSe[nombreCache];
        }

        private static void ValidarUsoDeCache(string nombreCache)
        {
            if (nombreCache == "Assembly") return;

            if (!UsaCache)
            {
                EliminarCache(nombreCache);
                return;
            }

            if ((nombreCache == CacheDe.RenderCrud || nombreCache == CacheDe.DescriptorDeCrud) && !UsaCacheParaRenderizar)
            {
                EliminarCache(nombreCache);
                return;
            }

            return;
        }

        public static void EliminarCaches(string comienzoNombreCache)
        {
            foreach (var cache in CachesDeSe.Keys)
                if (cache.StartsWith(comienzoNombreCache))
                    CachesDeSe[cache].Clear();
        }

        public static void EliminarTodas()
        {
            foreach (var cache in CachesDeSe.Keys)
            {
                if (cache.StartsWith(CacheDe.Fija))
                    continue;

                CachesDeSe[cache].Clear();
            }

        }

        public static void EliminarCache(string nombreCache)
        {
            if (CachesDeSe.ContainsKey(nombreCache))
                CachesDeSe[nombreCache].Clear();
        }

        public static void EliminarCachesDelTipo(Type tipo)
        {
            var fullname = tipo.FullName;
            EliminarCache(PorAk(fullname));
            EliminarCache(PorPropiedad(fullname));
            EliminarCache(PorId(fullname));
            EliminarCache(PorNombre(fullname));
            EliminarElemento(CacheDe.Dtm_HayRegistros, fullname);
        }

        public static void EliminarCachesDeDescriptores()
        {
            EliminarCache(CacheDe.RenderCrud);
            EliminarCache(CacheDe.RenderConsulta);
            EliminarCache(CacheDe.DescriptorDeCrud);
        }

        public static void EliminarCachesDeDescriptores(string indice)
        {
            EliminarElementos(CacheDe.RenderCrud, indice);
            EliminarElementos(CacheDe.DescriptorDeCrud, indice);
        }

        public static void EliminarElemento(string cache, string clave)
        {
            Obtener(cache).EliminarElemento(clave);
        }

        public static void EliminarElementos(string cache, string patron)
        {
            var cacheDeRegistros = Obtener(cache);
            if (cacheDeRegistros == null) return;

            // Extraemos las claves que coinciden primero
            var clavesAEliminar = cacheDeRegistros.Keys
                .Where(k => k.Contains(patron))
                .ToList();

            // Iteramos sobre la lista estática, no sobre la colección viva
            foreach (var clave in clavesAEliminar)
            {
                cacheDeRegistros.TryRemove(clave, out _);
            }
        }

        public static void EliminarElementosComiencenPor(string cache, string patron)
        {
            var cacheDeRegistros = Obtener(cache);

            // 1. Verificación de nulidad
            if (cacheDeRegistros == null || string.IsNullOrEmpty(patron))
                return;

            // 2. Extraer claves a una lista para evitar excepciones de modificación de colección
            // 3. Usar StringComparison para mayor control/rendimiento
            var clavesAEliminar = cacheDeRegistros.Keys
                .Where(k => k.StartsWith(patron, StringComparison.Ordinal))
                .ToList();

            foreach (var clave in clavesAEliminar)
            {
                cacheDeRegistros.TryRemove(clave, out _);
            }
        }

        public static string PorPropiedad(string nombreDeCache)
        {
            return $"{nombreDeCache}-{enumTipoCache.PorPropiedad}";
        }

        public static string PorAk(string nombreDeCache)
        {
            return $"{nombreDeCache}-{enumTipoCache.Ak}";
        }

        public static string PorNombre(string nombreDeCache)
        {
            return $"{nombreDeCache}-{enumTipoCache.PorNombre}";
        }

        public static string PorId(string nombreDeCache)
        {
            return $"{nombreDeCache}-{enumTipoCache.PorId}";
        }

        public static ConcurrentDictionary<string, object> ObtenerCache(string nombreCache, string propiedad)
        {
            if (propiedad.Equals("Id", StringComparison.CurrentCultureIgnoreCase))
                return Obtener(PorId(nombreCache));

            if (propiedad.Equals("Nombre", StringComparison.CurrentCultureIgnoreCase))
                return Obtener(PorNombre(nombreCache));

            return Obtener(PorPropiedad(nombreCache));
        }
    }

    public static class ApiDeCaches
    {
        public static void EliminarElemento(this ConcurrentDictionary<string, object> cache, string clave)
        {
            foreach (var indice in cache.Keys.Where(indice => clave == indice))
            {
                cache.TryRemove(indice, out _);
            }
        }
    }
}
