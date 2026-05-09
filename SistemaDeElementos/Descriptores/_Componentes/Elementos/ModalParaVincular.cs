using System;
using System.Collections.Generic;
using Utilidades;
using GestorDeElementos;
using GestoresDeNegocio.Negocio;
using ModeloDeDto;
using ServicioDeDatos;

namespace MVCSistemaDeElementos.Descriptores

{
    public class ModalParaVincular : ControlHtml
    {
        Type Dto { get; }
        int IdVinculado { get; }

        string Controlador { get; }

        private bool NegocioActivo => NegociosDeSe.Activo(NegociosDeSe.NegocioDeUnDto(Dto));

        private GestorDeNegocios GestorDeNegocio => GestorDeNegocios.Gestor(Contexto, Contexto.Mapeador);

        private ContextoSe Contexto;
        public string AccionTrasAbrirModal { get; set; }

        public ModalParaVincular(ContextoSe contexto, DescriptorDeExpansor padre,int idVinculado, Type dto, string controlador, string etiqueta) :
        base(padre, $"{padre.Id}-{enumTipoDeModal.ModalParaVincular}", etiqueta, null, "", null)
        {
            Dto = dto;
            IdVinculado = idVinculado;
            Controlador = controlador.Replace(ltrEndPoint.Controller, "");
            Tipo = enumTipoControl.ModalParaVincular;
            Contexto = contexto;
        }

        public string RendelModalParaVincular()
        {
            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros["grid-de-relacion-asociado"] = ((DescriptorDeExpansor)Padre).IdHtmlGridDeRelacion;
            otros["id-negocio"] = ((DescriptorDeExpansor)Padre).IdNegocio;
            otros["id-negocio-vinculado"] = IdVinculado;
            otros[EventosModal.TrasAbrir] = AccionTrasAbrirModal;
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalParaVincular,
                idHtml: IdHtml
                , controlador: Controlador
                , tituloH2: Etiqueta
                , cuerpo: DescriptorDeCreacion<ElementoDto>.RenderContenedorDeCreacionCuerpo(Padre, Dto,IdHtml,Controlador)
                        + DescriptorDeCreacion<ElementoDto>.RenderContenedorDeCreacionPie(IdHtml, true, "")
                , idOpcion: $"{IdHtml}-vincular"
                , opcion: NegocioActivo ? "Añadir" : ""
                , accion: NegocioActivo ? $"Crud.{enumGestorDeEventos.EventosModalDeRelacion}('{EventosModalDeRelacion.Vincular}','{IdHtml}')" :""
                , cerrar: $"Crud.{enumGestorDeEventos.EventosModalDeRelacion}('{EventosModalDeRelacion.Cerrar}','{IdHtml}')"
                , navegador: DescriptorDeCreacion<ElementoDto>.htmlRenderOpciones(IdHtml,true)
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos: otros);

            return htmlModal;
        }

        public override string RenderControl()
        {
           return RendelModalParaVincular();
        }
    }
}
