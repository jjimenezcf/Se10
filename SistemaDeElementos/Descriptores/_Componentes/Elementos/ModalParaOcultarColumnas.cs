using System.Collections.Generic;
using Utilidades;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores

{
    public class ModalParaOcultarColumnas : ControlHtml
    {

        private string Controlador => (string)Padre.LeerPropiedad(nameof(DescriptorDeCrud<ElementoDto>.Controlador));

        public string AccionTrasAbrirModal { get; private set; }

        public ModalParaOcultarColumnas(IControlHtml padre, string accion, string etiqueta) :
        base(padre, $"{padre.Id}-{accion}", etiqueta, null, "", null)
        {
            Tipo = enumTipoControl.ModalParaOcultarColumnas;

            AccionTrasAbrirModal = $"javascript:{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosModalDeOcultarColumnas}('{EventosModal.TrasAbrir}','{IdHtml}')";
        }

        public string RendelModalParaOcultarColumnas()
        {
            return RenderControl();
        }

        public override string RenderControl()
        {
            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros[EventosModal.TrasAbrir] = AccionTrasAbrirModal;
            otros[EventosModal.TituloOpcionCerrar] = enumOpcionDeMenu.Resetear.ToString();
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalParaOcultarColumnas,
                idHtml: IdHtml
                , controlador: Controlador
                , tituloH2: Etiqueta
                , cuerpo: RenderContenedorDeOcultarColumnas(IdHtml)
                        + DescriptorDeCreacion<ElementoDto>.RenderContenedorDeCreacionPie(IdHtml, true, "")
                , idOpcion: $"{IdHtml}-guardar"
                , opcion: "Guardar"
                , accion: $"{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosModalDeOcultarColumnas}('{EventosModal.AlAceptar}','{IdHtml}')"
                , cerrar: $"{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosModalDeOcultarColumnas}('{EventosModal.AlCerrar}','{IdHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeVista
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor
                , otrosAtributos: otros); ;

            return htmlModal;
        }


        private static string RenderContenedorDeOcultarColumnas(string idHtml)
        {
            var idHtmlCuerpoDeCreacion = $"contenedor_creacion_cuerpo_{idHtml}";
            var htmlModal = $@"<div id=¨{idHtmlCuerpoDeCreacion}¨ class=¨{enumCssEdicion.ContenedorDeEdicionCuerpo.Render()}¨>

                               </div>";
            return htmlModal;
        }

    }
}
