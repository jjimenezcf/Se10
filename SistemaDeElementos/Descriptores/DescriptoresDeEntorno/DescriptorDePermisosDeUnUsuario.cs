using ModeloDeDto;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePermisosDeUnUsuario : DescriptorDeCrud<PermisosDeUnUsuarioDto>
    {
        public DescriptorDePermisosDeUnUsuario(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(PermisosDeUnUsuarioController), nameof(PermisosDeUnUsuarioController.CrudPermisosDeUnUsuario), modo, enumNameSpaceTs.Entorno)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<PermisosDeUnUsuarioDto>(padre: fltGeneral
                  , etiqueta: "Usuario"
                  , propiedad: nameof(PermisosDeUnUsuarioDto.IdUsuario)
                  , ayuda: "buscar por usuario"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(UsuariosController),
                VistaDondeNavegar = nameof(UsuariosController.CrudUsuario),
                Negocio = enumNegocio.Usuario
            };

            var control = BuscarControlEnFiltro(ltrFiltros.Nombre);
            if (control!=null)
                control.CambiarAtributos("Permiso", nameof(PermisosDeUnUsuarioDto.Permiso), "Buscar por 'permiso'");

        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/PermisosDeUnUsuario.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            Entorno.CrearCrudDePermisosDeUnUsuario('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
