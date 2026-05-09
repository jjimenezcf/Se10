using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using GestoresDeNegocio.Negocio;
using ServicioDeDatos.Tarea;
using ModeloDeDto.Tarea;
using Gestor.Errores;
using ServicioDeDatos.SistemaDocumental;
using GestorDeElementos.Extensores;
using System;

namespace GestoresDeNegocio.Tarea
{
    public class GestorDeTiposDeTarea : GestorDeTiposDeElemento<ContextoSe, TipoDeTareaDtm, TipoDeTareaDto>
    {
        public class ltrDeUnTipoDeTarea
        {

        }

        public class MapearTipoDeTarea : MapearTipoDeElemento
        {
            public MapearTipoDeTarea()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDeTareaDtm, TipoDeTareaDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.Tarea))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre != null ? x.Padre.Expresion : null))
               .ForMember(dto => dto.TipoArchivador, dtm => dtm.MapFrom(x => x.TipoArchivador != null ? x.TipoArchivador.Nombre : null))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado != null ? x.Estado.Nombre : null));


                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDeTareaDto, TipoDeTareaDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore())
                .ForMember(dtm => dtm.TipoArchivador, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDeTarea(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.Tarea)
        {

        }

        public static GestorDeTiposDeTarea Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeTarea(contexto, mapeador);
        }

        public static TipoDeTareaDtm PersistirTipo(ContextoSe contexto,
            enumClaseDeTarea clsTarea,
            string nombre,
            int idEstado,
            enumClaseDeLibro clsLibro,
            string sigla,
            TipoDeArchivadorDtm tipoAri,
            bool usaPlanificacion,
            TipoDeTareaDtm padre = null,
            bool copiarDireccion = false)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDeTareaDtm();
                tipo.ClaseDeTarea = clsTarea;
                tipo.IdEstado = idEstado;
                tipo.ClaseDeLibro = clsLibro;
                tipo.Nombre = nombre;
                tipo.Sigla = sigla;
                tipo.UsaPlanificacion = usaPlanificacion;
                tipo.CopiarDireccionDelSolicitante = copiarDireccion;
                tipo.IdTipoArchivador = tipoAri != null ? tipoAri.Id : null;
                tipo.IdPadre = padre is null ? null : padre.Id;
                tipo.EsFacturable = false;
                tipo = gestor.PersistirRegistro(tipo, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(tipo.Id, false, false, false, true);
            }

            if (leido.Nombre != nombre || leido.Sigla != sigla || leido.ClaseDeLibro != clsLibro || leido.IdPadre.Entero() != (padre is null ? 0 : padre.Id))
            {
                leido.Nombre = nombre; leido.Sigla = sigla; leido.ClaseDeLibro = clsLibro;
                gestor.PersistirRegistro(leido, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
            return leido;
        }

        protected override IQueryable<TipoDeTareaDtm> AplicarJoins(IQueryable<TipoDeTareaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                .Include(x => x.TipoArchivador)
                .Include(x => x.Estado);
        }

        protected override int ValidarNoHayElementos(TipoDeTareaDtm tipoDeTarea, ParametrosDeNegocio parametros)
        {
            var cantidad = base.ValidarNoHayElementos(tipoDeTarea, parametros);
            if (cantidad > 0)
            {
                if (parametros.Operacion == enumTipoOperacion.Modificar)
                {
                    if (((TipoDeTareaDtm)parametros.registroEnBd).IdTipoArchivador != tipoDeTarea.IdTipoArchivador || ((TipoDeTareaDtm)parametros.registroEnBd).ClaseDeTarea != tipoDeTarea.ClaseDeTarea)
                        GestorDeErrores.Emitir($"No se puede modificar el tipo '{tipoDeTarea.Nombre}' ya que tiene elementos asociados");

                    if (tipoDeTarea.PropiedadCambiada<bool>(parametros.registroEnBd, nameof(TipoDeTareaDtm.UsaPlanificacion)) && !tipoDeTarea.UsaPlanificacion)
                        GestorDeErrores.Emitir($"Si ya no quiere usar planificación para el '{tipoDeTarea.Nombre}' de lo de baja y cree uno nuevo");

                }
            }
            return cantidad;
        }

        protected override void DespuesDePersistir(TipoDeTareaDtm tipo, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(tipo, parametros);
            if (parametros.Modificando)
            {
                if (!tipo.UsaPlanificacion && ((TipoDeTareaDtm)parametros.registroEnBd).UsaPlanificacion)
                    PropagarFlag(tipo, nameof(TipoDeTareaDtm.UsaPlanificacion), propagarALosHijos: false, propagarALosPadres: true, tipo.UsaPlanificacion);

                if (tipo.UsaPlanificacion && !((TipoDeTareaDtm)parametros.registroEnBd).UsaPlanificacion)
                {
                    PropagarFlag(tipo, nameof(TipoDeTareaDtm.UsaPlanificacion), propagarALosHijos: true, propagarALosPadres: false, tipo.UsaPlanificacion);
                    foreach (var tarea in Contexto.Set<TareaDtm>().Where(t => t.IdTipo == tipo.Id))
                    {
                        var hitoDeAsignacion = tarea.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeTareas> { enumEtapasDeTareas.TAR_Etapa_Cancelado, enumEtapasDeTareas.TAR_Etapa_Inicial }) 
                            ? null
                            : ExtensorDeElementosDeUnProceso.Hitos(tarea, Contexto, enumEtapasDeTareas.TAR_Etapa_Asignada.Lista()).FirstOrDefault();

                        var hitoDeIniciada = tarea.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeTareas> { enumEtapasDeTareas.TAR_Etapa_En_Resolucion,
                            enumEtapasDeTareas.TAR_Etapa_Validacion, enumEtapasDeTareas.TAR_Etapa_En_Espera, enumEtapasDeTareas.TAR_Etapa_Terminada })
                            ?  ExtensorDeElementosDeUnProceso.Hitos(tarea, Contexto, enumEtapasDeTareas.TAR_Etapa_En_Resolucion.Lista()).FirstOrDefault()
                            : null;

                        var hitoDeFinalizacion = tarea.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeTareas> { enumEtapasDeTareas.TAR_Etapa_Validacion, enumEtapasDeTareas.TAR_Etapa_Terminada })
                            ? ExtensorDeElementosDeUnProceso.Hitos(tarea, Contexto, enumEtapasDeTareas.TAR_Etapa_Validacion.Lista()).FirstOrDefault()
                            : null;

                        var planificacion = new PlfDeTareaDtm
                        {
                            IdElemento = tarea.Id,
                            PlfDeInicio = hitoDeAsignacion == null ? null : extFechas.RedondearAlMinuto(hitoDeAsignacion.Fecha, siguiente: true),
                            Iniciada = hitoDeIniciada == null ? null : extFechas.RedondearAlMinuto(hitoDeIniciada.Fecha, siguiente: true),
                            PlfDeFin = hitoDeFinalizacion == null ? null : extFechas.RedondearAlMinuto(hitoDeFinalizacion.Fecha, siguiente: false),
                            Finalizada = hitoDeFinalizacion == null ? null : extFechas.RedondearAlMinuto(hitoDeFinalizacion.Fecha, siguiente: false),
                            MedidoEn = Enumerados.enumDurabilidad.Dias,
                            Duracion = hitoDeAsignacion is not null && hitoDeFinalizacion is not null
                            ? (hitoDeFinalizacion.Fecha - hitoDeAsignacion.Fecha).Days
                            : null
                        };
                        if (planificacion.Duracion == 0) planificacion.Duracion = 1;
                        planificacion.Insertar(Contexto, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false} });
                    }
                }

                if (!tipo.CopiarDireccionDelSolicitante && ((TipoDeTareaDtm)parametros.registroEnBd).CopiarDireccionDelSolicitante)
                    PropagarFlag(tipo, nameof(TipoDeTareaDtm.CopiarDireccionDelSolicitante), propagarALosHijos: false, propagarALosPadres: true, tipo.CopiarDireccionDelSolicitante);

                if (tipo.CopiarDireccionDelSolicitante && !((TipoDeTareaDtm)parametros.registroEnBd).CopiarDireccionDelSolicitante)
                    PropagarFlag(tipo, nameof(TipoDeTareaDtm.CopiarDireccionDelSolicitante), propagarALosHijos: true, propagarALosPadres: false, tipo.CopiarDireccionDelSolicitante);
            }

        }

    }
}
