using System;
using System.Collections.Generic;
using Utilidades;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores
{
    public class Menu<TElemento> : ControlHtml where TElemento : ElementoDto
    {
        public ZonaDeMenu<TElemento> ZonaMenu => (ZonaDeMenu<TElemento>)Padre;
        public ICollection<OpcionDeMenu<TElemento>> OpcionesDeMenu { get; private set; } = new List<OpcionDeMenu<TElemento>>();

        public Menu(ZonaDeMenu<TElemento> padre)
        : base(
          padre: padre,
          id: $"{padre.Id}_{enumTipoControl.Menu.Render()}",
          etiqueta: null,
          propiedad: null,
          ayuda: null,
          posicion: null
        )
        {
            Tipo = enumTipoControl.Menu;
        }

        internal void Add(OpcionDeMenu<TElemento> opcion)
        {
            OpcionesDeMenu.Add(opcion);
        }

        private string RenderOpcionesMenu()
        {
            var htmlMenu = "<div id=¨{idMenu}¨ class='{clase}'>{hmlOpciones}</div>";
            var htmlOpciones = "";
            foreach (OpcionDeMenu<TElemento> opcion in OpcionesDeMenu)
            {
                if (ZonaMenu.EsZonaDeMenuDeMantenimiento && opcion.Accion.TipoDeAccion == eventosDeMnt.EditarElemento &&
                    (!ZonaMenu.Mnt.Crud.NegocioActivo || !ZonaMenu.Mnt.Crud.Editor.Editable))
                    opcion.Etiqueta = "Consultar";

                htmlOpciones = htmlOpciones + opcion.RenderControl() + Environment.NewLine;
            }
            
            return htmlMenu.Replace("{idMenu}", IdHtml).Replace("{clase}", enumCssOpcionMenu.BotonesDeMenu.Render()).Replace("{hmlOpciones}", $"{Environment.NewLine}{htmlOpciones}");
        }

        public override string RenderControl()
        {
            return RenderOpcionesMenu();
        }
    }

}
