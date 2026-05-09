using ServicioDeDatos.Juridico;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;
using System.Collections.Generic;
using System.Linq;
using ServicioDeDatos.MaestrosTecnico;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeLote
    {

        public static void ValidarQueLaLineaDelLoteEsModificable(this UnitariosDeUnLoteDtm unitario, ContextoSe contexto)
        {
            var lote = unitario.Lote == null
              ? contexto.SeleccionarPorId<LoteDeUnContratoDtm>(unitario.IdLote, aplicarJoin: true, parametros: new Dictionary<string, object> { { ltrLotesDeUnContrato.JoinConContratos, true } })
              : unitario.Lote;

            if (!ModoDeAcceso.HayPermisosDe(lote.ModoDeAccesoAlLote(contexto), enumModoDeAccesoDeDatos.Gestor))
                Emitir($"no se puede modificar la línea del lote, ya que el lote no es modificable");
        }

        public static void ValidarQueElLoteEsModificable(this LoteDeUnContratoDtm lote, ContextoSe contexto)
        {
            if (!ModoDeAcceso.HayPermisosDe(lote.ModoDeAccesoAlLote(contexto), enumModoDeAccesoDeDatos.Gestor))
                Emitir($"El lote indicado no es modificable por no ser editable su contrato, o por estar vigente");
        }

        public static enumModoDeAccesoDeDatos ModoDeAccesoAlLote(this LoteDeUnContratoDtm lote, ContextoSe contexto)
        {
            var contrato = contexto.SeleccionarPorId<ContratoDtm>(lote.IdContrato, aplicarJoin: true);

            var modoDeAcceso = ApiDePermisos.LeerModoDeAcceso(contexto, enumNegocio.Contrato, contrato.Id);
            if (ModoDeAcceso.HayPermisosDe(modoDeAcceso, enumModoDeAccesoDeDatos.Gestor))
            {
                if (!contrato.EstaEnElaboracion() && !contrato.EstaPdtDeProroga())
                    modoDeAcceso = enumModoDeAccesoDeDatos.Consultor;

                var hayGenerados = contexto.Set<PlanificadorDeVentaDtm>().Where(y => y.IdLote == lote.Id).Any(x => x.Generado);
                if (hayGenerados) modoDeAcceso = enumModoDeAccesoDeDatos.Consultor;

            }
            return modoDeAcceso;
        }

        public static ContratoDtm Contrato(this LoteDeUnContratoDtm lote, ContextoSe contexto) => lote.Contrato == null ? contexto.SeleccionarPorId<ContratoDtm>(lote.IdContrato) : lote.Contrato; 

        public static UnitarioDtm Unitario(this UnitariosDeUnLoteDtm unitario, ContextoSe contexto) => unitario.Unitario == null ? contexto.SeleccionarPorId<UnitarioDtm>(unitario.IdUnitario) : unitario.Unitario;

    }
}
