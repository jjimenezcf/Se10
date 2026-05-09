using Gestor.Errores;
using Newtonsoft.Json.Linq;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestorDeElementos.Extensores
{

    public class EstadosDeEspanes
    {
        public List<EstadoDeEspan> Estados { get; private set; }
        public EstadosDeEspanes(ContextoSe contexto, enumNegocio negocio, Dictionary<string, object> parametros)
        {
            if (negocio == enumNegocio.No_Definido)
                //Estados = new List<EstadoDeEspan>();
            {
                var nombrevista = parametros.LeerValor<string>(nameof(PlantillaDeFiltradoDtm.Vista));
                var vista = contexto.SeleccionarPorNombre<VistaMvcDtm>(nombrevista, errorSiNoHay: false);
                if (vista == null)
                    Estados = new List<EstadoDeEspan>();
                else
                {
                    var estados = (JObject)ExtensorDeParmetrosDeUsuario.LeerParametroDeVistaPorUsuario<JObject>(vista, contexto, enumParametrosDeUsuario.USU_Vista_De_Edicion);
                    Estados = estados.Count == 0 ? new List<EstadoDeEspan>() : estados[nameof(estados)].ToObject<List<EstadoDeEspan>>();
                }
            }
            else
            {

                var estados = (JObject)ExtensorDeParmetrosDeUsuario.LeerParametroDeUsuario<JObject>(negocio, contexto, enumParametrosDeUsuario.USU_Vista_De_Edicion);
                Estados = estados.Count == 0 ? new List<EstadoDeEspan>() : estados[nameof(estados)].ToObject<List<EstadoDeEspan>>();
            }
        }

        public static string ToString(List<EstadoDeEspan> lista)
        =>
        string.Join(Simbolos.Coma, lista.Select(x => $"{x.GetType().GetProperty(nameof(EstadoDeEspan.IdDelEspan)).GetValue(x)}, {x.GetType().GetProperty((nameof(EstadoDeEspan.Abierto))).GetValue(x)}"));

    }

    public class DatosDeCreacion
    {
        public int? IdCg => (int?)_valores.LeerValor(nameof(IUsaCg.IdCg), (long?)null);
        public int? IdTipo => (int?)_valores.LeerValor(nameof(IUsaTipo.IdTipo), (long?)null);
        public string Nombre => _valores.LeerValor(nameof(INombre.Nombre), (string)null);
        public string Descripcion => _valores.LeerValor(nameof(IUsaDescripcion.Descripcion), (string)null);
        public Dictionary<string, object> Otros
        {
            get
            {
                Dictionary<string, object> otros = new Dictionary<string, object>();
                foreach (var entrada in _valores)
                {
                    if (entrada.Key.Equals(nameof(IUsaCg.IdCg), System.StringComparison.InvariantCultureIgnoreCase) ||
                        entrada.Key.Equals(nameof(IUsaTipo.IdTipo), System.StringComparison.InvariantCultureIgnoreCase) ||
                        entrada.Key.Equals(nameof(INombre.Nombre), System.StringComparison.InvariantCultureIgnoreCase) ||
                        entrada.Key.Equals(nameof(IUsaDescripcion.Descripcion), System.StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    otros.Add(entrada.Key, entrada.Value);
                }
                return otros;
            }
        }

        private Dictionary<string, object> _valores;

        public DatosDeCreacion(ContextoSe contexto, enumNegocio negocio)
        {
            if (negocio == enumNegocio.No_Definido)
                _valores = new Dictionary<string, object>();
            else
            {

                var retorno = (JObject)ExtensorDeParmetrosDeUsuario.LeerParametroDeUsuario<JObject>(negocio, contexto, enumParametrosDeUsuario.USU_Valores_Por_Defecto);
                _valores = retorno.Count == 0 ? new Dictionary<string, object>() : retorno.ToObject<Dictionary<string, object>>();
            }
        }

        public DatosDeCreacion(ContextoSe contexto, int idPantillaDeCreacion)
        {
            var registro = contexto.SeleccionarPorId<PlantillaDeCreacionDtm>(idPantillaDeCreacion);
            var retorno = JObject.Parse(registro.Valor);
            _valores = retorno.Count == 0 ? new Dictionary<string, object>() : retorno.ToObject<Dictionary<string, object>>();
        }
    }

    public static class ExtensorDeParmetrosDeUsuario
    {
        public static void EliminarParametroDeUsuario(this enumNegocio negocio, ContextoSe contexto, enumParametrosDeUsuario parametro)
        {
            var parametroDtm = negocio.ParametroDeUsuario(contexto, parametro, errorSiNoHay: false, errorSinValor: false, errorSinMasDeUno: true);
            if (parametroDtm is null)
                return;

            parametroDtm.Eliminar(contexto);
        }

        public static ParametroDeUsuarioDtm ResetearParametroDeUsuario(this enumNegocio negocio, ContextoSe contexto, enumParametrosDeUsuario parametro, string valor)
        {
            var parametroDtm = negocio.ParametroDeUsuario(contexto, parametro, errorSiNoHay: false, errorSinValor: false, errorSinMasDeUno: true);
            if (parametroDtm is null)
                return new ParametroDeUsuarioDtm
                {
                    IdNegocio = negocio.IdNegocio(),
                    IdUsuario = contexto.DatosDeConexion.IdUsuario,
                    Nombre = parametro.ToString(),
                    Valor = valor,
                }.Insertar(contexto);

            if (parametroDtm.Valor == valor)
                return parametroDtm;

            parametroDtm.Valor = valor;
            return parametroDtm.Modificar(contexto);
        }


        public static void ResetearParametroDeVistaPorUsuario(this VistaMvcDtm vista, ContextoSe Contexto, enumParametrosDeUsuario parametro, string valor)
        {
            var datosDeLaVista = Contexto.SeleccionarPorAk<ParametroVistaPorUsuarioDtm>(new Dictionary<string, object> {
                { nameof(ParametroVistaPorUsuarioDtm.IdVista), vista.Id },
                { nameof(ParametroVistaPorUsuarioDtm.IdUsuario), Contexto.DatosDeConexion.IdUsuario },
                { nameof(ParametroVistaPorUsuarioDtm.Nombre),  parametro.ToString()},
            }, errorSiNoHay: false);

            if (datosDeLaVista is null)
            {
                datosDeLaVista = new ParametroVistaPorUsuarioDtm
                {
                    IdUsuario = Contexto.DatosDeConexion.IdUsuario,
                    IdVista = vista.Id,
                    Nombre = parametro.ToString(),
                    Valor = valor
                }.Insertar(Contexto);
                return;
            }

            if (datosDeLaVista.Valor == valor)
                return;

            datosDeLaVista.Valor = valor;
            datosDeLaVista.Modificar(Contexto);
        }


        public static object LeerParametroDeVistaPorUsuario<T>(this VistaMvcDtm vista, ContextoSe contexto, enumParametrosDeUsuario parametro)
        {
            var datosDeLaVista = contexto.SeleccionarPorAk<ParametroVistaPorUsuarioDtm>(new Dictionary<string, object>
            {
                { nameof(ParametroVistaPorUsuarioDtm.IdVista), vista.Id } ,
                { nameof(ParametroVistaPorUsuarioDtm.IdUsuario), contexto.DatosDeConexion.IdUsuario } ,
                { nameof(ParametroVistaPorUsuarioDtm.Nombre), parametro.ToString() }
            }, errorSiNoHay: false);

            if (datosDeLaVista is null) return typeof(T) == typeof(JObject) ? new JObject() : "";

            return typeof(T) == typeof(JObject) ? JObject.Parse(datosDeLaVista.Valor) : datosDeLaVista.Valor;
        }


        public static object LeerParametroDeUsuario<T>(this enumNegocio negocio, ContextoSe contexto, enumParametrosDeUsuario parametro)
        {
            var registro = contexto.SeleccionarPorAk<ParametroDeUsuarioDtm>(new Dictionary<string, object>
            {
                { nameof(ParametroDeUsuarioDtm.IdNegocio), negocio.IdNegocio() } ,
                { nameof(ParametroDeUsuarioDtm.IdUsuario), contexto.DatosDeConexion.IdUsuario } ,
                { nameof(ParametroDeUsuarioDtm.Nombre), parametro }
            }, errorSiNoHay: false);

            if (registro is null) return typeof(T) == typeof(JObject) ? new JObject() : "";

            return typeof(T) == typeof(JObject) ? JObject.Parse(registro.Valor) : registro.Valor;
        }

        public static void Guardar_USU_Tamano_Del_Encolumnado(this enumNegocio negocio, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var tamanosBd = negocio.LeerParametroDeUsuario<JObject>(contexto, enumParametrosDeUsuario.USU_Tamano_Del_Encolumnado);
            var tamanosColumnas = parametros.LeerValor<List<TamanoDeColumna>>(ltrParametrosEp.datosPeticion);
            JObject tamanosJson = JObject.FromObject(new { tamanos = tamanosColumnas });
            if (tamanosBd is null || tamanosBd.ToString() != tamanosJson.ToString())
            {
                negocio.ResetearParametroDeUsuario(contexto, enumParametrosDeUsuario.USU_Tamano_Del_Encolumnado, tamanosJson.ToString());
                var indice = $"{contexto.DatosDeConexion.IdUsuario}";
                ServicioDeCaches.EliminarCachesDeDescriptores(indice);
            }
        }

        public static void Guardar_USU_Cantidad_A_Leer(this enumNegocio negocio, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var cantidadBd = negocio.LeerParametroDeUsuario<JObject>(contexto, enumParametrosDeUsuario.USU_Cantidad_A_Leer);
            var cantidad = parametros.LeerValor<int>(ltrParametrosEp.datosPeticion);
            JObject cantidadJson = JObject.FromObject(new { cantidadPorLeer = cantidad });
            if (cantidadBd is null || cantidadBd.ToString() != cantidadJson.ToString())
            {
                negocio.ResetearParametroDeUsuario(contexto, enumParametrosDeUsuario.USU_Cantidad_A_Leer, cantidadJson.ToString());
                var indice = $"{contexto.DatosDeConexion.IdUsuario}-{ModoDescriptor.Mantenimiento}";
                ServicioDeCaches.EliminarCachesDeDescriptores(indice);
            }
        }

        public static void Guardar_USU_Tamano_Del_Visor(this enumNegocio negocio, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var tamanoBd = negocio.LeerParametroDeUsuario<JObject>(contexto, enumParametrosDeUsuario.USU_Tamano_Del_Visor);
            var tamano = parametros.LeerValor<int>(ltrParametrosEp.datosPeticion);
            JObject tamanoJson = JObject.FromObject(new { tamanoDelVisor = tamano });
            if (tamanoBd is null || tamanoBd.ToString() != tamanoJson.ToString())
            {
                negocio.ResetearParametroDeUsuario(contexto, enumParametrosDeUsuario.USU_Tamano_Del_Visor, tamanoJson.ToString());
                var indice = $"{contexto.DatosDeConexion.IdUsuario}-{ModoDescriptor.Mantenimiento}";
                ServicioDeCaches.EliminarCachesDeDescriptores(indice);
            }
        }

        public static void Guardar_USU_Mostrar_Visor_Al_Iniciar(this enumNegocio negocio, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var mostrarBd = negocio.LeerParametroDeUsuario<JObject>(contexto, enumParametrosDeUsuario.USU_Mostrar_El_Visor_Al_Iniciar);
            var mostrar = parametros.LeerValor<bool>(ltrParametrosEp.datosPeticion);
            JObject mostrarJson = JObject.FromObject(new { MostrarVisorAlIniciar = mostrar });
            if (mostrarBd is null || mostrarBd.ToString() != mostrarJson.ToString())
            {
                negocio.ResetearParametroDeUsuario(contexto, enumParametrosDeUsuario.USU_Mostrar_El_Visor_Al_Iniciar, mostrarJson.ToString());
                var indice = $"{contexto.DatosDeConexion.IdUsuario}-{ModoDescriptor.Mantenimiento}";
                ServicioDeCaches.EliminarCachesDeDescriptores(indice);
            }
        }

        public static void Guardar_USU_Disposicion_Del_Encolumnado(this enumNegocio negocio, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var disposicionBd = negocio.LeerParametroDeUsuario<JObject>(contexto, enumParametrosDeUsuario.USU_Disposicion_Del_Encolumnado);
            var disposicion = parametros.LeerValor<List<DisposicionDeColumna>>(ltrParametrosEp.datosPeticion);
            JObject encolumnadoJson = JObject.FromObject(new { encolumnado = disposicion });
            if (disposicionBd is null || disposicionBd.ToString() != encolumnadoJson.ToString())
            {
                negocio.ResetearParametroDeUsuario(contexto, enumParametrosDeUsuario.USU_Disposicion_Del_Encolumnado, encolumnadoJson.ToString());
                var indice = $"{contexto.DatosDeConexion.IdUsuario}-{ModoDescriptor.Mantenimiento}";
                ServicioDeCaches.EliminarCachesDeDescriptores(indice);
            }
        }

        public static void Guardar_USU_Ordenacion_Del_Resultado(this enumNegocio negocio, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var ordenacionBd = negocio.LeerParametroDeUsuario<JObject>(contexto, enumParametrosDeUsuario.USU_Ordenacion_Del_Resultado);
            var ordenEstablecido = parametros.LeerValor<List<OrdenDeColumna>>(ltrParametrosEp.datosPeticion);
            JObject ordenacionJobject = JObject.FromObject(new { ordenacion = ordenEstablecido });
            if (ordenacionBd is null || ordenacionBd.ToString() != ordenacionJobject.ToString())
            {
                negocio.ResetearParametroDeUsuario(contexto, enumParametrosDeUsuario.USU_Ordenacion_Del_Resultado, ordenacionJobject.ToString());
                var indice = $"{contexto.DatosDeConexion.IdUsuario}-{ModoDescriptor.Mantenimiento}";
                ServicioDeCaches.EliminarCachesDeDescriptores(indice);
            }
        }

        public static void Guardar_USU_Colunas_Del_Grid(this enumNegocio negocio, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var columnasBd = negocio.LeerParametroDeUsuario<JObject>(contexto, enumParametrosDeUsuario.USU_Colunas_Del_Grid);
            var columnas = parametros.LeerValor<List<VisibilidadDeColumna>>(ltrParametrosEp.datosPeticion);
            JObject columnasJson = JObject.FromObject(new { columnasJson = columnas });
            if (columnasBd is null || columnasBd.ToString() != columnasJson.ToString())
            {
                negocio.ResetearParametroDeUsuario(contexto, enumParametrosDeUsuario.USU_Colunas_Del_Grid, columnasJson.ToString());
                var indice = $"{contexto.DatosDeConexion.IdUsuario}-{ModoDescriptor.Mantenimiento}";
                ServicioDeCaches.EliminarCachesDeDescriptores(indice);
            }
        }


        public static void GuardarElTamanoDeColumnasDeVista(this VistaMvcDtm vista, ContextoSe Contexto, Dictionary<string, object> parametros)
        {
            var tamanosBd = Contexto.SeleccionarPorAk<ParametroVistaPorUsuarioDtm>(new Dictionary<string, object> {
                { nameof(ParametroVistaPorUsuarioDtm.IdVista), vista.Id },
                { nameof(ParametroVistaPorUsuarioDtm.IdUsuario), Contexto.DatosDeConexion.IdUsuario },
                { nameof(ParametroVistaPorUsuarioDtm.Nombre), enumParametrosDeUsuario.USU_Tamano_Del_Encolumnado },
            }, errorSiNoHay: false);


            var tamanosColumnas = parametros.LeerValor<List<TamanoDeColumna>>(ltrParametrosEp.datosPeticion);
            JObject tamanosJson = JObject.FromObject(new { tamanos = tamanosColumnas });
            vista.GuardarParametroDeVista(Contexto, enumParametrosDeUsuario.USU_Tamano_Del_Encolumnado, tamanosBd, tamanosJson.ToString());
            //if (tamanosBd is null || tamanosBd.Valor != tamanosJson.ToString())
            //{
            //    if (tamanosBd is null) tamanosBd = new ParametroVistaPorUsuarioDtm
            //    {
            //        IdUsuario = Contexto.DatosDeConexion.IdUsuario,
            //        IdVista = vista.Id,
            //        Nombre = enumParametrosDeUsuario.USU_Tamano_Del_Encolumnado.ToString(),
            //        Valor = tamanosJson.ToString()
            //    }.Insertar(Contexto);
            //    else
            //    {

            //        tamanosBd.Valor = tamanosJson.ToString();
            //        tamanosBd.Modificar(Contexto);
            //    }
            //    var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{ModoDescriptor.Mantenimiento}";
            //    ServicioDeCaches.EliminarCachesDeDescriptores(indice);
            //}
        }


        public static void Guardar_USU_Cantidad_A_Leer(this VistaMvcDtm vista, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (vista == null)
                return;

            var cantidadBd = contexto.SeleccionarPorAk<ParametroVistaPorUsuarioDtm>(new Dictionary<string, object> {
                { nameof(ParametroVistaPorUsuarioDtm.IdVista), vista.Id },
                { nameof(ParametroVistaPorUsuarioDtm.IdUsuario), contexto.DatosDeConexion.IdUsuario },
                { nameof(ParametroVistaPorUsuarioDtm.Nombre), enumParametrosDeUsuario.USU_Cantidad_A_Leer },
            }, errorSiNoHay: false);
            var cantidad = parametros.LeerValor<int>(ltrParametrosEp.datosPeticion);
            JObject cantidadJson = JObject.FromObject(new { cantidadPorLeer = cantidad });
            vista.GuardarParametroDeVista(contexto, enumParametrosDeUsuario.USU_Cantidad_A_Leer, cantidadBd, cantidadJson.ToString());
        }


        public static void GuardarDisposicionDeArchivos(this VistaMvcDtm vista, ContextoSe Contexto, Dictionary<string, object> parametros)
        {
            var disposicionBd = Contexto.SeleccionarPorAk<ParametroVistaPorUsuarioDtm>(new Dictionary<string, object> {
                { nameof(ParametroVistaPorUsuarioDtm.IdVista), vista.Id },
                { nameof(ParametroVistaPorUsuarioDtm.IdUsuario), Contexto.DatosDeConexion.IdUsuario },
                { nameof(ParametroVistaPorUsuarioDtm.Nombre), enumParametrosDeUsuario.USU_Disposicion_Archivos },
            }, errorSiNoHay: false);


            var disposicionDeArchivos = parametros.LeerValor<int>(ltrParametrosEp.datosPeticion);
            if (disposicionDeArchivos <= 0) disposicionDeArchivos = 5;
            JObject disposicionJson = JObject.FromObject(new { columnas = disposicionDeArchivos });
            vista.GuardarParametroDeVista(Contexto, enumParametrosDeUsuario.USU_Disposicion_Archivos, disposicionBd, disposicionJson.ToString());

            //if (disposicionBd is null || disposicionBd.Valor != disposicionJson.ToString())
            //{
            //    if (disposicionBd is null) disposicionBd = new ParametroVistaPorUsuarioDtm
            //    {
            //        IdUsuario = Contexto.DatosDeConexion.IdUsuario,
            //        IdVista = vista.Id,
            //        Nombre = enumParametrosDeUsuario.USU_Disposicion_Archivos.ToString(),
            //        Valor = disposicionJson.ToString()
            //    }.Insertar(Contexto);
            //    else
            //    {

            //        disposicionBd.Valor = disposicionJson.ToString();
            //        disposicionBd.Modificar(Contexto);
            //    }
            //}
        }

        private static void GuardarParametroDeVista(this VistaMvcDtm vista, ContextoSe contexto, enumParametrosDeUsuario parametro, ParametroVistaPorUsuarioDtm parametroDeBd, string valorJson)
        {
            if (parametroDeBd is null || parametroDeBd.Valor != valorJson)
            {
                if (parametroDeBd is null) parametroDeBd = new ParametroVistaPorUsuarioDtm
                {
                    IdUsuario = contexto.DatosDeConexion.IdUsuario,
                    IdVista = vista.Id,
                    Nombre = parametro.ToString(),
                    Valor = valorJson.ToString()
                }.Insertar(contexto);
                else
                {

                    parametroDeBd.Valor = valorJson.ToString();
                    parametroDeBd.Modificar(contexto);
                }
                var indice = $"{contexto.DatosDeConexion.IdUsuario.ToString()}-{ModoDescriptor.Mantenimiento}";
                ServicioDeCaches.EliminarCachesDeDescriptores(indice);
            }
        }

        public static int LeerDisposicionDeArchivos(this VistaMvcDtm vista, ContextoSe Contexto, Dictionary<string, object> parametros)
        {
            var disposicionBd = Contexto.SeleccionarPorAk<ParametroVistaPorUsuarioDtm>(new Dictionary<string, object> {
                { nameof(ParametroVistaPorUsuarioDtm.IdVista), vista.Id },
                { nameof(ParametroVistaPorUsuarioDtm.IdUsuario), Contexto.DatosDeConexion.IdUsuario },
                { nameof(ParametroVistaPorUsuarioDtm.Nombre), enumParametrosDeUsuario.USU_Disposicion_Archivos },
            }, errorSiNoHay: false);


            if (disposicionBd is null)
                return 5;

            var i = ExtraerNumeroDeColumnas(disposicionBd.Valor);
            if (i <= 0) return 5;
            return i;
        }

        private static int ExtraerNumeroDeColumnas(string jsonString)
        {
            var jsonObject = JObject.Parse(jsonString);

            if (jsonObject.TryGetValue("columnas", out JToken columnasToken))
            {
                return columnasToken.Value<int>();
            }

            throw new InvalidOperationException("No se pudo encontrar el valor de 'columnas' en el JSON.");
        }

        private static ParametroDeUsuarioDtm ParametroDeUsuario(this enumNegocio negocio, ContextoSe contexto, enumParametrosDeUsuario parametro, bool errorSiNoHay, bool errorSinValor, bool errorSinMasDeUno = true)
        {

            var cache = ServicioDeCaches.Obtener(CacheDe.Negocio_ParametrosDeUsuario);
            var indice = $"{negocio}-{contexto.DatosDeConexion.IdUsuario}-{parametro}";

            if (!cache.ContainsKey(indice))
            {
                var filtros = new Dictionary<string, object>
                           {
                               {nameof(ParametroDeUsuarioDtm.IdNegocio), negocio.IdNegocio() },
                               {nameof(ParametroDeUsuarioDtm.IdUsuario), contexto.DatosDeConexion.IdUsuario },
                               {nameof(ParametroDeUsuarioDtm.Nombre), parametro }
                           };
                var registro = contexto.SeleccionarTodos<ParametroDeUsuarioDtm>(filtros);

                if (errorSiNoHay && registro.Count() == 0)
                    GestorDeErrores.Emitir($"Debe definir para el negocio de {enumNegocio.Negocio.Singular(true)} el parámetro '{parametro}'");

                if (errorSinValor && registro.Count() == 1 && registro[0].Valor.IsNullOrEmpty())
                    GestorDeErrores.Emitir($"Debe definir un valor para el parámetro '{parametro}' del negocio {enumNegocio.Negocio.Singular(true)}");

                if (errorSinMasDeUno && registro.Count() > 1)
                    GestorDeErrores.Emitir($"Hay más de un parámetro definido '{parametro}' para el negocio {enumNegocio.Negocio.Singular(true)}");
                if (registro.Count() == 0) return null;

                cache[indice] = registro[0];
            }

            return (ParametroDeUsuarioDtm)cache[indice];
        }

    }
}
