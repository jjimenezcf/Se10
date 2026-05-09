using ModeloDeDto;
using ModeloDeDto.Callejero;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeCpsDeUnaProvincia : DescriptorDeCrud<CpsDeUnaProvinciaDto>
    {
        
        public DescriptorDeCpsDeUnaProvincia(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto,
              controlador: nameof(CpsDeUnaProvinciaController),
              vista: nameof(CpsDeUnaProvinciaController.CrudCpsDeUnaProvincia),
              modo: modo,
              rutaBase: enumNameSpaceTs.Callejero,
              id: null,
              tituloPlural: "Codigos postales de la provincia")
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<CpsDeUnaProvinciaDto>(padre: fltGeneral
                  , etiqueta: "Provincia"
                  , propiedad:nameof(CpsDeUnaProvinciaDto.IdProvincia)
                  , ayuda: "buscar por provincia"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(ProvinciasController),
                VistaDondeNavegar = nameof(ProvinciasController.CrudProvincias),
                Negocio = enumNegocio.Provincia
            };

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Código postal", nameof(CpsDeUnaProvinciaDto.CodigoPostal), "Buscar por 'código postal'");

            var modalDePuestos = new ModalDeRelacionarElementos<CpsDeUnaProvinciaDto, CodigoPostalDto>(mantenimiento: Mnt
                              , tituloModal: "Seleccione los códigos postales a relacionar"
                              , crudModal: new DescriptorDeCodigosPostales(contexto,ModoDescriptor.Relacion)
                              , propiedadRestrictora: nameof(CpsDeUnaProvinciaDto.IdProvincia));
            var relacionarCps = new RelacionarElementos(modalDePuestos.IdHtml, () => modalDePuestos.RenderControl(), "Añadir códigos postales a la provincia");
            var opcion = new OpcionDeMenu<CpsDeUnaProvinciaDto>(Mnt.ZonaMenu.Menu, relacionarCps, $"C.P.", enumModoDeAccesoDeDatos.Gestor);
            Mnt.ZonaMenu.Menu.Add(opcion);

            Mnt.OrdenacionInicial = @$"{nameof(CpsDeUnaProvinciaDto.CodigoPostal)}:{nameof(CpsDeUnaProvinciaDtm.Cp)}.{nameof(CpsDeUnaProvinciaDtm.Cp.Codigo)}:{enumModoOrdenacion.ascendente.Render()}";

        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/CpsDeUnaProvincia.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeCpsDeUnaProvincia('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
