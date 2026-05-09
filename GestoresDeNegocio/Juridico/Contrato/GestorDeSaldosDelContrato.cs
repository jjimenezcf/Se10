using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Juridico;
using ModeloDeDto.Juridico;
using Gestor.Errores;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Juridico
{
    public class ltrSaldosDelContrato
    {
        public static string FiltroPorImporte => nameof(FiltroPorImporte);
    }
    public class GestorDeSaldosDelContrato : GestorDeElementos<ContextoSe, SaldosDelContratoDtm, SaldosDelContratoDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;


        public class MapearSaldosDelContrato : Profile
        {
            public MapearSaldosDelContrato()
            {
                CreateMap<SaldosDelContratoDtm, SaldosDelContratoDto>();
                CreateMap<SaldosDelContratoDto, SaldosDelContratoDtm>();
            }
        }

        public GestorDeSaldosDelContrato(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeSaldosDelContrato Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeSaldosDelContrato(contexto, mapeador);
        }

        protected override void AntesDePersistir(SaldosDelContratoDtm saldos, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(saldos, parametros);
            var contrato = (ContratoDtm)parametros.Parametros[nameof(ContratoDtm)];

            if (saldos.Aviso > saldos.Bloqueo)
                GestorDeErrores.Emitir($"El porcentaje de aviso, {saldos.Aviso}, ha de ser menor que el de bloqueo, {saldos.Bloqueo}");
            if (saldos.Bloqueo > (decimal)999.99)
                GestorDeErrores.Emitir($"El porcentaje de bloqueo, {saldos.Bloqueo}, ha de ser menor de {999.99}");
            if (saldos.Adendado > 0 && saldos.Importe == 0)
                GestorDeErrores.Emitir($"No puede haber adenda sin indicar el importe del contrato");
            if (saldos.Importe < 0 || saldos.Importe - saldos.Adendado < 0)
                GestorDeErrores.Emitir($"No puede haber un contrato con importe negativo");
            
            if (parametros.Insertando && saldos.Adendado > 0 ) GestorDeErrores.Emitir($"No se puede adendar si el contrato no está vigente");

            if (parametros.Modificando)
            {
                saldos.ValidarPorcentageDeBloqueo(Contexto);

                var saldoEnBd = (SaldosDelContratoDtm)parametros.registroEnBd;

                if (parametros.EsUnaPeticion && saldos.Notificado != saldoEnBd.Notificado) GestorDeErrores.Emitir($"El indicador de aviso solo lo puede actualizar el sistema");

                if (!contrato.EstaEnLaEtapa(VariablesDeContratos.etapaDeElaboracion))
                {
                    if (saldos.Importe != saldoEnBd.Importe) GestorDeErrores.Emitir($"El importe sólo se puede modificar durante la elaboración del contrato");
                    if (saldos.Bloqueo != saldoEnBd.Bloqueo) GestorDeErrores.Emitir($"El porcentage de bloqueo sólo se puede modificar durante la elaboración del contrato");
                }
                if (!contrato.EstaEnLaEtapa(VariablesDeContratos.etapaVigente) && saldos.SeHaModificadoElCampo<decimal>(x => x.Name == nameof(SaldosDelContratoDtm.Adendado), parametros))
                    GestorDeErrores.Emitir($"No se puede adendar si el contrato no está vigente");
            }
        }

        protected override void DespuesDeMapearElElemento(SaldosDelContratoDtm saldo, SaldosDelContratoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(saldo, elemento, parametros);
            var contrato = Contexto.SeleccionarPorId<ContratoDtm>(saldo.IdElemento);
            var avance = contrato.Ampliacion<AvanceDtm>(Contexto);
            elemento.Saldo = saldo.Importe + saldo.Adendado - avance.Planificado - avance.Realizado - avance.Facturado;
        }
    }
}
