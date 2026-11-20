
using ServicioDeDatos;
using NUnit.Framework;
using ValidacionesBase;
using Inicializador.Procesos;
using Inicializador.Presupuestos;
using Inicializador.Expedientes;
using SistemaDeElementos.Inicializador;

namespace ValidacionesDeRn
{
    class ConfiguracionTest
    {
        [Test]
        public void DefinirProcesosTest()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                //InzTareasEvo.ModeloDeTareasEvo(contexto);
                //InzTareasSpc.ModeloDeTareasSpc(contexto);
                //InzTareasPlf.ModeloDeTareasPlf(contexto);
                //InzTareasRre.ModeloDeTareasTrr(contexto);

                //InzPresupuestos.ModeloDePresupuestos(contexto);
                //InzValoraciones.ModeloDeValoracion(contexto);

                //InzExpedientesDeFacturacion.ProcesoFCT(contexto);
                //InzExpedientesDeIrpf.ProcesoIrpf(contexto);
                //InzProcesoCGI.ProcesoCGI(contexto);
                //InzProcesoPIM.ProcesoPIM(contexto);
                //InzProcesoSprim.ProcesoDeSprin(contexto);
                InzAcciones.DefinirExportaciones(contexto);
            }
            //ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

    }
}

