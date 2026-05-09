namespace ServicioDeDatos.Gastos
{
    public class ltrDeUnaFacturaRec
    {
        public static string EventoDeVencimiento => $"Vencimiento: [{nameof(FacturaRecDtm.Numero)}]";

        public const string FiltroPorProveedor = nameof(FacturaRecDtm.IdProveedor);
        public const string FiltroPorNaturaleza = nameof(FacturaRecDtm.IdNaturaleza);
        public const string IdRemesaPag = nameof(IdRemesaPag);

        public const string IdContrato = nameof(IdContrato);
        public const string IdExpediente = nameof(IdExpediente);
        public const string IdEstimacionDirecta = nameof(IdEstimacionDirecta);
        public const string IdLoteContable = nameof(IdLoteContable);
        public const string AsociadaAUnContrato = nameof(AsociadaAUnContrato);
        public const string AsociadaAUnExpediente = nameof(AsociadaAUnExpediente);
        public const string AsociadaAUnLoteContable = nameof(AsociadaAUnLoteContable);
        public const string VinculosALotes = nameof(VinculosALotes);

        public const string AsociadaAUnElemento = nameof(AsociadaAUnElemento);


        public const string FiltroPorImporteSinIva = nameof(FiltroPorImporteSinIva);
        public const string FiltroPorTotalFactura = nameof(FiltroPorTotalFactura);
        public const string FiltroPorFechaDeEmision = nameof(FiltroPorFechaDeEmision);
        public const string FiltroPorFechaDeVencimiento = nameof(FiltroPorFechaDeVencimiento);
        public const string FiltroPorNumerosDeFactura = nameof(FiltroPorNumerosDeFactura);
        public const string FiltroPorRemesaPag = nameof(FiltroPorRemesaPag);
        public const string FiltroPorEjercicioDeFactura = nameof(FiltroPorEjercicioDeFactura);
        public const string FiltroPorEtapa = nameof(FiltroPorEtapa);

        public const string FiltroSiHayPreasiento = nameof(FiltroSiHayPreasiento);
        public const string FiltroConSpr = nameof(FiltroConSpr);
        public const string FiltroSinSpr = nameof(FiltroSinSpr);
        public const string FiltroConSprCan = nameof(FiltroConSprCan);
        

        public const string SelectorDeFacturaRemesada = nameof(SelectorDeFacturaRemesada);
        public const string FacturasPosiblesDelContrato = nameof (FacturasPosiblesDelContrato);
        public const string FacturasImputablesEnUnExpediente = nameof(FacturasImputablesEnUnExpediente);
        public const string SelectorDeFacturasNoRecificadas = nameof(SelectorDeFacturasNoRecificadas);


        public const string IdArchivoFactura = nameof(IdArchivoFactura);

        public const string ContactoImportado  = "Contacto importado del la eFactura";
        public const string TrazaDeIncorporacion = "eFactura Incorporada";

        public const string Accion_IncorporarFacturaE = nameof(Accion_IncorporarFacturaE);
        public const string Accion_CrearPagoAlCrearFactura = nameof(Accion_CrearPagoAlCrearFactura);
        public const string Accion_CopiarFactura = nameof(Accion_CopiarFactura);
        
        public const string AsuntoNumeroReferencia = nameof(AsuntoNumeroReferencia);
        public const string TrazaDeCopiaDeFactura = nameof(TrazaDeCopiaDeFactura);
        public const string TrazaDeRectificacionDeFactura = nameof(TrazaDeRectificacionDeFactura);
        public const string Accion_DevolverAContabilidad = nameof(Accion_DevolverAContabilidad);
        public const string Accion_EnviarAPagar = nameof(Accion_EnviarAPagar);
        public const string Accion_DarPorPagada = nameof(Accion_DarPorPagada);
        public const string Accion_CambiarProveedor = nameof(Accion_CambiarProveedor);
        public const string Accion_ModificarNaturalezas = nameof(Accion_ModificarNaturalezas);
        public const string Accion_ModificarIva = nameof(Accion_ModificarIva);
        public const string Accion_GenerarPreasiento = nameof(Accion_GenerarPreasiento);
        public const string Accion_QuitarContrato = nameof(Accion_QuitarContrato);
        public const string Accion_ImputarContrato = nameof(Accion_ImputarContrato);
    }
}
