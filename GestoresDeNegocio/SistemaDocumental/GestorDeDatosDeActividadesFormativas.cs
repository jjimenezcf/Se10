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
    public class GestorDeDatosDeActividadesFormativas : GestorDeElementos<ContextoSe, DatosDeActividadFormativaDtm, DatosDeActividadFormativaDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrDatosDeActividadesFormativas
        {
        }

        public class MapearDatosDeActividadesFormativas : Profile
        {
            public MapearDatosDeActividadesFormativas()
            {                
                CreateMap<DatosDeActividadFormativaDtm, DatosDeActividadFormativaDto>()
                .ForMember(dto => dto.Responsable, dtm => dtm.MapFrom(dtm => dtm.Responsable == null ? "" : dtm.Responsable.Expresion));
                CreateMap<DatosDeActividadFormativaDto, DatosDeActividadFormativaDtm>()
                .ForMember(dtm => dtm.Elemento, dto => dto.Ignore())
                .ForMember(dtm => dtm.Responsable, dto => dto.Ignore());
            }
        }

        public GestorDeDatosDeActividadesFormativas(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeDatosDeActividadesFormativas Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeDatosDeActividadesFormativas(contexto, mapeador);
        }

        protected override IQueryable<DatosDeActividadFormativaDtm> AplicarJoins(IQueryable<DatosDeActividadFormativaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta= base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Responsable);
            return consulta;
        }

        protected override IQueryable<DatosDeActividadFormativaDtm> AplicarFiltros(IQueryable<DatosDeActividadFormativaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            return base.AplicarFiltros(consulta, filtros, parametros);
        }

        protected override void AntesDePersistir(DatosDeActividadFormativaDtm datos, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(datos, parametros);

        }

        protected override void AntesDeMapearElElemento(DatosDeActividadFormativaDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDeMapearElElemento(registro, parametros);
        }

        protected override void DespuesDeMapearElElemento(DatosDeActividadFormativaDtm registro, DatosDeActividadFormativaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
        }

    }
}
