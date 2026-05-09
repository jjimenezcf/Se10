using System;
using System.Collections.Generic;
using Dapper;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.TrabajosSometidos;

namespace GestoresDeNegocio.TrabajosSometidos
{
    class GestorDeSemaforoDeTrabajos
    {

        public static void PonerSemaforo(TrabajoDeUsuarioDtm tu, string login)
        {
            var sentencia = new ConsultaSql<SemaforoDeTrabajosDtm>(SemaforoDeTrabajosSql.CrearSemaforo, CacheDeVariable.Cfg_HayQueDebuggar, $"{nameof(PonerSemaforo)}");

            var valores = new Dictionary<string, object> {
                { $"@{nameof(TrabajoDeUsuarioDtm.Id)}", tu.Id },
                { $"@{nameof(TrabajoDeUsuarioDtm.Iniciado)}", DateTime.Now },
                { $"@{nameof(TrabajoDeUsuarioDtm.Sometedor.Login)}", login } };
            var semaforo = 0;
            semaforo = sentencia.EjecutarSentencia(new DynamicParameters(valores));

            if (semaforo == 0)
                throw new Exception($"No se ha podido bloquear el trabajo {tu.Trabajo.Nombre} del usuario {tu.Sometedor.Login}");
        }

        public static void QuitarSemaforo(TrabajoDeUsuarioDtm tu)
        {
            var sentencia = new ConsultaSql<SemaforoDeTrabajosDtm>(SemaforoDeTrabajosSql.BorrarSemaforo, CacheDeVariable.Cfg_HayQueDebuggar, $"{nameof(QuitarSemaforo)}");

            var valores = new Dictionary<string, object> { { $"@{nameof(TrabajoDeUsuarioDtm.Id)}", tu.Id } };
            var semaforo = 0;
            semaforo = sentencia.EjecutarSentencia(new DynamicParameters(valores));

            if (semaforo == 0)
                throw new Exception($"No se ha podido bloquear el trabajo {tu.Trabajo.Nombre} del usuario {tu.Sometedor.Login}");
        }

    }
}
