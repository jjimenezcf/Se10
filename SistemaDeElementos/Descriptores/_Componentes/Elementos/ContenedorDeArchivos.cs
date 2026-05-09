using System.Collections.Generic;
using Utilidades;
using GestorDeElementos;
using ModeloDeDto;
using ModeloDeDto.SistemaDocumental;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;

namespace MVCSistemaDeElementos.Descriptores
{
    public class ContenedorDeArchivos : ControlHtml
    {
        public DescriptorDeExpansor Expansor => (DescriptorDeExpansor)Padre;

        public bool EsPaginaParaConsultar => Padre is DescriptorDePaginaDeConsulta;

        public enumNegocio Negocio { get; }

        public ContenedorDeArchivos(IControlHtml padre, enumNegocio negocio)
        : base(padre: padre, $"contenedor-{padre.Id}", "Datos del archivo", "", "", null)
        {
            Tipo = enumTipoControl.Bloque;
            Negocio = negocio;
        }

        public string RenderContonedorDeArchivos() => RenderControl();

        public override string RenderControl()
        {
            var div = $@"
                <div id=¨{IdHtml}-anexar¨ name=¨contenedor-control¨ class=¨componente-para-anexar¨>
                   [selector]
                   [contenedor]
                </div>
                ";

            var selectorDeArchivos = new SelectorDeArchivos(this, $"{Id}-selector", "seleccione archivos", "seleccione los archivos a anexar", "*");

            var a = new AtributosHtml(
                idHtml: IdHtml,
                propiedad: "",
                tipoDeControl: Tipo,
                visible: true,
                editable: false,
                obligatorio: false,
                ayuda: null,
                valorPorDefecto: null);

            var valores = a.MapearComunes();
            //contenedor-crud_archivadordto_panel-editor-archivos-archivos

            valores["IdHtmlContenedor"] = IdHtml;
            valores["Css"] = enumCssControles.ContenedorDeArchivos.Render();

            var htmlContonedorDeArchivos = PlantillasHtml.Render(PlantillasHtml.DivEnBlanco, valores);

            div = div.Replace("[selector]", selectorDeArchivos.RenderSelector()).Replace("[contenedor]", htmlContonedorDeArchivos);

            return div;
        }

        public static string RenderModalDeVisorDeArchivo(ContextoSe contexto, enumNegocio negocio, ContenedorDeArchivos contenedor)
        {
            var negocioActivo = NegociosDeSe.Activo(negocio);
            var idHtml = $"modal-{contenedor.IdHtml}";

            var eventos = contenedor.Padre.Padre is BloqueAnexado 
                ? $"Formulario.{enumGestorDeEventos.EventosModalDeEdicion}" 
                : $"Crud.{enumGestorDeEventos.EventosModalDeEdicion}";

            //var accionDescargarConGuid = $"Javascript: {enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_DescargarConGuid}('{idHtml}')";

            //var habilitarDescargaConGuid = $"<div class='contenido-modal-pie-derecho'>" +
            //                               $@"<input id='{idHtml}-descargar-con-guid' 
            //                                   type='button' 
            //                                   tipo='{enumTipoControl.Opcion.Render()}'
            //                                   clase='{Css.Render(enumCssOpcionMenu.DeElemento)}' 
            //                                   class='{Css.Render(enumCssOpcionMenu.BotonesDeMenu)}' 
            //                                   permisos-necesarios='{enumModoDeAccesoDeDatos.Gestor.Render()}' 
            //                                   value='Copiar Url' 
            //                                   onclick=¨{accionDescargarConGuid}¨ />" +
            //                               $"</div>";
            Dictionary<string, object> otros = new Dictionary<string, object>();
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeVisorDeArchivos,
                idHtml: idHtml
                , controlador: nameof(ArchivosController)
                , tituloH2: contenedor.Etiqueta
                , cuerpo: DescriptorDeEdicion<ArchivoDto>.RenderContenedorDeEdicionCuerpo(contenedor.Padre.Padre, typeof(ArchivoDto), idHtml, nameof(ArchivosController), null,null)
                        + DescriptorDeEdicion<ArchivoDto>.RenderContenedorDeEdicionPie(idHtml, DescriptorDeTabla.IdHtmlDeTabla(typeof(ArchivoDto).Name, enumModoDeTrabajo.Edicion, postFijo: ""), true)
                , idOpcion: $"{idHtml}-modificar"
                , opcion: negocioActivo ? "Modificar" : ""
                , accion: negocioActivo ? $"{eventos}('{eventosDeEdicion.ModificarElemento}','{idHtml}')" : ""
                , cerrar: $"{eventos}('{eventosDeEdicion.CerrarModal}','{idHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos: otros);

            return htmlModal;
        }

        public static string RenderModalDeFirmaDeArchivo(ContextoSe contexto, enumNegocio negocio, ContenedorDeArchivos contenedor)
        {
            var negocioActivo = NegociosDeSe.Activo(negocio);
            var idHtml = $"modal-{contenedor.IdHtml}-firma";

            var eventos = contenedor.Padre.Padre is BloqueAnexado
                ? $"Formulario.{enumGestorDeEventos.EventosModalDeEdicion}"
                : $"Crud.{enumGestorDeEventos.EventosModalDeEdicion}";

            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros["id-negocio"] = NegociosDeSe.IdNegocio(negocio);
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeFirmaDeArchivos,
                idHtml: idHtml
                , controlador: nameof(ArchivosController)
                , tituloH2: contenedor.Etiqueta
                , cuerpo: DescriptorDeEdicion<CertificadosDisponiblesDto>.RenderContenedorDeEdicionCuerpo(contenedor.Padre.Padre, typeof(CertificadosDisponiblesDto), idHtml, nameof(ArchivosController), null, null)
                , idOpcion: $"{idHtml}-firmar"
                , opcion: negocioActivo ? "Firmar" : ""
                , accion: negocioActivo ? $"Javascript: {enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.FirmarArchivo}('{idHtml}')" : ""
                , cerrar: $"{eventos}('{eventosDeEdicion.CerrarModal}','{idHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos: otros);

            return htmlModal;
        }


        public static string RenderModalDeDatosDeFirma(ContextoSe contexto, enumNegocio negocio, ContenedorDeArchivos contenedor)
        {
            var negocioActivo = NegociosDeSe.Activo(negocio);
            var idHtml = $"modal-{contenedor.IdHtml}-datos-firma";

            var eventos = contenedor.Padre.Padre is BloqueAnexado
                ? $"Formulario.{enumGestorDeEventos.EventosModalDeEdicion}"
                : $"Crud.{enumGestorDeEventos.EventosModalDeEdicion}";

            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros["id-negocio"] = NegociosDeSe.IdNegocio(negocio);
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeDatosDeFirma,
                idHtml: idHtml
                , controlador: nameof(ArchivosController)
                , tituloH2: contenedor.Etiqueta
                , cuerpo: DescriptorDeEdicion<FirmadoDto>.RenderContenedorDeEdicionCuerpo(contenedor.Padre.Padre, typeof(FirmadoDto), idHtml, nameof(ArchivosController), null, null)
                , idOpcion: $"{idHtml}-anular"
                , opcion: negocioActivo ? "Anular Firma" : ""
                , accion: negocioActivo ? $"Javascript: {enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.AnularFirma}('{idHtml}')" : ""
                , cerrar: $"{eventos}('{eventosDeEdicion.CerrarModal}','{idHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Administrador
                , otrosAtributos: otros);

            return htmlModal;
        }

        public static string RenderModalDeBloqueoArchivo(ContextoSe contexto, enumNegocio negocio, ContenedorDeArchivos contenedor)
        {
            var negocioActivo = NegociosDeSe.Activo(negocio);
            var idHtml = $"modal-{contenedor.IdHtml}-datos-bloqueo";

            var eventos = contenedor.Padre.Padre is BloqueAnexado
                ? $"Formulario.{enumGestorDeEventos.EventosModalDeEdicion}"
                : $"Crud.{enumGestorDeEventos.EventosModalDeEdicion}";

            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros["id-negocio"] = NegociosDeSe.IdNegocio(negocio);
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeDatosDeBloqueo,
                idHtml: idHtml
                , controlador: nameof(ArchivosController)
                , tituloH2: contenedor.Etiqueta
                , cuerpo: DescriptorDeEdicion<DatosBloqueoDto>.RenderContenedorDeEdicionCuerpo(contenedor.Padre.Padre, typeof(DatosBloqueoDto), idHtml, nameof(ArchivosController), null, null)
                , idOpcion: $"{idHtml}-bloquear"
                , opcion: negocioActivo ? "Bloquear" : ""
                , accion: negocioActivo ? $"Javascript: {enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_BloquearArchivo}('{idHtml}', true)" : ""
                , cerrar: $"{eventos}('{eventosDeEdicion.CerrarModal}','{idHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos: otros);

            return htmlModal;
        }


        public static string RenderModalDeDesbloqueoArchivo(ContextoSe contexto, enumNegocio negocio, ContenedorDeArchivos contenedor)
        {
            var negocioActivo = NegociosDeSe.Activo(negocio);
            var idHtml = $"modal-{contenedor.IdHtml}-datos-desbloqueo";

            var eventos = contenedor.Padre.Padre is BloqueAnexado
                ? $"Formulario.{enumGestorDeEventos.EventosModalDeEdicion}"
                : $"Crud.{enumGestorDeEventos.EventosModalDeEdicion}";

            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros["id-negocio"] = NegociosDeSe.IdNegocio(negocio);
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeDatosDeDesBloqueo,
                idHtml: idHtml
                , controlador: nameof(ArchivosController)
                , tituloH2: contenedor.Etiqueta
                , cuerpo: DescriptorDeEdicion<DatosDesbloqueoDto>.RenderContenedorDeEdicionCuerpo(contenedor.Padre.Padre, typeof(DatosDesbloqueoDto), idHtml, nameof(ArchivosController), null, null)
                , idOpcion: $"{idHtml}-desbloquear"
                , opcion: negocioActivo ? "Desbloquear" : ""
                , accion: negocioActivo ? $"Javascript: {enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_BloquearArchivo}('{idHtml}', false)" : ""
                , cerrar: $"{eventos}('{eventosDeEdicion.CerrarModal}','{idHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos: otros);

            return htmlModal;
        }

        public static string RenderModalDeSeleccionarDestino(ContextoSe contexto, enumNegocio negocio, ContenedorDeArchivos contenedor)
        {
            var negocioActivo = NegociosDeSe.Activo(negocio);
            var idHtml = $"modal-{contenedor.IdHtml}-seleccionar-destino";

            var eventos = contenedor.Padre.Padre is BloqueAnexado
                ? $"Formulario.{enumGestorDeEventos.EventosModalDeEdicion}"
                : $"Crud.{enumGestorDeEventos.EventosModalDeEdicion}";

            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros["id-negocio"] = NegociosDeSe.IdNegocio(negocio);
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeSeleccionarDestino,
                idHtml: idHtml
                , controlador: nameof(ArchivosController)
                , tituloH2: contenedor.Etiqueta
                , cuerpo: DescriptorDeEdicion<SeleccionarDestinoDto>.RenderContenedorDeEdicionCuerpo(contenedor.Padre.Padre, typeof(SeleccionarDestinoDto), idHtml, nameof(ArchivosController), null, null)
                , idOpcion: $"{idHtml}-aplicar"
                , opcion: negocioActivo ? "Aplicar" : ""
                , accion: negocioActivo ? $"Javascript: {enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_AplicarOperacion}('{idHtml}')" : ""
                , cerrar: $"{eventos}('{eventosDeEdicion.CerrarModal}','{idHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos: otros);

            return htmlModal;
        }

        public static string RenderModalDeDesbloqueoMultiple(ContextoSe contexto, enumNegocio negocio, ContenedorDeArchivos contenedor)
        {
            var negocioActivo = NegociosDeSe.Activo(negocio);
            var idHtml = $"modal-{contenedor.IdHtml}-desbloqueo-multiple";

            var eventos = contenedor.Padre.Padre is BloqueAnexado
                ? $"Formulario.{enumGestorDeEventos.EventosModalDeEdicion}"
                : $"Crud.{enumGestorDeEventos.EventosModalDeEdicion}";

            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros["id-negocio"] = NegociosDeSe.IdNegocio(negocio);
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeDesbloqueoMultiple,
                idHtml: idHtml
                , controlador: nameof(ArchivosController)
                , tituloH2: contenedor.Etiqueta
                , cuerpo: DescriptorDeEdicion<DesbloqueoMultipleDto>.RenderContenedorDeEdicionCuerpo(contenedor.Padre.Padre, typeof(DesbloqueoMultipleDto), idHtml, nameof(ArchivosController), null, null)
                , idOpcion: $"{idHtml}-desbloquear"
                , opcion: negocioActivo ? "Desbloquear" : ""
                , accion: negocioActivo ? $"Javascript: {enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_DesbloqueoMultiple}('{idHtml}')" : ""
                , cerrar: $"{eventos}('{eventosDeEdicion.CerrarModal}','{idHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos: otros);

            return htmlModal;
        }

        public static string RenderModalDeBloqueoMultiple(ContextoSe contexto, enumNegocio negocio, ContenedorDeArchivos contenedor)
        {
            var negocioActivo = NegociosDeSe.Activo(negocio);
            var idHtml = $"modal-{contenedor.IdHtml}-bloqueo-multiple";

            var eventos = contenedor.Padre.Padre is BloqueAnexado
                ? $"Formulario.{enumGestorDeEventos.EventosModalDeEdicion}"
                : $"Crud.{enumGestorDeEventos.EventosModalDeEdicion}";

            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros["id-negocio"] = NegociosDeSe.IdNegocio(negocio);
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeBloqueoMultiple,
                idHtml: idHtml
                , controlador: nameof(ArchivosController)
                , tituloH2: contenedor.Etiqueta
                , cuerpo: DescriptorDeEdicion<BloqueoMultipleDto>.RenderContenedorDeEdicionCuerpo(contenedor.Padre.Padre, typeof(BloqueoMultipleDto), idHtml, nameof(ArchivosController), null, null)
                , idOpcion: $"{idHtml}-bloquear"
                , opcion: negocioActivo ? "Bloquear" : ""
                , accion: negocioActivo ? $"Javascript: {enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_BloqueoMultiple}('{idHtml}')" : ""
                , cerrar: $"{eventos}('{eventosDeEdicion.CerrarModal}','{idHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos: otros);

            return htmlModal;
        }

        public static string RenderModalDeGenerarZip(ContextoSe contexto, enumNegocio negocio, ContenedorDeArchivos contenedor)
        {
            var negocioActivo = NegociosDeSe.Activo(negocio);
            var idHtml = $"modal-{contenedor.IdHtml}-generar-zip";

            var eventos = contenedor.Padre.Padre is BloqueAnexado
                ? $"Formulario.{enumGestorDeEventos.EventosModalDeEdicion}"
                : $"Crud.{enumGestorDeEventos.EventosModalDeEdicion}";

            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros["id-negocio"] = NegociosDeSe.IdNegocio(negocio);
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeGenerarZip,
                idHtml: idHtml
                , controlador: nameof(ArchivosController)
                , tituloH2: contenedor.Etiqueta
                , cuerpo: DescriptorDeEdicion<GenerarZipDto>.RenderContenedorDeEdicionCuerpo(contenedor.Padre.Padre, typeof(GenerarZipDto), idHtml, nameof(ArchivosController), null, null)
                , idOpcion: $"{idHtml}-generar-zip"
                , opcion: negocioActivo ? "Generar" : ""
                , accion: negocioActivo ? $"Javascript: {enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_GenerarZip}('{idHtml}')" : ""
                , cerrar: $"{eventos}('{eventosDeEdicion.CerrarModal}','{idHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor
                , otrosAtributos: otros);

            return htmlModal;
        }
    }
}
// $"Crud.EventosModalDeEdicion('{eventosDeEdicion.ModificarElemento}')"
