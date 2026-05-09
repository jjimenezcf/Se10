using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ModeloDeDto.Terceros;
using ModeloDeDto.Callejero;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeBancos : DescriptorDeCrud<BancoDto>
    {
        public DescriptorDeBancos(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(BancosController)
                 , nameof(BancosController.CrudBancos)
                 , modo
                 , enumNameSpaceTs.Terceros)
        {
            var listaPais = new ListasDinamicas<BancoDto>(Mnt.BloqueGeneral,
                etiqueta: enumNegocio.Pais.Singular(),
                filtrarPor: nameof(BancoDto.IdPais),
                ayuda: $"seleccione el {enumNegocio.Pais.Singular(true)}",
                seleccionarDe: nameof(PaisDto),
                buscarPor: nameof(PaisDto.Nombre),
                mostrarExpresion: $"([{nameof(PaisDto.Codigo)}]) [{nameof(PaisDto.Nombre)}]",
                criterioDeBusqueda: enumCriteriosDeFiltrado.comienza,
                posicion: new Posicion(0, 0),
                controlador: nameof(PaisesController),
                navegarA: enumVistasCallejero.CrudPaises,
                restringirPor: "",
                alSeleccionarBlanquearControl: "");
            listaPais.LongitudMinimaParaBuscar = 1;

            new EditorFiltro<BancoDto>(padre: Mnt.BloqueGeneral
                , etiqueta: "Código"
                , propiedad: nameof(BancoDto.Codigo)
                , ayuda: "buscar por código de banco"
                , new Posicion { fila = 0, columna = 1 });


            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(1, 0), "Banco", "Buscar por nombre de banco");
            Mnt.OrdenacionInicial = @$"{nameof(BancoDto.Pais)}:pais.nombre:{enumModoOrdenacion.ascendente.Render()};
                                       {nameof(BancoDto.Nombre)}:nombre:{enumModoOrdenacion.ascendente.Render()}";

        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Bancos.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeBancos('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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

