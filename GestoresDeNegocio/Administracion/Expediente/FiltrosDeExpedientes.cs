using GestorDeElementos;
using ModeloDeDto.Expediente;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.SistemaDocumental;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using ServicioDeDatos.Negocio;

namespace GestoresDeNegocio.Expediente
{
    internal static class FiltrosDeExpedientes
    {
        public static IQueryable<ExpedienteDtm> SeleccionarParaAsociarAUnPpt(this IQueryable<ExpedienteDtm> consulta, ClausulaDeFiltrado filtro)
        {
            if (filtro.Clausula.Equals(ltrDeUnPresupuesto.AsociarExpediente, StringComparison.CurrentCultureIgnoreCase))
            {
                var estados = enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos.Lista();
                consulta = consulta.Where(x => estados.Contains(x.IdEstado));
                consulta = consulta.AplicarFiltroDeCadena(new ClausulaDeFiltrado(nameof(ExpedienteDtm.Expresion), enumCriteriosDeFiltrado.porReferencia, filtro.Valor));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<ExpedienteDtm> SeleccionarParaAsociarAUnContrato(this IQueryable<ExpedienteDtm> consulta, ClausulaDeFiltrado filtro)
        {
            if (filtro.Clausula.Equals(ltrDeUnExpediente.ExpedienteConContrato, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.Tipo.ScDeVenta || x.Tipo.ScDeCompra);
                consulta = consulta.AplicarFiltroDeCadena(new ClausulaDeFiltrado(nameof(ExpedienteDtm.Expresion), enumCriteriosDeFiltrado.porReferencia, filtro.Valor));
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<ExpedienteDtm> ExpedientesConValoracion(this IQueryable<ExpedienteDtm> consulta, ClausulaDeFiltrado filtro)
        {
            if (filtro.Clausula.Equals(ltrDeUnExpediente.ExpedienteConValoracion, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.Tipo.UsaPpts);
                consulta = consulta.AplicarFiltroDeCadena(new ClausulaDeFiltrado(nameof(ExpedienteDtm.Expresion), enumCriteriosDeFiltrado.porReferencia, filtro.Valor));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<ExpedienteDtm> ExpedientesConDatosJuridicos(this IQueryable<ExpedienteDtm> consulta, ClausulaDeFiltrado filtro)
        {
            if (filtro.Clausula.Equals(ltrDeUnExpediente.ExpedienteConDatosJuridicos, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.Tipo.UsaDatosJuridicos);
                consulta = consulta.AplicarFiltroDeCadena(new ClausulaDeFiltrado(nameof(ExpedienteDtm.Expresion), enumCriteriosDeFiltrado.porReferencia, filtro.Valor));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<ExpedienteDtm> ExpedienteConContratos(this IQueryable<ExpedienteDtm> consulta, ContextoSe Contexto, ClausulaDeFiltrado filtro)
        {
            if (filtro.Clausula.Equals(ltrDeUnExpediente.ExpedienteConContrato, StringComparison.CurrentCultureIgnoreCase))
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => Contexto.Set<ContratoDtm>().Any(y => y.IdExpediente == x.Id));
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => !Contexto.Set<ContratoDtm>().Any(y => y.IdExpediente == x.Id));
                filtro.Aplicado = true;
            }

            if (filtro.Clausula.Equals(ltrDeUnExpediente.IdContrato, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => Contexto.Set<ContratoDtm>().Any(y => y.IdExpediente == x.Id && y.Id == filtro.Valor.Entero()));
                filtro.Aplicado = true;
            }

            if (filtro.Clausula.Equals(ltrDeUnExpediente.ExpedientePadre, StringComparison.CurrentCultureIgnoreCase))
            {
                filtro.Criterio = enumCriteriosDeFiltrado.porReferencia;
                consulta = consulta.AplicarFiltroDeCadena(filtro);
            }

            return consulta;
        }

        public static IQueryable<ExpedienteDtm> ExpedientesConPpts(this IQueryable<ExpedienteDtm> consulta, ContextoSe Contexto, ClausulaDeFiltrado filtro)
        {
            if (filtro.Clausula.Equals(ltrDeUnExpediente.ExpedientesConPpts, StringComparison.CurrentCultureIgnoreCase))
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => Contexto.Set<PresupuestoDtm>().Any(y => y.IdExpediente == x.Id));
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => !Contexto.Set<PresupuestoDtm>().Any(y => y.IdExpediente == x.Id));
                filtro.Aplicado = true;
            }
            if (filtro.Clausula.Equals(ltrDeUnExpediente.IdPresupuesto, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => Contexto.Set<PresupuestoDtm>().Any(y => y.IdExpediente == x.Id && y.Id == filtro.Valor.Entero()));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<ExpedienteDtm> FiltrosDeClientes(this IQueryable<ExpedienteDtm> consulta, ContextoSe Contexto, ClausulaDeFiltrado filtro)
        {
            if (filtro.Clausula.Equals(ltrDeUnExpediente.DatosContacto, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.Contacto.Contains(filtro.Valor) || x.Telefono.Contains(filtro.Valor) || x.eMail.Contains(filtro.Valor));
                filtro.Aplicado = true;
            }

            if (filtro.Clausula.Equals(ltrDeUnExpediente.IdCliente, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.IdSolicitante == filtro.Valor.Entero() || Contexto.Set<InterlocutoresDeUnExpedienteDtm>()
                                          .Any(y => y.idElemento2 == filtro.Valor.Entero() && y.idElemento1 == x.Id));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static void FiltrarPorTipoDeExpedienteSegunDescriptor(ContextoSe Contexto, List<ClausulaDeFiltrado> filtros, Dictionary<string, object> parametros)
        {
            var descriptor = parametros.LeerValor(ltrParametrosEp.Descriptor, string.Empty);
            if (descriptor == string.Empty || filtros.Any(f => f.Clausula.Equals(nameof(ExpedienteDto.IdTipo), StringComparison.CurrentCultureIgnoreCase)))
                return;

            var idttipo = VariablesDeExpedientes.IdDelTipoParaActividades(errorSiNoEstaDefinido: false);
            if (descriptor == ltrDescriptores.Administracion.DescriptorDeActividades)
            {
                if (idttipo == 0) enumNegocio.Expediente.IndicarQueFaltaDefinirElParámetro(enumParametrosDeExpedientes.EXP_Tipos_Para_Actividades);
                filtros.Add(new ClausulaDeFiltrado(nameof(ExpedienteDto.IdTipo), enumCriteriosDeFiltrado.igual, idttipo));
            }
            else
            {
                if (idttipo != 0)
                    filtros.Add(new ClausulaDeFiltrado(nameof(ExpedienteDto.IdTipo), enumCriteriosDeFiltrado.diferente, idttipo));
            }

        }

        public static IQueryable<TipoDeExpedienteDtm> FiltroParaSeleccionarTipo(this IQueryable<TipoDeExpedienteDtm> consulta, Dictionary<string, object> parametros)
        {
            var descriptor = parametros.LeerValor(ltrParametrosEp.Descriptor, string.Empty);
            if (descriptor != string.Empty)
            {
                if (descriptor == ltrDescriptores.Administracion.DescriptorDeExpedientes)
                {
                    var idtipo = VariablesDeExpedientes.IdDelTipoParaActividades(errorSiNoEstaDefinido: false);
                    consulta = consulta.Where(x => x.Id != idtipo);
                }
            }
            return consulta;
        }
    }
}
