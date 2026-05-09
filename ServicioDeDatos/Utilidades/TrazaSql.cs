using Utilidades;
using System;
using System.Data.Common;
using Gestor.Errores;
using System.Reflection;

namespace ServicioDeDatos.Utilidades
{

    public class TrazaSql : Traza
    {
        private double duraciAcomulada = 0;
        private int sentenciasEjecutadas = 0;

        private static Type _clase = ApiDeEnsamblados.ObtenerType(ApiDeEnsamblados.DllDelServicioDeDatos, ApiDeEnsamblados.ClaseCacheDeVariable);

        private static string _ruta = null;
        
        public static string Ruta
        {
            get
            {
                if (_ruta is not null)
                    return _ruta;

                PropertyInfo propiedad = _clase.GetProperty(ApiDeEnsamblados.CFG_Ruta_Ficheros_De_Debug);
                try
                {
                    _ruta = (string)propiedad.GetValue(null);
                    return _ruta;
                }
                catch
                {
                    return @"c:\Temp\Trazas";
                }
            }
            set { _ruta = value; }
        }

        public static TrazaSql CrearTraza(string fichero, NivelDeTraza nivel = NivelDeTraza.Siempre, string mensajeInicial = "Traza de debuguer", string ruta = null)
        {
            fichero = fichero.Replace(".txt", "").Replace(" ", Simbolos.Descarte);
            fichero = $"{fichero}.txt"; //_{DateTime.Now

            return new TrazaSql(nivel, ruta is null? Ruta: ruta, fichero, mensajeInicial);
        }

        public TrazaSql(NivelDeTraza nivel, string ruta, string fichero, string mensajeInicial)
        : base(nivel, ruta, fichero)
        {
            Cronometro.Start();
            Abrir(false);
            AnotarMensaje("Inicio", mensajeInicial);
        }

        public string CerrarTraza(string mensaje)
        {
            Cronometro.Stop();
            mensaje = $"Petición finalizada" + Environment.NewLine +
                      $"Total SQLs:     {duraciAcomulada,9:0.000}" + Environment.NewLine +
                      $"Total petición: {Cronometro.ElapsedMilliseconds,9:0.000}" + Environment.NewLine +
                      Environment.NewLine +
                      Environment.NewLine +
                      $"Total Sentencias:{sentenciasEjecutadas}" + Environment.NewLine +
                      mensaje;
            Registrar(mensaje);
            Cerrar();
            return mensaje;
        }

        public void AnotarTrazaSql(string setenciaSql, DbParameterCollection dbParametros, double duracion)
        {
            if (Abierta)
            {
                var parametros = dbParametros.ParsearParametros();
                duraciAcomulada += duracion;

                string logTraza = $"Parámetros:{Environment.NewLine}{parametros}{Environment.NewLine}" +
                                  $"{setenciaSql}{Environment.NewLine}" +
                                  $"Duracion SQL:    {duracion,9:0.000}" + Environment.NewLine +
                                  $"Total SQLs:      {duraciAcomulada,9:0.000}" + Environment.NewLine +
                                  $"Tiempo petición: {Cronometro.ElapsedMilliseconds,9:0.000}" + Environment.NewLine;

                //+ $"Traza petición: {Environment.StackTrace}" + Environment.NewLine;



                AnotarMensaje("Sentencia SQL ejecutada:", logTraza);
                sentenciasEjecutadas++;
            }
        }

        public void AnotarExcepcion(Exception exc, string setenciaSql, DbParameterCollection dbParametros)
        {
            var parametros = dbParametros.ParsearParametros();
            string logTraza = $"Parámetros:{Environment.NewLine}{parametros}{Environment.NewLine}" +
                $"{setenciaSql}{Environment.NewLine}" +
                $"Excepción: {Environment.NewLine}{exc.MensajeCompleto(true)}{Environment.NewLine}";

            AnotarMensaje("Excepción generada:", logTraza);
        }

        public void AnotarExcepcion(Exception exc)
        {
            AnotarMensaje("Excepción generada:", exc.MensajeCompleto(true));
        }

        public void AnotarMensaje(string asunto, string mensaje)
        {
            if (Abierta)
            {
                string logTraza = $"{asunto}{Environment.NewLine}{mensaje}{Environment.NewLine}";
                Registrar(logTraza);
                Separador();
            }
        }
        public void EmitirExcepcion(string excepcion, string traza)
        {
            AnotarMensaje("Excepción generada:", traza);
            GestorDeErrores.Emitir(excepcion);
        }

    }


}
