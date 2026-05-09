using System.Collections.Generic;
using Utilidades;
using ModeloDeDto;
using ModeloDeDto.Entorno;
using ModeloDeDto.Seguridad;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using UtilidadesParaIu;
using ServicioDeDatos.Elemento;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePuestoDeTrabajo : DescriptorDeCrud<PuestoDto>
    {
        public DescriptorDePuestoDeTrabajo(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(PuestoDeTrabajoController), nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo), modo, enumNameSpaceTs.Seguridad)
        {
            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(UsuariosDeUnPuestoController)
                , vista: nameof(UsuariosDeUnPuestoController.CrudUsuariosDeUnPuesto)
                , relacionarCon: nameof(UsuarioDto)
                , navegarAlCrud: DescriptorDeMantenimiento<UsuariosDeUnPuestoDto>.NombreMnt
                , nombreOpcion: "Usuarios"
                , propiedadQueRestringe: nameof(PuestoDto.Id)
                , propiedadRestrictora: nameof(UsuariosDeUnPuestoDto.IdPuesto)
                , "Incluir usuarios en el puesto seleccionado");

            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(RolesDeUnPuestoController)
                , vista: nameof(RolesDeUnPuestoController.CrudRolesDeUnPuesto)
                , relacionarCon: nameof(RolDto)
                , navegarAlCrud: DescriptorDeMantenimiento<RolesDeUnPuestoDto>.NombreMnt
                , nombreOpcion: "Roles"
                , propiedadQueRestringe: nameof(PuestoDto.Id)
                , propiedadRestrictora: nameof(RolesDeUnPuestoDto.IdPuesto)
                , "Añadir roles al puesto seleccionado");

            var modalDePermisos = new ModalDeConsultaDeRelaciones<PuestoDto, PermisosDeUnPuestoDto>(mantenimiento: Mnt
                              , tituloModal: "Permisos de un Puesto"
                              , crudModal: new DescriptorDePermisosHeredados(contexto, ModoDescriptor.ModalDeConsulta)
                              , propiedadRestrictora: nameof(PermisosDeUnPuestoDto.IdPuesto));

            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 0), "Puesto", "Buscar por denomonación de puesto, cg o código de cg");
            RecolocarControl(Mnt.Filtro.FiltroDeCg, new Posicion(1, 1), "CG", "Buscar por cg o código de cg");

            var listaUsuarios = new ListasDinamicas<PuestoDto>(Mnt.BloqueGeneral,
                 etiqueta: "Usuario",
                 filtrarPor: nameof(PuestosDeUnUsuarioDto.IdUsuario),
                 ayuda: "seleccione un usuario",
                 seleccionarDe: nameof(UsuarioDto),
                 buscarPor: nameof(UsuarioDto.NombreCompleto),
                 mostrarExpresion: $"[{nameof(UsuarioDto.NombreCompleto)}]",
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion(1, 0),
                 controlador: nameof(UsuariosController),
                 navegarA: nameof(UsuariosController.CrudUsuario),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "");
            listaUsuarios.LongitudMinimaParaBuscar = 1;

            var listaRol = new ListasDinamicas<PuestoDto>(Mnt.BloqueGeneral,
                  etiqueta: "Rol",
                  filtrarPor: nameof(RolesDeUnPuestoDto.IdRol),
                  ayuda: "seleccione el rol",
                  seleccionarDe: nameof(RolDto),
                  buscarPor: nameof(RolDto.Nombre),
                  mostrarExpresion: $"[{nameof(RolDto.Nombre)}]",
                  criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                  posicion: new Posicion(0, 2),
                  controlador: nameof(RolController),
                  navegarA: nameof(RolController.CrudRol),
                  restringirPor: "",
                  alSeleccionarBlanquearControl: "");
            listaRol.LongitudMinimaParaBuscar = 1;

            new ListasDinamicas<PuestoDto>(Mnt.BloqueGeneral,
                 etiqueta: "Permisos",
                 filtrarPor: nameof(PermisosDeUnPuestoDto.IdPermiso),
                 ayuda: "seleccione un permiso",
                 seleccionarDe: nameof(PermisoDto),
                 buscarPor: nameof(PermisoDto.Nombre),
                 mostrarExpresion: $"[{nameof(PermisoDto.Nombre)}]",
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion(0, 1),
                 controlador: nameof(PermisosController),
                 navegarA: nameof(PermisosController.CrudPermiso),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "").LongitudMinimaParaBuscar = 1;

            Mnt.BloqueGeneral.Tabla.AnchoDeEtiqueta = 5;



            Mnt.OrdenacionInicial = @$"{nameof(PuestoDto.Cg)}:cg.codigo:{enumModoOrdenacion.ascendente.Render()};
                                       {nameof(PuestoDto.Nombre)}:nombre:{enumModoOrdenacion.ascendente.Render()}";


            var mostrarPermisos = new ConsultarRelaciones(modalDePermisos.IdHtml, () => modalDePermisos.RenderControl(), "Mostrar los permisos de un puesto de trabajo");
            var opcion = new OpcionDeMenu<PuestoDto>(Mnt.ZonaMenu.Menu, mostrarPermisos, $"Permisos", enumModoDeAccesoDeDatos.Consultor);
            Mnt.ZonaMenu.Menu.Add(opcion);

            var expansorDeRoles = DescriptorDeExpansorPtRoles(Editor);
            Editor.Expanes.Insert(0, expansorDeRoles);

            var expansorDePemisosHeredados = DescriptorDeExpansorDePermisosHeredados(Editor);
            Editor.Expanes.Insert(1, expansorDePemisosHeredados);

            var expansorDePemisosAsignados = DescriptorDeExpansorDePermisosDirectos(Editor);
            Editor.Expanes.Insert(2, expansorDePemisosAsignados);

            var expansorDeUsuarios = DescriptorDeExpansorUsuarioDeUnPt(Editor);
            Editor.Expanes.Insert(3, expansorDeUsuarios);
        }

        private DescriptorDeExpansor DescriptorDeExpansorPtRoles(DescriptorDeEdicion<PuestoDto> editor)
        {
            var expansorDeRoles = new DescriptorDeExpansor(editor, $"{editor.Id}-roles", "Roles de un P.T.", mostrarPlegado: true, "Roles de un puesto de trabajo");

            //Definimos el grid de detalles del cuerpo
            var columnasDeRoles = new DescriptorDeColumnas("roles");
            columnasDeRoles.Add(titulo: "Rol", propiedad: nameof(RolesDeUnPuestoDto.Rol), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeRoles.Add(titulo: nameof(RolesDeUnPuestoDto.Id), propiedad: nameof(RolesDeUnPuestoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnasDeRoles.Add(titulo: nameof(RolesDeUnPuestoDto.IdRol), propiedad: nameof(RolesDeUnPuestoDto.IdRol), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(RolesDeUnPuestoController)}
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,RolesDeUnPuestoDtm, RolesDeUnPuestoDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(RolesDeUnPuestoDto.IdPuesto)}
              , { nameof(GridDeRelacion.OrdenarPor), $"{nameof(RolesDeUnPuestoDto.Rol)}.{nameof(RolDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}"  }
              , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(RolController)}/{nameof(RolController.CrudRol)}?id={nameof(RolesDeUnPuestoDto.IdRol)}"}
            };

            var grid = new GridDeRelacion(expansorDeRoles, columnasDeRoles, parametros)
            {
                PermitirBorrar = Contexto.SePuedeParametrizar()
            };

            if (Contexto.SePuedeParametrizar())
                expansorDeRoles.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(RolesDeUnPuestoDto), typeof(RolesDeUnPuestoController), nameof(RolesDeUnPuestoDto.IdPuesto), "Incluir rol");

            return expansorDeRoles;
        }

        private DescriptorDeExpansor DescriptorDeExpansorDePermisosDirectos(DescriptorDeEdicion<PuestoDto> editor)
        {
            var expansorDePermisos = new DescriptorDeExpansor(editor, $"{editor.Id}-asignados", "Permisos directos", mostrarPlegado: true, "Permisos asignados a un puesto de trabajo");

            //Definimos el grid de detalles del cuerpo
            var columnasDeRoles = new DescriptorDeColumnas("asignados");
            columnasDeRoles.Add(titulo: "Permiso directo", propiedad: nameof(PermisosDeUnPuestoDto.Permiso), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeRoles.Add(titulo: nameof(PermisosDeUnPuestoDto.Id), propiedad: nameof(PermisosDeUnPuestoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnasDeRoles.Add(titulo: nameof(PermisosDeUnPuestoDto.IdPermiso), propiedad: nameof(PermisosDeUnPuestoDto.IdPermiso), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PermisosDirectosController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,PermisosDirectosDtm, PermisosDeUnPuestoDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PermisosDeUnPuestoDto.IdPuesto) }
              , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PermisosController)}/{nameof(PermisosController.CrudPermiso)}?id={nameof(PermisosDeUnPuestoDto.IdPermiso)}"}
              , { nameof(GridDeRelacion.OrdenarPor) , $"{nameof(PermisosDirectosDtm.Permiso)}.{nameof(INombre.Nombre)}:{enumModoOrdenacion.ascendente}"}
            };

            new GridDeRelacion(expansorDePermisos, columnasDeRoles, parametros)
            {
                PermitirBorrar = Contexto.SePuedeParametrizar()
            };

            if (Contexto.SePuedeParametrizar())
                expansorDePermisos.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(PermisosDeUnPuestoDto), typeof(PermisosDirectosController), nameof(PermisosDeUnPuestoDto.IdPuesto), "Asignar permiso");
            
            return expansorDePermisos;
        }

        private DescriptorDeExpansor DescriptorDeExpansorDePermisosHeredados(DescriptorDeEdicion<PuestoDto> editor)
        {
            var expansorDePermisos = new DescriptorDeExpansor(editor, $"{editor.Id}-heredados", "Permisos de roles", mostrarPlegado: true, "Permisos heredados de los roles");

            //Definimos el grid de detalles del cuerpo
            var columnasDeRoles = new DescriptorDeColumnas("heredados");
            columnasDeRoles.Add(titulo: "Permiso heredado", propiedad: nameof(PermisosDeUnPuestoDto.Permiso), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeRoles.Add(titulo: "Roles", propiedad: nameof(PermisosDeUnPuestoDto.Roles), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeRoles.Add(titulo: nameof(PermisosDeUnPuestoDto.Id), propiedad: nameof(PermisosDeUnPuestoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnasDeRoles.Add(titulo: nameof(PermisosDeUnPuestoDto.IdPermiso), propiedad: nameof(PermisosDeUnPuestoDto.IdPermiso), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PermisosHeredadosController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,PermisosHeredadosDtm, PermisosDeUnPuestoDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PermisosDeUnPuestoDto.IdPuesto) }
              , { nameof(GridDeRelacion.OrdenarPor), $"{nameof(PermisosHeredadosDtm.Permiso)}.{nameof(PermisoDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}"  }
              , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PermisosController)}/{nameof(PermisosController.CrudPermiso)}?id={nameof(PermisosDeUnPuestoDto.IdPermiso)}"}
            };

            new GridDeRelacion(expansorDePermisos, columnasDeRoles, parametros)
            {
                PermitirBorrar = false
            };

            return expansorDePermisos;
        }

        private DescriptorDeExpansor DescriptorDeExpansorUsuarioDeUnPt(DescriptorDeEdicion<PuestoDto> editor)
        {
            var expansorDePermisos = new DescriptorDeExpansor(editor, $"{editor.Id}-usuarios", "Usuarios de un PT", mostrarPlegado: true, "Usuarios de un PT");

            //Definimos el grid de detalles del cuerpo
            var columnasDeUsuarios = new DescriptorDeColumnas("usuarios");
            columnasDeUsuarios.Add(titulo: "Usuario", propiedad: nameof(UsuariosDeUnPuestoDto.Usuario), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeUsuarios.Add(titulo: nameof(UsuariosDeUnPuestoDto.Id), propiedad: nameof(UsuariosDeUnPuestoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnasDeUsuarios.Add(titulo: nameof(UsuariosDeUnPuestoDto.IdUsuario), propiedad: nameof(UsuariosDeUnPuestoDto.IdUsuario), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(UsuariosDeUnPuestoController)}
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,PuestosDeUnUsuarioDtm, UsuariosDeUnPuestoDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(UsuariosDeUnPuestoDto.IdPuesto) }
              , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(UsuariosController)}/{nameof(UsuariosController.CrudUsuario)}?id={nameof(UsuariosDeUnPuestoDto.IdUsuario)}"}
            };

            new GridDeRelacion(expansorDePermisos, columnasDeUsuarios, parametros)
            {
                PermitirBorrar = Contexto.SePuedeParametrizar()
            };

            if (Contexto.SePuedeParametrizar())
                expansorDePermisos.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(UsuariosDeUnPuestoDto), typeof(UsuariosDeUnPuestoController), nameof(UsuariosDeUnPuestoDto.IdPuesto), "Añadir usuario");
          
            return expansorDePermisos;
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/PuestoDeTrabajo.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDePuestosDeTrabajo('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
