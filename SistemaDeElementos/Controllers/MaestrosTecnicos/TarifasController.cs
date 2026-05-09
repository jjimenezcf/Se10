using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using GestoresDeNegocio.MaestrosTecnico;
using ServicioDeDatos.MaestrosTecnico;
using ModeloDeDto.MaestrosTecnico;
using Utilidades;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Terceros;
using System;

namespace MVCSistemaDeElementos.Controllers
{
    public class TarifasController : EntidadController<ContextoSe, TarifaDtm, TarifaDto>
    {
        public TarifasController(GestorDeTarifas gestorDeTarifas, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeTarifas,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeTarifas(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearTarifa);

        private ParametrosDeNegocio AntesDeEjecutar_CrearTarifa(TarifaDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeTarifas(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);


        protected override IEnumerable<TarifaDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var parametros = new Dictionary<string, object>();
            (int idNegocio, int idElemento) restrictor = (0, 0);
            foreach (var filtro in filtros)
                if (filtro.Clausula.Equals(nameof(TarifaDto.IdProveedor), System.StringComparison.InvariantCultureIgnoreCase)) 
                {
                    restrictor.idElemento = filtro.Valor.Entero();
                    restrictor.idNegocio = NegociosDeSe.IdNegocio(enumNegocio.Proveedor);
                    parametros[ltrParametrosNeg.EnConsulta] = true;
                }
            
            if (restrictor.idElemento == 0)
                restrictor = ApiController.ObtenerNegocioYelemento(filtros);

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, NegociosDeSe.ToEnumerado(restrictor.idNegocio), restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {NegociosDeSe.ToEnumerado(restrictor.idNegocio)}");

            var gestor = GestorDeTarifas.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, parametros);
        }

        protected override TarifaDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var idNegocio = (long) parametros.LeerValor(ltrParametrosNeg.IdNegocio, (long)0);

            if (NegociosDeSe.ToEnumerado((int)idNegocio) == enumNegocio.Proveedor)
                parametros[ltrParametrosNeg.EnConsulta] = true;

            var gestor = GestorDeTarifas.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

        public JsonResult epLeerTarifa(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            Contexto.IniciarTraza(nameof(epLeerTarifa));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var idUnitario = (int)parametros.LeerValor<long>(nameof(ltrParametrosEp.idElemento));
                var idProveedor = (int)parametros.LeerValor<long>(nameof(TarifaDto.IdProveedor));
                
                r.Datos = Contexto.SeleccionarDtoPorAk<TarifaDto, TarifaDtm>(new Dictionary<string, object>
                {
                    {nameof(TarifaDto.IdElemento), idUnitario },
                    {nameof(TarifaDto.IdProveedor), idProveedor },
                }, parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, aplicarJoin: true));

                r.Consola = $"Datos de tarifa leidos correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al leer los datos de la tarifa.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

    }
}
