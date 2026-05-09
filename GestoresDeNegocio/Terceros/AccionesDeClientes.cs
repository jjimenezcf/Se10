using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using iText.Commons.Actions.Contexts;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Terceros
{
    public static class AccionesDeClientes
    {
        public const string N_CerrarArchivadoresDeClientes = "Cerrar archivadores de clientes";
        public const string N_AbrirArchivadorDeCliente = "Abrir archivador para cliente";

        public enum enumParametros { IdCg, IdTipoArchivador };


        public static void AbrirArchivadorDeCliente(EntornoDeUnaAccion entorno)
        {
            var cliente = (ClienteDtm)entorno.Registro;

            var parametros = ValidarParametrosDeCliente(entorno);
            cliente.AsociarArchivador(entorno.Contexto, parametros.IdCg, parametros.IdTipoArchivador);
        }


        public static void CerrarArchivadoresDeClientes(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;
            var cliente = (ClienteDtm)entorno.Registro;

            var parametros = ValidarParametrosDeCliente(entorno); var archivadores = cliente.Vinculados<ArchivadorDtm>(entorno.Contexto, new Dictionary<string, object>
            {
                { ltrParametrosNeg.IncluirBajas, false}
            });

            foreach (var archivador in archivadores.Where(x => x.IdTipo == parametros.IdTipoArchivador && x.IdCg == parametros.IdCg))
            {
                archivador.Baja = true;
                archivador.Modificar(entorno.Contexto, nameof(CerrarArchivadoresDeClientes));
            }
        }


        private static (int IdCg , int IdTipoArchivador) ValidarParametrosDeCliente(EntornoDeUnaAccion entorno)
        {
            var tipo = ExtensorDeClientes.LeerTipoDeArchivadorDeClientes(entorno.Contexto, errorSiNoEstaDefinido: true);
            var cg = ExtensorDeClientes.LeerCentroGestorDeClientes(entorno.Contexto, errorSiNoEstaDefinido: true);

            return (cg.Id, tipo.Id);
        }
    }
}
