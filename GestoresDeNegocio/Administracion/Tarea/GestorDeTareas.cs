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
            if (filtros.Any(x => x.Clausula.ToLower() == ltrDeUnaTarea.IdFacturaEmt.ToLower() || (x.Clausula.ToLower() == ltrDeUnaTarea.Facturada.ToLower() && x.Valor.Entero() == ltrParametrosNeg.ConRelacion)))
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
            consulta = consulta.FiltroSiHayDependenciaDe(filtros, filtrarPor:ltrDeUnaTarea.IdResponsable, filtroDeAsociacion: ltrDeUnaTarea.Asignacion, parametros, aplicarFiltroDeEstado: true);
            //consulta = consulta.FiltroPorEtapa(filtros);
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

            if (parametros.Peticion == enumPeticion.epLeerPorId)
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
            }

            if (parametros.Peticion == enumPeticion.epLeerPorId)
                elemento.Etapas = tarea.Lista();

            if (parametros.LeerDatosParaElGridOParaExportar && parametros.ColumnasDelGrid.Any(item => item == nameof(elemento.Durabilidad).ToLowerInvariant() ||
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
            return totales;
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
            var tarea = contexto.SeleccionarPorId<TareaDtm>(idTareaOrigen);
            var tareaNueva = tarea.Copiar(contexto, parametros);
            tareaNueva.CrearObservacion(contexto, "Copiada de", enumNegocio.Tarea.ComponerUrlPorId(contexto, idTareaOrigen).ToString(), new Dictionary<string, object>
            {
                { ltrDeObservaciones.CreadaPorAdminSe, true }
            });
            return tareaNueva.Id;
        }
    }


}
