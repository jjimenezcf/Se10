using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Contabilidad;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Terceros;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    // Ampliación de la sociedad con su información contable: no tiene tabla, se lee y se graba sobre el código fiscal de la sociedad y los
    // parámetros de preasientos que se definen por sociedad
    public class InformacionContableDeSociedadController : BaseController<InformacionContableDeSociedadDto>
    {
        // negocios que generan preasientos y la propiedad del Dto que indica si la sociedad los genera
        private static readonly Dictionary<enumNegocio, Func<InformacionContableDeSociedadDto, bool>> GenerarPreasientos = new()
        {
            { enumNegocio.FacturaEmitida, dto => dto.GenerarEmitidas },
            { enumNegocio.FacturaRecibida, dto => dto.GenerarRecibidas },
            { enumNegocio.Cobro, dto => dto.GenerarCobros },
            { enumNegocio.Pago, dto => dto.GenerarPagos },
            { enumNegocio.Almacen, dto => dto.GenerarAlmacen }
        };

        public InformacionContableDeSociedadController(ContextoSe contexto, IMapper mapeador, GestorDeErrores gestorDeErrores)
        : base(gestorDeErrores, contexto, mapeador)
        {
        }

        // Sin permiso (el cliente oculta el span) si no es gestor de una sociedad gestionada; editable si además puede parametrizar; si no, consulta
        private enumModoDeAccesoDeDatos ModoDeAccesoALaInformacionContable(SociedadDtm sociedad)
        {
            var modoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Sociedad, sociedad.Id);
            if (!modoDeAcceso.SoyGestor() || !sociedad.EsGestionada(Contexto))
                return enumModoDeAccesoDeDatos.SinPermiso;

            return Contexto.SePuedeParametrizar() ? enumModoDeAccesoDeDatos.Gestor : enumModoDeAccesoDeDatos.Consultor;
        }

        public JsonResult epLeerAmpliacionPorIdNegocio(int idNegocio, int idElemento, string parametrosJson)
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var sociedad = Contexto.SeleccionarPorId<SociedadDtm>(idElemento);
                var modoDeAcceso = ModoDeAccesoALaInformacionContable(sociedad);
                if (modoDeAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                {
                    r.Datos = null;
                    r.ModoDeAcceso = enumModoDeAccesoDeDatos.SinPermiso.Render();
                }
                else
                {
                    var (sistema, digitos) = sociedad.SistemaYDigitosParaContabilizar();
                    var idCgDocumental = sociedad.IdCgDocumentalDe(Contexto);
                    r.Datos = new InformacionContableDeSociedadDto
                    {
                        IdElemento = idElemento,
                        UltimoCierre = GestorDeCierresContables.UltimoCierre(Contexto, idElemento)?.CerradaEl.Date,
                        Contabilidad = sistema?.ToString(),
                        Digitos = digitos,
                        CodigoFiscal = sociedad.CodigoFiscal,
                        PlanContable = sociedad.IdPlanContableDe(),
                        IdCgDocumental = idCgDocumental,
                        CgDocumental = idCgDocumental is null ? null : Contexto.SeleccionarPorId<CentroGestorDtm>((int)idCgDocumental, errorSiNoHay: false)?.Expresion,
                        GenerarEmitidas = sociedad.GeneraPreasientos(enumNegocio.FacturaEmitida) ?? false,
                        GenerarRecibidas = sociedad.GeneraPreasientos(enumNegocio.FacturaRecibida) ?? false,
                        GenerarCobros = sociedad.GeneraPreasientos(enumNegocio.Cobro) ?? false,
                        GenerarPagos = sociedad.GeneraPreasientos(enumNegocio.Pago) ?? false,
                        GenerarAlmacen = sociedad.GeneraPreasientos(enumNegocio.Almacen) ?? false,
                        ModoDeAcceso = modoDeAcceso
                    };
                    r.ModoDeAcceso = modoDeAcceso.Render();
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

        // Al modificar la sociedad, graba su información contable: código fiscal en la sociedad y el resto en los parámetros de preasientos
        [HttpPost]
        public JsonResult epPersistirAmpliacionPorPost(int idNegocio)
        {
            var r = new Resultado();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var body = ApiController.LeerBody(HttpContext);
                var dto = JsonConvert.DeserializeObject<InformacionContableDeSociedadDto>(body.parametros[ltrParametrosEp.ElementoJson].ToString());

                var sociedad = Contexto.SeleccionarPorId<SociedadDtm>(dto.IdElemento);
                if (ModoDeAccesoALaInformacionContable(sociedad) != enumModoDeAccesoDeDatos.Gestor)
                    GestorDeErrores.Emitir($"No puede modificar la información contable de la sociedad '{sociedad.Expresion}': ha de poder parametrizar, ser gestor de la sociedad y que esta sea gestionada");

                PersistirInformacionContable(sociedad, dto);

                Contexto.Commit(tran);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "información contable actualizada";
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "No se ha podido grabar la información contable de la sociedad.");
            }

            return new JsonResult(r);
        }

        private void PersistirInformacionContable(SociedadDtm sociedad, InformacionContableDeSociedadDto dto)
        {
            foreach (var generar in GenerarPreasientos)
                ResetearParametroDeLaSociedad(VariablesDePreasiento.ParametroDeGenerarPreasiento(generar.Key), sociedad,
                    new Dictionary<string, object> { { "Valor", generar.Value(dto) ? "S" : "N" } });

            if (!dto.PlanContable.IsNullOrEmpty())
                ResetearParametroDeLaSociedad(enumParametrosDePreasiento.SPR_Plan_Contable, sociedad,
                    new Dictionary<string, object> { { "IdPlanContable", dto.PlanContable.Trim() } });

            if (dto.IdCgDocumental > 0)
            {
                // antes de almacenar el json se valida que el cg sea de la sociedad
                var cg = sociedad.ValidarCgDocumental(Contexto, (int)dto.IdCgDocumental);
                ResetearParametroDeLaSociedad(enumParametrosDePreasiento.SPR_Sociedad_CG_Documental, sociedad,
                    new Dictionary<string, object> { { "IdCgDocumental", cg.Id } });
            }

            // "-1" es la opción "Seleccionar..." de la lista: no se cambia la contabilidad
            if (Enum.TryParse<enumSistemaContable>(dto.Contabilidad, out var sistema) && Enum.IsDefined(sistema))
            {
                var digitos = sistema == enumSistemaContable.NCS
                    ? VariablesDePreasiento.DigitosDeNcs
                    : dto.Digitos > 0 ? dto.Digitos : VariablesDePreasiento.DigitosPorDefectoDeContasol;
                GestorDeCierresContables.CambiarSistemaContable(Contexto, sociedad, sistema, digitos);
            }

            var codigoFiscal = dto.CodigoFiscal?.Trim();
            if (codigoFiscal != sociedad.CodigoFiscal)
            {
                sociedad.CodigoFiscal = codigoFiscal;
                sociedad.ModificarComoAdministrador(Contexto);
            }
        }

        private void ResetearParametroDeLaSociedad(enumParametrosDePreasiento parametro, SociedadDtm sociedad, Dictionary<string, object> campos)
        {
            var json = VariablesDePreasiento.JsonDelParametroConLaSociedad(parametro, sociedad.Id, campos);
            if (json is not null)
                enumNegocio.Preasiento.ResetearParametro(Contexto, parametro, json);
        }
    }
}
