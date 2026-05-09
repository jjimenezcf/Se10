using GestoresDeNegocio.Expediente;
using GestoresDeNegocio.Negocio;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using Utilidades;
using GestorDeElementos;
using ServicioDeDatos.Entorno;
using Inicializador.Procesos;
using ServicioDeDatos.Tarea;

namespace Inicializador.Expedientes
{
    public static class InzExpeditesDeTalleres
    {
        public static readonly string a_tll = "TLL";

        public static void ModeloDeExpedienteDeTalleres(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                Etapas(contexto);
                Acciones(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public static readonly string n_estado_exp_tll_pdt_recepcion = $"{a_tll}: Pdt de reprepción";
        public static readonly string n_estado_exp_tll_en_espera = $"{a_tll}: En espera";
        public static readonly string n_estado_exp_tll_en_taller = $"{a_tll}: En taller";
        public static readonly string n_estado_exp_tll_en_chapista = $"{a_tll}: En chapista";
        public static readonly string n_estado_exp_tll_pdt_recoger = $"{a_tll}: Pdt. de recoger";
        public static readonly string n_estado_exp_tll_pdt_pago = $"{a_tll}: Pdt. de pago";
        public static readonly string n_estado_exp_tll_cerrado = $"{a_tll}: cerrado";
        public static readonly string n_estado_exp_tll_cancelado = $"{a_tll}: Cancelado";


        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados del expediente");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_tll_pdt_recepcion, inicial: true, orden: 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_tll_en_espera, orden: 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_tll_en_taller, orden: 30);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_tll_en_chapista, orden: 40);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_tll_pdt_recoger, orden: 50);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_tll_pdt_pago, orden: 60);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_tll_cerrado, terminado: true, orden: 70);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_exp_tll_cancelado, cancelado: true, orden: 90);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_tran_exp_tll_recepcionar = $"{a_tll}: Recepcionar vehículo";
        public static readonly string n_tran_exp_tll_enviar_chapista = $"{a_tll}: Enviar a chapa";
        public static readonly string n_tran_exp_tll_recepcionar_chapista = $"{a_tll}: Recibir de chapa";
        public static readonly string n_tran_exp_tll_al_prefacturar = $"{a_tll}: Al prefacturar";
        public static readonly string n_tran_exp_tll_pasar_taller = $"{a_tll}: Pasarlo al taller";
        public static readonly string n_tran_exp_tll_recogido_sin_Pago = $"{a_tll}: Recogerlo sin pago";
        public static readonly string n_tran_exp_tll_pagado = $"{a_tll}: Cerrar";
        public static readonly string n_tran_exp_tll_cancelar = $"{a_tll}: Cancelar";
        public static readonly string n_tran_exp_tll_nueva_entrada = $"{a_tll}: Nueva visita";
        public static readonly string n_tran_exp_tll_nueva_entrada_sin_pago = $"{a_tll}: Nueva visita";
        public static readonly string n_tran_exp_tll_devolver_pdt_pago = $"{a_tll}: Devolver a pdt de pago";
        public static readonly string n_tran_exp_tll_devolver_cerrado = $"{a_tll}: Devolver a cerrado";
        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de expedientes de obras");
            try
            {
                //pdt de recepcionar --> En espera, Cancelado, Cerrado, Pdt de pago
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_tll_recepcionar, n_estado_exp_tll_pdt_recepcion, n_estado_exp_tll_en_espera);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_tll_cancelar, n_estado_exp_tll_pdt_recepcion, n_estado_exp_tll_cancelado, asunto: "Motivo de cancelación");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_tll_devolver_pdt_pago, n_estado_exp_tll_pdt_recepcion, n_estado_exp_tll_pdt_pago, asunto: "Motivo de devolución");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_tll_devolver_cerrado, n_estado_exp_tll_pdt_recepcion, n_estado_exp_tll_cerrado, asunto: "Motivo de devolución");

                //en espera --> en chapista, en taller
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_tll_pasar_taller, n_estado_exp_tll_en_espera, n_estado_exp_tll_en_taller);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_tll_enviar_chapista, n_estado_exp_tll_en_espera, n_estado_exp_tll_en_chapista);

                //en chapista --> pdt de recoger
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_tll_al_prefacturar, n_estado_exp_tll_en_chapista, n_estado_exp_tll_pdt_recoger);

                //en taller --> pdt de recoger
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_tll_al_prefacturar, n_estado_exp_tll_en_taller, n_estado_exp_tll_pdt_recoger);

                //pdt de recoger --> pdt de pago, cerrar
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_tll_pagado, n_estado_exp_tll_pdt_recoger, n_estado_exp_tll_cerrado);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_tll_recogido_sin_Pago, n_estado_exp_tll_pdt_recoger, n_estado_exp_tll_pdt_pago, asunto: "Motivo de retirada sin pago");

                //Cerrado --> pdt de recepcionar
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_tll_nueva_entrada, n_estado_exp_tll_cerrado, n_estado_exp_tll_pdt_recepcion);

                //pdt de pago --> pdt de recepcionar
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_exp_tll_nueva_entrada_sin_pago, n_estado_exp_tll_pdt_pago, n_estado_exp_tll_pdt_recepcion, asunto: "Motivo de nueva entrada con pago pendiente");
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_exp_tipo_expediente_talle = $"{a_tll}: Reparación de vehiculos";
        
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de expedientes");
            try
            {
                var obr_estadoInicial = enumNegocio.Expediente.Estado(contexto, n_estado_exp_tll_pdt_recepcion);
                GestorDeTiposDeExpediente.PersistirTipo(contexto, enumClaseDeExpediente.ConValoracion, n_exp_tipo_expediente_talle, obr_estadoInicial.Id, 
                    enumClaseDeLibro.POR_CG_TIPO, a_tll, usaPpts: true, scDeVenta: false, scDeCompra: false, usaDatosJuridicos: false);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static void Acciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Acciones de las transiciones de tareas de TRR");
            try
            {
                CrearAccionesDeTalleres(contexto);
                AccionAlTransitar(contexto);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void CrearAccionesDeTalleres(ContextoSe contexto)
        {
            var accion = new AccionDtm();
            accion.Dll = $"{nameof(GestoresDeNegocio)}";
            accion.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.Expediente)}.{nameof(AccionesDeExpedientes)}";
            #region Enviar mensaje a un cliente de un taller
            accion.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            accion.Nombre = AccionesDeExpedientes.N_EnviarCorreoAlSolicitante;
            accion.Descripcion = $@"Envía un correo a un cliente de un expediente{Environment.NewLine}{AccionesDeExpedientes.ParametrosEnviarCorreoAlSolicitante}";
            accion.Metodo = nameof(AccionesDeExpedientes.EnviarCorreoAlSolicitante);
            accion.PersistirAccion(contexto);
            #endregion
        }

        private static void AccionAlTransitar(ContextoSe contexto)
        {
            var transiciones = enumNegocio.Expediente.TransicionesPorNombre(contexto, n_tran_exp_tll_al_prefacturar);
            foreach (var transicion in transiciones)
            {
                GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
                    negocio: enumNegocio.Expediente,
                    transicion: transicion,
                    nombreAccion: AccionesDeExpedientes.N_EnviarCorreoAlSolicitante,
                    momento: enumMomentoDeEjecucion.D,
                    parametro: AccionesDeExpedientes.ParametrosEnviarCorreoAlSolicitante,
                    orden: 10,
                    descripcion: $"Envío de un correo al cliente para que venga a recoger el vehículo");
            }
        }

        private static void Etapas(ContextoSe contexto)
        {
            ExtensorDeExpedientes.DefinirEtapasBasicas(contexto);
            
            var estados = enumNegocio.Expediente.Estados(contexto);
            var iniciales = "";
            foreach (EstadoDtm estado in estados)
                iniciales = $"{(iniciales.IsNullOrEmpty() ? estado.Id.ToString() : $"{iniciales},{estado.Id}")}";

            var enCurso = "";
            foreach (EstadoDtm estado in estados)
                if (!estado.Inicial && !estado.Terminado && !estado.Cancelado)
                    enCurso = $"{(enCurso.IsNullOrEmpty() ? estado.Id.ToString() : $"{enCurso},{estado.Id}")}";

            var etapaPpts =
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_exp_tll_pdt_recepcion).Id + "," +
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_exp_tll_en_espera).Id;

            var etapaEjecucion =
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_exp_tll_en_taller).Id.ToString() + "," +
            contexto.SeleccionarEstado<EstadoDeUnExpedienteDtm>(n_estado_exp_tll_en_chapista).Id.ToString();

            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas, iniciales + "," + enCurso);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos, etapaPpts);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Ejecucion, etapaEjecucion);
        }
    }
}
