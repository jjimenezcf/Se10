/*
0: cg:(select2)   tipo: (select2)  estado: (editor en consulta)
1: referencia:(editor en consulta) | nombre: (editor)
2: descripción: (text area)
 * */

namespace ModeloDeDto.SistemaDocumental
{
    public class IndCircuitosDoc
    {
        public const string IdTipoEstimacionDirecta = nameof(IdTipoEstimacionDirecta);
        public const string TipoEstimacionDirecta = nameof(TipoEstimacionDirecta);

        public const string IdTipoLoteContable = nameof(IdTipoLoteContable);
        public const string TipoLoteContable = nameof(TipoLoteContable);

        public const string IdTipoFichada = nameof(IdTipoFichada);
        public const string TipoFichada = nameof(TipoFichada);

        public const string IdTipoActividadFormativa = nameof(IdTipoActividadFormativa);
        public const string TipoActividadFormativa = nameof(TipoActividadFormativa);
    }

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class CircuitoDocDto : ElementoDeUnProcesoDto
    {

        [IUPropiedad(Visible = false)]
        public bool? EsLoteContable { get; set; }

        [IUPropiedad(Visible = false)]
        public bool? EsEstimacionDirecta { get; set; }

        [IUPropiedad(Visible = false)]
        public bool? EsFichada { get; set; }
    }


}
