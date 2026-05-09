using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Terceros;


namespace SistemaDeElementos.Inicializador.AytoBeniel
{
    public static class InzMaestrosBeniel
    {
        public const string n_nif_beniel = "P3001000C";
        public const string n_sociedad_beniel_nombre = "Ayuntamiento De Beniel";
        public const string n_sociedad_beniel_cf = "BNL";
        public const int n_sociedad_beniel_cc = 3;
        public static string n_cg_beniel_codigo = n_sociedad_beniel_cf;
        public static string n_cg_beniel_codigo_atc = $"{n_cg_beniel_codigo}.10";
        public static string n_cg_beniel_codigo_urb = $"{n_cg_beniel_codigo}.20";
        public static string n_cg_beniel_nombre = "Ayto de Beniel";
        public const string n_cg_beniel_nombre_atc = "Atención al ciudadano";
        public const string n_cg_beniel_nombre_urb = "Urbanismo";

        public static void Sociedad(ContextoSe contexto)
        {
            var ayto = ExtensorDeSociedades.CrearSiNoExiste(contexto, n_nif_beniel, n_sociedad_beniel_nombre, n_sociedad_beniel_nombre, n_sociedad_beniel_cf, "alcaldia@beniel.es", "968.60.04.94");
            DefinirServiciosDeUnAyto(contexto, ayto, n_cg_beniel_nombre);
        }

        private static void DefinirServiciosDeUnAyto(ContextoSe contexto, SociedadDtm ayto, string nombreCgPadre)
        {
            var cgPadre = ayto.CrearCg(contexto, nombreCgPadre);
            cgPadre.CrearHijo(contexto, n_cg_beniel_nombre_atc, n_cg_beniel_codigo_atc);
            cgPadre.CrearHijo(contexto, n_cg_beniel_nombre_urb, n_cg_beniel_codigo_urb);
        }

    }
}
