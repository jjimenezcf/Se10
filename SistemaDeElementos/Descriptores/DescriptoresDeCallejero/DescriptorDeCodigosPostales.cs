using ModeloDeDto;
using ModeloDeDto.Callejero;
using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeCodigosPostales : DescriptorDeCrud<CodigoPostalDto>
    {
        public DescriptorDeCodigosPostales(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(CodigosPostalesController)
               , nameof(CodigosPostalesController.CrudCodigosPostales)
               , modo
               , enumNameSpaceTs.Callejero
               , tituloPlural: "Códigos postales"
              )
        {            
           var opcionProvincias = AnadirOpcionDeDependencias(Mnt
                , controlador: nameof(ProvinciasController)
                , vista: nameof(ProvinciasController.CrudProvincias)
                , datosDependientes: nameof(ProvinciaDto)
                , navegarAlCrud: DescriptorDeMantenimiento<ProvinciaDto>.NombreMnt
                , nombreOpcion: "Provincias"
                , propiedadQueRestringe: nameof(CodigoPostalDto.Codigo)
                , propiedadRestrictora: nameof(CpsDeUnaProvinciaDto.CodigoPostal)
                , "Provincias de un CP");

            opcionProvincias.SoloMapearEnElFiltro = true;


           var opcionMunicipios = AnadirOpcionDeDependencias(Mnt
                , controlador: nameof(MunicipiosController)
                , vista: nameof(MunicipiosController.CrudMunicipios)
                , datosDependientes: nameof(MunicipioDto)
                , navegarAlCrud: DescriptorDeMantenimiento<MunicipioDto>.NombreMnt
                , nombreOpcion: "Municipios"
                , propiedadQueRestringe: nameof(CodigoPostalDto.Codigo)
                , propiedadRestrictora: nameof(CpsDeUnMunicipioDto.CodigoPostal)
                , "Municipios de un CP");

            opcionMunicipios.SoloMapearEnElFiltro = true;

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Código Postal", nameof(CodigoPostalDto.Codigo), "Buscar por 'código postal'");

            Mnt.OrdenacionInicial = @$"{nameof(CodigoPostalDto.Codigo)}:codigo:{enumModoOrdenacion.ascendente.Render()}";

        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/CodigosPostales.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeCodigosPostales('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
