using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Ventas;
using ServicioDeDatos.Ventas;

namespace GestoresDeNegocio.Ventas
{
    public class GestorDeTiposDeParteTr : GestorDeTiposDeElemento<ContextoSe, TipoDeParteTrDtm, TipoDeParteTrDto>
    {
        public class ltrDeUnTipoDeParteTr
        {

        }

        public class MapearTipoDeParteTr : MapearTipoDeElemento
        {
            public MapearTipoDeParteTr()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDeParteTrDtm, TipoDeParteTrDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.ParteDeTrabajo))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre))
               .ForMember(dto => dto.TipoFacturaEmt, dtm => dtm.MapFrom(x => x.TipoFacturaEmt.Nombre));

                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDeParteTrDto, TipoDeParteTrDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore())
                .ForMember(dtm => dtm.TipoFacturaEmt, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDeParteTr(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.ParteDeTrabajo)
        {

        }

        public static GestorDeTiposDeParteTr Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeParteTr(contexto, mapeador);
        }

        protected override void AntesDePersistir(TipoDeParteTrDtm tipoDeElemento, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(tipoDeElemento, parametros);
        }

        public static TipoDeParteTrDtm PersistirTipo(ContextoSe contexto, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDeParteTrDtm();
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

        protected override IQueryable<TipoDeParteTrDtm> AplicarJoins(IQueryable<TipoDeParteTrDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                .Include(x => x.Estado)
                .Include(x => x.TipoFacturaEmt);
        }

    }
}
