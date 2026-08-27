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
    public class GestorDeTiposDeAlmacen : GestorDeTiposDeElemento<ContextoSe, TipoDeAlmacenDtm, TipoDeAlmacenDto>
    {
        public class ltrDeUnTipoDeAlmacen
        {

        }

        public class MapearTipoDeAlmacen : MapearTipoDeElemento
        {
            public MapearTipoDeAlmacen()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDeAlmacenDtm, TipoDeAlmacenDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.Almacen))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre));

                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDeAlmacenDto, TipoDeAlmacenDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDeAlmacen(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.Almacen)
        {

        }

        public static GestorDeTiposDeAlmacen Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeAlmacen(contexto, mapeador);
        }

        public static TipoDeAlmacenDtm PersistirTipo(ContextoSe contexto, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla, bool permiteCrear, enumAlmacenCalculo calculo = enumAlmacenCalculo.Fifo)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDeAlmacenDtm();
                tipo.IdEstado = idEstado;
                tipo.ClaseDeLibro = clsLibro;
                tipo.Nombre = nombre;
                tipo.Sigla = sigla;
                tipo.PermiteCrear = permiteCrear;
                tipo.Calculo = calculo;
                tipo = gestor.PersistirRegistro(tipo, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(tipo.Id, false, false, false, true);
            }

            if (leido.Nombre != nombre || leido.Sigla != sigla || leido.ClaseDeLibro != clsLibro || leido.PermiteCrear != permiteCrear || leido.Calculo != calculo)
            {
                leido.Nombre = nombre; leido.Sigla = sigla; leido.ClaseDeLibro = clsLibro; leido.Calculo = calculo;
                gestor.PersistirRegistro(leido, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
            return leido;
        }

        protected override IQueryable<TipoDeAlmacenDtm> AplicarJoins(IQueryable<TipoDeAlmacenDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                           .Include(x => x.Estado);
        }

        protected override int ValidarNoHayElementos(TipoDeAlmacenDtm registro, ParametrosDeNegocio parametros)
        {
            var cantidad = base.ValidarNoHayElementos(registro, parametros);
            if (cantidad > 0 && parametros.Operacion == enumTipoOperacion.Modificar)
            {
                // TODO: completar cuando se conozca el detalle funcional del negocio de almacenes
            }
            return cantidad;
        }

        protected override void AntesDePersistir(TipoDeAlmacenDtm tipoDeAlmacen, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(tipoDeAlmacen, parametros);
        }

    }
}
