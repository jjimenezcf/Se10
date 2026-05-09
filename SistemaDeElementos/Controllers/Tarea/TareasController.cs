using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Tarea;
using Inicializador.Procesos;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Tarea;
using MVCSistemaDeElementos.Descriptores;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Tarea;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class TareasController : EntidadController<ContextoSe, TareaDtm, TareaDto>
    {
        public TareasController(GestorDeTareas gestorDeTareas, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeTareas,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudTareas()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeTareas).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Administracion}/{nameof(CrudTareas)}";
                    return base.View(destino, new DescriptorDeTareas(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<TareaDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeTareas(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }


        public IActionResult MaestrosDeTareas()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzTareasRre.ModeloDeTareasTrr(Contexto);
                InzTareasPlf.ModeloDeTareasPlf(Contexto);
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
                case eventosDeMf.Tar_IrAPartesTr:
                    return null;
                case eventosDeMf.Totalizador_Mostrar:
                    return null;
                case eventosDeMf.Tar_CopiarTarea:
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        protected override IEnumerable<TareaDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> parametros)
        {
            return base.LeerElementos(posicion, cantidad, filtros, orden, parametros);
        }

        public JsonResult epExcluirDeLaFactura(int id, string parametrosJson)
        {
            var parametros = Utilidades.extJson.ToDiccionarioDeParametros(parametrosJson);
            var r = new Resultado();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                GestorDeTareas.ExluirDeLaFactura(Contexto, id);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Petición realizada";
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "No se ha podido excluir la tarea de la factura.");
            }

            return new JsonResult(r);
        }

        public JsonResult epCopiarUnaTarea()
        {
            var body = ApiController.LeerBody(HttpContext);
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                r.Datos = GestorDeTareas.CopiarTarea(Contexto, body.parametros);
                r.Mensaje = $"tarea copiada";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al copiar la tarea.");
            }
            return new JsonResult(r);
        }



        [HttpPost]
        public async Task<JsonResult> epTotales(int posicion, int cantidad)
        {
            var body = ApiController.LeerBody(HttpContext);
            var filtro = body.parametros.LeerValor<string>(ltrParametrosEp.Filtro);
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epTotales));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                List<ClausulaDeFiltrado> filtros = filtro == null ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtro);
                Contexto.AnotarParametros(new List<object> { filtro, posicion, cantidad });
                var totales = await ((ITotalizador<TotalesDeTareas>)_GestorDeElementos).ObtenerTotalesAsync(filtros, posicion, cantidad);
                r.Consola = $"Totalización realizada";
                r.Datos = totales;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al totalizar las tareas.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

    }
}
