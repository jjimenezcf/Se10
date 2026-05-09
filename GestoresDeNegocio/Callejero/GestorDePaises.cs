using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using Gestor.Errores;
using Utilidades;
using GestoresDeNegocio.TrabajosSometidos;
using System.Reflection;
using Newtonsoft.Json;
using ServicioDeDatos.TrabajosSometidos;

namespace GestoresDeNegocio.Callejero
{

    public class GestorDePaises : GestorDeElementos<ContextoSe, PaisDtm, PaisDto>
    {

        public override enumNegocio Negocio => enumNegocio.Pais;

        class archivoParaImportar
        {
            public string parametro { get; set; }
            public int valor { get; set; }
        }

        public class ltrDeUnPais
        {
            public const string ParametroPais = "csvPais";
        }

        public class MapearPais : Profile
        {
            public MapearPais()
            {
                CreateMap<PaisDtm, PaisDto>();

                CreateMap<PaisDto, PaisDtm>()
                .ForMember(dtm => dtm.FechaCreacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.FechaModificacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdUsuaCrea, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdUsuaModi, dto => dto.Ignore());
            }
        }

        public GestorDePaises(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDePaises Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePaises(contexto, mapeador); ;
        }

        public static PaisDto CrearPais(ContextoSe contexto, string nombre, string iso2, string codigo, string prefijo, string nombreIngles)
        {
            var pais = contexto.SeleccionarPorPropiedad<PaisDtm>(nameof(PaisDtm.Nombre), nombre, errorSiNoHay: false);
            if (pais == null)
            {
                pais = new PaisDtm();
                pais.Nombre = nombre;
                pais.ISO2 = iso2;
                pais.Codigo = codigo;
                pais.Prefijo = prefijo;
                pais.NombreIngles = nombreIngles;
                pais = pais.Insertar(contexto);
            }

            return pais.MapearDto<PaisDto>(contexto);
        }

        internal static PaisDtm LeerPaisPorCodigo(ContextoSe contexto, string iso2Pais, bool errorSiNoHay = true, bool errorSiMasDeUno = true)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var filtros = new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(nameof(PaisDtm.ISO2), enumCriteriosDeFiltrado.igual, iso2Pais) };
            return gestor.LeerRegistroCacheadoPoAk(filtros, aplicarJoin: false, errorSiNoHay, errorSiMasDeUno);
        }

        protected override void AntesDePersistir(PaisDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            if (registro.ISO2 == ltrIsoPaises.Spain) registro.EsUE = true;
        }

        protected override void DespuesDePersistir(PaisDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);

            if (parametros.Operacion == enumTipoOperacion.Modificar || parametros.Operacion == enumTipoOperacion.Eliminar)
              GestorDeMunicipios.EliminarCacheDeMunicipiosPorNombre();
        }

        public static void SometerImportarCallejero(ContextoSe contexto, string parametros)
        {
            if (parametros.IsNullOrEmpty())
                GestorDeErrores.Emitir("No se han proporcionado los parámetros para someter el trabajo de importación");

            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(GestorDePaises).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, "Importar callejero", dll, clase, nameof(SometerImportarCallejero), comunicarFin: false);
            // crear trabajo de usuario

            var tu = GestorDeTrabajosDeUsuario.Crear(contexto, ts, new Dictionary<string, object> { { nameof(TrabajoDeUsuarioDtm.Parametros), parametros } });
            //liberarlo
        }

        public static void ImportarCallejero(EntornoDeTrabajo entorno)
        {
            var archivos = JsonConvert.DeserializeObject<List<archivoParaImportar>>(entorno.TrabajoDeUsuario.Parametros);

            if (archivos.Count == 0)
                GestorDeErrores.Emitir("No se ha sometido ningún fichero a cargar");

            foreach (archivoParaImportar archivo in archivos)
            {
                switch (archivo.parametro)
                {
                    case ltrDeUnPais.ParametroPais:
                        ApiParaImportarCallejero.ImportarFicheroDePaises(entorno, archivo.valor);
                        break;
                    case ltrDeUnaProvincia.ParametroProvincia:
                        ApiParaImportarCallejero.ImportarFicheroDeProvincias(entorno, archivo.valor);
                        break;
                    case ltrDeUnMunicipio.csvMunicipio:
                        ApiParaImportarCallejero.ImportarFicheroDeMunicipios(entorno, archivo.valor);
                        break;
                    case GestorDeTiposDeVia.ltrDeUnTipoDeVia.ParametroTipoDeVia:
                        ApiParaImportarCallejero.ImportarFicheroDeTiposDeVia(entorno, archivo.valor);
                        break;
                    case GestorDeCodigosPostales.ltrDeUnCp.csvCp:
                        ApiParaImportarCallejero.ImportarFicheroDeCodigosPostales(entorno, archivo.valor);
                        break;
                    case ltrCalles.csvCalle:
                        ApiParaImportarCallejero.ImportarFicheroDeCalles(entorno, archivo.valor);
                        break;
                    default:
                        GestorDeErrores.Emitir($"No es valido el parámetro {archivo.parametro} en el proceso {nameof(ImportarCallejero)}");
                        break;
                }
            }
        }

    }

    public static class ExtensionDePaises
    {
        public static ProvinciaDto CrearProvincia(this PaisDto pais, ContextoSe contexto, string nombre, string sigla, string codigo, string prefijo)
        {
            var provincia = CrearProvincia(contexto, pais.Id,nombre,sigla, codigo, prefijo);    
            return provincia.MapearDto<ProvinciaDto>(contexto);
        }

        private static ProvinciaDtm CrearProvincia(ContextoSe contexto, int idPais, string nombre, string sigla, string codigo, string prefijo)
        {
            var provincia = contexto.SeleccionarPorPropiedad<ProvinciaDtm>(nameof(ProvinciaDtm.Nombre), nombre, errorSiNoHay: false);
            if (provincia == null)
            {
                provincia = new ProvinciaDtm();
                provincia.Nombre = nombre;
                provincia.Sigla = sigla;
                provincia.Codigo = codigo;
                provincia.Prefijo = prefijo;
                provincia.IdPais = idPais;
                provincia = provincia.Insertar(contexto);
            }
            return provincia;
        }
    }
}
