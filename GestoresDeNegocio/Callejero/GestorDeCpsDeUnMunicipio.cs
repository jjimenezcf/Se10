using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using Utilidades;
using Gestor.Errores;

namespace GestoresDeNegocio.Callejero
{

    public class GestorDeCpsDeUnMunicipio : GestorDeRelaciones<ContextoSe, CpsDeUnMunicipioDtm, CpsDeUnMunicipioDto>
    {
        public class ltrCpsDeUnMunicipio
        {
            internal static readonly string JoinConMunicipios = nameof(JoinConMunicipios);
            internal static readonly string JoinConCps = nameof(JoinConCps);
        }

        public class MapearCpsDeUnMunicipio : Profile
        {
            public MapearCpsDeUnMunicipio()
            {
                CreateMap<CpsDeUnMunicipioDtm, CpsDeUnMunicipioDto>()
                    .ForMember(dto => dto.CodigoPostal, dtm => dtm.MapFrom(dtm => dtm.Cp.Codigo))
                    .ForMember(dto => dto.Municipio, dtm => dtm.MapFrom(dtm => dtm.Municipio.Nombre));

                CreateMap<CpsDeUnMunicipioDto, CpsDeUnMunicipioDtm>()
                    .ForMember(dtm => dtm.Cp, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Municipio, dto => dto.Ignore());
            }
        }


        public GestorDeCpsDeUnMunicipio(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }


        internal static GestorDeCpsDeUnMunicipio Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCpsDeUnMunicipio(contexto, mapeador);
        }

        internal static void RelacionarCpConMunicipio(ContextoSe contexto, CpsDeUnaCalleDtm registro)
        {
            var gestorDeCpsDeUnMunicipio = Gestor(contexto, contexto.Mapeador);
            var cpsDeMunicipio = gestorDeCpsDeUnMunicipio.LeerRegistro(nameof(CpsDeUnMunicipioDtm.IdCp), registro.IdCp.ToString(), errorSiNoHay: false, errorSiHayMasDeUno: false, conBloqueo: false, aplicarJoin: false);
            if (cpsDeMunicipio == null)
            {
                var calle = GestorDeCalles.Gestor(contexto, contexto.Mapeador).LeerRegistroPorId(registro.IdCalle, usarLaCache: true, traqueado: false, conBloqueo: false, aplicarJoin: false);
                gestorDeCpsDeUnMunicipio.CrearRelacion(nameof(CpsDeUnMunicipioDtm.IdCp), registro.IdCp, calle.IdMunicipio, errorSiYaExiste: false);
            }
            else
            {
                GestorDeCpsDeUnaProvincia.RelacionarCpConProvincia(contexto, cpsDeMunicipio);
            }
        }

        internal static void RelacionarCpConMunicipio(ContextoSe contexto, CodigoPostalDtm cp, MunicipioDtm municipioDtm)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            gestor.CrearRelacion(nameof(CpsDeUnMunicipioDtm.IdCp), cp.Id, municipioDtm.Id, false);
        }

        internal static void CrearRelacionConMunicipioSiNoExiste(ContextoSe contexto, CodigoPostalDtm codigoPostalDtm, string iso2Pais, string provincia, string municipio)
        {
            var municipioDtm = GestorDeMunicipios.LeerMunicipioPorNombre(contexto, iso2Pais, provincia, municipio, paraActualizar: false, errorSiNoHay: false, errorSiMasDeUno: true);
            if (municipioDtm != null)
                RelacionarCpConMunicipio(contexto, codigoPostalDtm, municipioDtm);
        }


        protected override IQueryable<CpsDeUnMunicipioDtm> AplicarJoins(IQueryable<CpsDeUnMunicipioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            if (parametros.HacerJoinCon(ltrCpsDeUnMunicipio.JoinConMunicipios))
                consulta = consulta.Include(rp => rp.Municipio);

            if (parametros.HacerJoinCon(ltrCpsDeUnMunicipio.JoinConCps))
                consulta = consulta.Include(rp => rp.Cp);
            return consulta;
        }

        //************************  Ya lo hace la Base
        protected override IQueryable<CpsDeUnMunicipioDtm> AplicarFiltros(IQueryable<CpsDeUnMunicipioDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarFiltros(registros, filtros, parametros);

            if (HayFiltroPorId(filtros))
                return registros;

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Aplicado)
                    continue;

                if (filtro.Clausula.Equals(nameof(CpsDeUnMunicipioDto.CodigoPostal), StringComparison.CurrentCultureIgnoreCase))
                    filtro.Clausula = ltrCpsDeUnMunicipioDtm.CodigoPostal;
            }
            return registros;

        }

        protected override void DespuesDePersistir(CpsDeUnMunicipioDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            if (parametros.Operacion == enumTipoOperacion.Insertar)
                GestorDeCpsDeUnaProvincia.RelacionarCpConProvinciaSiNoLoEsta(Contexto, registro);
        }

        protected override void AntesDeMapearElRegistroParaInsertar(CpsDeUnMunicipioDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(elemento, opciones);
            if (elemento.IdCp == 0 && !elemento.CodigoPostal.IsNullOrEmpty())
            {
                var cp = Contexto.SeleccionarPorPropiedad<CodigoPostalDtm>(nameof(CodigoPostalDtm.Codigo), elemento.CodigoPostal, errorSiNoHay: false);
                if (cp != null)
                    GestorDeErrores.Emitir("Seleccione correctamente el código postal");
                cp = new CodigoPostalDtm { Codigo = elemento.CodigoPostal }.Insertar(Contexto);
                elemento.IdCp = cp.Id;
            }
        }

    }
}
