using GestorDeElementos;
using ModeloDeDto.Logistica;
using ModeloDeDto.MaestrosTecnico;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Logistica;
using System.Collections.Generic;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeMovimientosDeAlmacen : DescriptorDeCrud<MovimientoDeAlmacenDto>
    {
        public DescriptorDeMovimientosDeAlmacen(ContextoSe contexto, string renderCache) : base(contexto, renderCache)
        {
        }

        public DescriptorDeMovimientosDeAlmacen(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto, 
              nameof(MovimientosDeAlmacenController), 
              nameof(MovimientosDeAlmacenController.CrudMovimientosDeAlmacen), 
              modo,
              rutaBase: enumNameSpaceTs.Logistica,
              tituloPlural: ltrVistasTitulos.CrudDeMovimientosDeAlmacen)
        {

           Mnt.BloqueGeneral.QuitarControl(nameof(INombre.Nombre));

            var almacenes = contexto.SeleccionarTodos<AlmacenDtm>(filtros: new Dictionary<string, object> {  }, 
                parametros: new Dictionary<string, object> { { ltrParametrosNeg.ExcluirTerminados, false} });

            var opciones = new Dictionary<string, string>();
            foreach (AlmacenDtm almacen in almacenes)
                opciones.Add(almacen.Id.ToString(), almacen.Expresion);

            new ListaDeValores<MovimientoDeAlmacenDto>(Mnt.BloqueGeneral,
                etiqueta: "Almacén",
                ayuda: "Seleccione el almacén   ",
                opciones: opciones,
                filtraPor: nameof(ltrDeUnMovimientoDeAlmacen.FiltroPorAlmacen),
                posicion: new Posicion() { fila = 0, columna = 0 });


            var tipos = contexto.Set<TipoMovimientoDtm>();

            var tiposDeMovimiento = new Dictionary<string, string>();
            foreach (TipoMovimientoDtm tipo in tipos)
                tiposDeMovimiento.Add(tipo.Id.ToString(), tipo.Nombre);

            new ListaDeValores<MovimientoDeAlmacenDto>(Mnt.BloqueGeneral,
                etiqueta: "Tipo",
                ayuda: "Seleccione el tipo de movimiento",
                opciones: tiposDeMovimiento,
                filtraPor: nameof(ltrDeUnMovimientoDeAlmacen.FiltroPorTipoMovimiento),
                posicion: new Posicion() { fila = 0, columna = 1 });

            new ListasDinamicas<MovimientoDeAlmacenDto>(Mnt.BloqueGeneral,
                 etiqueta: enumNegocio.Unitario.Singular(),
                 filtrarPor: nameof(UnitarioDto.Expresion),
                 ayuda: "seleccione el unitario",
                 seleccionarDe: nameof(UnitarioDto),
                 buscarPor: ltrDeUnMovimientoDeAlmacen.FiltroPorUnitario,
                 mostrarExpresion: nameof(UnitarioDto.Expresion),
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion(1, 0),
                 controlador: nameof(UnitariosController),
                 navegarA: nameof(UnitariosController.CrudUnitarios),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "");

            new FiltroEntreFechas<MovimientoDeAlmacenDto>(Mnt.BloqueGeneral,
                etiqueta: "Realizado",
                ayuda: "Seleccione el rango de fechas de movimiento",
                propiedad: ltrDeUnMovimientoDeAlmacen.FiltroPorRealizadoEl,
                posicion: new Posicion() { fila = 1, columna = 1 });
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
                      <script src=¨../../js/{RutaBase}/MovimientosDeAlmacen.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{
                           {RutaBase}.CrearCrudDeMovimientosDeAlmacen('{Mnt.IdHtml}')
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
