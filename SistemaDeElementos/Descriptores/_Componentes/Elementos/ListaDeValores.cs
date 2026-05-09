using System;
using System.Collections.Generic;
using Utilidades;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores
{
    public class ListaDeValores<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {

        public string GuardarEn { get; private set; }
        public Dictionary<string,string> Opciones { get; private set; }
        BloqueDeFitro<TElemento> Bloque => (Padre is BloqueDeFitro<TElemento>) ? (Padre as BloqueDeFitro<TElemento>) : null;

        public ListaDeValores(IControlHtml padre, string etiqueta, string ayuda, Dictionary<string,string> opciones, string filtraPor,  Posicion posicion = null)
        : base(
            padre: padre
          , id: $"{padre.Id}_{enumTipoControl.ListaDeValores.Render()}_{filtraPor}"
          , etiqueta: etiqueta
          , propiedad: filtraPor
          , ayuda: ayuda
          , posicion == null ? new Posicion(1,1): posicion
          )
        {
            Tipo = enumTipoControl.ListaDeValores;
            Opciones = opciones;
            if (Bloque != null)
            {
                Bloque.Tabla.Dimension.CambiarDimension(posicion);
                Bloque.AnadirControlEn(this);
            }
        }

        public ListaDeValores(FiltroDelFormulario filtro, string etiqueta, string ayuda, Dictionary<string, string> opciones, string filtraPor)
        : base(
            padre: filtro
          , id: $"{filtro.Id}_{filtraPor}"
          , etiqueta: etiqueta
          , propiedad: filtraPor
          , ayuda: ayuda
          , null
          )
        {
            Tipo = enumTipoControl.ListaDeValores;
            Opciones = opciones;
        }

        public override string RenderControl()
        {
           return RenderListaDeValores();
        }

        public string RenderListaDeValores()
        {
            var a = AtributosHtml.AtributosComunes($"div_{IdHtml}", IdHtml, PropiedadHtml, Tipo, Ayuda);
            var atributos = a.MapearComunes();

            var otraCalse = Padre is ModalDeFiltrado<TElemento> ? enumCssFiltro.ContenedorEnModalDeFiltros.Render() : "";
            atributos["CssContenedor"] = $"{Css.Render(enumCssFiltro.ContenedorListaDeElementos)} {otraCalse}";
            atributos["Css"] = Css.Render(enumCssFiltro.ListaDeElementos);

            var lista = PlantillasHtml.Render(PlantillasHtml.listaDeValoresFlt, atributos);
            var opciones = !Ayuda.IsNullOrEmpty() ? $"<option value='-1'>{Ayuda}</option>" : "";
            foreach (var clave in Opciones.Keys)
            {
                opciones = $"{(opciones.IsNullOrEmpty() ? "" : $"{opciones}{Environment.NewLine}")}<option value='{clave}' {(clave.StartsWith(ltrFiltros.separadorDeValoresEnLista) ? "disabled":"")} >{Opciones[clave]}</option>";
            }

            if (Ayuda.IsNullOrEmpty()) 
                lista = lista.Replace("title=¨[Ayuda]¨", "");

           return lista.Replace("[opcionesDeLaLista]", opciones);
        }

    }
}


#region validaciones de propiedades
//var propiedades = typeof(TElemento).GetProperties();
//var p = propiedades.FirstOrDefault(x => x.Name == filtraPor);
//IUPropiedadAttribute atributos = ElementoDto.ObtenerAtributos(p);

//if (atributos.Etiqueta.IsNullOrEmpty())
//    GestorDeErrores.Emitir($"No ha definido el atributo {nameof(atributos.Etiqueta)} de la propiedad {propiedad}");

//if (atributos.Ayuda.IsNullOrEmpty())
//    GestorDeErrores.Emitir($"No ha definido el atributo {nameof(atributos.Ayuda)} de la propiedad {propiedad}");

//if (atributos.GuardarEn.IsNullOrEmpty())
//    GestorDeErrores.Emitir($"No ha definido el atributo {nameof(atributos.GuardarEn)} de la propiedad {propiedad}");

//if (atributos.SeleccionarDe.IsNullOrEmpty())
//    GestorDeErrores.Emitir($"No ha definido el atributo {nameof(atributos.SeleccionarDe)} de la propiedad {propiedad}");
#endregion

