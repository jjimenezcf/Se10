using Gestor.Errores;
using ModeloDeDto;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class EditorDeFecha : ControlHtml
    {
        public EditorDeFecha(ControlHtml padre, string etiqueta, string propiedad, string ayuda) :
        base(padre: padre, $"{padre.Id}-{propiedad}", etiqueta, propiedad, ayuda, null)
        {
            Tipo = enumTipoControl.SelectorDeFechaHora;
        }


        public override string RenderControl()
        {
            var a = new AtributosHtml(
                idHtml: IdHtml,
                propiedad: PropiedadHtml,
                tipoDeControl: Tipo,
                visible: Visible,
                editable: Editable,
                obligatorio: Obligatorio,
                ayuda: Ayuda,
                valorPorDefecto: null);

            return RenderSelectorDeFechaHora(a);
        }


        public static string RenderSelectorDeFechaHora(AtributosHtml atributos)
        {
            var valores = atributos.MapearComunes();
            valores["CssContenedor"] = Css.Render(enumCssControles.ContenedorFecha);
            valores["Css"] = Css.Render(enumCssControles.SelectorDeFecha);
            valores["CssHora"] = Css.Render(enumCssControles.SelectorDeHora);
            valores["Placeholder"] = atributos.Ayuda;
            valores["ValorPorDefecto"] = atributos.ValorPorDefecto;

            if (!atributos.SelectorHasta.IsNullOrEmpty())
            {
                if (atributos.SelectorHasta.Split(":").Length != 3)
                    GestorDeErrores.Emitir($"El selector de fecha hora {atributos.Propiedad} está mal definido, el atributo {nameof(atributos.SelectorHasta)} debe ser control:x:y");
                var fechaHasta = atributos.SelectorHasta.Split(":")[0];
                var incEnDias = atributos.SelectorHasta.Split(":")[1];
                var incEnHoras = atributos.SelectorHasta.Split(":")[2];

                valores["ActualizarFechaHasta"] = "javascript:" + nameof(enumNameSpaceTs.MapearAlControl) + "." + nameof(enumFunctionTs.ProponerFechaEn) +
                                                  "(this,'" + fechaHasta + "', " + incEnDias + ")";
                valores["ActualizarHoraHasta"] = "javascript:" + nameof(enumNameSpaceTs.MapearAlControl) + "." + nameof(enumFunctionTs.ProponerHoraEn) +
                                                  "(this,'" + fechaHasta + "', " + incEnHoras + ")";
            }

            if (atributos.ConAccion)
            {
                if (atributos.CssBotonAccion == enumCssControles.Nulo || atributos.OnClick.IsNullOrEmpty())
                    GestorDeErrores.Emitir("Para definir un botón con acción ha de indicar el evento onClick"); 
                var boton = $@"<button id='{atributos.IdHtml}.accion' class='{atributos.CssBotonAccion.Render()}' onclick='{atributos.OnClick}'></button>";
                valores[nameof(IUPropiedadAttribute.ConAccion)] = boton;
            }
                        
            var htmSelectorDeFecha = PlantillasHtml.Render(PlantillasHtml.selectorDeFechaHoraDto, valores);

            htmSelectorDeFecha = htmSelectorDeFecha.Replace($"{nameof(IUPropiedadAttribute.OnBlur)}=¨[ActualizarFechaHasta]¨", "");
            htmSelectorDeFecha = htmSelectorDeFecha.Replace($"{nameof(IUPropiedadAttribute.OnBlur)}=¨[ActualizarHoraHasta]¨", "");
            htmSelectorDeFecha = htmSelectorDeFecha.Replace($"[{nameof(IUPropiedadAttribute.ConAccion)}]", "");
            return htmSelectorDeFecha;
        }

    }
}
