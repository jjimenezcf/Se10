using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using Gestor.Errores;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Gastos;
using ModeloDeDto.Gastos;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Contabilidad;

namespace GestoresDeNegocio.Gastos
{
    public class GestorDeLineasDeUnaFar : GestorDeElementos<ContextoSe, LineaDeUnaFarDtm, LineaDeUnaFarDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrLineasDeUnaFar
        {
        }

        public class MapearLineasDeUnaFar : Profile
        {
            public MapearLineasDeUnaFar()
            {
                CreateMap<LineaDeUnaFarDtm, LineaDeUnaFarDto>()
                .ForMember(dto => dto.Elemento, x => x.MapFrom(dtm => dtm.Elemento == null ? "" : dtm.Elemento.Expresion))
                .ForMember(dto => dto.Naturaleza, x => x.MapFrom(dtm => dtm.Naturaleza == null ? "" : dtm.Naturaleza.Expresion))
                .ForMember(dto => dto.Sigla, x => x.MapFrom(dtm => dtm.Naturaleza == null ? "" : dtm.Naturaleza.Sigla))
                .ForMember(dto => dto.Irpf, x => x.MapFrom(dtm => dtm.Irpf == null ? "" : dtm.Irpf.Expresion))
                .ForMember(dto => dto.IvaSoportado, x => x.MapFrom(dtm => dtm.IvaSoportado == null ? "" : dtm.IvaSoportado.Expresion))
                .ForMember(dto => dto.Unidad, x => x.MapFrom(dtm => dtm.Unidad  == null ? "" : dtm.Unidad.Expresion));
                CreateMap<LineaDeUnaFarDto, LineaDeUnaFarDtm>()
                .ForMember(dtm => dtm.Naturaleza, dto => dto.Ignore())
                .ForMember(dtm => dtm.Unidad, dto => dto.Ignore())
                .ForMember(dtm => dtm.Elemento, dto => dto.Ignore())
                .ForMember(dtm => dtm.IvaSoportado, dto => dto.Ignore())
                .ForMember(dtm => dtm.Irpf, dto => dto.Ignore());
            }
        }

        public GestorDeLineasDeUnaFar(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<LineaDeUnaFarDtm> AplicarJoins(IQueryable<LineaDeUnaFarDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Naturaleza);
            consulta = consulta.Include(x => x.IvaSoportado);
            consulta = consulta.Include(x => x.Irpf);
            return consulta;
        }

        public static GestorDeLineasDeUnaFar Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeLineasDeUnaFar(contexto, mapeador);
        }

        protected override void AntesDePersistir(LineaDeUnaFarDtm linea, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(linea, parametros);
            var factura = linea.DetalleDe<FacturaRecDtm>(Contexto);

            var permitir = parametros.AccionQueSeEjecuta == ltrDeUnaFacturaRec.Accion_ModificarIva || parametros.AccionQueSeEjecuta == ltrDeUnaFacturaRec.Accion_ModificarNaturalezas;
            if (!permitir)
            {
                if (!factura.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion))
                    GestorDeErrores.Emitir($"La factura '{factura.Referencia}' no es modificable por no estar en '{enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion.Nombre(true)}'");
            }

            ValidarSiEsIncorporada(linea, parametros, factura);

            ValidarIrpf(factura, linea);

            ValidarIva(factura, linea);
        }

        private void ValidarSiEsIncorporada(LineaDeUnaFarDtm linea, ParametrosDeNegocio parametros, FacturaRecDtm factura)
        {
            if (parametros.AccionQueSeEjecuta == ltrDeUnaFacturaRec.Accion_IncorporarFacturaE)
                return;

            if (factura.EsIncorporada(Contexto))
            {
                if (parametros.Insertando || parametros.Eliminando)
                {
                    GestorDeErrores.Emitir($"A la factura '{factura.Referencia}' no se le pueden {(parametros.Insertando ? "añadir": "eliminar")} filas por haber sido importada de una eFactura'");
                }
                if (parametros.Modificando)
                {
                    if (linea.BaseImponible != ((LineaDeUnaFarDtm)parametros.registroEnBd).BaseImponible)
                    {
                        GestorDeErrores.Emitir($"A la factura '{factura.Referencia}' no se le pueden modificar la BI por ser importada de una eFactura'");
                    }
                    if (linea.PorcentajeIva != ((LineaDeUnaFarDtm)parametros.registroEnBd).PorcentajeIva)
                    {
                        GestorDeErrores.Emitir($"A la factura '{factura.Referencia}' no se le pueden modificar el % IVA por ser importada de una eFactura'");
                    }
                    if (linea.PorcentajeIva != ((LineaDeUnaFarDtm)parametros.registroEnBd).PorcentajeIrpf)
                    {
                        GestorDeErrores.Emitir($"A la factura '{factura.Referencia}' no se le pueden modificar el % IRPF por ser importada de una eFactura'");
                    }
                }
            }
        }

        private void ValidarIva(FacturaRecDtm factura, LineaDeUnaFarDtm linea)
        {
            if (linea.Clase == Enumerados.enumClaseDeLineaFar.BaseImponible && linea.IdIvaS is not null)
                GestorDeErrores.Emitir($"La línea de la factura '{factura.Referencia}' no debe indicar Iva por ser de la clase '{linea.Clase.Descripcion()}'");

            if (linea.Clase == Enumerados.enumClaseDeLineaFar.BiConIva && linea.IdIvaS is null)
                GestorDeErrores.Emitir($"La línea de la factura '{factura.Referencia}' debe indicar el Iva por ser de la clase '{linea.Clase.Descripcion()}'");

            if (linea.Clase == Enumerados.enumClaseDeLineaFar.LineaDeIva && linea.IdIvaS is null)
                GestorDeErrores.Emitir($"La línea de la factura '{factura.Referencia}' debe indicar el Iva por ser de la clase '{linea.Clase.Descripcion()}'");

            if (linea.Clase == Enumerados.enumClaseDeLineaFar.BiExenta)
            {
                if (linea.IdIvaS is null)
                    GestorDeErrores.Emitir($"La línea de la factura '{factura.Referencia}' debe indicar el Iva por ser de la clase '{linea.Clase.Descripcion()}'");
                var iva = Contexto.SeleccionarPorId<IvaSoportadoDtm>((int)linea.IdIvaS);
                if (!iva.Exento)
                    GestorDeErrores.Emitir($"La línea de la factura '{factura.Referencia}' debe indicar un tipo de Iva exento, y el tipo '{iva.Expresion}' no está marcado como tal");
            }
        }

        private static void ValidarIrpf(FacturaRecDtm factura, LineaDeUnaFarDtm linea)
        {
            if (linea.Clase != ServicioDeDatos.Elemento.Enumerados.enumClaseDeLineaFar.LineaDeIrpf && linea.IdIrpf is not null)
                GestorDeErrores.Emitir($"La línea de la factura '{factura.Referencia}' no puede tener Irpf por ser de la clase '{linea.Clase.Descripcion()}'");

            if (linea.Clase == ServicioDeDatos.Elemento.Enumerados.enumClaseDeLineaFar.LineaDeIrpf && linea.IdIrpf is null)
                GestorDeErrores.Emitir($"La línea de la factura '{factura.Referencia}' debee tener Irpf por ser de la clase '{linea.Clase.Descripcion()}'");
        }

        protected override void DespuesDePersistir(LineaDeUnaFarDtm linea, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(linea, parametros);
        }

        protected override void EliminarCaches(LineaDeUnaFarDtm linea, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(linea, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Far_Base, linea.IdElemento.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Far_Iva, linea.IdElemento.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Far_Irpf, linea.IdElemento.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Far_Impuestos, linea.IdElemento.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Far_Naturalezas, linea.IdElemento.ToString());
        }

        protected override void DespuesDeMapearElElemento(LineaDeUnaFarDtm linea, LineaDeUnaFarDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(linea, elemento, parametros);

            var factura = linea.DetalleDe<FacturaRecDtm>(Contexto);
            if (!factura.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion)) elemento.ModoDeAcceso = ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor;
            elemento.DescripcionDeClase = linea.Clase.Descripcion();
            if (linea.Clase == Enumerados.enumClaseDeLineaFar.BaseImponible)
                elemento.importeLinea = linea.BaseImponible;
            else if (linea.Clase == Enumerados.enumClaseDeLineaFar.BiConIva)
                elemento.importeLinea = linea.BaseImponible + linea.ImporteDeIva;
            else if (linea.Clase == Enumerados.enumClaseDeLineaFar.BiExenta)
                elemento.importeLinea = linea.BaseImponible;
            else if (linea.Clase == Enumerados.enumClaseDeLineaFar.LineaDeIva)
                elemento.importeLinea = linea.ImporteDeIva;
            else
                elemento.importeLinea = linea.ImporteDeIrpf;

            elemento.Elemento = factura.Expresion;
        }

    }
}
