using Utilidades;
using Gestor.Errores;
using ModeloDeDto;
using GestorDeElementos;

namespace MVCSistemaDeElementos.Descriptores
{
    public class ListaDeElemento<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {

        public string GuardarEn { get; private set; }
        public string SeleccionarDe { get; private set; }
        public string MostrarExpresion { get; private set; }
        public string MostrarExpresionHtml => MostrarExpresion.ToLower();
        public string Controlador { get; }
        public string OnChange { get; set; }
        public bool AutoPosicionamiento { get; set; }

        public enumNegocio Negocio => NegociosDeSe.NegocioDeUnDto(typeof(TElemento));



        //string etiqueta, string filtrarPor, string ayuda, string seleccionarDe, string buscarPor, string mostrarExpresion, CriteriosDeFiltrado criterioDeBusqueda, Posicion posicion)

        public ListaDeElemento(IControlHtml padre, string etiqueta, string ayuda, string seleccionarDe, string filtraPor
            , string mostrarExpresion, Posicion posicion, string controlador)
        : base(
            padre: padre
          , id: $"{padre.Id}_{enumTipoControl.ListaDeElemento.Render()}_{filtraPor}"
          , etiqueta: etiqueta
          , filtraPor
          , ayuda: ayuda
          , posicion
          )
        {
            Tipo = enumTipoControl.ListaDeElemento;
            SeleccionarDe = seleccionarDe;
            MostrarExpresion = mostrarExpresion;

            if (padre is BloqueDeFitro<TElemento>)
                ((BloqueDeFitro<TElemento>)padre).AnadirSelectorElemento(this);

            if (controlador.IsNullOrEmpty())
                GestorDeErrores.Emitir($"Debe definir el controlador de la propiedad {Id}");

            Controlador = controlador.Replace(ltrEndPoint.Controller, "");
        }


        public override string RenderControl()
        {
            var a = AtributosHtml.AtributosComunes($"div_{IdHtml}", IdHtml, PropiedadHtml, Tipo, Ayuda);
            var valores = a.MapearComunes();

            var otraCalse = Padre is ModalDeFiltrado<TElemento> ? enumCssFiltro.ContenedorEnModalDeFiltros.Render() : "";
            valores["CssContenedor"] = Css.Render(enumCssFiltro.ContenedorListaDeElementos) + " " + otraCalse;
            valores["Css"] = Css.Render(enumCssFiltro.ListaDeElementos);
            valores["ClaseElemento"] = SeleccionarDe;
            valores[nameof(MostrarExpresion)] = MostrarExpresionHtml;
            valores[nameof(Controlador)] = Controlador;
            valores[ltrParametrosEp.idNegocio] = Negocio != enumNegocio.No_Definido ? Negocio.IdNegocio(): 0;
            valores[nameof(Ayuda)] = Ayuda;
            valores[nameof(OnChange)] = OnChange;

            var htmlLe = PlantillasHtml.Render(PlantillasHtml.listaDeElementosFlt, valores);

            htmlLe = htmlLe.Replace($"onChange=¨[{nameof(OnChange)}]¨", "");
            htmlLe = htmlLe.Replace($"[{nameof(AutoPosicionamiento)}]", $"{(AutoPosicionamiento? $"auto-posicionamiento='true'" :"")}");

            return htmlLe;
        }

    }
}


#region validaciones de propiedades
//var propiedades = typeof(TElemento).GetProperties();
//var p = propiedades.FirstOrDefault(x => x.Name == filtraPor);
//IUPropiedadAttribute atributos = ApiDeAtributos.ObtenerAtributos(p);

//if (atributos.Etiqueta.IsNullOrEmpty())
//    GestorDeErrores.Emitir($"No ha definido el atributo {nameof(atributos.Etiqueta)} de la propiedad {propiedad}");

//if (atributos.Ayuda.IsNullOrEmpty())
//    GestorDeErrores.Emitir($"No ha definido el atributo {nameof(atributos.Ayuda)} de la propiedad {propiedad}");

//if (atributos.GuardarEn.IsNullOrEmpty())
//    GestorDeErrores.Emitir($"No ha definido el atributo {nameof(atributos.GuardarEn)} de la propiedad {propiedad}");

//if (atributos.SeleccionarDe.IsNullOrEmpty())
//    GestorDeErrores.Emitir($"No ha definido el atributo {nameof(atributos.SeleccionarDe)} de la propiedad {propiedad}");
#endregion

