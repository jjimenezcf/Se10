using UtilidadesParaIu;
using MVCSistemaDeElementos.Controllers;
using ModeloDeDto.Negocio;
using ModeloDeDto.Entorno;
using ServicioDeDatos;
using Utilidades;
using ServicioDeDatos.Elemento;
using System.Linq;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeAuditoria : DescriptorDeCrud<AuditoriaDto>
    {
        public DescriptorDeAuditoria(ContextoSe contexto, ModoDescriptor modo, enumNegocio negocio)
        : base(contexto: contexto
               , controlador: nameof(AuditoriaController)
               , vista: $"{nameof(AuditoriaController.CrudDeAuditoria)}"
               , modo: modo
              , rutaBase: enumNameSpaceTs.Negocio)
        {
            Negocio = negocio;
            NegocioDtm = GestorDeNegocio.LeerNegocio(negocio);

            //RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(1, 1));

            Mnt.BloqueGeneral.QuitarControl(nameof(INombre.Nombre));

            new RestrictorDeFiltro<AuditoriaDto>(padre: Mnt.BloqueGeneral
                  , etiqueta: "Negocio"
                  , propiedad: NegocioPor.idNegocio
                  , ayuda: "negocio del elemento"
                  , new Posicion { fila = 0, columna = 0 });

            new RestrictorDeFiltro<AuditoriaDto>(padre: Mnt.BloqueGeneral
                  , etiqueta: "Elemento"
                  , propiedad: nameof(AuditoriaDto.IdElemento)
                  , ayuda: "elemento auditado"
                  , new Posicion { fila = 0, columna = 1 });

            var modalUsuario = new DescriptorDeUsuario(Contexto, ModoDescriptor.SeleccionarParaFiltrar);
            new SelectorDeFiltro<AuditoriaDto, UsuarioDto>(padre: Mnt.BloqueGeneral,
                                              etiqueta: "Usuario",
                                              filtrarPor: UsuariosPor.AlgunUsuario,
                                              ayuda: "Seleccionar usuario",
                                              posicion: new Posicion() { fila = 1, columna = 0 },
                                              paraFiltrar: nameof(UsuarioDto.Id),
                                              paraMostrar: nameof(UsuarioDto.NombreCompleto),
                                              crudModal: modalUsuario,
                                              propiedadDondeMapear: UsuariosPor.NombreCompleto.ToLower().ToString());

            Editor.MenuDeEdicion.QuitarOpcionDeMenu(eventosDeEdicion.ModificarElemento);
        }
        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Negocio}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Auditoria.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            Negocio.CrearCrudDeAuditoria('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}','{Borrado.IdHtml}') 
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
