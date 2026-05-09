using GestorDeElementos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos;
using Utilidades;
using ModeloDeDto;
using AutoMapper;
using ServicioDeDatos.Seguridad;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Negocio
{
    public class GestorDeClasesDelTipo<TContexto, TDtm, TDto> : GestorDeElementos<TContexto, TDtm, TDto>
        where TContexto : ContextoSe
        where TDtm : ClaseDelTipoDtm
        where TDto : ClaseDelTipoDto
    {

        public class MapearClasesDelTipo : Profile
        {
            public MapearClasesDelTipo()
            {

            }

            protected IMappingExpression<E, D> ReglasDeMapeoDelDtoAlDtm<E, D>(IMappingExpression<E, D> rn)
            where D : TDtm
            where E : TDto
            {
                return rn;
            }

            protected IMappingExpression<D, E> ReglasDeMapeoDelDtmAlDto<D, E>(IMappingExpression<D, E> rn)
            where D : TDtm
            where E : TDto
            {
                rn = rn
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(x => x.Tipo == null ? null : x.Tipo.Expresion))
                .ForMember(dto => dto.Activa, dtm => dtm.MapFrom(x => x.Clase.Activa))
                .ForMember(dto => dto.Clase, dtm => dtm.MapFrom(x =>  x.Clase.Expresion));
                return rn;
            }
        }


        enumNegocio _negocioDeClases;
        public override enumNegocio Negocio => _negocioDeClases;

        public GestorDeClasesDelTipo(TContexto contexto, IMapper mapeador, enumNegocio negocio)
        : base(contexto, mapeador)
        {
            _negocioDeClases = negocio;
        }

        public static GestorDeClasesDelTipo<TContexto, TDtm, TDto> Gestor(TContexto contexto, IMapper mapeador, enumNegocio negocio)
        {
            var cache = ServicioDeCaches.Obtener(nameof(GestorDeElementos));
            var indice = $"{negocio}-{typeof(GestorDeClasesDelTipo<TContexto, TDtm, TDto>).Name}";
            if (!cache.ContainsKey(indice))
                cache[indice] = new GestorDeClasesDelTipo<TContexto, TDtm, TDto>(contexto, mapeador, negocio);
            return (GestorDeClasesDelTipo<TContexto, TDtm, TDto>)cache[indice];

        }

        protected override IQueryable<TDtm> AplicarJoins(IQueryable<TDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Clase);
            return consulta;
        }

        protected override IQueryable<TDtm> AplicarOrden(IQueryable<TDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override void AntesDePersistir(TDtm clase, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(clase, parametros);

        }

        protected override void DespuesDePersistir(TDtm clase, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(clase, parametros);
            var indice = Negocio.IdNegocio().ToString() + "-" + clase.IdTipo.ToString();
            ServicioDeCaches.EliminarElemento(CacheDe.Negocio_HayClasesDelTipo, indice);
        }

        protected override void DespuesDeMapearElElemento(TDtm clase, TDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(clase, elemento, parametros);
            var tipo = (ITipoDtm)Negocio.CrearGestorDeTipo(Contexto).LeerRegistroPorId(clase.IdTipo, aplicarJoin: false);
            elemento.Tipo = tipo.Nombre;
            elemento.ModoDeAcceso = clase.Clase(Contexto).Activa && tipo.Activo && Contexto.DatosDeConexion.EsAdministrador ? enumModoDeAccesoDeDatos.Administrador : enumModoDeAccesoDeDatos.Consultor;
            elemento.IdNegocio = Negocio.IdNegocio();
        }
    }
}
