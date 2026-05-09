using Utilidades;
using GestorDeElementos;
using ModeloDeDto;
using ModeloDeDto.Entorno;
using ModeloDeDto.Negocio;
using ModeloDeDto.Seguridad;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using static GestoresDeNegocio.Entorno.GestorDeMenus;
using ServicioDeDatos;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeJerarqiaMenus : DescriptorDeFormulario
    {
        public DescriptorDeJerarqiaMenus(IGestor gestorDeNegocio, string id, string titulo, string controlador, string vista)
        : base(gestorDeNegocio.Contexto, id, titulo, controlador, ruta: enumNameSpaceTs.Entorno, vista: vista)
        {
            Dto = gestorDeNegocio.TipoDeNegocioDto;
            Negocio = gestorDeNegocio.Negocio;

            DefinirPanelesDeJerarquia(this, "Menus de SE", "jerarquía de Menus de SE", "detalle del Menú");

            IncluirControlesDeFiltrado();
        }

        private void IncluirControlesDeFiltrado()
        {
            Filtro.ControlesDeFiltrado.Add(new ListasDinamicas<MenuDto>(Filtro,
                                             etiqueta: "Vista",
                                             filtrarPor: ltrDeMenus.filtroPorVista,
                                             ayuda: "Seleccione la vista",
                                             seleccionarDe: nameof(VistaMvcDto),
                                             buscarPor: nameof(VistaMvcDto.Nombre),
                                             mostrarExpresion: $"[{nameof(VistaMvcDto.Nombre)}]",
                                             criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                                             posicion: new Posicion(0, 0),
                                             controlador: nameof(VistaMvcController),
                                             navegarA: nameof(VistaMvcController.CrudVistaMvc),
                                             restringirPor: "",
                                             alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 }
                                           );

            Filtro.ControlesDeFiltrado.Add(new ListasDinamicas<UsuarioDto>(Filtro,
                                             etiqueta: "Usuario",
                                             filtrarPor: ltrDeMenus.filtroPorUsuario,
                                             ayuda: "Menús permitidos para el usuario",
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
                                             filtrarPor: ltrDeMenus.filtroPorPuesto,
                                             ayuda: "Menús permitidos para el PT",
                                             seleccionarDe: nameof(PuestoDto),
                                             buscarPor: nameof(PuestoDto.Nombre),
                                             mostrarExpresion: $"[{nameof(PuestoDto.Nombre)}]",
                                             criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                                             posicion: new Posicion(0, 0),
                                             controlador: nameof(PuestoDeTrabajoController),
                                             navegarA: nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo),
                                             restringirPor: "",
                                             alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 }
                                           );

            Filtro.ControlesDeFiltrado.Add(new ListasDinamicas<UsuarioDto>(Filtro,
                                             etiqueta: "Rol",
                                             filtrarPor: ltrDeMenus.filtroPorRol,
                                             ayuda: "Menús permitidos para el rol",
                                             seleccionarDe: nameof(RolDto),
                                             buscarPor: nameof(RolDto.Nombre),
                                             mostrarExpresion: $"[{nameof(RolDto.Nombre)}]",
                                             criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                                             posicion: new Posicion(0, 0),
                                             controlador: nameof(RolController),
                                             navegarA: nameof(RolController.CrudRol),
                                             restringirPor: "",
                                             alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 }
                                          );

            Filtro.ControlesDeFiltrado.Add(new CheckFiltro<TipoDeElementoDto>(Filtro, "Mostrar los menús no activos", ltrDeMenus.filtroPorNoActivo, "Sólo muestra los menús no activos", false));

        }

        public string RenderCentrosGestores()
        {
            var render = RenderFormulario();

            render = render +
                   $@"<script src=¨../../js/_Formulario/Jerarquia.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaVista}/JerarquiaMenus.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaVista}.CrearFormulario('{IdHtml}','{Negocio.ToNombre()}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el formulario', (error instanceof Error) ? error.message : error);
                         }}
                      </script>

                    ";

            return render.Render();
        }
    }
}
