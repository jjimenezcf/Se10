using System;
using System.Collections.Generic;
using Utilidades;
using GestorDeElementos;
using GestoresDeNegocio.Negocio;
using ModeloDeDto;
using ServicioDeDatos;

namespace MVCSistemaDeElementos.Descriptores

{
    public class ModalDeCreacionDeRelacion : ControlHtml
    {
        Type Dto { get; }

        string Controlador { get; }

        private bool NegocioActivo => NegociosDeSe.Activo(NegociosDeSe.NegocioDeUnDto(Dto));

        private GestorDeNegocios GestorDeNegocio => GestorDeNegocios.Gestor(Contexto, Contexto.Mapeador);

        private ContextoSe Contexto;
        public string AccionTrasAbrirModal { get; set; }
        public string AccionTrasCrear { get; set; }
        public string AccionControlador { get; set; }

        public enumNameSpaceTs EspacioDeNombre {get;}
        

        public ModalDeCreacionDeRelacion(ContextoSe contexto, DescriptorDeExpansor padre, Type dto, string controlador, Referencia referencia, enumNameSpaceTs espacioDeNombre) :
        base(padre, $"{padre.Id}-{enumTipoDeModal.ModalDeCrearRelacion}", referencia.Etiqueta, null, "", null)
        {
            Dto = dto;
            Controlador = controlador.Replace(ltrEndPoint.Controller, "");
            Tipo = enumTipoControl.ModalDeCreacionRelacion;
            Contexto = contexto;
            EspacioDeNombre = espacioDeNombre;
        }

        public string RendelModalDeCreacionDeRelacion()
        {
            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros["grid-de-relacion-asociado"] = ((DescriptorDeExpansor)Padre).IdHtmlGridDeRelacion;
            otros["id-negocio"] = ((DescriptorDeExpansor)Padre).IdNegocio;
            otros[EventosModal.TrasAbrir] = AccionTrasAbrirModal;
            otros[EventosModal.TrasAceptar] = AccionTrasCrear;
            otros["accion-controlador"] = AccionControlador;
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeCrearRelacion,
                idHtml: IdHtml
                , controlador: Controlador
                , tituloH2: Etiqueta
                , cuerpo: DescriptorDeCreacion<ElementoDto>.RenderContenedorDeCreacionCuerpo(Padre, Dto,IdHtml,Controlador)
                        + DescriptorDeCreacion<ElementoDto>.RenderContenedorDeCreacionPie(IdHtml, true, "")
                , idOpcion: $"{IdHtml}-crear"
                , opcion: NegocioActivo ? "Crear" : ""
                , accion: NegocioActivo ? $"{EspacioDeNombre}.{enumGestorDeEventos.EventosModalDeRelacion}('{EventosModalDeRelacion.CrearRelacion}','{IdHtml}')" :""
                , cerrar: $"{EspacioDeNombre}.{enumGestorDeEventos.EventosModalDeRelacion}('{EventosModalDeRelacion.Cerrar}','{IdHtml}')"
                , navegador: DescriptorDeCreacion<ElementoDto>.htmlRenderOpciones(IdHtml,true)
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos: otros);

            return htmlModal;
        }

        public override string RenderControl()
        {
           return RendelModalDeCreacionDeRelacion();
        }
    }
}
