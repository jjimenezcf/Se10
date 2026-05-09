using ServicioDeDatos;

using pltMapeosDeTabla = System.Collections.Generic.Dictionary<string, string>;
using pltFilasDeTabla = System.Collections.Generic.Dictionary<string, string>;
using pltDatosDePlantilla = System.Collections.Generic.Dictionary<string, string>;
using pltFormulasDePlantilla = System.Collections.Generic.Dictionary<string, string>;
using System.Collections.Generic;
using System.Data;
using System;
using Utilidades;
using System.Linq;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos.Elemento;
using Gestor.Errores;
using System.Reflection;
using ModeloDeDto;
using ServicioDeDatos.Negocio;
using ModeloDeDto.Negocio;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDePlantillas
    {
        static pltFormulasDePlantilla _formulasDePlantilla = new()
        {
            ["impresaEl"] = "DateTime.Now"
        };


        public static enumNegocio NegocioDeUnaPltPorTipo(Type tipo)
        {
            if (!ApiDeEnsamblados.HeredaDe(tipo, typeof(PlantillaPorTipoDtm)))
                GestorDeErrores.Emitir($"El clase {tipo.Name} no hereda de {nameof(PlantillaPorTipoDtm)}");

            var negocio = (enumNegocio)tipo.GetProperty(nameof(IPlantillaPorTipo.Negocio), BindingFlags.Public).GetValue(null, null);
            if (negocio != enumNegocio.No_Definido) return negocio;

            var tablaDePlantillaPorTipo = ApiDeRegistroDtm.NombreDeTabla(tipo);
            var tablaDeNegocio = tablaDePlantillaPorTipo.Replace("_" + nameof(Sufijo.PLANTILLA) + "_" + nameof(Sufijo.TIPO), "");

            return ApiDeEnsamblados.ToEnumerado<enumNegocio>(tablaDeNegocio);
        }

        public static List<IPlantillaPlt> LeerPlantillasPlt(this enumNegocio negocio, ContextoSe contexto, int? idTipo)
        {
            var plantillas = new List<IPlantillaPlt>();

            var plantillasDeNegocio = negocio.LeerPlantillasDeNegocio(contexto);
            if (plantillasDeNegocio is not null && plantillasDeNegocio.Count > 0)
                plantillas.AddRange(plantillasDeNegocio.Select(p => new PlantillaPlt { IdPlantilla = p.Id, Plantilla = p.Nombre, Clase = enumClaseDePlantilla.deNegocio } as IPlantillaPlt).ToList());

            var plantillasPorTipo = idTipo is null ? null: negocio.LeerPlantillasPorTipo(contexto, (int)idTipo);                       
            if (plantillasPorTipo is not null && plantillasPorTipo.Count() > 0)
                plantillas.AddRange(plantillasPorTipo.Select(p => new PlantillaPlt { IdPlantilla = p.Id, Plantilla = p.Nombre, Clase = enumClaseDePlantilla.porTipo } as IPlantillaPlt).ToList());

            return plantillas;
        }

        public static IPlantillaConAccion LeerPlantillaPorId(this enumNegocio negocio, ContextoSe contexto, enumClaseDePlantilla clase,  int id)
        {
            if (clase == enumClaseDePlantilla.porTipo)
            {
                var tipoDePlantillaPorTipo = negocio.ObtenerMetadatos()?.PlantillasPorTipoDtm;
                if (tipoDePlantillaPorTipo is null)
                    return null;

                var gestor = NegociosDeSe.CrearGestor(contexto, tipoDePlantillaPorTipo, typeof(PlantillaPorTipoDto));
                return (IPlantillaConAccion)gestor.LeerRegistroPorId(id, aplicarJoin: false, parametros: new Dictionary<string, object> { { nameof(ltrParametrosNeg.ErrorSiNoLoHay), false } });
            }
            if (clase == enumClaseDePlantilla.deNegocio)
            {
                return contexto.SeleccionarPorId<PlantillaDeNegocioDtm>(id);
            }
            throw new Exception($"No se ha definido como leer una plantilla de la clase {clase}");
        }

        public static List<PlantillaPorTipoDto> LeerPlantillasPorTipoDto(this enumNegocio negocio, ContextoSe contexto, int idTipo)
        {
            var tipoDePlantillaPorTipo = negocio.ObtenerMetadatos()?.PlantillasPorTipoDtm;
            if (tipoDePlantillaPorTipo is null)
               return null;

            var filtro = new ClausulaDeFiltrado(nameof(PlantillaPorTipoDtm.IdTipo), enumCriteriosDeFiltrado.igual, idTipo);
            var gestor = NegociosDeSe.CrearGestor(contexto, tipoDePlantillaPorTipo, typeof(PlantillaPorTipoDto));
            return (List<PlantillaPorTipoDto>) gestor.LeerElementos(0, -1, filtros: new List<ClausulaDeFiltrado> { filtro}, orden: null, opcionesDeMapeo: new Dictionary<string, object> { { ltrParametrosNeg.AplicarJoin, true} });
        }

        private static List<PlantillaDeNegocioDtm> LeerPlantillasDeNegocio(this enumNegocio negocio, ContextoSe contexto)
        {
            var plantillas = (List<PlantillaDeNegocioDtm>) NegociosDeSe.CrearGestor(contexto, typeof(PlantillaDeNegocioDtm), typeof(PlantillaDeNegocioDto)).LeerRegistros(0, -1, filtros: new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado(nameof(PlantillaDeNegocioDtm.IdNegocio), enumCriteriosDeFiltrado.igual, negocio.IdNegocio())
            },
            aplicarJoin: false);

            var resultado = new List<PlantillaDeNegocioDtm>();
            foreach (var plantilla in plantillas)
                resultado.Add(new PlantillaDeNegocioDtm
                {
                    Id = plantilla.Id,
                    IdAccion = plantilla.IdAccion,
                    IdNegocio = plantilla.IdNegocio,
                    Nombre = plantilla.Nombre,
                    IdPermiso = plantilla.IdPermiso
                });

            return resultado;
        }

        private static List<PlantillaPorTipoDtm> LeerPlantillasPorTipo(this enumNegocio negocio, ContextoSe contexto, int idTipo)
        {
            var tipoDePlantillaPorTipo = negocio.ObtenerMetadatos()?.PlantillasPorTipoDtm;
            if (tipoDePlantillaPorTipo is null)
                return null;

            dynamic plantillas = NegociosDeSe.CrearGestor(contexto, tipoDePlantillaPorTipo, typeof(PlantillaPorTipoDto)).LeerRegistros(0, -1, filtros: new List<ClausulaDeFiltrado> 
            { 
                new ClausulaDeFiltrado(nameof(PlantillaPorTipoDtm.IdTipo), enumCriteriosDeFiltrado.igual, idTipo) 
            }, 
            aplicarJoin: false);

            var resultado = new List<PlantillaPorTipoDtm>();
            foreach (var plantilla in plantillas)
                resultado.Add(new PlantillaPorTipoDtm
                {
                    Negocio = negocio,
                    Id = plantilla.Id,
                    IdAccion = plantilla.IdAccion,
                    IdTipo = plantilla.IdTipo,
                    Nombre = plantilla.Nombre,
                    IdPermiso = plantilla.IdPermiso
                });

            return resultado;
        }

        public static (pltFormulasDePlantilla formulas, Dictionary<string, pltDatosDePlantilla> datos, Dictionary<string, List<pltFilasDeTabla>> filas, Dictionary<string, pltMapeosDeTabla> mapeos)
        LeerDatos(this ContextoSe contexto, string esquema, string nombrePa, int idNegocio, int idFactura)
        {
            var informacion = contexto.EjecutarPa(esquema, nombrePa, idNegocio, idFactura);
            (
             pltFormulasDePlantilla formulas,
             Dictionary<string, pltDatosDePlantilla> datos,
             Dictionary<string, List<pltFilasDeTabla>> filas,
             Dictionary<string, pltMapeosDeTabla> mapeos
            )
            diccionarios = (new pltFormulasDePlantilla(), new Dictionary<string, pltDatosDePlantilla>(), new Dictionary<string, List<pltFilasDeTabla>>(), new Dictionary<string, pltMapeosDeTabla>());

            var descriptores = informacion.Tables[0];
            ValidarDescriptorDePlantilla(descriptores);

            var registroDeFormulas = descriptores.Rows[0];
            var registroDeDatos = descriptores.Rows[1];
            var registroDeLineas = descriptores.Rows[2];
            var registroDeMapeos = descriptores.Rows[3];


            if (registroDeFormulas["dato"].ToString() == "-1")
                diccionarios.formulas = _formulasDePlantilla;
            else
                diccionarios.formulas = MapearCampoFormula(registroDeFormulas["dato"].ToString());

            diccionarios.datos = MapearCampoDatos(informacion, registroDeDatos["dato"].ToString());
            diccionarios.filas = MapearCampoFilas(informacion, registroDeLineas["dato"].ToString());
            diccionarios.mapeos = MapearMapeosDeLasTablas(informacion, registroDeMapeos["dato"].ToString());


            return diccionarios;
        }

        private static void ValidarDescriptorDePlantilla(DataTable descriptores)
        {
            if (descriptores.Rows.Count != 4)
                throw new Exception($"Una plantilla que usa PAs debe definir un los descriptores de mapeo: {nameof(pltFormulasDePlantilla)}, {nameof(pltDatosDePlantilla)}, {nameof(pltFilasDeTabla)}, {nameof(pltMapeosDeTabla)}");

            if (!descriptores.Columns.Contains("tipo"))
                throw new Exception($"El descriptor de plantillas debe tener dos columnas, la primera tipo, la segunda datos, con 4 reristros, donde tipo será alguno de estos: {nameof(pltFormulasDePlantilla)}, {nameof(pltDatosDePlantilla)}, {nameof(pltFilasDeTabla)}, {nameof(pltMapeosDeTabla)}");

            if (descriptores.Rows[0]["tipo"].ToString() != nameof(pltFormulasDePlantilla))
                throw new Exception($"El primer registro del descriptor debe indicar la posición de la consulta de las fórmulas, si no hay consulta, entonces -1");

            if (descriptores.Rows[1]["tipo"].ToString() != nameof(pltDatosDePlantilla))
                throw new Exception($"El segundo registro del descriptor debe indicar la posición de la consulta de datos de la plantilla, si no hay consulta, entonces -1");

            if (descriptores.Rows[2]["tipo"].ToString() != nameof(pltFilasDeTabla))
                throw new Exception($"El tercer registro del descriptor debe indicar la posición de la consulta de filas de la plantilla, si no hay consulta, entonces -1");

            if (descriptores.Rows[3]["tipo"].ToString() != nameof(pltMapeosDeTabla))
                throw new Exception($"El primer registro del descriptor debe indicar la posición de la consulta de los mapeos de las filas a las tablas, si no hay consulta, entonces -1");
        }

        private static Dictionary<string, pltMapeosDeTabla> MapearMapeosDeLasTablas(DataSet datos, string descriptor)
        {
            /*

                    static Dictionary<string, pltMapeosDeTablas> _descriptorDeMapeos = new()
                    {
                        ["lineasdefactura"] = new() { ["col0"] = "Concepto", ["col1"] = "Cantidad", ["col2"] = "Unidad", ["col3"] = "Precio", ["col4"] = "Importe", ["col5"] = "Iva", ["col6"] = "Total" },
                        ["cobros"] = new() { ["col0"] = "Fecha", ["col1"] = "Pendiente", ["col2"] = "Pagado", ["col3"] = "Resto" }
                    };


             * */

            var plfMapeos = new Dictionary<string, pltMapeosDeTabla>();

            var bloques = descriptor.Split('|').Select(x => x.Trim().ToLower()).ToArray();
            foreach (var bloque in bloques)
            {
                var parte = bloque.Split(':');
                if (parte.Length != 2)
                    throw new Exception($"La definición del bloque de mapeo '{bloque}' está mal realizada, debe indicar el nombre de la tabla de la plantilla y la serie de mapéo columna=campo");
                var mapeos = parte[1].Split(",");
                var mapeoDeColumnas = new pltMapeosDeTabla();
                foreach (var mapeo in mapeos)
                {
                    var definicion = mapeo.Split('=');
                    if (definicion.Length != 2)
                        throw new Exception($"La definición del mapeo '{mapeo}' está mal realizada, debe indicar el nombre de la columna de la plantilla y el campo de la tabla, columna=campo");
                    mapeoDeColumnas[definicion[0]] = definicion[1].ToString();
                }
                plfMapeos[parte[0]] = mapeoDeColumnas;
            }

            return plfMapeos;

        }

        private static Dictionary<string, List<pltFilasDeTabla>> MapearCampoFilas(DataSet datos, string descriptor)
        {
            /*
             * descriptor sigue el formato: '4:lineasdelafactura,5:Cobros'

                    static Dictionary<string, List<pltFilasDeTablas>> _filasDeTablas = new Dictionary<string, List<pltFilasDeTablas>>
                {
                    {"lineasdefactura", new List<pltFilasDeTablas>
                       {
                         new() {["Concepto"] = "Sercicio x" , ["Cantidad"] = "1.00" ,  ["Unidad"] = "Ud",  ["Precio"] = "1000.00" ,  ["Importe"] = "1000.00", ["Iva"] = "210.00",["Total"] = "1210.00" },
                         new() {["Concepto"] = "Sercicio x" , ["Cantidad"] = "1.00" ,  ["Unidad"] = "Ud",  ["Precio"] = "1000.00" ,  ["Importe"] = "1000.00", ["Iva"] = "210.00",["Total"] = "1210.00" },
                         new() {["Concepto"] = "Sercicio x" , ["Cantidad"] = "1.00" ,  ["Unidad"] = "Ud",  ["Precio"] = "1000.00" ,  ["Importe"] = "1000.00", ["Iva"] = "210.00",["Total"] = "1210.00" },
                         new() {["Concepto"] = "Sercicio x" , ["Cantidad"] = "1.00" ,  ["Unidad"] = "Ud",  ["Precio"] = "1000.00" ,  ["Importe"] = "1000.00", ["Iva"] = "210.00",["Total"] = "1210.00" },
                         new() {["Concepto"] = "Sercicio x" , ["Cantidad"] = "1.00" ,  ["Unidad"] = "Ud",  ["Precio"] = "1000.00" ,  ["Importe"] = "1000.00", ["Iva"] = "210.00",["Total"] = "1210.00" },
                         new() {["Concepto"] = "Sercicio x" , ["Cantidad"] = "1.00" ,  ["Unidad"] = "Ud",  ["Precio"] = "1000.00" ,  ["Importe"] = "1000.00", ["Iva"] = "210.00",["Total"] = "1210.00" },

                       }
                    },
                    {"cobros", new List<pltFilasDeTablas>
                       {
                         new() {["Fecha"] = "25-03-2023" , ["Pendiente"] = "1210.00" ,  ["Pagado"] = "5", ["Resto"] = "1200.00"  },
                         new() {["Fecha"] = "25-03-2023" , ["Pendiente"] = "1210.00" ,  ["Pagado"] = "5", ["Resto"] = "1200.00"  },
                         new() {["Fecha"] = "25-03-2023" , ["Pendiente"] = "1210.00" ,  ["Pagado"] = "5", ["Resto"] = "1200.00"  },
                         new() {["Fecha"] = "25-03-2023" , ["Pendiente"] = "1210.00" ,  ["Pagado"] = "5", ["Resto"] = "1200.00"  }
                       }
                    }
                };
             */

            var pltfilas = new Dictionary<string, List<pltFilasDeTabla>>();

            var bloques = descriptor.Split(',').Select(s => s.Trim().ToLower()).ToArray();
            foreach (var bloque in bloques)
            {
                var definidor = bloque.Split(':').Select(s => s.Trim().ToLower()).ToArray();
                if (definidor.Length != 2)
                    throw new Exception($"La definición del bloque de filas de tabla '{bloque}' está má realizada, debe indicar el numero de consulta empezando por 0 y sabiendo que la consulta 0 es el descriptor, y la clave de indexación del diccionario");

                if (!definidor[0].EsNumero() || definidor[0].Entero() >= datos.Tables.Count || definidor[0].Entero() < 1)
                    throw new Exception($"La definición del bloque de mapeos '{bloque}' está má realizada, el número de consulta debe ser un valor entre 1 y {datos.Tables.Count - 1}");

                var filas = new List<pltFilasDeTabla>();
                var datatable = datos.Tables[definidor[0].ToString().Entero()];
                foreach (DataRow row in datatable.Rows)
                {
                    pltFilasDeTabla fila = row.Table.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName.ToLower(), col => row[col].ToString().Trim());
                    filas.Add(fila);
                }
                pltfilas[definidor[1].ToString()] = filas;
            }
            return pltfilas;
        }

        private static Dictionary<string, pltDatosDePlantilla> MapearCampoDatos(DataSet datos, string descriptor)
        {
            /*

                    static Dictionary<string, pltDatosDePlantilla> _datosDePlantilla = new Dictionary<string, pltDatosDePlantilla>()
                    {
                        ["documento"] = new() { ["titulo"] = "Factura de prueba", ["impresaEl"] = "DateTime.Now" },
                        ["factura"] = new() { ["numero"] = "256-96", ["emitida"] = "21-03-2023", ["vence"] = $"21-04-2023", ["clase"] = "normal", ["bi"] = "1500.00", ["iva"] = "200.00", ["total"] = "1700.00" },
                        ["cliente"] = new()  { ["nombre"] = "Raúl Miras", ["direccion"] = $"Calle obispo frutos", ["municipio"] = "Murcia 30018" }, ["provincia"] = "Murcia (España)"},
                        ["sociedad"] = new() { ["nombre"] = "Femdek SL",  ["direccion"] = $"Calle sociedad Nº23", ["municipio"] = "Murcia - 30008", ["provincia"] = "Murcia (España)"},
                        ["cobrado"] = new() { ["total"] = "1221.00" }
                    };
            */

            var pltMapeos = new Dictionary<string, pltDatosDePlantilla>();
            var bloques = descriptor.Split(',').Select(s => s.Trim().ToLower()).ToArray();
            foreach (var bloque in bloques)
            {
                var definidor = bloque.Split(':').Select(s => s.Trim().ToLower()).ToArray();
                if (definidor.Length != 2)
                    throw new Exception($"La definición del bloque de mapeos '{bloque}' está má realizada, debe indicar el numero de consulta empezando por 0 y sabiendo que la consulta 0 es el descriptor, y la clave de indexación del diccionario");

                if (!definidor[0].EsNumero() || definidor[0].Entero() >= datos.Tables.Count || definidor[0].Entero() < 1)
                    throw new Exception($"La definición del bloque de mapeos '{bloque}' está má realizada, el número de consulta debe ser un valor entre 1 y {datos.Tables.Count - 1}");


                var dicionario = new pltDatosDePlantilla();
                var tabla = definidor[1];
                if (datos.Tables[definidor[0].Entero()].Rows.Count > 0)
                    foreach (DataColumn columna in datos.Tables[definidor[0].Entero()].Columns)
                        dicionario[columna.ColumnName.ToLower()] = datos.Tables[definidor[0].Entero()].Rows[0][columna.ColumnName].ToString().Trim();
                pltMapeos[tabla] = dicionario;
            }
            return pltMapeos;
        }

        private static pltFormulasDePlantilla MapearCampoFormula(string v)
        {
            throw new NotImplementedException();
        }
    }
}
