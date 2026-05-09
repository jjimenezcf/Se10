using System.Collections.Generic;
using GestoresDeNegocio.Terceros;
using ModeloDeDto;
using ModeloDeDto.MaestrosTecnico;
using ModeloDeDto.Terceros;
using ModeloDeDto.Ventas;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Ventas;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeTrabajadores : DescriptorDeCrud<TrabajadorDto>
    {
        public DescriptorDeTrabajadores(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , controlador: nameof(TrabajadoresController)
              , vista: nameof(TrabajadoresController.CrudTrabajadores)
              , modo: modo
               , rutaBase: enumNameSpaceTs.Terceros)
        {

            Mnt.OrdenacionInicial = @$"{nameof(TrabajadorDto.Expresion)}:{nameof(TrabajadorDto.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 0), ltrTrabajador.Trabajador, "Buscar por nif, apellido, nombre, mail, teléfono");

            var listaPersona = new ListasDinamicas<TrabajadorDto>(Mnt.BloqueGeneral,
                etiqueta: "Persona",
                filtrarPor: nameof(ltrTrabajador.IdPersona),
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

            var listaInter = new ListasDinamicas<TrabajadorDto>(Mnt.BloqueGeneral,
                etiqueta: "Interlocutor",
                filtrarPor: nameof(TrabajadorDto.IdInterlocutor),
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

            DescriptorDeCuentasDeTrabajador();
            DescriptorDeAsignaciones();
        }

        private void DescriptorDeAsignaciones()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-asignaciones-trabajador", "Partes de trabajo", true, "Asignaciones a un trabajador");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles de la tarifas
            var columnas = new DescriptorDeColumnas("asignaciones");
            columnas.Add(titulo: "Parte", propiedad: nameof(AsignacionDePtrDto.Elemento), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Cuando empezar", propiedad: nameof(AsignacionDePtrDto.PlfDeInicio), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 200, formato: enumFormato.FechaTiempo);
            columnas.Add(titulo: "Cuando terminar", propiedad: nameof(AsignacionDePtrDto.PlfDeFin), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 200, formato: enumFormato.FechaTiempo);
            columnas.Add(titulo: "Empezó", propiedad: nameof(AsignacionDePtrDto.Iniciada), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 200, formato: enumFormato.FechaTiempo);
            columnas.Add(titulo: "Terminó", propiedad: nameof(AsignacionDePtrDto.Finalizada), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 200, formato: enumFormato.FechaTiempo);
            columnas.Add(titulo: "Tiempo dedicado", propiedad: nameof(AsignacionDePtrDto.Duracion), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 200);
            columnas.Add(titulo: "", propiedad: nameof(AsignacionDePtrDto.LtrMedidoEn), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 200);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(AsignacionDePtrDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "IdTrabajador", propiedad: nameof(AsignacionDePtrDto.IdTrabajador), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(AsignacionDePtrDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(AsignacionDePtrDtm.Elemento)}.{nameof(AsignacionDePtrDtm.Elemento.Id)}:{enumModoOrdenacion.descendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(AsignacionesDePtrController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(AsignacionesDePtrController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(AsignacionDePtrDto.IdTrabajador) }
                ,  { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PartesTrController)}/{nameof(PartesTrController.CrudPartesDeTrabajo)}?id={nameof(AsignacionDePtrDto.IdElemento)}" }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.FiltrarPara = ltrDeUnaAsignacion.MostrarLasPendientes;

            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(AsignacionDePtrDto), typeof(AsignacionesDePtrController), "Editar asignación", soloConsulta: true);
        }


        private void DescriptorDeCuentasDeTrabajador()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-CuentasDeTrabajador", "Cuentas bancarias", true, "Cuentas bancarias del trabajador");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("CuentasDeTrabajador");
            columnas.Add(titulo: "Banco", propiedad: nameof(CuentaDeTrabajadorDto.Banco), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Alias", propiedad: nameof(CuentaDeTrabajadorDto.Alias), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Cuenta", propiedad: nameof(CuentaDeTrabajadorDto.Cuenta), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Clase", propiedad: nameof(CuentaDeTrabajadorDto.Clase), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Activa", propiedad: nameof(CuentaDeTrabajadorDto.Activa), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(CuentaDeTrabajadorDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(CuentaDeTrabajadorDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(CuentaDeTrabajadorDto.Id)}:{enumModoOrdenacion.descendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(CuentasDeTrabajadorController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(CuentasDeTrabajadorController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(CuentaDeTrabajadorDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(CuentaDeTrabajadorDto), typeof(CuentasDeTrabajadorController), nameof(CuentaDeTrabajadorDto.IdElemento), "Añadir cuenta");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Trb_InicializarModalParaCrearCuentas}('{modalDeCreacion.IdHtml}')";
            modalDeCreacion.AccionTrasCrear = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Trb_RecargarGridDeArchivos}()";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(CuentaDeTrabajadorDto), typeof(CuentasDeTrabajadorController), "Consultar cuenta", soloConsulta: false);
            modalDeEdicion.AccionTrasModificar = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Trb_RecargarGridDeArchivos}()";
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                      $@"<script src=¨../../js/{RutaBase}/Trabajadores.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeTrabajadores('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
