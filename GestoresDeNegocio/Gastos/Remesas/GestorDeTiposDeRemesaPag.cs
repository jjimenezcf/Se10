using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Gastos;
using ServicioDeDatos.Gastos;

namespace GestoresDeNegocio.Gastos
{
    public class GestorDeTiposDeRemesaPag : GestorDeTiposDeElemento<ContextoSe, TipoDeRemesaPagDtm, TipoDeRemesaPagDto>
    {
        public class ltrDeUnTipoDeRemesaPag
        {

        }

        public class MapearTipoDeRemesaPag : MapearTipoDeElemento
        {
            public MapearTipoDeRemesaPag()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDeRemesaPagDtm, TipoDeRemesaPagDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.RemesaPag))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre));

                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDeRemesaPagDto, TipoDeRemesaPagDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDeRemesaPag(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.RemesaPag)
        {

        }

        public static GestorDeTiposDeRemesaPag Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeRemesaPag(contexto, mapeador);
        }

        public static TipoDeRemesaPagDtm PersistirTipo(ContextoSe contexto, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDeRemesaPagDtm();
                tipo.IdEstado = idEstado;
                tipo.ClaseDeLibro = clsLibro;
                tipo.Nombre = nombre;
                tipo.Sigla = sigla;
                tipo = gestor.PersistirRegistro(tipo, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(tipo.Id, false, false, false, true);
            }

            if (leido.Nombre != nombre || leido.Sigla != sigla || leido.ClaseDeLibro != clsLibro)
            {
                leido.Nombre = nombre; leido.Sigla = sigla; leido.ClaseDeLibro = clsLibro; 
                gestor.PersistirRegistro(leido, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
            return leido;
        }

        protected override IQueryable<TipoDeRemesaPagDtm> AplicarJoins(IQueryable<TipoDeRemesaPagDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                           .Include(x => x.Estado);
        }



    }
}
