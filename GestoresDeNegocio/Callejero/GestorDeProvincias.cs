using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using System;
using Microsoft.EntityFrameworkCore;
using Gestor.Errores;
using Utilidades;
using ServicioDeDatos.Elemento;

namespace GestoresDeNegocio.Callejero
{
    public class GestorDeProvincias : GestorDeElementos<ContextoSe, ProvinciaDtm, ProvinciaDto>
    {
        public override enumNegocio Negocio => enumNegocio.Provincia;
        class archivoParaImportar
        {
            public string parametro { get; set; }
            public int valor { get; set; }
        }


        public class MapearProvincia : Profile
        {
            public MapearProvincia()
            {
                CreateMap<ProvinciaDtm, ProvinciaDto>()
                    .ForMember(dto => dto.Pais, dtm => dtm.MapFrom(dtm => dtm.Pais == null ? null : $"({dtm.Pais.Codigo}) {dtm.Pais.Nombre}"));

                CreateMap<ProvinciaDto, ProvinciaDtm>()
                .ForMember(dtm => dtm.Pais, dto => dto.Ignore())
                .ForMember(dtm => dtm.FechaCreacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.FechaModificacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdUsuaCrea, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdUsuaModi, dto => dto.Ignore());

            }
        }

        public GestorDeProvincias(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeProvincias Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeProvincias(contexto, mapeador); ;
        }

        internal static ProvinciaDtm LeerProvinciaPorCodigo(ContextoSe contexto, string iso2Pais, string codigoProvincia, bool paraActualizar, bool errorSiNoHay = true, bool errorSiMasDeUno = true)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var filtros = new List<ClausulaDeFiltrado>();
            var filtro1 = new ClausulaDeFiltrado(nameof(ProvinciaDtm.Pais.ISO2), enumCriteriosDeFiltrado.igual, iso2Pais);
            var filtro2 = new ClausulaDeFiltrado(nameof(ProvinciaDtm.Codigo), enumCriteriosDeFiltrado.igual, codigoProvincia);
            filtros.Add(filtro1);
            filtros.Add(filtro2);
            var p = new ParametrosDeNegocio(paraActualizar ? enumTipoOperacion.LeerConBloqueo : enumTipoOperacion.LeerSinBloqueo, aplicarJoin: false);
            //p.Parametros.Add(ltrJoinAudt.IncluirUsuarioDtm, false);
            List<ProvinciaDtm> provincias = gestor.LeerRegistros(0, -1, filtros, null, p);

            if (provincias.Count == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No se ha localizado la provincia para el código del pais {iso2Pais} y codigo de provincia {codigoProvincia}");
            if (provincias.Count > 1 && errorSiMasDeUno)
                GestorDeErrores.Emitir($"Se han localizado más de un registro de provincia con el código del pais {iso2Pais} y codigo de provincia {codigoProvincia}");

            return provincias.Count == 1 ? provincias[0] : null;
        }


        protected override IQueryable<ProvinciaDtm> AplicarJoins(IQueryable<ProvinciaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Pais);
            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Clausula == nameof(CpsDeUnaProvinciaDtm.IdCp).ToLower())
                {
                    consulta = consulta.Include(p => p.Cps);
                }
            }
            return consulta;
        }

        protected override IQueryable<ProvinciaDtm> AplicarFiltros(IQueryable<ProvinciaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            var seleccionarParaDireccion = filtros.FirstOrDefault(x => x.Clausula == ltrDeUnaProvincia.SeleccionarParaDireccion);
            if (seleccionarParaDireccion != null)
            {
                seleccionarParaDireccion.Clausula = nameof(INombre.Nombre);
                parametros.Parametros[ltrCalles.SeleccionarParaDireccion] = true;
            }

            foreach (ClausulaDeFiltrado filtro in filtros.Where(filtro => filtro.Clausula.Equals(ltrProvinciaDto.CodigoPostal, StringComparison.CurrentCultureIgnoreCase)))
            {
                consulta = filtro.Valor.Length == 5
                ? consulta.Where(x => x.Cps.Any(y => y.Cp.Codigo == filtro.Valor))
                : consulta.Where(x => x.Cps.Any(y => y.Cp.Codigo.StartsWith(filtro.Valor)));
                filtro.Aplicado = true;
            }

            return consulta;
        }

        protected override void DespuesDeMapearElElemento(ProvinciaDtm provincia, ProvinciaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(provincia, elemento, parametros);
            if (parametros.Parametros.LeerValor(ltrDeUnaProvincia.SeleccionarParaDireccion, false))
            {
                var pais = Contexto.SeleccionarPorId<PaisDtm>(provincia.IdPais);
                elemento.Pais = pais.Expresion;
                elemento.IdPais = pais.Id;
            }
        }

        protected override void AntesDePersistir(ProvinciaDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);

            if (parametros.Operacion == enumTipoOperacion.Modificar)
            {
                //validar que si la provincia está relacionada con códigos postales, los dos primeros dígitos del código son igual que el código de la provincia
                var a = Contexto.Set<CpsDeUnaProvinciaDtm>().AsNoTracking().FirstOrDefault(x => x.IdProvincia == registro.Id && x.Cp.Codigo.Substring(0, 2) != registro.Codigo);
                if (a != null)
                {
                    var codigoPostal = Contexto.Set<CodigoPostalDtm>().LeerCacheadoPorId(a.IdCp).Codigo;
                    GestorDeErrores.Emitir($"No se puede modificar la provincia ya que el código de la provincia es {registro.Codigo} y está relacionada con el código postal {codigoPostal}");
                }
            }

            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {
                //Validar que no hay municipios con la provincia
                var municipio = Contexto.Set<MunicipioDtm>().AsNoTracking().FirstOrDefault(x => x.IdProvincia == registro.Id);
                if (municipio != null)
                    GestorDeErrores.Emitir($"No se puede eliminar la provincia por estar relacionada con el municipio {municipio.Expresion}");

                //Elimina las relaciones del los cp con la provincia a borrar
                var a = Contexto.Set<CpsDeUnaProvinciaDtm>().AsNoTracking().Where(x => x.IdProvincia == registro.Id);
                GestorDeCpsDeUnaProvincia.Gestor(Contexto, Contexto.Mapeador).BorrarRegistros(a);
            }
        }

        protected override void DespuesDePersistir(ProvinciaDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);

            if (parametros.Operacion == enumTipoOperacion.Modificar || parametros.Operacion == enumTipoOperacion.Eliminar)
                GestorDeMunicipios.EliminarCacheDeMunicipiosPorNombre();
        }

    }

    public static class ExtensionDeProvincias
    {
        public static MunicipioDto CrearMunicipio(this ProvinciaDto provincia, ContextoSe contexto,  string nombre, string dc)
        {
            var municipio = CrearMunicipio(contexto, provincia.Id, nombre, dc);
            return municipio.MapearDto<MunicipioDto, MunicipioDtm>(contexto);
        }

        private static MunicipioDtm CrearMunicipio(ContextoSe contexto, int idProvincia, string nombre, string dc)
        {
            var municipio = contexto.SeleccionarPorPropiedad<MunicipioDtm>(nameof(MunicipioDtm.Nombre), nombre, errorSiNoHay: false);
            if (municipio == null)
            {
                municipio = new MunicipioDtm();
                municipio.Nombre = nombre;
                municipio.DC = dc;
                municipio.IdProvincia = idProvincia;
                municipio = municipio.Insertar(contexto);
            }
            return municipio;
        }
    }
}
