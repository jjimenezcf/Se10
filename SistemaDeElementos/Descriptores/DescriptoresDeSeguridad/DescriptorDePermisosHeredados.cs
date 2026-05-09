using ModeloDeDto;
using ModeloDeDto.Seguridad;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePermisosHeredados : DescriptorDeCrud<PermisosDeUnPuestoDto>
    {
        public DescriptorDePermisosHeredados(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(PermisosHeredadosController), nameof(PermisosHeredadosController.CrudPermisosHeredados), modo, enumNameSpaceTs.Seguridad)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<PermisosDeUnPuestoDto>(padre: fltGeneral
                  , etiqueta: "Puesto"
                  , propiedad: nameof(PermisosDeUnPuestoDto.IdPuesto)
                  , ayuda: "buscar por Puesto"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(PuestoDeTrabajoController),
                VistaDondeNavegar = nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo),
                Negocio = enumNegocio.Puesto
            };

            var control = BuscarControlEnFiltro(ltrFiltros.Nombre);
            if (control != null) control.CambiarAtributos("Permiso", nameof(PermisosDeUnPuestoDto.Permiso), "Buscar por 'permiso'");

            Mnt.OrdenacionInicial = @$"{nameof(PermisosDeUnPuestoDto.Permiso)}:{nameof(PermisosHeredadosDtm.Permiso)}.{nameof(PermisosHeredadosDtm.Permiso.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/PermisosDeUnPuesto.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDePermisosDeUnPuesto('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
