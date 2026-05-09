using ModeloDeDto.Callejero;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorTiposDeVia : DescriptorDeCrud<TipoDeViaDto>
    {
        public DescriptorTiposDeVia(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(TiposDeViaController)
               , nameof(TiposDeViaController.CrudTiposDeVia)
               , modo
               , rutaBase: enumNameSpaceTs.Callejero)
        {
            new EditorFiltro<TipoDeViaDto>(padre: Mnt.BloqueGeneral
                , etiqueta: "Sigla"
                , propiedad: nameof(TipoDeViaDto.Sigla)
                , ayuda: "buscar por sigla"
                , new Posicion { fila = 0, columna = 0 });
            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 1), "T.Vía", "buscar por tipo de vía");            
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/TiposDeVia.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudTiposDeVia('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el crud', error.message);
                         }}
                      </script>
                    ";
            ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice] = render.Render();
			return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
        }
    }

}
