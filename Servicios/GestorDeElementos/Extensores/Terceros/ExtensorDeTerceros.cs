using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using System.Text;
using Utilidades;
using static Utilidades.Ampliaciones;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeTerceros
    {
        public static string CodigoDeCtaContable(this ITerceroContable tercero, ContextoSe contexto)
        {
            return tercero.CuentaContable(contexto).Codigo + tercero.CodigoContable.ToString().PadLeft(4, '0');
        }

        public static CuentaDtm CuentaContable(this ITerceroContable tercero, ContextoSe contexto)
        =>
        tercero.Cuenta != null
        ? tercero.Cuenta
        : contexto.SeleccionarPorId<CuentaDtm>(tercero.IdCuenta);


        public static InterlocutorDtm Interlocutor(this ITerceroContable tercero, ContextoSe contexto)
        { 
            if (tercero.Interlocutor != null && tercero.Interlocutor.Id != tercero.IdInterlocutor) 
                return tercero.Interlocutor;

            var cache = ServicioDeCaches.Obtener(CacheDe.Ter_Interlocutor);
            var indice = $"{tercero.GetType().Name}-{tercero.IdInterlocutor}";
            if (!cache.ContainsKey(indice))
            {
                cache[indice] = contexto.SeleccionarPorId<InterlocutorDtm>(tercero.IdInterlocutor, aplicarJoin: true);
            }
            return (InterlocutorDtm)cache[indice];
        }


        public static void TrazarModificaciones<T>(this T tercero, ContextoSe contexto, T anterior)
        where T : IUsaTraza, IDatosDeContacto
        {
            StringBuilder mensaje = new StringBuilder();

            var negocio = NegociosDeSe.NegocioDeUnDtm(typeof(T));
            if (((IRegistro)tercero).PropiedadCambiada<string>((IRegistro)anterior, nameof(INombre.Nombre)))
            {
                mensaje.AppendLine($"el usuario '{contexto.Usuario.Login}' a modificado el nombre. Anterior: '{((INombre)anterior).Nombre}'");
            }
            if (((IRegistro)tercero).PropiedadCambiada<string>((IRegistro)anterior, nameof(IDatosDeContacto.eMail)))
            {
                mensaje.AppendLine($"el usuario '{contexto.Usuario.Login}' a modificado el eMail. Anterior: '{((IDatosDeContacto)anterior).eMail}'");
            }
            if (((IRegistro)tercero).PropiedadCambiada<string>((IRegistro)anterior, nameof(IDatosDeContacto.Telefono)))
            {
                mensaje.AppendLine($"el usuario '{contexto.Usuario.Login}' a modificado el telefono. Anterior: '{((IDatosDeContacto)anterior).Telefono}'");
            }

            if (negocio == enumNegocio.Persona)
            {
                if (((IRegistro)tercero).PropiedadCambiada<string>((IRegistro)anterior, nameof(PersonaDtm.Apellidos)))
                {
                    mensaje.AppendLine($"el usuario '{contexto.Usuario.Login}' a modificado los apellidos. Anterior: '{anterior.LeerPropiedad(nameof(PersonaDtm.Apellidos))}'");
                }
                if (((IRegistro)tercero).PropiedadCambiada<string>((IRegistro)anterior, nameof(PersonaDtm.NIF)))
                {
                    mensaje.AppendLine($"el usuario '{contexto.Usuario.Login}' a modificado el nif. Anterior: '{anterior.LeerPropiedad(nameof(PersonaDtm.NIF))}'");
                }
            }

            if (negocio == enumNegocio.Sociedad)
            {
                if (((IRegistro)tercero).PropiedadCambiada<string>((IRegistro)anterior, nameof(SociedadDtm.RazonSocial)))
                {
                    mensaje.AppendLine($"el usuario '{contexto.Usuario.Login}' a modificado la razón social. Anterior: '{anterior.LeerPropiedad(nameof(SociedadDtm.RazonSocial))}'");
                }
                if (((IRegistro)tercero).PropiedadCambiada<string>((IRegistro)anterior, nameof(SociedadDtm.NIF)))
                {
                    mensaje.AppendLine($"el usuario '{contexto.Usuario.Login}' a modificado el nif. Anterior: '{anterior.LeerPropiedad(nameof(SociedadDtm.NIF))}'");
                }
            }
            if (mensaje.Length > 0) tercero.CrearTraza(contexto, "Datos modificados", mensaje.ToString());
        }
    }
}
