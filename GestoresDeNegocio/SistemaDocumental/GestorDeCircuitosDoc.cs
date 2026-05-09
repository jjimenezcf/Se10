using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Contabilidad;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.SistemaDocumental
{

    public class GestorDeCircuitosDoc : GestorDeElementos<ContextoSe, CircuitoDocDtm, CircuitoDocDto>
    {
        public class MapearCircuitoDoc : Profile
        {
            public MapearCircuitoDoc()
            {
                CreateMap<CircuitoDocDtm, CircuitoDocDto>()
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Expresion))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado.Nombre));

                CreateMap<CircuitoDocDto, CircuitoDocDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.CircuitoDoc;

        //public override TiposDelTipoDeElemento TiposDelTipo => Negocio.TiposDelTipo();

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeCircuitoDoc.Gestor(Contexto, Contexto.Mapeador);

        public GestorDeCircuitosDoc(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeCircuitosDoc Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCircuitosDoc(contexto, mapeador);
        }


        protected override IQueryable<CircuitoDocDtm> AplicarOrden(IQueryable<CircuitoDocDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override IQueryable<CircuitoDocDtm> AplicarFiltros(IQueryable<CircuitoDocDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroPorExpedientePadre(Contexto, filtros);
            consulta = consulta.FiltroPorLoteContable(Contexto, filtros);
            consulta = consulta.FiltroPorEstimacion(Contexto, filtros);
            consulta = consulta.FiltroPorFichada(Contexto, filtros);
            consulta = consulta.FiltroPorLotePreasiento(Contexto, filtros);
            consulta = consulta.FiltroPorActividadFormativa(Contexto, filtros);
            return consulta;
        }

        protected override IQueryable<CircuitoDocDtm> AplicarSeguridad(IQueryable<CircuitoDocDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<CircuitoDocDtm, TipoDeCircuitoDocDtm, PermisoDelCircuitoDocDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<CircuitoDocDtm, PermisoDelCircuitoDocDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void AntesDeMapearElRegistroParaInsertar(CircuitoDocDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(elemento, opciones);

        }

        protected override void AntesDePersistir(CircuitoDocDtm cad, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(cad, parametros);
            //if (!parametros.Insertando)
            //{
            //    if (SemaforoDeProcesoSql.HaySemaforoPara(Contexto, Negocio.IdNegocio(), cad.Id, new List<enumOpercionesDeSemaforo> { enumOpercionesDeSemaforo.ALCT }))
            //    {
            //        var id = SemaforoDeProcesoSql.SemaforoPara(Contexto, Negocio.IdNegocio(), cad.Id, new List<enumOpercionesDeSemaforo> { enumOpercionesDeSemaforo.ALCT });
            //        if (id != parametros.Parametros.LeerValor(ltrParametrosNeg.IdSemaforo, 0))
            //            GestorDeErrores.Emitir($"El lote contable '{cad.Referencia}', no puede ser modificado por estar anulándose, inténtelo más tarde");
            //    }
            //}
            if (cad.EsUnaFichada(Contexto))
            {
                parametros.Parametros.Add(nameof(TrabajadorDtm), cad.ValidarFichada(Contexto));
            }
        }

        protected override void DespuesDePersistir(CircuitoDocDtm cad, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(cad, parametros);
            var trabajador = parametros.Parametros.LeerValor<TrabajadorDtm>(nameof(TrabajadorDtm), null);
            if (trabajador is not null)
            {
                GestorDeVinculos.Vincular(Contexto, trabajador, cad);
            }

            if (parametros.Insertando && cad.EsUnaActividadFormativa())
            {
                new DatosDeActividadFormativaDtm { IdElemento = cad.Id }.InsertarComoAdministrador(Contexto);
            }
        }

        protected override CircuitoDocDtm AntesDeTransitar(CircuitoDocDtm cad, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            cad = base.AntesDeTransitar(cad, transicion, parametros);

            if (transicion.EntreEtapas(enumEtapasDeCircuitosDoc.CAD_Etapa_Cerrado.Estados(), enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado.Estados()))
            {
                if (cad.EsLoteDePreasientos())
                {
                    cad.CrearTraza(Contexto, "Anulación de contabilización", $"Se ha anulado la contabilización del circuito '{cad.Referencia}' ya contabilizado, y por tanto se han desasociado sus preasientos");
                }
                else if (cad.EsEstimacionDirecta())
                {
                    cad.CrearTraza(Contexto, "Anulación de contabilización", $"Se ha anulado la contabilización del circuito '{cad.Referencia}' ya contabilizado, y por tanto las facturas emitidas, recibidas y pagos vuelven a estar pendientes de estimación directa");
                }
            }
            return cad;
        }

        protected override CircuitoDocDtm DespuesDeTransitar(CircuitoDocDtm cad, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            cad = base.DespuesDeTransitar(cad, transicion, parametros);
            if (transicion.DestinoEstaEnLaEtapa(enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado.Estados()))
                cad.TrasCancelarCad(Contexto);
            return cad;
        }

        protected override void DespuesDeMapearElElemento(CircuitoDocDtm cad, CircuitoDocDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(cad, elemento, parametros);

            elemento.EsFichada = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaFichada(errorSiNoEstaDefinido: false) == cad.IdTipo;
            elemento.EsLoteContable = (bool)elemento.EsFichada ? false : VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaLoteDePreasientos(errorSiNoEstaDefinido: false) == cad.IdTipo;
            elemento.EsEstimacionDirecta = (bool)elemento.EsFichada || (bool)elemento.EsLoteContable ? false : VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaEstimacionDirecta(errorSiNoEstaDefinido: false) == cad.IdTipo;
        }

        public void RegenerarLoteContable(ContextoSe contexto, int idLote)
        {
            var loteContable = contexto.SeleccionarPorId<CircuitoDocDtm>(idLote);
            if (VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaLoteDePreasientos(errorSiNoEstaDefinido: false) != loteContable.IdTipo)
                GestorDeErrores.Emitir($"El circuito '{loteContable.Referencia}' no es un lote contable, no se puede regenerar el lote contable.");

            if (!loteContable.EstaEnLaEtapa(enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto))
                GestorDeErrores.Emitir($"El circuito '{loteContable.Referencia}' no está abierto, no se puede regenerar.");

            if (!loteContable.EsAdministrador(Contexto))
                GestorDeErrores.Emitir($"Ha de ser administrador del lote contable '{loteContable.Referencia}' para poder regenerarlo");

            var regenerar = PrepararAccionParaRegenerarLoteContable(loteContable);

            var trans = Contexto.IniciarTransaccion();
            try
            {
                EntornoDeUnaAccion.EjecutarAccion(regenerar);
                Contexto.Commit(trans);
            }
            catch (Exception e)
            {
                Contexto.Rollback(trans);
                GestorDeErrores.Emitir(error: e.InnerException?.Message ?? e.Message);
            }
            finally
            {
                regenerar.Contexto.Accion = null;
                VaciarCacheDeRegistro(loteContable, enumTipoOperacion.Modificar, loteContable.Nombre);
            }


        }

        private EntornoDeUnaAccion PrepararAccionParaRegenerarLoteContable(CircuitoDocDtm loteContable)
        {

            var sociedad = loteContable.Sociedad(Contexto);
            var comoRegenerarLote = sociedad.RegenerarLotePara();
            var metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, typeof(ExportacionesDePreasientos).FullName, comoRegenerarLote.Metodo);
            if (metodo == null)
                GestorDeErrores.Emitir($"No está definido el método {metodo} en la clase {typeof(ExportacionesDePreasientos).FullName}");

            var parametros = new Dictionary<string, object>
                        {
                            {ltrDeUnLoteContable.IdLoteContable, loteContable.Id},
                        };
            var regenerar = new EntornoDeUnaAccion(Contexto, enumNegocio.Preasiento, parametros);

            regenerar.Contexto.Accion = new AccionDtm
            {
                Dll = ApiDeEnsamblados.GestoresDeNegocio,
                Clase = typeof(ExportacionesDePreasientos).FullName,
                Metodo = comoRegenerarLote.Metodo,
                Nombre = comoRegenerarLote.Nombre,
                ClaseDeAccion = enumClaseDeAccion.DLL.ToString()
            };

            return regenerar;
        }
    }

}
