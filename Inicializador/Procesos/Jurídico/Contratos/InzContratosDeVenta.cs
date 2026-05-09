using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Juridico;
using GestoresDeNegocio.Negocio;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Juridico;
using ModeloDeDto.SistemaDocumental;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using SistemaDeElementos.Inicializador;
using Utilidades;

namespace Inicializador.ContratosVnt
{
    public static class InzContratosVnt
    {
        public static SociedadDtm PepeFuster = new SociedadDtm
        {
            Nombre = "Mantenimientos Y Servicios Fuster, S.L",
            CodigoFiscal = "MSF",
            NIF = "B73954455",
            eMail = "pepe.fuster@gmail.com",
            Telefono = "968 93 33 43"
        };


        public static SociedadDtm AbioPep = new SociedadDtm
        {
            Nombre = "ABIOPEP, S.L.",
            CodigoFiscal = "GEN",
            NIF = "B73805731",
            eMail = "diegohernandez@trazadodecarreteras.es",
            Telefono = "968277844"
        };


        public static SociedadDtm AbanaProyectos = new SociedadDtm
        {
            Nombre = "ABIERTO POR REFORMAS, S.L",
            CodigoFiscal = "GEN",
            NIF = "B42797068",
            eMail = "santiago@abanaproyectos.com",
            Telefono = "600522633"
        };


        public static SociedadDtm NuevoVertice = new SociedadDtm
        {
            Nombre = "ACADEMIA NUEVO VERTICE S.L.  ",
            CodigoFiscal = "GEN",
            NIF = "B73831265",
            eMail = "josemiguel@academianuevovertice.es",
            Telefono = "615373021"
        };


        public static void ModeloDeContratosVnt(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                DefinirEtapas(contexto);
                DefinirVariables(contexto);
                Acciones(contexto);
                Tipos(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public const string n_ctv_estado_pdt_activar = "CTV: Pdt. de Activar";
        public const string n_ctv_estado_vigente = "CTV: Vigente";
        public const string n_ctv_estado_pdt_prorrogar = "CTV: Pdt. de prorrogar";
        public const string n_ctv_estado_derogado = "CTV: Derogado";
        public const string n_ctv_estado_finalizado = "CTV: Finalizado";
        public const string n_ctv_estado_cancelado = "CTV: Cancelado";


        private static void DefinirEtapas(ContextoSe contexto)
        {
            var CTR_Etapa_En_Elaboracion = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(nameof(INombre.Nombre), n_ctv_estado_pdt_activar);
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_En_Elaboracion, CTR_Etapa_En_Elaboracion.Id.ToString());

            var CTR_Etapa_Vigente = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(nameof(INombre.Nombre), n_ctv_estado_vigente);
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_Vigente, CTR_Etapa_Vigente.Id.ToString());

            var CTR_Etapa_Derogado = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(nameof(INombre.Nombre), n_ctv_estado_derogado);
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_Derogado, CTR_Etapa_Derogado.Id.ToString());

            var CTR_Etapa_Pdt_Prorroga = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(nameof(INombre.Nombre), n_ctv_estado_pdt_prorrogar);
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_Derogado, CTR_Etapa_Pdt_Prorroga.Id.ToString());


            var CTR_Etapa_Finalizacion = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(nameof(INombre.Nombre), n_ctv_estado_finalizado);
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_Finalizacion, CTR_Etapa_Finalizacion.Id.ToString());

            var CTR_Etapa_Cancelado = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(nameof(INombre.Nombre), n_ctv_estado_cancelado);
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_Cancelado, CTR_Etapa_Cancelado.Id.ToString());


        }

        private static void Estados(ContextoSe contexto)
        {
            new EstadoDeUnContratoDtm { Nombre = n_ctv_estado_pdt_activar, Inicial = true, Terminado = false, Cancelado = false, Orden = 10 }.PersistirEstado(contexto);
            new EstadoDeUnContratoDtm { Nombre = n_ctv_estado_vigente, Inicial = false, Terminado = false, Cancelado = false, Orden = 20 }.PersistirEstado(contexto);
            new EstadoDeUnContratoDtm { Nombre = n_ctv_estado_pdt_prorrogar, Inicial = false, Terminado = false, Cancelado = false, Orden = 30 }.PersistirEstado(contexto);
            new EstadoDeUnContratoDtm { Nombre = n_ctv_estado_finalizado, Inicial = false, Terminado = true, Cancelado = false, Orden = 50 }.PersistirEstado(contexto);
            new EstadoDeUnContratoDtm { Nombre = n_ctv_estado_derogado, Inicial = false, Terminado = true, Cancelado = false, Orden = 55 }.PersistirEstado(contexto);
            new EstadoDeUnContratoDtm { Nombre = n_ctv_estado_cancelado, Inicial = false, Terminado = true, Cancelado = true, Orden = 60 }.PersistirEstado(contexto);
        }


        public const string n_ctv_tran_iniciar = "CTV: Iniciar";
        public const string n_ctv_tran_devolver = "CTV: Devolver";
        public const string n_ctv_tran_cancelar = "CTV: Cancelar";
        public const string n_ctv_tran_reactivar = "CTV: Reactivar";
        public const string n_ctv_tran_pasar_a_prorrogar = "CTV: Pasar a prorrogar";
        public const string n_ctv_tran_prorrogar_explicitamente = "CTV: Prorrogar";
        public const string n_ctv_tran_prorrogar_tacitamente = "CTV: Renovación tácita";
        public const string n_ctv_tran_no_prorrogar = "CTV: No Prorrogar";
        public const string n_ctv_tran_finalizar = "CTV: Finalizar";
        public const string n_ctv_tran_derogar = "CTV: Derogar";


        private static void DefinirVariables(ContextoSe contexto)
        {
            enumNegocio.Contrato.ResetearParametro(contexto, enumParametrosDeContratos.CTR_Porcentaje_Aviso, "80");
            enumNegocio.Contrato.ResetearParametro(contexto, enumParametrosDeContratos.CTR_Porcentaje_Bloqueo, "100");

            var alIniciar = new TransicionPorMotivo
            {
                Motivo = VariablesDeContratos.enumMotivoTransicion.Iniciar.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_vigente).Id,
                IdTransicion = enumNegocio.Contrato.Transicion(contexto, n_ctv_tran_iniciar, n_ctv_estado_pdt_activar, n_ctv_estado_vigente, delSistema: false).Id
            };

            var alPdtProrrogar = new TransicionPorMotivo
            {
                Motivo = VariablesDeContratos.enumMotivoTransicion.PdtProrroga.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_vigente).Id,
                IdTransicion = enumNegocio.Contrato.Transicion(contexto, n_ctv_tran_pasar_a_prorrogar, n_ctv_estado_vigente, n_ctv_estado_pdt_prorrogar, delSistema: true).Id
            };

            var alProrrogar = new TransicionPorMotivo
            {
                Motivo = VariablesDeContratos.enumMotivoTransicion.Prorrogar.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_pdt_prorrogar).Id,
                IdTransicion = enumNegocio.Contrato.Transicion(contexto, n_ctv_tran_prorrogar_tacitamente, n_ctv_estado_pdt_prorrogar, n_ctv_estado_vigente, delSistema: true).Id
            };

            var alFinalizar = new TransicionPorMotivo
            {
                Motivo = VariablesDeContratos.enumMotivoTransicion.Finalizar.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_vigente).Id,
                IdTransicion = enumNegocio.Contrato.Transicion(contexto, n_ctv_tran_finalizar, n_ctv_estado_vigente, n_ctv_estado_finalizado, delSistema: true).Id
            };

            var parametro = enumNegocio.Contrato.LeerCrearParametro(contexto, enumParametrosDeContratos.CTR_Aplicar_Transicion, valor: "");
            var lista = new List<TransicionPorMotivo> { alPdtProrrogar, alProrrogar, alIniciar, alFinalizar };
            if (!parametro.Valor.IsNullOrEmpty())
            {
                var lista2 = JsonConvert.DeserializeObject<List<TransicionPorMotivo>>(parametro.Valor);
                if (lista2 is not null) foreach (var item in lista2)
                        if (lista.FirstOrDefault(x => x.IdEstado == item.IdEstado && x.IdTransicion == item.IdTransicion && x.Motivo == item.Motivo) is null)
                            lista.Add(item);
            }

            enumNegocio.Contrato.ResetearParametro(contexto, enumParametrosDeContratos.CTR_Aplicar_Transicion, JsonConvert.SerializeObject(lista));

            CacheDeVariable.BorrarVariable(contexto, enumParametrosDeContratos.CTR_Porcentaje_Bloqueo);
            CacheDeVariable.BorrarVariable(contexto, enumParametrosDeContratos.CTR_Porcentaje_Aviso);
            CacheDeVariable.BorrarVariable(contexto, enumEtapasDeContratos.CTR_Etapa_En_Elaboracion);
            CacheDeVariable.BorrarVariable(contexto, enumEtapasDeContratos.CTR_Etapa_Cancelado);
            CacheDeVariable.BorrarVariable(contexto, enumEtapasDeContratos.CTR_Etapa_Derogado);
            CacheDeVariable.BorrarVariable(contexto, enumEtapasDeContratos.CTR_Etapa_Finalizacion);
            CacheDeVariable.BorrarVariable(contexto, enumEtapasDeContratos.CTR_Etapa_Pdt_Prorroga);
            CacheDeVariable.BorrarVariable(contexto, enumEtapasDeContratos.CTR_Etapa_Vigente);
            CacheDeVariable.BorrarVariable(contexto, "CTR_Transiciones_Para_Finalizar");
            CacheDeVariable.BorrarVariable(contexto, "CTR_Transiciones_Para_Iniciar");
            CacheDeVariable.BorrarVariable(contexto, "CTR_Transiciones_Para_Pdt_Prorrogar");
            CacheDeVariable.BorrarVariable(contexto, "CTR_Transiciones_Para_Prorrogar");


        }


        private static void Transiciones(ContextoSe contexto)
        {
            //Iniciar: Pdt. de activar --> vigente
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctv_tran_iniciar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_pdt_activar).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_vigente).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = null,
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //cancelar: Pdt. de activar --> cancelado
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctv_tran_cancelar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_pdt_activar).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_cancelado).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Motivo de cancelación",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //reactivar: cancelado --> Pdt. de activar
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctv_tran_reactivar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_cancelado).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_pdt_activar).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = null,
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //pasar a pendiente de prorrogar: vigente --> pdt de prorroga
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctv_tran_pasar_a_prorrogar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_vigente).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_pdt_prorrogar).Id,
                DelSistema = true,
                ConObservacion = false,
                Asunto = null,
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Prorrogar explicitamente: pdt. de prorrogar --> vigente
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctv_tran_prorrogar_explicitamente,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_pdt_prorrogar).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_vigente).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = "Justificación de la prórroga",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Prorrogar tácitamente: pdt. de prorrogar --> vigente
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctv_tran_prorrogar_tacitamente,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_pdt_prorrogar).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_vigente).Id,
                DelSistema = true,
                ConObservacion = false,
                Asunto = "",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //No prorrogar: pdt. de prorrogar --> Finalizado
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctv_tran_no_prorrogar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_pdt_prorrogar).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_finalizado).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Motivo de no prorrogar",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Derogar: Vigente --> Derogado
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctv_tran_derogar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_vigente).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_derogado).Id,
                DelSistema = false,
                ConObservacion = false,
                Asunto = "Motivo de derogación",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Derogar: Vigente --> devolución
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctv_tran_devolver,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_vigente).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_pdt_activar).Id,
                DelSistema = false,
                ConObservacion = true,
                Asunto = "Motivo de devolución",
                Activo = true
            }.PersistirTransicionPorAk(contexto);

            //Derogar: Vigente --> finalizado
            new TransicionesDeUnContratoDtm
            {
                Nombre = n_ctv_tran_finalizar,
                IdOrigen = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_vigente).Id,
                IdDestino = contexto.SeleccionarEstado<EstadoDeUnContratoDtm>(n_ctv_estado_finalizado).Id,
                DelSistema = true,
                ConObservacion = false,
                Asunto = "",
                Activo = true
            }.PersistirTransicionPorAk(contexto);
        }

        private static void Acciones(ContextoSe contexto)
        {
            var gestor = GestorDeAcciones.Gestor(contexto, contexto.Mapeador);
            InzAcciones.CrearAccionesDeContratos(gestor);
            var jsonVacio = "[]";
            var transicion = enumNegocio.Contrato.Transicion(contexto, n_ctv_tran_cancelar);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Contrato, transicion, AccionesDeContratos.N_TransitarSolicitudSiTodosSusContratosVntCancelados, enumMomentoDeEjecucion.D, jsonVacio, 10, "Transita la solicitud asociada si todos los Contratos asociados lo están");

            transicion = enumNegocio.Contrato.Transicion(contexto, n_ctv_tran_reactivar);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Contrato, transicion, AccionesDeContratos.N_ValidarFechaInicioMayorQueHoy, enumMomentoDeEjecucion.A, jsonVacio, 10, "Valida que la fecha de inicio del contrato sea posterior al día de hoy");

            transicion = enumNegocio.Contrato.Transicion(contexto, n_ctv_tran_devolver);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Contrato, transicion, AccionesDeContratos.N_ValidarQueNoHayPlanificacionesDeVenta, enumMomentoDeEjecucion.A, jsonVacio, 10, AccionesDeContratos.N_ValidarQueNoHayPlanificacionesDeVenta);

        }

        public const string n_ctv_tipo_venta = "CTV: Venta de servicios";
        public const string n_ctv_sigla_venta = "VTS";
        private static void Tipos(ContextoSe contexto)
        {

            var tipoDeArchivador = new TipoDeArchivadorDtm
            {
                Nombre = "CTR: Archivador",
                ClaseDeLibro = enumClaseDeLibro.POR_CG_TIPO,
                Sigla = "D-CTR",
                IdPadre = null,
                Activo = true,
                TipoDtm = typeof(TipoDeArchivadorDtm).FullName,
                TipoDto = typeof(TipoDeArchivadorDto).FullName,
            }.PersistirPorSigla(contexto);

            var estado = enumNegocio.Contrato.Estados(contexto).First(x => x.Nombre.Equals(n_ctv_estado_pdt_activar));
            new TipoDeContratoDtm
            {
                Nombre = n_ctv_tipo_venta,
                ClaseDeContrato = enumClaseDeContrato.Venta,
                ClaseDeLibro = enumClaseDeLibro.POR_CG_TIPO,
                IdEstado = estado.Id,
                IdPadre = null,
                Activo = true,
                IdTipoArchivador = tipoDeArchivador.Id,
                TipoDtm = typeof(TipoDeContratoDtm).FullName,
                TipoDto = typeof(TipoDeContratoDto).FullName,
                Sigla = n_ctv_sigla_venta
            }.PersistirPorNombre(contexto, parametros: new Dictionary<string, object> { { nameof(ContratoDtm.ClaseDeContrato), enumClaseDeContrato.Venta } });


        }
       
        public static void InicializarTiposDeContrato(ContextoSe contexto)
        {
            Type tipoDelaPropiedad = typeof(TipoDeContratoDtm).TipoDeLaPropiedad(nameof(TipoDeContratoDtm.IdTipoArchivador));

            if (tipoDelaPropiedad == null)
                throw new Exception("El nivel de migración no se corresponde con la versión del software");

            var tran = contexto.IniciarTransaccion();
            try
            {
                var tipoDeArchivador = new TipoDeArchivadorDtm
                {
                    Nombre = "CTR: Archivador",
                    ClaseDeLibro = enumClaseDeLibro.POR_CG_TIPO,
                    Sigla = "D-CTR",
                    IdPadre = null,
                    Activo = true,
                    TipoDtm = typeof(TipoDeArchivadorDtm).FullName,
                    TipoDto = typeof(TipoDeArchivadorDto).FullName,
                }.PersistirPorSigla(contexto);

                if (tipoDelaPropiedad.Name == typeof(int?).Name)
                {
                    contexto.Database.ExecuteSql($@"update {Esquemas.JURIDICO}.{Tablas.CONTRATO}_{Sufijo.TIPO} 
                                                       set {ICampos.ID_TIPO_ARCHIVADOR} = {tipoDeArchivador.Id}
                                                       where {ICampos.ID_TIPO_ARCHIVADOR} = null");
                }

                var registros = contexto.RegistrosSinVinculosAl<ContratoDtm>(enumNegocio.Archivador);
                foreach (var registro in registros)
                {
                    var nuevo = ExtensorDeArchivadores.CrearArchivador(contexto, enumNegocio.Contrato, contexto.SeleccionarPorId<ContratoDtm>(registro.Id), "contractual", tipoDeArchivador.Id);
                    GestorDeVinculos.Vincular(contexto, enumNegocio.Contrato, enumNegocio.Archivador, registro.Id, nuevo.Id);
                }
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;

            }
        }


    }
}
