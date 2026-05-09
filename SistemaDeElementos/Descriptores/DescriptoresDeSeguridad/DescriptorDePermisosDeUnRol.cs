using ModeloDeDto;
using ModeloDeDto.Seguridad;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePermisosDeUnRol : DescriptorDeCrud<PermisosDeUnRolDto>
    {
        public DescriptorDePermisosDeUnRol(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto ,
              controlador: nameof(PermisosDeUnRolController),
              vista: nameof(PermisosDeUnRolController.CrudPermisosDeUnRol),
              modo: modo,
              rutaBase: enumNameSpaceTs.Seguridad,
              id: null,
              tituloPlural: "Permisos de un rol")
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<PermisosDeUnRolDto>(padre: fltGeneral
                  , etiqueta: "Rol"
                  , propiedad: nameof(PermisosDeUnRolDto.IdRol)
                  , ayuda: "buscar por rol"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(RolController),
                VistaDondeNavegar = nameof(RolController.CrudRol),
                Negocio = enumNegocio.Rol
            };

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Permiso", nameof(PermisosDeUnRolDto.Permiso), "Buscar por 'permiso'");

            //Añade una opcion de menú, para relacionar permisos
            //- Abre una modal de selección
            //- Le pasa el id del elemento con el que se va a relacionar (para no mostrar los ya relacionados)
            //- Al aceptar --> llama al negocio y relaciona los id's 
            //- Al cerrar no hace nada
            var modalDePermisos =  new ModalDeRelacionarElementos<PermisosDeUnRolDto, PermisoDto>(mantenimiento: Mnt
                  ,tituloModal: "Seleccione los permisos a relacionar"
                  ,crudModal: new DescriptorDePermiso(Contexto, ModoDescriptor.Relacion)
                  ,propiedadRestrictora: nameof(PermisosDeUnRolDto.IdRol));

            var relacionarPermisos = new RelacionarElementos(modalDePermisos.IdHtml, () => modalDePermisos.RenderControl(), "Seleccionar permisos a relacionar con el rol");
            var opcion = new OpcionDeMenu<PermisosDeUnRolDto>(Mnt.ZonaMenu.Menu, relacionarPermisos, $"Permisos", enumModoDeAccesoDeDatos.Gestor);
            Mnt.ZonaMenu.Menu.Add(opcion);

            Mnt.OrdenacionInicial = @$"{nameof(PermisosDeUnRolDto.Permiso)}:{nameof(PermisosDeUnRolDtm.Permiso)}.{nameof(PermisosDeUnRolDtm.Permiso.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/PermisosDeUnRol.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDePermisosDeUnRol('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
