using System.Collections.Generic;
using ModeloDeDto;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class Referencia : ControlHtml
    {
        public string Accion { get; }
        public bool AbrirOtraVentana { get; }
        public bool EnConsultaOcultar { get; }
        string ParaQueNavegar { get; }

        enumModoDeAccesoDeDatos? PermisosNecesarios { get; set; }

        public AccionDeGetionarDatosDependientes AccionPost { get; }

        public Referencia(IControlHtml padre, string id, string texto, string accion, string ayuda, bool enConsultaOcultar, enumModoDeAccesoDeDatos? permisosNecesarios = null) :
            this(padre: padre, id, propiedad: "", texto, accion, ayuda, enConsultaOcultar)
        {
            Tipo = enumTipoControl.Referencia;
            Accion = accion.Replace(ltrEndPoint.Controller, "");
            PermisosNecesarios = permisosNecesarios;
        }

        public Referencia(IControlHtml padre, string id, string texto, AccionDeGetionarDatosDependientes accion, bool enConsultaOcultar, string paraQueNavegar) :
            this(padre: padre, id, propiedad: "", texto, accion.UrlDelCrudDeDependientes, accion.Ayuda, enConsultaOcultar)
        {
            Tipo = enumTipoControl.ReferenciaPost;
            ParaQueNavegar = paraQueNavegar;
            AccionPost = accion;
        }

        public Referencia(IControlHtml padre, string id, string propiedad, string texto, string accion, string ayuda, bool enConsultaOcultar) :
            base(padre: padre, $"{id}-ref", texto, propiedad, ayuda, null)
        {
            Tipo = enumTipoControl.Referencia;
            Accion = accion;
            AbrirOtraVentana = Accion == null || !accion.StartsWith("javascript:");
            EnConsultaOcultar = enConsultaOcultar;
        }

        private string RenderRef()
        {
            return RenderControl();
        }

        public override string RenderControl()
        {
            var referencia = AccionPost == null
                ? $@"
                <a id=¨[IdHtml].ref¨
                  class=¨[cssClase]¨
                  propiedad=¨[Propiedad]¨ 
                  tipo = '[Tipo]'
                  permisos-necesarios = '{PermisosNecesarios}'
                  en-consulta-ocultar = [{nameof(IUPropiedadAttribute.EnConsultaOcultar)}]
                  valor-de-defecto = ¨[Accion]¨
                  target = '_blank'
                  href =¨[Accion]¨>
                  [Texto]
                </a>
            "
            : $@"
             <form id='{IdHtml}' action='{AccionPost.UrlDelCrudDeDependientes}' method='post' 
                               class='{enumCssControles.ReferenciaAlineadaAlFinal.Render()}'
                               navegar-al-crud='{AccionPost.NavegarAlCrud}' 
                               paraque-navegar='{ParaQueNavegar}'
                               restrictor='{IdHtml}-restrictor' 
                               orden='{IdHtml}-orden' 
                               solo-mapear-en-el-filtro = false
                               style='text-align-last: end;'>
                  <input id='{IdHtml}-restrictor' type='hidden' name ='restrictor'/>
                  <input id='{IdHtml}-orden' type='hidden' name='orden'/>
                  <input type='button' 
                         tipo='[Tipo]' 
                         id='[IdHtml].ref'
                         class='opcion-menu-de-elemento {enumCssControles.BotonComoReferencia.Render()}'
                         permisos-necesarios='{enumModoDeAccesoDeDatos.Gestor.Render()}' 
                         permite-multi-seleccion='N' 
                         valor-de-defecto = ¨{AccionPost.RenderAccion().Replace("idDeOpcMenu", IdHtml)}¨
                         value='{Etiqueta}' 
                         onclick=¨{AccionPost.RenderAccion().Replace("idDeOpcMenu", IdHtml)}¨
                         title='{Ayuda}' 
                         bloqueada='N'>
             </form>
            ";
            return referencia;
        }

        public string RenderReferencia(List<enumCssControles> css = null)
        {
            var htmlRef = $@"
             <div id = contenedor.[IdHtml].ref class='[CssContenedor]' title='{Ayuda}'>
             {RenderRef()}   
             </div>
            ";
            var valores = new Dictionary<string, object>();

            var listaDeCss = "";
            if (css != null) foreach (var c in css) listaDeCss = $"{listaDeCss} {c.Render()}".Trim();


            valores["CssContenedor"] = enumCssControles.ContenedorMenuDeReferencias.Render();
            valores[nameof(IdHtml)] = IdHtml;
            valores["cssClase"] = $"{enumCssControles.ReferenciaDeMenu.Render()} {listaDeCss}".Trim();
            valores["Accion"] = Accion;
            valores["Texto"] = Etiqueta;
            valores["Propiedad"] = Propiedad;
            valores["Tipo"] = Tipo.Render();
            valores[nameof(IUPropiedadAttribute.EnConsultaOcultar)] = EnConsultaOcultar;

            var h = PlantillasHtml.Render(htmlRef, valores);
            if (h.Contains("propiedad=¨[Propiedad]¨")) h = h.Replace("propiedad=¨[Propiedad]¨", "");

            if (!AbrirOtraVentana) h = h.Replace("target = '_blank'", "");

            return h;
        }

        internal static string AbrirModalDeCrearDetalle(string idModal, enumNameSpaceTs espaciodeNombres)
        {
            return $"javascript:{espaciodeNombres}.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.AbrirModalCrearDetalle}', '{idModal}')";
        }

        internal static string AbrirModalDeCrearVinculo(string idModal)
        {
            return $"javascript:{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.AbrirModalCrearVinculo}', '{idModal}')";
        }

        internal static string AbrirModalParaVincular(string idModal)
        {
            return $"javascript:{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.AbrirModalParaVincular}', '{idModal}')";
        }

        internal static string AbrirModalDeCrearRelacion(string idModal, string propiedadRestrictora, enumNameSpaceTs espaciodeNombres)
        {
            return $"javascript:{espaciodeNombres}.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.AbrirModalCrearRelacion}', '{idModal};{propiedadRestrictora.ToLower()}')";
        }
        internal static string BorrarRelacion(DescriptorDeExpansor expansor, enumNameSpaceTs espaciodeNombres, string accionDeBorrado)
        {
            return $"javascript:{espaciodeNombres}.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.EliminarRelacion}', '{expansor.IdHtmlGridDeRelacion};numeroDeFila{(!accionDeBorrado.IsNullOrEmpty() ? $";{accionDeBorrado}" : "")}')";
        }
        internal static string DarDeAlta(DescriptorDeExpansor expansor)
        {
            return $"javascript:{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.DarDeAlta}', '{expansor.IdHtmlGridDeRelacion};numeroDeFila')";
        }
        internal static string AbrirEditarDto(DescriptorDeExpansor expansor, string propiedadRestrictora, enumNameSpaceTs espaciodeNombres)
        {
            return $"javascript:{espaciodeNombres}.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.AbrirModalEditarRelacion}', '{expansor.IdHtmlGridDeRelacion};{propiedadRestrictora.ToLower()};numeroDeFila')";
        }
        internal static string NavegarAEditar(DescriptorDeExpansor expansor, string pagina, string propiedadRestrictora)
        {
            pagina = pagina.Replace(ltrEndPoint.Controller, "");
            return $"javascript:{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.NavegarAEditar}', '{expansor.IdHtmlGridDeRelacion};{pagina};{propiedadRestrictora.ToLower()};numeroDeFila')";
        }
        internal static string Agenda_AbrirAgenda(DescriptorDeExpansor expansor)
        {
            return $"javascript:{enumNameSpaceTs.ApiDeAgenda}.{enumFunctionTs.Agenda_AbrirAgenda}('{expansor.IdHtmlGridDeRelacion};numeroDeFila')";
        }
        internal static string SisDoc_CarpetasDeUnArchivador(DescriptorDeExpansor expansor)
        {
            return $"javascript:{enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_CarpetasDeUnArchivador}('{expansor.IdHtmlGridDeRelacion};numeroDeFila')";
        }
        internal static string MostrarPropiedad(DescriptorDeExpansor expansor, string propiedad, string titulo)
        {
            return $"javascript:{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.MostrarPropiedad}','{expansor.IdHtmlGridDeRelacion};numeroDeFila;{propiedad};{titulo}')";
        }
    }
}
