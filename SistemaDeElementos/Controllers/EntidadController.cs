using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.SistemaDocumental;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto;
using MVCSistemaDeElementos.Descriptores;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers;

public enum epAcciones { buscar, siguiente, anterior, ultima, ordenar, irA, historial }

[Authorize]
public class EntidadController<TContexto, TRegistro, TElemento> : BaseController<TElemento>
    where TContexto : ContextoSe
    where TRegistro : RegistroDtm
    where TElemento : ElementoDto
{

    protected GestorDeElementos<TContexto, TRegistro, TElemento> _GestorDeElementos { get; }


    public EntidadController(GestorDeElementos<TContexto, TRegistro, TElemento> gestorDeElementos, GestorDeErrores gestorErrores)
        : base(gestorErrores, gestorDeElementos.Contexto, gestorDeElementos.Mapeador)
    {
        _GestorDeElementos = gestorDeElementos;

    }

    public JsonResult epCrearElementoPorPost(int idNegocio)
    {
        var body = ApiController.LeerBody(HttpContext);
        return epCrearElemento(idNegocio, body.parametros[ltrParametrosEp.ElementoJson].ToString(), body.parametros[ltrParametrosEp.ParametrosDeCreacion].ToString());
    }

    public JsonResult epCrearElemento(int idNegocio, string elementoJson, string parametrosDeCreacion)
    {
        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epCrearElemento));
        try
        {
            Contexto.AnotarParametros(new List<object> { elementoJson });
            var elementos = ApiController.CrearElemento(_GestorDeElementos, elementoJson, parametrosDeCreacion, HttpContext, AntesDeEjecutar_CrearElemento);
            return elementos;
        }
        finally
        {
            Contexto.CerrarTraza();
        }
    }

    protected virtual ParametrosDeNegocio AntesDeEjecutar_CrearElemento(TElemento elemento)
    {
        AsignarNegocioAlGestorGenerico(elemento);
        var parametros = new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        parametros.EsUnaPeticion = true;
        return parametros;
    }

    protected virtual ParametrosDeNegocio AntesDeEjecutar_ModificarElemento(TElemento elemento)
    {
        AsignarNegocioAlGestorGenerico(elemento);
        var parametros = new ParametrosDeNegocio(enumTipoOperacion.Modificar);
        parametros.EsUnaPeticion = true;
        return parametros;
    }

    protected override TElemento LeerPorId(int id, Dictionary<string, object> parametros)
    {
        if (ApiDeEnsamblados.ImplementaInterface(_GestorDeElementos.GetType(), typeof(IGestorGenerico).FullName))
        {
            var idNegocio = (int)parametros.LeerValor<long>(ltrParametrosEp.idNegocio);
            var negocio = NegociosDeSe.ToEnumerado(idNegocio);
            ((IGestorGenerico)_GestorDeElementos).AsignarNegocio(negocio);
        }

        return _GestorDeElementos.LeerElementoPorId(id, parametros);
    }

    public JsonResult epModificarRelacionPorPost(int idNegocio)
    {
        var body = ApiController.LeerBody(HttpContext);
        return ApiController.PersistirElemento(_GestorDeElementos, body.parametros[ltrParametrosEp.ElementoJson].ToString(), HttpContext, AntesDeEjecutar_ModificarRelacion);
    }

    protected override ParametrosDeNegocio AntesDeEjecutar_ModificarRelacion(TElemento dto)
    {
        AsignarNegocioAlGestorGenerico(dto);
        return base.AntesDeEjecutar_ModificarRelacion(dto);
    }

    [HttpPost]
    public JsonResult epModificarPorPost(int idNegocio)
    {
        var body = ApiController.LeerBody(HttpContext);
        return epModificarPorId(body.parametros[ltrParametrosEp.ElementoJson].ToString(), body.parametrosJson);
    }

    public JsonResult epModificarPorId(string elementoJson, string parametrosJson)
    {
        var r = new Resultado();
        var parametros = Utilidades.extJson.ToDiccionarioDeParametros(parametrosJson);
        parametros[ltrParametrosNeg.EsUnaPeticion] = true;
        parametros[ltrParametrosNeg.Peticion] = enumPeticion.epModificarPorId;

        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epModificarPorId));
        var tran = _GestorDeElementos.IniciarTransaccion();
        try
        {
            Contexto.AnotarParametros(new List<object> { elementoJson });
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            var elemento = JsonConvert.DeserializeObject<TElemento>(elementoJson);
            var parametrosDeNegocio = AntesDeEjecutar_ModificarPorId(elemento, parametros);
            if (parametros.LeerValor(ltrParametrosEp.HayCambios, true))
                r.Datos = _GestorDeElementos.PersistirElementoDto(elemento, parametrosDeNegocio);
            else
            {
                r.Datos = elemento;
                r.Consola = $"El registro de la clase {typeof(TRegistro).Name} con {elemento.Id} no tenía modificaciones pendientes";
            }

            r.Estado = enumEstadoPeticion.Ok;
            r.Mensaje = "Registro modificado";

            if (r.Estado == enumEstadoPeticion.Ok)
                _GestorDeElementos.Commit(tran);
            else
                _GestorDeElementos.Rollback(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, "No se ha podido modificar.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }

        return new JsonResult(r);
    }

    public JsonResult epLeerAmpliacionPorNegocio(string negocio, int idElemento, string parametrosJson)
    {
        var idnegocio = NegociosDeSe.LeerNegocioPorEnumerado(ApiDeEnsamblados.ToEnumerado<enumNegocio>(negocio)).Id;
        return epLeerAmpliacionPorIdNegocio(idnegocio, idElemento, parametrosJson);
    }

    public JsonResult epLeerAmpliacionPorIdNegocio(int idNegocio, int idElemento, string parametrosJson)
    {
        var r = new Resultado();
        var tran = _GestorDeElementos.IniciarTransaccion();
        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerAmpliacionPorIdNegocio));
        try
        {
            Contexto.AnotarParametros(new List<object> { idNegocio, idElemento, parametrosJson });
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            var parametros = parametrosJson.ToDiccionarioDeParametros();
            parametros[ltrParametrosNeg.EsUnaPeticion] = true;
            var usaAmpliacion = NegociosDeSe.ToEnumerado(idNegocio).UsaLaAmpliacionDe(Contexto, (int)(long)parametros.LeerValor(ltrParametrosEp.idTipo, (long)0), typeof(TRegistro));
            if (usaAmpliacion)
            {
                r.Datos = Contexto.SeleccionarDtoPorAk<TElemento, TRegistro>(nameof(IAmpliacionDto.IdElemento), idElemento, errorSiNoHay: false);
                r.ModoDeAcceso = r.Datos == null
                    ? ApiDePermisos.LeerModoDeAcceso(Contexto, NegociosDeSe.ToEnumerado(idNegocio), idElemento).Render()
                    : ((enumModoDeAccesoDeDatos)r.Datos.ModoDeAcceso).Render();
            }
            else
            {
                //Indicamos que se ha de ocultar la ampliación en el cliente
                r.Datos = null;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.SinPermiso.Render();
            }
            r.Estado = enumEstadoPeticion.Ok;
            r.Consola = $"ampliación '{typeof(TRegistro).Name}' leida";
            r.Mensaje = "";
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, "No se ha podido modificar.");
        }

        return new JsonResult(r);
    }


    [HttpPost]
    public JsonResult epPersistirAmpliacionPorPost(int idNegocio)
    {
        var body = ApiController.LeerBody(HttpContext);
        return epPersistirAmpliacion(idNegocio, body.parametros[ltrParametrosEp.ElementoJson].ToString());
    }

    public JsonResult epPersistirAmpliacion(int idNegocio, string elementoJson)
    {
        var r = new Resultado();
        var tran = _GestorDeElementos.IniciarTransaccion();

        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epPersistirAmpliacion));
        try
        {
            Contexto.AnotarParametros(new List<object> { idNegocio, elementoJson });
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            var elemento = JsonConvert.DeserializeObject<TElemento>(elementoJson);
            elemento = AntesDeEjecutar_PersistirAmpliacion(elemento);
            var p = new ParametrosDeNegocio(elemento.Id > 0 ? enumTipoOperacion.Modificar : enumTipoOperacion.Insertar)
            {
                Peticion = enumPeticion.epPersistirAmpliacion
            };
            _GestorDeElementos.PersistirElementoDto(elemento, p);
            r.Estado = enumEstadoPeticion.Ok;
            r.Mensaje = "ampliación actualizada";
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, "No se ha podido modificar.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }

        return new JsonResult(r);
    }

    protected virtual ParametrosDeNegocio AntesDeEjecutar_ModificarPorId(TElemento elemento, Dictionary<string, object> parametros)
    {
        AsignarNegocioAlGestorGenerico(elemento);
        var paramDeNegocio = new ParametrosDeNegocio(enumTipoOperacion.Modificar, parametros)
        {
            EsUnaPeticion = true
        };
        return paramDeNegocio;
    }

    protected virtual TElemento AntesDeEjecutar_PersistirAmpliacion(TElemento elemento)
    {
        var registro = Contexto.SeleccionarPorFk<TRegistro>(nameof(IAmpliacionDto.IdElemento), ((IAmpliacionDto)elemento).IdElemento, errorSiNoHay: false);
        if (registro != null)
        {
            elemento.Id = registro.Id;
        }

        return elemento;
    }

    //END-POINT: Desde Grid.ts
    public JsonResult epBorrarPorId(string idsJson, string parametrosJson = null)
    {
        var listaIds = JsonConvert.DeserializeObject<List<int>>(idsJson);
        var parametros = parametrosJson.ToDiccionarioDeParametros();
        parametros[ltrParametrosNeg.EsUnaPeticion] = true;

        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epBorrarPorId));
        try
        {
            Contexto.AnotarParametros(new List<object> { idsJson, parametrosJson });
            return BorrarIds(listaIds, parametros);
        }
        finally
        {
            Contexto.CerrarTraza();
        }
    }

    protected JsonResult BorrarIds(List<int> listaIds, Dictionary<string, object> parametros)
    {
        var r = new Resultado();
        var tran = _GestorDeElementos.IniciarTransaccion();
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            parametros[ltrParametrosNeg.UsarLaCache] = false;
            foreach (var id in listaIds)
            {
                var elemento = LeerPorId(id, parametros);
                var p = AntesDeEjecutar_BorrarPorId(elemento);
                _GestorDeElementos.PersistirElementoDto(elemento, p);
            }
            r.Estado = enumEstadoPeticion.Ok;
            r.Mensaje = listaIds.Count > 1 ? "Registros eliminados" : "Registro eliminado";
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, "No se ha podido eliminar.");
        }

        return new JsonResult(r);
    }

    protected virtual ParametrosDeNegocio AntesDeEjecutar_BorrarPorId(TElemento elemento)
    {
        AsignarNegocioAlGestorGenerico(elemento);
        var parametros = new ParametrosDeNegocio(enumTipoOperacion.Eliminar);
        parametros.EsUnaPeticion = true;
        parametros.Peticion = enumPeticion.epBorrarPorId;
        return parametros;
    }

    [HttpPost]
    public async Task<JsonResult> epLeerDatosPost(string modo, string accion, string posicion, string cantidad)
    {
        var body = ApiController.LeerBody(HttpContext);
        return await epLeerDatosParaElGrid(modo, accion, posicion, cantidad, body.parametros.LeerValor<string>(ltrParametrosEp.Filtro), body.parametros.LeerValor<string>(ltrParametrosEp.Orden), body.parametrosJson);
    }


    public async Task<JsonResult> epLeerDatosParaElGrid(string modo, string accion, string posicion, string cantidad, string filtro, string orden, string parametrosJson)
    {
        var r = new Resultado();
        var pos = posicion.Entero();
        var can = cantidad.Entero();

        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerDatosParaElGrid));
        try
        {
            var parametros = parametrosJson.ToDiccionarioDeParametros();
            parametros[ltrParametrosNeg.EsUnaPeticion] = true;
            var seleccionadas = (string)parametros.LeerValor(ltrParametrosEp.Seleccionadas, "");
            var filtrosDePantalla = filtro == null ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtro);
            var filtrosDeIa = new List<ClausulaDeFiltrado>();
            string textoNatural = parametros.LeerValor(ltrParametrosEp.fraseDeFiltrado, "");

            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            Contexto.AnotarParametros(new List<object> { modo, accion, posicion, cantidad, filtro, orden, });
            if (parametros.LeerValor(ltrParametrosEp.filtrarConIa, false))
            {
                var negocio = NegociosDeSe.ToEnumerado(typeof(TRegistro), errorSiNoEsUnNegocio: false);
                var cache = ServicioDeCaches.Obtener(CacheDe.Ia_Filtros);
                var guid = parametros.LeerValor<string>(ltrParametrosEp.guid);
                var indice = $"{guid}_{textoNatural.ToLower().Replace(" ","")}";
                var nuevaPregunta = parametros.LeerValor<bool>(ltrParametrosEp.nuevaPregunta);
                if (nuevaPregunta)
                    ServicioDeCaches.EliminarElementosComiencenPor(CacheDe.Ia_Filtros, guid);
                if (!cache.ContainsKey(indice))
                {
                    var preguntaEnBd = nuevaPregunta ? IaPreguntaSql.BuscarPregunta(Contexto, textoNatural): null;
                    string filtrosIaJson;

                    if (preguntaEnBd != null)
                    {
                        filtrosIaJson = preguntaEnBd.Respuesta;
                        cache[indice] = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>((string)filtrosIaJson);
                    }
                    else
                    {
                        var cgs = Contexto.SeleccionarTodos<CentroGestorDtm>(new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado { Clausula = nameof(CentroGestorDtm.Baja), Criterio = enumCriteriosDeFiltrado.igual, Valor = false.ToString() } }).ToList();
                        var tipos = negocio.Tipos(Contexto).ToList();
                        var estados = negocio.Estados(Contexto).ToList();
                        var transiciones = negocio.ListaDeTransiciones(Contexto).ToList();

                        string listaDeCgs = $"{nameof(CentroGestorDtm.Id)}| {nameof(CentroGestorDtm.Nombre)}{Environment.NewLine}" + string.Join("\n", cgs.Select(cg => $"{cg.Id}|{cg.Nombre}"));

                        string listaDeTipos = $"{nameof(ITipoDeElementoDtm.Id)}| {nameof(ITipoDeElementoDtm.Nombre)}{Environment.NewLine}" + string.Join("\n", tipos.Select(t => $"{t.Id}|{t.Nombre}"));

                        string listaDeEstados = $"{nameof(EstadoDtm.Id)}| {nameof(EstadoDtm.Nombre)}| {nameof(EstadoDtm.Inicial)}| {nameof(EstadoDtm.Terminado)}| {nameof(EstadoDtm.Cancelado)}{Environment.NewLine}" + string.Join("\n",
                            estados.Select(e => $"{e.Id}|{e.Nombre}|{e.Inicial}|{e.Terminado}|{e.Cancelado}"));

                        string listaDeTransiciones = $"{nameof(TransicionDtm.Id)}| {nameof(TransicionDtm.Nombre)}| {nameof(TransicionDtm.IdOrigen)}| {nameof(TransicionDtm.IdDestino)}{Environment.NewLine}" + string.Join("\n",
                            transiciones.Select(t => $"{t.Id}|{t.Nombre}|{t.IdOrigen}|{t.IdDestino}"));

                        string listaDeEtapas = string.Join(Environment.NewLine, negocio.ListaDeEtapas(erroSiNoHay: false).Select((EtapaDto e) => (string)$"{e.nombre}|{e.descripcion}"));
                        string etapas = $"Etapa|Descripción{Environment.NewLine}" + listaDeEtapas;
                        var iaUsada = ExtensorDeUsuarios.IaUsada(Contexto);
                        var ia = ExtensorDeIa.CrearIa(iaUsada);
                        filtrosIaJson = await ExtensorDeIa.AnalizarTextoParaFiltros(Contexto, negocio, ia, textoNatural, listaDeCgs, listaDeTipos, listaDeEstados, listaDeTransiciones, etapas, guid, nuevaPregunta);
                        cache[indice] = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>((string)filtrosIaJson);

                    }

                    IaPreguntaSql.GrabarPregunta(Contexto, new IaPreguntaDtm
                    {
                        Guid = guid,
                        IdUsuario = Contexto.Usuario.Id,
                        Fecha = DateTime.Now,
                        Pregunta = textoNatural,
                        Respuesta = filtrosIaJson
                    });
                }
                filtrosDeIa = (List<ClausulaDeFiltrado>)cache[indice];
                filtro = Filtrar.FusionarFiltros(filtrosDePantalla, filtrosDeIa);
            }

            parametros[ltrParametrosNeg.Peticion] = enumPeticion.epLeerDatosParaElGrid;
            var datos = (elementos: (IEnumerable<IElementoDto>)null, total: 0);
            dynamic infoObtenida;
            var mensajeInfo = "";
            if (accion == epAcciones.historial.ToString())
            {
                datos = _GestorDeElementos.LeerHistorial(pos, can, parametros);
                infoObtenida = new ResultadoDeLectura<HistorialDto>(datos.elementos, pos, can, datos.total);
            }
            else if (VistaConDiferenteDto(parametros))
            {
                var resultado = LeerOtrosElementos(pos, can, filtro, orden, seleccionadas, parametros);
                datos = (resultado.elementos, resultado.total);
                infoObtenida = resultado.infoObtenida;
                var prop = resultado.infoObtenida.GetType().GetProperty("Mensaje");
                mensajeInfo = prop != null ? prop.GetValue(resultado.infoObtenida)?.ToString() : "";
            }
            else
            {
                datos = ApiController.LeerDatosParaElGrid(
                    () => Leer(pos, can, filtro, orden, seleccionadas, parametros),
                    () => accion == epAcciones.buscar.ToString() ? Contar(filtro, parametros) : Recontar(filtro, parametros));
                infoObtenida = new ResultadoDeLectura<TElemento>(datos.elementos, pos, can, datos.total);
            }
            r.Datos = infoObtenida;
            r.Estado = enumEstadoPeticion.Ok;
            r.Mensaje = !mensajeInfo.IsNullOrEmpty() ? mensajeInfo : pos > 0 && !datos.elementos.Any() ? "No hay más elementos" : "";

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            r.Consola = JsonConvert.SerializeObject(new ResultadoConsola
            {
                FiltrosDeIa = filtrosDeIa,
                FiltrosDePantalla = filtrosDePantalla,
                TextoNatural = textoNatural
            }, Formatting.Indented, settings);

        }
        catch (Exception e)
        {
            Contexto.AnotarExcepcion(e);
            ApiController.PrepararError(e, r, "No se ha podido recuperar datos para el grid.");
            r.Consola = r.Consola + Environment.NewLine + filtro;
        }
        finally
        {
            Contexto.CerrarTraza();
        }

        var a = new JsonResult(r);
        return a;
    }

    protected virtual (IEnumerable<IElementoDto> elementos, int total, dynamic infoObtenida)
    LeerOtrosElementos(int pos, int can, string filtro, string orden, string seleccionadas, Dictionary<string, object> parametros)
    {
        throw new NotImplementedException();
    }

    private bool VistaConDiferenteDto(Dictionary<string, object> parametros)
    {
        var accionVista = parametros.LeerValor(ltrParametrosEp.Vista, "");
        if (!accionVista.IsNullOrEmpty())
        {
            var vista = Contexto.SeleccionarPorPropiedad<VistaMvcDtm>(nameof(VistaMvcDtm.Accion), accionVista);
            return vista.ElementoDto != typeof(TElemento).FullName;
        }
        return false;
    }

    public JsonResult epLeerElemento(string filtrosJson, string parametrosJson)
    {
        var r = new Resultado();
        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerElemento));
        try
        {

            Contexto.AnotarParametros(new List<object> { filtrosJson, parametrosJson });
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            var parametros = parametrosJson.ToDiccionarioDeParametros();
            parametros[nameof(ltrParametrosNeg.Peticion)] = enumPeticion.epLeerElemento;
            List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosJson);
            var orden = new List<ClausulaDeOrdenacion>();
            AntesDeEjecutar_Leer(0, 1, filtros, orden, parametros);
            bool crearSiNohay = HayQueCrearSiNoSeEncuentra(parametros);
            parametros.Add(ltrParametrosNeg.AplicarJoin, true);
            var elementos = _GestorDeElementos.LeerElementos(0, 2, filtros, orden, parametros);

            var emitirErrorSiHayMasDeUno = parametros.LeerValor(ltrParametrosEp.ErrorSiHayMasDeUno, true);
            var emitirErrorSiNoHay = parametros.LeerValor(ltrParametrosEp.ErrorSiNoHay, true);
            var soloActivos = parametros.LeerValor(ltrParametrosEp.SoloActivos, true);

            if (crearSiNohay)
                elementos = CrearElementoSiNoSeEncuentra(elementos, parametros);

            if (elementos.Count() == 0 && emitirErrorSiNoHay)
            {
                if (parametros.ContainsKey(ltrParametrosEp.MensajeSiNoHay))
                {
                    GestorDeErrores.Emitir(parametros[ltrParametrosEp.MensajeSiNoHay].ToString());
                }
                else
                {
                    if (filtros.Any(filtro => filtro.Clausula.Equals(nameof(INombre.Nombre), StringComparison.CurrentCultureIgnoreCase) &&
                    filtro.Criterio.Equals(enumCriteriosDeFiltrado.igual)))
                    {
                        GestorDeErrores.Emitir($"{_GestorDeElementos.Negocio.IniciarFrase()} '{filtros.First(f => f.Clausula.Equals(nameof(INombre.Nombre), StringComparison.CurrentCultureIgnoreCase)).Valor}' no se ha localizado en la base de datos");
                    }
                    else
                        GestorDeErrores.Emitir($"No se han encontrado el elemento seleccionado según el filtro {filtrosJson}");
                }
            }

            if (elementos.Count() > 1 && emitirErrorSiHayMasDeUno)
            {
                if (parametros.ContainsKey(ltrParametrosEp.MensajeSiHayMasDeUno))
                {
                    GestorDeErrores.Emitir(parametros[ltrParametrosEp.MensajeSiHayMasDeUno].ToString());
                }
                else
                {
                    GestorDeErrores.Emitir($"Hay más de un elemento para el filtro {filtrosJson}");
                }
            }

            if (elementos.Count() == 1 && soloActivos)
            {
                if (elementos.First().GetType().ImplementaElementoDeUnProceso() && elementos.First().EstaCancelada)
                {
                    GestorDeErrores.Emitir($"{_GestorDeElementos.Negocio.IniciarFrase()} '{((IUsaNombreDto)elementos.First()).Nombre}' está cancelado");
                }

                if (elementos.First().GetType().ImplementaBajaDto() && ((IUsaBajaDto)elementos.First()).Baja)
                {
                    GestorDeErrores.Emitir($"{_GestorDeElementos.Negocio.IniciarFrase()} '{((IUsaNombreDto)elementos.First()).Nombre}' está de baja");
                }
            }

            //var infoParaDevolver = ElementosLeidos(elementos.ToList());
            r.Datos = elementos.Count() == 0 ? null : elementos.ToList()[0]; // infoParaDevolver.ToList()[0];
            r.Estado = enumEstadoPeticion.Ok;
            r.Mensaje = parametros.ContainsKey(ltrParametrosEp.MensajeSiNoHay) ? parametros[ltrParametrosEp.MensajeSiNoHay].ToString() : null;
        }
        catch (Exception e)
        {
            ApiController.PrepararError(e, r, "Error al leer.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }
        return new JsonResult(r);
    }

    protected virtual IEnumerable<TElemento> CrearElementoSiNoSeEncuentra(IEnumerable<TElemento> elementosLeidos, Dictionary<string, object> parametros)
    {
        throw new Exception($"Debe indicar como crear el elemento de la clase '{typeof(TElemento).Name}' si no se encuentra el buscado");
    }

    protected virtual bool HayQueCrearSiNoSeEncuentra(Dictionary<string, object> parametros)
    {
        return false;
    }

    [HttpPost]
    public JsonResult epExportar(int idNegocio, int idPlantilla)
    {
        var r = new Resultado();
        var body = ApiController.LeerBody(HttpContext);
        body.parametros[nameof(ElementoDto)] = typeof(TElemento).Name;
        body.parametros[nameof(RegistroDtm)] = typeof(TRegistro).Name;
        body.parametros[ltrParametrosNeg.Peticion] = enumPeticion.epExportar;

        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epExportar));
        try
        {
            Contexto.AnotarParametros(new List<object> { body.parametrosJson });
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            var idCg = (int)body.parametros.LeerValor<long>(ltrParametrosEp.IdCg);
            var archivador = body.parametros.LeerValor<string>(ltrParametrosEp.Archivador);
            var motivo = body.parametros.LeerValor<string>(ltrParametrosEp.Motivo);
            if (idNegocio > 0 && (idCg == 0 || archivador.IsNullOrEmpty() || motivo.IsNullOrEmpty()))
                GestorDeErrores.Emitir("Debe indicar el cg donde almacenar el archivador, el nombre y el motivo de la exportación");

            body.parametros[ltrParametrosEp.IdPlantilla] = idPlantilla;
            body.parametros[ltrParametrosEp.idNegocio] = idNegocio;
            body.parametros[ltrParametrosEp.Sometido] = true;
            if (idPlantilla == 0)
            {
                r.Datos = ExportacionEstandar(r, body.parametros);
                r.Mensaje = r.Datos == null ? "Trabajo sometido correctamente" : "Exportado";
            }
            else
            {
                r.Datos = GestorDePlantillasDeExportacion.Exportar(Contexto, idNegocio, idPlantilla, body.parametros);
                r.Mensaje = "Trabajo sometido correctamente";
            }
            r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
            r.Estado = enumEstadoPeticion.Ok;
        }
        catch (Exception e)
        {
            ApiController.PrepararError(e, r, "Error al exportar.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }
        return new JsonResult(r);
    }

    private string ExportacionEstandar(Resultado r, Dictionary<string, object> parametros)
    {
        if (parametros.LeerValor(ltrParametrosEp.Sometido, false))
        {
            ServidorDocumental.SometerExportacion(Contexto, parametros.ToJson());
            return null;
        }

        parametros.Add(ltrParametrosNeg.AplicarJoin, true);
        var cantidad = !parametros.ContainsKey(ltrFiltros.cantidad) ? -1 : parametros[ltrFiltros.cantidad].ToString().Entero();
        var posicion = !parametros.ContainsKey(ltrFiltros.posicion) ? 0 : parametros[ltrFiltros.posicion].ToString().Entero();
        List<ClausulaDeFiltrado> filtros = !parametros.ContainsKey(ltrFiltros.filtro) || parametros[ltrFiltros.filtro].ToString().IsNullOrEmpty() ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(parametros.LeerValor<string>(ltrParametrosEp.Filtro));
        List<ClausulaDeOrdenacion> orden = !parametros.ContainsKey(ltrFiltros.orden) || parametros[ltrFiltros.orden].ToString().IsNullOrEmpty() ? new List<ClausulaDeOrdenacion>() : JsonConvert.DeserializeObject<List<ClausulaDeOrdenacion>>(parametros.LeerValor<string>(ltrParametrosEp.Orden));
        AntesDeEjecutar_Leer(posicion, cantidad, filtros, orden, parametros);
        var elementos = _GestorDeElementos.LeerElementos(posicion, cantidad, filtros, orden, parametros);
        return ServidorDocumental.DescargarExcel(Contexto, elementos.ToList());
    }

    protected override IEnumerable<TElemento> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> parametros)
    {
        AntesDeEjecutar_Leer(posicion, cantidad, filtros, orden, parametros);
        var leidos = ApiController.LeerElementos(_GestorDeElementos, posicion, cantidad, filtros, orden, parametros);
        var procesados = DespuesDeEjecutar_Leer(leidos, filtros, parametros);
        return procesados;
    }


    //END-POINT: Desde ModalSeleccion.ts
    public JsonResult epLeerParaSelector(string filtro)
    {
        var r = new Resultado();
        List<TElemento> elementos;
        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerParaSelector));
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            elementos = Leer(0, 2, filtro, null, null, new Dictionary<string, object>()).ToList();
            r.Datos = elementos;
            r.Estado = enumEstadoPeticion.Ok;
        }
        catch (Exception e)
        {
            ApiController.PrepararError(e, r, "No se ha podido leer los datos.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }

        return new JsonResult(r);
    }

    public JsonResult epCargarLista(string claseElemento, string negocio, string filtro)
    {
        var r = new Resultado();
        dynamic elementos;

        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epCargarLista));
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            List<ClausulaDeFiltrado> filtros = filtro == null ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtro);
            elementos = CargarLista(claseElemento, NegociosDeSe.ToEnumerado(negocio, nullValido: true), filtros);
            r.Datos = elementos;
            r.Estado = enumEstadoPeticion.Ok;
        }
        catch (Exception e)
        {
            ApiController.PrepararError(e, r, "No se ha podido leer los datos.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }

        return new JsonResult(r);
    }

    public JsonResult epCargaDinamica(string claseElemento, int posicion, int cantidad, string filtrosJson)
    {
        var r = new Resultado();
        dynamic elementos;

        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epCargaDinamica));
        try
        {
            List<ClausulaDeFiltrado> filtros = filtrosJson == null ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosJson);

            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            elementos = CargaDinamica(claseElemento, posicion, cantidad, filtros);
            r.Datos = elementos;
            r.Estado = enumEstadoPeticion.Ok;
        }
        catch (Exception e)
        {
            ApiController.PrepararError(e, r, "No se ha podido leer los datos.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }

        return new JsonResult(r);
    }

    public JsonResult epLeerTipos(string parametrosJson)
    {
        var r = new Resultado();
        Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            if (!parametros.ContieneClave(nameof(ltrParametrosEp.Modo))) GestorDeErrores.Emitir("Para leer los tipo debe indicar el modo de acceso que requiere");
            if (!parametros.ContieneClave(nameof(ltrParametrosEp.enumNegocio))) GestorDeErrores.Emitir("Para leer los tipo debe indicar de que negocio");
            var modo = parametros.LeerValor<enumModoDeAccesoDeDatos>(nameof(ltrParametrosEp.Modo));
            var negocio = parametros.LeerValor<enumNegocio>(nameof(ltrParametrosEp.enumNegocio));

            var tipos = modo == enumModoDeAccesoDeDatos.Gestor
                ? ApiDeTipos.TiposConPermisoDeGestor(Contexto, negocio, parametros)
                : ApiDeTipos.TiposConPermisoDeConsultor(Contexto, negocio, parametros);
            r.Datos = tipos.Count > 0 ? tipos : null;
            r.Consola = $"tipos con permisos de {modo} leidos";
            r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
            r.Estado = enumEstadoPeticion.Ok;
        }
        catch (Exception e)
        {
            var modo = parametros.LeerValor<enumModoDeAccesoDeDatos>(nameof(ltrParametrosEp.enumNegocio));
            var negocio = parametros.LeerValor<enumNegocio>(nameof(ltrParametrosEp.enumNegocio));
            ApiController.PrepararError(e, r, $"error al acceder a leer los tipos con permisos de '{modo}' del negocio '{negocio}'.");
        }
        return new JsonResult(r);
    }

    public JsonResult epLeerModoDeAccesoAlElemento(string negocio, int id)
    {
        var r = new Resultado();

        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerModoDeAccesoAlElemento));
        try
        {
            var modoDeAcceso = enumModoDeAccesoDeDatos.SinPermiso;
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            var opcionesDeMapeo = new Dictionary<string, object>();

            modoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, NegociosDeSe.ToEnumerado(negocio), id);
            if (modoDeAcceso == enumModoDeAccesoDeDatos.SinPermiso)
            {
                GestorDeErrores.Emitir("El usuario conectado no tiene acceso al elemento solicitado");
            }

            var elemento = _GestorDeElementos.LeerElementoPorId(id, opcionesDeMapeo);

            r.Datos = elemento;
            r.ModoDeAcceso = modoDeAcceso.Render();
            r.Consola = $"El usuario {DatosDeConexion.Login} tiene permisos de {modoDeAcceso} sobre el elemento seleccionado";
            r.Estado = enumEstadoPeticion.Ok;
        }
        catch (Exception e)
        {
            ApiController.PrepararError(e, r, $"Error al obtener los permisos sobre el elemento {id} del {negocio} para el usuario {DatosDeConexion.Login}.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }

        return new JsonResult(r);
    }

    public JsonResult epBloquear(string parametrosJson)
    {
        var r = new Resultado();
        Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
        Contexto.IniciarTraza(nameof(epBloquear));
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            if (!parametros.ContieneClave(nameof(ltrParametrosEp.idElemento))) GestorDeErrores.Emitir("Debe indicar el id del elemento");
            if (!parametros.ContieneClave(nameof(ltrParametrosEp.Bloquear))) GestorDeErrores.Emitir("Debe indicar si se quiere bloquear o desbloquear");
            if (!parametros.ContieneClave(nameof(ltrParametrosEp.RowVersion))) GestorDeErrores.Emitir("Debe indicar la versión del registro");
            if (!typeof(TRegistro).ImplementaUsaBloqueo()) GestorDeErrores.Emitir($"la clase '{typeof(TRegistro).Name}' no implementa bloqueos");

            var idElemento = (int)parametros.LeerValor<long>(nameof(ltrParametrosEp.idElemento));
            var bloquear = parametros.LeerValor<bool>(nameof(ltrParametrosEp.Bloquear));
            var rowVersion = parametros.LeerValor<string>(nameof(ltrParametrosEp.RowVersion));
            var registro = Contexto.SeleccionarPorId<TRegistro>(idElemento);

            string rowVersionBase64 = Convert.ToBase64String(((IElementoDtm)registro).RowVersion);
            if (rowVersionBase64 != rowVersion)
                throw new Exception($"El registro ya ha sido modificado por otro usuario, vueva a leerlo");

            if (bloquear)
                ((IUsaBloqueo)registro).Bloquear<TRegistro>(Contexto);
            else
                ((IUsaBloqueo)registro).Desbloquear<TRegistro>(Contexto);

            r.Estado = enumEstadoPeticion.Ok;
        }
        catch (Exception exc)
        {
            Contexto.AnotarExcepcion(exc);
            ApiController.PrepararError(exc, r, "Error al bloquear");
        }
        finally
        {
            Contexto.CerrarTraza();
        }
        var a = new JsonResult(r);
        return a;
    }

    public JsonResult epObtenerUrlAlExpediente(string parametrosJson)
    {
        var r = new Resultado();

        Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

            var enumerado = parametros.LeerValor<string>(ltrParametrosEp.enumNegocio);
            var negocio = NegociosDeSe.ToEnumerado(enumerado);
            var idRegistro = (int)parametros.LeerValor<long>(ltrParametrosEp.id);
            var registro = negocio.LeerRegistro(Contexto, idRegistro);

            if (!registro.GetType().ImplementaUsaExpediente() && !registro.GetType().ImplementaUsaPresupuesto())
                GestorDeErrores.Emitir($"El negocio '{negocio.Singular()}' no usa '{enumNegocio.Expediente.Singular()}'");

            if (registro.GetType().ImplementaUsaExpediente() && ((IUsaExpediente)registro).IdExpediente.Entero() == 0)
                GestorDeErrores.Emitir($"No se ha podido obtener el acceso al expediente de '{negocio.Singular()}: {((IUsaReferencia)registro).Referencia}'");

            if (registro.GetType().ImplementaUsaPresupuesto() && ((IUsaPresupuesto)registro).IdPresupuesto.Entero() == 0)
                GestorDeErrores.Emitir($"No se ha podido obtener el acceso al expediente de '{negocio.Singular()}: {((IUsaReferencia)registro).Referencia}'");
            var idExpediente = 0;
            if (registro.GetType().ImplementaUsaExpediente()) idExpediente = (int)((IUsaExpediente)registro).IdExpediente;
            else
            {
                var idPresupuesto = ((IUsaPresupuesto)registro).IdPresupuesto.Entero();
                var ppt = Contexto.SeleccionarPorId<PresupuestoDtm>(idPresupuesto);
                if (ppt.IdExpediente is null)
                    GestorDeErrores.Emitir($"No se ha podido obtener el acceso al expediente de '{negocio.Singular()}: {((IUsaReferencia)registro).Referencia}'");
                idExpediente = (int)ppt.IdExpediente;

            }

            r.Datos = $"{enumNegocio.Expediente.Controlador()}/{enumNegocio.Expediente.VistaMvc(Contexto).Accion}?Id={idExpediente}";
            r.Consola = $"Origen obtenido correctamente";
            r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
            r.Estado = enumEstadoPeticion.Ok;
        }
        catch (Exception e)
        {
            ApiController.PrepararError(e, r, "No se puede obtener acceso al expediente.");
        }
        return new JsonResult(r);
    }

    public JsonResult epObtenerUrlAlProveedor(string parametrosJson)
    {
        var r = new Resultado();

        Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

            var enumerado = parametros.LeerValor<string>(ltrParametrosEp.enumNegocio);
            var negocio = NegociosDeSe.ToEnumerado(enumerado);
            var idRegistro = (int)parametros.LeerValor<long>(ltrParametrosEp.id);
            var registro = negocio.LeerRegistro(Contexto, idRegistro);

            if (!registro.GetType().ImplementaUsaProveedor())
                GestorDeErrores.Emitir($"El negocio '{negocio.Singular()}' no usa '{enumNegocio.Proveedor.Singular()}'");

            r.Datos = $"{enumNegocio.Proveedor.Controlador()}/{enumNegocio.Proveedor.VistaMvc(Contexto).Accion}?Id={((IUsaProveedor)registro).IdProveedor}";
            r.Consola = $"Origen obtenido correctamente";
            r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
            r.Estado = enumEstadoPeticion.Ok;
        }
        catch (Exception e)
        {
            ApiController.PrepararError(e, r, "No se puede obtener acceso al proveedor.");
        }
        return new JsonResult(r);
    }

    public JsonResult epObtenerUrlAlCliente(string parametrosJson)
    {
        var r = new Resultado();

        Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

            var enumerado = parametros.LeerValor<string>(ltrParametrosEp.enumNegocio);
            var negocio = NegociosDeSe.ToEnumerado(enumerado);
            var idRegistro = (int)parametros.LeerValor<long>(ltrParametrosEp.id);
            var registro = negocio.LeerRegistro(Contexto, idRegistro);

            if (!registro.GetType().ImplementaUsaCliente())
                GestorDeErrores.Emitir($"El negocio '{negocio.Singular()}' no usa '{enumNegocio.Cliente.Singular()}'");

            r.Datos = $"{enumNegocio.Cliente.Controlador()}/{enumNegocio.Cliente.VistaMvc(Contexto).Accion}?Id={((IUsaCliente)registro).IdCliente}";
            r.Consola = $"Origen obtenido correctamente";
            r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
            r.Estado = enumEstadoPeticion.Ok;
        }
        catch (Exception e)
        {
            ApiController.PrepararError(e, r, "No se puede obtener acceso al cliente.");
        }
        return new JsonResult(r);
    }


    [AllowAnonymous]
    public JsonResult epValidarConsultaPorGuid(int id, string parametrosJson)
    {
        var r = new Resultado();
        Contexto.IniciarTraza(nameof(epValidarConsultaPorGuid));
        try
        {
            Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
            ValidarConsultaPorGuid(id, parametrosJson);
        }
        catch (Exception e)
        {
            ApiController.PrepararError(e, r, $"Error al validar el Guid para consulta.");
        }
        finally
        {
            Contexto.CerrarTraza();
            Contexto.QuitarUsuario();
        }
        return new JsonResult(r);
    }


    public JsonResult epRegistrarConsultaConGuid(int id, DateTime? caducaEl)
    {
        var resultado = new Resultado();
        var tran = Contexto.IniciarTransaccion();
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            var registro = _GestorDeElementos.LeerRegistroPorId(id, false, false, false, false);

            var negocio = NegociosDeSe.NegocioDeUnDtm(typeof(TRegistro));
            if (caducaEl == null)
                caducaEl = DateTime.Now.AddDays(30);

            resultado.Datos = GestorDeConsultasConGuid.RegistrarConsultaConGuid((IElementoDtm)registro, Contexto, caducaEl, maximoDeDescargas: null);
            resultado.Mensaje = resultado.Datos == null
                ? "Se han desactivado las consultas por guid del elemento seleccionado"
                : $"La URL para consultar el elemento se ha copiado al porta papeles con validez hasta '{((DateTime)caducaEl).ToString(extFechas.DiaHora)}'";
            resultado.Estado = enumEstadoPeticion.Ok;
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, resultado, $"Error al generar un Guid para consulta.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }

        return new JsonResult(resultado);
    }

    [AllowAnonymous]
    public IActionResult Consultar(string guid, int id)
    {
        Contexto.IniciarTraza(nameof(Consultar));
        var indice = $"{nameof(Consultar)}-{typeof(TRegistro).Name}";
        var cache = ServicioDeCaches.Obtener(CacheDe.RenderConsulta);
        try
        {
            Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
            var negocio = NegociosDeSe.ToEnumerado(typeof(TRegistro));
            GestorDeConsultasConGuid.ValidarGuid(contexto: Contexto, negocio, id, guid);
            if (!cache.ContainsKey(indice))
            {
                var descriptor = DescriptorDePaginaDeConsulta.CrearDescriptorDeConsulta(Contexto, negocio, id);
                cache[indice] = ViewPagina((DescriptorDePaginaDeConsulta)descriptor);
            }
            return (ViewResult)cache[indice];
        }
        catch (Exception e)
        {
            ServicioDeCaches.EliminarCache(CacheDe.RenderConsulta);
            return RenderMensaje(e.MensajeCompleto());
        }
        finally
        {
            Contexto.CerrarTraza();
            Contexto.QuitarUsuario();
        }
    }

    protected virtual dynamic CargaDinamica(string claseElemento, int posicion, int cantidad, List<ClausulaDeFiltrado> filtros)
    {
        throw new Exception($"Debe implementar la función de CargaDinamica para la clase '{claseElemento}' en el controlador '{this.GetType().Name}'");
    }

    protected virtual dynamic CargarLista(string claseElemento, enumNegocio negocio, List<ClausulaDeFiltrado> filtros)
    {
        //if (claseElemento == nameof(ExportacionDto))
        //    return GestorDeExportaciones.LeerTipos(Contexto, claseElemento, negocio, filtros);

        throw new Exception($"Debe implementar la función de CargaDeElementos para la clase '{claseElemento}' en el controlador '{GetType().Name}'");
    }

    public int Contar(string filtro = null, Dictionary<string, object> parametros = null)
    {
        List<ClausulaDeFiltrado> filtros = filtro.IsNullOrEmpty()
            ? new List<ClausulaDeFiltrado>()
            : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtro);

        return _GestorDeElementos.Contar(filtros, new ParametrosDeNegocio(enumTipoOperacion.Contar, parametros));
    }


    public int Recontar(string filtro = null, Dictionary<string, object> parametros = null)
    {
        List<ClausulaDeFiltrado> filtros = filtro.IsNullOrEmpty()
            ? new List<ClausulaDeFiltrado>()
            : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtro);

        return _GestorDeElementos.Recontar(filtros, new ParametrosDeNegocio(enumTipoOperacion.Contar, parametros));
    }

    protected IEnumerable<TElemento> Leer(int posicion, int cantidad, string filtro, string orden, string seleccionadas, Dictionary<string, object> parametros)
    {
        parametros.Add(ltrParametrosNeg.AplicarJoin, true);
        List<ClausulaDeFiltrado> filtros = filtro == null ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtro);
        List<ClausulaDeOrdenacion> ordenes = orden == null ? new List<ClausulaDeOrdenacion>() : JsonConvert.DeserializeObject<List<ClausulaDeOrdenacion>>(orden);

        if (ordenes.Count == 0 && ApiDeInterfaceDtm.ImplementaNombre(typeof(TRegistro)))
        {
            ordenes.Add(new ClausulaDeOrdenacion() { OrdenarPor = nameof(RegistroConNombreDtm.Nombre), Modo = ModoDeOrdenancion.ascendente });
        }

        if (!seleccionadas.IsNullOrEmpty())
        {
            var f = new ClausulaDeFiltrado();
            f.Clausula = nameof(RegistroDtm.Id);
            var lista = JsonConvert.DeserializeObject<List<int>>(seleccionadas);
            foreach (var valor in lista)
            {
                if (valor > 0)
                {
                    f.Valor = $"{f.Valor},{valor}";
                }
            }

            if (f.Valor.Length > 0)
            {
                f.Criterio = enumCriteriosDeFiltrado.esAlgunoDe;
                f.Valor = f.Valor.Substring(1, f.Valor.Length - 1);
            }
            else
            {
                f.Criterio = enumCriteriosDeFiltrado.igual;
                f.Valor = "0";
            }
            filtros.Add(f);
        }
        AntesDeEjecutar_Leer(posicion, cantidad, filtros, ordenes, parametros);
        return _GestorDeElementos.LeerElementos(posicion, cantidad, filtros, ordenes, parametros);

    }


    protected virtual void AntesDeEjecutar_Leer(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> ordenes, Dictionary<string, object> parametros)
    {
        if (ApiDeEnsamblados.ImplementaInterface(_GestorDeElementos.GetType(), typeof(IGestorGenerico).FullName))
        {
            var idNegocio = ApiController.BuscarNegocio(filtros, parametros);
            if (idNegocio == 0)
            {
                GestorDeErrores.Emitir("Debe indicar el negocio a leer");
            }
            ((IGestorGenerico)_GestorDeElementos).AsignarNegocio(NegociosDeSe.ToEnumerado(idNegocio));
        }
    }


    protected virtual IEnumerable<TElemento> DespuesDeEjecutar_Leer(IEnumerable<TElemento> leidos, List<ClausulaDeFiltrado> filtros, Dictionary<string, object> parametros)
    {
        return leidos;
    }

    private void AsignarNegocioAlGestorGenerico(TElemento elemento)
    {
        if (ApiDeEnsamblados.ImplementaInterface(_GestorDeElementos.GetType(), typeof(IGestorGenerico).FullName))
        {
            if (!ApiDeEnsamblados.TienenLaPropiedad(typeof(TElemento), ltrParametrosEp.idNegocio))
            {
                GestorDeErrores.Emitir($"El objeto {typeof(TElemento)} debe tener la propiedad {ltrParametrosEp.idNegocio}");
            }

            if (((IUsaNegocioDto)elemento).IdNegocio == 0)
            {
                GestorDeErrores.Emitir($"El elemento debe indicar el negocio");
            }
            ((IGestorGenerico)_GestorDeElementos).AsignarNegocio(NegociosDeSe.ToEnumerado(((IUsaNegocioDto)elemento).IdNegocio));

        }
    }

    [HttpGet("movil/[controller]/{id:int}")]
    public async Task<JsonResult> GetEntity([FromRoute] int id) => await GetElementoAsync(id);

    protected virtual Task<JsonResult> GetElementoAsync(int id)
    {
        var ret = epLeerPorId(id);
        var result = (Resultado)ret.Value;
        if (result is not null && result.Estado == enumEstadoPeticion.Ok)
        {
            result.Datos = Mapeador.Map<TElemento, ElementoMovilOutput>(result.Datos);
        }

        return Task.FromResult(ret);
    }

    [HttpPost("movil/[controller]/search")]
    public async Task<JsonResult> GetAll([FromBody] LeerDatosParaElGridParam input) => await GetElementosAsync(input);

    protected virtual async Task<JsonResult> GetElementosAsync(LeerDatosParaElGridParam input)
    {
        //modo=Mantenimiento&accion=buscar&posicion=0&cantidad=10&filtro=[]&orden=[{"ordenarPor":"cg.codigo","modo":"ascendente"},{"ordenarPor":"tipo.nombre","modo":"ascendente"},{"ordenarPor":"referencia","modo":"ascendente"}]&parametrosJson=[]
        var options = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };
        var filters = input.Filtro is not null ? JsonConvert.SerializeObject(input.Filtro, options) : null;
        var sorting = input.Orden is not null ? JsonConvert.SerializeObject(input.Orden, options) : null;
        var parameters = input.Parametros is not null ? JsonConvert.SerializeObject(input.Parametros, options) : null;


        var ret = await epLeerDatosParaElGrid(enumModoDeTrabajo.Mantenimiento.ToString(), input.Accion.ToString(), input.Posicion.ToString(), input.Cantidad.ToString(), filters, sorting, parameters);
        return MappearResultadoToElementoMovilOutput(ret);
    }

    [HttpGet("movil/[controller]/{negocio}/{idElemento:int}")]
    public Task<JsonResult> GetLinksWith([FromRoute] int idElemento, [FromRoute] string negocio)
    {
        if (Enum.TryParse(negocio, true, out enumNegocio enumNegocio))
        {
            var ret = epLeerVinculosCon(enumNegocio.IdNegocio(), _GestorDeElementos.Negocio.IdNegocio(), idElemento, null);
            return Task.FromResult(MappearResultadoToElementoMovilOutput(ret));
        }

        var r = new Resultado
        {
            Estado = enumEstadoPeticion.Error,
            Mensaje = "Error al parsear el negocio"
        };

        return Task.FromResult(new JsonResult(r));
    }

    private JsonResult MappearResultadoToElementoMovilOutput(JsonResult ret)
    {
        var result = (Resultado)ret.Value;
        if (result is not null && result.Estado == enumEstadoPeticion.Ok)
        {
            if (result.Datos is ResultadoDeLectura<TElemento> a)
            {
                result.Datos = new ResultadoDeLectura<ElementoMovilOutput>()
                {
                    cantidad = a.cantidad,
                    posicion = a.posicion,
                    total = a.total,
                    registros = Mapeador.Map<List<TElemento>, List<ElementoMovilOutput>>(a.registros)
                };
            }
            else
            {
                result.Datos = Mapeador.Map<List<TElemento>, List<ElementoMovilOutput>>(result.Datos);
            }
        }

        return ret;
    }


    protected override Dictionary<string, object> IndicadoresParaInicializarLaVistaMnt(ContextoSe contexto, Dictionary<string, object> parametros)
    {
        var indicadores = base.IndicadoresParaInicializarLaVistaMnt(contexto, parametros);
        var usaTotalizador = ApiParaDtos.ImplementaITotalizador(_GestorDeElementos.GetType());
        if (usaTotalizador)
        {
            indicadores.Add(IndCrud.UsaTotalizador, true);
        }

        return indicadores;
    }

}