using ModeloDeDto.Callejero;
using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using ModeloDeDto;
using System.Collections.Generic;
using ServicioDeDatos.Callejero;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeBarrios : DescriptorDeCrud<BarrioDto>
    {
        public DescriptorDeBarrios(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(BarriosController)
               , nameof(BarriosController.CrudBarrios)
               , modo
               , rutaBase: enumNameSpaceTs.Callejero)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<BarrioDto>(padre: fltGeneral
                  , etiqueta: "Municipio"
                  , propiedad: nameof(BarrioDto.IdMunicipio)
                  , ayuda: "buscar por municipio"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(MunicipiosController),
                VistaDondeNavegar = nameof(MunicipiosController.CrudMunicipios),
                Negocio = enumNegocio.Municipio
            };


            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(CallesDeUnBarrioController)
                , vista: nameof(CallesDeUnBarrioController.CrudCallesDeUnBarrio)
                , relacionarCon: nameof(CalleDto)
                , navegarAlCrud: DescriptorDeMantenimiento<CallesDeUnBarrioDto>.NombreMnt
                , nombreOpcion: "Calles"
                , propiedadQueRestringe: nameof(BarrioDto.Id)
                , propiedadRestrictora: nameof(CallesDeUnBarrioDto.IdBarrio)
                , "Añadir calles a un barrio");



            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Barrio", nameof(BarrioDto.Nombre), "Buscar por 'barrio'");
            Mnt.OrdenacionInicial = @$"{nameof(BarrioDto.Nombre)}:{nameof(BarrioDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";


            var expansorDeCalles = DescriptorDeExpansorCalles(Editor);
            Editor.Expanes.Insert(0, expansorDeCalles);
        }
        private DescriptorDeExpansor DescriptorDeExpansorCalles(DescriptorDeEdicion<BarrioDto> editor)
        {
            var expansorDeBarrios = new DescriptorDeExpansor(editor, $"{editor.Id}-calles", "Calles", mostrarPlegado: true, "calles de un barrio");

            //Definimos el grid de detalles del cuerpo
            var columnasDeBarrios = new DescriptorDeColumnas("calles");
            columnasDeBarrios.Add(titulo: "Calle", propiedad: nameof(CallesDeUnBarrioDto.Calle), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeBarrios.Add(titulo: "Mano", propiedad: nameof(CallesDeUnBarrioDto.Mano), alineacion: enumAliniacion.izquierda);
            columnasDeBarrios.Add(titulo: "Número desde", propiedad: nameof(CallesDeUnBarrioDto.Desde), alineacion: enumAliniacion.derecha);
            columnasDeBarrios.Add(titulo: "Número hasta", propiedad: nameof(CallesDeUnBarrioDto.Hasta), alineacion: enumAliniacion.derecha);
            columnasDeBarrios.Add(titulo: "Id", propiedad: nameof(CallesDeUnBarrioDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(CallesDeUnBarrioController)}
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,BarriosDeUnaCalleDtm, CallesDeUnBarrioDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(CallesDeUnBarrioDto.IdBarrio) }
            };

            new GridDeRelacion(expansorDeBarrios, columnasDeBarrios, parametros);

            expansorDeBarrios.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(CallesDeUnBarrioDto), typeof(CallesDeUnBarrioController), nameof(CallesDeUnBarrioDto.IdBarrio), "Relacionar con un calle");
            expansorDeBarrios.DescriptorDeEditarRelaciones(editor.Crud.Contexto, typeof(CallesDeUnBarrioDto), typeof(CallesDeUnBarrioController), "Editar la calle de un barrio", false);

            return expansorDeBarrios;
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/literales.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/ApiCallejero.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/Barrios.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeBarrios('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
