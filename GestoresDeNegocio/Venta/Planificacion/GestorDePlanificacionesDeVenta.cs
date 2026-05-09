using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ModeloDeDto.Ventas;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Ventas;
using GestorDeElementos.Extensores;
using static Gestor.Errores.GestorDeErrores;
using ServicioDeDatos.Seguridad;
using System;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Tarea;
using static ServicioDeDatos.Ventas.VariablesDePlfsDeVenta;

namespace GestoresDeNegocio.Ventas
{

    public class GestorDePlanificacionesDeVenta : GestorDeElementos<ContextoSe, PlanificacionDeVentaDtm, PlanificacionDeVentaDto>
    {
        public class MapearPlanificacionDeVenta : Profile
        {
            public MapearPlanificacionDeVenta()
            {
                CreateMap<PlanificacionDeVentaDtm, PlanificacionDeVentaDto>()
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Expresion))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado.Nombre))
                .ForMember(dto => dto.Cliente, dtm => dtm.MapFrom(dtm => dtm.Cliente.Expresion))
                .ForMember(dto => dto.Planificador, dtm => dtm.MapFrom(dtm => dtm.Planificador.Expresion))
                .ForMember(dto => dto.TipoDeFactura, dtm => dtm.MapFrom(dtm => dtm.TipoDeFactura.Nombre))
                .ForMember(dto => dto.TipoDeParte, dtm => dtm.MapFrom(dtm => dtm.TipoDeParte.Expresion))
                .ForMember(dto => dto.FacturaEmt, dtm => dtm.MapFrom(dtm => dtm.FacturaEmt.Expresion))
                .ForMember(dto => dto.Contrato, dtm => dtm.MapFrom(dtm => dtm.Contrato.Expresion))
                .ForMember(dto => dto.ParteDeTrabajo, dtm => dtm.MapFrom(dtm => dtm.ParteTr.Expresion));

                CreateMap<PlanificacionDeVentaDto, PlanificacionDeVentaDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore())
                .ForMember(dtm => dtm.Cliente, dto => dto.Ignore())
                .ForMember(dtm => dtm.Planificador, dto => dto.Ignore())
                .ForMember(dtm => dtm.TipoDeFactura, dto => dto.Ignore())
                .ForMember(dtm => dtm.TipoDeParte, dto => dto.Ignore())
                .ForMember(dtm => dtm.FacturaEmt, dto => dto.Ignore())
                .ForMember(dtm => dtm.ParteTr, dto => dto.Ignore())
                .ForMember(dtm => dtm.Contrato, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.PlanificacionDeVenta;

        //public override TiposDelTipoDeElemento TiposDelTipo => Negocio.TiposDelTipo();

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposPlanificacionDeVenta.Gestor(Contexto, Contexto.Mapeador);

        public GestorDePlanificacionesDeVenta(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDePlanificacionesDeVenta Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePlanificacionesDeVenta(contexto, mapeador);
        }

        protected override IQueryable<PlanificacionDeVentaDtm> AplicarJoins(IQueryable<PlanificacionDeVentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Cliente);
            consulta = consulta.Include(x => x.Contrato);
            consulta = consulta.Include(x => x.TipoDeFactura);
            consulta = consulta.Include(x => x.TipoDeParte);
            consulta = consulta.Include(x => x.FacturaEmt);
            consulta = consulta.Include(x => x.ParteTr);
            consulta = consulta.Include(x => x.Planificador);
            return consulta;
        }

        protected override IQueryable<PlanificacionDeVentaDtm> AplicarFiltros(IQueryable<PlanificacionDeVentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            filtros.OmitirFiltrosPorEstado(new List<string> { ltrDeUnaPlanificacionDeVenta.IdContrato, ltrDeUnaPlanificacionDeVenta.IdParteTr, ltrDeUnaPlanificacionDeVenta.IdFacturaEmt, ltrDeUnaPlanificacionDeVenta.IdPlanificador });
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
           // consulta = consulta.AplicarFiltroPorEtapa(filtros);
            foreach (var filtro in filtros.Where(x => !x.Aplicado))
            {
                consulta = consulta.AplicarFiltroPorUnitario(Contexto, filtro);
                consulta = consulta.AplicarFiltroPorPlanificador(filtro);
                consulta = consulta.AplicarFiltroPorContratos(filtro);
                consulta = consulta.AplicarFiltroPorPartesTr(filtro);
                consulta = consulta.AplicarFiltroPorFacturaEmt(filtro);
            }
            return consulta;
        }

        protected override IQueryable<PlanificacionDeVentaDtm> AplicarSeguridad(IQueryable<PlanificacionDeVentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<PlanificacionDeVentaDtm, TipoDePlanificacionDeVentaDtm, PermisoDeLaPlanificacionDeVentaDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<PlanificacionDeVentaDtm, PermisoDeLaPlanificacionDeVentaDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void AntesDePersistir(PlanificacionDeVentaDtm plv, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(plv, parametros);
            if (plv.IdPlanificador != default)
                plv.IdContrato = Contexto.SeleccionarPorId<PlanificadorDeVentaDtm>((int)plv.IdPlanificador).IdContrato;

            if (plv.IdContrato != default)
            {
                var contrato = Contexto.SeleccionarPorId<ContratoDtm>((int)plv.IdContrato);
                if (!contrato.EstaVigente())
                    Emitir($"No se puede {(parametros.Insertando ? "crear" : "modificar")} la planificación '{plv.Referencia}' sobre el contrato {contrato.Referencia} por no estar vigente");
                
                parametros.Parametros[nameof(ContratoDtm)] = contrato;
                parametros.Parametros[nameof(DatosDelContratoDtm)] = contrato.FechasDelContrato(Contexto);
                plv.IdCliente = contrato.Cliente(Contexto).Id;
            }

            plv = plv.InicializarDatosCliente(Contexto, parametros);
            ValidarAsociarPlanificador(plv, parametros);
            ValidarAsociarContrato(plv, parametros);
            ValidarAsociarDestinoGenerado(plv, parametros);
        }

        protected override void DespuesDePersistir(PlanificacionDeVentaDtm plv, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(plv, parametros);

            if (parametros.Insertando && plv.IdPlanificador.Entero() > 0)
            {
                plv.CopiarLineasDelPlanificador(Contexto);
                plv.CrearEventoDePlanificacionAlContrato(Contexto);
                plv.CrearEventoDePlanificacionALaSociedad(Contexto);
            }

            if (parametros.Modificando && plv.SeHaModificadoElCampo<DateTime>(x => x.Name == nameof(PlanificacionDeVentaDtm.EjecutarEl), parametros))
            {
                plv.ModificarEventosDePlanificacion(Contexto);
                plv.CrearTraza(Contexto, "Fecha de ejecución cambiada", $"El usuario {Contexto.DatosDeConexion.Login} ha modificadola fecha de ejecución del {plv.EjecutarEl} al {((PlanificacionDeVentaDtm)parametros.registroEnBd).EjecutarEl}");
            }
        }

        private void ValidarAsociarContrato(PlanificacionDeVentaDtm plf, ParametrosDeNegocio parametros)
        {
            bool cambiandoCtr = parametros.Modificando && plf.PropiedadCambiada<int?>((PlanificacionDeVentaDtm)parametros.registroEnBd, nameof(PlanificacionDeVentaDtm.IdContrato));
            if (!cambiandoCtr && plf.IdContrato == default)
                return;

            var ctr = ((ContratoDtm)parametros.Parametros[nameof(ContratoDtm)]);
            if (cambiandoCtr)
                Emitir($"No se puede cambiar el contrato '{ctr.Referencia}' de la planificación '{plf.Referencia}'");

            (DateTime Inicio, DateTime? Fin) datos = ((DateTime Inicio, DateTime? Fin)) parametros.Parametros[nameof(DatosDelContratoDtm)];
            if (datos.Inicio > plf.EjecutarEl.Date)
                Emitir($"la fecha de ejecución de la planificación '{plf.EjecutarEl.Date.ToShortDateString()}' no puede ser anterior a la del contrato '{datos.Inicio.ToShortDateString()}'");

            DateTime fin = datos.Fin != default ? (DateTime) datos.Fin : datos.Inicio.AddYears(1);

            if (fin < plf.EjecutarEl.Date)
                Emitir($"la fecha de ejecución de la planificación '{plf.EjecutarEl.Date.ToShortDateString()}' no puede ser posterior a '{fin.ToShortDateString()}'");
        }

        private void ValidarAsociarPlanificador(PlanificacionDeVentaDtm plf, ParametrosDeNegocio parametros)
        {
            bool cambiandoPlfdor = parametros.Modificando && plf.PropiedadCambiada<int?>((PlanificacionDeVentaDtm)parametros.registroEnBd, nameof(PlanificacionDeVentaDtm.IdPlanificador));
            if (!cambiandoPlfdor && plf.IdPlanificador == default)
                return;

            var plfdor = Contexto.SeleccionarPorId<PlanificadorDeVentaDtm>((int)plf.IdPlanificador);
            if (cambiandoPlfdor)
                Emitir($"No se puede cambiar el planificador '{plfdor.Nombre}' de la planificación '{plf.Referencia}'");

            if (plfdor.Inicio.Date > plf.EjecutarEl.Date)
                Emitir($"la fecha de ejecución de la planificación '{plf.EjecutarEl.Date.ToShortDateString()}' no puede ser anterior a la del planificador '{plfdor.Inicio.Date.ToShortDateString()}'");

            if (plfdor.Hasta.Date < plf.EjecutarEl.Date)
                Emitir($"la fecha de ejecución de la planificación '{plf.EjecutarEl.Date.ToShortDateString()}' no puede ser posterior a la del planificador '{plfdor.Hasta.Date.ToShortDateString()}'");

        }

        private void ValidarAsociarDestinoGenerado(PlanificacionDeVentaDtm plf, ParametrosDeNegocio parametros)
        {
            if ((plf.IdTipoDeFactura.Entero() > 0 && plf.IdTipoDeParte.Entero() > 0) ||
                (plf.IdTipoDeFactura.Entero() == 0 && plf.IdTipoDeParte.Entero() == 0))
                Emitir("Al crear una planificación ha de indicarse o un tipo de parte o un tipo de factura");

            if (parametros.Modificando && !parametros.EsUnaTransicion && plf.Estado(Contexto).Terminado)
            {
                bool cambiandoParteTr = plf.PropiedadCambiada<int?>((PlanificacionDeVentaDtm)parametros.registroEnBd, nameof(PlanificacionDeVentaDtm.IdParteTr));
                bool cambiandoFacturaEmt = plf.PropiedadCambiada<int?>((PlanificacionDeVentaDtm)parametros.registroEnBd, nameof(PlanificacionDeVentaDtm.IdFacturaEmt));

                if (cambiandoParteTr || cambiandoFacturaEmt) Emitir($"La planificacion '{plf.Referencia}' ya está generada, no se puede cambiar su destino.");
            }
        }

        protected override void ObtenerModoDeAccesoAlElementoQueSeDevuelve(PlanificacionDeVentaDtm plf, PlanificacionDeVentaDto elemento, ParametrosDeNegocio parametros)
        {
            base.ObtenerModoDeAccesoAlElementoQueSeDevuelve(plf, elemento, parametros);
            if (ModoDeAcceso.HayPermisosDe(elemento.ModoDeAcceso, enumModoDeAccesoDeDatos.Gestor))
            {
                if (plf.IdFacturaEmt.Entero() > 0 || plf.IdParteTr.Entero() > 0)
                    elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
            }
        }

        protected override PlanificacionDeVentaDtm AntesDeTransitar(PlanificacionDeVentaDtm plf, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            plf = base.AntesDeTransitar(plf, transicion, parametros);
            if (transicion.EntreEtapas(enumEtapasDePlfsDeVenta.PLF_Etapa_Pendiente.Estados(), enumEtapasDePlfsDeVenta.PLF_Etapa_Generada.Estados()))
                plf = plf.AntesDeGenerar(Contexto);

            return plf;
        }

        protected override PlanificacionDeVentaDtm DespuesDeTransitar(PlanificacionDeVentaDtm plf, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            plf = base.DespuesDeTransitar(plf, transicion, parametros);

            if (transicion.EntreEtapas(enumEtapasDePlfsDeVenta.PLF_Etapa_Pendiente.Estados(), enumEtapasDePlfsDeVenta.PLF_Etapa_Anulada.Estados()))
                plf.TrasAnular(Contexto);

            return plf;
        }

        protected override void DespuesDeMapearElElemento(PlanificacionDeVentaDtm plf, PlanificacionDeVentaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(plf, elemento, parametros);
            elemento.TotalSinIva = plf.Total(Contexto, conIva: false);
            elemento.TotalConIva = plf.Total(Contexto, conIva: true);
        }

    }
}
