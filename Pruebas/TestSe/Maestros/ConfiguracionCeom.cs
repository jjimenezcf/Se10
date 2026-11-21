
using ServicioDeDatos;
using ValidacionesBase;
using Inicializador.Expedientes;
using Inicializador.SistemaDocumental;

namespace ValidacionesDeRn
{
    class ConfiguracionCeom
    {

        [Prueba]
        public void EjecutarConfiguracionCeom()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzExpeditesDeActividades.ModeloDeActividades(contexto);
                InzActividadesFormativa.ModeloDeActividadesFormativa(contexto);
            }
            ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            //ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

    }
}

