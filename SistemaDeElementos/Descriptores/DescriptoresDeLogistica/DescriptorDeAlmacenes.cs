using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ModeloDeDto.Logistica;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeAlmacenes : DescriptorDeCrud<AlmacenDto>
    {
        public DescriptorDeAlmacenes(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(AlmacenesController)
               , nameof(AlmacenesController.CrudAlmacenes)
               , modo
               , rutaBase: enumNameSpaceTs.Logistica)
        {
            // navegan a otros mantenimientos con el almacén seleccionado fijado (ver CrudDeAlmacenes.DespuesDeProcesarOpcionMf)
            IncluirOpcionDeRelacion(eventosDeMf.Alm_IrACompras, "Compras");
            IncluirOpcionDeRelacion(eventosDeMf.Alm_IrAVentas, "Ventas");
            IncluirOpcionDeRelacion(eventosDeMf.Alm_IrARecuentos, "Recuentos");
            Mnt.IncluirMfDeRelacion("<hr>");
            IncluirOpcionDeRelacion(eventosDeMf.Alm_IrAInventario, "Inventario");
            IncluirOpcionDeRelacion(eventosDeMf.Alm_IrAMovimientos, "Movimientos");
        }

        private void IncluirOpcionDeRelacion(string accion, string texto)
        {
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{accion}' accion-menu='{accion}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{texto}</li>");
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
                return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src='../../js/{RutaBase}/ApiDeLogistica.js?v={System.DateTime.Now.Ticks}'></script>
                      <script src=¨../../js/{RutaBase}/Almacenes.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{
                           {RutaBase}.CrearCrudDeAlmacenes('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}')
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
