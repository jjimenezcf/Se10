using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using System.Linq;
using System.Collections.Generic;
using ServicioDeDatos.Gastos;
using ModeloDeDto.Gastos;
using Microsoft.EntityFrameworkCore;
using GestorDeElementos.Extensores;
using Gestor.Errores;
using System;
using Utilidades;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Ventas;

namespace GestoresDeNegocio.Gastos
{

    public class GestorDePagosDeUnaRemesa : GestorDeRelaciones<ContextoSe, PagoDeUnaRemesaDtm, PagoDeUnaRemesaDto>
    {
        public class MapearPagoDeUnaRemesa : Profile
        {
            public MapearPagoDeUnaRemesa()
            {
                CreateMap<PagoDeUnaRemesaDtm, PagoDeUnaRemesaDto>()
                .ForMember(dto => dto.Elemento, dtm => dtm.MapFrom(dtm => dtm.Elemento.Expresion))
                .ForMember(dto => dto.Pago, dtm => dtm.MapFrom(dtm => dtm.Pago.Expresion));

                CreateMap<PagoDeUnaRemesaDto, PagoDeUnaRemesaDtm>()
                .ForMember(dtm => dtm.Pago, dto => dto.Ignore())
                .ForMember(dtm => dtm.PagadoEl, dto => dto.Ignore())
                .ForMember(dtm => dtm.Elemento, dto => dto.Ignore());
            }
        }

        public GestorDePagosDeUnaRemesa(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDePagosDeUnaRemesa Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePagosDeUnaRemesa(contexto, mapeador);
        }

        protected override IQueryable<PagoDeUnaRemesaDtm> AplicarJoins(IQueryable<PagoDeUnaRemesaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(rp => rp.Elemento);
            consulta = consulta.Include(rp => rp.Pago);
            return consulta;
        }

        protected override IQueryable<PagoDeUnaRemesaDtm> AplicarFiltros(IQueryable<PagoDeUnaRemesaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {

            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            return consulta;
        }

        protected override void AntesDePersistir(PagoDeUnaRemesaDtm pr, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(pr, parametros);
            var pago = pr.Pago(Contexto);
            var remesa = pr.DetalleDe<RemesaPagDtm>(Contexto);

            if (pago.Sociedad(Contexto).Id != remesa.Sociedad(Contexto).Id)
                GestorDeErrores.Emitir($"la remesa '{remesa.Referencia}' y el pago '{pago.Referencia}' han de ser de la misma sociedad");

            if (pago.Clase != enumClaseDePago.Remesa)
                GestorDeErrores.Emitir($"no se puede incluir el pago '{pago.Referencia}' en la remesa '{remesa.Referencia}' por no ser remesable");

            if (parametros.Insertando)
            {

                if (!remesa.EstaEnLaEtapa(enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion) && !parametros.EstaEjecutandoUnaAccion)
                    GestorDeErrores.Emitir($"La remesa '{remesa.Referencia}' no acepta añadir el pago '{pago.Referencia}' por estar en la etapa '{remesa.Etapa().Nombre(true)}', debe estar en la etapa de '{enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion.Nombre(true)}'");

                if (!pago.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Pendiente))
                    GestorDeErrores.Emitir($"El pago '{pago.Referencia}' no se puede incluir o excluir en la remesa '{remesa.Referencia}' por estar en la etapa '{pago.Etapa().Nombre(true)}', debe estar en la etapa de '{enumEtapasDePagos.PAG_Etapa_Pendiente.Nombre(true)}'");

                var otraRemesa = Contexto.SeleccionarPorFk<PagoDeUnaRemesaDtm>(nameof(PagoDeUnaRemesaDtm.IdPago), pago.Id, errorSiNoHay: false);
                if (otraRemesa != null)
                    GestorDeErrores.Emitir($"El pago '{pago.Referencia}' ya está incluido en otra remesa '{((RemesaPagDtm)otraRemesa.Elemento(Contexto)).Referencia}'");

                if (pago.PagadoEl is not null)
                    GestorDeErrores.Emitir($"El pago '{pago.Referencia}' no se puede incluir en la remesa '{remesa.Referencia}' por estar ya pagado '{((DateTime)pago.PagadoEl).ToShortDateString()}'");
            }

            if (parametros.Eliminando)
            {
                if (!remesa.EstaEnLaEtapa(enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion) && !parametros.EstaEjecutandoUnaAccion)
                    GestorDeErrores.Emitir($"La remesa '{remesa.Referencia}' no acepta excluir el pago '{pago.Referencia}' por estar en la etapa '{remesa.Etapa().Nombre(true)}', debe estar en la etapa de '{enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion.Nombre(true)}'");

                if (!pago.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Remesado))
                    GestorDeErrores.Emitir($"El pago '{pago.Referencia}' no se puede excluir en la remesa '{remesa.Referencia}' por estar en la etapa '{pago.Etapa().Nombre(true)}', debe estar en la etapa de '{enumEtapasDePagos.PAG_Etapa_Remesado.Nombre(true)}'");
             }

            if (parametros.Modificando)
            {
                if (!remesa.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeRemesasPag> { enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion, enumEtapasDeRemesasPag.REM_Etapa_De_Cierre })) 
                    GestorDeErrores.Emitir($"No se puede modificar el pago '{pago.Referencia}' en la remesa '{remesa.Referencia}' por no está en la etapa de '{enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion.Nombre(true)}'");

                #region Valido que si se ha modificado la fecha de anulación sea correcta
                if (pr.PropiedadCambiada<DateTime?>(nameof(PagoDeUnaRemesaDtm.AnuladoEl), parametros))
                {
                    if (pr.AnuladoEl.HasValue && ((PagoDeUnaRemesaDtm)parametros.registroEnBd).AnuladoEl.HasValue)
                        GestorDeErrores.Emitir($"El '{pago.Referencia}' de la remesa'{remesa.Referencia}' ya tiene una fecha de anulación, no puede modificarse, anule la anulación y vuelva a excluirla");

                    if (!pr.PropiedadCambiada<string>(nameof(PagoDeUnaRemesaDtm.Motivo), parametros))
                        GestorDeErrores.Emitir($"Debe indicar un motivo diferente al último indicado");

                    if (pr.AnuladoEl is not null)
                    {
                        if (!pr.PagadoEl.HasValue && !remesa.EsInterventor<TipoDeRemesaPagDtm>(Contexto))
                            GestorDeErrores.Emitir($"No se puede anular el pago '{pago.Referencia}' de la remesa  '{remesa.Referencia}' ya que aun no se ha pagado, para hacer esto necesita permisos de intervención");

                        if (remesa.PresentadaEl.Fecha().Date > pr.AnuladoEl)
                            GestorDeErrores.Emitir($"No se puede anular el pago '{pago.Referencia}' con fecha anterior a la presentación de la remesa'{remesa.Referencia}'");

                        if (remesa.PagadaEl is null)
                            GestorDeErrores.Emitir($"No se puede anular el pago '{pago.Referencia}' porque la remesa '{remesa.Referencia}' aun no ha sido pagada, anúlela, retire el pago y vuelva a presentarla o de la por pagada y lo anula");

                        if (pr.Motivo.IsNullOrEmpty())
                            GestorDeErrores.Emitir($"En la remesa '{remesa.Referencia}' se ha indicado que el pago '{pago.Referencia}' se quiere devolver, es obligatorio el motivo");

                        parametros.Parametros[nameof(VariableDePagos.TransicionesPorMotivo)] = VariableDePagos.enumMotivoTransicion.AnularPago;
                        pr.PagadoEl = null;
                    }
                    else
                    {
                        if (pr.Motivo.IsNullOrEmpty())
                            GestorDeErrores.Emitir($"Si quiere anular la anulación de el pago '{pago.Referencia}' de la remesa '{remesa.Referencia}' ha de indicar el motivo");

                        parametros.Parametros[nameof(VariableDePagos.TransicionesPorMotivo)] = VariableDePagos.enumMotivoTransicion.AnularAnulacion;
                        pr.PagadoEl = remesa.PagadaEl;
                    }
                }
                #endregion

                #region Valido que si se ha modificado la fecha de pago, sea correcta
                else if (pr.PropiedadCambiada<DateTime?>(nameof(PagoDeUnaRemesaDtm.PagadoEl), parametros))
                {
                    if (pr.PagadoEl.HasValue && ((PagoDeUnaRemesaDtm)parametros.registroEnBd).PagadoEl.HasValue)
                        GestorDeErrores.Emitir($"No se puede modificar la fecha '{((PagoDeUnaRemesaDtm)parametros.registroEnBd).PagadoEl.Fecha().ToShortDateString()}' de pago de el pago '{pago.Referencia}' remesada en '{remesa.Referencia}', anúlelo o devuélvalo");

                    if (!pr.PagadoEl.HasValue && pr.AnuladoEl.HasValue)
                        GestorDeErrores.Emitir($"No se puede anular el pago de el pago '{pago.Referencia}' remesado en '{remesa.Referencia}' ya que se devolvió el día '{pr.AnuladoEl.Fecha().ToShortDateString()}'");

                    if (pr.PagadoEl.HasValue && pr.AnuladoEl.HasValue)
                        GestorDeErrores.Emitir($"No se puede realizar el pago de el pago '{pago.Referencia}' remesado en '{remesa.Referencia}' ya que se devolvió el día '{pr.AnuladoEl.Fecha().ToShortDateString()}', anule la anulación y cárguelo");

                }
                else
                    GestorDeErrores.Emitir($"De el pago '{pago.Referencia}' remesada en '{remesa.Referencia}', solo se puede modificar la fecha de anulación o pago");

                #endregion
            }
        }

        protected override void DespuesDePersistir(PagoDeUnaRemesaDtm pr, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(pr, parametros);
            var pago = pr.Pago(Contexto);
            var remesa = pr.DetalleDe<RemesaPagDtm>(Contexto);

            if (parametros.Insertando)
            {
                pago.IdCuentaDePago = remesa.IdCuentaDePago;
                pago.PagarEl = remesa.PagarEl;
                pago.PagadoEl = null;
                pago.TransitarALaEtapa(Contexto, enumEtapasDePagos.PAG_Etapa_Remesado.EstadosDeLaEtapa(), new Dictionary<string, object>());
            }
            else if (parametros.Eliminando)
            {
                pago.PagarEl = null;
                pago.PagadoEl = null;
                pago.TransitarALaEtapa(Contexto, enumEtapasDePagos.PAG_Etapa_Pendiente.EstadosDeLaEtapa(), new Dictionary<string, object>());
            }
            else if(parametros.Modificando)
            {
                if (pr.AnuladoEl is not null)
                {
                    pago.PagadoEl = null;
                    pago.TransitarPorMotivo(Contexto, VariableDePagos.TransicionesPorMotivo, (Enum)parametros.Parametros[nameof(VariableDePagos.TransicionesPorMotivo)]);
                }
                else
                {
                    pago.PagadoEl = remesa.PagadaEl;
                    pago.TransitarPorMotivo(Contexto, VariableDePagos.TransicionesPorMotivo, (Enum)parametros.Parametros[nameof(VariableDePagos.TransicionesPorMotivo)]);
                }
            }

            ServicioDeCaches.EliminarElemento(CacheDe.Rem_Pag_Total, remesa.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Rem_Pag_Pagado, remesa.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Rem_Pag_Hay_Pagos, remesa.Id.ToString());
        }

        protected override void DespuesDeMapearElElemento(PagoDeUnaRemesaDtm pr, PagoDeUnaRemesaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(pr, elemento, parametros);
            var pago = pr.Pago(Contexto);
            var remesa = (RemesaPagDtm)pr.Elemento(Contexto);
            elemento.Acreedor = pago.Solicitante(Contexto).Expresion;
            elemento.ImportePago = pago.Importe;
            elemento.PagarEl = pago.PagarEl;
            elemento.EstaPagado = pr.PagadoEl.HasValue;
            elemento.EstaAnulado = pr.AnuladoEl.HasValue;
            if (parametros.Peticion == enumPeticion.epLeerPorId)
            {
                elemento.PagadoEl = elemento.EstaPagado ? pago.PagadoEl : pago.PagarEl;
                elemento.Etapas = remesa.ListaDeEtapas();
            }

            if (!remesa.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeRemesasPag> { enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion }))
            {
                elemento.EsInterventor = remesa.EsInterventor(Contexto);
                elemento.ModoDeAcceso = elemento.ModoDeAcceso.SoyInterventor() && remesa.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeRemesasPag> { enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion })
                ? elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Interventor
                : enumModoDeAccesoDeDatos.Consultor;
            }
        }

    }

}
