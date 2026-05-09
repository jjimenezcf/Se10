using ModeloDeDto.Negocio;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeAccionesDeRelacion : DescriptorDeCrud<AccionesDeRelacionDto>
    {
        public DescriptorDeAccionesDeRelacion(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(AccionesDeRelacionController), nameof(AccionesDeRelacionController.CrudDeAccionesDeRelacion)
              , modo
              , enumNameSpaceTs.Negocio)
        {
            var listaNegocio = new ListasDinamicas<AccionesDeRelacionDto>(Mnt.BloqueGeneral,
                 etiqueta: "Negocio",
                 filtrarPor: nameof(AccionesDeRelacionDto.IdNegocio),
                 ayuda: "seleccione el negocio",
                 seleccionarDe: nameof(NegocioDto),
                 buscarPor: nameof(NegocioDto.Nombre),
                 mostrarExpresion: $"[{nameof(NegocioDto.Nombre)}]",
                 criterioDeBusqueda: enumCriteriosDeFiltrado.comienza,
                 posicion: new Posicion(0, 0),
                 controlador: nameof(NegocioController),
                 navegarA: enumVistasNegocio.CrudDeNegocios,
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "");
            listaNegocio.LongitudMinimaParaBuscar = 1;

            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 1), "Acción", "Indique nombre o parte del nombre de la acción");
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Negocio}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/AccionesDeRelacion.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeAccionesDeRelacion('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
