using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Terceros;
using GestoresDeNegocio.TrabajosSometidos;
using MVCSistemaDeElementos.Controllers;
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
        [Prueba]
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
        [Prueba]
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
