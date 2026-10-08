using System.Collections.Generic;
using Utilidades;

namespace ModeloDeDto.Entorno
{


    [IUDto(AnchoEtiqueta = 20
         , AnchoSeparador = 5)]
    public class MenuDto : ElementoDto
    {
        //----------------------------------------------------------------
        private const string MostrarPadre = "[Padre].[Nombre]";

        [IUPropiedad(
            Etiqueta = "Id del menú padre",
            Visible = false
            )
        ]
        public int? idPadre { get; set; }

        [IUPropiedad(
            Etiqueta = "Padre",
            Ayuda = "Indique el menú padre",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(MenuDto),
            GuardarEn = nameof(idPadre),
            MostrarExpresion = MostrarPadre,
            Controlador = nameof(enumControladoresEntorno.Menus),
            VistaDondeNavegar = enumVistasEntorno.CrudMenu,
            AplicarJoin = true,
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            PorAnchoMnt = 15,
            Obligatorio = false
            )
        ]
        public  string Padre { get; set; }
        //----------------------------------------------------------------

        [IUPropiedad(
            Etiqueta = "Menu",
            Ayuda = "Nombre del menú",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 1,
            Posicion = 0,
            Ordenar = true,
            PorAnchoMnt = 15
            )
        ]
        public string Nombre { get; set; }
        //--------------------------------------------

        [IUPropiedad(
            Etiqueta = "Orden",
            Ayuda = "orden del menú",
            Tipo = typeof(int),
            Fila = 0,
            Columna = 1,
            Posicion = 1,
            AnchoMaximo = "10em",
            Ordenar = true,
            VisibleEnGrid = true
            )
        ]
        public string Orden { get; set; }
        //----------------------------------------------------------------

        [IUPropiedad(
            Etiqueta = "Icono",
            Ayuda = "Seleccione un icono",
            TipoDeControl = enumTipoControl.UrlDeArchivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.Imagenes,
            RutaDestino = "/images/menu",
            Tipo = typeof(string),
            Fila = 3,
            Columna = 0,
            PorAnchoMnt = 15
            )
        ]
        public string Icono { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descripción",
           Ayuda = "Descripción de la opción de menú",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           VisibleEnGrid = false,
           Obligatorio = false,
           NumeroDeFilas = 5,
            AutoSpan = true,
           Fila = 2,
           Columna = 0
          )
        ]
        public string Descripcion { get; set; }
        //----------------------------------------------------------------

        [IUPropiedad(
            Visible = false
            )
        ]
        public List<MenuDto> Submenus { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = nameof(VistaMvc),
            Ayuda = "Seleccione la vista",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(VistaMvcDto),
            GuardarEn = nameof(idVistaMvc),
            Controlador = nameof(enumControladoresEntorno.VistaMvc),
            VistaDondeNavegar = enumVistasEntorno.CrudVistaMvc,
            Fila = 3,
            Columna = 1,
            Obligatorio = false
            )
        ]
        public string VistaMvc { get; set; }
        [IUPropiedad(Etiqueta = "Id de la vista",
            Visible = false)]
        public int? idVistaMvc { get; set; }
        //----------------------------------------------------------------


        [IUPropiedad(
            Etiqueta ="Opción activa",
            Ayuda = "indica si la opción de menú está activa",
            VisibleEnGrid = true,
            PorAnchoMnt = 15,
            Obligatorio = true,
            Fila = 3,
            Columna = 1,
            Posicion =1,
            TipoDeControl = enumTipoControl.Check,
            Alineada = enumAliniacion.derecha,
            AlinearContenido = enumAliniacion.derecha,
            ValorPorDefecto = false
            )
        ]
        public bool Activo { get; set; }
        
        //----------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Parámetros",
           Ayuda = "Parámetros para adjuntar a la url de entrada",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           VisibleEnGrid = false,
           Obligatorio = false,
           NumeroDeFilas = 3,
           Fila = 4,
            AutoSpan = true,
           Columna = 0
          )
        ]
        public string Parametros { get; set; }

        //----------------------------------------------------------------
        // permiso de acceso de la vista del menú: la opción le sale a un usuario si es administrador o tiene este permiso.
        // Es de solo lectura; su navegador abre el permiso en el crud de permisos, para asignarlo a un rol o a un puesto
        [IUPropiedad(
            Etiqueta = "Permiso de acceso",
            Ayuda = "Permiso que da acceso a la vista del menú",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Permiso),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 5,
            Columna = 0
            )
        ]
        public int? IdPermiso { get; set; }

        [IUPropiedad(Visible = false)]
        public string Permiso { get; set; }

    }


}
