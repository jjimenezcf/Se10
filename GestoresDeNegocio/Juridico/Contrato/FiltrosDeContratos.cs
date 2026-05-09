using System;
using System.Linq;
using ServicioDeDatos.Juridico;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using static GestoresDeNegocio.Juridico.GestorDelPlanificadorDeVentas;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.Expediente;
using iText.Layout.Element;
using System.Collections.Generic;
using ServicioDeDatos.Elemento;
using static ServicioDeDatos.Ventas.VariablesDePlfsDeVenta;
using ModeloDeDto.Gastos;
using ModeloDeDto.Ventas;

namespace GestoresDeNegocio.Juridico
{
    internal static class FiltrosDeContratos
    {
        public static IQueryable<ContratoDtm> AplicarFiltroPorEvolucion(this IQueryable<ContratoDtm> consulta, ContextoSe contexto, ClausulaDeFiltrado filtro)
        {
            if (filtro.Clausula.Equals(ltrDeUnContrato.FiltrosPorEvolucion, StringComparison.CurrentCultureIgnoreCase))
            {
                if (filtro.Valor == ltrAvalesSolicitados.PendienteDeAval || filtro.Valor == ltrAvalesSolicitados.AvalDevuelto)
                {
                    var contratosPdtDeAval = contexto.Set<AvalSolicitadoDtm>().Where(x => filtro.Valor == ltrAvalesSolicitados.AvalDevuelto 
                                                                                          ? x.AvisoEnviado == true
                                                                                          : x.AvisoEnviado == null || x.AvisoEnviado == false);
                    consulta = consulta.Where(x => contratosPdtDeAval.Any(y => y.IdElemento == x.Id));
                    filtro.Aplicado = true;
                    return consulta;
                }

                if (filtro.Valor == ltrProrrogas.contratosProrrogados || filtro.Valor == ltrProrrogas.contratosFinalizados)
                {
                    var etapaPdtProrrogar = VariablesDeContratos.etapaPdtProrroga.Split(Simbolos.Coma).ToArray();
                    var etapaBuscada = filtro.Valor == ltrProrrogas.contratosProrrogados
                    ? VariablesDeContratos.etapaVigente.Split(Simbolos.Coma).ToArray()
                    : VariablesDeContratos.etapaFinalizacion.Split(Simbolos.Coma).ToArray();
                    /* Contratos que pasaron por la etapa pendientes de prorrogar  y están en la etapa buscada */
                    var transicionDeProrrogar = contexto.Set<TransicionesDeUnContratoDtm>().Where(x => etapaPdtProrrogar.Contains(x.IdOrigen.ToString()));
                    var hitosConEstaTransicion = contexto.Set<HitosDeUnContratoDtm>().Where(x => transicionDeProrrogar.Any(y => y.Id == x.IdTransicion));
                    var hitosPorElementoPosterioresAlHito = contexto.Set<HitosDeUnContratoDtm>().Where(x => hitosConEstaTransicion.Any(y => x.Id > y.Id && x.IdElemento == y.IdElemento));
                    var hitosSiguienteDeLosContratosSeleccionados = contexto.Set<HitosDeUnContratoDtm>().Where(y => hitosConEstaTransicion.Any(x => x.Id > y.Id));

                    consulta = consulta.Where(x => hitosPorElementoPosterioresAlHito.Any(y => y.IdElemento == x.Id) &&
                                                   transicionDeProrrogar.Any(z => z.IdDestino == x.IdEstado) &&
                                                   etapaBuscada.Contains(x.IdEstado.ToString()));
                    filtro.Aplicado = true;
                    return consulta;
                }

                if (filtro.Valor == ltrLotesDeUnContrato.ConLotes)
                {
                    consulta = consulta.Where(x => contexto.Set<LoteDeUnContratoDtm>().Any(y => y.IdContrato == x.Id));
                    filtro.Aplicado = true;
                    return consulta;
                }
                if (filtro.Valor == ltrLotesDeUnContrato.SinLotes)
                {
                    consulta = consulta.Where(x => contexto.Set<LoteDeUnContratoDtm>().Any(y => y.IdContrato != x.Id));
                    filtro.Aplicado = true;
                    return consulta;
                }
                if (filtro.Valor == ltrPlanificadorDeVentas.ConPlanificadores)
                {
                    consulta = consulta.Where(x => contexto.Set<PlanificadorDeVentaDtm>().Any(y => y.IdContrato == x.Id));
                    filtro.Aplicado = true;
                    return consulta;
                }
                if (filtro.Valor == ltrPlanificadorDeVentas.PlanificadoresPdts)
                {
                    consulta = consulta.Where(x => contexto.Set<PlanificadorDeVentaDtm>().Any(y => y.IdContrato == x.Id && !y.Generado));
                    filtro.Aplicado = true;
                    return consulta;
                }
                if (filtro.Valor == ltrPlanificadorDeVentas.SinPlanificadores)
                {
                    consulta = consulta.Where(x => contexto.Set<PlanificadorDeVentaDtm>().Any(y => y.IdContrato != x.Id));
                    filtro.Aplicado = true;
                    return consulta;
                }
            }
            return consulta;
        }

        public static IQueryable<ContratoDtm> AplicarFiltroDeImporte(this IQueryable<ContratoDtm> consulta, ContextoSe contexto, ClausulaDeFiltrado filtro)
        {
            if (filtro.Clausula.Equals(ltrSaldosDelContrato.FiltroPorImporte, StringComparison.CurrentCultureIgnoreCase))
            {
                var contratosEntreImportes = contexto.Set<SaldosDelContratoDtm>().AplicarFiltroEntreNumeros(filtro, nameof(SaldosDelContratoDtm.Importe), usarAbs: false);
                consulta = consulta.Where(y => contratosEntreImportes.Any(x => x.IdElemento == y.Id));
            }
            return consulta;
        }

        public static IQueryable<ContratoDtm> AplicarFiltroDeCabecera(this IQueryable<ContratoDtm> consulta, ContextoSe contexto, ClausulaDeFiltrado filtro)
        {

            if (filtro.Clausula.Equals(ltrDeUnContrato.FiltroPorConOSinResponsable, StringComparison.CurrentCultureIgnoreCase))
            {
                filtro.Aplicado = true;
                if (filtro.Valor == ltrDeUnContrato.ConResponsable)
                {
                    consulta = consulta.Where(y => y.IdResponsable != null);
                }
                if (filtro.Valor == ltrDeUnContrato.SinResponsable)
                {
                    consulta = consulta.Where(y => y.IdResponsable == null);
                }
            }

            if (filtro.Clausula.Equals(ltrDeUnContrato.FiltroPorResponsable, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.IdResponsable == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }
        
        public static IQueryable<ContratoDtm> AplicarFiltroDeProveedor(this IQueryable<ContratoDtm> consulta, ContextoSe contexto, ClausulaDeFiltrado filtro)
        {

            if (filtro.Clausula.Equals(ltrDatosDelContrato.FiltroPorProveedor, StringComparison.CurrentCultureIgnoreCase) ||
            filtro.Clausula.Equals(nameof(FacturaRecDto.IdProveedor), StringComparison.CurrentCultureIgnoreCase) ||
            filtro.Clausula.Equals(nameof(FacturaRecDto.Proveedor), StringComparison.CurrentCultureIgnoreCase))
            {
                var contratosDeUnProveedor = contexto.Set<DatosDelContratoDtm>().Where(x => x.IdProveedor == filtro.Valor.Entero());
                consulta = consulta.Where(y => contratosDeUnProveedor.Any(x => x.IdElemento == y.Id));
                filtro.Aplicado = true;
            }
            else if (filtro.Clausula.Equals(ltrDatosDelContrato.FiltroPorConOSinProveedor, StringComparison.CurrentCultureIgnoreCase))
            {
                filtro.Aplicado = true;
                if (filtro.Valor == ltrDatosDelContrato.ConProveedor)
                {
                    var contratosDeUnProveedor = contexto.Set<DatosDelContratoDtm>().Where(x => x.IdProveedor != null);
                    consulta = consulta.Where(y => contratosDeUnProveedor.Any(x => x.IdElemento == y.Id));
                }
                if (filtro.Valor == ltrDatosDelContrato.SinProveedor)
                {
                    var contratosDeUnProveedor = contexto.Set<DatosDelContratoDtm>().Where(x => x.IdProveedor == null);
                    consulta = consulta.Where(y => contratosDeUnProveedor.Any(x => x.IdElemento == y.Id));
                }
            }
            return consulta;
        }

        public static IQueryable<ContratoDtm> AplicarFiltroDeCliente(this IQueryable<ContratoDtm> consulta, ContextoSe contexto, ClausulaDeFiltrado filtro)
        {
            if (filtro.Clausula.Equals(ltrDatosDelContrato.FiltroPorCliente, StringComparison.CurrentCultureIgnoreCase) ||
                filtro.Clausula.Equals(nameof(CambiarDatosFae.Cliente), StringComparison.CurrentCultureIgnoreCase))
            {
                var contratosDeUnCliente = contexto.Set<DatosDelContratoDtm>().Where(x => x.IdCliente == filtro.Valor.Entero());
                consulta = consulta.Where(y => contratosDeUnCliente.Any(x => x.IdElemento == y.Id));
                filtro.Aplicado = true;
            }
            else if (filtro.Clausula.Equals(ltrDatosDelContrato.FiltroPorConOSinCliente, StringComparison.CurrentCultureIgnoreCase))
            {
                filtro.Aplicado = true;
                if (filtro.Valor == ltrDatosDelContrato.ConCliente)
                {
                    var contratosDeUnCliente = contexto.Set<DatosDelContratoDtm>().Where(x => x.IdCliente != null);
                    consulta = consulta.Where(y => contratosDeUnCliente.Any(x => x.IdElemento == y.Id));
                }
                if (filtro.Valor == ltrDatosDelContrato.SinCliente)
                {
                    var contratosDeUnCliente = contexto.Set<DatosDelContratoDtm>().Where(x => x.IdCliente == null);
                    consulta = consulta.Where(y => contratosDeUnCliente.Any(x => x.IdElemento == y.Id));
                }
            }

            return consulta;
        }

        public static IQueryable<ContratoDtm> ContratosConExpediente(this IQueryable<ContratoDtm> consulta, ClausulaDeFiltrado filtro, ref bool esNecesarioIndicarLaClase)
        {
            if (filtro.Clausula.Equals(ltrDeUnContrato.FiltroPorConOSinExpediente, StringComparison.CurrentCultureIgnoreCase))
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => x.IdExpediente != null);
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => x.IdExpediente == null);
                filtro.Aplicado = true;
            }
            if (filtro.Clausula.Equals(ltrDeUnContrato.IdExpediente, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.IdExpediente == filtro.Valor.Entero());
                esNecesarioIndicarLaClase = false;
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<ContratoDtm> AplicarFiltroPorPlfVenta(this IQueryable<ContratoDtm> consulta, ContextoSe contexto, ClausulaDeFiltrado filtro, ref bool esNecesarioIndicarLaClase)
        {
            if (filtro.Clausula.Equals(ltrDeUnContrato.FiltroPorConOSinPlfDeVenta, StringComparison.CurrentCultureIgnoreCase))
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => contexto.Set<PlanificacionDeVentaDtm>().Any(y => y.IdContrato == x.Id));
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => !contexto.Set<PlanificacionDeVentaDtm>().Any(y => y.IdContrato == x.Id));
                filtro.Aplicado = true;
            }
            if (filtro.Clausula.Equals(ltrDeUnContrato.IdPlfDeVenta, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.Id == contexto.Set<PlanificacionDeVentaDtm>().First(y => y.Id == filtro.Valor.Entero()).IdContrato);
                esNecesarioIndicarLaClase = false;
                filtro.Aplicado = true;
            }
            if (filtro.Clausula.Equals(ltrDeUnContrato.SelectorParaUnaPlvDeVenta, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.AplicarFiltroDeCadena(new ClausulaDeFiltrado(nameof(ExpedienteDtm.Expresion), enumCriteriosDeFiltrado.porReferencia, filtro.Valor));
                consulta = consulta.Where(x => x.ClaseDeContrato.Equals(enumClaseDeContrato.Venta));
                esNecesarioIndicarLaClase = false;
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<ContratoDtm> AplicarFiltroPorFacturaEmt(this IQueryable<ContratoDtm> consulta, ContextoSe Contexto, ClausulaDeFiltrado filtro, ref bool esNecesarioIndicarLaClase)
        {
            if (filtro.Clausula.Equals(ltrDeUnContrato.FiltroPorConOSinFacturaEmt, StringComparison.CurrentCultureIgnoreCase))
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => Contexto.Set<FacturaEmtDtm>().Any(y => y.IdContrato == x.Id));
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => !Contexto.Set<FacturaEmtDtm>().Any(y => y.IdContrato == x.Id));
                filtro.Aplicado = true;
            }
            if (filtro.Clausula.Equals(ltrDeUnContrato.IdFacturaEmt, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(ctr => Contexto.Set<FacturaEmtDtm>().Any(fae => ctr.Id == fae.IdContrato && fae.Id == filtro.Valor.Entero()));
                esNecesarioIndicarLaClase = false;
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<ContratoDtm> AplicarFiltroPorParteTr(this IQueryable<ContratoDtm> consulta, ContextoSe contexto, ClausulaDeFiltrado filtro, ref bool esNecesarioIndicarLaClase)
        {
            if (filtro.Clausula.Equals(ltrDeUnContrato.FiltroPorConOSinParteTr, StringComparison.CurrentCultureIgnoreCase))
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => contexto.Set<ParteTrDtm>().Any(y => y.IdContrato == x.Id));
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => !contexto.Set<ParteTrDtm>().Any(y => y.IdContrato == x.Id));
                filtro.Aplicado = true;
            }
            if (filtro.Clausula.Equals(ltrDeUnContrato.IdParteTr, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.Id == contexto.Set<ParteTrDtm>().First(y => y.Id == filtro.Valor.Entero()).IdContrato);
                esNecesarioIndicarLaClase = false;
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<ContratoDtm> AplicarFiltroPorUnitario(this IQueryable<ContratoDtm> consulta, ContextoSe contexto, ClausulaDeFiltrado filtro)
        {
            if (filtro.Clausula.Equals(ltrDeUnContrato.IdUnitario, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => contexto.Set<LoteDeUnContratoDtm>().Any(y => y.IdContrato == x.Id 
                  && contexto.Set<UnitariosDeUnLoteDtm>().Any(z => z.IdLote == y.Id && z.IdUnitario == filtro.Valor.Entero())));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<ContratoDtm> AplicarFiltroPorClaseDeContrato(this IQueryable<ContratoDtm> consulta,  ClausulaDeFiltrado filtro, ref bool hayFiltroPorClase)
        {
            if (filtro.Clausula.Equals(nameof(ContratoDtm.ClaseDeContrato), StringComparison.InvariantCultureIgnoreCase))
            {
                hayFiltroPorClase = true;
                filtro.Aplicado = true;
                consulta = consulta.Where(x => x.ClaseDeContrato.Equals(ApiDeEnsamblados.ToEnumerado<enumClaseDeContrato>(filtro.Valor)));
            }
            return consulta;
        }

        public static IQueryable<ContratoDtm> AplicarFiltroPorEtapas(this IQueryable<ContratoDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnContrato.FiltroPorEtapas.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var etapas = filtro.Valor.Split(Simbolos.separadorDeEtapas);
                var estados = ApiDeEnsamblados.ToEnumerado<enumEtapasDeContratos>(etapas[0]).Estados();
                for (var i = 1; i < etapas.Length; i++) estados = estados + Simbolos.Coma + ApiDeEnsamblados.ToEnumerado<enumEtapasDeContratos>(etapas[i]).Estados();
                var filtroPorEstados = new ClausulaDeFiltrado(nameof(IUsaEstado.IdEstado), enumCriteriosDeFiltrado.esAlgunoDe, estados);
                consulta = consulta.AplicarFiltroPorEntero(filtroPorEstados);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<ContratoDtm> AplicarFiltroPorEtapaDePlanificaciones(this IQueryable<ContratoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnContrato.FiltroPorEtapaDePlfsDeVenta.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var filtroPorEstados = new ClausulaDeFiltrado(nameof(IUsaEstado.IdEstado), enumCriteriosDeFiltrado.esAlgunoDe, ApiDeEnsamblados.ToEnumerado<enumEtapasDePlfsDeVenta>(filtro.Valor).Estados());
                var planificaciones = contexto.Set<PlanificacionDeVentaDtm>().AplicarFiltroPorEntero(filtroPorEstados);
                consulta = consulta.Where(x => planificaciones.Any(plv => plv.IdContrato == x.Id));
                filtro.Aplicado = true;
                return consulta;
            }
            return consulta;
        }

        public static IQueryable<ContratoDtm> AplicarFiltroPorEtapaDePartesTr(this IQueryable<ContratoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnContrato.FiltroPorEtapaDePartesTr.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var filtroPorEstados = new ClausulaDeFiltrado(nameof(IUsaEstado.IdEstado), enumCriteriosDeFiltrado.esAlgunoDe, ApiDeEnsamblados.ToEnumerado<enumEtapasDePartesTr>(filtro.Valor).Estados());
                var partes = contexto.Set<ParteTrDtm>().AplicarFiltroPorEntero(filtroPorEstados);
                consulta = consulta.Where(x => partes.Any(ptr => ptr.IdContrato == x.Id));
                filtro.Aplicado = true;
                return consulta;
            }
            return consulta;
        }

    }
}
