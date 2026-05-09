using UtilidadesParaIu;
using MVCSistemaDeElementos.Controllers;
using ModeloDeDto.TrabajosSometidos;
using ServicioDeDatos;
using ModeloDeDto.Entorno;
using Utilidades;
using ServicioDeDatos.TrabajosSometidos;
using ServicioDeDatos.Seguridad;
using static MVCSistemaDeElementos.Descriptores.DescriptorDeTrabajosDeUsuario;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeCorreos : DescriptorDeCrud<CorreoDto>
    {
        public DescriptorDeCorreos(ContextoSe contexto, ModoDescriptor modo)
        : base( contexto: contexto
               , controlador: nameof(CorreosController)
               , vista: $"{nameof(CorreosController.CrudDeCorreos)}"
               , modo: modo
               , rutaBase: enumNameSpaceTs.TrabajosSometido)
        {
            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 0), "Texto", "Buscar en el asunto o cuerpo del mensaje");

                        var listaUsuarios = new ListasDinamicas<CorreoDto>(Mnt.BloqueGeneral,
                 etiqueta: "Usuario",
                 filtrarPor: nameof(ltrFltCorreosDto.receptores),
                 ayuda: "seleccione el receptor",
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
            
            DefinirFiltrosEnModal();

            DescriptorDeEdicion<TrabajoDeUsuarioDto>.IncluirMfIndividual(Mnt.OpcionesPorElemento, $"<li id='{eventosDeMf.Correo_Enviar}' accion-menu='{eventosDeMf.Correo_Enviar}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador, permiteMultiSeleccion: true)}>Enviar o reenviar</li>");
            //DescriptorDeEdicion<TrabajoDeUsuarioDto>.QuitarOpcionDeMf(Mnt.OpcionesPorElemento, eventosDeMf.AbrirEnviarCorreo, errorSiNoExiste: false);

            
            Editor.MenuDeEdicion.QuitarOpcionDeMenu(eventosDeEdicion.ModificarElemento);

            Mnt.OrdenacionInicial = @$"{nameof(CorreoDto.Creado)}:creado:{enumModoOrdenacion.descendente.Render()};
                                       {nameof(CorreoDto.Enviado)}:enviado:{enumModoOrdenacion.ascendente.Render()}";

        }

        private void DefinirFiltrosEnModal()
        {
            var modal = new ModalDeFiltrado<CorreoDto>(Mnt.Filtro, "filtros-de-correos", "Filtrar correos", "Seleccione los correos a mostrar");
            Mnt.Filtro.Modales.Add(modal);

            var UsuarioCreador = new DescriptorDeUsuario(Contexto, ModoDescriptor.SeleccionarParaFiltrar);
            modal.ControlesDeFiltrado.Add(new ListasDinamicas<CorreoDto>(modal,
                 etiqueta: "Usuario",
                 filtrarPor: nameof(CorreoDtm.IdUsuario),
                 ayuda: "Usuario creador del correo",
                 seleccionarDe: nameof(UsuarioDto),
                 buscarPor: nameof(UsuarioDto.NombreCompleto),
                 mostrarExpresion: $"[{nameof(UsuarioDto.NombreCompleto)}]",
                 criterioDeBusqueda: enumCriteriosDeFiltrado.igual,
                 posicion: new Posicion(0, 1),
                 controlador: nameof(UsuariosController),
                 navegarA: nameof(UsuariosController.CrudUsuario),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 }
                );

            modal.ControlesDeFiltrado.Add(new FiltroEntreFechas<CorreoDto>(modal,
                    etiqueta: "creado entre",
                    propiedad: nameof(CorreoDto.Creado),
                    ayuda: "correos creados entre las fechas indicadas",
                    posicion: new Posicion() { fila = 1, columna = 0 }));

            modal.ControlesDeFiltrado.Add(new FiltroEntreFechas<CorreoDto>(modal,
                                etiqueta: "Enviado entre",
                                propiedad: nameof(CorreoDto.Enviado),
                                ayuda: "correos enviados entre las fechas indicadas",
                                posicion: new Posicion() { fila = 2, columna = 0 }));

            modal.ControlesDeFiltrado.Add(new CheckFiltro<CorreoDto>(modal,
                etiqueta: "Mostrar los no enviados",
                filtrarPor: nameof(ltrFltCorreosDto.NoSeHaEnviado),
                ayuda: "Sólo los no enviados",
                valorInicial: false,
                filtrarPorFalse: false,
                posicion: new Posicion(1, 1)));

            modal.ControlesDeFiltrado.Add(new CheckFiltro<CorreoDto>(modal,
                etiqueta: "Mostrar los enviados",
                filtrarPor: nameof(ltrFltCorreosDto.seHaEnviado),
                ayuda: "Sólo los enviados",
                valorInicial: false,
                filtrarPorFalse: false,
                posicion: new Posicion(0, 1)));
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Correos.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            TrabajosSometido.CrearCrudDeCorreos('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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


//var UsuarioReceptor = new DescriptorDeUsuario(Contexto, ModoDescriptor.SeleccionarParaFiltrar);
//new SelectorDeFiltro<CorreoDto, UsuarioDto>(padre: Mnt.BloqueGeneral,
//                                  etiqueta: "Receptor",
//                                  filtrarPor: UsuariosPor.eMail,
//                                  ayuda: "Seleccionar usuario receptor",
//                                  posicion: new Posicion() { fila = 0, columna = 1 },
//                                  paraFiltrar: nameof(UsuarioDto.Id),
//                                  paraMostrar: nameof(UsuarioDto.NombreCompleto),
//                                  crudModal: UsuarioReceptor,
//                                  propiedadDondeMapear: UsuariosPor.NombreCompleto.ToString());
