using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Gestor.Errores;
using Utilidades;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Juridico;
using System;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.Terceros;

namespace GestoresDeNegocio.Juridico
{

    public class GestorDelPlanificadorDeVentas : GestorDeElementos<ContextoSe, PlanificadorDeVentaDtm, PlanificadorDeVentaDto>
    {
        public override enumNegocio Negocio => enumNegocio.PlanificadorDeVenta;
        public class ltrPlanificadorDeVentas
        {
            internal static readonly string JoinConContratos = nameof(JoinConContratos);
            public static string ConPlanificadores = nameof(ConPlanificadores);
            public static string SinPlanificadores = nameof(SinPlanificadores);
            public static string PlanificadoresPdts = nameof(PlanificadoresPdts);

            public static string SelectorParaUnaPlvDeVenta = nameof(SelectorParaUnaPlvDeVenta);
        }

        public class MapearPlanificadorDeVentas : Profile
        {
            public MapearPlanificadorDeVentas()
            {
                CreateMap<PlanificadorDeVentaDtm, PlanificadorDeVentaDto>()
                .ForMember(dto => dto.CgDeLaPlanificacion, dtm => dtm.MapFrom(dtm => dtm.CgDeLaPlanificacion.Expresion))
                .ForMember(dto => dto.TipoDeFactura, dtm => dtm.MapFrom(dtm => dtm.TipoDeFactura.Nombre))
                .ForMember(dto => dto.TipoDePlanificacion, dtm => dtm.MapFrom(dtm => dtm.TipoDePlanificacion.Nombre))
                .ForMember(dto => dto.TipoDeParte, dtm => dtm.MapFrom(dtm => dtm.TipoDeParte.Nombre))
                .ForMember(dto => dto.Lote, dtm => dtm.MapFrom(dtm => dtm.Lote.Expresion))
                .ForMember(dto => dto.Contrato, dtm => dtm.MapFrom(dtm => dtm.Contrato.Expresion));

                CreateMap<PlanificadorDeVentaDto, PlanificadorDeVentaDtm>()
                .ForMember(dtm => dtm.CgDeLaPlanificacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.TipoDeFactura, dto => dto.Ignore())
                .ForMember(dtm => dtm.TipoDeParte, dto => dto.Ignore())
                .ForMember(dtm => dtm.TipoDePlanificacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.Lote, dto => dto.Ignore())
                .ForMember(dtm => dtm.Contrato, dto => dto.Ignore());
            }

        }
        public GestorDelPlanificadorDeVentas(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDelPlanificadorDeVentas Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDelPlanificadorDeVentas(contexto, mapeador);
        }

        protected override IQueryable<PlanificadorDeVentaDtm> AplicarJoins(IQueryable<PlanificadorDeVentaDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);

            if ((parametros.Peticion == enumPeticion.epLeerDatosParaElGrid || parametros.Peticion == enumPeticion.epExportar) && !parametros.FiltroPorId && filtros.Where(x => x.Clausula.Contains(nameof(PlanificadorDeVentaDtm.IdContrato))).Count() == 0)
                GestorDeErrores.Emitir($"Se han solicitado ver los planificadores de ventas y no se indica el contrato");

            if (parametros.EsUnaPeticion)
            {
                registros = registros.Include(p => p.CgDeLaPlanificacion);
                registros = registros.Include(p => p.TipoDePlanificacion);
                registros = registros.Include(p => p.TipoDeFactura);
                registros = registros.Include(p => p.TipoDeParte);
                registros = registros.Include(p => p.Lote);
            }
            if (parametros.HacerJoinCon(ltrPlanificadorDeVentas.JoinConContratos))
                registros = registros.Include(p => p.Contrato);
            return registros;
        }

        protected override void AntesDePersistir(PlanificadorDeVentaDtm planificador, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(planificador, parametros);

            var contrato = Contexto.SeleccionarPorId<ContratoDtm>(planificador.IdContrato);

            var etapasDelContrato = contrato.Etapas();
            if (!etapasDelContrato.Contains(enumEtapasDeContratos.CTR_Etapa_Vigente) && !etapasDelContrato.Contains(enumEtapasDeContratos.CTR_Etapa_En_Elaboracion))
            {
                GestorDeErrores.Emitir($"Para incluir o eliminar un planificador {contrato.DeLaClaseDeContrato} '{contrato.Referencia(Contexto)}' debe encontrarse en la etapa de '{enumEtapasDeContratos.CTR_Etapa_Vigente.Nombre()}' o de '{enumEtapasDeContratos.CTR_Etapa_En_Elaboracion.Nombre()}'");
            }

            if (parametros.Eliminando && ((PlanificadorDeVentaDtm)parametros.registroEnBd).Generado)
                GestorDeErrores.Emitir($"No se puede eliminar un planificador ya generado");

            if (parametros.Eliminando)
            {
                var lineasDePlanificador = planificador.Detalles<LineaDeUnPlfVentaDtm>(Contexto);
                foreach (var linea in lineasDePlanificador) linea.Eliminar(Contexto);
            }

            if (planificador.IdLote.Entero() == 0) planificador.IdLote = null;

            if (planificador.Inicio.Date >= planificador.Hasta.Date)
                GestorDeErrores.Emitir($"La fecha de inicio de la planificación ha de ser menor de la última planificación a generar");

            var fechas = contrato.FechasDelContrato(Contexto);

            var fechaInicial = fechas.Inicio;
            var fechaFinal = fechas.Fin.HasValue ? ((DateTime)fechas.Fin).Date : fechaInicial.AddYears(1);

            if (planificador.IdLote > 0)
            {
                var lote = Contexto.SeleccionarPorId<LoteDeUnContratoDtm>((int)planificador.IdLote);
                if (lote.VigenteDesde.HasValue) fechaInicial = ((DateTime)lote.VigenteDesde).Date;
                if (lote.VigenteHasta.HasValue) fechaFinal = ((DateTime)lote.VigenteHasta).Date;
            }

            if (fechaInicial > planificador.Inicio.Date)
                GestorDeErrores.Emitir($"La fecha inicial '{planificador.Inicio.ToShortDateString()}' de la planificación ha de ser mayor o igual que la fecha de inicial '{fechaInicial.ToShortDateString()}' del {(planificador.IdLote>0 ? "lote" : "contrato")} ");

            if (fechaFinal < planificador.Hasta.Date)
                GestorDeErrores.Emitir($"La fecha final '{planificador.Hasta.ToShortDateString()}' de la planificación ha de ser menor que la fecha de final '{fechaFinal.ToShortDateString()}' del {(planificador.IdLote > 0 ? "lote" : "contrato (si la tiene o máximo un año si no la tiene)")}");

            if (!Contexto.SeleccionarPorId<TipoDePlanificacionDeVentaDtm>(planificador.IdTipoDePlanificacion).Activo)
                GestorDeErrores.Emitir($"No se puede generar un planificación del tipo indicado por no estar activa");

            if (planificador.IdTipoDeFactura.HasValue && planificador.IdTipoDeParte.HasValue)
                GestorDeErrores.Emitir($"No se puede generar un planificador que genere un parte de trabajo y una factura");

            if (!planificador.IdTipoDeFactura.HasValue && !planificador.IdTipoDeParte.HasValue)
                GestorDeErrores.Emitir($"Un planificador debe tener indicar el tipo del destino (factura o parte)");

            if (planificador.IdTipoDeFactura.HasValue && !Contexto.SeleccionarPorId<TipoDeFacturaEmtDtm>(planificador.IdTipoDeFactura.Entero()).Activo)
                GestorDeErrores.Emitir($"No se puede generar un planificación del tipo de factura indicada por no estar activa");

            if (planificador.IdTipoDeParte.HasValue && !Contexto.SeleccionarPorId<TipoDeParteTrDtm>(planificador.IdTipoDeParte.Entero()).Activo)
                GestorDeErrores.Emitir($"No se puede generar un planificación con el tipo de parte indicado por no estar estar activa");

            if (Contexto.SeleccionarPorId<CentroGestorDtm>(planificador.IdCgDeLaPlanificacion).IdSociedad !=
                Contexto.SeleccionarPorId<CentroGestorDtm>(contrato.IdCg).IdSociedad)
                GestorDeErrores.Emitir("Las sociedades del contrato y el las de la planificación a generar han de ser las mismas");

            if (planificador.RepetirCada <= 0)
                GestorDeErrores.Emitir("Ha de indicar un número mayor ha cero de repeticiones");

           
            if (parametros.Operacion == enumTipoOperacion.Insertar)
                planificador.Generado = false;
        }

        protected override void ValidarPermisosDePersistencia(PlanificadorDeVentaDtm planificador, ParametrosDeNegocio parametros)
        {
            var contrato = Contexto.SeleccionarPorId<ContratoDtm>(planificador.IdContrato);
            var estado = contrato.Estado(Contexto);
            if (estado.Cancelado || estado.Terminado)
                GestorDeErrores.Emitir($"un contrato terminado o cancelado no acepta variaciones en sus planificadores");

            if (parametros.Insertando)
                return;

            if (parametros.Modificando && !planificador.SoloSeHaModificadoElCampo(parametros, nameof(planificador.Generado)))

            planificador.ValidarQueElPlanificadorEsModificable(Contexto);
        }

        protected override void ObtenerModoDeAccesoAlElementoQueSeDevuelve(PlanificadorDeVentaDtm planificador, PlanificadorDeVentaDto elemento, ParametrosDeNegocio parametros)
        =>
        elemento.ModoDeAcceso = planificador.ModoDeAccesoAlPlanificador(Contexto);

        protected override void DespuesDeMapearElElemento(PlanificadorDeVentaDtm plfdor, PlanificadorDeVentaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(plfdor, elemento, parametros);
            elemento.TotalSinIva = plfdor.Total(Contexto, conIva: false);
            elemento.TotalConIva = plfdor.Total(Contexto, conIva: true);
        }

        public static void Generar(ContextoSe contexto, List<int> listaIds)
        {
            foreach (int id in listaIds)
                contexto.SeleccionarPorId<PlanificadorDeVentaDtm>(id).GenerarPlanificaciones(contexto);
        }
    }
}
