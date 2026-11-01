using AutoMapper;
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

    public class GestorDeVentas : GestorDeElementos<ContextoSe, VentaDtm, VentaDto>
    {
        public class MapearVenta : Profile
        {
            public MapearVenta()
            {
                CreateMap<VentaDtm, VentaDto>()
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Expresion))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado.Nombre))
                .ForMember(dto => dto.Cliente, dtm => dtm.MapFrom(dtm => dtm.Cliente.Expresion))
                .ForMember(dto => dto.Almacen, dtm => dtm.MapFrom(dtm => dtm.Almacen.Expresion))
                .ForMember(dto => dto.Factura, dtm => dtm.MapFrom(dtm => dtm.Factura == null ? null : dtm.Factura.Expresion));

                CreateMap<VentaDto, VentaDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore())
                .ForMember(dtm => dtm.Cliente, dto => dto.Ignore())
                .ForMember(dtm => dtm.Almacen, dto => dto.Ignore())
                .ForMember(dtm => dtm.Factura, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.Venta;

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeVenta.Gestor(Contexto, Contexto.Mapeador);

        public GestorDeVentas(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeVentas Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeVentas(contexto, mapeador);
        }

        protected override void DespuesDeMapearElRegistro(VentaDto dto, VentaDtm venta, ParametrosDeNegocio opciones)
        {
            venta.IdCg = venta.Almacen(Contexto).IdCg;
        }

        protected override IQueryable<VentaDtm> AplicarJoins(IQueryable<VentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Cliente)
                           .Include(x => x.Almacen)
                           .Include(x => x.Factura);
        }

        protected override IQueryable<VentaDtm> AplicarOrden(IQueryable<VentaDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override IQueryable<VentaDtm> AplicarFiltros(IQueryable<VentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            // TODO: completar los filtros propios del negocio de documentos de venta
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            return consulta;
        }

        protected override IQueryable<VentaDtm> AplicarSeguridad(IQueryable<VentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<VentaDtm, TipoDeVentaDtm, PermisoDeLaVentaDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<VentaDtm, PermisoDeLaVentaDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void AntesDePersistir(VentaDtm venta, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(venta, parametros);
            venta.InicializarDatosCliente(Contexto, parametros);
        }

        protected override void DespuesDePersistir(VentaDtm venta, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(venta, parametros);
        }

        protected override VentaDtm AntesDeTransitar(VentaDtm venta, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            venta = base.AntesDeTransitar(venta, transicion, parametros);

            if (transicion.DestinoEstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Facturacion.Estados()))
                venta.AntesDeFacturar(Contexto);
            else if (transicion.DestinoEstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Cancelacion.Estados()))
                venta.AntesDeCancelar(Contexto);

            return venta;
        }

        protected override VentaDtm DespuesDeTransitar(VentaDtm venta, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            venta = base.DespuesDeTransitar(venta, transicion, parametros);

            if (transicion.DestinoEstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Facturacion.Estados()))
                venta.CrearPrefactura(Contexto);

            return venta;
        }

        protected override void DespuesDeMapearElElemento(VentaDtm venta, VentaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(venta, elemento, parametros);
        }

    }

}
