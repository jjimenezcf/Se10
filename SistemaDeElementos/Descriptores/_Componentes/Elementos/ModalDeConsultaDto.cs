using System;
using System.Collections.Generic;
using Utilidades;
using GestorDeElementos;
using GestoresDeNegocio.Negocio;
using ModeloDeDto;
using ServicioDeDatos;

namespace MVCSistemaDeElementos.Descriptores
{


    public class ModalDeConsultaDto : ControlHtml
    {
        Type Dto { get; }

        string Controlador { get; }

        private bool NegocioActivo => NegociosDeSe.Activo(NegociosDeSe.NegocioDeUnDto(Dto));

        private GestorDeNegocios GestorDeNegocio => GestorDeNegocios.Gestor(Contexto, Contexto.Mapeador);

        private ContextoSe Contexto;
       
        public ModalDeConsultaDto(ContextoSe contexto, IControlConIdNegocio padre, Type dto, string controlador, string titulo) :
        base(padre, $"{padre.Id}.{dto.Name}", titulo, null, "", null)
        {
            Dto = dto;
            Controlador = controlador.Replace(ltrEndPoint.Controller, "");
            Tipo = enumTipoControl.ModalDeConsultaDto;
            Contexto = contexto;
        }

        public string RendelModalDeConsultaDto()
        {
            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros["id-negocio"] = ((IControlConIdNegocio)Padre).IdNegocio;
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeConsulta,
                idHtml: IdHtml
                , controlador: Controlador
                , tituloH2: Etiqueta
                , cuerpo: DescriptorDeEdicion<ElementoDto>.RenderContenedorDeEdicionCuerpo(Padre, Dto, IdHtml, Controlador, null,null)
                        + DescriptorDeEdicion<ElementoDto>.RenderContenedorDeEdicionPie(IdHtml, DescriptorDeTabla.IdHtmlDeTabla(Dto.Name, enumModoDeTrabajo.Edicion,postFijo:""), true)
                , idOpcion: $"{IdHtml}-modificar"
                , opcion: ""
                , accion: ""
                , cerrar: $"Crud.{enumGestorDeEventos.EventosModalDeEdicion}('{eventosDeEdicion.CerrarModal}','{IdHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos: otros);

            return htmlModal;
        }

        public override string RenderControl()
        {
            return RendelModalDeConsultaDto();
        }
    }
}
