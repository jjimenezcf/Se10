using UtilidadesParaIu;
using MVCSistemaDeElementos.Controllers;
using ModeloDeDto.TrabajosSometidos;
using ServicioDeDatos;
using ServicioDeDatos.TrabajosSometidos;
using Utilidades;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeErroresDeUnTrabajo : DescriptorDeCrud<ErrorDeUnTrabajoDto>
    {
        public DescriptorDeErroresDeUnTrabajo(ContextoSe contexto, ModoDescriptor modo)
        : base( contexto: contexto
               , controlador: nameof(ErroresDeUnTrabajoController)
               , vista: $"{nameof(ErroresDeUnTrabajoController.CrudDeErroresDeUnTrabajo)}"
               , modo: modo
               , rutaBase: enumNameSpaceTs.TrabajosSometido)
        {
            new RestrictorDeFiltro<ErrorDeUnTrabajoDto>(padre: Mnt.BloqueGeneral
                  , etiqueta: "Trabajo de Usuario"
                  , propiedad: nameof(ErrorDeUnTrabajoDto.IdTrabajoDeUsuario)
                  , ayuda: "buscar por trabajo de usuario"
                  , new Posicion { fila = 0, columna = 0 });

            new EditorFiltro<ErrorDeUnTrabajoDto>(padre: Mnt.BloqueGeneral
                  , etiqueta: "Error"
                  , propiedad: nameof(ErrorDeUnTrabajoDtm.Detalle)
                  , ayuda: "buscar por detalle del error"
                  , new Posicion { fila = 0, columna = 1 });

            Mnt.OrdenacionInicial = @$"{nameof(ErrorDeUnTrabajoDto.Fecha)}:{nameof(ErrorDeUnTrabajoDtm.Fecha)}:{enumModoOrdenacion.ascendente.Render()}";


        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/ErroresDeUnTrabajo.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            TrabajosSometido.CrearCrudDeErroresDeUnTrabajo('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
