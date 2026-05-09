using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ModeloDeDto.SistemaDocumental;
using ModeloDeDto;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.TrabajosSometidos;
using System.Collections.Generic;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Contabilidad;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeArchivadores : DescriptorDeCrud<ArchivadorDto>
    {
        public DescriptorDeArchivadores(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }
        public DescriptorDeArchivadores(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(ArchivadoresController)
               , nameof(ArchivadoresController.CrudArchivadores)
               , modo
               , rutaBase: enumNameSpaceTs.SistemaDocumental)
        {

            Mnt.OrdenacionInicial = @$"{nameof(ArchivadorDto.CreadoEl)}:{nameof(ArchivadorDtm.FechaCreacion)}:{enumModoOrdenacion.descendente.Render()}";

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Archivador", nameof(ArchivadorDto.Nombre), "Buscar por nombre o referencia de archivador");
            //DefinirMf(menuIndividual, Mnt.OpcionesPorElemento);

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(ImportarZipDto), eventosDeMf.Arc_ImportarZip, "Seleccionar ZIP a importar"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(ProcesarFarConIaDto), eventosDeMf.Arc_ProcesarFarConIa, "Procesar archivos"));
            DescriptorDeEdicion<ArchivadorDto>.IncluirMfIndividual(Editor.OpcionesMf, "<hr>");
            Editor.IncluirMfIndividual("Exportar", eventosDeMf.Arc_ExportarArchivador, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);
            Editor.IncluirMfIndividual("Importar Zip", eventosDeMf.Arc_ImportarZip, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);
            Editor.IncluirMfIndividual("Crear facturas", eventosDeMf.Arc_ProcesarFarConIa, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor);

            var idsDeUsuarios = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Usuarios_Con_Permiso_Para_Generar, valorPorDefecto: Literal.Cero).Valor.ToLista<int>();
            if (idsDeUsuarios.Contains(contexto.DatosDeConexion.IdUsuario))
            {
                Editor.IncluirMfIndividual("Descontabilizar", eventosDeMf.Arc_Descontabilizar, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);
            }

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CopiarArchDto), eventosDeMf.Arc_CopiarArc, "Seleccionar archivador a copiar"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Arc_CopiarArc}' accion-menu='{eventosDeMf.Arc_CopiarArc}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>Copiar archivador</li>");
        }

        private void DefinirMf(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<TrabajoDeUsuarioDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<TrabajoDeUsuarioDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.1' accion-menu='{eventosDeMf.Arc_IrACapetas}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Mostrar carpetas</li>");
        }

        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;


            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Archivadores.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeArchivadores('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el crud', error.message);
                         }}
                      </script>
                    ";
            ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice] = render.Render();
            return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
        }


    }
}
