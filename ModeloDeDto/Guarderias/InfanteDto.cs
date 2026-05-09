using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using Utilidades;

namespace ModeloDeDto.Guarderias
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, EditarTrasCrear = true, OpcionDeBorrar = false)]
    public class InfanteDto : ElmentoAuditadoDto, IUsaNombreDto, IUsaBajaDto, IUsaArchivoDto
    {

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Nombre",
            EtiquetaGrid = "Niño",
            Ayuda = "indique el nombre del niño",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            PorAnchoMnt = 50,
            Obligatorio = true,
            LongitudMaxima = 250,
            AutoSpan = true
          )
        ]
        public string Nombre { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              TamanoFijo = "12em"
            , Etiqueta = "Nacido el"
            , Ordenar = true
            , OrdenarGridPor = nameof(InfanteDto.NacidoEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , Formato = enumFormato.Fecha
            , MantenerHuecoDeLaIzquierda = true
            , Fila = 0
            , Columna = 2
            , Posicion = 0)]
        public DateTime? NacidoEl { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            TamanoFijo ="12em",
            Etiqueta = "Edad",
            Ayuda = "edad del niño",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 2,
            Posicion = 1, 
            Ordenar = false,
            PorAnchoMnt = 15,
            VisibleAlCrear = false,
            EditableAlEditar = false
          )
        ]
        public string EdadFormateada { get; set; }


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del contacto del niño",
         Visible = false
         )]
        public int IdContacto { get; set; }

        [IUPropiedad(
            Etiqueta = "Contacto",
            Ayuda = "Persona de contacto",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdContacto),
            Controlador = nameof(enumControladoresTerceros.Interlocutores),
            BuscarPor = ltrInterlocutor.ParaInfante,
            SeleccionarDe = typeof(InterlocutorDto),
            VistaDondeNavegar = enumVistasTerceros.CrudInterlocutores,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Interlocutor) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Interlocutor,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = true,
            AutoSpan = true)]
        public string Contacto { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id de la madre del niño",
         Visible = false
         )]
        public int? IdMadre { get; set; }

        [IUPropiedad(
            Etiqueta = "Madre",
            Ayuda = "Madre del niño",
            VisibleEnGrid = false,
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdMadre),
            Controlador = nameof(enumControladoresTerceros.Personas),
            BuscarPor = ltrDePersonas.FiltarPorNombreDeMadrePadre,
            SeleccionarDe = typeof(PersonaDto),
            VistaDondeNavegar = enumVistasTerceros.CrudPersonas,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Persona) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Persona,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1,
            EditableAlCrear = true,
            EditableAlEditar = true,
            AutoSpan = true)]
        public string Madre { get; set; }


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del padre del niño",
         Visible = false
         )]
        public int? IdPadre { get; set; }

        [IUPropiedad(
            Etiqueta = "Padre",
            Ayuda = "Padre del niño",
            VisibleEnGrid = false,
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdPadre),
            Controlador = nameof(enumControladoresTerceros.Personas),
            BuscarPor = ltrDePersonas.FiltarPorNombreDeMadrePadre,
            SeleccionarDe = typeof(PersonaDto),
            VistaDondeNavegar = enumVistasTerceros.CrudPersonas,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Persona) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Persona,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 2,
            EditableAlCrear = true,
            EditableAlEditar = true,
            AutoSpan = true)]
        public string Padre { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            VisibleEnEdicion = true,
            Etiqueta = "Fotografía",
            Ayuda = "Seleccione un fichero",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.Archivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.Imagenes,
            UrlDelArchivo = nameof(Archivo),
            Obligatorio = false,
            Fila = 2,
            Columna = 0)]
        public int? IdArchivo { get; set; }

        [IUPropiedad(TipoDeControl = enumTipoControl.ImagenDelCanvas)]
        public string Archivo { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Curso asignado",
            Ayuda = "Curso del niño/a",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Curso),
            Fila = 3,
            Columna = 0,
            VisibleEnGrid = false,
            VisibleAlCrear = false,
            AutoSpan = false,
            Controlador = nameof(enumControladoresGuarderias.CursosDeGuarderia),
            VistaDondeNavegar = enumVistasGuarderias.CrudCursosDeGuarderia
            )
        ]
        public int? IdCurso { get; set; }

        [IUPropiedad( Etiqueta = "Curso",VisibleEnGrid = true, VisibleAlCrear =false, VisibleAlEditar = false, Obligatorio =false)]
        public string Curso { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Baja",
            Ayuda = "indica si el niño está de baja",
            Fila = 6,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            EditableAlCrear = false,
            EditableAlEditar = true,
            VisibleAlCrear = false,
            VisibleEnGrid = false
            )
        ]
        public bool Baja { get; set; }

        [IUPropiedad(Visible = false)]
        public int? IdAgenda { get; set; }

        [IUPropiedad(Visible = false)]
        public string Agenda { get; set; }

    }
}
