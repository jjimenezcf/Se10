class Program
{
    // Para después de probar en test: el zip ya está descomprimido en C:\Prepublicacion\Se (lo dejó el despliegue de preproductivo),
    // así que solo se paran los servicios y se copia a los sitios, sin publicar, empaquetar, copiar ni descomprimir
    static int Main(string[] args) => Desplegador.Ejecutar(args, new Destino(
        Nombre: "DespliegueTrasProbar",
        Alcance: "TODOS los sitios, con lo ya descomprimido y probado en test.se",
        ScriptDescomprimir: "03-Parar servicios sin descomprimir.cmd",
        ScriptDesplegar: "02-Desplegar y arrancar servicios.cmd",
        ZipYaDescomprimido: true,
        SitioProbado: "test.se"));
}
