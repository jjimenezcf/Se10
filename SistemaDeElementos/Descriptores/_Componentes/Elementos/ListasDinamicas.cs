using System;
using Utilidades;
using ModeloDeDto;
using GestorDeElementos;
using ServicioDeDatos;

namespace MVCSistemaDeElementos.Descriptores
{
    public enum enumAccionDeListaDinamica { Cargar, PerderFoco, ObtenerFoco }


    static class AccionDeListaDinamicaExtension
    {
        public static string Render(this enumAccionDeListaDinamica accion)
        {
            switch (accion)
            {
                case enumAccionDeListaDinamica.Cargar: return "ApiListaDinamica.Cargar(this)";
                case enumAccionDeListaDinamica.PerderFoco: return "ApiListaDinamica.PerderFoco(this)";
                case enumAccionDeListaDinamica.ObtenerFoco: return "ApiListaDinamica.ObtenerFoco(this)";
            }

            throw new Exception($"No se ha definido como renderizar el tipo de input {accion}");
        }
    }
    public class ListasDinamicas<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        public string SeleccionarDe { get; private set; }
        public string MostrarExpresion { get; private set; }
        public int LongitudMinimaParaBuscar { get; set; } = 3;
        public bool AplicarJoin { get; set; } = false;
        public bool SoloEnAlta { get; set; } = false;
        public string FiltrarPor { get; set; }
        public int Cantidad { get; set; } = 10;
        public string RestringidoPor { get; private set; }
        public string ContenidoEn { get; private set; }
        public string Controlador { get; private set; }
        public string AlSeleccionarBlanquearControlDependiente { get; private set; }
        public bool BlanquearAlSalir { get; set; }
        public string NavegarA { get; set; }
        public string OnClick { get; set; }
        public string TrasMapear { get; set; }
        public string ParametrosParaNavegar { get; set; }

        public enumNegocio Negocio { get; set; } = enumNegocio.No_Definido;

        public BloqueDeFitro<TElemento> Bloque => (BloqueDeFitro<TElemento>)Padre;

        public ListasDinamicas(IControlHtml padre
            , string etiqueta
            , string filtrarPor
            , string ayuda
            , string seleccionarDe
            , string buscarPor
            , string mostrarExpresion
            , enumCriteriosDeFiltrado criterioDeBusqueda
            , Posicion posicion
            , string controlador
            , string navegarA
            , string restringirPor
            , string alSeleccionarBlanquearControl
            , string onClick = ""
            , string trasMapear = "")
        : base(
            padre: padre
          , id: $"{padre.Id}_{enumTipoControl.ListaDeElemento.Render()}_{typeof(TElemento).Name}_{filtrarPor}"
          , etiqueta
          , propiedad: filtrarPor
          , ayuda
          , posicion
        )
        {
            IniciarObjeto(filtrarPor, seleccionarDe, buscarPor, mostrarExpresion, criterioDeBusqueda, controlador, restringirPor, alSeleccionarBlanquearControl, navegarA, onClick, trasMapear);
            
            if (padre is BloqueDeFitro<TElemento>)
                ((BloqueDeFitro<TElemento>)padre).AnadirSelectorElemento(this);
        }

        private void IniciarObjeto(string filtrarPor, string seleccionarDe, string buscarPor, string mostrarExpresion, enumCriteriosDeFiltrado criterioDeBusqueda
            , string controlador, string restringirPor, string alSeleccionarBlanquearControlDependiente, string navegarA, string onClick, string trasMapear)
        {
            Tipo = enumTipoControl.ListaDinamica;
            SeleccionarDe = seleccionarDe;
            FiltrarPor = filtrarPor;
            BuscarPor = buscarPor.IsNullOrEmpty() ? ltrFiltros.Nombre : buscarPor;
            MostrarExpresion = mostrarExpresion;
            Criterio = criterioDeBusqueda;
            RestringidoPor = restringirPor;
            Controlador = controlador.Replace(ltrEndPoint.Controller, "");
            AlSeleccionarBlanquearControlDependiente = alSeleccionarBlanquearControlDependiente;
            BlanquearAlSalir = true;
            NavegarA = navegarA;
            OnClick = onClick;
            TrasMapear = trasMapear;
            Negocio = NegociosDeSe.NegocioDeUnDto(typeof(TElemento));
        }

        public override string RenderControl()
        {
            var a = AtributosHtml.AtributosComunes($"div_{IdHtml}", IdHtml, PropiedadHtml, Tipo, Ayuda);
            var valores = a.MapearComunes();
            var otraCalse = Padre is ModalDeFiltrado<TElemento> ? enumCssFiltro.ContenedorEnModalDeFiltros.Render() : "";
            valores["CssContenedor"] = $"{Css.Render(enumCssFiltro.ContenedorListaDinamica)} {otraCalse}";
            valores["Css"] = Css.Render(enumCssFiltro.ListaDinamica);
            valores["ClaseElemento"] = SeleccionarDe;
            valores["MostrarExpresion"] = MostrarExpresion.ToLower();
            valores["BuscarPor"] = BuscarPor;
            valores["Longitud"] = LongitudMinimaParaBuscar;
            valores["AplicarJoin"] = AplicarJoin;
            valores["Cantidad"] = Cantidad;
            valores["CriterioDeFiltro"] = Criterio;
            valores["OnInput"] = enumAccionDeListaDinamica.Cargar.Render();
            valores["OnChange"] = enumAccionDeListaDinamica.PerderFoco.Render();
            valores["OnFocus"] = enumAccionDeListaDinamica.ObtenerFoco.Render();
            valores["Placeholder"] = $"{Ayuda} ({Criterio})";
            valores["RestringidoPor"] = RestringidoPor.IsNullOrEmpty() ? "" : RestringidoPor.ToLower();
            valores["PropiedadRestrictora"] = RestringidoPor.IsNullOrEmpty() ? "" : RestringidoPor.ToLower();
            valores["ContenidoEn"] = (Padre is BloqueDeFitro<TElemento>) ? Bloque.ZonaDeFiltrado.IdHtml : Padre.IdHtml;
            valores["Controlador"] = Controlador;
            valores["BlanquearControlesDependientes"] = AlSeleccionarBlanquearControlDependiente.ToLower();
            valores["BlanquearAlSalir"] = BlanquearAlSalir ? "S" : "N";
            valores[nameof(SoloEnAlta)] = SoloEnAlta;
            valores[nameof(ltrParametrosEp.negocio)] = Negocio.ToNombre();
            valores[nameof(TrasMapear)] = TrasMapear;
            valores[nameof(ParametrosParaNavegar)] = ParametrosParaNavegar;

            valores["Navegador"] = RenderDto.DefinirNavegador(IdHtml, Controlador, NavegarA, Negocio, OnClick);
            var htmlLd = PlantillasHtml.Render(PlantillasHtml.listaDinamicaFlt, valores);

            htmlLd = htmlLd.Replace("ordenar-por=¨[ordenarPor]¨", "");
            htmlLd = htmlLd.Replace("style =¨[Estilos]¨", "");
            htmlLd = htmlLd.Replace($"negocio='{enumNegocio.No_Definido}'", "");
            htmlLd = htmlLd.Replace($"tras-mapear='[TrasMapear]'", "");
            htmlLd = htmlLd.Replace($"parametros-para-navegar='[{nameof(ParametrosParaNavegar)}]'", "");

            if (Ayuda.IsNullOrEmpty()) htmlLd.Replace("title=¨[Ayuda]¨", "");
            return htmlLd;
        }


    }
}


