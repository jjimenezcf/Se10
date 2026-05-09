using System.Collections.Generic;
using Utilidades;
using ModeloDeDto;
using System;

namespace MVCSistemaDeElementos.Descriptores

{
    public class ModalDeTotales : ControlHtml
    {
        Type Dto { get; }

        private string Controlador => (string)Padre.LeerPropiedad(nameof(DescriptorDeCrud<ElementoDto>.Controlador));

        public string AccionTrasAbrirModal { get; private set; }

        public ModalDeTotales(IControlHtml padre, Type dto, string accion, string etiqueta) :
        base(padre, $"{padre.Id}-{accion}", etiqueta, null, "", null)
        {
            Dto = dto;
            Tipo = enumTipoControl.ModalDeTotales;

            AccionTrasAbrirModal = $"javascript:{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosModalDeTotales}('{EventosModal.TrasAbrir}','{IdHtml}')";
        }

        public string RendelModalDeTotales()
        {
            return RenderControl();
        }

        public override string RenderControl()
        {
            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros[EventosModal.TrasAbrir] = AccionTrasAbrirModal;
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeTotales,
                idHtml: IdHtml
                , controlador: Controlador
                , tituloH2: Etiqueta
                , cuerpo: DescriptorDeCreacion<ElementoDto>.RenderContenedorDeCreacionCuerpo(Padre, Dto, IdHtml, Controlador)
                        + DescriptorDeCreacion<ElementoDto>.RenderContenedorDeCreacionPie(IdHtml, true, "")
                , idOpcion: null
                , opcion: null
                , accion: null
                , cerrar: $"{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosModalDeTotales}('{EventosModal.AlCerrar}','{IdHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeVista
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor
                , otrosAtributos: otros); ;

            return htmlModal;
        }
    }
}
