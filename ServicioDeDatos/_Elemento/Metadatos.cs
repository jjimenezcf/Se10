using Utilidades;

namespace ServicioDeDatos.Elemento
{
    public static class IDominio
    {
        public const string INT = nameof(INT);
        internal const string BIGINT = nameof(BIGINT);
        internal const string VARCHAR_MAX = "VARCHAR(MAX)";
        internal const string VARCHAR_15 = "VARCHAR(15)";
        public const string VARCHAR_20 = "VARCHAR(20)";
        internal const string VARCHAR_25 = "VARCHAR(25)";
        internal const string VARCHAR_10 = "VARCHAR(10)";
        internal const string VARCHAR_30 = "VARCHAR(30)";
        internal const string VARCHAR_40 = "VARCHAR(40)";
        internal const string VARCHAR_50 = "VARCHAR(50)";
        internal const string VARCHAR_4 = "VARCHAR(4)";
        internal const string VARCHAR_1 = "VARCHAR(1)";
        internal const string VARCHAR_11 = "VARCHAR(11)";
        internal const string CHAR_1 = "CHAR(1)";
        internal const string CHAR_2 = "CHAR(2)";
        public const string CHAR = "CHAR";
        internal const string VARCHAR_2 = "VARCHAR(2)";
        internal const string VARCHAR_3 = "VARCHAR(3)";
        internal const string VARCHAR_5 = "VARCHAR(5)";
        public const string VARCHAR_255 = "VARCHAR(255)";
        public const string VARCHAR_250 = "VARCHAR(250)";
        public const string VARCHAR_2000 = "VARCHAR(2000)";
        public const string VARCHAR = "VARCHAR";
        internal const string BIT = nameof(BIT);
        internal const string DATE = "DATE";
        internal const string DATETIME_2 = "DATETIME2(7)";
        internal const string CHAR_64 = "CHAR(64)";
        public const string DECIMAL = "DECIMAL(18,6)";
        internal const string BINARY = "varbinary(max)";
        public const string DECIMAL_5 = "DECIMAL(5)";
        public const string DECIMAL_6 = "DECIMAL(6)";
        public const string DECIMAL_4 = "DECIMAL(4)";
        internal const string PORCENTAJE_MENOR_100 = "DECIMAL(4,2)";
        internal const string PORCENTAJE_MENOR_1000 = "DECIMAL(5,2)";
        internal const string UNIQUEIDENTIFIER = nameof(UNIQUEIDENTIFIER);
        internal const string URL = VARCHAR_2000;


        internal const string CUENTA_CONTABLE = VARCHAR_10;
        internal const string NEGOCIO_ENUMERADO = VARCHAR_250;

        public static int Longitud(string dominio)
        {
            return dominio.Replace("VARCHAR(", "").Replace(")", "").Entero();
        }
    }

    public static class Sufijo
    {
        public const string ESTADO = nameof(ESTADO);
        public const string TRANSICION = nameof(TRANSICION);
        public const string ACCION = nameof(ACCION);
        public const string TIPO = nameof(TIPO);
        public const string TIPO_CLASE = nameof(TIPO_CLASE);
        public const string PLANTILLA = nameof(PLANTILLA);
        internal const string HISTORIA = nameof(HISTORIA);
        public const string OBSERVACION = nameof(OBSERVACION);
        internal const string AUDITORIA = nameof(AUDITORIA);
        internal const string DESCARGA_GUID = nameof(DESCARGA_GUID);
        internal const string TRAZA = nameof(TRAZA);
        internal const string DIRECCION = nameof(DIRECCION);
        internal const string ARCHIVO = Tablas.ARCHIVO;
        internal const string PERMISO = Tablas.PERMISO;
        internal const string ARCHIVADOR = Tablas.ARCHIVADOR;
        internal const string INTERLOCUTOR = Tablas.INTERLOCUTOR;
        internal const string CIRCUITO_DOC = Tablas.CIRCUITO_DOC;
        internal const string PAGO = Tablas.PAGO;
        internal const string TAREA = Tablas.TAREA;
        internal const string REGISTRO = Tablas.REGISTRO;
        internal const string EXPEDIENTE = Tablas.EXPEDIENTE;
        internal const string AGENDA_EVENTO = Tablas.AGENDA_EVENTO;
        internal const string DATOS_JURIDICOS = nameof(DATOS_JURIDICOS);
        internal const string RECOBRO = nameof(RECOBRO);
        internal const string MINUTA = nameof(MINUTA);
        internal const string TARIFA = nameof(TARIFA);
        internal const string VENTA = nameof(VENTA);
        internal const string IRPF = nameof(IRPF);
        internal const string VERIFACTU = nameof(VERIFACTU);
        internal const string LOG_AEAT = nameof(LOG_AEAT);
        internal const string DETALLE_LOG_AEAT = nameof(DETALLE_LOG_AEAT);        
        internal const string LINEA = nameof(LINEA);
        internal const string APUNTE = nameof(APUNTE);        
        internal const string PPT_CLIENTE = nameof(PPT_CLIENTE);
        public const string AVANCE = nameof(AVANCE);
        public const string SALDOS = nameof(SALDOS);
        public const string PRORROGA = nameof(PRORROGA);
        public const string AVAL_SOLICITADO = nameof(AVAL_SOLICITADO);
        public const string NATURALEZAS = nameof(NATURALEZAS);
        public const string SERVICIOS = nameof(SERVICIOS);
        public const string INF_JURIDICA = nameof(INF_JURIDICA);
        public const string MATRICULA_GUARDERIA = nameof(MATRICULA_GUARDERIA);        
        public const string UNITARIO = nameof(UNITARIO);
        public const string USUARIO = nameof(USUARIO);
        public const string PUESTO = nameof(PUESTO);
        public const string PANIFICACION = nameof(PANIFICACION);
        public const string ASIGNACION = nameof(ASIGNACION);
        public const string CERTIFICADO = nameof(CERTIFICADO);
        internal const string CUENTA = nameof(CUENTA);
        internal const string TARJETA = nameof(TARJETA);
        internal const string COBRO = nameof(COBRO);
        internal const string RECTIFICATIVA = nameof(RECTIFICATIVA);
        internal const string BLOQUEO = nameof(BLOQUEO);
        internal const string PARAMETRO = nameof(PARAMETRO);
        internal const string PERIODO = nameof(PERIODO);
        internal const string CENTRO_ADMINISTRATIVO = nameof(CENTRO_ADMINISTRATIVO);
        internal const string BUZON = nameof(BUZON);
        internal const string PROFE = nameof(PROFE);
        internal const string INSCRITO = nameof(INSCRITO);
        internal const string VOLUNTARIO = nameof(VOLUNTARIO);
        internal const string ACTIVIDAD_FORMATIVA = nameof(ACTIVIDAD_FORMATIVA);
    }

    public static class Vistas
    {
        internal const string USUARIO_PERMISO = nameof(USUARIO_PERMISO);
        internal const string PERMISOS_HEREDADOS = nameof(PERMISOS_HEREDADOS);
    }

    public static class Tablas
    {
        public const string AGENDA = nameof(AGENDA);
        public const string MICORREO = nameof(MICORREO);
        public const string AGENDA_EVENTO = nameof(AGENDA_EVENTO);
        public const string VISTA_MVC = nameof(VISTA_MVC);
        public const string PARAMETRO_VISTA_USUARIO = nameof(PARAMETRO_VISTA_USUARIO);
        public const string ACCION = nameof(ACCION);
        public const string ACCESO_RECIENTE = nameof(ACCESO_RECIENTE);
        public const string VARIABLE = nameof(VARIABLE);
        public const string TRABAJO = nameof(TRABAJO);
        public const string SEMAFORO = nameof(SEMAFORO);
        public const string LIBRO_SEMAFORO = nameof(LIBRO_SEMAFORO);
        public const string LIBRO = nameof(LIBRO);
        public const string SEMAFORO_PROCESO = nameof(SEMAFORO_PROCESO);
        public const string ACCION_RELACION = nameof(ACCION_RELACION);
        public const string NEGOCIO = nameof(NEGOCIO);
        public const string PARAMETRO = nameof(PARAMETRO);
        public const string PARAMETRO_USUARIO = nameof(PARAMETRO_USUARIO);
        public const string PLANTILLA_CREACION = nameof(PLANTILLA_CREACION);
        public const string PLANTILLA_EXPORTACION = nameof(PLANTILLA_EXPORTACION);
        public const string PLANTILLA_FILTRADO = nameof(PLANTILLA_FILTRADO);
        public const string CLASE = nameof(CLASE);
        public const string CONSULTA_CON_GUID = nameof(CONSULTA_CON_GUID);


        public const string CLASE_PERMISO = nameof(CLASE_PERMISO);
        public const string ROL_PERMISO = nameof(ROL_PERMISO);
        public const string PERMISO_DIRECTOS = nameof(PERMISO_DIRECTOS);
        public const string USU_PUESTO = nameof(USU_PUESTO);
        public const string ROL_PUESTO = nameof(ROL_PUESTO);
        public const string PUESTO = nameof(PUESTO);
        public const string PERMISO = nameof(PERMISO);
        public const string USUARIO = nameof(USUARIO);


        public const string PERMISO_POR_TIPO = nameof(PERMISO_POR_TIPO);
        public const string PERMISO_POR_ESTADO = nameof(PERMISO_POR_ESTADO);
        public const string PERMISO_POR_NEGOCIO = nameof(PERMISO_POR_NEGOCIO);
        public const string PERMISO_POR_ELEMENTO = nameof(PERMISO_POR_ELEMENTO);
        public const string PERMISO_POR_TRANSICION = nameof(PERMISO_POR_TRANSICION);
        public const string PERMISO_POR_NEGOCIOS_CG = nameof(PERMISO_POR_NEGOCIOS_CG);


        public const string PAIS = nameof(PAIS);
        public const string PROVINCIA = nameof(PROVINCIA);
        public const string MUNICIPIO = nameof(MUNICIPIO);
        public const string BARRIO = nameof(BARRIO);
        public const string ZONA = nameof(ZONA);
        public const string TIPO_VIA = nameof(TIPO_VIA);
        public const string CALLE = nameof(CALLE);
        public const string CODIGO_POSTAL = nameof(CODIGO_POSTAL);
        public const string CP = nameof(CP);

        public const string ARCHIVO = nameof(ARCHIVO);
        public const string ARCHIVO_SINCRONIZADO = nameof(ARCHIVO_SINCRONIZADO);
        public const string SOCIEDAD = nameof(SOCIEDAD);
        public const string FACTURADOR = nameof(FACTURADOR);
        public const string SOCIEDAD_CONTACTO = nameof(SOCIEDAD_CONTACTO);
        public const string CENTRO_GESTOR = nameof(CENTRO_GESTOR);
        public const string CENTRO_GESTOR_NEGOCIO = nameof(CENTRO_GESTOR_NEGOCIO);


        public const string ARCHIVADOR = nameof(ARCHIVADOR);
        public const string ARCHIVADOR_TIPO = nameof(ARCHIVADOR_TIPO);
        public const string CIRCUITO_DOC = nameof(CIRCUITO_DOC);
        public const string PERSONA = nameof(PERSONA);
        public const string INTERLOCUTOR = nameof(INTERLOCUTOR);
        public const string REGISTRO = nameof(REGISTRO);
        public const string TAREA = nameof(TAREA);

        public const string EXPEDIENTE = nameof(EXPEDIENTE);
        public const string PROCURADOR = nameof(PROCURADOR);
        public const string ABOGADO = nameof(ABOGADO);
        public const string LICITANTE = nameof(LICITANTE);
        public const string PROVEEDOR = nameof(PROVEEDOR);
        public const string CLIENTE = nameof(CLIENTE);
        public const string TRABAJADOR = nameof(TRABAJADOR);
        public const string BANCO = nameof(BANCO);
        public const string APUNTE = nameof(APUNTE);

        public const string JUZGADO_CLASE = nameof(JUZGADO_CLASE);
        public const string JUZGADO = nameof(JUZGADO);
        public const string COLEGIO = nameof(COLEGIO);

        public const string PLEITO = nameof(PLEITO);
        public const string CONTRATO = nameof(CONTRATO);
        public const string LOTE = nameof(LOTE);
        public const string PLANIFICADOR_VENTA = nameof(PLANIFICADOR_VENTA);

        public const string CERTIFICADO = nameof(CERTIFICADO);
        public const string FIRMADO = nameof(FIRMADO);


        internal const string MT_NATURALEZA = nameof(MT_NATURALEZA);
        internal const string MT_UNIDAD = nameof(MT_UNIDAD);
        internal const string UNITARIO = nameof(UNITARIO);
        internal const string TABLA_DE_PRECIOS = nameof(TABLA_DE_PRECIOS);

        internal const string CUENTA = nameof(CUENTA);
        internal const string CUENTA_BANCARIA = nameof(CUENTA_BANCARIA);
        internal const string IVA_REPERCUTIDO = nameof(IVA_REPERCUTIDO);
        internal const string IVA_SOPORTADO = nameof(IVA_SOPORTADO);
        internal const string IRPF = nameof(IRPF);
        internal const string FORMA_DE_PAGO = nameof(FORMA_DE_PAGO);
        internal const string FORMA_DE_COBRO = nameof(FORMA_DE_COBRO);
        internal const string PREASIENTO = nameof(PREASIENTO);
        

        internal const string PRESUPUESTO = nameof(PRESUPUESTO);
        internal const string PARTE_TR = nameof(PARTE_TR);
        internal const string FACTURA_EMT = nameof(FACTURA_EMT);
        internal const string PETICION_DE_FACTURA_EMT = nameof(PETICION_DE_FACTURA_EMT);
        internal const string PLANIFICACION_VENTA = nameof(PLANIFICACION_VENTA);
        internal const string REMESA_FAE = nameof(REMESA_FAE);

        internal const string GASTO = nameof(GASTO);
        internal const string PAGO = nameof(PAGO);
        internal const string REMESA_PAG = nameof(REMESA_PAG);
        internal const string FACTURA_REC = nameof(FACTURA_REC);
        internal const string PEDIDO = nameof(PEDIDO);
        

        internal const string AULA = nameof(AULA);
        internal const string INFANTE = nameof(INFANTE);
        internal const string CURSO = nameof(CURSO);
        internal const string CURSO_INFANTE = nameof(CURSO_INFANTE);        
    }

    public static class Esquemas
    {
        public const string ENTORNO = nameof(ENTORNO);
        public const string SEGURIDAD = nameof(SEGURIDAD);
        public const string TERCEROS = nameof(TERCEROS);
        public const string SISDOC = nameof(SISDOC);
        public const string REGISTRO = nameof(REGISTRO);
        public const string TRABAJO = nameof(TRABAJO);
        public const string CALLEJERO = nameof(CALLEJERO);
        public const string NEGOCIO = nameof(NEGOCIO);
        public const string TAREA = nameof(TAREA);
        public const string EXPEDIENTE = nameof(EXPEDIENTE);
        public const string JURIDICO = nameof(JURIDICO);
        public const string MT = nameof(MT);
        public const string CONTABILIDAD = nameof(CONTABILIDAD);
        public const string PRESUPUESTO = nameof(PRESUPUESTO);
        public const string VENTA = nameof(VENTA);
        public const string GASTO = nameof(GASTO);
        public const string LOGISTICA = nameof(LOGISTICA);
        public const string GUARDERIA = nameof(GUARDERIA); 
        
    }

    public class Funciones
    {
        public const string OBTENER_ORIGEN_PUESTO_PERMISO = nameof(OBTENER_ORIGEN_PUESTO_PERMISO);
        public const string OBTENER_ROLES_DE_UN_PUESTO = nameof(OBTENER_ROLES_DE_UN_PUESTO);
        public const string CC_TOTAL_PPT = nameof(CC_TOTAL_PPT);
        public const string CC_VALORADO_EN = nameof(CC_VALORADO_EN);
        public const string CC_CALLE_EXPRESION = nameof(CC_CALLE_EXPRESION);
    }

    public static class ICampos
    {
        public const string PASSWORD = nameof(PASSWORD);
        public const string LOGIN = nameof(LOGIN);
        public const string ADMINISTRADOR = nameof(ADMINISTRADOR);
        public const string FALLIDOS = nameof(FALLIDOS);

        public const string ID = nameof(ID);
        public const string NOMBRE = nameof(NOMBRE);
        public const string EXPRESION = nameof(EXPRESION);
        public const string RESTRICTOR = nameof(RESTRICTOR);
        public const string ROLES = nameof(ROLES);
        internal const string DESCRIPCION = nameof(DESCRIPCION);
        internal const string AUDITORIA = nameof(AUDITORIA);
        internal const string GUID = nameof(GUID);
        internal const string APIKEY = nameof(APIKEY);

        internal const string REFERENCIA = nameof(REFERENCIA);
        internal const string REF_EXTERNA = nameof(REF_EXTERNA);
        internal const string ICONO = nameof(ICONO);
        internal const string BAJA = nameof(BAJA);
        internal const string ACTIVO = nameof(ACTIVO);
        internal const string ACTIVA = nameof(ACTIVA);
        internal const string PERMITIR_CONTADO = nameof(PERMITIR_CONTADO);
        internal const string PERMITIR_REMESAR = nameof(PERMITIR_REMESAR);
        internal const string CODIGO = nameof(CODIGO);
        internal const string BIC_SWIFT = nameof(BIC_SWIFT);
        internal const string ID_CG = nameof(ID_CG);
        internal const string ID_RESPONSABLE = nameof(ID_RESPONSABLE);
        internal const string RUTA = nameof(RUTA);
        internal const string EMAIL = nameof(EMAIL);
        internal const string BUZON = nameof(BUZON);
        internal const string ID_CREADOR = nameof(ID_CREADOR);
        internal const string CREADOR = nameof(CREADOR);
        internal const string ID_MODIFICADOR = nameof(ID_MODIFICADOR);
        internal const string MODIFICADOR = nameof(MODIFICADOR);
        internal const string ID_TIPO = nameof(ID_TIPO);
        internal const string ID_CLASE = nameof(ID_CLASE);
        internal const string ID_CLASE_ELEMENTO = nameof(ID_CLASE_ELEMENTO);
        internal const string FECHA = nameof(FECHA);
        internal const string FECCRE = nameof(FECCRE);
        internal const string FECMOD = nameof(FECMOD);
        internal const string ID_ADM = nameof(ID_ADM);
        internal const string ID_INTERVENTOR = nameof(ID_INTERVENTOR);
        internal const string ID_GESTOR = nameof(ID_GESTOR);
        internal const string ID_CONSULTOR = nameof(ID_CONSULTOR);
        internal const string ID_NEGOCIO = nameof(ID_NEGOCIO);
        internal const string NEGOCIO = nameof(NEGOCIO);
        internal const string ID_VINCULADO = nameof(ID_VINCULADO);
        internal const string VINCULADO = nameof(VINCULADO);
        internal const string ID_SOCIEDAD = nameof(ID_SOCIEDAD);
        internal const string ID_ARCHIVO = nameof(ID_ARCHIVO);
        internal const string ID_PADRE = nameof(ID_PADRE);
        internal const string ID_ARCHIVADOR = nameof(ID_ARCHIVADOR);
        internal const string ID_BLOCKCHAIN = nameof(ID_BLOCKCHAIN);
        internal const string ID_LOG = nameof(ID_LOG);

        internal const string VISIBLE = nameof(VISIBLE);
        


        internal const string USA_PLANIFICACION = nameof(USA_PLANIFICACION);
        internal const string ES_FACTURABLE = nameof(ES_FACTURABLE);
        internal const string COPIAR_DIRECCION = nameof(COPIAR_DIRECCION);
        internal const string USA_TAREAS = nameof(USA_TAREAS);
        internal const string USA_PPTS = nameof(USA_PPTS);
        internal const string USA_DATOS_JURIDICOS = nameof(USA_DATOS_JURIDICOS);
        internal const string SC_VENTA = nameof(SC_VENTA);
        internal const string SC_COMPRA = nameof(SC_COMPRA);
        internal const string TIPO_DTM = nameof(TIPO_DTM);
        internal const string TIPO_DTO = nameof(TIPO_DTO);
        internal const string CLASE_DE_LIBRO = nameof(CLASE_DE_LIBRO);
        internal const string SIGLA = nameof(SIGLA);
        internal const string MASCARA = nameof(MASCARA);
        internal const string MARCADOR = nameof(MARCADOR);
        internal const string ORDEN = nameof(ORDEN);
        internal const string PADRE = nameof(PADRE);
        public const string VISTA = nameof(VISTA);
        internal const string CONTROLADOR = nameof(CONTROLADOR);
        internal const string ACCION = nameof(ACCION);
        internal const string PARAMETROS = nameof(PARAMETROS);
        internal const string MAPEOS_JSON = nameof(MAPEOS_JSON);
        internal const string MODAL = nameof(MODAL);
        internal const string F_ALTA = nameof(F_ALTA);

        internal const string ID_USUARIO = nameof(ID_USUARIO);
        internal const string ELEMENTO = nameof(ELEMENTO);
        public const string ID_ELEMENTO = nameof(ID_ELEMENTO);
        internal const string ID_ELEMENTO1 = nameof(ID_ELEMENTO1);
        internal const string ID_ELEMENTO2 = nameof(ID_ELEMENTO2);
        internal const string OPERACION = nameof(OPERACION);
        internal const string ID_PROCESO = nameof(ID_PROCESO);
        internal const string REGISTRO = nameof(REGISTRO);
        internal const string AUDITADO_EL = nameof(AUDITADO_EL);
        internal const string SINCRONIZAR_CON = nameof(SINCRONIZAR_CON);

        internal const string PROPIETARIO = nameof(PROPIETARIO);
        internal const string CREADO_EL = nameof(CREADO_EL);
        internal const string SOLICITADO_EL = nameof(SOLICITADO_EL);
        internal const string BLOQUEADO_EL = nameof(BLOQUEADO_EL);
        internal const string DESCARGADO_EL = nameof(DESCARGADO_EL);
        internal const string CADUCA_EL = nameof(CADUCA_EL);
        internal const string MAXIMO_DESCARGAS = nameof(MAXIMO_DESCARGAS);
        internal const string ABONADO_EL = nameof(ABONADO_EL);
        internal const string IMPUTADO_EL = nameof(IMPUTADO_EL);
        internal const string MODIFICADO_EL = nameof(MODIFICADO_EL);
        internal const string SINCRONIZADO_EL = nameof(SINCRONIZADO_EL);
        internal const string LONGITUD = nameof(LONGITUD);

        internal const string ID_AGENDA = nameof(ID_AGENDA);
        public const string INICIO = nameof(INICIO);
        internal const string FIN = nameof(FIN);

        internal const string P_INICIO = nameof(P_INICIO);
        internal const string P_FIN = nameof(P_FIN);
        internal const string R_INICIO = nameof(R_INICIO);
        internal const string R_FIN = nameof(R_FIN);

        internal const string VALOR = nameof(VALOR);
        internal const string ID_LIBRO = nameof(ID_LIBRO);
        internal const string EJERCICIO = nameof(EJERCICIO);


        internal const string ID_PERMISO = nameof(ID_PERMISO);
        internal const string CALCULADO = nameof(CALCULADO);
        public const string ID_TRABAJO = nameof(ID_TRABAJO);
        internal const string ID_EJECUTOR = nameof(ID_EJECUTOR);
        internal const string ID_SOMETEDOR = nameof(ID_SOMETEDOR);
        internal const string ENTRADA = nameof(ENTRADA);
        public const string PLANIFICADO = nameof(PLANIFICADO);
        internal const string INICIADO = nameof(INICIADO);
        internal const string TERMINADO = nameof(TERMINADO);
        internal const string ESTADO = nameof(ESTADO);
        internal const string PERIODICIDAD = nameof(PERIODICIDAD);
        public const string MEDIDO_EN = nameof(MEDIDO_EN);
        internal const string INICIAL = nameof(INICIAL);
        internal const string CANCELADO = nameof(CANCELADO);
        internal const string REPETIR_CADA = nameof(REPETIR_CADA);
        internal const string GENERADO = nameof(GENERADO);

        internal const string ELEMENTO_DTM = nameof(ELEMENTO_DTM);
        public const string ELEMENTO_DTO = nameof(ELEMENTO_DTO);
        internal const string USA_SEGURIDAD = nameof(USA_SEGURIDAD);
        internal const string ES_DE_PARAMETRIZACION = nameof(ES_DE_PARAMETRIZACION);
        internal const string ENUMERADO = nameof(ENUMERADO);
        internal const string USA_CG = nameof(USA_CG);

        internal const string RAZON_SOCIAL = nameof(RAZON_SOCIAL);
        internal const string CODIGO_CONTABLE = nameof(CODIGO_CONTABLE);
        internal const string CODIGO_FISCAL = nameof(CODIGO_FISCAL);
        internal const string NIF = nameof(NIF);
        internal const string VAT = nameof(VAT);
        internal const string TELEFONO = nameof(TELEFONO);
        internal const string ES_INTERLOCUTOR = nameof(ES_INTERLOCUTOR);
        internal const string CODIGO_DIARIO = nameof(CODIGO_DIARIO);

        

        public const string PIE_DE_PPT = nameof(PIE_DE_PPT);
        public const string PIE_DE_FACTURA = nameof(PIE_DE_FACTURA);
        public const string INSCRITO_EN = nameof(INSCRITO_EN);

        internal const string PRINCIPAL = nameof(PRINCIPAL);
        internal const string INTERESES = nameof(INTERESES);
        internal const string ABONADO = nameof(ABONADO);


        internal const string ES_NIE = nameof(ES_NIE);
        internal const string APELLIDO = nameof(APELLIDO);
        internal const string APELLIDO_2 = nameof(APELLIDO_2);

        internal const string ID_TIPO_DE_VIA = nameof(ID_TIPO_DE_VIA);
        internal const string ID_PAIS = nameof(ID_PAIS);
        internal const string ID_PROVINCIA = nameof(ID_PROVINCIA);
        internal const string PROVINCIA = nameof(PROVINCIA);
        internal const string ID_MUNICIPIO = nameof(ID_MUNICIPIO);
        internal const string MUNICIPIOS = nameof(MUNICIPIOS);
        internal const string ID_BARRIO = nameof(ID_BARRIO);
        internal const string ID_CALLE = nameof(ID_CALLE);
        internal const string ID_ZONA = nameof(ID_ZONA);
        internal const string ID_CP = nameof(ID_CP);
        internal const string NUMERO = nameof(NUMERO);
        internal const string ESCALERA = nameof(ESCALERA);
        internal const string PISO = nameof(PISO);
        internal const string PUERTA = nameof(PUERTA);
        internal const string OTROS = nameof(OTROS);
        internal const string URL = nameof(URL);
        internal const string ICS = nameof(ICS);
        internal const string CALIFICADOR = nameof(CALIFICADOR);
        internal const string CP = nameof(CP);
        internal const string DC = nameof(DC);
        internal const string MANO = nameof(MANO);
        internal const string DESDE = nameof(DESDE);
        internal const string HASTA = nameof(HASTA);
        internal const string PREFIJO = nameof(PREFIJO);
        internal const string ISO2 = nameof(ISO2);
        internal const string NOMBRE_INGLES = nameof(NOMBRE_INGLES);
        internal const string ES_UE = nameof(ES_UE);

        internal const string ID_PERSONA = nameof(ID_PERSONA);
        internal const string ID_CONTACTO = nameof(ID_CONTACTO);
        internal const string ID_INTERLOCUTOR = nameof(ID_INTERLOCUTOR);
        internal const string ID_PROCURADOR = nameof(ID_PROCURADOR);
        internal const string ID_ABOGADO = nameof(ID_ABOGADO);
        internal const string ID_JUZGADO = nameof(ID_JUZGADO);

        internal const string ID_INSCRITO = nameof(ID_INSCRITO);
        internal const string ID_VOLUNTARIO = nameof(ID_VOLUNTARIO);
        internal const string ASISTIO = nameof(ASISTIO);

        internal const string NIG = nameof(NIG);
        internal const string RESULTADO = nameof(RESULTADO);
        internal const string LITIGADO = nameof(LITIGADO);
        internal const string SENTENCIADO = nameof(SENTENCIADO);
        internal const string COSTAS = nameof(COSTAS);
        internal const string SENTENCIADO_EL = nameof(SENTENCIADO_EL);

        public const string ID_ESTADO = nameof(ID_ESTADO);
        public const string ID_ORIGEN = nameof(ID_ORIGEN);
        public const string ID_DESTINO = nameof(ID_DESTINO);
        public const string DEL_SISTEMA = nameof(DEL_SISTEMA);
        public const string CON_OBSERVACION = nameof(CON_OBSERVACION);
        public const string POR_DEFECCTO = nameof(POR_DEFECCTO);
        
        public const string ASUNTO = nameof(ASUNTO);
        public const string ANOTACION = nameof(ANOTACION);
        public const string CONCEPTO = nameof(CONCEPTO);
        public const string TIEMPO = nameof(TIEMPO);

        public const string NOMBRE_MODIFICABLE = nameof(NOMBRE_MODIFICABLE);
        public const string PERMITE_CREAR = nameof(PERMITE_CREAR);
        public const string EDITAR_TRAS_CREAR = nameof(EDITAR_TRAS_CREAR);

        public const string ES_DLL = nameof(ES_DLL);
        public const string DLL = nameof(DLL);
        public const string CLASE = nameof(CLASE);
        public const string MODO = nameof(MODO);
        public const string ALIAS = nameof(ALIAS);


        public const string METODO = nameof(METODO);
        public const string ESQUEMA = nameof(ESQUEMA);
        public const string PA = nameof(PA);
        public const string COMUNICAR_FIN = nameof(COMUNICAR_FIN);
        public const string COMUNICAR_ERROR = nameof(COMUNICAR_ERROR);
        public const string ID_INFORMAR_A = nameof(ID_INFORMAR_A);
        public const string SQL = nameof(SQL);
        public const string CLASE_DE_ACCION = nameof(CLASE_DE_ACCION);
        public const string CLASE_DE_ACCESO = nameof(CLASE_DE_ACCESO);
        public const string ERROR = nameof(ERROR);
        public const string FACTURA_JSON = nameof(FACTURA_JSON);
        public const string VALIDADOR_JSON = nameof(VALIDADOR_JSON);


        public const string ID_TRANSICION = nameof(ID_TRANSICION);
        public const string ID_ACCION = nameof(ID_ACCION);
        public const string MOMENTO = nameof(MOMENTO);
        public const string ID_OBSERVACION = nameof(ID_OBSERVACION);
        public const string ID_LECTOR = nameof(ID_LECTOR);
        public const string ID_EXPORTADOR = nameof(ID_EXPORTADOR);


        internal const string CLASE_ES = nameof(CLASE_ES);
        internal const string CLASE_TAREA = nameof(CLASE_TAREA);
        internal const string ID_TIPO_ENTRADA = nameof(ID_TIPO_ENTRADA);
        internal const string ID_TIPO_SALIDA = nameof(ID_TIPO_SALIDA);
        internal const string ID_TIPO_INTERNO = nameof(ID_TIPO_INTERNO);
        public const string ID_TIPO_ARCHIVADOR = nameof(ID_TIPO_ARCHIVADOR);
        internal const string CONTACTO = nameof(CONTACTO);

        internal const string ID_SOLICITANTE = nameof(ID_SOLICITANTE);
        internal const string ID_ARCHIVADOR_ENTRADA = nameof(ID_ARCHIVADOR_ENTRADA);
        internal const string ID_ARCHIVADOR_SALIDA = nameof(ID_ARCHIVADOR_SALIDA);
        internal const string ID_ARCHIVADOR_INTERNO = nameof(ID_ARCHIVADOR_INTERNO);

        internal const string CLASE_EXPEDIENTE = nameof(CLASE_EXPEDIENTE);
        internal const string CLASE_PLEITO = nameof(CLASE_PLEITO);
        internal const string CLASE_CONTRATO = nameof(CLASE_CONTRATO);
        internal const string VALORADO_EN = nameof(VALORADO_EN);
        internal const string ID_CONTRATO = nameof(ID_CONTRATO);
        internal const string ID_PLANIFICADOR = nameof(ID_PLANIFICADOR);
        internal const string ID_LOTE = nameof(ID_LOTE);
        internal const string ID_TIPO_PLANIFICACION = nameof(ID_TIPO_PLANIFICACION);
        internal const string ID_TIPO_FACTURA = nameof(ID_TIPO_FACTURA);
        internal const string ID_TIPO_FAR = nameof(ID_TIPO_FAR);
        internal const string ID_TIPO_PARTE = nameof(ID_TIPO_PARTE);
        internal const string ID_FACTURA_EMT = nameof(ID_FACTURA_EMT);
        internal const string ID_FACTURA_REC = nameof(ID_FACTURA_REC);
        internal const string ID_PARTE_TR = nameof(ID_PARTE_TR);
        internal const string EJECUTAR_EL = nameof(EJECUTAR_EL);
        internal const string ID_FACTURA_REMESADA = nameof(ID_FACTURA_REMESADA);
        internal const string ID_FACTURADOR = nameof(ID_FACTURADOR);

        internal const string ID_MENSAJE = nameof(ID_MENSAJE);
        internal const string EMISOR = nameof(EMISOR);
        internal const string TO = nameof(TO);
        public const string CUERPO = nameof(CUERPO);
        public const string ADJUNTOS = nameof(ADJUNTOS);

        internal const string CLASE_CERTIFICADO = nameof(CLASE_CERTIFICADO);
        internal const string EXPIRA_EL = nameof(EXPIRA_EL);
        internal const string ID_CERTIFICADO = nameof(ID_CERTIFICADO);
        internal const string SUBIDO_EL = nameof(SUBIDO_EL);
        internal const string ACCEDIDO_EL = nameof(ACCEDIDO_EL);
        internal const string MOTIVO = nameof(MOTIVO);
        internal const string FIRMADO_EL = nameof(FIRMADO_EL);
        internal const string ID_ORIGINAL = nameof(ID_ORIGINAL);
        internal const string ID_FIRMADO = nameof(ID_FIRMADO);
        internal const string OPCION_HTML = nameof(OPCION_HTML);
        

        internal const string ID_UNIDAD = nameof(ID_UNIDAD);
        
        internal const string ID_PREASIENTO = nameof(ID_PREASIENTO);
        internal const string ID_NATURALEZA = nameof(ID_NATURALEZA);
        internal const string PVP = nameof(PVP);
        internal const string ID_CUENTA_GASTO = nameof(ID_CUENTA_GASTO);
        internal const string ID_CUENTA_INGRESO = nameof(ID_CUENTA_INGRESO);
        internal const string ID_CUENTA_CARGO = nameof(ID_CUENTA_CARGO);
        internal const string ID_CUENTA_ACREEDORA = nameof(ID_CUENTA_ACREEDORA);
        internal const string ID_CUENTA_NO_DEDUCIBLE = nameof(ID_CUENTA_NO_DEDUCIBLE);
        public const string ID_TARJETA = nameof(ID_TARJETA);

        internal const string ID_CUENTA = nameof(ID_CUENTA);
        internal const string ID_PROVEEDOR = nameof(ID_PROVEEDOR);
        internal const string ID_CLIENTE = nameof(ID_CLIENTE);
        internal const string ID_TRABAJADOR = nameof(ID_TRABAJADOR);
        internal const string DC_IBAN = nameof(DC_IBAN);
        internal const string ENTIDAD = nameof(ENTIDAD);
        internal const string OFICINA = nameof(OFICINA);
        internal const string DC_CCC = nameof(DC_CCC);
        internal const string ID_CENTRO_ADMINISTRATIVO = nameof(ID_CENTRO_ADMINISTRATIVO);
        internal const string ORGANO_GESTOR = nameof(ORGANO_GESTOR);
        internal const string UNIDAD_TRAMITADORA = nameof(UNIDAD_TRAMITADORA);
        internal const string OFICINA_CONTABLE = nameof(OFICINA_CONTABLE);

        internal const string CODIGO_CUENTA = nameof(CODIGO_CUENTA);
        public const string POSICION = nameof(POSICION);
        public const string DETALLE = nameof(DETALLE);


        internal const string CLASE_PPT = nameof(CLASE_PPT);
        internal const string TOTAL = nameof(TOTAL);
        internal const string ID_PRESUPUESTO = nameof(ID_PRESUPUESTO);
        public const string ID_EXPEDIENTE = nameof(ID_EXPEDIENTE);
        internal const string IVA = nameof(IVA);
        internal const string CLASE_IVA = nameof(CLASE_IVA);
        internal const string IRPF = nameof(IRPF);
        internal const string DESCUENTO = nameof(DESCUENTO);
        internal const string TIPO_LINEA = nameof(TIPO_LINEA);
        internal const string ID_UNITARIO = nameof(ID_UNITARIO);
        internal const string ID_IVA_R = nameof(ID_IVA_R);
        internal const string ID_IVA_S = nameof(ID_IVA_S);
        internal const string ID_IRPF = nameof(ID_IRPF);
        internal const string ES_EXPORTACION = nameof(ES_EXPORTACION);
        internal const string CON_PERIODO = nameof(CON_PERIODO);

        internal const string CANTIDAD = nameof(CANTIDAD);
        internal const string COSTE = nameof(COSTE);
        internal const string VENTA = nameof(VENTA);
        internal const string PRECIO = nameof(PRECIO);
        internal const string BI = nameof(BI);
        internal const string PORCENTAJE = nameof(PORCENTAJE);
        internal const string EXENTO = nameof(EXENTO);

        public const string REALIZADO = nameof(REALIZADO);
        public const string FACTURADO = nameof(FACTURADO);
        public const string COBRADO = nameof(COBRADO);
        public const string CLASE_PRORROGA = nameof(CLASE_PRORROGA);
        public const string MESES = nameof(MESES);
        public const string ULTIMA = nameof(ULTIMA);
        public const string IMPORTE = nameof(IMPORTE);
        public const string ADENDADO = nameof(ADENDADO);
        public const string AVISO = nameof(AVISO);
        public const string BLOQUEO = nameof(BLOQUEO);
        public const string NOTIFICADO = nameof(NOTIFICADO);
        public const string IMPORTE_MAXIMO = nameof(IMPORTE_MAXIMO);

        public const string ANO = nameof(ANO);
        public const string SERIE = nameof(SERIE);
        public const string DES_FISCAL = nameof(DES_FISCAL);

        public const string EMITIDA_EL = nameof(EMITIDA_EL);
        public const string FACTURADA_EL = nameof(FACTURADA_EL);
        public const string HUELLA = nameof(HUELLA);
        public const string FIRMA = nameof(FIRMA);
        public const string CSV = nameof(CSV);        
        public const string PREGUNTA = nameof(PREGUNTA);
        public const string RESPUESTA = nameof(RESPUESTA);        
        public const string RECIBIDA_EL = nameof(RECIBIDA_EL);
        public const string VENCE_EL = nameof(VENCE_EL);
        public const string CONTABILIZADA_EL = nameof(CONTABILIZADA_EL);
        public const string FECHA_CONTABLE = nameof(FECHA_CONTABLE);
        public const string COBRADO_EL = nameof(COBRADO_EL);
        public const string RECTIFICATIVA = nameof(RECTIFICATIVA);
        public const string CLASE_RECTIFICATIVA = nameof(CLASE_RECTIFICATIVA);
        public const string MOTIVO_RECTIFICACION = nameof(MOTIVO_RECTIFICACION);
        public const string CLASE_EMISION = nameof(CLASE_EMISION);
        public const string GENERADA_EL = nameof(GENERADA_EL);
        public const string CARGAR_EL = nameof(CARGAR_EL);
        public const string PRESENTADA_EL = nameof(PRESENTADA_EL);
        public const string PETICION = nameof(PETICION);

        public const string CARGADA_EL = nameof(CARGADA_EL);
        public const string DEVUELTA_EL = nameof(DEVUELTA_EL);
        public const string MAX_FECDEV = nameof(MAX_FECDEV);


        public const string ACREEDOR = nameof(ACREEDOR);
        public const string NIF_ACREEDOR = nameof(NIF_ACREEDOR);
        public const string SUF_ACREEDOR = nameof(SUF_ACREEDOR);

        public const string DEUDOR = nameof(DEUDOR);
        public const string NIF_DEUDOR = nameof(NIF_DEUDOR);
        public const string SUF_DEUDOR = nameof(SUF_DEUDOR);

        public const string PRESENTADOR = nameof(PRESENTADOR);
        public const string NIF_PRESENTADOR = nameof(NIF_PRESENTADOR);
        public const string SUF_PRESENTADOR = nameof(SUF_PRESENTADOR);


        public const string PAGADO_EL = nameof(PAGADO_EL);
        public const string PAGAR_EL = nameof(PAGAR_EL);
        public const string ANULADO_EL = nameof(ANULADO_EL);
        public const string ID_PAGO = nameof(ID_PAGO);


        public const string VENCIMIENTO = nameof(VENCIMIENTO);


        internal const string ID_AULA = nameof(ID_AULA);
        internal const string ID_INFANTE = nameof(ID_INFANTE);
        internal const string ID_CURSO = nameof(ID_CURSO);
        internal const string ID_MADRE = nameof(ID_MADRE);
        internal const string NACIDO_EL = nameof(NACIDO_EL);


        public const string PEDIDO_EL = nameof(PEDIDO_EL);
        public const string ENTREGAR_EL = nameof(ENTREGAR_EL);
        public const string RECIBIDO_EL = nameof(RECIBIDO_EL);
        public const string CERRADO_EL = nameof(CERRADO_EL);

        public const string ENTREGADA_EL = nameof(ENTREGADA_EL);
        public const string ENVIADA_EL = nameof(ENVIADA_EL);

        //obsoleto hay que cambiar por el bueno
        internal const string IDPADRE = nameof(IDPADRE);
        internal const string IDVISTA_MVC = nameof(IDVISTA_MVC);
        internal const string IDPERMISO = nameof(IDPERMISO);
        internal const string IDROL = nameof(IDROL);
        internal const string IDUSUA = nameof(IDUSUA);
        internal const string IDPUESTO = nameof(IDPUESTO);
        internal const string ORIGEN = nameof(ORIGEN);
        internal const string DESTINO = nameof(DESTINO);
        internal const string IDTIPO = nameof(IDTIPO);
        internal const string IDCLASE = nameof(IDCLASE);
        internal const string ID_MENU = nameof(ID_MENU);
        

    }
}
