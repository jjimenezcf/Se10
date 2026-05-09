using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestoresDeNegocio.Ventas
{
    public class GestorDeLineasDeUnaPlfVenta : GestorDeElementos<ContextoSe, LineaDeUnaPlfVentaDtm, LineaDeUnaPlfVentaDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrLineasDeUnaPlfVenta
        {
        }

        public class MapearLineasDeUnaPlfVenta : Profile
        {
            public MapearLineasDeUnaPlfVenta()
            {
                CreateMap<LineaDeUnaPlfVentaDtm, LineaDeUnaPlfVentaDto>()
                .ForMember(dto => dto.Unitario, x => x.MapFrom(dtm => dtm.Unitario.Expresion))
                .ForMember(dto => dto.Unidad, x => x.MapFrom(dtm => dtm.Unidad.Expresion))
                .ForMember(dto => dto.IvaRepercutido, x => x.MapFrom(dtm => dtm.IvaRepercutido.Expresion));

                CreateMap<LineaDeUnaPlfVentaDto, LineaDeUnaPlfVentaDtm>()
                .ForMember(dtm => dtm.Unitario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Naturaleza, dto => dto.Ignore())
                .ForMember(dtm => dtm.IvaRepercutido, dto => dto.Ignore())
                .ForMember(dtm => dtm.Unidad, dto => dto.Ignore());
            }
        }

        public GestorDeLineasDeUnaPlfVenta(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<LineaDeUnaPlfVentaDtm> AplicarJoins(IQueryable<LineaDeUnaPlfVentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Unitario);
            consulta = consulta.Include(x => x.Naturaleza);
            consulta = consulta.Include(x => x.Unidad);
            consulta = consulta.Include(x => x.IvaRepercutido);
            return consulta;
        }

        public static GestorDeLineasDeUnaPlfVenta Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeLineasDeUnaPlfVenta(contexto, mapeador);
        }

        protected override void AntesDePersistir(LineaDeUnaPlfVentaDtm linea, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(linea, parametros);

            if (parametros.Modificando)
            {
                if (((LineaDeUnPlfVentaDtm)parametros.registroEnBd).TipoDeLinea != linea.TipoDeLinea)
                    GestorDeErrores.Emitir("No se puede modificar el tipo de línea de un planificador de ventas");
            }

            if (linea.TipoDeLinea == enumTipoDeLinea.Unitario)
                AntesDePersistirUnitario(linea, parametros);
            else if (linea.TipoDeLinea == enumTipoDeLinea.Alzada)
                AntesDePersistirAlzada(linea, parametros);
            else
                AntesDePersistirComentario(linea, parametros);

        }

        private void AntesDePersistirComentario(LineaDeUnaPlfVentaDtm linea, ParametrosDeNegocio parametros)
        {
            if (linea.IdUnitario.Entero() > 0)
                GestorDeErrores.Emitir($"El tipo de línea '{linea.TipoDeLinea.Descripcion()}' no puede tener unitario");

            if (linea.Concepto.IsNullOrEmpty())
                GestorDeErrores.Emitir("Debe indicar el comentario de la línea");

            linea.Clase = null;
            linea.Cantidad = null;
            linea.Coste = null;
            linea.Descuento = null;
            linea.IdUnitario = null;
            linea.Iva = null;
            linea.Venta = null;
        }

        private void AntesDePersistirAlzada(LineaDeUnaPlfVentaDtm linea, ParametrosDeNegocio parametros)
        {
            GestorDeErrores.Emitir($"Una planificación de ventas no puede contener líneas del tipo '{enumTipoDeLinea.Alzada.Descripcion()}'");
        }

        private void AntesDePersistirUnitario(LineaDeUnaPlfVentaDtm linea, ParametrosDeNegocio parametros)
        {
            if (linea.IdUnitario.Entero() == 0)
                GestorDeErrores.Emitir($"No ha indicado el unitario en el para la línea del tipo '{linea.TipoDeLinea.Descripcion()}'");

            var unitario = linea.Unitario(Contexto, errorSiNoHay: false, aplicarJoin: true);
            if (unitario == null)
            {
                GestorDeErrores.Emitir($"El unitario con id '{linea.IdUnitario.Entero()}' indicado para la línea no se ha localizado en la BD");
            }


            if (linea.Cantidad * linea.Venta <= 0) GestorDeErrores.Emitir("El precio por línea ha de ser mayor de cero");
            if (linea.ImporteDeLinea <= 0) GestorDeErrores.Emitir("El importe de la línea con el descuento e iva ha de ser mayor de cero");
            if (linea.Descuento != null && (decimal)linea.Descuento <= 0 && (decimal)linea.Descuento > 100) GestorDeErrores.Emitir("El descuento a aplicar ha de ser mayor de cero y menor de 100");
            if (linea.Iva <= 0 && linea.Iva > 100) GestorDeErrores.Emitir("El IVA a aplicar ha de ser mayor de cero y menor de 100");

            linea.Coste = unitario.Coste;
            linea.Clase = unitario.Clase;
            linea.IdNaturaleza = unitario.IdNaturaleza;
            linea.IdUnidad = unitario.IdUnidad;
            linea.Iva = linea.PorcentageDeIva(Contexto);

            if (parametros.Insertando) linea.Concepto = unitario.Expresion;
            else if (parametros.Modificando)
            {
                linea.Venta = ((LineaDeUnaPlfVentaDtm)parametros.registroEnBd).Venta;

                linea.Concepto = (linea.IdUnitario == ((LineaDeUnaPlfVentaDtm)parametros.registroEnBd).IdUnitario)
                ? ((LineaDeUnaPlfVentaDtm)parametros.registroEnBd).Concepto
                : linea.Concepto = unitario.Expresion;

                linea.Clase = ((LineaDeUnaPlfVentaDtm)parametros.registroEnBd).Clase;
                linea.Naturaleza = ((LineaDeUnaPlfVentaDtm)parametros.registroEnBd).Naturaleza;
                linea.Unidad = ((LineaDeUnaPlfVentaDtm)parametros.registroEnBd).Unidad;
            }
        }

        protected override void DespuesDePersistir(LineaDeUnaPlfVentaDtm linea, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(linea, parametros);
            var contrato = linea.DetalleDe<PlanificacionDeVentaDtm>(Contexto, aplicarJoin: true).Contrato;
            if (contrato != null && !contrato.UsaLaAmpliacionDe(Contexto, typeof(AvanceDtm)))
            {
                decimal incremento = linea.ImporteConDto;
                decimal anterior = parametros.Insertando ? 0 : ((LineaDeUnaPlfVentaDtm)parametros.registroEnBd).ImporteConDto;
                contrato.RecalcularAvance(Contexto, enumAvaceOperacion.PlanificadoVariado, incremento - anterior, 0);
            }
        }

        protected override void EliminarCaches(LineaDeUnaPlfVentaDtm linea, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(linea, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Plv_TotalSinIva, linea.IdElemento.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Plv_TotalConIva, linea.IdElemento.ToString());
        }

        protected override void ValidarPermisosDePersistencia(LineaDeUnaPlfVentaDtm lineaDePlf, ParametrosDeNegocio parametros) 
        => 
        lineaDePlf.ValidarQueLaLineaDeLaPlanificacionEsModificable(Contexto);

        protected override void ObtenerModoDeAccesoAlElementoQueSeDevuelve(LineaDeUnaPlfVentaDtm linea, LineaDeUnaPlfVentaDto elemento, ParametrosDeNegocio parametros)
        =>
        elemento.ModoDeAcceso = linea.DetalleDe<PlanificacionDeVentaDtm>(Contexto).ModoDeAccesoALaPlanificacion(Contexto);

        protected override void DespuesDeMapearElElemento(LineaDeUnaPlfVentaDtm linea, LineaDeUnaPlfVentaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(linea, elemento, parametros);
            elemento.Expresion = linea.TipoDeLinea == enumTipoDeLinea.Unitario ? linea.Unitario(Contexto).Expresion : linea.Concepto;
        }
    }

}

