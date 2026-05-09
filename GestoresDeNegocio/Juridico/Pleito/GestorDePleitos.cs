using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ModeloDeDto.Juridico;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Juridico;
using GestoresDeNegocio.RegistroEs;

namespace GestoresDeNegocio.Juridico
{

    public class GestorDePleitos : GestorDeElementos<ContextoSe, PleitoDtm, PleitoDto>
    {
        public class ltrDeUnPleito
        {
        }

        public class MapearPleito : Profile
        {
            public MapearPleito()
            {
                CreateMap<PleitoDtm, PleitoDto>()
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Expresion))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado.Nombre))
                .ForMember(dto => dto.Solicitante, dtm => dtm.MapFrom(dtm => dtm.Solicitante.Expresion))
                .ForMember(dto => dto.Responsable, dtm => dtm.MapFrom(dtm => dtm.Responsable.Expresion))
                .ForMember(dto => dto.Abogado, dtm => dtm.MapFrom(dtm => dtm.Abogado.Expresion))
                .ForMember(dto => dto.Procurador, dtm => dtm.MapFrom(dtm => dtm.Procurador.Expresion))
                .ForMember(dto => dto.Juzgado, dtm => dtm.MapFrom(dtm => dtm.Juzgado.Expresion));

                CreateMap<PleitoDto, PleitoDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore())
                .ForMember(dtm => dtm.Solicitante, dto => dto.Ignore())
                .ForMember(dtm => dtm.Responsable, dto => dto.Ignore())
                .ForMember(dtm => dtm.Procurador, dto => dto.Ignore())
                .ForMember(dtm => dtm.Abogado, dto => dto.Ignore())
                .ForMember(dtm => dtm.Juzgado, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.Pleito;

        //public override TiposDelTipoDeElemento TiposDelTipo => Negocio.TiposDelTipo();
        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDePleito.Gestor(Contexto, Contexto.Mapeador);

        public GestorDePleitos(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDePleitos Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePleitos(contexto, mapeador);
        }


        protected override IQueryable<PleitoDtm> AplicarJoins(IQueryable<PleitoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Solicitante)
                .Include(x => x.Responsable);
            return consulta;
        }

        protected override IQueryable<PleitoDtm> AplicarFiltros(IQueryable<PleitoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            foreach (var filtro in filtros)
            {
                consulta = consulta.RegistrosEsRelacionados(Contexto, filtro);
            }

            return consulta;
        }

        protected override void AntesDePersistir(PleitoDtm Pleito, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(Pleito, parametros);
            Pleito.ClaseDePleito = ((TipoDePleitoDtm)parametros.TipoConFujo).ClaseDePleito;
        }

        protected override IQueryable<PleitoDtm> AplicarSeguridad(IQueryable<PleitoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<PleitoDtm, TipoDePleitoDtm, PermisoDelPleitoDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<PleitoDtm, PermisoDelPleitoDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }
    }

}
