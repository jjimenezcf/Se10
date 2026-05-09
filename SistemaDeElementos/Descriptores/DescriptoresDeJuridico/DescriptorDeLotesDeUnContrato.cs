using ModeloDeDto.Juridico;
using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using ModeloDeDto;
using System.Collections.Generic;
using ServicioDeDatos.Juridico;
using Utilidades;
using ModeloDeDto.MaestrosTecnico;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeLotesDeUnContrato : DescriptorDeCrud<LoteDeUnContratoDto>
    {
        public DescriptorDeLotesDeUnContrato(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(LotesDeUnContratoController)
               , nameof(LotesDeUnContratoController.CrudLotes)
               , modo
               , rutaBase: enumNameSpaceTs.Juridico)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<LoteDeUnContratoDto>(padre: fltGeneral
                  , etiqueta: "Contrato"
                  , propiedad: nameof(LoteDeUnContratoDto.IdContrato)
                  , ayuda: "buscar por contrato"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(ContratosController),
                VistaDondeNavegar = nameof(ContratosController.CrudContratos),
                Negocio = enumNegocio.Contrato
            };


            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(UnitariosDeUnLoteController)
                , vista: nameof(UnitariosDeUnLoteController.CrudUnitariosDeUnLote)
                , relacionarCon: nameof(UnitarioDto)
                , navegarAlCrud: DescriptorDeMantenimiento<UnitariosDeUnLoteDto>.NombreMnt
                , nombreOpcion: "Unitarios"
                , propiedadQueRestringe: nameof(LoteDeUnContratoDto.Id)
                , propiedadRestrictora: nameof(UnitariosDeUnLoteDto.IdLote)
                , "Añadir unitarios al lote seleccionado");



            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Lote", nameof(LoteDeUnContratoDto.Nombre), "Buscar por 'lote'");
            Mnt.OrdenacionInicial = @$"{nameof(LoteDeUnContratoDto.Nombre)}:{nameof(LoteDeUnContratoDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";


            var expan = DescriptorDeExpansorUnitarios(Editor);
            Editor.Expanes.Insert(0, expan);
        }
        private DescriptorDeExpansor DescriptorDeExpansorUnitarios(DescriptorDeEdicion<LoteDeUnContratoDto> editor)
        {
            var expansorDeUnitarios = new DescriptorDeExpansor(editor, $"{editor.Id}-unitarios", "Unitarios", mostrarPlegado: true, "unitarios de un lote");

            //Definimos el grid de detalles del cuerpo
            var columnasDeUnitarios = new DescriptorDeColumnas("unitarios");
            columnasDeUnitarios.Add(titulo: "Unitario", propiedad: nameof(UnitariosDeUnLoteDto.Unitario), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeUnitarios.Add(titulo: "Coste", propiedad: nameof(UnitariosDeUnLoteDto.Coste), alineacion: enumAliniacion.derecha);
            columnasDeUnitarios.Add(titulo: "PVP", propiedad: nameof(UnitariosDeUnLoteDto.Venta), alineacion: enumAliniacion.derecha);
            columnasDeUnitarios.Add(titulo: "Id", propiedad: nameof(UnitariosDeUnLoteDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(UnitariosDeUnLoteController)}
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,UnitariosDeUnLoteDtm, UnitariosDeUnLoteDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(UnitariosDeUnLoteDto.IdLote) }
            };

            new GridDeRelacion(expansorDeUnitarios, columnasDeUnitarios, parametros);

            expansorDeUnitarios.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(UnitariosDeUnLoteDto), typeof(UnitariosDeUnLoteController), nameof(UnitariosDeUnLoteDto.IdLote), "Relacionar con un unitario");
            expansorDeUnitarios.DescriptorDeEditarRelaciones(editor.Crud.Contexto, typeof(UnitariosDeUnLoteDto), typeof(UnitariosDeUnLoteController), "Editar el unitario de un lote", false);

            return expansorDeUnitarios;
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/LotesDeUnContrato.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeLotes('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
