using UtilidadesParaIu;
using MVCSistemaDeElementos.Controllers;
using ModeloDeDto.TrabajosSometidos;
using ServicioDeDatos;
using Utilidades;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeTrazasDeUnTrabajo : DescriptorDeCrud<TrazaDeUnTrabajoDto>
    {
        public DescriptorDeTrazasDeUnTrabajo(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , controlador: nameof(TrazasDeUnTrabajoController)
               , vista: $"{nameof(TrazasDeUnTrabajoController.CrudDeTrazasDeUnTrabajo)}"
               , modo: modo
               , rutaBase: enumNameSpaceTs.TrabajosSometido)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<TrazaDeUnTrabajoDto>(padre: fltGeneral
                  , etiqueta: "Trabajo de Usuario"
                  , propiedad: nameof(ErrorDeUnTrabajoDto.IdTrabajoDeUsuario)
                  , ayuda: "buscar por trabajo de usuario"
                  , new Posicion { fila = 0, columna = 0 });
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/TrazasDeUnTrabajo.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            TrabajosSometido.CrearCrudDeTrazasDeUnTrabajo('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
