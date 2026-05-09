using System;
using System.Collections.Generic;
using Utilidades;
using GestorDeElementos;
using GestoresDeNegocio.Negocio;
using ModeloDeDto;
using ServicioDeDatos;

namespace MVCSistemaDeElementos.Descriptores
{


    public class ModalDeEditarRelacion : ControlHtml
    {
        Type Dto { get; }

        string Controlador { get; }

        private bool NegocioActivo => NegociosDeSe.Activo(NegociosDeSe.NegocioDeUnDto(Dto));

        private ContextoSe Contexto;

        private bool SoloConsulta;
        public string AccionTrasAbrirModal { get; set; }
        public string AccionTrasModificar { get; set; }
        public string AccionControlador { get; set; }
        public enumNameSpaceTs EspacioDeNombre { get; set; }

        public ModalDeEditarRelacion(ContextoSe contexto, DescriptorDeExpansor padre, Type dto, string controlador, string titulo, bool soloConsulta, enumNameSpaceTs espacioDeNombre) :
        base(padre, padre.GidDeRelacion.idModalParaEditar, titulo, null, "", null)
        {
            Dto = dto;
            Controlador = controlador.Replace(ltrEndPoint.Controller, "");
            Tipo = enumTipoControl.ModalDeEditarRelacion;
            Contexto = contexto;
            SoloConsulta = soloConsulta;
            EspacioDeNombre = espacioDeNombre;
        }

        public string RendelModalDeEditarRelacion()
        {

           var post = Padre.GetType() == typeof(DescriptorDeExpansor) && ((DescriptorDeExpansor)Padre).PostFijoNombreIdDeLaTabla != null ?
                ((DescriptorDeExpansor)Padre).PostFijoNombreIdDeLaTabla.ToString()
                : "";


            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros["grid-de-relacion-asociado"] = ((DescriptorDeExpansor)Padre).IdHtmlGridDeRelacion;
            otros["id-negocio"] = ((DescriptorDeExpansor)Padre).IdNegocio;
            otros[EventosModal.TrasAbrir] = AccionTrasAbrirModal;
            otros[EventosModal.TrasModificar] = AccionTrasModificar;
            otros["solo-consulta"] =SoloConsulta;
            otros["accion-controlador"] = AccionControlador;
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeEditarRelacion,
                idHtml: IdHtml
                , controlador: Controlador
                , tituloH2: Etiqueta
                , cuerpo: DescriptorDeEdicion<ElementoDto>.RenderContenedorDeEdicionCuerpo(Padre, Dto, IdHtml, Controlador, null, null)
                        + DescriptorDeEdicion<ElementoDto>.RenderContenedorDeEdicionPie(IdHtml, DescriptorDeTabla.IdHtmlDeTabla(Dto.Name, enumModoDeTrabajo.Edicion, post),true)
                , idOpcion: $"{IdHtml}-modificar"
                , opcion: SoloConsulta ? "" : NegocioActivo ? "Modificar" : ""
                , accion: SoloConsulta ? "" : NegocioActivo ? $"{EspacioDeNombre}.{enumGestorDeEventos.EventosModalDeRelacion}('{EventosModalDeRelacion.ModificarRelacion}','{IdHtml}')" : ""
                , cerrar: $"{EspacioDeNombre}.{enumGestorDeEventos.EventosModalDeRelacion}('{EventosModalDeRelacion.Cerrar}','{IdHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos: otros);

            return htmlModal;
        }

        public override string RenderControl()
        {
            return RendelModalDeEditarRelacion();
        }
    }
}
