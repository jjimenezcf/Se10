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
    public class GestorDeTiposDeRemesaFae : GestorDeTiposDeElemento<ContextoSe, TipoDeRemesaFaeDtm, TipoDeRemesaFaeDto>
    {
        public class ltrDeUnTipoDeRemesaFae
        {

        }

        public class MapearTipoDeRemesaFae : MapearTipoDeElemento
        {
            public MapearTipoDeRemesaFae()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDeRemesaFaeDtm, TipoDeRemesaFaeDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.RemesaFae))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre));

                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDeRemesaFaeDto, TipoDeRemesaFaeDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDeRemesaFae(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.RemesaFae)
        {

        }

        public static GestorDeTiposDeRemesaFae Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeRemesaFae(contexto, mapeador);
        }

        public static TipoDeRemesaFaeDtm PersistirTipo(ContextoSe contexto, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDeRemesaFaeDtm();
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

        protected override IQueryable<TipoDeRemesaFaeDtm> AplicarJoins(IQueryable<TipoDeRemesaFaeDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                           .Include(x => x.Estado);
        }



    }
}
