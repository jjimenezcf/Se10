using ModeloDeDto;
using ServicioDeDatos;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class ControlDeArchivo : ControlHtml
    {
        public string IdInfoArchivo => $"{IdHtml}.nombre";
        public string IdHtmlSelector => $"{IdHtml}.ref";
        public string IdHtmlBarra => $"{IdHtml}.barra";
        public string IdHtmlContenedorBarra => $"{IdHtml}.contenedor.barra";

        public int LimiteEnByte { get; set; } = 0;
        public string ExtensionesValidas { get; }

        public string Controlador { get; set; }

        public ControlDeArchivo(IControlHtml padre, string id, string etiqueta, string propiedad, string ayuda, Posicion posicion, string extensionesValidas = ExtensorDeTipoDeArchivos.NoEditables, int limiteEnByte = 3000000)
            : base(padre, id, etiqueta, propiedad, ayuda, posicion)
        {
            ExtensionesValidas = extensionesValidas;
            LimiteEnByte = limiteEnByte;

            if (padre.GetType().IsGenericType &&
                padre.GetType().GetGenericTypeDefinition() == typeof(DescriptorDeTransitar<>))
            {
                // Al usar dynamic, no importa qué <T> tenga, buscará la ruta .Crud.Controlador
                Controlador = ((dynamic)padre).Crud.Controlador;
            }

        }

        // export function PegarPortaPapeles(eventOrNull, controlador: string, idCanvas: string, idImagen: string, idInputNombreArchivo: string, idInputIdArchivo: string)
        // export function SubirArchivoSeleccionado(idContenedor: string, idArchivo: string, idInfoArchivo: string)


        public string RenderArchivo(string estiloContenedor)
        {
            var idInputIdArchivo = $"{IdHtml}-idarchivo";
            var idCanvas = $"canvas-{idInputIdArchivo}";
            var idImagen = $"img-{idInputIdArchivo}";
            var idInputNombreArchivo = $"nombre-{idInputIdArchivo}";
            var htmlArchivo = $@"
<div id=""{IdHtml}.contenedor"" name=""contenedor-control"" {estiloContenedor}>
    <button type=""button"" 
            class=""btn-pegar-portapapeles"" 
            title=""Subir desde portapapeles"" 
            onclick=""ApiDeArchivos.PegarPortaPapeles(event, '{Controlador}', '{idCanvas}', '{idImagen}', '{idInputNombreArchivo}', '{idInputIdArchivo}')"">
        <img src=""{CacheDeVariable.Uri_SubirDelPortaPapeles}"" alt=""Icono subir portapapeles"">
    </button>
    
    <a id=""{idInputIdArchivo}.ref"" class=""{enumCssControlesFormulario.SelectorArchivo.Render()} {enumCssControles.Etiqueta.Render()}"" href=""javascript:ApiDeArchivos.SeleccionarArchivo('{idInputIdArchivo}')"">
       {Ayuda}
    </a>
    <div id=""{IdHtml}-form.contenedor"" name=""contenedor-control"" class=""{enumCssControles.ContenedorArchivo.Render()}"">
        <form method=""post"" action=""SubirArchivo"" enctype=""multipart/form-data"">
            
            <input id=""{idInputIdArchivo}"" 
                   tipo=""{enumTipoControl.SelectorDeUnArchivo.Render()}"" 
                   class=""{enumCssControlesFormulario.Archivo.Render()}"" title="" {Ayuda}"" propiedad=""{Propiedad}"" type=""file"" name=""fichero"" 
                   style=""display: none;"" 
                   accept=""{ExtensionesValidas}"" 
                   controlador=""{Controlador}"" 
                   info-archivo=""{idInputNombreArchivo}"" 
                   limite-en-byte=""{LimiteEnByte}"" 
                   barra-vinculada=""{idInputIdArchivo}.barra"" 
                   visible-en-visor-al-crear=""True"" 
                   onchange=""ApiDeArchivos.SubirArchivoSeleccionado('{Padre.IdHtml}','{idInputIdArchivo}','{idInputNombreArchivo}')"">
            
             <input id=""{idInputNombreArchivo}"" 
                   type=""text"" 
                   tipo=""{enumTipoControl.Editor.Render()}"" 
                   class=""{enumCssControlesFormulario.InfoArchivo.Render()}"" 
                   title="""" 
                   propiedad="""" 
                   style=""display: block;"">
            
            <div id=""{idInputIdArchivo}.contenedor.barra"" class=""{enumCssControlesFormulario.InfoArchivo.Render()}"" style=""display: none;"">
            <div id=""{idInputIdArchivo}.barra"" class=""{enumCssControles.BarraAzulArchivo}"" contenedor-barra=""{idInputIdArchivo}.contenedor.barra"" style=""display: none;"">
            <span></span>
            </div>
            </div>
        </form>
    </div>
</div>
";
            return htmlArchivo;
        }

        public override string RenderControl()
        {
            return RenderArchivo(estiloContenedor: "");
        }
    }
}
