using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using System;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Juridico
{
    public static class AccionesDeContratos
    {
        public const string N_TransitarSolicitudSiTodosSusContratosVntCancelados = "Transita la solicitud al estado anterior si todos los contratos están cancelados";
        public const string N_ValidarFechaInicioMayorQueHoy = "Valida que la fecha de inicio de contrato es mayor a hoy";
        public const string N_ValidarQueNoHayPlanificacionesDeVenta = "Validar que no hay planificaciones asociadas al contrato";

        public static void TransitarSolicitudSiTodosSusContratosVntCancelados(EntornoDeUnaAccion entorno)
        {
            if (((ContratoDtm)entorno.Registro).IdExpediente == null)
                return;

            var sc = entorno.Contexto.SeleccionarPorId<ExpedienteDtm>((int)((ContratoDtm)entorno.Registro).IdExpediente);
            var contratos = entorno.Contexto.SeleccionarTodos<ContratoDtm>(nameof(ContratoDtm.IdExpediente), sc.Id);

            //si no todos ellos están cancelados transito la solicitud
            foreach (var contrato in contratos)
            {
                var etapa = contrato.Etapa();
                if (etapa != enumEtapasDeContratos.CTR_Etapa_Cancelado)
                   return;
            }

            //Si el estado al que lo devuelvo la solicitud no es en la que se puede asociar contratos, no transito
            var estados =  enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Venta.Lista();
            var hitoAnterior = sc.HitoAnteriorAlActual(entorno.Contexto);
            if (!estados.Contains(hitoAnterior.IdEstado))
                return;

            var transicion = entorno.Contexto.Transicion(enumNegocio.Expediente, sc.IdEstado, hitoAnterior.IdEstado, delSistema: true);
            sc.Transitar(entorno.Contexto, transicion.Id);
        }

        public static void ValidarFechaInicioMayorQueHoy(EntornoDeUnaAccion entorno)
        {
            var contrato = (ContratoDtm) entorno.Registro;
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(entorno.Contexto);
            if (datos.InicioContrato.Date < DateTime.Now.Date)
                GestorDeErrores.Emitir("No se puede reactivar el contrato, ya que su fecha de inico es anterior al día de hoy");
        }

        public static void ValidarQueNoHayPlanificacionesDeVenta(EntornoDeUnaAccion entorno)
        {
            var contrato = (ContratoDtm)entorno.Registro;
            var planificadores = entorno.Contexto.SeleccionarTodos<PlanificadorDeVentaDtm>(nameof(PlanificadorDeVentaDtm.IdContrato),contrato.Id);
            if (planificadores.FirstOrDefault(x => x.Generado) != null)
                GestorDeErrores.Emitir("No se puede retroceder un contrato con planificaciones ya generadas");
        }

    }
}
