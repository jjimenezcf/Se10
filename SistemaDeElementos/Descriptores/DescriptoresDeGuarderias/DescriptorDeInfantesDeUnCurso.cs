using ModeloDeDto;
using ModeloDeDto.Guarderias;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeInfantesDeUnCurso : DescriptorDeCrud<InfanteDeUnCursoDto>
    {
        
        public DescriptorDeInfantesDeUnCurso(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto,
              nameof(InfantesDeUnCursoController),
              nameof(InfantesDeUnCursoController.CrudInfantesDeUnCurso),
              modo,
              enumNameSpaceTs.Guarderias)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<InfanteDeUnCursoDto>(padre: fltGeneral
                  , etiqueta: "Curso"
                  , propiedad:nameof(InfanteDeUnCursoDto.IdElemento)
                  , ayuda: "buscar por curso"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(CursosDeGuarderiaController),
                VistaDondeNavegar = nameof(CursosDeGuarderiaController.CrudCursosDeGuarderia),
                Negocio = enumNegocio.CursoDeGuarderia
            };

            Mnt.Etiqueta = "Niños del curso";

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Infante", nameof(InfanteDeUnCursoDto.Infante), "Buscar por 'niño'");

            var modalDeInfantes = new ModalDeRelacionarElementos<InfanteDeUnCursoDto, InfanteDto>(mantenimiento: Mnt
                              , tituloModal: "Seleccione los niños a incluir"
                              , crudModal: new DescriptorDeInfantes(contexto,ModoDescriptor.Relacion)
                              , propiedadRestrictora: nameof(InfanteDeUnCursoDto.IdElemento)
                              , filtrarPor: ltrDeInfante.SeleccionarParaUnCurso);
            var incluirInfantes = new RelacionarElementos(modalDeInfantes.IdHtml, () => modalDeInfantes.RenderControl(), "Incluir niños al curso");
            var opcion = new OpcionDeMenu<InfanteDeUnCursoDto>(Mnt.ZonaMenu.Menu, incluirInfantes, "Añadir", enumModoDeAccesoDeDatos.Gestor);
            Mnt.ZonaMenu.Menu.Add(opcion);

            Mnt.OrdenacionInicial = @$"{nameof(InfanteDeUnCursoDto.Infante)}:{nameof(InfanteDeUnCursoDtm.Infante)}.{nameof(InfanteDeUnCursoDtm.Infante.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/InfantesDeUnCurso.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudInfantesDeUnCurso('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
