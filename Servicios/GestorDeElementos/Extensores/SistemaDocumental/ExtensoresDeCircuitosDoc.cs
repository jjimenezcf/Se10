using Gestor.Errores;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestorDeElementos.Extensores
{
    public static class ExtensoresDeCircuitosDoc
    {
        public static bool UsaElDetalleDe(ContextoSe contexto, int idTipo, Type tipoDeDetalle)
        {
            if (typeof(InscritosEnActividadDtm) == tipoDeDetalle && VariablesDeCircuitosDoc.EsUnaActividadFormativa(idTipo))
                return true;
            if (typeof(VoluntarioDeActividadDtm) == tipoDeDetalle && VariablesDeCircuitosDoc.EsUnaActividadFormativa(idTipo))
                return true;

            return false;
        }

        public static bool UsaLaAmpliacionDe(ContextoSe contexto, int idTipo, Type tipoAmpliacion)
        {
            if (tipoAmpliacion == typeof(DatosDeActividadFormativaDtm) && VariablesDeCircuitosDoc.EsUnaActividadFormativa(idTipo))
                return true;

            return false;
        }

        public static IQueryable<VinculoDtm> CircuitosDoc(this enumNegocio negocio, ContextoSe contexto)
        {
            switch (negocio)
            {
                case enumNegocio.Trabajador:
                    return contexto.Set<CircuitoDocDeUnTrabajadorDtm>();
                case enumNegocio.Expediente:
                    return contexto.Set<CircuitoDocDeUnExpedienteDtm>();
            }

            throw new Exception($"Se debe indicar como obtener los circuitos documentales vinculados al negocio: {negocio}");
        }

        public static bool EsUnaFichada(this CircuitoDocDtm circuito, ContextoSe contexto)
        {
            var tiposDeFichadas = enumNegocio.CircuitoDoc.Parametro(enumParametrosDeCircuitosDoc.CAD_IdsDeTiposDeFichadas, crearParametro: true, valorPorDefecto: Literal.Cero).Valor.ToLista<int>();
            if (tiposDeFichadas.Count == 0 || (tiposDeFichadas.Count == 1 && tiposDeFichadas[0] == 0))
                GestorDeErrores.Emitir($"Debe parametrizar el parámetro '{enumParametrosDeCircuitosDoc.CAD_IdsDeTiposDeFichadas}', si no ha definido tipo de fichadas asígne el valor -1");

            return tiposDeFichadas.Contains(circuito.IdTipo);
        }

        public static bool EsEstimacionDirecta(this CircuitoDocDtm circuito) => EsEstimacionDirecta(circuito.IdTipo);

        public static bool EsEstimacionDirecta(int idTipo) => idTipo == VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaEstimacionDirecta(errorSiNoEstaDefinido: true);

        public static bool EsLoteDePreasientos(this CircuitoDocDtm circuito) => EsLoteDePreasientos(circuito.IdTipo);

        public static bool EsLoteDePreasientos(int idTipo) => idTipo == VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaLoteDePreasientos(errorSiNoEstaDefinido: true);

        public static TrabajadorDtm ValidarFichada(this CircuitoDocDtm circuito, ContextoSe contexto)
        {
            var cg = contexto.SeleccionarPorId<CentroGestorDtm>(circuito.IdCg, errorSiNoHay: false);
            if (cg is null)
                GestorDeErrores.Emitir($"El centro de gestión indicado no existe");
            var trabajadores = contexto.Set<TrabajadorDtm>().Where(x => x.IdCg == circuito.IdCg && x.Nombre.ToLower() == circuito.Nombre.ToLower());
            if (trabajadores.Count() == 0)
                GestorDeErrores.Emitir($"No hay ningún trabajador llamado '{circuito.Nombre}' en el centro gestor '{cg.Expresion}'");
            if (trabajadores.Count() > 1)
                GestorDeErrores.Emitir($"Hay {trabajadores.Count()} trabajadores identificados como '{circuito.Nombre}'");

            var trabajador = trabajadores.First();

            circuito.Nombre = trabajador.Nombre;

            if (trabajador.Baja)
                GestorDeErrores.Emitir($"El trabajador '{circuito.Nombre}' está de baja");

            return trabajador;

        }


        public static void TrasCancelarCad(this CircuitoDocDtm circuito, ContextoSe contexto)
        {
            if (circuito.EsLoteDePreasientos())
            {
                var preasientos = VinculoSql.LeerVinculosAl(contexto, typeof(PreasientoDtm), enumNegocio.CircuitoDoc, typeof(CircuitoDocDtm), circuito.Id, filtros: null);
                foreach (var vinculo in preasientos)
                {
                    var preasiento = contexto.SeleccionarPorId<PreasientoDtm>(vinculo.idElemento1);
                    preasiento.AnularContabilizacion(contexto, loteContable: circuito);
                }
                return;
            }

            if (circuito.EsEstimacionDirecta())
            {
                var facturasEmt = VinculoSql.LeerVinculosAl(contexto, typeof(FacturaEmtDtm), enumNegocio.CircuitoDoc, typeof(CircuitoDocDtm), circuito.Id, filtros: null);
                foreach (var vinculo in facturasEmt)
                {
                    contexto.EliminarPorId<CircuitoDocDeUnaFacturaEmtDtm>(vinculo.Id, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
                }

                var facturasRec = VinculoSql.LeerVinculosAl(contexto, typeof(FacturaRecDtm), enumNegocio.CircuitoDoc, typeof(CircuitoDocDtm), circuito.Id, filtros: null);
                foreach (var vinculo in facturasRec)
                {
                    contexto.EliminarPorId<CircuitoDocDeUnaFacturaRecDtm>(vinculo.Id, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
                }

                var pagos = VinculoSql.LeerVinculosAl(contexto, typeof(PagoDtm), enumNegocio.CircuitoDoc, typeof(CircuitoDocDtm), circuito.Id, filtros: null);
                foreach (var vinculo in pagos)
                {
                    contexto.EliminarPorId<CircuitoDocDeUnPagoDtm>(vinculo.Id, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
                }
            }
        }
    }
}
