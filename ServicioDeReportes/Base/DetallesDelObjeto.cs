using GestorDeElementos;
using GestoresDeNegocio.Negocio;
using ModeloDeDto;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeReportes.Base
{
    public enum enumEncabezadosDeTablas { Hitos, Archivos, Observaciones, Direcciones }

    public class DetallesDelObjeto
    {
        public Dictionary<string, List<IElementoDto>> Detalles { get; }

        public List<HitoDto> Hitos { get; }
        public List<ObservacionDto> Observaciones { get; }
        public List<DireccionDto> Direcciones { get; }
        public enumNegocio Negocio { get; }
        public ContextoSe Contexto { get; }

        public DetallesDelObjeto(ContextoSe contexto, ElementoDtm elemento)
        {
            Contexto = contexto;
            Detalles = new Dictionary<string, List<IElementoDto>>();

            var tipo = elemento.GetType();
            Negocio = NegociosDeSe.NegocioDeUnDtm(tipo);

            Hitos = Negocio.UsaFlujo()
            ? new GestorDeHitos(contexto, Negocio).LeerElementos(0, -1, new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(
                    clausula: nameof(IHitoDtm.IdElemento),
                    criterio: enumCriteriosDeFiltrado.igual,
                    valor: elemento.Id)},
                orden: null,
                parametros: new Dictionary<string, object> { { ltrParametrosNeg.AplicarJoin, true } }).ToList()
            : new List<HitoDto>();

            Observaciones = Negocio.UsaObservaciones()
            ? new GestorDeObservaciones(contexto, Negocio).LeerElementos(0, -1, new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(
                    clausula: nameof(IObservacion.IdElemento),
                    criterio: enumCriteriosDeFiltrado.igual,
                    valor: elemento.Id)},
                orden: null,
                parametros: new Dictionary<string, object> { { ltrParametrosNeg.AplicarJoin, true } }).ToList()
            : new List<ObservacionDto>();

            Direcciones = Negocio.UsaDirecciones()
            ? new GestorDeDirecciones(contexto, Negocio).LeerElementos(0, -1, new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(
                    clausula: nameof(IDireccionDtm.IdElemento),
                    criterio: enumCriteriosDeFiltrado.igual,
                    valor: elemento.Id)},
                orden: null,
                parametros: new Dictionary<string, object> { { ltrParametrosNeg.AplicarJoin, true } }).ToList()
            : new List<DireccionDto>();




            var listaDeTipos = Negocio.TiposDeDetalles();
            foreach (Type tipoDeDetalle in listaDeTipos)
            {
                
                if (Negocio.UsaTipo() && !ApiDeDetalles.UsaElDetalleDe(Negocio, contexto, ((IUsaTipo)elemento).IdTipo, tipoDeDetalle))
                    continue;

                var gestor = NegociosDeSe.CrearGestorDeUnDetalle(tipoDeDetalle, contexto);

                Detalles[tipoDeDetalle.Name.Replace("Dtm", "")] = ((IEnumerable<object>)gestor.LeerElementos(0, -1, new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(
                    clausula: nameof(IDetalle.IdElemento),
                    criterio: enumCriteriosDeFiltrado.igual,
                    valor: elemento.Id)}, orden: null,
                    opcionesDeMapeo: new Dictionary<string, object> { { ltrParametrosNeg.AplicarJoin, true } })).Cast<IElementoDto>().ToList();
            }
        }

    }
}
