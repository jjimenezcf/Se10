using System.Collections.Generic;
using System.Linq;
using ModeloDeDto;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeClientes : DescriptorDeCrud<ClienteDto>
    {
        public DescriptorDeClientes(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }
        public DescriptorDeClientes(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , controlador: nameof(ClientesController)
              , vista: nameof(ClientesController.CrudClientes)
              , modo: modo
               , rutaBase: enumNameSpaceTs.Terceros)
        {

            Mnt.OrdenacionInicial = @$"{nameof(ClienteDto.Expresion)}:{nameof(ClienteDto.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 0), ltrCliente.Cliente, "Buscar por nif, apellido, nombre, mail, teléfono");

            var listaPersona = new ListasDinamicas<ClienteDto>(Mnt.BloqueGeneral,
                etiqueta: "Persona",
                filtrarPor: nameof(ltrCliente.IdPersona),
                ayuda: "seleccione una persona",
                seleccionarDe: nameof(PersonaDto),
                buscarPor: nameof(PersonaDto.Expresion),
                mostrarExpresion: $"[{nameof(PersonaDto.Expresion)}]",
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(PersonasController),
                navegarA: nameof(PersonasController.CrudPersonas),
                restringirPor: "",
                alSeleccionarBlanquearControl: "");
            listaPersona.LongitudMinimaParaBuscar = 1;

            var listaSociedad = new ListasDinamicas<ClienteDto>(Mnt.BloqueGeneral,
                etiqueta: "Sociedad",
                filtrarPor: nameof(ltrCliente.IdSociedad),
                ayuda: "seleccione una sociedad",
                seleccionarDe: nameof(SociedadDto),
                buscarPor: nameof(SociedadDto.Expresion),
                mostrarExpresion: $"[{nameof(SociedadDto.Expresion)}]",
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 1),
                controlador: nameof(SociedadesController),
                navegarA: nameof(SociedadesController.CrudSociedades),
                restringirPor: "",
                alSeleccionarBlanquearControl: "");
            listaSociedad.LongitudMinimaParaBuscar = 1;

            var listaInter = new ListasDinamicas<ClienteDto>(Mnt.BloqueGeneral,
                etiqueta: "Interlocutor",
                filtrarPor: nameof(ClienteDto.IdInterlocutor),
                ayuda: "seleccione un interlocutor",
                seleccionarDe: nameof(InterlocutorDto),
                buscarPor: nameof(InterlocutorDto.Expresion),
                mostrarExpresion: $"[{nameof(InterlocutorDto.Expresion)}]",
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(0, 1),
                controlador: nameof(InterlocutoresController),
                navegarA: nameof(InterlocutoresController.CrudInterlocutores),
                restringirPor: "",
                alSeleccionarBlanquearControl: "");
            listaInter.LongitudMinimaParaBuscar = 1;


            Mnt.ZonaMenu.OpcionesDesplegables = new Dictionary<string, string>
            {
                { "-1", "Opciones de creación" },
                { enumOpcionDeMenuCliente.CrearSociedad, "Crear sociedad" },
                { enumOpcionDeMenuCliente.CrearPersona, "Crear persona" }
            };
            Mnt.ZonaMenu.ProcesarOpcionesDesplegables = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Cli_ProcesarOpcionDeMenuLista}()";

            DescriptorDeEdicion<ClienteDto>.IncluirMfIndividual(Editor.OpcionesMf, "<hr>");
            Editor.IncluirMfIndividual("Añadir Centro Administrativo", eventosDeMf.Cli_CentroAdministrativo, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);
            Editor.IncluirMfIndividual("Añadir Puesto de trabajo", eventosDeMf.Cli_PuestoDeTrabajo, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);
            Editor.IncluirMfIndividual("Añadir Usuario web", eventosDeMf.Cli_NuevoClienteWeb, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);
            Editor.IncluirMfIndividual("Validar Nif en AEAT", eventosDeMf.Cli_ValidarNif, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor);

            DescriptorDeCuentasDeCliente();
            DescriptorDeCentroAdministrativo();
            DescriptorDeExpedientesDeCliente();
            DescriptorDeUsuariosDeCliente();
            DescriptorDePuestosDeCliente();

            var archivadorExpan = Editor.Expanes.First(x => x.Id == $"{Editor.Id}-archivadores");
            //archivadorExpan.GidDeRelacion.acciones
        }


        private void DescriptorDeCuentasDeCliente()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-CuentasDeCliente", "Cuentas bancarias", true, "Cuentas bancarias del cliente");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("CuentasDeCliente");
            columnas.Add(titulo: nameof(CuentaDeClienteDto.Banco), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: nameof(CuentaDeClienteDto.Alias), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: nameof(CuentaDeClienteDto.Cuenta), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: nameof(CuentaDeClienteDto.Clase), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: nameof(CuentaDeClienteDto.Activa), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: nameof(CuentaDeClienteDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: nameof(CuentaDeClienteDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(CuentaDeClienteDto.Id)}:{enumModoOrdenacion.descendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(CuentasDeClienteController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(CuentasDeClienteController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(CuentaDeClienteDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(CuentaDeClienteDto), typeof(CuentasDeClienteController), nameof(CuentaDeClienteDto.IdElemento), "Añadir cuenta");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Cli_InicializarModalParaCrearCuentas}('{modalDeCreacion.IdHtml}')";
            modalDeCreacion.AccionTrasCrear = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Cli_RecargarGridDeArchivos}()";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(CuentaDeClienteDto), typeof(CuentasDeClienteController), "Consultar cuenta", soloConsulta: false);
            modalDeEdicion.AccionTrasModificar = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Cli_RecargarGridDeArchivos}()";
        }

        private void DescriptorDeCentroAdministrativo()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-centro-administrativo", "Centros administrativos", true, "Centros administrativos del cliente");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("centro-administrativo");
            columnas.Add(titulo: "Organo gestor", nameof(CentroAdministrativoDto.OrganoGestor));
            columnas.Add(titulo: "Unidad tramitadora", nameof(CentroAdministrativoDto.UnidadTramitadora));
            columnas.Add(titulo: "Oficina contable", nameof(CentroAdministrativoDto.OficinaContable));
            columnas.Add(titulo: "Organo Gestor", nameof(CentroAdministrativoDto.Contacto));
            columnas.Add(titulo: nameof(CentroAdministrativoDto.Activa), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: nameof(CentroAdministrativoDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: nameof(CentroAdministrativoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(CentroAdministrativoDto.Id)}:{enumModoOrdenacion.descendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(CentrosAdministrativosController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(CentrosAdministrativosController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(CentroAdministrativoDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(CentroAdministrativoDto), typeof(CentrosAdministrativosController), nameof(CentroAdministrativoDto.IdElemento), "Añadir centro");
            //modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Cli_InicializarModalParaCrearCentros('{modalDeCreacion.IdHtml}')";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(CentroAdministrativoDto), typeof(CentrosAdministrativosController), "Consultar centro", soloConsulta: false);
        }


        private void DescriptorDeExpedientesDeCliente()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-ExpedientesDeCliente", "Expedientes", true, "Expedientes del cliente");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(1, expansor);

            var columnas = new DescriptorDeColumnas("ExpedientesDeCliente");
            columnas.Add(titulo: nameof(ExpedienteDeClienteDto.Expediente));
            columnas.Add(titulo: nameof(ExpedienteDeClienteDto.Cg));
            columnas.Add(titulo: nameof(ExpedienteDeClienteDto.Tipo));
            columnas.Add(titulo: nameof(ExpedienteDeClienteDto.Estado));
            columnas.Add(titulo: nameof(ExpedienteDeClienteDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(ExpedienteDeClienteDto.IdExpediente), mostrar: false);
            columnas.Add(titulo: nameof(ExpedienteDeClienteDto.Id), mostrar: false);

            var orden = $"{nameof(ExpedienteDeClienteDto.Id)}:{enumModoOrdenacion.descendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(ExpedientesDeClienteController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(ExpedientesDeClienteController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ExpedienteDeClienteDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(ExpedientesController)}/{nameof(ExpedientesController.CrudExpedientes)}?id={nameof(ExpedienteDeClienteDto.IdExpediente)}"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = false;

        }

        private void DescriptorDeUsuariosDeCliente()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-UsuariosDeCliente", "Usuarios", true, "Usuarios del cliente");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(2, expansor);

            var columnas = new DescriptorDeColumnas("UsuariosDeCliente");
            columnas.Add(titulo: nameof(UsuarioDeClienteDto.Usuario));
            columnas.Add(titulo: nameof(UsuarioDeClienteDto.Activo), alineacion: enumAliniacion.derecha);
            columnas.Add(titulo: nameof(UsuarioDeClienteDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(UsuarioDeClienteDto.IdUsuario), mostrar: false);
            columnas.Add(titulo: nameof(UsuarioDeClienteDto.Id), mostrar: false);

            var orden = $"{nameof(UsuarioDeClienteDto.Id)}:{enumModoOrdenacion.descendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(UsuariosDeClienteController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(UsuariosDeClienteController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(UsuariosController)}/{nameof(UsuariosController.CrudUsuario)}?id={nameof(UsuarioDeClienteDto.IdUsuario)}"}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(UsuarioDeClienteDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;
            gridDeRelacion.PermitirEditar = false;


            var modal = expansor.DescriptorDeCrearDetalles(Editor.Crud.Contexto, typeof(UsuarioDeClienteNew), nameof(UsuariosDeClienteController), "Crear usuario Web", permisosNecesarios: enumModoDeAccesoDeDatos.Administrador);
            modal.AccionTrasAbrirModal = $"javascript: Crud.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.TrasAbrirModal}', '{modal.IdHtml}')";

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(UsuarioDeClienteDto), typeof(UsuariosDeClienteController), nameof(UsuarioDeClienteDto.IdElemento), "Añadir usuario");
            modalDeCreacion.AccionTrasCrear = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Cli_RecargarGridDeTrazas}()";
        }

        private void DescriptorDePuestosDeCliente()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-PuestosDeCliente", "Puestos de trabajo", true, "Puestos de trabajo del cliente");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(3, expansor);

            var columnas = new DescriptorDeColumnas("PuestosDeCliente");
            columnas.Add(titulo: nameof(PuestoDeClienteDto.Sociedad));
            columnas.Add(titulo: nameof(PuestoDeClienteDto.Puesto));
            columnas.Add(titulo: nameof(PuestoDeClienteDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(PuestoDeClienteDto.IdCg), mostrar: false);
            columnas.Add(titulo: nameof(PuestoDeClienteDto.IdPuesto), mostrar: false);
            columnas.Add(titulo: nameof(PuestoDeClienteDto.IdSociedad), mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(PuestoDeClienteDto.Id), mostrar: false);

            var orden = $"{nameof(PuestoDeClienteDto.Id)}:{enumModoOrdenacion.descendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(PuestosDeClienteController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(PuestosDeClienteController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PuestoDeTrabajoController)}/{nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo)}?id={nameof(PuestoDeClienteDto.IdPuesto)}"}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PuestoDeClienteDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;
            gridDeRelacion.PermitirEditar = false;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(PuestoDeClienteDto), typeof(PuestosDeClienteController), nameof(PuestoDeClienteDto.IdElemento), "Añadir puesto");
            modalDeCreacion.AccionTrasCrear = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Cli_RecargarGridDeTrazas}()";
        }

        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;


            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                      $@"<script src=¨../../js/{RutaBase}/Clientes.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeClientes('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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

/*
 * 
                    
*/
