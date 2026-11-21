using System;
using ServicioDeDatos;
using Utilidades;
using ValidacionesBase;

namespace TestLogistica
{
    // Ejecución de las pruebas de logística con el entorno común (Inicializaciones): la aplicación web sin arrancarla
    public static class Entorno
    {
        // Contexto del administrador en modo test, en un ámbito nuevo: un Commit de los gestores no confirma nada (ver ExtensorDeContexto.Commit)
        public static ContextoSe CrearContexto()
        {
            Inicializaciones.NuevoAmbito();
            return Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
        }

        // Ejecuta la prueba en una transacción que siempre se deshace: la BD queda como estaba
        public static void EjecutarConRollback(string nombre, Action<ContextoSe, Informe> prueba)
        {
            var informe = new Informe(nombre);
            var contexto = CrearContexto();
            var transaccion = contexto.IniciarTransaccion();
            try
            {
                prueba(contexto, informe);
            }
            catch (Exception e)
            {
                informe.Excepcion(e);
            }
            finally
            {
                contexto.Rollback(transaccion);
                ServicioDeCaches.EliminarTodas();
                informe.Resumen();
                Console.WriteLine("Rollback realizado: la BD queda como estaba.");
            }
        }
    }
}
