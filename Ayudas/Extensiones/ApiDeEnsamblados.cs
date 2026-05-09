using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Utilidades
{
    public static class ApiDeEnsamblados
    {

        public static string AntesDeVincular => nameof(AntesDeVincular);
        public static string DespuesDeVincular => nameof(DespuesDeVincular);
        public static string AntesDeQuitarVinculo => nameof(AntesDeQuitarVinculo);
        public static string DespuesDeQuitarVinculo => nameof(DespuesDeQuitarVinculo);
        public static string EstaEnEtapaDeConsulta => nameof(EstaEnEtapaDeConsulta);

        public static string CFG_Usar_Cache = nameof(CFG_Usar_Cache);
        public static string CFG_Ruta_Ficheros_De_Debug = nameof(CFG_Ruta_Ficheros_De_Debug);
        public static string Cfg_Usar_Cache_Descriptores_Json = nameof(Cfg_Usar_Cache_Descriptores_Json);


        public static readonly string DllDelSistemaDeElementos = "SistemaDeElementos.dll";
        public static readonly string DllDelGestorDeNegocio = "GestoresDeNegocio.dll";
        public static readonly string DllDelServicioDeDatos = "ServicioDeDatos.dll";
        public static readonly string DllDelModeloDeDto = "ModeloDeDto.dll";

        public static string GestoresDeNegocio => DllDelGestorDeNegocio.Replace(".dll", "");
        private static string SistemaDocumental => nameof(SistemaDocumental);
        private static string GestorDeArchivos => nameof(GestorDeArchivos);

        private static string NameSpaceDescriptores => "MVCSistemaDeElementos.Descriptores";


        public static readonly string ClaseDelServidorDocumental = $"{DllDelGestorDeNegocio.Replace(".dll", "")}.{SistemaDocumental}.ServidorDocumental";
        public static readonly string ClaseDelPemisosDelElemento = $"{DllDelGestorDeNegocio.Replace(".dll", "")}.Seguridad.GestorDePemisosDelElemento";
        public static readonly string ClaseCacheDeVariable = $"{DllDelServicioDeDatos.Replace(".dll", "")}.CacheDeVariable";
        public static readonly string ClaseVariableDtm = $"{DllDelServicioDeDatos.Replace(".dll", "")}.Entorno.VariableDtm";

        public static readonly string MetodoDeAnexarArchivo = "AnexarArchivo";
        public static readonly string MetodoDeCopiarArchivo = "CopiarArchivo";
        public static readonly string MetodoDeCrearPermisosDelElemento = "CrearPermisosDelElemento";

        public static readonly string MetodoDeEstimacionDirectaEnNcs = "EstimacionDirectaEnNcs";

        public static readonly string ClaseBase_GestorDeRelaciones = "GestorDeRelaciones";
        public static readonly string ClaseBase_GestorDeTipos = "GestorDeTiposDeElemento";
        public static readonly string ClaseBase_GestorDePlantillasPorTipo = "GestorDePlantillasPorTipo";
        public static readonly string ClaseBase_GestorDeClasesDelTipo = "GestorDeClasesDelTipo";
        public static readonly string ClaseBase_GestorDeEstados = "GestorDeEstados";
        public static readonly string ClaseBase_GestorDeTransiciones = "GestorDeTransiciones";

        public static readonly string Clase_GestorDeArchivos = $"{GestoresDeNegocio}.{SistemaDocumental}.{GestorDeArchivos}";

        public static readonly string DescriptorDeConsultaDeTareas = NameSpaceDescriptores + "." + enumVistasAdministrativo.DescriptorDeConsultaDeTarea;
        public static readonly string DescriptorDeConsultaDeInfantes = NameSpaceDescriptores + "." + enumVistasGuarderias.DescriptorDeConsultaDeInfante;
        public static readonly string DescriptorDeConsultaDeCad = NameSpaceDescriptores + "." + enumVistasSistemaDocumental.DescriptorDeConsultaDeCad;

        public static string RutaDeBinarios()
        {
            var path = AppContext.BaseDirectory;
            return Path.GetDirectoryName(path);
        }

        public static Assembly ObtenerDll(string dll)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == dll.Replace(".dll", ""));
            return assembly == null ? Assembly.LoadFile(Path.Combine(RutaDeBinarios(), dll)) : assembly;
        }

        public static Type ObtenerType(string dll, string nombreCompletoDeClase, bool emitirError = true)
        {
            var cache = ServicioDeCaches.Obtener(nameof(ObtenerType));
            var indice = $"{dll}-{nombreCompletoDeClase}";
            if (cache.ContainsKey(indice))
                return (Type)cache[indice];

            var assembly = ObtenerDll(dll);

            var tipo = assembly.GetType(nombreCompletoDeClase);
            if (tipo == null)
            {
                var tipos = assembly.GetTypes().Where(x => x.Name == nombreCompletoDeClase).ToList();
                if (tipos.Count == 1)
                    tipo = tipos[0];
            }
            if (tipo == null && emitirError)
                throw new Exception($"No se encuentra el tipo '{nombreCompletoDeClase}' dentro de '{dll}'");

            cache[indice] = tipo;
            return tipo;
        }

        public static MethodInfo ObtenerMetodoEstatico(string dll, string clase, string metodo)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Fija_Metodos);
            var indice = $"{dll}.{clase}.{metodo}";
            if (!cache.ContainsKey(indice))
            {
                //var rutaBinarios = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
                cache[indice] = ValidarMetodoEstatico(dll, clase, metodo);
            }
            return (MethodInfo)cache[indice];
        }

        private static MethodInfo ValidarMetodoEstatico(string dll, string nombreCompletoDeClase, string nombreMetodo)
        {
            var tipo = ObtenerType(dll, nombreCompletoDeClase);
            MethodInfo[] metodos = tipo.GetMethods(BindingFlags.Public | BindingFlags.Static);
            var metodo = metodos.FirstOrDefault(m => m.Name.Equals(nombreMetodo, StringComparison.InvariantCultureIgnoreCase));
            //foreach (var metodo in metodos)
            //    if (metodo.Name.ToLower() == nombreMetodo.ToLower())
            //        return metodo;

            if (metodo == null)
                throw new Exception($"Hay que implementar el método estático {nombreMetodo} en la clase {nombreCompletoDeClase}");

            return metodo;
        }

        public static MethodInfo MetodoEstatico(string dll, string nombreCompletoDeClase, string nombreMetodo)
        {
            var tipo = ObtenerType(dll, nombreCompletoDeClase);
            if (tipo == null)
                return null;

            MethodInfo[] metodos = tipo.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var metodo in metodos)
                if (metodo.Name.ToLower() == nombreMetodo.ToLower())
                    return metodo;

            return null;
        }


        public static Dictionary<string, object> EjecutarAccionDeEntorno(object entorno, string dll, string clase, string metodo)
        {
            var metodoInvocable = ObtenerMetodoEstatico(dll, clase, metodo);
            try
            {
                return (Dictionary<string, object>)metodoInvocable.Invoke(null, new object[] { entorno });
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;
                throw;
            }
        }

        public static object EjecutarMetodoEstatico(string dll, string clase, string metodo, object[] parametros)
        {
            var metodoInvocable = ObtenerMetodoEstatico(dll, clase, metodo);
            try
            {
                return metodoInvocable.Invoke(null, parametros);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;
                throw;
            }
        }

        public static PropertyInfo ObtenerPropiedad(string dll, string nombreCompletoDeClase, string nombrePropiedad)
        {
            var tipo = ObtenerType(dll, nombreCompletoDeClase);
            PropertyInfo[] propiedades = tipo.GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var propiedad in propiedades)
                if (propiedad.Name.ToLower() == nombrePropiedad.ToLower())
                    return propiedad;
            throw new Exception($"Hay que implementar la propiedad {nombrePropiedad} en la clase {nombreCompletoDeClase} antes de usarla");
        }

        public static IEnumerable<PropertyInfo> PropiedadesDelObjeto(this object objeto) => objeto.GetType().PropiedadesDelTipo();

        public static IEnumerable<PropertyInfo> PropiedadesDelTipo(this Type tipo)
        {
            var indice = tipo.FullName;
            var cache = ServicioDeCaches.Obtener(CacheDe.PropiedadesDelTipo);
            if (!cache.ContainsKey(indice))
            {
                cache[indice] = tipo.GetProperties(); //tipo.GetRuntimeProperties();
            }
            PropertyInfo[] propiedades = (PropertyInfo[])cache[indice];
            return propiedades;
        }

        public static T Propiedad<T>(this object objeto, Type tipo)
        {
            //var propiedades1 = objeto.GetType().GetProperties();//  
            //var propiedades2 = objeto.GetType().GetRuntimeProperties();
            var propiedades = objeto.PropiedadesDelObjeto();
            foreach (var p in propiedades)
            {
                if (p.PropertyType.FullName == tipo.FullName)
                    return (T)p.GetValue(objeto);
            }

            throw new Exception($"El objeto pasado no contiene una propiedad del tipo {tipo.Name}");
        }

        public static object LeerPropiedad(this object objeto, string propiedad)
        {
            var propiedadInfo = objeto.GetType().GetProperty(propiedad);
            if (propiedadInfo is null)
                throw new Exception($"La propiedad '{propiedad}' no existe en el objeto de la clase '{objeto.GetType().Name}'");

            return propiedadInfo.GetValue(objeto);
        }

        public static T? Valor<T>(this object objeto, Func<PropertyInfo, bool> predicado)
        {
            var propiedades = objeto.PropiedadesDelObjeto().Where(predicado).ToList();
            int indice = 0;
            if (propiedades.Count() == 0)
                throw new Exception($"El objeto de tipo {objeto.GetType()} no contiene la propiedad de tipo {typeof(T)}");
            if (propiedades.Count() > 1)
            {
                int conValor = 0;
                int numeroPropiedad = 0;
                foreach (var propiedad in propiedades)
                {
                    if (propiedad.GetValue(objeto) != null)
                    {
                        indice = numeroPropiedad;
                        conValor++;
                    }
                    if (conValor > 1)
                        throw new Exception($"El objeto de tipo {objeto.GetType()} contiene la propiedad de tipo {typeof(T)} más de una vez para el predicado");
                    numeroPropiedad++;
                }
            }
            var valor = propiedades[indice].GetValue(objeto);
            return valor is null ? default : (T)valor;
        }

        public static object Valor(this object objeto, Func<PropertyInfo, bool> predicado, Type tipo)
        {
            var propiedades = objeto.PropiedadesDelObjeto().Where(predicado).ToList();
            int indice = 0;
            if (propiedades.Count() == 0)
                throw new Exception($"El objeto de tipo {objeto.GetType()} no contiene la propiedad de tipo {tipo.FullName}");
            if (propiedades.Count() > 1)
            {
                int conValor = 0;
                int numeroPropiedad = 0;
                foreach (var propiedad in propiedades)
                {
                    if (propiedad.GetValue(objeto) != null)
                    {
                        indice = numeroPropiedad;
                        conValor++;
                    }
                    if (conValor > 1)
                        throw new Exception($"El objeto de tipo {objeto.GetType()} contiene la propiedad de tipo {tipo.FullName} más de una vez para el predicado");
                    numeroPropiedad++;
                }
            }
            var valor = propiedades[indice].GetValue(objeto);
            return valor;
        }

        public static void EscribirPropiedad(this object objeto, string propiedad, object valor, bool errorSiNoHay = false)
        {
            var propiedades = objeto.PropiedadesDelObjeto();
            bool encontrada = true;
            foreach (var p in propiedades)
            {
                if (p.Name == propiedad)
                {
                    encontrada = true;
                    p.SetValue(objeto, valor);
                }
            }
            if (!encontrada && errorSiNoHay)
                throw new Exception($"La propiedad '{propiedad}' no se encuentra en el objeto de la clase '{typeof(object).Name}'");
        }

        private static void ValidarPropiedadNoNula(this object objeto, string nombre)
        {
            var propiedades = objeto.PropiedadesDelObjeto();
            foreach (var propiedad in propiedades)
            {
                if (propiedad.Name.ToLower().Equals(nombre.ToLower()))
                {
                    if (((string)propiedad.GetValue(objeto)).IsNullOrEmpty())
                        throw new Exception($"la propiedad {nombre} del objeto {objeto.GetType().Name} es obligatorio");
                    break;
                }
            }
        }


        public static void ValidarQueSonIguales<T>(this T objeto_1, T objeto_2, List<string> excepto = null, List<string> solo = null, string mensaje = null)
        {
            var resultado = Iguales(objeto_1, objeto_2, excepto, solo);
            if (!resultado.sonIguales)
            {
                if (!mensaje.IsNullOrEmpty())
                {
                    throw new Exception(mensaje);
                }
                else
                {
                    throw new Exception($"la propiedad '{resultado.propiedadDiferente}' debe ser igual a la de BD");
                }
            }
        }


        public static (bool sonIguales, string propiedadDiferente) Iguales<T>(this T objeto_1, T objeto_2, List<string> excepto = null, List<string> solo = null)
        {
            var propiedades = objeto_1.PropiedadesDelObjeto();
            foreach (var propiedad in propiedades)
            {
                if (excepto is not null && excepto.Contains(propiedad.Name))
                    continue;

                if (solo is not null && !solo.Contains(propiedad.Name))
                    continue;

                if (propiedad.GetSetMethod() == null)
                    continue;

                if (propiedad.GetGetMethod() == null)
                    continue;

                if (propiedad.PropertyType == typeof(int) || propiedad.PropertyType == typeof(int?) ||
                    propiedad.PropertyType == typeof(string) ||
                    propiedad.PropertyType == typeof(bool) || propiedad.PropertyType == typeof(bool?) ||
                    propiedad.PropertyType == typeof(DateTime) || propiedad.PropertyType == typeof(DateTime?) ||
                    propiedad.PropertyType.BaseType == typeof(Enum))
                {
                    var v1 = propiedad.GetValue(objeto_1);
                    var v2 = propiedad.GetValue(objeto_2);
                    if (v1 is null && v2 is not null) return (false, propiedad.Name);
                    if (v1 is not null && v2 is null) return (false, propiedad.Name);
                    if (v1 is null && v2 is null) continue;
                    if (!v1.Equals(v2))
                        return (false, propiedad.Name);
                }
            }
            return (true, null);
        }

        public static void CopiarEn(this object origen, object destino)
        {
            var propiedades = origen.PropiedadesDelObjeto();
            foreach (var propiedad in propiedades)
            {
                if (propiedad.GetSetMethod() == null)
                    continue;

                if (propiedad.GetGetMethod() == null)
                    continue;

                if (Nullable.GetUnderlyingType(propiedad.PropertyType) == null && !ApiDeEnsamblados.ImplementaInterface(propiedad.PropertyType, typeof(IComparable).FullName))
                    continue;

                if (propiedad.PropertyType.IsEnum)
                {
                    var enumerado = Enum.Parse(propiedad.PropertyType, origen.GetType().GetProperty(propiedad.Name).GetValue(origen).ToString());
                    destino.EscribirPropiedad(propiedad.Name, enumerado);
                    continue;
                }

                var valor = origen.GetType().GetProperty(propiedad.Name).GetValue(origen);
                destino.GetType().GetProperty(propiedad.Name).SetValue(destino, valor);
            }
        }

        public static bool HeredaDe(this Type tipo, Type tipoBase, bool incluirTipo = true, bool permitirTipoNulo = true)
        {
            if (permitirTipoNulo && tipo == null) return false;

            if (incluirTipo && tipo == tipoBase) return true;

            var cache = ServicioDeCaches.Obtener(CacheDe.HeredaDe);
            var i = $"{tipo}-{tipoBase}";
            if (!cache.ContainsKey(i))
            {
                var t = tipo.BaseType;
                while (t != null)
                {
                    if (t.FullName == tipoBase.FullName)
                    {
                        cache[i] = true;
                        return (bool)cache[i];
                    }
                    t = t.BaseType;
                }
                cache[i] = false;

            }
            return (bool)cache[i];
        }

        public static Type TipoDeLaPropiedad(this Type tipo, string propiedad)
        {
            var propiedades = PropiedadesDelTipo(tipo);
            foreach (var p in propiedades)
            {
                if (p.Name.ToLower() == propiedad.ToLower())
                    return p.PropertyType;
            }
            return null;
        }

        public static bool TienenLaPropiedad(this Type tipo, string propiedad)
        {
            var propiedades = PropiedadesDelTipo(tipo);

            foreach (var p in propiedades)
            {
                if (p.Name.ToLower() == propiedad.ToLower())
                    return true;
            }

            return false;
        }

        public static Dictionary<string, string> ToDiccionario(this Type tipo, Func<string, string> ObtenerValor)
        {
            var opciones = new Dictionary<string, string>();
            foreach (var valor in Enum.GetValues(tipo))
            {
                var texto = ExtensorDeEnum.Descripcion((Enum)valor);
                var clave = ObtenerValor(valor.ToString());
                opciones.Add(clave, texto);
            }

            return opciones;
        }

        public static Dictionary<string, string> ToDiccionario(this Type enumerado)
        {
            var opciones = new Dictionary<string, string>();
            foreach (var valor in Enum.GetValues(enumerado))
            {
                var texto = ExtensorDeEnum.Descripcion((Enum)valor);
                var clave = valor.ToString();
                opciones.Add(clave, texto);
            }

            return opciones;
        }

        public static bool ImplementaInterface(Type claseDtm, string nombreCompleto)
        {
            var interfaces = claseDtm.GetInterfaces();
            foreach (var i in interfaces)
                if (i.FullName.Equals(nombreCompleto))
                    return true;
            return false;
        }


        public static T? ToEnumerado<T>(string enumerado, bool errorSiNoEsValido)
        where T : struct, Enum
        {
            try
            {
                return ToEnumerado<T>(enumerado);
            }
            catch
            {
                if (!errorSiNoEsValido)
                    return null;
                throw;
            }
        }

        public static T ToEnumerado<T>(string enumerado)
        where T : struct, Enum
        {
            var cache = ServicioDeCaches.Obtener($"{nameof(ToEnumerado)}_T");
            var i = typeof(T).Name + enumerado;
            if (cache.ContainsKey(i))
                return (T)cache[i];

            if (Enum.TryParse<T>(enumerado, ignoreCase: true, out var enumValue))
            {
                cache[i] = enumValue;
                return enumValue;
            }
            throw new Exception($"No se ha podido convertir el valor '{enumerado}' en un enumerado de la clase {typeof(T).Name}");
        }


        public static T DescripcionToEnumerado<T>(string descripcion, T? valorPorDefecto = null)
        where T : struct, Enum
        {
            var cache = ServicioDeCaches.Obtener($"{nameof(DescripcionToEnumerado)}_T");
            var i = typeof(T).Name + descripcion;
            if (cache.ContainsKey(i))
                return (T)cache[i];

            var type = typeof(T);
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null && attribute.Description == descripcion)
                {
                    cache[i] = (T)field.GetValue(null);
                    return (T)cache[i];
                }
            }

            if (valorPorDefecto.HasValue)
            {
                return valorPorDefecto.Value;
            }

            throw new Exception($"No se encontró un valor de enumeración con la descripción '{descripcion}' para el tipo {typeof(T).Name}");
        }


        public static dynamic ToEnumerado(Type tipo, string enumerado, bool errorSiNoHay = true)
        {
            if (Nullable.GetUnderlyingType(tipo) != null && Nullable.GetUnderlyingType(tipo).IsEnum)
            {
                tipo = Nullable.GetUnderlyingType(tipo);
            }

            var cache = ServicioDeCaches.Obtener($"{nameof(ToEnumerado)}_tipo");
            var i = tipo.Name + enumerado;
            if (cache.ContainsKey(i))
                return cache[i];

            var nombres = tipo.GetEnumNames();
            foreach (var nombre in nombres)
                if (nombre.ToLower() == enumerado.ToLower())
                {
                    cache[i] = Enum.Parse(tipo, nombre);
                    return cache[i];
                }

            if (!errorSiNoHay) return null;

            throw new Exception($"No se ha podido convertir el valor '{enumerado}' en un enumerado de la clase {tipo.Name}");
        }

        public static dynamic EjecutarMetodo(this DbContext contexto, Type tipo, string nombreMetodo)
        {
            MethodInfo method = typeof(DbContext).GetMethods().Where(x => x.Name == nombreMetodo).First();
            MethodInfo generic = method.MakeGenericMethod(tipo);
            return generic.Invoke(contexto, null);
        }

        public static T CrearObjeto<T>()
        where T : new()
        {
            Type tipo = typeof(T);
            Type tipoGenerico = tipo.MakeGenericType(typeof(int), typeof(string));
            object objeto = Activator.CreateInstance(tipoGenerico);
            return (T)objeto;
        }
        public static List<object> CastList(object obj, Type targetType)
        {
            var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(targetType);
            var toListMethod = typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(targetType);
            var castedList = (IEnumerable<object>)castMethod.Invoke(null, new[] { obj });
            return (List<object>)toListMethod.Invoke(null, new[] { castedList });
        }
    }
}
