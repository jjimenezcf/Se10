using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Contabilidad;
using GestoresDeNegocio.Gastos;
using GestoresDeNegocio.SistemaDocumental;
using Inicializador.SistemaDocumental;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto;
using ModeloDeDto.Contabilidad;
using ModeloDeDto.SistemaDocumental;
using MVCSistemaDeElementos.Descriptores;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class CircuitosDocController : EntidadController<ContextoSe, CircuitoDocDtm, CircuitoDocDto>
    {
        public CircuitosDocController(GestorDeCircuitosDoc gestorDeCircuitosDoc, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeCircuitosDoc,
           gestorDeErrores
         )
        {
        }

        protected override Dictionary<string, object> IndicadoresParaInicializarLaVistaMnt(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var indicadores = base.IndicadoresParaInicializarLaVistaMnt(contexto, parametros);

            if (parametros.LeerValor(ltrParametrosEp.Descriptor, string.Empty) == typeof(DescriptorDeEstimacionesDirectas).Name)
            {
                var idTipo = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaEstimacionDirecta(errorSiNoEstaDefinido: true);
                indicadores.Add(IndCircuitosDoc.IdTipoEstimacionDirecta, idTipo);
                indicadores.Add(IndCircuitosDoc.TipoEstimacionDirecta, contexto.SeleccionarPorId<TipoDeCircuitoDocDtm>(idTipo).Nombre);
            }
            else if (parametros.LeerValor(ltrParametrosEp.Descriptor, string.Empty) == typeof(DescriptorDeLotesContables).Name)
            {
                var idTipo = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaLoteDePreasientos(errorSiNoEstaDefinido: true);
                indicadores.Add(IndCircuitosDoc.IdTipoLoteContable, idTipo);
                indicadores.Add(IndCircuitosDoc.TipoLoteContable, contexto.SeleccionarPorId<TipoDeCircuitoDocDtm>(idTipo).Nombre);
            }
            else if (parametros.LeerValor(ltrParametrosEp.Descriptor, string.Empty) == typeof(DescriptorDeFichadas).Name)
            {
                var idTipo = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaFichada(errorSiNoEstaDefinido: true);
                indicadores.Add(IndCircuitosDoc.IdTipoFichada, idTipo);
                indicadores.Add(IndCircuitosDoc.TipoFichada, contexto.SeleccionarPorId<TipoDeCircuitoDocDtm>(idTipo).Nombre);
            }
            else if (parametros.LeerValor(ltrParametrosEp.Descriptor, string.Empty) == typeof(DescriptorDeActividadesFormativas).Name)
            {
                var idTipo = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaActividadesFormativas(errorSiNoEstaDefinido: true);
                indicadores.Add(IndCircuitosDoc.IdTipoActividadFormativa, idTipo);
                indicadores.Add(IndCircuitosDoc.TipoActividadFormativa, contexto.SeleccionarPorId<TipoDeCircuitoDocDtm>(idTipo).Nombre);
            }


            return indicadores;
        }

        public IActionResult CrudActividadesFormativas()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeActividadesFormativas).FullName}";
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.SistemaDocumental}/{nameof(CrudActividadesFormativas)}";
                    return base.View(destino, new DescriptorDeActividadesFormativas(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<CircuitoDocDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeActividadesFormativas(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        public IActionResult CrudFichadas()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeFichadas).FullName}";
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.SistemaDocumental}/{nameof(CrudFichadas)}";
                    return base.View(destino, new DescriptorDeFichadas(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<CircuitoDocDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeFichadas(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        public IActionResult CrudEstimacionesDirectas()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeEstimacionesDirectas).FullName}";
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.SistemaDocumental}/{nameof(CrudEstimacionesDirectas)}";
                    return base.View(destino, new DescriptorDeEstimacionesDirectas(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<CircuitoDocDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeEstimacionesDirectas(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        public IActionResult CrudLotesContables()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeLotesContables).FullName}";
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.SistemaDocumental}/{nameof(CrudLotesContables)}";
                    return base.View(destino, new DescriptorDeLotesContables(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<CircuitoDocDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeLotesContables(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        public IActionResult CrudCircuitosDoc()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeCircuitosDoc).FullName}";
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.SistemaDocumental}/{nameof(CrudCircuitosDoc)}";
                    return base.View(destino, new DescriptorDeCircuitosDoc(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<CircuitoDocDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeCircuitosDoc(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        public IActionResult MaestrosDeCircuitosDoc()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzCircuitosDoc.ModeloDeCircuitosDoc(Contexto);
                InzFichadas.ModeloDeFichadas(Contexto);
                ViewBag.Mensaje = "Maestros inicializados";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                return RenderMensaje($"No se ha podido inicializar los maestros.{Environment.NewLine}{GestorDeErrores.Detalle(e)}");
            }
            finally
            {
                ServicioDeCaches.EliminarTodas();
            }
            return VistaDelPanelDeControl(Contexto);
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Spr_CrearLote:
                    return null;

                case eventosDeMf.Spr_AnularLote:
                    var idsLc = (List<int>)parametros[ltrParametrosEp.ids];
                    if (idsLc.Count != 1) GestorDeErrores.Emitir("Solo ha de indicar el id de un lote contable");
                    TrabajosContables.SometerAnularLoteContable(Contexto, idsLc[0]);
                    return null;

                case eventosDeMf.Spr_RegenerarLote:
                    var idsGen = (List<int>)parametros[ltrParametrosEp.ids];
                    if (idsGen.Count != 1) GestorDeErrores.Emitir("Solo ha de indicar el id de un lote contable");
                    ((GestorDeCircuitosDoc)_GestorDeElementos).RegenerarLoteContable(Contexto, idsGen[0]);
                    return null;

                case eventosDeMf.Spr_AnularEstimacionDirecta:
                    var idsEd = (List<int>)parametros[ltrParametrosEp.ids];
                    if (idsEd.Count != 1) GestorDeErrores.Emitir("Solo ha de indicar el id de un lote de estimación directa");
                    TrabajosContables.SometerAnularEstimacionDirecta(Contexto, idsEd[0]);
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        public JsonResult epCrearLoteContable(string parametrosJson)
        {
            var r = new Resultado();

            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idSociedad = (int)parametros.LeerValor<long>(nameof(CrearLoteContableDto.IdSociedad));
                var ejercicio = (int)parametros.LeerValor<long>(nameof(CrearLoteContableDto.Ejercicio));
                var fechaContable = parametros.LeerValor<DateTime?>(nameof(CrearLoteContableDto.FechaContable), null);
                var filtros = parametros.LeerValor<string>(ltrFiltros.filtro);
                var clausulas = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtros);
                var clausula = clausulas.FirstOrDefault(c => c.Clausula.ToLower() == ltrParametrosEp.idTipo.ToLower());
                if (clausula is null)
                {
                    GestorDeErrores.Emitir("No se ha indicado el tipo de circuito a crear");
                }
                var descontabilizar = parametros.LeerValor<bool>(nameof(CrearLoteContableDto.Descontabilizar));
                var respetarFechaContable = parametros.LeerValor<bool>(nameof(CrearLoteContableDto.RespetarFechaContable)); 
                if (ExtensoresDeCircuitosDoc.EsEstimacionDirecta(clausula.Valor.Entero()))
                {
                    if (descontabilizar)
                        GestorDeErrores.Emitir("Ha solicitado crear una estimación directa y ha añadido información sobre descontabilizar, en una estimación esto no se puede indicar");
                    if (respetarFechaContable)
                        GestorDeErrores.Emitir("Ha solicitado crear una estimación directa y ha añadido información sobre respetar fecha contable, en una estimación esto no se puede indicar");
                    if (fechaContable.HasValue)
                        GestorDeErrores.Emitir("Ha solicitado crear una estimación directa y ha añadido información sobre la fecha contable, en una estimación esto no se puede indicar");
                    respetarFechaContable = true;
                    fechaContable = null;
                }

                filtros = filtros.Replace(ltrParametrosEp.idTipo.ToLower(), ltrDeUnCircuito.IdTipoCircuito.ToLower());
                TrabajosContables.SometerCrearLoteContable(Contexto, idSociedad, ejercicio, descontabilizar, respetarFechaContable, fechaContable, filtros);

                r.Mensaje = $"Trabajo de creación de lote contable sometido correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No ha podido someter la creación del lote contable.");
            }
            return new JsonResult(r);
        }

        public JsonResult epFichar(double? latitud = null, double? longitud = null)
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var tipos = enumNegocio.CircuitoDoc.Parametro(enumParametrosDeCircuitosDoc.CAD_IdsDeTiposDeFichadas, crearParametro: true, valorPorDefecto: Literal.Cero).Valor.ToLista<int>(quitarNegativos: true);

                if (tipos.Count > 1) throw new Exception("Hay definidas más de un tipo de fichadas, no se puede usar esta opción");
                if (tipos.Count == 0) throw new Exception("No hay parametrizada ningún tipo de fichada");

                var trabajador = Contexto.Trabajador();
                var ultimaFichada = trabajador.UltimaFichada(Contexto, tipos[0]);

                bool crearFichada = ultimaFichada == null || (ultimaFichada != null && ultimaFichada.Estado(Contexto).Terminado);

                if (crearFichada)
                {
                    ultimaFichada = new CircuitoDocDtm
                    {
                        IdCg = trabajador.IdCg,
                        IdTipo = tipos[0],
                        Nombre = trabajador.Nombre,
                    }.InsertarComoAdministrador(Contexto);

                    PosicionDeFichada(ultimaFichada, "Localización de entrada", latitud, longitud);

                    r.Mensaje = $"Fichada '{ultimaFichada.Referencia}' abierta";
                }
                else
                {
                    var estadosEtapa = enumEtapasDeCircuitosDoc.CAD_Etapa_Cerrado.EstadosDeLaEtapa();

                    PosicionDeFichada(ultimaFichada, "Localización de salida", latitud, longitud);

                    ultimaFichada.TransitarALaEtapa(Contexto, estadosEtapa);
                    r.Mensaje = $"Fichada '{ultimaFichada.Referencia}' cerrada";
                }


                r.Estado = enumEstadoPeticion.Ok;
                r.Datos = @$"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{nameof(CircuitosDocController).Replace("Controller", "")}/{nameof(CrudCircuitosDoc)}?Id={ultimaFichada.Id}";
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"Error al hacer la fichada.{Environment.NewLine}{GestorDeErrores.Detalle(e)}");
            }
            finally
            {
                var indice = $"{Contexto.DatosDeConexion.IdUsuario}-";
                ServicioDeCaches.EliminarCachesDeDescriptores(indice);
            }
            return new JsonResult(r);
        }

        private void PosicionDeFichada(CircuitoDocDtm circuito, string nombre, double? latitud, double? longitud)
        {
            string contenido;

            if (latitud.HasValue && longitud.HasValue)
            {
                var coordenadas = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", latitud, longitud);
                Uri uriMapa = new Uri($"https://www.google.com/maps?q={coordenadas}");
                contenido = uriMapa.ToString();
            }
            else
            {
                contenido = "No se informó de la posición de la fichada";
            }

            circuito.CrearObservacion(Contexto, nombre, contenido, new Dictionary<string, object>
            {
                { ltrDeObservaciones.CreadaPorAdminSe, true }
            });
        }

        protected override void AntesDeEjecutar_Leer(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> ordenes, Dictionary<string, object> parametros)
        {
            base.AntesDeEjecutar_Leer(posicion, cantidad, filtros, ordenes, parametros);
            if (parametros.LeerValor(ltrParametrosEp.Vista, string.Empty).ToLower() == nameof(CrudCircuitosDoc).ToLower())
            {
                if (!filtros.Any(f => f.Clausula.ToLower() == nameof(IUsaTipo.IdTipo).ToLower()) && !filtros.Any(f => f.Clausula.ToLower() == nameof(IRegistro.Id).ToLower()))
                {
                    var valor = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaEstimacionDirecta(errorSiNoEstaDefinido: false).ToString() + Simbolos.separadorDeEnteros +
                                VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaFichada(errorSiNoEstaDefinido: false).ToString() + Simbolos.separadorDeEnteros +
                                VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaLoteDePreasientos(errorSiNoEstaDefinido: false).ToString() + Simbolos.separadorDeEnteros +
                                VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaActividadesFormativas(errorSiNoEstaDefinido: false).ToString();

                    filtros.Add(new ClausulaDeFiltrado
                    {
                        Clausula = nameof(IUsaTipo.IdTipo),
                        Criterio = enumCriteriosDeFiltrado.noEsNingunoDe,
                        Valor = valor
                    });
                }
            }
        }
    }
}
