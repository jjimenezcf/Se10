using GestoresDeNegocio.Expediente;
using GestoresDeNegocio.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Entorno;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using Inicializador.SistemaDocumental;

namespace Inicializador.Expedientes
{
    public static class InzExpedientesDeFacturacion
    {

        #region definición de textos del flujo
        public static readonly string n_fct = "FCT";

        public static readonly string n_estado_fct_enero = $"{n_fct}: Enero";
        public static readonly string n_estado_fct_febrero = $"{n_fct}: Febrero";
        public static readonly string n_estado_fct_marzo = $"{n_fct}: Marzo";
        public static readonly string n_estado_fct_abril = $"{n_fct}: Abril";
        public static readonly string n_estado_fct_mayo = $"{n_fct}: Mayo";
        public static readonly string n_estado_fct_junio = $"{n_fct}: Junio";
        public static readonly string n_estado_fct_julio = $"{n_fct}: Julio";
        public static readonly string n_estado_fct_agosto = $"{n_fct}: Agosto";
        public static readonly string n_estado_fct_septiembre = $"{n_fct}: Septiembre";
        public static readonly string n_estado_fct_octubre = $"{n_fct}: Octubre";
        public static readonly string n_estado_fct_noviembre = $"{n_fct}: Noviembre";
        public static readonly string n_estado_fct_diciembre = $"{n_fct}: Diciembre";
        public static readonly string n_estado_fct_ejercicio_cerrado = $"{n_fct}: Ejercicio cerrado";
        public static readonly string n_estado_fct_terminado = $"{n_fct}: Terminado";
        public static readonly string n_estado_fct_cancelado = $"{n_fct}: Cancelado";


        public static readonly string n_exp_tipo_expediente_facturas = $"{n_fct}: Facturación";

        public static readonly string n_tran_fct_cancelar = $"{n_fct}: Cancelar";
        public static readonly string n_tran_fct_terminar = $"{n_fct}: Terminar";

        public static readonly string n_tran_fct_cerrar_enero = $"{n_fct}: Cerrar enero";
        public static readonly string n_tran_fct_abrir_enero = $"{n_fct}: Reabrir enero";

        public static readonly string n_tran_fct_cerrar_febrero = $"{n_fct}: Cerrar febrero";
        public static readonly string n_tran_fct_abrir_febrero = $"{n_fct}: Reabrir febrero";

        public static readonly string n_tran_fct_cerrar_marzo = $"{n_fct}: Cerrar marzo";
        public static readonly string n_tran_fct_abrir_marzo = $"{n_fct}: Reabrir marzo";

        public static readonly string n_tran_fct_cerrar_abril = $"{n_fct}: Cerrar abril";
        public static readonly string n_tran_fct_abrir_abril = $"{n_fct}: Reabrir abril";

        public static readonly string n_tran_fct_cerrar_mayo = $"{n_fct}: Cerrar mayo";
        public static readonly string n_tran_fct_abrir_mayo = $"{n_fct}: Reabrir mayo";

        public static readonly string n_tran_fct_cerrar_junio = $"{n_fct}: Cerrar junio";
        public static readonly string n_tran_fct_abrir_junio = $"{n_fct}: Reabrir junio";

        public static readonly string n_tran_fct_cerrar_julio = $"{n_fct}: Cerrar julio";
        public static readonly string n_tran_fct_abrir_julio = $"{n_fct}: Reabrir julio";

        public static readonly string n_tran_fct_cerrar_agosto = $"{n_fct}: Cerrar agosto";
        public static readonly string n_tran_fct_abrir_agosto = $"{n_fct}: Reabrir agosto";

        public static readonly string n_tran_fct_cerrar_septiembre = $"{n_fct}: Cerrar septiembre";
        public static readonly string n_tran_fct_abrir_septiembre = $"{n_fct}: Reabrir septiembre";

        public static readonly string n_tran_fct_cerrar_octubre = $"{n_fct}: Cerrar octubre";
        public static readonly string n_tran_fct_abrir_octubre = $"{n_fct}: Reabrir octubre";

        public static readonly string n_tran_fct_cerrar_noviembre = $"{n_fct}: Cerrar noviembre";
        public static readonly string n_tran_fct_abrir_noviembre = $"{n_fct}: Reabrir noviembre";

        public static readonly string n_tran_fct_cerrar_ejercicio = $"{n_fct}: Cerrar ejercicio";
        public static readonly string n_tran_fct_abrir_ejercicio = $"{n_fct}: Reabrir diciembre";
        #endregion

        private static readonly string parametroDeTransicion = @"[{""parametro"": ""p_2"",""valor"": @p_2 },{""parametro"": ""p_3"",""valor"": @p_3 }]"
        .Replace("p_2", AccionesDeFacturacion.enumParametros.IdTipoArchivador.ToString())
        .Replace("p_3", AccionesDeFacturacion.enumParametros.Mes.ToString());

        private static readonly string parametroDeNegocio =
        @"[{""parametro"": ""p_1"",""valor"": @p_1 },{""parametro"": ""p_2"",""valor"": @p_2 },{""parametro"": ""p_3"",""valor"": @p_3 }]"
        .Replace("p_1", AccionesDeFacturacion.enumParametros.IdTipoExpediente.ToString())
        .Replace("p_2", AccionesDeFacturacion.enumParametros.IdTipoArchivador.ToString())
        .Replace("p_3", AccionesDeFacturacion.enumParametros.Mes.ToString());

        public static void ProcesoFCT(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                Etapas(contexto);
                Acciones(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public static void Acciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Acciones de las transiciones de tareas de TRR");
            try
            {
                CrearAcciones(contexto);
                AccionesDeNegocio(contexto);
                AccionAlTransitar(contexto);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de expedientes");
            try
            {
                var fct_estadoInicial = enumNegocio.Expediente.Estado(contexto, n_estado_fct_enero);
                GestorDeTiposDeExpediente.PersistirTipo(contexto, enumClaseDeExpediente.DeCliente, n_exp_tipo_expediente_facturas, fct_estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_fct, usaPpts: false, scDeVenta: false, scDeCompra: false, usaDatosJuridicos: false);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void Transiciones(ContextoSe contexto)
        {
            //Enero --> Febrero, Cancelado
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_cerrar_enero, n_estado_fct_enero, n_estado_fct_febrero);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_cancelar, n_estado_fct_enero, n_estado_fct_cancelado, asunto: "Motivo de cancelación");

            //Febrero --> Enero, Marzo, Terminado
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_abrir_enero, n_estado_fct_febrero, n_estado_fct_enero, asunto: "Motivo de reapertura");
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_cerrar_febrero, n_estado_fct_febrero, n_estado_fct_marzo);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_terminar, n_estado_fct_febrero, n_estado_fct_terminado, asunto: "Motivo de terminación");

            //Marzo --> Febrero, Abril, Terminado
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_abrir_febrero, n_estado_fct_marzo, n_estado_fct_febrero, asunto: "Motivo de reapertura");
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_cerrar_marzo, n_estado_fct_marzo, n_estado_fct_abril);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_terminar, n_estado_fct_marzo, n_estado_fct_terminado, asunto: "Motivo de terminación");

            //Abril --> Marzo, Febrero, Terminado
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_abrir_marzo, n_estado_fct_abril, n_estado_fct_marzo, asunto: "Motivo de reapertura");
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_cerrar_abril, n_estado_fct_abril, n_estado_fct_mayo);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_terminar, n_estado_fct_abril, n_estado_fct_terminado, asunto: "Motivo de terminación");

            //Mayo --> 
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_abrir_abril, n_estado_fct_mayo, n_estado_fct_abril, asunto: "Motivo de reapertura");
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_cerrar_mayo, n_estado_fct_mayo, n_estado_fct_junio);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_terminar, n_estado_fct_mayo, n_estado_fct_terminado, asunto: "Motivo de terminación");

            //Junio --> 
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_abrir_mayo, n_estado_fct_junio, n_estado_fct_mayo, asunto: "Motivo de reapertura");
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_cerrar_junio, n_estado_fct_junio, n_estado_fct_julio);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_terminar, n_estado_fct_junio, n_estado_fct_terminado, asunto: "Motivo de terminación");

            //Julio --> 
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_abrir_junio, n_estado_fct_julio, n_estado_fct_junio, asunto: "Motivo de reapertura");
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_cerrar_julio, n_estado_fct_julio, n_estado_fct_agosto);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_terminar, n_estado_fct_julio, n_estado_fct_terminado, asunto: "Motivo de terminación");

            //Agosto --> 
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_abrir_julio, n_estado_fct_agosto, n_estado_fct_julio, asunto: "Motivo de reapertura");
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_cerrar_agosto, n_estado_fct_agosto, n_estado_fct_septiembre);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_terminar, n_estado_fct_agosto, n_estado_fct_terminado, asunto: "Motivo de terminación");

            //Septiembre --> 
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_abrir_agosto, n_estado_fct_septiembre, n_estado_fct_agosto, asunto: "Motivo de reapertura");
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_cerrar_septiembre, n_estado_fct_septiembre, n_estado_fct_octubre);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_terminar, n_estado_fct_septiembre, n_estado_fct_terminado, asunto: "Motivo de terminación");

            //Octubre --> 
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_abrir_septiembre, n_estado_fct_octubre, n_estado_fct_septiembre, asunto: "Motivo de reapertura");
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_cerrar_octubre, n_estado_fct_octubre, n_estado_fct_noviembre);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_terminar, n_estado_fct_octubre, n_estado_fct_terminado, asunto: "Motivo de terminación");

            //Noviembre --> 
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_abrir_octubre, n_estado_fct_noviembre, n_estado_fct_octubre, asunto: "Motivo de reapertura");
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_cerrar_noviembre, n_estado_fct_noviembre, n_estado_fct_diciembre);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_terminar, n_estado_fct_noviembre, n_estado_fct_terminado, asunto: "Motivo de terminación");

            //Diciembre --> 
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_abrir_noviembre, n_estado_fct_diciembre, n_estado_fct_noviembre, asunto: "Motivo de reapertura");
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_cerrar_ejercicio, n_estado_fct_diciembre, n_estado_fct_ejercicio_cerrado);
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_terminar, n_estado_fct_diciembre, n_estado_fct_terminado, asunto: "Motivo de terminación");

            //Reapertura de ejercicio
            GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Expediente, n_tran_fct_abrir_ejercicio, n_estado_fct_ejercicio_cerrado, n_estado_fct_diciembre);

        }

        private static void Estados(ContextoSe contexto)
        {
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_fct_enero, inicial: true, orden: 10);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_fct_febrero, orden: 15);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_fct_marzo, orden: 20);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_fct_abril, orden: 25);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_fct_mayo, orden: 30);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_fct_junio, orden: 35);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_fct_julio, orden: 40);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_fct_agosto, orden: 45);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_fct_septiembre, orden: 50);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_fct_octubre, orden: 55);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_fct_noviembre, orden: 60);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_fct_diciembre, orden: 65);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_fct_ejercicio_cerrado, terminado: true, orden: 70);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_fct_terminado, terminado: true, orden: 75);
            GestorDeEstados.PersistirEstado(contexto, enumNegocio.Expediente, n_estado_fct_cancelado, cancelado: true, orden: 80);
        }

        private static void Etapas(ContextoSe contexto)
        {
            ExtensorDeExpedientes.DefinirEtapasBasicas(contexto);
            var estados = enumNegocio.Expediente.Estados(contexto);
            var iniciales = "";
            var enCurso = "";
            foreach (EstadoDtm estado in estados)
                if (estado.Inicial) iniciales = $"{(iniciales.IsNullOrEmpty() ? estado.Id.ToString() : $"{iniciales},{estado.Id}")}";

            foreach (EstadoDtm estado in estados)
                if (!estado.Inicial && !estado.Terminado && !estado.Cancelado)
                    enCurso = $"{(enCurso.IsNullOrEmpty() ? estado.Id.ToString() : $"{enCurso},{estado.Id}")}";

            var etapaEjecucion = iniciales + "," + enCurso;

            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas, iniciales + "," + enCurso);
            enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Ejecucion, etapaEjecucion);
        }
        private static void CrearAcciones(ContextoSe contexto)
        {
            var a = new AccionDtm();
            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.Expediente)}.{nameof(AccionesDeFacturacion)}";
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();

            #region Cerrar archivador de facturas del mes
            a.Nombre = AccionesDeFacturacion.N_CerrarArchivadorDeFacturasParaElMes;
            a.Descripcion = @$"Cierra el archivador de facturas del mes indicado, si no existe lo crea y lo da de baja.{Environment.NewLine}Parámetros: {parametroDeTransicion}";
            a.Metodo = nameof(AccionesDeFacturacion.CerrarArchivadorDeFacturasParaElMes);
            a.PersistirAccion(contexto);
            #endregion

            #region Abrir archivador de facturas del mes
            a.Nombre = AccionesDeFacturacion.N_AbrirArchivadorDeFacturasParaElMes;
            a.Descripcion = $@"Crea o activa un archivador para poder incluir las facturas del mes indicado.{Environment.NewLine}Parámetros: {parametroDeTransicion}";
            a.Metodo = nameof(AccionesDeFacturacion.AbrirArchivadorDeFacturasParaElMes);
            a.PersistirAccion(contexto);
            #endregion

            #region Abrir expediente de facturas para el año
            a.Nombre = AccionesDeFacturacion.N_AbrirExpedienteDeFacturasParaElEjercicio;
            a.Descripcion = $@"Crea un expediente para poder incluir las facturas del ejercicio y lo enlaza con el que se cierra";
            a.Metodo = nameof(AccionesDeFacturacion.AbrirExpedienteDeFacturasParaElEjercicio);
            a.PersistirAccion(contexto);
            #endregion

        }

        private static void AccionesDeNegocio(ContextoSe contexto)
        {
            var tipoExpediente = contexto.SeleccionarPorNombre<TipoDeExpedienteDtm>(n_exp_tipo_expediente_facturas);
            var tipoArchivador = contexto.SeleccionarPorNombre<TipoDeArchivadorDtm>(InzArchivadoresEcoFin.n_tipo_contable);
            GestorDeAccionesDeNegocio.CrearAccionDeNegocio(contexto,
                negocio: enumNegocio.Expediente,
                nombre: AccionesDeFacturacion.N_AbrirArchivadorDeFacturasParaElMes,
                momento: enumMomentoDeAccion.DC,
                parametro: parametroDeNegocio.Replace($"@{AccionesDeFacturacion.enumParametros.IdTipoExpediente}", $"{tipoExpediente.Id}")
                           .Replace($"@{AccionesDeFacturacion.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString())
                           .Replace($"@{AccionesDeFacturacion.enumParametros.Mes}", $"{1}"),
                orden: 10,
                descripcion: $"Crear archivador para el mes de enero");

            GestorDeAccionesDeNegocio.CrearAccionDeNegocio(contexto,
                negocio: enumNegocio.Expediente,
                nombre: AccionesDeFacturacion.N_AbrirArchivadorDeFacturasParaElMes,
                momento: enumMomentoDeAccion.DC,
                parametro: parametroDeNegocio.Replace($"@{AccionesDeFacturacion.enumParametros.IdTipoExpediente}", $"{tipoExpediente.Id}")
                           .Replace($"@{AccionesDeFacturacion.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString())
                           .Replace($"@{AccionesDeFacturacion.enumParametros.Mes}", $"{2}"),
                orden: 12,
                descripcion: $"Crear archivador para el mes de febrero");
        }

        private static void AccionAlTransitar(ContextoSe contexto)
        {
            var tipoArchivador = contexto.SeleccionarPorNombre<TipoDeArchivadorDtm>(InzArchivadoresEcoFin.n_tipo_contable);
            for (int i = 1; i <= 12; i++)
            {
                //Al cerrar enero cierro  enero
                //Al cerrar diciembre cierro diciembre
                GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
                    negocio: enumNegocio.Expediente,
                    transicion: enumNegocio.Expediente.Transicion(contexto, AlCerrarElMesDe(i)),
                    nombreAccion: AccionesDeFacturacion.N_CerrarArchivadorDeFacturasParaElMes,
                    momento: enumMomentoDeEjecucion.A,
                    parametro: parametroDeTransicion.Replace($"@{AccionesDeFacturacion.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString())
                                                    .Replace($"@{AccionesDeFacturacion.enumParametros.Mes}", $"{i}"),
                    orden: 10,
                    descripcion: $"Cierro el archivador del mes de {extFechas.Mes(i)}");

                //Al cerrar enero abro  marzo
                //Al cerrar octubre abro diciembre
                if (i < 11) GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
                                          negocio: enumNegocio.Expediente,
                                          transicion: enumNegocio.Expediente.Transicion(contexto, AlCerrarElMesDe(i)),
                                          nombreAccion: AccionesDeFacturacion.N_AbrirArchivadorDeFacturasParaElMes,
                                          momento: enumMomentoDeEjecucion.D,
                                          parametro: parametroDeTransicion.Replace($"@{AccionesDeFacturacion.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString())
                                                                          .Replace($"@{AccionesDeFacturacion.enumParametros.Mes}", $"{i + 2}"),
                                          orden: 10,
                                          descripcion: $"Abro un archivador para el mes de {extFechas.Mes(i + 2)}");

                //Creo un nuevo expediente al cerrar diciembre
                if (i == 12)
                    GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
                                          negocio: enumNegocio.Expediente,
                                          transicion: enumNegocio.Expediente.Transicion(contexto, AlCerrarElMesDe(i)),
                                          nombreAccion: AccionesDeFacturacion.N_AbrirExpedienteDeFacturasParaElEjercicio,
                                          momento: enumMomentoDeEjecucion.D,
                                          parametro: null,
                                          orden: 10,
                                          descripcion: $"Abro un expediente de control de facturación para el próximo ejercicio, y lo enlazo con l actual");
            }

            for (int i = 1; i < 12; i++)
            {
                //al reabrir enero cierro marzo
                //Al reabrir octubre cierro diciembre 
                if (i < 11) GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
                    negocio: enumNegocio.Expediente,
                    transicion: enumNegocio.Expediente.Transicion(contexto, AlReabrirElMesDe(i)),
                    nombreAccion: AccionesDeFacturacion.N_CerrarArchivadorDeFacturasParaElMes,
                    momento: enumMomentoDeEjecucion.A,
                    parametro: parametroDeTransicion.Replace($"@{AccionesDeFacturacion.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString())
                                                    .Replace($"@{AccionesDeFacturacion.enumParametros.Mes}", $"{i + 2}"),
                    orden: 10,
                    descripcion: $"cierro el archivador para el mes de {extFechas.Mes(i + 2)}");

                //Al reabrir enero abro  enero
                //Al reabrir noviembre abro noviembre
                GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
                    negocio: enumNegocio.Expediente,
                    transicion: enumNegocio.Expediente.Transicion(contexto, AlReabrirElMesDe(i)),
                    nombreAccion: AccionesDeFacturacion.N_AbrirArchivadorDeFacturasParaElMes,
                    momento: enumMomentoDeEjecucion.D,
                    parametro: parametroDeTransicion.Replace($"@{AccionesDeFacturacion.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString())
                                                    .Replace($"@{AccionesDeFacturacion.enumParametros.Mes}", $"{i}"),
                    orden: 10,
                    descripcion: $"Reabro el archivador del mes de {extFechas.Mes(i)}");
            }

            //reapertura de ejercicio
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto,
                negocio: enumNegocio.Expediente,
                transicion: enumNegocio.Expediente.Transicion(contexto, AlReabrirElMesDe(12)),
                nombreAccion: AccionesDeFacturacion.N_AbrirArchivadorDeFacturasParaElMes,
                momento: enumMomentoDeEjecucion.D,
                parametro: parametroDeTransicion.Replace($"@{AccionesDeFacturacion.enumParametros.IdTipoArchivador}", tipoArchivador.Id.ToString())
                                                .Replace($"@{AccionesDeFacturacion.enumParametros.Mes}", $"{12}"),
                orden: 10,
                descripcion: $"Reabro el archivador del mes de {extFechas.Mes(12)}");
        }

        private static string AlCerrarElMesDe(int mes)
        {
            if (mes == 1) return n_tran_fct_cerrar_enero;
            if (mes == 2) return n_tran_fct_cerrar_febrero;
            if (mes == 3) return n_tran_fct_cerrar_marzo;
            if (mes == 4) return n_tran_fct_cerrar_abril;
            if (mes == 5) return n_tran_fct_cerrar_mayo;
            if (mes == 6) return n_tran_fct_cerrar_junio;
            if (mes == 7) return n_tran_fct_cerrar_julio;
            if (mes == 8) return n_tran_fct_cerrar_agosto;
            if (mes == 9) return n_tran_fct_cerrar_septiembre;
            if (mes == 10) return n_tran_fct_cerrar_octubre;
            if (mes == 11) return n_tran_fct_cerrar_noviembre;
            if (mes == 12) return n_tran_fct_cerrar_ejercicio;

            throw new Exception($"Mes '{mes}' no contemplado");
        }

        private static string AlReabrirElMesDe(int mes)
        {
            if (mes == 1) return n_tran_fct_abrir_enero;
            if (mes == 2) return n_tran_fct_abrir_febrero;
            if (mes == 3) return n_tran_fct_abrir_marzo;
            if (mes == 4) return n_tran_fct_abrir_abril;
            if (mes == 5) return n_tran_fct_abrir_mayo;
            if (mes == 6) return n_tran_fct_abrir_junio;
            if (mes == 7) return n_tran_fct_abrir_julio;
            if (mes == 8) return n_tran_fct_abrir_agosto;
            if (mes == 9) return n_tran_fct_abrir_septiembre;
            if (mes == 10) return n_tran_fct_abrir_octubre;
            if (mes == 11) return n_tran_fct_abrir_noviembre;
            if (mes == 12) return n_tran_fct_abrir_ejercicio;

            throw new Exception($"Mes '{mes}' no contemplado");
        }

    }
}
