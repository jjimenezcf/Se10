using AutoMapper;
using ServicioDeDatos.Entorno;
using ServicioDeDatos;
using ModeloDeDto.Entorno;
using GestorDeElementos;
using Utilidades;
using System.IO;
using Gestor.Errores;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Utilidades;

namespace GestoresDeNegocio.Entorno
{

    public class GestorDeVariables : GestorDeElementos<ContextoSe, VariableDtm, VariableDto>
    {
        public static readonly string RutaBase = enumRutas.RutaBase;
        public static readonly string RutaBaseConDestino = $@"{RutaBase}\wwwroot";
        public static readonly string RutaDeDescarga = enumRutas.RutaDeDescarga;
        public static readonly string RutaDeExportaciones = $@"{CacheDeVariable.Cfg_ServidorDeArchivos}\Exportaciones";
        public static readonly string RutaDeJson = enumRutas.RutaDeJson;
        public static readonly string RutaDeAgendas = enumRutas.RutaDeAgendas;
        //public static readonly string RutaDeBinarios = $@"..\{RutaBase}\bin";

        public override enumNegocio Negocio => enumNegocio.Variable;

        public class MapearVariables : Profile
        {
            public MapearVariables()
            {
                CreateMap<VariableDtm, VariableDto>();
                CreateMap<VariableDto, VariableDtm>();
            }
        }

        public GestorDeVariables(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {
        }
        internal static GestorDeVariables Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeVariables(contexto, mapeador);
        }

        protected override void AntesDePersistir(VariableDtm variable, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(variable, parametros);

            if (parametros.EsUnaPeticion && (parametros.Insertando || parametros.Eliminando))
                GestorDeErrores.Emitir($"No se puede crear y eliminar variables desde la interface");

            if (variable.PropiedadCambiada<string>(nameof(VariableDtm.Nombre), parametros))
                GestorDeErrores.Emitir($"El nombre de la variable '{((VariableDtm)parametros.registroEnBd).Nombre}' no se puede cambiar por el usuario");


        }

        protected override void DespuesDePersistir(VariableDtm variable, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(variable, parametros);
            if (!parametros.Insertando)
            {
                if (variable.Nombre == Variable.CFG_Usar_Cache && !variable.Valor.EsTrue())
                {
                    CacheDeVariable.Modificar(Variable.Cfg_Usar_Cache_Descriptores_Json, "N");
                    CacheDeVariable.Modificar(Variable.CFG_Usar_Cache_En_EpLeerPorId, "N");
                    ServicioDeCaches.UsaCacheParaRenderizar = false;
                }

                if (variable.Nombre == Variable.IA_Usada)
                    ServicioDeCaches.EliminarCache(CacheDe.RenderCrud);

                CacheDeVariable.BorrarCache(((VariableDtm)parametros.registroEnBd).Nombre);
                ServicioDeCaches.EliminarCache(CacheDe.Valores);
            }

            CacheDeVariable.Cache.Clear();
            if (variable.Nombre.Equals(ApiDeEnsamblados.CFG_Usar_Cache)) { ServicioDeCaches.UsaCache = variable.Valor.EsTrue(); }
            if (variable.Nombre.Equals(ApiDeEnsamblados.Cfg_Usar_Cache_Descriptores_Json)) { ServicioDeCaches.UsaCacheParaRenderizar = variable.Valor.EsTrue(); }

            if (variable.Nombre.Equals(ApiDeEnsamblados.CFG_Ruta_Ficheros_De_Debug))
            {
                if (!Directory.Exists(variable.Valor)) Directory.CreateDirectory(variable.Valor);
                TrazaSql.Ruta = variable.Valor;
            }

        }

        protected override void DespuesDeMapearElElemento(VariableDtm registro, VariableDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            if (elemento.Nombre.Equals(Variable.Cfg_Tipo_De_Archivador_De_BackUp, System.StringComparison.CurrentCultureIgnoreCase))
                elemento.ModoDeAcceso = ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor;
        }

        internal static string LeerValorDeVariable(ContextoSe contextoSe, string variable, bool emitirErrorSiNoExiste)
        {
            var indice = variable;
            if (CacheDeVariable.Cache.ContainsKey(indice))
                return (string)CacheDeVariable.Cache[indice];

            var gestor = Gestor(contextoSe, contextoSe.Mapeador);
            var registro = gestor.LeerRegistroCacheado(nameof(VariableDtm.Nombre), variable, emitirErrorSiNoExiste, errorSiHayMasDeUno: true, aplicarJoin: false);

            return registro == null ? null : registro.Valor;
        }

        private static VariableDtm CrearVariable(ContextoSe contexto, string variable, string descripcion, string valor)
        {
            var v = new VariableDtm();
            v.Nombre = variable;
            v.Valor = valor;
            v.Descripcion = descripcion;
            var gestor = Gestor(contexto, contexto.Mapeador);
            v = gestor.PersistirRegistro(v, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            return v;
        }

        public static string VariableDeRutaDeBinarios(ContextoSe contexto)
        {
            var ruta = LeerValorDeVariable(contexto, Variable.CFG_Ruta_De_Binarios, false);
            if (ruta.IsNullOrEmpty())
            {
                var rutaBinarios = ApiDeEnsamblados.RutaDeBinarios();
                ruta = CrearVariable(contexto, Variable.CFG_Ruta_De_Binarios, "Directorio donde se genera los binarios del sistema", rutaBinarios).Valor;
            }

            return ruta;
        }

        public static void CrearSiNoExiste(ContextoSe contexto, string variable, string descripcion, string valor)
        {
            if (LeerValorDeVariable(contexto, variable, emitirErrorSiNoExiste: false).IsNullOrEmpty()) CrearVariable(contexto, variable, descripcion, valor);
        }
    }
}
