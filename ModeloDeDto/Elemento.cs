using Utilidades;
using ServicioDeDatos.Seguridad;

namespace ModeloDeDto
{

    public class ElementoDto : IElementoDto
    {
        [IUPropiedad(
            Etiqueta = "Id",
            Ayuda = "id del elemento",
            Tipo = typeof(int),
            Visible = false
            )
        ]
        public int Id { get; set; }

        [IUPropiedad(
            Etiqueta = "ModoDeAcceso",
            Ayuda = "modo de acceso al elemento",
            Tipo = typeof(string),
            Visible = false
            )
        ]
        public enumModoDeAccesoDeDatos ModoDeAcceso { get; set; }

        [IUPropiedad(Visible = false)]
        public string informacion { get; set; }

        [IUPropiedad(Visible = false)]
        public bool EstaCancelada { get; set; }

        [IUPropiedad(Visible = false)]
        public bool EstaTerminada { get; set; }

        [IUPropiedad(Visible = false)]
        public bool NombreModificable { get; set; }
       
        [IUPropiedad(Visible = false)]
        public bool DelSistema { get; set; }

        [IUPropiedad(Oculto = true, Fila =0, Columna = 0, Obligatorio = false, Formato = enumFormato.base64)]
        public byte[] RowVersion { get; set; }
    }

    public class EsUnaAmpliacionDto : ElementoDto, IAmpliacionDto
    {
        [IUPropiedad(
            Etiqueta = "IdElemento",
            Ayuda = "id del elemento",
            Tipo = typeof(int),
            Visible = false
            )
        ]
        public int IdElemento { get; set; }
    }

    public class EsUnDetalleDto : ElementoDto, IDetalleDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Elemento",
            Ayuda = "elemento",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Elemento),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false,
            AutoSpan = true,
            PorAnchoMnt = 30
            )
        ]
        public virtual int IdElemento { get; set; }

        [IUPropiedad(Visible = false)]
        public string Elemento { get; set; }

        [IUPropiedad(Visible = false)]
        public string Titulo { get; set; }
    }

}