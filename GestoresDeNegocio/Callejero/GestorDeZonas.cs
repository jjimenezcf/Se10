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

    public class GestorDeZonas : GestorDeElementos<ContextoSe, ZonaDtm, ZonaDto>
    {
        public override enumNegocio Negocio => enumNegocio.Zona;

        public class ltrZonas
        {
            internal static readonly string JoinConMunicipio = nameof(JoinConMunicipio);

            public const string ParametroZona = "csvZonas";
        }

        public class MapearZonas : Profile
        {
            public MapearZonas()
            {
                CreateMap<ZonaDtm, ZonaDto>()
                    .ForMember(dto => dto.Municipio, dtm => dtm.MapFrom(dtm => dtm.Municipio.Nombre));

                CreateMap<ZonaDto, ZonaDtm>()
                .ForMember(dtm => dtm.FechaCreacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.FechaModificacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdUsuaCrea, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdUsuaModi, dto => dto.Ignore())
                .ForMember(dtm => dtm.Municipio, dto => dto.Ignore());

            }

        }
        public GestorDeZonas(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeZonas Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeZonas(contexto, mapeador); 
        }

        protected override IQueryable<ZonaDtm> AplicarJoins(IQueryable<ZonaDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            if (parametros.HacerJoinCon(ltrZonas.JoinConMunicipio))
                registros = registros.Include(p => p.Municipio);
            return registros;
        }

        protected override void AntesDePersistir(ZonaDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {
                var a = Contexto.Set<ZonasDeUnaCalleDtm>().LeerCacheadoPorPropiedad(nameof(ZonasDeUnaCalleDtm.IdZona), registro.Id, errorSiNoHay: false, errorSiHayMasDeUno: false);
                if (a != null)
                {
                    var nombreDeCalle = Contexto.Set<CalleDtm>().LeerCacheadoPorId(a.IdCalle).Expresion;
                    GestorDeErrores.Emitir($"la zona {registro.Nombre} está relacionado con la calle {nombreDeCalle}");
                }
            }
        }

        protected override IQueryable<ZonaDtm> AplicarFiltros(IQueryable<ZonaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.Equals(ltrZonasDeUnaCalleDtm.IdMunicipio, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    consulta = Filtrar.AplicarPredicado(consulta, filtro, x => x.IdMunicipio.Equals(Contexto.Set<CalleDtm>().AsNoTracking().First(x => x.Id.Equals(filtro.Valor.Entero())).IdMunicipio));
                }
            }

            return consulta;
        }

    }
}
