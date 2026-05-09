using Gestor.Errores;
using Newtonsoft.Json.Linq;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utilidades;

namespace ServicioDeDatos.SistemaDocumental
{
    public enum enumEtapasDeCircuitosDoc
    {
        [Description("Ids de estados en los un circuito está abierto")]
        CAD_Etapa_Abierto,
        [Description("Ids de estados en los un circuito está cerrado")]
        CAD_Etapa_Cerrado,
        [Description("Ids de estados en los que un circuito está cancelado")]
        CAD_Etapa_Cancelado
    }

    /* Json que define fichadas de entrada y salidas
     [
        {
          "idcg": 1,
          "idtipo": 2,
          "nombre": "Ejemplo 1",
          "horaCreacion": "09:00",
          "horaCierre": "18:00",
          "idtransicion": 3,
          "excluirperiodos": [
            "01:01, 01:31",
            "07:15, 08:15"
          ],
          "incluirperiodos": [
            "01:01, 01:31",
            "07:15, 08:15"
          ]
        },
        {
          "idcg": 2,
          "idtipo": 1,
          "nombre": "Ejemplo 2",
          "horaCreacion": "08:30",
          "horaCierre": "17:30",
          "idtransicion": 4,
          "excluirperiodos": [
            "12:01, 12:31",
            "06:01, 06:30"
          ]
        }
      ]
    
        [Description("Lista de ids de tipos de circuitos que representan lotes contables de preasientos")]
        CAD_IdsDeTiposDeLotesContables,
        [Description("Id del tipo de estimación directa")]
        CAD_IdDeTipoDeEstimacionDirecta,

     */

    public enum enumParametrosDeCircuitosDoc
    {
        [Description("Json que define que fichadas hay que automatizar")]
        CAD_ParametrosParaFichadaAutomatica,
        [Description("Indica que id del tipo de cad que representan actividades formativas")]
        CAD_Tipos_Para_Actividades_Formativas,
        [Description("Indica que id del tipo de cad que representan fichadas del personal")]
        CAD_IdsDeTiposDeFichadas,
        [Description("Indica que id del tipo de cad que usar para almacenar el fichero de los preasientos generados")]
        CAD_Tipo_Para_Lote_de_Preasientos,
        [Description("Indica que id del tipo de cad que usar para almacenar el fichero de estimación directa")]
        CAD_Tipo_Para_Estimacion_Directa,
        [Description("Hora máxima de cierre")]
        CAD_HoraDeCierre
    }

    public class PeriodosTemporales
    {
        public int MesDesde { get; set; }
        public int DiaDesde { get; set; }
        public int MesHasta { get; set; }
        public int DiaHasta { get; set; }
    }

    public class MetadatosDeFichada
    {
        public int IdCg { get; set; }
        public int IdTipo { get; set; }
        public int IdTrabajador { get; set; }
        public string Entrada { get; set; }
        public string Salida { get; set; }
        public int IdTransicion { get; set; }
        public List<PeriodosTemporales> PeriodosExcluidos { get; set; }
        public List<PeriodosTemporales> PeriodosIncluidos { get; set; }

        private TrabajadorDtm _trabajador;
        public TrabajadorDtm Trabajador(ContextoSe contexto)
        {
            if (_trabajador is not null)
                return _trabajador;

            _trabajador = contexto.Set<TrabajadorDtm>().Where(t => t.Id == IdTrabajador && !t.Baja && t.IdCg == IdCg).FirstOrDefault();
            if (_trabajador == null)
            {
                GestorDeErrores.Emitir($"El id de trabajador configurado '{IdTrabajador}' no está en la BD, o está de baja o no es de este centro gestor");
            }
            return _trabajador;
        }
    }

    public static class VariablesDeCircuitosDoc
    {
        private static string etapaDeAbierto => enumNegocio.CircuitoDoc.Parametro(enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto)?.Valor ?? null;
        private static string etapaDeCerrado => enumNegocio.CircuitoDoc.Parametro(enumEtapasDeCircuitosDoc.CAD_Etapa_Cerrado)?.Valor ?? null;
        private static string etapaDeCancelado => enumNegocio.CircuitoDoc.Parametro(enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado)?.Valor ?? null;


        public static string Estados(this enumEtapasDeCircuitosDoc etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDeCircuitosDoc.CAD_Etapa_Cerrado: estados = etapaDeCerrado; break;
                case enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto: estados = etapaDeAbierto; break;
                case enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado: estados = etapaDeCancelado; break;

            }

            return estados.IsNullOrEmpty() ? enumNegocio.CircuitoDoc.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnLaEtapa(this CircuitoDocDtm circuito, enumEtapasDeCircuitosDoc etapa) => etapa.Lista().Contains(circuito.IdEstado);

        public static List<int> Lista(this enumEtapasDeCircuitosDoc etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static bool ContieneLaEtapa(this List<enumEtapasDeCircuitosDoc> etapas, enumEtapasDeCircuitosDoc etapa) => etapas.Contains(etapa);

        public static bool EstaEnAlgunaDeLasEtapa(this CircuitoDocDtm circuito, List<enumEtapasDeCircuitosDoc> etapas)
        {
            var etapasDeLacircuito = circuito.Etapas();
            foreach (var etapa in etapas)
                if (etapasDeLacircuito.Contains(etapa)) return true;
            return false;
        }

        public static (List<int> estados, enumEtapasDeCircuitosDoc etapa) EstadosDeLaEtapa(this enumEtapasDeCircuitosDoc etapa) => (etapa.Lista(), etapa);


        public static List<enumEtapasDeCircuitosDoc> Etapas(this CircuitoDocDtm circuito)
        {
            var etapas = new List<enumEtapasDeCircuitosDoc>();
            if (circuito.EstaEnLaEtapa(enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto))
                etapas.Add(enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto);
            if (circuito.EstaEnLaEtapa(enumEtapasDeCircuitosDoc.CAD_Etapa_Cerrado))
                etapas.Add(enumEtapasDeCircuitosDoc.CAD_Etapa_Cerrado);
            if (circuito.EstaEnLaEtapa(enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado))
                etapas.Add(enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado);
            return etapas;
        }

        public static string CadenaDeEtapas(this CircuitoDocDtm circuito) => string.Join(Simbolos.separadorDeEtapas, circuito.Etapas());

        public static enumEtapasDeCircuitosDoc Etapa(this CircuitoDocDtm circuito)
        {
            var etapas = circuito.Etapas();
            if (etapas.Count == 0)
                throw new Exception($"No se ha definido la etapa del {enumNegocio.CircuitoDoc.Singular(true)}, " +
                    $"cuando éste está en el estado {circuito.Propiedad<EstadoDtm>(typeof(EstadoDeUnCircuitoDocDtm)).Nombre}");
            if (etapas.Count > 1)
                throw new Exception($"El estado del {enumNegocio.CircuitoDoc.Singular(true)} '{circuito.Referencia}' " +
                    $"se encuentra en las etapas {string.Join(',', etapas)} y sólo ha de estar en una");
            return etapas[0];
        }

        public static string Nombre(this enumEtapasDeCircuitosDoc etapa, bool minusculas = true)
        {
            switch (etapa)
            {
                case enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto: return minusculas ? "abierto" : "Abierto";
                case enumEtapasDeCircuitosDoc.CAD_Etapa_Cerrado: return minusculas ? "cerrado" : "Cerrado";
                case enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado: return minusculas ? "cancelado" : "Cancelado";
            }
            return etapa.ToString();
        }

        private const string valorPorDefecto =
                @"[
                            {
                              ""idcg"": 0,
                              ""idtipo"": 0,
                              ""idtrabajador"": 0,
                              ""horaCreacion"": ""09:00"",
                              ""horaCierre"": ""18:00"",
                              ""idtransicion"": 0,
                              ""excluidos"": [
                                ""01:01, 01:31"",
                                ""07:15, 08:15""
                              ],
                              ""incluidos"": [
                                ""01:01, 01:31"",
                                ""07:15, 08:15""
                              ]
                            }
                     ]
                    ";
        public static List<MetadatosDeFichada> ObtenerMetadatosDeFichadas(ContextoSe contexto, bool errorSiNoDefinido = true)
        {
            var json = enumNegocio.CircuitoDoc.Parametro(enumParametrosDeCircuitosDoc.CAD_ParametrosParaFichadaAutomatica, crearParametro: true, valorPorDefecto: valorPorDefecto).Valor;

            var metadatos = ParsearMetadatosDeFichadas(json);

            if (metadatos.Count == 0 || metadatos.Count == 1 && metadatos[0].IdCg == 0)
            {
                if (errorSiNoDefinido) return null;
                GestorDeErrores.Emitir(string.Format(ltrDeUnaEstimacion.Mensaje_FaltaConfigurarParametro, enumParametrosDeCircuitosDoc.CAD_ParametrosParaFichadaAutomatica.ToString()));
            }

            return metadatos;
        }

        private static List<MetadatosDeFichada> ParsearMetadatosDeFichadas(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new MetadatosDeFichada
                {
                    IdCg = item["idcg"].Value<int>(),
                    IdTipo = item["idtipo"].Value<int>(),
                    IdTrabajador = item["idtrabajador"].Value<int>(),
                    Entrada = item["horaCreacion"].Value<string>(),
                    Salida = item["horaCierre"].Value<string>(),
                    IdTransicion = item["idtransicion"].Value<int>(),
                    PeriodosExcluidos = ParsearPeriodos(item["excluidos"]),
                    PeriodosIncluidos = ParsearPeriodos(item["incluidos"])
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{valorPorDefecto}', debe definirlo en el parámetro de negocio '{enumParametrosDeCircuitosDoc.CAD_ParametrosParaFichadaAutomatica}'", ex);
            }
            return null;
        }

        private static List<PeriodosTemporales> ParsearPeriodos(JToken periodosToken)
        {
            if (periodosToken == null)
                return new List<PeriodosTemporales>();

            return periodosToken.Select(periodo =>
            {
                var partes = periodo.Value<string>().Split(',');
                if (partes.Length != 2)
                    GestorDeErrores.Emitir($"El periodo '{periodosToken}' está mal definido, debe ser con el formato mm:dd,mm:dd");

                var desde = partes[0].Trim().Split(':');
                var hasta = partes[1].Trim().Split(':');

                if (desde.Length != 2 || hasta.Length != 2)
                    GestorDeErrores.Emitir($"El periodo '{periodosToken}' está mal definido, debe ser con el formato mm:dd,mm:dd");

                var anio = DateTime.Now.Year;
                int mesDesde = extFechas.ValidarMes(desde[0], "desde");
                int diaDesde = extFechas.ValidarDia(desde[1], mesDesde, anio, "desde");
                int mesHasta = extFechas.ValidarMes(hasta[0], "hasta");
                int diaHasta = extFechas.ValidarDia(hasta[1], mesHasta, anio, "hasta");

                return new PeriodosTemporales
                {
                    MesDesde = mesDesde,
                    DiaDesde = diaDesde,
                    MesHasta = mesHasta,
                    DiaHasta = diaHasta
                };
            }).ToList();
        }

        public static int IdDelTipoCircuitoDocParaFichada(bool errorSiNoEstaDefinido = true)
        {
            var tipoCad = enumNegocio.CircuitoDoc.Parametro(enumParametrosDeCircuitosDoc.CAD_IdsDeTiposDeFichadas, valorPorDefecto: Literal.Cero).Valor.Entero();

            if (errorSiNoEstaDefinido && tipoCad == 0)
            {
                enumNegocio.CircuitoDoc.IndicarQueFaltaDefinirElParámetro(enumParametrosDeCircuitosDoc.CAD_IdsDeTiposDeFichadas);
            }
            return tipoCad;
        }



        public static int IdDelTipoCircuitoDocParaLoteDePreasientos(bool errorSiNoEstaDefinido = true)
        {
            var tipoCad = enumNegocio.CircuitoDoc.Parametro(enumParametrosDeCircuitosDoc.CAD_Tipo_Para_Lote_de_Preasientos, valorPorDefecto: Literal.Cero).Valor.Entero();

            if (errorSiNoEstaDefinido && tipoCad == 0)
            {
                enumNegocio.CircuitoDoc.IndicarQueFaltaDefinirElParámetro(enumParametrosDeCircuitosDoc.CAD_Tipo_Para_Lote_de_Preasientos);
            }

            return tipoCad;
        }

        public static int IdDelTipoCircuitoDocParaEstimacionDirecta(bool errorSiNoEstaDefinido = true)
        {
            var tipoCad = enumNegocio.CircuitoDoc.Parametro(enumParametrosDeCircuitosDoc.CAD_Tipo_Para_Estimacion_Directa, valorPorDefecto: Literal.Cero).Valor.Entero();

            if (errorSiNoEstaDefinido && tipoCad == 0)
            {
                enumNegocio.CircuitoDoc.IndicarQueFaltaDefinirElParámetro(enumParametrosDeCircuitosDoc.CAD_Tipo_Para_Estimacion_Directa);
            }

            return tipoCad;
        }


        public static bool EsUnaActividadFormativa(this CircuitoDocDtm circuito)
        {
            var idTipo = IdDelTipoCircuitoDocParaActividadesFormativas(errorSiNoEstaDefinido: false);
            return idTipo == circuito.IdTipo;
        }

        public static bool EsUnaActividadFormativa(int idTipo)
        {
            return idTipo == IdDelTipoCircuitoDocParaActividadesFormativas(errorSiNoEstaDefinido: true);
        }

        public static int IdDelTipoCircuitoDocParaActividadesFormativas(bool errorSiNoEstaDefinido = true)
        {
            var idtipoCad = enumNegocio.CircuitoDoc.Parametro(enumParametrosDeCircuitosDoc.CAD_Tipos_Para_Actividades_Formativas, valorPorDefecto: Literal.Cero).Valor.Entero();

            if (errorSiNoEstaDefinido && idtipoCad == 0)
            {
                enumNegocio.CircuitoDoc.IndicarQueFaltaDefinirElParámetro(enumParametrosDeCircuitosDoc.CAD_Tipos_Para_Actividades_Formativas);
            }

            return idtipoCad;
        }
    }
}
