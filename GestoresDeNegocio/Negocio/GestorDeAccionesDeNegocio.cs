using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using Gestor.Errores;
using System.Collections.Generic;
using System;
using ServicioDeDatos.Negocio;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Entorno;
using GestoresDeNegocio.Entorno;
using ServicioDeDatos.Elemento;
using ModeloDeDto.Negocio;

namespace GestoresDeNegocio.Negocio
{
    public class GestorDeAccionesDeNegocio : GestorDeElementos<ContextoSe, AccionDeNegocioDtm, AccionDeNegocioDto>
    {
        public class MapearAccionesDeNegocio : Profile
        {
            public MapearAccionesDeNegocio()
            {
                CreateMap<AccionDeNegocioDtm, AccionDeNegocioDto>()
                .ForMember(dto => dto.Negocio, dtm => dtm.MapFrom(dtm => dtm.Negocio.Nombre))
                .ForMember(dto => dto.IdNegocioAfectado, dtm => dtm.MapFrom(dtm => dtm.IdNegocio))
                .ForMember(dto => dto.Accion, dtm => dtm.MapFrom(dtm => dtm.Accion.Nombre));

                CreateMap<AccionDeNegocioDto, AccionDeNegocioDtm>()
                .ForMember(dtm => dtm.IdNegocio, dto => dto.MapFrom(dto => dto.IdNegocioAfectado))
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore())
                .ForMember(dtm => dtm.Accion, dto => dto.Ignore());
            }
        }

        public GestorDeAccionesDeNegocio(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {
        }
        public static GestorDeAccionesDeNegocio Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeAccionesDeNegocio(contexto, mapeador);
        }


        public static AccionDeNegocioDtm CrearAccionDeNegocio(ContextoSe contexto, enumNegocio negocio, string nombre, enumMomentoDeAccion momento, string parametro, int orden, string descripcion)
        {
            AccionDtm accion = GestorDeAcciones.LeerAccion(contexto, nombre);
            var gestor = Gestor(contexto, contexto.Mapeador);
            var idNegocio = NegociosDeSe.IdNegocio(negocio);
            var f1 = new ClausulaDeFiltrado(nameof(AccionDeNegocioDtm.IdNegocio), enumCriteriosDeFiltrado.igual, idNegocio);
            var f2 = new ClausulaDeFiltrado(nameof(AccionDeNegocioDtm.IdAccion), enumCriteriosDeFiltrado.igual, $"{accion.Id}");
            var f3 = new ClausulaDeFiltrado(nameof(AccionDeNegocioDtm.Momento), enumCriteriosDeFiltrado.igual, momento);
            var f4 = new ClausulaDeFiltrado(nameof(AccionDeNegocioDtm.Orden), enumCriteriosDeFiltrado.igual, orden);
            var filtros = new List<ClausulaDeFiltrado> { f1, f2, f3, f4 };
            var leido = gestor.LeerRegistros(0, -1, filtros);
            if (leido.Count == 0)
            {
                var accionDeRelacion = new AccionDeNegocioDtm();
                accionDeRelacion.IdNegocio = NegociosDeSe.IdNegocio(negocio);
                accionDeRelacion.IdAccion = accion.Id;
                accionDeRelacion.Momento = momento;
                accionDeRelacion.Activo = true;
                accionDeRelacion.Parametros = parametro;
                accionDeRelacion.Orden = orden;
                accionDeRelacion.Descripcion = descripcion;

                accionDeRelacion = gestor.PersistirRegistro(accionDeRelacion, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(accionDeRelacion.Id, false, false, false, true);
            }
            if (leido[0].Descripcion != descripcion || leido[0].Parametros != parametro)
            {
                leido[0].Descripcion = descripcion;
                leido[0].Parametros = parametro;
                gestor.PersistirRegistro(leido[0], new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }

            return leido[0];
        }

        protected override IQueryable<AccionDeNegocioDtm> AplicarJoins(IQueryable<AccionDeNegocioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Negocio)
                               .Include(a => a.Accion);
            return consulta;
        }

        protected override IQueryable<AccionDeNegocioDtm> AplicarFiltros(IQueryable<AccionDeNegocioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.Equals(nameof(INombre.Nombre), StringComparison.CurrentCultureIgnoreCase))
                {
                    filtro.Clausula = $"{nameof(AccionDeNegocioDtm.Accion)}.{nameof(INombre.Nombre)}";
                }
                if (filtro.Clausula.Equals(nameof(AccionDeNegocioDto.IdNegocioAfectado), StringComparison.CurrentCultureIgnoreCase))
                {
                    filtro.Aplicado = true;
                    var filtroIdNegocio = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(AccionDeNegocioDtm.IdNegocio).ToLower());
                    if (filtroIdNegocio != null)
                    {
                        filtroIdNegocio.Valor = filtro.Valor;
                        filtro.Aplicado = true;
                    }
                    else
                    {
                        filtro.Clausula = nameof(AccionDeNegocioDtm.IdNegocio);
                    }
                }
            }

            return consulta;
        }

        protected override void AntesDePersistir(AccionDeNegocioDtm accion, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(accion, parametros);
            if (parametros.Operacion == enumTipoOperacion.Insertar || parametros.Operacion == enumTipoOperacion.Modificar)
                ValidarAccion(accion);
        }

        protected override void ValidarPermisosDePersistencia(AccionDeNegocioDtm registro, ParametrosDeNegocio parametros)
        {
            if (!Contexto.DatosDeConexion.EsAdministrador)
                GestorDeErrores.Emitir("Una acción de relación solo es modificable por el administrador");
        }
        protected override void DespuesDeMapearElElemento(AccionDeNegocioDtm registro, AccionDeNegocioDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            if (Contexto.DatosDeConexion.EsAdministrador) 
                elemento.ModoDeAcceso = ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor;
        }

        private void ValidarAccion(AccionDeNegocioDtm accion)
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
        }


    }
}
