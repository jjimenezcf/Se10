using Gestor.Errores;
using ServicioDeDatos.Elemento;
using ServicioDeDatos;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Gastos;
using System.Collections.Concurrent;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Contabilidad;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeTransiciones
    {

        public static enumNegocio Negocio(this Type tipo)
        {
            if (typeof(TransicionesDeUnPleitoDtm) == tipo) return enumNegocio.Pleito;
            if (typeof(TransicionesDeUnaTareaDtm) == tipo) return enumNegocio.Tarea;
            if (typeof(TransicionesDeUnRegistroEsDtm) == tipo) return enumNegocio.Registro;
            if (typeof(TransicionesDeUnExpedienteDtm) == tipo) return enumNegocio.Expediente;
            if (typeof(TransicionesDeUnPresupuestoDtm) == tipo) return enumNegocio.Presupuesto;
            if (typeof(TransicionesDeUnContratoDtm) == tipo) return enumNegocio.Contrato;
            if (typeof(TransicionesDeUnaFacturaEmtDtm) == tipo) return enumNegocio.FacturaEmitida;
            if (typeof(TransicionesDeUnParteTrDtm) == tipo) return enumNegocio.ParteDeTrabajo;
            if (typeof(TransicionesDeUnaPlanificacionDeVentaDtm) == tipo) return enumNegocio.PlanificacionDeVenta;
            if (typeof(TransicionesDeUnaRemesaFaeDtm) == tipo) return enumNegocio.RemesaFae;
            if (typeof(TransicionesDeUnaFacturaRecDtm) == tipo) return enumNegocio.FacturaRecibida;
            if (typeof(TransicionesDeUnPedidoDtm) == tipo) return enumNegocio.Pedido;
            if (typeof(TransicionesDeUnPreasientoDtm) == tipo) return enumNegocio.Preasiento;
            if (typeof(TransicionesDeUnCircuitoDocDtm) == tipo) return enumNegocio.CircuitoDoc;
            if (typeof(TransicionesDeUnPagoDtm) == tipo) return enumNegocio.Pago;
            if (typeof(TransicionesDeUnaRemesaPagDtm) == tipo) return enumNegocio.RemesaPag;


            throw new Exception($"No se ha definido cual es el negocio para la clase transiciones {tipo.Name}");
        }

        public static TransicionDtm Transicion(this ContextoSe contexto, enumNegocio negocio, int idEstadoOrigen, int idEstadoDestino, bool errorSiNohay = true, bool errorSiHayMasDeUno = true, bool? delSistema = null)
        =>
        negocio.Transicion(contexto, idEstadoOrigen.ToString(), idEstadoDestino.ToString(), errorSiNohay, errorSiHayMasDeUno, delSistema);

        public static TransicionDtm Transicion(this ContextoSe contexto, enumNegocio negocio, string estadoOrigen, string estadoDestino, bool errorSiNohay = true, bool errorSiHayMasDeUno = true, bool? delSistema = null)
        =>
        negocio.Transicion(contexto, estadoOrigen, estadoDestino, errorSiNohay, errorSiHayMasDeUno, delSistema);

        public static IElementoDeProcesoDtm TransitarPorMotivo(this IElementoDeProcesoDtm elemento, ContextoSe contexto, string motivos, Enum motivo)
        {
            var transiciones = TransicionAplicable.Transiciones(motivos, motivo, errorSiNoHay:false);
            if (transiciones.Count == 0)
            {
                var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
                GestorDeErrores.Emitir($"No se ha definido en el parámetros de transiciones aplicables del negocio de '{negocio.Singular()}' que transición aplicar para el motivo '{motivo}' ");
            }
            return elemento.IntentarAplicarTransicion(contexto, transiciones, parametros: new Dictionary<string, object> { { ltrParametrosNeg.AccionQueSeEjecuta, motivo.ToString() } }).elemento;
        }

        public static T TransitarALaEtapa<T>(this T elemento, ContextoSe contexto, (List<int> estados, Enum etapa) estadosDeUnaEtapa, Dictionary<string, object> parametros = null, bool delSistema = true, bool emitirErrorSiNoHay = true)
        where T : IElementoDeProcesoDtm
        {
            if (parametros is null) parametros = new Dictionary<string, object>();
            
            if (!parametros.ContainsKey(ltrParametrosNeg.EstaEjecutandoUnaAccion))
                parametros[ltrParametrosNeg.EstaEjecutandoUnaAccion] = true;

            var transicion = elemento.TransicionPosible(contexto, estadosDeUnaEtapa.etapa, estadosDeUnaEtapa.estados, delSistema: delSistema, emitirErrorSiNoHay: emitirErrorSiNoHay);
            return transicion == null ? elemento : elemento.Transitar(contexto, transicion.Id, parametros);
        }

        public static TransicionDtm TransicionPosible(this IElementoDeProcesoDtm elemento, ContextoSe contexto, Enum etapa, List<int> destinos, bool delSistema = true, bool emitirErrorSiNoHay = true)
        {
            var negocio = NegociosDeSe.ToEnumerado(NegociosDeSe.LeerNegocioPorDtm(elemento.GetType().FullName).Nombre);
            var transiciones = negocio.ListaDeTransiciones(contexto).Where(x => x.DelSistema == delSistema && x.Activo && x.IdOrigen == elemento.IdEstado && destinos.Contains(x.IdDestino));
            if (transiciones.Count() != 1)
            {
                if (transiciones.Count() == 0)
                {
                    if (emitirErrorSiNoHay)
                        GestorDeErrores.Emitir($"No hay una transición {(delSistema ? "del sistema" : "de usuario")} desde el estado '{elemento.Estado(contexto).Nombre}' a la etapa '{etapa}'");
                    else return null;
                }

                GestorDeErrores.Emitir($"Hay más de una transición {(delSistema ? "del sistema" : "de usuario")} desde el estado '{elemento.Estado(contexto).Nombre}' a la etapa '{etapa}'");
            }
            return transiciones.ToList()[0];
        }

        public static T Transitar<T>(this T elemento, ContextoSe contexto, string nombreTransicion, Dictionary<string, object> parametros = null, bool erroSiNoTransita = true, bool? delSistema = null)
        where T : IElementoDeProcesoDtm
        {
            var gestor = NegociosDeSe.NegocioDeUnDtm(elemento.GetType()).CrearGestorDeTransiciones(contexto);

            var filtros = new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado(nameof(TransicionDtm.IdOrigen), enumCriteriosDeFiltrado.igual, elemento.IdEstado),
                new ClausulaDeFiltrado(nameof(TransicionDtm.Nombre), enumCriteriosDeFiltrado.igual, nombreTransicion)
            };

            if (delSistema is not null)
                filtros.Add(new ClausulaDeFiltrado(nameof(TransicionDtm.DelSistema), enumCriteriosDeFiltrado.igual, delSistema));

            var transiciones = (List<TransicionDtm>)gestor.LeerRegistros(0, -1, filtros, aplicarJoin: true, parametros);

            if (transiciones.Count() == 0)
                GestorDeErrores.Emitir($"No hay parametrizada una transición llamada '{nombreTransicion}' que parta del estado '{elemento.Estado(contexto).Nombre}'");

            if (transiciones.Count() > 1)
                GestorDeErrores.Emitir($"Hay parametrizada más de una transición llamada '{nombreTransicion}' que parten del mismo estado '{elemento.Estado(contexto).Nombre}'");
            return elemento.Transitar(contexto, transiciones[0].Id, parametros, erroSiNoTransita);
        }

        public static T Transitar<T>(this T elemento, ContextoSe contexto, string transicionesPorMotivo, Enum motivo, Dictionary<string, object> parametros = null, bool erroSiNoTransita = true)
        where T : IElementoDeProcesoDtm
        {
            var transicionesAplicables = TransicionAplicable.Transiciones(transicionesPorMotivo, motivo, errorSiNoHay: true);
            var posibles = transicionesAplicables.Count(x => x.IdEstado == elemento.IdEstado);
            if (posibles != 1)
                GestorDeErrores.Emitir($"{(posibles > 1 ? "Hay más de una" : "No hay")} transición definida desde el estado '{typeof(T).NegocioDeUnDtm().Estado(contexto, elemento.IdEstado).Nombre}' de '{elemento.Referencia(contexto)}' por el motivo {motivo}");

            return elemento.Transitar(contexto, transicionesAplicables[0].IdTransicion, parametros, erroSiNoTransita);
        }

        public static T Transitar<T>(this T elemento, ContextoSe contexto, int idDeTransicion, Dictionary<string, object> parametros = null, bool erroSiNoTransita = true)
        where T : IElementoDeProcesoDtm
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            var gestor = NegociosDeSe.CrearGestor(contexto, negocio);
            try
            {
                parametros = parametros == null ? new Dictionary<string, object>() : parametros;
                gestor.TransitarRegistro(elemento, idDeTransicion, parametros);
            }
            catch
            {
                if (erroSiNoTransita)
                    throw;
            }
            return (T)NegociosDeSe.CrearGestor(contexto, negocio).LeerRegistroPorId(((IRegistro)elemento).Id, true, usarLaCache: false, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });
        }

        public static T TransitarAl<T>(this T elemento, ContextoSe contexto, int idEstadoDestino, Dictionary<string, object> parametros = null, bool erroSiNoTransita = true, bool errorSiMasDeUnaTransicion = true)
        where T : IElementoDeProcesoDtm
        => elemento.TransitarAl(contexto, idEstadoDestino.ToString(), parametros, erroSiNoTransita, errorSiMasDeUnaTransicion);

        public static T TransitarAl<T>(this T elemento, ContextoSe contexto, string estadoDestino, Dictionary<string, object> parametros = null, bool erroSiNoTransita = true, bool errorSiMasDeUnaTransicion = true)
        where T : IElementoDeProcesoDtm
        {
            var filtros = new Dictionary<string, object>();
            filtros[nameof(TransicionDtm.Origen)] = elemento.IdEstado.ToString();
            filtros[nameof(TransicionDtm.Destino)] = estadoDestino;

            var transiciones = contexto.SeleccionarTodos<TransicionDtm>(filtros, NegociosDeSe.NegocioDeUnDtm(elemento.GetType()));
            if (transiciones.Count != 1)
            {
                if (estadoDestino.EsNumero()) estadoDestino = NegociosDeSe.NegocioDeUnDtm(elemento.GetType()).Estado(contexto, estadoDestino.Entero()).Nombre;
                if (transiciones.Count == 0)
                    GestorDeErrores.Emitir($"No se ha definido ninguna transición entre el estado {elemento.Estado(contexto).Nombre} y el estado {estadoDestino}");
                
                if (transiciones.Count > 1 && errorSiMasDeUnaTransicion)
                    GestorDeErrores.Emitir($"Se han definido más de una transición entre el estado {elemento.Estado(contexto).Nombre} y el estado {estadoDestino}");
            }

            return elemento.Transitar(contexto, transiciones[0].Id, parametros, erroSiNoTransita);
        }

        public static T Devolver<T>(this T elemento, ContextoSe contexto, Dictionary<string, object> parametros = null, bool erroSiNoTransita = true, bool errorSiMasDeUnaTransicion = true)
        where T : IElementoDeProcesoDtm
        {
            var hito = elemento.HitoAnteriorAlActual(contexto);
            return elemento.TransitarAl(contexto, hito.IdEstado, parametros, erroSiNoTransita, errorSiMasDeUnaTransicion);
        }

        public static List<TransicionDtm> Transicciones(this EntornoDeUnaAccion entorno, enumNegocio negocio, bool errorSiNoHay = true)
        {
            List<int> IdsDeTransiciones;
            if (!entorno.Parametros.ContainsKey(nameof(IdsDeTransiciones)) && errorSiNoHay)
                GestorDeErrores.Emitir($"El diccionario de parámetros pasado a de contener la lista de ids de transiciones a extraer");

            IdsDeTransiciones = entorno.Parametros[nameof(IdsDeTransiciones)].ToString().JsonToLista<int>();
            if (IdsDeTransiciones.Count == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"El parámetro '{nameof(IdsDeTransiciones)}' no tiene indicada ningún id de transición");

            List<TransicionDtm> transiciones = new List<TransicionDtm>();
            var gestorTr = negocio.CrearGestorDeTransiciones(entorno.Contexto);
            var aplicarPermisos = (bool)entorno.Parametros.LeerValor(ltrTransiciones.aplicarPermisos, true);

            var transicionesIndicadas = "";
            foreach (var idDeTransicion in IdsDeTransiciones)
            {
                var transicion = (TransicionDtm)gestorTr.LeerRegistroPorId(idDeTransicion, aplicarJoin: false);
                transicionesIndicadas = $"{(transicionesIndicadas.IsNullOrEmpty() ? "" : $"{transicionesIndicadas}, ")}{transicion.Nombre}";
                if (aplicarPermisos && !ApiDePermisos.HayPermisosDeTransicion(entorno.Contexto, negocio, transicion))
                    continue;
                transiciones.Add(transicion);
            }

            if (transiciones.Count == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"El usuario '{entorno.Contexto.DatosDeConexion.Login}' no tiene acceso a ninguna de las transiciones: {transicionesIndicadas}");

            return transiciones;
        }

        public static IQueryable<TransicionDtm> Transiciones<T>(this ContextoSe contexto) where T : TransicionDtm => contexto.Set<T>().Cast<T>();

        public static List<TransicionDtm> ListaDeTransiciones(this enumNegocio negocio, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Negocio_Transiciones);
            if (!cache.ContainsKey(negocio.ToString()))
            {
                cache[negocio.ToString()] = Transiciones(contexto, negocio).ToList();
            }
            return (List<TransicionDtm>)cache[negocio.ToString()];
        }

        public static IQueryable<TransicionDtm> Transiciones(this ContextoSe contexto, enumNegocio negocio)
        {
            switch (negocio)
            {
                case enumNegocio.Registro:
                    return contexto.Set<TransicionesDeUnRegistroEsDtm>().Cast<TransicionDtm>();
                case enumNegocio.Tarea:
                    return contexto.Set<TransicionesDeUnaTareaDtm>().Cast<TransicionDtm>();
                case enumNegocio.Expediente:
                    return contexto.Set<TransicionesDeUnExpedienteDtm>().Cast<TransicionDtm>();
                case enumNegocio.Pleito:
                    return contexto.Set<TransicionesDeUnPleitoDtm>().Cast<TransicionDtm>();
                case enumNegocio.Contrato:
                    return contexto.Set<TransicionesDeUnContratoDtm>().Cast<TransicionDtm>();
                case enumNegocio.Presupuesto:
                    return contexto.Set<TransicionesDeUnPresupuestoDtm>().Cast<TransicionDtm>();
                case enumNegocio.PlanificacionDeVenta:
                    return contexto.Set<TransicionesDeUnaPlanificacionDeVentaDtm>().Cast<TransicionDtm>();
                case enumNegocio.ParteDeTrabajo:
                    return contexto.Set<TransicionesDeUnParteTrDtm>().Cast<TransicionDtm>();
                case enumNegocio.FacturaEmitida:
                    return contexto.Set<TransicionesDeUnaFacturaEmtDtm>().Cast<TransicionDtm>();
                case enumNegocio.RemesaFae:
                    return contexto.Set<TransicionesDeUnaRemesaFaeDtm>().Cast<TransicionDtm>();
                case enumNegocio.Pago:
                    return contexto.Set<TransicionesDeUnPagoDtm>().Cast<TransicionDtm>();
                case enumNegocio.RemesaPag:
                    return contexto.Set<TransicionesDeUnaRemesaPagDtm>().Cast<TransicionDtm>();
                case enumNegocio.FacturaRecibida:
                    return contexto.Set<TransicionesDeUnaFacturaRecDtm>().Cast<TransicionDtm>();
                case enumNegocio.Pedido:
                    return contexto.Set<TransicionesDeUnPedidoDtm>().Cast<TransicionDtm>();
                case enumNegocio.Preasiento:
                    return contexto.Set<TransicionesDeUnPreasientoDtm>().Cast<TransicionDtm>();
                case enumNegocio.CircuitoDoc:
                    return contexto.Set<TransicionesDeUnCircuitoDocDtm>().Cast<TransicionDtm>();
            }
            throw new Exception($"Se debe indicar como obtener el dbSet de los Transicions del negocio: {negocio}");
        }

        public static T Cancelar<T>(this T registro, ContextoSe contexto, bool errorSiNoTransita = true)
        where T : IElementoDeProcesoDtm
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(registro.GetType());
            var transiciones = negocio.Transiciones(contexto, idEstadoOrigen: registro.IdEstado);

            bool b = true;
            var mensaje = "";
            foreach (var transicion in transiciones)
            {
                if (!transicion.EsCancelado)
                    continue;
                b = true;
                try
                {
                    registro.Transitar(contexto, transicion.Id, new Dictionary<string, object>(), errorSiNoTransita);
                    break;
                }
                catch (Exception e)
                {
                    b = false;
                    mensaje = e.Message;
                }
            }

            if (!b && errorSiNoTransita)
                GestorDeErrores.Emitir(mensaje);

            return registro.Recargar(contexto);
        }

        public static T PersistirTransicion<T>(this T transicion, ContextoSe contexto, bool errorSiYaExiste = false, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
        where T : TransicionDtm
        {
            if (transicion.Id > 0)
                return transicion.Modificar(contexto);

            var leido = contexto.SeleccionarTransicion<T>(nameof(INombre.Nombre), transicion.Nombre, errorSiNoHay: errorSiYaExiste);
            if (leido == null)
                return transicion.Insertar(contexto, parametros, aplicarJoin);

            transicion.Id = leido.Id;
            transicion.IdPermiso = leido.IdPermiso;
            return transicion.Modificar(contexto, parametros, aplicarJoin);
        }

        public static TransicionDtm PersistirTransicionPorAk<T>(this T transicion, ContextoSe contexto, bool errorSiYaExiste = false, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
        where T : TransicionDtm
        {
            if (transicion.Id > 0)
                return transicion.Modificar(contexto);

            var ak = new Dictionary<string, object>
            {
                { nameof(transicion.IdDestino), transicion.IdDestino },
                { nameof(transicion.IdOrigen), transicion.IdOrigen },
                { nameof(transicion.Nombre), transicion.Nombre }
            };

            var leido = contexto.SeleccionarTransicionPorAk<T>(ak, errorSiYaExiste, true, false);
            if (leido == null)
                return transicion.Insertar(contexto, parametros, aplicarJoin);

            transicion.Id = leido.Id;
            return transicion.Modificar(contexto, parametros, aplicarJoin);
        }

        public static TransicionDtm SeleccionarTransicion<T>(this ContextoSe contexto, int id, bool errorSiNoHay = true, bool errorSiMasDeuno = true, bool aplicarJoin = false)
        where T : TransicionDtm
        {
            if (typeof(T) == typeof(TransicionDtm))
                throw new Exception($"El método {nameof(SeleccionarTransicion)} necesita que se le indique el negocio usado el tipo de transición");

            return SeleccionarTransicion<T>(contexto, nameof(EstadoDtm.Id), id.ToString(), errorSiNoHay, errorSiMasDeuno, aplicarJoin);
        }

        public static TransicionDtm SeleccionarTransicion<T>(this ContextoSe contexto, string propiedad, string valor, bool errorSiNoHay = true, bool errorSiMasDeuno = true, bool aplicarJoin = false)
        where T : TransicionDtm
        {
            var gestor = NegociosDeSe.CrearGestorDeUnDtm<T>(contexto);
            var filtros = new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(propiedad, enumCriteriosDeFiltrado.igual, valor) };
            var seleccionados = (List<TransicionDtm>)gestor.LeerRegistros(0, -1, filtros, aplicarJoin);
            var mensaje = $"para la clase {typeof(T).FullName} con el criterio '{propiedad}' con valor '{valor}'";
            return seleccionados.DevolverSeleccionado(mensaje, errorSiNoHay, errorSiMasDeuno);
        }

        public static TransicionDtm SeleccionarTransicionPorAk<T>(this ContextoSe contexto, Dictionary<string, object> filtrosPorAk, bool errorSiNoHay = true, bool errorSiMasDeuno = true, bool aplicarJoin = false)
        where T : TransicionDtm
        {
            var filtros = filtrosPorAk.ToFiltros();

            var negocio = Negocio(typeof(T));
            var seleccionados = negocio.SeleccionarPorFiltro<TransicionDtm>(contexto, filtros, aplicarJoin);
            var mensaje = $"del negocio {negocio} para los criterios '{filtrosPorAk.Keys.ToList().ToString(",")}'";
            return seleccionados.DevolverSeleccionado(mensaje, errorSiNoHay, errorSiMasDeuno);
        }

        public static bool OrigenEstaEnLaEtapa(this TransicionDtm transicion, string etapa) => etapa.ToLista<int>(Simbolos.Coma).Contains(transicion.IdOrigen);

        public static bool DestinoEstaEnLaEtapa(this TransicionDtm transicion, string etapa) => etapa.ToLista<int>(Simbolos.Coma).Contains(transicion.IdDestino);

        public static bool DestinoEstaEnAlgunaDeLasEtapas(this TransicionDtm transicion, List<string> etapas)
        {
            foreach (var etapa in etapas)
            {
                if (etapa.ToLista<int>(Simbolos.Coma).Contains(transicion.IdDestino))
                    return true;
            }
            return false;
        }

        public static bool EntrarEnLaEtapaDe(this TransicionDtm transicion, string etapa) => !transicion.OrigenEstaEnLaEtapa(etapa) && transicion.DestinoEstaEnLaEtapa(etapa);
        public static bool SalirDeLaEtapaDe(this TransicionDtm transicion, string etapa) => transicion.OrigenEstaEnLaEtapa(etapa) && !transicion.DestinoEstaEnLaEtapa(etapa);


        public static bool EntreEtapas(this TransicionDtm transicion, string estadosDeLaEtapaOrigen, string estadosDeLaEtapaDestino)
        =>
        ExtensorDeEstados.EstaEnLaEtapa(transicion.IdOrigen, estadosDeLaEtapaOrigen) && ExtensorDeEstados.EstaEnLaEtapa(transicion.IdDestino, estadosDeLaEtapaDestino);

        public static TransicionDtm Transicion(this enumNegocio negocio, ContextoSe contexto, string estadoOrigen, string estadoDestino, bool errorSiNoHay = true, bool errorSiHayMasDeUno = true, bool? delSistema = null)
        {
            var filtros = new Dictionary<string, object>();
            filtros[nameof(TransicionDtm.Origen)] = estadoOrigen;
            filtros[nameof(TransicionDtm.Destino)] = estadoDestino;
            if (delSistema != null)
            {
                filtros[nameof(TransicionDtm.DelSistema)] = delSistema;
            }

            var transiciones = contexto.SeleccionarTodos<TransicionDtm>(filtros, negocio, aplicarJoin: true);
            if (errorSiNoHay && transiciones.Count == 0)
            {
                if (estadoOrigen.Entero() > 0)
                {
                    estadoOrigen = negocio.Estado(contexto, estadoOrigen.Entero()).Nombre;
                }

                if (estadoDestino.Entero() > 0)
                {
                    estadoDestino = negocio.Estado(contexto, estadoDestino.Entero()).Nombre;
                }

                var s = delSistema != null && (bool)delSistema ? "para el sistema " : "";
                s = s.IsNullOrEmpty() && delSistema != null && !(bool)delSistema ? "para el usuario " : s;
                GestorDeErrores.Emitir($"no se ha definido {s}ninguna transición entre el estado {estadoOrigen} y el estado {estadoDestino}");
            }
            if (errorSiHayMasDeUno && transiciones.Count > 1)
            {
                if (estadoOrigen.Entero() > 0)
                {
                    estadoOrigen = negocio.Estado(contexto, estadoOrigen.Entero()).Nombre;
                }

                if (estadoDestino.Entero() > 0)
                {
                    estadoDestino = negocio.Estado(contexto, estadoDestino.Entero()).Nombre;
                }

                var s = delSistema != null && (bool)delSistema ? "para el sistema " : "";
                s = s.IsNullOrEmpty() && delSistema != null && !(bool)delSistema ? "para el usuario " : s;
                GestorDeErrores.Emitir($"Hay más de una transición {s}entre el estado {estadoOrigen} y el estado {estadoDestino}");
            }
            return transiciones.Count == 0 ? null : transiciones[0];
        }

        public static List<TransicionDtm> Transiciones(this enumNegocio negocio, ContextoSe contexto, string estadoOrigen = null, string estadoDestino = null)
        {
            var filtros = new Dictionary<string, object>();
            if (!estadoOrigen.IsNullOrEmpty())
            {
                filtros[nameof(TransicionDtm.Origen)] = estadoOrigen;
            }

            if (!estadoDestino.IsNullOrEmpty())
            {
                filtros[nameof(TransicionDtm.Destino)] = estadoDestino;
            }

            return contexto.SeleccionarTodos<TransicionDtm>(filtros, negocio, aplicarJoin: true);
        }

        public static List<TransicionDtm> Transiciones(this enumNegocio negocio, ContextoSe contexto, int? idEstadoOrigen = default, int? idEstadoDestino = default)
        {
            var filtros = new Dictionary<string, object>();
            if (!idEstadoOrigen.Equals(default))
            {
                filtros[nameof(TransicionDtm.Origen)] = idEstadoOrigen;
            }

            if (!idEstadoDestino.Equals(default))
            {
                filtros[nameof(TransicionDtm.Destino)] = idEstadoDestino;
            }

            return contexto.SeleccionarTodos<TransicionDtm>(filtros, negocio, true);
        }

        public static List<TransicionDtm> TransicionesPorNombre(this enumNegocio negocio, ContextoSe contexto, string nombre, bool errorSiNoHay = true)
        {
            var filtros = new Dictionary<string, object>() { { nameof(INombre.Nombre), nombre } };
            var transiciones = contexto.SeleccionarTodos<TransicionDtm>(filtros, negocio, aplicarJoin: true);
            if (transiciones.Count == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No hay ninguna transición con el nombre indicado '{nombre}'");

            return transiciones;
        }


    }
}
