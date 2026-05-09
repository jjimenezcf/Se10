using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using Gestor.Errores;

namespace GestoresDeNegocio.Callejero
{

    public class GestorDeCpsDeUnaProvincia : GestorDeRelaciones<ContextoSe, CpsDeUnaProvinciaDtm, CpsDeUnaProvinciaDto>
    {
        public class ltrCpsDeUnaProvincia
        {
            internal static readonly string JoinConProvincias = nameof(JoinConProvincias);
            internal static readonly string JoinConCps = nameof(JoinConCps);
        }

        public class MapearCpsDeUnaProvincia : Profile
        {
            public MapearCpsDeUnaProvincia()
            {
                CreateMap<CpsDeUnaProvinciaDtm, CpsDeUnaProvinciaDto>()
                    .ForMember(dto => dto.CodigoPostal, dtm => dtm.MapFrom(dtm => dtm.Cp.Codigo))
                    .ForMember(dto => dto.Provincia, dtm => dtm.MapFrom(dtm => $"({dtm.Provincia.Codigo}) {dtm.Provincia.Nombre}"));

                CreateMap<CpsDeUnaProvinciaDto, CpsDeUnaProvinciaDtm>()
                    .ForMember(dtm => dtm.Cp, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Provincia, dto => dto.Ignore());
            }
        }


        public GestorDeCpsDeUnaProvincia(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {
        }


        internal static GestorDeCpsDeUnaProvincia Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCpsDeUnaProvincia(contexto, mapeador);
        }

        public static void RelacionarCpConProvincia(ContextoSe contexto, CodigoPostalDtm cp, ProvinciaDtm provincia)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            gestor.CrearRelacion(nameof(CpsDeUnaProvinciaDtm.IdCp), cp.Id, provincia.Id, false);
        }

        internal static void RelacionarCpConProvinciaSiNoLoEsta(ContextoSe contexto, CpsDeUnMunicipioDtm registro)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var cpsDeProvincia = gestor.LeerRegistro(nameof(CpsDeUnaProvinciaDtm.IdCp), registro.IdCp.ToString(), errorSiNoHay: false, errorSiHayMasDeUno: false, conBloqueo: false, aplicarJoin: false);
            if (cpsDeProvincia == null)
                RelacionarCpConProvincia(contexto, registro);
        }

        internal static void RelacionarCpConProvincia(ContextoSe contexto,  CpsDeUnMunicipioDtm registro)
        {
            GestorDeCpsDeUnaProvincia gestorDeCpsDeUnaProvincia = Gestor(contexto, contexto.Mapeador);
            var municipio = GestorDeMunicipios.Gestor(gestorDeCpsDeUnaProvincia.Contexto, gestorDeCpsDeUnaProvincia.Contexto.Mapeador).LeerRegistroPorId(registro.IdMunicipio, usarLaCache: true, traqueado: false, conBloqueo: false, aplicarJoin: false);
            gestorDeCpsDeUnaProvincia.CrearRelacion(nameof(CpsDeUnaProvinciaDtm.IdCp), registro.IdCp, municipio.IdProvincia, errorSiYaExiste: false);
        }

        protected override IQueryable<CpsDeUnaProvinciaDtm> AplicarJoins(IQueryable<CpsDeUnaProvinciaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            if (parametros.HacerJoinCon(ltrCpsDeUnaProvincia.JoinConProvincias))
                consulta = consulta.Include(rp => rp.Provincia);

            if (parametros.HacerJoinCon(ltrCpsDeUnaProvincia.JoinConCps))
                consulta = consulta.Include(rp => rp.Cp);
            return consulta;
        }


        protected override IQueryable<CpsDeUnaProvinciaDtm> AplicarFiltros(IQueryable<CpsDeUnaProvinciaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Aplicado)
                    continue;

                if (filtro.Clausula.Equals(nameof(CpsDeUnaProvinciaDto.CodigoPostal), StringComparison.CurrentCultureIgnoreCase))
                    filtro.Clausula = ltrCpsDeUnProvinciaDtm.CodigoPostal;
            }
            return consulta;
        }

        private static IQueryable<CpsDeUnaProvinciaDtm> filtroPorCodigoPostal(IQueryable<CpsDeUnaProvinciaDtm> registros, ClausulaDeFiltrado filtro)
        {
            switch (filtro.Criterio)
            {
                case enumCriteriosDeFiltrado.comienza:
                    registros = registros.Where(x => x.Cp.Codigo.StartsWith(filtro.Valor));
                    break;
                case enumCriteriosDeFiltrado.contiene:
                    registros = registros.Where(x => x.Cp.Codigo.Contains(filtro.Valor));
                    break;
                case enumCriteriosDeFiltrado.igual:
                    registros = registros.Where(x => x.Cp.Codigo.Equals(filtro.Valor));
                    break;
                default:
                    GestorDeErrores.Emitir($"Se ha solicitado un filtro por código postal en {nameof(GestorDeCpsDeUnaProvincia)} no implementado");
                    break;
            }

            return registros;
        }

        protected override void AntesDePersistir(CpsDeUnaProvinciaDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);

            if (parametros.Operacion.Equals(enumTipoOperacion.Eliminar)) return;

            var provincia = Contexto.Set<ProvinciaDtm>().LeerCacheadoPorId(registro.IdProvincia);
            var codigoPostal = Contexto.Set<CodigoPostalDtm>().LeerCacheadoPorId(registro.IdCp);

            if (!codigoPostal.Codigo.Substring(0, 2).Equals(provincia.Codigo))
                GestorDeErrores.Emitir($"El código postal {codigoPostal.Codigo} no se puede relacionar con la provincia {provincia.Expresion} por no ser sus dos primeros dígitos {provincia.Codigo}");
        }

    }
}
