using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.TrabajosSometidos;
using Inicializador.Procesos;
using ServicioDeDatos;
using SistemaDeElementos.Inicializador;
using SistemaDeElementos.Inicializador.AytoBeniel;

namespace ValidacionesBase
{
    // De ValidacionesContexto/TestDefinirFlujos sólo lo que usa TestConRegistrosEs (DefinirFlujoRee)
    public class TestDefinirFlujos
    {
        public static void CrearFlujo(ContextoSe contexto)
        {
            InzAcciones.DefinirAcciones(contexto);
            InzNegocios.DefinirAccionesDeRelacion(contexto);

            InzSeguridadComun.SeguridadFuncional(contexto);
            InzSeguridadComun.SeguridadDatosComunes(contexto);

            InzMaestrosBeniel.Sociedad(contexto);
            InzRegistroEs.ModeloDeRegistroEs(contexto);

            InzTareasRre.ModeloDeTareasTrr(contexto);

            InzSeguridadBeniel.CrearRolesDeDatos(contexto);
            InzSeguridadBeniel.CrearPuestosDeTrabajo(contexto);
            InzUsuariosBeniel.CrearUsuarios(contexto);

            InzUsuariosBeniel.CrearUsuarios(contexto);
            TrabajosDeEntorno.SometerGenerarSeguridad(contexto).EjecutarTrabajo(contexto);
        }
    }
}
