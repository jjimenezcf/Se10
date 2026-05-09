using System;
using System.Collections.Generic;
using ModeloDeDto;
using Utilidades;


namespace MVCSistemaDeElementos.Descriptores
{

    public enum enumAccionesModalDeFiltrado { AplicarFiltro, AbrirFiltro, CerrarFiltro, TeclaPulsada }


    static class AccionesModalDeFiltradoExtension
    {
        public static string Render(this enumAccionesModalDeFiltrado accion, string parametros)
        {
            switch (accion)
            {
                case enumAccionesModalDeFiltrado.AplicarFiltro: return $"javascript:Crud.{enumGestorDeEventos.EventosModalDeFiltrado}('{eventosDeFormulario.AplicarFiltro}', '{parametros}');";
                case enumAccionesModalDeFiltrado.CerrarFiltro: return $"javascript:Crud.{enumGestorDeEventos.EventosModalDeFiltrado}('{eventosDeFormulario.CerrarFiltro}', '{parametros}');";
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

            var onkeypress = $"onkeypress = ¨{enumAccionesModalDeFiltrado.TeclaPulsada.Render("")}¨";
            // <div id=¨{IdHtml}_cuerpo¨ class=¨{enumCssModal.ContenidoCuerpo.Render()}¨ {onkeypress} >
            string _htmlMiModal =
                $@"
                 <!--  ******************  Filtro de {Etiqueta}  ********************************* -->
                 <div id=¨{IdHtml}¨ class=¨contenedor-modal¨ tipo=¨{Tipo.Render()}¨ tipomodal = ¨{enumTipoDeModal.ModalDeFiltrado.Render()}¨ zona-filtrado=¨{ZonaDeFiltrado.IdHtml}¨ referencia-de-filtrado=¨{IdHtml}-ref.ref¨>
                    <div id=¨{IdHtml}_contenido¨ class=¨{enumCssModal.ContenidoModal.Render()}¨ >
                 	   <div id=¨{IdHtml}_cabecera¨ class=¨{enumCssModal.ContenidoCabecera.Render()}¨>
                 		    	{Etiqueta}
                       </div>
                 	   <div id=¨{IdHtml}_cuerpo¨ class=¨{enumCssModal.ContenidoCuerpo.Render()}¨>
                 	      {RenderControlesDeFiltrado()}
                       </div>
                       <div id=¨{IdHtml}_pie¨ class=¨{enumCssModal.ContenidoPie.Render()}¨>
                          <input type=¨text¨ id=¨{IdHtml}_Aceptar¨ 
                              class=¨boton-modal {Css.Render(enumCssOpcionMenu.BotonPorDefecto)}¨ 
                              value=¨Filtrar¨ 
                              clase=¨{Css.Render(enumCssOpcionMenu.Basico)}¨ 
                              readonly 
                              onclick=¨{enumAccionesModalDeFiltrado.AplicarFiltro.Render(IdHtml)}¨/>
                          <input type=¨text¨ id=¨{IdHtml}_Cerrar¨  
                              class=¨boton-modal¨ 
                              value=¨Cerrar¨ 
                              clase=¨{Css.Render(enumCssOpcionMenu.Basico)}¨ 
                              readonly 
                              onclick=¨{enumAccionesModalDeFiltrado.CerrarFiltro.Render(IdHtml)}¨ />
                       </div>
                    </div>
                 </div>";

            return _htmlMiModal;
        }

    }
}
