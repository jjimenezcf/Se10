using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Logistica;
using ModeloDeDto.Logistica;
using Gestor.Errores;
using ServicioDeDatos.MaestrosTecnico;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestoresDeNegocio.Logistica
{
    public class GestorDeLineasDeUnPedido : GestorDeElementos<ContextoSe, LineaDeUnPedidoDtm, LineaDeUnPedidoDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrLineasDeUnPedido
        {
        }

        public class MapearLineasDeUnPedido : Profile
        {
            public MapearLineasDeUnPedido()
            {
                CreateMap<LineaDeUnPedidoDtm, LineaDeUnPedidoDto>()
                .ForMember(dto => dto.Unitario, x => x.MapFrom(dtm => dtm.Unitario.Expresion))
                .ForMember(dto => dto.Unidad, x => x.MapFrom(dtm => dtm.Unidad.Expresion));
                CreateMap<LineaDeUnPedidoDto, LineaDeUnPedidoDtm>()
                .ForMember(dtm => dtm.Unitario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Naturaleza, dto => dto.Ignore())
                .ForMember(dtm => dtm.Unidad, dto => dto.Ignore());
            }
        }

        public GestorDeLineasDeUnPedido(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<LineaDeUnPedidoDtm> AplicarJoins(IQueryable<LineaDeUnPedidoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Unitario);
            consulta = consulta.Include(x => x.Naturaleza);
            consulta = consulta.Include(x => x.Unidad);
            return consulta;
        }

        public static GestorDeLineasDeUnPedido Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeLineasDeUnPedido(contexto, mapeador);
        }

        protected override void AntesDePersistir(LineaDeUnPedidoDtm linea, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(linea, parametros);
            var pedido = Contexto.SeleccionarPorId<PedidoDtm>(linea.IdElemento);

            if (!pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion))
                GestorDeErrores.Emitir($"El pedido {pedido.Referencia} no se puede modificar por no estar en la etapa de {enumEtapasDePedido.PED_Etapa_De_Cumplimentacion.Nombre(true)}");

            linea.Negocio.ValidarUsaDetalleDe(Contexto, pedido.IdTipo, typeof(LineaDeUnPedidoDtm));

            if (linea.TipoDeLinea == enumTipoDeLinea.Unitario || linea.TipoDeLinea == enumTipoDeLinea.Alzada)
            {
                if (linea.Cantidad == null || linea.Precio == null) GestorDeErrores.Emitir("La cantidad y el precio por línea han de ser distintos de nulos");
                if (linea.Cantidad * linea.Precio <= 0) GestorDeErrores.Emitir("El precio por línea ha de ser mayor de cero");
                if (linea.ImporteDeLinea <= 0) GestorDeErrores.Emitir("El importe de la línea con el descuento e iva ha de ser mayor de cero");
                if (linea.Descuento != null && (decimal)linea.Descuento <= 0 && (decimal)linea.Descuento > 100) GestorDeErrores.Emitir("El descuento a aplicar ha de ser mayor de cero y menor de 100");
            }

            if (linea.TipoDeLinea == enumTipoDeLinea.Unitario)
            {
                if (linea.IdUnitario.Entero() == 0) GestorDeErrores.Emitir("El tipo de línea seleccionado exige indicar el unitario");
                if (parametros.Insertando)
                {
                    var unitario = Contexto.SeleccionarPorId<UnitarioDtm>((int)linea.IdUnitario, aplicarJoin: true);
                    linea.Precio = unitario.Venta;
                    linea.Concepto = unitario.Expresion;
                    linea.Clase = unitario.Clase;
                    linea.IdNaturaleza = unitario.IdNaturaleza;
                    linea.IdUnidad = unitario.IdUnidad;
                }
                if (parametros.Modificando)
                {
                    linea.Precio = ((LineaDeUnPedidoDtm)parametros.registroEnBd).Precio;
                    linea.Concepto = ((LineaDeUnPedidoDtm)parametros.registroEnBd).Concepto;
                    linea.Clase = ((LineaDeUnPedidoDtm)parametros.registroEnBd).Clase;
                    linea.Naturaleza = ((LineaDeUnPedidoDtm)parametros.registroEnBd).Naturaleza;
                    linea.Unidad = ((LineaDeUnPedidoDtm)parametros.registroEnBd).Unidad;
                }
            }
            if (linea.TipoDeLinea != enumTipoDeLinea.Unitario && linea.IdUnitario.Entero() > 0)
                GestorDeErrores.Emitir("El tipo de línea seleccionado no permite seleccionar unitario");

            if (linea.TipoDeLinea == enumTipoDeLinea.Alzada)
            {
                if (linea.Concepto.IsNullOrEmpty()) GestorDeErrores.Emitir("El tipo de línea exige un concepto");
                if (linea.Clase == null) GestorDeErrores.Emitir("debe indicar una clase");
                if (linea.IdNaturaleza.Entero() == 0) GestorDeErrores.Emitir("El tipo de línea exige una naturaleza");
                if (linea.IdUnidad.Entero() == 0) GestorDeErrores.Emitir("El tipo de línea exige una unidad de medida");
            }

            if (linea.TipoDeLinea == enumTipoDeLinea.Comentario)
            {
                if (linea.Concepto.IsNullOrEmpty()) GestorDeErrores.Emitir($"Un comentario exige una desripción breve  ({nameof(LineaDeUnPedidoDtm.Concepto)})");
                linea.Cantidad = null;
                linea.Precio = null;
                linea.Descuento = null;
                linea.IdNaturaleza = null;
                linea.Clase = null;
            }

            ServicioDeCaches.EliminarElemento(CacheDe.Pedido_Total, linea.IdElemento.ToString());
        }

    }
}
