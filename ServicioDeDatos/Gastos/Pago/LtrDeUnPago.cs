namespace ServicioDeDatos.Gastos
{
    public class ltrDeUnPago
    {
        public static string EventoDePago => $"Pagar: [{nameof(PagoDtm.Referencia)}]";

        public const string IdAcreedor = nameof(PagoDtm.IdSolicitante);
        
        public const string ClaseDeAcreedor = nameof(ClaseDeAcreedor);
        public const string IdCuentaDePago = nameof(IdCuentaDePago);
        public const string IdFacturaRec = nameof(PagoDtm.IdFacturaRec);
        public const string IdLoteContable = nameof(IdLoteContable);
        
        public const string CualquierAcreedor = "AC";
        public const string SoloProveedores = "PR";
        public const string SoloTrabajadores = "TR";

        public const string CualquierPago = "CP";
        public const string SoloConFacturas = "SCF";
        public const string SoloSinFacturas = "SSF";

        public const string DependeDeFae = nameof(DependeDeFae);

        public const string IdRemesaPag = nameof(IdRemesaPag);
        public const string AsociadaAUnaRemesa = nameof(AsociadaAUnaRemesa);
        public const string IncluidaEnRemesa = nameof(IncluidaEnRemesa);

        public const string FiltroPorImporte = nameof(FiltroPorImporte);
        public const string FiltroPorPagadoEl = nameof(FiltroPorPagadoEl);
        public const string FiltroPorPagarEl = nameof(FiltroPorPagarEl);
        public const string FiltroPorFactura = nameof(FiltroPorFactura);
        public const string FiltroPorEtapa = nameof(FiltroPorEtapa);

        public const string FiltroSiHayPreasiento = nameof(FiltroSiHayPreasiento);
        public const string FiltroConSpr = nameof(FiltroConSpr);
        public const string FiltroSinSpr = nameof(FiltroSinSpr);
        public const string FiltroConSprCan = nameof(FiltroConSprCan);
        public const string FiltroConFacSin = nameof(FiltroConFacSin);
        

        public const string ExcluirPagosDeUnaRemesa = "ExcluirPag";

        public const string FiltroPorFormaDePago = nameof(FiltroPorFormaDePago);
        public const string FiltroDePagosContado = nameof(FiltroDePagosContado);
        public const string FiltroDePagosTarjeta = nameof(FiltroDePagosTarjeta);
        public const string FiltroDePagosDomiciliado = nameof(FiltroDePagosDomiciliado);
        public const string FiltroDePagosTransferencia = nameof(FiltroDePagosTransferencia);
        public const string FiltroDePagosRemesa = nameof(FiltroDePagosRemesa);

        
        public const string FiltroDeIvaIrpf = nameof(FiltroDeIvaIrpf);
        public const string FiltroConIva = nameof(FiltroConIva);
        public const string FiltroConIrpf = nameof(FiltroConIrpf);
        public const string FiltroConIvaExento = nameof(FiltroConIvaExento);
        public const string FiltroSinIvaNiIrpf = nameof(FiltroSinIvaNiIrpf);
        public const string FiltroConIvaIsp = nameof(FiltroConIvaIsp);
        public const string FiltroConIvaNsj = nameof(FiltroConIvaNsj);

        public const string Accion_Pagar = nameof(Accion_Pagar);
        public const string Accion_DarPorPagadoAlTransitarFactura = nameof(Accion_DarPorPagadoAlTransitarFactura);
        public const string Accion_CambiarProveedor = nameof(Accion_CambiarProveedor);
        public const string Accion_GenerarPreasiento = nameof(Accion_GenerarPreasiento);
        public const string Accion_CrearAbono = nameof(Accion_CrearAbono);

        public const string Mensaje_FaltaDePago = "El pago '{0}' debe tener un justificante de pago, asocie el archivo";

    }
}
