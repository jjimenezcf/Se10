using Utilidades;


namespace ModeloDeDto.Callejero
{
    [IUDto(AnchoEtiqueta = 20
         , AnchoSeparador = 5
         , MostrarExpresion = "Nombre")]
    public class ZonaDto : ElmentoAuditadoDto
    {
        //----------------------------------------------

        [IUPropiedad(
            Etiqueta = nameof(Municipio),
            Ayuda = "Zonas de un municipio",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Municipio),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false,
            Controlador = nameof(enumControladoresCallejero.Municipios),
            VistaDondeNavegar = enumVistasCallejero.CrudMunicipios
            )
        ]
        public int IdMunicipio { get; set; }

        [IUPropiedad(Etiqueta = "Municipio", Visible = false)]
        public string Municipio { get; set; }


        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Zona",
            Ayuda = "Indique el nombre del zona",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            PorAnchoMnt = 50,
            Obligatorio = true,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }

    }
}
