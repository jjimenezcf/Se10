using Gestor.Errores;
using Newtonsoft.Json.Linq;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestorDeElementos.Extensores
{


    public static class ExtensorDeEstados
    {

        public static enumNegocio Negocio(Type tipo)
        {
            if (typeof(EstadoDeUnPleitoDtm) == tipo) return enumNegocio.Pleito;
            if (typeof(EstadoDeUnaTareaDtm) == tipo) return enumNegocio.Tarea;
            if (typeof(EstadoDeUnRegistroEsDtm) == tipo) return enumNegocio.Registro;
            if (typeof(EstadoDeUnExpedienteDtm) == tipo) return enumNegocio.Expediente;
            if (typeof(EstadoDeUnPresupuestoDtm) == tipo) return enumNegocio.Presupuesto;
            if (typeof(EstadoDeUnContratoDtm) == tipo) return enumNegocio.Contrato;
            if (typeof(EstadoDeUnParteTrDtm) == tipo) return enumNegocio.ParteDeTrabajo;
            if (typeof(EstadoDeUnaFacturaEmtDtm) == tipo) return enumNegocio.FacturaEmitida;
            if (typeof(EstadoDeUnaPlanificacionDeVentaDtm) == tipo) return enumNegocio.PlanificacionDeVenta;
            if (typeof(EstadoDeUnaRemesaFaeDtm) == tipo) return enumNegocio.RemesaFae;
            if (typeof(EstadoDeUnCircuitoDocDtm) == tipo) return enumNegocio.CircuitoDoc;
            if (typeof(EstadoDeUnPagoDtm) == tipo) return enumNegocio.Pago;
            if (typeof(EstadoDeUnaRemesaPagDtm) == tipo) return enumNegocio.RemesaPag;
            if (typeof(EstadoDeUnaFacturaRecDtm) == tipo) return enumNegocio.FacturaRecibida;
            if (typeof(EstadoDeUnPedidoDtm) == tipo) return enumNegocio.Pedido;
            if (typeof(EstadoDeUnPreasientoDtm) == tipo) return enumNegocio.Preasiento;
            if (typeof(EstadoDeUnAlmacenDtm) == tipo) return enumNegocio.Almacen;
            if (typeof(EstadoDeUnaRegularizacionDtm) == tipo) return enumNegocio.Regularizacion;
            if (typeof(EstadoDeUnaVentaDtm) == tipo) return enumNegocio.Venta;

            throw new Exception($"No se ha definido cual es el negocio para la clase estado {tipo.Name}");
        }

        public static Type TipoDtmDelEstado(this enumNegocio negocio)
        {
            if (!negocio.UsaEstado())
                throw new Exception($"Ha solicitado el Dtm de estado asociado al negocio {negocio} y dicho negocio no usa Estado");

            var indice = negocio.ToString();
            var cache = ServicioDeCaches.Obtener(nameof(TipoDtmDelEstado));
            if (!cache.ContainsKey(indice))
            {
                var tipoDelObjeto = negocio.TipoDtm();
                var propiedades = tipoDelObjeto.PropiedadesDelTipo();
                foreach (var propieda in propiedades)
                {
                    if (propieda.PropertyType.FullName == typeof(EstadoDtm).FullName)
                        continue;
                    if (propieda.PropertyType.HeredaDe(typeof(EstadoDtm), false))
                    {
                        cache[indice] = propieda.PropertyType;
                        break;
                    }
                }
                if (!cache.ContainsKey(indice))
                    throw new Exception($"No se ha definido cual es el tipoDtm de estado para el negocio de {negocio.Singular()}");
            }
            return (Type)cache[indice];
        }

        public static List<int> Cancelados(this Type tipoDtm, ContextoSe contexto)
        {
            var tabla = ApiDeEstado.TablaDeEstados(tipoDtm);
            var cache = ServicioDeCaches.Obtener($"{nameof(EstadoDtm.Cancelado)}");
            if (!cache.ContainsKey(tabla))
            {
                var canceldos = EstadoSql.LeerEstadosPorSituacion(contexto, tabla, nameof(EstadoDtm.Cancelado));
                cache[tabla] = canceldos.Select(x => x.Id).ToList<int>();
            }
            return (List<int>)cache[tabla];
        }

        public static List<int> Terminados(this Type tipoDtm, ContextoSe contexto)
        {
            var tabla = ApiDeEstado.TablaDeEstados(tipoDtm);
            var cache = ServicioDeCaches.Obtener($"{nameof(EstadoDtm.Terminado)}");
            if (!cache.ContainsKey(tabla))
            {
                var canceldos = EstadoSql.LeerEstadosPorSituacion(contexto, tabla, nameof(EstadoDtm.Terminado));
                cache[tabla] = canceldos.Select(x => x.Id).ToList<int>();
            }
            return (List<int>)cache[tabla];
        }

        public static List<int> Iniciales(this Type tipoDtm, ContextoSe contexto)
        {
            var tabla = ApiDeEstado.TablaDeEstados(tipoDtm);
            var cache = ServicioDeCaches.Obtener($"{nameof(EstadoDtm.Inicial)}");
            if (!cache.ContainsKey(tabla))
            {
                var canceldos = EstadoSql.LeerEstadosPorSituacion(contexto, tabla, nameof(EstadoDtm.Inicial));
                cache[tabla] = canceldos.Select(x => x.Id).ToList<int>();
            }
            return (List<int>)cache[tabla];
        }

        public static List<EstadoDtm> Siguientes<T>(this enumNegocio negocio, ContextoSe contexto, int idEstado)
        {
            var tabla = ApiDeEstado.TablaDeEstados(negocio.TipoDtm());
            var cache = ServicioDeCaches.Obtener(CacheDe.EstadosSiguientes);
            if (!cache.ContainsKey(tabla))
            {
                var filtros = new List<ClausulaDeFiltrado>
                {
                    new ClausulaDeFiltrado(nameof(TransicionDtm.Origen), enumCriteriosDeFiltrado.igual, idEstado)
                };
                var transiciones = TransicionSql.LeerTransiciones(contexto, ApiDeTransicion.TablaDeTransiciones(negocio.TipoDtm()), 0, -1, filtros, new List<ClausulaDeOrdenacion>());
                var siguientes = new List<EstadoDtm>();
                foreach (TransicionDtm t in transiciones)
                {
                    var estado = EstadoSql.LeerEstadoPorId(contexto, tabla, t.IdDestino);
                    siguientes.Add(estado);
                }
                cache[tabla] = siguientes;
            }
            return (List<EstadoDtm>)cache[tabla];
        }

        public static IQueryable<T> Estados<T>(this ContextoSe contexto) where T : EstadoDtm => contexto.Set<T>();

        public static List<EstadoDtm> Estados(this enumNegocio negocio, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Estados);
            if (!cache.ContainsKey(negocio.ToString()))
            {
                cache[negocio.ToString()] = contexto.Estados(negocio).ToList();
            }
            return (List<EstadoDtm>)cache[negocio.ToString()];
        }

        public static IQueryable<EstadoDtm> Estados(this ContextoSe contexto, enumNegocio negocio)
        {
            switch (negocio)
            {
                case enumNegocio.Registro:
                    return contexto.Set<EstadoDeUnRegistroEsDtm>().Cast<EstadoDtm>();
                case enumNegocio.Tarea:
                    return contexto.Set<EstadoDeUnaTareaDtm>().Cast<EstadoDtm>();
                case enumNegocio.Expediente:
                    return contexto.Set<EstadoDeUnExpedienteDtm>().Cast<EstadoDtm>();
                case enumNegocio.Pleito:
                    return contexto.Set<EstadoDeUnPleitoDtm>().Cast<EstadoDtm>();
                case enumNegocio.Contrato:
                    return contexto.Set<EstadoDeUnContratoDtm>().Cast<EstadoDtm>();
                case enumNegocio.Presupuesto:
                    return contexto.Set<EstadoDeUnPresupuestoDtm>().Cast<EstadoDtm>();
                case enumNegocio.PlanificacionDeVenta:
                    return contexto.Set<EstadoDeUnaPlanificacionDeVentaDtm>().Cast<EstadoDtm>();
                case enumNegocio.ParteDeTrabajo:
                    return contexto.Set<EstadoDeUnParteTrDtm>().Cast<EstadoDtm>();
                case enumNegocio.FacturaEmitida:
                    return contexto.Set<EstadoDeUnaFacturaEmtDtm>().Cast<EstadoDtm>();
                case enumNegocio.FacturaRecibida:
                    return contexto.Set<EstadoDeUnaFacturaRecDtm>().Cast<EstadoDtm>();
                case enumNegocio.Pedido:
                    return contexto.Set<EstadoDeUnPedidoDtm>().Cast<EstadoDtm>();
                case enumNegocio.Preasiento:
                    return contexto.Set<EstadoDeUnPreasientoDtm>().Cast<EstadoDtm>();
                case enumNegocio.RemesaFae:
                    return contexto.Set<EstadoDeUnaRemesaFaeDtm>().Cast<EstadoDtm>();
                case enumNegocio.RemesaPag:
                    return contexto.Set<EstadoDeUnaRemesaPagDtm>().Cast<EstadoDtm>();
                case enumNegocio.CircuitoDoc:
                    return contexto.Set<EstadoDeUnCircuitoDocDtm>().Cast<EstadoDtm>();
                case enumNegocio.Pago:
                    return contexto.Set<EstadoDeUnPagoDtm>().Cast<EstadoDtm>();
                case enumNegocio.Almacen:
                    return contexto.Set<EstadoDeUnAlmacenDtm>().Cast<EstadoDtm>();
                case enumNegocio.Regularizacion:
                    return contexto.Set<EstadoDeUnaRegularizacionDtm>().Cast<EstadoDtm>();
                case enumNegocio.Venta:
                    return contexto.Set<EstadoDeUnaVentaDtm>().Cast<EstadoDtm>();
                default:
                    throw new Exception($"Se debe indicar como obtener el dbSet de los estados del negocio: {negocio}");
            }

        }

        public static T PersistirEstado<T>(this T estado, ContextoSe contexto, bool errorSiYaExiste = false, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
        where T : EstadoDtm
        {
            if (estado.Id > 0)
                return estado.Modificar(contexto);

            var leido = contexto.SeleccionarEstado<T>(nameof(INombre.Nombre), estado.Nombre, errorSiNoHay: errorSiYaExiste);
            if (leido == null)
                return estado.Insertar(contexto, parametros, aplicarJoin);

            estado.Id = leido.Id;
            estado.IdPermiso = leido.IdPermiso;
            return estado.Modificar(contexto, parametros, aplicarJoin);
        }

        public static EstadoDtm SeleccionarEstado<T>(this ContextoSe contexto, int id, bool errorSiNoHay = true, bool errorSiMasDeuno = true, bool aplicarJoin = false)
        where T : EstadoDtm
        {
            if (typeof(T) == typeof(EstadoDtm))
                throw new Exception($"El método {nameof(SeleccionarEstado)} necesita que se le indique el negocio usado el tipo de transición");

            return SeleccionarEstado<T>(contexto, nameof(EstadoDtm.Id), id.ToString(), errorSiNoHay, errorSiMasDeuno, aplicarJoin);
        }

        public static EstadoDtm SeleccionarEstado<T>(this ContextoSe contexto, string nombre, bool errorSiNoHay = true, bool errorSiMasDeuno = true, bool aplicarJoin = false)
        where T : EstadoDtm
        => SeleccionarEstado<T>(contexto, nameof(EstadoDtm.Nombre), nombre, errorSiNoHay, errorSiMasDeuno, aplicarJoin);

        public static EstadoDtm SeleccionarEstado<T>(this ContextoSe contexto, string propiedad, string valor, bool errorSiNoHay = true, bool errorSiMasDeuno = true, bool aplicarJoin = false)
        where T : EstadoDtm
        {
            var gestor = NegociosDeSe.CrearGestorDeUnDtm<T>(contexto);
            var filtros = new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(propiedad, enumCriteriosDeFiltrado.igual, valor) };
            var seleccionados = (List<EstadoDtm>)gestor.LeerRegistros(0, -1, filtros, aplicarJoin);
            var mensaje = $"para la clase {typeof(T).FullName} con el criterio '{propiedad}' con valor '{valor}'";
            return DevolverSeleccionado(seleccionados, mensaje, errorSiNoHay, errorSiMasDeuno);
        }

        public static List<EstadoDtm> Estados<T>(this ContextoSe contexto, string propiedad, object valor, bool aplicarJoin = false)
        where T : EstadoDtm
        {
            var gestor = NegociosDeSe.CrearGestorDeUnDtm<T>(contexto);
            var filtros = new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(propiedad, enumCriteriosDeFiltrado.igual, valor.ToString()) };
            var seleccionados = (List<EstadoDtm>)gestor.LeerRegistros(0, -1, filtros, aplicarJoin);
            return seleccionados;
        }

        public static List<EstadoDtm> Siguientes<T>(this T estado, ContextoSe contexto, enumNegocio negocio)
        where T : EstadoDtm
        {
            return negocio.Siguientes<EstadoDtm>(contexto, estado.Id);
        }

        private static EstadoDtm DevolverSeleccionado(List<EstadoDtm> seleccionados, string mensaje, bool errorSiNoHay, bool errorSiMasDeuno)
        {
            if (seleccionados.Count == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No hay ningún elemento {mensaje}");

            if (seleccionados.Count > 1 && errorSiMasDeuno)
                GestorDeErrores.Emitir($"Hay mas de un elemento {mensaje}");

            return seleccionados.Count == 0 ? null : seleccionados[0];
        }

        public static bool EstaEnLaEtapa(int idEstado, string estadosDeUnaEtapa) => estadosDeUnaEtapa.ToLista<int>(Simbolos.Coma).Contains(idEstado);

        // ───────────── Etapas en las que está un estado ─────────────
        // Cada etapa del negocio (enumerado indicado en MetadatosDelNegocio.TipoEtapas) es un parámetro de negocio con el mismo nombre
        // cuyo valor son los ids de los estados que la componen separados por comas ('0' si no tiene ninguno).

        private static Type TipoDeEtapas(this enumNegocio negocio) => negocio.ObtenerMetadatos(emitirError: false)?.TipoEtapas;

        private static List<int> EstadosDeLaEtapa(this enumNegocio negocio, Type tipoDeEtapas, string etapa)
        {
            var valor = negocio.Parametro((Enum)Enum.Parse(tipoDeEtapas, etapa), emitirError: false)?.Valor;
            if (valor.IsNullOrEmpty())
                return new List<int>();

            return valor.Split(',')
                        .Select(x => x.Trim())
                        .Where(x => int.TryParse(x, out var id) && id > 0)
                        .Select(int.Parse)
                        .Distinct()
                        .ToList();
        }

        /// <summary>Etapas del negocio indicando cuáles incluyen al estado. Vacía si el negocio no define etapas.</summary>
        public static List<EtapaDeUnEstado> EtapasDeUnEstado(this enumNegocio negocio, int idEstado)
        {
            var tipo = negocio.TipoDeEtapas();
            if (tipo is null)
                return new List<EtapaDeUnEstado>();

            return Enum.GetValues(tipo).Cast<Enum>()
                       .Select(etapa => new EtapaDeUnEstado
                       {
                           Nombre = etapa.ToString(),
                           Descripcion = etapa.Descripcion(),
                           Incluida = negocio.EstadosDeLaEtapa(tipo, etapa.ToString()).Contains(idEstado)
                       })
                       .ToList();
        }

        /// <summary>
        /// Deja al estado únicamente en las etapas indicadas, añadiendo o quitando su id en el parámetro de cada etapa que cambie.
        /// Devuelve un aviso (no un error) si el estado queda en más de una etapa, porque hay negocios en los que sólo puede estar en una.
        /// </summary>
        public static string ActualizarEtapasDeUnEstado(this enumNegocio negocio, ContextoSe contexto, int idEstado, List<string> etapasMarcadas)
        {
            var tipo = negocio.TipoDeEtapas();
            if (tipo is null)
                return string.Empty;

            var marcadas = new HashSet<string>(etapasMarcadas ?? new List<string>());
            var etapasDelEstado = new List<string>();
            foreach (var nombre in Enum.GetNames(tipo))
            {
                var estados = negocio.EstadosDeLaEtapa(tipo, nombre);
                var estaba = estados.Contains(idEstado);
                var debeEstar = marcadas.Contains(nombre);

                if (estaba != debeEstar)
                {
                    if (debeEstar) estados.Add(idEstado); else estados.Remove(idEstado);
                    var valor = estados.Count == 0 ? ltrEstados.EstadoNulo : string.Join(", ", estados.OrderBy(x => x));
                    negocio.Resetear((Enum)Enum.Parse(tipo, nombre), valor);
                }

                if (debeEstar) etapasDelEstado.Add(nombre);
            }

            if (etapasDelEstado.Count <= 1)
                return string.Empty;

            return $"El estado '{negocio.Estado(contexto, idEstado).Nombre}' ha quedado en las etapas {string.Join(", ", etapasDelEstado)}. " +
                   $"Hay negocios en los que un estado sólo puede estar en una etapa; si es el caso, las operaciones que consultan la etapa del elemento darán error.";
        }

        public static List<PosicionDeEstado> PosicionesDeEstados(this enumNegocio negocio,  ContextoSe contexto)
        {
            var jDatos = (JObject)ExtensorDeParmetrosDeUsuario.LeerParametroDeUsuario<JObject>(negocio, contexto, enumParametrosDeUsuario.USU_Disposicion_Estados);

            if (jDatos == null || !jDatos.HasValues)
                return new List<PosicionDeEstado>();

            return jDatos["estados"]?.ToObject<List<PosicionDeEstado>>() ?? new List<PosicionDeEstado>();
        }
    }

    /// <summary>Una etapa del negocio y si incluye o no al estado que se está editando</summary>
    public class EtapaDeUnEstado
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Incluida { get; set; }
    }
}
