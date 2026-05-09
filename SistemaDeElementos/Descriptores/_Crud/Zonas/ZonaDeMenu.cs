using System;
using Utilidades;
using GestorDeElementos;
using GestoresDeNegocio.Negocio;
using ModeloDeDto;
using ServicioDeDatos.Seguridad;
using UtilidadesParaIu;
using System.Collections.Generic;
using ServicioDeDatos.Elemento;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Descriptores
{
    public class ZonaDeMenu<TElemento> : ControlHtml where TElemento : ElementoDto
    {
        public DescriptorDeMantenimiento<TElemento> Mnt
        {
            get
            {
                if (Padre is DescriptorDeMantenimiento<TElemento>)
                    return (DescriptorDeMantenimiento<TElemento>)Padre;

                if (Padre is DescriptorDeCreacion<TElemento>)
                    return Creador.Mnt;

                return Editor.Mnt;
            }
        }

        public bool EsZonaDeMenuDeMantenimiento => Padre is DescriptorDeMantenimiento<TElemento>;
        public bool EsZonaDeMenuDeEdicion => Padre is DescriptorDeEdicion<TElemento>;
        public bool EsZonaDeMenuDeCreacion => Padre is DescriptorDeCreacion<TElemento>;
        public bool EsZonaDeMenuDeHistorial => !(EsZonaDeMenuDeMantenimiento || EsZonaDeMenuDeCreacion || EsZonaDeMenuDeEdicion);

        public DescriptorDeCreacion<TElemento> Creador => (DescriptorDeCreacion<TElemento>)Padre;
        public DescriptorDeEdicion<TElemento> Editor => (DescriptorDeEdicion<TElemento>)Padre;

        public Dictionary<string, string> OpcionesDesplegables { get; set; } = new Dictionary<string, string>();
        public string ProcesarOpcionesDesplegables { get; set; }

        public Menu<TElemento> Menu { get; set; }

        public ZonaDeMenu(ControlHtml padre)
        : base(
          padre: padre,
          id: $"{padre.Id}_{enumTipoControl.ZonaDeMenu.Render()}",
          etiqueta: null,
          propiedad: null,
          ayuda: null,
          posicion: null
        ) => IniciarClase();


        public ZonaDeMenu(DescriptorDeCreacion<TElemento> creador)
        : base(
          padre: creador,
          id: $"{creador.Id}_{enumTipoControl.ZonaDeMenu.Render()}",
          etiqueta: null,
          propiedad: null,
          ayuda: null,
          posicion: null
        ) => IniciarClase();

        public ZonaDeMenu(DescriptorDeEdicion<TElemento> editor)
        : base(
          padre: editor,
          id: $"{editor.Id}_{enumTipoControl.ZonaDeMenu.Render()}",
          etiqueta: null,
          propiedad: null,
          ayuda: null,
          posicion: null
        ) => IniciarClase();

        private void IniciarClase()
        {
            Menu = new Menu<TElemento>(this);
            Tipo = enumTipoControl.ZonaDeMenu;
        }

        public override string RenderControl()
        {
            var htmlContenedorPadre =  Menu.RenderControl();
            if (EsZonaDeMenuDeMantenimiento)
                htmlContenedorPadre = htmlContenedorPadre + 
                        (OpcionesDesplegables.Count > 1 
                        ? @$"<div class='{enumCssFiltro.ContenedorListaDeElementos.Render()}'>
                             <select id='{IdHtml}-otras' name='{Id}' 
                                class='{enumCssFiltro.ListaDeMenu.Render()} {enumCssFiltro.OpcionesMenuDeCreacion.Render()}' 
                                tipo='{enumTipoControl.ListaDeMenu.Render()}'
                                onchange ='{ProcesarOpcionesDesplegables}' >
                             {OpcionesDesplegables.RenderOptions()}
                             </select>
                          </div>" 
                        : @$"<div class='{enumCssFiltro.ContenedorListaDeElementos.Render()}'> </div>");
           
            return htmlContenedorPadre.Render();
        }

        #region Opciones de mantenimiento
        internal void AnadirOpcionDeIrACrear()
        {
            //if (Mnt.Crud.Negocio == enumNegocio.No_Definido)
            //    return;

            if (Mnt.Crud.SinCreacion)
                return;

            if (Mnt.Crud.Negocio != enumNegocio.No_Definido && Mnt.Crud.Dtm != null && Mnt.Crud.Dtm.ImplementaNecesitaSerParametrizador() && !Mnt.Crud.Contexto.SePuedeParametrizar())
                return;

            var crearElemento = new CrearElemento();
            var opcion = new OpcionDeMenu<TElemento>(Menu, crearElemento, enumOpcionDeMenu.Nuevo, enumModoDeAccesoDeDatos.Gestor);
            Menu.Add(opcion);

            Mnt.CrearMenuDeCreacionDeUsuario(enumNameSpaceTs.ApiDelCrud, enumFunctionTs.Neg_ProcesarOpcionDeMenuLista);
        }
        internal void AnadirOpcionDeIrAEditar()
        {
            if (Mnt.Crud.SinEdicion)
                return;

            var editarElemento = new EditarElemento();
            var opcion = new OpcionDeMenu<TElemento>(Menu, editarElemento, enumOpcionDeMenu.Editar, enumModoDeAccesoDeDatos.Consultor);
            Menu.Add(opcion);
        }

        internal void AnadirOpcionDeIrAHistorial()
        {
            var historial = new HistorialDelProceso();
            var opcion = new OpcionDeMenu<TElemento>(Menu, historial, enumOpcionDeMenu.Historial, enumModoDeAccesoDeDatos.Consultor);
            Menu.Add(opcion);
        }

        internal void AnadirOpcionDeIrAExportar()
        {
            if (!Mnt.Crud.NegocioActivo || Mnt.Crud.Modo == ModoDescriptor.Consulta || !(bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.OpcionDeExportar)))
                return;

            var exportarElemento = new ExportarElementos();
            var opcion = new OpcionDeMenu<TElemento>(Menu, exportarElemento, $"Exportar", enumModoDeAccesoDeDatos.Consultor);
            Menu.Add(opcion);
        }

        internal void AnadirOpcionDeEnviareMail()
        {
            if (!Mnt.Crud.NegocioActivo || Mnt.Crud.Modo == ModoDescriptor.Consulta || !(bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.OpcionDeEnviar)))
                return;

            var enviarElementos = new EnviarElementos();
            if (Mnt.Crud.Negocio == enumNegocio.No_Definido)
            {
                enviarElementos.NumeroMaximoEnLaMultiseleccion = 1;
            }
            else
            {
                if (Mnt.Crud.Negocio.EsUnNegocio())
                {
                    //Todo--> añadir variable de correos a enviar y variables de entidades de negocio
                    var negocioDtm = GestorDeNegocios.LeerNegocio(Mnt.Crud.Contexto, Mnt.Crud.Negocio);
                }

                enviarElementos.NumeroMaximoEnLaMultiseleccion = 5;
            }

            var opcion = new OpcionDeMenu<TElemento>(Menu, enviarElementos, $"Enviar", enumModoDeAccesoDeDatos.Consultor);
            Menu.Add(opcion);
        }

        internal void AnadirOpcionDeTransitar()
        {
            if (!Mnt.Crud.NegocioActivo || Mnt.Crud.Modo == ModoDescriptor.Consulta || !(bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.OpcionDeTransitar)))
                return;
            if (!NegociosDeSe.UsaHitos(Mnt.Crud.Negocio))
                return;

            var transitar = new TransitarElementos();
            var opcion = new OpcionDeMenu<TElemento>(Menu, transitar, $"Transitar", enumModoDeAccesoDeDatos.Consultor);
            Menu.Add(opcion);
        }

        internal void AnadirOpcionDeBorrar()
        {
            if (!Mnt.Crud.NegocioActivo || Mnt.Crud.Modo == ModoDescriptor.Consulta || !(bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.OpcionDeBorrar)))
                return;

            var BorrarElemento = new BorrarElemento();
            var opcion = new OpcionDeMenu<TElemento>(Menu, BorrarElemento, enumOpcionDeMenu.Borrar, enumModoDeAccesoDeDatos.Gestor);
            Menu.Add(opcion);
        }

        #endregion


        #region Opciones de creacion
        internal void AnadirOpcionDeNuevoElemento()
        {
            var nuevoElemento = new NuevoElemento();
            var opcion = new OpcionDeMenu<TElemento>(Menu, nuevoElemento, $"Crear", enumModoDeAccesoDeDatos.Gestor);
            Menu.Add(opcion);
        }

        internal void AnadirOpcionDeCerrarCreacion()
        {
            var cerrarCreacion = new CerrarCreacion();
            var opcion = new OpcionDeMenu<TElemento>(Menu, cerrarCreacion, $"Cerrar", enumModoDeAccesoDeDatos.Consultor);
            Menu.Add(opcion);
        }
        #endregion

        #region opciones de edición
        internal void AnadirOpcionDeModificarElemento()
        {
            var modificarElemento = new ModificarElemento();
            var opcion = new OpcionDeMenu<TElemento>(Menu, modificarElemento, $"Modificar", enumModoDeAccesoDeDatos.Gestor);
            Menu.Add(opcion);
        }
        internal void AnadirOpcionDeCancelarEdicion()
        {
            var cancelarEdicion = new CancelarEdicion();
            var opcion = new OpcionDeMenu<TElemento>(Menu, cancelarEdicion, $"Cerrar", enumModoDeAccesoDeDatos.Consultor);
            Menu.Add(opcion);
        }
        #endregion

        internal void AnadirOpcionDeCerrarHistorial()
        {
            var cerrarHistorial = new CerrarHistorial();
            var opcion = new OpcionDeMenu<TElemento>(Menu, cerrarHistorial, $"Cerrar", enumModoDeAccesoDeDatos.Consultor);
            Menu.Add(opcion);
        }

        internal void QuitarOpcionDeMenu(string tipoDeAccion)
        {
            foreach (var opcion in Menu.OpcionesDeMenu)
            {
                if (opcion.Accion.TipoDeAccion == tipoDeAccion)
                {
                    Menu.OpcionesDeMenu.Remove(opcion);
                    return;
                }
            }

            throw new Exception($"Se ha solicitado quitar la opción de menú {tipoDeAccion}, y no se ha localizado en {Menu.Id}");
        }

        internal void ModificarPermisosNecesarios(string opcion, enumModoDeAccesoDeDatos permisos)
        {

            foreach (var opcionDeMenu in Menu.OpcionesDeMenu)
            {
                if (opcionDeMenu.Etiqueta == opcion)
                    opcionDeMenu.PermisosNecesarios = permisos;
            }
        }
    }
}