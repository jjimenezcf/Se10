using System;
using System.Collections.Generic;
using ModeloDeDto;
using Utilidades;


namespace MVCSistemaDeElementos.Descriptores
{

    public enum enumAccionesModalDeFiltrado { AplicarFiltro, AbrirFiltro, CerrarFiltro, LimpiarFiltro, TeclaPulsada }


    static class AccionesModalDeFiltradoExtension
    {
        public static string Render(this enumAccionesModalDeFiltrado accion, string parametros)
        {
            switch (accion)
            {
                case enumAccionesModalDeFiltrado.AplicarFiltro: return $"javascript:Crud.{enumGestorDeEventos.EventosModalDeFiltrado}('{eventosDeFormulario.AplicarFiltro}', '{parametros}');";
                case enumAccionesModalDeFiltrado.CerrarFiltro: return $"javascript:Crud.{enumGestorDeEventos.EventosModalDeFiltrado}('{eventosDeFormulario.CerrarFiltro}', '{parametros}');";
                case enumAccionesModalDeFiltrado.LimpiarFiltro: return $"javascript:Crud.{enumGestorDeEventos.EventosModalDeFiltrado}('{eventosDeFormulario.LimpiarFiltro}', '{parametros}');";
                case enumAccionesModalDeFiltrado.AbrirFiltro: return $"javascript:Crud.{enumGestorDeEventos.EventosModalDeFiltrado}('{eventosDeFormulario.AbrirFiltro}', '{parametros}');";
                case enumAccionesModalDeFiltrado.TeclaPulsada: return $"javascript:Crud.{enumGestorDeEventos.EventosModalDeFiltrado}('{eventosDeFormulario.TeclaPulsada}', '{parametros}');";
            }

            throw new Exception($"No se ha definido como renderizar el tipo de input {accion}");
        }
    }

    public class ModalDeFiltrado<TElemento> : ControlHtml where TElemento : ElementoDto
    {
        DescriptorDeMantenimiento<TElemento> Mnt => ZonaDeFiltrado.Mnt;
        ZonaDeFiltro<TElemento> ZonaDeFiltrado => (ZonaDeFiltro<TElemento>)Padre;

        public List<ControlFiltroHtml> ControlesDeFiltrado = new List<ControlFiltroHtml>();

        public ModalDeFiltrado(ZonaDeFiltro<TElemento> zonaDeFiltro, string id, string titulo, string ayuda = "")
        : base(zonaDeFiltro, $"modal-filtro-{zonaDeFiltro.Id}.{id}", titulo, "", ayuda.IsNullOrEmpty() ? titulo : ayuda, null)
        {
            Tipo = enumTipoControl.ModalDeFiltrado;
        }
        public string RenderModalDeFiltrado()
        {
            return RenderControl();
        }

        public string RenderReferenciaParaAbrirModalDeFiltrado()
        {
            var referencia = new Referencia(this,this.Id, Etiqueta, enumAccionesModalDeFiltrado.AbrirFiltro.Render(IdHtml), Ayuda, false);
            return referencia.RenderReferencia(new List<enumCssControles> { enumCssControles.ReferenciaAbrirFiltro });
        }

        private string RenderControlesDeFiltrado()
        {
            var html = "";
            foreach (var control in ControlesDeFiltrado)
            {
                html = $"{html}{Environment.NewLine}{control.RenderControl()}";
            }
            return html;
        }

        public override string RenderControl()
        {

            // el intro lo trata ApiPanel.AbrirModal (pulsa Filtrar), aquí se recoge el escape para cerrar
            var onkeydown = $"onkeydown = ¨{enumAccionesModalDeFiltrado.TeclaPulsada.Render(IdHtml)}¨";
            string _htmlMiModal =
                $@"
                 <!--  ******************  Filtro de {Etiqueta}  ********************************* -->
                 <div id=¨{IdHtml}¨ class=¨contenedor-modal¨ tipo=¨{Tipo.Render()}¨ tipomodal = ¨{enumTipoDeModal.ModalDeFiltrado.Render()}¨ zona-filtrado=¨{ZonaDeFiltrado.IdHtml}¨ referencia-de-filtrado=¨{IdHtml}-ref.ref¨ {onkeydown}>
                    <div id=¨{IdHtml}_contenido¨ class=¨{enumCssModal.ContenidoModal.Render()}¨ >
                 	   <div id=¨{IdHtml}_cabecera¨ class=¨{enumCssModal.ContenidoCabecera.Render()}¨>
                          <span class=¨titulo-modal-de-filtrado¨>
                             <svg class=¨icono-modal-de-filtrado¨ viewBox=¨0 0 24 24¨ aria-hidden=¨true¨><path d=¨M3 4h18l-7 8.5V19l-4 2v-8.5z¨/></svg>
                             {Etiqueta}
                          </span>
                          <span class=¨cerrar-modal-de-filtrado¨ title=¨Cerrar¨ onclick=¨{enumAccionesModalDeFiltrado.CerrarFiltro.Render(IdHtml)}¨>&times;</span>
                       </div>
                 	   <div id=¨{IdHtml}_cuerpo¨ class=¨{enumCssModal.ContenidoCuerpo.Render()}¨>
                 	      {RenderControlesDeFiltrado()}
                       </div>
                       <div id=¨{IdHtml}_pie¨ class=¨{enumCssModal.ContenidoPieSoloBotones.Render()}¨>
                          <!-- se cierra con la × de la cabecera o con escape -->
                          <input type=¨text¨ id=¨{IdHtml}_Limpiar¨
                              class=¨{enumCssModal.BotonSecundario.Render()}¨
                              value=¨Limpiar¨
                              title=¨Deja los filtros de esta ventana con su valor por defecto¨
                              clase=¨{ExtensorDeCss.Render(enumCssOpcionMenu.Basico)}¨
                              readonly
                              onclick=¨{enumAccionesModalDeFiltrado.LimpiarFiltro.Render(IdHtml)}¨ />
                          <input type=¨text¨ id=¨{IdHtml}_Aceptar¨ 
                              class=¨{enumCssModal.BotonPrincipal.Render()}¨ 
                              value=¨Filtrar¨ 
                              clase=¨{ExtensorDeCss.Render(enumCssOpcionMenu.Basico)}¨ 
                              readonly 
                              onclick=¨{enumAccionesModalDeFiltrado.AplicarFiltro.Render(IdHtml)}¨/>
                       </div>
                    </div>
                 </div>";

            return _htmlMiModal;
        }

    }
}
