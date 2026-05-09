using System;
using System.Collections.Generic;
using Utilidades;
using GestorDeElementos;
using ModeloDeDto;
using ModeloDeDto.Entorno;
using ModeloDeDto.Negocio;
using ModeloDeDto.Seguridad;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using UtilidadesParaIu;
using static GestoresDeNegocio.Terceros.GestorDeCentrosGestores;
using ServicioDeDatos.Negocio;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeCentrosGestores : DescriptorDeFormulario
    {

        public DescriptorDeCentrosGestores(IGestor gestorDeNegocio, string id, string titulo, string controlador, string vista)
        : base(gestorDeNegocio.Contexto, id, titulo, controlador, ruta: enumNameSpaceTs.Terceros, vista: vista)
        {
            Dto = gestorDeNegocio.TipoDeNegocioDto;
            Negocio = gestorDeNegocio.Negocio;

            var bloques = DefinirPanelesDeJerarquia(this, "Centros de gestión", "jerarquía de Cgs", "detalle del CG");

            bloques.contenedorDto.Expansores.Add(DescriptorDeExpansorPuesto(bloques.contenedorDto));
            bloques.contenedorDto.Expansores.Add(DescriptorDeExpansorNegocios(bloques.contenedorDto));
            bloques.contenedorDto.Expansores.Add(DescriptorDeExpansor.ExpansorDeAuditoria(bloques.contenedorDto, enumNameSpaceTs.Formulario, Negocio.ToNombre()));

            IncluirControlesDeFiltrado();
            DefinirMfIndividual();
        }

        private DescriptorDeExpansor DescriptorDeExpansorPuesto(BloqueAnexado contenedorDto)
        {
            var expansorDePts = new DescriptorDeExpansor(contenedorDto, $"{contenedorDto.Id}-puestos", "PTs definidos en el CG", mostrarPlegado: true, "PT definidos en un CG");

            //Definimos el grid de detalles del cuerpo
            var columnasDePts = new DescriptorDeColumnas("puestos");
            columnasDePts.Add(titulo: "Puesto de trabajo", propiedad: nameof(PuestoDto.Nombre), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: "Id", propiedad: nameof(PuestoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PuestoDeTrabajoController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe, PuestoDtm, PuestoDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PuestoDtm.IdCg) }
              , { nameof(GridDeRelacion.OrdenarPor), $"{nameof(PuestoDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}" }
            };

            new GridDeRelacion(expansorDePts, columnasDePts, parametros)
            {
                PermitirBorrar = false
            };

            expansorDePts.NavegarA(texto: "Gestionar PTs"
                , controlador: nameof(PuestoDeTrabajoController)
                , crud: nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo)
                , soloFiltra: false
                , propiedadRestrictora: nameof(PuestoDto.IdCg)
                , idRestrictor: nameof(CentroGestorDto.Id)
                , textoMostrar: nameof(CentroGestorDto.Expresion));

            return expansorDePts;
        }

        private DescriptorDeExpansor DescriptorDeExpansorNegocios(BloqueAnexado contenedorDto)
        {
            var expansorDeNegocios = new DescriptorDeExpansor(contenedorDto, $"{contenedorDto.Id}-negocios", "Negocios definidos en el CG", mostrarPlegado: true, "Negocios definidos en un CG");

            //Definimos el grid de detalles del cuerpo
            var columnasDePts = new DescriptorDeColumnas("negocios");
            columnasDePts.Add(titulo: "Negocio", propiedad: nameof(NegociosDeUnCgDto.Negocio), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: "Gestor", propiedad: nameof(NegociosDeUnCgDto.Gestor), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: "Consultor", propiedad: nameof(NegociosDeUnCgDto.Consultor), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: "Id", propiedad: nameof(NegociosDeUnCgDto.Id), alineacion: enumAliniacion.izquierda, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(NegociosDeUnCgController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe, NegociosDeUnCgDtm, NegociosDeUnCgDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(NegociosDeUnCgDto.IdCg) }
              , { nameof(GridDeRelacion.OrdenarPor), $"{nameof(NegociosDeUnCgDto.Negocio)}:{nameof(NegociosDeUnCgDto.Negocio)}.{nameof(NegocioDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}" }
            };

            new GridDeRelacion(expansorDeNegocios, columnasDePts, parametros)
            {
                PermitirBorrar = false
            };

            expansorDeNegocios.NavegarA(texto: "Gestionar Permisos"
                , controlador: nameof(PermisosController)
                , crud: nameof(PermisosController.CrudPermiso)
                , soloFiltra: true
                , propiedadRestrictora: nameof(NegociosDeUnCgDto.IdCg)
                , idRestrictor: nameof(CentroGestorDto.Id)
                , textoMostrar: nameof(CentroGestorDto.Expresion));


            return expansorDeNegocios;
        }

        private void IncluirControlesDeFiltrado()
        {
            Filtro.ControlesDeFiltrado.Add(new ListasDinamicas<CentroGestorDto>(Filtro,
                                             etiqueta: "Sociedad",
                                             filtrarPor: ltrCentrosGestores.filtroPorSociedad,
                                             ayuda: "Seleccione la sociedad",
                                             seleccionarDe: nameof(SociedadDto),
                                             buscarPor: nameof(SociedadDto.Nombre),
                                             mostrarExpresion: $"[{nameof(SociedadDto.Expresion)}]",
                                             criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                                             posicion: new Posicion(0, 0),
                                             controlador: nameof(SociedadesController),
                                             navegarA: nameof(SociedadesController.CrudSociedades),
                                             restringirPor: "",
                                             alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 }
                                           );

            Filtro.ControlesDeFiltrado.Add(new ListasDinamicas<UsuarioDto>(Filtro,
                                             etiqueta: "Usuario",
                                             filtrarPor: ltrCentrosGestores.filtroPorUsuario,
                                             ayuda: "Cgs permitidos para el usuario",
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
                                             filtrarPor: ltrCentrosGestores.filtroPorPuesto,
                                             ayuda: "Cgs permitidos para el PT",
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
                                             filtrarPor: ltrCentrosGestores.filtroPorRol,
                                             ayuda: "Cgs permitidos para el rol",
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

            Dictionary<string, string> opcTipoPermiso = typeof(enumTipoPermiso).ToDiccionario();

            Filtro.ControlesDeFiltrado.Add(new ListaDeValores<PermisoDto>(Filtro
            , "Tipo de permisos"
            , "Seleccione el tipo de permiso por el que filtrar"
            , opcTipoPermiso
            , ltrCentrosGestores.filtroPorTipoPermiso));

            Dictionary<string, string> opciones = new Dictionary<string, string>();

            foreach (var negocio in Enum.GetValues(typeof(enumNegocio)))
            {
                if (!NegociosDeSe.UsaPermisosPorCg((enumNegocio)negocio))
                    continue;
                var negocioDtm = NegociosDeSe.LeerNegocioPorNombre(((enumNegocio)negocio).ToNombre());
                opciones.Add(negocioDtm.Id.ToString(), negocioDtm.Nombre);
            }

            Filtro.ControlesDeFiltrado.Add(new ListaDeValores<NegocioDto>(Filtro
            , "A que accede"
            , "Seleccione el negocio al que quiere saber si accede"
            , opciones
            , ltrCentrosGestores.filtroPorNegocio));

            Filtro.ControlesDeFiltrado.Add(new CheckFiltro<TipoDeElementoDto>(Filtro, "Mostrar los Cgs de baja", ltrCentrosGestores.filtroPorTiposNoActivo, "Sólo muestra los Cgs de baja", false));


        }

        internal void DefinirMfIndividual()
        {
            Cabecera.OpcionesIndividuales.Add($"<li id='{DescriptorDeCrud<ElementoDto>.menuEdicion}.{eventosDeMf.DarDeBaja}' accion-menu='{eventosDeMf.DarDeBaja}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Dar de baja</li>");
            Cabecera.OpcionesIndividuales.Add($"<li id='{DescriptorDeCrud<ElementoDto>.menuEdicion}.{eventosDeMf.DarDeAlta}' accion-menu='{eventosDeMf.DarDeAlta}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Dar de alta</li>");
        }


        public string RenderCentrosGestores()
        {
            var render = RenderFormulario();

            render = render +
                   $@"<script src=¨../../js/_Formulario/Jerarquia.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaVista}/CentrosGestores.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaVista}.CrearFormulario('{IdHtml}','{Negocio.ToNombre()}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el formulario', error);
                         }}
                      </script>

                    ";

            return render.Render();
        }
    }
}
