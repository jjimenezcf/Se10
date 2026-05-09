
using ServicioDeDatos;
using NUnit.Framework;
using ValidacionesBase;
using Inicializador.Expedientes;

namespace ValidacionesDeRn
{
    class ConfiguracionAcromur
    {
        [Test]
        public void DefinirProcesosAcromur()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzExpedientesDeFacturacion.ProcesoFCT(contexto);
                InzExpedientesDeIrpf.ProcesoIrpf(contexto);
                InzProcesoCGI.ProcesoCGI(contexto);
                InzProcesoPIM.ProcesoPIM(contexto);
            }
            //ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

    }
}

