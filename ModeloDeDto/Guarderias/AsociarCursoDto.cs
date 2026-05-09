using ServicioDeDatos.Seguridad;
using ServicioDeDatos;
using Utilidades;
using ServicioDeDatos.Guarderias;

namespace ModeloDeDto.Guarderias
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class AsociarCursoDto: ISelectorDto
    {

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Niño/a",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Infante),
            Fila = 0,
            Columna = 0,
            AutoSpan = false,
            Controlador = nameof(enumControladoresGuarderias.Infantes),
            VistaDondeNavegar = enumVistasGuarderias.CrudInfantes
            )
        ]
        public int IdInfante { get; set; }

        [IUPropiedad(Visible = false)]
        public string Infante { get; set; }


        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del curso", Visible = false)]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Curso",
            Ayuda ="seleccionar el curso al que asociar al niño/a",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CursoDeGuarderiaDto),
            GuardarEn = nameof(IdElemento),
            BuscarPor = ltrDeCursosDeGuarderia.FiltrarParaAsociarCurso,
            Controlador = nameof(enumControladoresGuarderias.CursosDeGuarderia),
            VistaDondeNavegar = enumVistasGuarderias.CrudCursosDeGuarderia,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.CursoDeGuarderia) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            RestringidoPorControl = nameof(IdInfante),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            Negocio = enumNegocio.CursoDeGuarderia,
            LongitudMinimaParaBuscar = 1,
            Fila = 0,
            Columna = 1,
            EditableAlCrear = true,
            EditableAlEditar = true,
            Obligatorio = false)
        ]
        public string Elemento { get; set; }
    }
}
