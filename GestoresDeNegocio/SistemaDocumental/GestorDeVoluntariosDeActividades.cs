using AutoMapper;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.SistemaDocumental;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.SistemaDocumental
{
    public class GestorDeVoluntariosDeActividades : GestorDeElementos<ContextoSe, VoluntarioDeActividadDtm, VoluntarioDeActividadDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrVoluntarios
        {
        }

        public class MapearVoluntarios : Profile
        {
            public MapearVoluntarios()
            {
                CreateMap<VoluntarioDeActividadDtm, VoluntarioDeActividadDto>()
                .ForMember(dto => dto.Elemento, x => x.MapFrom(dtm => dtm.Elemento != null ? dtm.Elemento.Expresion : null))
                .ForMember(dto => dto.Interlocutor, x => x.MapFrom(dtm => dtm.Interlocutor != null ? dtm.Interlocutor.Expresion : null));
                CreateMap<VoluntarioDeActividadDto, VoluntarioDeActividadDtm>()
                .ForMember(dtm => dtm.Interlocutor, dto => dto.Ignore());
            }
        }

        public GestorDeVoluntariosDeActividades(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeVoluntariosDeActividades Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeVoluntariosDeActividades(contexto, mapeador);
        }

        protected override IQueryable<VoluntarioDeActividadDtm> AplicarJoins(IQueryable<VoluntarioDeActividadDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Interlocutor);
            return consulta;
        }

        protected override void AntesDePersistir(VoluntarioDeActividadDtm inscrito, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(inscrito, parametros);
        }

        protected override void DespuesDePersistir(VoluntarioDeActividadDtm inscrito, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(inscrito, parametros);           
        }

        protected override void EliminarCaches(VoluntarioDeActividadDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
        }

        protected override void DespuesDeMapearElElemento(VoluntarioDeActividadDtm inscrito, VoluntarioDeActividadDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(inscrito, elemento, parametros);
        }

    }
}
