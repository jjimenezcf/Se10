using System;
using Utilidades;

namespace ModeloDeDto.Terceros
{
    // Ampliación de solo consulta de la sociedad con cómo se contabiliza; no tiene tabla, se calcula al leerla
    // (ver InformacionContableDeSociedadController.epLeerAmpliacionPorIdNegocio)
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeCrear = false, OpcionDeEditar = false, OpcionDeBorrar = false)]
    public class InformacionContableDeSociedadDto : EsUnaAmpliacionDto
    {
        [IUPropiedad(
            Etiqueta = "Último cierre",
            Ayuda = "Fecha hasta la que está cerrada contablemente la sociedad",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false,
            Fila = 0,
            Columna = 0)]
        public DateTime? UltimoCierre { get; set; }

        [IUPropiedad(
            Etiqueta = "Contabilidad",
            Ayuda = "Sistema contable en el que se contabiliza la sociedad",
            TipoDeControl = enumTipoControl.Editor,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false,
            Fila = 0,
            Columna = 1)]
        public string SistemaContable { get; set; }

        [IUPropiedad(
            Etiqueta = "Dígitos",
            Ayuda = "Dígitos de las cuentas auxiliares en la contabilidad",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(int),
            Alineada = enumAliniacion.derecha,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false,
            Fila = 0,
            Columna = 2)]
        public int Digitos { get; set; }
    }
}
