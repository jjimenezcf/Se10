using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Gestor.Errores;
using Utilidades;

namespace GestoresDeNegocio.Callejero
{

    public class GestorDeBarrios : GestorDeElementos<ContextoSe, BarrioDtm, BarrioDto>
    {
        public override enumNegocio Negocio => enumNegocio.Barrio;
        public class ltrBarrios
        {
            internal static readonly string JoinConMunicipio = nameof(JoinConMunicipio);

            public const string ParametroBarrio = "csvBarrios";
        }

        public class MapearBarrios : Profile
        {
            public MapearBarrios()
            {
                CreateMap<BarrioDtm, BarrioDto>()
                    .ForMember(dto => dto.Municipio, dtm => dtm.MapFrom(dtm => dtm.Municipio.Nombre));

                CreateMap<BarrioDto, BarrioDtm>()
                .ForMember(dtm => dtm.Municipio, dto => dto.Ignore())
                .ForMember(dtm => dtm.FechaCreacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.FechaModificacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdUsuaCrea, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdUsuaModi, dto => dto.Ignore());

            }

        }
        public GestorDeBarrios(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeBarrios Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeBarrios(contexto, mapeador);
        }

        protected override IQueryable<BarrioDtm> AplicarJoins(IQueryable<BarrioDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            if (parametros.HacerJoinCon(ltrBarrios.JoinConMunicipio))
                registros = registros.Include(p => p.Municipio);
            return registros;
        }

        protected override void AntesDePersistir(BarrioDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {
                var a = Contexto.Set<BarriosDeUnaCalleDtm>().AsNoTracking().LeerCacheadoPorPropiedad(nameof(BarriosDeUnaCalleDtm.IdBarrio), registro.Id, errorSiNoHay: false, errorSiHayMasDeUno: false);
                if (a != null)
                { 
                    var nombreDeCalle = Contexto.Set<CalleDtm>().LeerCacheadoPorId(a.IdCalle).Expresion;
                    GestorDeErrores.Emitir($"el barrio {registro.Nombre} está relacionado con la calle {nombreDeCalle}");
                }
            }
        }

        protected override IQueryable<BarrioDtm> AplicarFiltros(IQueryable<BarrioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            foreach(var filtro in filtros)
            {
                if (filtro.Clausula.Equals(ltrBarriosDeUnaCalleDtm.IdMunicipio, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    consulta = Filtrar.AplicarPredicado(consulta, filtro, x => x.IdMunicipio.Equals(Contexto.Set<CalleDtm>().AsNoTracking().First(x => x.Id.Equals(filtro.Valor.Entero())).IdMunicipio));
                }
            }

            return consulta;
        }
    }
}
