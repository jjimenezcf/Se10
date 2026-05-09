using UtilidadesParaIu;
using MVCSistemaDeElementos.Controllers;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using Utilidades;
using GestorDeElementos;
using ServicioDeDatos.Elemento;
using ModeloDeDto;
using System.Collections.Generic;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeTransiciones : DescriptorDeCrud<TransicionDto>
    {
        public DescriptorDeTransiciones(ContextoSe contexto, ModoDescriptor modo, enumNegocio negocio)
        : base(contexto: contexto
               , controlador: nameof(TransicionesController)
               , vista: $"{nameof(TransicionesController.CrudDeTransiciones)}"
               , modo: modo
               , rutaBase: enumNameSpaceTs.Negocio)
        {
            Negocio = negocio;
            NegocioDtm = GestorDeNegocio.LeerNegocio(negocio);

            new RestrictorDeFiltro<TransicionDto>(padre: Mnt.BloqueGeneral
                  , etiqueta: "Negocio"
                  , propiedad: NegocioPor.idNegocio
                  , ayuda: "negocio del elemento"
                  , new Posicion { fila = 0, columna = 0 })
            {
                TextoParaMostrar = negocio.ToNombre(),
                Restrictor = negocio.IdNegocio()
            };

            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 1), "Transición", "Buscar por nombre de transición");

            var estados = new ListasDinamicas<TransicionDto>(Mnt.BloqueGeneral,
                 etiqueta: "Estado",
                 filtrarPor: ltrTransiciones.filtroOrPorIdDeEstado,
                 ayuda: "seleccione estado",
                 seleccionarDe: nameof(EstadoDto),
                 buscarPor: nameof(EstadoDto.Nombre),
                 mostrarExpresion: nameof(EstadoDto.Nombre),
                 criterioDeBusqueda: enumCriteriosDeFiltrado.comienza,
                 posicion: new Posicion(0, 2),
                 controlador: nameof(EstadosController),
                 navegarA: enumVistasNegocio.CrudDeEstados,
                 restringirPor: "",
                 alSeleccionarBlanquearControl: ""
                 );
            estados.LongitudMinimaParaBuscar = 1;
            estados.Negocio = Negocio;
            Editor.Expanes.Insert(0, DescriptorDeAcciones(Editor));

            Mnt.OrdenacionInicial = @$"{nameof(TransicionDto.Origen)}:origen:{enumModoOrdenacion.ascendente.Render()};
                                       {nameof(TransicionDto.Nombre)}:nombre:{enumModoOrdenacion.ascendente.Render()};
                                       {nameof(TransicionDto.Destino)}:destino:{enumModoOrdenacion.ascendente.Render()}";
        }


        private DescriptorDeExpansor DescriptorDeAcciones(DescriptorDeEdicion<TransicionDto> editor)
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
              , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(AccionesController)}/{nameof(AccionesController.CrudDeAcciones)}?id={nameof(AccionesDeTrnDto.idAccion)}"}
            };

            new GridDeRelacion(expansor, columnasDeZonas, parametros);
            expansor.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(AccionesDeTrnDto), typeof(AccionesDeTrnController), nameof(AccionesDeTrnDto.IdTransicion), "Añadir acciones");
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
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeTransiciones('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}','{Borrado.IdHtml}') 
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
