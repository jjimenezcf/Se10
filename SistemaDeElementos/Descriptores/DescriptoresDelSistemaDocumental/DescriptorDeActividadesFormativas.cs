using ModeloDeDto;
using ModeloDeDto.SistemaDocumental;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeActividadesFormativas : DescriptorDeCrud<CircuitoDocDto>
    {
        public DescriptorDeActividadesFormativas(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }

        public DescriptorDeActividadesFormativas(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(CircuitosDocController)
               , nameof(CircuitosDocController.CrudActividadesFormativas)
               , modo
               , rutaBase: enumNameSpaceTs.SistemaDocumental
               , eliminarCreacion: true)
        {
            Mnt.OrdenacionInicial = $"{nameof(CircuitoDocDto.Referencia)}:{nameof(CircuitoDocDto.Referencia)}:{enumModoOrdenacion.descendente.Render()}";

            Mnt.Etiqueta = "Gestión de actividades formativas";
            Editor.Etiqueta = "Consultar actividad";

            DescriptorDeDatosActividadFormativa();
            DescriptorDeInscritos();
            DescriptorDeVoluntarios();
        }


        private void DescriptorDeDatosActividadFormativa()
        {
            var datos = new AmpliacionDeEdicion(Editor, Ampliaciones.CircuitosDoc.DatosDeActividadFormativa, "Datos de actividad formativa", new Dimension(2, 2), ayuda: "Información complementaria de la actividad formativa");
            datos.Dto = typeof(DatosDeActividadFormativaDto);
            datos.Controlador = nameof(DatosDeActividadesFormativasController);
            Editor.Ampliaciones.Add(datos);
        }


        private DescriptorDeExpansor DescriptorDeInscritos()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-inscritos", "Inscritos", true, "inscritos en la actividad");
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("inscritos");
            columnas.Add(titulo: "Alumno", propiedad: nameof(InscritosEnActividadDto.Interlocutor));
            columnas.Add(titulo: nameof(InscritosEnActividadDto.Pagado), tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: nameof(InscritosEnActividadDto.Asistio));
            columnas.Add(titulo: nameof(InscritosEnActividadDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(InscritosEnActividadDto.Id), mostrar: false);

            var orden = $"{nameof(InscritosEnActividadDtm.Interlocutor)}.{nameof(InterlocutorDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(InscritosEnActividadesController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(InscritosEnActividadesController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(InscritosEnActividadDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(InscritosEnActividadDto), typeof(InscritosEnActividadesController), nameof(InscritosEnActividadDto.IdElemento), "Añadir inscripción");
            //modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Administracion}.{enumFunctionTs.Exp_InicializarModalParaCrearApuntes}()";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(InscritosEnActividadDto), typeof(InscritosEnActividadesController), "Editar inscripción", soloConsulta: false);
            //modalDeEdicion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Administracion}.{enumFunctionTs.Exp_InicializarModalParaEditarApuntes}();";

            return expansor;
        }

        private DescriptorDeExpansor DescriptorDeVoluntarios()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-voluntarios", "Voluntarios", true, "voluntarios en la actividad");
            Editor.Expanes.Insert(1, expansor);

            var columnas = new DescriptorDeColumnas("voluntarios");
            columnas.Add(titulo: "Voluntario", propiedad: nameof(VoluntarioDeActividadDto.Interlocutor), alineacion: enumAliniacion.izquierda);
            columnas.Add(titulo: nameof(VoluntarioDeActividadDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(VoluntarioDeActividadDto.Id), mostrar: false);

            var orden = $"{nameof(VoluntarioDeActividadDto.Interlocutor)}.{nameof(InterlocutorDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(VoluntarioDeActividadesController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(VoluntarioDeActividadesController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(VoluntarioDeActividadDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;
            gridDeRelacion.PermitirEditar = false;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(VoluntarioDeActividadDto), typeof(VoluntarioDeActividadesController), nameof(VoluntarioDeActividadDto.IdElemento), "Añadir voluntario");

            return expansor;
        }

        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/ActividadesFormativas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeActividadesFormativas('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
