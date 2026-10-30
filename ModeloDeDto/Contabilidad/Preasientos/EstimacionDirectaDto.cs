using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ModeloDeDto.Contabilidad
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class EstimacionDirectaDto
    {
        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la sociedad", Visible = false)]
        public int IdSociedad { get; set; }

        [IUPropiedad(
            Etiqueta = "Sociedad",
            Ayuda = "Sociedad de autónomo a contabilizar por estimación directa",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdSociedad),
            Controlador = nameof(enumControladoresTerceros.Sociedades),
            SeleccionarDe = typeof(SociedadDto),
            VistaDondeNavegar = enumVistasTerceros.CrudSociedades,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Sociedad) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Negocio = enumNegocio.Sociedad,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(SociedadDto.Expresion),
            BuscarPor = ltrDeSociedad.FiltroParaSociedadesGestionadasAutonomas,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            ColSpan = 2,
            EditableAlCrear = true)]
        public string Sociedad { get; set; }


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Etiqueta = "Ejercicio contable",
             Ayuda = "Movimientos del ejercicio que se han de procesar",
             EditableAlCrear = true,
             Tipo = typeof(int),
             TipoDeControl = enumTipoControl.Editor,
             Alineada = enumAliniacion.derecha,
             Fila = 0,
             Columna = 2,
             AnchoMaximo = "200px",
             LongitudMaxima =4)]
        public int Ejercicio { get; set; }

    }
}
