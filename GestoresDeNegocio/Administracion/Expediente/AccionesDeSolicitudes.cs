using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;

namespace GestoresDeNegocio.Expediente
{
    public static class AccionesDeSolicitudes
    {
        public const string N_DesasociarContratosDeLaSolicitud = "Desasociar los contratos de la solicitud";
        public const string N_ValidarQueNoHayaContratos = "Valida que la solicitud no tenga contratos";
        public const string N_ValidarQueSiHayContratosEstanCancelados = "Valida que la solicitud si tiene contratos están cancelados";
        public const string N_ValidarQueHayAlMenosUnContratoAsociadoNoCancelado = "Valida que la solicitud al menos tiene un contrato no cancelado";

        public static void DesasociarContratosDeLaSolicitud(EntornoDeUnaAccion entorno)
        {
            //obtengo los contratos de los expedientes
            var expediente = (ExpedienteDtm)entorno.Registro;
            var contratos = entorno.Contexto.SeleccionarTodos<ContratoDtm>(nameof(ContratoDtm.IdExpediente), expediente.Id);

            //si no todos ellos están en la etapa de elaboración o cancelados no lo permito
            foreach (var contrato in contratos)
            {
                var etapa = contrato.Etapa();
                if (etapa != enumEtapasDeContratos.CTR_Etapa_En_Elaboracion && etapa != enumEtapasDeContratos.CTR_Etapa_Cancelado)
                    GestorDeErrores.Emitir($"La Solicitud de contrato no se puede reactivar por tener el contrato '{contrato.Referencia}' activo o finalizado");
            }

            //recorro los contratos y blanqueo el campo idexpediente
            foreach (var contrato in contratos)
            {
                contrato.IdExpediente = null;
                contrato.Modificar(entorno.Contexto, esUnaAccion:true);
            }
        }


        public static void ValidarQueNoHayaContratos(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;
            if (entorno.Contexto.Existen<ContratoDtm>(nameof(ContratoDtm.IdExpediente), expediente.Id))
                GestorDeErrores.Emitir($"La solicitud de contrato no puede transitarse por tener contratos asociados");
        }

        public static void ValidarQueSiHayContratosEstanCancelados(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;
            var contratos = entorno.Contexto.SeleccionarTodos<ContratoDtm>(nameof(ContratoDtm.IdExpediente), expediente.Id);
            foreach (var contrato in contratos)
            {
                var etapa = contrato.Etapa();
                if (etapa != enumEtapasDeContratos.CTR_Etapa_Cancelado)
                    GestorDeErrores.Emitir($"La Solicitud de contrato no puede transitarse por tener el contrato '{contrato.Referencia}' no cancelado");
            }

        }

        public static void ValidarQueHayAlMenosUnContratoAsociadoNoCancelado(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;
            var contratos = entorno.Contexto.SeleccionarTodos<ContratoDtm>(nameof(ContratoDtm.IdExpediente), expediente.Id);
            if (contratos.Count == 0)
                GestorDeErrores.Emitir($"La Solicitud de contrato no puede transitarse por no tener contratos");

            var todosCancelados = true;
            foreach (var contrato in contratos)
            {
                var etapa = contrato.Etapa();
                if (etapa != enumEtapasDeContratos.CTR_Etapa_Cancelado)
                {
                    todosCancelados = false;
                    break;
                }
            }
            if (todosCancelados)
                GestorDeErrores.Emitir($"La Solicitud de contrato no puede transitarse por tener todos sus contratos cancelados");
        }        

    }
}
