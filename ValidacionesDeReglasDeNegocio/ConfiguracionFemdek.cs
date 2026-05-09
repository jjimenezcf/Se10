using ServicioDeDatos;
using NUnit.Framework;
using ValidacionesBase;
using Inicializador.Expedientes;
using Inicializador.Presupuestos;
using Inicializador.Procesos;

namespace ValidacionesDeRn
{
    class ConfiguracionFemdek
    {
        [Test]
        public void DefinirProcesosFemdek()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzTareasEvo.ModeloDeTareasEvo(contexto);
                InzTareasSpc.ModeloDeTareasSpc(contexto);

                InzPresupuestos.ModeloDePresupuestos(contexto);
                InzValoraciones.ModeloDeValoracion(contexto);

                InzProcesoSprim.ProcesoDeSprin(contexto);
            }
           // ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
           ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }
    }
}

