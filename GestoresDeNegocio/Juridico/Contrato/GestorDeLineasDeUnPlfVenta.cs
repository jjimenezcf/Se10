using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Juridico;
using ServicioDeDatos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.MaestrosTecnico;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestoresDeNegocio.Juridico
{
    public class GestorDeLineasDeUnPlfVenta : GestorDeElementos<ContextoSe, LineaDeUnPlfVentaDtm, LineaDeUnPlfVentaDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrLineasDeUnPlfVenta
        {
        }

        public class MapearLineasDeUnPlfVenta : Profile
        {
            public MapearLineasDeUnPlfVenta()
            {
                CreateMap<LineaDeUnPlfVentaDtm, LineaDeUnPlfVentaDto>()
                .ForMember(dto => dto.Unitario, x => x.MapFrom(dtm => dtm.IdUnitario == null || dtm.Unitario == null ? "" : dtm.Unitario.Expresion))
                .ForMember(dto => dto.Unidad, x => x.MapFrom(dtm => dtm.IdUnitario == null || dtm.Unidad == null ? "" : dtm.Unidad.Expresion))
                .ForMember(dto => dto.IvaRepercutido, x => x.MapFrom(dtm => dtm.IdIvaR == null || dtm.IvaRepercutido == null ? "" : dtm.IvaRepercutido.Expresion))
                .ForMember(dto => dto.Clase, x => x.MapFrom(dtm => dtm.Clase == null ? enumClaseUnitario.Material : dtm.Clase)); 

                CreateMap<LineaDeUnPlfVentaDto, LineaDeUnPlfVentaDtm>()
                .ForMember(dtm => dtm.Unitario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Naturaleza, dto => dto.Ignore())
                .ForMember(dtm => dtm.IvaRepercutido, dto => dto.Ignore())
                .ForMember(dtm => dtm.Unidad, dto => dto.Ignore());
            }
        }

        public GestorDeLineasDeUnPlfVenta(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<LineaDeUnPlfVentaDtm> AplicarJoins(IQueryable<LineaDeUnPlfVentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Unitario);
            consulta = consulta.Include(x => x.Naturaleza);
            consulta = consulta.Include(x => x.Unidad);
            consulta = consulta.Include(x => x.IvaRepercutido);
            return consulta;
        }

        public static GestorDeLineasDeUnPlfVenta Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeLineasDeUnPlfVenta(contexto, mapeador);
        }

        protected override void AntesDePersistir(LineaDeUnPlfVentaDtm linea, ParametrosDeNegocio parametros)
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

        private void AntesDePersistirComentario(LineaDeUnPlfVentaDtm linea, ParametrosDeNegocio parametros)
        {
            if (linea.IdUnitario.Entero() > 0 )
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

        private void AntesDePersistirAlzada(LineaDeUnPlfVentaDtm linea, ParametrosDeNegocio parametros)
        {
            GestorDeErrores.Emitir($"Un planificador de ventas no puede contener líneas del tipo '{enumTipoDeLinea.Alzada.Descripcion()}'");
        }

        private void AntesDePersistirUnitario(LineaDeUnPlfVentaDtm linea, ParametrosDeNegocio parametros)
        {
            if (linea.IdUnitario.Entero() == 0)
                GestorDeErrores.Emitir($"No ha indicado el unitario en el para la línea del tipo '{linea.TipoDeLinea.Descripcion()}'");

            var unitario = linea.Unitario(Contexto, errorSiNoHay: false, aplicarJoin: true);
            if (unitario == null)
            {
                GestorDeErrores.Emitir($"El unitario con id '{linea.IdUnitario.Entero()}' indicado para la línea no se ha localizado en la BD");
            }

            var planificador = linea.DetalleDe<PlanificadorDeVentaDtm>(Contexto);

            if (planificador.IdLote != null)
            {
                var ul = Contexto.SeleccionarPorAk<UnitariosDeUnLoteDtm>(new Dictionary<string, object> {
                             { nameof(UnitariosDeUnLoteDtm.IdLote),planificador.IdLote },
                             { nameof(UnitariosDeUnLoteDtm.IdUnitario), linea.IdUnitario } }, errorSiNoHay: false);

                if (ul is null)
                    GestorDeErrores.Emitir($"El unitario '{unitario.Expresion}' no está asignado al lote '{planificador.Lote.Expresion}'");
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
                linea.Venta = ((LineaDeUnPlfVentaDtm)parametros.registroEnBd).Venta;
                
                linea.Concepto = (linea.IdUnitario == ((LineaDeUnPlfVentaDtm)parametros.registroEnBd).IdUnitario)
                ? ((LineaDeUnPlfVentaDtm)parametros.registroEnBd).Concepto
                : linea.Concepto = unitario.Expresion;
               
                linea.Clase = ((LineaDeUnPlfVentaDtm)parametros.registroEnBd).Clase;
                linea.Naturaleza = ((LineaDeUnPlfVentaDtm)parametros.registroEnBd).Naturaleza;
                linea.Unidad = ((LineaDeUnPlfVentaDtm)parametros.registroEnBd).Unidad;
            }
        }

        protected override void ValidarPermisosDePersistencia(LineaDeUnPlfVentaDtm lineaDePlf, ParametrosDeNegocio parametros)
        =>
        lineaDePlf.ValidarQueLaLineaDelPlanificadorEsModificable(Contexto);

        protected override void ObtenerModoDeAccesoAlElementoQueSeDevuelve(LineaDeUnPlfVentaDtm linea, LineaDeUnPlfVentaDto elemento, ParametrosDeNegocio parametros)
        =>
        elemento.ModoDeAcceso = linea.DetalleDe<PlanificadorDeVentaDtm>(Contexto).ModoDeAccesoAlPlanificador(Contexto);

        protected override void EliminarCaches(LineaDeUnPlfVentaDtm linea, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(linea, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Pldor_TotalSinIva, linea.IdElemento.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Pldor_TotalConIva, linea.IdElemento.ToString());
        }

        protected override void DespuesDeMapearElElemento(LineaDeUnPlfVentaDtm linea, LineaDeUnPlfVentaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(linea, elemento, parametros);
            elemento.Expresion = linea.TipoDeLinea == enumTipoDeLinea.Unitario ? linea.Unitario(Contexto).Expresion : linea.Concepto;
            if (parametros.FiltroPorId)
            {
                var planificador = linea.DetalleDe<PlanificadorDeVentaDtm>(Contexto, aplicarJoin: true);
                if (planificador.IdLote != default)
                {
                    elemento.IdLote = planificador.IdLote;
                    elemento.Lote = planificador.Lote.Expresion;
                }
            }
        }
    }

}

