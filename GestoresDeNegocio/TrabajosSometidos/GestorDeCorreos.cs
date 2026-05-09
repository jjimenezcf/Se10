using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.TrabajosSometidos;
using ModeloDeDto.TrabajosSometidos;
using System;
using GestoresDeNegocio.Entorno;
using Utilidades;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ModeloDeDto;
using Gestor.Errores;
using ServicioDeDatos.Entorno;
using System.Threading.Tasks;
using Dapper;
using GestorDeElementos.Extensores;
using ModeloDeDto.Negocio;

namespace GestoresDeNegocio.TrabajosSometidos
{
    public class GestorDeCorreos : GestorDeElementos<ContextoSe, CorreoDtm, CorreoDto>
    {

        public class ltrParamCorreos
        {
            internal static readonly string usuarios = nameof(usuarios);
            internal static readonly string puestos = nameof(puestos);
            internal static readonly string receptores = nameof(receptores);
            internal static readonly string asunto = nameof(asunto);
            internal static readonly string cuerpo = nameof(cuerpo);
            internal static readonly string adjuntos = nameof(adjuntos);
            internal static readonly string archivos = nameof(archivos);
            internal static readonly string elementosDeNegocio = nameof(elementosDeNegocio);
            internal static readonly string JoinConUsuarios = nameof(JoinConUsuarios);
        }


        public class MapearArchivos : Profile
        {
            public MapearArchivos()
            {
                CreateMap<CorreoDtm, CorreoDto>()
                .ForMember(dto => dto.Creador, dtm => dtm.MapFrom(x => $"({x.Usuario.Login})- {x.Usuario.Nombre} {x.Usuario.Apellido}"));
                CreateMap<CorreoDto, CorreoDtm>();
            }
        }

        public static bool HayVistaParaMostrarElDto<T>() where T : ElementoDto
        {
            try
            {
                ExtensionesDto.UrlBaseDeUnDto(typeof(T), vista: "", errorSiMasDeUno: false);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public GestorDeCorreos(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeCorreos Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCorreos(contexto, mapeador);
        }

        protected override IQueryable<CorreoDtm> AplicarJoins(IQueryable<CorreoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            if (parametros.HacerJoinCon(ltrParamCorreos.JoinConUsuarios))
                consulta = consulta.Include(p => p.Usuario);
            return consulta;
        }

        protected override IQueryable<CorreoDtm> AplicarFiltros(IQueryable<CorreoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            if (!Contexto.DatosDeConexion.EsAdministrador)
                consulta = consulta.Where(x => x.IdUsuario == Contexto.DatosDeConexion.IdUsuario);

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Clausula.ToLower() == nameof(ElementoDtm.Nombre).ToLower())
                {
                    filtro.Clausula = $"{nameof(CorreoDtm.Asunto)}{Simbolos.Pipe}{nameof(CorreoDtm.Cuerpo)}";
                    consulta = consulta.AplicarFiltroDeCadena(filtro);
                }

                if (filtro.Clausula.ToLower() == ltrFltCorreosDto.seHaEnviado.ToLower())
                    consulta = Filtrar.AplicarFiltroPorFechaNoNula(consulta, nameof(CorreoDtm.Enviado));

                if (filtro.Clausula.ToLower() == ltrFltCorreosDto.NoSeHaEnviado.ToLower())
                    consulta = Filtrar.AplicarFiltroPorFechaNula(consulta, nameof(CorreoDtm.Enviado));

                if (filtro.Clausula.ToLower() == ltrFltCorreosDto.receptores.ToLower())
                {
                    var usuario = Contexto.Set<UsuarioDtm>().FirstOrDefault(u => u.Id == filtro.Valor.Entero());
                    consulta = consulta.Where(x => x.Receptores.Contains(usuario.eMail));
                    filtro.Aplicado = true;
                }

            }

            return consulta;
        }

        protected override void DespuesDePersistir(CorreoDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            var enviados = parametros.Parametros.LeerValor<List<TipoDtoElmento>>(nameof(TipoDtoElmento), null);
            if (enviados != null) foreach (var enviado in enviados)
            {

                var negocio = NegociosDeSe.NegocioDeUnDto(enviado.ClaseDto());
                if (negocio == enumNegocio.No_Definido || !negocio.UsaTrazas())
                    continue;

                var traza = new TrazaDtm();
                traza.IdElemento = enviado.IdElemento;
                traza.Negocio = negocio;
                traza.Nombre = enumTraza.envioDeCorreo.Descripcion();
                traza.Descripcion = $"Emisor: {registro.Emisor}{Environment.NewLine}" +
                                    $"Receptor: {registro.Receptores}{Environment.NewLine}" +
                                    $"{ltrDeTrazas.asuntoCorreo}{registro.Asunto}{Environment.NewLine}" +
                                    $"{ltrDeTrazas.cuerpoCorreo}{Environment.NewLine}{registro.Cuerpo}";
                traza.InsertarTraza(Contexto);
            }
        }

        public static CorreoDtm CrearCorreoPara(ContextoSe contexto, List<string> receptores, string asunto, string cuerpo, List<TipoDtoElmento> elementos, List<string> archivos)
        =>
        contexto.CrearCorreo(receptores, asunto, cuerpo, elementos, archivos);

        public static CorreoDtm CrearCorreoDe(ContextoSe contexto, string emisor, List<string> receptores, string asunto, string cuerpo, List<TipoDtoElmento> elementos, List<string> archivos)
        {
            var correo = new CorreoDtm();
            correo.IdUsuario = contexto.DatosDeConexion.IdUsuario;
            correo.Emisor = emisor;
            correo.Receptores = receptores.ToJson();
            correo.Asunto = asunto;
            correo.Cuerpo = cuerpo;
            correo.Elementos = elementos.ToJson();
            correo.Archivos = archivos.ToJson();
            var gestor = Gestor(contexto, contexto.Mapeador);
            var p = new ParametrosDeNegocio(enumTipoOperacion.Insertar);
            p.Parametros[ltrParametrosNeg.ValidarPermisosDePersistencia] = false;
            p.Parametros.Add(nameof(TipoDtoElmento), elementos);
            return gestor.PersistirRegistro(correo, p);
        }

        public static CorreoDtm CrearCorreoDe(ContextoSe contexto, Dictionary<string, object> parametros)
        {

            if (!parametros.ContainsKey(ltrParamCorreos.archivos))
                parametros[ltrParamCorreos.archivos] = new List<string>();

            ValidarParametrosDeCorreo(contexto, parametros);

            var otorgarPermisos = parametros.ContieneClave(nameof(EnviarElementoDto.OtorgarGestor));
            if (otorgarPermisos)
            {
                var deGestor = parametros.LeerValor<bool>(nameof(EnviarElementoDto.OtorgarGestor), false);
                var elementos = (List<TipoDtoElmento>)parametros[ltrParamCorreos.adjuntos];
                if (elementos != null && elementos.Count == 1)
                {
                    var negocio = NegociosDeSe.NegocioDeUnDto(elementos[0].ClaseDto());
                    var registro = negocio.LeerRegistro(contexto, elementos[0].IdElemento);
                }
            }

            return CrearCorreoDe(contexto
                , GestorDeUsuarios.LeerUsuario(contexto, contexto.DatosDeConexion.IdUsuario).eMail
                , (List<string>)parametros[ltrParamCorreos.receptores]
                , (string)parametros[ltrParamCorreos.asunto]
                , (string)parametros[ltrParamCorreos.cuerpo]
                , (List<TipoDtoElmento>)parametros[ltrParamCorreos.adjuntos]
                , (List<string>)parametros[ltrParamCorreos.archivos]);
        }

        private static void ValidarParametrosDeCorreo(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (!parametros.ContainsKey(ltrParamCorreos.usuarios) && !parametros.ContainsKey(ltrParamCorreos.puestos))
                GestorDeErrores.Emitir("Debe indicar algún receptor");

            var usuarios = parametros.ContainsKey(ltrParamCorreos.usuarios) ? parametros[ltrParamCorreos.usuarios].ToString().JsonToLista<int>() : new List<int>();
            var puestos = parametros.ContainsKey(ltrParamCorreos.puestos) ? parametros[ltrParamCorreos.puestos].ToString().JsonToLista<int>() : new List<int>();

            if (usuarios.Count == 0 && puestos.Count == 0)
                GestorDeErrores.Emitir("Debe indicar algún receptor");

            var lista = new List<string>();
            foreach (var idUsuario in usuarios)
            {
                var mail = GestorDeUsuarios.LeerUsuario(contexto, idUsuario).eMail;
                if (!lista.Any(x => x.Equals(mail, StringComparison.CurrentCultureIgnoreCase))) lista.Add(mail);
            }

            foreach (var idPuesto in puestos)
            {
                List<UsuarioDtm> usuariosDelPuesto = ExtensorDeUsuarios.UsuariosDeUnPuesto(contexto, idPuesto);
                foreach (var usuario in usuariosDelPuesto)
                    if (!lista.Any(x => x.Equals(usuario.eMail, StringComparison.CurrentCultureIgnoreCase))) lista.Add(usuario.eMail);
            }

            parametros[ltrParamCorreos.receptores] = lista;

            if (((string)parametros[ltrParamCorreos.asunto]).IsNullOrEmpty()) GestorDeErrores.Emitir("Debe indicar el asunto");
            if (((string)parametros[ltrParamCorreos.cuerpo]).IsNullOrEmpty()) GestorDeErrores.Emitir("Debe indicar el cuerpo");

            if (parametros.ContainsKey(ltrParamCorreos.adjuntos))
            {
                var elementosDto = parametros[ltrParamCorreos.adjuntos].ToString().JsonToLista<TipoDtoElmento>();
                parametros[ltrParamCorreos.adjuntos] = elementosDto; // lista;
            }

        }

        public void EnviarCorreoPendientes()
        {
            var filtro = new ClausulaDeFiltrado(nameof(CorreoDtm.Enviado), enumCriteriosDeFiltrado.esNulo);
            var parametros = new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo);
            parametros.Parametros[ltrParamCorreos.JoinConUsuarios] = false;
            var pendientes = LeerRegistros(0, 10, new List<ClausulaDeFiltrado> { filtro }, null, parametros);
            foreach (var pendiente in pendientes)
                try
                {
                    var resultado = EnviarCorreoDeAsync(pendiente, ejecutadoPorLaCola: true).Result;
                    if (!resultado.EsOk)
                        throw new Exception(resultado.Mensaje);
                }
                catch (Exception e)
                {
                    try
                    {
                        Contexto.EnviarCorreoPorAdministrador(CacheDeVariable.Cfg_ServidorDeCorreo, new List<string> { "jjimenezcf@gmail.com" }
                            , "Fallo al enviar correos"
                            , $"Error en:{Environment.NewLine}" +
                              $"Servidor '{Contexto.DatosDeConexion.Bd}'{Environment.NewLine}" +
                              $"Enviar el correo con id: {pendiente.Id}{Environment.NewLine}" +
                              $"Asunto: '{pendiente.Asunto}'{Environment.NewLine}" +
                              $"{GestorDeErrores.Mensaje(e)}"
                            );
                        pendiente.Enviado = DateTime.Now;
                        PersistirRegistro(pendiente, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
                    }
                    catch (Exception ei)
                    {
                        Contexto.IniciarTraza("Error en el envío de correo", debugar: true);
                        try
                        {
                            Contexto.AnotarExcepcion(ei);
                        }
                        finally
                        {
                            Contexto.CerrarTraza();
                        }
                    }
                }
        }

        public int EliminarCorreos()
        {
            var correos = 0;
            try
            {
                correos = EliminarCorreosAsync(ejecutadoPorLaCola: true).Result;
            }
            catch (Exception e)
            {
                try
                {
                    Contexto.EnviarCorreoPorAdministrador(CacheDeVariable.Cfg_ServidorDeCorreo, new List<string> { "jjimenezcf@gmail.com" }
                        , "Fallo al eliminar correos de la bandeja de enviados"
                        , $"Error en:{Environment.NewLine}" +
                          $"Servidor '{Contexto.DatosDeConexion.Bd}'{Environment.NewLine}" +
                          $"{GestorDeErrores.Mensaje(e)}"
                        );
                }
                catch (Exception ei)
                {
                    Contexto.IniciarTraza("Error en el envío de correo", debugar: true);
                    try
                    {
                        Contexto.AnotarExcepcion(ei);
                    }
                    finally
                    {
                        Contexto.CerrarTraza();
                    }
                }
            }
            return correos;
        }

        public void EnviarCorreos(List<int> ids)
        {
            foreach (var id in ids)
            {
                var correoDtm = Contexto.SeleccionarPorId<CorreoDtm>(id);
                var resultado = EnviarCorreoDeAsync(correoDtm, ejecutadoPorLaCola: false).Result;
                if (!resultado.EsOk)
                {
                    Contexto.IniciarTraza($"Error al enviar correo {id}", debugar: true);
                    Contexto.AnotarTraza($"Error al enviar el correo '{correoDtm.Asunto}'", resultado.Mensaje);
                    Contexto.CerrarTraza();
                    throw new Exception(resultado.Mensaje);
                }
            }
        }

        private async Task<ResultadoDelProceso> EnviarCorreoDeAsync(CorreoDtm correoDtm, bool ejecutadoPorLaCola)
        {
            try
            {
                await Contexto.EnviarCorreo<GestorDeCorreos>(CacheDeVariable.Cfg_ServidorDeCorreo, correoDtm, ejecutadoPorLaCola, esHtlm: true);
                return new ResultadoDelProceso(true, null);
            }
            catch (Exception exc)
            {
                return new ResultadoDelProceso(false, exc.InnerException is null ? exc.MensajeCompleto(mostrarPila: true) : exc.InnerException.MensajeCompleto(mostrarPila: true));
            }

        }

        private async Task<int> EliminarCorreosAsync(bool ejecutadoPorLaCola)
        {
            var anterioresA = CacheDeVariable.Cfg_Eliminar_Correos_Anteriores_A;
            var correosBorrados = await Contexto.EliminarCorreos<GestorDeCorreos>(CacheDeVariable.Cfg_ServidorDeCorreo, anterioresA, ejecutadoPorLaCola);
            return correosBorrados;
        }

        public static void ActualizarFechaDeEnvio(ContextoSe contexto, CorreoDtm correoDtm)
        {
            var sentencia = new ConsultaSql<CorreoDtm>(contexto, CorreoSql.ActualizarFechaDeEnvio);
            var valores = new Dictionary<string, object> { { $"@{nameof(CorreoDtm.Enviado)}", DateTime.Now }, { $"@{nameof(CorreoDtm.Id)}", correoDtm.Id } };
            sentencia.EjecutarSentencia(new DynamicParameters(valores));
        }

        // Método invocados por reflexión desde el servidor de correos
        public static void IndicarQueElCorreoHaSidoEnviado(ContextoSe contexto, CorreoDtm correoDtm)
        {
            correoDtm.Enviado = DateTime.Now;
            correoDtm.Modificar(contexto, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
        }

        // Método invocados por reflexión desde el servidor de correos
        public static void AnotarTraza(ContextoSe contexto, CorreoDtm correoDtm)
        {
            contexto.IniciarTraza("EnviosDeCorreo_Cancelacion");
            if (contexto.Traza is null)
                return;
            contexto.Traza.AnotarMensaje("Correo enviado", $"El correo {correoDtm.Asunto} se ha cancelado");
            contexto.CerrarTraza("Fin de anotación de cancelación");
        }

        // Método invocados por reflexión desde el servidor de correos
        public static void AnotarExcepcion(ContextoSe contexto, CorreoDtm correoDtm, Exception error)
        {
            var errorMsj = $"El correo '{correoDtm.Asunto}' NO se ha podido enviar.{Environment.NewLine}Error: {error.Message}";

            if (!contexto.TrabajoSometido)
                throw new Exception(errorMsj);

            contexto.IniciarTraza("EnviosDeCorreo_Error");
            if (contexto.Traza is null)
                return;
            contexto.Traza.AnotarMensaje("Error en el envío", errorMsj);
            contexto.CerrarTraza("Fin de anotación de error");
        }

        protected override void AntesDePersistir(CorreoDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                registro.Creado = DateTime.Now;
            }
        }
    }
}
