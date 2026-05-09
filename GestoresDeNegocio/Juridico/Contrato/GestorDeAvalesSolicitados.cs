using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Juridico;
using ModeloDeDto.Juridico;
using GestorDeElementos.Extensores;
using Gestor.Errores;

namespace GestoresDeNegocio.Juridico
{
    public class ltrAvalesSolicitados
    {
        public static string PendienteDeAval = nameof(PendienteDeAval);
        public static string AvalDevuelto = nameof(AvalDevuelto);
        public static string MostrarAval = nameof(MostrarAval);
    }
    public class GestorDeAvalesSolicitados : GestorDeElementos<ContextoSe, AvalSolicitadoDtm, AvalSolicitadoDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;


        public class MapearAvalesSolicitados : Profile
        {
            public MapearAvalesSolicitados()
            {
                CreateMap<AvalSolicitadoDtm, AvalSolicitadoDto>();
                CreateMap<AvalSolicitadoDto, AvalSolicitadoDtm>();
            }
        }

        public GestorDeAvalesSolicitados(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeAvalesSolicitados Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeAvalesSolicitados(contexto, mapeador);
        }

        protected override void AntesDePersistir(AvalSolicitadoDtm aval, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(aval, parametros);
            var contrato = (ContratoDtm) aval.AmpliacionDe(Contexto);
            if (!contrato.EstaEnLaEtapa(VariablesDeContratos.etapaDeElaboracion) && parametros.Modificando)
            {
                if (aval.ImporteAval != ((AvalSolicitadoDtm)parametros.registroEnBd).ImporteAval)
                    GestorDeErrores.Emitir($"No se puede modificar el importe del aval, ya que el contrato no está en la etapa de eleboración");
                if (aval.MesesDeAval.Entero() != ((AvalSolicitadoDtm)parametros.registroEnBd).MesesDeAval.Entero())
                    GestorDeErrores.Emitir($"No se puede modificar los meses que han de pasar para solicitar la recuperación del aval");
            }
            
            if (parametros.EsUnaPeticion && aval.SeHaModificadoElCampo<bool?>(x => x.Name == nameof(AvalSolicitadoDtm.AvisoEnviado), parametros))
                GestorDeErrores.Emitir($"No se puede modificar el flag de 'aviso envido' con una petición del controlador");

            contrato.SiHayAvalHayFechaDeFinDeContrato(Contexto);

        }
    }
}
