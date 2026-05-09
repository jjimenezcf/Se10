using ServicioDeDatos.Negocio;
using System.ComponentModel;
using Utilidades;

namespace ServicioDeDatos.SistemaDocumental
{

    public enum enumParametrosDeArchivadores
    {
        [Description("Indica la clave del sistema ConvertApi")]
        ARC_Clave_ConvertApi,
        [Description("Indica si al importar un ZIP somete la descompresión")]
        ARC_Someter_Importar_ZIP,
        [Description("Indica para el negocio donde se encuentra, si el estado pertenece a la etapa documental")]
        NEGOCIO_Etapa_Documental,
        [Description("Indica si hay sincronización de archivos con alguna carpeta")]
        ARC_Sincronizacion_Habilitada,
        [Description("Indica la localización que se muestra en un archivo pdf firmado")]
        CER_Location,
        [Description("Indica el Sistema Informático en un archivo pdf firmado")]
        CER_Razon,
    }

    public static class ParametrosDeArchivadores
    {
        public static string ClaveConvertApi => enumNegocio.Archivador.Parametro(enumParametrosDeArchivadores.ARC_Clave_ConvertApi).Valor;
        public static string LugarDeFirma => enumNegocio.Archivador.Parametro(enumParametrosDeArchivadores.CER_Location, crearParametro: true, valorPorDefecto: "España").Valor;
        public static string SistemaInformaticoQueFirma => enumNegocio.Archivador.Parametro(enumParametrosDeArchivadores.CER_Razon, crearParametro: true, valorPorDefecto: "Firmado por S.E.").Valor;

    }

}
