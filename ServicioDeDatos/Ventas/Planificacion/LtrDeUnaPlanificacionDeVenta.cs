namespace ServicioDeDatos.Ventas
{
    public class ltrDeUnaPlanificacionDeVenta
    {
        public const string IdCliente = nameof(PlanificacionDeVentaDtm.IdCliente);
        public const string CambioDePpt = nameof(CambioDePpt);
        public const string CambioDePt = nameof(CambioDePt);
        public const string EventoDeEjecucion = $"planificación de venta: [{nameof(PlanificacionDeVentaDtm.Referencia)}]";
        
        public const string IdContrato = nameof(IdContrato);
        public const string IdParteTr = nameof(IdParteTr);
        public const string IdFacturaEmt = nameof(IdFacturaEmt);
        public const string IdUnitario = nameof(IdUnitario);
        public const string IdPlanificador = nameof(IdPlanificador);

        public const string FiltroPorConOSinContrato = nameof(FiltroPorConOSinContrato);
        public const string FiltroPorConOSinParteTr = nameof(FiltroPorConOSinParteTr);
        public const string FiltroPorConOSinFacturaEmt = nameof(FiltroPorConOSinFacturaEmt);
        public const string FiltroPorConOSinPlanificador = nameof(FiltroPorConOSinPlanificador);
        public const string FiltroPorEtapa = nameof(FiltroPorEtapa);
    }
}
