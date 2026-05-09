using ServicioDeDatos;
using SistemaDeElementos.Inicializador.AytoBeniel;

namespace SistemaDeElementos.Inicializador
{
    public static class InzUsuariosBeniel
    {

        public const string n_usuario_ventanilla = "ventanilla.beniel";
        public const string n_usuario_administrativo = "administrativo.beniel";
        public const string n_usuario_tecnico = "tecnico.beniel";
        public const string n_usuario_jefe_atc = "js.atc";
        public const string n_usuario_jefe_urb = "js.urb";
        public const string n_usuario_responsable_ventanilla = "resposable.registro";

        public static void CrearUsuarios(ContextoSe contexto)
        {
            InzSeguridadComun.CrearUsuarioEnPuesto(contexto, n_usuario_ventanilla, "José", "Villanueva", InzSeguridadBeniel.n_pt_PuestoDeVentanilla);
            InzSeguridadComun.CrearUsuarioEnPuesto(contexto, n_usuario_responsable_ventanilla, "María", "López", InzSeguridadBeniel.n_pt_ResponsableDeVentanilla);
            InzSeguridadComun.CrearUsuarioEnPuestoPorNombreDeCg(contexto, n_usuario_jefe_atc, "Luis", "Berlanga", InzSeguridadBeniel.n_pt_JefeDeServicio, InzMaestrosBeniel.n_cg_beniel_nombre_atc);
            InzSeguridadComun.CrearUsuarioEnPuestoPorNombreDeCg(contexto, n_usuario_jefe_urb, "Manolo", "Pérez", InzSeguridadBeniel.n_pt_JefeDeServicio, InzMaestrosBeniel.n_cg_beniel_nombre_urb);
            InzSeguridadComun.CrearUsuarioEnPuestoPorNombreDeCg(contexto, n_usuario_tecnico, "Antonia", "Guillén", InzSeguridadBeniel.n_pt_Tecnico, InzMaestrosBeniel.n_cg_beniel_nombre_urb);
            InzSeguridadComun.CrearUsuarioEnPuestoPorNombreDeCg(contexto, n_usuario_administrativo, "Soledad", "Jiménez", InzSeguridadBeniel.n_pt_Administrativo, InzMaestrosBeniel.n_cg_beniel_nombre_atc);
        }

    }
}
