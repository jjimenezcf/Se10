using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using Gestor.Errores;
using System.Collections.Generic;
using System;
using ServicioDeDatos.Negocio;
using ModeloDeDto.Negocio;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Entorno;
using GestoresDeNegocio.Entorno;
using ServicioDeDatos.Elemento;

namespace GestoresDeNegocio.Negocio
{
    public class GestorDeAccionesDeRelacion : GestorDeElementos<ContextoSe, AccionesDeRelacionDtm, AccionesDeRelacionDto>
    {
        public override enumNegocio Negocio => enumNegocio.Accion;

        public class MapearAcciones : Profile
        {
            public MapearAcciones()
            {
                CreateMap<AccionesDeRelacionDtm, AccionesDeRelacionDto>()
                .ForMember(dto => dto.Negocio, dtm => dtm.MapFrom(dtm => dtm.Negocio.Nombre))
                .ForMember(dto => dto.Vinculado, dtm => dtm.MapFrom(dtm => dtm.Vinculado.Nombre))
                .ForMember(dto => dto.Accion, dtm => dtm.MapFrom(dtm => dtm.Accion.Nombre));

                CreateMap<AccionesDeRelacionDto, AccionesDeRelacionDtm>()
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore())
                .ForMember(dtm => dtm.Vinculado, dto => dto.Ignore())
                .ForMember(dtm => dtm.Accion, dto => dto.Ignore());
            }
        }

        public GestorDeAccionesDeRelacion(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {
        }
        public static GestorDeAccionesDeRelacion Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeAccionesDeRelacion(contexto, mapeador);
        }
        public static List<ClausulaDeFiltrado> FiltrosPorMomento(enumNegocio negocio, enumNegocio vinculado, enumMomentoDeRelacion momento)
        {            
            var idNegocio = NegociosDeSe.IdNegocio(negocio);
            var idVinculado = NegociosDeSe.IdNegocio(vinculado);
            var f1 = new ClausulaDeFiltrado(nameof(AccionesDeRelacionDtm.IdNegocio), enumCriteriosDeFiltrado.igual, idNegocio);
            var f2 = new ClausulaDeFiltrado(nameof(AccionesDeRelacionDtm.IdVinculado), enumCriteriosDeFiltrado.igual, idVinculado);
            var f3 = new ClausulaDeFiltrado(nameof(AccionesDeRelacionDtm.Momento), enumCriteriosDeFiltrado.igual, momento.ToString());
            return new List<ClausulaDeFiltrado> { f1, f2, f3 };
        }

        public static AccionesDeRelacionDtm CrearAccionDeRelacion(ContextoSe contexto, enumNegocio negocio, enumNegocio vinculado, string nombre, enumMomentoDeRelacion momento, string parametro, int orden, string descripcion)
        {
            AccionDtm accion = GestorDeAcciones.LeerAccion(contexto, nombre);
            var gestor = Gestor(contexto, contexto.Mapeador);
            var filtros = FiltrosPorMomento(negocio, vinculado, momento);
            filtros.Add(new ClausulaDeFiltrado(nameof(AccionesDeRelacionDtm.IdAccion), enumCriteriosDeFiltrado.igual, $"{accion.Id}"));
            var leido = gestor.LeerRegistros(0, -1, filtros);
            if (leido.Count == 0)
            {
                var accionDeRelacion = new AccionesDeRelacionDtm();
                accionDeRelacion.IdNegocio = NegociosDeSe.IdNegocio(negocio);
                accionDeRelacion.IdVinculado = NegociosDeSe.IdNegocio(vinculado);
                accionDeRelacion.IdAccion = accion.Id;
                accionDeRelacion.Momento = momento.ToString();
                accionDeRelacion.Activo = true;
                accionDeRelacion.Parametros = parametro;
                accionDeRelacion.Orden = orden;
                accionDeRelacion.Descripcion = descripcion;

                accionDeRelacion = gestor.PersistirRegistro(accionDeRelacion, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(accionDeRelacion.Id, false, false, false, true);
            }
            if (leido[0].Momento != momento.ToString() || leido[0].Descripcion != descripcion
                || leido[0].Parametros != parametro)
            {
                leido[0].Momento = momento.ToString();
                leido[0].Descripcion = descripcion;
                leido[0].Parametros = parametro;
                gestor.PersistirRegistro(leido[0], new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }

            return leido[0];
        }

        protected override IQueryable<AccionesDeRelacionDtm> AplicarJoins(IQueryable<AccionesDeRelacionDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Negocio)
                               .Include(y => y.Vinculado)
                               .Include(a => a.Accion);
            return consulta;
        }

        protected override IQueryable<AccionesDeRelacionDtm> AplicarFiltros(IQueryable<AccionesDeRelacionDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.Equals(nameof(INombre.Nombre), StringComparison.CurrentCultureIgnoreCase))
                {
                    filtro.Clausula = $"{nameof(AccionesDeRelacionDtm.Accion)}.{nameof(INombre.Nombre)}";
                }
            }

            return consulta;

        }

        protected override void AntesDePersistir(AccionesDeRelacionDtm accion, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(accion, parametros);
            if (parametros.Operacion == enumTipoOperacion.Insertar || parametros.Operacion == enumTipoOperacion.Modificar)
                ValidarAccion(accion);
        }

        protected override void ValidarPermisosDePersistencia(AccionesDeRelacionDtm registro, ParametrosDeNegocio parametros)
        {
            if (!Contexto.DatosDeConexion.EsAdministrador)
                GestorDeErrores.Emitir("Una acción de relación solo es modificable por el administrador");
        }

        private void ValidarAccion(AccionesDeRelacionDtm accion)
        {
            try
            {
                if (!accion.Parametros.IsNullOrEmpty())
                    JsonConvert.DeserializeObject<List<Parametro>>(accion.Parametros);
            }
            catch (Exception e)
            {
                GestorDeErrores.Emitir($"Parámetros Json {accion.Parametros} para la acción {accion.Accion} mal definidos", e);
            }

            if (!NegociosDeSe.ToEnumerado(accion.IdNegocio).EsVinculable(NegociosDeSe.ToEnumerado(accion.IdVinculado)))
                GestorDeErrores.Emitir($"Los negocios {NegociosDeSe.ToEnumerado(accion.IdNegocio)} y {NegociosDeSe.ToEnumerado(accion.IdVinculado)} no son vinculables, defina la vinculación en el MD");
        }


    }
}
