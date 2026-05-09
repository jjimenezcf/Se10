using GestorDeElementos;
using ModeloDeDto;
using ModeloDeDto.Contabilidad;
using ModeloDeDto.SistemaDocumental;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using System.Collections.Generic;
using Utilidades;
using UtilidadesParaIu;
namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeLotesContables : DescriptorDeCrud<CircuitoDocDto>
    {
        public DescriptorDeLotesContables(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }

        public DescriptorDeLotesContables(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(CircuitosDocController)
               , nameof(CircuitosDocController.CrudLotesContables)
               , modo
               , rutaBase: enumNameSpaceTs.SistemaDocumental
               , eliminarCreacion: true)
        {
            Mnt.OrdenacionInicial = $"{nameof(CircuitoDocDto.Referencia)}:{nameof(CircuitoDocDto.Referencia)}:{enumModoOrdenacion.descendente.Render()}";
            Mnt.Filtro.FiltroDeNombre.CambiarAtributos("Varios", nameof(ltrDeUnLoteContable.BuscarPorLotePreasiento), "nombre del lote (l:) o referencia del preasiento (p:)");

            Mnt.Etiqueta = "Gestión de lotes contables";
            Editor.Etiqueta = "Consultar lote";
            Editor.PermiteConsultasConGuid = false;


            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CrearLoteContableDto), eventosDeMf.Spr_CrearLote, ltrDeUnPreasiento.Menu_CrearLoteContable));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Spr_CrearLote}' accion-menu='{eventosDeMf.Spr_CrearLote}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>{ltrDeUnPreasiento.Menu_CrearLoteContable}</li>");
            Editor.IncluirMfIndividual("Anular lote contable", eventosDeMf.Spr_AnularLote, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);
            Editor.IncluirMfIndividual("Regenerar lote contable", eventosDeMf.Spr_RegenerarLote, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);


            DefinirDescriptorDePreasientos();
        }


        private void DefinirDescriptorDePreasientos()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-preasientos", "Preasientos", true, "Preasientos del lote contable");
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("preasientos");
            columnas.Add(titulo: "Tipo", propiedad: nameof(PreasientoDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 200);
            columnas.Add(titulo: "Preasiento", propiedad: nameof(PreasientoDto.Origen), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Importe", propiedad: nameof(PreasientoDto.Importe), alineacion: enumAliniacion.derecha, formato: enumFormato.Moneda, mostrar: true);
            columnas.Add(titulo: "Id", propiedad: nameof(PreasientoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(CircuitosDocController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(BaseController<CircuitoDocDto>.epLeerVinculosCon)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(VinculoDtm.idElemento2) }
                 , { nameof(ltrParametrosEp.idVinculado) , NegociosDeSe.IdNegocio(enumNegocio.Preasiento) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PreasientosController)}/{nameof(PreasientosController.CrudPreasientos)}"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = false;
            gridDeRelacion.ConCapa = false;
        }


        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/LotesContables.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeLotesContables('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
