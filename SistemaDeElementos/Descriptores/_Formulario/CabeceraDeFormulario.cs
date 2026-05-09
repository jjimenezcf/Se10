using System;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class CabeceraDeFormulario: ControlHtml
    {
        DescriptorDeFormulario Formulario => (DescriptorDeFormulario)Padre;

        public List<string> OpcionesIndividuales { get; set; } = new List<string>();

        public MenuDeCabecera Menu { get; private set; }
        public bool RenderizarMenu { get; set; } = true;


        public CabeceraDeFormulario(DescriptorDeFormulario formulario)
            :base(formulario, $"cabecera-{formulario.Id}","","","",null)
        {
            Menu = new MenuDeCabecera(this);
        }

        public string RenderCabecera()
        {
            return RenderControl();
        }

        public override string RenderControl()
        {
            var mf = OpcionesIndividuales.Count > 0
                ? $@"<div id='{IdHtml}.{Formulario.MenuFormulario}' class='{Css.Render(enumCssMnt.MenuFormulario)}' offset-x = 70 menu-flotante='{Formulario.MenuFormulario}'> </div> "
                : "";
            var onkeypress = $"onkeypress = ¨{enumAccionDeFormulario.TeclaPulsada.Render()}¨";
            return $@"
                    <div id='{Menu.IdHtml}-contenedor' class='{enumCssFormulario.CabeceraFormularioMenu.Render()}'>
                       {(!RenderizarMenu ? "" : $@"{Menu.RenderMenu()}")}
                    </div>
                    <div id='menu-filtro-contenedor' class='{enumCssFormulario.CabeceraFormularioOpciones.Render()}' {onkeypress}>
                       {Formulario.Filtro.RenderOpciones()}
                    </div>
                    <div id='menu-filtro-contenedor' class='{enumCssFormulario.CabeceraFormularioFiltro.Render()}'>
                       {mf}
                       {Formulario.Filtro.RenderBotonAbrirModalDeFiltro()}
                    </div>";
        }
    }
}