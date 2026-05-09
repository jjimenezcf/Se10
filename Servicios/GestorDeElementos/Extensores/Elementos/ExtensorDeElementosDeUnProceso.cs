using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilidades;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeElementosDeUnProceso
    {
        public static int IdDelEstadoInicial<T>(this T elemento, ContextoSe contexto)
        where T : IElementoDeProcesoDtm
        =>
        elemento.Tipo == null
        ? ((TipoConFlujoDtm)NegociosDeSe.NegocioDeUnDtm(elemento.GetType()).CrearGestorDeTipo(contexto).LeerRegistroPorId(elemento.IdTipo, true)).Estado.Id
        : ((IUsaEstado)elemento.Tipo).IdEstado;

        public static EstadoDtm Estado<T>(this T elemento, ContextoSe contexto)
        where T : IElementoDeProcesoDtm
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            return negocio.Estado(contexto, elemento.IdEstado);
        }

        public static (TransicionAplicable aplicada, T elemento) IntentarAplicarTransicion<T>(this T elemento, ContextoSe contexto, List<TransicionAplicable> transicionesAplicables, Dictionary<string, object> parametros = null, bool errorSiNoSeAplica = true)
        where T : IElementoDeProcesoDtm
        {
            if (transicionesAplicables.Count == 0)
                GestorDeErrores.Emitir($"No hay transiciones posibles a aplicar al elemento '{elemento.Referencia}' del negocio '{typeof(T).NegocioDeUnDtm().Singular(true)}'");

            foreach (var transicion in transicionesAplicables.Where(transicion => elemento.IdEstado.Equals(transicion.IdEstado)))
            {
                var tran = contexto.IniciarTransaccion();
                try
                {
                    elemento = elemento.Transitar(contexto, transicion.IdTransicion, parametros);
                    contexto.Commit(tran);
                    return (transicion, elemento);
                }
                catch
                {
                    contexto.Rollback(tran);
                    throw;
                }
            }

            if (errorSiNoSeAplica)
                GestorDeErrores.Emitir($"No se ha podido aplicar al elemento '{elemento.Referencia}' del negocio {elemento.GetType().NegocioDeUnDtm()} ninguna transición desde el estado '{elemento.Estado(contexto).Nombre}'");

            return (null, elemento);
        }

        public static HitoDtm UltimoHito<T>(this T elemento, ContextoSe contexto)
        where T : IElementoDeProcesoDtm
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            return HitoSql.LeerUltimoHito(contexto, negocio.TablaDeHitos(), ((IRegistro)elemento).Id)[0];
        }

        public static List<HitoDtm> HitosDeUnaEtapaPosteriorA<T>(this T elemento, ContextoSe contexto, List<int> etapaRestrictora, List<int> etapaDeFiltrado)
        where T : IElementoDeProcesoDtm
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            return HitoSql.LeerHitosDeUnaEtapaPosteriorA(contexto, negocio.TablaDeHitos(), ((IRegistro)elemento).Id, etapaRestrictora, etapaDeFiltrado);
        }

        public static IEnumerable<HitoDtm> Hitos<T>(this T elemento, ContextoSe contexto, List<int> estados = null)
        where T : IElementoDeProcesoDtm
        {
            IEnumerable<HitoDtm> hitos;
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            if (estados == null)
                hitos = negocio.Hitos(contexto).Where(x => x.IdElemento == elemento.Id).OrderByDescending(x => x.Fecha);
            else
                hitos = negocio.Hitos(contexto).Where(x => x.IdElemento == elemento.Id && estados.Contains(x.IdEstado)).OrderByDescending(x => x.Fecha); 
            return hitos;
        }

        public static HitoDtm HitoAnteriorAlActual<T>(this T elemento, ContextoSe contexto, bool errorSiNoHay = true)
        where T : IElementoDeProcesoDtm
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            var cache = ServicioDeCaches.Obtener(CacheDe.elemento_HitoAnterior_AlActual);
            var i = $"{negocio.ToString()}-{((IRegistro)elemento).Id}-{((IElementoDeProcesoDtm)elemento).IdEstado}";
            if (!cache.ContainsKey(i))
            {
                var hito = HitoSql.LeerAnteriorHito(contexto, negocio.TablaDeHitos(), ((IRegistro)elemento).Id);
                if (errorSiNoHay && hito == null)
                    GestorDeErrores.Emitir("El elemento solicitado sólo tiene historia inicial");

                if (hito != null)
                    cache[i] = hito;
                else
                    return null;
            }
            return (HitoDtm)cache[i];
        }

        public static HitoDtm HitoAnteriorAlPrimero<T>(this T elemento, ContextoSe contexto, bool errorSiNoHay = true)
        where T : IElementoDeProcesoDtm
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            var cache = ServicioDeCaches.Obtener(CacheDe.elemento_HitoAnterior_AlPrimero);
            var i = $"{negocio.ToString()}-{((IElementoDeProcesoDtm)elemento).Id}-{((IElementoDeProcesoDtm)elemento).IdEstado}";
            if (!cache.ContainsKey(i))
            {
                var hitos = elemento.Hitos(contexto).OrderBy(h => h.Fecha).ToList();
                var primerHito = hitos.First(h => h.IdEstado == elemento.IdEstado);
                var anterior = hitos.Where(h => h.Fecha <= primerHito.Fecha && h.Id < primerHito.Id).OrderByDescending(h => h.Fecha).FirstOrDefault();
                if (errorSiNoHay && anterior == null)
                    GestorDeErrores.Emitir("El elemento solicitado sólo tiene historia inicial");

                if (anterior != null)
                {
                    anterior.Estado = negocio.Estados(contexto).First(e => e.Id == anterior.IdEstado).Nombre;
                    cache[i] = anterior;
                }
                else
                    return null;
            }
            return (HitoDtm)cache[i];
        }

        public static TransicionDtm ComoDevolverA<T>(this T elemento, ContextoSe contexto, int idEstadoDestino, bool delSistema, bool errorSiNoHay = false, bool errorSiMasDeUna = false)
        where T : IElementoDeProcesoDtm
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            var transiciones = TransicionesHasta(contexto, negocio, elemento.IdEstado, idEstadoDestino, activo: true, delSistema);
            if (transiciones.Count == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No se puede devolver el elemento '{elemento.Referencia}' desde '{negocio.Estado(contexto, elemento.IdEstado).Nombre}' a '{negocio.Estado(contexto, idEstadoDestino).Nombre}'");

            if (transiciones.Count > 1 && errorSiMasDeUna)
                GestorDeErrores.Emitir($"Hay '{transiciones.Count}' transiciones para devolver el elemento '{elemento.Referencia}' desde '{negocio.Estado(contexto, elemento.IdEstado).Nombre}' a '{negocio.Estado(contexto, idEstadoDestino).Nombre}'");

            return transiciones.Count == 0 ? null : transiciones[0];
        }

        private static List<TransicionDtm> TransicionesHasta(ContextoSe contexto, enumNegocio negocio, int idEstadoOrigen, int idEstadoDestino, bool activo = false, bool? delSistema = null)
        {
            var i = $"{negocio.ToString()}-{idEstadoOrigen}-{idEstadoDestino}-{delSistema}-{activo}";
            var cache = ServicioDeCaches.Obtener(CacheDe.Negocio_TransicionesHasta);
            if (!cache.ContainsKey(i))
            {
                var filtros = new List<ClausulaDeFiltrado>
                {
                    new ClausulaDeFiltrado(nameof(TransicionDtm.IdOrigen), enumCriteriosDeFiltrado.igual, idEstadoOrigen),
                    new ClausulaDeFiltrado(nameof(TransicionDtm.IdDestino), enumCriteriosDeFiltrado.igual, idEstadoDestino),
                    new ClausulaDeFiltrado(nameof(TransicionDtm.Activo), enumCriteriosDeFiltrado.igual, activo)
                };
                if (delSistema != null) filtros.Add(new ClausulaDeFiltrado(nameof(TransicionDtm.DelSistema), enumCriteriosDeFiltrado.igual, delSistema));
                cache[i] = TransicionSql.LeerTransiciones(contexto, ApiDeTransicion.TablaDeTransiciones(negocio.TipoDtm()), 0, -1, filtros, new List<ClausulaDeOrdenacion>());
            }
            return (List<TransicionDtm>)cache[i];
        }

        public static List<TransicionAplicable> TransicionesAplicables<T>(this T elemento, ContextoSe contexto, bool delSistema, bool conObservacion, bool activa = true)
        where T : IElementoDeProcesoDtm
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            var idEstado = elemento.IdEstado;

            var i = $"{negocio.ToString()}-{idEstado}-{delSistema}-{conObservacion}-{activa}";
            var cache = ServicioDeCaches.Obtener(CacheDe.Negocio_TransicionesDisponibles);
            if (!cache.ContainsKey(i))
            {
                var transicionesAplicables = new List<TransicionAplicable>();
                var filtros = new List<ClausulaDeFiltrado>
                {
                    new ClausulaDeFiltrado(nameof(TransicionDtm.Origen), enumCriteriosDeFiltrado.igual, idEstado),
                    new ClausulaDeFiltrado(nameof(TransicionDtm.DelSistema), enumCriteriosDeFiltrado.igual, delSistema),
                    new ClausulaDeFiltrado(nameof(TransicionDtm.ConObservacion), enumCriteriosDeFiltrado.igual, conObservacion),
                    new ClausulaDeFiltrado(nameof(TransicionDtm.Activo), enumCriteriosDeFiltrado.igual, activa)
                };
                var transiciones = TransicionSql.LeerTransiciones(contexto, ApiDeTransicion.TablaDeTransiciones(negocio.TipoDtm()), 0, -1, filtros, new List<ClausulaDeOrdenacion>());
                foreach (var transicion in transiciones)
                {
                    if (!ApiDePermisos.HayPermisosDeTransicion(contexto, negocio, transicion)) continue;
                    transicionesAplicables.Add(new TransicionAplicable { IdEstado = idEstado, IdTransicion = transicion.Id, Transicion = transicion.Nombre, IdEstadoDestino = transicion.IdDestino, PorDefecto = transicion.PorDefecto });
                }

                cache[i] = transicionesAplicables;
            }

            return (List<TransicionAplicable>)cache[i];
        }

        public static void ValidarTieneRefearenciaA<T>(ContextoSe contexto, List<int> ids, string propiedad, string nombreDelObjeto)
        where T : IElementoDeProcesoDtm
        {
            foreach (var id in ids)
            {
                var fae = contexto.SeleccionarElemento<T>(id, errorSiNoHay: false);
                if (fae.Valor<int?>(x => x.Name == propiedad) == null)
                    GestorDeErrores.Emitir($"El/La {NegociosDeSe.NegocioDeUnDtm(typeof(T)).Singular(true)} {((IElementoDeProcesoDtm)fae).Referencia} no referencia un/a {nombreDelObjeto}");
            }
        }

        public static SociedadDtm Sociedad(this IUsaCg elemento, ContextoSe contexto)
        {
            if (elemento.Cg != null && elemento.Cg.Sociedad != null)
                return elemento.Cg.Sociedad;

            return elemento.Cg(contexto).Sociedad ??= contexto.SeleccionarPorId<SociedadDtm>(elemento.Cg.IdSociedad);
        }

        public static void ValidarTieneRefearenciaA(this enumNegocio negocio, ContextoSe contexto, List<int> idsDeElementos, string propiedad, string nombreDelObjeto = null)
        {
            foreach (var id in idsDeElementos)
            {
                var elemento = negocio.ElementoPorId(contexto, id, errorSiNoHay: false);
                if (elemento.Valor<int?>(x => x.Name == propiedad) == null)
                {
                    if (nombreDelObjeto == null)
                    {
                        var propiedades = elemento.PropiedadesDelObjeto();
                        foreach (var p in propiedades)
                        {
                            if (propiedad.StartsWith("Id") && p.Name == propiedad.Substring(2))
                            {
                                var negocioReferenciado = NegociosDeSe.NegocioDeUnDtm(p.PropertyType);
                                if (negocioReferenciado == enumNegocio.No_Definido)
                                    nombreDelObjeto = p.Name;
                                else
                                    nombreDelObjeto = negocioReferenciado.Singular(true);
                                break;
                            }
                        }
                        if (nombreDelObjeto == null)
                            throw new Exception($"Error al implementar la llamada al método '{nameof(ValidarTieneRefearenciaA)}', falta definir el parámetro '{nameof(nombreDelObjeto)}'");
                    }
                    GestorDeErrores.Emitir($"El/La {negocio.Singular(true)} {(negocio.UsaTipo() ? ((IUsaReferencia)elemento).Referencia : elemento.Nombre)} no referencia un/a {nombreDelObjeto}");
                }
            }
        }

        public static IElementoDeProcesoDtm ElementoDeProcesoPorId(this enumNegocio negocio, ContextoSe contexto, int id, bool aplicarJoin = false, bool usarLaCache = true, bool errorSiNoHay = true)
        {
            var tipo = negocio.TipoDtm();
            if (!tipo.ImplementaElementoDeUnProceso())
                throw new Exception($"No se puede usar el método '{nameof(ElementoDeProcesoPorId)}' para el tipo '{tipo.Name}' ya que no implementa la interface '{nameof(IElementoDeProcesoDtm)}'");

            var parametros = new Dictionary<string, object>();
            parametros[ltrParametrosNeg.ErrorSiNoLoHay] = errorSiNoHay;

            return (IElementoDeProcesoDtm)negocio.CrearGestor(contexto, tipo).LeerRegistroPorId(id, aplicarJoin, usarLaCache, parametros: parametros);
        }

        public static void ValidarSiExistenDespendientes<T>(this ExpedienteDtm expediente, ContextoSe contexto)
        where T : RegistroDtm, IElementoDeProcesoDtm, IUsaExpediente
        {
            var negocio = NegociosDeSe.ToEnumerado(typeof(T));

            if (contexto.Existen<T>(nameof(IUsaExpediente.IdExpediente), expediente.Id))
            {
                GestorDeErrores.Emitir($"No se puede cancelar el expediente '{expediente.Referencia}' por tener {negocio.Plural(true)} imputados/as, quítelos/as");
            }
        }

        public static IQueryable<TareaDtm> Tareas(this IElementoDeProcesoDtm elemento, ContextoSe contexto, bool excluirCanceladas = true, bool excluirTerminadas = false)
        {
            var tareas = elemento.SusTareas(contexto);

            if (excluirCanceladas || excluirTerminadas)
                return tareas.Join(contexto.Estados(enumNegocio.Tarea)
                                        .Where(x => excluirCanceladas ? !x.Cancelado : true)
                                        .Where(x => excluirTerminadas ? !x.Terminado : true),
                                     tarea => tarea.IdEstado,
                                     estado => estado.Id,
                                     (tarea, estado) => tarea);
            return tareas;
        }

        public static IQueryable<TareaDtm> TareasTerminadas(this IElementoDeProcesoDtm elemento, ContextoSe contexto)
        {
            return elemento.SusTareas(contexto).Join(contexto.Estados(enumNegocio.Tarea).Where(x => x.Terminado),
                                 tarea => tarea.IdEstado,
                                 estado => estado.Id,
                                 (tarea, estado) => tarea);
        }

        private static IQueryable<TareaDtm> SusTareas(this IElementoDeProcesoDtm elemento, ContextoSe contexto)
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            if (!negocio.UsaTareas())
                GestorDeErrores.Emitir($"El {negocio.Singular(true)} no usa {enumNegocio.Tarea.Plural(true)} y ha solictado obtener las vinculadas al '{elemento.Referencia}'");

            return contexto.Set<TareaDtm>().Join(negocio.Tareas(contexto).Where(vin => vin.idElemento1 == elemento.Id),
                                                 tar => tar.Id,
                                                 tarVin => tarVin.idElemento2,
                                                 (tarea, tareaVinculada) => tarea);
        }

        public static void HistorialDeTareas(this IElementoDeProcesoDtm registro, ContextoSe contexto, List<HistorialDto> sucesos, List<ClausulaDeFiltrado> filtros, int nivel = 1)
        {
            ClausulaDeFiltrado filtro = filtros.First(f => f.Clausula == ltrSucesosFiltros.excluir);
            var valores = filtro.Valor.Split(Simbolos.separadorDeValores);
            if (valores.Contains(ltrSucesosExcluir.tareas) || (nivel > 1 && valores.Contains(ltrSucesosExcluir.nivel2tareas)))
                return;
            var tareas = registro.Tareas(contexto, excluirCanceladas: false).ToList();
            for (var i = tareas.Count() - 1; i >= 0; i--)
            {
                var tarea = tareas[i];
                var suceso = new HistorialDto();
                suceso.Id = sucesos.Count + 1;
                suceso.IdRegistro = tarea.Id;
                suceso.Elemento = tarea.Referencia(contexto);
                suceso.Clase = $"{ltrSucesosCss.suceso}{Simbolos.separadorCss}{ltrSucesosCss.tarea}";
                suceso.Suceso = $"Tarea asociada: {tarea.Nombre}";
                suceso.EstaCancelada = enumNegocio.Tarea.Estado(contexto, tarea.IdEstado).Cancelado;
                suceso.EstaTerminada = enumNegocio.Tarea.Estado(contexto, tarea.IdEstado).Terminado;
                suceso.Usuario = contexto.SeleccionarPorId<UsuarioDtm>(tarea.IdUsuaCrea).Login;
                suceso.OcurridoEl = tarea.FechaCreacion;
                suceso.Detalle = tarea.Descripcion;
                suceso.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                suceso.Accion = "Abrir";
                suceso.AccionJs = ltrAccionesDeSucesos.AbrirTarea;

                suceso.Negocio = enumNegocio.Tarea.Singular();
                suceso.enumNegocio = enumNegocio.Tarea;
                suceso.IdNegocio = enumNegocio.Tarea.IdNegocio();
                suceso.Nivel = nivel;
                suceso.IdElemento = registro.Id;

                sucesos.Add(suceso);

                tarea.HistorialDeHitos(contexto, sucesos, filtros, nivel + 1);
                tarea.HistorialDeObservaciones(contexto, sucesos, filtros, nivel + 1);
                tarea.HistorialDeArchivadores(contexto, sucesos, filtros);
                tarea.HistorialDeArchivos(contexto, sucesos, filtros);
                tarea.HistorialDeTrazas(contexto, sucesos, filtros, enumTraza.envioDeCorreo);
            }
        }

        public static void HistorialDeHitos(this IElementoDeProcesoDtm registro, ContextoSe contexto, List<HistorialDto> sucesos, List<ClausulaDeFiltrado> filtros, int nivel = 1)
        {
            ClausulaDeFiltrado filtro = filtros.First(f => f.Clausula == ltrSucesosFiltros.excluir);
            var valores = filtro.Valor.Split(Simbolos.separadorDeValores);
            if (valores.Contains(ltrSucesosExcluir.hitos) || (nivel > 1 && valores.Contains(ltrSucesosExcluir.nivel2hitos)))
                return;
            var negocio = NegociosDeSe.NegocioDeUnDtm(registro.GetType());
            var hitos = negocio.Hitos(contexto).Where(h => h.IdElemento == registro.Id).OrderBy(h => h.Fecha).ToList();
            for (var i = hitos.Count() - 1; i >= 0; i--)
            {
                var hito = hitos[i];
                var estado = negocio.Estado(contexto, hito.IdEstado);
                var transicion = i == 0 ? null : negocio.Transicion(contexto, hitos[i - 1].IdTransicion.Entero());
                var suceso = new HistorialDto();
                suceso.Id = sucesos.Count + 1;
                suceso.IdRegistro = hito.Id;
                suceso.Elemento = registro.Referencia; 
                suceso.Clase = $"{ltrSucesosCss.suceso}{Simbolos.separadorCss}{ltrSucesosCss.hito}";
                suceso.Suceso = i == 0
                ? $"Se crea la {negocio.Singular(true)} en estado '{estado.Nombre}'"
                : $"Usando la transición '{transicion.Nombre}' se ha cambiado al estado '{estado.Nombre}'";
                suceso.EstaCancelada = estado.Cancelado;
                suceso.EstaTerminada = estado.Terminado;
                suceso.Usuario = contexto.SeleccionarPorId<UsuarioDtm>(hito.IdUsuario).Login;
                suceso.OcurridoEl = hito.Fecha;
                suceso.Detalle = hito.IdObservacion is not null ? negocio.Observaciones(contexto).First(o => o.Id == hito.IdObservacion).Nombre : "";
                suceso.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;

                suceso.Negocio = negocio.Singular();
                suceso.enumNegocio = negocio;
                suceso.IdNegocio = negocio.IdNegocio();
                suceso.Nivel = nivel;
                suceso.IdElemento = registro.Id;

                sucesos.Add(suceso);
            }
        }

        public static void HistorialDeEventos(this IElementoDeProcesoDtm registro, ContextoSe contexto, List<HistorialDto> sucesos, List<ClausulaDeFiltrado> filtros, int nivel = 1)
        {
            ClausulaDeFiltrado filtro = filtros.First(f => f.Clausula == ltrSucesosFiltros.excluir);
            var valores = filtro.Valor.Split(Simbolos.separadorDeValores);
            if (valores.Contains(ltrSucesosExcluir.eventos))
                return;
            var negocio = NegociosDeSe.NegocioDeUnDtm(registro.GetType());
            var eventos = registro.LeerEventos(contexto, validarPermisosDeConsulta: true);
            for (var i = eventos.Count() - 1; i >= 0; i--)
            {
                var evento = eventos[i];
                var suceso = new HistorialDto();
                suceso.Id = sucesos.Count + 1;
                suceso.IdRegistro = evento.Id;
                suceso.Elemento = registro.Referencia(contexto); 
                suceso.Clase = $"{ltrSucesosCss.suceso}{Simbolos.separadorCss}{ltrSucesosCss.evento}";
                suceso.Suceso = evento.Nombre;
                suceso.EstaCancelada = false;
                suceso.EstaTerminada = false;
                suceso.Usuario = contexto.SeleccionarPorId<UsuarioDtm>(evento.IdUsuaCrea).Login;
                suceso.OcurridoEl = evento.Inicio;
                suceso.Detalle = evento.Descripcion;
                suceso.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                suceso.Accion = "Mostrar";
                suceso.AccionJs = ltrAccionesDeSucesos.MostrarEvento;

                suceso.Negocio = negocio.Singular();
                suceso.enumNegocio = negocio;
                suceso.IdNegocio = negocio.IdNegocio();
                suceso.Nivel = nivel;
                suceso.IdElemento = registro.Id;

                sucesos.Add(suceso);
            }
        }

        public static IElementoConTipo NuevoDtm(Type T, int idTipo, string nombre, string descripcion, Dictionary<string, object> parametros)
        {
            var obj = (IElementoConTipo)ExtensorDeElementos.NuevoDtm(T, nombre, descripcion, parametros);
            obj.IdTipo = idTipo;
            return obj;
        }

        public static IUsaTipoConCG NuevoDtm(Type T, int idCg, int idTipo, string nombre, string descripcion, Dictionary<string, object> parametros)
        {
            var obj = (IUsaTipoConCG)NuevoDtm(T, idTipo, nombre, descripcion, parametros);
            obj.IdCg = idCg;

            if (T.ImplementaUsaSolicitante())
            {
                if (!parametros.ContieneClave(nameof(IUsaSolicitante.IdSolicitante))) throw new Exception("Debe indicar el solicitante");
                ((IUsaSolicitante)obj).IdSolicitante = (int)parametros.LeerValor<long>(nameof(IUsaSolicitante.IdSolicitante));
            }

            if (T.ImplementaUsaProveedor())
            {
                if (!parametros.ContieneClave(nameof(IUsaProveedor.IdProveedor))) throw new Exception("Debe indicar el proveedor");
                ((IUsaProveedor)obj).IdProveedor = (int)parametros.LeerValor<long>(nameof(IUsaProveedor.IdProveedor));
            }

            if (T.ImplementaPuedeUsarResponsable())
            {
                var idResponsable = (int?)parametros.LeerValor<long?>(nameof(IPuedeUsarResponsable.IdResponsable), null);
                ((IPuedeUsarResponsable)obj).IdResponsable = idResponsable == 0 || idResponsable == null ? null : (int)idResponsable;
            }

            return obj;
        }

        public static void MarcarArchivosComoCancelados<T>(this T registro, ContextoSe contexto)
        where T : IElementoDtm
        {
            var archivos = registro.Archivos(contexto);
            var vinculado = NegociosDeSe.NegocioDeUnDtm(registro.GetType());
            foreach (var archivo in archivos)
            {
                if (archivo.Nombre.StartsWith(Simbolos.ArchivoCancelado))
                    continue;
                var relacionado = false;
                foreach (var negocio in Enum.GetValues(typeof(enumNegocio)))
                {
                    if ((enumNegocio)negocio == vinculado)
                        continue;

                    if (!NegociosDeSe.UsaArchivos((enumNegocio)negocio))
                        continue;

                    if (GestorDeVinculos.Existen(contexto, enumNegocio.Archivos, (enumNegocio)negocio, archivo.Id))
                    {
                        relacionado = true;
                        break;
                    }
                }
                if (relacionado)
                    continue;

                if (GestorDeVinculos.Cantidad(contexto, enumNegocio.Archivos, vinculado, archivo.Id) != 1)
                    continue;

                archivo.Cancelar(contexto, ltrDeAuditoriaDeArchivo.Cancelar.Replace("[0]", ((IElementoDtm)registro).Referencia(contexto)));
            }
        }

        public static void DesmarcarArchivosCancelados<T>(this T registro, ContextoSe contexto)
        where T : IElementoDtm
        {
            var archivos = registro.Archivos(contexto);
            foreach (var archivo in archivos)
            {
                archivo.Activar(contexto, ltrDeAuditoriaDeArchivo.Reactivar.Replace("[0]", ((IElementoDtm)registro).Referencia(contexto)));
            }
        }

        public static bool PermitirDesvincularArchivoAlEstarTerminado(this IElementoDeProcesoDtm elemento, ContextoSe contexto)
        {
            var estadoDelElemento = elemento.Estado(contexto);
            if (!estadoDelElemento.Terminado)
                GestorDeErrores.Emitir($"El estado '{estadoDelElemento.Nombre}' de '{elemento.Referencia}' no es es terminado, la función '{nameof(PermitirDesvincularArchivoAlEstarTerminado)}' no es aplicable");

            var negocioDtm = NegocioSqls.LeerNegocioPorDtm(elemento.GetType().FullName);
            var negocio = ApiDeEnsamblados.ToEnumerado<enumNegocio>(negocioDtm[0].Enumerado);
            var estados = ParametroDeNegocioSql.Parametro(negocio, enumParametrosDeArchivadores.NEGOCIO_Etapa_Documental, emitirError: false, crearParametro: true, "0").Valor.ToLista<int>();

            return estados.Contains(elemento.IdEstado) && contexto.SePuedeArchivarDocumentacionHistorica();
        }

        public static void ProcesarArchivo<T>(this T elemento, ContextoSe contexto, ParametrosDeNegocio parametros)
        where T : IElementoDeProcesoDtm, IUsaArchivo
        {
            var procesarArchivo =
            (parametros.Insertando && elemento.IdArchivo is not null) ||
            elemento.PropiedadCambiada<int?>(nameof(IUsaArchivo.IdArchivo), parametros);

            if (procesarArchivo && parametros.Modificando && ((IUsaArchivo)parametros.registroEnBd).IdArchivo is not null)
            {
                var archivoAnterior = contexto.SeleccionarPorId<ArchivoDtm>((int)((IUsaArchivo)parametros.registroEnBd).IdArchivo);
                archivoAnterior.Nombre = Path.GetFileNameWithoutExtension(archivoAnterior.Nombre) + "-old" + Path.GetExtension(archivoAnterior.Nombre);
                archivoAnterior.Nombre = elemento.ProponerNombreDeArchivo(contexto, archivoAnterior.Nombre);
                archivoAnterior.Modificar(contexto, ltrDeUnArchivo.Accion_Permitir_Modificar_Nombre);
            }

            if (procesarArchivo && elemento.IdArchivo is null)
            {
                GestorDeVinculos.BorrarVinculo(contexto,
                     NegociosDeSe.NegocioDeUnDtm(typeof(T)),
                     enumNegocio.Archivos,
                     elemento.Id,
                     (int)((IUsaArchivo)parametros.registroEnBd).IdArchivo,
                     new Dictionary<string, object>());
            }

            if (procesarArchivo && elemento.IdArchivo is not null)
            {
                GestorDeVinculos.Vincular(contexto,
                    NegociosDeSe.NegocioDeUnDtm(typeof(T)),
                    enumNegocio.Archivos,
                    elemento.Id,
                    (int)elemento.IdArchivo,
                    new Dictionary<string, object>());
                var archivo = contexto.SeleccionarPorId<ArchivoDtm>((int)elemento.IdArchivo);
                archivo.Nombre = archivo.Nombre.Replace(Path.GetFileNameWithoutExtension(archivo.Nombre), elemento.Referencia);
                archivo.Modificar(contexto, ltrDeUnArchivo.Accion_Permitir_Modificar_Nombre);
            }
        }


        public static void ValidarQueEstaActivo<T>(this T elemento, ContextoSe contexto, string posfijoMensajeError = "")
        where T : ElementoDeProcesoDtm
        {
            elemento.ValidarNoTerminado(contexto, posfijoMensajeError);
            elemento.ValidarNoCancelado(contexto, posfijoMensajeError);
        }

        private static void ValidarNoTerminado<T>(this T elemento, ContextoSe contexto, string posfijoMensajeError = "")
        where T : ElementoDeProcesoDtm
        {
            if (elemento.Estado(contexto).Terminado)
            {
                var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
                GestorDeErrores.Emitir($"{negocio.Singular()} '{elemento.Expresion}' está '{elemento.Estado(contexto).Nombre.ToLower()}'{posfijoMensajeError}");
            }
        }

        private static void ValidarNoCancelado<T>(this T elemento, ContextoSe contexto, string posfijoMensajeError = "")
        where T : ElementoDeProcesoDtm
        {
            if (elemento.Estado(contexto).Cancelado)
            {
                var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
                GestorDeErrores.Emitir($"{negocio.Singular()} '{elemento.Expresion}' está '{elemento.Estado(contexto).Nombre.ToLower()}'{posfijoMensajeError}");
            }
        }

        public static List<EventoDeAgendaDtm> LeerEventos(this IElementoDeProcesoDtm elemento, ContextoSe contexto, bool validarPermisosDeConsulta)
        {
            var filtro = new Dictionary<string, object>();
            filtro[nameof(EventoDeAgendaDtm.IdNegocio)] = NegociosDeSe.NegocioDeUnDtm(elemento.GetType()).IdNegocio();
            filtro[nameof(EventoDeAgendaDtm.IdElemento)] = elemento.Id;
            return contexto.SeleccionarTodos<EventoDeAgendaDtm>(filtro, parametros: new Dictionary<string, object> { { nameof(ltrParametrosNeg.ValidarPermisosDeConsulta), validarPermisosDeConsulta } });
        }

        public static void CebarCacheDeIds<T>(ContextoSe contexto, List<int> listaDeIds, Func<IQueryable<T>, IQueryable<T>> incluyeExtras = null, bool incluirTerminados = true, bool excluirCancelados = true)
        where T : ElementoDeProcesoDtm
        {
            var cacheRegistros = ServicioDeCaches.ObtenerCache(typeof(T).FullName, nameof(IRegistro.Id));
            var idsNuevos = listaDeIds
                            .Where(id => !cacheRegistros.ContainsKey(ExtensorDeElementos.IndiceCacheDeRegistro(id)))
                            .Distinct()
                            .ToList();

            if (!idsNuevos.Any()) return;

            IQueryable<T> query = contexto.Set<T>()
                      .Include(p => p.Estado)
                      .Include(p => p.Tipo);

            if (incluyeExtras != null)
                query = incluyeExtras(query);

            query = query.Where(p => idsNuevos.Contains(p.Id));

            if (excluirCancelados)
                query = query.Where(p => p.Estado.Cancelado == false);

            if (!incluirTerminados)
                query = query.Where(p => p.Estado.Terminado == false);

            var registros = query.ToList();

            foreach (var registro in registros)
            {
                cacheRegistros[ExtensorDeElementos.IndiceCacheDeRegistro(registro.Id)] = registro;
            }
        }

    }
}

