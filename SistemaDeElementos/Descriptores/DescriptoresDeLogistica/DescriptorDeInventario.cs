using GestorDeElementos;
using ModeloDeDto.Logistica;
using ModeloDeDto.MaestrosTecnico;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Seguridad;
using System.Collections.Generic;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeInventario : DescriptorDeCrud<InventarioDto>
    {
        public DescriptorDeInventario(ContextoSe contexto, string renderCache) : base(contexto, renderCache)
        {
        }

        public DescriptorDeInventario(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto,
              nameof(InventarioController),
              nameof(InventarioController.CrudInventario),
              modo,
              rutaBase: enumNameSpaceTs.Logistica,
              tituloPlural: ltrVistasTitulos.CrudDeInventario)
        {
            Mnt.BloqueGeneral.QuitarControl(nameof(INombre.Nombre));

            var almacenes = contexto.SeleccionarTodos<AlmacenDtm>(filtros: new Dictionary<string, object> { },
                parametros: new Dictionary<string, object> { { ltrParametrosNeg.ExcluirTerminados, false } });

            var opciones = new Dictionary<string, string>();
            foreach (AlmacenDtm almacen in almacenes)
                opciones.Add(almacen.Id.ToString(), almacen.Expresion);

            new ListaDeValores<InventarioDto>(Mnt.BloqueGeneral,
                etiqueta: "Almacén",
                ayuda: "Seleccione el almacén",
                opciones: opciones,
                filtraPor: nameof(ltrDeUnMovimientoDeAlmacen.FiltroPorAlmacen),
                posicion: new Posicion() { fila = 0, columna = 0 });

            // la fecha se propone a hoy al inicializar el crud (ver Inventario.ts), no aquí: el render se cachea por usuario
            new FiltroDeFecha<InventarioDto>(Mnt.BloqueGeneral,
                etiqueta: "Fecha",
                propiedad: ltrDeUnMovimientoDeAlmacen.FiltroPorFechaDeInventario,
                ayuda: "Fecha del inventario",
                posicion: new Posicion() { fila = 0, columna = 1 })
            { AnchoDeColumna = "250px" };

            new ListasDinamicas<InventarioDto>(Mnt.BloqueGeneral,
                 etiqueta: "Material",
                 filtrarPor: ltrDeUnMovimientoDeAlmacen.FiltroPorUnitario,
                 ayuda: "seleccione el material",
                 seleccionarDe: nameof(UnitarioDto),
                 buscarPor: nameof(UnitarioDto.Expresion),
                 mostrarExpresion: nameof(UnitarioDto.Expresion),
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion(0, 2),
                 controlador: nameof(UnitariosController),
                 navegarA: nameof(UnitariosController.CrudUnitarios),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "")
            { OtrosClausulasDeFiltrado = "javascript: " + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ral_FiltrosPorClaseDeUnitario) + "()" };

            // vende el material de la fila seleccionada: la modal se abre con su almacén, su material y su precio de venta (ver Inventario.ts)
            // y al aceptar crea el documento de venta, la salida del almacén y su preasiento (ver VentasController.epCrearVenta)
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CrearVentaDto), eventosDeMf.Ral_VenderMaterial, "Venta de material"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Ral_VenderMaterial}' accion-menu='{eventosDeMf.Ral_VenderMaterial}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Consultor, false)}>Venta de material</li>");

            // genera el Excel para el recuento del almacén filtrado (la modal se abre con el almacén seleccionado, ver Inventario.ts)
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(PrepararRecuentoDto), eventosDeMf.Ral_PrepararRecuento, "Preparar recuento"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Ral_PrepararRecuento}' accion-menu='{eventosDeMf.Ral_PrepararRecuento}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Consultor, false)}>Preparar recuento</li>");
        }

        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
                return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];

            var render = base.RenderControl();

            render = render +
                   $@"<script src='../../js/{RutaBase}/ApiDeLogistica.js?v={System.DateTime.Now.Ticks}'></script>
                      <script src=¨../../js/{RutaBase}/Inventario.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{
                           {RutaBase}.CrearCrudDeInventario('{Mnt.IdHtml}')
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
