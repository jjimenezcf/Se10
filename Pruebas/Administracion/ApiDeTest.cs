using GestorDeElementos;
using GestoresDeNegocio.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ValidacionesDeRn
{
    public static class ApiDeTest
    {

        public static EstadoDtm CrearEstado(ContextoSe contexto, enumNegocio negocio, string nombre, bool inicial, bool final, bool cancelado)
        {
            EstadoDtm estado = negocio.CrearEstado();
            estado.Inicial = inicial;
            estado.Cancelado = cancelado;
            estado.Terminado = final;
            estado.Nombre = nombre;
            var gestor = GestorDeEstados.Gestor(contexto, negocio);
            estado = gestor.PersistirRegistro(estado, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            var leido = gestor.LeerRegistroPorId(estado.Id);
            return leido;
        }
        public static TransicionDtm CrearTransicion(ContextoSe contexto, enumNegocio negocio, EstadoDtm e1, string nombre, EstadoDtm e2)
        {
            TransicionDtm t = new TransicionDtm();
            t.IdOrigen = e1.Id;
            t.IdDestino = e2.Id;
            t.Nombre = nombre;
            var gestor = GestorDeTransiciones.Gestor(contexto, negocio);
            t = gestor.PersistirRegistro(t, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            var leido = gestor.LeerRegistroPorId(t.Id);
            return leido;
        }
    }
}
