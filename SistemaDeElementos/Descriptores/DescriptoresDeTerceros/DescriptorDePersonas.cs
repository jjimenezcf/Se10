using System.Collections.Generic;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto;
using ModeloDeDto.Guarderias;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePersonas : DescriptorDeCrud<PersonaDto>
    {
        public DescriptorDePersonas(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }
        public DescriptorDePersonas(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
              , controlador: nameof(PersonasController)
              , vista: nameof(PersonasController.CrudPersonas)
              , modo: modo
              , rutaBase: enumNameSpaceTs.Terceros)
        {
            Mnt.OrdenacionInicial = @$"{nameof(PersonaDto.Expresion)}:{nameof(PersonaDto.Expresion)}:{enumModoOrdenacion.ascendente.Render()}";

            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 0), "Persona", "Buscar por nif, apellido, nombre, mail, teléfono");
            Mnt.Filtro.FiltroDeNombre.CambiarPropiedad(nameof(PersonaDto.Expresion));
            new CheckFiltro<PersonaDto>(padre: Mnt.BloqueGeneral,
                etiqueta: "Es interlocutor",
                filtrarPor: nameof(PersonaDto.EsInterlocutor),
                ayuda: "Sólo las persona que son interlocutoras",
                valorInicial: false,
                filtrarPorFalse: false,
                posicion: new Posicion(0, 1))
            {
                EsOnOff = true
            };

            DefinirMf(menuIndividual, Mnt.OpcionesPorElemento);
            DefinirMf(menuEdicion, Editor.OpcionesMf);

            DescriptorDeDireccion(Ampliaciones.Persona.DireccionAlCrear);
            if (ExtensorDeGuarderias.ModuloActivo(Contexto)) DefinirDescriptorDeInfantes(Contexto);
        }

        private void DefinirDescriptorDeInfantes(ContextoSe contexto)
        {
            var infante = enumNegocio.Infante.LeerNegocio();
            var tienePermisos = ApiDePermisos.TieneAlgunPermiso(contexto, new List<int> { infante.IdAdministrador, infante.IdGestor, infante.IdConsultor });
            if (!tienePermisos) return;

            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-infantes", "Niños/as de la persona", true, "Niños de la persona en la guardería");
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

        private void DefinirMf(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<PersonaDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<PersonaDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.3' accion-menu='{eventosDeMf.Interlocutores}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Interlocutor</li>");
        }
        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;


            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                      $@"<script src=¨../../js/{RutaBase}/Personas.js?v={System.DateTime.Now.Ticks}¨></script>
                         <script src=¨../../js/{RutaBase}/Interlocutores.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDePersonas('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
