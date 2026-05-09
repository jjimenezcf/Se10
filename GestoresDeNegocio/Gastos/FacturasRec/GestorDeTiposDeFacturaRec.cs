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
    public class GestorDeTiposDeFacturaRec : GestorDeTiposDeElemento<ContextoSe, TipoDeFacturaRecDtm, TipoDeFacturaRecDto>
    {
        public class ltrDeUnTipoDeFacturaRec
        {

        }

        public class MapearTipoDeFacturaRec : MapearTipoDeElemento
        {
            public MapearTipoDeFacturaRec()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDeFacturaRecDtm, TipoDeFacturaRecDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.FacturaRecibida))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre));

                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDeFacturaRecDto, TipoDeFacturaRecDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDeFacturaRec(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.FacturaRecibida)
        {

        }

        public static GestorDeTiposDeFacturaRec Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeFacturaRec(contexto, mapeador);
        }

        public static TipoDeFacturaRecDtm PersistirTipo(ContextoSe contexto, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDeFacturaRecDtm();
                tipo.IdEstado = idEstado;
                tipo.ClaseDeLibro = clsLibro;
                tipo.Nombre = nombre;
                tipo.Sigla = sigla;
                tipo = gestor.PersistirRegistro(tipo, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(tipo.Id, false, false, false, true);
            }

            if (leido.Nombre != nombre || leido.Sigla != sigla ||  leido.ClaseDeLibro != clsLibro )
            {
                leido.Nombre = nombre; leido.Sigla = sigla; leido.ClaseDeLibro = clsLibro;  
                gestor.PersistirRegistro(leido, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
            return leido;
        }

        protected override IQueryable<TipoDeFacturaRecDtm> AplicarJoins(IQueryable<TipoDeFacturaRecDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                           .Include(x => x.Estado);
        }

        protected override int ValidarNoHayElementos(TipoDeFacturaRecDtm registro, ParametrosDeNegocio parametros)
        {
            var cantidad = base.ValidarNoHayElementos(registro, parametros);
            if (cantidad > 0 && parametros.Operacion == enumTipoOperacion.Modificar)
            {

            }
            return cantidad;
        }

        protected override void AntesDePersistir(TipoDeFacturaRecDtm tipoFactura, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(tipoFactura, parametros);
        }

    }
}
