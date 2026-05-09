using ModeloDeDto;
using ModeloDeDto.Negocio;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{

    public class FiltroDelTipoRelacionado<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {

        ListasDinamicas<TElemento> Tipos { get; }
        ListasDinamicas<TElemento> Estados { get; }

        public FiltroDelTipoRelacionado(IControlHtml padre, ListasDinamicas<TElemento> lista, Dictionary<string, string> opciones, string propiedad, enumNegocio negocioVinculado, string ayuda)
        : base(padre, $"{padre.Id}-{propiedad}", "", propiedad, ayuda, new Posicion(1,0))
        {
            Tipo = enumTipoControl.FiltroDeTipos;
            Criterio = enumCriteriosDeFiltrado.deTipos;
            
            Tipos = new ListasDinamicas<TElemento>(padre,
                     etiqueta: "Tipo",
                     filtrarPor: $"{negocioVinculado}-{nameof(IUsaTipo.IdTipo)}",
                     ayuda: $"tipo de {negocioVinculado.Singular(true)}",
                     seleccionarDe: nameof(TipoDeElementoDto),
                     buscarPor: nameof(TipoDeElementoDto.Nombre),
                     mostrarExpresion: $"[{nameof(TipoDeElementoDto.Nombre)}]",
                     criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                     posicion: new Posicion(0, 1),
                     controlador: nameof(TiposDeElementoController),
                     navegarA: enumVistasNegocio.CrudDeTipos(negocioVinculado),
                     restringirPor: "",
                     alSeleccionarBlanquearControl: nameof(IUsaEstado.IdEstado))
            {
                LongitudMinimaParaBuscar = 1,
                Negocio = negocioVinculado
            }; 

            Estados = new ListasDinamicas<TElemento>(padre,
            etiqueta: "Estado",
            filtrarPor: negocioVinculado + "-" + nameof(IUsaEstado.IdEstado),
            ayuda: "con el estado ...",
            seleccionarDe: nameof(EstadoDto),
            buscarPor: nameof(EstadoDto.Nombre),
            mostrarExpresion: $"[{nameof(EstadoDto.Nombre)}]",
            criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
            posicion: new Posicion(1, 2),
            controlador: nameof(EstadosController),
            navegarA: nameof(EstadosController.CrudDeEstados),
            restringirPor: "",
            alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1,
                Negocio = negocioVinculado
            };
        }

        public override string RenderControl()
        {
            return RenderFiltroDeRelaciones();
        }

        private string RenderFiltroDeRelaciones()
        {
            //var renderEtiqueta = $"<div class='{enumCssFiltro.EtiquetaEntreFechas.Render()}'>{Etiqueta}</div>";
            var renderTipos = Tipos.RenderControl();
            var renderEstados = Estados.RenderControl();

            /*
             *$@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group [CssContenedor]' title='[Ayuda]'>
            [tipos]
            [estados]
         </div>
        ";
             * */

            return PlantillasHtml.filtroDelTipoRelacionado.Replace("[IdHtmlContenedor]", IdHtml + ".contenedor")
                .Replace("[Ayuda]", Ayuda)
                .Replace("[CssContenedor]", $"{enumCssFiltro.ContenedorDelTipoRelacionadoModal.Render()} {enumCssFiltro.ContenedorEnModalDeFiltros.Render()}")
                .Replace("[Estados]", renderEstados)
                .Replace("[Tipos]", renderTipos);
        }
    }
}
