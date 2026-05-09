using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ModeloDeDto.RegistroEs;
using ModeloDeDto.Tarea;
using System.Collections.Generic;
using GestorDeElementos;
using ServicioDeDatos.Seguridad;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeRegistrosEs : DescriptorDeCrud<RegistroEsDto>
    {
        public DescriptorDeRegistrosEs(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(RegistrosEsController)
               , nameof(RegistrosEsController.CrudRegistrosEs)
               , modo
               , rutaBase: enumNameSpaceTs.Administracion)
        {
            IncluirFiltrosRelacionados();
            DefinirMf(menuIndividual, Mnt.OpcionesPorElemento);
            DefinirMf(menuEdicion, Editor.OpcionesMf);
        }

        private void DefinirMf(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<RegistroEsDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.{eventosDeMf.CrearTareas}'"+
                                               $"accion-menu='{eventosDeMf.CrearTareas}' " +
                                               $"{AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)} " +
                                               $"idVinculado={NegociosDeSe.IdNegocio(enumNegocio.Tarea)}>Tarea de resolución</li>");
        }

        private void IncluirFiltrosRelacionados()
        {
            var modal = new ModalDeFiltrado<RegistroEsDto>(Mnt.Filtro, "filtrosDeRelaciones", "Elementos relacionados");
            Mnt.Filtro.Modales.Add(modal);
            FiltroRelacionadosConTareas(modal);
        }

        private static void FiltroRelacionadosConTareas(ModalDeFiltrado<RegistroEsDto> modal)
        {
            var lista = new ListasDinamicas<RegistroEsDto>(modal,
                etiqueta: "Tarea",
                filtrarPor: ltrDeUnaTarea.IdTarea,
                ayuda: "seleccione la tarea",
                seleccionarDe: nameof(TareaDto),
                buscarPor: nameof(TareaDto.Expresion),
                mostrarExpresion: nameof(TareaDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(TareasController),
                navegarA: nameof(TareasController.CrudTareas),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin tareas relacionadas" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con tareas" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin tareas" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<RegistroEsDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: nameof(ltrDeUnaTarea.VinculosATareas),
                filtrarPor: nameof(ltrDeUnaTarea.VinculosATareas),
                ayuda: "Filtros de tareas"
                ));

            modal.ControlesDeFiltrado.Add(new FiltroDelTipoRelacionado<RegistroEsDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: "tiposDeTareasRelacionados",
                negocioVinculado: enumNegocio.Tarea,
                ayuda: "Filtros de tipos de tareas relacionadas"
                ));
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();
            render = render +
                   $@"<script src=¨../../js/{RutaBase}/RegistrosEs.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeRegistrosEs('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
