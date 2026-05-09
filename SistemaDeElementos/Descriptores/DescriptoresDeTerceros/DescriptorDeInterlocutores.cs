using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto;
using ModeloDeDto.Guarderias;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeInterlocutores : DescriptorDeCrud<InterlocutorDto>
    {
        public DescriptorDeInterlocutores(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
              , controlador: nameof(InterlocutoresController)
              , vista: nameof(InterlocutoresController.CrudInterlocutores)
              , modo: modo
              , rutaBase: enumNameSpaceTs.Terceros)
        {

            Mnt.OrdenacionInicial = @$"{nameof(InterlocutorDto.Expresion)}:{nameof(InterlocutorDto.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";
            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 0), "Interlocutor", "Buscar por nif, apellido, nombre, mail, teléfono");
            RecolocarControl(Mnt.Filtro.FiltroPorBaja, new Posicion(0, 1));
            var listaPersona = new ListasDinamicas<InterlocutorDto>(Mnt.BloqueGeneral,
                etiqueta: "Persona",
                filtrarPor: nameof(InterlocutorDto.IdPersona),
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

            var listaSociedad = new ListasDinamicas<InterlocutorDto>(Mnt.BloqueGeneral,
                etiqueta: "Sociedad",
                filtrarPor: nameof(InterlocutorDto.IdSociedad),
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


            DefinirMf(menuIndividual, Mnt.OpcionesPorElemento);
            DefinirMf(menuEdicion, Editor.OpcionesMf);
            Mnt.ZonaMenu.OpcionesDesplegables = new Dictionary<string, string>
            {
                { "-1", "Opciones de creación" },
                { enumOpcionDeMenuInterlocutor.CrearSociedad, "Crear sociedad" },
                { enumOpcionDeMenuInterlocutor.CrearPersona, "Crear persona" },
                { enumOpcionDeMenuInterlocutor.CrearContacto, "Crear contacto" }
            };

            Mnt.ZonaMenu.ProcesarOpcionesDesplegables = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Int_ProcesarOpcionDeMenuLista}()";

            DescriptorDeCuentasDeInterlocutor();
            if (ExtensorDeGuarderias.ModuloActivo(Contexto)) DefinirDescriptorDeInfantes(Contexto);
            DescriptorDeDireccion(Ampliaciones.Interlocutor.DireccionAlCrear);
        }
        private void DefinirMf(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<InterlocutorDto>.IncluirMfIndividual(opciones, "<hr>");
            if (ExtensorDeExpedientes.HayTiposJuridicos(Contexto))
            {
                DescriptorDeEdicion<InterlocutorDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.{eventosDeMf.Procuradores}' accion-menu='{eventosDeMf.Procuradores}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Procurador</li>");
                DescriptorDeEdicion<InterlocutorDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.{eventosDeMf.Abogados}' accion-menu='{eventosDeMf.Abogados}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Abogado</li>");
            }
            DescriptorDeEdicion<InterlocutorDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.{eventosDeMf.Proveedor}' accion-menu='{eventosDeMf.Proveedor}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Proveedor</li>");
            DescriptorDeEdicion<InterlocutorDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.{eventosDeMf.Cliente}' accion-menu='{eventosDeMf.Cliente}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Cliente</li>");
            //DescriptorDeEdicion<InterlocutorDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.7' accion-menu='{eventosDeMf.Trabajador}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Trabajador</li>");
        }

        private void DefinirDescriptorDeInfantes(ContextoSe contexto)
        {
            var infante = enumNegocio.Infante.LeerNegocio();
            var tienePermisos = ApiDePermisos.TieneAlgunPermiso(contexto, new List<int> { infante.IdAdministrador, infante.IdGestor, infante.IdConsultor });
            if (!tienePermisos) return;

            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-infantes", "Niños/as del tutor", true, "Niños del tutor indicado como cantacto principal");
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("infantes");
            columnas.Add(titulo: "Nombre", propiedad: nameof(InfanteDto.Nombre));
            columnas.Add(titulo: "Curso", propiedad: nameof(InfanteDto.Curso));
            columnas.Add(titulo: "Id", propiedad: nameof(InfanteDto.Id), mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(InfantesController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(InfantesController.epLeerInfantesTutelados)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(IDetalleDto.IdElemento) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(InfantesController)}/{nameof(InfantesController.CrudInfantes)}?id={nameof(IElementoDto.Id)}"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = false;
        }

        private void DescriptorDeCuentasDeInterlocutor()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-CuentasDeInterlocutor", "Cuentas bancarias", true, "Cuentas bancarias del interlocutor");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("CuentasDeInterlocutor");
            columnas.Add(titulo: "Banco", propiedad: nameof(CuentaDeInterlocutorDto.Banco), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Alias", propiedad: nameof(CuentaDeInterlocutorDto.Alias), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Cuenta", propiedad: nameof(CuentaDeInterlocutorDto.Cuenta), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Clase", propiedad: nameof(CuentaDeInterlocutorDto.Clase), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Activa", propiedad: nameof(CuentaDeInterlocutorDto.Activa), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(CuentaDeInterlocutorDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(CuentaDeInterlocutorDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(CuentaDeInterlocutorDto.Id)}:{enumModoOrdenacion.descendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(CuentasDeInterlocutorController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(CuentasDeInterlocutorController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(CuentaDeInterlocutorDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(CuentaDeInterlocutorDto), typeof(CuentasDeInterlocutorController), nameof(CuentaDeInterlocutorDto.IdElemento), "Añadir cuenta");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Int_InicializarModalParaCrearCuentas}('{modalDeCreacion.IdHtml}')";
            modalDeCreacion.AccionTrasCrear = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Int_RecargarGridDeArchivos}()";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(CuentaDeInterlocutorDto), typeof(CuentasDeInterlocutorController), "Consultar cuenta", soloConsulta: false);
            modalDeEdicion.AccionTrasModificar = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Int_RecargarGridDeArchivos}()";
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                      $@"<script src=¨../../js/{RutaBase}/Interlocutores.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeInterlocutores('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
