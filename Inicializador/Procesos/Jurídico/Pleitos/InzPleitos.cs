using GestorDeElementos.Extensores;
using ModeloDeDto.Juridico;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Juridico;

namespace Inicializador.Pleitos
{
    public static class InzPleitos
    {

        public static void ModeloDePleitos(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public const string n_estado_abierto = "JUD: Abierto";
        public const string n_estado_en_negociacion = "JUD: En negociación";
        public const string n_estado_demandado = "JUD: Demandado";
        public const string n_estado_pdt_de_fallo = "JUD: Pdt de fallo";
        public const string n_estado_cerrado = "JUD: Cerrado";
        public const string n_estado_perdido = "JUD: Perdido";
        public const string n_estado_acordado = "JUD: Acordado";


        private static void Estados(ContextoSe contexto)
        {
            new EstadoDeUnPleitoDtm { Nombre = n_estado_abierto, Inicial = true, Terminado = false, Cancelado = false, Orden = 10 }.PersistirEstado(contexto);
            new EstadoDeUnPleitoDtm { Nombre = n_estado_en_negociacion, Inicial = false, Terminado = false, Cancelado = false, Orden = 20 }.PersistirEstado(contexto);
            new EstadoDeUnPleitoDtm { Nombre = n_estado_demandado, Inicial = false, Terminado = false, Cancelado = false, Orden = 30 }.PersistirEstado(contexto);
            new EstadoDeUnPleitoDtm { Nombre = n_estado_pdt_de_fallo, Inicial = false, Terminado = false, Cancelado = false, Orden = 35 }.PersistirEstado(contexto);
            new EstadoDeUnPleitoDtm { Nombre = n_estado_cerrado, Inicial = false, Terminado = true, Cancelado = false, Orden = 50 }.PersistirEstado(contexto);
            new EstadoDeUnPleitoDtm { Nombre = n_estado_perdido, Inicial = false, Terminado = true, Cancelado = false, Orden = 60 }.PersistirEstado(contexto);
            new EstadoDeUnPleitoDtm { Nombre = n_estado_acordado, Inicial = false, Terminado = true, Cancelado = false, Orden = 40 }.PersistirEstado(contexto);
        }


        public const string n_tran_negociar = "JUD: Negociar";
        public const string n_tran_demandar = "JUD: Demanda";
        public const string n_tran_acordar = "JUD: Acordar";
        public const string n_tran_negociado = "JUD: Negociar";
        public const string n_tran_juicio_celebrado = "JUD: Pdt. de fallo";
        public const string n_tran_fallado_positivo = "JUD: Fallo a favor";
        public const string n_tran_fallado_negativo = "JUD: Fallo en contra";


        private static void Transiciones(ContextoSe contexto)
        {
            //Negociar: Abierto --> en negociación
            new TransicionesDeUnPleitoDtm { 
                Nombre = n_tran_negociar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_abierto).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_en_negociacion).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = "",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Demandar: en negociación --> demandado
            new TransicionesDeUnPleitoDtm
            {
                Nombre = n_tran_demandar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_en_negociacion).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_demandado).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Motivo del no acuerdo",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Acordar: en negociación --> acordado
            new TransicionesDeUnPleitoDtm
            {
                Nombre = n_tran_acordar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_en_negociacion).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_acordado).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Reseña del acuerdo",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Pdt. de Fallo: demandado --> pdt de fallo
            new TransicionesDeUnPleitoDtm
            {
                Nombre = n_tran_juicio_celebrado,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_demandado).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_pdt_de_fallo).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Reseña del juicio",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Acordar: demandado --> acordado
            new TransicionesDeUnPleitoDtm
            {
                Nombre = n_tran_acordar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_demandado).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_acordado).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Reseña del acuerdo",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Ganado: pdt de fallo --> cerrado
            new TransicionesDeUnPleitoDtm
            {
                Nombre = n_tran_fallado_positivo,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_pdt_de_fallo).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_cerrado).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = "",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Perdido: pdt de fallo --> perdido
            new TransicionesDeUnPleitoDtm
            {
                Nombre = n_tran_fallado_positivo,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_pdt_de_fallo).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_perdido).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = "",
                Activo = true
            }.PersistirTransicionPorAk(contexto);
        }

        public const string n_tipo_deuda = "RDD: Reclamación de deuda";
        public const string n_tipo_divorcio = "CIV: Divorcio";
        public const string n_tipo_rdd_sigla = "RDD";
        public const string n_tipo_div_sigla = "DIV";
        private static void Tipos(ContextoSe contexto)
        {
            new TipoDePleitoDtm
            {
                Nombre = n_tipo_deuda,
                ClaseDePleito = enumClaseDePleito.recobro,
                ClaseDeLibro = enumClaseDeLibro.POR_CG_TIPO,
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_abierto).Id,
                IdPadre = null,
                Activo=true,
                TipoDtm = typeof(TipoDePleitoDtm).FullName,
                TipoDto = typeof(TipoDePleitoDto).FullName,
                Sigla = n_tipo_rdd_sigla
            }.PersistirPorSigla(contexto);


            new TipoDePleitoDtm
            {
                Nombre = n_tipo_divorcio,
                ClaseDePleito = enumClaseDePleito.familia,
                ClaseDeLibro = enumClaseDeLibro.POR_CG_TIPO,
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnPleitoDtm>(n_estado_abierto).Id,
                IdPadre = null,
                Activo = true,
                TipoDtm = typeof(TipoDePleitoDtm).FullName,
                TipoDto = typeof(TipoDePleitoDto).FullName,
                Sigla = n_tipo_div_sigla
            }.PersistirPorSigla(contexto);
        }

    }
}
