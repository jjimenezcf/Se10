using System;
using System.Collections.Generic;
using Utilidades;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores

{
    public class ModalParaPedirDatos : ControlHtml
    {
        Type Dto { get; }

        private string Controlador => (string)Padre.LeerPropiedad(nameof(DescriptorDeCrud<ElementoDto>.Controlador));

        public string AccionTrasAbrirModal { get; private set; }

        public ModalParaPedirDatos(IControlHtml padre, Type dto, string accion, string etiqueta) :
        base(padre, $"{padre.Id}-{accion}", etiqueta, null, "", null)
        {
            Dto = dto;
            Tipo = enumTipoControl.ModalParaPedirDatos;

            AccionTrasAbrirModal = $"javascript:{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosModalDePedirDatos}('{EventosModal.TrasAbrir}','{IdHtml}')";
        }

        public string RendelModalParaPedirDatos()
        {
            return RenderControl();
        }

        public override string RenderControl()
        {
            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros[EventosModal.TrasAbrir] = AccionTrasAbrirModal;
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalParaPedirDatos,
                idHtml: IdHtml
                , controlador: Controlador
                , tituloH2: Etiqueta
                , cuerpo: DescriptorDeCreacion<ElementoDto>.RenderContenedorDeCreacionCuerpo(Padre, Dto, IdHtml, Controlador)
                        + DescriptorDeCreacion<ElementoDto>.RenderContenedorDeCreacionPie(IdHtml, true, "")
                , idOpcion: $"{IdHtml}-aceptar"
                , opcion: "Aceptar"
                , accion: $"{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosModalDePedirDatos}('{EventosModal.AlAceptar}','{IdHtml}')"
                , cerrar: $"{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosModalDePedirDatos}('{EventosModal.AlCerrar}','{IdHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos: otros); ;

            return htmlModal;
        }
    }
}
