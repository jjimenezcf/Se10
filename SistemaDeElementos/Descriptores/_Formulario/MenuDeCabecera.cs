using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class MenuDeCabecera: ControlHtml
    {
        public CabeceraDeFormulario Cabecera { get; }

        public List<OpcionesCabecera> Opciones { get; }

        public MenuDeCabecera(CabeceraDeFormulario cabecera)
            : base(cabecera, $"menu-{cabecera.Id}", "", "", "", null)
        {
            Cabecera = cabecera;
            Opciones = new List<OpcionesCabecera>();
            var opcionCerrar = new OpcionesCabecera(this, "cerrar", "Cerrar", enumAccionDeFormulario.CerrarFormulario, "Cerra sin procesar");
            var opcionAceptar = new OpcionesCabecera(this, "aceptar", "Aceptar", enumAccionDeFormulario.Aceptar, "Cerrar y procesar formulario");
            Opciones.Add(opcionCerrar);
            Opciones.Add(opcionAceptar);
        }

        public string RenderMenu()
        {
            return RenderControl();
        }

        public override string RenderControl()
        {

            var menu = "";
            var i = 0;
            foreach (var opcion in Opciones)
            {
                menu = $@"{menu}{opcion.RenderOpcion()}";
                i++;
            }
            return $@"<div id=¨{IdHtml}¨ class=¨{Css.Render(enumCssControlesFormulario.MenuCabecera)}¨>{menu}</div>";
        }
    }
}