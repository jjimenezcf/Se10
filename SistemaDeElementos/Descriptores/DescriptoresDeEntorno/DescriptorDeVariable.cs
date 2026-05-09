using ModeloDeDto;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeVariable : DescriptorDeCrud<VariableDto>
    {
        public DescriptorDeVariable(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(VariablesController), nameof(VariablesController.CrudVariable), modo, enumNameSpaceTs.Entorno)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new EditorFiltro<VariableDto>(padre: fltGeneral
                , etiqueta: "Valor"
                , propiedad: nameof(VariableDto.Valor)
                , ayuda: "buscar por valor"
                , new Posicion { fila = 0, columna = 1 });
        }


        public override string RenderControl()
        {
            
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            if (cache.ContainsKey(indice))
                return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Variables.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           Entorno.CrearCrudDeVariables('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el crud', error.message);
                         }}
                      </script>
                    ";
            cache[indice] = render.Render();
            return (string)cache[indice];
        }
    }

}
