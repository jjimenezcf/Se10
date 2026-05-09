using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Negocio;
using Microsoft.Extensions.Configuration;
using ModeloDeDto.Entorno;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Negocio;
using Utilidades;

namespace SistemaDeElementos.Inicializador.Datos
{
    public static class InzEntorno
    {
        private static IConfigurationSection DatosIniciales()
        {
            var generador = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var configuration = generador.Build();
            return configuration.GetSection(typeof(ltrAppSetting.DatosIniciales).Name);
        }

        public static void PersistirDatosIniciales(ContextoSe contexto)
        {
            contexto.IniciarTraza("Definición de datos iniciales", true);
            try
            {
                var datosIniciales = DatosIniciales();

                UsuarioDtm usuario = new UsuarioDtm();
                usuario.Login = ContextoSe.Login_Admin;
                usuario.Nombre = "Juan";
                usuario.Apellido = "Jiménez-Cervantes";
                usuario.eMail = "jjimenezcf@gmail.com";
                usuario.EsAdministrador = true;
                var gestor = GestorDeUsuarios.Gestor(contexto, contexto.Mapeador);
                gestor.PersistirRegistro(usuario, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                contexto.AsignarUsuario(usuario);
                var gestorDeNecocio = GestorDeNegocios.Gestor(contexto, contexto.Mapeador);
                GestorDeNegocios.CrearNegocioSiNoExiste(gestorDeNecocio, enumNegocio.Variable, "Variables", typeof(VariableDtm), typeof(VariableDto), "cog-solid.svg", true, usaCg: false);
                GestorDeVariables.CrearSiNoExiste(contexto, Variable.CFG_Crear_Registros_De_Entorno, Descripciones.CFG_Crear_Registros_De_Entorno, "S");
                GestorDeVariables.CrearSiNoExiste(contexto, Variable.CFG_UrlBase, Descripciones.CFG_UrlBase, datosIniciales.Value == null ? "https://localhost:44396" : datosIniciales.GetSection(ltrAppSetting.DatosIniciales.UrlBase).Value);

                var almacenDocumental = datosIniciales.Value == null
                ? $@"c:\{ltrAppSetting.DatosIniciales.AlmacenDocumental}"
                : datosIniciales.GetSection(ltrAppSetting.DatosIniciales.AlmacenDocumental).Value;

                Directory.CreateDirectory(almacenDocumental is null ? $@"c:\{ltrAppSetting.DatosIniciales.AlmacenDocumental}" : almacenDocumental);
                GestorDeVariables.CrearSiNoExiste(contexto, Variable.CFG_Servidor_Archivos, Descripciones.CFG_Servidor_Archivos, almacenDocumental);
                GestorDeVariables.CrearSiNoExiste(contexto, Variable.Cola_Ejecutor, Descripciones.Cola_Ejecutor, datosIniciales.Value == null ? ContextoSe.Login_Admin : datosIniciales.GetSection(ltrAppSetting.DatosIniciales.EjecutorDeLaCola).Value);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }


        public static void InicializarVariables(ContextoSe contexto)
        {
            GestorDeVariables.CrearSiNoExiste(contexto, Variable.CFG_Crear_Registros_De_Entorno, Descripciones.CFG_Crear_Registros_De_Entorno, "N");
            GestorDeVariables.CrearSiNoExiste(contexto, Variable.CTA_Cuenta_De_Cliente, Descripciones.CTA_Cuenta_De_Cliente, ltrCuenta.ClienteCodigo);
            GestorDeVariables.CrearSiNoExiste(contexto, Variable.CTA_Cuenta_De_Proveedor, Descripciones.CTA_Cuenta_De_Cliente, ltrCuenta.ProveedorCodigo);
            GestorDeVariables.CrearSiNoExiste(contexto, Variable.CTA_Cuenta_De_Sueldos, Descripciones.CTA_Cuenta_De_Sueldos, ltrCuenta.SueldosCodigo);
            GestorDeVariables.CrearSiNoExiste(contexto, Variable.CTA_Cuenta_De_Consultoria, Descripciones.CTA_Cuenta_De_Consultoria, ltrCuenta.ConsultoriaCodigo);
            enumParametrosDeContratos.CTR_Porcentaje_Aviso.CrearSiNoExiste("80");
            enumParametrosDeContratos.CTR_Porcentaje_Aviso.CrearSiNoExiste("100");
        }

        public static void InicializarParametrosDeNegocio(ContextoSe contexto)
        {
            enumNegocio.Archivador.ResetearParametro(contexto, enumParametrosDeNegocio.CFG_Permite_Subir_Archivos_Desde_El_Movil, "S");
        }
    }
}

