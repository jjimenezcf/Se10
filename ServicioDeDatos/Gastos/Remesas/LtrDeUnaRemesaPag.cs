namespace ServicioDeDatos.Gastos
{
    public class ltrDeUnaRemesaPag
    {
        public static string EventoDePago => $"Pago: [{nameof(RemesaPagDtm.Referencia)}]";

        public const string IdProveedor = nameof(PagoDtm.IdProveedor);
        public const string IdFacturaRec = nameof(PagoDtm.IdFacturaRec);        

        public const string IdAcreedor = nameof(PagoDtm.IdSolicitante);
        public const string ClaseDeAcreedor = nameof(ClaseDeAcreedor);
        public const string CualquierAcreedor = "AC";
        public const string SoloProveedores = "PR";
        public const string SoloTrabajadores = "TR";
        public const string FiltroPorFactura = nameof(FiltroPorFactura);

        public const string FiltroPorImporte = nameof(FiltroPorImporte);
        public const string FiltroPorFechaDeGeneracion = nameof(FiltroPorFechaDeGeneracion);
        public const string FiltroPorFechaDePago = nameof(FiltroPorFechaDePago);
        public const string IdPagoEnRemesa = nameof(IdPagoEnRemesa);
        

        public const string SelectorParaUnPago = nameof(SelectorParaUnPago);

        public const string IdArchivoSepa = nameof(IdArchivoSepa);

        public const string Accion_PagoDeRemesa = nameof(Accion_PagoDeRemesa);
        public const string Accion_AnularPagoDeRemesa = nameof(Accion_AnularPagoDeRemesa);
        public const string Accion_AnularPago = nameof(Accion_AnularPago);
        public const string Accion_CambiarFechaDePago = nameof(Accion_CambiarFechaDePago);
        public const string Accion_AnularAnulacionDePago = nameof(Accion_AnularAnulacionDePago);
        public const string Accion_AsociarSepa = nameof(Accion_AsociarSepa);
        public const string Accion_AsociarAuditoriaSii = nameof(Accion_AsociarAuditoriaSii);

    }
}
