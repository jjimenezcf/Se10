using System;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public enum enumAccionDeFormulario { CerrarFormulario, Aceptar , AplicarFiltro, AbrirFiltro, CerrarFiltro, TeclaPulsada }

    public enum enumAccionDeJerarquia { CrearNodo, ModificarNodo, EliminarNodo, MostrarJerarquia, CancelarModificacion }


    static class AccionDeFormularioExtension
    {
        public static string Render(this enumAccionDeFormulario accion)
        {
            switch (accion)
            {
                case enumAccionDeFormulario.CerrarFormulario: return $"javascript:Formulario.{enumGestorDeEventos.EventosDelFormulario}('{eventosDeFormulario.Cerrar}');";
                case enumAccionDeFormulario.Aceptar: return $"javascript:Formulario.{enumGestorDeEventos.EventosDelFormulario}('{eventosDeFormulario.Aceptar}');";
                case enumAccionDeFormulario.AplicarFiltro: return $"javascript:Formulario.{enumGestorDeEventos.EventosDelFormulario}('{eventosDeFormulario.AplicarFiltro}');";
                case enumAccionDeFormulario.CerrarFiltro: return $"javascript:Formulario.{enumGestorDeEventos.EventosDelFormulario}('{eventosDeFormulario.CerrarFiltro}');";
                case enumAccionDeFormulario.AbrirFiltro: return $"javascript:Formulario.{enumGestorDeEventos.EventosDelFormulario}('{eventosDeFormulario.AbrirFiltro}');";
                case enumAccionDeFormulario.TeclaPulsada: return $"javascript:Formulario.{enumGestorDeEventos.EventosDelFormulario}('{eventosDeFormulario.TeclaPulsada}');";
        }

            throw new Exception($"No se ha definido como renderizar el tipo de input {accion}");
        }

        public static string Render(this enumAccionDeJerarquia accion, string ruta)
        {
            switch (accion)
            {
                case enumAccionDeJerarquia.CrearNodo: return $"javascript:{ruta}.{enumGestorDeEventos.EventosDeJerarquia}('{eventosDeJerarquia.CrearNodo}');";
                case enumAccionDeJerarquia.ModificarNodo: return $"javascript:{ruta}.{enumGestorDeEventos.EventosDeJerarquia}('{eventosDeJerarquia.ModificarNodo}');";
                case enumAccionDeJerarquia.EliminarNodo: return $"javascript:{ruta}.{enumGestorDeEventos.EventosDeJerarquia}('{eventosDeJerarquia.EliminarNodo}');";
                case enumAccionDeJerarquia.MostrarJerarquia: return $"javascript:{ruta}.{enumGestorDeEventos.EventosDeJerarquia}('{eventosDeJerarquia.MostarJerarquia}');";
                case enumAccionDeJerarquia.CancelarModificacion: return $"javascript:{ruta}.{enumGestorDeEventos.EventosDeJerarquia}('{eventosDeJerarquia.CancelarModificacion}');";
            }

            throw new Exception($"No se ha definido como renderizar el tipo de input {accion}");
        }
    }

    public class OpcionesCabecera
    {
        MenuDeCabecera Menu { get; }

        public string Id { get; }
        public string Etiqueta { get; }
        public enumAccionDeFormulario Accion { get; }

        public string IdHtml => $"{Menu.IdHtml}-{Id}".ToLower();

        public object Ayuda { get; }

        public OpcionesCabecera(MenuDeCabecera menu, string id, string etiqueta, enumAccionDeFormulario accion, string ayuda)
        {
            Menu = menu;
            Id = id;
            Etiqueta = etiqueta;
            Accion = accion;
            Ayuda = ayuda;

            new OpcionHtml(menu, id, etiqueta, ayuda, accion.Render());

        }

        public string RenderOpcion()
        {           

            return $@"<div id = ¨{IdHtml}¨ class=¨{Css.Render(enumCssControlesFormulario.ContenedorOpcion)}¨>
                        <input id=¨{IdHtml}¨ 
                               type=¨button¨ 
                               class=¨{Css.Render(enumCssOpcionMenu.Basico)}¨ 
                               value=¨{Etiqueta}¨ 
                               onClick=¨{Accion.Render()}¨
                               title=¨{Ayuda}¨/>
                      </div>
                     ";
        }
    }

    public class OpcionesPie
    {
        MenuDePie Menu { get; }

        public string Id { get; }
        public string Etiqueta { get; }
        public Enum Accion { get; }

        public string IdHtml => $"{Id}".ToLower();

        public object Ayuda { get; }

        public OpcionesPie(MenuDePie menu, string id, string etiqueta, enumAccionDeFormulario accion, string ayuda)
        {
            Menu = menu;
            Id = $"{menu.Id}-{id}";
            Etiqueta = etiqueta;
            Accion = accion;
            Ayuda = ayuda;

            new OpcionHtml(menu, id, etiqueta, ayuda, accion.Render());

        }
        public OpcionesPie(MenuDePie menu, string id, string etiqueta, enumAccionDeJerarquia accion, string ayuda)
        {
            Menu = menu;
            Id = $"{menu.Id}-{id}";
            Etiqueta = etiqueta;
            Accion = accion;
            Ayuda = ayuda;

            new OpcionHtml(menu, id, etiqueta, ayuda, accion.Render(Menu.Pie.Formulario.namespaceTs));

        }
        public string RenderOpcion()
        {
            return $@"<div id = ¨{IdHtml}.contenedor¨ class=¨{Css.Render(enumCssControlesFormulario.ContenedorOpcion)}¨>
                        <input id=¨{IdHtml}¨ 
                               type=¨button¨ 
                               Tipo = 'opcion'
                               class=¨{Css.Render(enumCssOpcionMenu.Basico)}¨ 
                               value=¨{Etiqueta}¨ 
                               onClick=¨{(Accion is enumAccionDeFormulario ? ((enumAccionDeFormulario)Accion).Render(): ((enumAccionDeJerarquia)Accion).Render(Menu.Pie.Formulario.namespaceTs))}¨
                               title=¨{Ayuda}¨/>
                      </div>
                     ";
        }
    }
}