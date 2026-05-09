using ServicioDeDatos;
using Utilidades;
using ModeloDeDto.Entorno;
using ServicioDeDatos.Entorno;

namespace ModeloDeDto.Terceros
{
    [IUDto(MostrarExpresion = "Certificados societarios")]
    public class CertificadoDeUnaSociedadDto
    {

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Sociedad",
            Ayuda = "Sociedad gestionada",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Sociedad),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public int IdSociedad { get; set; }

        [IUPropiedad(
            Etiqueta = "Sociedad",
            Visible = false
            )
        ]
        public string Sociedad { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del certificado",
            Visible = false
            )
        ]
        public int IdCertificado { get; set; }


        [IUPropiedad(
            Etiqueta = "Certificado",
            Ayuda = "seleccione el certificado societario",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CertificadoDto),
            GuardarEn = nameof(IdCertificado),
            Controlador = nameof(enumControladoresEntorno.Certificados),
            BuscarPor = nameof(ltrCertificados.FiltrarParaSociedad),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 1,
            Columna = 0,
            Obligatorio = true,
            AutoSpan = true
            )
        ]
        public string Certificado { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Password",
            Ayuda = "password del certificado",
            TipoDeControl = enumTipoControl.Password,
            Tipo = typeof(string),
            Obligatorio = false,
            Fila = 2,
            Columna = 0)]
        public string PassworDelCertificado { get; set; }
    }
}
