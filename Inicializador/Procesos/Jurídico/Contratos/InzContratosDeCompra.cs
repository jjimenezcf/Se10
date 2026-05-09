using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto.Juridico;
using ModeloDeDto.SistemaDocumental;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace Inicializador.ContratosCmp
{
    public static class InzContratosCmp
    {

        public static void ModeloDeContratosCmp(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                DefinirEtapas(contexto);
                DefinirVariables(contexto);
                Acciones(contexto);
                Tipos(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        private static void DefinirVariables(ContextoSe contexto)
        {
            enumNegocio.Contrato.ResetearParametro(contexto, enumParametrosDeContratos.CTR_Porcentaje_Aviso, "80");
            enumNegocio.Contrato.ResetearParametro(contexto, enumParametrosDeContratos.CTR_Porcentaje_Bloqueo, "100");

            var alIniciar = new TransicionPorMotivo
            {
                Motivo = VariablesDeContratos.enumMotivoTransicion.Iniciar.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_vigente).Id,
                IdTransicion = enumNegocio.Contrato.Transicion(contexto, n_ctc_tran_iniciar, n_ctc_estado_pdt_activar, n_ctc_estado_vigente, delSistema: false).Id
            };

            var alPdtProrrogar = new TransicionPorMotivo
            {
                Motivo = VariablesDeContratos.enumMotivoTransicion.PdtProrroga.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_vigente).Id,
                IdTransicion = enumNegocio.Contrato.Transicion(contexto, n_ctc_tran_pasar_a_prorrogar, n_ctc_estado_vigente, n_ctc_estado_pdt_prorrogar, delSistema: true).Id
            };

            var alProrrogar = new TransicionPorMotivo
            {
                Motivo = VariablesDeContratos.enumMotivoTransicion.Prorrogar.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_pdt_prorrogar).Id,
                IdTransicion = enumNegocio.Contrato.Transicion(contexto, n_ctc_tran_prorrogar_tacitamente, n_ctc_estado_pdt_prorrogar, n_ctc_estado_vigente, delSistema: true).Id
            };

            var alFinalizar = new TransicionPorMotivo
            {
                Motivo = VariablesDeContratos.enumMotivoTransicion.Finalizar.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_vigente).Id,
                IdTransicion = enumNegocio.Contrato.Transicion(contexto, n_ctc_tran_finalizar, n_ctc_estado_vigente, n_ctc_estado_finalizado, delSistema: true).Id
            };

            var parametro = enumNegocio.Contrato.LeerCrearParametro(contexto, enumParametrosDeContratos.CTR_Aplicar_Transicion, valor: "");
            var lista = new List<TransicionPorMotivo> {alPdtProrrogar, alProrrogar, alIniciar, alFinalizar};
            if (!parametro.Valor.IsNullOrEmpty())
            {
                var lista2 = JsonConvert.DeserializeObject<List<TransicionPorMotivo>>(parametro.Valor);
                if (lista2 is not null) foreach (var item in lista2)
                        if (lista.FirstOrDefault(x => x.IdEstado == item.IdEstado && x.IdTransicion == item.IdTransicion && x.Motivo == item.Motivo) is null)
                            lista.Add(item);
            }

            enumNegocio.Contrato.ResetearParametro(contexto, enumParametrosDeContratos.CTR_Aplicar_Transicion, JsonConvert.SerializeObject(lista));
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var estadoDerogado = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(nameof(INombre.Nombre), n_ctc_estado_derogado);
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_Derogado, estadoDerogado.Id.ToString());
            //ExtensorDeContratos.InicializarEtapas(contexto);
        }

        public const string n_ctc_estado_pdt_activar = "CTC: Pdt. de Activar";
        public const string n_ctc_estado_vigente = "CTC: Vigente";
        public const string n_ctc_estado_pdt_prorrogar = "CTC: Pdt. de prorrogar";
        public const string n_ctc_estado_derogado = "CTC: Derogado";
        public const string n_ctc_estado_finalizado = "CTC: Finalizado";
        public const string n_ctc_estado_cancelado = "CTC: Cancelado";


        private static void Estados(ContextoSe contexto)
        {
            new EstadoDeUnContratoDtm { Nombre = n_ctc_estado_pdt_activar  , Inicial = true , Terminado = false, Cancelado = false, Orden = 10 }.PersistirEstado(contexto);
            new EstadoDeUnContratoDtm { Nombre = n_ctc_estado_vigente      , Inicial = false, Terminado = false, Cancelado = false, Orden = 20 }.PersistirEstado(contexto);
            new EstadoDeUnContratoDtm { Nombre = n_ctc_estado_pdt_prorrogar, Inicial = false, Terminado = false, Cancelado = false, Orden = 30 }.PersistirEstado(contexto);
            new EstadoDeUnContratoDtm { Nombre = n_ctc_estado_finalizado   , Inicial = false, Terminado = true , Cancelado = false, Orden = 50 }.PersistirEstado(contexto);
            new EstadoDeUnContratoDtm { Nombre = n_ctc_estado_derogado     , Inicial = false, Terminado = true,  Cancelado = false, Orden = 55 }.PersistirEstado(contexto);
            new EstadoDeUnContratoDtm { Nombre = n_ctc_estado_cancelado    , Inicial = false, Terminado = true , Cancelado = true , Orden = 60 }.PersistirEstado(contexto);
        }


        public const string n_ctc_tran_iniciar = "CTC: Iniciar";
        public const string n_ctc_tran_devolver = "CTC: Devolver";
        public const string n_ctc_tran_cancelar = "CTC: Cancelar";
        public const string n_ctc_tran_reactivar = "CTC: Reactivar";
        public const string n_ctc_tran_pasar_a_prorrogar = "CTC: Pasar a prorrogar";
        public const string n_ctc_tran_prorrogar_explicitamente = "CTC: Prorrogar";
        public const string n_ctc_tran_prorrogar_tacitamente = "CTC: Renovación tácita";
        public const string n_ctc_tran_no_prorrogar = "CTC: No Prorrogar";
        public const string n_ctc_tran_finalizar = "CTC: Finalizar";
        public const string n_ctc_tran_derogar = "CTC: Derogar";


        private static void Transiciones(ContextoSe contexto)
        {
            //Negociar: Pdt. de activar --> vigente
            new TransicionesDeUnContratoDtm { 
                Nombre = n_ctc_tran_iniciar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_pdt_activar).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_vigente).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = null,
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //cancelar: Pdt. de activar --> cancelado
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctc_tran_cancelar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_pdt_activar).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_cancelado).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Motivo de cancelación",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //reactivar: cancelado --> Pdt. de activar
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctc_tran_reactivar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_cancelado).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_pdt_activar).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = null,
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //pasar a pendiente de prorrogar: vigente --> pdt de prorroga
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctc_tran_pasar_a_prorrogar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_vigente).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_pdt_prorrogar).Id,
                DelSistema = true,
                ConObservacion = false,
                Asunto = null,
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Prorrogar explicitamente: pdt. de prorrogar --> vigente
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctc_tran_prorrogar_explicitamente,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_pdt_prorrogar).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_vigente).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = "Justificación de la prórroga",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Prorrogar tácitamente: pdt. de prorrogar --> vigente
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctc_tran_prorrogar_tacitamente,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_pdt_prorrogar).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_vigente).Id,
                DelSistema = true,
                ConObservacion = false,
                Asunto = "",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //No prorrogar: pdt. de prorrogar --> Finalizado
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctc_tran_no_prorrogar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_pdt_prorrogar).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_finalizado).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Motivo de no prorrogar",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Derogar: Vigente --> Derogado
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctc_tran_derogar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_vigente).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_derogado).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = "Motivo de derogación",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Derogar: Vigente --> devolución
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctc_tran_devolver,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_vigente).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_pdt_activar).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Motivo de devolución",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Derogar: Vigente --> finalizado
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctc_tran_finalizar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_vigente).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctc_estado_finalizado).Id,
                DelSistema = true,
                ConObservacion = false,
                Asunto = "",
                Activo = true
            }.PersistirTransicionPorAk(contexto);
        }

        private static void Acciones(ContextoSe contexto)
        {
        }

        public const string n_ctc_tipo_compra = "CTC: Compra de servicios";
        public const string n_ctc_sigla_compra = "CMP";
        private static void Tipos(ContextoSe contexto)
        {

            var tipoDeArchivador = new TipoDeArchivadorDtm
            {
                Nombre = "CTR: Archivador",
                ClaseDeLibro = enumClaseDeLibro.POR_CG_TIPO,
                Sigla = "D-CTR",
                IdPadre = null,
                Activo = true,
                TipoDtm = typeof(TipoDeArchivadorDtm).FullName,
                TipoDto = typeof(TipoDeArchivadorDto).FullName,
            }.PersistirPorSigla(contexto);

            var estado = enumNegocio.Contrato.Estados(contexto).First(x => x.Nombre.Equals(n_ctc_estado_pdt_activar));
            new TipoDeContratoDtm
            {
                Nombre = n_ctc_tipo_compra,
                ClaseDeContrato = enumClaseDeContrato.Compra,
                ClaseDeLibro = enumClaseDeLibro.POR_CG_TIPO,
                IdEstado = estado.Id,
                IdPadre = null,
                Activo=true,
                IdTipoArchivador = tipoDeArchivador.Id,
                TipoDtm = typeof(TipoDeContratoDtm).FullName,
                TipoDto = typeof(TipoDeContratoDto).FullName,
                Sigla = n_ctc_sigla_compra
            }.PersistirPorNombre(contexto, parametros: new Dictionary<string, object> { { nameof(ContratoDtm.ClaseDeContrato),enumClaseDeContrato.Compra} });


        }

    }
}
