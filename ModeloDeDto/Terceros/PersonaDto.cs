using Utilidades;

namespace ModeloDeDto.Terceros
{
    [IUDto(AnchoEtiqueta = 20
         , AnchoSeparador = 5
         , MostrarExpresion = nameof(Expresion))]
    public class PersonaDto : ElmentoAuditadoDto, IUsaNombreDto, IUsaBajaDto
    {

        //--------------------------------------------
        [IUPropiedad(Etiqueta = "Expresion", 
            TipoDeControl = enumTipoControl.Editor, 
            VisibleEnEdicion = false,
            Ordenar = true,
            VisibleEnGrid = true,
            Obligatorio = false)]
        public new string Expresion { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Persona",
            Ayuda = "Indique el nombre de la persona",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Obligatorio = true,
            LongitudMaxima = 250,
            VisibleEnGrid = false
          )
        ]
        public string Nombre { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Apellidos",
            Ayuda = "indique los apellidos",
            LongitudMaxima = 255,
            Fila = 0,
            Columna = 1,
            VisibleEnGrid = false
          )
        ]
        public string Apellidos { get; set; }
        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "NIF",
           Ayuda = "indique el NIF/CIF",
           LongitudMaxima = 15,
           Fila = 1,
           Columna = 0,
            VisibleEnGrid = false
          )
        ]
        public string Nif { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Es nie",
           Ayuda = "Indique si es un nie",
           TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
           Fila = 1,
           Columna = 1,
            VisibleEnGrid = false
          )
        ]
        public string EsNie { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "eMail",
           Ayuda = "indique el eMail de la persona",
           Fila = 2,
           Columna = 0,
           Ordenar =true,
           LongitudMaxima = 50
          )
        ]
        public string eMail { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Teléfono",
           Ayuda = "indique el teléfono de la persona",
           Fila = 2,
           Columna = 1,
           LongitudMaxima = 15
          )
        ]
        public string Telefono { get; set; }
        
        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Crear interlocutor",
            Ayuda = "indica si al crear la persona también se crea un interlocutor",
            Fila = 3,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            EditableAlCrear = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Persona_CrearInterlocutor_Change) + "(this)"
            )
        ]
        public bool CrearInterlocutor { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Crear procurador",
            Ayuda = "indica si al crear la persona también se crea como procurador",
            Fila = 4,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            EditableAlCrear = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Persona_CrearProcurador_Change) + "(this)"
            )
        ]
        public bool CrearProcurador { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Crear abogado",
            Ayuda = "indica si al crear la persona también se crea como abogado",
            Fila = 5,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            EditableAlCrear = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Persona_CrearAbogado_Change) + "(this)"
            )
        ]
        public bool CrearAbogado { get; set; }


        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Crear proveedor",
            Ayuda = "indica si al crear la persona tambié, se crea como proveedor",
            Fila = 6,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            EditableAlCrear = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Persona_CrearProveedor_Change) + "(this)"
            )
        ]
        public bool CrearProveedor { get; set; }


        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Crear cliente",
            Ayuda = "indica si al crear la persona también se crea como cliente",
            Fila = 7,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            EditableAlCrear = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Persona_CrearCliente_Change) + "(this)"
            )
        ]
        public bool CrearCliente { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Crear trabajador",
            Ayuda = "indica si al crear la persona también se crea como trabajador",
            Fila = 8,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            EditableAlCrear = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Persona_CrearTrabajador_Change) + "(this)"
            )
        ]
        public bool CrearTrabajador { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Es interlocutor",
            Ayuda = "indica si la persona es un interlocutor",
            Fila = 3,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            VisibleAlCrear = false,
            VisibleAlEditar = true,
            EditableAlEditar = false
            )
        ]
        public bool EsInterlocutor { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Baja",
            Ayuda = "indica si la persona está de baja",
            Fila = 6,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleAlCrear = false,
            VisibleEnGrid = false
            )
        ]
        public bool Baja { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)] public int? IdCliente { get; set; }
        [IUPropiedad(Visible = false)] public int? IdProveedor { get; set; }
        [IUPropiedad(Visible = false)] public int? IdInterlocutor { get; set; }
        [IUPropiedad(Visible = false)] public int? IdAbogado { get; set; }
        [IUPropiedad(Visible = false)] public int? IdProcurador { get; set; }

    }
}
