using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;
using ServicioDeDatos.Tarea;
using ModeloDeDto.Tarea;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using GestoresDeNegocio.SistemaDocumental;
using ServicioDeDatos.Terceros;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.Expediente;
using System;
using System.Text;
using Gestor.Errores;
using ServicioDeDatos.Gastos;
using System.Threading.Tasks;
using static ServicioDeDatos.Elemento.Enumerados;
using ServicioDeDatos.RegistroEs;

namespace GestoresDeNegocio.Tarea
{

    public class GestorDeTareas : GestorDeElementos<ContextoSe, TareaDtm, TareaDto>, IImportadorDelCorreo, ITotalizador<TotalesDeTareas>
    {
        public class MapearTarea : Profile
        {
            public MapearTarea()
            {
                CreateMap<TareaDtm, TareaDto>()
                .DtmToDto();

                CreateMap<TareaDto, TareaDtm>()
                .DtoToDtm()
                .ForMember(dtm => dtm.Archivador, dto => dto.Ignore())
                .ForMember(dtm => dtm.FacturaEmt, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.Tarea;

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeTarea.Gestor(Contexto, Contexto.Mapeador);

        public GestorDeTareas(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeTareas Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTareas(contexto, mapeador);
        }

        protected override IQueryable<TareaDtm> AplicarJoins(IQueryable<TareaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Archivador)
                .Include(x => x.Solicitante)
                .Include(x => x.Responsable);
            return consulta;
        }

        protected override IQueryable<TareaDtm> AplicarFiltros(IQueryable<TareaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            if (filtros.Any(x => x.Clausula.ToLower() == ltrDeUnaTarea.IdFacturaEmt.ToLower() || (x.Clausula.ToLower() == ltrDeUnaTarea.Facturada.ToLower() && x.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                                  || x.Clausula.ToLower() == ltrDeUnaTarea.PrioridadesDeTarea.ToLower()))
            {
                //var filtroEstado = filtros.FirstOrDefault(x => x.Clausula.Equals(ltrParametrosNeg.QueMostrar, StringComparison.InvariantCultureIgnoreCase));
                //if (filtroEstado != null) filtroEstado.Valor = ltrParametrosNeg.MostrarTodos.ToString();
                parametros.AplicarFiltroQueMostrar = false;
            }
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltrarPorPlfDeInicio(Contexto, filtros);
            consulta = consulta.FiltrarPorPlfDeFin(Contexto, filtros);
            consulta = consulta.FiltroDeVinculadosA(Contexto, filtros);
            consulta = consulta.FiltrarParaVincular(Contexto, filtros);
            consulta = consulta.ExcluirTareasYaRelacionadasConExpediente(Contexto, filtros);
            consulta = consulta.FiltroPorPresupuestos(Contexto, filtros);
            consulta = consulta.FiltroPorExpedientes(Contexto, filtros);
            consulta = consulta.FiltrosDeSolicitantes(Contexto, filtros);
            consulta = consulta.FiltrosDeResponsables(Contexto, filtros);
            consulta = consulta.FiltrosDeFacturas(Contexto, filtros, parametros);
            consulta = consulta.FiltroSiHayDependenciaDe(filtros, filtrarPor: ltrDeUnaTarea.IdResponsable, filtroDeAsociacion: ltrDeUnaTarea.Asignacion, parametros, aplicarFiltroDeEstado: true);
            consulta = consulta.FiltroPorPrioridad(filtros);
            consulta = consulta.FiltroConPrioridad(filtros);
            consulta = consulta.FiltroPorEtapa(filtros);
            consulta = consulta.ExcluirCuandoRealizar(Contexto, filtros);
            consulta = consulta.FiltroPorTareasAnterioresA(Contexto, filtros);
            consulta = consulta.FiltroPorTareasPosterioresA(Contexto, filtros);
            return consulta;
        }

        protected override void AntesDeMapearElRegistroParaInsertar(TareaDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(elemento, opciones);
            opciones.Parametros[nameof(TareaDto.IdArchivoAlCrear)] = elemento.IdArchivoAlCrear;
        }

        protected override void AntesDePersistir(TareaDtm tarea, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(tarea, parametros);
            tarea.ClaseDeTarea = ((TipoDeTareaDtm)parametros.TipoConFujo).ClaseDeTarea;
            if (parametros.Insertando)
            {
                var tipo = (TipoDeTareaDtm)parametros.Parametros[nameof(TipoConFlujoDtm)];
                if (tarea.IdArchivador == null && tipo.IdTipoArchivador != null)
                    tarea.IdArchivador = CrearArchivador(tarea, (int)tipo.IdTipoArchivador);
                tarea.IdFacturaEmt = null;
            }
            if (parametros.Insertando) tarea.IdFacturaEmt = null;
            else if (parametros.AccionQueSeEjecuta != nameof(ltrDeUnaTarea.Accion_IncluirEnLaFactura))
            {
                tarea.IdFacturaEmt = parametros.AccionQueSeEjecuta == nameof(ltrDeUnaTarea.Accion_ExluirDeLaFactura)
                ? null
                : ((TareaDtm)parametros.registroEnBd).IdFacturaEmt;
            }
        }

        protected override void DespuesDePersistir(TareaDtm tarea, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(tarea, parametros);
            if (parametros.Insertando && tarea.Tipo<TipoDeTareaDtm>(Contexto).CopiarDireccionDelSolicitante)
            {
                AsociarDireccion(tarea, parametros);
                if (tarea.IdArchivador != null) GestorDeVinculos.Vincular(Contexto, enumNegocio.Tarea, enumNegocio.Archivador, tarea.Id, (int)tarea.IdArchivador);
            }

            if (parametros.Insertando)
            {
                var idExpediente = parametros.Parametros.LeerValor(ltrDeUnaTarea.VincularAlExpediente, 0);

                if (idExpediente == 0)
                {
                    var idExpedienteVinculado = parametros.Parametros.LeerValor<int?>(ltrDeUnaTarea.IdExpediente, null);
                    if (idExpedienteVinculado.Entero() > 0)
                        idExpediente = (int)idExpedienteVinculado;
                }

                if (idExpediente > 0)
                {
                    var expediente = Contexto.SeleccionarPorId<ExpedienteDtm>(idExpediente);
                    GestorDeVinculos.Vincular(Contexto, expediente, tarea);
                }

                var idRegistro = parametros.Parametros.LeerValor<int>(ltrDeUnaTarea.IdRegistroEs, 0);
                if (idRegistro > 0)
                {
                    var registro = Contexto.SeleccionarPorId<RegistroEsDtm>(idRegistro);
                    GestorDeVinculos.Vincular(Contexto, registro, tarea);
                }

                var idArchivoAlCrear = parametros.Parametros.LeerValor<int?>(nameof(TareaDto.IdArchivoAlCrear), null);
                if (idArchivoAlCrear.Entero() > 0)
                    GestorDeVinculos.Vincular(Contexto, Negocio, enumNegocio.Archivos, tarea.Id, (int)idArchivoAlCrear);
            }
        }

        protected override TareaDtm AntesDeTransitar(TareaDtm tarea, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            tarea = base.AntesDeTransitar(tarea, transicion, parametros);

            if (tarea.IdFacturaEmt is not null)
            {
                var factura = Contexto.SeleccionarPorId<FacturaEmtDtm>(tarea.IdFacturaEmt.Entero(), aplicarPermisos: false);
                if (!factura.Etapas().Contains(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura))
                    GestorDeErrores.Emitir($"No puede transitar la tarea '{tarea.Referencia}' por estar incluida en la factura '{factura.Referencia}'");
            }

            if (transicion.EntreEtapas(enumEtapasDeTareas.TAR_Etapa_Inicial.Estados(), enumEtapasDeTareas.TAR_Etapa_Asignada.Estados()))
                tarea.AntesDeAsignar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeTareas.TAR_Etapa_Asignada.Estados(), enumEtapasDeTareas.TAR_Etapa_En_Resolucion.Estados()) ||
                transicion.EntreEtapas(enumEtapasDeTareas.TAR_Etapa_Inicial.Estados(), enumEtapasDeTareas.TAR_Etapa_En_Resolucion.Estados()))
                tarea.AntesDeComenzar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeTareas.TAR_Etapa_En_Resolucion.Estados(), enumEtapasDeTareas.TAR_Etapa_Validacion.Estados()))
                tarea.AntesDeFinalizar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeTareas.TAR_Etapa_Asignada.Estados(), enumEtapasDeTareas.TAR_Etapa_Inicial.Estados()))
                tarea.AntesDeDesasignar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeTareas.TAR_Etapa_En_Resolucion.Estados(), enumEtapasDeTareas.TAR_Etapa_Asignada.Estados()) ||
                transicion.EntreEtapas(enumEtapasDeTareas.TAR_Etapa_En_Resolucion.Estados(), enumEtapasDeTareas.TAR_Etapa_Inicial.Estados()))
                tarea.AntesDeAnularLaEjecucion(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeTareas.TAR_Etapa_Validacion.Estados(), enumEtapasDeTareas.TAR_Etapa_En_Resolucion.Estados()))
                tarea.AntesDeNoAceptarLaFinalizacion(Contexto, parametros);
            if (transicion.EntreEtapas(enumEtapasDeTareas.TAR_Etapa_Validacion.Estados(), enumEtapasDeTareas.TAR_Etapa_Terminada.Estados()))
                tarea.AntesDeTerminarTarea(Contexto, parametros);

            return tarea;
        }

        protected override TareaDtm DespuesDeTransitar(TareaDtm tarea, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            tarea = base.DespuesDeTransitar(tarea, transicion, parametros);

            if (transicion.EntreEtapas(enumEtapasDeTareas.TAR_Etapa_En_Resolucion.Estados(), enumEtapasDeTareas.TAR_Etapa_Validacion.Estados()))
                tarea.TrasFinalizar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeTareas.TAR_Etapa_Inicial.Estados(), enumEtapasDeTareas.TAR_Etapa_Asignada.Estados()))
                tarea.TrasAsignar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeTareas.TAR_Etapa_Asignada.Estados(), enumEtapasDeTareas.TAR_Etapa_Inicial.Estados()))
            {
                tarea.TrasDesasignar(Contexto, parametros);
                tarea = tarea.QuitarResponsable();
                tarea = tarea.Modificar(Contexto, esUnaAccion: true);
            }

            return tarea;
        }

        private void AsociarDireccion(TareaDtm tarea, ParametrosDeNegocio parametros)
        {
            var presentador = (InterlocutorDtm)parametros.Parametros[nameof(InterlocutorDtm)];
            var direcciones = GestorDeDirecciones.LeerRegistros(Contexto, enumNegocio.Interlocutor, presentador.Id).ToList();
            var b = false;
            var direccionFiscal = direcciones.Find(d => d.Calificador == enumCalificadorDireccion.fiscal);
            if (direccionFiscal != null)
            {
                direccionFiscal.Calificador = enumCalificadorDireccion.contacto;
                GestorDeDirecciones.AsociarDireccion(Contexto, enumNegocio.Tarea, tarea.Id, direccionFiscal);
                b = true;
            }
            else
                foreach (var direccion in direcciones)
                {
                    direccion.Calificador = enumCalificadorDireccion.contacto;
                    GestorDeDirecciones.AsociarDireccion(Contexto, enumNegocio.Tarea, tarea.Id, direccion);
                    b = true;
                    break;
                }

            if (!b)
            {
                if (presentador.EsPersona)
                    direcciones = GestorDeDirecciones.LeerRegistros(Contexto, enumNegocio.Persona, (int)presentador.IdPersona).ToList();
                else
                    direcciones = GestorDeDirecciones.LeerRegistros(Contexto, enumNegocio.Sociedad, (int)presentador.IdSociedad).ToList();
                foreach (var direccion in direcciones)
                {
                    direccion.Calificador = enumCalificadorDireccion.contacto;
                    GestorDeDirecciones.AsociarDireccion(Contexto, enumNegocio.Tarea, tarea.Id, direccion);
                    b = true;
                    break;
                }
            }
        }

        private int CrearArchivador(TareaDtm tarea, int idTipo)
        {
            var archivador = new ArchivadorDtm();
            archivador.IdCg = tarea.IdCg;
            archivador.IdTipo = idTipo;
            archivador.Nombre = $"Documentación de la tarea: {tarea.Referencia}";
            GestorDeArchivadores.Gestor(Contexto, Contexto.Mapeador).PersistirRegistro(archivador, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            return archivador.Id;
        }

        protected override TareaDto DespuesDePersistirElementoDto(TareaDto tareaDto, TareaDtm tareaDtm, ParametrosDeNegocio parametros)
        {
            tareaDto = base.DespuesDePersistirElementoDto(tareaDto, tareaDtm, parametros);
            return tareaDto;
        }

        protected override IQueryable<TareaDtm> AplicarSeguridad(IQueryable<TareaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<TareaDtm, TipoDeTareaDtm, PermisoDeLaTareaDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<TareaDtm, PermisoDeLaTareaDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void DespuesDeMapearElElemento(TareaDtm tarea, TareaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(tarea, elemento, parametros);
            var tipo = Contexto.SeleccionarPorId<TipoDeTareaDtm>(tarea.IdTipo);
            elemento.UsaPlanificacion = tipo.UsaPlanificacion;
            elemento.EsFacturable = tipo.EsFacturable;

            if (parametros.CargarLista)
                return;

            if (parametros.LeerPorId)
            {
                var expedientes = tarea.Vinculados<ExpedienteDtm>(Contexto);
                if (expedientes.Count == 1)
                {
                    elemento.IdExpediente = expedientes[0].Id;
                    elemento.Expediente = expedientes[0].Expresion;
                }
                else if (expedientes.Count > 1)
                {
                    foreach (var expediente in expedientes) elemento.Expediente = elemento.Expediente.IsNullOrEmpty() ? $"{elemento.Expediente}" : $"{elemento.Expediente}, {expediente.Referencia}";
                }
                else elemento.Expediente = "";

                elemento.Prioridad = tarea.Prioridad == null ? enumPrioridad.NoDefinida : ((enumPrioridad)tarea.Prioridad);
            }
            else if (parametros.LeerDatosParaElGridOParaExportar)
            {

                var tareaAnterior = tarea.TareasAnteriores(Contexto)?.FirstOrDefault();
                if (tareaAnterior != null)
                {
                    elemento.DespesDe = tareaAnterior.Referencia;
                    elemento.IdTareaAnterior = tareaAnterior.Id;
                }

                var tareaPosterior = tarea.TareasPosteriores(Contexto)?.FirstOrDefault();
                if (tareaPosterior != null)
                {
                    elemento.AntesQue = tareaPosterior.Referencia;
                    elemento.IdTareaPosterior = tareaPosterior.Id;
                }


                if (parametros.ColumnasDelGrid.Any(item => item == nameof(elemento.Durabilidad).ToLowerInvariant() ||
                                            item == nameof(elemento.Planificada).ToLowerInvariant() ||
                                            item == nameof(elemento.Ejecutada).ToLowerInvariant()))

                {
                    var planificacion = elemento.UsaPlanificacion ? tarea.Planificacion(Contexto, errorSiNoHay: false) : null;
                    if (planificacion is not null)
                    {
                        elemento.Planificada = planificacion.PlfDeInicio?.ToString("dd-MM-yyyy") + " - " + planificacion.PlfDeFin?.ToString("dd-MM-yyyy");
                        elemento.Ejecutada = planificacion.Iniciada?.ToString("dd-MM-yyyy") + " - " + planificacion.Finalizada?.ToString("dd-MM-yyyy");
                        elemento.Durabilidad = planificacion.Duracion?.Formatear() + " - " + planificacion.MedidoEn?.ToString();
                    }

                }
            }


            if (parametros.CargarGridDeRelacion && parametros.Filtros.Any(x => x.Clausula == ltrDeUnaTarea.IdFacturaEmt) && tarea.IdFacturaEmt.HasValue)
            {
                elemento.FacturaEmt = tarea.FacturaEmt(Contexto).Expresion;
                var planificacion = elemento.UsaPlanificacion ? tarea.Planificacion(Contexto, errorSiNoHay: false) : null;
                elemento.Facturado = planificacion?.Duracion;
                elemento.Medido = planificacion?.MedidoEn;
            }
        }

        public static void ExluirDeLaFactura(ContextoSe contexto, int id)
        {
            var tarea = contexto.SeleccionarPorId<TareaDtm>(id, aplicarPermisos: true);
            if (tarea.IdFacturaEmt is null)
                GestorDeErrores.Emitir($"No se puede excluir la tarea '{tarea.Referencia}' de ninguna factura, por no estar facturada");

            var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(tarea.IdFacturaEmt.Entero(), aplicarPermisos: true);
            if (factura.Etapas().Contains(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura))
            {
                tarea.IdFacturaEmt = null;
                tarea.Modificar(contexto, accionEjecutada: ltrDeUnaTarea.Accion_ExluirDeLaFactura);
                factura.CrearTraza(contexto, $"Tarea '{tarea.Referencia}' excluida", $"El usuario '{contexto.DatosDeConexion.Login}' ha excluido la tarea '{tarea.Referencia}' de la factura");
                tarea.CrearTraza(contexto, $"Anulación de facturación", $"El usuario '{contexto.DatosDeConexion.Login}' ha excluido la tarea de la factura '{factura.Referencia}' ");
                return;
            }
            GestorDeErrores.Emitir($"No se puede excluir la tarea '{tarea.Referencia}' de la factura '{factura.Referencia}' por no estar en la etapa de prefacturación");
        }

        public IUsaTipoConCG ImportarDelCorreo(int idCg, int idTipo, string nombre, string descripcion, Dictionary<string, object> parametros)
        {
            var tarea = (TareaDtm)ExtensorDeElementosDeUnProceso.NuevoDtm(Negocio.TipoDtm(), idCg, idTipo, nombre, descripcion, parametros);
            return tarea;
        }

        public async Task<TotalesDeTareas> ObtenerTotalesAsync(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            return await Task.Run(() => ObtenerTotales(filtros, posicion, cantidad));
        }

        public TotalesDeTareas ObtenerTotales(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            var tiposPlanificados = Negocio.Tipos(Contexto).Where(tipo => ((TipoDeTareaDtm)tipo).UsaPlanificacion == true);
            if (!tiposPlanificados.Any())
            {
                return new TotalesDeTareas
                {
                    Procesados = 0,
                    Totales = "No hay tipos planificados en la BD"
                };
            }

            var filtro = filtros.FirstOrDefault(filtro => filtro.Clausula == nameof(IElementoConTipo.IdTipo));
            if (filtro == null) filtros.Add(new ClausulaDeFiltrado
            {
                Clausula = nameof(IElementoConTipo.IdTipo),
                Criterio = enumCriteriosDeFiltrado.esAlgunoDe,
                Valor = string.Join(",", tiposPlanificados.Select(tipo => tipo.Id.ToString()).ToArray())
            });
            else
            {
                var tipo = Contexto.SeleccionarPorId<TipoDeTareaDtm>(filtro.Valor.Entero());
                if (!tipo.UsaPlanificacion)
                    return new TotalesDeTareas
                    {
                        Procesados = 0,
                        Totales = $"el tipo '{tipo.Nombre}' no usa planificación"
                    };

            }

            var tareas = Contexto.SeleccionarTodos<TareaDtm>(filtros, parametros: new Dictionary<string, object> {
                { ltrParametrosNeg.PosicionInicial, posicion},
                { ltrParametrosNeg.CantidadPorLeer, cantidad},
                { ltrParametrosNeg.Peticion, enumPeticion.epTotales},
            });
            var totales = new TotalesDeTareas();

            var duraciones = new Dictionary<enumDurabilidad, decimal> { { enumDurabilidad.Minutos, 0 }, { enumDurabilidad.Horas, 0 }, { enumDurabilidad.Jornadas, 0 }, { enumDurabilidad.Dias, 0 } };
            foreach (var tarea in tareas)
            {
                var planificacion = tarea.Planificacion(Contexto, errorSiNoHay: false);
                if (planificacion == null || planificacion.MedidoEn == null || planificacion.Duracion == null) continue;
                if (planificacion.MedidoEn == enumDurabilidad.Jornadas)
                    duraciones[enumDurabilidad.Jornadas] = duraciones[enumDurabilidad.Jornadas] + (decimal)planificacion.Duracion;
                else if (planificacion.MedidoEn == enumDurabilidad.Dias)
                    duraciones[enumDurabilidad.Dias] = duraciones[enumDurabilidad.Dias] + (decimal)planificacion.Duracion;
                else if (planificacion.MedidoEn == enumDurabilidad.Horas)
                    duraciones[enumDurabilidad.Horas] = duraciones[enumDurabilidad.Horas] + (decimal)planificacion.Duracion;
                else if (planificacion.MedidoEn == enumDurabilidad.Minutos)
                    duraciones[enumDurabilidad.Minutos] = duraciones[enumDurabilidad.Minutos] + (decimal)planificacion.Duracion;
            }
            var totalJornadas =
                duraciones[enumDurabilidad.Dias].DuracionEn(enumDurabilidad.Dias, enumDurabilidad.Jornadas) +
                duraciones[enumDurabilidad.Jornadas].DuracionEn(enumDurabilidad.Jornadas, enumDurabilidad.Jornadas) +
                duraciones[enumDurabilidad.Horas].DuracionEn(enumDurabilidad.Horas, enumDurabilidad.Jornadas) +
                duraciones[enumDurabilidad.Minutos].DuracionEn(enumDurabilidad.Minutos, enumDurabilidad.Jornadas);

            totales.Totales = $"{enumDurabilidad.Dias.Descripcion()}: {duraciones[enumDurabilidad.Dias].Formatear(alineacion: false)}{Environment.NewLine}" +
                   $"{enumDurabilidad.Jornadas.Descripcion()}: {duraciones[enumDurabilidad.Jornadas].Formatear(alineacion: false)}{Environment.NewLine}" +
                   $"{enumDurabilidad.Horas.Descripcion()}: {duraciones[enumDurabilidad.Horas].Formatear(alineacion: false)}{Environment.NewLine}" +
                   $"{enumDurabilidad.Minutos.Descripcion()}: {duraciones[enumDurabilidad.Minutos].Formatear(alineacion: false)}{Environment.NewLine}" +
                   $"{Environment.NewLine}" +
                   $"Total en {enumDurabilidad.Jornadas.Descripcion()}: {totalJornadas.Formatear(alineacion: false)}{Environment.NewLine}" +
                   $"{enumDurabilidad.Jornadas.Descripcion()} por tarea: {(tareas.Count() == 0 ? 0.Formatear(alineacion: false) : (totalJornadas / tareas.Count()).Formatear(alineacion: false))}";
            totales.Procesados = tareas.Count();
            totales.TotalesPorEjecutor = FormatearTotalesPorEjecutor(tareas);
            totales.TotalesPorSolicitante = FormatearTotalesPorSolicitante(tareas);
            totales.TotalesPorExpediente = FormatearTotalesPorExpediente(tareas);
            return totales;
        }

        private string FormatearTotalesPorSolicitante(List<TareaDtm> tareas)
        {
            if (!tareas.Any()) return string.Empty;

            var grupos = tareas
                .SoloConRelacionada(t => t.Planificacion(Contexto, errorSiNoHay: false), plf => (plf.EnJornadas() ?? 0) > 0)
                .AgruparPorRelacionada(t => t.Solicitante(Contexto), s => s.Nombre)
                .AsEnumerable()
                .Select(g => new
                {
                    Nombre = g.Key,
                    Tareas = g.Count(),
                    TotalJornadas = g.Sum(t => t.Planificacion(Contexto, errorSiNoHay: false)?.EnJornadas() ?? 0m),
                    MediaJornadas = g.Average(t => t.Planificacion(Contexto, errorSiNoHay: false)?.EnJornadas() ?? 0m)
                })
                .OrderByDescending(g => g.TotalJornadas)
                .ToList();

            if (!grupos.Any()) return string.Empty;

            const int anchoNombre = 40;
            const int anchoNum = 10;
            const int anchoJornadas = 16;

            var sb = new StringBuilder();
            sb.AppendLine(
                $"{"Solicitante".PadRight(anchoNombre)}" +
                $"{"Tareas".PadLeft(anchoNum)}" +
                $"{"Total Jorn.".PadLeft(anchoJornadas)}" +
                $"{"Media Jorn.".PadLeft(anchoJornadas)}"
            );
            sb.AppendLine(new string('-', anchoNombre + anchoNum + anchoJornadas * 2));

            foreach (var grupo in grupos)
            {
                var nombre = grupo.Nombre;
                if (nombre.Length > anchoNombre) nombre = nombre.Substring(0, anchoNombre - 1) + "…";

                sb.AppendLine(
                    $"{nombre.PadRight(anchoNombre)}" +
                    $"{grupo.Tareas.ToString().PadLeft(anchoNum)}" +
                    $"{grupo.TotalJornadas.Formatear(alineacion: false).PadLeft(anchoJornadas)}" +
                    $"{grupo.MediaJornadas.Formatear(alineacion: false).PadLeft(anchoJornadas)}"
                );
            }

            return sb.ToString();
        }

        private string FormatearTotalesPorEjecutor(List<TareaDtm> tareas)
        {
            if (!tareas.Any()) return string.Empty;

            var grupos = tareas
                .SoloConRelacionada(t => t.Responsable(Contexto))
                .SoloConRelacionada(t => t.Planificacion(Contexto, errorSiNoHay: false), plf => (plf.EnJornadas() ?? 0) > 0)
                .AgruparPorRelacionada(t => t.Responsable(Contexto), u => u.Login)
                .AsEnumerable()
                .Select(g => new
                {
                    Login = g.Key,
                    Tareas = g.Count(),
                    TotalJornadas = g.Sum(t => t.Planificacion(Contexto, errorSiNoHay: false)?.EnJornadas() ?? 0m),
                    MediaJornadas = g.Average(t => t.Planificacion(Contexto, errorSiNoHay: false)?.EnJornadas() ?? 0m)
                })
                .OrderByDescending(g => g.TotalJornadas)
                .ToList();

            if (!grupos.Any()) return string.Empty;

            const int anchoLogin = 30;
            const int anchoNum = 10;
            const int anchoJornadas = 16;

            var sb = new StringBuilder();
            sb.AppendLine(
                $"{"Ejecutor".PadRight(anchoLogin)}" +
                $"{"Tareas".PadLeft(anchoNum)}" +
                $"{"Total Jorn.".PadLeft(anchoJornadas)}" +
                $"{"Media Jorn.".PadLeft(anchoJornadas)}"
            );
            sb.AppendLine(new string('-', anchoLogin + anchoNum + anchoJornadas * 2));

            foreach (var grupo in grupos)
            {
                var login = grupo.Login;
                if (login.Length > anchoLogin) login = login.Substring(0, anchoLogin - 1) + "…";

                sb.AppendLine(
                    $"{login.PadRight(anchoLogin)}" +
                    $"{grupo.Tareas.ToString().PadLeft(anchoNum)}" +
                    $"{grupo.TotalJornadas.Formatear(alineacion: false).PadLeft(anchoJornadas)}" +
                    $"{grupo.MediaJornadas.Formatear(alineacion: false).PadLeft(anchoJornadas)}"
                );
            }

            return sb.ToString();
        }

        public static int CopiarTarea(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (!parametros.ContieneClave(nameof(CopiarTareaDto.IdElemento))) GestorDeErrores.Emitir("No se ha indicado la tarea a copiar");
            if (!parametros.ContieneClave(nameof(CopiarTareaDto.IdTipo))) GestorDeErrores.Emitir("No se ha indicado el tipo de la tarea");
            if (!parametros.ContieneClave(nameof(CopiarTareaDto.IdCg))) GestorDeErrores.Emitir("No se ha indicado el cg de la tarea");
            if (!parametros.ContieneClave(nameof(CopiarTareaDto.IdSolicitante))) GestorDeErrores.Emitir("No se ha indicado el solicitante");
            if (!parametros.ContieneClave(nameof(CopiarTareaDto.Nombre))) GestorDeErrores.Emitir("No se ha indicado el asunto de la tarea");
            if (!parametros.ContieneClave(nameof(CopiarTareaDto.Descripcion))) GestorDeErrores.Emitir("No se ha indicado la descripción de la tarea");

            var idTareaOrigen = (int)(long)parametros[nameof(CopiarTareaDto.IdElemento)];
            var t1 = contexto.SeleccionarPorId<TareaDtm>(idTareaOrigen);
            var t2 = t1.Copiar(contexto, parametros);

            if (parametros.ContieneClave(nameof(CopiarTareaDto.ReferenciadaComo)))
            {
                var referenciadaComo = parametros.LeerValor(nameof(CopiarTareaDto.ReferenciadaComo), enumTareaReferenciadaComo.Seleccionar);
                if (referenciadaComo != enumTareaReferenciadaComo.Seleccionar)
                {
                    if (referenciadaComo == enumTareaReferenciadaComo.Copia)
                    {
                        t2.CrearObservacion(contexto, referenciadaComo.Descripcion(), enumNegocio.Tarea.ComponerUrlPorId(contexto, t1.Id).ToString(), new Dictionary<string, object> { { ltrDeObservaciones.CreadaPorAdminSe, true } });
                    }
                    //else
                    //if (referenciadaComo == enumTareaReferenciadaComo.Anterior)
                    //{
                    //    t1.CrearObservacion(contexto, enumTareaReferenciadaComo.Despues.Descripcion(), enumNegocio.Tarea.ComponerUrlPorId(contexto, t2.Id).ToString(), new Dictionary<string, object> { { ltrDeObservaciones.CreadaPorAdminSe, true } });
                    //    t2.CrearObservacion(contexto, referenciadaComo.Descripcion(), enumNegocio.Tarea.ComponerUrlPorId(contexto, t1.Id).ToString(), new Dictionary<string, object> { { ltrDeObservaciones.CreadaPorAdminSe, true } });
                    //}
                    //else
                    //if (referenciadaComo == enumTareaReferenciadaComo.Despues)
                    //{
                    //    t1.CrearObservacion(contexto, enumTareaReferenciadaComo.Anterior.Descripcion(), enumNegocio.Tarea.ComponerUrlPorId(contexto, t2.Id).ToString(), new Dictionary<string, object> { { ltrDeObservaciones.CreadaPorAdminSe, true } });
                    //    t2.CrearObservacion(contexto, referenciadaComo.Descripcion(), enumNegocio.Tarea.ComponerUrlPorId(contexto, t1.Id).ToString(), new Dictionary<string, object> { { ltrDeObservaciones.CreadaPorAdminSe, true } });
                    //}
                }
            }


            return t2.Id;
        }

        public static void CuandoRealizar(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (!parametros.ContieneClave(nameof(CuandoRealizarDto.IdTareaEditada))) GestorDeErrores.Emitir("No se ha indicado la tarea editada");

            var idTareaEditada = parametros.LeerValor<int>(nameof(CuandoRealizarDto.IdTareaEditada));
            var idTareaAnterior = parametros.LeerValor(nameof(CuandoRealizarDto.IdTareaAnterior), 0);
            var idTareaPosterior = parametros.LeerValor(nameof(CuandoRealizarDto.IdTareaPosterior), 0);

            if (idTareaAnterior <= 0 && idTareaPosterior <= 0)
                GestorDeErrores.Emitir("No se ha indicado ninguna tarea anterior ni posterior");

            if (idTareaAnterior > 0 && idTareaAnterior == idTareaPosterior)
                GestorDeErrores.Emitir("No se puede indicar la misma tarea como anterior y como posterior");

            var tareaEditada = contexto.SeleccionarPorId<TareaDtm>(idTareaEditada);
            var tareaAnterior = idTareaAnterior > 0 ? contexto.SeleccionarPorId<TareaDtm>(idTareaAnterior) : null;
            var tareaPosterior = idTareaPosterior > 0 ? contexto.SeleccionarPorId<TareaDtm>(idTareaPosterior) : null;

            if (!tareaEditada.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeTareas> { enumEtapasDeTareas.TAR_Etapa_Inicial, enumEtapasDeTareas.TAR_Etapa_Asignada, enumEtapasDeTareas.TAR_Etapa_En_Espera }))
                GestorDeErrores.Emitir($"Para establecer secuencialidad de resolución no se admiten tareas canceladas, terminadas o pendientes de validación; la tarea '{tareaEditada.Referencia}' está en etapa '{tareaEditada.Etapa().Nombre(minusculas: true)}'");

            ValidarEsInterventorDeLaSecuencia(contexto, tareaEditada);
            if (tareaAnterior != null) ValidarEsInterventorDeLaSecuencia(contexto, tareaAnterior);
            if (tareaPosterior != null) ValidarEsInterventorDeLaSecuencia(contexto, tareaPosterior);

            if (tareaAnterior != null)
                EnlazarSecuencia(contexto, tareaAnterior, tareaEditada);

            if (tareaPosterior != null)
                EnlazarSecuencia(contexto, tareaEditada, tareaPosterior);
        }

        // El usuario ha de poder modificar (como interventor) las tres tareas implicadas, ya que la creación/borrado de las
        // observaciones de secuencia se hace en cada una de ellas y, sin ese permiso, esa persistencia fallaría.
        private static void ValidarEsInterventorDeLaSecuencia(ContextoSe contexto, TareaDtm tarea)
        {
            if (!tarea.EsInterventor(contexto))
                GestorDeErrores.Emitir($"Sólo un interventor de la tarea '{tarea.Referencia}' puede establecer su secuencia de ejecución");
        }

        // Enlaza 'tareaAnterior' y 'tareaPosterior' como secuencia de ejecución, creando la observación correspondiente en cada una.
        private static void EnlazarSecuencia(ContextoSe contexto, TareaDtm tareaAnterior, TareaDtm tareaPosterior)
        {
            var etapasNoPermitidas = new List<enumEtapasDeTareas> { enumEtapasDeTareas.TAR_Etapa_Validacion, enumEtapasDeTareas.TAR_Etapa_Terminada, enumEtapasDeTareas.TAR_Etapa_Cancelado };

            if (tareaAnterior.EstaEnAlgunaDeLasEtapa(etapasNoPermitidas))
                GestorDeErrores.Emitir($"La tarea '{tareaAnterior.Referencia}' no puede formar parte de una secuencia de ejecución por estar en etapa '{tareaAnterior.Etapa().Nombre(minusculas: true)}'");

            if (tareaPosterior.EstaEnAlgunaDeLasEtapa(etapasNoPermitidas))
                GestorDeErrores.Emitir($"La tarea '{tareaPosterior.Referencia}' no puede formar parte de una secuencia de ejecución por estar en etapa '{tareaPosterior.Etapa().Nombre(minusculas: true)}'");

            if (tareaPosterior.ArbolDeRealizacionPosterior(contexto).Any(t => t.Id == tareaAnterior.Id))
                GestorDeErrores.Emitir($"No se puede indicar que la tarea '{tareaAnterior.Referencia}' se ha de ejecutar antes que '{tareaPosterior.Referencia}' porque se entraría en una secuencia recursiva de ejecución");

            var cuerpoEnLaAnterior = enumNegocio.Tarea.ComponerUrlPorId(contexto, tareaPosterior.Id).ToString();
            var cuerpoEnLaPosterior = enumNegocio.Tarea.ComponerUrlPorId(contexto, tareaAnterior.Id).ToString();

            CrearOCorregirObservacionDeSecuencia(contexto, tareaAnterior, tareaPosterior, enumCuandoRealizar.Anterior.Descripcion(), cuerpoEnLaAnterior);
            CrearOCorregirObservacionDeSecuencia(contexto, tareaPosterior, tareaAnterior, enumCuandoRealizar.Despues.Descripcion(), cuerpoEnLaPosterior);

            IgualarPrioridadDeLaAnteriorSiLaPosteriorEsMasUrgente(contexto, tareaAnterior, tareaPosterior);
        }

        // Si la tarea posterior tiene una prioridad más urgente que la anterior, la anterior ha de ejecutarse con esa misma
        // urgencia para no retrasarla; por eso se le iguala la prioridad (enumPrioridad ordena de más a menos urgente).
        private static void IgualarPrioridadDeLaAnteriorSiLaPosteriorEsMasUrgente(ContextoSe contexto, TareaDtm tareaAnterior, TareaDtm tareaPosterior)
        {
            var prioridadAnterior = tareaAnterior.Prioridad ?? enumPrioridad.NoDefinida;
            var prioridadPosterior = tareaPosterior.Prioridad ?? enumPrioridad.NoDefinida;

            if ((int)prioridadPosterior >= (int)prioridadAnterior) return;

            tareaAnterior.Prioridad = prioridadPosterior;
            tareaAnterior.Modificar(contexto, esUnaAccion: true);
        }

        // Si ya existe, para la misma tarea y el mismo cuerpo, una observación con el asunto opuesto (Anterior/Despues), se corrige esa
        // observación en lugar de crear una nueva; esto cubre el caso de haberse equivocado al definir la secuencia de ejecución.
        private static void CrearOCorregirObservacionDeSecuencia(ContextoSe contexto, TareaDtm tarea1, TareaDtm tarea2, string cuandoResolver, string cuerpo)
        {
            var cuandoNoResolver = cuandoResolver == enumCuandoRealizar.Anterior.Descripcion()
                ? enumCuandoRealizar.Despues.Descripcion()
                : enumCuandoRealizar.Anterior.Descripcion();

            var observacionACorregir = enumNegocio.Tarea.Observaciones(contexto).FirstOrDefault(o => o.IdElemento == tarea1.Id && o.Descripcion == cuerpo && o.Nombre == cuandoNoResolver);

            if (observacionACorregir != null)
            {
                observacionACorregir.Nombre = cuandoResolver;
                observacionACorregir.ModificarObservacion(contexto, new Dictionary<string, object> { { ltrDeObservaciones.CreadaPorAdminSe, true }, { ltrDeObservaciones.ModificarAsunto, true } });
                return;
            }

            ValidarQueNoExisteLaSecuencialidad(contexto, tarea1, tarea2, cuandoResolver);
            tarea1.CrearObservacion(contexto, cuandoResolver, cuerpo, new Dictionary<string, object> { { ltrDeObservaciones.CreadaPorAdminSe, true }, { ltrDeObservaciones.CreandoSecuencia, true } });
        }

        private static void ValidarQueNoExisteLaSecuencialidad(ContextoSe contexto, TareaDtm tarea1, TareaDtm tarea2, string cuandoResolver)
        {
            var observaciones = enumNegocio.Tarea.Observaciones(contexto).Where(o => o.IdElemento == tarea1.Id && o.Nombre == cuandoResolver);
            foreach (var observacion in observaciones)
            {
                var tareaReferenciada = observacion.TareaEnlazada(contexto);
                if (tareaReferenciada != null && tareaReferenciada.Id == tarea2.Id)
                    GestorDeErrores.Emitir($"La tarea '{tarea1.Referencia}' ya se le ha indicado que '{cuandoResolver}' la '{tareaReferenciada.Referencia}'");
            }
        }

        public static void EliminarCuandoRealizar(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (!parametros.ContieneClave(nameof(CuandoRealizarDto.IdTareaEditada))) GestorDeErrores.Emitir("No se ha indicado la tarea editada");

            var idTareaEditada = parametros.LeerValor<int>(nameof(CuandoRealizarDto.IdTareaEditada));
            var tareaEditada = contexto.SeleccionarPorId<TareaDtm>(idTareaEditada);

            if (!tareaEditada.EsInterventor(contexto))
                GestorDeErrores.Emitir($"Sólo un interventor de la tarea '{tareaEditada.Referencia}' puede eliminar su secuencia de ejecución");

            var teniaAnteriores = EliminarSecuencia(contexto, tareaEditada, enumCuandoRealizar.Despues.Descripcion());
            var teniaPosteriores = EliminarSecuencia(contexto, tareaEditada, enumCuandoRealizar.Anterior.Descripcion());

            if (!teniaAnteriores && !teniaPosteriores)
                GestorDeErrores.Emitir($"La tarea '{tareaEditada.Referencia}' no tiene definida ninguna secuencia de ejecución anterior ni posterior");
        }

        // Elimina, para 'tareaEditada', las observaciones de secuencia con nombre 'cuandoResolver' (Antes que/Después de) y, en cada
        // tarea a la que hacían referencia, la observación recíproca que apuntaba de vuelta a 'tareaEditada'.
        private static bool EliminarSecuencia(ContextoSe contexto, TareaDtm tareaEditada, string cuandoResolver)
        {
            var cuandoResolverOpuesto = cuandoResolver == enumCuandoRealizar.Anterior.Descripcion()
                ? enumCuandoRealizar.Despues.Descripcion()
                : enumCuandoRealizar.Anterior.Descripcion();

            var observaciones = enumNegocio.Tarea.Observaciones(contexto).Where(o => o.IdElemento == tareaEditada.Id && o.Nombre == cuandoResolver).ToList();
            if (!observaciones.Any()) return false;

            var cuerpoDeLaEditada = enumNegocio.Tarea.ComponerUrlPorId(contexto, tareaEditada.Id).ToString();
            var parametrosDeBorrado = new Dictionary<string, object> { { ltrDeObservaciones.CreadaPorAdminSe, true }, { ltrDeObservaciones.PermitirEliminar, true } };

            foreach (var observacion in observaciones)
            {
                var tareaReferenciada = observacion.TareaEnlazada(contexto);

                var observacionReciproca = enumNegocio.Tarea.Observaciones(contexto)
                    .FirstOrDefault(o => o.IdElemento == tareaReferenciada.Id && o.Nombre == cuandoResolverOpuesto && o.Descripcion == cuerpoDeLaEditada);

                if (observacionReciproca != null)
                    observacionReciproca.EliminarObservacion(contexto, parametrosDeBorrado);

                observacion.EliminarObservacion(contexto, parametrosDeBorrado);
            }

            return true;
        }

        private string FormatearTotalesPorExpediente(List<TareaDtm> tareas)
        {
            if (!tareas.Any())
                return string.Empty;

            var grupos = tareas
                .SoloConRelacionada(t => t.Planificacion(Contexto, errorSiNoHay: false), plf => (plf.EnJornadas() ?? 0) > 0)
                .AgruparPorVinculos(t => t.Vinculados<ExpedienteDtm>(Contexto), e => e.Referencia)
                .Select(g => new
                {
                    Referencia = g.Key,
                    Tareas = g.Count(),
                    TotalJornadas = g.Sum(t => t.Planificacion(Contexto, errorSiNoHay: false)?.EnJornadas() ?? 0m),
                    MediaJornadas = g.Average(t => t.Planificacion(Contexto, errorSiNoHay: false)?.EnJornadas() ?? 0m)
                })
                .OrderByDescending(g => g.TotalJornadas)
                .ToList();

            if (!grupos.Any()) return string.Empty;

            const int anchoRef = 30;
            const int anchoNum = 10;
            const int anchoJornadas = 16;

            var sb = new StringBuilder();
            sb.AppendLine(
                $"{"Expediente".PadRight(anchoRef)}" +
                $"{"Tareas".PadLeft(anchoNum)}" +
                $"{"Total Jorn.".PadLeft(anchoJornadas)}" +
                $"{"Media Jorn.".PadLeft(anchoJornadas)}"
            );
            sb.AppendLine(new string('-', anchoRef + anchoNum + anchoJornadas * 2));

            foreach (var grupo in grupos)
            {
                var ref_ = grupo.Referencia ?? "Sin expediente";
                if (ref_.Length > anchoRef) ref_ = ref_.Substring(0, anchoRef - 1) + "…";

                sb.AppendLine(
                    $"{ref_.PadRight(anchoRef)}" +
                    $"{grupo.Tareas.ToString().PadLeft(anchoNum)}" +
                    $"{grupo.TotalJornadas.Formatear(alineacion: false).PadLeft(anchoJornadas)}" +
                    $"{grupo.MediaJornadas.Formatear(alineacion: false).PadLeft(anchoJornadas)}"
                );
            }

            return sb.ToString();
        }
    }


}
