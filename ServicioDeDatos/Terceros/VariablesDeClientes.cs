using Gestor.Errores;
using Newtonsoft.Json.Linq;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utilidades;

namespace ServicioDeDatos.Terceros
{
    public enum enumParametrosDeCliente
    {
        [Description("Indica para un motivo y un estado de la factura que transición se le aplica")]
        Cli_Como_Emitir_Factura,
        [Description("Indica la versión en Ubl a utilizar (2.1 o 2.5), si exite el id y su valor es 'true' se utilizará Peppol (2.1)")]
        Cli_Ubl_Usar_Peppol,
        [Description("Nombre del puesto de trabajo por defecto de un cliente")]
        CLI_PuestoDeTrabajo,
        [Description("Nombre del tipo de archivador a crear cuando se crea un cliente")]
        CLI_TipoArchivador,
        [Description("Código del CG donde se crea el archivador del cliente")]
        CLI_CG_De_Cliente,
    }

    public class ComoEmitir
    {
        public int IdCliente { get; set; }
        public string Valor { get; set; }
    }

    public class UsarPeppol
    {
        public int IdCliente { get; set; }
        public bool Valor { get; set; }
    }

    public static class VariablesDeClientes
    {
        private static readonly string _jsonDeCli_Como_Emitir_Factura = "[{\"IdCliente\": 0,\"Valor\": \"eFactura322\"}]";

        private static readonly string _jsonDeCli_Usar_Peppol = "[{\"IdCliente\": 0,\"Valor\": \"false\"}]";


        public static enumClaseDeEmision ClaseDeEmision(this ClienteDtm cliente)
        {
            var parametro = enumParametrosDeCliente.Cli_Como_Emitir_Factura;
            var json = enumNegocio.Cliente.Parametro(parametro, crearParametro: true, valorPorDefecto: _jsonDeCli_Como_Emitir_Factura).Valor;

            List<ComoEmitir> relaciones = ParsearCli_Como_Emitir_Factura(json);
            var relacion = relaciones.FirstOrDefault(x => x.IdCliente == cliente.Id);
            enumClaseDeEmision? clase = null;
            if (relacion != null)
            {
                clase = ApiDeEnsamblados.ToEnumerado<enumClaseDeEmision>(relacion.Valor, errorSiNoEsValido: false);
                if (clase != null) return (enumClaseDeEmision)clase;
                throw Excepciones.Emitir($"El valor configurado en el parámetro '{parametro}' para el cliente '{cliente.Id}' no es válido, actualícelo, valores válidos: '{enumClaseDeEmision.eFactura322}','{enumClaseDeEmision.eFactura32}' y '{enumClaseDeEmision.Impresa}'");
            }

            relacion = relaciones.FirstOrDefault(x => x.IdCliente == 0);
            if (relacion != null)
            {
                clase = ApiDeEnsamblados.ToEnumerado<enumClaseDeEmision>(relacion.Valor, errorSiNoEsValido: false);
                if (clase != null) return (enumClaseDeEmision)clase;
                throw Excepciones.Emitir($"El valor configurado en el parámetro '{parametro}' para cualquier cliente (id = 0) no es válido, actualícelo, valores válidos: '{enumClaseDeEmision.eFactura322}','{enumClaseDeEmision.eFactura32}' y '{enumClaseDeEmision.Impresa}'");
            }

            throw Excepciones.Emitir($"El valor configurado en el parámetro '{parametro}' debe indicar un id de cliente, o 0 para todos, y algún modo de edición, actualícelo, valores válidos: '{enumClaseDeEmision.eFactura322}','{enumClaseDeEmision.eFactura32}' y '{enumClaseDeEmision.Impresa}'");
        }

        public static bool UsarPeppol(this ClienteDtm cliente)
        {
            var parametro = enumParametrosDeCliente.Cli_Ubl_Usar_Peppol;
            var json = enumNegocio.Cliente.Parametro(parametro, crearParametro: true, valorPorDefecto: _jsonDeCli_Usar_Peppol).Valor;
            List<UsarPeppol> relaciones = ParsearCli_Ubl_Usar_Pepplo(json);
            var relacion = relaciones.FirstOrDefault(x => x.IdCliente == cliente.Id);

            if (relacion == null)
            {
                return false;
            }

            return relacion.Valor;
        }

        private static List<ComoEmitir> ParsearCli_Como_Emitir_Factura(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new ComoEmitir
                {
                    IdCliente = item["IdCliente"].Value<int>(),
                    Valor = item["Valor"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{nameof(ComoEmitir)}', actualice el parámetro '{enumParametrosDeCliente.Cli_Como_Emitir_Factura}'");
            }
        }

        private static List<UsarPeppol> ParsearCli_Ubl_Usar_Pepplo(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new UsarPeppol
                {
                    IdCliente = item["IdCliente"].Value<int>(),
                    Valor = item["Valor"].Value<bool>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{nameof(UsarPeppol)}', actualice el parámetro '{enumParametrosDeCliente.Cli_Ubl_Usar_Peppol}'");
            }

        }
    }
}
