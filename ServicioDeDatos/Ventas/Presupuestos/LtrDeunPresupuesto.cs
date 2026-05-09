
namespace ServicioDeDatos.Presupuesto
{
    public class ltrDeUnPresupuesto
    {
        public const string IdExpediente = nameof(PresupuestoDtm.IdExpediente);
        public const string IdCliente = nameof(PresupuestoDtm.IdSolicitante);
        public const string IdTarea = nameof(IdTarea);
        public const string IdResponsable = nameof(PresupuestoDtm.IdResponsable);
        public const string DependeDeExpediente = nameof(DependeDeExpediente);
        public const string AsociarExpediente = nameof(AsociarExpediente);
        public static string TrazaDelCambioDeExpediente = nameof(TrazaDelCambioDeExpediente);
        
        public static string PptDeUnExpediente = nameof(PptDeUnExpediente);        
        public static string PptDeUnaFactura = nameof(PptDeUnaFactura);
        public static string PptDeUnPartTr = nameof(PptDeUnPartTr);
        public static string PptsConTareas = nameof(PptsConTareas);        
        public static string PptDeUnaAsignacionPtr = nameof(PptDeUnaAsignacionPtr);

        public static string IdFacturaEmt = nameof(IdFacturaEmt);
        public static string IdParteTr = nameof(IdParteTr);
        public const string FiltroPorConOSinFacturaEmt = nameof(FiltroPorConOSinFacturaEmt);
        
        public const string FiltroPorConOSinParteTr = nameof(FiltroPorConOSinParteTr);
        public const string ConPartesPdtDeFacturar = nameof(ConPartesPdtDeFacturar);
        public const string ConPartesFacturados = nameof(ConPartesFacturados);

        public const string FiltroPorPartesPrefacturados = nameof(FiltroPorPartesPrefacturados);
        public const string SelectorParaUnaFacturaEmt = nameof(SelectorParaUnaFacturaEmt);
    }
}
