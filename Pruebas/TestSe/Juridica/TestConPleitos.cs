using Inicializador.Pleitos;
using ServicioDeDatos;
using ValidacionesBase;

namespace ValidacionesDeRn
{
    class TestConPleitos
    {

        //----------------------------------------------------------------------------------------------------------------------------------
        [Prueba]
        public void DefinirFlujoReclamacionDeDeuda()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzPleitos.ModeloDePleitos(contexto);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }
    }
}
