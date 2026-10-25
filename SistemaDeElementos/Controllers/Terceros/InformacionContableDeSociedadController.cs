using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Contabilidad;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    // Ampliación de la sociedad con su información contable: no tiene tabla, se calcula al leerla y siempre es de consulta
    public class InformacionContableDeSociedadController : BaseController<InformacionContableDeSociedadDto>
    {
        public InformacionContableDeSociedadController(ContextoSe contexto, IMapper mapeador, GestorDeErrores gestorDeErrores)
        : base(gestorDeErrores, contexto, mapeador)
        {
        }

        // Solo la ven los gestores de una sociedad gestionada; para el resto se devuelve sin permiso y el cliente oculta el span
        public JsonResult epLeerAmpliacionPorIdNegocio(int idNegocio, int idElemento, string parametrosJson)
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var sociedad = Contexto.SeleccionarPorId<SociedadDtm>(idElemento);
                var modoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Sociedad, idElemento);
                if (!modoDeAcceso.SoyGestor() || !sociedad.EsGestionada(Contexto))
                {
                    r.Datos = null;
                    r.ModoDeAcceso = enumModoDeAccesoDeDatos.SinPermiso.Render();
                }
                else
                {
                    var (sistema, digitos) = sociedad.SistemaYDigitosParaContabilizar();
                    r.Datos = new InformacionContableDeSociedadDto
                    {
                        IdElemento = idElemento,
                        UltimoCierre = GestorDeCierresContables.UltimoCierre(Contexto, idElemento)?.CerradaEl.Date,
                        SistemaContable = sistema is null ? "Sin configurar" : ((enumSistemaContable)sistema).Descripcion(),
                        Digitos = digitos,
                        ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor
                    };
                    r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                }

                r.Estado = enumEstadoPeticion.Ok;
                r.Consola = "información contable de la sociedad leida";
                r.Mensaje = "";
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se ha podido leer la información contable de la sociedad.");
            }

            return new JsonResult(r);
        }
    }
}
