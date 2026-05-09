
namespace ModeloDeDto
{
    public static class ltrFiltros
    {
        public static string Nombre = nameof(Nombre).ToLower();
        public static string NombreDeArchivo = nameof(NombreDeArchivo).ToLower();
        public static string Observacion = nameof(Observacion).ToLower();
        public static string Estados = nameof(Estados).ToLower();
        public static string IdsDeEstado = nameof(IdsDeEstado).ToLower();
        public static string FechasDeEstado = nameof(FechasDeEstado).ToLower();
        public static string IdsDeTransicion = nameof(IdsDeTransicion).ToLower();
        public static string FechasDeTransiciones = nameof(FechasDeTransiciones).ToLower();
        public static string Id = nameof(Id).ToLower();
        public const string IdEditado = nameof(IdEditado);
        public static string filtro = nameof(filtro).ToLower();
        public static string posicion = nameof(posicion).ToLower();
        public static string orden = nameof(orden).ToLower();
        public static string cantidad = nameof(cantidad).ToLower();
        public static string separadorDeValoresEnLista = nameof(separadorDeValoresEnLista).ToLower();
        public static readonly string VincularCon = nameof(VincularCon).ToLower();
        public static readonly string VinculadosA = nameof(VinculadosA).ToLower();
        public const string SeleccionarDestino = nameof(SeleccionarDestino);
        public const string IdOrigenDiferente = nameof(IdOrigenDiferente);
        public const string enumNegocio = nameof(enumNegocio);
        public const string CargarGridDeRelacion = nameof(CargarGridDeRelacion);
        public static string NombreTipo = nameof(NombreTipo).ToLower();
        public static string NombreCg = nameof(NombreCg).ToLower();

        public const string SinRelacion = "6";

        public const string FiltroPorDirecciones = nameof(FiltroPorDirecciones);
        public const string Expresion = nameof(Expresion);

        public const string FiltroPorEtapa = nameof(FiltroPorEtapa);
    }

    public static class ltrParametrosDto
    {
        public static string DescargarGestionDocumental = "descargar-gestion-documental";
        public const string Negocio = nameof(Negocio);
    }

    public enum enumModoDeTrabajo { Nuevo, Consulta, Edicion, Mantenimiento, Jerarquia, NuevaRelacion }

    public enum enumAliniacion { no_definida, izquierda, centrada, derecha, justificada };

    
}