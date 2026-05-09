using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using GestoresDeNegocio.Ventas;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;
using Utilidades;
using MVCSistemaDeElementos.Descriptores;
using System;
using System.Text;

namespace MVCSistemaDeElementos.Controllers
{
    public class AsignacionesDePtrController : EntidadController<ContextoSe, AsignacionDePtrDtm, AsignacionDePtrDto>
    {
        public AsignacionesDePtrController(GestorDeAsignacionesDePtr gestorDeAsignacionesDePtr, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeAsignacionesDePtr,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeAsignacionesDePtr(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearAsignacionDePtr);

        private ParametrosDeNegocio AntesDeEjecutar_CrearAsignacionDePtr(AsignacionDePtrDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeAsignacionesDePtr(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);


        public JsonResult epAplicarDatosDeEjecucion(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(FechasDeEjecucionDto.Iniciada))) throw new Exception("Debe indicar la fecha de inicio a aplicar a las asignaciones seleccionadas");
                if (!parametros.ContieneClave(ltrParametrosEp.ids)) throw new Exception("Debe indicar las asignaciones a acctualizar ");
                var ids = parametros.LeerValor<List<int>>(ltrParametrosEp.ids);

                r.Consola = GestorDeAsignacionesDePtr.AplicarDatosDeEjecucion(Contexto, ids, parametros).ToString();
                r.Mensaje = $"Datos de ejecución aplicados correctamente, más detalles en la consola";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se han podido aplicar la ejecución a los partes indicados.");
            }
            return new JsonResult(r);
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            StringBuilder mensajeDeConsola = null;
            switch (opcion)
            {
                case eventosDeMf.Ptr_DarPorRealizadasHoy:
                    mensajeDeConsola = GestorDeAsignacionesDePtr.DarPorRealizadasHoy(Contexto, (List<int>)parametros[ltrParametrosEp.ids]);
                    break;
                case eventosDeMf.Ptr_DarPorRealizadasSegunPlan:
                    mensajeDeConsola = GestorDeAsignacionesDePtr.DarPorRealizadasSegunPlan(Contexto, (List<int>)parametros[ltrParametrosEp.ids]);
                    break;
                case eventosDeMf.Ptr_SolicitarFechaDeEjecucion:
                    return null;
            }

            if (mensajeDeConsola != null)
                return new Resultado
                {
                    Consola = mensajeDeConsola.ToString(),
                    Datos = null,
                    Mensaje = $"Opción {opcion} procesada correctamente, más detalle en la consola.",
                    Estado = enumEstadoPeticion.Ok
                };
            
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        protected override IEnumerable<AsignacionDePtrDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> parametros)
        {
            //            var parametros = new Dictionary<string, object>();
            (int idNegocio, int idElemento) restrictor = (0, 0);
            foreach (var filtro in filtros)
                if (filtro.Clausula.Equals(nameof(AsignacionDePtrDto.IdTrabajador), System.StringComparison.InvariantCultureIgnoreCase))
                {
                    restrictor.idElemento = filtro.Valor.Entero();
                    restrictor.idNegocio = NegociosDeSe.IdNegocio(enumNegocio.Trabajador);
                    parametros[ltrParametrosNeg.EnConsulta] = true;
                }

            if (restrictor.idElemento == 0)
                restrictor = ApiController.ObtenerNegocioYelemento(filtros);

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, NegociosDeSe.ToEnumerado(restrictor.idNegocio), restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {NegociosDeSe.ToEnumerado(restrictor.idNegocio)}");

            var gestor = GestorDeAsignacionesDePtr.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, parametros);
        }

        protected override AsignacionDePtrDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var idNegocio = (long)parametros.LeerValor(ltrParametrosNeg.IdNegocio, (long)0);

            if (NegociosDeSe.ToEnumerado((int)idNegocio) == enumNegocio.Trabajador)
                parametros[ltrParametrosNeg.EnConsulta] = true;

            var gestor = GestorDeAsignacionesDePtr.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

        public IActionResult CrudAsignacionesPtr()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            return ViewCrud(new DescriptorDeAsignacionesPtr(Contexto, ModoDescriptor.Mantenimiento));
        }

    }
}
