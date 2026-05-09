using System;
using Utilidades;
using ServicioDeDatos.Elemento;

namespace ModeloDeDto
{

    public class ElmentoAuditadoDto : ElementoDto, IElmentoAuditadoDto
    {
        [IUPropiedad(
              Visible = false
            , EtiquetaGrid = "Creado el"
            , PorAnchoMnt = 15
            , Etiqueta = "Creado el"
            , Ordenar = true
            , Obligatorio = true
            , OrdenarGridPor = nameof(ElementoDtm.FechaCreacion)
            , TipoDeControl = enumTipoControl.SelectorDeFechaHora
            , Alineada = enumAliniacion.derecha)]
        public DateTime CreadoEl { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false
            , PorAnchoMnt = 15
            , Etiqueta = "Modificado el"
            , EtiquetaGrid = "Modificado el"
            , Ordenar = true
            , OrdenarGridPor = nameof(ElementoDtm.FechaModificacion)
            , TipoDeControl = enumTipoControl.SelectorDeFechaHora
            , Alineada = enumAliniacion.derecha)]
        public DateTime? ModificadoEl { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false, PorAnchoMnt = 20, Etiqueta = "Creado por", EtiquetaGrid = "Creador")]
        public string Creador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false, Etiqueta = "Id del creador")]
        public int IdCreador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false, PorAnchoMnt = 20, Etiqueta = "Modificado por", EtiquetaGrid = "Modificador")]
        public string Modificador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false, Etiqueta = "Id del modificador")]
        public int? IdModificador { get; set; }

        //--------------------------------------------
        [IUPropiedad(Etiqueta = "Expresion", TipoDeControl = enumTipoControl.Editor, Visible = false, Obligatorio = false )]
        public string Expresion { get; set; }

        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public int IdNegocio { get; set; }

        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool HayClases { get; set; }
    }
}