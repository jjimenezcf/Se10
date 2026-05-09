using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace GestoresDeNegocio.Seguridad
{

    public class GestorDePermisosPorCg : GestorDeElementos<ContextoSe, PermisosPorCgDtm, PermisosPorCgDto>
    {
        public class MapearPermisosPorCg : Profile
        {
            public MapearPermisosPorCg()
            {
                CreateMap<PermisosPorCgDtm, PermisosPorCgDto>()
                .ForMember(dto => dto.Negocio, dtm => dtm.MapFrom(dtm => dtm.Negocio.Nombre))
                .ForMember(dto => dto.Usuario, dtm => dtm.MapFrom(dtm => dtm.Usuario.Expresion))
                .ForMember(dto => dto.Permiso, dtm => dtm.MapFrom(dtm => dtm.Permiso.Nombre))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion));
                CreateMap<PermisosPorCgDto, PermisosPorCgDtm>()
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore())
                .ForMember(dtm => dtm.Usuario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Permiso, dto => dto.Ignore())
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore());
            }
        }

        public GestorDePermisosPorCg(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {

        }

        public static GestorDePermisosPorCg Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePermisosPorCg(contexto, mapeador);
        }

        protected override IQueryable<PermisosPorCgDtm> AplicarJoins(IQueryable<PermisosPorCgDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Negocio);
            consulta = consulta.Include(p => p.Permiso);
            consulta = consulta.Include(p => p.Usuario);
            consulta = consulta.Include(p => p.Cg);
            return consulta;
        }

        protected override IQueryable<PermisosPorCgDtm> AplicarFiltros(IQueryable<PermisosPorCgDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.ToLower().Equals(nameof(PermisosPorCgDtm.IdNegocio).ToLower())
                    && filtro.Criterio.Equals(enumCriteriosDeFiltrado.igual)
                    && NegociosDeSe.LeerNegocioPorId(filtro.Valor.ToString().Entero()).Enumerado.Equals(enumNegocio.Usuario.ToString())
                    )
                    filtro.Aplicado = true;
            }

            return consulta;
        }

        protected override void AntesDePersistir(PermisosPorCgDtm permisoPorCg, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(permisoPorCg, parametros);
            var permiso = GestorDePermisos.LeerRegistroPorId(Contexto, permisoPorCg.IdPermiso);

            if (!Contexto.DatosDeConexion.EsAdministrador)
                GestorDeErrores.Emitir("La gestión de permisos solo pueda hacerse por un administrador");

            if (parametros.Operacion == enumTipoOperacion.Eliminar && permisoPorCg.Calculado)
                GestorDeErrores.Emitir($"Para borrar un permiso {permiso.Nombre} calculado, ha de modificar el rol o el permiso en el PT que lo incluye");

            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                ApiDePermisos.ValidarQueElPermisoNoEstaOtorgado(Contexto, permisoPorCg, permiso);

                if (PermisosPorCgSql.EstaElPermisoOtorgado(Contexto, permisoPorCg))
                    GestorDeErrores.Emitir($"El permisos {permiso.Nombre} ya está otorgado, o de manera directa o por haberse heredado al asignar uno de índole superior");
            }
            GestorDePermisos.ActualizarCachesDePermisos();
        }

        protected override void DespuesDePersistir(PermisosPorCgDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
        }

        protected override void DespuesDeMapearElElemento(PermisosPorCgDtm registro, PermisosPorCgDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            elemento.ModoDeAcceso = registro.Calculado
            ? enumModoDeAccesoDeDatos.Consultor
            : Contexto.DatosDeConexion.EsAdministrador ? enumModoDeAccesoDeDatos.Gestor : enumModoDeAccesoDeDatos.Consultor;
        }

    }
}
