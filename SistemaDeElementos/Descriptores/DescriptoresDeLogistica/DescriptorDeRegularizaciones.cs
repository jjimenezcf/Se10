using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ModeloDeDto;
using ModeloDeDto.Logistica;
using System.Collections.Generic;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeRegularizaciones : DescriptorDeCrud<RegularizacionDto>
    {
        public DescriptorDeRegularizaciones(ContextoSe contexto, string renderCache) : base(contexto, renderCache)
        {
        }

        public DescriptorDeRegularizaciones(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(RegularizacionesController)
               , nameof(RegularizacionesController.CrudRegularizaciones)
               , modo
               , rutaBase: enumNameSpaceTs.Logistica)
        {
            // filtro (y restrictor al llegar desde el menú del almacén) por el almacén de la regularización
            new ListasDinamicas<RegularizacionDto>(Mnt.BloqueGeneral,
                etiqueta: enumNegocio.Almacen.Singular(),
                filtrarPor: nameof(RegularizacionDto.IdAlmacen),
                ayuda: "seleccione el almacén",
                seleccionarDe: nameof(AlmacenDto),
                buscarPor: nameof(AlmacenDto.Expresion),
                mostrarExpresion: nameof(AlmacenDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(0, 1),
                controlador: nameof(AlmacenesController),
                navegarA: nameof(AlmacenesController.CrudAlmacenes),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            DescriptorDeLineasDeUnaRegularizacion();

            // sólo se puede importar con la regularización recontando (ver CrudEdicionRegularizacion.AjustarLineasSegunEtapa)
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(RecuentoDto), eventosDeMf.Ral_ImportarRecuento, "Seleccionar el recuento a importar"));
            Editor.IncluirMfIndividual("Importar recuento", eventosDeMf.Ral_ImportarRecuento, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor);

            // aplicar el recuento genera los movimientos de almacén en segundo plano (ver TrabajosParaRegularizaciones.AplicarRecuento)
            Editor.IncluirMfIndividual("Aplicar recuento", eventosDeMf.Ral_AplicarRecuento, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor);
        }

        private void DescriptorDeLineasDeUnaRegularizacion()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-lineasDeUnaRegularizacion", "Detalle", mostrarPlegado: true, "Líneas de la regularización");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles de la regularización
            var columnas = new DescriptorDeColumnas("lineasDeUnaRegularizacion");
            columnas.Add(titulo: nameof(LineasDeUnaRegularizacionDto.Orden), alineacion: enumAliniacion.derecha, tamano: 100);
            columnas.Add(titulo: nameof(LineasDeUnaRegularizacionDto.Fecha), alineacion: enumAliniacion.centrada, tamano: 120, formato: enumFormato.Fecha);
            columnas.Add(titulo: nameof(LineasDeUnaRegularizacionDto.Unitario));
            columnas.Add(titulo: nameof(LineasDeUnaRegularizacionDto.Cantidad), tamano: 150, formato: enumFormato.Numero_6);
            columnas.Add(titulo: nameof(LineasDeUnaRegularizacionDto.Precio), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(LineasDeUnaRegularizacionDto.IdElemento), mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(LineasDeUnaRegularizacionDto.Id), mostrar: false);

            var orden = $"{nameof(LineasDeUnaRegularizacionDto.Orden)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(LineasDeUnaRegularizacionController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(LineasDeUnaRegularizacionController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(LineasDeUnaRegularizacionDto.IdElemento) }
                ,  { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(LineasDeUnaRegularizacionDto), typeof(LineasDeUnaRegularizacionController), nameof(LineasDeUnaRegularizacionDto.IdElemento), "Añadir línea");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {RutaBase}.{enumFunctionTs.Ral_InicializarModalParaCrearLineas}({ExtensorDeRegularizaciones.IncrementarOrdenEn(Contexto)})";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(LineasDeUnaRegularizacionDto), typeof(LineasDeUnaRegularizacionController), "Editar línea", soloConsulta: false);
            modalDeEdicion.AccionTrasAbrirModal = $"javascript: {RutaBase}.{enumFunctionTs.Ral_InicializarModalParaEditarLineas}()";
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
                return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src='../../js/{RutaBase}/ApiDeLogistica.js?v={System.DateTime.Now.Ticks}'></script>
                      <script src=¨../../js/{RutaBase}/Regularizaciones.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{
                           {RutaBase}.CrearCrudDeRegularizaciones('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}')
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
