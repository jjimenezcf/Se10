using System.Collections.Generic;
using ModeloDeDto;
using NuGet.Protocol;
using ServicioDeCorreos;
using ServicioDeDatos.TrabajosSometidos;
using ServicioDeDatos;
using Microsoft.Extensions.DependencyInjection;
using System;
using static ServicioDeCorreos.ServicioDeCorreo;
using Utilidades;
using System.Threading.Tasks;
using Gestor.Errores;
using System.Linq;
using System.IO;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeCorreos
    {
        public static CorreoDtm CrearCorreo(this ContextoSe contexto, List<string> receptores, string asunto, string cuerpo, List<TipoDtoElmento> elementos, List<string> archivos, Dictionary<string, object> parametros = null)
        {
            var correo = new CorreoDtm();
            correo.IdUsuario = contexto.DatosDeConexion.IdUsuario;
            correo.Emisor = new ServicioDeCorreo(CacheDeVariable.Cfg_ServidorDeCorreo).Emisor;
            correo.Receptores = receptores.ToJson();
            correo.Asunto = asunto;
            correo.Cuerpo = cuerpo;
            correo.Elementos = elementos.ToJson();
            correo.Archivos = archivos.ToJson();

            if (parametros == null) parametros = new Dictionary<string, object>();
            parametros[ltrParametrosNeg.ValidarPermisosDePersistencia] = false;
            parametros.Add(nameof(TipoDtoElmento), elementos);

            return correo.Insertar(contexto, parametros);
        }

        public static void RegistrarConEnvio(this ContextoSe contexto, string asunto, Exception e)
        =>
        contexto.RegistrarConEnvio(asunto, GestorDeErrores.Detalle(e));

        public static void RegistrarConEnvio(this ContextoSe contexto, string asunto, string error)
        {
            contexto.RegistrarConEnvio(CacheDeVariable.CFG_Ruta_Ficheros_De_Excepciones, nombreFichero: asunto.Left(50) + $"_{DateTime.Now}", asunto, error, borrar: true);
        }

        public static void RegistrarConEnvio(this ContextoSe contexto, string ruta, string nombreFichero, string asunto, string contenido, bool borrar = true)
        {
            contexto.RegistrarLogPorFecha(ruta, nombreFichero, contenido, borrar);
            contexto.EnviarCorreoPorAdministrador(CacheDeVariable.Cfg_ServidorDeCorreo, CacheDeVariable.Cfg_CorreoDeSoporte.Split(';').ToList(), $"{contexto.DatosDeConexion.Bd} ({contexto.DatosDeConexion.Login}): " + asunto, contenido);
        }

        public static void EnviarCorreoPorAdministrador(this ContextoSe contexto, string servidor, List<string> receptores, string asunto, string mensaje, bool esHtlm = true, List<string> archivos = null, ManejadorDeCorreo manejador = null)
        {
            var distribuidor = contexto.DistribuidorDeCorreos();
            if (distribuidor == null)
            {
                contexto.CrearCorreo(receptores, asunto, mensaje, elementos: new List<TipoDtoElmento>(), archivos == null ? new List<string>() : archivos);
            }
            var servicio = new ServicioDeCorreo(servidor, distribuidor);
            servicio.EnviarPara(contexto.Administrador().Login, receptores, asunto, mensaje, esHtlm, archivos == null ? new List<string>() : archivos, manejador);
        }

        public static async Task EnviarCorreo<T>(this ContextoSe contexto, string servidor, CorreoDtm correo, bool ejecutadoPorLaCola, bool esHtlm = true)
        {
            string cuerpo = correo is not null ? AdjuntarElementos(correo) : "";
            var archivos = correo is not null ? correo.Archivos.JsonToLista<string>() : new List<string>();
            var receptores = correo is not null ? correo.Receptores.JsonToLista<string>() : new List<string>();

            //ObtenerContextoParaUnTs(contexto, ejecutadoPorLaCola),

            var manejador = new ManejadorDeCorreo
            {
                CorreoDtm = correo,
                Contexto = ContextoSe.Crear(contexto, ejecutadoPorLaCola),
                GestorDeCorreo = typeof(T)
            };

            var servicio = new ServicioDeCorreo(servidor, contexto.DistribuidorDeCorreos());

            var login = correo?.Usuario?.Login ?? contexto.Administrador().Login;

            await servicio.EnviarDe(login, correo?.Emisor ?? contexto.Administrador().Login, receptores, correo?.Asunto ?? $"Mensaje del sistema {contexto.DatosDeConexion.Bd}", cuerpo, esHtlm, archivos, manejador);
        }

        public static async Task<int> EliminarCorreos<T>(this ContextoSe contexto, string servidor, int anterioresA, bool ejecutadoPorLaCola)
        {
            var servicio = new ServicioDeCorreo(servidor, contexto.DistribuidorDeCorreos());
            var correosBorrados = await servicio.EliminarCorreos(anterioresA);
            return correosBorrados;
        }

        private static string AdjuntarElementos(CorreoDtm correoDtm)
        {
            var elementos = correoDtm.Elementos.JsonToLista<TipoDtoElmento>();
            var cuerpo = correoDtm.Cuerpo;
            foreach (TipoDtoElmento elemento in elementos)
            {
                cuerpo = $"{cuerpo}{Environment.NewLine}{elemento.ComponerUrl()}";
            }

            return cuerpo;
        }

        private static IDistribuidorDeCorreos DistribuidorDeCorreos(this ContextoSe contexto)
        {
            var servicioDeCorreo = contexto.Configuracion.GetSection(typeof(ltrAppSetting.ServidorDeCorreo).Name).GetSection(CacheDeVariable.Cfg_ServidorDeCorreo);
            var nombreDelSistemaDeCorreo = servicioDeCorreo[ltrAppSetting.ServidorDeCorreo.Sistema];
            if (nombreDelSistemaDeCorreo.IsNullOrEmpty())
            {
                GestorDeErrores.Emitir($"Falta por definir el valor de '{servicioDeCorreo.Path}.{ltrAppSetting.ServidorDeCorreo.Sistema}' en la configuración");
            }
            var sistemaDeCorreo = ApiDeEnsamblados.ToEnumerado<enumSistemaDeCorreo>(nombreDelSistemaDeCorreo);
            try
            {
                IDistribuidorDeCorreos distribuidor = sistemaDeCorreo == enumSistemaDeCorreo.GRAPH
                ? contexto.ServiceProvider.GetRequiredService<IDistribuidorOfice365>()
                : sistemaDeCorreo == enumSistemaDeCorreo.MAILJET
                ? contexto.ServiceProvider.GetRequiredService<IDistribuidorMailJet>()
                : sistemaDeCorreo == enumSistemaDeCorreo.SMTP
                ? contexto.ServiceProvider.GetRequiredService<IDistribuidorSmtp>()
                : null;

                return distribuidor;
            }
            catch
            {
                return null;
            }
        }
    }
}
