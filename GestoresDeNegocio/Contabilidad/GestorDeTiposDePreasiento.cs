using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using GestoresDeNegocio.Negocio;
using ServicioDeDatos.Contabilidad;
using ModeloDeDto.Contabilidad;

namespace GestoresDeNegocio.Contabilidad
{
    public class GestorDeTiposDePreasiento : GestorDeTiposDeElemento<ContextoSe, TipoDePreasientoDtm, TipoDePreasientoDto>
    {
        public class ltrDeUnTipoDePreasiento
        {

        }

        public class MapearTipoDePreasiento : MapearTipoDeElemento
        {
            public MapearTipoDePreasiento()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDePreasientoDtm, TipoDePreasientoDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.Preasiento))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre));

                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDePreasientoDto, TipoDePreasientoDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDePreasiento(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.Preasiento)
        {

        }

        public static GestorDeTiposDePreasiento Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDePreasiento(contexto, mapeador);
        }

        public static TipoDePreasientoDtm PersistirTipo(ContextoSe contexto, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla, bool permitirCrear)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDePreasientoDtm();
                tipo.IdEstado = idEstado;
                tipo.ClaseDeLibro = clsLibro;
                tipo.Nombre = nombre;
                tipo.Sigla = sigla;
                tipo.PermiteCrear = permitirCrear;
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

        protected override IQueryable<TipoDePreasientoDtm> AplicarJoins(IQueryable<TipoDePreasientoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                           .Include(x => x.Estado);
        }



    }
}
