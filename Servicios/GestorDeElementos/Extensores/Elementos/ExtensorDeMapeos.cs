using AutoMapper;
using ModeloDeDto;
using ServicioDeDatos.Elemento;


namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeMapeos
    {
        public static IMappingExpression<TDtm, TDto> DtmToDto<TDtm, TDto>(this IMappingExpression<TDtm, TDto> mapeos)
        where TDtm : ElementoDeProcesoDtm
        where TDto : ElementoDeUnProcesoDto
        {
            mapeos = mapeos
            .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg == null ? null : dtm.Cg.Expresion))
            .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo == null ? null : dtm.Tipo.Expresion))
            .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado == null ? null : dtm.Estado.Nombre));

            if (typeof(TDtm).ImplementaUsaSolicitante())
                mapeos = mapeos.ForMember(dto => ((IUsaSolicitanteDto)dto).Solicitante, dtm => dtm.MapFrom(dtm =>
                ((IUsaSolicitante)dtm).Solicitante == null
                ? null
                : ((IUsaSolicitante)dtm).Solicitante.Expresion));

            if (typeof(TDtm).ImplementaPuedeUsarResponsable())
                mapeos = mapeos.ForMember(dto => ((IPuedeUsarResponsableDto)dto).Responsable, dtm => dtm.MapFrom(dtm =>
                ((IPuedeUsarResponsable)dtm).Responsable == null
                ? null
                : ((IPuedeUsarResponsable)dtm).Responsable.Expresion));

            return mapeos;
        }


        public static IMappingExpression<TDto, TDtm> DtoToDtm<TDto, TDtm>(this IMappingExpression<TDto, TDtm> mapeos)
        where TDto : ElementoDeUnProcesoDto
        where TDtm : ElementoDeProcesoDtm
        {
            mapeos = mapeos
            .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
            .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
            .ForMember(dtm => dtm.Estado, dto => dto.Ignore());

            if (typeof(TDtm).ImplementaUsaSolicitante())
                mapeos = mapeos.ForMember(dtm => ((IUsaSolicitante)dtm).Solicitante, dto => dto.Ignore());

            if (typeof(TDtm).ImplementaPuedeUsarResponsable())
                mapeos = mapeos.ForMember(dtm => ((IPuedeUsarResponsableDto)dtm).Responsable, dto => dto.Ignore());

            return mapeos;
        }
    }
}
