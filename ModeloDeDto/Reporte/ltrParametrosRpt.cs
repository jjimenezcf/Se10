using Newtonsoft.Json;
using System.Collections.Generic;

namespace ModeloDeDto.Reporte
{
    public static class ltrParametrosRpt
    {
        public static string TamanoEncabezado = nameof(TamanoEncabezado);
        public static string IndicarFila = nameof(IndicarFila);
        public static string MostrarCalificadorDireccion = nameof(MostrarCalificadorDireccion);
        public static string Marco = nameof(Marco);
        public static string MostrarImpresoEl = nameof(MostrarImpresoEl);
        public static string ImprimirFechaCreacion = nameof(ImprimirFechaCreacion);
        public static string ColorTitulo = nameof(ColorTitulo);
        public static string TamanoTitulo = nameof(TamanoTitulo);
        public static string AnchoLogo = nameof(AnchoLogo);
        public static string AltoLogo = nameof(AltoLogo);
        public static string PaddingMargenIzquierdo = nameof(PaddingMargenIzquierdo);
        public static string PaddingTopPie = nameof(PaddingTopPie);
        public static string TamanoPieDePagina = nameof(TamanoPieDePagina);
        
        public static Dictionary<string,object> Parametros => new Dictionary<string, object> {
                { IndicarFila, false },
                { MostrarCalificadorDireccion, true},
                { MostrarImpresoEl, false },
                { ImprimirFechaCreacion, false },
                { ColorTitulo, "#2196f3" },
                { TamanoEncabezado, 8F },
                { Marco, 50F },
                { TamanoTitulo, 16F },
                { AnchoLogo, 100F },
                { AltoLogo, 50F },
                { PaddingMargenIzquierdo, 20F },
                { PaddingTopPie, 25F },
                { TamanoPieDePagina, 5F }
            };

        public static string ParametrosPorDefecto()
        {            
            string json = JsonConvert.SerializeObject(Parametros, Formatting.Indented);
            return json;
        }
    }
}
