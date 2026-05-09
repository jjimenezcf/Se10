using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeAcciones : DescriptorDeCrud<AccionDto>
    {
        public DescriptorDeAcciones(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(AccionesController), nameof(AccionesController.CrudDeAcciones)
              , modo
              , enumNameSpaceTs.Entorno)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new EditorFiltro<AccionDto>(padre: fltGeneral
                , etiqueta: "Acción"
                , propiedad: nameof(AccionDto.Nombre)
                , ayuda: "buscar por Nombre"
                , new Posicion { fila = 0, columna = 1 });
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Acciones.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeAcciones('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
