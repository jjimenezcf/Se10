using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Logistica;
using ServicioDeDatos.Logistica;

namespace GestoresDeNegocio.Logistica
{
    public class GestorDeTiposDePedido : GestorDeTiposDeElemento<ContextoSe, TipoDePedidoDtm, TipoDePedidoDto>
    {
        public class ltrDeUnTipoDePedido
        {

        }

        public class MapearTipoDePedido : MapearTipoDeElemento
        {
            public MapearTipoDePedido()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDePedidoDtm, TipoDePedidoDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.Pedido))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre));

                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDePedidoDto, TipoDePedidoDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDePedido(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.Pedido)
        {

        }

        public static GestorDeTiposDePedido Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDePedido(contexto, mapeador);
        }

        public static TipoDePedidoDtm PersistirTipo(ContextoSe contexto, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla, bool permiteCrear)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDePedidoDtm();
                tipo.IdEstado = idEstado;
                tipo.ClaseDeLibro = clsLibro;
                tipo.Nombre = nombre;
                tipo.Sigla = sigla;
                tipo.PermiteCrear = permiteCrear;
                tipo = gestor.PersistirRegistro(tipo, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(tipo.Id, false, false, false, true);
            }

            if (leido.Nombre != nombre || leido.Sigla != sigla ||  leido.ClaseDeLibro != clsLibro || leido.PermiteCrear != permiteCrear)
            {
                leido.Nombre = nombre; leido.Sigla = sigla; leido.ClaseDeLibro = clsLibro;  
                gestor.PersistirRegistro(leido, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
            return leido;
        }

        protected override IQueryable<TipoDePedidoDtm> AplicarJoins(IQueryable<TipoDePedidoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                           .Include(x => x.Estado);
        }

        protected override int ValidarNoHayElementos(TipoDePedidoDtm registro, ParametrosDeNegocio parametros)
        {
            var cantidad = base.ValidarNoHayElementos(registro, parametros);
            if (cantidad > 0 && parametros.Operacion == enumTipoOperacion.Modificar)
            {

            }
            return cantidad;
        }

        protected override void AntesDePersistir(TipoDePedidoDtm tipoPedido, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(tipoPedido, parametros);
        }

    }
}
