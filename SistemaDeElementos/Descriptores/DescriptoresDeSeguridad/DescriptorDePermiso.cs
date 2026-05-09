using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using ModeloDeDto.Seguridad;
using ModeloDeDto.Entorno;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos;
using ModeloDeDto.Terceros;
using ModeloDeDto;
using Utilidades;
using System.Collections.Generic;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Entorno;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePermiso : DescriptorDeCrud<PermisoDto>
    {
        public DescriptorDePermiso(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , controlador: nameof(PermisosController), vista: nameof(PermisosController.CrudPermiso), modo: modo, enumNameSpaceTs.Seguridad)
        {
            if (modo == ModoDescriptor.Mantenimiento)
            {
                var modalUsuario = new DescriptorDeUsuario(contexto, ModoDescriptor.SeleccionarParaFiltrar);

                RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 0), "Permiso", "buscar por nombre del permiso");

                var listaUsuarios = new ListasDinamicas<PermisoDto>(Mnt.BloqueGeneral,
                     etiqueta: "Usuario",
                     filtrarPor: nameof(PuestosDeUnUsuarioDto.IdUsuario),
                     ayuda: "seleccione un usuario",
                     seleccionarDe: nameof(UsuarioDto),
                     buscarPor: nameof(UsuarioDto.NombreCompleto),
                     mostrarExpresion: $"[{nameof(UsuarioDto.NombreCompleto)}]",
                     criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                     posicion: new Posicion(0, 1),
                     controlador: nameof(UsuariosController),
                     navegarA: nameof(UsuariosController.CrudUsuario),
                     restringirPor: "",
                     alSeleccionarBlanquearControl: "");
                listaUsuarios.LongitudMinimaParaBuscar = 1;

                var listaCg = new ListasDinamicas<PermisoDto>(Mnt.BloqueGeneral,
                     etiqueta: "C.G.",
                     filtrarPor: nameof(NegociosDeUnCgDto.IdCg),
                     ayuda: "seleccione el centro gestor",
                     seleccionarDe: nameof(CentroGestorDto),
                     buscarPor: nameof(CentroGestorDto.Nombre),
                     mostrarExpresion: $"([{nameof(CentroGestorDto.Codigo)}]) [{nameof(CentroGestorDto.Nombre)}]",
                     criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                     posicion: new Posicion(0, 2),
                     controlador: nameof(CentrosGestoresController),
                     navegarA: nameof(CentrosGestoresController.CrudCentrosGestores),
                     restringirPor: "",
                     alSeleccionarBlanquearControl: "");
                listaCg.LongitudMinimaParaBuscar = 1;

                new ListasDinamicas<PermisoDto>(Mnt.BloqueGeneral,
                     etiqueta: "Puesto",
                     filtrarPor: nameof(PermisosDeUnPuestoDto.IdPuesto),
                     ayuda: "seleccione un puesto de trabajo",
                     seleccionarDe: nameof(PuestoDto),
                     buscarPor: nameof(PuestoDto.Expresion),
                     mostrarExpresion: $"[{nameof(PuestoDto.Expresion)}]",
                     criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                     posicion: new Posicion(1, 0),
                     controlador: nameof(PuestoDeTrabajoController),
                     navegarA: nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo),
                     restringirPor: "",
                     alSeleccionarBlanquearControl: "")
                { LongitudMinimaParaBuscar = 1, AplicarJoin = true };

                new ListaDeElemento<PermisoDto>(Mnt.BloqueGeneral,
                                                etiqueta: "Clase de permiso",
                                                ayuda: "selecciona una clase",
                                                seleccionarDe: nameof(ClasePermisoDto),
                                                filtraPor: nameof(PermisoDto.IdClase),
                                                mostrarExpresion: ClasePermisoDto.MostrarExpresion,
                                                posicion: new Posicion() { fila = 1, columna = 1 },
                                                nameof(ClaseDePermisoController));

                new ListaDeElemento<PermisoDto>(Mnt.BloqueGeneral,
                                                etiqueta: "Tipo de permiso",
                                                ayuda: "selecciona un tipo",
                                                seleccionarDe: nameof(TipoPermisoDto),
                                                filtraPor: nameof(PermisoDto.IdTipo),
                                                mostrarExpresion: nameof(TipoPermisoDto.Nombre),
                                                posicion: new Posicion() { fila = 1, columna = 2 },
                                                nameof(TipoDePermisoController));

                new ListasDinamicas<PermisoDto>(Mnt.BloqueGeneral,
                      etiqueta: "Rol",
                      filtrarPor: nameof(RolesDeUnPermisoDto.IdRol),
                      ayuda: "seleccione el rol",
                      seleccionarDe: nameof(RolDto),
                      buscarPor: nameof(RolDto.Nombre),
                      mostrarExpresion: $"[{nameof(RolDto.Nombre)}]",
                      criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                      posicion: new Posicion(2, 0),
                      controlador: nameof(RolController),
                      navegarA: nameof(RolController.CrudRol),
                      restringirPor: "",
                      alSeleccionarBlanquearControl: "").LongitudMinimaParaBuscar = 1;

                AnadirOpciondeRelacion(Mnt
                    , controlador: nameof(RolesDeUnPermisoController)
                    , vista: nameof(RolesDeUnPermisoController.CrudRolesDeUnPermiso)
                    , relacionarCon: nameof(RolDto)
                    , navegarAlCrud: DescriptorDeMantenimiento<RolesDeUnPermisoDto>.NombreMnt
                    , nombreOpcion: "Roles"
                    , propiedadQueRestringe: nameof(PermisoDto.Id)
                    , propiedadRestrictora: nameof(PermisosDeUnRolDto.IdPermiso)
                    , "Añadir roles al permiso seleccionado");

            }

            Editor.Expanes.Insert(0, DescriptorDeExpansorPtsHeredados(Editor));
            Editor.Expanes.Insert(1, DescriptorDeExpansorPtsDirectos(Editor));
            Editor.Expanes.Insert(2, DescriptorDeExpansorRolesDeUnPermiso(Editor));
            Editor.Expanes.Insert(3, DescriptorDeExpansorUsuariosDeUnPermiso(Editor));
        }

        private DescriptorDeExpansor DescriptorDeExpansorUsuariosDeUnPermiso(DescriptorDeEdicion<PermisoDto> editor)
        {
            var expansorDeUsuarios = new DescriptorDeExpansor(editor, $"{editor.Id}-usuarios", "Usuarios de un permiso", mostrarPlegado: true, "Usuarios de un permiso");

            //Definimos el grid de detalles del cuerpo
            var columnasDeUsuarios = new DescriptorDeColumnas("usuarios");
            columnasDeUsuarios.Add(titulo: "Usuario", propiedad: nameof(PermisosDeUnUsuarioDto.Usuario), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeUsuarios.Add(titulo: nameof(PermisosDeUnUsuarioDto.Id), propiedad: nameof(PermisosDeUnUsuarioDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnasDeUsuarios.Add(titulo: nameof(PermisosDeUnUsuarioDto.IdUsuario), propiedad: nameof(PermisosDeUnUsuarioDto.IdUsuario), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PermisosDeUnUsuarioController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,UsuariosDeUnPermisoDtm, PermisosDeUnUsuarioDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PermisosDeUnUsuarioDto.IdPermiso) }
              , { nameof(GridDeRelacion.OrdenarPor), $"{nameof(PermisosDeUnUsuarioDto.Usuario)}.{nameof(UsuarioDtm.Login)}:{enumModoOrdenacion.ascendente.Render()}"  }
              , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(UsuariosController)}/{nameof(UsuariosController.CrudUsuario)}?id={nameof(PermisosDeUnUsuarioDto.IdUsuario)}"}
            };

            new GridDeRelacion(expansorDeUsuarios, columnasDeUsuarios, parametros)
            {
                PermitirBorrar = false
            };

            return expansorDeUsuarios;
        }

        private DescriptorDeExpansor DescriptorDeExpansorPtsHeredados(DescriptorDeEdicion<PermisoDto> editor)
        {
            var expansorDePts = new DescriptorDeExpansor(editor, $"{editor.Id}-heredados", "Puestos de trabajo que heredan el permiso", mostrarPlegado: true, "PTs que han heredado el permiso");

            //Definimos el grid de detalles del cuerpo
            var columnasDePts = new DescriptorDeColumnas("pts-heredados");
            columnasDePts.Add(titulo: "CG", propiedad: nameof(PermisosDeUnPuestoDto.CgDelPuesto), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: "Puesto", propiedad: nameof(PermisosDeUnPuestoDto.Puesto), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: "Roles", propiedad: nameof(PermisosDeUnPuestoDto.Roles), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: nameof(PermisosDeUnPuestoDto.Id), propiedad: nameof(PermisosDeUnPuestoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnasDePts.Add(titulo: nameof(PermisosDeUnPuestoDto.IdPuesto), propiedad: nameof(PermisosDeUnPuestoDto.IdPuesto), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(PermisosDeUnPuestoDto.Puesto)}.{nameof(PuestoDtm.Cg)}.{nameof(CentroGestorDtm.Codigo)}:{enumModoOrdenacion.ascendente.Render()}" +
                       $";{nameof(PermisosDeUnPuestoDto.Puesto)}.{nameof(PuestoDtm.Cg)}.{nameof(CentroGestorDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}" +
                       $";{nameof(PermisosDeUnPuestoDto.Permiso)}.{nameof(PermisoDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PermisosHeredadosController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,PermisosHeredadosDtm, PermisosDeUnPuestoDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PermisosDeUnPuestoDto.IdPermiso) }
              , { nameof(GridDeRelacion.OrdenarPor), orden }
              , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PuestoDeTrabajoController)}/{nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo)}?id={nameof(PermisosDeUnPuestoDto.IdPuesto)}"}
             };

            new GridDeRelacion(expansorDePts, columnasDePts, parametros) { PermitirBorrar = false };


            return expansorDePts;
        }

        private DescriptorDeExpansor DescriptorDeExpansorPtsDirectos(DescriptorDeEdicion<PermisoDto> editor)
        {
            var expansorDePts = new DescriptorDeExpansor(editor, $"{editor.Id}-directos", "PTs relacionados con el permiso", mostrarPlegado: true, "PTs a los que se ha asignado el permiso");

            //Definimos el grid de detalles del cuerpo
            var columnasDePts = new DescriptorDeColumnas("pts-directos");
            columnasDePts.Add(titulo: "CG", propiedad: nameof(PuestosDeUnPermisoDto.CgDelPuesto), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: "Puesto", propiedad: nameof(PuestosDeUnPermisoDto.Puesto), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: nameof(PuestosDeUnPermisoDto.Id), propiedad: nameof(PuestosDeUnPermisoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnasDePts.Add(titulo: nameof(PuestosDeUnPermisoDto.IdPuesto), propiedad: nameof(PuestosDeUnPermisoDto.IdPuesto), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(PuestosDeUnPermisoDto.Puesto)}.{nameof(PuestoDtm.Cg)}.{nameof(CentroGestorDtm.Codigo)}:{enumModoOrdenacion.ascendente.Render()}" +
                       $"{Simbolos.separadorDeClausulasDeOrdenacion}{nameof(PuestosDeUnPermisoDto.Puesto)}.{nameof(PuestoDtm.Cg)}.{nameof(CentroGestorDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}" +
                       $"{Simbolos.separadorDeClausulasDeOrdenacion}{nameof(PuestosDeUnPermisoDto.Permiso)}.{nameof(PermisoDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PermisosDirectosController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,PermisosDirectosDtm, PermisosDeUnPuestoDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PuestosDeUnPermisoDto.IdPermiso) }
              , { nameof(GridDeRelacion.OrdenarPor), orden }
              , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PuestoDeTrabajoController)}/{nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo)}?id={nameof(PuestosDeUnPermisoDto.IdPuesto)}"}
             };

            new GridDeRelacion(expansorDePts, columnasDePts, parametros)
            {
                PermitirBorrar = Contexto.SePuedeParametrizar()
            };

            if (Contexto.SePuedeParametrizar()) expansorDePts.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(PuestosDeUnPermisoDto), typeof(PermisosDirectosController), nameof(PuestosDeUnPermisoDto.IdPermiso), "Incuir el permiso en un PT", ocultarEnConsulta: true);

            return expansorDePts;
        }

        private DescriptorDeExpansor DescriptorDeExpansorRolesDeUnPermiso(DescriptorDeEdicion<PermisoDto> editor)
        {
            var expansorDeRoles = new DescriptorDeExpansor(editor, $"{editor.Id}-roles", "Roles de un permiso", mostrarPlegado: true, "Roles de un permiso");

            //Definimos el grid de detalles del cuerpo
            var columnasDeRoles = new DescriptorDeColumnas("roles");
            columnasDeRoles.Add(titulo: "Rol", propiedad: nameof(RolesDeUnPermisoDto.Rol), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeRoles.Add(titulo: nameof(RolesDeUnPermisoDto.Id), propiedad: nameof(RolesDeUnPermisoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnasDeRoles.Add(titulo: nameof(RolesDeUnPermisoDto.IdRol), propiedad: nameof(RolesDeUnPermisoDto.IdRol), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(RolesDeUnPermisoController)}
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,PermisosDeUnRolDtm, RolesDeUnPermisoDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(RolesDeUnPermisoDto.IdPermiso)}
              , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(RolController)}/{nameof(RolController.CrudRol)}?id={nameof(RolesDeUnPermisoDto.IdRol)}"}
              , { nameof(GridDeRelacion.OrdenarPor), $"{nameof(RolesDeUnPuestoDto.Rol)}.{nameof(RolDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}"  }
            };

            new GridDeRelacion(expansorDeRoles, columnasDeRoles, parametros)
            {
                PermitirBorrar = Contexto.SePuedeParametrizar()
            };

            if (Contexto.SePuedeParametrizar())
            {
                expansorDeRoles.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(RolesDeUnPermisoDto), typeof(RolesDeUnPermisoController), nameof(RolesDeUnPermisoDto.IdPermiso), "Incluir rol");

                expansorDeRoles.NavegarA(texto: "Gestionar Roles"
                    , controlador: nameof(RolController)
                    , crud: nameof(RolController.CrudRol)
                    , soloFiltra: true
                    , propiedadRestrictora: nameof(RolesDeUnPermisoDto.IdPermiso)
                    , idRestrictor: nameof(PermisoDto.Id)
                    , textoMostrar: nameof(PermisoDto.Nombre));
            }


            return expansorDeRoles;
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
                return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Permisos.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDePermisos('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
