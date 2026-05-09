using Gestor.Errores;
using GestorDeElementos.Extensores;
using ModeloDeDto.Contabilidad;
using ModeloDeDto.Expediente;
using ModeloDeDto.Gastos;
using ModeloDeDto.Juridico;
using ModeloDeDto.Logistica;
using ModeloDeDto.Presupuesto;
using ModeloDeDto.RegistroEs;
using ModeloDeDto.SistemaDocumental;
using ModeloDeDto.Tarea;
using ModeloDeDto.Ventas;
using Newtonsoft.Json.Linq;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utilidades;
using static ServicioDeDatos.Ventas.VariablesDePlfsDeVenta;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Entorno;

namespace GestorDeElementos
{
    public class Metadatos
    {
        public Type TipoDto { get; set; }
        public Type TipoDtm { get; set; }
        public Type EstadoDtm { get; set; }
        public Type HitosDtm { get; set; }
        public Type ObservacionesDtm { get; set; }
        public Type TrazaDtm { get; set; }
        public Type ArchivosDtm { get; set; }
        public Type DireccionesDtm { get; set; }
        public Type ArchivadoresDtm { get; set; }
        public Type CircuitosDocDtm { get; set; }
        public Type TareasDtm { get; set; }
        public Type PlantillasPorTipoDtm { get; set; }
        public Type ClasesDelTipoDtm { get; set; }
        public Type TipoEtapas { get; set; }

        public Func<Enum, List<int>> EstadosDeLaEtapa;
        public Type TipoParametros { get; set; }

        public Type DescriptoDeConsultas { get; set; } = null;
    }

    public enum enumParametroDeNegocio
    {
        [Description("Indica si para el tipo de negocio permite crear una observación aun estando en estado terminado")]
        NEG_ObservacionesSiTerminado
    }

    public class ObservacionesSiTerminado
    {
        public int IdTipo { get; set; }
        public bool Permitir { get; set; }
    }

    public static class VariablesDeNegocio
    {
        private const string _jsonObservacionesSiTerminado = "[{\"IdTipo\": 0,\"Permitir\": false}]";

        public static bool PermitirObservacionesSiTerminado(this enumNegocio negocio, int idTipo, ContextoSe contexto)
        {
            var parametro = enumParametroDeNegocio.NEG_ObservacionesSiTerminado;
            var json = negocio.Parametro(parametro, crearParametro: true, valorPorDefecto: _jsonObservacionesSiTerminado).Valor;

            var registros = ParsearPermitirSiTerminado(negocio, json);

            var registro = registros.FirstOrDefault(x => x.IdTipo == idTipo);

            if (idTipo > 0 && registro == null && registros.Any(x => x.IdTipo == 0 && x.Permitir))
            {
                return true;
            }

            if (registro == null)
                return false;

            return registro.Permitir;
        }

        private static List<ObservacionesSiTerminado> ParsearPermitirSiTerminado(enumNegocio negocio, string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new ObservacionesSiTerminado
                {
                    IdTipo = item["IdTipo"].Value<int>(),
                    Permitir = item["Permitir"].Value<bool>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonObservacionesSiTerminado}', debe definir en el negocio '{negocio.Singular()}' el parámetro '{enumParametroDeNegocio.NEG_ObservacionesSiTerminado}' según el patrón '{_jsonObservacionesSiTerminado}'", ex);
            }
            return null;
        }
    }

    public static class MetadatosDelNegocio
    {
        public static Metadatos ObtenerMetadatos(this enumNegocio negocio, bool emitirError = true)
        {
            if (negocio == enumNegocio.No_Definido)
            {
                if (emitirError)
                    GestorDeErrores.Emitir($"Ha solicitado metadatos para el negocio {enumNegocio.No_Definido} ");
                return null;
            }

            Metadatos metadatos = null;
            switch (negocio)
            {
                case enumNegocio.Expediente:
                    metadatos = MetadatosDeExpedientes();
                    break;
                case enumNegocio.Presupuesto:
                    metadatos = MetadatosDePresupuestos();
                    break;
                case enumNegocio.Registro:
                    metadatos = MetadatosDeRegistroEs();
                    break;
                case enumNegocio.Tarea:
                    metadatos = MetadatosDeTareas();
                    break;
                case enumNegocio.Pleito:
                    metadatos = MetadatosDePleitos();
                    break;
                case enumNegocio.Archivador:
                    metadatos = MetadatosDeArchivadores();
                    break;
                case enumNegocio.CircuitoDoc:
                    metadatos = MetadatosDeCircuitosDoc();
                    break;
                case enumNegocio.Contrato:
                    metadatos = MetadatosDeContratos();
                    break;
                case enumNegocio.ParteDeTrabajo:
                    metadatos = MetadatosDePartesTr();
                    break;
                case enumNegocio.FacturaEmitida:
                    metadatos = MetadatosDeFacturasEmitidas();
                    break;
                case enumNegocio.PlanificacionDeVenta:
                    metadatos = MetadatosDePlanificacionDeVenta();
                    break;
                case enumNegocio.RemesaFae:
                    metadatos = MetadatosDeRemesasFae();
                    break;
                case enumNegocio.Pago:
                    metadatos = MetadatosDePagos();
                    break;
                case enumNegocio.Gasto:
                    metadatos = MetadatosDeGastos();
                    break;
                case enumNegocio.RemesaPag:
                    metadatos = MetadatosDeRemesasPag();
                    break;
                case enumNegocio.FacturaRecibida:
                    metadatos = MetadatosDeFacturasRecibidas();
                    break;
                case enumNegocio.Pedido:
                    metadatos = MetadatosDePedidos();
                    break;
                case enumNegocio.Preasiento:
                    metadatos = MetadatosDePreasientos();
                    break;
                case enumNegocio.Cliente:
                    metadatos = MetadatosDeClientes();
                    break;
                case enumNegocio.Proveedor:
                    metadatos = MetadatosDeProveedores();
                    break;
                case enumNegocio.Trabajador:
                    metadatos = MetadatosDeTrabajadores();
                    break;
                case enumNegocio.Sociedad:
                    metadatos = MetadatosDeSociedades();
                    break;
                case enumNegocio.CursoDeGuarderia:
                    metadatos = MetadatosDeCursosDeGuarderia();
                    break;
                case enumNegocio.Infante:
                    metadatos = MetadatosDeInfantes();
                    break;
                case enumNegocio.Persona:
                    metadatos = MetadatosDePersonas();
                    break;
                case enumNegocio.Abogado:
                    metadatos = MetadatosDeAbogados();
                    break;
                case enumNegocio.Procurador:
                    metadatos = MetadatosDeProcuradores();
                    break;
                case enumNegocio.Interlocutor:
                    metadatos = MetadatosDeInterlocutores();
                    break;
                case enumNegocio.Menu:
                    metadatos = MetadatosDeMenus();
                    break;
                case enumNegocio.Usuario:
                    metadatos = MetadatosDeUsuarios();
                    break;
            }

            if (metadatos == null && (negocio.UsaTipo() || negocio.UsaEstado()))
            {
                GestorDeErrores.Emitir($"No se han indicado cuales son los tipos del negocio {negocio}");
            }

            if (metadatos != null && negocio.UsaEstado() && metadatos.EstadoDtm == null)
            {
                GestorDeErrores.Emitir($"No se han indicado cuales es el tipo de objeto EstadoDtm del negocio {negocio}");
            }

            if (metadatos != null && negocio.UsaTipo() && (metadatos.TipoDto == null || metadatos.TipoDtm == null))
            {
                GestorDeErrores.Emitir($"No se han indicado cuales son los tipos de objeto TipoDtm y TipoDto del negocio {negocio}");
            }

            if (negocio.UsaPlantillasPorTipo() && (metadatos == null || metadatos.PlantillasPorTipoDtm == null))
                GestorDeErrores.Emitir($"No se ha indicado cual el valor de la propiedad '{nameof(metadatos.PlantillasPorTipoDtm)}' en los metadatos del negocio '{negocio}'");

            if (negocio.UsaClasesDelTipo() && (metadatos == null || metadatos.ClasesDelTipoDtm == null))
                GestorDeErrores.Emitir($"No se ha indicado cual el valor de la propiedad '{nameof(metadatos.ClasesDelTipoDtm)}' en los metadatos del negocio '{negocio}'");

            return metadatos;
        }


        public static string ObtenerDescripcionDelParametro(this enumNegocio negocio, string parametro)
        {
            var metadatos = negocio.ObtenerMetadatos();

            // 1. Validamos que existan metadatos y que se haya definido un tipo de parámetros si no cojemos el enumerado de parámetros de negocio de negocio
            var tipoParametros = metadatos?.TipoParametros == null ? typeof(enumParametrosDeNegocio) : metadatos.TipoParametros;


            // 2. Intentamos parsear el string al enumerado correspondiente
            // Usamos ignoreCase: true por seguridad
            if (Enum.TryParse(tipoParametros, parametro, true, out object valorEnum))
            {
                // 3. Al ser un objeto devuelto por TryParse, lo casteamos a Enum 
                // y usamos tu método de extensión existente
                return ((Enum)valorEnum).Descripcion();
            }

            return string.Empty;
        }

        public static string ObtenerDescripcionDeEtapa(this enumNegocio negocio, string etapa)
        {
            var metadatos = negocio.ObtenerMetadatos();
            if (metadatos?.TipoEtapas == null) return string.Empty;

            if (Enum.TryParse(metadatos.TipoEtapas, etapa, true, out object valorEnum))
            {
                return ((Enum)valorEnum).Descripcion();
            }

            return string.Empty;
        }

        public static Metadatos MetadatosDeFacturasEmitidas() => new Metadatos
        {
            TipoDto = typeof(TipoDeFacturaEmtDto),
            TipoDtm = typeof(TipoDeFacturaEmtDtm),
            EstadoDtm = typeof(EstadoDeUnaFacturaEmtDtm),
            HitosDtm = typeof(HitosDeUnaFacturaEmtDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnaFacturaEmtDtm),
            TrazaDtm = typeof(TrazasDeUnaFacturaEmtDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnaFacturaEmtDtm),
            ArchivosDtm = typeof(ArchivosDeUnaFacturaEmtDtm),
            TipoEtapas = typeof(enumEtapasDeFacturasEmt),
            TipoParametros = typeof(enumParametrosDeFacturasEmt),
            EstadosDeLaEtapa = etapa => VariableDeFacturasEmt.Lista((enumEtapasDeFacturasEmt)etapa),
            DireccionesDtm = typeof(DireccionDeUnaFacturaEmtDtm),
            PlantillasPorTipoDtm = typeof(PlantillaPorTipoDeFacturaEmtDtm),
        };


        public static Metadatos MetadatosDeFacturasRecibidas() => new Metadatos
        {
            TipoDto = typeof(TipoDeFacturaRecDto),
            TipoDtm = typeof(TipoDeFacturaRecDtm),
            EstadoDtm = typeof(EstadoDeUnaFacturaRecDtm),
            HitosDtm = typeof(HitosDeUnaFacturaRecDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnaFacturaRecDtm),
            TrazaDtm = typeof(TrazasDeUnaFacturaRecDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnaFacturaRecDtm),
            ArchivosDtm = typeof(ArchivosDeUnaFacturaRecDtm),
            TipoParametros = typeof(enumParametrosDeFacturasRec),
            TipoEtapas = typeof(enumEtapasDeFacturasRec),
            EstadosDeLaEtapa = etapa => VariableDeFacturasRec.Lista((enumEtapasDeFacturasRec)etapa),
            DireccionesDtm = typeof(DireccionDeUnaFacturaRecDtm)
        };

        public static Metadatos MetadatosDePedidos() => new Metadatos
        {
            TipoDto = typeof(TipoDePedidoDto),
            TipoDtm = typeof(TipoDePedidoDtm),
            EstadoDtm = typeof(EstadoDeUnPedidoDtm),
            HitosDtm = typeof(HitosDeUnPedidoDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnPedidoDtm),
            TrazaDtm = typeof(TrazasDeUnPedidoDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnPedidoDtm),
            ArchivosDtm = typeof(ArchivosDeUnPedidoDtm),
            TipoParametros = typeof(enumParametrosDePedidos),
            TipoEtapas = typeof(enumEtapasDePedido),
            EstadosDeLaEtapa = etapa => VariableDePedidos.Lista((enumEtapasDePedido)etapa),
            DireccionesDtm = typeof(DireccionDeUnPedidoDtm)
        };
        public static Metadatos MetadatosDePreasientos() => new Metadatos
        {
            TipoDto = typeof(TipoDePreasientoDto),
            TipoDtm = typeof(TipoDePreasientoDtm),
            EstadoDtm = typeof(EstadoDeUnPreasientoDtm),
            HitosDtm = typeof(HitosDeUnPreasientoDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnPreasientoDtm),
            TrazaDtm = typeof(TrazasDeUnPreasientoDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnPreasientoDtm),
            ArchivosDtm = null,
            TipoParametros = typeof(enumParametrosDePreasiento),
            TipoEtapas = typeof(enumEtapasDePreasiento),
            EstadosDeLaEtapa = etapa => VariablesDePreasiento.Lista((enumEtapasDePreasiento)etapa),
            DireccionesDtm = null
        };

        public static Metadatos MetadatosDeExpedientes() => new Metadatos
        {
            TipoDto = typeof(TipoDeExpedienteDto),
            TipoDtm = typeof(TipoDeExpedienteDtm),
            EstadoDtm = typeof(EstadoDeUnExpedienteDtm),
            HitosDtm = typeof(HitosDeUnExpedienteDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnExpedienteDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnExpedienteDtm),
            TrazaDtm = typeof(TrazasDeUnExpedienteDtm),
            ArchivosDtm = typeof(ArchivosDeUnExpedienteDtm),
            PlantillasPorTipoDtm = typeof(PlantillaPorTipoDeExpedienteDtm),
            DireccionesDtm = typeof(DireccionDeUnExpedienteDtm),
            TareasDtm = typeof(TareasDeUnExpedienteDtm),
            TipoParametros = typeof(enumParametrosDeExpedientes),
            TipoEtapas = typeof(enumEtapasDeExpedientes),
            EstadosDeLaEtapa = etapa => ParametrosDeExpedientes.Lista((enumEtapasDeExpedientes)etapa)
        };

        public static Metadatos MetadatosDePresupuestos() => new Metadatos
        {
            TipoDto = typeof(TipoDePresupuestoDto),
            TipoDtm = typeof(TipoDePresupuestoDtm),
            EstadoDtm = typeof(EstadoDeUnPresupuestoDtm),
            HitosDtm = typeof(HitosDeUnPresupuestoDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnPresupuestoDtm),
            TrazaDtm = typeof(TrazasDeUnPresupuestoDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnPresupuestoDtm),
            ArchivosDtm = typeof(ArchivosDeUnPresupuestoDtm),
            DireccionesDtm = typeof(DireccionDeUnPresupuestoDtm),
            PlantillasPorTipoDtm = null,
            TipoParametros = typeof(enumParametrosDePresupuesto),
            TareasDtm = typeof(TareasDeUnPresupuestoDtm),
            TipoEtapas = typeof(enumEtapasDePpts),
            EstadosDeLaEtapa = etapa => VariableDePpts.Lista((enumEtapasDePpts)etapa)
        };

        public static Metadatos MetadatosDeRegistroEs() => new Metadatos
        {
            TipoDto = typeof(TipoDeRegistroEsDto),
            TipoDtm = typeof(TipoDeRegistroEsDtm),
            EstadoDtm = typeof(EstadoDeUnRegistroEsDtm),
            HitosDtm = typeof(HitosDeUnRegistroEsDtm),
            TrazaDtm = typeof(TrazasDeUnRegistroEsDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnRegistroEsDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnRegistroEsDtm),
            ArchivosDtm = typeof(ArchivosDeUnRegistroEsDtm),
            DireccionesDtm = typeof(DireccionDeUnRegistroEsDtm),
            TareasDtm = typeof(TareasDeUnRegistroDtm),
            TipoParametros = null,
            TipoEtapas = null,
            PlantillasPorTipoDtm = null
        };

        public static Metadatos MetadatosDeTareas() => new Metadatos
        {
            TipoDto = typeof(TipoDeTareaDto),
            TipoDtm = typeof(TipoDeTareaDtm),
            EstadoDtm = typeof(EstadoDeUnaTareaDtm),
            HitosDtm = typeof(HitosDeUnaTareaDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnaTareaDtm),
            TrazaDtm = typeof(TrazasDeUnaTareaDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnaTareaDtm),
            ArchivosDtm = typeof(ArchivosDeUnaTareaDtm),
            DireccionesDtm = typeof(DireccionDeUnaTareaDtm),
            TipoParametros = typeof(enumParametrosDeTareas),
            TipoEtapas = typeof(enumEtapasDeTareas),
            EstadosDeLaEtapa = etapa => VariablesDeTareas.Lista((enumEtapasDeTareas)etapa),
            PlantillasPorTipoDtm = null,
            ClasesDelTipoDtm = typeof(ClaseDelTipoTareaDtm),
            DescriptoDeConsultas = ApiDeEnsamblados.ObtenerType(ApiDeEnsamblados.DllDelSistemaDeElementos, ApiDeEnsamblados.DescriptorDeConsultaDeTareas, emitirError: false)
        };

        public static Metadatos MetadatosDePleitos() => new Metadatos
        {
            TipoDto = typeof(TipoDePleitoDto),
            TipoDtm = typeof(TipoDePleitoDtm),
            EstadoDtm = typeof(EstadoDeUnPleitoDtm),
            HitosDtm = typeof(HitosDeUnPleitoDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnPleitoDtm),
            TrazaDtm = typeof(TrazasDeUnPleitoDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnPleitoDtm),
            ArchivosDtm = typeof(ArchivosDeUnPleitoDtm),
            DireccionesDtm = typeof(DireccionDeUnPleitoDtm),
            TareasDtm = typeof(TareasDeUnPleitoDtm),
            TipoParametros = null,
            TipoEtapas = null,
            PlantillasPorTipoDtm = typeof(PlantillaPorTipoDePleitoDtm)
        };

        public static Metadatos MetadatosDeArchivadores() => new Metadatos
        {
            TipoDto = typeof(TipoDeArchivadorDto),
            TipoDtm = typeof(TipoDeArchivadorDtm),
            EstadoDtm = null,
            HitosDtm = null,
            ObservacionesDtm = typeof(ObservacionesDeUnArchivadorDtm),
            TrazaDtm = typeof(TrazasDeUnArchivadorDtm),
            ArchivadoresDtm = null,
            ArchivosDtm = typeof(ArchivosDeUnArchivadorDtm),
            TipoParametros = typeof(enumParametrosDeArchivadores),
            TipoEtapas = null,
            DireccionesDtm = null,
            PlantillasPorTipoDtm = null
        };

        public static Metadatos MetadatosDeCircuitosDoc() => new Metadatos
        {
            TipoDto = typeof(TipoDeCircuitoDocDto),
            TipoDtm = typeof(TipoDeCircuitoDocDtm),
            EstadoDtm = typeof(EstadoDeUnCircuitoDocDtm),
            HitosDtm = typeof(HitosDeUnCircuitoDocDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnCircuitoDocDtm),
            TrazaDtm = typeof(TrazasDeUnCircuitoDocDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnCircuitoDocDtm),
            ArchivosDtm = typeof(ArchivosDeUnCircuitoDocDtm),
            TipoParametros = typeof(enumParametrosDeCircuitosDoc),
            TipoEtapas = null,
            DireccionesDtm = null,
            PlantillasPorTipoDtm = null,
            DescriptoDeConsultas = ApiDeEnsamblados.ObtenerType(ApiDeEnsamblados.DllDelSistemaDeElementos, ApiDeEnsamblados.DescriptorDeConsultaDeCad, emitirError: false)
        };

        public static Metadatos MetadatosDeContratos() => new Metadatos
        {
            TipoDto = typeof(TipoDeContratoDto),
            TipoDtm = typeof(TipoDeContratoDtm),
            EstadoDtm = typeof(EstadoDeUnContratoDtm),
            HitosDtm = typeof(HitosDeUnContratoDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnContratoDtm),
            TrazaDtm = typeof(TrazasDeUnContratoDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnContratoDtm),
            ArchivosDtm = typeof(ArchivosDeUnContratoDtm),
            DireccionesDtm = typeof(DireccionDeUnContratoDtm),
            TareasDtm = typeof(TareasDeUnContratoDtm),
            TipoParametros = typeof(enumParametrosDeContratos),
            TipoEtapas = typeof(enumEtapasDeContratos),
            EstadosDeLaEtapa = etapa => VariablesDeContratos.Lista((enumEtapasDeContratos)etapa),
            PlantillasPorTipoDtm = null
        };

        public static Metadatos MetadatosDePartesTr() => new Metadatos
        {
            TipoDto = typeof(TipoDeParteTrDto),
            TipoDtm = typeof(TipoDeParteTrDtm),
            EstadoDtm = typeof(EstadoDeUnParteTrDtm),
            HitosDtm = typeof(HitosDeUnParteTrDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnParteTrDtm),
            TrazaDtm = typeof(TrazasDeUnParteTrDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnParteTrDtm),
            ArchivosDtm = typeof(ArchivosDeUnParteTrDtm),
            DireccionesDtm = typeof(DireccionDeUnParteTrDtm),
            TareasDtm = typeof(TareasDeUnParteTrDtm),
            TipoParametros = typeof(enumParametrosDePartes),
            TipoEtapas = typeof(enumEtapasDePartesTr),
            EstadosDeLaEtapa = etapa => VariableDePartesTr.Lista((enumEtapasDePartesTr)etapa),
            PlantillasPorTipoDtm = null
        };

        public static Metadatos MetadatosDePlanificacionDeVenta() => new Metadatos
        {
            TipoDto = typeof(TipoDePlanificacionDeVentaDto),
            TipoDtm = typeof(TipoDePlanificacionDeVentaDtm),
            EstadoDtm = typeof(EstadoDeUnaPlanificacionDeVentaDtm),
            HitosDtm = typeof(HitosDeUnaPlanificacionDeVentaDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnaPlanificacionDeVentaDtm),
            TrazaDtm = typeof(TrazasDeUnaPlanificacionDeVentaDtm),
            ArchivadoresDtm = null,
            ArchivosDtm = typeof(ArchivosDeUnaPlanificacionDeVentaDtm),
            DireccionesDtm = typeof(DireccionDeUnaPlanificacionDeVentaDtm),
            TipoParametros = null,
            TipoEtapas = typeof(enumEtapasDePlfsDeVenta),
            EstadosDeLaEtapa = etapa => VariablesDePlfsDeVenta.Lista((enumEtapasDePlfsDeVenta)etapa),
            PlantillasPorTipoDtm = null
        };

        public static Metadatos MetadatosDeRemesasFae() => new Metadatos
        {
            TipoDto = typeof(TipoDeRemesaFaeDto),
            TipoDtm = typeof(TipoDeRemesaFaeDtm),
            EstadoDtm = typeof(EstadoDeUnaRemesaFaeDtm),
            HitosDtm = typeof(HitosDeUnaRemesaFaeDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnaRemesaFaeDtm),
            TrazaDtm = typeof(TrazasDeUnaRemesaFaeDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnaRemesaFaeDtm),
            ArchivosDtm = typeof(ArchivosDeUnaRemesaFaeDtm),
            TipoParametros = typeof(enumParametrosDeRemesasFae),
            TipoEtapas = typeof(enumEtapasDeRemesasFae),
            EstadosDeLaEtapa = etapa => VariableDeRemesasFae.Lista((enumEtapasDeRemesasFae)etapa),
            DireccionesDtm = null,
            PlantillasPorTipoDtm = null
        };

        public static Metadatos MetadatosDeRemesasPag() => new Metadatos
        {
            TipoDto = typeof(TipoDeRemesaPagDto),
            TipoDtm = typeof(TipoDeRemesaPagDtm),
            EstadoDtm = typeof(EstadoDeUnaRemesaPagDtm),
            HitosDtm = typeof(HitosDeUnaRemesaPagDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnaRemesaPagDtm),
            TrazaDtm = typeof(TrazasDeUnaRemesaPagDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnaRemesaPagDtm),
            ArchivosDtm = typeof(ArchivosDeUnaRemesaPagDtm),
            TipoParametros = typeof(enumParametrosDeRemesasPag),
            TipoEtapas = typeof(enumEtapasDeRemesasPag),
            EstadosDeLaEtapa = etapa => VariableDeRemesasPag.Lista((enumEtapasDeRemesasPag)etapa),
            DireccionesDtm = null,
            PlantillasPorTipoDtm = null
        };

        public static Metadatos MetadatosDePagos() => new Metadatos
        {
            TipoDto = typeof(TipoDePagoDto),
            TipoDtm = typeof(TipoDePagoDtm),
            EstadoDtm = typeof(EstadoDeUnPagoDtm),
            HitosDtm = typeof(HitosDeUnPagoDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnPagoDtm),
            TrazaDtm = typeof(TrazasDeUnPagoDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnPagoDtm),
            ArchivosDtm = typeof(ArchivosDeUnPagoDtm),
            TipoParametros = typeof(enumParametrosDePagos),
            TipoEtapas = typeof(enumEtapasDePagos),
            EstadosDeLaEtapa = etapa => VariableDePagos.Lista((enumEtapasDePagos)etapa),
            DireccionesDtm = null,
            PlantillasPorTipoDtm = null
        };

        public static Metadatos MetadatosDeGastos() => new Metadatos
        {
            TipoDto = null,
            TipoDtm = typeof(TipoDeGastoDtm),
            EstadoDtm = typeof(EstadoDeUnGastoDtm),
            HitosDtm = typeof(HitosDeUnGastoDtm),
            ObservacionesDtm = typeof(ObservacionesDeUnGastoDtm),
            TrazaDtm = typeof(TrazasDeUnGastoDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnGastoDtm),
            ArchivosDtm = typeof(ArchivosDeUnGastoDtm),
            TipoParametros = null,
            TipoEtapas = null,
            EstadosDeLaEtapa = etapa => new List<int>(),
            DireccionesDtm = null,
            PlantillasPorTipoDtm = null
        };

        public static Metadatos MetadatosDeClientes() => new Metadatos
        {
            TipoDto = null,
            TipoDtm = null,
            EstadoDtm = null,
            HitosDtm = null,
            ObservacionesDtm = typeof(ObservacionesDeUnClienteDtm),
            TrazaDtm = typeof(TrazasDeUnClienteDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnClienteDtm),
            ArchivosDtm = typeof(ArchivosDeUnClienteDtm),
            DireccionesDtm = typeof(DireccionDeUnClienteDtm),
            TipoParametros = typeof(enumParametrosDeCliente),
            TipoEtapas = null,
            PlantillasPorTipoDtm = null
        };
        public static Metadatos MetadatosDeTrabajadores() => new Metadatos
        {
            TipoDto = null,
            TipoDtm = null,
            EstadoDtm = null,
            HitosDtm = null,
            ObservacionesDtm = typeof(ObservacionesDeUnTrabajadorDtm),
            TrazaDtm = typeof(TrazasDeUnTrabajadorDtm),
            ArchivadoresDtm = null,
            ArchivosDtm = typeof(ArchivosDeUnTrabajadorDtm),
            DireccionesDtm = typeof(DireccionDeTrabajadorDtm),
            CircuitosDocDtm = typeof(CircuitoDocDeUnTrabajadorDtm),
            TipoParametros = null,
            TipoEtapas = null,
            PlantillasPorTipoDtm = null
        };
        public static Metadatos MetadatosDeProveedores() => new Metadatos
        {
            TipoDto = null,
            TipoDtm = null,
            EstadoDtm = null,
            HitosDtm = null,
            ObservacionesDtm = typeof(ObservacionesDeUnProveedorDtm),
            TrazaDtm = typeof(TrazasDeUnProveedorDtm),
            ArchivadoresDtm = null,
            ArchivosDtm = typeof(ArchivosDeUnProveedorDtm),
            DireccionesDtm = typeof(DireccionDeUnProveedorDtm),
            TipoParametros = null,
            TipoEtapas = null,
            PlantillasPorTipoDtm = null
        };

        public static Metadatos MetadatosDePersonas() => new Metadatos
        {
            TipoDto = null,
            TipoDtm = null,
            EstadoDtm = null,
            HitosDtm = null,
            ObservacionesDtm = typeof(ObservacionesDeUnaPersonaDtm),
            TrazaDtm = typeof(TrazasDeUnaPersonaDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnaPersonaDtm),
            ArchivosDtm = typeof(ArchivosDeUnaPersonaDtm),
            DireccionesDtm = typeof(DireccionDeUnaPersonaDtm),
            TipoParametros = null,
            TipoEtapas = null,
            PlantillasPorTipoDtm = null
        };

        public static Metadatos MetadatosDeSociedades() => new Metadatos
        {
            TipoDto = null,
            TipoDtm = null,
            EstadoDtm = null,
            HitosDtm = null,
            ObservacionesDtm = typeof(ObservacionesDeUnaSociedadDtm),
            TrazaDtm = typeof(TrazasDeUnaSociedadDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnaSociedadDtm),
            ArchivosDtm = typeof(ArchivosDeUnaSociedadDtm),
            DireccionesDtm = typeof(DireccionDeLaSociedadDtm),
            TipoParametros = typeof(enumParametrosDeSociedad),
            TipoEtapas = null,
            PlantillasPorTipoDtm = null
        };


        public static Metadatos MetadatosDeAbogados() => new Metadatos
        {
            TipoDto = null,
            TipoDtm = null,
            EstadoDtm = null,
            HitosDtm = null,
            ObservacionesDtm = typeof(ObservacionesDeUnAbogadoDtm),
            TrazaDtm = typeof(TrazasDeUnAbogadoDtm),
            ArchivadoresDtm = null,
            ArchivosDtm = typeof(ArchivosDeUnAbogadoDtm),
            DireccionesDtm = typeof(DireccionDeAbogadoDtm),
            TipoParametros = null,
            TipoEtapas = null,
            PlantillasPorTipoDtm = null
        };

        public static Metadatos MetadatosDeProcuradores() => new Metadatos
        {
            TipoDto = null,
            TipoDtm = null,
            EstadoDtm = null,
            HitosDtm = null,
            ObservacionesDtm = typeof(ObservacionesDeUnProcuradorDtm),
            TrazaDtm = typeof(TrazasDeUnProcuradorDtm),
            ArchivadoresDtm = null,
            ArchivosDtm = typeof(ArchivosDeUnProcuradorDtm),
            DireccionesDtm = typeof(DireccionDeProcuradorDtm),
            TipoParametros = null,
            TipoEtapas = null,
            PlantillasPorTipoDtm = null
        };

        public static Metadatos MetadatosDeInterlocutores() => new Metadatos
        {
            TipoDto = null,
            TipoDtm = null,
            EstadoDtm = null,
            HitosDtm = null,
            ObservacionesDtm = typeof(ObservacionesDeUnInterlocutorDtm),
            TrazaDtm = typeof(TrazasDeUnInterlocutorDtm),
            ArchivadoresDtm = null,
            ArchivosDtm = typeof(ArchivosDeUnInterlocutorDtm),
            DireccionesDtm = typeof(DireccionDeInterlocutorDtm),
            TipoParametros = typeof(enumParametrosDeInterlocutor),
            TipoEtapas = null,
            PlantillasPorTipoDtm = null
        };

        public static Metadatos MetadatosDeCursosDeGuarderia() => new Metadatos
        {
            TipoDto = null,
            TipoDtm = null,
            EstadoDtm = null,
            HitosDtm = null,
            ObservacionesDtm = typeof(ObservacionesDeUnCursoDeGuarderiaDtm),
            TrazaDtm = typeof(TrazasDeUnCursoDeGuarderiaDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnCursoDeGuarderiaDtm),
            ArchivosDtm = typeof(ArchivosDeUnCursoDeGuarderiaDtm),
            TipoParametros = typeof(enumParametrosDeGuarderia),
            TipoEtapas = null,
            EstadosDeLaEtapa = etapa => new List<int>(),
            DireccionesDtm = null
        };

        public static Metadatos MetadatosDeInfantes() => new Metadatos
        {
            TipoDto = null,
            TipoDtm = null,
            EstadoDtm = null,
            HitosDtm = null,
            ObservacionesDtm = typeof(ObservacionesDeUnInfanteDtm),
            TrazaDtm = typeof(TrazasDeUnInfanteDtm),
            ArchivadoresDtm = typeof(ArchivadoresDeUnInfanteDtm),
            ArchivosDtm = typeof(ArchivosDeUnInfanteDtm),
            TipoParametros = null,
            TipoEtapas = null,
            EstadosDeLaEtapa = etapa => new List<int>(),
            DireccionesDtm = null,
            DescriptoDeConsultas = ApiDeEnsamblados.ObtenerType(ApiDeEnsamblados.DllDelSistemaDeElementos,  ApiDeEnsamblados.DescriptorDeConsultaDeInfantes, emitirError: false)
        };

        public static Metadatos MetadatosDeMenus() => new Metadatos
        {
            TipoDto = null,
            TipoDtm = null,
            EstadoDtm = null,
            HitosDtm = null,
            ObservacionesDtm = null,
            TrazaDtm = null,
            ArchivadoresDtm = null,
            ArchivosDtm = null,
            DireccionesDtm = null,
            TipoParametros = typeof(enumParametrosDeMenus),
            TipoEtapas = null,
            PlantillasPorTipoDtm = null
        };

        public static Metadatos MetadatosDeUsuarios() => new Metadatos
        {
            TipoDto = null,
            TipoDtm = null,
            EstadoDtm = null,
            HitosDtm = null,
            ObservacionesDtm = null,
            TrazaDtm = null,
            ArchivadoresDtm = null,
            ArchivosDtm = null,
            DireccionesDtm = null,
            TipoParametros = typeof(enumParametrosDeUsuarios),
            TipoEtapas = null,
            PlantillasPorTipoDtm = null
        };
    }
}
