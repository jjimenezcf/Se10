using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using System;
using GestorDeElementos;
using GestoresDeNegocio.Terceros;
using System.Collections.Generic;
using ModeloDeDto.Terceros;
using ModeloDeDto;
using Utilidades;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using GestorDeElementos.Extensores;
using System.Linq;
using ModeloDeDto.Negocio;

namespace MVCSistemaDeElementos.Controllers
{
    public class CentrosGestoresController : JerarquiaController<ContextoSe>
    {
        private GestorDeCentrosGestores gestorDeCg;

        public CentrosGestoresController(GestorDeCentrosGestores gestor, GestorDeErrores gestorDeErrores)
         : base(gestor.Contexto
               , gestor.Mapeador
               , gestorDeErrores)
        {
            gestorDeCg = gestor;
        }


        public IActionResult CrudCentrosGestores()
        {
            try
            {
                return ViewFormulario(new DescriptorDeCentrosGestores(gestorDeCg, "cg", "Centros de gestión", nameof(CentrosGestoresController), nameof(CrudCentrosGestores)));
            }
            catch (Exception e)
            {
                return RenderMensaje(GestorDeErrores.Mensaje(e));
            }
        }

        public override JsonResult epLeerJerarquia(string negocio, int idPadre, string filtrosJson)
        {
            var filtros = filtrosJson.ToDiccionario();
            return LeerJerarquia(negocio, idPadre, () => GestorDeCentrosGestores.LeerJerarquia(Contexto, idPadre, filtros));
        }

        public JsonResult epCrearNodo(string negocio, string json)
        {
           return PersistirElemento(negocio, enumTipoOperacion.Insertar.ToString(), () => GestorDeCentrosGestores.PersistirCgJson(Contexto, json, new ParametrosDeNegocio(enumTipoOperacion.Insertar)));
        }

        public JsonResult epPersistirNodo(string negocio, string json, string operacion)
        {
            return PersistirElemento(negocio, operacion.ToTipoOperacion().ToString(), () => GestorDeCentrosGestores.PersistirCgJson(Contexto, json, new ParametrosDeNegocio(operacion.ToTipoOperacion())));
        }

        protected override IEnumerable<CentroGestorDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            return ApiController.LeerElementos(gestorDeCg, posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }
        public JsonResult epLeerNodoSeleccionado(string negocio, int id, string filtrosJson)
        {
            var parametros = new Dictionary<string, object>();
            parametros.Add(ltrParametrosDto.DescargarGestionDocumental, true);
            return LeerNodoSeleccionado(negocio, id, () => gestorDeCg.LeerElementoPorId(id, parametros));
        }

        public JsonResult epLeerDireccion(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            Contexto.IniciarTraza(nameof(epLeerDireccion));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var idCg = (int)parametros.LeerValor<long>(nameof(ltrParametrosEp.id));
                var calificador = parametros.LeerValor(nameof(enumCalificadorDireccion), enumCalificadorDireccion.contacto);

                r.Datos = null;
                var cg = Contexto.SeleccionarPorId<CentroGestorDtm>(idCg, aplicarJoin: true);
                var direcciones = cg.Sociedad.Direcciones(Contexto);
                var direccion = direcciones.FirstOrDefault(d => d.Calificador == calificador);
                if (direccion is null && direcciones.Count > 0)
                {
                    direccion = direcciones[0];
                    direccion.Calificador = calificador;
                }
                r.Datos = direccion is null ? null : direccion.MapearDto(Contexto, enumNegocio.Sociedad);
                r.Consola = $"Datos de la dirección de sociedad leidos correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al leer la dirección.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }
    }
}
