using GestorDeElementos;
using ModeloDeDto.Entorno;
using ModeloDeDto.Gastos;
using ModeloDeDto.RegistroEs;
using ModeloDeDto.SistemaDocumental;
using ModeloDeDto.Tarea;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeMiCorreo : DescriptorDeCrud<MiCorreoDto>
    {

        public DescriptorDeMiCorreo(ContextoSe contexto, string renderCache) : base(contexto, renderCache)
        {
        }
        public DescriptorDeMiCorreo(ContextoSe contexto, ModoDescriptor modo, string titulo)
        : base(contexto: contexto, 
              controlador: nameof(MiCorreoController), 
              vista: nameof(MiCorreoController.CrudDeMiCorreo),
              modo: modo, 
              rutaBase: enumNameSpaceTs.Entorno, 
              id: nameof(MiCorreoController.CrudDeMiCorreo), 
              tituloPlural: titulo,
              tituloSingular: titulo)
        {
            var buzones = Contexto.SeleccionarTodos<BuzonDeMiSociedadDtm>(new List<ClausulaDeFiltrado>());
            new ListaDeValores<MiCorreoDto>(Mnt.BloqueGeneral
                , "Buzón"
                , ""
                , buzones.ToDictionary(x => x.Id.ToString(), x => $"{x.Buzon} ({Contexto.SeleccionarPorId<SociedadDtm>(x.IdElemento).NIF})")
                , nameof(MiCorreoDto.Buzon)
                , new Posicion() { fila = 0, columna = 0 });

            Mnt.Filtro.FiltroDeNombre.CambiarAtributos("Correo", nameof(MiCorreoDto.Asunto), "Buscar por asunto(contenido)");
            Mnt.Filtro.FiltroDeNombre.Posicion = new Posicion { columna = 1, fila = 0 };
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(SeleccionarComoArchivarDto), eventosDeMf.MiCorreo_ComoArchivar, "Seleccionar como archivar el correo"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(ArchivadorDto), eventosDeMf.MiCorreo_CrearArchivador, "Archivar correo como archivador"));

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(SeleccionarComoVincularDto), eventosDeMf.MiCorreo_ComoVincular, "Seleccionar como incorporar el correo"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(TareaDto), eventosDeMf.MiCorreo_CrearTarea, "Archivar correo como tarea"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(RegistroEsDto), eventosDeMf.MiCorreo_CrearRegistroEs, "Archivar correo como registro de E/S"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(ImportarFacturaRecDto), eventosDeMf.MiCorreo_CrearFacturaRec, "Archivar correo como factura"));
        }


        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Mnt.Etiqueta}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/MiCorreo.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           Entorno.CrearCrudDeMiCorreo('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '') 
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
