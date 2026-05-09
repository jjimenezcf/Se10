
using ServicioDeDatos;
using NUnit.Framework;
using ValidacionesBase;
using Inicializador.ContratosVnt;
using Inicializador.Procesos;
using SistemaDeElementos.Inicializador;
using Inicializador.Presupuestos;
using Inicializador.Ventas;
using Inicializador.Gastos;
using GestoresDeNegocio.Juridico;
using GestorDeElementos.Extensores;
using Inicializador.Expedientes;

namespace ValidacionesDeRn
{
    class ConfiguracionInteriorismo
    {
        [Test]
        public void DefinirProcesosInteriorismo()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzAcciones.DefinirAcciones(contexto);
                InzSolicitudDeContrato.ModeloDeSolicitudesDeContrato(contexto);
                InzContratosVnt.ModeloDeContratosVnt(contexto);
                ExtensorDeContratos.InicializarEtapas(contexto);
                InzPresupuestos.ModeloDePresupuestos(contexto);
                InzPartesTr.ModeloDePartesTr(contexto);
                InzFacturasEmt.ModeloDeFacturasEmt(contexto);
                InzPlanificacionesDeVenta.ModeloDePlanificacionesDeVenta(contexto);
                InzRemesasFae.ModeloDeRemesasFae(contexto);
                InzPagos.ModeloDePagos(contexto);
                InzRemesasPag.ModeloDeRemesasPag(contexto);
                InzFacturasRec.ModeloDeFacturasRec(contexto);
                TrabajosDeContratos.SometerNotificarPorcentajeDeAvisoSobrepasado(contexto);
                TrabajosDeContratos.SometerMotorDeContratos(contexto);
            }
            //ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }


        [Test]
        public void DefinirProcesosObras()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzExpeditesDeObra.ModeloDeExpedienteDeObras(contexto);
            }
            //ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

    }
}

