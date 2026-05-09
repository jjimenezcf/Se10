using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Guarderias;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto;
using ModeloDeDto.Guarderias;
using ModeloDeDto.Negocio;
using MVCSistemaDeElementos.Descriptores;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace MVCSistemaDeElementos.Controllers
{
    public class InfantesController : EntidadController<ContextoSe, InfanteDtm, InfanteDto>
    {
        public InfantesController(GestorDeInfantes gestorDeInfantes, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeInfantes,
           gestorDeErrores
         )
        {
            if (!ExtensorDeGuarderias.ModuloActivo(Contexto))
                Emitir(ltrDeGuarderias.ModuloNoActivo, enumCodigoDeError.ModuloNoActivo);
        }

        public IActionResult CrudInfantes()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeInfantes).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Guarderias}/{nameof(CrudInfantes)}";
                    return base.View(destino, new DescriptorDeInfantes(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<InfanteDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeInfantes(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Infantes_AsociarCurso:
                    GestorDeInfantes.PuedenCambiarseDeCurso(Contexto, (List<int>)parametros[ltrParametrosEp.ids]);
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        public JsonResult epAsociarCurso(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idInfante = (int)parametros.LeerValor<long>(nameof(AsociarCursoDto.IdInfante));
                var idCurso = (int)parametros.LeerValor<long>(nameof(AsociarCursoDto.IdElemento));

                var curso = GestorDeInfantes.AsociarCurso(Contexto, idInfante, idCurso);

                r.Consola = $"curso asociado correctamente.";
                r.Datos = curso;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al asociar el curso.");
            }
            return new JsonResult(r);
        }

        public JsonResult epLeerDireccionesDeFamiliares(string filtrosJson, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerDireccionesDeFamiliares));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();
                parametros[ltrParametrosNeg.EsUnaPeticion] = true;
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epLeerElementos;
                List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosJson);

                List<DireccionDto> direcciones = ((GestorDeInfantes)_GestorDeElementos).LeerDireccionesDeFamiliares(filtros.First(x => x.Clausula.ToLower() == nameof(DireccionDtm.IdElemento).ToLower()).Valor.Entero());
                r.Datos = direcciones;
                r.Total = direcciones.Count();
                r.Consola = $"se han leido {r.Datos.Count} direcciones";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                if (e.Message.StartsWith(ApiDeDetalles.MensajeDeNoUsaDetalle))
                {
                    r.ModoDeAcceso = enumModoDeAccesoDeDatos.SinPermiso.Render();
                    r.Mensaje = e.Message;
                    r.Datos = null;
                }
                else
                {
                    ApiController.PrepararError(e, r, "Error al leer las direcciones de familiares.");
                }
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        public JsonResult epLeerInfantesTutelados(string filtrosJson, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerInfantesTutelados));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();
                parametros[ltrParametrosNeg.EsUnaPeticion] = true;
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epLeerElementos;
                List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosJson);

                List<InfanteDto> infantes = ((GestorDeInfantes)_GestorDeElementos).LeerInfantesTutelados(
                    filtros.First(x => x.Clausula.ToLower() == nameof(DireccionDto.IdNegocio).ToLower()).Valor.Entero(),
                    filtros.First(x => x.Clausula.ToLower() == nameof(IDetalleDto.IdElemento).ToLower()).Valor.Entero());
                r.Datos = infantes;
                r.Total = infantes.Count();
                r.Consola = $"se han leido {r.Datos.Count} direcciones";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                if (e.Message.StartsWith(ApiDeDetalles.MensajeDeNoUsaDetalle))
                {
                    r.ModoDeAcceso = enumModoDeAccesoDeDatos.SinPermiso.Render();
                    r.Mensaje = e.Message;
                    r.Datos = null;
                }
                else
                {
                    ApiController.PrepararError(e, r, "Error al leer las direcciones de familiares.");
                }
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }


    }
}
