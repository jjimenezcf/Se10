class Program
{
    static int Main(string[] args) => Desplegador.Ejecutar(args, new Destino(
        Nombre: "DesplieguePreProductivo",
        Alcance: "SOLO el sitio de preproductivo (test.se)",
        ScriptDescomprimir: "20 - Preproductivo Descomprimir y parar servicios.cmd",
        ScriptDesplegar: "20 - Preproductivo Desplegar y arrancar servicios.cmd"));
}
