using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto.Presupuesto;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Ventas;
using Utilidades;

namespace Inicializador.Presupuestos
{
    public static class InzValoraciones
    {
        private static readonly string n_val = "VAL";

        public static void ModeloDeValoracion(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                DefinirEtapas(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }


        public static readonly string n_estado_abierto = $"{n_val}: Abierto";
        public static readonly string n_estado_cerrado = $"{n_val}: Cerrado";
        public static readonly string n_estado_cancelado = $"{n_val}: Cancelado";


        private static void Estados(ContextoSe contexto)
        {
            new EstadoDeUnPresupuestoDtm { Nombre = n_estado_abierto, Inicial = true, Orden = 10 }.PersistirEstado(contexto);
            new EstadoDeUnPresupuestoDtm { Nombre = n_estado_cerrado, Terminado = true, Orden = 40 }.PersistirEstado(contexto);
            new EstadoDeUnPresupuestoDtm { Nombre = n_estado_cancelado, Cancelado = true, Orden = 50 }.PersistirEstado(contexto);

        }

        public static readonly string n_tran_cerrar = $"{n_val}: Cerrar";
        public static readonly string n_tran_cancelar = $"{n_val}: Cancelar";
        public static readonly string n_tran_reabrir = $"{n_val}: Reabrir";

        private static void Transiciones(ContextoSe contexto)
        {
            //Cerrar valoración: Abierto --> cerrado
            new TransicionesDeUnPresupuestoDtm
            {
                Nombre = n_tran_cerrar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_abierto).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_cerrado).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = "",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            new TransicionesDeUnPresupuestoDtm
            {
                Nombre = n_tran_cerrar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_cerrado).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_abierto).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Motivo de reapertura",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Cancelar valoración
            new TransicionesDeUnPresupuestoDtm
            {
                Nombre = n_tran_cancelar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_abierto).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_cancelado).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = "",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

        }

        public static readonly string n_tipo_valoracion = $"{n_val}: Sprint";
        private static void Tipos(ContextoSe contexto)
        {
            new TipoDePresupuestoDtm
            {
                Nombre = n_tipo_valoracion,
                ClaseDePresupuesto = enumClaseDePresupuesto.venta,
                ClaseDeLibro = enumClaseDeLibro.POR_CG_TIPO,
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_abierto).Id,
                IdPadre = null,
                Activo = true,
                TipoDtm = typeof(TipoDePresupuestoDtm).FullName,
                TipoDto = typeof(TipoDePresupuestoDto).FullName,
                Sigla = n_val,
                IdTipoFacturaEmt = contexto.Set<TipoDeFacturaEmtDtm>().First().Id
            }.PersistirPorNombre(contexto);
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var etapaDeElaboracion = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_abierto).Id.ToString();
            enumNegocio.Presupuesto.IncluirEnParametro(contexto, enumEtapasDePpts.PPT_Etapa_Elaboracion, etapaDeElaboracion);

            var etapaDeAceptado = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_cerrado).Id.ToString() ;
            enumNegocio.Presupuesto.IncluirEnParametro(contexto, enumEtapasDePpts.PPT_Etapa_Aceptado, etapaDeAceptado);

            var etapaDeCancelado = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_cancelado).Id.ToString();
            enumNegocio.Presupuesto.IncluirEnParametro(contexto, enumEtapasDePpts.PPT_Etapa_Cancelado, etapaDeCancelado);


            enumNegocio.Presupuesto.IncluirEnParametro(contexto, enumEtapasDePpts.PPT_Etapa_Pendiente, etapaDeElaboracion);
            enumNegocio.Presupuesto.IncluirEnParametro(contexto, enumEtapasDePpts.PPT_Etapa_PermiteFacturar, etapaDeElaboracion);
            enumNegocio.Presupuesto.IncluirEnParametro(contexto, enumEtapasDePpts.PPT_Etapa_AsociarParteTr, etapaDeElaboracion);
            enumNegocio.Presupuesto.IncluirEnParametro(contexto, enumEtapasDePpts.PPT_Etapa_Rechazo, ltrEstados.EstadoNulo);


        }
    }
}

/*
declare @idAceptado int
declare @idEnEjecucion int

declare @idAceptar int
declare @idIniciar int

set @idAceptado = (select id from PRESUPUESTO.PRESUPUESTO_ESTADO where nombre like 'VAL: Aceptado')
set @idEnEjecucion = (select id from PRESUPUESTO.PRESUPUESTO_ESTADO where nombre like 'VAL: En ejecución')


set @idAceptar = (select id from PRESUPUESTO.PRESUPUESTO_TRANSICION where nombre like 'VAL: Aceptar')
set @idAceptar = (select id from PRESUPUESTO.PRESUPUESTO_TRANSICION where nombre like 'VAL: Iniciar')

update PRESUPUESTO.PRESUPUESTO_HISTORIA set ID_ESTADO = @idEnEjecucion where ID_ESTADO = @idAceptado
update PRESUPUESTO.PRESUPUESTO set ID_ESTADO = @idEnEjecucion where ID_ESTADO = @idAceptado
update PRESUPUESTO.PRESUPUESTO_HISTORIA set ID_TRANSICION = @idIniciar where ID_TRANSICION = @idAceptado
 * */