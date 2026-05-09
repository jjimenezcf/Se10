using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Terceros;
using Inicializador.Expedientes;
using ModeloDeDto.Expediente;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Terceros;
using SistemaDeElementos.Inicializador.Mra;

namespace SistemaDeElementos.Inicializador.Datos
{
    public static class InzDatos
    {

        public static ExpedienteDto CrearExpediente(ContextoSe contexto)
        {
            var persona =  CrearPersona(contexto);
            var solicitante = persona.CrearInterlocutor(contexto, errorSihay: false);

            contexto.AsignarUsuario(contexto.SeleccionarPorPropiedad<UsuarioDtm>(nameof(UsuarioDtm.Login), InzMra.n_usuario_gerente));

            var expediente = contexto.SeleccionarPorPropiedad<ExpedienteDtm>(nameof(ExpedienteDtm.Nombre), "Mi primer expediente de prueba", errorSiNoHay: false);
            if (expediente == null)
            {
                expediente = new ExpedienteDtm();
                expediente.IdCg = contexto.SeleccionarPorPropiedad<CentroGestorDtm>(nameof(CentroGestorDtm.Nombre), InzMra.n_cg_mra_nombre).Id;
                expediente.IdTipo = contexto.SeleccionarPorPropiedad<TipoDeExpedienteDtm>(nameof(TipoDeExpedienteDtm.Nombre), InzProcesosJuridicos.n_exp_tipo_expediente_juridico).Id;
                expediente.IdSolicitante = solicitante.Id;
                expediente.IdResponsable = contexto.DatosDeConexion.IdUsuario;
                expediente.Nombre = "Mi primer expediente de prueba";
                expediente.Descripcion = "Creamos un expediente para poder añadirle una tarea, varios archivadores, y algun evento";

                expediente = expediente.Insertar(contexto);
            }

            return expediente.MapearDto<ExpedienteDto>(contexto);

        }

        private static PersonaDtm CrearPersona(ContextoSe contexto)
        {
            var persona = contexto.SeleccionarPorPropiedad<PersonaDtm>(nameof(PersonaDtm.NIF), "27485405Z", errorSiNoHay: false);
            if (persona == null)
            {
                persona = new PersonaDtm();
                persona.NIF = "27485405Z";
                persona.Nombre = "Juan";
                persona.Apellidos = "Jimenez-Cervantes Frigols";
                persona.Baja = false;
                persona.eMail = "jjimenezcf@gmail.com";
                persona.Telefono = "619.70.25.47"; 
                persona = persona.Insertar(contexto);
            }

            return persona;
        }
    }
}
