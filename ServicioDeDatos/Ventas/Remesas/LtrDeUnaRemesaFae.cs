namespace ServicioDeDatos.Ventas
{
    public class ltrDeUnaRemesaFae
    {
        public static string EventoDeCargo => $"Cargo: [{nameof(RemesaFaeDtm.Referencia)}]";

        public const string IdCliente = nameof(FacturaEmtDtm.IdCliente);


        public const string FiltroPorImporte = nameof(FiltroPorImporte);
        public const string FiltroPorImporteCobrado = nameof(FiltroPorImporteCobrado);
        public const string FiltroPorFechaDeGeneracion = nameof(FiltroPorFechaDeGeneracion);
        public const string FiltroPorFechaDeCargo = nameof(FiltroPorFechaDeCargo);
        public const string IdFacturaEnRemesa = nameof(IdFacturaEnRemesa);
        

        public const string SelectorParaUnFacturaEmt = nameof(SelectorParaUnFacturaEmt);

        public const string IdArchivoSepa = nameof(IdArchivoSepa);

        public const string Accion_CargoDeRemesa = nameof(Accion_CargoDeRemesa);
        public const string Accion_AnularCargoDeRemesa = nameof(Accion_AnularCargoDeRemesa);
        public const string Accion_DevolverFactura = nameof(Accion_DevolverFactura);
        public const string Accion_AnularDevolucionDeFactura = nameof(Accion_AnularDevolucionDeFactura);
        public const string Accion_AsociarQ19 = nameof(Accion_AsociarQ19);
        
    }
}
