using System;
using AutoMapper;
using Utilidades;
using Gestor.Errores;
using GestoresDeNegocio.Entorno;
using Microsoft.AspNetCore.Mvc;
using ServicioDeDatos;
using MVCSistemaDeElementos.Descriptores;
using ModeloDeDto.TrabajosSometidos;
using Microsoft.AspNetCore.Authorization;
using SistemaDeElementos.Inicializador;
using SistemaDeElementos.Inicializador.Acromur;
using SistemaDeElementos.Inicializador.Mra;
using SistemaDeElementos.Inicializador.AytoBeniel;
using SistemaDeElementos.Inicializador.Datos;
using Inicializador.Pleitos;
using Inicializador.Expedientes;
using Inicializador.Procesos;
using Inicializador.Presupuestos;
using Inicializador.ContratosVnt;
using GestoresDeNegocio.Juridico;
using Inicializador.Ventas;
using Inicializador.SistemaDocumental;
using Inicializador.Gastos;
using Inicializador.ContratosCmp;
using SistemaDeElementos.Controllers.Entorno;
using Inicializador.MatriculasDeGuarderia;
using GestorDeElementos.Extensores;
using Inicializador.Procedimientos;

namespace MVCSistemaDeElementos.Controllers
{
    public class InicializadorController : BaseController<TrabajoDeUsuarioDto>
    {
        private IServiceProvider ServicioDeId { get; }
        public InicializadorController(IServiceProvider services, ContextoSe contexto, IMapper mapeador, GestorDeErrores gestorDeErrores) :
        base(gestorDeErrores, contexto, mapeador)
        {
            ServicioDeId = services;
        }

        [Authorize]
        public IActionResult AplicarSeguridad()
        {
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.DatosDeConexion.EsAdministrador)
                    GestorDeErrores.Emitir("Esta opción sólo se permite a administradores");

                TrabajosDeEntorno.SometerGenerarSeguridad(Contexto, DateTime.Now.AddSeconds(1));
                Contexto.Commit(tran);
                ViewBag.Mensaje = "Trabajo de reseteo de seguridad sometido correctamente";
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                return RenderMensaje($"No se ha podido someter el trabajo de reseteo de seguridad.{Environment.NewLine}{GestorDeErrores.Detalle(e)}");
            }

            try
            {
                var descriptor = new DescriptorDeTrabajosDeUsuario(Contexto, ModoDescriptor.Mantenimiento);
                var destino = ApiController.PrepararDescriptor<TrabajoDeUsuarioDto>(this, ControllerContext, descriptor, Contexto, HttpContext);
                ViewBag.DatosDeConexion = DatosDeConexion;
                return base.View(destino, descriptor);
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }

        [Authorize]
        public IActionResult InicializarBD_1()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.DatosDeConexion.EsAdministrador)
                    GestorDeErrores.Emitir("Esta opción sólo se permite a administradores");

                if (CacheDeVariable.Cfg_CrearRegistrosDeEntorno)
                {
                    InicializarRegistroDeEntorno(Contexto);
                    //InicializarSeed(Contexto);
                    CacheDeVariable.Modificar(Variable.CFG_Crear_Registros_De_Entorno, "N");
                    ViewBag.Mensaje = "Inicialización realizada";
                }
                else
                    ViewBag.Mensaje = $"La variable {Variable.CFG_Crear_Registros_De_Entorno} está a N, póngala en S";

                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                return RenderMensaje($"No se ha podido inicializar la BD.{Environment.NewLine}{GestorDeErrores.Detalle(e)}");
            }
            return VistaDelPanelDeControl(Contexto);
        }

        [Authorize]
        public IActionResult InicializarBD()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch 
            {
                return RenderMensaje($"No se ha podido inicializar navegar a los Gráficos de un proceso");
            }
            return VistaGraficoDeUnProceso(Contexto);
        }

        public static void InicializarSeed(ContextoSe contexto)
        {
            InzMaestros.CrearCallejo(contexto);
            InzMaestros.CrearTerceros(contexto);
            InzMaestros.CrearJuzgados(contexto);
            InzMaestros.InicializarUnidadesDeMedida(contexto);
            InzMaestros.CrearNaturalezas(contexto);
            InzMaestros.InicializarCuentasContables(contexto);
            InzMaestros.CrearIvasRepercutidos(contexto);
            InzMaestros.CrearIvasSoportados(contexto);

            InzArchivadoresEcoFin.ArchivadoresEconomicoFinanciero(contexto);
            InzRegistroEs.ModeloDeRegistroEs(contexto);
            InzTareasRre.ModeloDeTareasTrr(contexto);
            InzTareasPlf.ModeloDeTareasPlf(contexto);
            InicializadorController.InzProcesosDeExpedientes(contexto);
            InzPleitos.ModeloDePleitos(contexto);
            InzPresupuestos.ModeloDePresupuestos(contexto);

            InzArchivosJuridicos.ArchivadoresJuridicos(contexto);
            InzContratosVnt.ModeloDeContratosVnt(contexto);
            InzContratosCmp.ModeloDeContratosCmp(contexto);
            InzMatriculasDeGuarderia.ModeloDeMatriculasDeGuarderia(contexto);

            InzPartesTr.ModeloDePartesTr(contexto);
            InzFacturasEmt.ModeloDeFacturasEmt(contexto);
            InzPlanificacionesDeVenta.ModeloDePlanificacionesDeVenta(contexto);

            InzRemesasFae.ModeloDeRemesasFae(contexto);

            InzPagos.ModeloDePagos(contexto);
            InzRemesasPag.ModeloDeRemesasPag(contexto);
            InzFacturasRec.ModeloDeFacturasRec(contexto);

            InicializadorController.ModeloAcromur(contexto);
            InicializadorController.ModeloBeniel(contexto);
            InicializadorController.ModeloMra(contexto);

            InzRegistroEs.AccionesAlTransitar(contexto);
            InzTareasRre.AccionesAlTransitar(contexto);

            TrabajosDeContratos.SometerNotificarPorcentajeDeAvisoSobrepasado(contexto);
            TrabajosDeContratos.SometerMotorDeContratos(contexto);
        }

        [Authorize]
        public IActionResult InicializarMaestros()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.DatosDeConexion.EsAdministrador)
                    GestorDeErrores.Emitir("Esta opción sólo se permite a administradores");
                DefinirMaestros(Contexto);
                enumVariablesDelSistema.Cfg_Usar_Cache_Descriptores_Json.CrearSiNoExiste("S");
                CacheDeVariable.Modificar(Variable.CFG_Debugar_Sqls, "N");
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

        public static void InicializarRegistroDeEntorno(ContextoSe contexto)
        {
            var t = contexto.IniciarTransaccion();
            try
            {
                ServicioDeCaches.EliminarTodas();
                ArbolDeMenuController.InicializarEntornoInternal(contexto);
                InzAcciones.DefinirAcciones(contexto);
                InzNegocios.DefinirAccionesDeRelacion(contexto);
                DefinirMaestros(contexto);
                contexto.Commit(t);
            }
            catch(Exception e)
            {
                contexto.Rollback(t);
                contexto.RegistrarConEnvio(asunto: $"Error al {nameof(InicializarRegistroDeEntorno)}", error: GestorDeErrores.Detalle(e));
                throw;
            }
            finally
            {
                ServicioDeCaches.EliminarTodas();
            }
        }

        private static void DefinirMaestros(ContextoSe contexto)
        {
            InzEntorno.InicializarVariables(contexto); 
            InzEntorno.InicializarParametrosDeNegocio(contexto);
            InzMaestros.InicializarUnidadesDeMedida(contexto);
            InzMaestros.InicializarCuentasContables(contexto);

            //InzArchivadoresEcoFin.ArchivadoresEconomicoFinanciero(contexto);

            InzSeguridadComun.SeguridadFuncional(contexto);
            InzSeguridadComun.SeguridadDatosComunes(contexto);

            InzMaestros.CrearIvasRepercutidos(contexto);
            InzMaestros.CrearIvasSoportados(contexto);

            //InicializadorDeEntorno.InicializarTiposDeContrato(contexto);
            //InzAcciones.DefinirExportaciones(contexto);
        }

        public static void InzProcesosDeExpedientes(ContextoSe contexto)
        {
            InzProcesosJuridicos.ModeloDeJuridica(contexto);
            InzExpeditesDeObra.ModeloDeExpedienteDeObras(contexto);
            InzSolicitudDeContrato.ModeloDeSolicitudesDeContrato(contexto);
            InzExpeditesDeTalleres.ModeloDeExpedienteDeTalleres(contexto);
        }

        public static void InzProcedimientosJudiciales(ContextoSe contexto)
        {
            InzProcedimientoCivil.ModeloCivil(contexto);
        }

        public static void ModeloAcromur(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                InzAcromur.Sociedad(contexto);
                InzAcromur.PuestosDeTrabajo(contexto);
                InzAcromur.Usuarios(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public static void ModeloMra(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                InzMra.PuestosDeTrabajo(contexto);
                InzMra.Usuarios(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public static void ModeloBeniel(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                InzSeguridadBeniel.CrearRolesDeDatos(contexto);
                InzSeguridadBeniel.CrearPuestosDeTrabajo(contexto);
                InzUsuariosBeniel.CrearUsuarios(contexto);

                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }


    }
}
