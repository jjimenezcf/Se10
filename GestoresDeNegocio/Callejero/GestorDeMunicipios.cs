using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using Utilidades;
using System;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using Gestor.Errores;
using ServicioDeDatos.Elemento;

namespace GestoresDeNegocio.Callejero
{
    public class GestorDeMunicipios : GestorDeElementos<ContextoSe, MunicipioDtm, MunicipioDto>
    {
        public override enumNegocio Negocio => enumNegocio.Municipio;


        public class MapearMunicipio : Profile
        {

            public MapearMunicipio()
            {
                CreateMap<MunicipioDtm, MunicipioDto>()
                    .ForMember(dto => dto.Provincia, dtm => dtm.MapFrom(dtm => dtm.Provincia == null ? "" : $"({dtm.Provincia.Codigo}) {dtm.Provincia.Nombre}"))
                    .ForMember(dto => dto.Pais, dtm => dtm.MapFrom(dtm => dtm.Provincia == null || dtm.Provincia.Pais == null ? "" : $"({dtm.Provincia.Pais.Codigo}) {dtm.Provincia.Pais.Nombre}"))
                    .ForMember(dto => dto.IdPais, dtm => dtm.MapFrom(dtm => dtm.Provincia == null || dtm.Provincia.Pais == null ? 0 : dtm.Provincia.Pais.Id));

                CreateMap<MunicipioDto, MunicipioDtm>()
                .ForMember(dtm => dtm.Provincia, dto => dto.Ignore())
                .ForMember(dtm => dtm.FechaCreacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.FechaModificacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdUsuaCrea, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdUsuaModi, dto => dto.Ignore());

            }

        }


        public GestorDeMunicipios(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeMunicipios Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeMunicipios(contexto, mapeador); ;
        }

        public static void EliminarCacheDeMunicipiosPorNombre()
        {
            ServicioDeCaches.EliminarCache(nameof(LeerMunicipioCacheadoPorNombre));
        }

        public static MunicipioDtm LeerMunicipioCacheadoPorNombre(ContextoSe contexto, string iso2Pais, string nombreProvincia, string nombreMunicipio, bool paraActualizar, bool errorSiNoHay = true, bool errorSiMasDeUno = true, bool aplicarJoin = false)
        {
            var cache = ServicioDeCaches.Obtener(nameof(LeerMunicipioCacheadoPorNombre));
            var indice = $"{iso2Pais}.{nombreProvincia}.{nombreMunicipio}";
            if (!cache.ContainsKey(indice))
                cache[indice] = LeerMunicipioPorNombre(contexto, iso2Pais, nombreProvincia, nombreMunicipio, paraActualizar, errorSiNoHay, errorSiMasDeUno, aplicarJoin);
            return (MunicipioDtm)cache[indice];
        }

        public static MunicipioDtm LeerMunicipioPorCodigo(ContextoSe contexto, string iso2Pais, string codigoProvincia, string nombreMunicipio, bool paraActualizar, bool errorSiNoHay = true, bool errorSiMasDeUno = true, bool aplicarJoin = false)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var filtros = new List<ClausulaDeFiltrado>();
            var filtro1 = new ClausulaDeFiltrado(ltrDeUnMunicipio.filtroPorPaisIso2, enumCriteriosDeFiltrado.igual, iso2Pais);
            var filtro2 = new ClausulaDeFiltrado(ltrDeUnMunicipio.filtroPorCodigoProvincia, enumCriteriosDeFiltrado.igual, codigoProvincia);
            var filtro3 = new ClausulaDeFiltrado(nameof(INombre.Nombre), enumCriteriosDeFiltrado.igual, nombreMunicipio);
            filtros.Add(filtro1);
            filtros.Add(filtro2);
            filtros.Add(filtro3);
            var p = new ParametrosDeNegocio(paraActualizar ? enumTipoOperacion.LeerConBloqueo : enumTipoOperacion.LeerSinBloqueo, aplicarJoin);
            List<MunicipioDtm> municipios = gestor.LeerRegistros(0, -1, filtros, null, p);

            if (municipios.Count == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No se ha localizado el municipio con Iso2 del pais {iso2Pais}, provincia {codigoProvincia} y municipio {nombreMunicipio}");
            if (municipios.Count > 1 && errorSiMasDeUno)
                GestorDeErrores.Emitir($"Se han localizado más de un registro con Iso2 del pais {iso2Pais}, provincia {codigoProvincia} y municipio {nombreMunicipio}");

            return municipios.Count == 1 ? municipios[0] : null;
        }

        public static MunicipioDto SeleccionarMunicipioDto(ContextoSe contexto, string provincia, string municipio)
        {
            var provinciaDtm = contexto.SeleccionarPorPropiedad<ProvinciaDtm>(nameof(ProvinciaDtm.Nombre), provincia);
            var ak = new Dictionary<string, object>
            {
                { nameof(MunicipioDtm.IdProvincia), provinciaDtm.Id },
                { nameof(MunicipioDtm.Nombre), municipio }
            };
            var municipioDtm = contexto.SeleccionarPorAk<MunicipioDtm>(ak);
            return municipioDtm.MapearDto<MunicipioDto>(contexto);
        }

        public static MunicipioDtm LeerMunicipioPorNombre(ContextoSe contexto, string iso2Pais, string nombreProvincia, string nombreMunicipio, bool paraActualizar, bool errorSiNoHay = true, bool errorSiMasDeUno = true, bool aplicarJoin = false)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var filtros = new List<ClausulaDeFiltrado>();
            var filtro1 = new ClausulaDeFiltrado(ltrDeUnMunicipio.filtroPorPaisIso2, enumCriteriosDeFiltrado.igual, iso2Pais);
            var filtro2 = new ClausulaDeFiltrado(ltrDeUnMunicipio.filtroPorProvincia, enumCriteriosDeFiltrado.igual, nombreProvincia);
            var filtro3 = new ClausulaDeFiltrado(nameof(INombre.Nombre), enumCriteriosDeFiltrado.igual, nombreMunicipio);
            filtros.Add(filtro1);
            filtros.Add(filtro2);
            filtros.Add(filtro3);
            var p = new ParametrosDeNegocio(paraActualizar ? enumTipoOperacion.LeerConBloqueo : enumTipoOperacion.LeerSinBloqueo, aplicarJoin);
            List<MunicipioDtm> municipios = gestor.LeerRegistros(0, -1, filtros, null, p);

            if (municipios.Count == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No se ha localizado el municipio con Iso2 del pais {iso2Pais}, provincia {nombreProvincia} y municipio {nombreMunicipio}");
            if (municipios.Count > 1 && errorSiMasDeUno)
                GestorDeErrores.Emitir($"Se han localizado más de un registro con Iso2 del pais {iso2Pais}, provincia {nombreProvincia} y municipio {nombreMunicipio}");

            return municipios.Count == 1 ? municipios[0] : null;
        }

        protected override IQueryable<MunicipioDtm> AplicarJoins(IQueryable<MunicipioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Provincia);
            consulta = consulta.Include(p => p.Provincia.Pais);
            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Clausula == nameof(CpsDeUnMunicipioDtm.IdCp).ToLower())
                {
                    consulta = consulta.Include(p => p.Cps);
                }
            }
            return consulta;
        }

        protected override IQueryable<MunicipioDtm> AplicarFiltros(IQueryable<MunicipioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            var seleccionarParaDireccion = filtros.FirstOrDefault(x => x.Clausula == ltrDeUnMunicipio.SeleccionarParaDireccion);
            if (seleccionarParaDireccion != null)
            {
                seleccionarParaDireccion.Clausula = nameof(INombre.Nombre);
                parametros.Parametros[ltrCalles.SeleccionarParaDireccion] = true;
            }

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Clausula.Equals(nameof(MunicipioDto.IdProvincia), StringComparison.CurrentCultureIgnoreCase))
                    consulta = consulta.Where(x => x.Provincia.Id == filtro.Valor.Entero());

                if (filtro.Clausula.ToLower() == nameof(MunicipioDto.IdPais).ToLower())
                    consulta = consulta.Where(x => x.Provincia.Pais.Id == filtro.Valor.Entero());

                if (filtro.Clausula.ToLower() == nameof(CpsDeUnMunicipioDto.CodigoPostal).ToLower())
                {
                    consulta = filtro.Valor.Length == 5
                    ? consulta.Where(x => x.Cps.Any(y => y.Cp.Codigo == filtro.Valor))
                    : consulta.Where(x => x.Cps.Any(y => y.Cp.Codigo.StartsWith(filtro.Valor)));
                }
            }

            return consulta;
        }

        protected override void DespuesDeMapearElElemento(MunicipioDtm municipio, MunicipioDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(municipio, elemento, parametros);

            if (parametros.Parametros.LeerValor(ltrDeUnMunicipio.SeleccionarParaDireccion, false))
            {
                var provincia = Contexto.SeleccionarPorId<ProvinciaDtm>(municipio.IdProvincia);
                var pais = Contexto.SeleccionarPorId<PaisDtm>(provincia.IdPais);
                elemento.Pais = pais.Expresion;
                elemento.IdPais = pais.Id;
                elemento.Provincia = provincia.Expresion;
                elemento.IdProvincia = provincia.Id;
            }
        }


        //Todo: --> Reglas de negocio
        protected override void AntesDePersistir(MunicipioDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);

            if (parametros.Operacion == enumTipoOperacion.Modificar || parametros.Operacion == enumTipoOperacion.Insertar)
            {
                //if (registro.DC.Length > 1)
                //    GestorDeErrores.Emitir($"La longitud del DC de un municipio sólo es de un caracter, uds ha indicado {registro.DC}");
                //Obtener el código de la provincia del municipio

                //ver si el municipio está relacionado con códigos postales

                //si lo está, validar que los dos primeros dígitos del código postal corresponden con el código de la provincia
            }

            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {
                //Validar que no hay calles relacionadas con el municipio

                //Eliminar los CPS relacionados con el municipio

            }

        }

        protected override void DespuesDePersistir(MunicipioDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);

            if (parametros.Operacion == enumTipoOperacion.Modificar || parametros.Operacion == enumTipoOperacion.Eliminar)
                EliminarCacheDeMunicipiosPorNombre();
        }

    }
}
