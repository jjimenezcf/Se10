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
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Contabilidad;

namespace GestoresDeNegocio.Ventas
{
    public class GestorDeLineasDeUnaFae : GestorDeElementos<ContextoSe, LineaDeUnaFaeDtm, LineaDeUnaFaeDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrLineasDeUnaFae
        {
        }

        public class MapearLineasDeUnaFae : Profile
        {
            public MapearLineasDeUnaFae()
            {
                CreateMap<LineaDeUnaFaeDtm, LineaDeUnaFaeDto>()
                .ForMember(dto => dto.Unitario, x => x.MapFrom(dtm => dtm.Unitario.Expresion))
                .ForMember(dto => dto.Unidad, x => x.MapFrom(dtm => dtm.Unidad.Expresion))
                .ForMember(dto => dto.IvaRepercutido, x => x.MapFrom(dtm => dtm.IvaRepercutido.Expresion));
                CreateMap<LineaDeUnaFaeDto, LineaDeUnaFaeDtm>()
                .ForMember(dtm => dtm.Elemento, dto => dto.Ignore())
                .ForMember(dtm => dtm.Unitario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Naturaleza, dto => dto.Ignore())
                .ForMember(dtm => dtm.Unidad, dto => dto.Ignore())
                .ForMember(dtm => dtm.IvaRepercutido, dto => dto.Ignore())
                .ForMember(dtm => dtm.ParteTr, dto => dto.Ignore());
            }
        }

        public GestorDeLineasDeUnaFae(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<LineaDeUnaFaeDtm> AplicarJoins(IQueryable<LineaDeUnaFaeDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Unitario);
            consulta = consulta.Include(x => x.Naturaleza);
            consulta = consulta.Include(x => x.Unidad);
            consulta = consulta.Include(x => x.IvaRepercutido);
            return consulta;
        }

        public static GestorDeLineasDeUnaFae Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeLineasDeUnaFae(contexto, mapeador);
        }

        protected override void AntesDePersistir(LineaDeUnaFaeDtm linea, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(linea, parametros);

            var fae = (FacturaEmtDtm)linea.DetalleDe(Contexto);
            if (fae.Etapa() != enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura)
                GestorDeErrores.Emitir($"No se puede modificar el detalle de la factura '{fae.Referencia}' por no estar el la etapa '{enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.Nombre(true)}'");


            if (!parametros.Insertando)
                linea.IdParteTr = ((LineaDeUnaFaeDtm)parametros.registroEnBd).IdParteTr;

            if (fae.IdPresupuesto is not null && linea.IdParteTr is not null && linea.ParteTr(Contexto).IdPresupuesto != fae.IdPresupuesto)
            {
                GestorDeErrores.Emitir($"No se puede facturar el {enumNegocio.ParteDeTrabajo.Singular(true)} '{linea.ParteTr(Contexto).Referencia}'  ya que su {enumNegocio.Presupuesto.Singular(true)} es distinto al de la cabecera de factura");
            }

            ValidarDatosSegunTipoDeLinea(fae, linea, parametros);

            if (linea.TipoDeLinea != enumTipoDeLinea.Comentario && linea.IdIvaR is not null)
            {
                var tipo = fae.Tipo<TipoDeFacturaEmtDtm>(Contexto);
                if (tipo.EsExportacion)
                {
                    var direccion = fae.DireccionFiscal(Contexto, erroSiNoHay: false);
                    if (direccion is not null)
                    {
                        if (direccion.ExtraComunitaria && linea.IvaRepercutido(Contexto).Clase != enumClasesDeIvaRep.NSJ)
                            GestorDeErrores.Emitir($"las facturas de exportación a una dirección extracomunitaria, llevan Iva de clase '{enumClasesDeIvaRep.NSJ.Descripcion()}'");
                        if (direccion.IntraComunitaria && linea.IvaRepercutido(Contexto).Clase != enumClasesDeIvaRep.ISP)
                            GestorDeErrores.Emitir($"las facturas de exportación a una dirección intracomunitaria, llevan Iva de clase '{enumClasesDeIvaRep.ISP.Descripcion()}'");
                    }
                }
            }
        }

        private void ValidarDatosSegunTipoDeLinea(FacturaEmtDtm fae, LineaDeUnaFaeDtm linea, ParametrosDeNegocio parametros)
        {
            if (parametros.Eliminando)
            {
                if (linea.IdParteTr is not null)
                {
                    var parte = Contexto.SeleccionarPorId<ParteTrDtm>((int)linea.IdParteTr);
                    parte.IntentarAplicarTransicion(Contexto, TransicionAplicable.Transiciones(VariableDePartesTr.TransicionesPorMotivo, VariableDePartesTr.enumMotivoTransicion.EliminarParteDeUnaLineaDeFactura, errorSiNoHay: true));
                    parte.CrearTraza(Contexto, $"Parte eliminado de la prefactura {fae.Referencia}", $"El parte se ha eliminado de la prefactura '{fae.Expresion}' por el usuario {Contexto.DatosDeConexion.Login}");
                }
            }
            else
            {
                if (linea.TipoDeLinea == enumTipoDeLinea.Unitario || linea.TipoDeLinea == enumTipoDeLinea.Alzada)
                {
                    if (linea.Cantidad == null || linea.Precio == null) GestorDeErrores.Emitir("La cantidad y el precio por línea han de ser distintos de nulos");
                    if (!fae.EsRectificativa && linea.Cantidad * linea.Precio <= 0) GestorDeErrores.Emitir("El precio por línea ha de ser mayor de cero");
                    if (!fae.EsRectificativa && linea.ImporteDeLinea <= 0) GestorDeErrores.Emitir("El importe de la línea con el descuento e iva ha de ser mayor de cero");
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
                        linea.Precio = ((LineaDeUnaFaeDtm)parametros.registroEnBd).Precio;
                        linea.Concepto = ((LineaDeUnaFaeDtm)parametros.registroEnBd).Concepto;
                        linea.Clase = ((LineaDeUnaFaeDtm)parametros.registroEnBd).Clase;
                        linea.Naturaleza = ((LineaDeUnaFaeDtm)parametros.registroEnBd).Naturaleza;
                        linea.Unidad = ((LineaDeUnaFaeDtm)parametros.registroEnBd).Unidad;
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
                    if (linea.Concepto.IsNullOrEmpty()) GestorDeErrores.Emitir($"Un comentario exige una desripción breve  ({nameof(LineaDeUnaFaeDtm.Concepto)})");
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
        }

        protected override void DespuesDePersistir(LineaDeUnaFaeDtm linea, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(linea, parametros);

            var fae = linea.DetalleDe<FacturaEmtDtm>(Contexto);
            if (parametros.Insertando && linea.IdParteTr is not null)
            {
                var parteTr = linea.ParteTr(Contexto);
                parteTr.IdFacturaEmt = linea.IdElemento;
                parteTr.TransitarALaEtapa(Contexto, enumEtapasDePartesTr.PTR_Etapa_Facturado.EstadosDeLaEtapa(), new Dictionary<string, object>
                {
                    { ltrDeUnaFacturaEmt.FacturadoComoLinea, true},
                    { ltrDeUnaFacturaEmt.FacturaDelParte, linea.DetalleDe<FacturaEmtDtm>(Contexto) }
                });
            }
            if (parametros.Modificando)
            {
                var anterior = (LineaDeUnaFaeDtm)parametros.registroEnBd;

                if (anterior.Concepto != linea.Concepto)
                    fae.CrearTraza(Contexto, "Variación de concepto",
                        $"El usuario {Contexto.DatosDeConexion.Login} ha cambiado el concepto de '{anterior.Concepto}' a '{linea.Concepto}'");

                if (anterior.Precio != linea.Precio)
                    fae.CrearTraza(Contexto, "Variación de precio",
                        $"El usuario {Contexto.DatosDeConexion.Login} ha indicado que el precio de {linea.Concepto} cambia de {anterior.Precio} a {linea.Precio}");

                if (anterior.Cantidad != linea.Cantidad)
                    fae.CrearTraza(Contexto, "Variación de cantidad",
                        $"El usuario {Contexto.DatosDeConexion.Login} ha indicado que la cantidad de {linea.Concepto} cambia de {anterior.Cantidad} a {linea.Cantidad}");

                if (anterior.Descuento != linea.Descuento)
                    fae.CrearTraza(Contexto, "Variación de descuento",
                        $"El usuario {Contexto.DatosDeConexion.Login} ha indicado que el descuento de {linea.Concepto} cambia de {anterior.Descuento} a {linea.Descuento}");

            }
            if (parametros.Eliminando)
            {
                var anterior = (LineaDeUnaFaeDtm)parametros.registroEnBd;
                fae.CrearTraza(Contexto, "Eliminación de concepto", $"El usuario {Contexto.DatosDeConexion.Login} ha eliminado el concepto '{anterior.Concepto}'");
            }

            fae.VaciarCachesDeImportes();
        }

        protected override void DespuesDeMapearElElemento(LineaDeUnaFaeDtm linea, LineaDeUnaFaeDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(linea, elemento, parametros);

            var factura = linea.DetalleDe<FacturaEmtDtm>(Contexto);
            if (!factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura)) elemento.ModoDeAcceso = ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor;
        }

    }
}
