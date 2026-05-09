using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using System;
using Microsoft.EntityFrameworkCore;
using Gestor.Errores;
using Utilidades;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Callejero;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeBancos : GestorDeElementos<ContextoSe, BancoDtm, BancoDto>
    {
        public override enumNegocio Negocio => enumNegocio.Banco;

        public class MapearBanco : Profile
        {
            public MapearBanco()
            {
                CreateMap<BancoDtm, BancoDto>()
                .ForMember(dto => dto.Pais, dtm => dtm.MapFrom(dtm => dtm.Pais == null ? "" : $"({dtm.Pais.Codigo}) {dtm.Pais.Nombre}"))
                .ForMember(dto => dto.Iso2, dtm => dtm.MapFrom(dtm => dtm.Pais == null ? "" : dtm.Pais.ISO2));

                CreateMap<BancoDto, BancoDtm>()
                .ForMember(dtm => dtm.Pais, dto => dto.Ignore())
                .ForMember(dtm => dtm.FechaCreacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.FechaModificacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdUsuaCrea, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdUsuaModi, dto => dto.Ignore());

            }
        }

        public GestorDeBancos(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeBancos Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeBancos(contexto, mapeador); ;
        }


        protected override IQueryable<BancoDtm> AplicarJoins(IQueryable<BancoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Pais);
            return consulta;
        }

        protected override IQueryable<BancoDtm> AplicarFiltros(IQueryable<BancoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == nameof(PaisDtm.ISO2).ToLower());

            if (filtro != null) 
            {
                var paises = Contexto.Set<PaisDtm>().Where(p => p.ISO2.ToLower() == filtro.Valor.ToLower());
                consulta = consulta.Where(b => paises.Any(p => p.Id == b.IdPais));
            }
            return consulta;
        }

        protected override void AntesDePersistir(BancoDtm banco, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(banco, parametros);
            banco.BicSwift = banco.BicSwift.PadRight(11, 'X').Substring(0, 11);
            //TODO: Si hay cuentas bancarias asociadas no se permite
            if (parametros.Operacion == enumTipoOperacion.Modificar || parametros.Operacion == enumTipoOperacion.Eliminar)
            {

            }
        }

        protected override void DespuesDePersistir(BancoDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            ServicioDeCaches.EliminarCache(CacheDe.Ter_Bancos);
            ServicioDeCaches.EliminarCache(CacheDe.Pag_DatosDelPagoDto);
        }

        protected override void DespuesDeMapearElElemento(BancoDtm banco, BancoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(banco, elemento, parametros);
            //TODO: Si hay cuentas bancarias el modo de acceso a consulta
            //elemento.ModoDeAcceso = ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor;
        }
    }


}
