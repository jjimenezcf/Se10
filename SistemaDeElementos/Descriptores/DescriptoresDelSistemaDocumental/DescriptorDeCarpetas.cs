using Utilidades;
using GestorDeElementos;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeCarpetas : DescriptorDeFormulario
    {
        int _idArchivador;

        public DescriptorDeCarpetas(IGestor gestorDeNegocio, string id, string titulo, string controlador, string vista, int idArchivador)
        : base(gestorDeNegocio.Contexto, id, titulo, controlador, ruta: enumNameSpaceTs.SistemaDocumental, vista: vista)
        {
            Dto = gestorDeNegocio.TipoDeNegocioDto;
            Negocio = gestorDeNegocio.Negocio;
            _idArchivador = idArchivador;

            var bloques = DefinirPanelesDeJerarquia(this, "Capetas", "Carpetas del archivador", "detalle del Carpeta");
            DefinirDescriptorDeArchivos(bloques.contenedorDto);
        }

        private void DefinirDescriptorDeArchivos(BloqueAnexado contenedorDto)
        {
            if (Negocio == enumNegocio.No_Definido)
                return;

            if (NegociosDeSe.UsaArchivos(Negocio))
            {
                var expanDeAnexados = new DescriptorDeExpansor(contenedorDto, $"{contenedorDto.Id}-archivos", "Archivos", true, "Archivos anexados");
                contenedorDto.Expansores.Add(expanDeAnexados);
                expanDeAnexados.CuerpoDelExpansor = new ContenedorDeArchivos(expanDeAnexados, Negocio);
            }
        }

        public string RenderCarpetas()
        {
            var render = RenderFormulario();

            render = render +
                   $@"<script src=¨../../js/_Formulario/Jerarquia.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaVista}/Carpetas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaVista}.CrearFormulario('{IdHtml}','{Negocio.ToNombre()}', {_idArchivador}) 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el formulario', error);
                         }}
                      </script>

                    ";
            return render.Render();
        }
    }
}
