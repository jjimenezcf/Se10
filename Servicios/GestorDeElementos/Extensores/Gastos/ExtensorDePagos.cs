using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;
namespace GestorDeElementos.Extensores
{
    public static class ExtensorDePagos
    {
        public static bool UsaElDetalleDe(Type tipoDeDetalle)
        {

            return false;
        }

        public static List<string> ListaDeEtapas(this PagoDtm pago) => pago.CadenaDeEtapas().ToLista<string>(Simbolos.separadorDeEtapas);

        public static bool EstaEnLaEtapa(this PagoDtm pago, string etapa) => etapa.ToLista<int>(Simbolos.Coma).Contains(pago.IdEstado);

        public static CuentaBancariaDtm CuentaBancariaAcreedora(this PagoDtm pago, ContextoSe contexto)
        =>
        pago.CuentaDeAcreedor ?? (pago.CuentaDeAcreedor = contexto.SeleccionarPorId<CuentaBancariaDtm>((int)pago.IdCuentaDeAcreedor));

        public static CuentaBancariaDtm CuentaBancariaDePago(this PagoDtm pago, ContextoSe contexto)
        {
            if (pago.CuentaDePago is null)
                pago.CuentaDePago = contexto.SeleccionarPorId<CuentaDeMiSociedadDtm>((int)pago.IdCuentaDePago, aplicarJoin: true);

            return pago.CuentaDePago.Cuenta(contexto);
        }

        public static CuentaDeMiSociedadDtm CuentaDePago(this PagoDtm pago, ContextoSe contexto)
        =>
        pago.CuentaDePago is null
        ? contexto.SeleccionarPorId<CuentaDeMiSociedadDtm>((int)pago.IdCuentaDePago) :
        pago.CuentaDePago;

        public static bool CuentaDeAcreedorActiva(this PagoDtm pago, ContextoSe contexto)
        {
            var inter = contexto.SeleccionarPorId<InterlocutorDtm>(pago.IdSolicitante);
            if (!pago.EsAbono(contexto))
            {
                var proveedor = inter.Proveedor(contexto, crearProveedor: false);
                if (proveedor is not null)
                    return proveedor.CuentaDeProveedor(contexto, enumClaseDeCuentaBancaria.Ingreso).Activa;

                var trabajador = inter.Trabajador(contexto);
                if (trabajador is not null)
                    return trabajador.CuentaDeTrabajador(contexto, enumClaseDeCuentaBancaria.Ingreso).Activa;

                return inter.CuentaDeInterlocutor(contexto, enumClaseDeCuentaBancaria.Ingreso).Activa;
            }
            else
            {
                var cliente = inter.Cliente(contexto, crearCliente: false);
                if (cliente is null)
                    Emitir($"El pago '{pago.Referencia}' debe ser un abono a un cliente, y el Nif '{inter.NIF(contexto)}' no lo es");
                return cliente.CuentaDeCliente(contexto, enumClaseDeCuentaBancaria.Ingreso).Activa;
            }

        }

        public static CuentaBancariaDtm CuentaDeAcreedor(this PagoDtm pago, ContextoSe contexto)
        {
            var inter = contexto.SeleccionarPorId<InterlocutorDtm>(pago.IdSolicitante);
            var proveedor = inter.Proveedor(contexto, crearProveedor: false);
            if (proveedor is not null)
                return proveedor.CuentaDeProveedor(contexto, enumClaseDeCuentaBancaria.Ingreso).Cuenta(contexto);

            var trabajador = inter.Trabajador(contexto);
            if (trabajador is not null)
                return trabajador.CuentaDeTrabajador(contexto, enumClaseDeCuentaBancaria.Ingreso).Cuenta(contexto);

            return inter.CuentaDeInterlocutor(contexto, enumClaseDeCuentaBancaria.Ingreso).Cuenta(contexto);
        }

        public static PagoDtm AntesDeCancelar(this PagoDtm pago, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (contexto.Existen<PagoDeUnaRemesaDtm>(nameof(PagoDeUnaRemesaDtm.IdPago), pago.Id))
                Emitir($"No se puede cancelar el pago {pago.Referencia} por que está remesado, quítelo de la remesa");

            if (pago.PagadoEl is not null)
                Emitir($"No se puede cancelar el pago {pago.Referencia} por que se indica que ya está pagado con fecha '{((DateTime)pago.PagadoEl).ToShortDateString()}'");

            var anexados = pago.LeerAnexados(contexto);
            if (anexados.Count > 0 && pago.IdFacturaRec is not null)
            {
                var factura = pago.FacturaRec(contexto);
                if (factura.IdArchivo is not null)
                {
                    var ficheroFactura = anexados.FirstOrDefault(x => x.Id == factura.IdArchivo);
                    if (ficheroFactura is not null)
                    {
                        pago.QuitarAnexado(contexto, ficheroFactura.Id, validarPersistencia: false, QuitarDeRestoDeAnexados: true);
                    }
                }
            }

            pago.PagarEl = null;
            pago.CancelarPreasiento(contexto);
            return pago;
        }

        public static PagoDtm AntesDeReabrir(this PagoDtm pago, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (contexto.Existen<PagoDeUnaRemesaDtm>(nameof(PagoDeUnaRemesaDtm.IdPago), pago.Id))
                Emitir($"No se puede reabrir el pago {pago.Referencia} por que está remesado");

            pago.PagarEl = pago.IdFacturaRec is null ? null : contexto.SeleccionarPorId<FacturaRecDtm>((int)pago.IdFacturaRec, aplicarPermisos: false).VenceEl;
            pago.PagadoEl = null;

            pago.AnularPreasiento(contexto, parametros);

            return pago;
        }


        public static PagoDtm AntesDeAnular(this PagoDtm pago, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (pago.Clase != enumClaseDePago.Remesa)
                Emitir($"No se puede anular el pago '{pago.Referencia}'. Sólo se pueden anular los pagos remesados");

            pago.PagadoEl = null;
            return pago;
        }

        public static PagoDtm AntesDePagar(this PagoDtm pago, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            pago.ValidarPago(contexto);

            var factura = pago.FacturaRec(contexto, errorSiNoHay: false);
            if (factura != null)
            {
                if (factura.Detalles<LineaDeUnaFarDtm>(contexto, errorSiNoHay: false).Count() == 0)
                    Emitir($"No puede dar por pagado '{pago.Referencia}' ya que la factura referenciada '{factura.Referencia}' no tiene el detalle cumplimentado");

                var totalDelPago = factura.TotalDelPago;
                var importePagados = factura.ImportesDePagosConfirmados(contexto);
                var importesEnCurso = factura.PagosEnCurso(contexto).Sum(p => p.Importe);

                //if (factura.EstaPagada(contexto))
                //    Emitir($"No se puede transitar el pago '{pago.Referencia}' asociado a la factura '{factura.Referencia}' ya que esta está pagada");


                if (pago.Clase == enumClaseDePago.Contado && pago.ModoDePago == enumModoDePagoContado.Domiciliacion)
                {
                    if (factura.IdArchivo == null)
                        Emitir($"Está creando el pago domiciliado '{pago.Referencia}' sobre la factura '{factura.Referencia}' y la factura no tiene archivo asociada, asócielo");
                }
                else
                {
                    var exigeJustificante = ParametroDeNegocioSql.Parametro(enumNegocio.Pago, enumParametrosDePagos.PAG_Validar_Justificante_De_Pago, emitirError: false, crearParametro: true, valorPorDefecto: "S").Valor.EsTrue();
                    if (exigeJustificante && (pago.Clase == enumClaseDePago.Contado || pago.Clase == enumClaseDePago.Transferencia))
                    {
                        if (!GestorDeVinculos.Existen(contexto, enumNegocio.Pago, enumNegocio.Archivos, pago.Id))
                            Emitir(string.Format(ltrDeUnPago.Mensaje_FaltaDePago, pago.Referencia));
                    }
                }

            }

            if (pago.PagarEl is null)
                pago.PagarEl = DateTime.Now.Date;

            if (pago.PagadoEl is null && pago.Clase == enumClaseDePago.Contado)
                pago.PagadoEl = DateTime.Now.Date;

            if (pago.Clase == enumClaseDePago.Transferencia && (pago.IdCuentaDeAcreedor is null || pago.IdCuentaDePago is null))
                Emitir($"No puede dar por pagado la transferencia '{pago.Referencia}' por no tener indicada la cuenta deudora o acreedora");

            if (pago.PagadoEl is null && pago.Clase == enumClaseDePago.Transferencia)
            {
                var dias = enumNegocio.Pago.LeerCrearParametro(contexto, enumParametrosDeRemesasFae.REM_DiasDeEsperaDeCargo, "2");
                pago.PagadoEl = pago.PagarEl.Fecha().AddDays(dias.Valor.Entero());
            }

            if (pago.PagadoEl is null && pago.Clase == enumClaseDePago.Remesa)
            {
                var remesa = pago.Remesa(contexto, errorSiNoHay: false);
                if (remesa is null)
                    Emitir($"El pago '{pago.Referencia}' no puede ser transitado, se ha indicado que es para remesar, se pagará tras presentar la remesa");
                else
                    Emitir($"El pago '{pago.Referencia}' no puede ser transitado, está incluido en la remesa '{remesa.Referencia}', se dará por pagado cuando cierre la remesa");
            }

            if (pago.PagadoEl is not null && pago.PagadoEl.Fecha().Date > DateTime.Now.Date)
            {
                Emitir($"El pago '{pago.Referencia}' no puede ser transitado,por ser su día de pago '{((DateTime)pago.PagadoEl).ToShortDateString()}' mayor que el día de hoy");
            }
            pago = pago.Preasentar(contexto);

            if (parametros.LeerValor(ltrParametrosNeg.AccionQueSeEjecuta, "") != ltrDeUnPago.Accion_DarPorPagadoAlTransitarFactura)
                parametros[ltrParametrosNeg.AccionQueSeEjecuta] = ltrDeUnPago.Accion_Pagar;

            return pago;
        }

        public static void ValidarPago(this PagoDtm pago, ContextoSe contexto)
        {
            pago.ValidarImporteNegativo(contexto);
            pago.ValidarImporteNoMayorAlDeLaFactura(contexto);
            pago.ValidarMediosDePagoActivo(contexto);
            pago.ValidarNaturaleza(contexto);
        }

        private static void ValidarNaturaleza(this PagoDtm pago, ContextoSe contexto)
        {
            if (pago.IdNaturaleza is not null && pago.IdCliente is not null)
            {
                Emitir($"El pago '{pago.Referencia}' no puede indicar naturaleza de gasto ya que está referenciado a un abono de una factura de cliente");
            }
            if (pago.IdNaturaleza is not null && pago.IdFacturaRec is not null && !pago.FacturaRec(contexto).EsRectificativa)
            {
                Emitir($"El pago '{pago.Referencia}' no puede indicar naturaleza de gasto ya que está pagando la factura '{pago.FacturaRec(contexto).Referencia}'");
            }
            if (pago.IdNaturaleza is not null && pago.IdFacturaRec is not null && pago.FacturaRec(contexto).EsRectificativa)
            {
                Emitir($"El pago '{pago.Referencia}' no puede indicar naturaleza de gasto ya que es una devolución de la factura '{pago.FacturaRec(contexto).Referencia}'");
            }
            if (pago.IdNaturaleza is null && pago.IdFacturaRec is null && pago.IdCliente is null)
            {
                Emitir($"No se ha indicado la naturaleza del gasto en el pago '{pago.Referencia}'");
            }

            if (pago.IdFacturaRec is not null || pago.EsAbono(contexto)) return;

            if (pago.Sociedad(contexto).ContabilizarEn(errorSiNoDefinido: false)?.Metodo == ApiDeEnsamblados.MetodoDeEstimacionDirectaEnNcs)
            {
                pago.Naturaleza(contexto).ConceptoDeGasto();
            }

        }


        private static void ValidarImporteNegativo(this PagoDtm pago, ContextoSe contexto)
        {
            if (pago.Importe <= 0)
            {
                if (pago.Clase == enumClaseDePago.Contado && !pago.EsAbono(contexto))
                {
                    var factura = pago.FacturaRec(contexto, errorSiNoHay: false);

                    if (factura == null && pago.Trabajador(contexto, errorSiNoHay: false) == null && pago.Proveedor(contexto, errorSiNoHay: false) == null)
                        return;

                    if (factura != null)
                    {
                        var pagado = factura.PagosRealizados(contexto).Where(p => p.Id != pago.Id).Sum(p => p.Importe);
                        if (factura.EsRectificativa)
                        {
                            var yaDevuelto = Math.Abs(pagado);
                            var totalPorDevolver = Math.Abs(factura.TotalDelPago);
                            var devolucion = Math.Abs(pago.Importe);
                            var maximo = totalPorDevolver - yaDevuelto;

                            if (totalPorDevolver - yaDevuelto < devolucion)
                                Emitir($"Lo máximo que le pueden devolver por la factura '{factura.Referencia}' es de '{maximo.ToMoneda()}' y la devolución es de '{devolucion.ToMoneda()}'");
                            return;
                        }

                        if (pago.ModoDePago == enumModoDePagoContado.Contado)
                        {
                            var importeAceptado = Math.Abs(factura.TotalDelPago - (pagado + pago.Importe)) <= VariableDeFacturasRec.ToleranciaEnImportes();
                            if (!importeAceptado)
                            {
                                Emitir($"No puede crear el pago '{pago.Importe.ToMoneda()}' sobre la factura '{factura.Referencia}' ya que el total de la factura es '{factura.TotalDelPago.ToMoneda()}' y la suma de los pagos sería '{(pagado + pago.Importe).ToMoneda()}'");
                                return;
                            }
                        }
                    }
                }
                else
                {
                    Emitir($"Un pago de modo '{pago.Clase.Descripcion()}' no puede ser negativo");
                }
            }
        }

        private static void ValidarImporteNoMayorAlDeLaFactura(this PagoDtm pago, ContextoSe contexto)
        {
            var factura = pago.FacturaRec(contexto, errorSiNoHay: false);
            if (factura is not null)
            {
                var totalPagos = Math.Abs(factura.PagosRealizados(contexto).Where(x => x.Id != pago.Id).Sum(x => x.Importe) + pago.Importe);
                if (totalPagos - Math.Abs(factura.TotalDelPago) > VariableDeFacturasRec.ToleranciaEnImportes())
                {
                    if (factura.TotalDelPago < 0)
                        Emitir($"No puede crear o modificar el pago '{pago.Referencia}' ya que el total a pagar de la factura es '{factura.TotalDelPago.ToMoneda()}' y lo ya registrado más este pago '{totalPagos.ToMoneda()}' hacen que la devolución sea mayor");
                    else
                        Emitir($"No puede crear o modificar el pago '{pago.Referencia}' ya que el total a pagar de la factura es '{factura.TotalDelPago.ToMoneda()}' y lo ya registrado más este pago serían '{totalPagos.ToMoneda()}' y esto sobrepasa la factura");
                }
            }
        }

        private static void ValidarMediosDePagoActivo(this PagoDtm pago, ContextoSe contexto)
        {
            if (pago.IdTarjetaDePago is not null)
            {
                var tarjeta = contexto.SeleccionarPorId<TarjetaDeMiSociedadDtm>(pago.IdTarjetaDePago.Entero());
                if (!tarjeta.Activa)
                    Emitir($"la tarjeta seleccionada '{tarjeta.Expresion}' no está activa");

                if (tarjeta.IdElemento != pago.Cg(contexto).IdSociedad)
                {
                    Emitir($"la tarjeta seleccionada '{tarjeta.Expresion}' debe de ser de la sociedad '{pago.Sociedad(contexto).RazonSocial}' ");
                }

                pago.IdCuentaDePago = tarjeta.IdCuentaDeCargo;
            }

            if (pago.IdCuentaDePago is not null)
            {
                var cuentaDePago = pago.CuentaDePago(contexto);
                if (!cuentaDePago.Activa)
                    Emitir($"La cuenta de pago '{pago.CuentaBancariaDePago(contexto).NumeroIban}' asociada a '{pago.Referencia}' debe estar activa");

                if (cuentaDePago.IdElemento != pago.Cg(contexto).IdSociedad)
                {
                    Emitir($"la cuenta '{pago.CuentaBancariaDePago(contexto).NumeroIban}' debe de ser de la sociedad '{pago.Sociedad(contexto).RazonSocial}' ");
                }
            }

            if (pago.IdCuentaDeAcreedor is not null && !pago.CuentaDeAcreedorActiva(contexto))
                Emitir($"La cuenta acreedora asociada al pago '{pago.Referencia}' debe estar activa");
        }

        public static TarjetaDeMiSociedadDtm Tarjeta(this PagoDtm pago, ContextoSe contexto, bool erroSiNoHay = true)
        {
            if (pago.IdTarjetaDePago == null)
            {
                if (erroSiNoHay)
                    Emitir($"Se ha solicitado la tarjeta del pago '{pago.Referencia}' pero dicho pago no ha sido hecho con tarjeta");
                pago.TarjetaDePago = null;
            }
            else
            {
                if (pago.TarjetaDePago == null)
                    pago.TarjetaDePago = contexto.SeleccionarPorId<TarjetaDeMiSociedadDtm>(pago.IdTarjetaDePago.Entero());
            }

            return pago.TarjetaDePago;
        }


        public static string DetalleDeFormaDePago(this PagoDtm pago, ContextoSe contexto)
        {
            var detalle = pago.FormaDePago;
            if (pago.ModoDePago == enumModoDePagoContado.Tarjeta)
                detalle = $"{detalle} ({pago.Tarjeta(contexto).Alias})";
            else if (pago.ModoDePago == enumModoDePagoContado.Domiciliacion)
                detalle = $"{detalle} ({pago.CuentaDePago(contexto).Alias})";
            else if (pago.Clase == enumClaseDePago.Transferencia)
                detalle = $"{detalle} ({pago.CuentaBancariaAcreedora(contexto).NumeroIban})";
            else if (pago.Clase == enumClaseDePago.Remesa)
                detalle = $"{detalle} ({pago.Remesa(contexto, incluirTerminadas: true, errorSiNoHay: false)?.Referencia ?? ""})";

            detalle = $"{detalle}: {pago.Importe.ToMoneda()}";

            return detalle;

        }

        public static PagoDtm AntesDeRemesar(this PagoDtm pago, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (pago.Importe <= 0)
                Emitir($"No puede dar por pagado el pago '{pago.Referencia}' por no tener un importe mayor de cero ");

            return pago;
        }

        public static void PersistirEvento(this PagoDtm pago, ContextoSe contexto)
        {
            var sociedad = pago.Sociedad(contexto);
            if (sociedad.IdAgenda is not null)
                pago.EliminarEvento(contexto);
            pago.AnotarEvento(contexto);
        }

        private static void AnotarEvento(this PagoDtm pago, ContextoSe contexto)
        {
            if (pago.PagarEl is null) return;
            var sociedad = pago.Sociedad(contexto);
            var agenda = sociedad.IdAgenda == null ? sociedad.CrearAgenda(contexto) : contexto.SeleccionarPorId<AgendaDtm>((int)sociedad.IdAgenda);

            var evento = new EventoDeAgendaDtm();
            evento.IdAgenda = agenda.Id;
            evento.IdElemento = pago.Id;
            evento.IdNegocio = enumNegocio.Pago.IdNegocio();
            evento.EsDelSistema = true;
            evento.Inicio = ((DateTime)pago.PagarEl).Date;
            evento.Nombre = ltrDeUnPago.EventoDePago.Replace($"[{nameof(PagoDtm.Referencia)}]", pago.Referencia);
            evento.Descripcion = $"Pago: {pago.Referencia}, NIF : {pago.Nif}";
            evento.Fin = evento.Inicio;
            GestorDeVinculos.Vincular(contexto, pago, evento, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
        }

        public static void EliminarEvento(this PagoDtm pago, ContextoSe contexto)
        {
            var nombreEvento = ltrDeUnPago.EventoDePago.Replace($"[{nameof(PagoDtm.Referencia)}]", pago.Referencia);
            var eventos = contexto.SeleccionarEventos(pago.Id, nombreEvento);
            var p = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
            foreach (var e in eventos.Where(e => e.EsDelSistema)) pago.Desvincular(contexto, e, p);
        }

        public static DireccionDto DireccionDePago(this PagoDtm pago, ContextoSe contexto)
        {
            if (pago.IdProveedor is not null)
            {
                var proveedor = contexto.SeleccionarPorId<ProveedorDtm>((int)pago.IdProveedor);
                return proveedor.DireccionFiscal(contexto);
            }

            if (pago.IdTrabajador is not null)
            {
                var trabajador = contexto.SeleccionarPorId<TrabajadorDtm>((int)pago.IdTrabajador);
                return trabajador.DireccionDeContacto(contexto);
            }

            var inter = contexto.SeleccionarPorId<InterlocutorDtm>(pago.IdSolicitante);
            var dir = inter.DireccionDto(contexto, enumCalificadorDireccion.contacto, errorSiNoHay: false);
            if (dir != null) return dir;

            dir = inter.DireccionDto(contexto, enumCalificadorDireccion.contacto, errorSiNoHay: true);
            return dir;
        }

        public static RemesaPagDtm Remesa(this PagoDtm pago, ContextoSe contexto, bool errorSiNoHay = true, bool incluirTerminadas = false)
        {
            var remesas = contexto.SeleccionarTodos<RemesaPagDtm>(new Dictionary<string, object> { { ltrDeUnaRemesaPag.IdPagoEnRemesa, pago.Id } });
            if (remesas.Count == 0 && errorSiNoHay)
                Emitir($"El pago '{pago.Referencia}', no está incluido en ninguna remesa");

            if (remesas.Count == 0 && !errorSiNoHay) return null;

            var etapas = new List<enumEtapasDeRemesasPag>
            {
                enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion,
                enumEtapasDeRemesasPag.REM_Etapa_Generada,
                enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion
            };
            if (incluirTerminadas) etapas.Add(enumEtapasDeRemesasPag.REM_Etapa_De_Cierre);

            var remesa = remesas.Where(x => x.EstaEnAlgunaDeLasEtapa(etapas)).OrderByDescending(x => x.FechaCreacion).FirstOrDefault();

            if (remesa == null && errorSiNoHay)
                Emitir($"El pago '{pago.Referencia}', no está incluido en ninguna remesa válida");

            return remesa;
        }

        public static void AsignarCuentaAcreedora(this PagoDtm pago, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (pago.Solicitante(contexto).Proveedor(contexto) is not null)
                pago.IdCuentaDeAcreedor = pago.Solicitante(contexto).Proveedor(contexto).CuentaDeProveedor(contexto, enumClaseDeCuentaBancaria.Ingreso).IdCuenta;
            else if (pago.Solicitante(contexto).Trabajador(contexto) is not null)
                pago.IdCuentaDeAcreedor = pago.Solicitante(contexto).Trabajador(contexto).CuentaDeTrabajador(contexto, enumClaseDeCuentaBancaria.Ingreso).IdCuenta;
            else
                pago.IdCuentaDeAcreedor = pago.Solicitante(contexto).CuentaDeInterlocutor(contexto, enumClaseDeCuentaBancaria.Ingreso).IdCuenta;

            if (errorSiNoHay && pago.IdCuentaDeAcreedor is null)
                Emitir($"No se le puede asociar cuenta acreedora al pago '{pago.Referencia}' por no tenerla definida el acreedor del pago");
        }

        private static void DevolverAPdtDePagoLaFactura(this PagoDtm pago, ContextoSe contexto)
        {
            if (pago.IdFacturaRec is not null)
            {
                var factura = pago.FacturaRec(contexto);
                if (factura.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_Pagada))
                    factura.TransitarALaEtapa(contexto, enumEtapasDeFacturasRec.FAR_Etapa_De_Pago.EstadosDeLaEtapa(), new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
            }
        }


        public static void TrasPagar(this PagoDtm pago, ContextoSe contexto)
        {
            if (pago.IdFacturaRec is not null)
            {
                var factura = pago.FacturaRec(contexto);
                var pagosConfirmados = factura.PagosConfirmados(contexto).Sum(p => p.Importe);
                var quienMeRestifica = factura.QuienMeRectifica(contexto);
                decimal rectificado = 0;
                decimal yaDevuelto = 0;
                if (quienMeRestifica is not null)
                {
                    rectificado = Math.Abs(quienMeRestifica.TotalDelPago);
                    yaDevuelto = Math.Abs(factura.ImportesDevueltosConfirmados(contexto));
                }
                var saldoPendienteDePago = factura.TotalDelPago - pagosConfirmados - rectificado + yaDevuelto;
                if (factura.Etapas().Contains(enumEtapasDeFacturasRec.FAR_Etapa_De_Pago) && Math.Abs(saldoPendienteDePago) < VariableDeFacturasRec.ToleranciaEnImportes())
                    factura.TransitarALaEtapa(contexto, enumEtapasDeFacturasRec.FAR_Etapa_Pagada.EstadosDeLaEtapa(), new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
            }
        }

        public static void TrasCancelarPago(this PagoDtm pago, ContextoSe contexto)
        {
            if (pago.EsAbono(contexto))
            {
                var factura = pago.FacturaAbonada(contexto);
                if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Abonada))
                    factura.TransitarALaEtapa(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.EstadosDeLaEtapa());
            }
        }

        public static void TrasDevolverPago(this PagoDtm pago, ContextoSe contexto)
        =>
        pago.DevolverAPdtDePagoLaFactura(contexto);

        public static void ValidarFactura(this PagoDtm pago, ContextoSe contexto)
        {
            if (pago.IdFacturaRec is null) return;

            var factura = pago.FacturaRec(contexto);
            if (factura.Cg(contexto).IdSociedad != pago.Cg(contexto).IdSociedad)
                Emitir($"El pago '{pago.Referencia}' no pertenece a la misma sociedad que la factura que se quiere pagar '{factura.Referencia}'");

            if (factura.IdProveedor != pago.IdProveedor)
                Emitir($"El pago ha ser al proveedor que la factura '{factura.Referencia}'");

            var bi = factura.Total(contexto, Enumerados.enumImporteFar.BaseImponible);
            if (bi == 0)
                Emitir($"El pago '{pago.Referencia}' de la '{factura.Referencia}' no tiene BI, no puede crear el pago");
            if (pago.Importe == 0)
                Emitir($"El pago '{pago.Referencia}' de la '{factura.Referencia}' ha de ser distinto de cero");
            var pendiente = factura.TotalDelPago - factura.PagosRealizados(contexto).Sum(x => x.Importe);

            var nuevoPendiente = pendiente - pago.Importe;
            if (pendiente > 0 && nuevoPendiente < 0 && Math.Abs(nuevoPendiente) > VariableDeFacturasRec.ToleranciaEnImportes())
            {
                Emitir($"El pago '{pago.Referencia}' es de '{pago.Importe.ToMoneda()}', hay pendiente '{pendiente.ToMoneda()}' de pagar, no puede crear el pago");
            }

            if (pendiente < 0 && nuevoPendiente > 0 && Math.Abs(nuevoPendiente) > VariableDeFacturasRec.ToleranciaEnImportes())
            {
                Emitir($"La devolución '{pago.Referencia}' es de '{pago.Importe.ToMoneda()}', hay pendiente '{pendiente.ToMoneda()}' por que se nos devuelva, no puede crear la devolución");
            }


            var pagos = factura.Vinculados<PagoDtm>(contexto);
            decimal importeRealizado = 0;
            foreach (var pagoFar in pagos)
            {
                if (pagoFar.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePagos> { enumEtapasDePagos.PAG_Etapa_Cancelado, enumEtapasDePagos.PAG_Etapa_Anulacion }))
                    continue;
                importeRealizado = importeRealizado + pagoFar.Importe;
            }

            if (factura.TotalDelPago > 0)
            {
                nuevoPendiente = factura.TotalDelPago - (importeRealizado + pago.Importe);
                if (nuevoPendiente < 0 && Math.Abs(nuevoPendiente) > VariableDeFacturasRec.ToleranciaEnImportes())
                    Emitir($"El importe del pago '{pago.Referencia}' no puede ser mayor que el pendiente de la factura '{factura.Referencia}', el valor máximo del pago es de '{(factura.TotalDelPago - importeRealizado).ToMoneda()}'");
            }
            else
            {
                nuevoPendiente = factura.TotalDelPago - (importeRealizado + pago.Importe);
                if (nuevoPendiente > 0 && Math.Abs(nuevoPendiente) > VariableDeFacturasRec.ToleranciaEnImportes())
                    Emitir($"El importe del abono '{pago.Referencia}' no puede ser mayor que lo pendiente de la factura '{factura.Referencia}', el valor máximo del abono es de '{(factura.TotalDelPago - importeRealizado).ToMoneda()}'");
            }
        }

        public static FacturaRecDtm FacturaRec(this PagoDtm pago, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (pago.FacturaRec != null && pago.IdFacturaRec == pago.FacturaRec.Id)
                return pago.FacturaRec;

            if (pago.IdFacturaRec is null && errorSiNoHay) Emitir($"el pago '{pago.Referencia}' no está asociado a una factura");
            if (pago.IdFacturaRec is null && !errorSiNoHay) return null;

            pago.FacturaRec = contexto.SeleccionarPorId<FacturaRecDtm>((int)pago.IdFacturaRec);

            return pago.FacturaRec;
        }

        public static TrabajadorDtm Trabajador(this PagoDtm pago, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (pago.Trabajador != null && pago.IdTrabajador == pago.Trabajador.Id)
                return pago.Trabajador;

            if (pago.IdTrabajador is null && errorSiNoHay) Emitir($"el pago '{pago.Referencia}' no está asociado a un trabajador");
            if (pago.IdTrabajador is null && !errorSiNoHay) return null;

            pago.Trabajador = contexto.SeleccionarPorId<TrabajadorDtm>((int)pago.IdTrabajador);

            return pago.Trabajador;
        }

        public static ProveedorDtm Proveedor(this PagoDtm pago, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (pago.Proveedor != null && pago.IdProveedor == pago.Proveedor.Id)
                return pago.Proveedor;

            if (pago.IdProveedor is null && errorSiNoHay) 
                Emitir($"el pago '{pago.Referencia}' no está asociado a un proveedor");
            if (pago.IdProveedor is null && !errorSiNoHay) return null;

            pago.Proveedor = pago.Solicitante(contexto).Proveedor(contexto);

            return pago.Proveedor;
        }

        public static NaturalezaDtm Naturaleza(this PagoDtm pago, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (pago.Naturaleza != null && pago.IdNaturaleza == pago.Naturaleza.Id)
                return pago.Naturaleza;

            if (pago.IdNaturaleza is null && errorSiNoHay) Emitir($"El pago '{pago.Referencia}' no tiene indicado la naturaleza contable");
            if (pago.IdNaturaleza is null && !errorSiNoHay) return null;

            pago.Naturaleza = contexto.SeleccionarPorId<NaturalezaDtm>((int)pago.IdNaturaleza);

            return pago.Naturaleza;
        }


        public static ClienteDtm Cliente(this PagoDtm pago, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (pago.Cliente != null && pago.IdCliente == pago.Cliente.Id)
                return pago.Cliente;

            if (pago.IdCliente is null && errorSiNoHay) Emitir($"el pago '{pago.Referencia}' no está asociado a un cliente");
            if (pago.IdCliente is null && !errorSiNoHay) return null;

            pago.Cliente = pago.Solicitante(contexto).Cliente(contexto); 

            return pago.Cliente;
        }

        public static ParametroDeNegocioDtm TipoDePagoPorDefecto(ContextoSe contexto)
        {
            var tipoPagoPorDefecto = enumNegocio.Pago.Parametro(contexto, enumParametrosDePagos.PAG_Tipo_Pago_Contado, errorSiNoHay: false, errorSinValor: false);
            if (tipoPagoPorDefecto is null || tipoPagoPorDefecto.Valor.Entero() == 0)
            {
                var tiposDepago = enumNegocio.Pago.Tipos(contexto).ToList();
                if (tiposDepago.Count() != 1)
                {
                    if (tipoPagoPorDefecto is null) ParametroDeNegocioSql.Crear(enumNegocio.Pago.IdNegocio(), enumParametrosDePagos.PAG_Tipo_Pago_Contado, 0.ToString());
                    Emitir($"Ha de definir el parametro '{enumParametrosDePagos.PAG_Tipo_Pago_Contado}' para poder crear el pago al contado al crear la factura");
                }

                tipoPagoPorDefecto = enumNegocio.Pago.ResetearParametro(contexto, enumParametrosDePagos.PAG_Tipo_Pago_Contado, tiposDepago[0].Id.ToString());
            }

            return tipoPagoPorDefecto;
        }

        public static bool IntentarCerrarPagoDeFactura(this PagoDtm pago, ContextoSe contexto, bool hayJustificante)
        {
            if (!pago.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Pendiente))
            {
                return false;
            }
            var fac = pago.FacturaRec(contexto, errorSiNoHay: false);
            if (fac != null)
            {
                var transitar = false;
                var exigeJustificante = ParametroDeNegocioSql.Parametro(enumNegocio.Pago, enumParametrosDePagos.PAG_Validar_Justificante_De_Pago, emitirError: false, crearParametro: true, valorPorDefecto: Literal.True).Valor.EsTrue();
                transitar = (exigeJustificante && hayJustificante) || !exigeJustificante;
                if (transitar)
                {
                    var facturaSaldadaConUnUnicoPago = !fac.HayDiferenciaEntreLosTotalPagar(pago.Importe);
                    if (facturaSaldadaConUnUnicoPago && (pago.Clase == enumClaseDePago.Contado || (pago.Clase == enumClaseDePago.Transferencia && pago.PagadoEl != null && ((DateTime)pago.PagadoEl).Date <= DateTime.Now.Date)))
                    {
                        if ((pago.PagarEl is not null && pago.PagarEl <= DateTime.Now) || (pago.PagadoEl is not null && pago.PagadoEl <= DateTime.Now))
                            pago.TransitarALaEtapa(contexto, enumEtapasDePagos.PAG_Etapa_Pagado.EstadosDeLaEtapa(), new Dictionary<string, object> {
                                      { ltrParametrosNeg.AccionQueSeEjecuta, ltrDeUnaFacturaRec.Accion_DarPorPagada },
                       }, delSistema: false);
                        return true;
                    }
                }
            }
            return false;
        }

        public static PagoDtm DarPorPagada(this FacturaRecDtm factura, ContextoSe contexto)
        {
            decimal totalDelPago = 0;

            if (factura.EsRectificativa)
            {
                var importeRectificado = factura.TotalDelPago * -1;
                var importeDevuelto = factura.ImportesDePagosConfirmados(contexto) * -1;
                var faltaPorQueMeDevuelvan = importeRectificado - importeDevuelto;
                totalDelPago = faltaPorQueMeDevuelvan * -1;
            }
            else
            {
                var quienMeRectifica = factura.QuienMeRectifica(contexto);
                if (quienMeRectifica != null)
                {
                    var importeRectificado = quienMeRectifica.TotalDelPago * -1;
                    var importeDevuelto = quienMeRectifica.ImportesDePagosConfirmados(contexto) * -1;
                    totalDelPago = factura.TotalDelPago + importeDevuelto - importeRectificado;
                }
                else
                {
                    totalDelPago = factura.TotalDelPago - factura.ImportesDePagosConfirmados(contexto);
                }
            }

            ParametroDeNegocioDtm tipoPagoPorDefecto = TipoDePagoPorDefecto(contexto);

            var pago = new PagoDtm
            {
                IdCg = factura.IdCg,
                IdTipo = tipoPagoPorDefecto.Valor.Entero(),
                Clase = enumClaseDePago.Contado,
                IdProveedor = factura.IdProveedor,
                IdTarjetaDePago = null,
                IdCuentaDePago = null,
                PagadoEl = factura.FacturadaEl,
                PagarEl = factura.FacturadaEl,
                Importe = totalDelPago,
                IdSolicitante = contexto.SeleccionarPorId<ProveedorDtm>(factura.IdProveedor).IdInterlocutor,
                IdFacturaRec = factura.Id
            };

            pago.Nombre = $"Pago la factura '{factura.Numero}'";

            pago.InsertarComoAdministrador(contexto, accionEjecutada: nameof(DarPorPagada));
            pago = pago.Recargar(contexto);
            return pago;
        }

        public static PagoDtm ModificarImporte(this PagoDtm pago, ContextoSe contexto, decimal importe)
        {
            if (pago.Importe == importe) return pago;
            pago.Importe = importe;
            pago = pago.ModificarComoAdministrador(contexto, accionQueSeEjecuta: ltrDeUnaFacturaRec.Accion_ModificarIva);
            if (pago.IdPreasiento.HasValue)
            {
                pago.Preasentar(contexto);
                pago = pago.ModificarComoAdministrador(contexto, accionQueSeEjecuta: ltrDeUnPago.Accion_GenerarPreasiento);
            }
            return pago;
        }

    }
}
