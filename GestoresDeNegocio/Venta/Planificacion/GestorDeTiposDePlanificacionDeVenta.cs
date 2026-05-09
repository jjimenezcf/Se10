using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Ventas;
using ServicioDeDatos.Ventas;

namespace GestoresDeNegocio.Ventas
{
    public class GestorDeTiposPlanificacionDeVenta : GestorDeTiposDeElemento<ContextoSe, TipoDePlanificacionDeVentaDtm, TipoDePlanificacionDeVentaDto>
    {
        public class ltrDeUnTipoDePlanificacionDeVenta
        {

        }

        public class MapearTipoDePlanificacionDeVenta : MapearTipoDeElemento
        {
            public MapearTipoDePlanificacionDeVenta()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDePlanificacionDeVentaDtm, TipoDePlanificacionDeVentaDto>())
               .ForMember(dto => dto.Negocio, dtm => enumNegocio.PlanificacionDeVenta.Plural())
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre));

                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDePlanificacionDeVentaDto, TipoDePlanificacionDeVentaDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }


        public GestorDeTiposPlanificacionDeVenta(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.PlanificacionDeVenta)
        {

        }

        public static GestorDeTiposPlanificacionDeVenta Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposPlanificacionDeVenta(contexto, mapeador);
        }

        public static TipoDePlanificacionDeVentaDtm PersistirTipo(ContextoSe contexto, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDePlanificacionDeVentaDtm();
                tipo.IdEstado = idEstado;
                tipo.ClaseDeLibro = clsLibro;
                tipo.Nombre = nombre;
                tipo.Sigla = sigla;
                tipo = gestor.PersistirRegistro(tipo, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(tipo.Id, false, false, false, true);
            }

            if (leido.Nombre != nombre || leido.Sigla != sigla || leido.ClaseDeLibro != clsLibro)
            {
                leido.Nombre = nombre; leido.Sigla = sigla; leido.ClaseDeLibro = clsLibro;
                gestor.PersistirRegistro(leido, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
            return leido;
        }

        protected override IQueryable<TipoDePlanificacionDeVentaDtm> AplicarJoins(IQueryable<TipoDePlanificacionDeVentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                .Include(x => x.Estado);
        }

    }
}
