using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Juridico;
using ModeloDeDto.Juridico;
using ModeloDeDto.SistemaDocumental;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace Inicializador.MatriculasDeGuarderia
{
    public static class InzMatriculasDeGuarderia
    {

        public static void ModeloDeMatriculasDeGuarderia(ContextoSe contexto)
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
            var alIniciar = new TransicionPorMotivo
            {
                Motivo = VariablesDeContratos.enumMotivoTransicion.Iniciar.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_mtg_estado_en_cumplimentacion).Id,
                IdTransicion = enumNegocio.Contrato.Transicion(contexto, n_mtg_tran_activar, n_mtg_estado_en_cumplimentacion, n_mtg_estado_activa, delSistema: false).Id
            };

            var alFinalizar = new TransicionPorMotivo
            {
                Motivo = VariablesDeContratos.enumMotivoTransicion.Finalizar.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_mtg_estado_activa).Id,
                IdTransicion = enumNegocio.Contrato.Transicion(contexto, n_mtg_tran_finalizar, n_mtg_estado_activa, n_mtg_estado_finalizada, delSistema: true).Id
            };

            var parametro = enumNegocio.Contrato.LeerCrearParametro(contexto, enumParametrosDeContratos.CTR_Aplicar_Transicion, valor: "");
            var lista = new List<TransicionPorMotivo> {alIniciar, alFinalizar};
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
            var encumplimentacion = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(nameof(INombre.Nombre), n_mtg_estado_en_cumplimentacion);
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_En_Elaboracion, encumplimentacion.Id.ToString());

            var terminado = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(nameof(INombre.Nombre), n_mtg_estado_finalizada);
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_Finalizacion, terminado.Id.ToString());

            var activo = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(nameof(INombre.Nombre), n_mtg_estado_activa);
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_Vigente, activo.Id.ToString());

            var cancelado = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(nameof(INombre.Nombre), n_mtg_estado_cancelada);
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_Cancelado, cancelado.Id.ToString());
        }

        public const string n_mtg_estado_en_cumplimentacion = "MTG: Cumplimentándose";
        public const string n_mtg_estado_activa = "MTG: Activa";
        public const string n_mtg_estado_finalizada = "MTG: Finalizada";
        public const string n_mtg_estado_cancelada = "MTG: Cancelada";


        private static void Estados(ContextoSe contexto)
        {
            new EstadoDeUnContratoDtm { Nombre = n_mtg_estado_en_cumplimentacion  , Inicial = true , Terminado = false, Cancelado = false, Orden = 10 }.PersistirEstado(contexto);
            new EstadoDeUnContratoDtm { Nombre = n_mtg_estado_activa      , Inicial = false, Terminado = false, Cancelado = false, Orden = 20 }.PersistirEstado(contexto);
            new EstadoDeUnContratoDtm { Nombre = n_mtg_estado_finalizada   , Inicial = false, Terminado = true , Cancelado = false, Orden = 50 }.PersistirEstado(contexto);
            new EstadoDeUnContratoDtm { Nombre = n_mtg_estado_cancelada    , Inicial = false, Terminado = false , Cancelado = true , Orden = 60 }.PersistirEstado(contexto);
        }


        public const string n_mtg_tran_activar = "MTG: Activar";
        public const string n_mtg_tran_procesar = "MTG: Procesar";
        public const string n_mtg_tran_corregir = "MTG: Corregir";
        public const string n_mtg_tran_cancelar = "MTG: Cancelar";
        public const string n_mtg_tran_reactivar = "MTG: Reactivar";
        public const string n_mtg_tran_finalizar = "MTG: Finalizar";


        private static void Transiciones(ContextoSe contexto)
        {
            //Negociar: Cumplimentandose --> activa
            new TransicionesDeUnContratoDtm { 
                Nombre = n_mtg_tran_activar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_mtg_estado_en_cumplimentacion).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_mtg_estado_activa).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = null,
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Negociar: Cumplimentandose --> activa
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_mtg_tran_procesar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_mtg_estado_en_cumplimentacion).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_mtg_estado_activa).Id,
                DelSistema = true,
                ConObservacion = false,
                Asunto = null,
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //cancelar: Cumplimentandose --> cancelado
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_mtg_tran_cancelar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_mtg_estado_en_cumplimentacion).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_mtg_estado_cancelada).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Motivo de cancelación",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //reactivar: cancelado --> Cumplimentandose
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_mtg_tran_reactivar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_mtg_estado_cancelada).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_mtg_estado_en_cumplimentacion).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = null,
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Finalizar: activa --> finalizada
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_mtg_tran_finalizar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_mtg_estado_activa).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_mtg_estado_finalizada).Id,
                DelSistema = true,
                ConObservacion = false,
                Asunto = null,
                Activo = true
            }.PersistirTransicionPorAk(contexto);


            //Corregir: activa --> cumplimentándose
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_mtg_tran_corregir,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_mtg_estado_activa).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_mtg_estado_en_cumplimentacion).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Motivo de correción",
                Activo = true
            }.PersistirTransicionPorAk(contexto);
        }

        private static void Acciones(ContextoSe contexto)
        {
            TrabajosDeContratos.SometerActivarMatriculasDeGuarderia(contexto);
        }

        public const string n_mtg_tipo = "MTG: Matrícula";
        public const string n_mtg_sigla = "MTG";
        private static void Tipos(ContextoSe contexto)
        {

            var tipoDeArchivador = new TipoDeArchivadorDtm
            {
                Nombre = $"{n_mtg_sigla}: Archivador",
                ClaseDeLibro = enumClaseDeLibro.POR_CG_TIPO,
                Sigla = $"D-{n_mtg_sigla}",
                IdPadre = null,
                Activo = true,
                TipoDtm = typeof(TipoDeArchivadorDtm).FullName,
                TipoDto = typeof(TipoDeArchivadorDto).FullName,
            }.PersistirPorSigla(contexto);

            var estado = enumNegocio.Contrato.Estados(contexto).First(x => x.Nombre.Equals(n_mtg_estado_en_cumplimentacion));
            new TipoDeContratoDtm
            {
                Nombre = n_mtg_tipo,
                ClaseDeContrato = enumClaseDeContrato.MatriculaDeGuarderia,
                ClaseDeLibro = enumClaseDeLibro.POR_CG_TIPO,
                IdEstado = estado.Id,
                IdPadre = null,
                Activo=true,
                IdTipoArchivador = tipoDeArchivador.Id,
                TipoDtm = typeof(TipoDeContratoDtm).FullName,
                TipoDto = typeof(TipoDeContratoDto).FullName,
                Sigla = n_mtg_sigla
            }.PersistirPorNombre(contexto, parametros: new Dictionary<string, object> { { nameof(ContratoDtm.ClaseDeContrato),enumClaseDeContrato.MatriculaDeGuarderia} });


        }

    }
}
