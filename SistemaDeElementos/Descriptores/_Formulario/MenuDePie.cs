using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class MenuDePie: ControlHtml
    {
        public PieDeFormulario Pie { get; }


        public List<OpcionesPie> Opciones { get; }



        public MenuDePie(PieDeFormulario pie)
            : base(pie, $"menu-{pie.Id}", "", "", "", null)
        {
            Pie = pie;
            Opciones = new List<OpcionesPie>();
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
            return $@"<div id=¨{IdHtml}¨ class=¨{Css.Render(enumCssControlesFormulario.MenuPie)}¨>{menu}</div>";
        }


        public static void OpcionesDelMenuDelPie(MenuDePie menu)
        {
            var opcionCrear = new OpcionesPie(menu, "crear", "Crear", enumAccionDeJerarquia.CrearNodo, $"Crear");
            var opcionModificar = new OpcionesPie(menu, "modificar", "Modificar", enumAccionDeJerarquia.ModificarNodo, $"Modificar");
            var opcionEliminar = new OpcionesPie(menu, "eliminar", "Eliminar", enumAccionDeJerarquia.EliminarNodo, $"Eliminar");
            var opcionCancelar = new OpcionesPie(menu, "cancelar", "Cancelar", enumAccionDeJerarquia.CancelarModificacion, "Cancelar modificación");
            menu.Opciones.Add(opcionCancelar);
            menu.Opciones.Add(opcionEliminar);
            menu.Opciones.Add(opcionModificar);
            menu.Opciones.Add(opcionCrear);
        }
    }
}