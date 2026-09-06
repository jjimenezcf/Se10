using AutoMapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Logistica;
using ModeloDeDto.Logistica;

namespace GestoresDeNegocio.Logistica
{

    public class GestorDeRegularizaciones : GestorDeElementos<ContextoSe, RegularizacionDtm, RegularizacionDto>
    {
        public class MapearRegularizacion : Profile
        {
            public MapearRegularizacion()
            {
                CreateMap<RegularizacionDtm, RegularizacionDto>()
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Expresion))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado.Nombre))
                .ForMember(dto => dto.Almacen, dtm => dtm.MapFrom(dtm => dtm.Almacen.Expresion));

                CreateMap<RegularizacionDto, RegularizacionDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore())
                .ForMember(dtm => dtm.Almacen, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.Regularizacion;

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeRegularizacion.Gestor(Contexto, Contexto.Mapeador);

        public GestorDeRegularizaciones(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeRegularizaciones Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeRegularizaciones(contexto, mapeador);
        }

        protected override void DespuesDeMapearElRegistro(RegularizacionDto dto, RegularizacionDtm regularizacion, ParametrosDeNegocio opciones)
        {
            regularizacion.IdCg = regularizacion.Almacen(Contexto).IdCg;
        }

        protected override IQueryable<RegularizacionDtm> AplicarJoins(IQueryable<RegularizacionDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Almacen);
        }

        protected override IQueryable<RegularizacionDtm> AplicarOrden(IQueryable<RegularizacionDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override IQueryable<RegularizacionDtm> AplicarFiltros(IQueryable<RegularizacionDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            // TODO: completar los filtros propios del negocio de regularizaciones
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            return consulta;
        }

        protected override IQueryable<RegularizacionDtm> AplicarSeguridad(IQueryable<RegularizacionDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<RegularizacionDtm, TipoDeRegularizacionDtm, PermisoDeLaRegularizacionDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<RegularizacionDtm, PermisoDeLaRegularizacionDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void AntesDeMapearElRegistroParaInsertar(RegularizacionDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(elemento, opciones);
        }

        protected override void AntesDeMapearElRegistroParaModificar(RegularizacionDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaModificar(elemento, opciones);
        }


        protected override void AntesDePersistir(RegularizacionDtm regularizacion, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(regularizacion, parametros);

            if (parametros.Insertando)
            {
                var almacen = regularizacion.Almacen(Contexto);
                almacen.ValidarQueNoHayaRegularizacionViva(Contexto);
                almacen.TransitarALaEtapa(Contexto, enumEtapasDeAlmacen.ALM_Etapa_En_Inventario.EstadosDeLaEtapa(), delSistema: true);
            }
        }

        protected override void DespuesDePersistir(RegularizacionDtm regularizacion, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(regularizacion, parametros);

        }

        protected override RegularizacionDtm AntesDeTransitar(RegularizacionDtm regularizacion, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            regularizacion = base.AntesDeTransitar(regularizacion, transicion, parametros);
            return regularizacion;
        }

        protected override RegularizacionDtm DespuesDeTransitar(RegularizacionDtm regularizacion, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            regularizacion = base.DespuesDeTransitar(regularizacion, transicion, parametros);
            return regularizacion;
        }

        protected override void DespuesDeMapearElElemento(RegularizacionDtm regularizacion, RegularizacionDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(regularizacion, elemento, parametros);
        }

    }

}
