using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Gastos;
using ModeloDeDto;
using System.Collections.Generic;
using ServicioDeDatos.Gastos;
using ModeloDeDto.Terceros;
using ModeloDeDto.Ventas;
using GestorDeElementos;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeRemesasPag : DescriptorDeCrud<RemesaPagDto>
    {
        public DescriptorDeRemesasPag(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }
        public DescriptorDeRemesasPag(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(RemesasPagController)
               , nameof(RemesasPagController.CrudRemesasPag)
               , modo
               , rutaBase: enumNameSpaceTs.Gasto)
        {
            Mnt.OrdenacionInicial = $"{nameof(RemesaPagDto.Referencia)}:{nameof(RemesaPagDto.Referencia)}:{enumModoOrdenacion.descendente.Render()}";

            IncluirFiltros();
            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(PagosDeUnaRemesaController)
                , vista: nameof(PagosDeUnaRemesaController.CrudPagosDeUnaRemesa)
                , relacionarCon: nameof(PagoDto)
                , navegarAlCrud: DescriptorDeMantenimiento<PagoDeUnaRemesaDto>.NombreMnt
                , nombreOpcion: "Pagos"
                , propiedadQueRestringe: nameof(RemesaPagDto.Id)
                , propiedadRestrictora: nameof(PagoDeUnaRemesaDto.IdElemento)
                , "Gestionar los pagos de la remesa"
                , permisos: enumModoDeAccesoDeDatos.Consultor);


            var expansor = DescriptorDePagosDeUnaRemesa(Editor);
            Editor.Expanes.Insert(0, expansor);

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(PagarRemesaDto), eventosDeMf.Rem_Pag_Pagar, "Pagar la remesa"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(RetrocederPagoDto), eventosDeMf.Rem_Pag_RetrodePago, "Retroceder pago"));


            Editor.IncluirMfIndividual("Adelantar pago", eventosDeMf.Rem_Pag_Pagar, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);
            Editor.IncluirMfIndividual("Retroceder pago", eventosDeMf.Rem_Pag_RetrodePago, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);


        }

        private void IncluirFiltros()
        {
            var modalDeDatos = new ModalDeFiltrado<RemesaPagDto>(Mnt.Filtro, "filtrosDeRemesas", "Datos de las remesas");
            Mnt.Filtro.Modales.Add(modalDeDatos);
            FiltroPorPago(modalDeDatos);
            FiltrosPorImporte(modalDeDatos);
            FiltrosPorFechaDeGeneracion(modalDeDatos);
            FiltrosPorFechaDePago(modalDeDatos);
            FiltroPorAcreedor(modalDeDatos);

            var modalDeRelacion = new ModalDeFiltrado<RemesaPagDto>(Mnt.Filtro, "filtrosDeRemesasPor", "Relaciones con las remesas");
            Mnt.Filtro.Modales.Add(modalDeRelacion);
            FiltroPorFacturas(modalDeRelacion);
        }


        private static void FiltroPorFacturas(ModalDeFiltrado<RemesaPagDto> modal)
        {
            var lista = new ListasDinamicas<RemesaPagDto>(modal,
                etiqueta: enumNegocio.FacturaRecibida.Singular(),
                filtrarPor: ltrDeUnaRemesaPag.IdFacturaRec,
                ayuda: "seleccione la factura recibida",
                seleccionarDe: nameof(FacturaRecDto),
                buscarPor: nameof(ltrDeUnaFacturaRec.SelectorDeFacturaRemesada),
                mostrarExpresion: nameof(FacturaRecDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.porReferencia,
                posicion: new Posicion(1, 0),
                controlador: nameof(FacturasRecController),
                navegarA: nameof(FacturasRecController.CrudFacturasRec),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin facturas" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "con facturas" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "sin facturas" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<RemesaPagDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaRemesaPag.FiltroPorFactura,
                filtrarPor: ltrDeUnaRemesaPag.FiltroPorFactura,
                ayuda: "Filtros de presupuesto"
                ));
        }

        private void FiltrosPorFechaDeGeneracion(ModalDeFiltrado<RemesaPagDto> modal)
        {
            var fechas = new FiltroEntreFechas<RemesaPagDto>(modal,
                    etiqueta: "Fec. de generación",
                    propiedad: ltrDeUnaRemesaPag.FiltroPorFechaDeGeneracion,
                    ayuda: "remesas generada entre fechas");

            var entreFechas = new FiltroEntreFechasConCheck<RemesaPagDto>(modal, fechas, nameof(RemesaPagDto.GeneradaEl), "Mostrar", columnas: new List<string> { nameof(RemesaPagDto.GeneradaEl) });
            modal.ControlesDeFiltrado.Add(entreFechas);
        }

        private void FiltrosPorFechaDePago(ModalDeFiltrado<RemesaPagDto> modal)
        {
            var fechas = new FiltroEntreFechas<RemesaPagDto>(modal,
                    etiqueta: "Fec. de pago",
                    propiedad: ltrDeUnaRemesaPag.FiltroPorFechaDePago,
                    ayuda: "remesas que se pagarán entre fechas");

            var entreFechas = new FiltroEntreFechasConCheck<RemesaPagDto>(modal, fechas, nameof(RemesaPagDto.PagarEl), "Mostrar", columnas: new List<string> { nameof(RemesaPagDto.PagarEl) });
            modal.ControlesDeFiltrado.Add(entreFechas);
        }


        private void FiltroPorPago(ModalDeFiltrado<RemesaPagDto> modal)
        {
            modal.ControlesDeFiltrado.Add(new ListasDinamicas<RemesaPagDto>(modal,
                etiqueta: enumNegocio.Pago.Singular(),
                filtrarPor: ltrDeUnaRemesaPag.IdPagoEnRemesa,
                ayuda: $"seleccione el {enumNegocio.Pago.Singular(true)}",
                seleccionarDe: nameof(PagoDto),
                buscarPor: ltrDeUnaRemesaPag.IdPagoEnRemesa,
                mostrarExpresion: nameof(PagoDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(PagosController),
                navegarA: nameof(PagosController.CrudPagos),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 });
        }

        private static void FiltroPorAcreedor(ModalDeFiltrado<RemesaPagDto> modal)
        {
            var lista = new ListasDinamicas<RemesaPagDto>(modal,
                etiqueta: enumNegocio.Interlocutor.Singular(),
                filtrarPor: ltrDeUnaRemesaPag.IdAcreedor,
                ayuda: "seleccione el acreedor",
                seleccionarDe: nameof(InterlocutorDto),
                buscarPor: nameof(ltrDeUnaRemesaPag.IdAcreedor),
                mostrarExpresion: nameof(InterlocutorDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(InterlocutoresController),
                navegarA: nameof(InterlocutoresController.CrudInterlocutores),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrDeUnaRemesaPag.CualquierAcreedor}", "Cualquier acreedor" }
                    , { $"{ltrDeUnaRemesaPag.SoloProveedores}", "Solo proveedores" }
                    , { $"{ltrDeUnaRemesaPag.SoloTrabajadores}", "Solo trabajadores" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<RemesaPagDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaRemesaPag.ClaseDeAcreedor,
                filtrarPor: ltrDeUnaRemesaPag.ClaseDeAcreedor,
                ayuda: "Filtro por acreedor"
                ));
        }

        private void FiltrosPorImporte(ModalDeFiltrado<RemesaPagDto> modal)
        {
            var importes = new FiltroEntreImportes<RemesaPagDto>(modal,
                    etiqueta: "Importe remesa",
                    propiedad: ltrDeUnaRemesaPag.FiltroPorImporte,
                    ayuda: "filtrar por importe de la remesa");
            var entreImportes = new FiltroEntreImportesConCheck<RemesaPagDto>(modal, importes, nameof(RemesaPagDto.ImporteRemesa), "Ocultar", columnas: new List<string> { nameof(RemesaPagDto.ImporteRemesa) });
            modal.ControlesDeFiltrado.Add(entreImportes);
        }


        private DescriptorDeExpansor DescriptorDePagosDeUnaRemesa(DescriptorDeEdicion<RemesaPagDto> editor)
        {
            var expansorDePagos = new DescriptorDeExpansor(editor, $"{editor.Id}-pagos", "Pagos incluidos", mostrarPlegado: true, "pagos de una remesa");

            //Definimos el grid de detalles del cuerpo
            var columnasDePagos = new DescriptorDeColumnas("pagos");
            columnasDePagos.Add(titulo: nameof(PagoDeUnaRemesaDto.Pago));
            columnasDePagos.Add(titulo: nameof(PagoDeUnaRemesaDto.Acreedor));
            columnasDePagos.Add(titulo: "Procesada", nameof(PagoDeUnaRemesaDto.EstaPagado), tamano: 100);
            columnasDePagos.Add(titulo: "Pagar El", propiedad: nameof(PagoDeUnaRemesaDto.PagarEl), formato: enumFormato.Fecha, tamano: 150);
            columnasDePagos.Add(titulo: "Pagado El", propiedad: nameof(PagoDeUnaRemesaDto.PagadoEl), formato: enumFormato.Fecha, tamano: 150);
            columnasDePagos.Add(titulo: "Anulado El", propiedad: nameof(PagoDeUnaRemesaDto.AnuladoEl), formato: enumFormato.Fecha, tamano: 150);
            columnasDePagos.Add(titulo: "Importe", propiedad: nameof(PagoDeUnaRemesaDto.ImportePago), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Moneda);
            columnasDePagos.Add(titulo: nameof(PagoDeUnaRemesaDto.Id), mostrar: false);
            columnasDePagos.Add(titulo: nameof(PagoDeUnaRemesaDto.IdPago), mostrar: false);

            var parametros = new Dictionary<string, object> {
               { nameof(GridDeRelacion.Controlador), typeof(PagosDeUnaRemesaController) },
               { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,PagoDeUnaRemesaDtm, PagoDeUnaRemesaDto>.epLeerElementos)},
               { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PagosController)}/{nameof(PagosController.CrudPagos)}?id={nameof(PagoDeUnaRemesaDtm.IdPago)}"},
               { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PagoDeUnaRemesaDto.IdElemento) }
            };

            new GridDeRelacion(expansorDePagos, columnasDePagos, parametros);

            expansorDePagos.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(PagoDeUnaRemesaDto), typeof(PagosDeUnaRemesaController), nameof(PagoDeUnaRemesaDto.IdElemento), "Incluir en la remesa");
            expansorDePagos.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(PagoDeUnaRemesaDto), typeof(PagosDeUnaRemesaController), "Editar datos", soloConsulta: false);

            return expansorDePagos;
        }
        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();
            render = render +
                   $@"<script src='../../js/{RutaBase}/RemesasPag.js?v={System.DateTime.Now.Ticks}'></script>
                      <script src='../../js/{RutaBase}/ApiDeGastos.js?v={System.DateTime.Now.Ticks}'></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeRemesasPag('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
