using ModeloDeDto;
using ModeloDeDto.Seguridad;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePuestosDeUnUsuario : DescriptorDeCrud<PuestosDeUnUsuarioDto>
    {
        
        public DescriptorDePuestosDeUnUsuario(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(PuestosDeUnUsuarioController), nameof(PuestosDeUnUsuarioController.CrudPuestosDeUnUsuario), modo, enumNameSpaceTs.Seguridad)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<PuestosDeUnUsuarioDto>(padre: fltGeneral
                  , etiqueta: "Usuario"
                  , propiedad:nameof(PuestosDeUnUsuarioDto.IdUsuario)
                  , ayuda: "buscar por usuario"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(UsuariosController),
                VistaDondeNavegar = nameof(UsuariosController.CrudUsuario),
                Negocio = enumNegocio.Usuario
            };

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Puesto", nameof(PuestosDeUnUsuarioDto.Puesto), "Buscar por 'puesto'");


            var modalDePuestos = new ModalDeRelacionarElementos<PuestosDeUnUsuarioDto, PuestoDto>(mantenimiento: Mnt
                              , tituloModal: "Seleccione los puestos a relacionar"
                              , crudModal: new DescriptorDePuestoDeTrabajo(contexto, ModoDescriptor.Relacion)
                              , propiedadRestrictora: nameof(PuestosDeUnUsuarioDto.IdUsuario));
            var relacionarPuestos = new RelacionarElementos(modalDePuestos.IdHtml, () => modalDePuestos.RenderControl(), "Seleccionar los puestos de un usuario");
            var opcion = new OpcionDeMenu<PuestosDeUnUsuarioDto>(Mnt.ZonaMenu.Menu, relacionarPuestos, $"Puestos", enumModoDeAccesoDeDatos.Gestor);
            Mnt.ZonaMenu.Menu.Add(opcion);

            Mnt.OrdenacionInicial = @$"{nameof(PuestosDeUnUsuarioDto.CgDelPuesto)}:puesto.cg.codigo:{enumModoOrdenacion.ascendente.Render()};
                                       {nameof(PuestosDeUnUsuarioDto.Puesto)}:puesto.nombre:{enumModoOrdenacion.ascendente.Render()}";

            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(RolesDeUnPuestoController)
                , vista: nameof(RolesDeUnPuestoController.CrudRolesDeUnPuesto)
                , relacionarCon: nameof(RolDto)
                , navegarAlCrud: DescriptorDeMantenimiento<RolesDeUnPuestoDto>.NombreMnt
                , nombreOpcion: "Roles"
                , propiedadQueRestringe: nameof(PuestosDeUnUsuarioDto.IdPuesto)
                , propiedadRestrictora: nameof(RolesDeUnPuestoDto.IdPuesto)
                , ayuda: "Añadir roles al puesto seleccionado");
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/PuestosDeUnUsuario.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDePuestosDeUnUsuario('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
