using System.Collections.Generic;
using Utilidades;
using ModeloDeDto;
using ModeloDeDto.Seguridad;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using UtilidadesParaIu;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Entorno;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeRol : DescriptorDeCrud<RolDto>
    {
        public DescriptorDeRol(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(RolController), nameof(RolController.CrudRol), modo, enumNameSpaceTs.Seguridad)
        {
            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(PuestosDeUnRolController)
                , vista: nameof(PuestosDeUnRolController.CrudPuestosDeUnRol)
                , relacionarCon: nameof(PuestoDto)
                , navegarAlCrud: DescriptorDeMantenimiento<PuestosDeUnRolDto>.NombreMnt
                , nombreOpcion: "Puestos"
                , propiedadQueRestringe: nameof(RolDto.Id)
                , propiedadRestrictora: nameof(PuestosDeUnRolDto.IdRol)
                , "Incluir el rol a los puestos seleccionados");

            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(PermisosDeUnRolController)
                , vista: nameof(PermisosDeUnRolController.CrudPermisosDeUnRol)
                , relacionarCon: nameof(PermisoDto)
                , navegarAlCrud: DescriptorDeMantenimiento<PermisosDeUnRolDto>.NombreMnt
                , nombreOpcion: "Permisos"
                , propiedadQueRestringe: nameof(RolDto.Id)
                , propiedadRestrictora: nameof(PermisosDeUnRolDto.IdRol)
                , "Añadir permisos al rol seleccionado");

            new ListasDinamicas<RolDto>(Mnt.BloqueGeneral,
                 etiqueta: "Puesto",
                 filtrarPor: nameof(RolesDeUnPuestoDto.IdPuesto),
                 ayuda: "seleccione un puesto de trabajo",
                 seleccionarDe: nameof(PuestoDto),
                 buscarPor: nameof(PuestoDto.Expresion),
                 mostrarExpresion: $"[{nameof(PuestoDto.Expresion)}]",
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion(0,1),
                 controlador: nameof(PuestoDeTrabajoController),
                 navegarA: nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "") { LongitudMinimaParaBuscar = 1, AplicarJoin = true };

            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0,0), "Rol", "Buscar por denomonación del rol");

            new ListasDinamicas<RolDto>(Mnt.BloqueGeneral,
                                           etiqueta: "Permisos asociado",
                                           filtrarPor: nameof(PermisosDeUnRolDto.IdPermiso),
                                           ayuda: "roles con el permiso",
                                           seleccionarDe: nameof(PermisoDto),
                                           buscarPor: nameof(PermisoDto.Nombre),
                                           mostrarExpresion: nameof(PermisoDto.Nombre),
                                           criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                                           posicion: new Posicion(1, 0),
                                           controlador: nameof(PermisosController),
                                           navegarA: nameof(PermisosController.CrudPermiso),
                                           restringirPor: "",
                                           alSeleccionarBlanquearControl: "").LongitudMinimaParaBuscar = 3;

            new ListasDinamicas<RolDto>(Mnt.BloqueGeneral,
                 etiqueta: "Usuario",
                 filtrarPor: nameof(PuestosDeUnUsuarioDto.IdUsuario),
                 ayuda: "seleccione un usuario para ver sus roles",
                 seleccionarDe: nameof(UsuarioDto),
                 buscarPor: nameof(UsuarioDto.NombreCompleto),
                 mostrarExpresion: $"[{nameof(UsuarioDto.NombreCompleto)}]",
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion(1, 1),
                 controlador: nameof(UsuariosController),
                 navegarA: nameof(UsuariosController.CrudUsuario),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "").LongitudMinimaParaBuscar = 1;



            var expansorDePermisos = DescriptorDeExpansorRolPermisos(Editor);
            Editor.Expanes.Insert(0, expansorDePermisos);

            var expansorDePts = DescriptorDeExpansorRolPuesto(Editor);
            Editor.Expanes.Insert(1, expansorDePts);
        }

        private DescriptorDeExpansor DescriptorDeExpansorRolPermisos(DescriptorDeEdicion<RolDto> editor)
        {
            var expansorDePermisos = new DescriptorDeExpansor(editor, $"{editor.Id}-permisos", "Permisos de un rol", mostrarPlegado: true, "Permisos de un rol");

            //Definimos el grid de detalles del cuerpo
            var columnasDePermisos = new DescriptorDeColumnas("permisos");
            columnasDePermisos.Add(titulo: "Permiso", propiedad: nameof(PermisosDeUnRolDto.Permiso), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePermisos.Add(titulo: "Id", propiedad: nameof(PermisosDeUnRolDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnasDePermisos.Add(titulo: nameof(PermisosDeUnRolDto.IdPermiso), propiedad: nameof(PermisosDeUnRolDto.IdPermiso), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PermisosDeUnRolController)}
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,PermisosDeUnRolDtm, PermisosDeUnRolDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PermisosDeUnRolDto.IdRol) }
              , { nameof(GridDeRelacion.OrdenarPor), $"{nameof(PermisosDeUnRolDto.Permiso)}.{nameof(PermisoDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}"  }
              , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PermisosController)}/{nameof(PermisosController.CrudPermiso)}?id={nameof(PermisosDeUnRolDto.IdPermiso)}"}
            };

            new GridDeRelacion(expansorDePermisos, columnasDePermisos, parametros)
            {
                PermitirBorrar = Contexto.SePuedeParametrizar()
            };

           if (Contexto.SePuedeParametrizar()) expansorDePermisos.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(PermisosDeUnRolDto), typeof(PermisosDeUnRolController), nameof(PermisosDeUnRolDto.IdRol), "Añadir permiso");

            return expansorDePermisos;
        }

        private DescriptorDeExpansor DescriptorDeExpansorRolPuesto(DescriptorDeEdicion<RolDto> editor)
        {
            var expansorDePts = new DescriptorDeExpansor(editor, $"{editor.Id}-puestos", "PTs asociados al rol", mostrarPlegado: true, "PT que incluyen el rol");

            //Definimos el grid de detalles del cuerpo
            var columnasDePts = new DescriptorDeColumnas("puestos");
            columnasDePts.Add(titulo: "CG", propiedad: nameof(PuestosDeUnRolDto.CgDelPuesto), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: "Puesto", propiedad: nameof(PuestosDeUnRolDto.Puesto), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDePts.Add(titulo: nameof(PuestosDeUnRolDto.Id), propiedad: nameof(PuestosDeUnRolDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnasDePts.Add(titulo: nameof(PuestosDeUnRolDto.IdPuesto), propiedad: nameof(PuestosDeUnRolDto.IdPuesto), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(PuestosDeUnRolDto.Puesto)}.{nameof(PuestoDtm.Cg)}.{nameof(CentroGestorDtm.Codigo)}:{enumModoOrdenacion.ascendente.Render()}" + Simbolos.separadorDeClausulasDeOrdenacion +
                        $"{nameof(PuestosDeUnRolDto.Puesto)}.{nameof(PuestoDtm.Cg)}.{nameof(CentroGestorDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}" + Simbolos.separadorDeClausulasDeOrdenacion +
                        $"{nameof(PuestosDeUnRolDto.Puesto)}.{nameof(PuestoDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(PuestosDeUnRolController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,RolesDeUnPuestoDtm, PuestosDeUnRolDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PuestosDeUnRolDto.IdRol) }
              , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PuestoDeTrabajoController)}/{nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo)}?id={nameof(PuestosDeUnRolDto.IdPuesto)}"}
              , { nameof(GridDeRelacion.OrdenarPor), orden }
             };

            new GridDeRelacion(expansorDePts, columnasDePts, parametros)
            {
                PermitirBorrar = Contexto.SePuedeParametrizar()
            };

            if (Contexto.SePuedeParametrizar()) 
                expansorDePts.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(PuestosDeUnRolDto), typeof(PuestosDeUnRolController), nameof(PuestosDeUnRolDto.IdRol), "Incluirlo en el PT");

            return expansorDePts;
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Rol.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeRoles('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
