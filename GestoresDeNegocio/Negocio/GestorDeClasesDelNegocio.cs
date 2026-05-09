using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Negocio;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Negocio;
using ServicioDeDatos.Elemento;
using Utilidades;
using Gestor.Errores;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Negocio
{

    public class GestorDeClasesDelNegocio : GestorDeElementos<ContextoSe, ClaseDelNegocioDtm, ClaseDelNegocioDto>
    {
        public class MapearClasesDelNegocio : Profile
        {
            public MapearClasesDelNegocio()
            {
                CreateMap<ClaseDelNegocioDtm, ClaseDelNegocioDto>()
                .ForMember(dto => dto.Negocio, dtm => dtm.MapFrom(dtm => dtm.Negocio.Nombre));

                CreateMap<ClaseDelNegocioDto, ClaseDelNegocioDtm>()
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore());
            }
        }

        public GestorDeClasesDelNegocio(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }
        internal static GestorDeClasesDelNegocio Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeClasesDelNegocio(contexto, mapeador);
        }

        protected override IQueryable<ClaseDelNegocioDtm> AplicarJoins(IQueryable<ClaseDelNegocioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Negocio);
            return consulta;
        }

        protected override IQueryable<ClaseDelNegocioDtm> AplicarFiltros(IQueryable<ClaseDelNegocioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            var filtroPorTipo = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(ClaseDelTipoDtm.IdTipo).ToLower() || x.Clausula.ToLower() == nameof(ClaseDelTipoDtm.Tipo).ToLower());
            if (filtroPorTipo is not null)
            {
                var filtroPorNegocio = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(ClaseDelNegocioDtm.IdNegocio).ToLower());
                if (filtroPorNegocio is null)
                    GestorDeErrores.Emitir("Para obtener las clases disponibles para un tipo, ha de indicar el negocio del tipo");
                var idNegocio = filtroPorNegocio.Valor.Entero();
                if (NegociosDeSe.ToEnumerado(idNegocio).UsaClasesDelTipo())
                {
                    if (filtroPorTipo.Clausula.ToLower() == nameof(ClaseDelTipoDtm.IdTipo).ToLower())
                        consulta = consulta.Where(x => !NegociosDeSe.ToEnumerado(idNegocio).ClasesDelTipo(Contexto).Any(y => y.IdTipo == filtroPorTipo.Valor.Entero() && y.IdClase == x.Id)
                                                    && x.IdNegocio == idNegocio
                                                    && x.Activa);
                    if (filtroPorTipo.Clausula.ToLower() == nameof(ClaseDelTipoDtm.Tipo).ToLower())
                        consulta = consulta.Where(x => NegociosDeSe.ToEnumerado(idNegocio).ClasesDelTipo(Contexto).Any(y => y.IdTipo == filtroPorTipo.Valor.Entero() && y.IdClase == x.Id)
                                                    && x.IdNegocio == idNegocio
                                                    && x.Activa);
                }

                filtroPorNegocio.Aplicado = true;
                filtroPorTipo.Aplicado = true;
            }
            return consulta;
        }

        protected override void AntesDePersistir(ClaseDelNegocioDtm clase, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(clase, parametros);
            if (parametros.Insertando) clase.Activa = true;
        }

        protected override void DespuesDePersistir(ClaseDelNegocioDtm clase, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(clase, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Negocio_HayClasesDelNegocio, clase.IdNegocio.ToString());
        }
    }
}
