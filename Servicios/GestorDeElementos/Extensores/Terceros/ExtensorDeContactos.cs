using ServicioDeDatos.Terceros;
using ServicioDeDatos;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using Gestor.Errores;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeContactos
    {
        public static InterlocutorDtm CrearInterlocutor(this ContactoDtm contacto, ContextoSe contexto, bool errorSihay = false)
        {
            var porContacto = new ClausulaDeFiltrado(nameof(InterlocutorDtm.IdContacto), enumCriteriosDeFiltrado.igual, contacto.Id);
            var interlocutores = enumNegocio.Interlocutor.SeleccionarPorFiltro<InterlocutorDtm>(contexto, new List<ClausulaDeFiltrado> { porContacto });
            if (interlocutores.Count() == 0)
            {
                return new InterlocutorDtm
                {
                    IdSociedad = contacto.IdSociedad,
                    IdContacto = contacto.Id,
                    Baja = false,
                    eMail = contacto.eMail,
                    Telefono = contacto.Telefono
                }.Insertar(contexto);
            }

            if (errorSihay)
                GestorDeErrores.Emitir($"ya existe el interlocutor '{interlocutores[0].Referencia(contexto)}' asociado al contacto '{contacto.Referencia(contexto)}'");

            return interlocutores[0];
        }

        public static InterlocutorDtm Interlocutor(this ContactoDtm contacto, ContextoSe contexto, bool crearSiNoLoHay = true, bool errorSiNoHay = true)
        {
            var porContacto = new ClausulaDeFiltrado(nameof(InterlocutorDtm.IdContacto), enumCriteriosDeFiltrado.igual, contacto.Id);
            var interlocutores = enumNegocio.Interlocutor.SeleccionarPorFiltro<InterlocutorDtm>(contexto, new List<ClausulaDeFiltrado> { porContacto });
            if (interlocutores.Count() == 0 && crearSiNoLoHay)
                return contacto.CrearInterlocutor(contexto);

            if (interlocutores.Count() == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No se ha definido el interlocutor para la persona '{contacto.Referencia(contexto)}'");

            return interlocutores[0];
        }

        public static InterlocutorDtm ModificarNombreInterlocutor(this ContactoDtm contacto, ContextoSe contexto, bool errorSihay = false)
        {
            var porContacto = new ClausulaDeFiltrado(nameof(InterlocutorDtm.IdContacto), enumCriteriosDeFiltrado.igual, contacto.Id);
            var interlocutores = enumNegocio.Interlocutor.SeleccionarPorFiltro<InterlocutorDtm>(contexto, new List<ClausulaDeFiltrado> { porContacto });
            if (interlocutores.Count() == 1)
            {
                return interlocutores[0].Modificar(contexto);
            }
            return null;
        }

        public static SociedadDtm Sociedad(this ContactoDtm contacto, ContextoSe contexto)
        =>
        contacto.Sociedad ??= contexto.SeleccionarPorId<SociedadDtm>(contacto.IdSociedad, aplicarJoin: true);

        public static string Referencia(this ContactoDtm contacto, ContextoSe contexto)
        => 
        $"{contacto.Sociedad(contexto).Referencia} ({contacto.Nombre})";

        public static string Expresion(this ContactoDtm contacto, ContextoSe contexto)
        {
            if (contacto.Sociedad == null)
                contacto.Sociedad = contexto.SeleccionarPorId<SociedadDtm>(contacto.IdSociedad, aplicarJoin: true);
            return contacto.Expresion;
        }

        public static void SincronizarConInterlocutor(this ContactoDtm contacto, ContextoSe contexto, ContactoDtm anterior)
        {
            if (contacto.Expresion == anterior.Expresion && contacto.eMail == anterior.eMail && contacto.Telefono == anterior.Telefono)
                return;

            var interlocutor = contacto.Interlocutor(contexto, crearSiNoLoHay: false, errorSiNoHay: false);
            if (interlocutor is null) return;
            interlocutor.Nombre = contacto.Expresion(contexto);
            if (contacto.eMail != anterior.eMail) interlocutor.eMail = contacto.eMail;
            if (contacto.Telefono != anterior.Telefono) interlocutor.Telefono = contacto.Telefono;
            interlocutor.Modificar(contexto);
        }
    }
}
