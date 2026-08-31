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
    public class GestorDeTiposDeRegularizacion : GestorDeTiposDeElemento<ContextoSe, TipoDeRegularizacionDtm, TipoDeRegularizacionDto>
    {
        public class MapearTipoDeRegularizacion : MapearTipoDeElemento
        {
            public MapearTipoDeRegularizacion()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDeRegularizacionDtm, TipoDeRegularizacionDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.Regularizacion))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre));

                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDeRegularizacionDto, TipoDeRegularizacionDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDeRegularizacion(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.Regularizacion)
        {

        }

        public static GestorDeTiposDeRegularizacion Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeRegularizacion(contexto, mapeador);
        }

        public static TipoDeRegularizacionDtm PersistirTipo(ContextoSe contexto, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla, bool permiteCrear, enumRegularizacionAlm claseDeRegularizacion)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDeRegularizacionDtm();
                tipo.IdEstado = idEstado;
                tipo.ClaseDeLibro = clsLibro;
                tipo.Nombre = nombre;
                tipo.Sigla = sigla;
                tipo.PermiteCrear = permiteCrear;
                tipo.ClaseDeRegularizacion = claseDeRegularizacion;
                tipo = gestor.PersistirRegistro(tipo, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(tipo.Id, false, false, false, true);
            }

            if (leido.Nombre != nombre || leido.Sigla != sigla || leido.ClaseDeLibro != clsLibro || leido.PermiteCrear != permiteCrear || leido.ClaseDeRegularizacion != claseDeRegularizacion)
            {
                leido.Nombre = nombre; leido.Sigla = sigla; leido.ClaseDeLibro = clsLibro; leido.ClaseDeRegularizacion = claseDeRegularizacion;
                gestor.PersistirRegistro(leido, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
            return leido;
        }

        protected override IQueryable<TipoDeRegularizacionDtm> AplicarJoins(IQueryable<TipoDeRegularizacionDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                           .Include(x => x.Estado);
        }

        protected override int ValidarNoHayElementos(TipoDeRegularizacionDtm registro, ParametrosDeNegocio parametros)
        {
            var cantidad = base.ValidarNoHayElementos(registro, parametros);
            if (cantidad > 0 && parametros.Operacion == enumTipoOperacion.Modificar)
            {
                // TODO: completar cuando se conozca el detalle funcional del negocio de regularizaciones
            }
            return cantidad;
        }

        protected override void AntesDePersistir(TipoDeRegularizacionDtm tipoDeRegularizacion, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(tipoDeRegularizacion, parametros);
        }

    }
}
