using ModeloDeDto;
using ModeloDeDto.Gastos;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorPagosDeUnaRemesa : DescriptorDeCrud<PagoDeUnaRemesaDto>
    {
        
        public DescriptorPagosDeUnaRemesa(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto,
              nameof(PagosDeUnaRemesaController),
              nameof(PagosDeUnaRemesaController.CrudPagosDeUnaRemesa),
              modo,
              enumNameSpaceTs.Gasto)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<PagoDeUnaRemesaDto>(padre: fltGeneral
                  , etiqueta: "Remesa"
                  , propiedad:nameof(PagoDeUnaRemesaDto.IdElemento)
                  , ayuda: "buscar por remesa"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(RemesasPagController),
                VistaDondeNavegar = nameof(RemesasPagController.CrudRemesasPag),
                Negocio = enumNegocio.RemesaPag
            };

            Mnt.Etiqueta = "Pagos de la remesa";

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Pago", nameof(PagoDeUnaRemesaDto.Pago), "Buscar por 'pago'");

            var modalDePagos = new ModalDeRelacionarElementos<PagoDeUnaRemesaDto, PagoDto>(mantenimiento: Mnt
                              , tituloModal: "Seleccione las pagos a incluir"
                              , crudModal: new DescriptorDePagos(contexto,ModoDescriptor.Relacion)
                              , propiedadRestrictora: nameof(PagoDeUnaRemesaDto.IdElemento)
                              , filtrarPor: ltrDeUnPago.ExcluirPagosDeUnaRemesa);
            var incluirPagos = new RelacionarElementos(modalDePagos.IdHtml, () => modalDePagos.RenderControl(), "Incluir pagos a la remesa");
            var opcion = new OpcionDeMenu<PagoDeUnaRemesaDto>(Mnt.ZonaMenu.Menu, incluirPagos, "Añadir", enumModoDeAccesoDeDatos.Gestor);
            Mnt.ZonaMenu.Menu.Add(opcion);

            Mnt.OrdenacionInicial = @$"{nameof(PagoDeUnaRemesaDto.Pago)}:{nameof(PagoDeUnaRemesaDtm.Pago)}.{nameof(PagoDeUnaRemesaDtm.Pago.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src='../../js/{RutaBase}/ApiDeGastos.js?v={System.DateTime.Now.Ticks}'></script>
                      <script src=¨../../js/{RutaBase}/PagosDeUnaRemesa.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudPagosDeUnaRemesa('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
