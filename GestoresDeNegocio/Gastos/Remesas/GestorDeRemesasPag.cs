using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ModeloDeDto.Gastos;
using Gestor.Errores;
using ServicioDeDatos.Terceros;
using System;
using GestorDeElementos.Extensores;
using Microsoft.EntityFrameworkCore;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.SistemaDocumental;
using System.IO;
using ServicioXml;
using ServicioDeDatos.SistemaDocumental;

namespace GestoresDeNegocio.Gastos
{

    public class GestorDeRemesasPag : GestorDeElementos<ContextoSe, RemesaPagDtm, RemesaPagDto>
    {
        public class MapearRemesaPag : Profile
        {
            public MapearRemesaPag()
            {
                CreateMap<RemesaPagDtm, RemesaPagDto>()
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Expresion))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado.Nombre))
                .ForMember(dto => dto.CuentaDePago, dtm => dtm.MapFrom(dtm => dtm.CuentaDePago.Cuenta.NumeroIban));

                CreateMap<RemesaPagDto, RemesaPagDtm>()
                .ForMember(dtm => dtm.CuentaDePago, dto => dto.Ignore())
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.RemesaPag;

        //public override TiposDelTipoDeElemento TiposDelTipo => Negocio.TiposDelTipo();

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeRemesaPag.Gestor(Contexto, Contexto.Mapeador);

        public GestorDeRemesasPag(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeRemesasPag Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeRemesasPag(contexto, mapeador);
        }

        protected override IQueryable<RemesaPagDtm> AplicarJoins(IQueryable<RemesaPagDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.CuentaDePago).ThenInclude(y => y.Cuenta);
            return consulta;
        }

        protected override IQueryable<RemesaPagDtm> AplicarOrden(IQueryable<RemesaPagDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override IQueryable<RemesaPagDtm> AplicarFiltros(IQueryable<RemesaPagDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {

            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroPorAcreedor(Contexto, filtros);
            consulta = consulta.FiltroParaBuscarRemesasDeUnPago(Contexto, filtros);
            consulta = consulta.FiltroParaBuscarRemesasDeUnaFactura(Contexto, filtros);
            consulta = consulta.FiltroPorFechaDeGeneracion(filtros);
            consulta = consulta.FiltroPorFechaDePago(filtros);
            consulta = consulta.FiltroPorImporteDeRemesa(Contexto, filtros);
            return consulta;
        }

        protected override IQueryable<RemesaPagDtm> AplicarSeguridad(IQueryable<RemesaPagDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<RemesaPagDtm, TipoDeRemesaPagDtm, PermisoDeLaRemesaPagDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<RemesaPagDtm, PermisoDeLaRemesaPagDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void AntesDePersistir(RemesaPagDtm remesa, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(remesa, parametros);
            if (parametros.AccionQueSeEjecuta == ltrDeUnaRemesaPag.Accion_AsociarSepa)
                return;

            if (remesa.PagarEl.HasValue && remesa.PagadaEl is null && remesa.PagarEl < DateTime.Now.Date && !parametros.ProcesandoTransicion(remesa))
                GestorDeErrores.Emitir($"No puede indicar en la remesa '{remesa.Referencia}' una fecha de cuando pagar '{remesa.PagarEl.Fecha().ToShortDateString()}' anterior al día de hoy");

            if (remesa.PagarEl.HasValue && remesa.PagarEl.Fecha() != remesa.PagarEl.Fecha().Date)
                GestorDeErrores.Emitir($"No puede indicar hora en la fecha de cuando pagar remesa '{remesa.Referencia}'");

            if (remesa.PagadaEl.HasValue && remesa.PagadaEl.Fecha() != remesa.PagadaEl.Fecha().Date)
                GestorDeErrores.Emitir($"No puede indicar hora en la fecha de pago de la remesa '{remesa.Referencia}'");

            //Asigno nulo si inserto o retrocedo a cumplimentación, en el resto de casos lo que haya en BD
            remesa.IdArchivo = parametros.Insertando
                ? null
                : parametros.Transitando &&
                  enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion.Lista().Contains(parametros.Parametros.LeerValor<EstadoDtm>(nameof(ltrParametrosNeg.EstadoDestino)).Id)
                ? null
                : ((RemesaPagDtm)parametros.registroEnBd).IdArchivo;

            if (parametros.Insertando)
            {
                remesa.PresentadaEl = null;
                remesa.GeneradaEl = null;
                remesa.PagadaEl = null;
                remesa.Deudor = remesa.Sociedad(Contexto).Nombre;
                remesa.NifDelDeudor = remesa.Sociedad(Contexto).NIF;
                if (remesa.Presentador.IsNullOrEmpty()) remesa.Presentador = remesa.Deudor;
                if (remesa.NifDelPresentador.IsNullOrEmpty()) remesa.NifDelPresentador = remesa.NifDelDeudor;
                if (remesa.SufijoPresentador.IsNullOrEmpty()) remesa.SufijoPresentador = remesa.SufijoDeudor;
                var cuenta = Contexto.SeleccionarPorId<CuentaDeMiSociedadDtm>(remesa.IdCuentaDePago, aplicarJoin: true);
                remesa.Entidad = cuenta.Cuenta.Entidad;
                remesa.Oficina = cuenta.Cuenta.Oficina;
            }

            if (parametros.Modificando)
            {
                remesa.Deudor = ((RemesaPagDtm)parametros.registroEnBd).Deudor;
                remesa.NifDelDeudor = ((RemesaPagDtm)parametros.registroEnBd).NifDelDeudor;
            }

            if (parametros.Operacion == enumTipoOperacion.Modificar && !parametros.ProcesandoTransicion(remesa) && !remesa.EstaEnLaEtapa(enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion))
                if (!parametros.EstaEjecutandoUnaAccion)
                    GestorDeErrores.Emitir($"La remesa '{remesa.Referencia}' no está en la etapa de {enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion.Nombre(true)} y por tanto no es modificable");

            if (remesa.PropiedadCambiada<enumClaseDeRemesaPag>(nameof(RemesaPagDtm.Clase), parametros) && remesa.Detalles<PagoDeUnaRemesaDtm>(Contexto).Count() > 0)
                GestorDeErrores.Emitir($"No se puede modificar la clase de la remesa '{remesa.Referencia}' por tener ya pagos seleccionados");


            if (remesa.PropiedadCambiada<DateTime?>(nameof(RemesaPagDtm.PagadaEl), parametros))
            {
                if (((RemesaPagDtm)parametros.registroEnBd).PagadaEl.HasValue && remesa.PagadaEl.HasValue)
                    GestorDeErrores.Emitir($"Para modificar la fecha del pago anule la presentación de la remesa '{remesa.Referencia}' y vuélvalo a presentarla");

                var bEstaCerrando = parametros.Transitando && enumEtapasDeRemesasPag.REM_Etapa_De_Cierre.Lista().Contains(((EstadoDtm)parametros.Parametros[ltrParametrosNeg.EstadoDestino]).Id);

                if (!remesa.EstaEnLaEtapa(enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion) && !bEstaCerrando)
                    GestorDeErrores.Emitir($"Para dar por pagada o anular el pago de la remesa '{remesa.Referencia}' ha de estar en la etapa de '{enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion.Nombre(minusculas: true)}'");

                if (remesa.PagadaEl.HasValue)
                {
                    if (remesa.PagadaEl.Fecha() < remesa.GeneradaEl.Fecha().Date)
                        GestorDeErrores.Emitir($"La fecha del pago '{remesa.PagadaEl.Fecha().ToShortDateString()}' de la remesa '{remesa.Referencia}' no puede ser anterior a la de generación '{remesa.GeneradaEl.Fecha().ToShortDateString()}', y debería ser posterior a la presentación '{remesa.PresentadaEl.Fecha().ToShortDateString()}'");

                    if (remesa.PagadaEl.Fecha().Date > DateTime.Today)
                        GestorDeErrores.Emitir($"La fecha del pago '{remesa.PagadaEl.Fecha().ToShortDateString()}' de la remesa '{remesa.Referencia}' no puede ser mayor a la de hoy");
                }

                if (!remesa.EsInterventor(Contexto))
                    GestorDeErrores.Emitir($"Para pagar o anular el pago de la remesa '{remesa.Referencia}' en una fecha '{remesa.PagadaEl.Fecha().ToShortDateString()}' diferente a la parametrizada '{remesa.PagarEl.Fecha().ToShortDateString()}' se necesita un perfil de intervención");
            }


            if (remesa.PropiedadCambiada<DateTime?>(nameof(RemesaPagDtm.PagarEl), parametros) && remesa.GeneradaEl.HasValue && !parametros.Transitando)
                GestorDeErrores.Emitir($"La remesa '{remesa.Referencia}' ya ha sido generada, no se le puede cambiar la fecha de pago, devuélvala a pendiente, corrija la fecha de pago y vuelva a generarla");
        }

        protected override void DespuesDePersistir(RemesaPagDtm remesa, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(remesa, parametros);

            if (remesa.PagarEl is not null)
            {
                if (parametros.Insertando)
                    remesa.CrearEventoDePago(Contexto);
                if (remesa.PropiedadCambiada<DateTime?>(nameof(RemesaPagDtm.PagarEl), parametros))
                    remesa.PersistirEventoDePago(Contexto);
            }
            else remesa.EliminarEventoDePago(Contexto);

            if (remesa.PropiedadCambiada<DateTime?>(nameof(RemesaPagDtm.PagarEl), parametros))
            {
                var fechaAnterior = ((RemesaPagDtm)parametros.registroEnBd).PagarEl;
                remesa.ActualizarPagos(Contexto);
                if (fechaAnterior is not null && remesa.PagarEl is not null)
                    remesa.CrearTraza(Contexto, "Actualización de fecha de pago",
                       $"El usuario '{Contexto.DatosDeConexion.Login}' ha modificado la fecha de pago '{fechaAnterior.Fecha().ToShortDateString()}' por '{remesa.PagarEl.Fecha().ToShortDateString()}'");

                if (fechaAnterior is not null && remesa.PagarEl is null)
                    remesa.CrearTraza(Contexto, "Eliminación de fecha de pago",
                       $"El usuario '{Contexto.DatosDeConexion.Login}' ha eliminado la fecha de pago '{fechaAnterior.Fecha().ToShortDateString()}'");
            }

            if (remesa.PropiedadCambiada<DateTime?>(nameof(RemesaPagDtm.PagadaEl), parametros))
            {
                if (remesa.PagadaEl is null) 
                    remesa.RetrocederPago(Contexto, ((RemesaPagDtm)parametros.registroEnBd).PagadaEl.Fecha());
                if (remesa.PagadaEl is not null)
                {
                    remesa.Pagar(Contexto, remesa.PagadaEl.Fecha(), parametros.AccionQueSeEjecuta == ltrDeUnaRemesaPag.Accion_PagoDeRemesa
                                                         ? VariableDePagos.enumMotivoTransicion.PagarRemesa
                                                         : VariableDePagos.enumMotivoTransicion.CerrarRemesa);
                }
            }
        }

        protected override RemesaPagDtm AntesDeTransitar(RemesaPagDtm remesa, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            remesa = base.AntesDeTransitar(remesa, transicion, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion.Estados(), enumEtapasDeRemesasPag.REM_Etapa_Cancelada.Estados()))
                remesa.AntesDeCancelar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion.Estados(), enumEtapasDeRemesasPag.REM_Etapa_Generada.Estados()))
                remesa.AntesDeGenerar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasPag.REM_Etapa_Generada.Estados(), enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion.Estados()))
                remesa.AntesDePresentar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion.Estados(), enumEtapasDeRemesasPag.REM_Etapa_De_Cierre.Estados()))
                remesa.AntesDeDarPorPagada(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasPag.REM_Etapa_Generada.Estados(), enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion.Estados()))
                remesa = remesa.AntesDeAnularGeneracion(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion.Estados(), enumEtapasDeRemesasPag.REM_Etapa_Generada.Estados()))
                remesa = remesa.AntesDeAnularPresentacion(Contexto, parametros);


            return remesa;
        }

        protected override RemesaPagDtm DespuesDeTransitar(RemesaPagDtm remesa, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            remesa = base.DespuesDeTransitar(remesa, transicion, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion.Estados(), enumEtapasDeRemesasPag.REM_Etapa_Generada.Estados()))
                GenerarSepaQ14(Contexto, remesa);

            if (transicion.EntreEtapas(enumEtapasDeRemesasPag.REM_Etapa_Generada.Estados(), enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion.Estados()))
                remesa.DespuesDeAnularGeneracion(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion.Estados(), enumEtapasDeRemesasPag.REM_Etapa_Generada.Estados()))
                remesa.DespuesDeAnularPresentacion(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion.Estados(), enumEtapasDeRemesasPag.REM_Etapa_De_Cierre.Estados()))
                remesa.EliminarEventoDePago(Contexto);

            return remesa;
        }

        protected override void DespuesDeMapearElElemento(RemesaPagDtm remesa, RemesaPagDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(remesa, elemento, parametros);
            elemento.ImporteRemesa = remesa.Total(Contexto);
            elemento.Pagado = remesa.Pagado(Contexto);
            elemento.Incluidos = remesa.Detalles<PagoDeUnaRemesaDtm>(Contexto).Count();
            elemento.Etapas = remesa.ListaDeEtapas();
        }

        private void GenerarSepaQ14(ContextoSe contexto, RemesaPagDtm remesa)
        {
            var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, $"Rem-{remesa.Referencia}.xml".NormalizarFichero());
            remesa.GenerarSepaQ14(contexto, rutaConFichero);
            var idArchivo = ServidorDocumental.SubirArchivo(contexto, rutaConFichero, sanitizar: false);
            remesa.AsociarArchivo(contexto, idArchivo, ltrDeUnaRemesaPag.Accion_AsociarSepa);
        }

        public static void AntesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var idRemesa = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var remesa = entorno.Contexto.SeleccionarPorId<RemesaPagDtm>(idRemesa);

            if (vinculado == enumNegocio.Archivos)
            {
                if (remesa.IdArchivo.Entero() == entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado)))
                {
                    var archivo = entorno.Contexto.SeleccionarPorId<ArchivoDtm>(remesa.IdArchivo.Entero());
                    GestorDeErrores.Emitir($"No puede quitar de la {enumNegocio.RemesaPag.Singular(true)} '{remesa.Referencia}' el {enumNegocio.Archivos.Singular(true)} '{archivo.Nombre}' por ser el original de la emisión");
                }
            }
        }

        public static void AdelantarPago(ContextoSe contexto, int idRemesa, DateTime pagadaEl)
        {
            var remesa = contexto.SeleccionarPorId<RemesaPagDtm>(idRemesa);
            if (remesa.PagadaEl.HasValue)
                GestorDeErrores.Emitir($"La remesa '{remesa.Referencia}' se pagó el día '{remesa.PagadaEl.Fecha().ToShortDateString()}'");


            var tran = contexto.IniciarTransaccion();
            try
            {
                remesa.PagadaEl = pagadaEl;
                remesa.Modificar(contexto, accionEjecutada: ltrDeUnaRemesaPag.Accion_PagoDeRemesa);
                remesa.EliminarEventoDePago(contexto);
                remesa.CrearTraza(contexto, "Pago adelantado", $"El usuario '{contexto.DatosDeConexion.Login}' ha adelantado el pago fijado el día '{remesa.PagarEl.Fecha().ToShortDateString()}' al día '{remesa.PagadaEl.Fecha().ToShortDateString()}'");
                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran, e);
                throw;
            }
        }

        public static PagarRemesaDto InformacionDePagar(ContextoSe contexto, int idRemesa)
        {
            contexto.IniciarTraza(nameof(InformacionDePagar));
            try
            {
                var remesa = contexto.SeleccionarPorId<RemesaPagDtm>(idRemesa);
                if (remesa.PagadaEl.HasValue)
                    GestorDeErrores.Emitir($"La remesa '{remesa.Referencia}' tiene fecha de pago '{remesa.PagadaEl.Fecha().ToShortDateString()}', si quiere modificar la fecha de pago, anúlela, y vuélvala a pagar");

                if (!remesa.EsInterventor(contexto))
                    GestorDeErrores.Emitir($"Para pagar la remesa '{remesa.Referencia}' manualmente, necesita permisos de intervención");

                var info = new PagarRemesaDto
                {
                    IdElemento = idRemesa,
                    Elemento = remesa.Expresion,
                    PagarEl = remesa.PagarEl,
                    PagadaEl = null,
                    Incluidos = remesa.Detalles<PagoDeUnaRemesaDtm>(contexto).Count(),
                    Anulados = remesa.Detalles<PagoDeUnaRemesaDtm>(contexto).Where(x => x.AnuladoEl != null).Count(),
                    ImporteRemesa = remesa.Total(contexto),
                    Pagado = remesa.Pagado(contexto),
                };
                contexto.CerrarTraza();
                return info;
            }
            catch (Exception e)
            {
                contexto.CerrarTraza(e.Message);
                throw;
            }
        }

        public static void RetrocederPago(ContextoSe contexto, int idRemesa)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                var remesa = contexto.SeleccionarPorId<RemesaPagDtm>(idRemesa);
                remesa.PagadaEl = null;
                remesa.Modificar(contexto, accionEjecutada: ltrDeUnaRemesaPag.Accion_AnularPagoDeRemesa);
                remesa.PersistirEventoDePago(contexto);
                remesa.CrearTraza(contexto, "Pago retrocedido", $"El usuario '{contexto.DatosDeConexion.Login}' ha retrocedido el pago de la remesa realizada el '{remesa.PagarEl.Fecha().ToShortDateString()}'");
                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran, e);
                throw;
            }
        }

        public static RetrocederPagoDto InformacionDeRetrocederPago(ContextoSe contexto, int idRemesa)
        {
            contexto.IniciarTraza(nameof(InformacionDeRetrocederPago));
            try
            {
                var remesa = contexto.SeleccionarPorId<RemesaPagDtm>(idRemesa);
                if (!remesa.PagadaEl.HasValue)
                    GestorDeErrores.Emitir($"La remesa '{remesa.Referencia}' aun no ha sido pagada, tiene fecha de planificación de pago '{remesa.PagarEl.Fecha().ToShortDateString()}', no se puede anular su pago");

                if (!remesa.EsInterventor(contexto))
                    GestorDeErrores.Emitir($"Para anular el pago de la remesa '{remesa.Referencia}' manualmente, necesita permisos de intervención");

                var info = new RetrocederPagoDto
                {
                    IdElemento = idRemesa,
                    Elemento = remesa.Expresion,
                    PagarEl = remesa.PagarEl,
                    PagadaEl = remesa.PagadaEl,
                    Incluidos = remesa.Detalles<PagoDeUnaRemesaDtm>(contexto).Count(),
                    Anulados = remesa.Detalles<PagoDeUnaRemesaDtm>(contexto).Where(x => x.AnuladoEl != null).Count(),
                    ImporteRemesa = remesa.Total(contexto),
                    Pagado = remesa.Pagado(contexto),
                };
                contexto.CerrarTraza();
                return info;
            }
            catch (Exception e)
            {
                contexto.CerrarTraza(e.Message);
                throw;
            }
        }

    }

}
