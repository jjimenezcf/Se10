using System;
using System.Collections.Generic;
using Utilidades;
using GestorDeElementos;
using GestoresDeNegocio.Negocio;
using ModeloDeDto;
using ServicioDeDatos;

namespace MVCSistemaDeElementos.Descriptores

{
    public class ModalDeCrearVinculo : ControlHtml
    {
        Type Dto { get; }

        string Controlador { get; }

        private bool NegocioActivo => NegociosDeSe.Activo(NegociosDeSe.NegocioDeUnDto(Dto));

        private GestorDeNegocios GestorDeNegocio => GestorDeNegocios.Gestor(Contexto, Contexto.Mapeador);

        private ContextoSe Contexto;
        public string AccionTrasAbrirModal { get; set; }
        public string Vista { get; set; }

        public ModalDeCrearVinculo(ContextoSe contexto, DescriptorDeExpansor padre, Type dto, string controlador, Referencia referencia) :
        base(padre, $"{padre.Id}-{enumTipoDeModal.ModalDeCrearVinculo}", referencia.Etiqueta, null, "", null)
        {
            Dto = dto;
            Controlador = controlador.Replace(ltrEndPoint.Controller, "");
            Tipo = enumTipoControl.ModalDeCrearVinculo;
            Contexto = contexto;
        }

        public string RendelModalDeCrearVinculo()
        {
            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros["grid-de-relacion-asociado"] = ((DescriptorDeExpansor)Padre).IdHtmlGridDeRelacion;
            otros["id-negocio"] = ((DescriptorDeExpansor)Padre).IdNegocio;
            otros["id-negocio-vinculado"] = NegociosDeSe.LeerNegocioPorDto(Dto).Id;
            if (!AccionTrasAbrirModal.IsNullOrEmpty()) otros[EventosModal.TrasAbrir] = AccionTrasAbrirModal;
            if (!Vista.IsNullOrEmpty()) otros[ltrParametrosEp.Vista] = Vista;
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeCrearVinculo,
                idHtml: IdHtml
                , controlador: Controlador
                , tituloH2: Etiqueta
                , cuerpo: DescriptorDeCreacion<ElementoDto>.RenderContenedorDeCreacionCuerpo(Padre, Dto,IdHtml,Controlador)
                        + DescriptorDeCreacion<ElementoDto>.RenderContenedorDeCreacionPie(IdHtml, true, "")
                , idOpcion: $"{IdHtml}-crear"
                , opcion: NegocioActivo ? "Añadir" : ""
                , accion: NegocioActivo ? $"Crud.{enumGestorDeEventos.EventosModalDeRelacion}('{EventosModalDeRelacion.CrearVinculo}','{IdHtml}')" :""
                , cerrar: $"Crud.{enumGestorDeEventos.EventosModalDeRelacion}('{EventosModalDeRelacion.Cerrar}','{IdHtml}')"
                , navegador: DescriptorDeCreacion<ElementoDto>.htmlRenderOpciones(IdHtml,true)
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos: otros);

            return htmlModal;
        }

        public override string RenderControl()
        {
           return RendelModalDeCrearVinculo();
        }
    }
}
