using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ModeloDeDto.Juridico;
using ModeloDeDto;
using System.Collections.Generic;
using GestorDeElementos;
using ServicioDeDatos.Elemento;
using ModeloDeDto.RegistroEs;
using ServicioDeDatos.RegistroEs;
using ModeloDeDto.Tarea;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Terceros;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePleitos : DescriptorDeCrud<PleitoDto>
    {
        public DescriptorDePleitos(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(PleitosController)
               , nameof(PleitosController.CrudPleitos)
               , modo
               , rutaBase: enumNameSpaceTs.Juridico)
        {
            Mnt.OrdenacionInicial = @$"{nameof(PleitoDto.Referencia)}:{nameof(PleitoDto.Referencia)}:{enumModoOrdenacion.descendente.Render()}";
            IncluirFiltros();

            DescriptorDeRecobro();
            DescriptorDeMinuta();
            DescriptorDeSpanDeRegistroEs();
            DefinirMf(menuEdicion, Editor.OpcionesMf);
        }

        private void IncluirFiltros()
        {

            var modal = new ModalDeFiltrado<PleitoDto>(Mnt.Filtro, "filtros-de-pleitos", "Filtros de pleitos", "Seleccione que pleitos mostrar");
            Mnt.Filtro.Modales.Add(modal);

            modal.ControlesDeFiltrado.Add(new ListasDinamicas<PleitoDto>(modal,
                etiqueta: "Abogado",
                filtrarPor: nameof(PleitoDto.IdAbogado),
                ayuda: "seleccione el abogado contrario",
                seleccionarDe: nameof(AbogadoDto),
                buscarPor: nameof(AbogadoDto.Nombre),
                mostrarExpresion: nameof(AbogadoDto.Nombre),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(AbogadosController),
                navegarA: nameof(AbogadosController.CrudAbogados),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 });

            modal.ControlesDeFiltrado.Add(new ListasDinamicas<PleitoDto>(modal,
                etiqueta: "Procurador",
                filtrarPor: nameof(PleitoDto.IdProcurador),
                ayuda: "seleccione el procurador",
                seleccionarDe: nameof(ProcuradorDto),
                buscarPor: nameof(ProcuradorDto.Nombre),
                mostrarExpresion: nameof(ProcuradorDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 1),
                controlador: nameof(ProcuradoresController),
                navegarA: nameof(ProcuradoresController.CrudProcuradores),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 });

            modal.ControlesDeFiltrado.Add(new ListasDinamicas<PleitoDto>(modal,
                etiqueta: "Juzgado",
                filtrarPor: nameof(PleitoDto.Juzgado),
                ayuda: "seleccione el juzgado por el que buscar",
                seleccionarDe: nameof(JuzgadoDto),
                buscarPor: nameof(JuzgadoDto.Nombre),
                mostrarExpresion: nameof(JuzgadoDto.Nombre),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 2),
                controlador: nameof(JuzgadosController),
                navegarA: nameof(JuzgadosController.CrudJuzgados),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 });
        }

        private void DefinirMf(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<PleitoDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<PleitoDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.1' accion-menu='{eventosDeMf.Plt_VincularRegistroEntrada}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Asociar registro de E/S</li>");
            //DescriptorDeEdicion<PleitoDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.3' accion-menu='{eventosDeMf.Interlocutores}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Gestionar interlocutores</li>");
        }

        private void DescriptorDeSpanDeRegistroEs()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-registrosEs", "RegistrosEs", true, "Registros de ES del pleito");
            Editor.Expanes.Insert(1, expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("registrosEs");
            columnas.Add(titulo: "Registro", propiedad: nameof(RegistroEsDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Estado", propiedad: nameof(RegistroEsDto.Estado), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Id", propiedad: nameof(RegistroEsDtm.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(RegistrosEsController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(BaseController<PleitoDto>.epLeerVinculosCon)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(VinculoDtm.idElemento1) }
                 , { nameof(ltrParametrosEp.idVinculado) , NegociosDeSe.IdNegocio(enumNegocio.Registro) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(RegistrosEsController)}/{nameof(RegistrosEsController.CrudRegistrosEs)}"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            expansor.DescriptorParaVincular(Editor.Crud.Contexto, NegociosDeSe.IdNegocio(enumNegocio.Registro), typeof(SelectorDeRegistroEsDto), nameof(RegistrosEsController), "Asociar registro de E/S");
        }

        private void DescriptorDeRecobro()
        {
            var recobro = new AmpliacionDeEdicion(Editor, Ampliaciones.Pleitos.recobro, "Datos del recobro", new Dimension(2, 2), ayuda: "Información sobre datos de la deuda");
            recobro.Dto = typeof(RecobroDto);
            recobro.Controlador = nameof(RecobrosController);
            Editor.Ampliaciones.Add(recobro);
        }

        private void DescriptorDeMinuta()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-minuta", "Minuta", true, "Minuta de un pleito");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles de la minuta
            var columnas = new DescriptorDeColumnas("minuta");
            columnas.Add(titulo: "Orden", propiedad: nameof(MinutaDto.Orden), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 100);
            columnas.Add(titulo: "Concepto", propiedad: nameof(MinutaDto.Concepto), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Valor", propiedad: nameof(MinutaDto.Valor), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150);
            columnas.Add(titulo: "Abonado", propiedad: nameof(MinutaDto.Abonado), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150);
            columnas.Add(titulo: "Pendiente", propiedad: nameof(MinutaDto.Pendiente), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(MinutaDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(MinutaDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(MinutaDto.Orden)}:{enumModoOrdenacion.ascendente.Render()};" +
                        $"{nameof(MinutaDto.CreadoEl)}:{enumModoOrdenacion.ascendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(MinutasController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(MinutasController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(MinutaDto.IdElemento) }
                ,  { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(MinutaDto), typeof(MinutasController), nameof(MinutaDto.IdElemento), "Añadir concepto");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Juridico}.{enumFunctionTs.InicializarModalParaCrearMinuta}('{modalDeCreacion.IdHtml}')";

            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(MinutaDto), typeof(MinutasController), "Editar concepto", soloConsulta: false);
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Pleitos.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDePleitos('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
