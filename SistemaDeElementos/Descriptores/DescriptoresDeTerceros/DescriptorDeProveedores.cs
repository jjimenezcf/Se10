using System.Collections.Generic;
using ModeloDeDto;
using ModeloDeDto.MaestrosTecnico;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeProveedores : DescriptorDeCrud<ProveedorDto>
    {
        public DescriptorDeProveedores(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }
        public DescriptorDeProveedores(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , controlador: nameof(ProveedoresController)
              , vista: nameof(ProveedoresController.CrudProveedores)
              , modo: modo
               , rutaBase: enumNameSpaceTs.Terceros)
        {

            Mnt.OrdenacionInicial = @$"{nameof(ProveedorDto.Expresion)}:{nameof(ProveedorDto.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 0), ltrProveedor.Proveedor, "Buscar por nif, apellido, nombre, mail, teléfono");

            var listaPersona = new ListasDinamicas<ProveedorDto>(Mnt.BloqueGeneral,
                etiqueta: "Persona",
                filtrarPor: nameof(ltrProveedor.IdPersona),
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

            var listaSociedad = new ListasDinamicas<ProveedorDto>(Mnt.BloqueGeneral,
                etiqueta: "Sociedad",
                filtrarPor: nameof(ltrProveedor.IdSociedad),
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

            var listaInter = new ListasDinamicas<ProveedorDto>(Mnt.BloqueGeneral,
                etiqueta: "Interlocutor",
                filtrarPor: nameof(ProveedorDto.IdInterlocutor),
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

            DescriptorDeCuentasDeProveedor();
            DescriptorDeTarifas();

            Editor.IncluirMfIndividual("Importar tarifa", eventosDeMf.Prv_ImportarTarifa, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor);
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(ImportarTarifaDto), eventosDeMf.Prv_ImportarTarifa, "Importar la tarifa del proveedor"));
        }



        private void DescriptorDeCuentasDeProveedor()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-CuentasDeProveedor", "Cuentas bancarias", true, "Cuentas bancarias del proveedor");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("CuentasDeProveedor");
            columnas.Add(titulo: "Banco", propiedad: nameof(CuentaDeProveedorDto.Banco), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Alias", propiedad: nameof(CuentaDeProveedorDto.Alias), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Cuenta", propiedad: nameof(CuentaDeProveedorDto.Cuenta), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Clase", propiedad: nameof(CuentaDeProveedorDto.Clase), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Activa", propiedad: nameof(CuentaDeProveedorDto.Activa), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(CuentaDeProveedorDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(CuentaDeProveedorDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(CuentaDeProveedorDto.Id)}:{enumModoOrdenacion.descendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(CuentasDeProveedorController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(CuentasDeProveedorController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(CuentaDeProveedorDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(CuentaDeProveedorDto), typeof(CuentasDeProveedorController), nameof(CuentaDeProveedorDto.IdElemento), "Añadir cuenta");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Prv_InicializarModalParaCrearCuentas}('{modalDeCreacion.IdHtml}')";
            modalDeCreacion.AccionTrasCrear = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Prv_RecargarGridDeArchivos}()";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(CuentaDeProveedorDto), typeof(CuentasDeProveedorController), "Consultar cuenta", soloConsulta: false);
            modalDeEdicion.AccionTrasModificar = $"javascript: {enumNameSpaceTs.Terceros}.{enumFunctionTs.Prv_RecargarGridDeArchivos}()";
        }

        private void DescriptorDeTarifas()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-tarifas", "Tarifas", true, "Tarifa de un proveedor");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(1, expansor);

            //Definimos el grid de detalles de la tarifas
            var columnas = new DescriptorDeColumnas("tarifas");
            columnas.Add(titulo: "Elemento", propiedad: nameof(UnitariosDeUnProveedorDto.Elemento), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Referencia", propiedad: nameof(UnitariosDeUnProveedorDto.Referencia), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 200);
            columnas.Add(titulo: "Tarifa", propiedad: nameof(UnitariosDeUnProveedorDto.Tarifa), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(UnitariosDeUnProveedorDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "IdPoveedor", propiedad: nameof(UnitariosDeUnProveedorDto.IdProveedor), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(UnitariosDeUnProveedorDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(TarifaDtm.Elemento)}.{nameof(TarifaDtm.Elemento.Referencia)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(UnitariosDeUnProveedorController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(UnitariosDeUnProveedorController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(UnitariosDeUnProveedorDto.IdProveedor) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(UnitariosController)}/{nameof(UnitariosController.CrudUnitarios)}?id={nameof(UnitariosDeUnProveedorDto.IdElemento)}" }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(UnitariosDeUnProveedorDto), typeof(UnitariosDeUnProveedorController), nameof(UnitariosDeUnProveedorDto.IdProveedor), "Añadir tarifa de un unitario",
                accionControlador: nameof(UnitariosDeUnProveedorController.epCrearTarifa));

            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(UnitariosDeUnProveedorDto), typeof(UnitariosDeUnProveedorController), "Editar tarifa", soloConsulta: false,
                accionControlador: nameof(UnitariosDeUnProveedorController.epModificarTarifa));
        }


        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;


            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
                return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];

            var render = base.RenderControl();

            render = render +
                      $@"<script src=¨../../js/{RutaBase}/Proveedores.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeProveedores('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
