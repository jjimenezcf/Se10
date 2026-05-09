using System.Collections.Generic;
using Utilidades;
using ModeloDeDto;
using ModeloDeDto.TrabajosSometidos;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using System.Runtime.Caching;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeExportacion<TElemento> : ControlHtml where TElemento : ElementoDto
    {
        public DescriptorDeCrud<TElemento> Crud => (DescriptorDeCrud<TElemento>)Padre;
        public DescriptorDeMantenimiento<TElemento> Mnt => Crud.Mnt;
        public DescriptorDeExportacion(DescriptorDeCrud<TElemento> crud)
        : base(
          padre: crud,
          id: $"{crud.Id}_{enumTipoControl.pnlExportacion.Render()}",
          etiqueta: "Selección de exportación",
          propiedad: null,
          ayuda: null,
          posicion: null
        )
        {
            Tipo = enumTipoControl.pnlExportacion;
        }

        public string RenderDeExportacion()
        {
            return RenderControl();
        }

        public override string RenderControl()
        {
                var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeExportacion,
                    idHtml: IdHtml
                    , controlador: Crud.Controlador
                    , tituloH2: Etiqueta
                    , cuerpo: cuerpoDeExportacion()
                    , idOpcion: $"{IdHtml}-exportar"
                    , opcion: Crud.NegocioActivo ? "Exportar" : ""
                    , accion: Crud.NegocioActivo ? $"Crud.{enumGestorDeEventos.EventosModalDeExportacion}('{eventosDeExportar.Exportar}','{IdHtml}')" : ""
                    , cerrar: $"Crud.{enumGestorDeEventos.EventosModalDeExportacion}('{eventosDeExportar.Cerrar}','{IdHtml}')"
                    , navegador: ""
                    , claseBoton: enumCssOpcionMenu.DeElemento
                    , permisosNecesarios: enumModoDeAccesoDeDatos.Consultor);

            return htmlModal;
        }

        private string cuerpoDeExportacion()
        {
            var htmlCuerpo = $@"<div id='{IdHtml}_cuerpo_contenedor' class='{enumCssExportacion.Contenedor.Render()}'>
                                     <div id='{IdHtml}_cuerpo_exportacion' class='{enumCssExportacion.plantilla.Render()}'>
                                        {listaDeExportaciones()}
                                        {RenderCheckFormulario($"{IdHtml}_mostradas",  propiedadHtml: "las-mostradas",chequeado: true, etiqueta: "Las mostradas", accion: null, css: enumCssExportacion.mostradas.Render())}
                                     </div>   
                                     <div id='{IdHtml}_cuerpo_destino' class='{enumCssExportacion.destino.Render()}'>
                                        {listaDeCgs()}
                                        {RenderEditor($"{IdHtml}_{ltrParametrosEp.Archivador}", ltrParametrosEp.Archivador, "Indique el nombre del archivador donde almacenar la exportación", new Dictionary<string, string>())}
                                        {RenderTextArea($"{IdHtml}_{ltrParametrosEp.Motivo}", etiqueta: "Motivo de exportación", propiedad: ltrParametrosEp.Motivo, ayuda: "indique el motivo de la exportación", new Dictionary<string, string>())}
                                     </div>
                                </div>";
            return htmlCuerpo;
            /*
            <!--
            <div id=¨{IdHtml}_cuerpo_sometido¨ class=¨{enumCssExportacion.sometido.Render()}¨>
               {checkDeSometido()}
            </div>
            <div id=¨{IdHtml}_cuerpo_enviar¨ class=¨{enumCssExportacion.enviar.Render()}¨>
               {editorDeEMail()}
            </div>
            -->
             */
        }

        private string listaDeExportaciones()
        {
            return RenderListaConEtiquetaEncima($"{IdHtml}_{ltrParametrosEp.IdPlantilla}", 
                elemetoDto: typeof(PlantillaDeExportacionDto), 
                mostrarExpresion: nameof(PlantillaDeExportacionDto.Nombre),
                propiedad: nameof(PlantillaDeExportacionDto.Nombre),
                etiqueta: "Plantilla", 
                controlador: enumControladoresNegocio.PlantillasDeExportacion.ToString(),
                onChange: "javascript:" + nameof(enumNameSpaceTs.ApiDelCrud) + "." + nameof(enumFunctionTs.Expotacion_AlCambiar_Plantilla) + "(this)").Replace("Seleccionar ...", ltrExcelExportador.Estandar);
        }

        private string listaDeCgs()
        {
            return RenderListaConEtiquetaEncima($"{IdHtml}_{ltrParametrosEp.IdCg}",
                elemetoDto: typeof(CentroGestorDto),
                mostrarExpresion: nameof(CentroGestorDto.Expresion),
                propiedad: nameof(ltrParametrosEp.IdCg),
                etiqueta: "Datos para almacenar la exportación",
                controlador: enumControladoresNegocio.PlantillasDeExportacion.ToString(),
                onChange: null).Replace("Seleccionar ...", "Seleccionar CG del archivador");
        }

        private string checkDeSometido()
        {
            var accion = $"onClick = ¨Crud.{enumGestorDeEventos.EventosModalDeExportacion}('{eventosDeExportar.PulsarSometer}')¨";
            return RenderCheckFormulario($"{IdHtml}_sometido", ltrFltCorreosDto.sometido, true, "Someter", accion, enumCssControlesFormulario.Check.Render()) +
                   RenderCheckFormulario($"{IdHtml}_mostradas", "", true, "Las mostradas", accion, enumCssControlesFormulario.Check.Render());
        }

        private string editorDeEMail()
        {
            var otrosAtributosEditor = new Dictionary<string, string>();
            otrosAtributosEditor["estilo"] = "style='padding :0px;'";
            otrosAtributosEditor["onBlur"] = $"onBlur = ¨Crud.{enumGestorDeEventos.EventosModalDeExportacion}('{eventosDeExportar.SalirListaDeCorreos}')¨";
            otrosAtributosEditor["readOnly"] = "Readonly";

            var otrosAtributosEtiqueta = new Dictionary<string, string>();
            otrosAtributosEtiqueta["estilo"] = "style='padding :0px;'";

            return RenderEditorConEtiquetaEncima($"{IdHtml}_correos", "Mensaje", ltrFltCorreosDto.receptores, "Indique los correos de e-mail receptores", otrosAtributosEditor, otrosAtributosEtiqueta);

        }
    }
}
