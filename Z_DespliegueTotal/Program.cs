class Program
{
    static int Main(string[] args) => Desplegador.Ejecutar(args, new Destino(
        Nombre: "DespliegueTotal",
        Alcance: "TODOS los sitios",
        ScriptDescomprimir: "01-Descomprimir y parar servicios.cmd",
        ScriptDesplegar: "02-Desplegar y arrancar servicios.cmd"));
}
