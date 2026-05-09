using ServicioDeDatos.Gastos;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Ventas;
using System.ComponentModel;
using Utilidades;

namespace ServicioDeDatos.Negocio
{
    public enum enumParametrosDeNegocio
    {
        [Description("Indica si se puede subir documentos desde el móvil")]
        CFG_Permite_Subir_Archivos_Desde_El_Movil,
        [Description("Indica el prompt que se le envía a la Ia para resumir el documento")]
        IA_Prompt_Resumen,
        [Description("Indica información adicional al prompt que se le envía a la Ia para obtener el filtro")]
        IA_Prompt_Filtro,
    }


    public static class VariablesDeNegocio
    {
        public static string FiltoPorDefecto(this enumNegocio negocio)
        {
            switch (negocio)
            {
                case enumNegocio.Tarea: return VariablesDeTareas.IA_Reglas_de_filtrado;
                case enumNegocio.FacturaEmitida: return VariableDeFacturasEmt.IA_Reglas_de_filtrado;
                case enumNegocio.FacturaRecibida: return VariableDeFacturasRec.IA_Reglas_de_filtrado;
            }
            return IIaPromptFiltrar.SinReglas;
        }

    }
}

