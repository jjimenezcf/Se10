using ModeloDeDto;
using ModeloDeDto.Seguridad;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeRolesDeUnPuesto : DescriptorDeCrud<RolesDeUnPuestoDto>
    {
        public DescriptorDeRolesDeUnPuesto(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(RolesDeUnPuestoController), nameof(RolesDeUnPuestoController.CrudRolesDeUnPuesto), modo, enumNameSpaceTs.Seguridad)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<RolesDeUnPuestoDto>(padre: fltGeneral
                  , etiqueta: "Puesto"
                  , propiedad: nameof(RolesDeUnPuestoDto.IdPuesto)
                  , ayuda: "buscar por puesto"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(PuestoDeTrabajoController),
                VistaDondeNavegar = nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo),
                Negocio = enumNegocio.Puesto
            };

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Rol", nameof(RolesDeUnPuestoDto.Rol), "Buscar por 'rol'");

            var modalDeRoles = new ModalDeRelacionarElementos<RolesDeUnPuestoDto, RolDto>(mantenimiento: Mnt
                              , tituloModal: "Seleccione los roles a relacionar"
                              , crudModal: new DescriptorDeRol(contexto, ModoDescriptor.Relacion)
                              , propiedadRestrictora: nameof(RolesDeUnPuestoDto.IdPuesto));

            var relacionarRoles = new RelacionarElementos(modalDeRoles.IdHtml, () => modalDeRoles.RenderControl(), "Añadir roles al puesto");
            var opcion = new OpcionDeMenu<RolesDeUnPuestoDto>(Mnt.ZonaMenu.Menu, relacionarRoles, $"Roles", enumModoDeAccesoDeDatos.Gestor);
            Mnt.ZonaMenu.Menu.Add(opcion);

            Mnt.OrdenacionInicial = @$"{nameof(RolesDeUnPuestoDto.Rol)}:{nameof(RolesDeUnPuestoDtm.Rol)}.{nameof(RolesDeUnPuestoDtm.Rol.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(PermisosDeUnRolController)
                , vista: nameof(PermisosDeUnRolController.CrudPermisosDeUnRol)
                , relacionarCon: nameof(PermisoDto)
                , navegarAlCrud: DescriptorDeMantenimiento<PermisosDeUnRolDto>.NombreMnt
                , nombreOpcion: "Permisos"
                , propiedadQueRestringe: nameof(RolesDeUnPuestoDto.IdRol)
                , propiedadRestrictora: nameof(PermisosDeUnRolDto.IdRol)
                , "Incluir permisos en el rol seleccionado");

        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/RolesDeUnPuesto.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeRolesDeUnPuesto('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
