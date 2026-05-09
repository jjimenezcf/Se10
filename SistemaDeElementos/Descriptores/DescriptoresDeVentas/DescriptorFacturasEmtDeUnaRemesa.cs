using ModeloDeDto;
using ModeloDeDto.Ventas;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Ventas;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorFacturasEmtDeUnaRemesa : DescriptorDeCrud<FacturaEmtDeUnaRemesaDto>
    {
        
        public DescriptorFacturasEmtDeUnaRemesa(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto,
              nameof(FacturasEmtDeUnaRemesaController),
              nameof(FacturasEmtDeUnaRemesaController.CrudFacturasEmtDeUnaRemesa),
              modo,
              enumNameSpaceTs.Venta)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<FacturaEmtDeUnaRemesaDto>(padre: fltGeneral
                  , etiqueta: "Remesa"
                  , propiedad:nameof(FacturaEmtDeUnaRemesaDto.IdElemento)
                  , ayuda: "buscar por remesa"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(RemesasFaeController),
                VistaDondeNavegar = nameof(RemesasFaeController.CrudRemesasFae),
                Negocio = enumNegocio.RemesaFae
            };

            Mnt.Etiqueta = "Facturas de la remesa";

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Factura", nameof(FacturaEmtDeUnaRemesaDto.Factura), "Buscar por 'factura'");

            var modalDeFacturas = new ModalDeRelacionarElementos<FacturaEmtDeUnaRemesaDto, FacturaEmtDto>(mantenimiento: Mnt
                              , tituloModal: "Seleccione las facturas a incluir"
                              , crudModal: new DescriptorDeFacturasEmt(contexto,ModoDescriptor.Relacion)
                              , propiedadRestrictora: nameof(FacturaEmtDeUnaRemesaDto.IdElemento)
                              , filtrarPor: ltrDeUnaFacturaEmt.ExcluirFacturasDeUnaRemesa);
            var incluirFacturas = new RelacionarElementos(modalDeFacturas.IdHtml, () => modalDeFacturas.RenderControl(), "Incluir facturas a la remesa");
            var opcion = new OpcionDeMenu<FacturaEmtDeUnaRemesaDto>(Mnt.ZonaMenu.Menu, incluirFacturas, "Añadir", enumModoDeAccesoDeDatos.Gestor);
            Mnt.ZonaMenu.Menu.Add(opcion);

            Mnt.OrdenacionInicial = @$"{nameof(FacturaEmtDeUnaRemesaDto.Factura)}:{nameof(FacturaEmtDeUnaRemesaDtm.Factura)}.{nameof(FacturaEmtDeUnaRemesaDtm.Factura.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/ApiDeRemesasFae.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/FacturasEmtDeUnaRemesa.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudFacturasEmtDeUnaRemesa('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
