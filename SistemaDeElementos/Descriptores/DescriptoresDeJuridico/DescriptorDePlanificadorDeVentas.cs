using ModeloDeDto.Juridico;
using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using ModeloDeDto;
using System.Collections.Generic;
using ServicioDeDatos.Juridico;
using Utilidades;
using ServicioDeDatos.Seguridad;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePlanificadorDeVentas : DescriptorDeCrud<PlanificadorDeVentaDto>
    {
        public DescriptorDePlanificadorDeVentas(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(PlanificadorDeVentasController)
               , nameof(PlanificadorDeVentasController.CrudPlanificadorDeVentas)
               , modo
               , rutaBase: enumNameSpaceTs.Juridico)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<PlanificadorDeVentaDto>(padre: fltGeneral
                  , etiqueta: "Contrato"
                  , propiedad: nameof(PlanificadorDeVentaDto.IdContrato)
                  , ayuda: "buscar por contrato"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(ContratosController),
                VistaDondeNavegar = nameof(ContratosController.CrudContratos),
                Negocio = enumNegocio.Contrato
            };


            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Planificador", nameof(PlanificadorDeVentaDto.Nombre), "Buscar por 'planificador'");
            Mnt.OrdenacionInicial = @$"{nameof(PlanificadorDeVentaDto.Nombre)}:{nameof(PlanificadorDeVentaDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";


            LineasDelPlanificador();
            DefinirMf(menuIndividual, Mnt.OpcionesPorElemento);
            DefinirMf(menuEdicion, Editor.OpcionesMf);
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.PlanificacionDeVenta}' accion-menu='{eventosDeMf.Plf_IrAPlvDeVenta}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.PlanificacionDeVenta.Plural()}</li>");

        }


        private void DefinirMf(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<PlanificadorDeVentaDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<PlanificadorDeVentaDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.3' accion-menu='{eventosDeMf.Plf_GenerarPlanificadorDeVenta}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Generar planificaciones</li>");
        }

        private void LineasDelPlanificador()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-lineasDeUnPlfVenta", "Detalle", true, "lineas del planificador");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("lineasDeUnPlfVenta");
            columnas.Add(titulo: "Orden", propiedad: nameof(LineaDeUnPlfVentaDto.Orden), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 100);
            columnas.Add(titulo: "Concepto", propiedad: nameof(LineaDeUnPlfVentaDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Unidad", propiedad: nameof(LineaDeUnPlfVentaDto.Unidad), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Cantidad", propiedad: nameof(LineaDeUnPlfVentaDto.Cantidad), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150, formato: enumFormato.Numero_2);
            columnas.Add(titulo: "P. de venta", propiedad: nameof(LineaDeUnPlfVentaDto.Venta), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: "Descuento", propiedad: nameof(LineaDeUnPlfVentaDto.Descuento), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150, formato: enumFormato.Porcentaje);
            columnas.Add(titulo: "B.I.", propiedad: nameof(LineaDeUnPlfVentaDto.BaseImponible), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: "Iva", propiedad: nameof(LineaDeUnPlfVentaDto.Iva), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150, formato: enumFormato.Porcentaje);
            columnas.Add(titulo: "Importe", propiedad: nameof(LineaDeUnPlfVentaDto.ImporteDeLinea), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(LineaDeUnPlfVentaDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(LineaDeUnPlfVentaDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(LineaDeUnPlfVentaDto.Orden)}:{enumModoOrdenacion.ascendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(LineasDeUnPlfVentaController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(LineasDeUnPlfVentaController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(LineaDeUnPlfVentaDto.IdElemento) }
                ,  { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnPlfVentaDto), typeof(LineasDeUnPlfVentaController), nameof(LineaDeUnPlfVentaDto.IdElemento), "Añadir línea");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Juridico}.{enumFunctionTs.Plv_InicializarModalParaCrearLineas}()";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnPlfVentaDto), typeof(LineasDeUnPlfVentaController), "Editar línea", soloConsulta: false);
            modalDeEdicion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Juridico}.{enumFunctionTs.Plv_InicializarModalParaEditarLineas}();";

        }



        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/ApiDeContratos.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/ApiDePlanificadorDeVentas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/PlanificadorDeVentas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDePlanificadorDeVentas('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
