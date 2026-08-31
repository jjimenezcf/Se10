using AutoMapper;
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

    public class GestorDeAlmacenes : GestorDeElementos<ContextoSe, AlmacenDtm, AlmacenDto>
    {
        public class MapearAlmacen : Profile
        {
            public MapearAlmacen()
            {
                CreateMap<AlmacenDtm, AlmacenDto>()
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Expresion))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado.Nombre));

                CreateMap<AlmacenDto, AlmacenDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.Almacen;

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeAlmacen.Gestor(Contexto, Contexto.Mapeador);

        public GestorDeAlmacenes(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeAlmacenes Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeAlmacenes(contexto, mapeador);
        }

        protected override void DespuesDeMapearElRegistro(AlmacenDto dto, AlmacenDtm dtm, ParametrosDeNegocio opciones)
        {
            // TODO: completar cuando se conozca el detalle funcional del negocio de almacenes
        }

        protected override IQueryable<AlmacenDtm> AplicarJoins(IQueryable<AlmacenDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta;
        }

        protected override IQueryable<AlmacenDtm> AplicarOrden(IQueryable<AlmacenDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override IQueryable<AlmacenDtm> AplicarFiltros(IQueryable<AlmacenDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltrarParaRegularizar(filtros);
            return consulta;
        }

        protected override IQueryable<AlmacenDtm> AplicarSeguridad(IQueryable<AlmacenDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<AlmacenDtm, TipoDeAlmacenDtm, PermisoDelAlmacenDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<AlmacenDtm, PermisoDelAlmacenDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void AntesDeMapearElRegistroParaInsertar(AlmacenDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(elemento, opciones);
        }

        protected override void AntesDeMapearElRegistroParaModificar(AlmacenDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaModificar(elemento, opciones);
        }

        protected override void AntesDePersistir(AlmacenDtm almacen, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(almacen, parametros);
            // TODO: completar cuando se conozca el detalle funcional del negocio de almacenes
        }

        protected override void DespuesDePersistir(AlmacenDtm almacen, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(almacen, parametros);
            // TODO: completar cuando se conozca el detalle funcional del negocio de almacenes
        }

        protected override AlmacenDtm AntesDeTransitar(AlmacenDtm almacen, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            almacen = base.AntesDeTransitar(almacen, transicion, parametros);

            if (transicion.DestinoEstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_Cancelado.Estados()))
                almacen.AntesDeCancelar(Contexto);
            else if (transicion.DestinoEstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_Cerrado.Estados()))
                almacen.AntesDeCerrar(Contexto);
            else if (transicion.DestinoEstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_En_Inventario.Estados()))
                almacen.AntesDeRecontar(Contexto);

            return almacen;
        }

        protected override AlmacenDtm DespuesDeTransitar(AlmacenDtm almacen, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            almacen = base.DespuesDeTransitar(almacen, transicion, parametros);
            return almacen;
        }

        protected override void DespuesDeMapearElElemento(AlmacenDtm almacen, AlmacenDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(almacen, elemento, parametros);
        }

    }

}
