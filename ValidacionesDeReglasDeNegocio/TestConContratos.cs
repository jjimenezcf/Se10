using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Callejero;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Juridico;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Terceros;
using GestoresDeNegocio.TrabajosSometidos;
using Inicializador.ContratosVnt;
using Inicializador.Ventas;
using iText.Commons.Actions.Contexts;
using ModeloDeDto.Juridico;
using ModeloDeDto.MaestrosTecnico;
using ModeloDeDto.Terceros;
using Newtonsoft.Json;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using SistemaDeElementos.Inicializador.Acromur;
using SistemaDeElementos.Inicializador.Datos;
using Utilidades;
using ValidacionesBase;
using static Gestor.Errores.GestorDeErrores;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ValidacionesDeRn
{
    class TestConContratos
    {

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DefinirFlujoContratos()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzContratosVnt.ModeloDeContratosVnt(contexto);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void AsociarArchivadores()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzContratosVnt.InicializarTiposDeContrato(contexto);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void ProbarFlujoDeContratos()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                var modelo = CrearModeloDeDatosYMaestros(contexto);

                //modifico los porcentajes para que falle
                var contrato = CreoUnContratoConPorcentajesDeAvisoIncoherentes(contexto, modelo.tipo.Id);

                //Añado importes e intento adendar importe
                ModificoLosImportesDelContratoHeIntentoAdendarEnLaEtapaInicial(contexto, contrato);

                //Pongo fecha de inicio de contrato y fin, las modifico y veo que se sincroniza con la agenda del contrato//
                //Esto ya no es así, ya que los eventos se incluyenb en la agenda al pasar el contrato a vigente
                PongoFechaDeInicioContrato_ValidoLaAgenda(contexto, contrato);
                PongoFechaDeFinContrato_ValidoLaAgenda(contexto, contrato);

                //Solicitar un aviso antes de la finalización, y validar que se incluye en la agenda, ademas no se ha de poder
                //solicitar aviso sin fecha de fin

                //Someto y ejecuto los trabajos de notificaciones que afectan a los contratos y valido que envian los correos
                TrabajosDeContratos.SometerNotificarPorcentajeDeAvisoSobrepasado(contexto).EjecutarTrabajo(contexto);
                TrabajosDeAgenda.SometerComunicadosDeAgendasDelSistema(contexto).EjecutarTrabajo(contexto);
                AlEnviarElEventoDeAvisoPrevioSeMarcaElFlagDeRecordatorioEnviado(contexto, contrato);

                ModificamosLaFechaDeFinDeContratoYSeModificaLaDeAvisoPrevio(contexto, contrato);
                AnulamosLosMesesDelRecordatorioDeAvisoDeAvisoPrevio(contexto, contrato);
                var numero = DateTime.Now.Ticks;
                var contrato1 = CrearContrato(contexto, modelo.tipo.Id, modelo.cliente, $"Contrato nº: {numero + 1}", seIniciaEn: 0, duracionInicial: 1, prorrogarCada: 0, prorrogas: 0, enumClaseDeProrroga.noProrrogable);
                //TrabajosDeContratos.SometerMotorDeContratos(contexto).EjecutarTrabajo(contexto);
                //contrato1 = contrato1.Recargar(contexto);
                //if (contrato1.Estado(contexto).Nombre != InzContratos.n_ctv_estado_vigente) Emitir("El contrato 1 debería estar vigente tras aplicarle el motor");

                var contrato2 = CrearContrato(contexto, modelo.tipo.Id, modelo.cliente, $"Contrato nº: {numero + 2}", seIniciaEn: 0, duracionInicial: 2, prorrogarCada: 2, prorrogas: 3, enumClaseDeProrroga.tacita);
                var contrato3 = CrearContrato(contexto, modelo.tipo.Id, modelo.cliente, $"Contrato nº: {numero + 3}", seIniciaEn: 0, duracionInicial: 2, prorrogarCada: 2, prorrogas: 4, enumClaseDeProrroga.explicita);
                TrabajosDeContratos.SometerMotorDeContratos(contexto).EjecutarTrabajo(contexto);

                QuitarMesesALaFechaDeFinEInicio(contexto, contrato1, mesesQuitar: 2);
                QuitarMesesALaFechaDeFinEInicio(contexto, contrato2, mesesQuitar: 2);
                QuitarMesesALaFechaDeFinEInicio(contexto, contrato3, mesesQuitar: 2);

                AsociarSaldos(contexto, contrato2, 20000);

                //TrabajosDeContratos.SometerMotorDeContratos(contexto).EjecutarTrabajo(contexto);
                contrato2 = contrato2.Recargar(contexto);

                contrato2 = contrato2.Transitar(contexto, InzContratosVnt.n_ctv_tran_iniciar);
                var traza = new TrazaDtm();
                traza.IdElemento = contrato2.Id;
                traza.Negocio = enumNegocio.Contrato;
                traza.Nombre = "Inicio del contrato";
                traza.Descripcion = $"El usuario activo el contrato {contrato2.Referencia} el día {DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")}";

                traza.InsertarTraza(contexto);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void SometerNotificacionesDeEventos()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                new EventoDeAgendaDtm
                {
                    Inicio = DateTime.Now.AddDays(0),
                    Fin = DateTime.Now.AddDays(0),
                    Nombre = "hola",
                    AvisarAntesDe = 1,
                    MedidoEn = enumDurabilidad.Dias,
                    IdAgenda = contexto.SeleccionarPorId<UsuarioDtm>(contexto.DatosDeConexion.IdUsuario).IdAgenda,
                    IdNegocio = enumNegocio.Expediente.IdNegocio(),
                    IdElemento = contexto.Set<ExpedienteDtm>().First().Id
                }.Insertar(contexto);
                TrabajosDeAgenda.SometerComunicadosDeAgendasDelSistema(contexto).EjecutarTrabajo(contexto);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void ProbarPlanificadores()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                //crear modelo de datos y maestros
                var modelo = CrearModeloDeDatosYMaestros(contexto);
                TestConMaestros.CrearUnitariosEjemplos(contexto);

                //crear un contrato
                var contrato = CrearContratoEjemplo(contexto, modelo);

                //transitarlo a cancelado
                contrato = contrato.Cancelar(contexto);

                //intentar añadir un planificador sin lote, debe dar errorvar lote = new PlanificadorDeVentaDtm
                var planificador = new PlanificadorDeVentaDtm
                {
                    Nombre = $"Planificador del contrato {contrato.Referencia} 1",
                    IdContrato = contrato.Id,
                    Inicio = DateTime.Now.AddDays(-1),
                    Hasta = contrato.Ampliacion<DatosDelContratoDtm>(contexto).FinContrato.Fecha(),
                    IdCgDeLaPlanificacion = contrato.IdCg,
                    IdTipoDePlanificacion = contexto.SeleccionarPorNombre<TipoDePlanificacionDeVentaDtm>(InzPlanificacionesDeVenta.n_plg_tipo_general).Id,
                    IdTipoDeParte = contexto.SeleccionarPorNombre<TipoDeParteTrDtm>(InzPartesTr.n_ptr_tipo_general).Id,
                    RepetirCada = 1,
                    Periodicidad = enumPeriodicidad.Mensual
                };
                planificador = IntentarCrearPlanificador(contexto, planificador);

                //devolverlo a pendiente de activar
                contrato = contrato.Transitar(contexto, nombreTransicion: InzContratosVnt.n_ctv_tran_reactivar);

                //añadir un planificador de partes de trabajo con fechas anteriores (debe dar error)
                planificador = IntentarCrearPlanificador(contexto, planificador);

                //añadir un planificador en la ventana de tiempo del contrato
                planificador.Inicio = planificador.Inicio.AddDays(2);
                planificador = planificador.Insertar(contexto);
                UnitarioDtm s1 = contexto.SeleccionarPorNombre<UnitarioDtm>("servicio 1");

                //añadir servicios al planificador
                var linea = new LineaDeUnPlfVentaDtm
                {
                    IdElemento = planificador.Id,
                    Orden = 10,
                    IdUnitario = s1.Id,
                    Cantidad = 10,
                    Venta = 500,
                    IdIvaR = contexto.SeleccionarPorPropiedad<IvaRepercutidoDtm>(nameof(IvaRepercutidoDtm.Clase), "IRG").Id
                }.Insertar(contexto);

                //generar planificaciones
                AsociarSaldos(contexto, contrato, 3000000);
                contrato = contrato.Recargar(contexto);
                contrato = contrato.Transitar(contexto, nombreTransicion: InzContratosVnt.n_ctv_tran_iniciar);
                planificador = planificador.GenerarPlanificaciones(contexto);

                //añadir otro planificador y generar los partes y sus planificaciones
                planificador.Nombre = planificador.Nombre.Replace("1", "2");
                planificador = planificador.Insertar(contexto);
                linea.IdElemento = planificador.Id;
                linea.Insertar(contexto);
                var contratos = GestorDeContratos.ValidarExistenPlanificadoresPendientes(contexto, new List<ClausulaDeFiltrado> {
                new ClausulaDeFiltrado(nameof(enumClaseDeContrato.Venta), enumCriteriosDeFiltrado.igual, enumClaseDeContrato.Venta)
                });
                if (contratos.Where(c => c.FechaCreacion.Date == DateTime.Now.Date).Count() != 1)
                    Emitir("Debería haber un contrato vigente con un planificador pendiente");
                Dictionary<string, object> parametros = new Dictionary<string, object>
                {
                    { nameof(FechasDeGeneracionDto.FechaDesde), DateTime.Now },
                    { nameof(FechasDeGeneracionDto.FechaHasta), planificador.Hasta }
                };
                List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(parametros.LeerValor(ltrParametrosEp.Filtro, "[]"));
                GestorDeContratos.PrepararPartesDeTrabajo(contexto, filtros, parametros.LeerValor<DateTime>(nameof(FechasDeGeneracionDto.FechaDesde)), parametros.LeerValor<DateTime>(nameof(FechasDeGeneracionDto.FechaHasta)));
                var partes = contexto.SeleccionarTodos<ParteTrDtm>(nameof(ParteTrDtm.IdContrato), contrato.Id);
                if (partes.Count() == 0) Emitir("Debería haber algún parte generado");
                partes[0].Transitar(contexto, InzPartesTr.n_tran_ptr_gen_realizar);
                partes[1].Transitar(contexto, InzPartesTr.n_tran_ptr_gen_realizar);
                filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(parametros.LeerValor(ltrParametrosEp.Filtro, "[]"));

                //parametrizar los tipos para poder facturar un contrato
                var tipoDeContrato = contexto.SeleccionarPorId<TipoDeContratoDtm>(contrato.IdTipo);
                var tipoDeFactura = contexto.SeleccionarPorNombre<TipoDeFacturaEmtDtm>(InzFacturasEmt.n_fae_tipo_general);
                tipoDeContrato.IdTipoFacturaEmt = tipoDeFactura.Id;
                tipoDeContrato.Modificar(contexto);
                tipoDeFactura.IdUnidadDefecto = contexto.SeleccionarPorPropiedad<UnidadDtm>(nameof(UnidadDtm.Sigla), InzMaestros.n_unidad_ud).Id;
                tipoDeFactura.IdNaturalezaDefecto = contexto.SeleccionarPorPropiedad<NaturalezaDtm>(nameof(NaturalezaDtm.Sigla), InzMaestros.n_natu_consultoria).Id;
                tipoDeFactura.IdIvaRDefecto = contexto.SeleccionarPorNombre<IvaRepercutidoDtm>(InzMaestros.n_iva_general).Id;
                tipoDeFactura.Modificar(contexto);

                var calles = GestorDeCalles.Gestor(contexto, contexto.Mapeador).LeerRegistros(0, 1, new List<ClausulaDeFiltrado>(), parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, true));
                DireccionDtm direccion = calles[0].CrearDireccion(enumCalificadorDireccion.fiscal);
                contrato.Ampliacion<DatosDelContratoDtm>(contexto, aplicarJoin: true).Cliente.AsignarDireccion(contexto, direccion);

                //facturar los partes de un contrato
                GestorDeContratos.PrefacturarContratos(contexto, filtros, parametros.LeerValor<DateTime>(nameof(FechasDeGeneracionDto.FechaDesde)), parametros.LeerValor<DateTime>(nameof(FechasDeGeneracionDto.FechaHasta)));
                var prefacturas = contexto.SeleccionarTodos<FacturaEmtDtm>(nameof(FacturaEmtDtm.IdContrato), contrato.Id);
                if (prefacturas.Count() != 1) Emitir("Debería haber 1 prefacturas");
                if (prefacturas[0].Detalles<LineaDeUnaFaeDtm>(contexto).Count() != 2) Emitir("Debería haber 2 partes prefacturados");
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        private static PlanificadorDeVentaDtm IntentarCrearPlanificador(ContextoSe contexto, PlanificadorDeVentaDtm planificador)
        {
            try
            {
                planificador.Insertar(contexto);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("un contrato terminado o cancelado") ||
                    e.Message.Contains("de la planificación ha de ser mayor o igual que la fecha de inicial"))
                    return planificador;
                throw;
            }

            return planificador;
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void ProbarLotes()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                //crear modelo de datos y maestros
                var modelo = CrearModeloDeDatosYMaestros(contexto);

                //crear unitarios
                TestConMaestros.CrearUnitariosEjemplos(contexto);

                var contrato = CrearContratoEjemplo(contexto, modelo);
                contrato = contrato.Cancelar(contexto);

                //intentar añadir un lote, debe dar error
                var lote = new LoteDeUnContratoDtm
                {
                    Nombre = $"Lote del contrato {contrato.Referencia}",
                    IdContrato = contrato.Id,
                    VigenteDesde = DateTime.Now.AddDays(-1),
                    VigenteHasta = contrato.Ampliacion<DatosDelContratoDtm>(contexto).FinContrato.Fecha()
                };
                try
                {
                    lote.Insertar(contexto);
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("un contrato terminado o cancelado"))
                        throw;
                }

                //devolverlo a pendiente de activar
                contrato = contrato.Transitar(contexto, nombreTransicion: InzContratosVnt.n_ctv_tran_reactivar);

                //añadir un lote con fechas anteriores y posteriores al trabajo (debe dar error)
                try
                {
                    lote.Insertar(contexto);
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("no puede iniciarse"))
                        throw;
                }

                //añadir un lote en la ventana de tiempo del contrato
                lote.VigenteDesde = ((DateTime)lote.VigenteDesde).AddDays(2);
                lote = lote.Insertar(contexto);

                //añadir servicios al lote
                var p = new Dictionary<string, object> { { ltrParametrosNeg.Peticion, enumPeticion.epCrearRelaciones } };
                new UnitariosDeUnLoteDto { IdLote = lote.Id, IdUnitario = contexto.SeleccionarPorNombre<UnitarioDtm>("servicio 1").Id }.InsertarDto(contexto, p);
                new UnitariosDeUnLoteDto { IdLote = lote.Id, IdUnitario = contexto.SeleccionarPorNombre<UnitarioDtm>("servicio 2").Id }.InsertarDto(contexto, p);
                new UnitariosDeUnLoteDto { IdLote = lote.Id, IdUnitario = contexto.SeleccionarPorNombre<UnitarioDtm>("material 1").Id }.InsertarDto(contexto, p);
                new UnitariosDeUnLoteDto { IdLote = lote.Id, IdUnitario = contexto.SeleccionarPorNombre<UnitarioDtm>("material 2").Id }.InsertarDto(contexto, p);

                //crear un planificador basándome en el lote
                var planificador = new PlanificadorDeVentaDtm
                {
                    Nombre = $"Planificador del contrato {contrato.Referencia}",
                    IdContrato = contrato.Id,
                    Inicio = DateTime.Now.AddDays(1),
                    Hasta = contrato.Ampliacion<DatosDelContratoDtm>(contexto).FinContrato.Fecha(),
                    IdCgDeLaPlanificacion = contrato.IdCg,
                    IdLote = lote.Id,
                    IdTipoDePlanificacion = contexto.SeleccionarPorNombre<TipoDePlanificacionDeVentaDtm>(InzPlanificacionesDeVenta.n_plg_tipo_general).Id,
                    IdTipoDeParte = contexto.SeleccionarPorNombre<TipoDeParteTrDtm>(InzPartesTr.n_ptr_tipo_general).Id,
                    RepetirCada = 1,
                    Periodicidad = enumPeriodicidad.Mensual
                }.Insertar(contexto);

                //añadir servicios al planificador
                var linea = DefinirLineaDePlanificador(planificador, contexto, contexto.ElementoPorNombre<UnitarioDto, UnitarioDtm>("servicio 3"), 10);
                //añadir servicios al planificador que no están en el lote, debe dar error
                try
                {
                    linea.Insertar(contexto);
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("No hay ningún elemento"))
                        throw;
                }

                //añadir servicios del lote al planificador
                linea.IdUnitario = contexto.SeleccionarPorNombre<UnitarioDtm>("servicio 1").Id;
                linea.Insertar(contexto);
                DefinirLineaDePlanificador(planificador, contexto, contexto.ElementoPorNombre<UnitarioDto, UnitarioDtm>("servicio 1"), 20).Insertar(contexto);
                DefinirLineaDePlanificador(planificador, contexto, contexto.ElementoPorNombre<UnitarioDto, UnitarioDtm>("material 1"), 30).Insertar(contexto);
                linea = DefinirLineaDePlanificador(planificador, contexto, contexto.ElementoPorNombre<UnitarioDto, UnitarioDtm>("material 2"), 40).Insertar(contexto);
                linea.Orden = 50;
                linea.Modificar(contexto);
                //generar planificaciones
                AsociarSaldos(contexto, contrato, 2000000);
                contrato = contrato.Recargar(contexto);
                contrato = contrato.Transitar(contexto, nombreTransicion: InzContratosVnt.n_ctv_tran_iniciar);
                planificador = planificador.GenerarPlanificaciones(contexto);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        public static ContratoDtm CrearContrato(ContextoSe contexto, int idTipo, ClienteDtm cliente, string nombre, int seIniciaEn, int duracionInicial, int prorrogarCada, int prorrogas, enumClaseDeProrroga clase)
        {
            DatosDelContratoDtm datos;

            var contrato = enumNegocio.Contrato.SeleccionarPorPropiedad<ContratoDtm>(contexto, nameof(INombre.Nombre), nombre, errorSiNoHay: false);
            if (contrato == null)
            {
                contrato = new ContratoDtm
                {
                    IdCg = contexto.SeleccionarDtoPorAk<CentroGestorDto, CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), InzAcromur.n_cg_acm_codigo).Id,
                    IdTipo = idTipo,
                    Nombre = nombre,
                    IdResponsable = contexto.DatosDeConexion.IdUsuario
                }.Insertar(contexto);
            }

            datos = AsociarCliente(contexto, cliente, seIniciaEn, duracionInicial, contrato);
            AsociarProrroga(contexto, prorrogarCada, prorrogas, clase, datos, contrato);

            return contrato;
        }

        internal static LineaDeUnPlfVentaDtm DefinirLineaDePlanificador(PlanificadorDeVentaDtm planificador, ContextoSe contexto, UnitarioDto s3, int posicion)
        {
            return new LineaDeUnPlfVentaDtm
            {
                IdElemento = planificador.Id,
                Orden = posicion,
                IdUnitario = s3.Id,
                Cantidad = 10,
                Venta = s3.Venta,
                IdIvaR = contexto.SeleccionarPorPropiedad<IvaRepercutidoDtm>(nameof(IvaRepercutidoDtm.Clase), "IRG").Id
            };
        }

        internal static (TipoDeElementoDtm tipo, ClienteDtm cliente) CrearModeloDeDatosYMaestros(ContextoSe contexto)
        {
            TestConMaestros.CrearMaestros(contexto);
            InzContratosVnt.ModeloDeContratosVnt(contexto);
            InzPlanificacionesDeVenta.ModeloDePlanificacionesDeVenta(contexto);
            InzPartesTr.ModeloDePartesTr(contexto);

            var sociedadDelSistema = InzAcromur.Sociedad(contexto);
            var tipoArchivador = GestorDeTiposDeArchivadores.PersistirTipo(contexto, ltrTipoArchivador.TipoClientes, enumClaseDeLibro.POR_CG_TIPO, "CLI", visible: true, delSistema: false, nombreModificable: false);
            var cgCliente = ExtensionCentrosGestores.Cfg_CG_De_ClienteWeb(contexto, sociedadDelSistema.Id);

            var parametroTipoArchivador = enumNegocio.Cliente.ResetearParametro(contexto, enumParametrosDeCliente.CLI_TipoArchivador, valor: tipoArchivador.Nombre);
            var parametroCgCliente = enumNegocio.Cliente.ResetearParametro(contexto, enumParametrosDeCliente.CLI_CG_De_Cliente, valor: cgCliente.Codigo);

            var tipo = contexto.Tipos<TipoDeContratoDtm>().Where(x => x.Nombre.Equals(InzContratosVnt.n_ctv_tipo_venta)).First();
            var idTipo = tipo.Id;
            var cliente1 = InzContratosVnt.PepeFuster.CrearCliente(contexto);
            var cliente2 = InzContratosVnt.AbioPep.CrearCliente(contexto);
            var cliente3 = InzContratosVnt.AbanaProyectos.CrearCliente(contexto);
            var cliente4 = InzContratosVnt.NuevoVertice.CrearCliente(contexto);

            return (tipo, cliente1);
        }

        internal static SaldosDelContratoDtm AsociarSaldos(ContextoSe contexto, ContratoDtm contrato, decimal importe)
        {
            SaldosDelContratoDtm saldos = contrato.Ampliacion<SaldosDelContratoDtm>(contexto);
            saldos.Importe = importe;
            saldos.Modificar(contexto);
            return saldos;
        }

        private static ContratoDtm CrearContratoEjemplo(ContextoSe contexto, (TipoDeElementoDtm tipo, ClienteDtm cliente) modelo)
        {
            return CrearContrato(contexto,
                 contexto.Tipos<TipoDeContratoDtm>().Where(x => x.Nombre.Equals(InzContratosVnt.n_ctv_tipo_venta)).First().Id,
                 contexto.SeleccionarPorId<ClienteDtm>(modelo.cliente.Id),
                 $"Contrato nº: {DateTime.Now.Ticks + 1}",
                 seIniciaEn: 0,
                 duracionInicial: 5,
                 prorrogarCada: 0,
                 prorrogas: 0,
                 enumClaseDeProrroga.noProrrogable);
        }

        private static void QuitarMesesALaFechaDeFinEInicio(ContextoSe contexto, ContratoDtm contrato, int mesesQuitar)
        {
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto);
            datos.InicioContrato = datos.InicioContrato.AddMonths(-mesesQuitar);
            datos.FinContrato = datos.FinContrato.Fecha().AddMonths(-mesesQuitar);
            contexto.Update(datos);
            contexto.SaveChanges();
            datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto, usarLaCache: false);
        }

        private static ProrrogaDtm AsociarProrroga(ContextoSe contexto, int prorrogarCada, int prorrogas, enumClaseDeProrroga clase, DatosDelContratoDtm datos, ContratoDtm contrato)
        {
            ProrrogaDtm prorroga = contrato.Ampliacion<ProrrogaDtm>(contexto);
            prorroga.ClaseDeProrroga = clase;
            if (clase != enumClaseDeProrroga.noProrrogable)
            {
                prorroga.Meses = prorrogarCada;
                prorroga.FechaUltimaProrroga = datos.FinContrato.Fecha().AddMonths(prorrogarCada * prorrogas);
            }
            prorroga.Modificar(contexto);
            return prorroga;
        }

        private static DatosDelContratoDtm AsociarCliente(ContextoSe contexto, ClienteDtm cliente, int seIniciaEn, int duracionInicial, ContratoDtm contrato)
        {
            DatosDelContratoDtm datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto);
            datos.InicioContrato = DateTime.Now.AddDays(seIniciaEn);
            datos.FinContrato = datos.InicioContrato.AddMonths(duracionInicial);
            datos.IdCliente = cliente.Id;
            datos.Modificar(contexto);
            return datos;
        }

        private static void AlEnviarElEventoDeAvisoPrevioSeMarcaElFlagDeRecordatorioEnviado(ContextoSe contexto, ContratoDtm contrato)
        {
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto);
            datos.AvisarAntesDe = 3;
            datos = datos.Modificar(contexto);
            var eventos = contexto.SeleccionarEventos(contrato.IdAgenda, contrato.Id, ltrDatosDelContrato.AvisoPrevioDeFinDeContrato);
            contexto.SeleccionarPorId<ContratoDtm>(eventos[0].IdElemento).EventoNotificado(contexto, eventos[0]);
            datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto);
            if (!datos.RecordatorioEnviado.Cierto())
                Emitir("Debería haberse marcado el recordatorio de fin de contrato");
        }

        private static void ModificamosLaFechaDeFinDeContratoYSeModificaLaDeAvisoPrevio(ContextoSe contexto, ContratoDtm contrato)
        {
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto);
            var finactual = (DateTime)datos.FinContrato;
            datos.FinContrato = null;
            try
            {
                datos.Modificar(contexto);
            }
            catch (Exception e)
            {
                if (e.Message.IndexOf("Ha indicado que se avise") == -1)
                    throw;
            }
            datos.FinContrato = finactual.AddDays(-2);
            datos = datos.Modificar(contexto);
            var eventos = contexto.SeleccionarEventos(contrato.IdAgenda, contrato.Id, ltrDatosDelContrato.AvisoPrevioDeFinDeContrato);
            EventoDeAgendaDtm elEventoBuscado = default;
            foreach (var evento in eventos)
                if (datos.FechaDeAvisoPrevio == evento.Inicio)
                {
                    elEventoBuscado = evento;
                    break;
                }
            if (elEventoBuscado == default)
                Emitir($"debería haber algún evento pendiente que coincidiera con la del aviso previo {datos.FechaDeAvisoPrevio}");

            datos.FinContrato = ((DateTime)datos.FinContrato).AddDays(-2);
            datos = datos.Modificar(contexto);
            if (datos.FechaDeAvisoPrevio != elEventoBuscado.Recargar(contexto).Inicio)
                Emitir($"El evento buscado tiene esta fecha {elEventoBuscado.Inicio} y debería tener {datos.FechaDeAvisoPrevio}");

        }

        private static void AnulamosLosMesesDelRecordatorioDeAvisoDeAvisoPrevio(ContextoSe contexto, ContratoDtm contrato)
        {
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto);
            datos.AvisarAntesDe = 0;
            datos.Modificar(contexto);
            var eventos = contexto.SeleccionarEventos(contrato.IdAgenda, contrato.Id, ltrDatosDelContrato.AvisoPrevioDeFinDeContrato);
            if (eventos.Count > 0)
                Emitir($"la no debería de haber eventos de aviso previo sin enviar");
        }

        private static void PongoFechaDeFinContrato_ValidoLaAgenda(ContextoSe contexto, ContratoDtm contrato)
        {
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto);
            datos.FinContrato = datos.InicioContrato.AddYears(1);
            datos.Modificar(contexto);
            //var desde = datos.InicioContrato;
            //var hasta = DateTime.Now.AddYears(1).AddDays(-1);
            //var eventos = contexto.SeleccionarEventos(contrato.IdAgenda, datos.IdElemento, ltrDatosDelContrato.ContratoSinFechaDeFin, hasta);
            //if (eventos.Count > 0)
            //    Emitir("No debería haber encontrado el evento de falta fin de contratos");
            //eventos = contexto.SeleccionarEventos(contrato.IdAgenda, datos.IdElemento, ltrDatosDelContrato.AvisoDeFinDeContrato, desde);
            //if (eventos.Count == 0)
            //    Emitir("Debería haber encontrado el evento de fin de contratos");
            //if (eventos[0].Fin.Date != ((DateTime)datos.FinContrato).Date)
            //    Emitir($"El aviso debería ser con fecha {((DateTime)datos.FinContrato).Date}");
        }

        private static void PongoFechaDeInicioContrato_ValidoLaAgenda(ContextoSe contexto, ContratoDtm contrato)
        {
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto);
            datos.InicioContrato = DateTime.Now.AddDays(1);
            datos.Modificar(contexto);
            //var desde = datos.InicioContrato;
            //var hasta = DateTime.Now.AddYears(1).AddDays(-1);
            //var eventos = contexto.SeleccionarEventos(contrato.IdAgenda, datos.IdElemento, ltrDatosDelContrato.ContratoSinFechaDeFin, desde, hasta);
            //if (eventos.Count > 0)
            //    Emitir("El evento debe estar un día después");
            //eventos = contexto.SeleccionarEventos(contrato.IdAgenda, datos.IdElemento, ltrDatosDelContrato.ContratoSinFechaDeFin, hasta);
            //if (eventos.Count == 0)
            //    Emitir("Debería haber encontrado el evento de falta fin de contrato");
        }

        private static ContratoDtm CreoUnContratoConPorcentajesDeAvisoIncoherentes(ContextoSe contexto, int idTipo)
        {
            var contrato = new ContratoDtm
            {
                IdCg = contexto.SeleccionarDtoPorAk<CentroGestorDto, CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), InzAcromur.n_cg_acm_codigo).Id,
                IdTipo = idTipo,
                Nombre = $"Mi primer contrato: {DateTime.Now.Ticks}",
                IdResponsable = contexto.DatosDeConexion.IdUsuario
            };
            contrato.Insertar(contexto);
            var saldos = contrato.Ampliacion<SaldosDelContratoDtm>(contexto);
            saldos.Bloqueo = (decimal)40.00;
            try
            {
                saldos.Modificar(contexto);
            }
            catch (Exception exc)
            {
                if (exc.Message.IndexOf("El porcentaje de aviso") == -1)
                    throw;
            }
            return contrato;
        }

        private static void ModificoLosImportesDelContratoHeIntentoAdendarEnLaEtapaInicial(ContextoSe contexto, ContratoDtm contrato)
        {
            var saldos = contrato.Ampliacion<SaldosDelContratoDtm>(contexto);
            saldos.Importe = 10000;
            saldos = saldos.Modificar(contexto);
            saldos.Adendado = 100;
            ActualizarSaldosConError(contexto, saldos);
        }

        private static void ActualizarSaldosConError(ContextoSe contexto, SaldosDelContratoDtm saldos)
        {
            var contrato = saldos.Elemento<ContratoDtm>(contexto);
            try
            {
                contrato.Modificar(contexto);
            }
            catch (Exception exc)
            {
                if (exc.Message.IndexOf("No se puede adendar si el contrato no está vigente") == -1)
                    throw;
            }
        }
    }
}
