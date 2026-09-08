using System.Collections.Generic;

namespace ServicioDeReportes.Base
{
    /// <summary>
    /// Datos de un maestro relacionado con el elemento que se imprime (p.ej. su Cliente, su Proveedor...),
    /// listos para sustituir etiquetas {{{maestro.&lt;clave&gt;....}}} (ver ApiDePlantillas.ProcesarEtiquetasDeMaestros).
    /// DatosPrincipales son las propiedades del propio maestro; Direcciones y CuentasBancarias son sus listas
    /// de detalle, cada una ya aplanada con ToDictionary() igual que el resto del sistema de plantillas.
    ///
    /// Es un tipo "tonto" a propósito (sin dependencias de Gestores de negocio): quien lo rellena es
    /// GestoresDeNegocio.SistemaDocumental.DatosMaestros, que sí puede ver los Gestores de Terceros
    /// (Cliente/Proveedor/Interlocutor...) sin crear una referencia circular con este proyecto.
    /// </summary>
    public class DatosDeUnMaestro
    {
        public Dictionary<string, object> DatosPrincipales { get; set; } = new();
        public List<Dictionary<string, object>> Direcciones { get; set; } = new();
        public List<Dictionary<string, object>> CuentasBancarias { get; set; } = new();
    }
}
