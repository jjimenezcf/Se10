using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using Gestor.Errores;
using Utilidades;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;

namespace GestoresDeNegocio.Callejero
{

    public class GestorDeCodigosPostales : GestorDeElementos<ContextoSe, CodigoPostalDtm, CodigoPostalDto>
    {

        class archivoParaImportar
        {
            public string parametro { get; set; }
            public int valor { get; set; }
        }

        public class ltrDeUnCp
        {
            internal static readonly string NombreProvincia = nameof(NombreProvincia);
            internal static readonly string NombreMunicipio = nameof(NombreMunicipio);
            public const string csvCp = nameof(csvCp);
        }


        public class MapearVariables : Profile
        {
            public MapearVariables()
            {
                CreateMap<CodigoPostalDtm, CodigoPostalDto>()
                    .ForMember(dto => dto.Provincia, dtm => dtm.MapFrom(x => x.NombreProvincia))
                    .ForMember(dto => dto.Municipios, dtm => dtm.MapFrom(x => x.NombreMunicipio));
                CreateMap<CodigoPostalDto, CodigoPostalDtm>()
                    .ForMember(dtm => dtm.NombreProvincia, dto => dto.Ignore())
                    .ForMember(dtm => dtm.NombreMunicipio, dto => dto.Ignore());
            }
        }

        public GestorDeCodigosPostales(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeCodigosPostales Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCodigosPostales(contexto, mapeador); ;
        }


        public List<CodigoPostalDto> LeerCodigosPostales(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros)
        {
            List<ClausulaDeOrdenacion> orden = new List<ClausulaDeOrdenacion>();
            orden.Add(new ClausulaDeOrdenacion() { OrdenarPor = nameof(CodigoPostalDtm.Codigo), Modo = ModoDeOrdenancion.ascendente });

            var registros = LeerRegistros(posicion, cantidad, filtros, orden);
            return MapearElementos(registros).ToList();
        }

        internal static CodigoPostalDtm LeerTipoDeViaPorCp(ContextoSe contexto, string cp, bool paraActualizar, bool errorSiNoHay = true, bool errorSiMasDeUno = true)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            return gestor.LeerRegistro(nameof(CodigoPostalDtm.Codigo), cp, errorSiNoHay, errorSiMasDeUno, paraActualizar ? true : false, aplicarJoin: false);
        }

        protected override void AntesDePersistir(CodigoPostalDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                if (parametros.Parametros.ContainsKey(ltrDeUnCp.NombreProvincia) && parametros.Parametros.ContainsKey(ltrDeUnCp.NombreMunicipio))
                {
                    var np = parametros.Parametros[ltrDeUnCp.NombreProvincia].ToString();
                    var nm = parametros.Parametros[ltrDeUnCp.NombreMunicipio].ToString();
                    var municipioDtm = GestorDeMunicipios.LeerMunicipioPorNombre(Contexto, ltrIsoPaises.Spain, np, nm, paraActualizar: false, errorSiNoHay: false);
                    if (municipioDtm != null)
                        parametros.Parametros[nameof(MunicipioDtm)] = municipioDtm;
                }

            }
            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {
                var a = Contexto.Set<CpsDeUnaCalleDtm>().AsNoTracking().LeerCacheadoPorPropiedad(nameof(CpsDeUnaCalleDtm.IdCp), registro.Id, errorSiNoHay: false, errorSiHayMasDeUno: false);
                if (a != null)
                {
                    var nombreDeCalle = Contexto.Set<CalleDtm>().LeerCacheadoPorId(a.IdCalle).Expresion;
                    GestorDeErrores.Emitir($"el cp {registro.Codigo} está relacionado con la calle {nombreDeCalle}");
                }

                var b = Contexto.Set<CpsDeUnMunicipioDtm>().AsNoTracking().LeerCacheadoPorPropiedad(nameof(CpsDeUnMunicipioDtm.IdCp), registro.Id, errorSiNoHay: false, errorSiHayMasDeUno: false);
                if (b != null)
                {
                    var nombreDelMunicipio = Contexto.Set<MunicipioDtm>().LeerCacheadoPorId(b.IdMunicipio).Expresion;
                    GestorDeErrores.Emitir($"el cp {registro.Codigo} está relacionado con el municipio de {nombreDelMunicipio}");
                }

                var c = Contexto.Set<CpsDeUnaProvinciaDtm>().AsNoTracking().LeerCacheadoPorPropiedad(nameof(CpsDeUnaProvinciaDtm.IdCp), registro.Id, errorSiNoHay: false, errorSiHayMasDeUno: false);
                if (c != null)
                {
                    var nombreDeLaProvincia = Contexto.Set<ProvinciaDtm>().LeerCacheadoPorId(c.IdProvincia).Expresion;
                    GestorDeErrores.Emitir($"el cp {registro.Codigo} está relacionado con la provincia de {nombreDeLaProvincia}");
                }
            }
        }

        protected override void DespuesDePersistir(CodigoPostalDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            if (parametros.Operacion == enumTipoOperacion.Insertar || parametros.Operacion == enumTipoOperacion.Modificar)
            {
                //relacionar con la provincia usando los dos primeros caractéres
                var gestorProvincias = GestorDeProvincias.Gestor(Contexto, Contexto.Mapeador);
                var provinciaDtm = gestorProvincias.LeerRegistro(nameof(ProvinciaDtm.Codigo), registro.Codigo.PadLeft(5, '0').Substring(0, 2), errorSiNoHay: true, errorSiHayMasDeUno: true, conBloqueo: false, aplicarJoin: false);
                GestorDeCpsDeUnaProvincia.RelacionarCpConProvincia(Contexto, registro, provinciaDtm);

                //relacionar con el municipio usando lo indicado en los parámetros
                if (parametros.Parametros.ContainsKey(nameof(MunicipioDtm)))
                {
                    var municipioDtm = (MunicipioDtm)parametros.Parametros[nameof(MunicipioDtm)];
                    GestorDeCpsDeUnMunicipio.RelacionarCpConMunicipio(Contexto, registro, municipioDtm);
                }
            }
        }

        protected override IQueryable<CodigoPostalDtm> AplicarFiltros(IQueryable<CodigoPostalDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            foreach (var filtro in filtros)
            {
                //Selecciona los códigos postales que no están relacionados con niguna provincia y de éstos sólo coge aquellos que el código de la provincia 
                //coincide con los dos primeros dígitos del código postal.
                if (filtro.Clausula.Equals(nameof(CpsDeUnaProvinciaDto.IdProvincia), StringComparison.CurrentCultureIgnoreCase))
                {
                    consulta = consulta.Where(x => !x.Provincias.Any(p => true));
                    consulta = consulta.Where(x => x.Codigo.Substring(0, 2) == Contexto.Set<ProvinciaDtm>().AsNoTracking().FirstOrDefault(p => p.Id.Equals(filtro.Valor.Entero())).Codigo);
                    filtro.Aplicado = true;
                }

                //Selecciona los códigos postales que no están relacionados con nigún municipio y de éstos sólo coge aquellos que el código de su provincia 
                //coincide con los dos primeros dígitos del código postal.
                if (filtro.Clausula.Equals(nameof(CpsDeUnMunicipioDto.IdMunicipio), StringComparison.CurrentCultureIgnoreCase) && filtro.Criterio == enumCriteriosDeFiltrado.diferente)
                {
                    consulta = consulta.Where(x => !x.Municipios.Any(p => true));
                    consulta = consulta.Where(x => x.Codigo.Substring(0, 2) == 
                             Contexto.Set<ProvinciaDtm>().AsNoTracking().FirstOrDefault(p => p.Id == 
                                  Contexto.Set<MunicipioDtm>().AsNoTracking().FirstOrDefault(m => m.Id == filtro.Valor.Entero()).IdProvincia ).Codigo);
                    filtro.Aplicado = true;
                }

                //selecciona los cps relacionados con el municipio
                if (filtro.Clausula.Equals(nameof(CpsDeUnMunicipioDto.IdMunicipio), StringComparison.CurrentCultureIgnoreCase) && filtro.Criterio == enumCriteriosDeFiltrado.igual)
                {
                    consulta = consulta.Where(x => x.Municipios.Any(m => m.IdMunicipio.Equals(filtro.Valor.Entero())));
                    filtro.Aplicado = true;
                }

                //Selecciona los códigos postales cuyos dos primeros dígitos coinciden con el código de de su provincia
                if (filtro.Clausula.Equals(nameof(CpsDeUnaCalleDto.IdCalle), StringComparison.CurrentCultureIgnoreCase))
                {
                    consulta = consulta.Where(x => x.Codigo.Substring(0, 2) ==
                             Contexto.Set<ProvinciaDtm>().AsNoTracking().
                             FirstOrDefault(p => p.Id == Contexto.Set<MunicipioDtm>().AsNoTracking().
                                                         FirstOrDefault(m => m.Id == Contexto.Set<CalleDtm>().AsNoTracking().
                                                                                     FirstOrDefault(c => c.Id == filtro.Valor.Entero()).IdMunicipio).IdProvincia).Codigo);
                    filtro.Aplicado = true;
                }

            }

            return consulta;
        }

    }
}
