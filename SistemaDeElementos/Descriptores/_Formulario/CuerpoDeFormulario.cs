using System;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class CuerpoDeFormulario: IControlHtml
    {
        public DescriptorDeFormulario Formulario { get; }

        public List<BloqueApilado> BloquesApilados = new List<BloqueApilado>();
        public List<BloqueAnexado> BloquesAnexados = new List<BloqueAnexado>();

        public ContenedorDeArchivos CtdDeArchivo
        {
            get
            {
                foreach (var b in BloquesAnexados)
                    foreach (var e in b.Expansores)
                        if (e.CuerpoDelExpansor is ContenedorDeArchivos)
                            return (ContenedorDeArchivos)e.CuerpoDelExpansor;
                throw new Exception("Debe definir un contenedor de archivos como cuerpo de un expansor de archivos");
            }
        }

        public string Id => $"datos-{Formulario.Id}";

        public string IdHtml => Id.ToLower();

        private string CabeceraDeUnBloqueApilado =>
                $@"
                <!-- ****************   Bloque de [Titulo] ************************  -->
                <div id=¨[id-contenedor]¨ class=¨{enumCssFormulario.ContenedorDeBloquesApilados.Render()}¨>
                  <div id=¨[id-contenedor]-expansor¨ class=¨{enumCssFormulario.BloqueExpansor.Render()}¨>
                      <a id=¨mostrar.{IdHtml}.ref¨ 
                         class=¨{Css.Render(enumCssFormulario.referenciaExpansor)}¨
                         href=¨javascript:Formulario.{enumGestorDeEventos.EventosDelFormulario}('{eventosDeFormulario.OcultarMostrarBloque}', '[id-contenedor]-datos');¨>                           
                         bloque: [Titulo]
                      </a>
                      <input id=¨expandir.[id-contenedor]-datos.input¨ type=¨hidden¨ value=¨1¨/>
                  </div>
                  <div id=¨[id-contenedor]-datos¨ class=¨{enumCssFormulario.BloqueDatos.Render()}¨>
                     [RenderBloque]
                  </div>
                </div>";


        public string Etiqueta => Formulario.Etiqueta;

        public IControlHtml Padre => Formulario;

        public int IdNegocio => Formulario.IdNegocio;

        public CuerpoDeFormulario(DescriptorDeFormulario formulario)
        {
            Formulario = formulario;
        }

        public string RenderControl()
        {
            return RenderCuerpo();
        }
        public string RenderCuerpo()
        {
            return BloquesApilados.Count > 0 ? RenderCuerpoConBloquesApilados() : RenderCuerpoConBloquesAnexados();
        }


        public string RenderCuerpoConBloquesApilados()
        {
            var htmlBloques = "";
            foreach (var bloque in BloquesApilados)
            {
                htmlBloques = $"{htmlBloques}{CabeceraDeUnBloqueApilado}"
                    .Replace("[id-contenedor]", bloque.IdHtml)
                    .Replace("[Titulo]", bloque.Titulo)
                    .Replace("[RenderBloque]", bloque.RenderBloqueApilado());
            }

            return htmlBloques;
        }

        public string RenderCuerpoConBloquesAnexados()
        {
            var htmlDeUnContenedor = $@"
                <!-- ****************   Bloque de [Titulo] ************************  -->
                <div id=¨[id-contenedor]¨ class=¨[css]¨ >
                     [RenderBloque]
                </div>";
            var htmlBloques = "";
            var renderDelPrimero = true;
            foreach (var bloque in BloquesAnexados)
            {
                htmlBloques = $"{htmlBloques}{htmlDeUnContenedor.Replace("[ancho]", bloque.Ancho).Replace("[css]",
                       renderDelPrimero ? enumCssFormulario.ContenedorDePrimerBloquesAnexados.Render() + " " + enumCssJerarquia.Contenedor.Render():
                                          enumCssFormulario.ContenedorDeSiguientesBloquesAnexados.Render())}"
                    .Replace("[id-contenedor]", bloque.IdHtml)
                    .Replace("[Titulo]", bloque.Etiqueta)
                    .Replace("[RenderBloque]", bloque.RenderBloqueAnexado());
                renderDelPrimero = false;
            }
            return htmlBloques;
        }

    }
}