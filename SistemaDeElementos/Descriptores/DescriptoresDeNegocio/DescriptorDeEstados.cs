using UtilidadesParaIu;
using MVCSistemaDeElementos.Controllers;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using Utilidades;
using GestorDeElementos;
using ModeloDeDto;
using System.Collections.Generic;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeEstados : DescriptorDeCrud<EstadoDto>
    {
        public DescriptorDeEstados(ContextoSe contexto, ModoDescriptor modo, enumNegocio negocio)
        : base(contexto: contexto
               , controlador: nameof(EstadosController)
               , vista: $"{nameof(EstadosController.CrudDeEstados)}"
               , modo: modo
               , rutaBase: enumNameSpaceTs.Negocio)
        {
            Negocio = negocio;
            NegocioDtm = GestorDeNegocio.LeerNegocio(negocio);

            new RestrictorDeFiltro<EstadoDto>(padre: Mnt.BloqueGeneral
                  , etiqueta: "Negocio"
                  , propiedad: NegocioPor.idNegocio
                  , ayuda: "negocio del elemento"
                  , new Posicion { fila = 0, columna = 0 })
            {
                TextoParaMostrar = negocio.ToNombre(),
                Restrictor = negocio.IdNegocio()
            };

            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 1), "Estado", "Buscar por nombre de estado");

            Mnt.OrdenacionInicial = @$"{nameof(EstadoDto.Nombre)}:nombre:{enumModoOrdenacion.ascendente.Render()};{nameof(EstadoDto.Orden)}:orden:{enumModoOrdenacion.ascendente.Render()}";


            DefinirMf(menuIndividual, Mnt.OpcionesPorElemento);

            var expAcciones = DescriptorDeExpansorDeAcciones(Editor);
            Editor.Expanes.Insert(0, DescriptorDeExpansorTransiciones(Editor, expAcciones.IdHtmlGridDeRelacion));
            Editor.Expanes.Insert(1, expAcciones);
        }
        private void DefinirMf(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<EstadoDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<EstadoDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.{eventosDeMf.Transiciones}' accion-menu='{eventosDeMf.Transiciones}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Transiciones</li>");
        }


        private DescriptorDeExpansor DescriptorDeExpansorTransiciones(DescriptorDeEdicion<EstadoDto> editor, string idGridDeAcciones)
        {
            var expansor = new DescriptorDeExpansor(editor, $"{editor.Id}-transiciones", "Transiciones", mostrarPlegado: true, "Transiciones desde un estado");

            //Definimos el grid de detalles del cuerpo
            var columnasDeZonas = new DescriptorDeColumnas("transicionesSalientes");
            columnasDeZonas.Add(titulo: "Activa", propiedad: nameof(TransicionDto.Activo), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 100);
            columnasDeZonas.Add(titulo: "Sistema", propiedad: nameof(TransicionDto.DelSistema), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 130);
            columnasDeZonas.Add(titulo: "Observación", propiedad: nameof(TransicionDto.ConObservacion), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 130);
            columnasDeZonas.Add(titulo: "Por defecto", propiedad: nameof(TransicionDto.PorDefecto), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 130);
            columnasDeZonas.Add(titulo: "Transición", propiedad: nameof(TransicionDto.Nombre), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeZonas.Add(titulo: "Destino", propiedad: nameof(TransicionDto.Destino), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeZonas.Add(titulo: "IdDestino", propiedad: nameof(TransicionDto.IdDestino), alineacion: enumAliniacion.izquierda, mostrar: false);
            columnasDeZonas.Add(titulo: "Id", propiedad: nameof(TransicionDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(TransicionesController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,TransicionDtm, TransicionDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(TransicionDto.Origen)}
              , { nameof(GridDeRelacion.IdNegocio), NegociosDeSe.IdNegocio(Negocio) }
              , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(EstadosController)}/{nameof(EstadosController.CrudDeEstados)}?negocio={Negocio.ToNombre()}&id={nameof(TransicionDto.IdDestino)}"}
              , { nameof(GridDeRelacion.TituloDeAbrir) , $"Destino"}

            };

            var grid = new GridDeRelacion(expansor, columnasDeZonas, parametros);


            var paginaDeTransiciones = $"{nameof(TransicionesController)}/{nameof(TransicionesController.CrudDeTransiciones)}?negocio={Negocio.ToNombre()}&id={nameof(TransicionDto.Id)}";
            grid.acciones.Add(new ColumnaAccion
            {
                accion = Referencia.NavegarAEditar(expansor, paginaDeTransiciones.Replace(ltrEndPoint.Controller, ""), nameof(IRegistro.Id))
             ,
                titulo = "Transicion"
             ,
                tamano = 130
             ,
                visible = true
            });

            grid.acciones.Add(new ColumnaAccion
            {
                accion = Acciones(grid.IdHtmlContenedor, idGridDeAcciones, Negocio.IdNegocio())
             ,
                titulo = "Acciones"
             ,
                tamano = 130
             ,
                visible = true
            });

            expansor.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(TransicionDto), typeof(TransicionesController), nameof(TransicionDto.Origen), "Añadir transición");
            expansor.DescriptorDeEditarRelaciones(editor.Crud.Contexto, typeof(TransicionDto), typeof(TransicionesController), "Editar la transición", false);
            return expansor;
        }

        private static string Acciones(string idGridDelExpansorOrigen, string idGridDelExpansorDestino, int IdNegocio)
        {
            return $"javascript:{enumNameSpaceTs.Negocio}.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.MostrarAcciones}', '{idGridDelExpansorOrigen};{idGridDelExpansorDestino};{IdNegocio};numeroDeFila')";
        }

        private DescriptorDeExpansor DescriptorDeExpansorDeAcciones(DescriptorDeEdicion<EstadoDto> editor)
        {
            var expansor = new DescriptorDeExpansor(editor, $"{editor.Id}-accion", "Acción de la transición", mostrarPlegado: true, "acción de una transición");

            //Definimos el grid de detalles del cuerpo
            var columnasDeZonas = new DescriptorDeColumnas("accion");
            columnasDeZonas.Add(titulo: "Mto.", propiedad: nameof(AccionesDeTrnDto.Momento), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 50);
            columnasDeZonas.Add(titulo: "Ord.", propiedad: nameof(AccionesDeTrnDto.Orden), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 50);
            columnasDeZonas.Add(titulo: "Acción", propiedad: nameof(AccionesDeTrnDto.Accion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeZonas.Add(titulo: "Descripción", propiedad: nameof(AccionesDeTrnDto.Descripcion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeZonas.Add(titulo: "IdAccion", propiedad: nameof(AccionesDeTrnDto.idAccion), alineacion: enumAliniacion.derecha, mostrar: false);
            columnasDeZonas.Add(titulo: "Id", propiedad: nameof(AccionesDeTrnDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(AccionesDeTrnController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,AccionesDeTrnDtm, AccionesDeTrnDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(AccionesDeTrnDto.IdTransicion) }
              , { nameof(GridDeRelacion.CargarPorEvento), true}
              , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(AccionesController)}/{nameof(AccionesController.CrudDeAcciones)}?id={nameof(AccionesDeTrnDto.idAccion)}"}
            };

            new GridDeRelacion(expansor, columnasDeZonas, parametros) { PermitirBorrar = false };

            expansor.DescriptorDeEditarRelaciones(editor.Crud.Contexto, typeof(AccionesDeTrnDto), typeof(AccionesDeTrnController), "Editar la acción", false);
            return expansor;
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Negocio}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Transiciones.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/Estados.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeEstados('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}','{Borrado.IdHtml}') 
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
