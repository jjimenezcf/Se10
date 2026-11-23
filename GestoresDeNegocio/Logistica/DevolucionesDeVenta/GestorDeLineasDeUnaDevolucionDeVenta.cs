using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Utilidades;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Logistica;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Gestor.Errores;

namespace GestoresDeNegocio.Logistica
{
    public class GestorDeLineasDeUnaDevolucionDeVenta : GestorDeElementos<ContextoSe, LineaDeUnaDevolucionDeVentaDtm, LineaDeUnaDevolucionDeVentaDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class MapearLineasDeUnaDevolucionDeVenta : Profile
        {
            public MapearLineasDeUnaDevolucionDeVenta()
            {
                CreateMap<LineaDeUnaDevolucionDeVentaDtm, LineaDeUnaDevolucionDeVentaDto>()
                .ForMember(dto => dto.LineaDeVenta, x => x.MapFrom(dtm => dtm.LineaDeVenta.Unitario.Expresion))
                .ForMember(dto => dto.IdVenta, x => x.MapFrom(dtm => dtm.LineaDeVenta.IdElemento))
                .ForMember(dto => dto.Venta, x => x.MapFrom(dtm => dtm.LineaDeVenta.Elemento.Expresion))
                .ForMember(dto => dto.Importe, x => x.MapFrom(dtm => dtm.Importe))
                .ForMember(dto => dto.Devolucion, x => x.Ignore())
                .ForMember(dto => dto.EstadoDeLaDevolucion, x => x.Ignore())
                .ForMember(dto => dto.Aceptada, x => x.Ignore())
                .ForMember(dto => dto.Cancelada, x => x.Ignore());
                CreateMap<LineaDeUnaDevolucionDeVentaDto, LineaDeUnaDevolucionDeVentaDtm>()
                .ForMember(dtm => dtm.LineaDeVenta, dto => dto.Ignore());
            }
        }

        public GestorDeLineasDeUnaDevolucionDeVenta(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeLineasDeUnaDevolucionDeVenta Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeLineasDeUnaDevolucionDeVenta(contexto, mapeador);
        }

        protected override IQueryable<LineaDeUnaDevolucionDeVentaDtm> AplicarJoins(IQueryable<LineaDeUnaDevolucionDeVentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.LineaDeVenta).ThenInclude(l => l.Unitario)
                           .Include(x => x.LineaDeVenta).ThenInclude(l => l.Elemento);
        }

        // el filtro por documento de venta va antes que los de la base: IdVenta no es una columna de la línea
        protected override IQueryable<LineaDeUnaDevolucionDeVentaDtm> AplicarFiltros(IQueryable<LineaDeUnaDevolucionDeVentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = consulta.FiltroPorVenta(filtros);
            return base.AplicarFiltros(consulta, filtros, parametros);
        }

        protected override void AntesDePersistir(LineaDeUnaDevolucionDeVentaDtm linea, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(linea, parametros);

            // las líneas sólo se crean, modifican o borran mientras la devolución se tramita
            if (parametros.EsUnaTransicion || parametros.EstaEjecutandoUnaAccion)
                return;
            var devolucion = linea.DetalleDe<DevolucionDeVentaDtm>(Contexto);

            if (!devolucion.EstaEnLaEtapa(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion))
                GestorDeErrores.Emitir($"Las líneas de la {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}' no se pueden {(parametros.Eliminando ? "borrar" : parametros.Insertando ? "añadir" : "modificar")} por no estar en la etapa de {enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion.Nombre(true)}");

            // el precio y el descuento son los de la línea vendida, y no se devuelve más de lo que queda por devolver
            if (parametros.Insertando || parametros.Modificando)
            {
                if (parametros.Insertando && linea.Orden <= 0)
                    linea.Orden = devolucion.SiguienteOrden(Contexto);
                linea.ValidarLinea(Contexto, devolucion);
            }
        }

        protected override void DespuesDeMapearElElemento(LineaDeUnaDevolucionDeVentaDtm linea, LineaDeUnaDevolucionDeVentaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(linea, elemento, parametros);

            // para el grid de devoluciones del documento de venta: la devolución, si está solicitada (en tramitación), aceptada o cancelada
            var devolucion = linea.DetalleDe<DevolucionDeVentaDtm>(Contexto);
            elemento.Devolucion = devolucion.Referencia;
            elemento.Aceptada = devolucion.EstaEnLaEtapa(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada);
            elemento.Cancelada = devolucion.EstaEnLaEtapa(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Cancelada);
            elemento.EstadoDeLaDevolucion = elemento.Aceptada ? "Aceptada" : elemento.Cancelada ? "Cancelada" : "Solicitada";

            // las líneas sólo se editan mientras la devolución se tramita: en el resto se consultan
            if (parametros.LeerPorId && elemento.ModoDeAcceso.HayPermisosDe(enumModoDeAccesoDeDatos.Gestor)
                && !devolucion.EstaEnLaEtapa(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion))
                elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
        }
    }
}
