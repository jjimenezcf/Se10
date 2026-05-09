using ModeloDeDto;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{

    public class FiltroDeDireccion<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        FiltroConEditor<TElemento> edtCalle { get; }
        FiltroConEditor<TElemento> edtMunicipio { get; }
        FiltroConEditor<TElemento> edtCp { get; }
        FiltroConEditor<TElemento> edtZona { get; }
        FiltroConEditor<TElemento> edtBarrio { get; }

        //CheckDeMostrarColumna<TElemento> check { get; }
        public FiltroDeDireccion(IControlHtml padre)
        : base(padre, $"{padre.Id}-direccion", "Dirección", "direccion", "Búsqueda por dirección", new Posicion(1, 0))
        {
            edtCalle = new FiltroConEditor<TElemento>(padre, "", ltrDireccion.FiltroPorCalle, $"calle", posicion: null, renderEtiqueta: false);
            edtMunicipio = new FiltroConEditor<TElemento>(padre, "", ltrDireccion.FiltroPorMunicipio, $"Municipio", posicion: null, renderEtiqueta: false);
            edtCp = new FiltroConEditor<TElemento>(padre, "", ltrDireccion.FiltroPorCp, $"Cp", posicion: null, renderEtiqueta: false);
            edtZona = new FiltroConEditor<TElemento>(padre, "", ltrDireccion.FiltroPorZona, $"Zona", posicion: null, renderEtiqueta: false);
            edtBarrio = new FiltroConEditor<TElemento>(padre, "", ltrDireccion.FiltroPorBarrio, $"Barrio", posicion: null, renderEtiqueta: false);


            //check = new CheckDeMostrarColumna<TElemento>(padre, "Mostrar dirección", "Mostrar la dirección como columna", false, nameof(PresupuestoDto.Direcciones));
        }

        public override string RenderControl()
        {
            return RenderFiltroDeDireccion();
        }

        private string RenderFiltroDeDireccion()
        {
            var renderCalle = edtCalle.RenderControl();

            var renderMunicipio = edtMunicipio.RenderControl();
            var renderCp = edtCp.RenderControl();
            var renderZona = edtZona.RenderControl();
            var renderBarrio = edtBarrio.RenderControl();


            return PlantillasHtml.filtroDeDireccion.Replace("[IdHtmlContenedor]", IdHtml + ".contenedor")
                .Replace("[Ayuda]", Ayuda)
                .Replace("[CssContenedorEditores]", $"{enumCssFiltro.ContenedorDireccion.Render()} {enumCssFiltro.ContenedorEnModalDeFiltros.Render()}")
                .Replace("[etiqueta]", RenderEtiqueta())
                .Replace("[edtCalle]", renderCalle)
                .Replace("[edtMunicipio]", renderMunicipio)
                .Replace("[edtCp]", renderCp)
                .Replace("[edtZona]", renderZona)
                .Replace("[edtBarrio]", renderBarrio);
                //.Replace("[checkMostrarDireccion]", check.RenderCheck());
        }
    }
}
