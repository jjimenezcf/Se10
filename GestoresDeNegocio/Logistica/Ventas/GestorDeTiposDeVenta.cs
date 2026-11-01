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
    public class GestorDeTiposDeVenta : GestorDeTiposDeElemento<ContextoSe, TipoDeVentaDtm, TipoDeVentaDto>
    {
        public class MapearTipoDeVenta : MapearTipoDeElemento
        {
            public MapearTipoDeVenta()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDeVentaDtm, TipoDeVentaDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.Venta))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre));

                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDeVentaDto, TipoDeVentaDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDeVenta(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.Venta)
        {

        }

        public static GestorDeTiposDeVenta Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeVenta(contexto, mapeador);
        }

        public static TipoDeVentaDtm PersistirTipo(ContextoSe contexto, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla, bool permiteCrear, bool editarTrasCrear)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDeVentaDtm();
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

        protected override IQueryable<TipoDeVentaDtm> AplicarJoins(IQueryable<TipoDeVentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                           .Include(x => x.Estado);
        }

        protected override int ValidarNoHayElementos(TipoDeVentaDtm registro, ParametrosDeNegocio parametros)
        {
            var cantidad = base.ValidarNoHayElementos(registro, parametros);
            if (cantidad > 0 && parametros.Operacion == enumTipoOperacion.Modificar)
            {
                // TODO: completar cuando se conozca el detalle funcional del negocio de documentos de venta
            }
            return cantidad;
        }

        protected override void AntesDePersistir(TipoDeVentaDtm tipoDeVenta, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(tipoDeVenta, parametros);
        }

    }
}
