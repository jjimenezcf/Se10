using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Utilidades;
using ServicioDeDatos.Logistica;
using ModeloDeDto.Logistica;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Gestor.Errores;
using ServicioDeDatos.Seguridad;

namespace GestoresDeNegocio.Logistica
{
    public class GestorDeLineasDeUnaVenta : GestorDeElementos<ContextoSe, LineaDeUnaVentaDtm, LineaDeUnaVentaDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class MapearLineasDeUnaVenta : Profile
        {
            public MapearLineasDeUnaVenta()
            {
                CreateMap<LineaDeUnaVentaDtm, LineaDeUnaVentaDto>()
                .ForMember(dto => dto.Unitario, x => x.MapFrom(dtm => dtm.Unitario.Expresion))
                .ForMember(dto => dto.Importe, x => x.MapFrom(dtm => dtm.Importe))
                .ForMember(dto => dto.PrecioAlmacen, x => x.Ignore())
                .ForMember(dto => dto.Margen, x => x.Ignore())
                .ForMember(dto => dto.CantidadDevuelta, x => x.Ignore());
                CreateMap<LineaDeUnaVentaDto, LineaDeUnaVentaDtm>()
                .ForMember(dtm => dtm.Unitario, dto => dto.Ignore());
            }
        }

        public GestorDeLineasDeUnaVenta(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeLineasDeUnaVenta Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeLineasDeUnaVenta(contexto, mapeador);
        }

        protected override IQueryable<LineaDeUnaVentaDtm> AplicarJoins(IQueryable<LineaDeUnaVentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Unitario);
            return consulta;
        }

        protected override void AntesDePersistir(LineaDeUnaVentaDtm linea, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(linea, parametros);

            // las líneas sólo se crean, modifican o borran mientras el documento de venta está en la etapa de solicitud: una vez servido
            // tienen su salida del almacén (y quizá su prefactura)
            if (parametros.EsUnaTransicion || parametros.EstaEjecutandoUnaAccion)
                return;
            var venta = Contexto.SeleccionarPorId<VentaDtm>(linea.IdElemento, usarLaCache: false);
            if (!venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Solicitud))
                GestorDeErrores.Emitir($"Las líneas del {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' no se pueden {(parametros.Operacion == enumTipoOperacion.Eliminar ? "borrar" : parametros.Operacion == enumTipoOperacion.Insertar ? "añadir" : "modificar")} por no estar en la etapa de {enumEtapasDeVenta.DSA_Etapa_Solicitud.Nombre(true)}");
        }

        protected override void DespuesDeMapearElElemento(LineaDeUnaVentaDtm linea, LineaDeUnaVentaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(linea, elemento, parametros);
            elemento.PrecioAlmacen = linea.PrecioAlmacen(Contexto);
            elemento.Margen = linea.Margen(elemento.PrecioAlmacen);
            elemento.CantidadDevuelta = linea.CantidadDevuelta(Contexto);

            // las líneas sólo se editan en la etapa de solicitud del documento de venta (ver AntesDePersistir): en el resto se consultan
            if (parametros.LeerPorId && elemento.ModoDeAcceso.HayPermisosDe(enumModoDeAccesoDeDatos.Gestor)
                && !Contexto.SeleccionarPorId<VentaDtm>(linea.IdElemento).EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Solicitud))
                elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
        }
    }
}
