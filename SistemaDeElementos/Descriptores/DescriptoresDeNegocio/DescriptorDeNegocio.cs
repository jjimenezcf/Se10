using UtilidadesParaIu;
using MVCSistemaDeElementos.Controllers;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using Utilidades;
using ModeloDeDto;
using ServicioDeDatos.Seguridad;
using System.Collections.Generic;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeNegocio : DescriptorDeCrud<NegocioDto>
    {
        public DescriptorDeNegocio(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , controlador: nameof(NegocioController)
               , vista: $"{nameof(NegocioController.CrudDeNegocios)}"
               , modo: modo
              , rutaBase: enumNameSpaceTs.Negocio)
        {
            AnadirOpcionDeDependencias(Mnt
            , controlador: nameof(ParametrosDeNegocioController)
            , vista: nameof(ParametrosDeNegocioController.CrudDeParametrosDeNegocio)
            , datosDependientes: nameof(ParametroDeNegocioDto)
            , navegarAlCrud: DescriptorDeMantenimiento<ParametroDeNegocioDto>.NombreMnt
            , nombreOpcion: "Parámetros"
            , propiedadQueRestringe: nameof(NegocioDto.Id)
            , propiedadRestrictora: nameof(ParametroDeNegocioDto.IdNegocio)
            , "Parametros de un negocio");

            AnadirOpcionDeDependencias(Mnt
            , controlador: nameof(PlantillasDeExportacionController)
            , vista: nameof(PlantillasDeExportacionController.CrudDePlantillasDeExportacion)
            , datosDependientes: nameof(PlantillaDeExportacionDto)
            , navegarAlCrud: DescriptorDeMantenimiento<PlantillaDeExportacionDto>.NombreMnt
            , nombreOpcion: "Exportaciones"
            , propiedadQueRestringe: nameof(NegocioDto.Id)
            , propiedadRestrictora: nameof(PlantillaDeExportacionDto.IdNegocio)
            , "Plantillas de exportación de un negocio");

            Editor.Expanes.Insert(0, DescriptorDeExpansorAcciones(Editor));
            Editor.Expanes.Insert(0, DescriptorDeExpansorPlantillas(Editor));
            Editor.Expanes.Insert(0, DescriptorDeExpansorClases(Editor)); 
        }


        private DescriptorDeExpansor DescriptorDeExpansorAcciones(DescriptorDeEdicion<NegocioDto> editor)
        {
            var expansor = new DescriptorDeExpansor(editor, $"{editor.Id}-acciones", "Acciones de un negocio", mostrarPlegado: true, "Acciones de un negocio");

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("acciones");
            columnas.Add(titulo: "Momento", propiedad: nameof(AccionDeNegocioDto.Momento), alineacion: enumAliniacion.izquierda, tamano: 100);
            columnas.Add(titulo: "Orden", propiedad: nameof(AccionDeNegocioDto.Orden), alineacion: enumAliniacion.derecha, tamano: 100);
            columnas.Add(titulo: "Acción", propiedad: nameof(AccionDeNegocioDto.Accion), alineacion: enumAliniacion.izquierda);
            columnas.Add(titulo: nameof(AccionDeNegocioDto.Id), propiedad: nameof(AccionDeNegocioDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: nameof(AccionDeNegocioDto.IdAccion), propiedad: nameof(AccionDeNegocioDto.IdAccion), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(AccionesDeNegocioController)}
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(AccionesDeNegocioController.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(AccionDeNegocioDto.IdNegocioAfectado)}
              , { nameof(GridDeRelacion.OrdenarPor), $"{nameof(AccionDeNegocioDto.Momento)}:{enumModoOrdenacion.ascendente.Render()};" +
                                                     $"{nameof(AccionDeNegocioDto.Orden)}:{enumModoOrdenacion.ascendente.Render()}"}
            };

            var grid = new GridDeRelacion(expansor, columnas, parametros);
            grid.PermitirBorrar = false;

            expansor.DescriptorDeEditarRelaciones(editor.Crud.Contexto, typeof(AccionDeNegocioDto), typeof(AccionesDeNegocioController), "Editar la acción", false);

            return expansor;
        }

        private DescriptorDeExpansor DescriptorDeExpansorPlantillas(DescriptorDeEdicion<NegocioDto> editor)
        {
            var expansorDePtl = new DescriptorDeExpansor(editor, $"{editor.Id}-plantillas", "Plantillas", mostrarPlegado: true, "Plantillas de impresión");

            //Definimos el grid de detalles del cuerpo
            var columnasDePlt = new DescriptorDeColumnas("plantillas");
            columnasDePlt.Add(titulo: "Plantilla", propiedad: nameof(PlantillaDeNegocioDto.Nombre), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePlt.Add(titulo: nameof(PlantillaDeNegocioDto.Accion), propiedad: nameof(PlantillaDeNegocioDto.Accion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePlt.Add(titulo: nameof(PlantillaDeNegocioDto.Plantilla), propiedad: nameof(PlantillaDeNegocioDto.Plantilla), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePlt.Add(titulo: nameof(PlantillaDeNegocioDto.Id), propiedad: nameof(PlantillaDeNegocioDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnasDePlt.Add(titulo: nameof(PlantillaDeNegocioDto.IdNegocio), propiedad: nameof(PlantillaDeNegocioDto.IdNegocio), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PlantillasDeNegocioController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(PlantillasDeNegocioController.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PlantillaDeNegocioDto.IdNegocio)}
              , { nameof(GridDeRelacion.OrdenarPor), $"{nameof(PlantillaDeNegocioDto.Nombre)}:{enumModoOrdenacion.ascendente.Render()}" }
            };

            new GridDeRelacion(expansorDePtl, columnasDePlt, parametros)
            {
                PermitirBorrar = Contexto.DatosDeConexion.EsAdministrador,
                PermitirEditar = Contexto.DatosDeConexion.EsAdministrador
            };

            if (Contexto.DatosDeConexion.EsAdministrador)
            {
                var modalDeCrear = expansorDePtl.DescriptorDeCrearRelaciones(Contexto, typeof(PlantillaDeNegocioDto), typeof(PlantillasDeNegocioController), propiedadRestrictora: nameof(PlantillaDeNegocioDto.IdNegocio), "Añadir plantilla");
                modalDeCrear.AccionTrasAbrirModal = $"javascript:{enumNameSpaceTs.Negocio}.{enumFunctionTs.Negocio_TrasAbrirModalDePlantillasDeNegocio}('{modalDeCrear.IdHtml}', 'crear')";

                var modal = expansorDePtl.DescriptorDeEditarRelaciones(Contexto, typeof(PlantillaDeNegocioDto), typeof(PlantillasDeNegocioController), "Editar plantilla", soloConsulta: false);
            }

            return expansorDePtl;
        }

        private DescriptorDeExpansor DescriptorDeExpansorClases(DescriptorDeEdicion<NegocioDto> editor)
        {
            var expansor = new DescriptorDeExpansor(editor, $"{editor.Id}-clases", "Clases", mostrarPlegado: true, "Clases del negocio");


            var columnas = new DescriptorDeColumnas("clases");
            columnas.Add(titulo: "Referencia", propiedad: nameof(ClaseDelNegocioDto.Referencia), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 200);
            columnas.Add(titulo: "Clase", propiedad: nameof(ClaseDelNegocioDto.Nombre), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(ClaseDelNegocioDto.Activa), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: nameof(ClaseDelNegocioDto.Id), mostrar: false);
            columnas.Add(titulo: nameof(ClaseDelNegocioDto.IdNegocio), mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(ClasesDelNegocioController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(ClasesDelNegocioController.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ClaseDelNegocioDto.IdNegocio)}
              , { nameof(GridDeRelacion.OrdenarPor), $"{nameof(ClaseDelNegocioDto.Nombre)}:{enumModoOrdenacion.ascendente.Render()}" }
            };

            new GridDeRelacion(expansor, columnas, parametros)
            {
                PermitirBorrar = Contexto.DatosDeConexion.EsAdministrador,
                PermitirEditar = Contexto.DatosDeConexion.EsAdministrador
            };

            if (Contexto.DatosDeConexion.EsAdministrador)
            {
                var modalDeCrear = expansor.DescriptorDeCrearRelaciones(Contexto, typeof(ClaseDelNegocioDto), typeof(ClasesDelNegocioController), propiedadRestrictora: nameof(ClaseDelNegocioDto.IdNegocio), "Añadir clase");
                var modal = expansor.DescriptorDeEditarRelaciones(Contexto, typeof(ClaseDelNegocioDto), typeof(ClasesDelNegocioController), "Editar clase", soloConsulta: false);
            }

            return expansor;
        }



        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            if (cache.ContainsKey(indice))
                return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Negocio.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeNegocios('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el crud', error.message);
                         }}
                      </script>
                    ";
            cache[indice] = render.Render();
            return (string)cache[indice];
        }

    }
}
