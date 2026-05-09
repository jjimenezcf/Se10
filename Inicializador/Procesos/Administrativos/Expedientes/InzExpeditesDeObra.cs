using GestoresDeNegocio.Expediente;
using GestoresDeNegocio.Negocio;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using Utilidades;
using GestorDeElementos;

namespace Inicializador.Expedientes
{
    public static class InzExpeditesDeObra
    {
        public static readonly string n_Obr = "OBR";

        public static void ModeloDeExpedienteDeObras(ContextoSe contexto)
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

        public static readonly string n_estado_exp_obr_abierto = $"{n_Obr}: Abierto";
        public static readonly string n_estado_exp_obr_iniciado = $"{n_Obr}: Iniciado";
        public static readonly string n_estado_exp_obr_cerrado = $"{n_Obr}: Cerrado";
        public static readonly string n_estado_exp_obr_cancelado = $"{n_Obr}: Cancelado";


        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados del expediente");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_obr_abierto, inicial: true, terminado: false, cancelado: false, 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_obr_iniciado, false, false, false, 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_obr_cerrado, false, true, false, 40);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_obr_cancelado, false, false, true, 50);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_tran_exp_obr_iniciar = $"{n_Obr}: Iniciar";
        public static readonly string n_tran_exp_obr_devolver = $"{n_Obr}: Devolver";
        public static readonly string n_tran_exp_obr_cerrar = $"{n_Obr}: Cerrar";
        public static readonly string n_tran_exp_obr_reabrir = $"{n_Obr}: Reabrir";
        public static readonly string n_tran_exp_obr_cancelar = $"{n_Obr}: Cancelar";
        
        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de expedientes de obras");
            try
            {
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_obr_iniciar, n_estado_exp_obr_abierto, n_estado_exp_obr_iniciado);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_obr_cancelar, n_estado_exp_obr_abierto, n_estado_exp_obr_cancelado, asunto: "Motivo de cancelación");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_obr_devolver, n_estado_exp_obr_iniciado, n_estado_exp_obr_abierto, asunto: "Motivo de devolución a inicial");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_obr_cerrar, n_estado_exp_obr_iniciado, n_estado_exp_obr_cerrado, asunto:"Anotación de cierre");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_obr_reabrir, n_estado_exp_obr_cerrado, n_estado_exp_obr_iniciado, asunto: "Anotación de reapertura");
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_exp_tipo_expediente_obra = $"{n_Obr}: Obra";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de expedientes");
            try
            {                
                var obr_estadoInicial = enumNegocio.Expediente.Estado(contexto, n_estado_exp_obr_abierto);
                GestorDeTiposDeExpediente.PersistirTipo(contexto, enumClaseDeExpediente.ConValoracion, n_exp_tipo_expediente_obra, obr_estadoInicial.Id, 
                    enumClaseDeLibro.POR_CG_TIPO, n_Obr, usaPpts: true, scDeVenta: false, scDeCompra: false, usaDatosJuridicos: false);
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
            var iniciales = "";
            var enCurso = "";
            foreach (EstadoDtm estado in estados)
                if (estado.Inicial) iniciales = $"{(iniciales.IsNullOrEmpty() ? estado.Id.ToString() : $"{iniciales},{estado.Id}")}";

            foreach (EstadoDtm estado in estados)
                if (!estado.Inicial && !estado.Terminado && !estado.Cancelado)
                    enCurso = $"{(enCurso.IsNullOrEmpty() ? estado.Id.ToString() : $"{enCurso},{estado.Id}")}";

            var etapaPpts =
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(InzExpeditesDeObra.n_estado_exp_obr_iniciado).Id + "," +
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(InzExpeditesDeObra.n_estado_exp_obr_abierto).Id + ",";



            var etapaEjecucion =
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(InzExpeditesDeObra.n_estado_exp_obr_iniciado).Id.ToString() ;

            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas, iniciales + "," + enCurso);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos, etapaPpts);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Ejecucion, etapaEjecucion);
        }
    }
}
