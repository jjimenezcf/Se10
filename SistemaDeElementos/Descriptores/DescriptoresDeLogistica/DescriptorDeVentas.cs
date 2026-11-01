using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ModeloDeDto;
using ModeloDeDto.Logistica;
using System.Collections.Generic;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeVentas : DescriptorDeCrud<VentaDto>
    {
        public DescriptorDeVentas(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(VentasController)
               , nameof(VentasController.CrudVentas)
               , modo
               , rutaBase: enumNameSpaceTs.Logistica)
        {
            DescriptorDeLineasDeUnaVenta();
        }

        private void DescriptorDeLineasDeUnaVenta()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-lineasDeUnaVenta", "Detalle", mostrarPlegado: true, "Líneas del documento de venta");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles del documento de venta
            var columnas = new DescriptorDeColumnas("lineasDeUnaVenta");
            columnas.Add(titulo: nameof(LineaDeUnaVentaDto.Orden), alineacion: enumAliniacion.derecha, tamano: 100);
            columnas.Add(titulo: nameof(LineaDeUnaVentaDto.Unitario));
            columnas.Add(titulo: nameof(LineaDeUnaVentaDto.Cantidad), tamano: 150, formato: enumFormato.Numero_6);
            columnas.Add(titulo: nameof(LineaDeUnaVentaDto.Precio), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: nameof(LineaDeUnaVentaDto.Descuento), formato: enumFormato.Porcentaje, tamano: 100);
            columnas.Add(titulo: nameof(LineaDeUnaVentaDto.Importe), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: "Precio almacén", propiedad: nameof(LineaDeUnaVentaDto.PrecioAlmacen), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: nameof(LineaDeUnaVentaDto.Margen), formato: enumFormato.Porcentaje, tamano: 100);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(LineaDeUnaVentaDto.IdElemento), mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(LineaDeUnaVentaDto.Id), mostrar: false);

            var orden = $"{nameof(LineaDeUnaVentaDto.Orden)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(LineasDeUnaVentaController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(LineasDeUnaVentaController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(LineaDeUnaVentaDto.IdElemento) }
                ,  { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnaVentaDto), typeof(LineasDeUnaVentaController), nameof(LineaDeUnaVentaDto.IdElemento), "Añadir línea");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {RutaBase}.{enumFunctionTs.Dsa_InicializarModalParaCrearLineas}({ExtensorDeVentas.IncrementarOrdenEn(Contexto)})";

            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnaVentaDto), typeof(LineasDeUnaVentaController), "Editar línea", soloConsulta: false);
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
                return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src='../../js/{RutaBase}/ApiDeLogistica.js?v={System.DateTime.Now.Ticks}'></script>
                      <script src=¨../../js/{RutaBase}/Ventas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{
                           {RutaBase}.CrearCrudDeVentas('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}')
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
