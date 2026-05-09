using ModeloDeDto;
using ModeloDeDto.Callejero;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePais : DescriptorDeCrud<PaisDto>
    {
        public DescriptorDePais(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(PaisesController)
               , nameof(PaisesController.CrudPaises)
               , modo
               , enumNameSpaceTs.Callejero)
        {            
            new EditorFiltro<PaisDto>(padre: Mnt.BloqueGeneral
                , etiqueta: "Codigo"
                , propiedad: nameof(PaisDto.Codigo)
                , ayuda: "buscar por codigo"
                , new Posicion { fila = 0, columna = 0 });

            RenombrarEtiqueta("Nombre", "País");

            AnadirOpcionDeDependencias(Mnt
                , controlador: nameof(ProvinciasController)
                , vista: nameof(ProvinciasController.CrudProvincias)
                , datosDependientes: nameof(ProvinciaDto)
                , navegarAlCrud: DescriptorDeMantenimiento<ProvinciaDto>.NombreMnt
                , nombreOpcion: "Provincias"
                , propiedadQueRestringe: nameof(PaisDto.Id)
                , propiedadRestrictora: nameof(ProvinciaDto.IdPais)
                , "Provincias de un pais");
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Paises.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDePaises('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
