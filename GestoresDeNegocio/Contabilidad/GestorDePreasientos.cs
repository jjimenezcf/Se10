using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Gastos;
using GestoresDeNegocio.Ventas;
using ModeloDeDto;
using ModeloDeDto.Contabilidad;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilidades;

namespace GestoresDeNegocio.Contabilidad
{

    public class GestorDePreasientos : GestorDeElementos<ContextoSe, PreasientoDtm, PreasientoDto>, ITotalizador<TotalesPorCuenta>
    {
        public class MapearPreasiento : Profile
        {
            public MapearPreasiento()
            {
                CreateMap<PreasientoDtm, PreasientoDto>()
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Expresion))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado.Nombre));

                CreateMap<PreasientoDto, PreasientoDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.Preasiento;

        //public override TiposDelTipoDeElemento TiposDelTipo => Negocio.TiposDelTipo();

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDePreasiento.Gestor(Contexto, Contexto.Mapeador);

        public GestorDePreasientos(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDePreasientos Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePreasientos(contexto, mapeador);
        }


        protected override IQueryable<PreasientoDtm> AplicarOrden(IQueryable<PreasientoDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override IQueryable<PreasientoDtm> AplicarFiltros(IQueryable<PreasientoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            parametros.AplicarFiltroQueMostrar = !filtros.OmitirFiltrosPorEstado(new List<string> { ltrDeUnPreasiento.FiltroLoteContable });
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroPorLoteContable(Contexto, filtros);
            consulta = consulta.FiltroPorReferenciado(Contexto, filtros);
            consulta = consulta.FiltroPorPreasientoReferenciaCuentaApunte(Contexto, filtros);
            consulta = consulta.FiltroEntreImportes(Contexto, filtros);
            return consulta;
        }

        protected override IQueryable<PreasientoDtm> AplicarSeguridad(IQueryable<PreasientoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<PreasientoDtm, TipoDePreasientoDtm, PermisoDelPreasientoDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<PreasientoDtm, PermisoDelPreasientoDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void AntesDeMapearElRegistroParaInsertar(PreasientoDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(elemento, opciones);

        }

        protected override void AntesDePersistir(PreasientoDtm spr, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(spr, parametros);

            if (parametros.EsUnaPeticion && parametros.Peticion != enumPeticion.epTransitar)
                GestorDeErrores.Emitir("Los preasientos no son modificables");

            if (parametros.Insertando)
            {
                var cancelados = Negocio.Estados(Contexto).Where(e => e.Cancelado).Select(e => e.Id).ToList();
                var preasiento = Contexto.Set<PreasientoDtm>().FirstOrDefault(x => x.NegocioReferenciado == spr.NegocioReferenciado && x.IdReferenciado == spr.IdReferenciado && !cancelados.Contains(x.IdEstado));
                if (preasiento != null)
                {
                    GestorDeErrores.Emitir($"Antes de preasentar el '{preasiento.Referenciado(Contexto).Referencia}', del negocio '{spr.NegocioReferenciado}' ha de cancelar el existente, '{preasiento.Referencia}'");
                }

                //var ejercicioValido = preasiento.Ejercicio == DateTime.Now.Year || (preasiento.Ejercicio == DateTime.Now.Year - 1 && DateTime.Now.Month < 6);

                //if (!ejercicioValido)
                //    GestorDeErrores.Emitir($"No se puede crear un preasiento para el ejercicio {preasiento.Ejercicio} por estar cerrado");
            }

        }


        protected override void DespuesDePersistir(PreasientoDtm spr, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(spr, parametros);
        }

        protected override PreasientoDtm AntesDeTransitar(PreasientoDtm spr, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            spr = base.AntesDeTransitar(spr, transicion, parametros);

            //if (transicion.EntreEtapas(enumEtapasDePreasiento.SPR_Etapa_Contabilizado.Estados(), enumEtapasDePreasiento.SPR_Etapa_Pendiente.Estados()))
            //    spr.AntesDeDevolverAPendiente(Contexto);

            return spr;
        }

        protected override PreasientoDtm DespuesDeTransitar(PreasientoDtm spr, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            spr = base.DespuesDeTransitar(spr, transicion, parametros);

            return spr;
        }

        protected override void DespuesDeMapearElElemento(PreasientoDtm spr, PreasientoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(spr, elemento, parametros);
            ObtenerOrigenImporte(spr, elemento);
            if (parametros.Peticion == enumPeticion.epLeerPorId)
                elemento.Descripcion = FormatearAsiento(spr);
        }

        private void ObtenerOrigenImporte(PreasientoDtm spr, PreasientoDto elemento)
        {
            elemento.Origen = spr.NegocioReferenciado.Singular();
            if (spr.NegocioReferenciado == enumNegocio.FacturaRecibida)
            {
                var factura = (FacturaRecDtm)spr.Referenciado(Contexto, errorSiNoHay: false);
                if (factura != null)
                {
                    elemento.Origen = $"FR: {factura.Referencia}, {factura.Contacto}: {factura.Numero}";
                    elemento.Importe = factura.Total(Contexto, Enumerados.enumImporteFar.TotalPagar);
                }
                else GestorDeErrores.Emitir($"El preasiento '{spr.Referencia}' indica una referencia a  '{elemento.Origen}' pero dicha referencia no existe");
            }
            else if (spr.NegocioReferenciado == enumNegocio.Pago)
            {
                var pago = (PagoDtm)spr.Referenciado(Contexto, errorSiNoHay: false);
                if (pago != null)
                {
                    elemento.Origen = $"PG: {pago.Referencia}, {pago.Contacto}: {pago.FormaDePago}";
                    elemento.Importe = pago.Importe;
                }
                else GestorDeErrores.Emitir($"El preasiento '{spr.Referencia}' indica una referencia a  '{elemento.Origen}' pero dicha referencia no existe");

            }
            else if (spr.NegocioReferenciado == enumNegocio.FacturaEmitida)
            {
                var factura = (FacturaEmtDtm)spr.Referenciado(Contexto, errorSiNoHay: false);
                if (factura != null)
                {
                    elemento.Origen = "FE: " + factura.Referencia;
                    elemento.Importe = factura.APagar(Contexto);
                }
                else GestorDeErrores.Emitir($"El preasiento '{spr.Referencia}' indica una referencia a  '{elemento.Origen}' pero dicha referencia no existe");
            }
            else if (spr.NegocioReferenciado == enumNegocio.Cobro)
            {
                var cobro = (CobroDeFaeDtm)spr.Referenciado(Contexto, errorSiNoHay: false);
                if (cobro != null)
                {
                    cobro.CalcularReferencia(Contexto);
                    elemento.Origen = "CO: " + cobro.Referencia;
                    elemento.Importe = cobro.Cobrado;
                }
            }
        }

        private string FormatearAsiento(PreasientoDtm spr)
        {
            var apuntes = spr.Detalles<ApunteDeUnPreasientoDtm>(Contexto);

            const int anchoApunte = 40;
            const int anchoSeparador = 5;

            var sb = new StringBuilder();

            // Encabezados
            sb.AppendLine($"{enumPosicionContable.Debe.Descripcion().PadRight(anchoApunte)}{"".PadRight(anchoSeparador)}{enumPosicionContable.Haber.Descripcion().PadLeft(anchoApunte)}");
            sb.AppendLine(new string('-', anchoApunte + anchoSeparador + anchoApunte));

            decimal totalDebe = 0;
            foreach (var debe in apuntes.Where(apunte => apunte.Posicion == enumPosicionContable.Debe && apunte.Importe != 0))
            {
                totalDebe += debe.Importe;
                var linea = $"{debe.Cuenta}: {debe.Importe.Moneda(alineacion: false, mostrarUltimoDecimal: false)} ({debe.Concepto})".PadRight(anchoApunte) + "".PadRight(anchoSeparador + anchoApunte);
                sb.AppendLine(linea.Left(anchoApunte + anchoSeparador + anchoApunte));
            }


            decimal totalHaber = 0;
            foreach (var haber in apuntes.Where(apunte => apunte.Posicion == enumPosicionContable.Haber && apunte.Importe != 0))
            {
                totalHaber += haber.Importe;
                var linea = "".PadRight(anchoApunte + anchoSeparador) + $" ({haber.Concepto}) {haber.Importe.Moneda(alineacion: false, mostrarUltimoDecimal: false)}: {haber.Cuenta}".PadLeft(anchoApunte);
                sb.AppendLine(linea.Right(anchoApunte + anchoSeparador + anchoApunte));
            }

            // Línea separadora
            sb.AppendLine(new string('-', anchoApunte + anchoSeparador + anchoApunte));
            sb.AppendLine($"{totalDebe.Moneda(alineacion: false, mostrarUltimoDecimal: false)}".PadRight(anchoApunte) +
                "".PadRight(anchoSeparador) +
                $"{totalHaber.Moneda(alineacion: false, mostrarUltimoDecimal: false)}".PadLeft(anchoApunte));


            return sb.ToString();
        }

        public void CrearLoteConUnPreasiento(List<int> ids)
        {
            if (ids.Count != 1)
                GestorDeErrores.Emitir("Esta opción es para crear un lote contable con un preasiento");

            var preasiento = Contexto.SeleccionarPorId<PreasientoDtm>(ids.First());

            if (!preasiento.EsAdministrador(Contexto))
                GestorDeErrores.Emitir($"Ha de ser administrador del preasiento '{preasiento.Referencia}' para poder contabilizar");

            if (preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Cancelado) || preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Anulado))
            {
                GestorDeErrores.Emitir($"El preasiento '{preasiento.Referencia}' está anulado o cancelado, regenérelo");
            }

            var contabilizar = PrepararAccionParaContabilizar(preasiento);

            var trans = Contexto.IniciarTransaccion();
            try
            {
                if (preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Contabilizado))
                {
                    preasiento.AnularContabilizacion(Contexto);
                }
                EntornoDeUnaAccion.EjecutarAccion(contabilizar);

                Contexto.Commit(trans);
            }
            catch (Exception e)
            {
                Contexto.Rollback(trans);
                throw e.Emitir(mensaje: e.InnerException?.Message ?? e.Message);
            }
            finally
            {
                contabilizar.Contexto.Accion = null;
                VaciarCacheDeRegistro(preasiento, enumTipoOperacion.Modificar, preasiento.Nombre);
            }
        }

        private EntornoDeUnaAccion PrepararAccionParaContabilizar(PreasientoDtm preasiento)
        {
            var sociedad = preasiento.Cg(Contexto, aplicarJoin: true).Sociedad;
            var idSociedad = sociedad.Id;
            var comoContabilizar = sociedad.ContabilizarEn();
            var metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, typeof(ExportacionesDePreasientos).FullName, comoContabilizar.Metodo);
            if (metodo == null)
                GestorDeErrores.Emitir($"No está definido el método {metodo} en la clase {typeof(ExportacionesDePreasientos).FullName}");

            var filtros = new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(nameof(PreasientoDtm.Id), enumCriteriosDeFiltrado.igual, preasiento.Id) };
            var filtrosJson = JsonConvert.SerializeObject(filtros);
            var parametros = new Dictionary<string, object>
                        {
                            {ltrFiltros.filtro, filtrosJson},
                            {nameof(SociedadDtm.Id), idSociedad},
                            {nameof(PreasientoDtm.FechaContable), null },
                            {nameof(CrearLoteContableDto.Descontabilizar),false },
                            {nameof(CrearLoteContableDto.RespetarFechaContable),true }
                        };
            var contabilizar = new EntornoDeUnaAccion(Contexto, enumNegocio.Preasiento, parametros);

            contabilizar.Contexto.Accion = new AccionDtm
            {
                Dll = ApiDeEnsamblados.GestoresDeNegocio,
                Clase = typeof(ExportacionesDePreasientos).FullName,
                Metodo = comoContabilizar.Metodo,
                Nombre = comoContabilizar.Nombre,
                ClaseDeAccion = enumClaseDeAccion.DLL.ToString()
            };
            return contabilizar;
        }

        public void RegenerarPreasientos(List<int> ids)
        {
            var trans = Contexto.IniciarTransaccion();
            try
            {
                foreach (int id in ids)
                {
                    var preasiento = Contexto.SeleccionarPorId<PreasientoDtm>(id);

                    if (!preasiento.EsAdministrador(Contexto))
                        GestorDeErrores.Emitir($"Ha de ser administrador del preasiento '{preasiento.Referencia}' para poder cancelarlo");

                    if (!preasiento.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePreasiento> { enumEtapasDePreasiento.SPR_Etapa_Pendiente }))
                        GestorDeErrores.Emitir($"No se puede regenerar el  preasiento '{preasiento.Referencia}' por no estar en la etapa correcta");

                    var referenciado = preasiento.Referenciado(Contexto);
                    if (referenciado.IdPreasiento is not null)
                    {
                        if (referenciado.IdPreasiento == preasiento.Id)
                        {
                            IGeneradorDePreasiento gestor = null;
                            switch (preasiento.NegocioReferenciado)
                            {
                                case enumNegocio.FacturaEmitida:
                                    gestor = GestorDeFacturasEmt.Gestor(Contexto, Mapeador);
                                    break;
                                case enumNegocio.FacturaRecibida:
                                    gestor = GestorDeFacturasRec.Gestor(Contexto, Mapeador);
                                    break;
                                case enumNegocio.Pago:
                                    gestor = GestorDePagos.Gestor(Contexto, Mapeador);
                                    break;
                                case enumNegocio.Cobro:
                                    gestor = GestorDeCobrosDeFae.Gestor(Contexto, Mapeador);
                                    break;
                                default:
                                    GestorDeErrores.Emitir($"No se puede cancelar el preasiento por estar referenciado por '{preasiento.NegocioReferenciado}: {preasiento.Referenciado(Contexto).Referencia}', regenérelo");
                                    break;
                            }
                            gestor.GenerarPreasiento(new List<int> { preasiento.Referenciado(Contexto).Id });
                        }
                        else
                            preasiento.CancelarPreasiento(Contexto);
                    }
                }
                Contexto.Commit(trans);
            }
            catch
            {
                Contexto.Rollback(trans);
                throw;
            }
        }

        public async Task<TotalesPorCuenta> ObtenerTotalesAsync(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            return await Task.Run(() => ObtenerTotales(filtros, posicion, cantidad));
        }


        private TotalesPorCuenta ObtenerTotales(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            var totales = new TotalesPorCuenta();
            var preasientos = Contexto.SeleccionarTodos<PreasientoDtm>(filtros, parametros: new Dictionary<string, object> {
                { ltrParametrosNeg.PosicionInicial, posicion},
                { ltrParametrosNeg.CantidadPorLeer, cantidad},
                { ltrParametrosNeg.Peticion, enumPeticion.epTotales},
            });

            var saldosPorCuenta = new List<Saldo>();
            var saldosPorGrupo = new List<Saldo>();
            foreach (var p in preasientos)
            {
                var apuntes = p.Detalles<ApunteDeUnPreasientoDtm>(Contexto);
                foreach (var apunte in apuntes)
                {
                    var porCuenta = saldosPorCuenta.FirstOrDefault(s => s.Cuenta == apunte.Cuenta);
                    totalizarApunte(saldosPorCuenta, apunte, porCuenta, apunte.Cuenta);

                    var porGrupo = saldosPorGrupo.FirstOrDefault(s => s.Cuenta == apunte.Cuenta.Left(3));
                    totalizarApunte(saldosPorGrupo, apunte, porGrupo, apunte.Cuenta.Left(3));

                }
            }

            totales.TotalesPorCuentas = "Cuenta".PadLeft(15, ' ') + "Debe".PadLeft(15, ' ') + "Haber".PadLeft(15, ' ');
            totales.TotalesPorCuentas = totales.TotalesPorCuentas.Left(100) + Environment.NewLine;
            totales.TotalesPorCuentas = totales.TotalesPorCuentas + "-".PadLeft(100, '-') + Environment.NewLine;
            foreach (Saldo saldo in saldosPorCuenta.OrderBy(s => s.Cuenta))
            {
                var nombreCuenta = "";
                if (saldo.Cuenta.StartsWith("43") && saldo.Cuenta.Length == 10)
                {
                    var codigoContable = saldo.Cuenta.Right(4);
                    var cliente = Contexto.SeleccionarPorPropiedad<ClienteDtm>(nameof(ClienteDtm.CodigoContable), codigoContable.Entero(), errorSiNoHay: false);
                    nombreCuenta = cliente is null ? "No definido" : cliente.RazonSocial(Contexto);
                }

                if (saldo.Cuenta.StartsWith("40") && saldo.Cuenta.Length == 10)
                {
                    var codigoContable = saldo.Cuenta.Right(4);
                    var proveedor = Contexto.SeleccionarPorPropiedad<ProveedorDtm>(nameof(ProveedorDtm.CodigoContable), codigoContable.Entero(), errorSiNoHay: false);
                    nombreCuenta = proveedor is null ? "No definido" : proveedor.RazonSocial(Contexto);
                }

                if (saldo.Cuenta.Length == 6)
                {
                    var cuenta = Contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), saldo.Cuenta, errorSiNoHay: false);
                    nombreCuenta = cuenta is null ? "No definida" : cuenta.Nombre;
                }
                var linea = $"{saldo.Cuenta.PadLeft(15, ' ')}{saldo.Debe.Moneda().PadLeft(15, ' ')}{saldo.Haber.Moneda().PadLeft(15, ' ')}"
                    + "          " + nombreCuenta;
                totales.TotalesPorCuentas = totales.TotalesPorCuentas + linea.Left(100) + Environment.NewLine;
            }

            totales.TotalesPorGrupo = "Grupo".PadLeft(30, ' ') + "Debe".PadLeft(15, ' ') + "Haber".PadLeft(15, ' ') + Environment.NewLine +
                "-".PadLeft(100, '-') + Environment.NewLine;

            foreach (Saldo saldo in saldosPorGrupo.OrderBy(s => s.Cuenta))
            {
                var lineaa = $"{saldo.Cuenta.PadLeft(30, ' ')}{saldo.Debe.Moneda().PadLeft(15, ' ')}{saldo.Haber.Moneda().PadLeft(15, ' ')}";
                totales.TotalesPorGrupo = totales.TotalesPorGrupo + lineaa + Environment.NewLine;
            }


            totales.Procesados = preasientos.Count;
            return totales;
        }

        private static void totalizarApunte(List<Saldo> saldos, ApunteDeUnPreasientoDtm apunte, Saldo saldo, string indice)
        {
            if (saldo != null)
            {
                if (apunte.Posicion == enumPosicionContable.Debe)
                    saldo.Debe = saldo.Debe + apunte.Importe;
                else
                    saldo.Haber = saldo.Haber + apunte.Importe;
            }
            else
            {
                saldo = new Saldo();
                saldo.Cuenta = indice;
                if (apunte.Posicion == enumPosicionContable.Debe)
                    saldo.Debe = apunte.Importe;
                else
                    saldo.Haber = apunte.Importe;
                saldos.Add(saldo);
            }
        }
    }

}
