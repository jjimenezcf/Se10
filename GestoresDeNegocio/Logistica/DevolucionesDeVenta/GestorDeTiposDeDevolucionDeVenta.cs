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
    public class GestorDeTiposDeDevolucionDeVenta : GestorDeTiposDeElemento<ContextoSe, TipoDeDevolucionDeVentaDtm, TipoDeDevolucionDeVentaDto>
    {
        public class MapearTipoDeDevolucionDeVenta : MapearTipoDeElemento
        {
            public MapearTipoDeDevolucionDeVenta()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDeDevolucionDeVentaDtm, TipoDeDevolucionDeVentaDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.DevolucionVenta))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre));

                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDeDevolucionDeVentaDto, TipoDeDevolucionDeVentaDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDeDevolucionDeVenta(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.DevolucionVenta)
        {

        }

        public static GestorDeTiposDeDevolucionDeVenta Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeDevolucionDeVenta(contexto, mapeador);
        }

        public static TipoDeDevolucionDeVentaDtm PersistirTipo(ContextoSe contexto, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla, bool permiteCrear, bool editarTrasCrear)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDeDevolucionDeVentaDtm();
                tipo.IdEstado = idEstado;
                tipo.ClaseDeLibro = clsLibro;
                tipo.Nombre = nombre;
                tipo.Sigla = sigla;
                tipo.PermiteCrear = permiteCrear;
                tipo.EditarTrasCrear = editarTrasCrear;
                tipo = gestor.PersistirRegistro(tipo, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(tipo.Id, false, false, false, true);
            }

            if (leido.Nombre != nombre || leido.Sigla != sigla || leido.ClaseDeLibro != clsLibro || leido.PermiteCrear != permiteCrear || leido.EditarTrasCrear != editarTrasCrear)
            {
                leido.Nombre = nombre; leido.Sigla = sigla; leido.ClaseDeLibro = clsLibro; leido.PermiteCrear = permiteCrear; leido.EditarTrasCrear = editarTrasCrear;
                gestor.PersistirRegistro(leido, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
            return leido;
        }

        protected override IQueryable<TipoDeDevolucionDeVentaDtm> AplicarJoins(IQueryable<TipoDeDevolucionDeVentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                           .Include(x => x.Estado);
        }

    }
}
