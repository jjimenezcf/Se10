using ModeloDeDto;
using ModeloDeDto.Guarderias;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeCursosDeGuarderia : DescriptorDeCrud<CursoDeGuarderiaDto>
    {
        public DescriptorDeCursosDeGuarderia(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }
        public DescriptorDeCursosDeGuarderia(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(CursosDeGuarderiaController)
               , nameof(CursosDeGuarderiaController.CrudCursosDeGuarderia)
               , modo
               , rutaBase: enumNameSpaceTs.Guarderias)
        {


            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(InfantesDeUnCursoController)
                , vista: nameof(InfantesDeUnCursoController.CrudInfantesDeUnCurso)
                , relacionarCon: nameof(InfanteDto)
                , navegarAlCrud: DescriptorDeMantenimiento<InfanteDeUnCursoDto>.NombreMnt
                , nombreOpcion: "Niños"
                , propiedadQueRestringe: nameof(CursoDeGuarderiaDto.Id)
                , propiedadRestrictora: nameof(InfanteDeUnCursoDto.IdElemento)
                , "Gestionar los niños de un curso"
                , permisos: enumModoDeAccesoDeDatos.Consultor);


            DescriptorDeProfesores();
            DescriptorDeInfantes();
        }


        private void DescriptorDeProfesores()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-Profesores", "Otros profesores", mostrarPlegado: true, "otros profesores del curso");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("Profesores");
            columnas.Add(titulo: "Profesor", propiedad: nameof(ProfeDeCursoDeGuarderiaDto.Trabajador));
            columnas.Add(titulo: nameof(ProfeDeCursoDeGuarderiaDto.Id), mostrar: false);
            columnas.Add(titulo: nameof(ProfeDeCursoDeGuarderiaDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(ProfeDeCursoDeGuarderiaDto.IdTrabajador), mostrar: false);

            var orden = $"{nameof(ProfeDeCursoDeGuarderiaDto.Trabajador)}.{nameof(TrabajadorDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(ProfesDeCursoDeGuarderiaController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(ProfesDeCursoDeGuarderiaController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ProfeDeCursoDeGuarderiaDto.IdElemento) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(TrabajadoresController)}/{nameof(TrabajadoresController.CrudTrabajadores)}?id={nameof(ProfeDeCursoDeGuarderiaDtm.IdTrabajador)}"}
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;
            gridDeRelacion.PermitirEditar = false;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(ProfeDeCursoDeGuarderiaDto), typeof(ProfesDeCursoDeGuarderiaController), nameof(ProfeDeCursoDeGuarderiaDto.IdElemento), "Añadir profesor");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Guarderias}.{enumFunctionTs.Cursos_InicializarModalParaIncluirProfesore}()";
        }

        private void DescriptorDeInfantes()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-Infantes", "Niños del curso", mostrarPlegado: true, "niños del curso");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(1, expansor);

            var columnas = new DescriptorDeColumnas("Infantes");
            columnas.Add(titulo: "Niño", propiedad: nameof(InfanteDeUnCursoDto.Infante));
            columnas.Add(titulo: nameof(InfanteDeUnCursoDto.Id), mostrar: false);
            columnas.Add(titulo: nameof(InfanteDeUnCursoDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(InfanteDeUnCursoDto.IdInfante), mostrar: false);

            var orden = $"{nameof(InfanteDeUnCursoDto.Infante)}.{nameof(InfanteDtm.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(InfantesDeUnCursoController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(InfantesDeUnCursoController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(InfanteDeUnCursoDto.IdElemento) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(InfantesController)}/{nameof(InfantesController.CrudInfantes)}?id={nameof(InfanteDeUnCursoDtm.IdInfante)}"}
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;
            gridDeRelacion.PermitirEditar = false;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(InfanteDeUnCursoDto), typeof(InfantesDeUnCursoController), nameof(InfanteDeUnCursoDto.IdElemento), "Añadir niño");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Guarderias}.{enumFunctionTs.Cursos_InicializarModalParaIncluirInfante}()";
        }

        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;


            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/CursosDeGuarderia.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeCursosDeGuarderia('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
