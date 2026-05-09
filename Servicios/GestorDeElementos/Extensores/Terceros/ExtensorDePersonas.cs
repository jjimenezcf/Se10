using ServicioDeDatos.Terceros;
using ServicioDeDatos;
using Gestor.Errores;
using ModeloDeDto.Negocio;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using ServicioDeDatos.Elemento;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDePersonas
    {
        public static InterlocutorDtm CrearInterlocutor(this PersonaDtm persona, ContextoSe contexto, bool errorSihay = true)
        {
            var inter = contexto.SeleccionarPorFk<InterlocutorDtm>(nameof(InterlocutorDtm.IdPersona), persona.Id, errorSiNoHay: false);
            if (inter is null)
            {
                return new InterlocutorDtm
                {
                    IdPersona = persona.Id,
                    Nombre = persona.Expresion,
                    Baja = false,
                    eMail = persona.eMail,
                    Telefono = persona.Telefono
                }.Insertar(contexto);
            }

            if (errorSihay)
                GestorDeErrores.Emitir($"ya existe el interlocutor '{inter.Referencia(contexto)}' asociado a la persona {persona.Referencia}");

            return inter;
        }

        public static InterlocutorDtm Interlocutor(this PersonaDtm persona, ContextoSe contexto, bool crearSiNoLoHay = true, bool errorSiNoHay = true)
        {
            var porPersona = new ClausulaDeFiltrado(nameof(InterlocutorDtm.IdPersona), enumCriteriosDeFiltrado.igual, persona.Id);

            var interlocutores = enumNegocio.Interlocutor.SeleccionarPorFiltro<InterlocutorDtm>(contexto, new List<ClausulaDeFiltrado> { porPersona });
            if (interlocutores.Count() == 0 && crearSiNoLoHay)
                return persona.CrearInterlocutor(contexto);

            if (interlocutores.Count() == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No se ha definido el interlocutor para la persona '{persona.Referencia}'");

            return interlocutores[0];
        }

        public static DireccionDto DireccionFiscal(this PersonaDtm persona, ContextoSe contexto)
        =>
        persona.DireccionDto(contexto, enumCalificadorDireccion.fiscal, true);

        public static DireccionDto DireccionDto(this PersonaDtm persona, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay)
        =>
        persona.Direccion(contexto, calificador, errorSiNoHay)?.MapearDto(contexto, enumNegocio.Persona);

        public static DireccionDtm Direccion(this PersonaDtm persona, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.Persona, persona.Id).ToList();
            var direccion = direcciones.FirstOrDefault(x => x.Calificador == calificador && x.Activo);
            if (direccion == null)
            {
                if (errorSiNoHay)
                    GestorDeErrores.Emitir($"La {enumNegocio.Persona.Singular(true)} '{persona.Referencia}' debe tener una dirección '{calificador.Descripcion()}'");
                return null;
            }
            direccion.Negocio = enumNegocio.Persona;
            return direccion;
        }


        public static DireccionDtm Direccion(this PersonaDtm persona, ContextoSe contexto, bool errorSiNoHay = false)
        {
            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.Persona, persona.Id).ToList();
            var direccion = direcciones.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.correspondencia && x.Activo);
            if (direccion is null) direccion = direcciones.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.contacto && x.Activo);
            if (direccion is null) direccion = direcciones.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.fiscal && x.Activo);
            if (direccion == null)
            {
                if (errorSiNoHay)
                    GestorDeErrores.Emitir($"La {enumNegocio.Persona.Singular(true)} '{persona.Referencia}' debe tener alguna dirección, ya sea de {enumCalificadorDireccion.correspondencia.Descripcion()}, {enumCalificadorDireccion.contacto.Descripcion()} o {enumCalificadorDireccion.fiscal.Descripcion()}");
                return null;
            }
            direccion.Negocio = enumNegocio.Persona;
            return direccion;
        }

        public static bool TieneFacturas(this PersonaDtm persona, ContextoSe contexto)
        =>
        persona.EsInterlocutor ? persona.Interlocutor(contexto, crearSiNoLoHay: false, errorSiNoHay: false).TieneFacturas(contexto) : false;


        public static PersonaDtm CrearSiNoExiste(ContextoSe contexto, string nif, string nombre, string apellido, bool esNie, string mail, string telefono)
        {
            var persona = contexto.SeleccionarPorPropiedad<PersonaDtm>(nameof(PersonaDtm.NIF), nif, false);
            if (persona == null)
            {
                persona = new PersonaDtm();
                persona.NIF = nif;
                persona.Nombre = nombre;
                persona.Apellidos = apellido;
                persona.EsNie = esNie;
                persona.eMail = mail;
                persona.Telefono = telefono;
                persona = persona.Insertar(contexto);
            }
            return persona;
        }

        public static void SincronizarConInterlocutor(this PersonaDtm persona, ContextoSe contexto, PersonaDtm anterior)
        {
            if (persona.Expresion == anterior.Expresion && persona.eMail == anterior.eMail && persona.Telefono == anterior.Telefono)
                return;

            var interlocutor = persona.Interlocutor(contexto, crearSiNoLoHay: false, errorSiNoHay: false);
            if (interlocutor is null) return;
            interlocutor.Nombre = persona.Expresion;
            if (persona.eMail != anterior.eMail) interlocutor.eMail = persona.eMail;
            if (persona.Telefono != anterior.Telefono) interlocutor.Telefono = persona.Telefono;
            interlocutor.Modificar(contexto);
            ServicioDeCaches.EliminarCache(CacheDe.Ter_NifDeProveedor);
            ServicioDeCaches.EliminarCache(CacheDe.Ter_NifDeCliente);
        }
    }
}
