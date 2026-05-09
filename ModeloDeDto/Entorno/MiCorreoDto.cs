using System;
using Utilidades;

namespace ModeloDeDto.Entorno
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false, OpcionDeEditar = true, OpcionDeCrear = false, ConMfs = false)]
    public class MiCorreoDto : ElementoDto
    {

        [IUPropiedad(Etiqueta = "id del mensage en Gmail", Visible = false)]
        public string IdMensaje { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Buzón", TamanoFijo = "8em", VisibleAlEditar = false)]
        public string Buzon { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Fecha", TamanoFijo = "10em", Fila = 0, Columna = 0, Posicion = 0, AnchoMaximo = "15em", TipoDeControl = enumTipoControl.SelectorDeFechaHora)]
        public DateTime Fecha { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Emisor", TamanoFijo = "30em", Fila = 0, Columna = 0, Posicion = 1, AutoSpan = true)]
        public string Emisor { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Destinatario", VisibleEnGrid = false, TamanoFijo = "20em", Fila = 0, Columna = 0, Posicion = 1, AutoSpan = true)]
        public string To { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Asunto", Fila = 1, Columna = 0, AutoSpan = true)]
        public string Asunto { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Cuerpo",
            EtiquetaRef = "Abrir Correo",
            TipoDeControl = enumTipoControl.AreaDeTexto,
            Fila = 2,
            Columna = 0,
            NumeroDeFilas = 5,
            VisibleEnGrid = false,
            AutoSpan = true,
            OnClick  = "Entorno.ExpandirContenido()"
          )
        ]
        public string Cuerpo { get; set; }        

        [IUPropiedad(Oculto = true, EditableAlEditar = false)]
        public string CuerpoHtml { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Adjuntos",
            Ayuda = "Indicar si el correo tiene adjuntos",
            VisibleEnGrid = false,
            VisibleEnEdicion = false,
            TipoDeControl = enumTipoControl.Check,
            TamanoFijo = "7em",
            Alineada = enumAliniacion.centrada
            )
        ]
        public bool ConAdjuntos { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TamanoFijo = "6em"
         , Etiqueta = "Acción"
         , EtiquetaGrid = ""
         , TipoDeControl = enumTipoControl.Referencia
         , Fila = 3
         , Columna = 0
         , AccionRef = "javascript: " + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_ComoArchivar) + "(numeroDeFila)"
         , Alineada = enumAliniacion.derecha)]
        public string Accion_1 { get; set; } = "Archivar";

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TamanoFijo = "6em"
         , Etiqueta = "Acción"
         , EtiquetaGrid = ""
         , TipoDeControl = enumTipoControl.Referencia
         , Fila = 3
         , Columna = 0
         , AccionRef = "javascript: " + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_ComoVincular) + "(numeroDeFila)"
         , Alineada = enumAliniacion.derecha)]
        public string Accion_2 { get; set; } = "Asociar a";

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TamanoFijo = "6em"
         , Etiqueta = "Acción"
         , EtiquetaGrid = ""
         , TipoDeControl = enumTipoControl.Referencia
         , Fila = 3
         , Columna = 0
         , AccionRef = "javascript: " + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_Imprimir) + "(numeroDeFila)"
         , Alineada = enumAliniacion.derecha)]
        public string Accion_3 { get; set; } = "Imprimir";

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TamanoFijo = "6em"
         , Etiqueta = "Acción"
         , EtiquetaGrid = ""
         , TipoDeControl = enumTipoControl.Referencia
         , Fila = 3
         , Columna = 0
         , AccionRef = "javascript: " + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_Eliminar) + "(numeroDeFila)"
         , Alineada = enumAliniacion.derecha)]
        public string Accion_4 { get; set; } = "Eliminar";

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string Adjuntos { get; set; }

    }
}
