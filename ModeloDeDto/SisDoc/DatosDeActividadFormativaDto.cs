using ModeloDeDto.Entorno;
using System;
using Utilidades;


namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class DatosDeActividadFormativaDto : EsUnaAmpliacionDto
    {

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del usuario responsable",
            Visible = false
            )
        ]
        public int? IdResponsable { get; set; }

        [IUPropiedad(
            Etiqueta = "Responsable",
            Ayuda = "Responsable de la actividad formativa",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(UsuarioDto),
            GuardarEn = nameof(IdResponsable),
            MostrarExpresion = UsuarioDto.ExpresionElemento,
            Controlador = nameof(enumControladoresEntorno.Usuarios),
            VistaDondeNavegar = enumVistasEntorno.CrudUsuario,
            Fila = 0,
            Columna = 0,
            VisibleAlEditar = true,
            Obligatorio = false,
            LongitudMinimaParaBuscar = 1,
            AutoSpan = true
            )
        ]
        public string Responsable { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Inicio",
            Ayuda = "Fecha en la que se debe iniciara la actividad",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            SelectorHasta = nameof(Fin) + ":0:1",
            Fila = 0,
            Columna = 1,
            Obligatorio = false
           )
        ]
        public DateTime? Inicio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Cuando se terminará",
            PorAnchoSel = 20,
            Ayuda = "fecha planificada de terminación",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            Fila = 0,
            Columna = 2,
            Obligatorio = false
           )
        ]
        public DateTime? Fin { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Coste",
           Tipo = typeof(decimal),
           Ayuda = "importe total de la actividad",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           Formato = enumFormato.Numero_6,
           Fila = 0,
           Columna = 3)
        ]
        public decimal? Coste { get; set; }
    }
}
