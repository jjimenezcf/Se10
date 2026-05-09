using ModeloDeDto.Callejero;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeJuzgados : DescriptorDeCrud<JuzgadoDto>
    {
        public DescriptorDeJuzgados(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , controlador: nameof(JuzgadosController)
              , vista: nameof(JuzgadosController.CrudJuzgados)
              , modo: modo
              , rutaBase: enumNameSpaceTs.Terceros)
        {
            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 0), "Juzgado", "Buscar por nombre (contenido)");

            new ListaDeElemento<JuzgadoDto>(Mnt.BloqueGeneral,
                etiqueta: "Clase",
                ayuda: "selecciona una clase",
                seleccionarDe: nameof(ClaseDeJuzgadoDto),
                filtraPor: nameof(JuzgadoDto.IdClase),
                mostrarExpresion: ClaseDeJuzgadoDto.MostrarExpresion,
                posicion: new Posicion() { fila = 0, columna = 1 },
                nameof(ClasesDeJuzgadoController));

            var listaMunicipio = new ListasDinamicas<JuzgadoDto>(Mnt.BloqueGeneral,
                etiqueta: "Municipio",
                filtrarPor: nameof(JuzgadoDto.IdMunicipio),
                ayuda: "seleccione el municipio",
                seleccionarDe: nameof(MunicipioDto),
                buscarPor: nameof(MunicipioDto.Nombre),
                mostrarExpresion: $"[{nameof(MunicipioDto.Nombre)}]",
                criterioDeBusqueda: enumCriteriosDeFiltrado.comienza,
                posicion: new Posicion(0, 2),
                controlador: nameof(MunicipiosController),
                navegarA: nameof(MunicipiosController.CrudMunicipios),
                restringirPor:"",
                alSeleccionarBlanquearControl: "");
            listaMunicipio.LongitudMinimaParaBuscar = 1;
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                      $@"<script src=¨../../js/{RutaBase}/Juzgados.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeJuzgados('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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

/*
 * 
                    
*/
