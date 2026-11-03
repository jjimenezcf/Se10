using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Negocio;
using MVCSistemaDeElementos.Descriptores;
using Newtonsoft.Json.Linq;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class EstadosController : EntidadController<ContextoSe, EstadoDtm, EstadoDto>
    {
        public EstadosController(GestorDeEstados gestorDeEstados, GestorDeErrores gestorDeErrores, IMapper mapper)
        : base(gestorDeEstados, gestorDeErrores)
        {
            Contexto.Mapeador = mapper;
        }

        public IActionResult CrudDeEstados()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            try
            {
                enumNegocio negocio = ApiController.AsignarNegocioDeLaRequest(_GestorDeElementos, HttpContext);
                return ViewCrud(new DescriptorDeEstados(Contexto, ModoDescriptor.Mantenimiento, negocio));
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Transiciones:
                    return null;
                case eventosDeMf.Flujo:
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        [HttpGet]
        public JsonResult epObtenerFlujo(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                if (!parametros.ContieneClave(nameof(ltrParametrosEp.enumNegocio))) throw new Exception("Debe indicar el negocio para el flujo que se quiere mostrar");
                if (!parametros.ContieneClave(nameof(ltrParametrosEp.idEstado))) throw new Exception("Debe indicar el id del estado inicial");

                var negocio = parametros.LeerValor<string>(nameof(ltrParametrosEp.enumNegocio));
                var idEstado = (int)parametros.LeerValor<long>(nameof(ltrParametrosEp.idEstado));
                Contexto.IniciarTraza(GetType().Name + "_" + nameof(epObtenerFlujo));
                try
                {
                    ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                    var negocioEnum = GestorDeElementos.NegociosDeSe.ToEnumerado(negocio);
                    r.Datos = NegociosDeSe.ObtenerFlujoDeNegocio(Contexto, negocioEnum, idEstado);
                    r.Estado = enumEstadoPeticion.Ok;
                    r.Mensaje = "flujo obtenido";
                }
                catch (Exception e)
                {
                    ApiController.PrepararError(e, r, $"Error al obtener el flujo del negocio {negocio} para el estado {idEstado}");
                }
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, e.Message);
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }


        [HttpGet]
        public JsonResult epLeerPosicionesDeEstados(string negocio)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerPosicionesDeEstados));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                r.Datos = NegociosDeSe.ToEnumerado(negocio).PosicionesDeEstados(Contexto);
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"Error al obtener la disposición del dashboard para el usuario {DatosDeConexion.Login}.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }


        [HttpPost]
        public JsonResult epGrabarDisposicionDeEstados(string negocio)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epGrabarDisposicionDeEstados));
            var peticion = eventosDeMf.Comun_GuardarDisposicionEstados.ToString();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var body = ApiController.LeerBody(HttpContext);
                r.Datos = ProcesarPeticion(NegociosDeSe.ToEnumerado(negocio), vista: null, peticion, body.parametros);
                r.Consola = $"Petición '{peticion}' procesada correctamente";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"Error al procesar la petición '{peticion}'.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        /// <summary>
        /// Explicación (del documento de ayuda del flujo) de las transiciones que se pueden aplicar desde un estado: la de avanzar y la de retroceder.
        /// Se busca el apartado '### {estado} ---> {transición} --> ...' del documento. Si no hay documento o apartado, el texto va vacío.
        /// </summary>
        [HttpGet]
        public JsonResult epExplicarTransiciones(string negocio, string estado, string avanzar, string retroceder)
        {
            var r = new Resultado();
            try
            {
                var carpeta = Path.Combine(HttpContext.RequestServices.GetService<IWebHostEnvironment>().WebRootPath, "ayuda", "procesos", NegociosDeSe.ToEnumerado(negocio).ToString());
                var documentos = new List<string>();
                if (Directory.Exists(carpeta) && !string.IsNullOrWhiteSpace(estado))
                    documentos = Directory.GetFiles(carpeta, "*.md").OrderBy(f => f).Select(f => System.IO.File.ReadAllText(f)).Where(t => t.Contains(estado)).ToList();

                string Explicacion(string transicion)
                {
                    if (string.IsNullOrWhiteSpace(transicion)) return "";
                    var cabecera = $"### {estado} ---> {transicion} --> ";
                    foreach (var texto in documentos)
                    {
                        var lineas = texto.Replace("\r", "").Split('\n').ToList();
                        var inicio = lineas.FindIndex(l => l.StartsWith(cabecera));
                        if (inicio < 0) continue;
                        var cuerpo = lineas.Skip(inicio + 1).TakeWhile(l => !l.StartsWith("#")).ToList();
                        return string.Join("\n", cuerpo).Trim();
                    }
                    return "";
                }

                r.Datos = new { hayDocumento = documentos.Count > 0, avanzar = Explicacion(avanzar), retroceder = Explicacion(retroceder) };
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"Error al leer la explicación de las transiciones del estado {estado}.");
            }
            return new JsonResult(r);
        }

        /// <summary>Descarga el documento .md de ayuda del flujo al que pertenece el estado</summary>
        [HttpGet]
        public IActionResult epDescargarAyudaDelFlujo(string negocio, string estado)
        {
            try
            {
                var carpeta = Path.Combine(HttpContext.RequestServices.GetService<IWebHostEnvironment>().WebRootPath, "ayuda", "procesos", NegociosDeSe.ToEnumerado(negocio).ToString());
                if (!Directory.Exists(carpeta) || string.IsNullOrWhiteSpace(estado))
                    return Content("No hay documento de ayuda para este flujo.", "text/plain; charset=utf-8");

                var ficheros = Directory.GetFiles(carpeta, "*.md").OrderBy(f => f).Where(f => System.IO.File.ReadAllText(f).Contains(estado)).ToList();
                var fichero = ficheros.FirstOrDefault(f => System.IO.File.ReadAllText(f).Contains($"### {estado} ---> ")) ?? ficheros.FirstOrDefault();
                if (fichero == null)
                    return Content("No hay documento de ayuda para este flujo.", "text/plain; charset=utf-8");

                return File(System.IO.File.ReadAllBytes(fichero), "text/markdown; charset=utf-8", Path.GetFileName(fichero));
            }
            catch (Exception e)
            {
                return Content($"No se ha podido leer la ayuda: {e.Message}", "text/plain; charset=utf-8");
            }
        }

        /// <summary>
        /// Página con el documento de ayuda (wwwroot/ayuda/procesos/{negocio}/*.md) del flujo al que pertenece el estado.
        /// Se elige el documento que contiene al estado, mejor si lo tiene como origen de alguna transición.
        /// </summary>
        [HttpGet]
        public ContentResult epAyudaDelFlujo(string negocio, string estado)
        {
            string Pagina(string cuerpo) => $"<!doctype html><html><head><meta charset=\"utf-8\"><title>Ayuda del flujo</title></head><body style=\"font-family:sans-serif;max-width:60rem;margin:2rem auto;padding:0 1rem\">{cuerpo}</body></html>";
            try
            {
                var carpeta = Path.Combine(HttpContext.RequestServices.GetService<IWebHostEnvironment>().WebRootPath, "ayuda", "procesos", NegociosDeSe.ToEnumerado(negocio).ToString());
                if (!Directory.Exists(carpeta) || string.IsNullOrWhiteSpace(estado))
                    return Content(Pagina("<p>No hay documento de ayuda para este flujo.</p>"), "text/html; charset=utf-8");

                var documentos = Directory.GetFiles(carpeta, "*.md").OrderBy(f => f).Select(f => System.IO.File.ReadAllText(f)).Where(t => t.Contains(estado)).ToList();
                var texto = documentos.FirstOrDefault(t => t.Contains($"### {estado} ---> ")) ?? documentos.FirstOrDefault();
                if (texto == null)
                    return Content(Pagina("<p>No hay documento de ayuda para este flujo.</p>"), "text/html; charset=utf-8");

                var html = Pagina("<div id=\"doc\"></div>" +
                    "<script src=\"https://cdn.jsdelivr.net/npm/marked@12.0.2/marked.min.js\"></script>" +
                    "<script src=\"https://cdn.jsdelivr.net/npm/mermaid@10.9.1/dist/mermaid.min.js\"></script>" +
                    "<script>const md = " + Newtonsoft.Json.JsonConvert.SerializeObject(texto).Replace("</", "<\\/") + ";" +
                    "const d = document.getElementById('doc'); d.innerHTML = marked.parse(md);" +
                    "d.querySelectorAll('pre > code.language-mermaid').forEach(c => { const m = document.createElement('div'); m.className = 'mermaid'; m.textContent = c.textContent; c.parentElement.replaceWith(m); });" +
                    "mermaid.initialize({ startOnLoad: false }); mermaid.run();</script>");
                return Content(html, "text/html; charset=utf-8");
            }
            catch (Exception e)
            {
                return Content(Pagina($"<p>No se ha podido leer la ayuda: {System.Net.WebUtility.HtmlEncode(e.Message)}</p>"), "text/html; charset=utf-8");
            }
        }

        /// <summary>Etapas del negocio indicando cuáles incluyen al estado, y si el usuario puede modificarlas</summary>
        [HttpGet]
        public JsonResult epLeerEtapasDeUnEstado(string negocio, int idEstado)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerEtapasDeUnEstado));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                r.Datos = new
                {
                    puedeModificar = Contexto.SePuedeParametrizar(),
                    etapas = NegociosDeSe.ToEnumerado(negocio).EtapasDeUnEstado(idEstado)
                };
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"Error al leer las etapas del estado {idEstado} del negocio {negocio}.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        /// <summary>
        /// Deja al estado sólo en las etapas indicadas en el parámetro 'etapas' del cuerpo.
        /// Si queda en más de una etapa no es un error: se devuelve un aviso en el mensaje del resultado.
        /// </summary>
        [HttpPost]
        public JsonResult epGrabarEtapasDeUnEstado(string negocio, int idEstado)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epGrabarEtapasDeUnEstado));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Sólo un parametrizador puede modificar las etapas de un estado");

                var body = ApiController.LeerBody(HttpContext);
                var etapas = body.parametros.LeerValor("etapas", new List<string>());
                r.Mensaje = NegociosDeSe.ToEnumerado(negocio).ActualizarEtapasDeUnEstado(Contexto, idEstado, etapas);
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, $"Error al grabar las etapas del estado {idEstado} del negocio {negocio}.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

    }

}
