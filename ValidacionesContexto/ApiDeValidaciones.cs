using System;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.SistemaDocumental;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.SistemaDocumental;

namespace ValidacionesBase
{
    public static class ApiDeValidaciones
    {
        public static void EjecutarConRollback(this ContextoSe contexto, Action prueba)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                contexto.IniciarTraza(prueba.Method.Name.Substring(1, prueba.Method.GetBaseDefinition().Name.IndexOf('>') - 1));
                prueba();
                contexto.Rollback(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                var stackTrace = e.StackTrace;
                while (e.Message.Contains("See the inner exception for details"))
                {
                    e = e.InnerException;
                }
                Assert.Fail(e.Message + Environment.NewLine + stackTrace);
            }
            finally
            {
                contexto.CerrarTraza();
            }
            Assert.IsTrue(true);
        }

        public static void EjecutarConCommit(this ContextoSe contexto, Action prueba)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                contexto.Test = false;
                contexto.IniciarTraza(prueba.Method.Name.Substring(1, prueba.Method.GetBaseDefinition().Name.IndexOf('>') - 1));
                prueba();
                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                var stackTrace = e.StackTrace;
                while (e.Message.Contains("See the inner exception for details"))
                {
                    e = e.InnerException;
                }
                Assert.Fail(e.Message + Environment.NewLine + stackTrace);
            }
            finally
            {
                contexto.CerrarTraza();
                contexto.Test = true;   
            }
            Assert.Pass();
        }

        public static ArchivoDtm AnexarNuevoFichero(this ArchivadorDtm archivador, ContextoSe contexto, string fichero)
        =>
        archivador.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo(fichero));

        public static string CrearFicheroWindowsParaAnexar(string ruta, string nombre)
        => 
        ServidorDocumental.NuevoArchivo(nombre, ruta);


        public static void IntentarEjecutar(System.Action accion, string mensaje)
        {
            try
            {
                accion();
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains(mensaje))
                {
                    if (ex.InnerException is not null && ex.InnerException.Message.Contains(mensaje))
                        return;
                    throw;
                }
            }
        }
    }
}
