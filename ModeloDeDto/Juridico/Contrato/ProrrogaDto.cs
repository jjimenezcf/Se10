using ServicioDeDatos.Juridico;
using System;
using Utilidades;


namespace ModeloDeDto.Juridico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class ProrrogaDto : EsUnaAmpliacionDto
    {        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase de la prórroga",
            Ayuda = "indique la clase del prórroga",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDeProrroga),
            GuardarEn = nameof(ClaseDeProrroga),
            Fila = 0,
            Columna = 0,
            Obligatorio = false,
            VisibleEnEdicion = true
          )
        ]
        public enumClaseDeProrroga ClaseDeProrroga { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Se prorroga por:... (en meses)",
           Tipo = typeof(int),
           Ayuda = "nº de meses a sumar a la fecha de fin",
           TipoDeControl = enumTipoControl.Editor,
           EditableAlEditar = true,
           Alineada = enumAliniacion.derecha,
           Fila = 0,
           Obligatorio = false,
           Columna = 1)
        ]
        public int? Meses { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha última prórroga",
            Ayuda = "Fecha máxima de prorrogación del contrato",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Alineada = enumAliniacion.derecha,
            Fila = 0,
            Columna = 2,
            VisibleEnGrid = true,
            Obligatorio = false,
            Ordenar = true
           )
        ]
        public DateTime? FechaUltimaProrroga { get; set; }

    }
}
