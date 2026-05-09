using Utilidades;

namespace ModeloDeDto.Entorno
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class MiCertificadoDto : ElementoDto
    {       

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Certificado",
            Ayuda = "Seleccione un certificado",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.Certificados,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.ApiDeCertificados) + "." + nameof(enumFunctionTs.BlanquearPasswordDelCertificado) + "(¨[idContenedor]¨)",
            Fila = 1,
            Columna = 0)]
        public int? IdArchivoDelCertificado { get; set; }


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
