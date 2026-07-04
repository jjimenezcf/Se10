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
        [Description("Indica la documentación del modelo de datos que se le envía a la Ia para preguntas de conteo")]
        IA_Prompt_Modelo,
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

        public static string ModeloDeDatosDefault(this enumNegocio negocio)
        {
            var especifico = negocio switch
            {
                enumNegocio.Tarea            => VariablesDeTareas.IA_Modelo_de_datos,
                enumNegocio.FacturaRecibida  => VariableDeFacturasRec.IA_Modelo_de_datos,
                enumNegocio.FacturaEmitida   => VariableDeFacturasEmt.IA_Modelo_de_datos,
                enumNegocio.Presupuesto      => VariableDePpts.IA_Modelo_de_datos,
                enumNegocio.Pago             => VariableDePagos.IA_Modelo_de_datos,
                _                            => string.Empty
            };
            return IA_Modelo_de_datos_comun + (especifico.Length > 0 ? "\n\n" + especifico : string.Empty);
        }

        internal static readonly string IA_Modelo_de_datos_comun = @"
## MODELO DE DATOS COMÚN (ElementoDeProcesoDtm)

Todos los elementos de negocio heredan de `ElementoDeProcesoDtm`, que a su vez hereda de `ElementoConCgDtm` y `ElementoDtm`.

### Propiedades base disponibles para filtros y métricas:
| Propiedad        | Tipo       | Descripción                                      |
|------------------|------------|--------------------------------------------------|
| Id               | int        | Identificador único                              |
| Nombre           | string     | Nombre o asunto del elemento                     |
| Referencia       | string     | Referencia textual del elemento                  |
| Descripcion      | string     | Descripción o detalle                            |
| IdTipo           | int        | ID del tipo de elemento (→ Tipos)                |
| IdCg             | int        | ID del centro gestor (→ Centros Gestores)        |
| IdEstado         | int        | ID del estado actual (→ Estados)                 |
| FechaCreacion    | DateTime   | Fecha y hora de creación                         |
| IdUsuaCrea       | int        | ID del usuario que creó el registro (→ Usuarios) |
| FechaModificacion| DateTime   | Fecha y hora de última modificación              |
| IdUsuaModi       | int        | ID del usuario que modificó (→ Usuarios)         |

### Objetos relacionados comunes (disponibles en todos los negocios):
- **EstadoDtm** (`IdEstado`): `Id`, `Nombre`, `Inicial`, `Terminado`, `Cancelado`
- **TransicionDtm**: `Id`, `Nombre`, `IdOrigen`, `IdDestino`
- **ObservacionDtm**: `Id`, `Observacion`, `FechaCreacion`, `IdUsuaCrea`
- **HitoDtm**: `Id`, `Nombre`, `Fecha`, `Completado`
- **DireccionDtm**: dirección postal asociada
- **VinculoDtm** (Archivos, Archivadores, Agenda): vínculos con otros registros
";

    }
}

