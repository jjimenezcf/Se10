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
    public class DescriptorDeCpsDeUnMunicipio : DescriptorDeCrud<CpsDeUnMunicipioDto>
    {
        
        public DescriptorDeCpsDeUnMunicipio(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto,
              controlador: nameof(CpsDeUnMunicipioController),
              vista: nameof(CpsDeUnMunicipioController.CrudCpsDeUnMunicipio),
              modo: modo,
              rutaBase: enumNameSpaceTs.Callejero,
              id: null,
              tituloPlural: "Codigos postales del municipio")
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<CpsDeUnMunicipioDto>(padre: fltGeneral
                  , etiqueta: "Municipio"
                  , propiedad:nameof(CpsDeUnMunicipioDto.IdMunicipio)
                  , ayuda: "buscar por municipio"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(MunicipiosController),
                VistaDondeNavegar = nameof(MunicipiosController.CrudMunicipios),
                Negocio = enumNegocio.Municipio
            };

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Código postal", nameof(CpsDeUnMunicipioDto.CodigoPostal), "Buscar por 'código postal'");

            var modalDeCps = new ModalDeRelacionarElementos<CpsDeUnMunicipioDto, CodigoPostalDto>(mantenimiento: Mnt
                              , tituloModal: "Seleccione los códigos postales a relacionar"
                              , crudModal: new DescriptorDeCodigosPostales(contexto,ModoDescriptor.Relacion)
                              , propiedadRestrictora: nameof(CpsDeUnMunicipioDto.IdMunicipio));
            var relacionarCps = new RelacionarElementos(modalDeCps.IdHtml, () => modalDeCps.RenderControl(), "Añadir códigos postales a la municipio");
            var opcion = new OpcionDeMenu<CpsDeUnMunicipioDto>(Mnt.ZonaMenu.Menu, relacionarCps, $"C.P.", enumModoDeAccesoDeDatos.Gestor);
            Mnt.ZonaMenu.Menu.Add(opcion);

            Mnt.OrdenacionInicial = @$"{nameof(CpsDeUnMunicipioDto.CodigoPostal)}:{nameof(CpsDeUnMunicipioDtm.Cp)}.{nameof(CpsDeUnMunicipioDtm.Cp.Codigo)}:{enumModoOrdenacion.ascendente.Render()}";
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/CpsDeUnMunicipio.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeCpsDeUnMunicipio('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
