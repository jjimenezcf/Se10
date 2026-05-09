using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Ventas;
using ServicioDeDatos.Ventas;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeRemesasFae : DescriptorDeCrud<RemesaFaeDto>
    {
        public DescriptorDeRemesasFae(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(RemesasFaeController)
               , nameof(RemesasFaeController.CrudRemesasFae)
               , modo
               , rutaBase: enumNameSpaceTs.Venta)
        {
            IncluirFiltros();

            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(FacturasEmtDeUnaRemesaController)
                , vista: nameof(FacturasEmtDeUnaRemesaController.CrudFacturasEmtDeUnaRemesa)
                , relacionarCon: nameof(FacturaEmtDto)
                , navegarAlCrud: DescriptorDeMantenimiento<FacturaEmtDeUnaRemesaDto>.NombreMnt
                , nombreOpcion: "Facturas"
                , propiedadQueRestringe: nameof(RemesaFaeDto.Id)
                , propiedadRestrictora: nameof(FacturaEmtDeUnaRemesaDto.IdElemento)
                , "Gestionar las facturas de la remesa"
                , permisos: enumModoDeAccesoDeDatos.Consultor);

            var expansor = DescriptorDeFacturasDeUnaRemesa(Editor);
            Editor.Expanes.Insert(0, expansor);

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CargarRemesaDto), eventosDeMf.Rem_Fae_Cargar, "Cargar la remesa"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(AnularCargoRemesaDto), eventosDeMf.Rem_Fae_AnularCargo, "Anular cargo"));


            Editor.IncluirMfIndividual("Cargar la remesa", eventosDeMf.Rem_Fae_Cargar, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);
            Editor.IncluirMfIndividual("Anular cargo", eventosDeMf.Rem_Fae_AnularCargo, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);


            Mnt.OrdenacionInicial = $"{nameof(RemesaFaeDto.Referencia)}:{nameof(RemesaFaeDto.Referencia)}:{enumModoOrdenacion.descendente.Render()}";
        }

        private void IncluirFiltros()
        {
            var modalDeDatos = new ModalDeFiltrado<RemesaFaeDto>(Mnt.Filtro, "filtrosDeLaFae", "Datos de las remesas");
            Mnt.Filtro.Modales.Add(modalDeDatos);
            FiltroPorFactura(modalDeDatos);
            FiltrosPorImporte(modalDeDatos);
            FiltrosPorFechaDeGeneracion(modalDeDatos);
            FiltrosPorFechaDeVencimiento(modalDeDatos);
            //FiltrosPorImporteCobrado(modalDeDatos);

            //var modalDeRelacion = new ModalDeFiltrado<RemesaFaeDto>(Mnt.Filtro, "filtrosDeRelacionesConFae", "Relaciones con las remesas");
            //Mnt.Filtro.Modales.Add(modalDeRelacion);
            //FiltroPorFacturaEmt(modalDeRelacion);
        }

        private void FiltrosPorImporte(ModalDeFiltrado<RemesaFaeDto> modal)
        {
            var importes = new FiltroEntreImportes<RemesaFaeDto>(modal,
                    etiqueta: "Importe de la remesa",
                    propiedad: ltrDeUnaRemesaFae.FiltroPorImporte,
                    ayuda: "filtrar por rango de importes");
            modal.ControlesDeFiltrado.Add(importes);
        }

        private void FiltrosPorFechaDeGeneracion(ModalDeFiltrado<RemesaFaeDto> modal)
        {
            var fechas = new FiltroEntreFechas<RemesaFaeDto>(modal,
                    etiqueta: "Fec. de generación",
                    propiedad: ltrDeUnaRemesaFae.FiltroPorFechaDeGeneracion,
                    ayuda: "remesas generada entre fechas");

            var entreFechas = new FiltroEntreFechasConCheck<RemesaFaeDto>(modal, fechas, nameof(RemesaFaeDto.GeneradaEl), "Mostrar", columnas: new List<string> { nameof(RemesaFaeDto.GeneradaEl) });
            modal.ControlesDeFiltrado.Add(entreFechas);
        }

        private void FiltrosPorFechaDeVencimiento(ModalDeFiltrado<RemesaFaeDto> modal)
        {
            var fechas = new FiltroEntreFechas<RemesaFaeDto>(modal,
                    etiqueta: "Fec. de cargo",
                    propiedad: ltrDeUnaRemesaFae.FiltroPorFechaDeCargo,
                    ayuda: "remesas que se abonan entre fechas");

            var entreFechas = new FiltroEntreFechasConCheck<RemesaFaeDto>(modal, fechas, nameof(RemesaFaeDto.CargarEl), "Mostrar", columnas: new List<string> { nameof(RemesaFaeDto.CargarEl) });
            modal.ControlesDeFiltrado.Add(entreFechas);
        }

        private void FiltrosPorImporteCobrado(ModalDeFiltrado<RemesaFaeDto> modal)
        {
            var importes = new FiltroEntreImportes<RemesaFaeDto>(modal,
                    etiqueta: "Importe remesa",
                    propiedad: ltrDeUnaRemesaFae.FiltroPorImporteCobrado,
                    ayuda: "filtrar por importe de la remesa");
            var entreImportes = new FiltroEntreImportesConCheck<RemesaFaeDto>(modal, importes, nameof(RemesaFaeDto.ImporteRemesa), "Mostrar", columnas: new List<string> { nameof(RemesaFaeDto.Cobrado), nameof(RemesaFaeDto.Pendiente) });
            modal.ControlesDeFiltrado.Add(entreImportes);
        }


        private void FiltroPorFactura(ModalDeFiltrado<RemesaFaeDto> modal)
        {
            modal.ControlesDeFiltrado.Add(new ListasDinamicas<RemesaFaeDto>(modal,
                etiqueta: enumNegocio.FacturaEmitida.Singular(),
                filtrarPor: ltrDeUnaRemesaFae.IdFacturaEnRemesa,
                ayuda: $"seleccione la {enumNegocio.FacturaEmitida.Singular(true)}",
                seleccionarDe: nameof(FacturaEmtDto),
                buscarPor: ltrDeUnaRemesaFae.IdFacturaEnRemesa,
                mostrarExpresion: "[" + nameof(FacturaEmtDto.NumeroFactura) + "] [" + nameof(FacturaEmtDto.Expresion) + "]",
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(FacturasEmtController),
                navegarA: nameof(FacturasEmtController.CrudFacturasEmt),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 });
        }


        private DescriptorDeExpansor DescriptorDeFacturasDeUnaRemesa(DescriptorDeEdicion<RemesaFaeDto> editor)
        {
            var expansorDeFacturas = new DescriptorDeExpansor(editor, $"{editor.Id}-facturas", "Facturas incluidas", mostrarPlegado: true, "facturas de una remesa");

            //Definimos el grid de detalles del cuerpo
            var columnasDeFacturas = new DescriptorDeColumnas("facturas");
            columnasDeFacturas.Add(titulo: nameof(FacturaEmtDeUnaRemesaDto.Factura));
            columnasDeFacturas.Add(titulo: nameof(FacturaEmtDeUnaRemesaDto.Cliente));
            columnasDeFacturas.Add(titulo: "Abonada", nameof(FacturaEmtDeUnaRemesaDto.EstaCargada), tamano: 100);
            columnasDeFacturas.Add(titulo: "Cargada El", propiedad: nameof(FacturaEmtDeUnaRemesaDto.CargadaEl), formato: enumFormato.Fecha, tamano: 150);
            columnasDeFacturas.Add(titulo: "Devuelta El", propiedad: nameof(FacturaEmtDeUnaRemesaDto.DevueltoEl), formato: enumFormato.Fecha, tamano: 150);
            columnasDeFacturas.Add(titulo: "Dev. Hasta", propiedad: nameof(FacturaEmtDeUnaRemesaDto.FechaMaximaDeDevolucion), formato: enumFormato.Fecha, tamano: 150);
            columnasDeFacturas.Add(titulo: "Importe", propiedad: nameof(FacturaEmtDeUnaRemesaDto.ImporteFactura), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Moneda);
            columnasDeFacturas.Add(titulo: nameof(FacturaEmtDeUnaRemesaDto.Id), mostrar: false);
            columnasDeFacturas.Add(titulo: nameof(FacturaEmtDeUnaRemesaDto.IdFactura), mostrar: false);

            var parametros = new Dictionary<string, object> {
               { nameof(GridDeRelacion.Controlador), typeof(FacturasEmtDeUnaRemesaController) },
               { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,FacturaEmtDeUnaRemesaDtm, FacturaEmtDeUnaRemesaDto>.epLeerElementos)},
               { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(FacturasEmtController)}/{nameof(FacturasEmtController.CrudFacturasEmt)}?id={nameof(FacturaEmtDeUnaRemesaDtm.IdFactura)}"},
               { nameof(GridDeRelacion.PropiedadRestrictora), nameof(FacturaEmtDeUnaRemesaDto.IdElemento) }
            };

            new GridDeRelacion(expansorDeFacturas, columnasDeFacturas, parametros);

            expansorDeFacturas.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(FacturaEmtDeUnaRemesaDto), typeof(FacturasEmtDeUnaRemesaController), nameof(FacturaEmtDeUnaRemesaDto.IdElemento), "Incluir en la remesa");
            expansorDeFacturas.DescriptorDeEditarRelaciones(editor.Crud.Contexto, typeof(FacturaEmtDeUnaRemesaDto), typeof(FacturasEmtDeUnaRemesaController), "Editar la factura de la remesa", false);

            return expansorDeFacturas;
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            //render = render.Replace("Imprimir seleccionado", "Imprimir prefactura");

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/ApiDeRemesasFae.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/RemesasFae.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeRemesasFae('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
