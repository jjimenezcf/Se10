using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ModeloDeDto.Guarderias;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Entorno;
using ModeloDeDto.Negocio;
using ModeloDeDto;
using ServicioDeDatos.Elemento;
using System.Collections.Generic;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeInfantes : DescriptorDeCrud<InfanteDto>
    {
        public DescriptorDeInfantes(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }
        public DescriptorDeInfantes(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(InfantesController)
               , nameof(InfantesController.CrudInfantes)
               , modo
               , rutaBase: enumNameSpaceTs.Guarderias)
        {
            DescriptorDeEdicion<AgendaDto>.IncluirMfIndividual(Editor.OpcionesMf, "<hr>");
            Editor.IncluirMfIndividual("Cambiar de curso", eventosDeMf.Infantes_AsociarCurso, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(AsociarCursoDto), eventosDeMf.Infantes_AsociarCurso, "Cambiar o asignar curso"));

            DefinirDescriptorDeDireccionesDeFamiliares();

        }

        private void DefinirDescriptorDeDireccionesDeFamiliares()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-direcciones-familiares", "Direcciones de familiares", true, "Direcciones de los familiares");
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("direcciones-familiares");
            columnas.Add(titulo: "Dirección", propiedad: nameof(DireccionDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Activa", propiedad: nameof(DireccionDto.Activo), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 130);
            columnas.Add(titulo: "Calle", propiedad: nameof(DireccionDto.Calle), mostrar: false);
            columnas.Add(titulo: "Numero", propiedad: nameof(DireccionDto.Numero), mostrar: false);
            columnas.Add(titulo: "Cp", propiedad: nameof(DireccionDto.CodigoPostal), mostrar: false);
            columnas.Add(titulo: "Municipio", propiedad: nameof(DireccionDto.Municipio), mostrar: false);
            columnas.Add(titulo: "NombreDireccion", propiedad: nameof(DireccionDto.NombreDireccion), mostrar: false);
            columnas.Add(titulo: "Url", propiedad: nameof(DireccionDto.Url), mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(DireccionDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(InfantesController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(InfantesController.epLeerDireccionesDeFamiliares)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(DireccionDtm.IdElemento) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , "MostrarDireccion"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = false;
        }

        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;


            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Infantes.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeInfantes('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
