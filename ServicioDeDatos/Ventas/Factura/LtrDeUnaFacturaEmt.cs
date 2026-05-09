namespace ServicioDeDatos.Ventas
{
    public class ltrDeUnaFacturaEmt
    {
        public static string EventoDeVencimiento => $"Vencimiento: [{nameof(FacturaEmtDtm.NumeroDeFactura)}]";

        public const string IdSociedad = nameof(IdSociedad);
        public const string IdCliente = nameof(FacturaEmtDtm.IdCliente);
        public const string DependeDePpt = nameof(DependeDePpt);
        public const string DependeDeParteTr = nameof(DependeDeParteTr);
        public const string CambioDePpt = nameof(CambioDePpt);
        public const string CambioDeParteEnFae = nameof(CambioDeParteEnFae);
        public const string DependeDePlfVenta = nameof(DependeDePlfVenta);
        public const string FacturaDelParte = nameof(FacturaDelParte);
        public const string PrefacturaEmitida = nameof(PrefacturaEmitida);
        public const string FacturadoComoLinea = nameof(FacturadoComoLinea);

        public const string IdExpediente = nameof(IdExpediente);
        public const string IdEstimacionDirecta = nameof(IdEstimacionDirecta);
        public const string IdLoteContable = nameof(IdLoteContable);
        
        public const string IdPresupuesto = nameof(IdPresupuesto);
        public const string NombrePresupuesto = nameof(NombrePresupuesto);        
        public const string AsociadaAUnPpt = nameof(AsociadaAUnPpt);

        public const string IdRemesaFae = nameof(IdRemesaFae);
        public const string AsociadaAUnaRemesa = nameof(AsociadaAUnaRemesa);

        public const string IdPlfDeVenta = nameof(IdPlfDeVenta);
        public const string AsociadaAUnaPlv = nameof(AsociadaAUnaPlv);
        public const string NombrePlfDeVenta = nameof(NombrePlfDeVenta);        

        public const string IdParteTr = nameof(IdParteTr);
        public const string AsociadaAUnPtr = nameof(AsociadaAUnPtr);
        public const string NombreParteTr = nameof(NombreParteTr);

        public const string IdContrato = nameof(IdContrato);
        public const string AsociadaAUnContrato = nameof(AsociadaAUnContrato);
        public const string IncluidaEnRemesa = nameof(IncluidaEnRemesa);
        public const string Rectificadas = nameof(Rectificadas);

        public const string FiltrarPorRectificativa = nameof(FiltrarPorRectificativa);
        public const string FiltroPorImporteSinIva = nameof(FiltroPorImporteSinIva);
        public const string FiltroPorFechaDeEmision = nameof(FiltroPorFechaDeEmision);
        public const string FiltroPorFechaDeVencimiento = nameof(FiltroPorFechaDeVencimiento);
        public const string FiltroPorNumerosDeFactura = nameof(FiltroPorNumerosDeFactura);
        public const string FiltroPorNumeroDeFactura = nameof(FiltroPorNumeroDeFactura);
        public const string FiltroPorExprexion = nameof(FacturaEmtDtm.Expresion);
        public const string FiltrarPorNombreCliente = nameof(FiltrarPorNombreCliente);        
        public const string FiltroPorCobrado = nameof(FiltroPorCobrado);
        public const string PrefacturaDeUnPpt = nameof(PrefacturaDeUnPpt);
        public const string FacturaDeUnaTarea = nameof(FacturaDeUnaTarea);
        public const string FiltroPorEtapa = nameof(FiltroPorEtapa);
        public const string AnoDeEmisison = nameof(AnoDeEmisison);  
        public const string MesDeEmision = nameof(MesDeEmision);
        public const string BuscarPorVarios = nameof(BuscarPorVarios);      


        public const string IdArchivoFactura = nameof(IdArchivoFactura);
        public static string NombreTipoArchivadorDeReclamacion(string sigla) => $"{sigla}: Reclamación";

        public const string ExcluirFacturasDeUnaRemesa = "ExcluirFr";

        public const string Accion_AsociarArchivo = nameof(Accion_AsociarArchivo);
        public const string Accion_CrearArchivadorDeReclamacion = nameof(Accion_CrearArchivadorDeReclamacion);
        public const string Accion_CambiarVencimiento = nameof(Accion_CambiarVencimiento);
        public const string Accion_CambiarDatos = nameof(Accion_CambiarDatos);
        public const string Accion_EmitirFactura = nameof(Accion_EmitirFactura);

        public const string EtiquetaDeFacturaRectificativa = "Rectificativa";
        public const string IdRectificativa = nameof(IdRectificativa);
        public const string NombreRectificativa = nameof(NombreRectificativa);
    }
}
