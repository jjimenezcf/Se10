using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GestorDeElementos;
using GestoresDeNegocio.Entorno;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace GestoresDeNegocio.Seguridad
{

    public class GestorDeRolesDeUnPuesto : GestorDeRelaciones<ContextoSe, RolesDeUnPuestoDtm, RolesDeUnPuestoDto>
    {

        public class MapearRolesDeUnPuesto : Profile
        {
            public MapearRolesDeUnPuesto()
            {
                CreateMap<RolesDeUnPuestoDtm, RolesDeUnPuestoDto>()
                    .ForMember(dto => dto.Puesto, dtm => dtm.MapFrom(dtm => dtm.Puesto.Nombre))
                    .ForMember(dto => dto.Rol, dtm => dtm.MapFrom(dtm => dtm.Rol.Nombre));

                CreateMap<RolesDeUnPuestoDto, RolesDeUnPuestoDtm>()
                    .ForMember(dtm => dtm.Rol, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Puesto, dto => dto.Ignore()); ;
            }
        }

        public GestorDeRolesDeUnPuesto(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {


        }

        //public static GestorDeRolesDeUnPuesto Gestor(ContextoSe contexto) => Gestor(contexto, () => new GestorDeRolesDeUnPuesto(contexto, contexto.Mapeador));

        //public static GestorDeRolesDeUnPuesto Gestor(ContextoSe contexto, IMapper mapeador)
        //{
        //    return Gestor(contexto); // new GestorDePermisos(contexto, mapeador);
        //}

        public static GestorDeRolesDeUnPuesto Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeRolesDeUnPuesto(contexto, mapeador);
        }

        protected override IQueryable<RolesDeUnPuestoDtm> AplicarJoins(IQueryable<RolesDeUnPuestoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Rol);
            consulta = consulta.Include(p => p.Puesto);
            return consulta;
        }

        protected override IQueryable<RolesDeUnPuestoDtm> AplicarFiltros(IQueryable<RolesDeUnPuestoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            if (HayFiltroPorId(filtros))
                return consulta;

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Clausula.ToLower() == nameof(RolesDeUnPuestoDtm.IdPuesto).ToLower())
                    consulta = consulta.Where(x => x.IdPuesto == filtro.Valor.Entero());

                if (filtro.Clausula.ToLower() == nameof(RolesDeUnPuestoDtm.IdRol).ToLower())
                    consulta = consulta.Where(x => x.IdRol == filtro.Valor.Entero());

                if (filtro.Clausula.ToLower() == nameof(RolesDeUnPuestoDto.Rol).ToLower())
                    consulta = consulta.Where(x => x.Rol.Nombre.Contains(filtro.Valor));
            }

            return consulta;
        }

        protected override void DespuesDePersistir(RolesDeUnPuestoDtm rolDeUnPt, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(rolDeUnPt, parametros);
            SometerGenerarSegurida(Contexto, rolDeUnPt);
        }


        public static void SometerGenerarSegurida(ContextoSe contexto, RolesDeUnPuestoDtm rolDeUnPt)
        {
            var puestos = contexto.SeleccionarTodos<RolesDeUnPuestoDtm>(nameof(RolesDeUnPuestoDtm.IdRol), rolDeUnPt.IdRol).Select(rpt => rpt.IdPuesto).ToList();

            if (!puestos.Contains(rolDeUnPt.IdPuesto)) puestos.Add(rolDeUnPt.IdPuesto);

            SometerSeguridadParaLosPuestos(contexto, puestos);
        }

        public static void SometerSeguridadParaLosPuestos(ContextoSe contexto, List<int> puestos)
        {
            var idsDeUsuarios = new List<int>();
            foreach (var idPuesto in puestos)
            {
                var ids = contexto.SeleccionarTodos<PuestosDeUnUsuarioDtm>(new Dictionary<string, object> { { nameof(PuestosDeUnUsuarioDtm.IdPuesto), idPuesto } }).Select(x => x.IdUsuario).ToList();
                var nuevosIds = ids.Except(idsDeUsuarios);
                idsDeUsuarios.AddRange(nuevosIds);
            }


            if (idsDeUsuarios.Count > 0) TrabajosDeEntorno.SometerGenerarSeguridadParaLosUsuario(contexto, idsDeUsuarios);
            GestorDePermisos.ActualizarCachesDePermisos();
        }

        internal static void Copiar(ContextoSe contexto, int idPtOrigen, int idPtDestino) 
        => 
        RolesDeUnPuestoSql.Copiar(contexto, idPtOrigen, idPtDestino);
    }
}

