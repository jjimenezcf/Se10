using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class SeleccionarDestinoDto : ElementoDto
    {

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Origen",
            Ayuda = "Origen de la copia",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Origen),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public int IdOrigenDiferente { get; set; }

        [IUPropiedad(Visible = false)]
        public string Origen { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del negocio", Visible = false)]
        public int IdNegocio { get; set; }

        [IUPropiedad(Etiqueta = "Id del destino",Visible = false)]
        public int IdDestino { get; set; }


        [IUPropiedad(
            Etiqueta = "Destino",
            Ayuda = "Seleccione el destino donde aplicar la operación",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdDestino),
            Controlador = nameof(enumControladoresSistemaDocumental.Archivos),
            SeleccionarDe = typeof(ArchivadorDto),
            BuscarPor = ltrFiltros.SeleccionarDestino,
            MostrarExpresion =nameof(ElementoDtm.Expresion),
            LongitudMinimaParaBuscar = 1,
            RestringidoPorControl = nameof(IdOrigenDiferente),
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.ApiDeArchivos) + "." + nameof(enumFunctionTs.SisDoc_Tras_Blanquear_Destino) + "([" + nameof(enumParamTs.idLista) + "])",
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.ApiDeArchivos) + "." + nameof(enumFunctionTs.SisDoc_Tras_Seleccionar_Destino) + "([" + nameof(enumParamTs.idLista) + "])",
            EditableAlCrear = true,
            EditableAlEditar = true)]
        public string Destino { get; set; }

        ////-------------------------------------------------------------------------------------------------------------
        //[IUPropiedad(
        // Etiqueta = "Id del archivador seleccionado",
        // Visible = false
        // )]
        //public int IdOtroArchivador { get; set; }

        //[IUPropiedad(
        //    Etiqueta = "Archivador",
        //    Ayuda = "Archivador a vincular",
        //    TipoDeControl = enumTipoControl.ListaDinamica,
        //    GuardarEn = nameof(IdOtroArchivador),
        //    Controlador = nameof(enumControladoresSistemaDocumental.Archivadores),
        //    SeleccionarDe = typeof(ArchivadorDto),
        //    VistaDondeNavegar = enumVistasSistemaDocumental.CrudArchivadores,
        //    RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Archivador) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
        //    OrdenarListaDinamicaPor = nameof(ArchivadorDtm.Referencia),
        //    Negocio = enumNegocio.Archivador,
        //    MostrarExpresion = nameof(ArchivadorDto.Expresion),
        //    LongitudMinimaParaBuscar = 1,
        //    Tipo = typeof(string),
        //    Fila = 2,
        //    Columna = 1,
        //    AutoSpan = true)]
        //public string OtroArchivador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del archivador de destito", Visible = false)]
        public int IdArchivadorDestino { get; set; }

        [IUPropiedad(
            Etiqueta = "Archivador",
            Ayuda = "Seleccione el archivador de destino",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(ArchivadorDto),
            Controlador = nameof(enumControladoresSistemaDocumental.Archivadores),
            MostrarExpresion = nameof(ArchivadorDto.Expresion),
            GuardarEn = nameof(IdArchivadorDestino),
            RestringidoPorControl = nameof(IdOrigenDiferente),
            CargarBajoDemanda = true,
            AutoSpan = true,
            Fila = 3,
            Columna = 0,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.ApiDeArchivos) + "." + nameof(enumFunctionTs.SisDoc_Tras_Seleccionar_Archivador) + "(["+ nameof(enumParamTs.idLista) +"])"
            )
        ]
        public string ArchivadorDestino { get; set; }

    }
}
