
namespace ServicioDeDatos.Ventas
{
    public class ltrDeUnParteTr
    {
        public const string IdCliente = nameof(ParteTrDtm.IdCliente);
        public const string DependeDePpt = nameof(DependeDePpt);  

        public const string IdPlfDeVenta = nameof(IdPlfDeVenta);
        public const string IdFacturaEmt = nameof(IdFacturaEmt);
        public const string IdContrato = nameof(IdContrato);
        public const string IdTarea = nameof(IdTarea);
        public const string IdUnitario = nameof(IdUnitario);
        public const string IdPresupuesto = nameof(IdPresupuesto);

        public const string FiltroPorConOSinContrato = nameof(FiltroPorConOSinContrato);
        public const string FiltroPorConOSinFacturaEmt = nameof(FiltroPorConOSinFacturaEmt);
        public const string FiltroPorConOSinTarea = nameof(FiltroPorConOSinTarea);
        public const string FiltroPorConOSinPlfDeVenta = nameof(FiltroPorConOSinPlfDeVenta);
        public const string FiltroPorConOSinPresupuesto = nameof(FiltroPorConOSinPresupuesto);
        public const string EjecutadoDeUnPpt = nameof(EjecutadoDeUnPpt);
        
        public const string FiltroPorEtapa = nameof(FiltroPorEtapa);

        public static string PtrDeUnaFactura = nameof(PtrDeUnaFactura);

        public static string MostrarPtrsAsignadosPendientes = "PtrAsigPdt".ToLower();
        public static string MostrarPtrsAsignadosEjecutados = "PtrAsigEje".ToLower();

        public const string EventoDePlanificacion = $"planificación del parte de trabajo: [{nameof(ParteTrDtm.Referencia)}]";

        public const string TrazaDeCancelacionDePrefactura = $"Se canceló la prefactura: [{nameof(FacturaEmtDtm.Referencia)}]";
        public const string DescripcionDeTrazaDeCancelacionDePrefactura = $"El usuario [{nameof(ContextoSe.DatosDeConexion.Login)}] canceló la factura [{nameof(FacturaEmtDtm.Expresion)}]";
    }
}
