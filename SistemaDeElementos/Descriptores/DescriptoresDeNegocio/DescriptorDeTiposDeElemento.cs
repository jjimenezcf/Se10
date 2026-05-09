using System.Collections.Generic;
using Utilidades;
using GestorDeElementos;
using ModeloDeDto.Entorno;
using ModeloDeDto.Negocio;
using ModeloDeDto.Seguridad;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using static GestoresDeNegocio.Negocio.GestorDeTiposDeElemento<ServicioDeDatos.ContextoSe, ServicioDeDatos.Elemento.TipoDeElementoDtm, ModeloDeDto.Negocio.TipoDeElementoDto>;
using ServicioDeDatos;
using ModeloDeDto;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Negocio;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeTiposDeElemento : DescriptorDeFormulario
    {
        public string FicheroDeApi { get; }

        public DescriptorDeTiposDeElemento(IGestor gestorDeNegocio, string id, string titulo, string controlador, string vista, string accion)
        : base(gestorDeNegocio.Contexto, id, titulo, controlador, ruta: enumNameSpaceTs.Negocio, vista: vista)
        {
            Dto = gestorDeNegocio.Metadatos.TipoDto;
            Negocio = gestorDeNegocio.Negocio;
            Accion = accion;

            var bloques = DefinirPanelesDeJerarquia(this, $"Tipos de {gestorDeNegocio.Negocio.ToNombre()}", "jerarquía de tipos", "detalle del tipo");

            if (Negocio.UsaPlantillasPorTipo())
                bloques.contenedorDto.Expansores.Add(DescriptorDeExpansorPlantillas(bloques.contenedorDto));

            if (Negocio.UsaClasesDelTipo())
                bloques.contenedorDto.Expansores.Add(DescriptorDeExpansorClases(bloques.contenedorDto));
            

            IncluirControlesDeFiltrado();
            FicheroDeApi = enumFicheroDeApi.Tipo(Negocio);
        }

        public static DescriptorDeTiposDeElemento CrearDescriptor(IGestor gestorDeNegocio, string id, string titulo, string controlador, string vista, string accion)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.DescriptorDeCrud);
            var indice = $"{gestorDeNegocio.Contexto.DatosDeConexion.IdUsuario}-{gestorDeNegocio.Negocio}-{accion}";
            if (!cache.ContainsKey(indice))
            {
                cache[indice] = new DescriptorDeTiposDeElemento(gestorDeNegocio, $"tipo-{gestorDeNegocio.Negocio}", $"Tipos de {gestorDeNegocio.Negocio.ToNombre()}", controlador, vista, accion);
            }
            return (DescriptorDeTiposDeElemento)cache[indice];
        }

        private DescriptorDeExpansor DescriptorDeExpansorPlantillas(BloqueAnexado contenedorDto)
        {
            var expansorDePtl = new DescriptorDeExpansor(contenedorDto, $"{contenedorDto.Id}-plantillas", "Plantillas", mostrarPlegado: true, "Plantillas de impresión");

            //Definimos el grid de detalles del cuerpo
            var columnasDePlt = new DescriptorDeColumnas("plantillas");
            columnasDePlt.Add(titulo: "Plantilla", propiedad: nameof(PlantillaPorTipoDto.Nombre), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePlt.Add(titulo: nameof(PlantillaPorTipoDto.Accion), propiedad: nameof(PlantillaPorTipoDto.Accion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePlt.Add(titulo: nameof(PlantillaPorTipoDto.Id), propiedad: nameof(PlantillaPorTipoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnasDePlt.Add(titulo: nameof(PlantillaPorTipoDto.IdTipo), propiedad: nameof(PlantillaPorTipoDto.IdTipo), alineacion: enumAliniacion.derecha, mostrar: false);
            columnasDePlt.Add(titulo: nameof(PlantillaPorTipoDto.IdNegocio), propiedad: nameof(PlantillaPorTipoDto.IdNegocio), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(TiposDeElementoController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(TiposDeElementoController.epLeerPlantillas)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PlantillaPorTipoDto.IdTipo) }
              , { nameof(GridDeRelacion.OrdenarPor), $"{nameof(PlantillaPorTipoDto.Nombre)}:{enumModoOrdenacion.ascendente.Render()}" }
              , { nameof(GridDeRelacion.EspaciodeNombres), enumNameSpaceTs.Formulario }
              , { nameof(GridDeRelacion.AccionDeBorrado),  nameof(TiposDeElementoController.epBorrarPlantilla) }
              , { nameof(GridDeRelacion.AccionDeLeerPorId),  nameof(TiposDeElementoController.epLeerPlantilla) }
            };

            new GridDeRelacion(expansorDePtl, columnasDePlt, parametros)
            {
                PermitirBorrar = Contexto.DatosDeConexion.EsAdministrador,
                PermitirEditar = Contexto.DatosDeConexion.EsAdministrador
            };

            if (Contexto.DatosDeConexion.EsAdministrador)
            {
                var modalDeCrear = expansorDePtl.DescriptorDeCrearRelaciones(Contexto, typeof(PlantillaPorTipoDto), typeof(TiposDeElementoController), nameof(PlantillaPorTipoDto.IdTipo), "Añadir plantilla",
                    espacioDeNombre: enumNameSpaceTs.Formulario,
                    accionControlador: nameof(TiposDeElementoController.epCrearPlantilla));
                modalDeCrear.AccionTrasAbrirModal = $"javascript:{enumNameSpaceTs.Negocio}.{enumFunctionTs.Negocio_TrasAbrirModalDePlantillasPorTipo}('{modalDeCrear.IdHtml}', 'crear')";

                var modal = expansorDePtl.DescriptorDeEditarRelaciones(Contexto, typeof(PlantillaPorTipoDto), typeof(TiposDeElementoController), "Editar plantilla", false,
                    espacioDeNombre: enumNameSpaceTs.Formulario,
                    accionControlador: nameof(TiposDeElementoController.epModificarPlantilla));
            }

            return expansorDePtl;
        }

        private DescriptorDeExpansor DescriptorDeExpansorClases(BloqueAnexado contenedorDto)
        {
            var expansor = new DescriptorDeExpansor(contenedorDto, $"{contenedorDto.Id}-clases", "Clases", mostrarPlegado: true, "Clases del tipo");

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("clases");
            columnas.Add(titulo: "Clase", mostrar: true);
            columnas.Add(titulo: "Activa", alineacion: enumAliniacion.derecha, tamano:100);
            columnas.Add(titulo: nameof(ClaseDelTipoDto.Id),  mostrar: false);
            columnas.Add(titulo: nameof(ClaseDelTipoDto.IdTipo), mostrar: false);
            columnas.Add(titulo: nameof(ClaseDelTipoDto.IdClase), mostrar: false);
            columnas.Add(titulo: nameof(ClaseDelTipoDto.IdNegocio), mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(TiposDeElementoController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(TiposDeElementoController.epLeerClases)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ClaseDelTipoDto.IdTipo) }
              , { nameof(GridDeRelacion.OrdenarPor), $"{nameof(ClaseDelTipoDto.Clase)}:{nameof(ClaseDelNegocioDtm.Referencia)}.{nameof(ClaseDelNegocioDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}" }
              , { nameof(GridDeRelacion.EspaciodeNombres), enumNameSpaceTs.Formulario }
              , { nameof(GridDeRelacion.AccionDeBorrado),  nameof(TiposDeElementoController.epBorrarClase) }
              , { nameof(GridDeRelacion.AccionDeLeerPorId),  nameof(TiposDeElementoController.epLeerClase) }
            };

            new GridDeRelacion(expansor, columnas, parametros)
            {
                PermitirBorrar = Contexto.DatosDeConexion.EsAdministrador,
                PermitirEditar = false
            };

            if (Contexto.DatosDeConexion.EsAdministrador)
            {
                var modalDeCrear = expansor.DescriptorDeCrearRelaciones(Contexto, typeof(ClaseDelTipoDto), typeof(TiposDeElementoController), nameof(ClaseDelTipoDto.IdTipo), "Añadir clase",
                    espacioDeNombre: enumNameSpaceTs.Formulario,
                    accionControlador: nameof(TiposDeElementoController.epCrearClase));
            }

            return expansor;
        }

        private void IncluirControlesDeFiltrado()
        {
            Filtro.ControlesDeFiltrado.Add(new ListasDinamicas<UsuarioDto>(Filtro,
                                             etiqueta: "Usuario",
                                             filtrarPor: ltrDeUnTipoDeElemento.filtroPorUsuario,
                                             ayuda: "Tipos permitidos para el usuario",
                                             seleccionarDe: nameof(UsuarioDto),
                                             buscarPor: nameof(UsuarioDto.Nombre),
                                             mostrarExpresion: $"[{nameof(UsuarioDto.NombreCompleto)}]",
                                             criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                                             posicion: new Posicion(0, 0),
                                             controlador: nameof(UsuariosController),
                                             navegarA: nameof(UsuariosController.CrudUsuario),
                                             restringirPor: "",
                                             alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 }
                                           );

            Filtro.ControlesDeFiltrado.Add(new ListasDinamicas<UsuarioDto>(Filtro,
                                             etiqueta: "Puesto de trabajo",
                                             filtrarPor: ltrDeUnTipoDeElemento.filtroPorPuesto,
                                             ayuda: "Tipos permitidos para el PT",
                                             seleccionarDe: nameof(PuestoDto),
                                             buscarPor: nameof(PuestoDto.Nombre),
                                             mostrarExpresion: $"[{nameof(PuestoDto.Nombre)}]",
                                             criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                                             posicion: new Posicion(0, 0),
                                             controlador: nameof(PuestoDeTrabajoController),
                                             navegarA: nameof(UsuariosController.CrudUsuario),
                                             restringirPor: "",
                                             alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 }
                                           );

            Filtro.ControlesDeFiltrado.Add(new ListasDinamicas<UsuarioDto>(Filtro,
                                             etiqueta: "Rol",
                                             filtrarPor: ltrDeUnTipoDeElemento.filtroPorRol,
                                             ayuda: "Tipos permitidos para el rol",
                                             seleccionarDe: nameof(RolDto),
                                             buscarPor: nameof(RolDto.Nombre),
                                             mostrarExpresion: $"[{nameof(RolDto.Nombre)}]",
                                             criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                                             posicion: new Posicion(0, 0),
                                             controlador: nameof(RolController),
                                             navegarA: nameof(UsuariosController.CrudUsuario),
                                             restringirPor: "",
                                             alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 }
                                          );

            Dictionary<string, string> opciones = new Dictionary<string, string> { { "adm", "Permisos de administrador" }, { "ges", "Permisos de gestor" }, { "con", "Permisos de consultor" } };

            Filtro.ControlesDeFiltrado.Add(new ListaDeValores<TipoDeElementoDto>(Filtro
                , "Modo de acceso"
                , "Seleccione el modo por el que filtrar"
                , opciones
                , ltrDeUnTipoDeElemento.filtroPorModoDeAcceso));

            Filtro.ControlesDeFiltrado.Add(new CheckFiltro<TipoDeElementoDto>(Filtro, "Mostrar tipos no activos", ltrDeUnTipoDeElemento.filtroPorTiposNoActivo, "Sólo muestra los tipos no activos", false));


        }

        public string RenderTiposDeElemento()
        {

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{GetType().FullName}-{Negocio}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            if (!cache.ContainsKey(indice))
            {
                var render = RenderFormulario();

                var nombreFuncion = $"{RutaVista}.CrearFormulario('{IdHtml}','{Negocio.ToNombre()}')";

                if (!FicheroDeApi.IsNullOrEmpty())
                    nombreFuncion = $"{enumFicheroDeApi.EspacioDeNombre(Negocio)}.CrearFormulario('{IdHtml}')";

                render = render +
                       $@"<script src=¨../../js/_Formulario/Jerarquia.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaVista}/TiposDeElemento.js?v={System.DateTime.Now.Ticks}¨></script>{(FicheroDeApi.IsNullOrEmpty() ? "" : $@"
                      <script src=¨../../js/{FicheroDeApi}.js?v={System.DateTime.Now.Ticks}¨></script>")}
                      <script>
                         try {{                           
                            [nombreFuncion]
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el formulario', error);
                         }}
                      </script>
                    ";
                render = render.Replace($"[nombreFuncion]", nombreFuncion);
                cache[indice] = render.Render().QuitarDobleIntro();
            }
            return (string)cache[indice];
        }
    }
}
