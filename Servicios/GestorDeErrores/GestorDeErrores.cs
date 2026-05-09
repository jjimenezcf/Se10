using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace Gestor.Errores
{
    public class GestorDeErrores
    {
        public enum Datos { Mostrar, Consola, CodigoError, EmitidoPorMi }

        public enum enumCodigoDeError
        {
            Nulo = 0,
            MensajeInformativo = 1,
            ElementoYaRenderizado = 99,
            ModuloNoActivo = 900
        }

        public GestorDeErrores()
        {
        }

        public static string Detalle(Exception e)
        {
            var retorno = Mensaje(e);

            var s = e.StackTrace;
            retorno = retorno + Environment.NewLine + s;
            return retorno;
        }

        public static string Mensaje(Exception e)
        {
            var retorno = "";
            while (e != null)
            {
                if (!e.Message.Contains("Exception has been thrown by the target of an invocation"))
                {
                    if (!e.Message.Contains("See the inner exception for details"))
                    {
                        retorno += e.Message + (e.InnerException != null ? Environment.NewLine : "");
                    }
                }
                e = e.InnerException;
            }

            return retorno;
        }

        public static void Emitir(string error, string consola, Exception e = null)
        {
            Emitir(error, enumCodigoDeError.Nulo, consola, e);
        }
        public static void Emitir(string error, Exception e = null)
        {
            Emitir(error, enumCodigoDeError.Nulo, null, e);
        }


        public static void Emitir(string error, enumCodigoDeError codigoError)
        {
            Emitir(error, codigoError, consola: null, e: null);
        }

        public static void Emitir(string error, enumCodigoDeError codigoError, string consola, Exception e = null)
        {
            if (e != null)
                RegistrarExcepcion(error, e);

            var exc = new Exception(error, e);
            exc.Data[Datos.Mostrar] = true;
            exc.Data[Datos.EmitidoPorMi] = true;
            if (!consola.IsNullOrEmpty()) exc.Data[Datos.Consola] = consola;
            if (codigoError != enumCodigoDeError.Nulo) exc.Data[Datos.CodigoError] = codigoError;

            throw exc;
        }

        private static void RegistrarExcepcion(string error, Exception e)
        {
            /* registrar en el logger de excepciones 
             
            - Fecha Hora
            - Usuario
            - error
            - Excepción            
             
             */

        }

        public static void EnviarExcepcionPorCorreo(string servidor, string asunto, Exception e)
        {
            var mensajeDeError = Detalle(e);
            //ServicioDeCorreos.ServicioDeCorreo.EnviarCorreoPara(servidor, new System.Collections.Generic.List<string> { "jjimenezcf@gmail.com" }, $"{asunto} en {e.TargetSite.DeclaringType.Name}.{e.TargetSite.Name}", mensajeDeError);
        }

    }


    public static class Excepciones
    {
        public static string MensajeCompleto(this Exception exc, bool mostrarPila = false)
        =>
        mostrarPila ? GestorDeErrores.Detalle(exc) : GestorDeErrores.Mensaje(exc);

        public static Exception Emitir(this Exception e, string mensaje)
        {
            Exception nueva = new Exception(mensaje, e);
            nueva.Data[Datos.Mostrar] = true;
            return nueva;
        }
        public static Exception Emitir(string mensaje)
        {
            Exception nueva = new Exception(mensaje);
            nueva.Data[Datos.Mostrar] = true;
            return nueva;
        }

        public static Exception Emitir(string mensaje, Exception exc)
        {
            Exception nueva = new Exception(mensaje, exc);
            nueva.Data[Datos.Mostrar] = true;
            return nueva;
        }

        public static void EtapaNoDefinida(enumNegocio negocio, string estado)
        {
            throw Excepciones.Emitir($"No se ha definido la etapa en el negocio '{negocio.Singular(enMinuscula: false)}', cuando éste está en el estado '{estado}', defínala");
        }

        public static void MasDeUnaEtapa<T>(enumNegocio negocio, string referencia, List<T> etapas) where T : System.Enum
        {
            throw Excepciones.Emitir($"El estado del elemento '{referencia}' del negocio '{negocio.Singular()}' se encuentra en las etapas {string.Join(',', etapas)} y sólo ha de estar en una");
        }

        public static bool EsElCodigoError(this Exception e, enumCodigoDeError codigo) => e.Data != null && e.Data.Contains(Datos.CodigoError) && (enumCodigoDeError)e.Data[Datos.CodigoError] == codigo;
    }

}
