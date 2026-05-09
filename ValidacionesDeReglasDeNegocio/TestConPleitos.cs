using Inicializador.Pleitos;
using NUnit.Framework;
using ServicioDeDatos;
using ValidacionesBase;

namespace ValidacionesDeRn
{
    class TestConPleitos
    {

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
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
