using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ModeloDeDto;
using ModeloDeDto.Logistica;
using System.Collections.Generic;
using GestorDeElementos;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeDevolucionesDeVenta : DescriptorDeCrud<DevolucionDeVentaDto>
    {
        public DescriptorDeDevolucionesDeVenta(ContextoSe contexto, string renderCache) : base(contexto, renderCache)
        {
        }

        public DescriptorDeDevolucionesDeVenta(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(DevolucionesDeVentaController)
               , nameof(DevolucionesDeVentaController.CrudDevolucionesDeVenta)
               , modo
               , rutaBase: enumNameSpaceTs.Logistica)
        {
            DescriptorDeLineasDeUnaDevolucionDeVenta();
        }

        private void DescriptorDeLineasDeUnaDevolucionDeVenta()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-lineasDeUnaDevolucionDeVenta", "Detalle", mostrarPlegado: true, "Líneas devueltas");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("lineasDeUnaDevolucionDeVenta");
            columnas.Add(titulo: nameof(LineaDeUnaDevolucionDeVentaDto.Orden), alineacion: enumAliniacion.derecha, tamano: 100);
            columnas.Add(titulo: "Documento de venta", propiedad: nameof(LineaDeUnaDevolucionDeVentaDto.Venta));
            columnas.Add(titulo: "Material", propiedad: nameof(LineaDeUnaDevolucionDeVentaDto.LineaDeVenta));
            columnas.Add(titulo: nameof(LineaDeUnaDevolucionDeVentaDto.Cantidad), tamano: 150, formato: enumFormato.Numero_4);
            columnas.Add(titulo: nameof(LineaDeUnaDevolucionDeVentaDto.Precio), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: nameof(LineaDeUnaDevolucionDeVentaDto.Descuento), formato: enumFormato.Porcentaje, tamano: 100);
            columnas.Add(titulo: nameof(LineaDeUnaDevolucionDeVentaDto.Importe), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(LineaDeUnaDevolucionDeVentaDto.IdElemento), mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(LineaDeUnaDevolucionDeVentaDto.Id), mostrar: false);

            var orden = $"{nameof(LineaDeUnaDevolucionDeVentaDto.Orden)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(LineasDeUnaDevolucionDeVentaController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(LineasDeUnaDevolucionDeVentaController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(LineaDeUnaDevolucionDeVentaDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnaDevolucionDeVentaDto), typeof(LineasDeUnaDevolucionDeVentaController), nameof(LineaDeUnaDevolucionDeVentaDto.IdElemento), "Añadir devolución");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {RutaBase}.{enumFunctionTs.Ddv_InicializarModalParaCrearLineas}({ExtensorDeDevolucionesDeVenta.IncrementarOrdenEn(Contexto)})";

            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnaDevolucionDeVentaDto), typeof(LineasDeUnaDevolucionDeVentaController), "Editar devolución", soloConsulta: false);
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
                return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src='../../js/{RutaBase}/ApiDeLogistica.js?v={System.DateTime.Now.Ticks}'></script>
                      <script src=¨../../js/{RutaBase}/DevolucionesDeVenta.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{
                           {RutaBase}.CrearCrudDeDevolucionesDeVenta('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}')
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
