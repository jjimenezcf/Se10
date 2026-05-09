using UtilidadesParaIu;
using MVCSistemaDeElementos.Controllers;
using ModeloDeDto.Entorno;
using ModeloDeDto.Seguridad;
using ModeloDeDto;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos;
using Utilidades;
using System.Collections.Generic;
using ModeloDeDto.Guarderias;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Elemento;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeUsuario : DescriptorDeCrud<UsuarioDto>
    {

        public DescriptorDeUsuario(ContextoSe contexto, ModoDescriptor modo) : this(contexto, modo, null)
        {

        }
        public DescriptorDeUsuario(ContextoSe contexto, ModoDescriptor modo, string id)
        : base(contexto,
              controlador: nameof(UsuariosController)
               , vista: $"{nameof(UsuariosController.CrudUsuario)}"
               , modo: modo
              , rutaBase: enumNameSpaceTs.Entorno
              , id)
        {
            Mnt.Filtro.FiltroDeNombre.CambiarAtributos("Usuario", UsuariosPor.NombreCompleto.ToLower(), "Buscar por 'apellido, nombre'");
            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 0));

            if (modo == ModoDescriptor.Mantenimiento)
            {
                //new SelectorDeFiltro<UsuarioDto, PermisoDto>(
                //       Mnt.BloqueGeneral,
                //       etiqueta: "Permisos",
                //       filtrarPor: UsuariosPor.Permisos,
                //       ayuda: "Seleccionar Permiso",
                //       posicion: new Posicion(0, 1),
                //       paraFiltrar: nameof(PermisoDto.Id),
                //       paraMostrar: nameof(PermisoDto.Nombre),
                //       crudModal: new DescriptorDePermiso(Contexto, ModoDescriptor.SeleccionarParaFiltrar),
                //       propiedadDondeMapear: ltrFiltros.Nombre.ToString());


                new ListasDinamicas<UsuarioDto>(Mnt.BloqueGeneral,
                                                etiqueta: "Permiso",
                                                filtrarPor: nameof(PermisosDeUnUsuarioDto.IdPermiso),
                                                ayuda: "permisos de un usuario",
                                                seleccionarDe: nameof(PermisoDto),
                                                buscarPor: nameof(PermisoDto.Nombre),
                                                mostrarExpresion: nameof(PermisoDto.Nombre),
                                                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                                                posicion: new Posicion(0, 1),
                                                controlador: nameof(PermisosController),
                                                navegarA: nameof(PermisosController.CrudPermiso),
                                                restringirPor: "",
                                                alSeleccionarBlanquearControl: "").LongitudMinimaParaBuscar = 1;

                new ListasDinamicas<UsuarioDto>(Mnt.BloqueGeneral,
                                                etiqueta: "Puesto de trabajo",
                                                filtrarPor: nameof(PuestosDeUnUsuarioDto.IdPuesto),
                                                ayuda: "usuarios de este puesto",
                                                seleccionarDe: nameof(PuestoDto),
                                                buscarPor: nameof(PuestoDto.Nombre),
                                                mostrarExpresion: nameof(PuestoDto.Nombre),
                                                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                                                posicion: new Posicion(1, 0),
                                                controlador: nameof(PuestoDeTrabajoController),
                                                navegarA: nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo),
                                                restringirPor: "",
                                                alSeleccionarBlanquearControl: "").LongitudMinimaParaBuscar = 1;


                new ListasDinamicas<UsuarioDto>(Mnt.BloqueGeneral,
                                                etiqueta: "Roles",
                                                filtrarPor: nameof(RolesDeUnPuestoDto.IdRol),
                                                ayuda: "usuarios de un rol",
                                                seleccionarDe: nameof(RolDto),
                                                buscarPor: nameof(RolDto.Nombre),
                                                mostrarExpresion: nameof(RolDto.Nombre),
                                                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                                                posicion: new Posicion(1, 1),
                                                controlador: nameof(RolController),
                                                navegarA: nameof(RolController.CrudRol),
                                                restringirPor: "",
                                                alSeleccionarBlanquearControl: "").LongitudMinimaParaBuscar = 1;
            }

            var modalDePermisos = new ModalDeConsultaDeRelaciones<UsuarioDto, PermisosDeUnUsuarioDto>(mantenimiento: Mnt
                              , tituloModal: "Permisos de un usuario"
                              , crudModal: new DescriptorDePermisosDeUnUsuario(Contexto, ModoDescriptor.ModalDeConsulta)
                              , propiedadRestrictora: nameof(PermisosDeUnUsuarioDto.IdUsuario));

            var mostrarPermisos = new ConsultarRelaciones(modalDePermisos.IdHtml, () => modalDePermisos.RenderControl(), "Mostrar los permisos de un usuario");
            var opcion = new OpcionDeMenu<UsuarioDto>(Mnt.ZonaMenu.Menu, mostrarPermisos, $"Permisos", enumModoDeAccesoDeDatos.Consultor);
            Mnt.ZonaMenu.Menu.Add(opcion);

            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(PuestosDeUnUsuarioController)
                , vista: nameof(PuestosDeUnUsuarioController.CrudPuestosDeUnUsuario)
                , relacionarCon: nameof(PuestoDto)
                , navegarAlCrud: DescriptorDeMantenimiento<PuestosDeUnUsuarioDto>.NombreMnt
                , nombreOpcion: "Puestos"
                , propiedadQueRestringe: nameof(UsuarioDto.Id)
                , propiedadRestrictora: nameof(PuestosDeUnUsuarioDto.IdUsuario)
                , "Añadir puestos al usuario seleccionado");

            Mnt.OrdenacionInicial = @$"{nameof(UsuarioDto.NombreCompleto)}:login:{enumModoOrdenacion.ascendente.Render()}";

            if (contexto.DatosDeConexion.EsAdministrador)
            {
                Editor.Expanes.Insert(0, DescriptorDeExpansorPuestosDeUnUsuario(Editor));
                Editor.Expanes.Insert(1, DescriptorDeExpansorPermisosPorNegocio(Editor));
                Editor.Expanes.Insert(2, DescriptorDeExpansorPermisosPorTipo(Editor));
                Editor.Expanes.Insert(3, DescriptorDeExpansorPermisosPorEstado(Editor));
                Editor.Expanes.Insert(4, DescriptorDeExpansorPermisosPorTransicion(Editor));
                Editor.Expanes.Insert(5, DescriptorDeExpansorPermisosPorCg(Editor));
                Editor.Expanes.Insert(6, DescriptorDeExpansorPermisosPorElemento(Editor));
            }


            //DefinirMf(menuEdicion, Editor.OpcionesMf);
        }

        private void DefinirMf(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<UsuarioDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<UsuarioDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.1' accion-menu='{eventosDeMf.MiCertificado}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Añadir mi certificado</li>");
        }


        private DescriptorDeExpansor DescriptorDeExpansorPuestosDeUnUsuario(DescriptorDeEdicion<UsuarioDto> editor)
        {
            var expansorDePts = new DescriptorDeExpansor(editor, $"{editor.Id}-puestos", "PTs del usuario", mostrarPlegado: true, "PT del usuario");

            //Definimos el grid de detalles del cuerpo
            var columnasDePts = new DescriptorDeColumnas("puestos");
            columnasDePts.Add(titulo: "CG", propiedad: nameof(PuestosDeUnUsuarioDto.CgDelPuesto), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: "Puesto", propiedad: nameof(PuestosDeUnUsuarioDto.Puesto), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: "Id", propiedad: nameof(PuestosDeUnUsuarioDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnasDePts.Add(titulo: nameof(PuestosDeUnUsuarioDto.IdPuesto), propiedad: nameof(PuestosDeUnUsuarioDto.IdPuesto), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PuestosDeUnUsuarioController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,PuestosDeUnUsuarioDtm, PuestosDeUnUsuarioDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PuestosDeUnUsuarioDto.IdUsuario) }
              , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PuestoDeTrabajoController)}/{nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo)}?id={nameof(PuestosDeUnUsuarioDto.IdPuesto)}"}
            };

            new GridDeRelacion(expansorDePts, columnasDePts, parametros)
            {
                PermitirBorrar = false
            };

            //expansorDePts.NavegarA(texto: "Gestionar PTs"
            //    , controlador: nameof(PuestoDeTrabajoController)
            //    , crud: nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo)
            //    , soloFiltra: true
            //    , propiedadRestrictora: nameof(PuestosDeUnUsuarioDto.IdUsuario)
            //    , idRestrictor: nameof(UsuarioDto.Id)
            //    , textoMostrar: nameof(UsuarioDto.NombreCompleto));

            return expansorDePts;
        }

        private DescriptorDeExpansor DescriptorDeExpansorPermisosPorElemento(DescriptorDeEdicion<UsuarioDto> editor)
        {
            var expansorDePd = new DescriptorDeExpansor(editor, $"{editor.Id}-por-elementos", "Permisos por elementos", mostrarPlegado: true, "Permisos por elementos");

            //Definimos el grid de detalles del cuerpo
            var columnasDePts = new DescriptorDeColumnas("permisos-por-elementos");
            columnasDePts.Add(titulo: "Negocio", propiedad: nameof(PermisosPorElementoDto.Negocio), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: "Elemento", propiedad: nameof(PermisosPorElementoDto.Elemento), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: "Permisos", propiedad: nameof(PermisosPorElementoDto.Permiso), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: "Heredado", propiedad: nameof(PermisosPorElementoDto.Calculado), enumAliniacion.derecha, mostrar: true, tamano: 100);
            columnasDePts.Add(titulo: "Id", propiedad: nameof(PermisosPorElementoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(PermisosPorElementoDto.Negocio)}.{nameof(NegocioDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}" + Simbolos.separadorDeClausulasDeOrdenacion +
                        $"{nameof(PermisosPorElementoDto.Permiso)}.{nameof(PermisoDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PermisosPorElementoController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,PermisosPorElementoDtm, PermisosPorElementoDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PermisosPorElementoDto.IdUsuario) }
              , { nameof(GridDeRelacion.OrdenarPor), orden }
            };

            new GridDeRelacion(expansorDePd, columnasDePts, parametros)
            {
                PermitirBorrar = Contexto.DatosDeConexion.EsAdministrador
            };

            expansorDePd.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(PermisosPorElementoDto), typeof(PermisosPorElementoController), nameof(PermisosPorElementoDto.IdUsuario), "Asociar un permiso");

            return expansorDePd;
        }

        private DescriptorDeExpansor DescriptorDeExpansorPermisosPorTipo(DescriptorDeEdicion<UsuarioDto> editor)
        {
            var expanDePermisosPorTipo = new DescriptorDeExpansor(editor, $"{editor.Id}-por-tipos", "Permisos por tipos", mostrarPlegado: true, "Permisos por tipos");

            //Definimos el grid de detalles del cuerpo
            var colDePermisosPorTipo = new DescriptorDeColumnas("permisos-por-tipo");
            colDePermisosPorTipo.Add(titulo: "Negocio", propiedad: nameof(PermisosPorTipoDto.Negocio), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 200);
            colDePermisosPorTipo.Add(titulo: "Tipo", propiedad: nameof(PermisosPorTipoDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true);
            colDePermisosPorTipo.Add(titulo: "Permisos", propiedad: nameof(PermisosPorTipoDto.Permiso), alineacion: enumAliniacion.izquierda, mostrar: true);
            colDePermisosPorTipo.Add(titulo: "Heredado", propiedad: nameof(PermisosPorTipoDto.Calculado), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 100);
            colDePermisosPorTipo.Add(titulo: "Id", propiedad: nameof(PermisosPorTipoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(PermisosPorTipoDto.Negocio)}.{nameof(NegocioDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}" + Simbolos.separadorDeClausulasDeOrdenacion +
                        $"{nameof(PermisosPorTipoDto.Permiso)}.{nameof(PermisoDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PermisosPorTipoController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,PermisosPorTipoDtm, PermisosPorTipoDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PermisosPorTipoDto.IdUsuario) }
              , { nameof(GridDeRelacion.OrdenarPor), orden }
            };

            new GridDeRelacion(expanDePermisosPorTipo, colDePermisosPorTipo, parametros)
            {
                PermitirBorrar = Contexto.DatosDeConexion.EsAdministrador
            };

            expanDePermisosPorTipo.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(PermisosPorTipoDto), typeof(PermisosPorTipoController), nameof(PermisosPorTipoDto.IdUsuario), "Asociar un permiso");

            return expanDePermisosPorTipo;
        }

        private DescriptorDeExpansor DescriptorDeExpansorPermisosPorNegocio(DescriptorDeEdicion<UsuarioDto> editor)
        {
            var expansorDePd = new DescriptorDeExpansor(editor, $"{editor.Id}-por-negocios", "Permisos por negocios", mostrarPlegado: true, "Permisos por negocios");

            //Definimos el grid de detalles del cuerpo
            var columnasDePts = new DescriptorDeColumnas("permisos-por-negocio");
            columnasDePts.Add(titulo: "Negocio", propiedad: nameof(PermisosPorNegocioDto.Negocio), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: "Permisos", propiedad: nameof(PermisosPorNegocioDto.Permiso), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: "Heredado", propiedad: nameof(PermisosPorNegocioDto.Calculado), alineacion: enumAliniacion.derecha, mostrar: true, tamano:100);
            columnasDePts.Add(titulo: "Id", propiedad: nameof(PermisosPorNegocioDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(PermisosPorNegocioDto.Negocio)}.{nameof(NegocioDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}" + Simbolos.separadorDeClausulasDeOrdenacion +
                        $"{nameof(PermisosPorNegocioDto.Permiso)}.{nameof(PermisoDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";
            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PermisosPorNegocioController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,PermisosPorNegocioDtm, PermisosPorNegocioDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PermisosPorNegocioDto.IdUsuario) }
              , { nameof(GridDeRelacion.OrdenarPor), orden }
            };

            new GridDeRelacion(expansorDePd, columnasDePts, parametros)
            {
                PermitirBorrar = Contexto.DatosDeConexion.EsAdministrador
            };

            expansorDePd.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(PermisosPorNegocioDto), typeof(PermisosPorNegocioController), nameof(PermisosPorNegocioDto.IdUsuario), "Asociar un permiso");

            return expansorDePd;
        }

        private DescriptorDeExpansor DescriptorDeExpansorPermisosPorEstado(DescriptorDeEdicion<UsuarioDto> editor)
        {
            var expansorDePermisosPorEstado = new DescriptorDeExpansor(editor, $"{editor.Id}-por-estados", "Permisos por estados", mostrarPlegado: true, "Permisos por estados");

            //Definimos el grid de detalles del cuerpo
            var columnasDePermisosPorEstados = new DescriptorDeColumnas("permisos-por-estados");
            columnasDePermisosPorEstados.Add(titulo: "Negocio", propiedad: nameof(PermisosPorEstadoDto.Negocio), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 200);
            columnasDePermisosPorEstados.Add(titulo: "Estado", propiedad: nameof(PermisosPorEstadoDto.Estado), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 250);
            columnasDePermisosPorEstados.Add(titulo: "Permisos", propiedad: nameof(PermisosPorEstadoDto.Permiso), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePermisosPorEstados.Add(titulo: "Heredado", propiedad: nameof(PermisosPorEstadoDto.Calculado), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 100);
            columnasDePermisosPorEstados.Add(titulo: "Id", propiedad: nameof(PermisosPorEstadoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(PermisosPorEstadoDto.Negocio)}.{nameof(NegocioDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}" + Simbolos.separadorDeClausulasDeOrdenacion +
                        $"{nameof(PermisosPorEstadoDto.Permiso)}.{nameof(PermisoDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PermisosPorEstadoController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,PermisosPorEstadoDtm, PermisosPorEstadoDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PermisosPorEstadoDto.IdUsuario) }
              , { nameof(GridDeRelacion.OrdenarPor), orden }
            };

            new GridDeRelacion(expansorDePermisosPorEstado, columnasDePermisosPorEstados, parametros)
            {
                PermitirBorrar = Contexto.DatosDeConexion.EsAdministrador
            };

            expansorDePermisosPorEstado.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(PermisosPorEstadoDto), typeof(PermisosPorEstadoController), nameof(PermisosPorEstadoDto.IdUsuario), "Asociar un permiso");

            return expansorDePermisosPorEstado;
        }
        private DescriptorDeExpansor DescriptorDeExpansorPermisosPorTransicion(DescriptorDeEdicion<UsuarioDto> editor)
        {
            var expansorDePermisosPorTransicion = new DescriptorDeExpansor(editor, $"{editor.Id}-por-transiciones", "Permisos por transiciones", mostrarPlegado: true, "Permisos por transiciones");

            //Definimos el grid de detalles del cuerpo
            var columnasDePermisosPorTransicions = new DescriptorDeColumnas("permisos-por-transiciones");
            columnasDePermisosPorTransicions.Add(titulo: "Negocio", propiedad: nameof(PermisosPorTransicionDto.Negocio), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 200);
            columnasDePermisosPorTransicions.Add(titulo: "Transicion", propiedad: nameof(PermisosPorTransicionDto.Transicion), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 250);
            columnasDePermisosPorTransicions.Add(titulo: "Permisos", propiedad: nameof(PermisosPorTransicionDto.Permiso), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePermisosPorTransicions.Add(titulo: "Heredado", propiedad: nameof(PermisosPorTransicionDto.Calculado), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 100);
            columnasDePermisosPorTransicions.Add(titulo: "Id", propiedad: nameof(PermisosPorTransicionDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(PermisosPorTransicionDto.Negocio)}.{nameof(NegocioDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}" + Simbolos.separadorDeClausulasDeOrdenacion +
                        $"{nameof(PermisosPorTransicionDto.Permiso)}.{nameof(PermisoDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PermisosPorTransicionController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,PermisosPorTransicionDtm, PermisosPorTransicionDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PermisosPorTransicionDto.IdUsuario) }
              , { nameof(GridDeRelacion.OrdenarPor), orden }
            };

            new GridDeRelacion(expansorDePermisosPorTransicion, columnasDePermisosPorTransicions, parametros)
            {
                PermitirBorrar = Contexto.DatosDeConexion.EsAdministrador
            };

            expansorDePermisosPorTransicion.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(PermisosPorTransicionDto), typeof(PermisosPorTransicionController), nameof(PermisosPorTransicionDto.IdUsuario), "Asociar un permiso");

            return expansorDePermisosPorTransicion;
        }

        private DescriptorDeExpansor DescriptorDeExpansorPermisosPorCg(DescriptorDeEdicion<UsuarioDto> editor)
        {
            var expansorDePermisosPorCg = new DescriptorDeExpansor(editor, $"{editor.Id}-por-cg", "Permisos por cg", mostrarPlegado: true, "Permisos por cg");

            var columnasDePermisosPorCg = new DescriptorDeColumnas("permisos-por-cg");
            columnasDePermisosPorCg.Add(titulo: "Centro Gestor", propiedad: nameof(PermisosPorCgDto.Cg), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 250);
            columnasDePermisosPorCg.Add(titulo: "Negocio", propiedad: nameof(PermisosPorCgDto.Negocio), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 200);
            columnasDePermisosPorCg.Add(titulo: "Permisos", propiedad: nameof(PermisosPorCgDto.Permiso), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePermisosPorCg.Add(titulo: "Heredado", propiedad: nameof(PermisosPorCgDto.Calculado), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 100);
            columnasDePermisosPorCg.Add(titulo: "Id", propiedad: nameof(PermisosPorCgDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(PermisosPorCgDto.Negocio)}.{nameof(NegocioDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}" + Simbolos.separadorDeClausulasDeOrdenacion +
                        $"{nameof(PermisosPorCgDto.Permiso)}.{nameof(PermisoDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PermisosPorCgController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,PermisosPorCgDtm, PermisosPorCgDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PermisosPorCgDto.IdUsuario) }
              , { nameof(GridDeRelacion.OrdenarPor), orden }
            };

            new GridDeRelacion(expansorDePermisosPorCg, columnasDePermisosPorCg, parametros)
            {
                PermitirBorrar = Contexto.DatosDeConexion.EsAdministrador
            };

            expansorDePermisosPorCg.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(PermisosPorCgDto), typeof(PermisosPorCgController), nameof(PermisosPorCgDto.IdUsuario), "Asociar un permiso");

            return expansorDePermisosPorCg;
        }

        public override string RenderControl()
        {
            var render = base.RenderControl(); // + RenderModalDeMiCertificado(Contexto, Negocio, Editor);

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Usuario.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                           Entorno.CrearCrudDeUsuarios('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el crud', error.message);
                         }}
                      </script>
                    ";
            return render.Render();
        }


        private static string RenderModalDeMiCertificado(ContextoSe contexto, enumNegocio negocio, DescriptorDeEdicion<UsuarioDto> editor)
        {
            var idHtml = $"modal-{editor.IdHtml}-mi-certificado";

            var eventos = $"Crud.{enumGestorDeEventos.EventosModalDeEdicion}";

            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros[EventosModal.TrasAbrir] = $"javascript: {enumNameSpaceTs.ApiDeCertificados}.{enumFunctionTs.InicializarModalMiCertificado}('{idHtml}')";
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalMiCertificado,
                idHtml: idHtml
                , controlador: nameof(UsuariosController)
                , tituloH2: editor.Etiqueta
                , cuerpo: DescriptorDeEdicion<MiCertificadoDto>.RenderContenedorDeEdicionCuerpo(editor, typeof(MiCertificadoDto), idHtml, nameof(UsuariosController), null, null)
                , idOpcion: $"{idHtml}-anular"
                , opcion: "Añadir"
                , accion: $"Javascript: {enumNameSpaceTs.ApiDeCertificados}.{enumFunctionTs.SubirMiCertificado}('{idHtml}')"
                , cerrar: $"{eventos}('{eventosDeEdicion.CerrarModal}','{idHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: enumModoDeAccesoDeDatos.Consultor
                , otrosAtributos: otros); 

            return htmlModal;
        }

    }
}
