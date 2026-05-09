using Gestor.Errores;
using Newtonsoft.Json.Linq;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utilidades;

namespace ServicioDeDatos.Contabilidad
{

    public enum enumSistemaContable
    {
        [Description("Contablidad Ncs")]
        NCS
    }

    public enum enumEtapasDePreasiento
    {
        [Description("Ids de estados en los un preasiento está pendiente")]
        SPR_Etapa_Pendiente,
        [Description("Ids de estados en los un preasiento está contabilizado")]
        SPR_Etapa_Contabilizado,
        [Description("Ids de estados en los que un preasiento está cancelado")]
        SPR_Etapa_Cancelado,
        [Description("Ids de estados en los que un preasiento está anulado en contabilidad")]
        SPR_Etapa_Anulado
    }

    public enum enumParametrosDePreasiento
    {
        [Description("Relación entre un id de tipo de preasiento y el código de diario a usar")]
        SPR_Codigos_De_Diario,
        [Description("Relación entre la sociedad, el negocio, el id del tipo y el código de actividad a usar")]
        SPR_Codigos_De_Actividad,
        [Description("Relación entre un id de cuenta bancaria y el código de cuenta contable")]
        SPR_CtaBancaria_CtaContable,
        [Description("código de cuenta contable de caja")]
        SPR_Cuenta_De_Caja,
        [Description("Relación entre un id de tarjeta y el código de cuenta contable")]
        SPR_Tarjeta_CtaContable,
        [Description("Relación entre un id de tipo de preasiento y el origen de su generación")]
        SPR_Negocio_Origen,
        [Description("Indica para un Id de sociedad cual es el Id del CG documental")]
        SPR_Sociedad_CG_Documental,
        [Description("Indica que id de archivador usar para almacenar el fichero de los preasientos generados")]
        SPR_Tipo_De_Archivador,
        [Description("Define los campos de la acción necesaria para contabilizar en un sistema informático (depende de la sociedad)")]
        SPR_Contabilizar_En,
        [Description("Define los campos de la acción necesaria para regenerar un lote contable (depende de la sociedad)")]
        SPR_RegenerarLote_Para,
        [Description("Define códigos contables por naturaleza")]
        SPR_CodigosPorNaturaleza,
        [Description("Define los campos de la acción necesaria para generar el fichero de cuentas contables de terceros (depende de la sociedad)")]
        SPR_CuentasDeTerceros_En,
        [Description("Define los planes contables de las sociedades gestionadas")]
        SPR_Plan_Contable,
        [Description("Indica si la sociedad usa ROI")]
        SPR_Usa_Roi,
        [Description("lista de los ids de naturalezas que indican que son materiales")]
        SPR_Naturalezas_De_Materiales,
        [Description("lista de los ids de naturalezas que indican que son suministros")]
        SPR_Naturalezas_De_Suministros,
        [Description("lista de los ids de naturalezas que indican que es arrendamiento")]
        SPR_Naturalezas_De_Arrendamiento,
        [Description("lista de ids de usuarios que pueden generar preasientos de manera manual para su contabilización")]
        SPR_Usuarios_Con_Permiso_Para_Generar,
        [Description("Indica si ha de generar asientos de pagos")]
        SPR_Generar_Preasiento_De_Pago,
        [Description("Indica si ha de generar asientos de facturas emitidas")]
        SPR_Generar_Preasiento_De_FacturaEmitida,
        [Description("Indica si ha de generar asientos de facturas recibidas")]
        SPR_Generar_Preasiento_De_FacturaRecibida,
        [Description("Indica si ha de generar asientos de cobros")]
        SPR_Generar_Preasiento_De_Cobro,
        [Description("Indica el concepto de gatos en la Estimación Directa de NCS")]
        SPR_NCS_Conceptos_De_Gasto
    }

    internal class CtaContablePorCtaBancaria
    {
        internal int IdCuentaBancaria { get; set; }
        internal string CodigoDeCuenta { get; set; }
    }
    internal class TarjetaPorCtaBancaria
    {
        internal int IdTarjeta { get; set; }
        internal string CodigoDeCuenta { get; set; }
    }
    internal class UsaPreasiento
    {
        internal int IdSociedad { get; set; }
        internal string Valor { get; set; }
    }


    public class CodigosDeDiario
    {
        public int IdTipoPreasiento { get; set; }
        public string CodigoDeDiario { get; set; }
    }


    public class ActividadesEstimacionDirecta
    {
        public int IdSociedad { get; set; }
        public int IdNegocio { get; set; }
        public int IdTipo { get; set; }
        public string Actividad { get; set; }
    }

    internal class ConceptosDeGastoEnNcsDeEstimacionDirecta
    {
        internal int IdNaturaleza { get; set; }
        internal string Concepto { get; set; }
    }

    public class Negocios
    {
        public int IdTipoPreasiento { get; set; }
        public int IdNegocio { get; set; }
    }
    public class ContabilizarEn
    {
        public int IdSociedad { get; set; }
        public string Metodo { get; set; }
        public string Nombre { get; set; }
    }

    public class RegenerarLotePara
    {
        public int IdSociedad { get; set; }
        public string Metodo { get; set; }
        public string Nombre { get; set; }
    }

    public class CodigosPorNaturaleza
    {
        public int IdNaturaleza { get; set; }
        public string CodigoConcepto { get; set; }
        public string CodigoIva { get; set; }
    }

    public class CuentasDeTerceros
    {
        public int IdSociedad { get; set; }
        public string Metodo { get; set; }
        public string Nombre { get; set; }
    }

    public class PlanContable
    {
        public int IdSociedad { get; set; }
        public string IdPlanContable { get; set; }
    }
    public class UsaRoi
    {
        public int IdSociedad { get; set; }
        public string LoUsa { get; set; }
    }
    public class CGDocumentalDeLaSociedad
    {
        public int IdSociedad { get; set; }
        public int IdCgDocumental { get; set; }
    }

    public static class VariablesDePreasiento
    {
        private static readonly string _jsonDeSPR_CtaBancaria_CtaContable = $"[{{\"IdCuentaBancaria\": 0,\"CodigoDeCuenta\": \"{ltrCuenta.Banco}\"}}]";
        private static readonly string _jsonDeSPR_Tarjeta_CtaContable = $"[{{\"IdTarjeta\": 0,\"CodigoDeCuenta\": \"{ltrCuenta.Tarjeta}\"}}]";
        private const string _jsonDeSPR_Codigos_De_Diario = "[{\"IdTipoPreasiento\": 0,\"CodigoDeDiario\": \"0\"}]";
        private static readonly string _jsonDeSPR_Codigos_De_Actividad = $"[{{\"IdSociedad\": 0,\"IdNegocio\": 0,\"IdTipo\": 0,\"Actividad\": \"0\"}}]";
        private const string _jsonDeSPR_Negocio_Origen = "[{\"IdTipoPreasiento\": 0,\"IdNegocio\": 0}]";
        private const string _jsonDeSPR_Sociedad_Cg_Documental = "[{\"IdSociedad\": 0,\"IdCgDocumental\": 0}]";
        private const string _jsonDeSPR_Contabilizar_En = "[{\"IdSociedad\": 0,\"Metodo\": \"Contabilizar_en\",\"Nombre\": \"Contabilizar en ...\"}]";
        private const string _jsonDeSPR_RegenerarLote_Para = "[{\"IdSociedad\": 0,\"Metodo\": \"RegenerarLote_Para\",\"Nombre\": \"Regenerar lote para ...\"}]";
        private const string _jsonDeSPR_CuentasDeTercero_En = "[{\"IdSociedad\": 0,\"Metodo\": \"MetodoQueGenera\",\"Nombre\": \"Cuentas contables de tercero en ...\"}]";
        private const string _jsonDeSPR_CodigosPorNaturaleza = "[{\"IdNaturaleza\": 0,\"CodigoConcepto\": \"\",\"CodigoIva\": \"\"}]";
        private const string _jsonDeSPR_Sociedad_PlanContable = "[{\"IdSociedad\": 0,\"IdPlanContable\": \"0\"}]";
        private const string _jsonDeSPR_Sociedad_Usa_Roi = "[{\"IdSociedad\": 0,\"LoUsa\": \"-1\"}]";
        private const string _SPR_Naturalezas_Contables_Materiales = Literal.Cero;
        private const string _SPR_Naturalezas_Contables_Suministros = Literal.Cero;
        private const string _SPR_Naturalezas_Contables_Arrendamiento = Literal.Cero;
        private static readonly string _jsonDeSPR_Genera_Preasiento = $"[{{\"IdSociedad\": 0,\"Valor\": \"S\"}}]";
        private static readonly string _jsonDeSPR_NCS_Concepto_Del_Gasto = $"[{{\"IdNaturaleza\": 0,\"Concepto\": \"\"}}]";

        private static string etapaDePendiente => enumNegocio.Preasiento.Parametro(enumEtapasDePreasiento.SPR_Etapa_Pendiente).Valor;
        private static string etapaDeContabilizado => enumNegocio.Preasiento.Parametro(enumEtapasDePreasiento.SPR_Etapa_Contabilizado).Valor;
        private static string etapaDeCancelado => enumNegocio.Preasiento.Parametro(enumEtapasDePreasiento.SPR_Etapa_Cancelado).Valor;
        private static string etapaDeAnulado => enumNegocio.Preasiento.Parametro(enumEtapasDePreasiento.SPR_Etapa_Anulado).Valor;


        public static string Estados(this enumEtapasDePreasiento etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDePreasiento.SPR_Etapa_Contabilizado: estados = etapaDeContabilizado; break;
                case enumEtapasDePreasiento.SPR_Etapa_Pendiente: estados = etapaDePendiente; break;
                case enumEtapasDePreasiento.SPR_Etapa_Cancelado: estados = etapaDeCancelado; break;
                case enumEtapasDePreasiento.SPR_Etapa_Anulado: estados = etapaDeAnulado; break;
            }

            return estados.IsNullOrEmpty() ? enumNegocio.Preasiento.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnLaEtapa(this PreasientoDtm preasiento, enumEtapasDePreasiento etapa) => etapa.Lista().Contains(preasiento.IdEstado);

        public static List<int> Lista(this enumEtapasDePreasiento etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static bool ContieneLaEtapa(this List<enumEtapasDePreasiento> etapas, enumEtapasDePreasiento etapa) => etapas.Contains(etapa);

        public static bool EstaEnAlgunaDeLasEtapa(this PreasientoDtm preasiento, List<enumEtapasDePreasiento> etapas)
        {
            var etapasDeLacircuito = preasiento.Etapas();
            foreach (var etapa in etapas)
                if (etapasDeLacircuito.Contains(etapa)) return true;
            return false;
        }

        public static (List<int> estados, enumEtapasDePreasiento etapa) EstadosDeLaEtapa(this enumEtapasDePreasiento etapa) => (etapa.Lista(), etapa);


        public static List<enumEtapasDePreasiento> Etapas(this PreasientoDtm preasiento)
        {
            var etapas = new List<enumEtapasDePreasiento>();
            if (preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Pendiente))
                etapas.Add(enumEtapasDePreasiento.SPR_Etapa_Pendiente);
            if (preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Contabilizado))
                etapas.Add(enumEtapasDePreasiento.SPR_Etapa_Contabilizado);
            if (preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Cancelado))
                etapas.Add(enumEtapasDePreasiento.SPR_Etapa_Cancelado);
            if (preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Anulado))
                etapas.Add(enumEtapasDePreasiento.SPR_Etapa_Anulado);
            return etapas;
        }

        public static string CadenaDeEtapas(this PreasientoDtm preasiento) => string.Join(Simbolos.separadorDeEtapas, preasiento.Etapas());

        public static enumEtapasDePreasiento Etapa(this PreasientoDtm preasiento)
        {
            var etapas = preasiento.Etapas();
            if (etapas.Count == 0)
                Excepciones.EtapaNoDefinida(enumNegocio.Preasiento, preasiento.Propiedad<EstadoDtm>(typeof(EstadoDeUnPreasientoDtm)).Nombre);

            if (etapas.Count > 1)
                Excepciones.MasDeUnaEtapa(enumNegocio.Preasiento, preasiento.Referencia, etapas);

            return etapas[0];
        }

        public static string Nombre(this enumEtapasDePreasiento etapa, bool minusculas = true)
        {
            switch (etapa)
            {
                case enumEtapasDePreasiento.SPR_Etapa_Pendiente: return minusculas ? "pendiente" : "Pendiente";
                case enumEtapasDePreasiento.SPR_Etapa_Contabilizado: return minusculas ? "contabilizado" : "Contabilizado";
                case enumEtapasDePreasiento.SPR_Etapa_Cancelado: return minusculas ? "cancelado" : "Cancelado";
                case enumEtapasDePreasiento.SPR_Etapa_Anulado: return minusculas ? "anulado" : "Anulado";
            }
            return etapa.ToString();
        }


        public static ContabilizarEn ContabilizarEn(this SociedadDtm sociedad, bool errorSiNoDefinido = true)
        {
            var json = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Contabilizar_En, valorPorDefecto: _jsonDeSPR_Contabilizar_En).Valor;

            var sistemasContables = ParsearContabilizarEn(json);
            var contabilizarEn = sistemasContables.FirstOrDefault(x => x.IdSociedad == sociedad.Id);
            if (contabilizarEn == null && errorSiNoDefinido)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDePreasiento.SPR_Contabilizar_En.ToString()}' para la sociedad '{sociedad.NIF}' con id '{sociedad.Id}'");

            return contabilizarEn;
        }

        public static RegenerarLotePara RegenerarLotePara(this SociedadDtm sociedad)
        {
            var json = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_RegenerarLote_Para, valorPorDefecto: _jsonDeSPR_RegenerarLote_Para).Valor;

            var metodParaRegenerar = ParsearRegenerarLotePara(json);
            var regenerarPara = metodParaRegenerar.FirstOrDefault(x => x.IdSociedad == sociedad.Id);
            if (regenerarPara == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDePreasiento.SPR_RegenerarLote_Para.ToString()}' para la sociedad '{sociedad.NIF}' con id '{sociedad.Id}'");

            return regenerarPara;
        }

        public static CodigosPorNaturaleza CodigosPorNaturaleza(this NaturalezaDtm naturaleza)
        {
            var json = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_CodigosPorNaturaleza, valorPorDefecto: _jsonDeSPR_CodigosPorNaturaleza).Valor;

            var codigosPorNaturaleza = ParsearCodigosPorNaturaleza(json);
            var codigoPorNaturaleza = codigosPorNaturaleza.FirstOrDefault(x => x.IdNaturaleza == naturaleza.Id);
            if (codigoPorNaturaleza == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDePreasiento.SPR_CodigosPorNaturaleza.ToString()}' para la naturaleza '{naturaleza.Nombre}' con Id '{naturaleza.Id}'");

            return codigoPorNaturaleza;
        }

        private static List<CodigosPorNaturaleza> ParsearCodigosPorNaturaleza(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new CodigosPorNaturaleza
                {
                    IdNaturaleza = item["IdNaturaleza"].Value<int>(),
                    CodigoConcepto = item["CodigoConcepto"].Value<string>(),
                    CodigoIva = item["CodigoIva"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeSPR_CodigosPorNaturaleza}', debe definirlo en el parámetro de negocio '{enumParametrosDePreasiento.SPR_CodigosPorNaturaleza.ToString()}'", ex);
            }
            return null;
        }

        public static CuentasDeTerceros CuentasDeTerceros(this SociedadDtm sociedad)
        {
            var json = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_CuentasDeTerceros_En, valorPorDefecto: _jsonDeSPR_CuentasDeTercero_En).Valor;

            var cuentasDeTerceros = ParsearCuentasDeTercerosEn(json);
            var cuentasDeTercerosEn = cuentasDeTerceros.FirstOrDefault(x => x.IdSociedad == sociedad.Id);
            if (cuentasDeTercerosEn == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDePreasiento.SPR_CuentasDeTerceros_En.ToString()}' para la sociedad '{sociedad.NIF}'");

            return cuentasDeTercerosEn;
        }
                

        private static List<CuentasDeTerceros> ParsearCuentasDeTercerosEn(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new CuentasDeTerceros
                {
                    IdSociedad = item["IdSociedad"].Value<int>(),
                    Metodo = item["Metodo"].Value<string>(),
                    Nombre = item["Nombre"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeSPR_Contabilizar_En}', debe definirlo en el parámetro de negocio '{enumParametrosDePreasiento.SPR_Contabilizar_En.ToString()}'", ex);
            }
            return null;
        }

        private static List<ContabilizarEn> ParsearContabilizarEn(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new ContabilizarEn
                {
                    IdSociedad = item["IdSociedad"].Value<int>(),
                    Metodo = item["Metodo"].Value<string>(),
                    Nombre = item["Nombre"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeSPR_Contabilizar_En}', debe definirlo en el parámetro de negocio '{enumParametrosDePreasiento.SPR_Contabilizar_En.ToString()}'", ex);
            }
            return null;
        }

        private static List<RegenerarLotePara> ParsearRegenerarLotePara(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new RegenerarLotePara
                {
                    IdSociedad = item["IdSociedad"].Value<int>(),
                    Metodo = item["Metodo"].Value<string>(),
                    Nombre = item["Nombre"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeSPR_RegenerarLote_Para}', debe definirlo en el parámetro de negocio '{enumParametrosDePreasiento.SPR_RegenerarLote_Para.ToString()}'", ex);
            }
            return null;
        }

        public static PlanContable PlanContable(this SociedadDtm sociedad)
        {
            var json = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Plan_Contable, valorPorDefecto: _jsonDeSPR_Sociedad_PlanContable).Valor;

            var planesContables = ParsearPlanesContables(json);
            var planContable = planesContables.FirstOrDefault(x => x.IdSociedad == sociedad.Id);
            if (planContable == null)
                GestorDeErrores.Emitir($"Ha de configurar el plan contable '{enumParametrosDePreasiento.SPR_Plan_Contable.ToString()}' para la sociedad '{sociedad.NIF}'");

            return planContable;
        }

        public static bool UsaRoi(this SociedadDtm sociedad)
        {
            var json = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Usa_Roi, valorPorDefecto: _jsonDeSPR_Sociedad_Usa_Roi).Valor;

            var quienUsaRoi = ParsearUsaRoi(json);
            var usaRoi = quienUsaRoi.FirstOrDefault(x => x.IdSociedad == sociedad.Id);
            if (usaRoi == null || (usaRoi.LoUsa != "S" && usaRoi.LoUsa != "N"))
                GestorDeErrores.Emitir($"Ha de configurar si la sociedad '{sociedad.NIF}' usa Roi, valores válidos: S/N");

            return usaRoi.LoUsa == "S";
        }

        public static bool EsSuministro(NaturalezaDtm naturaleza)
        =>
        NaturalezasDeSuministros().Contains(naturaleza.Id);

        public static bool EsMaterial(NaturalezaDtm naturaleza)
        =>
        NaturalezasDeMateriales().Contains(naturaleza.Id);

        public static bool EsArrendamiento(NaturalezaDtm naturaleza)
        =>
        NaturalezasDeArrendamiento().Contains(naturaleza.Id);

        private static List<int> NaturalezasDeMateriales()
        {
            var lista = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Naturalezas_De_Materiales, valorPorDefecto: _SPR_Naturalezas_Contables_Materiales).Valor.ToLista<int>();
            return lista;
        }

        private static List<int> NaturalezasDeArrendamiento()
        {
            var lista = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Naturalezas_De_Arrendamiento, valorPorDefecto: _SPR_Naturalezas_Contables_Arrendamiento).Valor.ToLista<int>();
            return lista;
        }

        private static List<int> NaturalezasDeSuministros()
        {
            var lista = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Naturalezas_De_Suministros, valorPorDefecto: _SPR_Naturalezas_Contables_Suministros).Valor.ToLista<int>();
            return lista;
        }

        public static void ValidarNaturalezasContables()
        {
            var naturalezas = NaturalezasDeMateriales();
            if (naturalezas.Count == 0 || (naturalezas.Count == 1 && naturalezas[0] == 0))
            {
                GestorDeErrores.Emitir($"Debe definir las clases de naturalezas contables en el parámetro '{enumParametrosDePreasiento.SPR_Naturalezas_De_Materiales.ToString()}'");
            }
            naturalezas = NaturalezasDeSuministros();
            if (naturalezas.Count == 0 || (naturalezas.Count == 1 && naturalezas[0] == 0))
            {
                GestorDeErrores.Emitir($"Debe definir las clases de naturalezas contables en el parámetro '{enumParametrosDePreasiento.SPR_Naturalezas_De_Suministros.ToString()}'");
            }
            naturalezas = NaturalezasDeArrendamiento();
            if (naturalezas.Count == 0 || (naturalezas.Count == 1 && naturalezas[0] == 0))
            {
                GestorDeErrores.Emitir($"Debe definir las clases de naturalezas contables en el parámetro '{enumParametrosDePreasiento.SPR_Naturalezas_De_Arrendamiento.ToString()}'");
            }
        }

        private static List<PlanContable> ParsearPlanesContables(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new PlanContable
                {
                    IdSociedad = item["IdSociedad"].Value<int>(),
                    IdPlanContable = item["IdPlanContable"].Value<string>()
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeSPR_Sociedad_PlanContable}', debe definirlo en el parámetro de negocio '{enumParametrosDePreasiento.SPR_Plan_Contable.ToString()}'", ex);
            }
            return null;
        }

        private static List<UsaRoi> ParsearUsaRoi(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new UsaRoi
                {
                    IdSociedad = item["IdSociedad"].Value<int>(),
                    LoUsa = item["LoUsa"].Value<string>()
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeSPR_Sociedad_Usa_Roi}', debe definirlo en el parámetro de negocio '{enumParametrosDePreasiento.SPR_Usa_Roi.ToString()}'", ex);
            }
            return null;
        }
        public static int IdCgDocumentalDeSpr(this SociedadDtm sociedad)
        {
            var json = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Sociedad_CG_Documental, valorPorDefecto: _jsonDeSPR_Sociedad_Cg_Documental).Valor;

            var cgsDocumentales = ParsearCGDocumental(json);
            var cgDocumental = cgsDocumentales.FirstOrDefault(x => x.IdSociedad == sociedad.Id);
            if (cgDocumental == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDePreasiento.SPR_Sociedad_CG_Documental.ToString()}' para la sociedad '{sociedad.NIF}'");

            return cgDocumental.IdCgDocumental;
        }

        private static List<CGDocumentalDeLaSociedad> ParsearCGDocumental(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new CGDocumentalDeLaSociedad
                {
                    IdSociedad = item["IdSociedad"].Value<int>(),
                    IdCgDocumental = item["IdCgDocumental"].Value<int>()
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeSPR_Sociedad_Cg_Documental}', debe definirlo en el parámetro de negocio '{enumParametrosDePreasiento.SPR_Sociedad_CG_Documental.ToString()}'", ex);
            }
            return null;
        }

        public static int IdNegocio(this ITipoDeElementoDtm tipo)
        {
            var json = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Negocio_Origen, valorPorDefecto: _jsonDeSPR_Negocio_Origen).Valor;

            var relaciones = ParsearNegocios(json);
            var relacion = relaciones.FirstOrDefault(x => x.IdTipoPreasiento == tipo.Id);
            if (relacion == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDePreasiento.SPR_Codigos_De_Diario.ToString()}' para el id de tipo preasiento '{tipo.Id}'");

            return relacion.IdNegocio;
        }


        private static List<Negocios> ParsearNegocios(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new Negocios
                {
                    IdTipoPreasiento = item["IdTipoPreasiento"].Value<int>(),
                    IdNegocio = item["IdNegocio"].Value<int>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeSPR_Negocio_Origen}', debe definirlo en el parámetro de negocio '{enumParametrosDePreasiento.SPR_Negocio_Origen.ToString()}'", ex);
            }
            return null;
        }


        public static string CodigoDeDiario(this ITipoDeElementoDtm tipo, ContextoSe contexto)
        {
            var json = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Codigos_De_Diario, crearParametro: true, valorPorDefecto: _jsonDeSPR_Codigos_De_Diario).Valor;

            var relaciones = ParsearDiarios(json);
            var relacion = relaciones.FirstOrDefault(x => x.IdTipoPreasiento == tipo.Id);
            if (relacion == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDePreasiento.SPR_Codigos_De_Diario.ToString()}' para el id de tipo preasiento '{tipo.Id}'");

            return relacion.CodigoDeDiario;
        }

        public static string CodigoDeActividad(this SociedadDtm sociedad, int idNegocio, int idTipo)
        {
            var json = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Codigos_De_Actividad, crearParametro: true, valorPorDefecto: _jsonDeSPR_Codigos_De_Actividad).Valor;

            var relaciones = ParsearActividades(json);
            var relacion = relaciones.FirstOrDefault(x => x.IdSociedad == sociedad.Id && x.IdNegocio == idNegocio && x.IdTipo == idTipo);
            if (relacion == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDePreasiento.SPR_Codigos_De_Actividad.ToString()}' para el id de sociedad '{sociedad.Id}',  el id de negocio '{idNegocio}' y id de tipo '{idTipo}'");

            return relacion.Actividad;
        }

        public static string ConceptoDeGasto(this NaturalezaDtm naturaleza)
        {
            var json = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_NCS_Conceptos_De_Gasto, crearParametro: true, valorPorDefecto: _jsonDeSPR_NCS_Concepto_Del_Gasto).Valor;

            var relaciones = ParsearConceptoDeGastoEnNcs(json);
            var relacion = relaciones.FirstOrDefault(x => x.IdNaturaleza == naturaleza.Id);
            
            if (relacion == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDePreasiento.SPR_NCS_Conceptos_De_Gasto.ToString()}' para el id de naturaleza '{naturaleza.Id}'");

            if (relaciones.Count() > 1)
                GestorDeErrores.Emitir($"La naturaleza '{naturaleza.Nombre}' tiene más de una asignación en el parámetro '{enumParametrosDePreasiento.SPR_NCS_Conceptos_De_Gasto.ToString()}', sólo puede tener un concepto");

            if (relacion.Concepto.Entero() <= 0 || relacion.Concepto.Entero() > 20)
                GestorDeErrores.Emitir($"El concepto asignado en el parámetro '{enumParametrosDePreasiento.SPR_NCS_Conceptos_De_Gasto.ToString()}' para el id de naturaleza '{naturaleza.Id}' no es válido, ha de ser un valor numérico mayor de '0' y menor de '21'");

            return relacion.Concepto;
        }

        public static List<ActividadesEstimacionDirecta> ActividadesDeEstimacionDirecto(this SociedadDtm sociedad)
        {
            var json = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Codigos_De_Actividad, crearParametro: true, valorPorDefecto: _jsonDeSPR_Codigos_De_Actividad).Valor;

            var relaciones = ParsearActividades(json);
            if (relaciones.Count(x => x.IdSociedad == sociedad.Id) == 0)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDePreasiento.SPR_Codigos_De_Actividad.ToString()}' para el id de sociedad '{sociedad.Id}'");

            return relaciones;
        }

        public static bool UsaPreasientos(this SociedadDtm sociedad, ContextoSe contexto, enumNegocio negocio)
        {
            var parametro = negocio == enumNegocio.Pago
                   ? enumParametrosDePreasiento.SPR_Generar_Preasiento_De_Pago
                   : negocio == enumNegocio.FacturaEmitida 
                   ? enumParametrosDePreasiento.SPR_Generar_Preasiento_De_FacturaEmitida
                   : negocio == enumNegocio.FacturaRecibida
                   ? enumParametrosDePreasiento.SPR_Generar_Preasiento_De_FacturaRecibida
                   : enumParametrosDePreasiento.SPR_Generar_Preasiento_De_Cobro;

            var json = enumNegocio.Preasiento.Parametro(parametro, crearParametro: true, valorPorDefecto: _jsonDeSPR_Genera_Preasiento).Valor;
            if (json == _jsonDeSPR_Genera_Preasiento)
            {
                GestorDeErrores.Emitir($"Ha de indicar en el parámetro '{parametro}' si se generan preasientos para la sociedad '{sociedad.Id}'");
            }
            var relaciones = ParsearUsaPreasiento(json);

            var relacion = relaciones.FirstOrDefault(x => x.IdSociedad == sociedad.Id);

            if (relacion == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{parametro}' para el id de sociedad '{sociedad.Id}'");

            if (relacion.Valor != "S" && relacion.Valor != "N")
                GestorDeErrores.Emitir($"El valor asignado en el parámetro '{parametro}' para el id de sociedad '{sociedad.Id}' no es válido, ha de ser o 'S' o 'N'");

            return relacion.Valor.EsTrue();
        }

        private static List<UsaPreasiento> ParsearUsaPreasiento(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new UsaPreasiento
                {
                    IdSociedad = item["IdSociedad"].Value<int>(),
                    Valor = item["Valor"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeSPR_Genera_Preasiento}'", ex);
            }
            return null;
        }

        public static string CuentaContableDelCobro(this CobroDeFaeDtm cobro, ContextoSe contexto)
        {
            if (cobro.Clase == enumClaseDeCobro.Contado)
            {
                return enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Cuenta_De_Caja, crearParametro: true, valorPorDefecto: ltrCuenta.Caja).Valor;
            }

            if (cobro.Clase == enumClaseDeCobro.Transferencia)
            {
                if (cobro.IdCuentaDeIngreso is null)
                    GestorDeErrores.Emitir($"El cobro '{cobro.Referencia}' es por transferencia, y necesita tener informado la cuenta de ingreso");

                var json = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_CtaBancaria_CtaContable, crearParametro: true, valorPorDefecto: _jsonDeSPR_CtaBancaria_CtaContable).Valor;
                var relaciones = ParsearCtaBancariaParaCtaContable(json);
                var relacion = relaciones.FirstOrDefault(x => x.IdCuentaBancaria == (int)cobro.IdCuentaDeIngreso);
                if (relacion == null)
                {
                    var mensaje = $"Ha de configurar el parámetro '{enumParametrosDePreasiento.SPR_CtaBancaria_CtaContable.ToString()}' para el id de la cta bancaria '{(int)cobro.IdCuentaDeIngreso}'";
                    GestorDeErrores.Emitir(mensaje);
                }
                if (relacion.CodigoDeCuenta == ltrCuenta.Banco)
                    return (relacion.CodigoDeCuenta.Entero() + (int)cobro.IdCuentaDeIngreso).ToString();

                return relacion.CodigoDeCuenta;
            }

            throw new Exception($"Pendiente de implementar la modalidad de cobro: {cobro.Clase.Descripcion()}");
        }

        public static string CuentaContableDelPago(this PagoDtm pago, ContextoSe contexto)
        {
            if (pago.IdTarjetaDePago is not null)
            {
                var json = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Tarjeta_CtaContable, crearParametro: true, valorPorDefecto: _jsonDeSPR_Tarjeta_CtaContable).Valor;
                if (_jsonDeSPR_Tarjeta_CtaContable != json)
                {

                    var relaciones = ParsearTarjetaParaCtaContable(json);
                    var relacion = relaciones.FirstOrDefault(x => x.IdTarjeta == (int)pago.IdTarjetaDePago);
                    if (relacion != null)
                    {
                        if (relacion.CodigoDeCuenta == ltrCuenta.Tarjeta)
                            return (relacion.CodigoDeCuenta.Entero() + (int)pago.IdTarjetaDePago).ToString();
                    }
                }
            }

            if (pago.IdCuentaDePago is not null)
            {
                var json = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_CtaBancaria_CtaContable, crearParametro: true, valorPorDefecto: _jsonDeSPR_CtaBancaria_CtaContable).Valor;
                var relaciones = ParsearCtaBancariaParaCtaContable(json);
                var relacion = relaciones.FirstOrDefault(x => x.IdCuentaBancaria == (int)pago.IdCuentaDePago);
                if (relacion == null)
                {
                    var mensaje = pago.IdTarjetaDePago is not null
                    ? $"Ha de configurar el parámetro '{enumParametrosDePreasiento.SPR_CtaBancaria_CtaContable.ToString()}' o '{enumParametrosDePreasiento.SPR_Tarjeta_CtaContable.ToString()}'o para el id de la cta bancaria '{(int)pago.IdCuentaDePago}' o el id de la tarjeta '{(int)pago.IdTarjetaDePago}' "
                    : $"Ha de configurar el parámetro '{enumParametrosDePreasiento.SPR_CtaBancaria_CtaContable.ToString()}' para el id de la cta bancaria '{(int)pago.IdCuentaDePago}'";
                    GestorDeErrores.Emitir(mensaje);
                }
                if (relacion.CodigoDeCuenta == ltrCuenta.Banco)
                    return (relacion.CodigoDeCuenta.Entero() + (int)pago.IdCuentaDePago).ToString();

                return relacion.CodigoDeCuenta;
            }

            return enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Cuenta_De_Caja, crearParametro: true, valorPorDefecto: ltrCuenta.Caja).Valor;
        }

        private static List<ActividadesEstimacionDirecta> ParsearActividades(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new ActividadesEstimacionDirecta
                {
                    IdSociedad = item["IdSociedad"].Value<int>(),
                    IdNegocio = item["IdNegocio"].Value<int>(),
                    IdTipo = item["IdTipo"].Value<int>(),
                    Actividad = item["Actividad"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeSPR_Codigos_De_Actividad}', debe definirlo en el parámetro de negocio '{enumParametrosDePreasiento.SPR_Codigos_De_Actividad.ToString()}'", ex);
            }
            return null;
        }

        private static List<ConceptosDeGastoEnNcsDeEstimacionDirecta> ParsearConceptoDeGastoEnNcs(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new ConceptosDeGastoEnNcsDeEstimacionDirecta
                {
                    IdNaturaleza = item["IdNaturaleza"].Value<int>(),
                    Concepto = item["Concepto"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeSPR_NCS_Concepto_Del_Gasto}', debe definirlo en el parámetro de negocio '{enumParametrosDePreasiento.SPR_NCS_Conceptos_De_Gasto.ToString()}'", ex);
            }
            return null;
        }

        private static List<CodigosDeDiario> ParsearDiarios(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new CodigosDeDiario
                {
                    IdTipoPreasiento = item["IdTipoPreasiento"].Value<int>(),
                    CodigoDeDiario = item["CodigoDeDiario"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeSPR_Codigos_De_Diario}', debe definirlo en el parámetro de negocio '{enumParametrosDePreasiento.SPR_Codigos_De_Diario.ToString()}'", ex);
            }
            return null;
        }

        private static List<CtaContablePorCtaBancaria> ParsearCtaBancariaParaCtaContable(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new CtaContablePorCtaBancaria
                {
                    IdCuentaBancaria = item["IdCuentaBancaria"].Value<int>(),
                    CodigoDeCuenta = item["CodigoDeCuenta"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeSPR_CtaBancaria_CtaContable}', debe definirlo en el parámetro de negocio '{enumParametrosDePreasiento.SPR_CtaBancaria_CtaContable.ToString()}'", ex);
            }
            return null;
        }

        private static List<TarjetaPorCtaBancaria> ParsearTarjetaParaCtaContable(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new TarjetaPorCtaBancaria
                {
                    IdTarjeta = item["IdTarjeta"].Value<int>(),
                    CodigoDeCuenta = item["CodigoDeCuenta"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeSPR_Tarjeta_CtaContable}', debe definirlo en el parámetro de negocio '{enumParametrosDePreasiento.SPR_Tarjeta_CtaContable.ToString()}'", ex);
            }
            return null;
        }

        public static string IndicarMigrado(enumSistemaContable sistema, string codigoSociedad) => $"Migrado al sistema {sistema} de la sociedad {codigoSociedad}";


    }
}
