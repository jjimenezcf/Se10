using UtilidadesParaIu;
using MVCSistemaDeElementos.Controllers;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePlantillasDeExportacion : DescriptorDeCrud<PlantillaDeExportacionDto>
    {
        public DescriptorDePlantillasDeExportacion(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , controlador: nameof(PlantillasDeExportacionController)
               , vista: $"{nameof(PlantillasDeExportacionController.CrudDePlantillasDeExportacion)}"
               , modo: modo
              , rutaBase: enumNameSpaceTs.Negocio)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<PlantillaDeExportacionDto>(fltGeneral, "Negocio", nameof(PlantillaDeExportacionDto.IdNegocio), "parámetros del negocio", new Posicion(0, 0));

            Mnt.ZonaMenu.ModificarPermisosNecesarios(enumOpcionDeMenu.Nuevo, ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Administrador);
            Mnt.ZonaMenu.ModificarPermisosNecesarios(enumOpcionDeMenu.Editar, ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Administrador);
            Mnt.ZonaMenu.ModificarPermisosNecesarios(enumOpcionDeMenu.Borrar, ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Administrador);
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/PlantillasDeExportacion.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDePlantillasDeExportacion('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
