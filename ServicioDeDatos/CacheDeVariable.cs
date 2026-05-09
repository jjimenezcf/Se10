using Dapper;
using Gestor.Errores;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using Utilidades;

namespace ServicioDeDatos
{

    public enum enumVariablesDelSistema
    {
        [Description("Indica si se usa la cache de los ficheros *.json")]
        Cfg_Usar_Cache_Descriptores_Json
    }

    public class Variable
    {
        public static readonly string CFG_UrlBase = nameof(CFG_UrlBase);
        public static readonly string CFG_Version = nameof(CFG_Version);
        public static readonly string CFG_Debugar_Sqls = nameof(CFG_Debugar_Sqls);
        public static readonly string CFG_Resetear_Libros = nameof(CFG_Resetear_Libros);
        public static readonly string CFG_Correo_De_Soporte = nameof(CFG_Correo_De_Soporte);
        public static readonly string Cfg_Eliminar_Correos_Anteriores_A = nameof(Cfg_Eliminar_Correos_Anteriores_A);
        public static readonly string CFG_SeparadorDecimal_En_BD = nameof(CFG_SeparadorDecimal_En_BD);
        public static readonly string CFG_Crear_Registros_De_Entorno = nameof(CFG_Crear_Registros_De_Entorno);
        public static readonly string CFG_Crear_Etapa = nameof(CFG_Crear_Etapa);


        public static readonly string CFG_Servidor_Archivos = nameof(CFG_Servidor_Archivos);
        public static readonly string CFG_Servidor_De_Correo = nameof(CFG_Servidor_De_Correo);

        public static readonly string CFG_Ruta_De_Binarios = nameof(CFG_Ruta_De_Binarios);
        public static readonly string CFG_Ruta_De_Descarga = nameof(CFG_Ruta_De_Descarga);
        public static readonly string CFG_Ruta_Ficheros_De_Debug = ApiDeEnsamblados.CFG_Ruta_Ficheros_De_Debug;

        public static readonly string Cfg_Tipo_De_Archivador_De_BackUp = nameof(Cfg_Tipo_De_Archivador_De_BackUp);
        public static readonly string Cfg_Tipo_De_Archivador_De_Exportacion = nameof(Cfg_Tipo_De_Archivador_De_Exportacion);
        public static readonly string Cfg_Tipo_De_Archivador_Basico = nameof(Cfg_Tipo_De_Archivador_Basico);
        public static readonly string Cfg_Tipo_De_Archivador_Zip = nameof(Cfg_Tipo_De_Archivador_Zip);
        public static readonly string Cfg_Tipo_De_Archivador_Sii = nameof(Cfg_Tipo_De_Archivador_Sii);
        public static readonly string CFG_Tamano_Maximo_Zip = nameof(CFG_Tamano_Maximo_Zip);

        public static readonly string CFG_Usar_Cache_En_EpLeerPorId = nameof(CFG_Usar_Cache_En_EpLeerPorId);
        public static readonly string CFG_Usar_Cache = ApiDeEnsamblados.CFG_Usar_Cache;
        public static readonly string Cfg_Usar_Cache_Descriptores_Json = ApiDeEnsamblados.Cfg_Usar_Cache_Descriptores_Json;

        public static readonly string CFG_CG_De_Documentacion = nameof(CFG_CG_De_Documentacion);
        public static readonly string CFG_Sociedad_Del_Sistema = nameof(CFG_Sociedad_Del_Sistema);
        public static readonly string CFG_CG_De_ClientesWeb = nameof(CFG_CG_De_ClientesWeb);

        public static readonly string Cola_Activa = nameof(Cola_Activa);
        public static readonly string Cola_Tiempo_De_Espera = nameof(Cola_Tiempo_De_Espera);
        public static readonly string Cola_Usa_Gestor_De_Colas = nameof(Cola_Usa_Gestor_De_Colas);

        public static readonly string Cola_Trazar = nameof(Cola_Trazar);
        public static readonly string Cola_Ultima_Ejecucion = nameof(Cola_Ultima_Ejecucion);        
        public static readonly string Cola_Ejecutor = nameof(Cola_Ejecutor);
        public static readonly string Cola_Emisor = nameof(Cola_Emisor);
        public static readonly string Cola_Receptor = nameof(Cola_Receptor);

        public static readonly string CTA_Cuenta_De_Proveedor = nameof(CTA_Cuenta_De_Proveedor);
        public static readonly string CTA_Cuenta_De_Cliente = nameof(CTA_Cuenta_De_Cliente);
        public static readonly string CTA_Cuenta_De_Sueldos = nameof(CTA_Cuenta_De_Sueldos);
        public static readonly string CTA_Cuenta_De_Consultoria = nameof(CTA_Cuenta_De_Consultoria);

        public static readonly string IVA_General = nameof(IVA_General);
        public static readonly string IVA_Reducido = nameof(IVA_Reducido);
        public static readonly string IVA_Super_Reducido = nameof(IVA_Super_Reducido);
        public static readonly string IVA_Exportacion = nameof(IVA_Exportacion);
        public static readonly string IVA_NoSujeto = nameof(IVA_NoSujeto);


        public static readonly string UND_Hora = nameof(UND_Hora);

        public static readonly string PTR_Horas_De_Jornada = nameof(PTR_Horas_De_Jornada);

        public static readonly string LYT_Opacidad = nameof(LYT_Opacidad);
        public static readonly string LYT_Imagen_De_Fondo = nameof(LYT_Imagen_De_Fondo);

        public static readonly string IA_Usada = IIa.IA_Usada;
        public static readonly string IA_ApiKey = IIa.IA_ApiKey;
        public static readonly string IA_Modelo = nameof(IA_Modelo);
        public static readonly string IA_Resetera_Filtros = nameof(IA_Resetera_Filtros);
        
    }

    public class Descripciones
    {
        public static readonly string CFG_Crear_Registros_De_Entorno = "Indica si al iniciar el sitio web se han de crear registros en el entorno";
        public static readonly string CFG_Crear_Etapa = "Indica si se ha de crear una etapa con valor 0 si no está definida";

        public static readonly string CFG_Debugar_Sqls = "Indica si hay que debugar";
        public static readonly string CFG_Resetear_Libros = "Indica si hay que resetear los libros de registro al leer el índice que le toca";
        public static readonly string Cfg_Usar_Cache_Descriptores_Json = enumVariablesDelSistema.Cfg_Usar_Cache_Descriptores_Json.Descripcion();

        public static readonly string CFG_UrlBase = "Indica el sitio WEB donde está ubicado";
        public static readonly string CFG_Servidor_Archivos = "Define la ruta donde se localiza el servidor de archivos";
        public static readonly string Cola_Ejecutor = "Indica el login con el que se ejecuta la cola de trabajos sometidos";
        public static readonly string Cola_Ultima_Ejecucion = "Indica la fecha de la última ejecución de la cola";
        public static readonly string Cola_Tiempo_De_Espera = "Indica el tiempo (en minutos) que se duerme el proceso antes de volverse a ejecutar";
        public static readonly string CFG_Movil_Negocio_Con_Documentacion = "Json con la lista de negocios a los que se puede asociar documentación desde el móvil";
        public static readonly string Cola_Usa_Gestor_De_Colas = "Indica si se usa el gestor de colas de windows, si es que si, es responsabilidad de windows su ejecución";
        

        public static readonly string CFG_SeparadorDecimal_En_BD = "Indica el separador decimal usado por la BD";

        public static readonly string CTA_Cuenta_De_Proveedor = $"Indica el nº de cuenta: {ltrCuenta.ProveedorDescripcion}";
        public static readonly string CTA_Cuenta_De_Cliente = $"Indica el nº de cuenta: {ltrCuenta.ClienteDescripcion}";
        public static readonly string CTA_Cuenta_De_Sueldos = $"Indica el nº de cuenta: {ltrCuenta.SueldosDescripcion}";
        public static readonly string CTA_Cuenta_De_Consultoria = $"Indica el nº de cuenta: {ltrCuenta.ServiciosProfesionales}";

        public static readonly string IVA_General = "Indica el código del iva repercutido general";
        public static readonly string IVA_Reducido = "Indica el código del iva repercutido reducido";
        public static readonly string IVA_Super_Reducido = "Indica el código del iva repercutido super reducido";
        public static readonly string IVA_Exportacion = "Indica el código del iva repercutido usado para exportaciones";
        public static readonly string IVA_NoSujeto = "Indica el código del iva repercutido no sujeto a tributación";

        public static readonly string UND_Hora = "Indica la sigla de la unidad de media hora";

        public static readonly string PTR_Horas_De_Jornada = $"Indica el nº de horas de una joranada laboral por defecto, para automatizar el cálculo del tiempo de ejecución de un parte asignado";

        public static readonly string LYT_Opacidad = "Indica la opcidad de la imagen del fondo del layout";
        public static readonly string LYT_Imagen_De_Fondo = "Indica la la imagen del fondo del layout, dicha imagen debe existir en la ruta del sitio web ../images/";
        
    }

    public static class CacheDeVariable
    {
        private const string _PostfijoDeRutaDeDebug = "Trazas";

        private static readonly string RutaDeSii = $@"{Cfg_ServidorDeArchivos}\Sii";

        private static ConcurrentDictionary<string, object> _cache = null;
        public static ConcurrentDictionary<string, object> Cache
        {
            get
            {
                if (_cache == null)
                    _cache = ServicioDeCaches.Obtener(typeof(VariableDtm).FullName);
                return _cache;
            }
        }

        public static string Cfg_ServidorDeArchivos => ObtenerVariable(Variable.CFG_Servidor_Archivos, "Define la ruta donde se localiza el servidor de archivos", @"C:\AlmacenDocumental\");

        public static string Cfg_ServidorDeCorreo => ObtenerVariable(Variable.CFG_Servidor_De_Correo, "Define el nombre de la sección del fichero appsetting.json donde se encuentran las características del servidor de correo a utilizar", "CorreoPorDefecto");

        public static string Cfg_CorreoDeSoporte => ObtenerVariable(Variable.CFG_Correo_De_Soporte, "Define las direcciones de correo electrónico receptoras de mensajes del sisitema a soporte", "jjimenezcf@gmail.com");

        public static int Cfg_Eliminar_Correos_Anteriores_A => ObtenerVariable(Variable.Cfg_Eliminar_Correos_Anteriores_A, "Fecha desde la que se eliminarán correos a partir del día de hoy de la bandeja de Enviados de SMTP", "10").Entero();
              

        public static string CFG_SeparadorDecimal_En_BD => ObtenerVariable(Variable.CFG_SeparadorDecimal_En_BD, Descripciones.CFG_SeparadorDecimal_En_BD, ".");

        public static bool Cfg_HayQueDebuggar => ObtenerVariable(Variable.CFG_Debugar_Sqls, Descripciones.CFG_Debugar_Sqls, "S") == "S";

        public static bool Cfg_Usar_Cache_Json => ObtenerVariable(Variable.Cfg_Usar_Cache_Descriptores_Json, Descripciones.Cfg_Usar_Cache_Descriptores_Json, "S") == "S";

        public static bool Cfg_HayResetearLibros => ObtenerVariable(Variable.CFG_Resetear_Libros, Descripciones.CFG_Resetear_Libros, "S") == "S";

        public static string Cfg_Version => ObtenerVariable(Variable.CFG_Version, "Versión del sistema de elementos", "0.1");

        public static string Cfg_UrlBase => ObtenerVariable(Variable.CFG_UrlBase, "Indica el sitio WEB donde está ubicado", "https://localhost:44396/");

        public static string Uri_Compartir =>  "/images/compartir.png";
        public static string Uri_ConsultarConGuid => "/images/consultarConGuid.svg";
        public static string Uri_EnviarCorreo => "/images/enviarCorreo.png";
        public static string Uri_SubirDelPortaPapeles => "/images/menu/subirPortaPapeles.png";
        

        public static string Cfg_RutaDeDescarga
        {
            get
            {
                var valor = ResetearVariable(Variable.CFG_Ruta_De_Descarga, "Indica la dirección absoluta de donde se dejan los ficheros temporales que se descargan", $@"{Path.Combine(Cfg_ServidorDeArchivos, "descargas")}");
                if (!Directory.Exists(valor))
                    Directory.CreateDirectory(valor);
                return valor;
            }
        }

        public static bool Cfg_CrearRegistrosDeEntorno => ObtenerVariable(Variable.CFG_Crear_Registros_De_Entorno, Descripciones.CFG_Crear_Registros_De_Entorno, "N") == "S";

        public static bool Cfg_CrearEtapa => ObtenerVariable(Variable.CFG_Crear_Etapa, Descripciones.CFG_Crear_Etapa, "N") == "S";

        public static string Cola_Receptor => ObtenerVariable(Variable.Cola_Receptor, "Indica el mail del receptor de mensajes de soporte de la cola de trabajos sometidos", "jjimenezcf@gmail.com");

        public static string Cola_Emisor => ObtenerVariable(Variable.Cola_Emisor, "Indica el mail del emisor de mensajes de la cola de trabajos sometidos", "back.ground.cola@gmail.com");

        public static string Cola_LoginDeEjecutor => ObtenerVariable(Variable.Cola_Ejecutor, "Indica el login con el que se ejecuta la cola de trabajos sometidos", "raul.miras");

        public static bool Cola_Trazar => ObtenerVariable(Variable.Cola_Trazar, "Indica si se trazan las consultas SQL del contexto de la cola", "S") == "S";
        public static DateTime Cola_Ultima_Ejecucion
        {
            get
            {
                string fechaString = ObtenerVariable(Variable.Cola_Ultima_Ejecucion, Descripciones.Cola_Ultima_Ejecucion, DateTime.MinValue.ToString());
                if (DateTime.TryParse(fechaString, out DateTime resultado))
                {
                    return resultado;
                }
                return DateTime.MinValue;
            }
        }

        public static bool Cola_Activa => ObtenerVariable(Variable.Cola_Activa, "Indica si la cola de trabajos sometidos está activa", "S") == "S";
        public static int Cola_Tiempo_De_Espera => ObtenerVariable(Variable.Cola_Tiempo_De_Espera, Descripciones.Cola_Tiempo_De_Espera, "5").Entero();

        public static bool Cola_Usa_Gestor_De_Colas => ObtenerVariable(Variable.Cola_Usa_Gestor_De_Colas, Descripciones.Cola_Usa_Gestor_De_Colas, "S") == "S";

        public static string Cfg_Tipo_De_Archivador_De_BackUp => ObtenerVariable(Variable.Cfg_Tipo_De_Archivador_De_BackUp, "Indica el nombre del tipo de archivador usado para copia de seguridad", ltrTipoArchivador.TipoBackUp);

        public static string Cfg_Tipo_De_Archivador_De_Exportacion => ObtenerVariable(Variable.Cfg_Tipo_De_Archivador_De_Exportacion, "Indica el nombre del tipo de archivador usado para exportaciones", ltrTipoArchivador.TipoExportacion);

        public static string Cfg_Tipo_De_Archivador_Basico => ObtenerVariable(Variable.Cfg_Tipo_De_Archivador_Basico, "Indica el nombre del tipo de archivador básico", ltrTipoArchivador.TipoGeneral);

        public static string Cfg_Tipo_De_Archivador_Zip => ObtenerVariable(Variable.Cfg_Tipo_De_Archivador_Zip, "Indica el nombre del tipo de archivador para almacenar un zip", ltrTipoArchivador.TipoZip);

        public static string Cfg_Tipo_De_Archivador_Sii => ObtenerVariable(Variable.Cfg_Tipo_De_Archivador_Sii, "Indica el nombre del tipo de archivador para almacenar la auditoría de los ficheros del Sii", ltrTipoArchivador.TipoSii);

        public static string Cfg_CG_De_ClientesWeb => ObtenerVariable(Variable.CFG_CG_De_ClientesWeb, "Indica el nombre del CG de una sociedad donde se dan de alta los puestos de trabajo de clientes", ltrDeSociedad.CentroGestorDeClientesWeb);

        public static string Cfg_CG_De_Documentacion => ObtenerVariable(Variable.CFG_CG_De_Documentacion, "Indica el nombre del CG donde se almacenan documentos generados por el sistema", ltrDeSociedad.CentroGestorDeDocumentacion);

        public static string Cfg_Sociedad_Del_Sistema => ObtenerVariable(Variable.CFG_Sociedad_Del_Sistema, "Indica el CIF de la sociedad por defecto parael el sistema, en ella se asociaran los CG del sistema", ltrDeSociedad.SociedadNula);

        public static long Cfg_Tamano_Maximo_Zip
        {
            get
            {
                var valor = ObtenerVariable(Variable.CFG_Tamano_Maximo_Zip, "Indica el tamaño máximo para crear un fichero ZIP", "104857600");
                long size;
                if (!long.TryParse(valor, out size))
                {
                    GestorDeErrores.Emitir($"la variable '{Variable.CFG_Tamano_Maximo_Zip}' está mal definida, el valor '{valor}' no es valido");
                }
                return size;
            }
        }



        public static string CFG_Usar_Cache_En_EpLeerPorId => ObtenerVariable(Variable.CFG_Usar_Cache_En_EpLeerPorId, "Indica si ha de usar la cache en el EndPoint LeerPorId", "N");

        public static bool CFG_Usar_Cache => ObtenerVariable(Variable.CFG_Usar_Cache, "Indica si ha de usar la cache del sistema", "S") == "S";
        public static string CFG_Ruta_Ficheros_De_Debug => ObtenerVariable(Variable.CFG_Ruta_Ficheros_De_Debug, $"Indica la ruta donde almacenar los ficheros de trazas si la variable {Variable.CFG_Debugar_Sqls} está activo ", $@"C:\Temp\{_PostfijoDeRutaDeDebug}");

        public static string CFG_Ruta_Raiz_De_Excepciones => CFG_Ruta_Ficheros_De_Debug.Replace(_PostfijoDeRutaDeDebug, "Excepciones");
        public static string CFG_Ruta_Ficheros_De_Excepciones => ComplementarRutaConFecha(CFG_Ruta_Raiz_De_Excepciones);
        public static string CFG_Ruta_Ficheros_De_Zip => CFG_Ruta_Ficheros_De_Debug.Replace(_PostfijoDeRutaDeDebug, "Zip");
        public static string CFG_Ruta_Ficheros_A_Firmar => CFG_Ruta_Ficheros_De_Debug.Replace(_PostfijoDeRutaDeDebug, "Firmar");

        
        public static string CFG_Ruta_Ficheros_De_Sii => RutaDeSii;

        public static string CFG_Ruta_Ficheros_De_Certificados => CFG_Ruta_Ficheros_De_Debug.Replace(_PostfijoDeRutaDeDebug, "Certificados");

        public static string CFG_Ruta_Fichero_De_Sii_Inbox => Path.Combine(CFG_Ruta_Ficheros_De_Sii, $"Inbox{Path.DirectorySeparatorChar.ToString()}");
        public static string CFG_Ruta_Fichero_De_Sii_BackUp => Path.Combine(CFG_Ruta_Ficheros_De_Sii, $"BackUp{Path.DirectorySeparatorChar.ToString()}");
        public static string CFG_Ruta_Fichero_De_Sii_Outbox => Path.Combine(CFG_Ruta_Ficheros_De_Sii, $"Outbox{Path.DirectorySeparatorChar.ToString()}");
        public static string CFG_Ruta_Fichero_De_Sii_Blockchains => Path.Combine(CFG_Ruta_Ficheros_De_Sii, $"Blockchains{Path.DirectorySeparatorChar.ToString()}");
        public static string CFG_Ruta_Fichero_De_Sii_Invoices => Path.Combine(CFG_Ruta_Ficheros_De_Sii, $"Invoices{Path.DirectorySeparatorChar.ToString()}");
        public static string CFG_Ruta_Fichero_De_Sii_Log => Path.Combine(CFG_Ruta_Ficheros_De_Sii, $"Log{Path.DirectorySeparatorChar.ToString()}");

        public static string LYT_Imagen_De_Fondo => $"../images/{ObtenerVariable(Variable.LYT_Imagen_De_Fondo, Descripciones.LYT_Imagen_De_Fondo, @"logoSe_3.svg")}" ;
        public static double LYT_Opacidad
        {
            get
            {
                var valor = ObtenerVariable(Variable.LYT_Opacidad, Descripciones.LYT_Opacidad, "0.1");
                double opacidad = 0.1; 

                if (double.TryParse(valor.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double resultado))
                    opacidad = resultado;

                return opacidad;
            }
        }

        public static string IA_Usada => ObtenerVariable(Variable.IA_Usada, "Indica el nombre de la IA que se usa", nameof(IaOpenAI));
        public static string IA_ApiKey => ObtenerVariable(Variable.IA_ApiKey, "Indica la Api_Key contratada", "LA_API_KEY");
        public static string IA_Modelo => ObtenerVariable(Variable.IA_Modelo, "Inidica el modelo a usar por la IA", "sonar");
        public static bool IA_Resetera_Filtros => ObtenerVariable(Variable.IA_Resetera_Filtros, 
            $"Inidica si el filtro que se usa es el definido por programa o el indicado en el parame negocio '{enumParametrosDeNegocio.IA_Prompt_Filtro}'", 
            true.ToString()).EsTrue();

        


        public static string ResetearVariable(string variable, string descripcion, string valor)
        {
            if (!Cache.ContainsKey(variable))
            {
                Cache[variable] = LeerCrear(variable, descripcion, valor);
            }

            if (Cache[variable].ToString() != valor)
            {
                Modificar(variable, valor);
                Cache[variable] = valor;
            }

            return Cache[variable].ToString();
        }

        public static string ObtenerVariable(string variable, string descripcion, string valor)
        {
            if (!Cache.ContainsKey(variable))
                Cache[variable] = LeerCrear(variable, descripcion, valor);
            return Cache[variable].ToString();
        }

        private static string LeerCrear(string variable, string descripcion, string valorInicial)
        {
            var valor = LeerValorDeVariable(variable, emitirError: false);
            if (valor == Literal.VariableNoDefinida)
            {
                valor = CrearVariable(variable, descripcion, valorInicial);
            }
            return valor;
        }

        private static string ComplementarRutaConFecha(string ruta)
        {
            var fecha = DateTime.Now;
            ruta = Path.Combine(ruta, fecha.Year.ToString(), fecha.Month.ToString(), fecha.Day.ToString());
            if (!Directory.Exists(ruta))
            {
                Directory.CreateDirectory(ruta);
            }

            return ruta;
        }

        private static string ComplementarRutaConCodigoDeEmpresa(string ruta,string nif)
        {
            ruta = Path.Combine(ruta, nif, DateTime.Now.Year.ToString());
            if (!Directory.Exists(ruta))
            {
                Directory.CreateDirectory(ruta);
            }

            return ruta;
        }

        public static string LeerValorDeVariable(string variable, bool emitirError = true, bool usarLaCache = true)
        {
            ConcurrentDictionary<string, object> cache = null;

            if (usarLaCache)
            {
                cache = ServicioDeCaches.Obtener(typeof(VariableDtm).FullName);
                if (cache.ContainsKey(variable))
                    return cache[variable].ToString();
            }

            var consulta = new ConsultaSql<VariableDtm>(VariableSqls.LeerValorDeVariable);
            var valores = new Dictionary<string, object> { { $"@{nameof(variable)}", variable } };
            var resultado = consulta.LanzarConsulta(new DynamicParameters(valores));


            if (resultado.Count == 0)
            {
                if (emitirError)
                    GestorDeErrores.Emitir($"No se localiza la variable {variable}");
                else
                    return Literal.VariableNoDefinida;
            }

            if (resultado.Count > 1)
                GestorDeErrores.Emitir($"Hay más de un registros para la {variable}");

            if (resultado.Count == 1 && cache is not null)
                cache[variable] = resultado[0].Valor;

            return resultado[0].Valor;
        }


        public static void Persistir<T>(this T variable, string valor)
        where T : struct, System.Enum
        {
            var valorEnBd = LeerValorDeVariable(variable.ToString(), emitirError: false);
            if (valorEnBd == Literal.VariableNoDefinida)
            {
                CrearVariable(variable.ToString(), variable.Descripcion(), valor);
            }
            else
            {
                Modificar(variable.ToString(), valor);
            }
        }


        public static void Persistir<T>(this T variable, ContextoSe contexto, string valor)
        where T : struct, System.Enum
        {
            var valorEnBd = LeerValorDeVariable(variable.ToString(), emitirError: false);
            if (valorEnBd == Literal.VariableNoDefinida)
            {
                CrearVariable(contexto, variable.ToString(), variable.Descripcion(), valor);
            }
            else
            {
                if (!valorEnBd.Equals(valor)) Modificar(contexto, variable.ToString(), valor);
            }
        }

        public static void CrearSiNoExiste<T>(this T variable, string valorSiNoExiste = Literal.Cero)
        where T : struct, System.Enum
        => LeerVariable<T>(variable, valorSiNoExiste);

        public static string LeerVariable<T>(this T variable, string valorSiNoExiste = Literal.Cero)
        where T : struct, System.Enum
        {
            var valor = LeerValorDeVariable(variable.ToString(), false);
            if (valor == Literal.VariableNoDefinida)
            {
                return CrearVariable(variable.ToString(), variable.Descripcion(), valorSiNoExiste);
            }
            return valor;
        }

        public static string CrearVariable(string variable, string descripcion, string valor)
        {
            var debuggar = variable.Equals(nameof(Variable.CFG_Debugar_Sqls)) || variable.Equals(nameof(Variable.CFG_Ruta_Ficheros_De_Debug)) ? false : Cfg_HayQueDebuggar;
            var sentencia = new ConsultaSql<VariableDtm>(VariableSqls.CrearVariable, debuggar, variable);
            var valores = new Dictionary<string, object> { { $"@{nameof(variable)}", variable }, { $"@{nameof(descripcion)}", descripcion }, { $"@{nameof(valor)}", valor } };
            sentencia.EjecutarSentencia(new DynamicParameters(valores));
            BorrarCache(variable);
            return valor;
        }

        public static string CrearVariable(ContextoSe contexto, string variable, string descripcion, string valor)
        {
            var sentencia = new ConsultaSql<VariableDtm>(contexto, VariableSqls.CrearVariable);
            var valores = new Dictionary<string, object> { { $"@{nameof(variable)}", variable }, { $"@{nameof(descripcion)}", descripcion }, { $"@{nameof(valor)}", valor } };
            sentencia.EjecutarSentencia(new DynamicParameters(valores));
            BorrarCache(variable);
            return valor;
        }

        public static void BorrarVariable(ContextoSe contexto, Enum variable)
        {
            BorrarVariable(contexto, variable.ToString());
        }

        public static void BorrarVariable(ContextoSe contexto, string variable)
        {
            var sentencia = new ConsultaSql<VariableDtm>(contexto, VariableSqls.BorrarVariable);
            var valores = new Dictionary<string, object> { { $"@{nameof(variable)}", variable } };
            sentencia.EjecutarSentencia(new DynamicParameters(valores));
            BorrarCache(variable);
        }

        public static void Modificar(string variable, string valor)
        {
            var sentencia = new ConsultaSql<VariableDtm>(VariableSqls.ModificarVariable, Cfg_HayQueDebuggar, $"{variable}");
            var valores = new Dictionary<string, object> { { $"@{nameof(valor)}", valor }, { $"@{nameof(variable)}", variable } };
            sentencia.EjecutarSentencia(new DynamicParameters(valores));
            BorrarCache(variable);
        }

        public static void Modificar(ContextoSe contexto, string variable, string valor)
        {
            var sentencia = new ConsultaSql<VariableDtm>(contexto, VariableSqls.ModificarVariable);
            var valores = new Dictionary<string, object> { { $"@{nameof(valor)}", valor }, { $"@{nameof(variable)}", variable } };
            sentencia.EjecutarSentencia(new DynamicParameters(valores));
            BorrarCache(variable);
        }

        public static void BorrarCache(string variable)
        {
            ServicioDeCaches.EliminarElemento(typeof(VariableDtm).FullName, variable);
        }
    }
}
