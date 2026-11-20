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
        [Description("Obsoleto: el método de contabilización de las compras se indica en cada almacén. Se lee sólo para orientar al definir la información contable de los almacenes")]
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

        // El método de contabilización de las compras de almacén se definía por sociedad en el parámetro SOCIEDAD_USA_VARIACION_DE_EXISTENCIA;
        // ahora es de cada almacén (ver AlmacenDtm.UsaVariacionDeExistencias). Sólo se lee para orientar al definir la información contable
        // de los almacenes que aún no la tienen (ver ExtensorDeAlmacenes.ValidarInformacionContable): null si no existe el parámetro, no
        // tiene entrada para la sociedad o no se puede leer
        public static bool? UsabaVariacionDeExistencias(this SociedadDtm sociedad)
        {
            try
            {
                var json = enumNegocio.Sociedad.Parametro(enumParametrosDeSociedad.SOCIEDAD_USA_VARIACION_DE_EXISTENCIA, emitirError: false)?.Valor;
                if (json.IsNullOrEmpty())
                    return null;

                var entrada = JArray.Parse(json).FirstOrDefault(item => item["IdSociedad"]?.Value<int>() == sociedad.Id);
                return entrada is null ? null : entrada["Valor"]?.Value<string>().EsTrue();
            }
            catch
            {
                return null;
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
