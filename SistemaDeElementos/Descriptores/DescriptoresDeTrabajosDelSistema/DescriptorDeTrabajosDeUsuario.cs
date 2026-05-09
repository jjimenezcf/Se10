using UtilidadesParaIu;
using MVCSistemaDeElementos.Controllers;
using ModeloDeDto.TrabajosSometidos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.TrabajosSometidos;
using Utilidades;
using System.Collections.Generic;
using ModeloDeDto.Gastos;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores
{

    public class DescriptorDeTrabajosDeUsuario : DescriptorDeCrud<TrabajoDeUsuarioDto>
    {

        public class eventosDeMfDeTrabajos
        {
            public const string MfEjecutar = "ejecutar";
            public const string MfBloquear = "bloquear";
            public const string MfDesbloquear = "desbloquear";
            public const string MfResometer = "resometer";
        }

        public DescriptorDeTrabajosDeUsuario(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , controlador: nameof(TrabajosDeUsuarioController)
               , vista: $"{nameof(TrabajosDeUsuarioController.CrudDeTrabajosDeUsuario)}"
               , modo: modo
               , rutaBase: enumNameSpaceTs.TrabajosSometido
              , tituloPlural: "Trabajos de usuario"
              , tituloSingular: "Trabajo de usuario")
        {
            DefinirMf(menuIndividual, Mnt.OpcionesPorElemento);
            DefinirMf(menuEdicion, Editor.OpcionesMf);

            AnadirOpcionDeDependencias(Mnt
                                 , controlador: nameof(TrazasDeUnTrabajoController)
                                 , vista: nameof(TrazasDeUnTrabajoController.CrudDeTrazasDeUnTrabajo)
                                 , datosDependientes: nameof(TrazaDeUnTrabajoDto)
                                 , navegarAlCrud: DescriptorDeMantenimiento<TrazaDeUnTrabajoDto>.NombreMnt
                                 , nombreOpcion: "Traza"
                                 , propiedadQueRestringe: nameof(TrabajoDeUsuarioDto.Id)
                                 , propiedadRestrictora: nameof(TrazaDeUnTrabajoDto.IdTrabajoDeUsuario)
                                 , "Consultar la traza del trabajo de usuario");

            AnadirOpcionDeDependencias(Mnt
                                 , controlador: nameof(ErroresDeUnTrabajoController)
                                 , vista: nameof(ErroresDeUnTrabajoController.CrudDeErroresDeUnTrabajo)
                                 , datosDependientes: nameof(ErrorDeUnTrabajoDto)
                                 , navegarAlCrud: DescriptorDeMantenimiento<ErrorDeUnTrabajoDto>.NombreMnt
                                 , nombreOpcion: "Errores"
                                 , propiedadQueRestringe: nameof(TrabajoDeUsuarioDto.Id)
                                 , propiedadRestrictora: nameof(ErrorDeUnTrabajoDto.IdTrabajoDeUsuario)
                                 , "Consultar errores del trabajo de usuario");
            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 0), "Trabajo", "Buscar por nombre del trabajo");

            DefinirFiltrosEnModal();
            DescriptorDeErrores();
            DescriptorDeTraza();

            Mnt.OrdenacionInicial = @$"{nameof(TrabajoDeUsuarioDto.Planificado)}:planificado:{enumModoOrdenacion.descendente.Render()};
                                       {nameof(TrabajoDeUsuarioDto.Iniciado)}:iniciado:{enumModoOrdenacion.ascendente.Render()}";

        }

        private void DefinirFiltrosEnModal()
        {
            var modal = new ModalDeFiltrado<TrabajoDeUsuarioDto>(Mnt.Filtro, "filtros-de-trabajos", "Filtrar trabajos de usuario", "Seleccione los trabajos de usuario a mostrar");
            Mnt.Filtro.Modales.Add(modal);

            Dictionary<string, string> opciones = typeof(enumEstadosDeUnTrabajo).ToDiccionario((x) => TrabajoSometido.EnumeradoToDtm(x));
            modal.ControlesDeFiltrado.Add(new ListaDeValores<TrabajoDeUsuarioDto>(modal
                , "Estado"
                , "Seleccione el estado por el que filtrar"
                , opciones
                , nameof(TrabajoDeUsuarioDto.Estado)
                , new Posicion() { fila = 0, columna = 1 }));

            modal.ControlesDeFiltrado.Add(new FiltroEntreFechas<TrabajoDeUsuarioDto>(modal,
                                etiqueta: "Planificado",
                                propiedad: nameof(TrabajoDeUsuarioDto.Planificado),
                                ayuda: "trabajos planificados entre",
                                posicion: new Posicion() { fila = 0, columna = 0 }));
            modal.ControlesDeFiltrado.Add(new FiltroEntreFechas<TrabajoDeUsuarioDto>(modal,
                                etiqueta: "Ejecutado entre",
                                propiedad: nameof(TrabajoDeUsuarioDto.Iniciado),
                                ayuda: "trabajos ejecutados entre",
                                posicion: new Posicion() { fila = 1, columna = 0 }));
        }

        private void DefinirMf(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<TrabajoDeUsuarioDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<TrabajoDeUsuarioDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.1' accion-menu='{eventosDeMfDeTrabajos.MfEjecutar}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Ejecutar trabajo</li>");
            DescriptorDeEdicion<TrabajoDeUsuarioDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.2' accion-menu='{eventosDeMfDeTrabajos.MfBloquear}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Bloquear</li>");
            DescriptorDeEdicion<TrabajoDeUsuarioDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.3' accion-menu='{eventosDeMfDeTrabajos.MfDesbloquear}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Desbloquear</li>");
            DescriptorDeEdicion<TrabajoDeUsuarioDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.4' accion-menu='{eventosDeMfDeTrabajos.MfResometer}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Resometer</li>");
        }


        private void DescriptorDeErrores()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-errores", "Errores", true, "Errores del trabajo");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("errores");
            columnas.Add(titulo: nameof(ErrorDeUnTrabajoDto.Fecha), alineacion: enumAliniacion.derecha, tamano: 200);
            columnas.Add(titulo: nameof(ErrorDeUnTrabajoDto.Error), propiedad: nameof(ErrorDeUnTrabajoDto.Error));
            columnas.Add(titulo: nameof(ErrorDeUnTrabajoDto.Id), mostrar: false);
            columnas.Add(titulo: nameof(ErrorDeUnTrabajoDto.IdTrabajoDeUsuario), mostrar: false);

            var orden = $"{nameof(ErrorDeUnTrabajoDto.Fecha)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(ErroresDeUnTrabajoController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(ErroresDeUnTrabajoController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ErrorDeUnTrabajoDto.IdTrabajoDeUsuario) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = false;

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(ErrorDeUnTrabajoDto), typeof(ErroresDeUnTrabajoController), "Consultar error", soloConsulta: true);
        }

        private void DescriptorDeTraza()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-trazas", "Trazas", true, "Trazas del trabajo");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("trazas");
            columnas.Add(titulo: nameof(TrazaDeUnTrabajoDto.Fecha), alineacion: enumAliniacion.derecha, tamano: 200);
            columnas.Add(titulo: nameof(TrazaDeUnTrabajoDto.Traza), propiedad: nameof(TrazaDeUnTrabajoDto.Traza));
            columnas.Add(titulo: nameof(TrazaDeUnTrabajoDto.Id), mostrar: false);
            columnas.Add(titulo: nameof(TrazaDeUnTrabajoDto.IdTrabajoDeUsuario), mostrar: false);

            var orden = $"{nameof(TrazaDeUnTrabajoDto.Fecha)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(TrazasDeUnTrabajoController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(TrazasDeUnTrabajoController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(TrazaDeUnTrabajoDto.IdTrabajoDeUsuario) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = false;

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(TrazaDeUnTrabajoDto), typeof(TrazasDeUnTrabajoController), "Consultar traza", soloConsulta: true);
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
                return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/TrabajosDeUsuario.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            TrabajosSometido.CrearCrudDeTrabajosDeUsuario('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
        public class AccionesDeTu : AccionDeMenuMnt
        {
            const string desbloquear = "desbloquear-trabajo";
            const string bloquear = "bloquear-trabajo";
            const string iniciar = "iniciar-trabajo";
            const string resometer = "resometer-trabajo";
            public AccionesDeTu(string accion, string ayuda, bool permiteMultiSeleccion)
            : base(accion, enumCssOpcionMenu.DeElemento, ayuda)
            {
                PermiteMultiSeleccion = permiteMultiSeleccion;
            }

            public static AccionesDeTu Desbloquear => new AccionesDeTu(desbloquear, "Desbloquear un trabajo", true);
            public static AccionesDeTu Bloquear => new AccionesDeTu(bloquear, "Bloquear un trabajo", true);
            public static AccionesDeTu Iniciar => new AccionesDeTu(iniciar, "Ejecutar un trabajo", false);
            public static AccionesDeTu Resometer => new AccionesDeTu(resometer, "Resometer un trabajo", false);

            public override string RenderAccion()
            {
                return $"javascript:TrabajosSometido.Eventos('{TipoDeAccion}','')";
            }
        }


            //var opcion = new OpcionDeMenu<TrabajoDeUsuarioDto>(Mnt.ZonaMenu.Menu, AccionesDeTu.Iniciar, $"Ejecutar", enumModoDeAccesoDeDatos.Gestor);
            //Mnt.ZonaMenu.Menu.Add(opcion);

            //var opcionBloquear = new OpcionDeMenu<TrabajoDeUsuarioDto>(Mnt.ZonaMenu.Menu, AccionesDeTu.Bloquear, $"Bloquear", enumModoDeAccesoDeDatos.Gestor);
            //Mnt.ZonaMenu.Menu.Add(opcionBloquear);

            //var opcionDesbloquear = new OpcionDeMenu<TrabajoDeUsuarioDto>(Mnt.ZonaMenu.Menu, AccionesDeTu.Desbloquear, $"Desbloquear", enumModoDeAccesoDeDatos.Gestor);
            //Mnt.ZonaMenu.Menu.Add(opcionDesbloquear);

            //var opcionResometer = new OpcionDeMenu<TrabajoDeUsuarioDto>(Mnt.ZonaMenu.Menu, AccionesDeTu.Resometer, $"Resometer", enumModoDeAccesoDeDatos.Gestor);
            //Mnt.ZonaMenu.Menu.Add(opcionResometer);



 * */