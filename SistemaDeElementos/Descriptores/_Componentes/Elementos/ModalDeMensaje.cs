using ModeloDeDto.SistemaDocumental;
using ModeloDeDto;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public static class ModalDeMensaje
    {
        public static string RenderModal<TElemento>(IControlHtml padre, enumNameSpaceTs nameSpaceTs, string titulo, string parametrosTrasApertura)
        where TElemento: ElementoDto
        {
            var idHtml = $"modal-{enumTipoControl.ModalDeMensaje}-{typeof(TElemento).Name}".ToLower();

            var eventos = $"{nameSpaceTs}.{enumGestorDeEventos.EventosModalDeMensaje}";

            Dictionary<string, object> otros = new Dictionary<string, object>();
            if (!parametrosTrasApertura.IsNullOrEmpty())
                 otros[EventosModal.TrasAbrir] = $"{eventos}('{eventosDeMensaje.TrasAbrirModal}','{idHtml}{$"{Simbolos.separadorDeParametrosJs}{parametrosTrasApertura}"}')";

            var htmlModal = ControlHtml.RenderizarModal(enumTipoDeModal.ModalDeMensaje,
                idHtml: idHtml
                , controlador: ""
                , tituloH2: titulo
                , cuerpo: DescriptorDeEdicion<TElemento>.RenderContenedorDeEdicionCuerpo(padre, typeof(TElemento), idHtml, "", null, null)
                        + DescriptorDeEdicion<TElemento>.RenderContenedorDeEdicionPie(idHtml, DescriptorDeTabla.IdHtmlDeTabla(typeof(TElemento).Name, enumModoDeTrabajo.Edicion, postFijo: ""), true)
                , idOpcion: $""
                , opcion:  ""
                , accion:  ""
                , cerrar: $"{eventos}('{eventosDeMensaje.CerrarModal}','{idHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeVista
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos: otros);

            return htmlModal;
        }
    }
}
