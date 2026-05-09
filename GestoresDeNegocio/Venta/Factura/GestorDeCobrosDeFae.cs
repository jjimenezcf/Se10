using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Ventas
{
    public class GestorDeCobrosDeFae : GestorDeElementos<ContextoSe, CobroDeFaeDtm, CobroDeFaeDto>, IGeneradorDePreasiento
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class MapearCobrosDeFae : Profile
        {
            public MapearCobrosDeFae()
            {
                CreateMap<CobroDeFaeDtm, CobroDeFaeDto>()
                .ForMember(dto => dto.Elemento, x => x.MapFrom(dtm => dtm.Elemento != null ? dtm.Elemento.Expresion : null))
                .ForMember(dto => dto.CuentaDeIngreso, x => x.MapFrom(dtm => dtm.CuentaDeIngreso != null ? $"({dtm.CuentaDeIngreso.Alias}) {dtm.CuentaDeIngreso.Cuenta.NumeroIban}" : null))
                .ForMember(dto => dto.CuentaDeCargo, x => x.MapFrom(dtm => dtm.CuentaDeCargo != null ? $"({dtm.CuentaDeCargo.Alias}) {dtm.CuentaDeCargo.Cuenta.NumeroIban}" : null));
                CreateMap<CobroDeFaeDto, CobroDeFaeDtm>()
                .ForMember(dtm => dtm.Elemento, dto => dto.Ignore())
                .ForMember(dtm => dtm.CuentaDeCargo, dto => dto.Ignore())
                .ForMember(dtm => dtm.CuentaDeIngreso, dto => dto.Ignore());
            }
        }

        public GestorDeCobrosDeFae(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<CobroDeFaeDtm> AplicarJoins(IQueryable<CobroDeFaeDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Elemento);
            consulta = consulta.Include(x => x.CuentaDeIngreso).ThenInclude(y => y.Cuenta);
            return consulta;
        }

        public static GestorDeCobrosDeFae Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCobrosDeFae(contexto, mapeador);
        }

        protected override void DespuesDeMapearElRegistro(CobroDeFaeDto elemento, CobroDeFaeDtm cobro, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(elemento, cobro, opciones);
            var fae = cobro.DetalleDe<FacturaEmtDtm>(Contexto);
            cobro.CalcularReferencia(Contexto);
        }

        protected override void DespuesDeLeer(List<CobroDeFaeDtm> registros, ParametrosDeNegocio parametros)
        {
            base.DespuesDeLeer(registros, parametros);
            foreach (var cobro in registros)
            {
                cobro.CalcularReferencia(Contexto);
            }
        }

        public override CobroDeFaeDtm PersistirRegistro(CobroDeFaeDtm cobro, ParametrosDeNegocio parametros)
        {
            var factura = cobro.Factura(Contexto);

            var idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(Contexto, enumNegocio.FacturaEmitida.IdNegocio(), cobro.IdElemento, enumOpercionesDeSemaforo.COB, factura.Referencia).Id;
            try
            {
                parametros.ValidarPermisosDePersistencia = false;
                if (!factura.EsGestor<TipoDeFacturaEmtDtm>(Contexto))
                    GestorDeErrores.Emitir($"Para {parametros.Operacion.Descripcion().ToLower()} el cobro de la factura '{factura.Referencia}' ha de tener permisos de gestión sobre ella");
                cobro = base.PersistirRegistro(cobro, parametros);
            }
            finally
            {
                SemaforoDeProcesoSql.QuitarSemaforo(Contexto, idSemaforo);
            }
            return cobro;
        }


        protected override void AntesDePersistir(CobroDeFaeDtm cobro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(cobro, parametros);

            var factura = cobro.Factura(Contexto);
            if (factura.EsRectificativa)
            {
                GestorDeErrores.Emitir($"la factura '{factura.Referencia}' es rectificativa '{factura.ClaseRectificativa.Descripcion()}', motivo '{factura.MotivoDeRectificacion.Descripcion()}'. No se puede cobrar");
            }

            if (parametros.Insertando)
            {
                if (cobro.Cobrado <= 0)
                    GestorDeErrores.Emitir($"No se puede crear un cobro menor o igual a cero");

                var rectificativa = factura.RectificadaPor(Contexto, errorSiNoHay: false);
                var valorRectificado = rectificativa is not null ? rectificativa.APagar(Contexto) : 0;
                var valorAbonado = (decimal)0;
                var cobroMaximo = factura.APagar(Contexto) + valorRectificado - factura.Cobrado(Contexto) + (-1 * valorAbonado);
                if (rectificativa is not null)
                {
                    if (cobroMaximo - cobro.Cobrado < 0)
                        GestorDeErrores.Emitir($"No se puede cobrar la factura '{factura.Referencia}' por  valor de '{cobro.Cobrado.ToMoneda()}' ya que está rectificada por '{valorRectificado.ToMoneda()}' y se a abonado '{valorAbonado.ToMoneda()}', sólo se puede cobrar por '{cobroMaximo.ToMoneda()}'");
                }

                var valorPendiente = cobroMaximo - cobro.Cobrado;
                if (valorPendiente < 0 && Math.Abs(valorPendiente) > VariableDeFacturasEmt.ToleranciaDeCobro)
                    GestorDeErrores.Emitir($"No se puede crear el cobro de '{cobro.Cobrado}' sobre el total de la factura '{factura.APagar(Contexto).ToMoneda()}' ya que el cobro máximo es de '{cobroMaximo.ToMoneda()}' y hay una tolerancia de '{VariableDeFacturasEmt.ToleranciaDeCobro}'");
            }

            if (parametros.Operacion == enumTipoOperacion.Modificar && parametros.AccionQueSeEjecuta != nameof(GestorDeFacturasEmt.GenerarPreasiento))
                GestorDeErrores.Emitir($"No se puede modificar un cobro, suprímalo y cree uno nuevo");

            if ((parametros.Insertando && factura.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasEmt> { enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura,
                enumEtapasDeFacturasEmt.FAE_Etapa_No_Cobrable,
                enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada,
                enumEtapasDeFacturasEmt.FAE_Etapa_Anulada })) ||
                (parametros.Eliminando && factura.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasEmt> { enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura,
                enumEtapasDeFacturasEmt.FAE_Etapa_No_Cobrable,
                enumEtapasDeFacturasEmt.FAE_Etapa_Anulada })))
                GestorDeErrores.Emitir($"$No se puede persistir el cobro por estar la factura '{factura.Referencia}' en la etapa '{factura.CadenaDeEtapas()}' y en esta etapa no es cobrable");

            if (parametros.EsUnaPeticion && cobro.Clase == enumClaseDeCobro.Remesa)
                GestorDeErrores.Emitir($"No se pueden crear o borrar cobros de remesas, estos cobros se hacen al cargar o anular cargos de remesas, o al devolver o anular devoluciones de facturas remesadas");

            if (parametros.Insertando)
            {
                if (cobro.CobradoEl.Date < ((DateTime)factura.FacturadaEl).Date)
                    GestorDeErrores.Emitir($"No se puede crear un cobro ya que la fecha es anterior a la emisión de la factura");

                if (cobro.CobradoEl.Date >= DateTime.Now)
                    GestorDeErrores.Emitir($"No se puede crear un cobro con fecha futura");

                if (cobro.CobradoEl.Date == ((DateTime)factura.FacturadaEl).Date && cobro.CobradoEl.TimeOfDay < ((DateTime)factura.FacturadaEl).TimeOfDay)
                    cobro.CobradoEl = (DateTime)factura.FacturadaEl;

                if (cobro.Clase == enumClaseDeCobro.Transferencia || cobro.Clase == enumClaseDeCobro.CartaDePago || cobro.Clase == enumClaseDeCobro.Remesa)
                {
                    if (cobro.IdCuentaDeIngreso is null)
                        GestorDeErrores.Emitir($"El cobro de la factura '{factura.Referencia}' debe indicar la cuenta de ingreso");

                    var sociedad = factura.Sociedad(Contexto);
                    var cbDeMiSociedad = sociedad.Detalles<CuentaDeMiSociedadDtm>(Contexto).FirstOrDefault(x => x.Id == cobro.IdCuentaDeIngreso);
                    if (cbDeMiSociedad is null)
                        GestorDeErrores.Emitir($"La cuenta de ingreso del cobro de la factura '{factura.Referencia}' debe ser de la sociedad que factura");
                    if (!cbDeMiSociedad.Activa)
                        GestorDeErrores.Emitir($"La cuenta de ingreso '{cobro.CuentaDeIngreso(Contexto).NumeroIban}' del cobro de la factura '{factura.Referencia}' debe estar activa");
                }

                if (cobro.Clase == enumClaseDeCobro.Remesa)
                {
                    if (cobro.IdCuentaDeCargo is null)
                        GestorDeErrores.Emitir($"El cobro de la factura '{((FacturaEmtDtm)cobro.Elemento(Contexto)).Referencia}' debe indicar la cuenta de cargo");

                    if (cobro.IdFacturaRemesada is null)
                        GestorDeErrores.Emitir($"El cobro de la factura '{((FacturaEmtDtm)cobro.Elemento(Contexto)).Referencia}' debe indicar la remesa en la que se incluye");
                    var facturaRemesada = Contexto.SeleccionarPorId<FacturaEmtDeUnaRemesaDtm>(cobro.IdFacturaRemesada.Entero());
                    if (facturaRemesada.IdFactura != factura.Id)
                        GestorDeErrores.Emitir($"La factura '{factura.Referencia}' no está incluida en la remesa '{((RemesaFaeDtm)facturaRemesada.Elemento(Contexto)).Referencia}', por lo que no se le puede crear un cobro");

                }

                if (cobro.Clase == enumClaseDeCobro.Transferencia || cobro.Clase == enumClaseDeCobro.Contado)
                    cobro.Preasentar(Contexto);
            }

            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {
                if (factura.Cobrado(Contexto) - cobro.Cobrado < 0)
                    GestorDeErrores.Emitir($"No se puede anular el cobro de {cobro.Cobrado.Moneda()} ya que el importe cobrado es {factura.Cobrado(Contexto).Moneda()}");

                if (cobro.Clase == enumClaseDeCobro.Remesa && parametros.EsUnaPeticion)
                    GestorDeErrores.Emitir($"No se puede anular un cobro remesado, anule en la remesa o retroceda toda la remesa");

                ((CobroDeFaeDtm)parametros.registroEnBd).CancelarPreasiento(Contexto);
            }
        }

        protected override void DespuesDePersistir(CobroDeFaeDtm cobro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(cobro, parametros);

            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                cobro.TrasRealizarUnCobro(Contexto, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
                if (cobro.Clase == enumClaseDeCobro.Transferencia || cobro.Clase == enumClaseDeCobro.Contado)
                {
                    cobro.Preasiento(Contexto).IdReferenciado = cobro.Id;
                    cobro.Preasiento.ModificarComoAdministrador(Contexto);
                }
            }
            else
            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {
                var paramDeTransicion = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
                if (parametros.Parametros.ContainsKey(nameof(VariableDeFacturasEmt.enumMotivoTransicion)))
                    paramDeTransicion.Add(nameof(VariableDeFacturasEmt.enumMotivoTransicion), parametros.Parametros[nameof(VariableDeFacturasEmt.enumMotivoTransicion)]);
                cobro.TrasEliminarUnCobro(Contexto, paramDeTransicion);
                cobro.Factura(Contexto).CrearTraza(Contexto,
                $"Eliminacion de cobro",
                $"El usuario {Contexto.DatosDeConexion.Login} ha eliminado el cobro del día {cobro.CobradoEl} de clase '{cobro.Clase.Descripcion()}' por valor de {cobro.Cobrado.Moneda()}");
            }
        }

        protected override void EliminarCaches(CobroDeFaeDtm cobro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(cobro, parametros);
            var factura = cobro.Factura(Contexto);
            ServicioDeCaches.EliminarElemento(CacheDe.Fae_Cobrado, $"{factura.Id}");
            VaciarCacheDeDetalle(typeof(CobroDeFaeDtm), factura.Id);
            var expediente = factura.Presupuesto(Contexto)?.Expediente(Contexto) ?? null;
            if (expediente is not null) ServicioDeCaches.EliminarElemento(CacheDe.Exp_Cobros, expediente.Id.ToString());
        }

        protected override void DespuesDeMapearElElemento(CobroDeFaeDtm cobro, CobroDeFaeDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(cobro, elemento, parametros);
            var ctaCargo = cobro.CuentaDeCargo(Contexto, errorSiNoHay: false);
            if (ctaCargo.cb != null) elemento.CuentaDeCargo = $"({ctaCargo.alias}) {ctaCargo.cb.NumeroIban}";
        }

        public void GenerarPreasiento(List<int> ids)
        {

            var trans = Contexto.IniciarTransaccion();
            try
            {
                foreach (int id in ids)
                {
                    var cobro = Contexto.SeleccionarPorId<CobroDeFaeDtm>(id);
                    var sociedad = cobro.Sociedad(Contexto);

                    if (!sociedad.UsaPreasientos(Contexto, enumNegocio.Cobro))
                        GestorDeErrores.Emitir($"La sociedad del cobro '{cobro.Referencia(Contexto)}' no usa preasientos, configúrela en '{enumParametrosDePreasiento.SPR_Generar_Preasiento_De_Cobro}'");

                    if (!cobro.Factura(Contexto).EsAdministrador(Contexto))
                        GestorDeErrores.Emitir($"Ha de ser administrador del cobro '{cobro.Referencia(Contexto)}' para poder preasentarlo");

                    if (cobro.Factura(Contexto).EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasEmt> { enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura, enumEtapasDeFacturasEmt.FAE_Etapa_Anulada }))
                        GestorDeErrores.Emitir($"No se puede generar un preasiento de '{cobro.Referencia(Contexto)}' por no estar en la etapa correcta");

                    var preasientoAnterior = cobro.Preasiento(Contexto, errorSiNoHay: false);
                    var cancelado = preasientoAnterior?.Estado(Contexto).Cancelado ?? false;
                    if (cobro.IdPreasiento is not null && !cancelado)
                    {
                        cobro.CancelarPreasiento(Contexto);
                    }
                    cobro.Preasentar(Contexto);
                    cobro.ModificarComoAdministrador(Contexto, accionQueSeEjecuta: nameof(GenerarPreasiento));
                }
                Contexto.Commit(trans);
            }
            catch
            {
                Contexto.Rollback(trans);
                throw;
            }
        }
    }
}
