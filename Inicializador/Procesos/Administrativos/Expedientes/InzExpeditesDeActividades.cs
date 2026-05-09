using GestoresDeNegocio.Expediente;
using GestoresDeNegocio.Negocio;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using Utilidades;
using GestorDeElementos;
using ServicioDeDatos.Tarea;

namespace Inicializador.Expedientes
{
    public static class InzExpeditesDeActividades
    {
        public static readonly string a_act = "ACT";

        public static void ModeloDeActividades(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                Etapas(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public static readonly string n_estado_act_abierto = $"{a_act}: Abierto";
        public static readonly string n_estado_act_cerrado = $"{a_act}: Cerrado";
        public static readonly string n_estado_act_cancelado = $"{a_act}: Cancelado";


        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados del expediente");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_act_abierto, inicial: true, orden: 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_act_cerrado, terminado: true, orden: 70);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_act_cancelado, cancelado: true, orden: 90);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_tran_act_cerrar = $"{a_act}: Cerrar actividad";
        public static readonly string n_tran_act_cancelar = $"{a_act}: Cancelar";
        public static readonly string n_tran_act_reabrir = $"{a_act}: Reabrir";
        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de expedientes de obras");
            try
            {
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_act_cerrar, n_estado_act_abierto, n_estado_act_cerrado, porDefecto:true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_act_cancelar, n_estado_act_abierto, n_estado_act_cancelado, asunto: "Motivo de cancelación");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_act_reabrir, n_estado_act_cerrado, n_estado_act_abierto, asunto: "Motivo de reapertura");
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_exp_tipo_actividad = $"{a_act}: Actividades formativas";
        
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("tipos de actividades formativas");
            try
            {
                var obr_estadoInicial = enumNegocio.Expediente.Estado(contexto, n_estado_act_abierto);
                var tipo =  GestorDeTiposDeExpediente.PersistirTipo(contexto, enumClaseDeExpediente.ConValoracion, n_exp_tipo_actividad, obr_estadoInicial.Id, 
                    enumClaseDeLibro.POR_CG_TIPO, a_act, usaPpts: true, scDeVenta: false, scDeCompra: false, usaDatosJuridicos: false);
                enumNegocio.Expediente.ResetearParametro(contexto, enumParametrosDeExpedientes.EXP_Tipos_Para_Actividades, tipo.Id.ToString());
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void Etapas(ContextoSe contexto)
        {
            ExtensorDeExpedientes.DefinirEtapasBasicas(contexto);
            
            var estados = enumNegocio.Expediente.Estados(contexto);
            
            var enCurso = contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_act_abierto).Id.ToString();

            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas, enCurso);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos, enCurso);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Ejecucion, enCurso);
        }
    }
}
