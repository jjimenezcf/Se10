using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.SistemaDocumental;
using GestorDeElementos;
using ModeloDeDto;
using ServicioDeDatos.Elemento;
using System.Collections.Generic;
using ModeloDeDto.Ventas;
using ModeloDeDto.Gastos;
using ModeloDeDto.Contabilidad;
namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeEstimacionesDirectas : DescriptorDeCrud<CircuitoDocDto>
    {
        public DescriptorDeEstimacionesDirectas(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }

        public DescriptorDeEstimacionesDirectas(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(CircuitosDocController)
               , nameof(CircuitosDocController.CrudEstimacionesDirectas)
               , modo
               , rutaBase: enumNameSpaceTs.SistemaDocumental
               , eliminarCreacion: true)
        {
            Mnt.OrdenacionInicial = $"{nameof(CircuitoDocDto.Referencia)}:{nameof(CircuitoDocDto.Referencia)}:{enumModoOrdenacion.descendente.Render()}";

            Mnt.Etiqueta = "Gestión de estimaciones directas";
            Editor.Etiqueta = "Consultar estimación";


            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CrearLoteContableDto), eventosDeMf.Spr_CrearLote, "Crear estimación directa"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Spr_CrearLote}' accion-menu='{eventosDeMf.Spr_CrearLote}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>Generar estimación directa</li>");
            Editor.IncluirMfIndividual("Anular estimación.", eventosDeMf.Spr_AnularEstimacionDirecta, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);
           
            DefinirDescriptorDeFacturasEmitidas();
            DefinirDescriptorDeFacturasRecibidas();
            DefinirDescriptorDePagos();
        }

        private void DefinirDescriptorDePagos()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-pago", "Pagos", true, "pagos asociadas al lote contable");
            Editor.Expanes.Insert(3, expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas(enumNegocio.Pago.ToString());
            columnas.Add(titulo: "Tipo", propiedad: nameof(PagoDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 200);
            columnas.Add(titulo: "Pago", propiedad: nameof(PagoDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Importe", propiedad: nameof(PagoDto.Importe), alineacion: enumAliniacion.derecha, formato: enumFormato.Moneda, mostrar: true);
            columnas.Add(titulo: "Id", propiedad: nameof(PagoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(CircuitosDocController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(BaseController<CircuitoDocDto>.epLeerVinculosCon)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(VinculoDtm.idElemento2) }
                 , { nameof(ltrParametrosEp.idVinculado) , NegociosDeSe.IdNegocio(enumNegocio.Pago) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PagosController)}/{nameof(PagosController.CrudPagos)}"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = false;
        }

        private void DefinirDescriptorDeFacturasRecibidas()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-fars", "Facturas Recibidas", true, "facturas recibidas asociadas al lote contable");
            Editor.Expanes.Insert(2, expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas(enumNegocio.FacturaRecibida.ToString());
            columnas.Add(titulo: "Tipo", propiedad: nameof(FacturaRecDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 200);
            columnas.Add(titulo: "Factura", propiedad: nameof(FacturaRecDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "BI.", propiedad: nameof(FacturaRecDto.BaseImponible), alineacion: enumAliniacion.derecha, formato: enumFormato.Moneda, mostrar: true);
            columnas.Add(titulo: "Total", propiedad: nameof(FacturaRecDto.TotalDelPago), alineacion: enumAliniacion.derecha, formato: enumFormato.Moneda, mostrar: true);
            columnas.Add(titulo: "Id", propiedad: nameof(FacturaRecDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(CircuitosDocController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(BaseController<CircuitoDocDto>.epLeerVinculosCon)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(VinculoDtm.idElemento2) }
                 , { nameof(ltrParametrosEp.idVinculado) , NegociosDeSe.IdNegocio(enumNegocio.FacturaRecibida) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(FacturasRecController)}/{nameof(FacturasRecController.CrudFacturasRec)}"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = false;
        }

        private void DefinirDescriptorDeFacturasEmitidas()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-faes", "Facturas Emitidas", true, "facturas emitidas asociadas al lote contable");
            Editor.Expanes.Insert(1, expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas(enumNegocio.FacturaEmitida.ToString());
            columnas.Add(titulo: "Tipo", propiedad: nameof(FacturaEmtDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 200);
            columnas.Add(titulo: "Factura", propiedad: nameof(FacturaEmtDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "BI.", propiedad: nameof(FacturaEmtDto.TotalSinIva), alineacion: enumAliniacion.derecha, formato: enumFormato.Moneda, mostrar: true);
            columnas.Add(titulo: "Total", propiedad: nameof(FacturaEmtDto.APagar), alineacion: enumAliniacion.derecha, formato: enumFormato.Moneda, mostrar: true);
            columnas.Add(titulo: "Id", propiedad: nameof(FacturaEmtDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(CircuitosDocController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(BaseController<CircuitoDocDto>.epLeerVinculosCon)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(VinculoDtm.idElemento2) }
                 , { nameof(ltrParametrosEp.idVinculado) , NegociosDeSe.IdNegocio(enumNegocio.FacturaEmitida) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(FacturasEmtController)}/{nameof(FacturasEmtController.CrudFacturasEmt)}"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = false;
        }

        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/EstimacionesDirectas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeEstimacionesDirectas('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
