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

    public class GestorDeZonasDeUnaCalle : GestorDeRelaciones<ContextoSe, ZonasDeUnaCalleDtm, ZonasDeUnaCalleDto>
    {
        public class ltrZonasDeUnaCalle
        {
            internal static readonly string JoinConCalles = nameof(JoinConCalles);
            internal static readonly string JoinConZonas = nameof(JoinConZonas);
        }

        public class MapearZonasDeUnaCalle : Profile
        {
            public MapearZonasDeUnaCalle()
            {
                CreateMap<ZonasDeUnaCalleDtm, ZonasDeUnaCalleDto>()
                    .ForMember(dto => dto.Zona, dtm => dtm.MapFrom(dtm => dtm.Zona.Nombre));

                CreateMap<ZonasDeUnaCalleDto, ZonasDeUnaCalleDtm>()
                    .ForMember(dtm => dtm.Zona, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Calle, dto => dto.Ignore());

            }
        }

        public GestorDeZonasDeUnaCalle(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {
        }
        internal static GestorDeZonasDeUnaCalle Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeZonasDeUnaCalle(contexto, mapeador);
        }

        protected override void AntesDeMapearElRegistroParaInsertar(ZonasDeUnaCalleDto zonaDeUnaCalle, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(zonaDeUnaCalle, opciones);
            if (!zonaDeUnaCalle.Zona.IsNullOrEmpty() && zonaDeUnaCalle.idZona == 0)
            {
                var calle = Contexto.SeleccionarPorId<CalleDtm>(zonaDeUnaCalle.IdCalle);
                zonaDeUnaCalle.idZona = new ZonaDtm { Nombre = zonaDeUnaCalle.Zona, IdMunicipio = calle.IdMunicipio }.Insertar(Contexto).Id;
            }
        }

        protected override IQueryable<ZonasDeUnaCalleDtm> AplicarJoins(IQueryable<ZonasDeUnaCalleDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            if (parametros.HacerJoinCon(ltrZonasDeUnaCalle.JoinConCalles))
                registros = registros.Include(rp => rp.Calle);

            if (parametros.HacerJoinCon(ltrZonasDeUnaCalle.JoinConZonas))
                registros = registros.Include(rp => rp.Zona);
            return registros;
        }

        protected override void AntesDePersistir(ZonasDeUnaCalleDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            ValidarMismoMunicipio(Contexto, registro);
        }

        protected override void DespuesDePersistir(ZonasDeUnaCalleDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            
            ServicioDeCaches.EliminarElemento(CacheDe.Callejero_ZonasDeUnaCalle, registro.IdCalle.ToString());
        }

        internal static void ValidarMismoMunicipio(ContextoSe contexto, ZonasDeUnaCalleDtm registro)
        {
            var muniCalle = GestorDeCalles.LeerRegistroPorId(contexto, registro.IdCalle, aplicarJoin: true);
            var muniZona = GestorDeZonas.LeerRegistroPorId(contexto, registro.IdZona, aplicarJoin: true);

            if (muniCalle.IdMunicipio != muniZona.IdMunicipio)
            {
                GestorDeErrores.Emitir($"El municipio de la calle dada, {muniCalle.Municipio.Nombre}, ha de ser el mismo que el de la zona. El municipio de la zona es: {muniZona.Municipio.Nombre}");
            }
        }


    }
}
