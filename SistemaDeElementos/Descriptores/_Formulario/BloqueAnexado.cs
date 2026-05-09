using System;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class BloqueAnexado : IBloqueDeFormulario, IControlHtml, IControlConIdNegocio
    {
        internal List<DescriptorDeExpansor> Expansores = new List<DescriptorDeExpansor>();

        public CuerpoDeFormulario Cuerpo { get; }
        public string Id { get; set; }

        public string IdHtml => $"{Cuerpo.IdHtml}-{Id.ToLower()}";
        public string Etiqueta { get; set; }

        private string _contenidoHtml { get; }

        public string Ancho { get; }

        public IControlHtml Padre => Cuerpo;

        public int IdNegocio => Cuerpo.IdNegocio;


        public BloqueAnexado(CuerpoDeFormulario cuerpo, string id, string etiqueta, string ancho, string contenido)
        {
            Cuerpo = cuerpo;
            Id = id;
            Etiqueta = etiqueta;
            _contenidoHtml = contenido;
            Ancho = ancho;
        }

        public string RenderControl()
        {
            return RenderBloqueAnexado();
        }

        public string RenderBloqueAnexado()
        {
            var html = _contenidoHtml;
            var htmlExpansores = "";
           
            foreach (DescriptorDeExpansor expan in Expansores)
            {
                htmlExpansores = htmlExpansores + Environment.NewLine + expan.RenderExpansor();
            }
            htmlExpansores = htmlExpansores.IsNullOrEmpty() ? "": $"<div>{htmlExpansores}</div>";

            return html.Replace("[divExpansores]", htmlExpansores); ;
        }


        public string RenderModalesDeCreacion()
        {
            var htmlModales = "";

            foreach (DescriptorDeExpansor expan in Expansores)
            {
                foreach (ControlHtml control in expan.ControlesDelPie)
                {
                    if (control is ModalDeCreacionDeRelacion)
                        htmlModales = $"{htmlModales}{Environment.NewLine}{((ModalDeCreacionDeRelacion)control).RendelModalDeCreacionDeRelacion()}";
                    if (control is ModalDeEditarRelacion)
                        htmlModales = $"{htmlModales}{Environment.NewLine}{((ModalDeEditarRelacion)control).RendelModalDeEditarRelacion()}";
                }

            }

            return htmlModales;
        }

    }
}
