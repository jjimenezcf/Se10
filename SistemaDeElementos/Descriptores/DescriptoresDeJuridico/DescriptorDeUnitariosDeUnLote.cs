using ModeloDeDto;
using ModeloDeDto.Juridico;
using ModeloDeDto.MaestrosTecnico;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeUnitariosDeUnLote : DescriptorDeCrud<UnitariosDeUnLoteDto>
    {
        
        public DescriptorDeUnitariosDeUnLote(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(UnitariosDeUnLoteController), nameof(UnitariosDeUnLoteController.CrudUnitariosDeUnLote), modo, enumNameSpaceTs.Juridico)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<UnitariosDeUnLoteDto>(padre: fltGeneral
                  , etiqueta: "Lote"
                  , propiedad:nameof(UnitariosDeUnLoteDto.IdLote)
                  , ayuda: "buscar por lote"
                  , new Posicion { fila = 0, columna = 0 });

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Unitario", nameof(UnitariosDeUnLoteDto.Unitario), "Buscar por 'unitario'");

            var modalDeUnitarios = new ModalDeRelacionarElementos<UnitariosDeUnLoteDto, UnitarioDto>(mantenimiento: Mnt
                              , tituloModal: "Seleccione los unitarios a relacionar"
                              , crudModal: new DescriptorDeUnitarios(contexto,ModoDescriptor.Relacion)
                              , propiedadRestrictora: nameof(UnitariosDeUnLoteDto.IdLote));
            var relacionarUnitarios = new RelacionarElementos(modalDeUnitarios.IdHtml, () => modalDeUnitarios.RenderControl(), "Añadir unitarios al lote");
            var opcion = new OpcionDeMenu<UnitariosDeUnLoteDto>(Mnt.ZonaMenu.Menu, relacionarUnitarios, $"Unitarios", enumModoDeAccesoDeDatos.Gestor);
            Mnt.ZonaMenu.Menu.Add(opcion);

            Mnt.OrdenacionInicial = @$"{nameof(UnitariosDeUnLoteDto.Unitario)}:{nameof(UnitariosDeUnLoteDtm.Unitario)}.{nameof(UnitariosDeUnLoteDtm.Unitario.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/UnitariosDeUnLote.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeUnitariosDeUnLote('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
