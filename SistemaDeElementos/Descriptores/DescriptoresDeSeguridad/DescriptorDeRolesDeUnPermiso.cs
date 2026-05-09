using ModeloDeDto;
using ModeloDeDto.Seguridad;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeRolesDeUnPermiso : DescriptorDeCrud<RolesDeUnPermisoDto>
    {

        public DescriptorDeRolesDeUnPermiso(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(RolesDeUnPermisoController), nameof(RolesDeUnPermisoController.CrudRolesDeUnPermiso), modo, enumNameSpaceTs.Seguridad)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<RolesDeUnPermisoDto>(padre: fltGeneral
                  , etiqueta: "Permiso"
                  , propiedad: nameof(RolesDeUnPermisoDto.IdPermiso)
                  , ayuda: "buscar por permiso"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(PermisosController),
                VistaDondeNavegar = nameof(PermisosController.CrudPermiso),
                Negocio = enumNegocio.Permiso
            };

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Rol", nameof(RolesDeUnPermisoDto.Rol), "Buscar por 'rol'");

            var modalDeRoles = new ModalDeRelacionarElementos<RolesDeUnPermisoDto, RolDto>(mantenimiento: Mnt
                              , tituloModal: "Seleccione los roles a relacionar"
                              , crudModal: new DescriptorDeRol(contexto, ModoDescriptor.Relacion)
                              , propiedadRestrictora: nameof(RolesDeUnPermisoDto.IdPermiso));
            var relacionarRoles = new RelacionarElementos(modalDeRoles.IdHtml, () => modalDeRoles.RenderControl(), "Seleccionar los roles donde incluir el permiso");
            var opcion = new OpcionDeMenu<RolesDeUnPermisoDto>(Mnt.ZonaMenu.Menu, relacionarRoles, $"Roles", enumModoDeAccesoDeDatos.Gestor);
            Mnt.ZonaMenu.Menu.Add(opcion);

            Mnt.OrdenacionInicial = @$"{nameof(RolesDeUnPermisoDto.Rol)}:{nameof(PermisosDeUnRolDtm.Rol)}.{nameof(PermisosDeUnRolDtm.Rol.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(PuestosDeUnRolController)
                , vista: nameof(PuestosDeUnRolController.CrudPuestosDeUnRol)
                , relacionarCon: nameof(PuestoDto)
                , navegarAlCrud: DescriptorDeMantenimiento<PuestosDeUnRolDto>.NombreMnt
                , nombreOpcion: "Puestos"
                , propiedadQueRestringe: nameof(PuestosDeUnRolDto.IdRol)
                , propiedadRestrictora: nameof(PuestosDeUnRolDto.IdRol)
                , ayuda: "Incluir el rol a los puestos seleccionados");
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/RolesDeUnPermiso.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeRolesDeUnPermiso('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
