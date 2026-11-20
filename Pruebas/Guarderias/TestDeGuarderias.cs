using Inicializador.MatriculasDeGuarderia;
using NUnit.Framework;
using ServicioDeDatos;
using ValidacionesBase;

namespace ValidacionesDeRn
{
    public class TestDeGuarderias
    {
        //ContextoSe _contexto;
        //[SetUp]
        //public void Setup()
        //{
        //    _contexto = Inicializaciones.CrearContexto();
        //}

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void InicializarFlujoMTG()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzMatriculasDeGuarderia.ModeloDeMatriculasDeGuarderia(contexto);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

    }
}
