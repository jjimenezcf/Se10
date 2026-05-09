using System;
using Utilidades;

namespace ServicioDeDatos.Seguridad
{
    public enum enumClaseDePermiso
    {
        Tipo,
        Estado,
        Transicion,
        CentroGestor,
        Negocio,
        Elemento,
        Funcion,
        Vista,
        Menu,
        Agenda,
        Certificado,
        Buzon,
        Exportacion,
        Plantilla
    }

    public static class ClaseDePermiso
    {
        public static string ToString(enumClaseDePermiso claseDePermiso)
        {
            return claseDePermiso.Descripcion();
        }
    }

    public enum enumModoDeAccesoDeDatos
    {
        Administrador,
        Interventor,
        Gestor,
        Consultor,
        SinPermiso,
        SerAdministrador
    }

    public enum enumModoDeAccesoFuncional
    {
        Acceso,
        SinAcceso
    }

    public static class ModoDeAcceso
    {

        public const string Creador = nameof(Creador);

        public static string Nombre(this enumModoDeAccesoDeDatos modoDeAcceso)
        {
            switch (modoDeAcceso)
            {
                case enumModoDeAccesoDeDatos.Administrador: return enumModoDeAccesoDeDatos.Administrador.ToString();
                case enumModoDeAccesoDeDatos.Gestor: return enumModoDeAccesoDeDatos.Gestor.ToString();
                case enumModoDeAccesoDeDatos.Consultor: return enumModoDeAccesoDeDatos.Consultor.ToString();
                case enumModoDeAccesoDeDatos.Interventor: return enumModoDeAccesoDeDatos.Interventor.ToString();
                case enumModoDeAccesoDeDatos.SinPermiso: return enumModoDeAccesoDeDatos.SinPermiso.ToString();
            }

            throw new Exception($"El modo de acceso de datos '{modoDeAcceso}' no está definido, no se puede parsear");
        }

        public static enumModoDeAccesoDeDatos ToModoAcceso(this string modoDeAcceso) => ApiDeEnsamblados.ToEnumerado<enumModoDeAccesoDeDatos>(modoDeAcceso);


        public static bool SoyConsultor(this enumModoDeAccesoDeDatos modoDeAcceso)
        {
            return SoyGestor(modoDeAcceso) || modoDeAcceso.Equals(enumModoDeAccesoDeDatos.Consultor);
        }

        public static bool SoyGestor(this enumModoDeAccesoDeDatos modoDeAcceso)
        {
            return SoyAdministrador(modoDeAcceso)
                 || modoDeAcceso.Equals(enumModoDeAccesoDeDatos.Gestor)
                 || modoDeAcceso.Equals(enumModoDeAccesoDeDatos.Interventor);
        }
       
        public static bool SoyAdministrador(this enumModoDeAccesoDeDatos modoDeAcceso)
        {
            return modoDeAcceso.Equals(enumModoDeAccesoDeDatos.Administrador);
        }


        public static bool SoyInterventor(this enumModoDeAccesoDeDatos modoDeAcceso)
        {
            return SoyAdministrador(modoDeAcceso)
                 || modoDeAcceso.Equals(enumModoDeAccesoDeDatos.Interventor);
        }

        public static string Render(this enumModoDeAccesoDeDatos modoDeAcceso)
        {
            return Nombre(modoDeAcceso);
        }

        public static string ToString(enumModoDeAccesoFuncional modoDeAcceso)
        {
            switch (modoDeAcceso)
            {
                case enumModoDeAccesoFuncional.Acceso: return enumModoDeAccesoFuncional.Acceso.ToString();
                case enumModoDeAccesoFuncional.SinAcceso: return enumModoDeAccesoFuncional.SinAcceso.ToString();
            }

            throw new Exception($"El modo de acceso funcional '{modoDeAcceso}' no está definido, no se puede parsear");
        }

        public static bool PuedoRelacionar(enumModoDeAccesoDeDatos permisosNecesarios, enumModoDeAccesoDeDatos m1, enumModoDeAccesoDeDatos m2)
        {
            if (permisosNecesarios.Consultor() && m1.SoyConsultor() && m2.SoyConsultor())
                return true;

            if (permisosNecesarios.Gestor() && (m1.SoyGestor() && m2.SoyConsultor() || m2.SoyGestor() && m1.SoyConsultor()))
                return true;

            if (permisosNecesarios.Administrador() && m1.SoyAdministrador() && m2.SoyAdministrador())
                return true;

            return false;
        }
        public static enumModoDeAccesoDeDatos ModoAccesoDeRelacion(enumModoDeAccesoDeDatos m1, enumModoDeAccesoDeDatos m2)
        {
            if (SoyAdministrador(m1) && SoyAdministrador(m2))
                return enumModoDeAccesoDeDatos.Administrador;

            if (SoyGestor(m1) && SoyGestor(m2))
                return enumModoDeAccesoDeDatos.Gestor;

            if ((SoyGestor(m1) && SoyConsultor(m2)) || (SoyConsultor(m1) && SoyGestor(m2)))
                return enumModoDeAccesoDeDatos.Gestor;

            if (!SoyConsultor(m1) && !SoyConsultor(m1))
                return enumModoDeAccesoDeDatos.SinPermiso;

            return enumModoDeAccesoDeDatos.Consultor;
        }

        public static bool HayPermisosDe(this enumModoDeAccesoDeDatos modoQueTengo, enumModoDeAccesoDeDatos modoSolicitado)
        {
            if (modoQueTengo == modoSolicitado) return true;
            if (modoQueTengo.Administrador()) return true;
            if (modoSolicitado.Intervetor() && modoQueTengo.SoyGestor()) return true;
            if (modoSolicitado.Gestor() && modoQueTengo.SoyGestor()) return true;
            if (modoSolicitado.Consultor() && modoQueTengo.SoyConsultor()) return true;
            return false;
        }


        public static bool Consultor(this enumModoDeAccesoDeDatos modoQueTengo) => modoQueTengo == enumModoDeAccesoDeDatos.Consultor;
        public static bool Intervetor(this enumModoDeAccesoDeDatos modoQueTengo) => modoQueTengo == enumModoDeAccesoDeDatos.Interventor;
        public static bool Gestor(this enumModoDeAccesoDeDatos modoQueTengo) => modoQueTengo == enumModoDeAccesoDeDatos.Gestor;
        public static bool Administrador(this enumModoDeAccesoDeDatos modoQueTengo) => modoQueTengo == enumModoDeAccesoDeDatos.Administrador;
    }




}
