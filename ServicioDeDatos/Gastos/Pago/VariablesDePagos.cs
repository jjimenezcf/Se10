using Gestor.Errores;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utilidades;

namespace ServicioDeDatos.Gastos
{

    public enum enumEtapasDePagos
    {
        [Description("Ids de estados en los que un pago aun no es firme")]
        PAG_Etapa_Pendiente,
        [Description("Ids de estados en los que un pago ya se ha emitido")]
        PAG_Etapa_Pagado,
        [Description("Ids de estados en los que un pago están anulados")]
        PAG_Etapa_Cancelado,
        [Description("Ids de estados en los que un pago está remesado")]
        PAG_Etapa_Remesado,
        [Description("Ids de estados en los que un pago no se han realizado una vez presentado")]
        PAG_Etapa_Anulacion
    }

    public enum enumParametrosDePagos
    {
        [Description("Indica para un motivo y un estado del pago que transición se le aplica")]
        PAG_Aplicar_Transicion,
        [Description("Indica el tipo de pago por defecto al crear una factura e indicar que está pagada")]
        PAG_Tipo_Pago_Contado,
        [Description("Indica si el pago contado y de transferencia ha de tener un justificante de pago")]
        PAG_Validar_Justificante_De_Pago,
        [Description("Indica para un id de tipo pago que id de tipo de preasiento se ha de usar")]
        PAG_Relacion_Con_Tipo_De_Preasiento
    }

    public class RelacionDeTipoPagSpr
    {
        public int IdTipoPago { get; set; }
        public int IdTipoPreasiento { get; set; }
    }

    public static class VariableDePagos
    {
        private static string etapaPendiente => enumNegocio.Pago.Parametro(enumEtapasDePagos.PAG_Etapa_Pendiente)?.Valor ?? null;
        private static string etapaDePagado => enumNegocio.Pago.Parametro(enumEtapasDePagos.PAG_Etapa_Pagado)?.Valor ?? null;
        private static string etapaRemesada => enumNegocio.Pago.Parametro(enumEtapasDePagos.PAG_Etapa_Remesado)?.Valor ?? null;
        private static string etapaDevuelta => enumNegocio.Pago.Parametro(enumEtapasDePagos.PAG_Etapa_Anulacion)?.Valor ?? null;
        private static string etapaAnulada => enumNegocio.Pago.Parametro(enumEtapasDePagos.PAG_Etapa_Cancelado)?.Valor ?? null;

        public enum enumMotivoTransicion { PagarRemesa, RetrocederRemesa, CerrarRemesa, AnularPago, AnularAnulacion };


        public enum Parametro
        {
            Pag_DiasParaEjecutarTransferencia
        }

        public static string TransicionesPorMotivo => 
        enumParametrosDePagos.PAG_Aplicar_Transicion.LeerVariable(JsonConvert.SerializeObject(new List<object> {
            new TransicionPorMotivo
                {
                    Motivo = "",
                    IdEstado = 0,
                    IdTransicion = 0
                }
}
        ));

        //JsonConvert.SerializeObject(enumNegocio.Pago.Parametro(enumVariablesDePagos.PAG_Aplicar_Transicion).Valor);
        public static string Estados(this enumEtapasDePagos etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDePagos.PAG_Etapa_Pendiente: estados =etapaPendiente; break;
                case enumEtapasDePagos.PAG_Etapa_Pagado: estados =etapaDePagado; break;
                case enumEtapasDePagos.PAG_Etapa_Remesado: estados =etapaRemesada; break;
                case enumEtapasDePagos.PAG_Etapa_Cancelado: estados =etapaAnulada; break;
                case enumEtapasDePagos.PAG_Etapa_Anulacion: estados =etapaDevuelta; break;

            }
            return estados.IsNullOrEmpty() ? enumNegocio.Pago.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnLaEtapa(this PagoDtm pago, enumEtapasDePagos etapa) => etapa.Lista().Contains(pago.IdEstado);

        public static bool ContieneLaEtapa(this List<enumEtapasDePagos> etapas, enumEtapasDePagos etapa) => etapas.Contains(etapa);

        public static bool EstaEnAlgunaDeLasEtapa(this PagoDtm pago, List<enumEtapasDePagos> etapas)
        {
            var etapasDePago = pago.Etapas();
            foreach (var etapa in etapas)
                if (etapasDePago.Contains(etapa)) return true;
            return false;
        }

        public static bool EstaEnEtapaNoContabilizada(this PagoDtm pago)
        {
            return !pago.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePagos> {enumEtapasDePagos.PAG_Etapa_Pagado});
        }

        public static (List<int> estados, enumEtapasDePagos etapa) EstadosDeLaEtapa(this enumEtapasDePagos etapa) => (etapa.Lista(), etapa);

        public static List<int> Lista(this enumEtapasDePagos etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static List<enumEtapasDePagos> Etapas(this PagoDtm pago)
        {
            var etapas = new List<enumEtapasDePagos>();
            if (pago.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Pendiente))
                etapas.Add(enumEtapasDePagos.PAG_Etapa_Pendiente);
            if (pago.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Pagado))
                etapas.Add(enumEtapasDePagos.PAG_Etapa_Pagado);
            if (pago.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Remesado))
                etapas.Add(enumEtapasDePagos.PAG_Etapa_Remesado);
            if (pago.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Anulacion))
                etapas.Add(enumEtapasDePagos.PAG_Etapa_Anulacion);
            if (pago.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Cancelado))
                etapas.Add(enumEtapasDePagos.PAG_Etapa_Cancelado);
            return etapas;
        }

        public static string CadenaDeEtapas(this PagoDtm pago) => string.Join(Simbolos.separadorDeEtapas, pago.Etapas());

        public static enumEtapasDePagos Etapa(this PagoDtm pago)
        {
            var etapas = pago.Etapas();
            if (etapas.Count == 0)
                throw new Exception($"No se ha definido la etapa de la {enumNegocio.Pago.Singular(true)}, " +
                    $"cuando éste está en el estado {pago.Propiedad<EstadoDtm>(typeof(EstadoDeUnPagoDtm)).Nombre}");
            if (etapas.Count > 1)
                throw new Exception($"El estado del pago {enumNegocio.Pago.Singular(true)} '{pago.Referencia}' " +
                    $"se encuentra en las etapas {string.Join(',', etapas)} y sólo ha de estar en una");
            return etapas[0];
        }

        public static string Nombre(this enumEtapasDePagos etapa, bool minusculas)
        {
            switch (etapa)
            {
                case enumEtapasDePagos.PAG_Etapa_Pendiente: return minusculas ? "pendiente" : "Pendiente";
                case enumEtapasDePagos.PAG_Etapa_Pagado: return minusculas ? "pagado" : "Pagado";
                case enumEtapasDePagos.PAG_Etapa_Remesado: return minusculas ? "remesado" : "Remesado";
                case enumEtapasDePagos.PAG_Etapa_Anulacion: return minusculas ? "devuelto" : "Devuelto";
                case enumEtapasDePagos.PAG_Etapa_Cancelado: return minusculas ? "cancelado" : "Cancelado";
            }
            return etapa.ToString();
        }


        private const string _jsonDeTipoDePreasiento = "[{\"IdTipoPago\": 0,\"IdTipoPreasiento\": 0}]";


        public static int IdTipoPreasiento(this PagoDtm pago, ContextoSe contexto)
        {
            var json = enumNegocio.Pago.Parametro(enumParametrosDePagos.PAG_Relacion_Con_Tipo_De_Preasiento, crearParametro: true, valorPorDefecto: _jsonDeTipoDePreasiento).Valor;

            var relaciones = ParsearRealacion(json);
            var relacion = relaciones.FirstOrDefault(x => x.IdTipoPago == pago.IdTipo);
            if (relacion == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDePagos.PAG_Relacion_Con_Tipo_De_Preasiento}' para el id de tipo de pago '{pago.IdTipo}'");

            return relacion.IdTipoPreasiento;
        }

        private static List<RelacionDeTipoPagSpr> ParsearRealacion(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new RelacionDeTipoPagSpr
                {
                    IdTipoPago = item["IdTipoPago"].Value<int>(),
                    IdTipoPreasiento = item["IdTipoPreasiento"].Value<int>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al un objeto del tipo '{_jsonDeTipoDePreasiento}', debe definirlo en el parámetro de negocio '{enumParametrosDePagos.PAG_Relacion_Con_Tipo_De_Preasiento}'", ex);
            }
            return null;
        }

    }

}
