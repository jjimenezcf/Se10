using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Ventas
{
    public class GestorDeAbonosDeFae : GestorDeElementos<ContextoSe, AbonoDeFaeDtm, AbonoDeFaeDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class MapearAbonosDeFae : Profile
        {
            public MapearAbonosDeFae()
            {
                CreateMap<AbonoDeFaeDtm, AbonoDeFaeDto>()
                .ForMember(dto => dto.IdElemento, x => x.MapFrom(dtm => dtm.idElemento1))
                .ForMember(dto => dto.IdAbono, x => x.MapFrom(dtm => dtm.idElemento2));
                CreateMap<AbonoDeFaeDto, AbonoDeFaeDtm>();
            }
        }

        public GestorDeAbonosDeFae(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<AbonoDeFaeDtm> AplicarJoins(IQueryable<AbonoDeFaeDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Pago);
            return consulta;
        }

        public static GestorDeAbonosDeFae Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeAbonosDeFae(contexto, mapeador);
        }

        protected override IQueryable<AbonoDeFaeDtm> AplicarFiltros(IQueryable<AbonoDeFaeDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            var filtroPorFactura = filtros.FirstOrDefault(f => f.Clausula.ToLower() == nameof(IUsaElemento.IdElemento).ToLower());
            if (filtroPorFactura is not null)
            {
                filtroPorFactura.Clausula = nameof(AbonoDeFaeDtm.idElemento1);
            }
            return consulta;
        }

        public override AbonoDeFaeDtm MapearRegistro(AbonoDeFaeDto elemento, ParametrosDeNegocio opciones)
        {
            var factura = Contexto.SeleccionarPorId<FacturaEmtDtm>(elemento.IdElemento);

            if (!factura.EsGestor<TipoDeFacturaEmtDtm>(Contexto))
                GestorDeErrores.Emitir($"Para {opciones.Operacion.Descripcion().ToLower()} el abono de la factura '{factura.Referencia}' ha de tener permisos de gestión sobre ella");

            var idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(Contexto, enumNegocio.FacturaEmitida.IdNegocio(), factura.Id, enumOpercionesDeSemaforo.ABO, factura.Referencia).Id;
            try
            {
                return base.MapearRegistro(elemento, opciones);
            }
            finally
            {
                SemaforoDeProcesoSql.QuitarSemaforo(Contexto, idSemaforo);
            }
        }

        protected override void DespuesDeMapearElRegistro(AbonoDeFaeDto elemento, AbonoDeFaeDtm abono, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(elemento, abono, opciones);

            if (opciones.Modificando)
                GestorDeErrores.Emitir($"No puede modificarse el abono '{abono.Pago(Contexto).Referencia}'");

            if (opciones.Eliminando)
            {
                if (abono.Pago(Contexto).EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Pendiente))
                    GestorDeErrores.Emitir($"No se puede eliminar el abono '{abono.Pago(Contexto).Referencia}' ya que el pago asociado ha de estar pendiente");

                abono.Pago(Contexto).Cancelar(Contexto);
            }

            if (opciones.Insertando)
            {
                var factura = Contexto.SeleccionarPorId<FacturaEmtDtm>(elemento.IdElemento);
                var solicitante = Contexto.SeleccionarPorId<ClienteDtm>(elemento.IdCliente).Interlocutor(Contexto);
                if (factura.IdCliente != elemento.IdCliente)
                {
                    GestorDeErrores.Emitir($"El cliente de la factura rectificativa '{factura.Referencia}' a de coincidir con el cliente del abono '{solicitante.NIF(Contexto)}'");
                }

                var fechaDeAbono = elemento.AbonadoEl.HasValue ? (DateTime)elemento.AbonadoEl : DateTime.Now.Date;

                abono.Pago = new PagoDtm
                {
                    IdCg = factura.IdCg,
                    IdTipo = factura.IdTipoPago(Contexto),
                    Nombre = $"Abono de la factura '{factura.Referencia}'",
                    Descripcion = elemento.Anotacion,
                    Clase = elemento.Clase,
                    IdCliente = elemento.IdCliente,
                    IdSolicitante = solicitante.Id,
                    Nif = solicitante.NIF(Contexto),
                    Contacto = factura.Contacto,
                    Telefono = factura.Telefono,
                    eMail = factura.eMail,
                    IdCuentaDePago = elemento.IdCuentaDeCargo.Entero() > 0 ? elemento.IdCuentaDeCargo : null,
                    IdCuentaDeAcreedor = elemento.IdCuentaDeAbono.Entero() > 0 ? Contexto.SeleccionarPorId<CuentaDeClienteDtm>((int)elemento.IdCuentaDeCargo).IdCuenta : null,
                    PagarEl = fechaDeAbono > DateTime.Now.Date ? fechaDeAbono : null,
                    PagadoEl = fechaDeAbono <= DateTime.Now.Date ? fechaDeAbono : null,
                    Importe = elemento.Importe,
                };
                factura.ValidarAbono(Contexto, abono.Pago);

                abono.FacturaEmt = factura;
                abono.Pago = abono.Pago.Insertar(Contexto, accionEjecutada: ltrDeUnPago.Accion_CrearAbono, parametros: new Dictionary<string, object> { { nameof(AbonoDeFaeDtm.FacturaEmt), factura} });
                abono.idElemento2 = abono.Pago.Id;
                abono.idElemento1 = factura.Id;

                if (elemento.IdJustificante.Entero() > 0)
                    abono.Pago.Vincular(Contexto, enumNegocio.Archivos, (int)elemento.IdJustificante);
            }
        }

        protected override void DespuesDePersistir(AbonoDeFaeDtm abono, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(abono, parametros);

            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                abono.TrasRealizarUnAbono(Contexto, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
            }
            else
            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {
                var paramDeTransicion = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
                if (parametros.Parametros.ContainsKey(nameof(VariableDeFacturasEmt.enumMotivoTransicion)))
                    paramDeTransicion.Add(nameof(VariableDeFacturasEmt.enumMotivoTransicion), parametros.Parametros[nameof(VariableDeFacturasEmt.enumMotivoTransicion)]);
                
                abono.TrasEliminarUnAbono(Contexto, paramDeTransicion);
                abono.Factura(Contexto).CrearTraza(Contexto,
                $"Eliminacion de abono",
                $"El usuario {Contexto.DatosDeConexion.Login} ha cancelado el abono {abono.Pago(Contexto).Referencia}");
            }
        }

        protected override void EliminarCaches(AbonoDeFaeDtm abono, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(abono, parametros);
            var factura = abono.Factura(Contexto).RectificaA(Contexto);
            ServicioDeCaches.EliminarElemento(CacheDe.Fae_Cobrado, $"{factura.Id}");
            ServicioDeCaches.EliminarElemento(CacheDe.Fae_Abonado, $"{abono.Factura(Contexto).Id}");
            VinculoSql.VaciarCache(Contexto, typeof(FacturaEmtDtm), enumNegocio.Pago, abono.idElemento1, abono.idElemento2);
            var expediente = factura.Presupuesto(Contexto)?.Expediente(Contexto) ?? null;
            if (expediente is not null) ServicioDeCaches.EliminarElemento(CacheDe.Exp_Cobros, expediente.Id.ToString());
        }

        protected override void DespuesDeMapearElElemento(AbonoDeFaeDtm abono, AbonoDeFaeDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(abono, elemento, parametros);

            elemento.IdElemento = abono.idElemento1;
            elemento.IdAbono = abono.idElemento2;
            elemento.Elemento = abono.Factura(Contexto).Expresion;
            elemento.Abono = abono.Pago(Contexto)?.Expresion ?? string.Empty;
            elemento.Cliente = abono.Pago(Contexto).Cliente(Contexto).Expresion;
            elemento.IdCliente = abono.Factura(Contexto).IdCliente;
            elemento.Referencia = abono.Pago(Contexto).Referencia;
            elemento.Estado = abono.Pago(Contexto).Estado(Contexto).Nombre;
            elemento.Clase = abono.Pago(Contexto).Clase;
            elemento.IdCuentaDeAbono = abono.Pago(Contexto).IdCuentaDeAcreedor;
            elemento.IdCuentaDeCargo = abono.Pago(Contexto).IdCuentaDePago;
            elemento.CuentaDeAbono = abono.CuentaDeAbono(Contexto, errorSiNoHay: false)?.NumeroIban ?? string.Empty;
            elemento.CuentaDeCargo = abono.CuentaDeCargo(Contexto, errorSiNoHay: false)?.NumeroIban ?? string.Empty;
            elemento.AbonadoEl = abono.Pago(Contexto).PagadoEl;
            elemento.Pendiente = abono.Pendiente(Contexto);
            elemento.Importe = abono.Pago(Contexto).Importe;

            elemento.Clase = abono.Pago(Contexto).Clase;
        }

    }
}
