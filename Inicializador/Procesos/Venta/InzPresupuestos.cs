using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Presupuesto;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Ventas;
using Utilidades;

namespace Inicializador.Presupuestos
{
    public static class InzPresupuestos
    {

        public static void ModeloDePresupuestos(ContextoSe contexto)
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

        public const string n_estado_abierto = "PVT: Abierto";
        public const string n_estado_pdt_aceptar = "PVT: Pdt de aceptar";
        public const string n_estado_ejecucion = "PVT: En ejecución";
        public const string n_estado_cerrado = "PVT: Cerrado";
        public const string n_estado_rechazado = "PVT: Rechazado";
        public const string n_estado_cancelado = "PVT: Cancelado";

        public const string n_estado_pgt_abierto = "PGT: Abierto";
        public const string n_estado_pgt_cerrado = "PGT: Cerrado";

        public const string n_estado_inv_abierto = "PIN: Abierto";
        public const string n_estado_inv_cerrado = "PIN: Cerrado";


        private static void Estados(ContextoSe contexto)
        {
            new EstadoDeUnPresupuestoDtm { Nombre = n_estado_abierto, Inicial = true, Terminado = false, Cancelado = false, Orden = 10 }.PersistirEstado(contexto);
            new EstadoDeUnPresupuestoDtm { Nombre = n_estado_pdt_aceptar, Inicial = false, Terminado = false, Cancelado = false, Orden = 20 }.PersistirEstado(contexto);
            new EstadoDeUnPresupuestoDtm { Nombre = n_estado_ejecucion, Inicial = false, Terminado = false, Cancelado = false, Orden = 30 }.PersistirEstado(contexto);
            new EstadoDeUnPresupuestoDtm { Nombre = n_estado_rechazado, Inicial = false, Terminado = true, Cancelado = false, Orden = 35 }.PersistirEstado(contexto);
            new EstadoDeUnPresupuestoDtm { Nombre = n_estado_cerrado, Inicial = false, Terminado = true, Cancelado = false, Orden = 40 }.PersistirEstado(contexto);
            new EstadoDeUnPresupuestoDtm { Nombre = n_estado_cancelado, Inicial = false, Terminado = false, Cancelado = true, Orden = 50 }.PersistirEstado(contexto);

            new EstadoDeUnPresupuestoDtm { Nombre = n_estado_pgt_abierto, Inicial = true, Terminado = false, Cancelado = false, Orden = 10 }.PersistirEstado(contexto);
            new EstadoDeUnPresupuestoDtm { Nombre = n_estado_pgt_cerrado, Inicial = false, Terminado = true, Cancelado = false, Orden = 30 }.PersistirEstado(contexto);

            new EstadoDeUnPresupuestoDtm { Nombre = n_estado_inv_abierto, Inicial = true, Terminado = false, Cancelado = false, Orden = 10 }.PersistirEstado(contexto);
            new EstadoDeUnPresupuestoDtm { Nombre = n_estado_inv_cerrado, Inicial = false, Terminado = true, Cancelado = false, Orden = 30 }.PersistirEstado(contexto);

        }

        public const string n_tran_entregar = "PVT: Entregar al cliente";
        public const string n_tran_iniciar = "PVT: Iniciar";
        public const string n_tran_rechazar = "PVT: Rechazar";
        public const string n_tran_cerrar = "PVT: Cerrar";
        public const string n_tran_reabrir = "PVT: Reabrir";
        public const string n_tran_cancelar = "PVT: Cancelar";
        public const string n_tran_devolver = "PVT: Devolver";
        public const string n_tran_no_ejecutar = "PVT: No ejecutar";

        public const string n_tran_inv_cerrar = "PIN: Cerrar";
        public const string n_tran_gto_cerrar = "PGT: Cerrar";

        private static void Transiciones(ContextoSe contexto)
        {
            //Cerrar inversión: Abierto --> cerrado
            new TransicionesDeUnPresupuestoDtm
            {
                Nombre = n_tran_inv_cerrar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_inv_abierto).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_inv_cerrado).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = "",
                Activo = true
            }.PersistirTransicionPorAk(contexto);


            //Cerrar gasto: Abierto --> cerrado
            new TransicionesDeUnPresupuestoDtm
            {
                Nombre = n_tran_gto_cerrar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_pgt_abierto).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_pgt_cerrado).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = "",
                Activo = true
            }.PersistirTransicionPorAk(contexto);


            //Entregar: Abierto --> pdt de aceptar
            new TransicionesDeUnPresupuestoDtm { 
                Nombre = n_tran_entregar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_abierto).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_pdt_aceptar).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = "",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Cancelar: Abierto --> cancelado
            new TransicionesDeUnPresupuestoDtm
            {
                Nombre = n_tran_cancelar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_abierto).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_cancelado).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Motivo de cancelación",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Iniciar: pdt de aceptar a en ejecución
            new TransicionesDeUnPresupuestoDtm
            {
                Nombre = n_tran_iniciar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_pdt_aceptar).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_ejecucion).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = "",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Rechazar: pdt de aceptar a rechazado
            new TransicionesDeUnPresupuestoDtm
            {
                Nombre = n_tran_rechazar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_pdt_aceptar).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_rechazado).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Reseña del acuerdo",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Cerrar:En ejecución a cerrado
            new TransicionesDeUnPresupuestoDtm
            {
                Nombre = n_tran_cerrar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_ejecucion).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_cerrado).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = "",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Reabrir:Cerrado a En Ejecución
            new TransicionesDeUnPresupuestoDtm
            {
                Nombre = n_tran_reabrir,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_cerrado).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_ejecucion).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Motivo de reapertura",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Devolver: En ejecución --> Abierto
            new TransicionesDeUnPresupuestoDtm
            {
                Nombre = n_tran_devolver,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_ejecucion).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_abierto).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Motivo de devolución",
                Activo = true
            }.PersistirTransicionPorAk(contexto);


            //Devolver: En ejecución --> rechazado
            new TransicionesDeUnPresupuestoDtm
            {
                Nombre = n_tran_no_ejecutar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_ejecucion).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_rechazado).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Motivo de anulación de ejecución",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

        }

        public const string n_tipo_venta = "PVT: Venta";
        public const string n_tipo_gasto = "PGT: Gasto";
        public const string n_tipo_inversión = "PIV Inversión";
        private static void Tipos(ContextoSe contexto)
        {
            new TipoDePresupuestoDtm
            {
                Nombre = n_tipo_venta,
                ClaseDePresupuesto = enumClaseDePresupuesto.venta,
                ClaseDeLibro = enumClaseDeLibro.POR_CG_TIPO,
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_abierto).Id,
                IdPadre = null,
                Activo=true,
                TipoDtm = typeof(TipoDePresupuestoDtm).FullName,
                TipoDto = typeof(TipoDePresupuestoDto).FullName,
                Sigla = "PVT"
            }.PersistirPorNombre(contexto);


            new TipoDePresupuestoDtm
            {
                Nombre = n_tipo_inversión,
                ClaseDePresupuesto = enumClaseDePresupuesto.gasto,
                ClaseDeLibro = enumClaseDeLibro.POR_CG_TIPO,
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_inv_abierto).Id,
                IdPadre = null,
                Activo = true,
                TipoDtm = typeof(TipoDePresupuestoDtm).FullName,
                TipoDto = typeof(TipoDePresupuestoDto).FullName,
                Sigla = "PIN"
            }.PersistirPorNombre(contexto);

            new TipoDePresupuestoDtm
            {
                Nombre = n_tipo_gasto,
                ClaseDePresupuesto = enumClaseDePresupuesto.gasto,
                ClaseDeLibro = enumClaseDeLibro.POR_CG_TIPO,
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_pgt_abierto).Id,
                IdPadre = null,
                Activo = true,
                TipoDtm = typeof(TipoDePresupuestoDtm).FullName,
                TipoDto = typeof(TipoDePresupuestoDto).FullName,
                Sigla = "PGT"
            }.PersistirPorNombre(contexto);
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var etapaDeElaboracion = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_abierto).Id.ToString() + ","
                                 + contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_inv_abierto).Id.ToString() + ","
                                 + contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_pgt_abierto).Id.ToString();

            enumNegocio.Presupuesto.IncluirEnParametro(contexto, enumEtapasDePpts.PPT_Etapa_Elaboracion, etapaDeElaboracion);


            var etapaDePendiente = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_pdt_aceptar).Id.ToString();
            enumNegocio.Presupuesto.IncluirEnParametro(contexto, enumEtapasDePpts.PPT_Etapa_Pendiente, etapaDePendiente);

            var etapaDeAceptado = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_ejecucion).Id.ToString() + ","
                                + contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_cerrado).Id.ToString() + ","
                                + contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_inv_cerrado).Id.ToString() + ","
                                + contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_pgt_cerrado).Id.ToString();
            enumNegocio.Presupuesto.IncluirEnParametro(contexto, enumEtapasDePpts.PPT_Etapa_Aceptado, etapaDeAceptado);

            var etapaDePdtFacturar = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_ejecucion).Id.ToString();
            enumNegocio.Presupuesto.IncluirEnParametro(contexto, enumEtapasDePpts.PPT_Etapa_PermiteFacturar, etapaDePdtFacturar);

            var etapaDePdtParteTr = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_ejecucion).Id.ToString();
            enumNegocio.Presupuesto.IncluirEnParametro(contexto, enumEtapasDePpts.PPT_Etapa_AsociarParteTr, etapaDePdtParteTr);

            var etapaDeRechazado = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_rechazado).Id.ToString();
            enumNegocio.Presupuesto.IncluirEnParametro(contexto, enumEtapasDePpts.PPT_Etapa_Rechazo, etapaDeRechazado);

            var etapaDeCancelado = contexto.SeleccionarEstado<EstadoDeUnPresupuestoDtm>(n_estado_cancelado).Id.ToString();
            enumNegocio.Presupuesto.IncluirEnParametro(contexto, enumEtapasDePpts.PPT_Etapa_Cancelado, etapaDeCancelado);


        }
    }
}

/*
declare @idAceptado int
declare @idEnEjecucion int

declare @idAceptar int
declare @idIniciar int

set @idAceptado = (select id from PRESUPUESTO.PRESUPUESTO_ESTADO where nombre like 'PVT: Aceptado')
set @idEnEjecucion = (select id from PRESUPUESTO.PRESUPUESTO_ESTADO where nombre like 'PVT: En ejecución')


set @idAceptar = (select id from PRESUPUESTO.PRESUPUESTO_TRANSICION where nombre like 'PVT: Aceptar')
set @idAceptar = (select id from PRESUPUESTO.PRESUPUESTO_TRANSICION where nombre like 'PVT: Iniciar')

update PRESUPUESTO.PRESUPUESTO_HISTORIA set ID_ESTADO = @idEnEjecucion where ID_ESTADO = @idAceptado
update PRESUPUESTO.PRESUPUESTO set ID_ESTADO = @idEnEjecucion where ID_ESTADO = @idAceptado
update PRESUPUESTO.PRESUPUESTO_HISTORIA set ID_TRANSICION = @idIniciar where ID_TRANSICION = @idAceptado
 * */