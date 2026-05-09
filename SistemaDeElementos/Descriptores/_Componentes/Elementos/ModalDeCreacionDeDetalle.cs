using System;
using System.Collections.Generic;
using Utilidades;
using GestorDeElementos;
using GestoresDeNegocio.Negocio;
using ModeloDeDto;
using ServicioDeDatos;

namespace MVCSistemaDeElementos.Descriptores

{
    public class ModalDeCrearDetalle : ControlHtml
    {
        Type Dto { get; }

        string Controlador { get; }

        private bool NegocioActivo => NegociosDeSe.Activo(NegociosDeSe.NegocioDeUnDto(Dto));

        private GestorDeNegocios GestorDeNegocio => GestorDeNegocios.Gestor(Contexto, Contexto.Mapeador);

        private ContextoSe Contexto;
        public string AccionTrasAbrirModal { get; set; }
        public string AccionControlador { get; set; }

        public enumNameSpaceTs EspacioDeNombre { get; }

        public string TituloDelBotonDeCrear { get; set; }

        public ModalDeCrearDetalle(ContextoSe contexto, DescriptorDeExpansor padre, Type dto, string controlador, Referencia referencia, enumNameSpaceTs espacioDeNombre) :
        base(padre, $"{padre.Id}-{enumTipoDeModal.ModalDeCrearDetalle}", referencia.Etiqueta, null, "", null)
        {
            Dto = dto;
            Controlador = controlador.Replace(ltrEndPoint.Controller, "");
            Tipo = enumTipoControl.ModalDeCrearDetalle;
            Contexto = contexto;
            EspacioDeNombre = espacioDeNombre;
            TituloDelBotonDeCrear = "Crear";
        }

        private string RendelModalDeCrearDetalle()
        {
            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros["grid-de-relacion-asociado"] = ((DescriptorDeExpansor)Padre).IdHtmlGridDeRelacion;
            otros["id-negocio"] = ((DescriptorDeExpansor)Padre).IdNegocio;
            otros[EventosModal.TrasAbrir] = AccionTrasAbrirModal;
            otros["accion-controlador"] = AccionControlador;
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeCrearDetalle,
                idHtml: IdHtml
                , controlador: Controlador
                , tituloH2: Etiqueta
                , cuerpo: DescriptorDeCreacion<ElementoDto>.RenderContenedorDeCreacionCuerpo(Padre, Dto, IdHtml, Controlador)
                        + DescriptorDeCreacion<ElementoDto>.RenderContenedorDeCreacionPie(IdHtml, true, "")
                , idOpcion: $"{IdHtml}-crear"
                , opcion: NegocioActivo ? TituloDelBotonDeCrear : ""
                , accion: NegocioActivo ? $"{EspacioDeNombre}.{enumGestorDeEventos.EventosModalDeRelacion}('{EventosModalDeRelacion.CrearDetalle}','{IdHtml}')" : ""
                , cerrar: $"{EspacioDeNombre}.{enumGestorDeEventos.EventosModalDeRelacion}('{EventosModalDeRelacion.Cerrar}','{IdHtml}')"
                , navegador: DescriptorDeCreacion<ElementoDto>.htmlRenderOpciones(IdHtml, true)
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos: otros);

            return htmlModal;
        }

        public override string RenderControl()
        {
            return RendelModalDeCrearDetalle();
        }
    }
}
