using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Tarea;
using ModeloDeDto.Tarea;
using GestorDeElementos.Extensores;
using System;

namespace GestoresDeNegocio.Tarea
{
    public class GestorDePlfDeTareas : GestorDeElementos<ContextoSe, PlfDeTareaDtm, PlfDeTareaDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrPlfDeTareas
        {
        }

        public class MapearPlfDeTareas : Profile
        {
            public MapearPlfDeTareas()
            {
                CreateMap<PlfDeTareaDtm, PlfDeTareaDto>();
                CreateMap<PlfDeTareaDto, PlfDeTareaDtm>();
            }
        }

        public GestorDePlfDeTareas(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDePlfDeTareas Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePlfDeTareas(contexto, mapeador);
        }


        protected override void AntesDePersistir(PlfDeTareaDtm planificacion, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(planificacion, parametros);
            if (parametros.EsUnaTransicion) return;

            //if (planificacion.Iniciada.HasValue && planificacion.Finalizada.HasValue && ((DateTime)planificacion.Finalizada).FechaSinHora())
            //    planificacion.Finalizada = planificacion.Finalizada.AsignarHora();

            planificacion.ValidarPlanificacion(Contexto, parametros);
            if (parametros.Modificando) planificacion.ValidarDatosPorEtapas(Contexto, (PlfDeTareaDtm)parametros.registroEnBd, parametros.Parametros);
        }

        protected override void DespuesDePersistir(PlfDeTareaDtm planificacion, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(planificacion, parametros);
            planificacion.PersistirEvento(Contexto, parametros);
        }

        override protected void EliminarCaches(PlfDeTareaDtm planificacion, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(planificacion, parametros);
            ServicioDeCaches.EliminarCache(CacheDe.Exp_Tareas);
        }
    }
}
