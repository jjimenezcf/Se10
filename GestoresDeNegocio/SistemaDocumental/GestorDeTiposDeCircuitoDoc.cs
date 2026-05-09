using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Contabilidad;
using Gestor.Errores;

namespace GestoresDeNegocio.SistemaDocumental
{
    public class GestorDeTiposDeCircuitoDoc : GestorDeTiposDeElemento<ContextoSe, TipoDeCircuitoDocDtm, TipoDeCircuitoDocDto>
    {
        public class ltrDeUnTipoDeCircuitoDoc
        {

        }

        public class MapearTipoDeCircuitoDoc : MapearTipoDeElemento
        {
            public MapearTipoDeCircuitoDoc()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDeCircuitoDocDtm, TipoDeCircuitoDocDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.CircuitoDoc))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre));

                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDeCircuitoDocDto, TipoDeCircuitoDocDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDeCircuitoDoc(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.CircuitoDoc)
        {

        }

        public static GestorDeTiposDeCircuitoDoc Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeCircuitoDoc(contexto, mapeador);
        }

        protected override IQueryable<TipoDeCircuitoDocDtm> AplicarFiltros(IQueryable<TipoDeCircuitoDocDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta =  base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroParaSeleccionarTipo(parametros.Parametros);
            return consulta;
        }

        public static TipoDeCircuitoDocDtm PersistirTipo(ContextoSe contexto, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla, bool permiteCrear = true)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDeCircuitoDocDtm();
                tipo.IdEstado = idEstado;
                tipo.ClaseDeLibro = clsLibro;
                tipo.Nombre = nombre;
                tipo.Sigla = sigla;
                tipo.PermiteCrear = permiteCrear;
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

        protected override IQueryable<TipoDeCircuitoDocDtm> AplicarJoins(IQueryable<TipoDeCircuitoDocDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                           .Include(x => x.Estado);
        }



    }
}
