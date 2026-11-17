using Gestor.Errores;
using Newtonsoft.Json.Linq;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utilidades;

namespace ServicioDeDatos.Terceros
{

    public enum enumParametrosDeSociedad
    {
        [Description("Id del CG que se usa para crear los puestos de trabajo asociables al administrador")]
        SOCIEDAD_ID_CG_PARA_PT_DE_ADMINISTRADOR,
        [Description("Id del PT que se usa para agrupar permisos del administrador")]
        SOCIEDAD_ID_PT_DE_ADMINISTRADOR,
        [Description("Id del CG que se usa la sociedad para asignar a un trabajador")]
        SOCIEDAD_ID_CG_RRHH,
        [Description("Indica si la sociedad con Id ha de facturar con IRPF")]
        SOCIEDAD_ID_FACTURAR_CON_IRPF,
        [Description("Indica el Id de aplicación para lexnet, asignado por el ministerio")]
        LEXNET_Login,
        [Description("Indica por sociedad (Valor S/N, T/F o true/false) si contabiliza las compras de almacén por el método de la variación de existencias (al gasto) o contra la cuenta de existencias (311)")]
        SOCIEDAD_USA_VARIACION_DE_EXISTENCIA
    }


    public class LEXNET_Login
    {
        public int IdSociedad { get; set; }
        public string IdAplicacion { get; set; }
        public string Nif { get; set; }
        public string CodigoColegio { get; set; }
        public string TipoProfesional { get; set; }
    }

    public class CgDeRRHH
    {
        public int IdSociedad { get; set; }
        public int IdCg { get; set; }
    }

    public class FacturarConIrpf
    {
        public int IdSociedad { get; set; }
        public bool ConIrpf { get; set; }
    }

    public static class ParametrosDeSociedades
    {

        private static readonly string _jsonDeLEXNET_Login = "[{\"IdSociedad\": 0,\"IdAplicacion\": \"0\",\"Nif\": \"0\",\"CodigoColegio\": \"0\",\"TipoProfesional\": \"0\"}]";

        private static readonly string _jsonCgDeRRHH = "[{\"IdSociedad\": 0,\"IdCg\": 0}]";

        private static readonly string _jsonFacturarConIrpf = "[{\"IdSociedad\": 0,\"ConIrpf\": true}]";

        private const string _jsonUsaVariacionDeExistencias = "[{\"IdSociedad\": 0,\"Valor\": \"F\"}]";

        // Si la sociedad contabiliza las compras de almacén por el método de la variación de existencias: la entrega va al gasto de la
        // naturaleza contra albaranes y a fin de ejercicio se regulariza la 311 contra la 611; si no, la entrega va a existencias (311).
        // null si el parámetro no tiene entrada para la sociedad: entonces se deduce de sus preasientos (ver
        // ExtensorDePreasientos.UsaVariacionDeExistencias). Al crear el parámetro, las sociedades que ya generaban preasientos de almacén
        // (que se hacían contra el gasto) entran usándola, así no cambian de método sin el asiento de cuadre (ver CambioDeVariacionDeExistencias)
        public static bool? VariacionDeExistenciasDefinida(this SociedadDtm sociedad) =>
            VariacionDeExistenciasPorSociedad(JsonDeUsaVariacionDeExistencias()).TryGetValue(sociedad.Id, out var usa) ? usa : null;

        // Json del parámetro con la entrada de la sociedad creada o actualizada con el valor indicado; null si no cambia
        public static string JsonDeUsaVariacionDeExistenciasConLaSociedad(int idSociedad, bool usa)
        {
            var json = JsonDeUsaVariacionDeExistencias();
            var jsonArray = JArray.Parse(json);
            var item = jsonArray.FirstOrDefault(x => x["IdSociedad"]?.Value<int>() == idSociedad) as JObject;
            if (item is null)
            {
                item = new JObject { ["IdSociedad"] = idSociedad };
                jsonArray.Add(item);
            }
            item["Valor"] = usa ? "S" : "N";

            var nuevo = jsonArray.ToString(Newtonsoft.Json.Formatting.None);
            return nuevo == JArray.Parse(json).ToString(Newtonsoft.Json.Formatting.None) ? null : nuevo;
        }

        private static string JsonDeUsaVariacionDeExistencias() =>
            enumNegocio.Sociedad.Parametro(enumParametrosDeSociedad.SOCIEDAD_USA_VARIACION_DE_EXISTENCIA, crearParametro: true, valorPorDefecto: ValorInicialDeUsaVariacionDeExistencias()).Valor;

        private static string ValorInicialDeUsaVariacionDeExistencias()
        {
            var jsonArray = JArray.Parse(_jsonUsaVariacionDeExistencias);
            foreach (var idSociedad in Contabilidad.VariablesDePreasiento.SociedadesQueGeneranPreasientos(enumNegocio.Almacen))
                jsonArray.Add(new JObject { ["IdSociedad"] = idSociedad, ["Valor"] = "S" });
            return jsonArray.ToString(Newtonsoft.Json.Formatting.None);
        }

        // Por cada sociedad con entrada en el json del parámetro, si usa la variación de existencias; la entrada 0 es la plantilla
        public static Dictionary<int, bool> VariacionDeExistenciasPorSociedad(string json)
        {
            try
            {
                return JArray.Parse(json.IsNullOrEmpty() ? "[]" : json)
                    .Select(item => new { IdSociedad = item["IdSociedad"].Value<int>(), Valor = item["Valor"]?.Value<string>() })
                    .Where(x => x.IdSociedad > 0)
                    .GroupBy(x => x.IdSociedad)
                    .ToDictionary(g => g.Key, g => g.Last().Valor.EsTrue());
            }
            catch (Exception ex)
            {
                throw Excepciones.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonUsaVariacionDeExistencias}', debe definirlo en el parámetro de negocio '{enumParametrosDeSociedad.SOCIEDAD_USA_VARIACION_DE_EXISTENCIA}'", ex);
            }
        }


        public static LEXNET_Login LexnetLogin(this SociedadDtm sociedad, bool errorSiNoDefinido = true)
        {
            var json = enumNegocio.Sociedad.Parametro(enumParametrosDeSociedad.LEXNET_Login, valorPorDefecto: _jsonDeLEXNET_Login).Valor;

            var datosParaLogearse = ParsearLEXNET_Login(json);
            var loginSociedad = datosParaLogearse.FirstOrDefault(x => x.IdSociedad == sociedad.Id);
            if (loginSociedad == null && errorSiNoDefinido)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDeSociedad.LEXNET_Login.ToString()}' para la sociedad '{sociedad.NIF}' con id '{sociedad.Id}'");

            return loginSociedad;
        }

        public static CentroGestorDtm CgDeRRHH(this SociedadDtm sociedad, ContextoSe contexto, bool errorSiNoDefinido = true)
        {
            var json = enumNegocio.Sociedad.Parametro(enumParametrosDeSociedad.SOCIEDAD_ID_CG_RRHH, valorPorDefecto: _jsonCgDeRRHH).Valor;

            var cgsDeRRHH = ParsearCgDeRRHH(json);
            var cgDeSociedad = cgsDeRRHH.FirstOrDefault(x => x.IdSociedad == sociedad.Id);
            if (cgDeSociedad == null && errorSiNoDefinido)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDeSociedad.SOCIEDAD_ID_CG_RRHH.ToString()}' para la sociedad '{sociedad.NIF}' con id '{sociedad.Id}'");

            if (cgDeSociedad == null)
                return null;

            if (cgDeSociedad.IdCg <= 0 && errorSiNoDefinido)
                GestorDeErrores.Emitir($"El CG definido en el parámetro '{enumParametrosDeSociedad.SOCIEDAD_ID_CG_RRHH.ToString()}' para la sociedad '{sociedad.NIF}' con id '{sociedad.Id}' no es válido");

            var cg = contexto.Set<CentroGestorDtm>().FirstOrDefault(x => x.Id == cgDeSociedad.IdCg);
            
            if (cg == null && errorSiNoDefinido)
                GestorDeErrores.Emitir($"El CG con id '{cgDeSociedad.IdCg}' definido en el parámetro '{enumParametrosDeSociedad.SOCIEDAD_ID_CG_RRHH.ToString()}' para la sociedad '{sociedad.NIF}' con id '{sociedad.Id}' no existe en la base de datos");
            
            if (cg == null) 
                return null;

            return cg;
        }

        public static bool? FacturarConIrpf(this SociedadDtm sociedad, bool errorSiNoDefinido = true)
        {
            var json = enumNegocio.Sociedad.Parametro(enumParametrosDeSociedad.SOCIEDAD_ID_FACTURAR_CON_IRPF, valorPorDefecto: _jsonFacturarConIrpf).Valor;

            var sociedades = ParsearFacturarConIrpf(json);
            var facturar = sociedades.FirstOrDefault(x => x.IdSociedad == sociedad.Id);
            if (facturar == null && errorSiNoDefinido)
                enumNegocio.Sociedad.IndicarQueFaltaDefinirElParámetro(enumParametrosDeSociedad.SOCIEDAD_ID_FACTURAR_CON_IRPF);

            return facturar == null ? null : facturar.ConIrpf;
        }

        private static List<LEXNET_Login> ParsearLEXNET_Login(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new LEXNET_Login
                {
                    IdSociedad = item["IdSociedad"].Value<int>(),
                    IdAplicacion = item["IdAplicacion"].Value<string>(),
                    Nif = item["Nif"].Value<string>(),
                    CodigoColegio = item["CodigoColegio"].Value<string>(),
                    TipoProfesional = item["TipoProfesional"].Value<string>()
                }).ToList();
            }
            catch (Exception ex)
            {
              throw Excepciones.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeLEXNET_Login}', debe definirlo en el parámetro de negocio '{enumParametrosDeSociedad.LEXNET_Login.ToString()}'", ex);
            }
        }
        private static List<CgDeRRHH> ParsearCgDeRRHH(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new CgDeRRHH
                {
                    IdSociedad = item["IdSociedad"].Value<int>(),
                    IdCg = item["IdCg"].Value<int>()
                }).ToList();
            }
            catch (Exception ex)
            {
                throw Excepciones.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonCgDeRRHH}', debe definirlo en el parámetro de negocio '{enumParametrosDeSociedad.SOCIEDAD_ID_CG_RRHH.ToString()}'", ex);
            }
        }



        private static List<FacturarConIrpf> ParsearFacturarConIrpf(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new FacturarConIrpf
                {
                    IdSociedad = item["IdSociedad"].Value<int>(),
                    ConIrpf = item["ConIrpf"].Value<bool>()
                }).ToList();
            }
            catch (Exception ex)
            {
                throw Excepciones.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonFacturarConIrpf}', debe definirlo en el parámetro de negocio '{enumParametrosDeSociedad.SOCIEDAD_ID_FACTURAR_CON_IRPF.ToString()}'", ex);
            }
        }
    }
}
