using System;
using ServicioDeDatos;
using Utilidades;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Terceros;
using System.Threading.Tasks;
using ServicioDeDatos.Negocio;

namespace GestorDeElementos.Extensores
{
    public static class ltrDePleitos
    {
        public static readonly string ModuloNoActivo = "El módulo de gestión de pleitos no está activo";
    }

    public static class ExtensorDePleitos
    {
        public static async Task<bool> ModuloActivoAsync()
        {
            return await Task.Run(() => ParametroDeNegocioSql.Parametro(enumNegocio.Interlocutor, enumParametrosDeInterlocutor.INT_TercerosJudiciales, crearParametro: true, valorPorDefecto: false).Valor.EsTrue());
        }

        public static bool ModuloActivo(ContextoSe contexto) => enumNegocio.Interlocutor.LeerCrearParametro(contexto, enumParametrosDeInterlocutor.INT_TercerosJudiciales, valor: "N").Valor.EsTrue();

        public static bool UsaLaAmpliacionDe(ContextoSe contexto, int idTipo, Type tipoAmpliacion)
        {
            var tipoDtm = (TipoDePleitoDtm)enumNegocio.Pleito.CrearGestorDeTipo(contexto).LeerRegistroPorId(idTipo, aplicarJoin: false);
            if (tipoDtm.ClaseDePleito == enumClaseDePleito.recobro && tipoAmpliacion == typeof(RecobroDtm))
                return true;

            return false;
        }

        internal static bool UsaElDetalleDe(ContextoSe contexto, int idTipo, Type tipoDeDetalle)
        {
            if (tipoDeDetalle.Equals(typeof(MinutaDtm)))
                return true;

            return false;
        }
    }
}
