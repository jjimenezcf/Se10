using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.MaestrosTecnico;
using ModeloDeDto.Juridico;
using ModeloDeDto;
using System.Collections.Generic;
using ServicioDeDatos.MaestrosTecnico;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeUnitarios : DescriptorDeCrud<UnitarioDto>
    {
        public DescriptorDeUnitarios(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(UnitariosController)
               , nameof(UnitariosController.CrudUnitarios)
               , modo
               , rutaBase: enumNameSpaceTs.MaestrosTecnico)
        {
            Mnt.OrdenacionInicial = @$"{nameof(UnitarioDto.Referencia)}:{nameof(UnitarioDto.Referencia)}:{enumModoOrdenacion.descendente.Render()}";
            DescriptorDeTarifas();
        }


        private void DescriptorDeTarifas()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-tarifas", "Tarifas", true, "Tarifas de un unitario");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles de la tarifas
            var columnas = new DescriptorDeColumnas("tarifas");
            columnas.Add(titulo: "Proveedor", propiedad: nameof(TarifaDto.Proveedor), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Referencia", propiedad: nameof(TarifaDto.Referencia), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 200);
            columnas.Add(titulo: "Tarifa", propiedad: nameof(TarifaDto.Tarifa), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(TarifaDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "IdProveedor", propiedad: nameof(TarifaDto.IdProveedor), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(TarifaDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(TarifaDtm.Proveedor)}.{nameof(TarifaDtm.Proveedor.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(TarifasController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(TarifasController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(TarifaDto.IdElemento) }
                ,  { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(ProveedoresController)}/{nameof(ProveedoresController.CrudProveedores)}?id={nameof(TarifaDto.IdProveedor)}" }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(TarifaDto), typeof(TarifasController), nameof(TarifaDto.IdElemento), "Añadir tarifa de proveedor");
            //modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.MaestrosTecnico}.{enumFunctionTs.Unitario_InicializarModalParaCrearTarifa}('{modalDeCreacion.IdHtml}')";

            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(TarifaDto), typeof(TarifasController), "Editar tarifa", soloConsulta: false);
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Unitarios.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeUnitarios('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
