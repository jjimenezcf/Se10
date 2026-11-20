using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Terceros;
using GestoresDeNegocio.TrabajosSometidos;
using MVCSistemaDeElementos.Controllers;
using NUnit.Framework;
using ServicioDeDatos;
using SistemaDeElementos.Inicializador;
using SistemaDeElementos.Inicializador.Datos;
using ValidacionesBase;

namespace ValidacionesDeRn
{
    public class TestInicializarBd
    {
        //ContextoSe _contexto;
        //[SetUp]
        //public void Setup()
        //{
        //    _contexto = Inicializaciones.CrearContexto();
        //}

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void InicializarBd()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InicializadorController.InicializarSeed(contexto);
                TrabajosDeEntorno.SometerGenerarSeguridad(contexto).EjecutarTrabajo(contexto);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void InicializarMaestros()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzMenus.DefinirMenus(contexto);
                InzEntorno.InicializarVariables(contexto);
                InzMaestros.InicializarCuentasContables(contexto);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }
    }
}
