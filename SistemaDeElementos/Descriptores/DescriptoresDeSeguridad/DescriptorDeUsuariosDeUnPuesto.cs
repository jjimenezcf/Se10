using ModeloDeDto;
using ModeloDeDto.Entorno;
using ModeloDeDto.Seguridad;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeUsuariosDeUnPuesto : DescriptorDeCrud<UsuariosDeUnPuestoDto>
    {
        
        public DescriptorDeUsuariosDeUnPuesto(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(UsuariosDeUnPuestoController), nameof(UsuariosDeUnPuestoController.CrudUsuariosDeUnPuesto), modo, enumNameSpaceTs.Seguridad)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<UsuariosDeUnPuestoDto>(padre: fltGeneral
                  , etiqueta: "Puesto"
                  , propiedad:nameof(UsuariosDeUnPuestoDto.IdPuesto)
                  , ayuda: "buscar por puesto"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(PuestoDeTrabajoController),
                VistaDondeNavegar = nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo),
                Negocio = enumNegocio.Puesto
            };

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Usuario", nameof(UsuariosDeUnPuestoDto.Usuario), "Buscar por 'usuario'");

            var modalDePuestos = new ModalDeRelacionarElementos<UsuariosDeUnPuestoDto, UsuarioDto>(mantenimiento: Mnt
                              , tituloModal: "Seleccione los usuarios a relacionar"
                              , crudModal: new DescriptorDeUsuario(contexto,ModoDescriptor.Relacion)
                              , propiedadRestrictora: nameof(UsuariosDeUnPuestoDto.IdPuesto));
            var relacionarPuestos = new RelacionarElementos(modalDePuestos.IdHtml, () => modalDePuestos.RenderControl(), "Añadir usuarios al puesto");
            var opcion = new OpcionDeMenu<UsuariosDeUnPuestoDto>(Mnt.ZonaMenu.Menu, relacionarPuestos, $"Usuarios", enumModoDeAccesoDeDatos.Gestor);
            Mnt.ZonaMenu.Menu.Add(opcion);

            Mnt.OrdenacionInicial = @$"{nameof(UsuariosDeUnPuestoDto.Usuario)}:{nameof(PuestosDeUnUsuarioDtm.Usuario)}.{nameof(PuestosDeUnUsuarioDtm.Usuario.Login)}:{enumModoOrdenacion.ascendente.Render()}";


        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/UsuariosDeUnPuesto.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeUsuariosDeUnPuesto('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
