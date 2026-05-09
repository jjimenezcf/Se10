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

    public class GestorDePermisosPorNegocio : GestorDeElementos<ContextoSe, PermisosPorNegocioDtm, PermisosPorNegocioDto>
    {
        public class MapearPermisosPorNegocio : Profile
        {
            public MapearPermisosPorNegocio()
            {
                CreateMap<PermisosPorNegocioDtm, PermisosPorNegocioDto>()
                .ForMember(dto => dto.Negocio, dtm => dtm.MapFrom(dtm => dtm.Negocio.Nombre))
                .ForMember(dto => dto.Usuario, dtm => dtm.MapFrom(dtm => dtm.Usuario.Expresion))
                .ForMember(dto => dto.Permiso, dtm => dtm.MapFrom(dtm => dtm.Permiso.Nombre));
                CreateMap<PermisosPorNegocioDto, PermisosPorNegocioDtm>()
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore())
                .ForMember(dtm => dtm.Usuario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Permiso, dto => dto.Ignore());
            }
        }

        public GestorDePermisosPorNegocio(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {

        }

        public static GestorDePermisosPorNegocio Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePermisosPorNegocio(contexto, mapeador);
        }

        protected override IQueryable<PermisosPorNegocioDtm> AplicarJoins(IQueryable<PermisosPorNegocioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Negocio);
            consulta = consulta.Include(p => p.Permiso);
            consulta = consulta.Include(p => p.Usuario);
            return consulta;
        }

        protected override IQueryable<PermisosPorNegocioDtm> AplicarFiltros(IQueryable<PermisosPorNegocioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.ToLower().Equals(nameof(PermisosPorNegocioDtm.IdNegocio).ToLower())
                    && filtro.Criterio.Equals(enumCriteriosDeFiltrado.igual)
                    && NegociosDeSe.LeerNegocioPorId(filtro.Valor.ToString().Entero()).Enumerado.Equals(enumNegocio.Usuario.ToString())
                    )
                    filtro.Aplicado = true;
            }

            return consulta;
        }

        protected override void AntesDePersistir(PermisosPorNegocioDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            var permiso = GestorDePermisos.LeerRegistroPorId(Contexto, registro.IdPermiso);

            if (!Contexto.DatosDeConexion.EsAdministrador)
                GestorDeErrores.Emitir("La gestión de permisos solo pueda hacerse por un administrador");

            if (parametros.Operacion == enumTipoOperacion.Eliminar && registro.Calculado)
                GestorDeErrores.Emitir($"Para borrar un permiso {permiso.Nombre} calculado, ha de modificar el rol que lo incluye");

            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                ApiDePermisos.ValidarQueElPermisoNoEstaOtorgado(Contexto, registro, permiso);

                if (PermisosPorNegocioSql.EstaElPermisoOtorgado(Contexto, registro))
                    GestorDeErrores.Emitir($"El permisos {permiso.Nombre} ya está otorgado, o de manera directa o por haberse heredado al asignar uno de índole superior");
            }
            GestorDePermisos.ActualizarCachesDePermisos();
        }

        protected override void DespuesDeMapearElElemento(PermisosPorNegocioDtm registro, PermisosPorNegocioDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            elemento.ModoDeAcceso = registro.Calculado
            ? enumModoDeAccesoDeDatos.Consultor
            : Contexto.DatosDeConexion.EsAdministrador ? enumModoDeAccesoDeDatos.Gestor : enumModoDeAccesoDeDatos.Consultor;
        }


        protected override PermisosPorNegocioDto DespuesDePersistirElementoDto(PermisosPorNegocioDto elementoDto, PermisosPorNegocioDtm registro, ParametrosDeNegocio parametros)
        {
            var nuevoDto = base.DespuesDePersistirElementoDto(elementoDto, registro, parametros);
            if (parametros.Operacion != enumTipoOperacion.Insertar)
                return nuevoDto;

            var permiso = GestorDePermisos.LeerRegistroPorId(Contexto, registro.IdPermiso, true);
            var tipo = ApiDeEnsamblados.ToEnumerado<enumModoDeAccesoDeDatos>(permiso.Tipo.Nombre);

            if (tipo == enumModoDeAccesoDeDatos.Consultor)
                return nuevoDto;

            var negocio = NegociosDeSe.LeerNegocioPorId(nuevoDto.IdNegocio);
            OtorgarPermiso(nuevoDto, negocio.IdConsultor);
            if (tipo == enumModoDeAccesoDeDatos.Administrador)
                OtorgarPermiso(nuevoDto, negocio.IdGestor);
            return nuevoDto;

        }

        private void OtorgarPermiso(PermisosPorNegocioDto elementoDto, int idPermiso)
        {
            var registro = new PermisosPorNegocioDtm
            {
                IdNegocio = elementoDto.IdNegocio,
                IdPermiso = idPermiso,
                IdUsuario = elementoDto.IdUsuario,
                Calculado = false
            };
            var idPuesto = ApiDePermisos.PuestoDondeEsta(Contexto, registro);
            if (idPuesto == 0 && !PermisosPorNegocioSql.EstaElPermisoOtorgado(Contexto,registro))
                PersistirRegistro(registro, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
        }
    }
}
