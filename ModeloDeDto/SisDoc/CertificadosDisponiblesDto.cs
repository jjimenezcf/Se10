using ModeloDeDto.Entorno;
using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class CertificadosDisponiblesDto : ElementoDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Certificado",
            Ayuda = "seleccione el certificado con el que firmar",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CertificadoDto),
            Controlador = nameof(enumControladoresEntorno.Certificados),
            GuardarEn = nameof(Id),
            MostrarExpresion = nameof(Certificado),
            Fila = 1,
            Columna = 0
            )
        ]
        public string Certificado { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Contraseña",
            Ayuda = "indique la contraseña del certificado",
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            EditableAlEditar = true,
            TipoDeControl = enumTipoControl.Password,
            LongitudMaxima = 250,
            AutoSpan = true
          )
        ]
        public string Password { get; set; }

    }
}
