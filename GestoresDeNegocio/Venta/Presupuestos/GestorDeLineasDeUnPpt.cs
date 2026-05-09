using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Presupuesto;
using ModeloDeDto.Presupuesto;
using Gestor.Errores;
using ServicioDeDatos.MaestrosTecnico;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using static ServicioDeDatos.Elemento.Enumerados;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.Contabilidad;

namespace GestoresDeNegocio.Presupuesto
{
    public class GestorDeLineasDeUnPpt : GestorDeElementos<ContextoSe, LineaDeUnPptDtm, LineaDeUnPptDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrLineasDeUnPpt
        {
        }

        public class MapearLineasDeUnPpt : Profile
        {
            public MapearLineasDeUnPpt()
            {
                CreateMap<LineaDeUnPptDtm, LineaDeUnPptDto>()
                .ForMember(dto => dto.Unitario, x => x.MapFrom(dtm => dtm.Unitario.Expresion))
                .ForMember(dto => dto.Unidad, x => x.MapFrom(dtm => dtm.Unidad.Expresion))
                .ForMember(dto => dto.IvaRepercutido, x => x.MapFrom(dtm => dtm.IvaRepercutido.Expresion));
                CreateMap<LineaDeUnPptDto, LineaDeUnPptDtm>()
                .ForMember(dtm => dtm.Unitario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Naturaleza, dto => dto.Ignore())
                .ForMember(dtm => dtm.Unidad, dto => dto.Ignore())
                .ForMember(dtm => dtm.IvaRepercutido, dto => dto.Ignore());
            }
        }

        public GestorDeLineasDeUnPpt(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<LineaDeUnPptDtm> AplicarJoins(IQueryable<LineaDeUnPptDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Unitario);
            consulta = consulta.Include(x => x.Naturaleza);
            consulta = consulta.Include(x => x.Unidad);
            consulta = consulta.Include(x => x.IvaRepercutido);
            return consulta;
        }

        public static GestorDeLineasDeUnPpt Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeLineasDeUnPpt(contexto, mapeador);
        }

        protected override void AntesDePersistir(LineaDeUnPptDtm linea, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(linea, parametros);
            var ppt = Contexto.SeleccionarPorId<PresupuestoDtm>(linea.IdElemento);

            if (!ppt.EstaEnLaEtapa(enumEtapasDePpts.PPT_Etapa_Elaboracion))
                GestorDeErrores.Emitir($"El presupuesto {ppt.Referencia} no se puede modificar por no estar en la etapa de {enumEtapasDePpts.PPT_Etapa_Elaboracion.Nombre(true)}");

            linea.Negocio.ValidarUsaDetalleDe(Contexto, ppt.IdTipo, typeof(LineaDeUnPptDtm));

            if (linea.TipoDeLinea == enumTipoDeLinea.Unitario || linea.TipoDeLinea == enumTipoDeLinea.Alzada)
            {
                if (linea.Cantidad == null || linea.Precio == null) GestorDeErrores.Emitir("La cantidad y el precio por línea han de ser distintos de nulos");
                if (linea.Cantidad * linea.Precio <= 0) GestorDeErrores.Emitir("El precio por línea ha de ser mayor de cero");
                if (linea.ImporteDeLinea <= 0) GestorDeErrores.Emitir("El importe de la línea con el descuento e iva ha de ser mayor de cero");
                if (linea.Descuento != null && (decimal)linea.Descuento <= 0 && (decimal)linea.Descuento > 100) GestorDeErrores.Emitir("El descuento a aplicar ha de ser mayor de cero y menor de 100");
                if (linea.Iva != null && (decimal)linea.Iva <= 0 && (decimal)linea.Iva > 100) GestorDeErrores.Emitir("El IVA a aplicar ha de ser mayor de cero y menor de 100");
                if (linea.IdIvaR == 0) linea.IdIvaR = null;
                if (linea.Iva is null && linea.IdIvaR is not null) linea.Iva = Contexto.SeleccionarPorId<IvaRepercutidoDtm>((int)linea.IdIvaR).Porcentaje;
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
                    linea.Precio = ((LineaDeUnPptDtm)parametros.registroEnBd).Precio;
                    linea.Concepto = ((LineaDeUnPptDtm)parametros.registroEnBd).Concepto;
                    linea.Clase = ((LineaDeUnPptDtm)parametros.registroEnBd).Clase;
                    linea.Naturaleza = ((LineaDeUnPptDtm)parametros.registroEnBd).Naturaleza;
                    linea.Unidad = ((LineaDeUnPptDtm)parametros.registroEnBd).Unidad;
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
                if (linea.Concepto.IsNullOrEmpty()) GestorDeErrores.Emitir($"Un comentario exige una desripción breve  ({nameof(LineaDeUnPptDtm.Concepto)})");
                linea.Cantidad = null;
                linea.Precio = null;
                linea.Iva = null;
                linea.Descuento = null;
                linea.IdIvaR = null;
                linea.IdUnidad = null;
                linea.IdNaturaleza = null;
                linea.Clase = null;
            }
        }

        protected override void EliminarCaches(LineaDeUnPptDtm linea, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(linea, parametros);

            ServicioDeCaches.EliminarElemento(CacheDe.Ppt_TotalSinIva, linea.IdElemento.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Ppt_TotalConIva, linea.IdElemento.ToString());
        }

    }
}
