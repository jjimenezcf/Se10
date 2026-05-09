using GestorDeElementos;
using ModeloDeDto;
using ModeloDeDto.Gastos;
using ModeloDeDto.SistemaDocumental;
using ModeloDeDto.Ventas;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.SistemaDocumental;
using System.Collections.Generic;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeConsultaDeCad : DescriptorDePaginaDeConsulta
    {
        public DescriptorDeConsultaDeCad(ContextoSe contexto) :
        base(contexto, nameof(CircuitosDocController), nameof(EntidadController<ContextoSe, CircuitoDocDtm, CircuitoDocDto>.Consultar), rutaBase: enumNameSpaceTs.SistemaDocumental, typeof(CircuitoDocDto), "Consulta de Cad")
        {
            var columnasFar = new DescriptorDeColumnas(enumNegocio.FacturaRecibida.ToString());
            columnasFar.Add(titulo: "Tipo", propiedad: nameof(FacturaRecDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 200);
            columnasFar.Add(titulo: "Factura", propiedad: nameof(FacturaRecDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasFar.Add(titulo: "BI.", propiedad: nameof(FacturaRecDto.BaseImponible), alineacion: enumAliniacion.derecha, formato: enumFormato.Moneda, mostrar: true);
            columnasFar.Add(titulo: "Total", propiedad: nameof(FacturaRecDto.TotalDelPago), alineacion: enumAliniacion.derecha, formato: enumFormato.Moneda, mostrar: true);
            columnasFar.Add(titulo: "Id", propiedad: nameof(FacturaRecDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametrosFar = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(CircuitosDocController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(BaseController<CircuitoDocDto>.epLeerVinculosCon)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(VinculoDtm.idElemento2) }
                 , { nameof(ltrParametrosEp.idVinculado) , NegociosDeSe.IdNegocio(enumNegocio.FacturaRecibida) }
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var expansorFar = new EspansorDeVinculos(this, enumNegocio.FacturaRecibida, "facturas asociadas a la estimación directa", columnasFar, parametrosFar);
            expansorFar.DefinirDescriptorDeVinculos();

            var columnasFae = new DescriptorDeColumnas(enumNegocio.FacturaEmitida.ToString());
            columnasFae.Add(titulo: "Tipo", propiedad: nameof(FacturaEmtDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 200);
            columnasFae.Add(titulo: "Factura", propiedad: nameof(FacturaEmtDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasFae.Add(titulo: "BI.", propiedad: nameof(FacturaEmtDto.TotalSinIva), alineacion: enumAliniacion.derecha, formato: enumFormato.Moneda, mostrar: true);
            columnasFae.Add(titulo: "Total", propiedad: nameof(FacturaEmtDto.APagar), alineacion: enumAliniacion.derecha, formato: enumFormato.Moneda, mostrar: true);
            columnasFae.Add(titulo: "Id", propiedad: nameof(FacturaEmtDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametrosFae = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(CircuitosDocController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(BaseController<CircuitoDocDto>.epLeerVinculosCon)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(VinculoDtm.idElemento2) }
                 , { nameof(ltrParametrosEp.idVinculado) , NegociosDeSe.IdNegocio(enumNegocio.FacturaEmitida) }
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var expansorPagos = new EspansorDeVinculos(this, enumNegocio.FacturaEmitida, "facturas asociadas a la estimación directa", columnasFae, parametrosFae);
            expansorPagos.DefinirDescriptorDeVinculos();
        }

        public override string RenderControl()
        {
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/CircuitosDoc.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearConsultaDeCad('{Pagina.IdHtml}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el crud', error.message);
                         }}
                      </script>
                    ";
            return render.Render();
        }
    }

}
