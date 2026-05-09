using UtilidadesParaIu;
using MVCSistemaDeElementos.Controllers;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using Utilidades;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeParametrosDeNegocio : DescriptorDeCrud<ParametroDeNegocioDto>
    {
        public DescriptorDeParametrosDeNegocio(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , controlador: nameof(ParametrosDeNegocioController)
               , vista: $"{nameof(ParametrosDeNegocioController.CrudDeParametrosDeNegocio)}"
               , modo: modo
              , rutaBase: enumNameSpaceTs.Negocio
              , tituloPlural: "Parametros"
              , tituloSingular: "Parametro"
              , eliminarCreacion: contexto.SePuedeParametrizar() == false)
        {

            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<ParametroDeNegocioDto>(fltGeneral, "Negocio", nameof(ParametroDeNegocioDto.IdNegocio), "parámetros del negocio", new Posicion(0, 0));

            Mnt.ZonaMenu.ModificarPermisosNecesarios(enumOpcionDeMenu.Editar, ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Administrador);
            Mnt.PermiteCrear = contexto.SePuedeParametrizar();
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/ParametrosDeNegocio.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeParametrosDeNegocio('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
