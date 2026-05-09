using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using Gestor.Errores;
using ServicioDeDatos.MaestrosTecnico;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;
using static ServicioDeDatos.Elemento.Enumerados;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Ventas
{
    public class GestorDeLineasDeUnPtr : GestorDeElementos<ContextoSe, LineaDeUnPtrDtm, LineaDeUnPtrDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrLineasDeUnPtr
        {
        }

        public class MapearLineasDeUnPtr : Profile
        {
            public MapearLineasDeUnPtr()
            {
                CreateMap<LineaDeUnPtrDtm, LineaDeUnPtrDto>()
                .ForMember(dto => dto.Unitario, x => x.MapFrom(dtm => dtm.Unitario.Expresion))
                .ForMember(dto => dto.Unidad, x => x.MapFrom(dtm => dtm.Unidad.Expresion))
                .ForMember(dto => dto.IvaRepercutido, x => x.MapFrom(dtm => dtm.IvaRepercutido.Expresion));
                CreateMap<LineaDeUnPtrDto, LineaDeUnPtrDtm>()
                .ForMember(dtm => dtm.Unitario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Naturaleza, dto => dto.Ignore())
                .ForMember(dtm => dtm.IvaRepercutido, dto => dto.Ignore())
                .ForMember(dtm => dtm.Unidad, dto => dto.Ignore());
            }
        }

        public GestorDeLineasDeUnPtr(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<LineaDeUnPtrDtm> AplicarJoins(IQueryable<LineaDeUnPtrDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Unitario);
            consulta = consulta.Include(x => x.Naturaleza);
            consulta = consulta.Include(x => x.Unidad);
            consulta = consulta.Include(x => x.IvaRepercutido);
            return consulta;
        }

        public static GestorDeLineasDeUnPtr Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeLineasDeUnPtr(contexto, mapeador);
        }

        protected override void AntesDePersistir(LineaDeUnPtrDtm linea, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(linea, parametros);

            linea.Negocio.ValidarUsaDetalleDe(typeof(LineaDeUnPtrDtm));

            var parte = linea.DetalleDe<ParteTrDtm>(Contexto);

            if (parte.Etapa() == enumEtapasDePartesTr.PTR_Etapa_Facturado)
                GestorDeErrores.Emitir("No se puede modificar el detalle de un parte facturado");

            if (parte.EstaEnLaEtapa(enumEtapasDePartesTr.PTR_Etapa_Facturado) || parte.EstaEnLaEtapa(enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar))
                GestorDeErrores.Emitir("Un parte facturado o realizado no permite modificaciones");

            if (linea.TipoDeLinea == enumTipoDeLinea.Unitario || linea.TipoDeLinea == enumTipoDeLinea.Alzada)
            {
                if (linea.Cantidad == null || linea.Precio == null) GestorDeErrores.Emitir("La cantidad y el precio por línea han de ser distintos de nulos");
                if (linea.Cantidad * linea.Precio <= 0) GestorDeErrores.Emitir("El precio por línea ha de ser mayor de cero");
                if (linea.ImporteDeLinea <= 0) GestorDeErrores.Emitir("El importe de la línea con el descuento e iva ha de ser mayor de cero");
                if (linea.Descuento != null && (decimal)linea.Descuento <= 0 && (decimal)linea.Descuento > 100) GestorDeErrores.Emitir("El descuento a aplicar ha de ser mayor de cero y menor de 100");
                if (linea.Iva != null && (decimal)linea.Iva <= 0 && (decimal)linea.Iva > 100) GestorDeErrores.Emitir("El IVA a aplicar ha de ser mayor de cero y menor de 100");
                if (linea.IdIvaR == 0) linea.IdIvaR = null;
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
                    linea.Precio = ((LineaDeUnPtrDtm)parametros.registroEnBd).Precio;
                    linea.Concepto = ((LineaDeUnPtrDtm)parametros.registroEnBd).Concepto;
                    linea.Clase = ((LineaDeUnPtrDtm)parametros.registroEnBd).Clase;
                    linea.Naturaleza = ((LineaDeUnPtrDtm)parametros.registroEnBd).Naturaleza;
                    linea.Unidad = ((LineaDeUnPtrDtm)parametros.registroEnBd).Unidad;
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
                if (linea.Concepto.IsNullOrEmpty()) GestorDeErrores.Emitir($"Un comentario exige una desripción breve  ({nameof(LineaDeUnPtrDtm.Concepto)})");
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

        protected override void DespuesDePersistir(LineaDeUnPtrDtm linea, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(linea, parametros);
            if (parametros.Modificando)
            {
                var parte = linea.DetalleDe<ParteTrDtm>(Contexto);
                var anterior = (LineaDeUnPtrDtm)parametros.registroEnBd;

                if (anterior.Concepto != linea.Concepto)
                    parte.CrearTraza(Contexto, "Variación de concepto",
                        $"El usuario {Contexto.DatosDeConexion.Login} ha cambiado el concepto de '{anterior.Concepto}' a '{linea.Concepto}'");

                if (anterior.Precio != linea.Precio)
                    parte.CrearTraza(Contexto, "Variación de precio",
                        $"El usuario {Contexto.DatosDeConexion.Login} ha indicado que el precio de {linea.Concepto} cambia de {anterior.Precio} a {linea.Precio}");

                if (anterior.Cantidad != linea.Cantidad)
                    parte.CrearTraza(Contexto, "Variación de cantidad",
                        $"El usuario {Contexto.DatosDeConexion.Login} ha indicado que la cantidad de {linea.Concepto} cambia de {anterior.Cantidad} a {linea.Cantidad}");

                if (anterior.Descuento != linea.Descuento)
                    parte.CrearTraza(Contexto, "Variación de descuento",
                        $"El usuario {Contexto.DatosDeConexion.Login} ha indicado que el descuento de {linea.Concepto} cambia de {anterior.Descuento} a {linea.Descuento}");

            }
            if (parametros.Eliminando)
            {
                var parte = linea.DetalleDe<ParteTrDtm>(Contexto);
                var anterior = (LineaDeUnPtrDtm)parametros.registroEnBd;
                parte.CrearTraza(Contexto, "Eliminación de concepto", $"El usuario {Contexto.DatosDeConexion.Login} ha eliminado el concepto '{anterior.Concepto}'");
            }
        }

        protected override void EliminarCaches(LineaDeUnPtrDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Ptr_TotalSinIva, registro.IdElemento.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Ptr_TotalConIva, registro.IdElemento.ToString());
        }

        protected override void DespuesDeMapearElElemento(LineaDeUnPtrDtm lineaPtr, LineaDeUnPtrDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(lineaPtr, elemento, parametros);
            if (parametros.Peticion == enumPeticion.epLeerPorId)
                elemento.Elemento = lineaPtr.DetalleDe<ParteTrDtm>(Contexto).Expresion;
        }

        protected override void ObtenerModoDeAccesoAlElementoQueSeDevuelve(LineaDeUnPtrDtm linea, LineaDeUnPtrDto elemento, ParametrosDeNegocio parametros)
        =>
        elemento.ModoDeAcceso = ((ParteTrDtm)linea.DetalleDe(Contexto)).ModoDeAccesoAlParteTr(Contexto);
    }
}
