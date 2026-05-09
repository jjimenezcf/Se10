using ServicioDeDatos.Seguridad;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class NavegarDesdeEdicion : ControlHtml
    {
        string Url { get; set; }
        public string NameEspaceTs { get; }
        public string IdPaginaDestino { get; }

        public NavegarDesdeEdicion(ControlHtml padre, string etiqueta, string ayuda, string nameSpaceTs, string url, string idPagina) 
        : base(padre, $"{padre.Id}-abrir", etiqueta, null, ayuda, null)
        {
            Tipo = enumTipoControl.Opcion;
            Url = url;
            NameEspaceTs = nameSpaceTs;
            IdPaginaDestino = idPagina;
        }

        public override string RenderControl()
        {
            var a = new AtributosHtml();
            a = AtributosHtml.AtributosComunes(
                       idHtmlContenedor: $"{IdHtml}-contenedor",
                       idHtml: IdHtml,
                       propiedad: null,
                       tipoDeControl: Tipo,
                       ayuda: Ayuda);

            a.Etiqueta = Etiqueta;
            a.Url = Url;
            a.NameSpaceTs = NameEspaceTs;
            a.IdPaginaDestino = IdPaginaDestino;

            return RenderAbrirEnPestana(a);
        }

        public static string RenderAbrirEnPestana(AtributosHtml atributos)
        {
            Dictionary<string, object> valores = atributos.MapearComunes();
            valores["CssContenedor"] = Css.Render(enumCssControles.ContenedorEditor);
            valores["Css"] = Css.Render(enumCssControles.BotonComoReferencia);
            valores["PermisosNecesarios"] = enumModoDeAccesoDeDatos.SerAdministrador;
            valores["Accion"] = $"{atributos.NameSpaceTs}.EventosDeExpansores('{eventosDeExpansor.NavegarDesdeEdicion}','{atributos.Url};{atributos.IdPaginaDestino}')";
            valores["claseBoton"] = $"{Css.Render(enumCssOpcionMenu.DeElemento)}";

            var htmlOpcionDeNavegar = PlantillasHtml.Render(PlantillasHtml.opcionNavegar, valores);
            htmlOpcionDeNavegar = htmlOpcionDeNavegar.Replace("[disbled]", "");

            return htmlOpcionDeNavegar;
        }

    }
}
