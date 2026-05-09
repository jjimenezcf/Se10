using System;
using System.Linq;
using Utilidades;
using ServicioDeDatos.Elemento;
using System.IO;
using System.Collections.Generic;
using ServicioDeDatos.Entorno;
using Dapper;
using Gestor.Errores;
using ServicioDeDatos.Negocio;
using System.Reflection;
using ServicioDeDatos;
using Microsoft.SqlServer.Dac.Model;

namespace ModeloDeDto
{

    public class TipoDtoElmento
    {
        public string TipoDto { get; set; }
        public int IdElemento { get; set; }
        public string Referencia { get; set; }
        public string Vista { get; set; }

        public Type ClaseDto()
        {
            return ExtensionesDto.ObtenerTypoDto(TipoDto);
        }

        public static TipoDtoElmento Crear<T>(ElementoDtm elementoDtm, string vista = null)
            where T : ElementoDto
        {
            var tipo = new TipoDtoElmento();
            tipo.TipoDto = typeof(T).FullName;
            tipo.IdElemento = elementoDtm.Id;
            tipo.Vista = vista;
            if (elementoDtm.GetType().ImplementaUsaReferencia())
                tipo.Referencia = ((IUsaReferencia)elementoDtm).Referencia + "-" + elementoDtm.Nombre;
            else 
                tipo.Referencia = elementoDtm.Nombre;
            return tipo;

        }
    }

    public static class ExtensionesDto
    {

        //public static void EscribirPropiedad(this ElementoDto elemento, string propiedad, object valor)
        //{
        //    elemento.GetType().GetProperty(propiedad).SetValue(elemento, valor);
        //}

        public static enumAliniacion Alineada(this Type tipo)
        {
            if (tipo == typeof(string)) return enumAliniacion.izquierda;
            if (tipo == typeof(int)) return enumAliniacion.derecha;
            if (tipo == typeof(int?)) return enumAliniacion.derecha;
            if (tipo == typeof(decimal)) return enumAliniacion.derecha;
            if (tipo == typeof(decimal?)) return enumAliniacion.derecha;
            if (tipo == typeof(DateTime)) return enumAliniacion.centrada;
            if (tipo == typeof(DateTime?)) return enumAliniacion.centrada;

            //Si es un date time nulable
            if (tipo.GetProperties().Count() == 2 && tipo.GetProperties()[0].PropertyType.Equals(typeof(DateTime)))
                return enumAliniacion.centrada;

            return enumAliniacion.izquierda;
        }

        public static string ToExcel<T>(this List<T> elementos, ContextoSe contexto, string ruta, string fichero, List<VisibilidadDeColumna> visibilidad,
            List<DisposicionDeColumna> disposicion,
            string patron = null)
        {
            var objetoParaExportar = new ObjetoParaExportar(
                rutaConFichero: Path.Combine(ruta, fichero),
                datos: new Dictionary<string, object> { { ltrExcelExportador.Registros, elementos } }
                );

            var encabezado = Encabezado(typeof(T), visibilidad, disposicion);
            var excel = new ExportarExcel<T>(objetoParaExportar, encabezado, patron);
            contexto.AnotarTraza("Encabezados", string.Join(" | ", encabezado.Select(c => c.Etiqueta)));
            return excel.Exportar();
        }


        private static List<ColumnaDelExcel> Encabezado(Type tipo, List<VisibilidadDeColumna> visibilidad, List<DisposicionDeColumna> disposicion)
        {
            var atributosJson = ApiClasesComunes.ObtenerAtributosJson(tipo, enumRutas.RutaDeJson, ServicioDeCaches.UsaCacheParaRenderizar);
            var encabezado = new List<ColumnaDelExcel>();
            List<string> propiedadesOrdenadas = null;

            if (visibilidad is not null && disposicion is not null)
            {

                propiedadesOrdenadas = (
                    from v in visibilidad
                    join d in disposicion on v.Propiedad equals d.Propiedad
                    where v.Visible
                    orderby d.Posicion
                    select v.Propiedad
                ).ToList();
            }
            else
            {
                propiedadesOrdenadas = new List<string>();
                foreach (var propiedad in tipo.GetProperties())
                {
                    if (propiedad.Name.StartsWith("Id", StringComparison.CurrentCultureIgnoreCase) && (propiedad.PropertyType == typeof(int) || propiedad.PropertyType == typeof(int?)))
                    {
                        continue;
                    }
                    propiedadesOrdenadas.Add(propiedad.Name);
                }
            }

            foreach (var propiedad in propiedadesOrdenadas)
            {
                var propertyInfo = tipo.GetProperty(propiedad, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (propertyInfo != null)
                {
                    IUPropiedadAttribute atributos = ApiDeAtributos.ObtenerAtributos(propertyInfo, atributosJson);
                    var titulo = ApiDeAtributos.Titulo(propertyInfo, atributos);
                    if (!titulo.IsNullOrEmpty())
                        encabezado.Add(new ColumnaDelExcel
                        {
                            Propiedad = propertyInfo,
                            Etiqueta = atributos.EtiquetaGrid,
                            Totalizar = atributos.Totalizar,
                            Formato = atributos.Formato
                        });
                }
            }

            return encabezado;
        }

        public static Type ObtenerTypoDto(string tipoDto, bool emitirError = true)
        {
            var cache = ServicioDeCaches.Obtener(nameof(ObtenerTypoDto));
            if (!cache.ContainsKey(tipoDto))
                cache[tipoDto] = ApiDeEnsamblados.ObtenerType(ApiDeEnsamblados.DllDelModeloDeDto, tipoDto, emitirError);

            return (Type)cache[tipoDto];
        }

        public static string UrlBaseDeUnDto(Type claseDto, string vista, bool errorSiMasDeUno = true)
        {

            var cache = ServicioDeCaches.Obtener(nameof(UrlBaseDeUnDto));
            var i = claseDto.FullName + "-" + vista;
            if (!cache.ContainsKey(i))
            {
                var consulta = vista.IsNullOrEmpty()
                    ? new ConsultaSql<VistaMvcDtm>(VistaMvcSqls.LeerVistaPorDto)
                    : new ConsultaSql<VistaMvcDtm>(VistaMvcSqls.LeerVistaPorVista);

                var valores = new Dictionary<string, object> { { $"@{ICampos.ELEMENTO_DTO}", claseDto.FullName } };
                if (!vista.IsNullOrEmpty()) { valores.Add($"@{ICampos.VISTA}", vista); }

                var vistas = consulta.LanzarConsulta(new DynamicParameters(valores));

                if (vistas.Count == 0)
                    GestorDeErrores.Emitir($"No se ha indicado la vista para mostrar el dto {claseDto.FullName}");
                if (vistas.Count > 1 && errorSiMasDeUno)
                    GestorDeErrores.Emitir($"Se ha indicado más de una vista para mostrar el dto {claseDto.FullName}");

                cache[i] = $"{vistas[0].Controlador}/{vistas[0].Accion}";
            }

            return (string)cache[i];
        }

        public static Type TipoDtm<T>(bool emitirError = true) where T : ElementoDto => typeof(T).TipoDtm(emitirError);

        public static Type TipoDtm(this Type tipoDto, bool emitirError = true)
        {
            var nombreTipoDtm = tipoDto.FullName.Replace(nameof(ModeloDeDto), nameof(ServicioDeDatos)).Replace("Dto", "Dtm");
            return ApiDeRegistroDtm.ObtenerTypoDtm(nombreTipoDtm, emitirError);
        }

    }
}