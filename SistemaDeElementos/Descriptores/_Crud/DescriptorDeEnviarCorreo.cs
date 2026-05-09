using System;
using System.Collections.Generic;
using Utilidades;
using ModeloDeDto;
using ModeloDeDto.Entorno;
using ModeloDeDto.Seguridad;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Seguridad;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeEnviarCorreo<TElemento> : ControlHtml where TElemento : ElementoDto
    {
        public DescriptorDeCrud<TElemento> Crud => (DescriptorDeCrud<TElemento>)Padre;
        public DescriptorDeMantenimiento<TElemento> Mnt => Crud.Mnt;

        private ModalParaSeleccionar<UsuarioDto> ModalDeUsuarios { get; }
        private ModalParaSeleccionar<PuestoDto> ModalDePuestos { get; }

        private SelectorEnModal<UsuarioDto> SelectorDeUsuarios { get; }
        private SelectorEnModal<PuestoDto> SelectorDePuestoTr { get; }

        public DescriptorDeEnviarCorreo(DescriptorDeCrud<TElemento> crud)
        : base(
          padre: crud,
          id: $"{crud.Id}_{enumTipoControl.pnlEnviarCorreo.Render()}",
          etiqueta: "Envío de correo",
          propiedad: null,
          ayuda: null,
          posicion: null
        )
        {
            Tipo = enumTipoControl.pnlEnviarCorreo;

            ModalDeUsuarios = new ModalParaSeleccionar<UsuarioDto>(this,
                                         tituloModal: "Seleccionar usuario",
                                         crudModal: new DescriptorDeUsuario(Crud.Contexto, ModoDescriptor.ParaSeleccionar),
                                         propiedadRestrictora: "");

            ModalDePuestos = new ModalParaSeleccionar<PuestoDto>(this,
                                         tituloModal: "Seleccionar puestos de trabajo",
                                         crudModal: new DescriptorDePuestoDeTrabajo(Crud.Contexto, ModoDescriptor.ParaSeleccionar),
                                         propiedadRestrictora: "");


            SelectorDeUsuarios = new SelectorEnModal<UsuarioDto>(this, "selector-usuario", "Usuario", "Seleccione usuarios", "IdsDeUsuarios", nameof(UsuarioDto.Id), nameof(UsuarioDto.NombreCompleto), ModalDeUsuarios);
            SelectorDePuestoTr = new SelectorEnModal<PuestoDto>(this, "selector-puestos", "Puestos", "Seleccione puestos", "IdsDePuestos", nameof(PuestoDto.Id), nameof(UsuarioDto.Nombre), ModalDePuestos);

        }

        public string RenderDeEnvioDeCorreo()
        {
            return RenderControl();
        }

        public override string RenderControl()
        {
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeCorreo,
                idHtml: IdHtml
                , controlador: Crud.Controlador
                , tituloH2: Etiqueta
                , cuerpo: cuerpoDeEnvioDeCorreo()
                , idOpcion: $"{IdHtml}-enviar"
                , opcion: Crud.NegocioActivo ? "Enviar" : ""
                , accion: Crud.NegocioActivo ? $"Crud.{enumGestorDeEventos.EventosModalDeEnviarCorreo}('{eventosDeEnviarCorreo.Enviar}','{IdHtml}')" : ""
                , cerrar: $"Crud.{enumGestorDeEventos.EventosModalDeEnviarCorreo}('{eventosDeEnviarCorreo.Cerrar}','{IdHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: enumModoDeAccesoDeDatos.Consultor);

            return htmlModal;
        }

        private string cuerpoDeEnvioDeCorreo()
        {
            //
            var htmlCuerpo = $@"<div id=¨{IdHtml}_cuerpo_contenedor¨ class=¨{enumCssEnviarCorreo.Contenedor.Render()}¨>
                                     <div id=¨{IdHtml}_cuerpo_enviar_correo¨ class=¨{enumCssEnviarCorreo.cabecera.Render()}¨>
                                        PuestosDeTrabajoReceptores
                                        UsuariosReceptores
                                     </div>
                                     <div id=¨{IdHtml}_cuerpo_sometido¨ class=¨{enumCssEnviarCorreo.cuerpo.Render()}¨>
                                        {RenderEditorAsunto()}
                                        {RenderTextArea($"{IdHtml}_mensaje", "mensaje", "cuerpoMensaje", "indique el mensaje")}
                                     </div>
                                     <div id=¨{IdHtml}_cuerpo_enviar¨ class=¨{enumCssEnviarCorreo.adjuntos.Render()}¨>                                        
                                        {RenderElementos()}
                                     </div>
                                </div>
                                ";

            var htmlUsuariosReceptores = SelectorDeUsuarios.RenderSelectorEnModal();
            var htmlPuestosDeTrabajoReceptores = Crud.Contexto.DatosDeConexion.EsClienteWeb ? "": SelectorDePuestoTr.RenderSelectorEnModal();

            htmlCuerpo = htmlCuerpo.Replace("UsuariosReceptores", htmlUsuariosReceptores);
            htmlCuerpo = htmlCuerpo.Replace("PuestosDeTrabajoReceptores", htmlPuestosDeTrabajoReceptores);
            return htmlCuerpo;
        }

        private string RenderElementos()
        {
            var oaEtiqueta = new Dictionary<string, string> { { "estilo", "style='padding :0px;'" } };
            var oaContenedor = new Dictionary<string, string> { { "estilo", "style='height: calc(1.5em)';'" } };

            var html = RenderDivConEtiquetaParaLinks($"{IdHtml}_elementos", "Elementos", oaEtiqueta, oaContenedor);
            return html;
        }

        private string RenderEditorAsunto()
        {
            var otrosAtributosEditor = new Dictionary<string, string>();
            otrosAtributosEditor["LongitudMaxima"] = "maxlength='255'"; ;
            otrosAtributosEditor["Obligatorio"] = "obligatorio='S'";

            var otrosAtributosEtiqueta = new Dictionary<string, string>();
            otrosAtributosEtiqueta["estilo"] = "style='padding :0px;'";

            return RenderEditorConEtiquetaEncima($"{IdHtml}_asunto", "Mensaje", "asunto", "indique el asunto", otrosAtributosEditor, otrosAtributosEtiqueta);
        }

        internal object RenderDeModalesParaSeleccionarReceptores()
        {
            return SelectorDeUsuarios.RenderModalAsociadaAlSelector() + Environment.NewLine + SelectorDePuestoTr.RenderModalAsociadaAlSelector();
        }


    }
}
