using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using ModeloDeDto.Negocio;
using ModeloDeDto.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace GestoresDeNegocio.Seguridad
{

    public class GestorDePermisosPorTipo : GestorDeElementos<ContextoSe, PermisosPorTipoDtm, PermisosPorTipoDto>
    {
        public class MapearPermisosPorTipo : Profile
        {
            public MapearPermisosPorTipo()
            {
                CreateMap<PermisosPorTipoDtm, PermisosPorTipoDto>()
                .ForMember(dto => dto.Negocio, dtm => dtm.MapFrom(dtm => dtm.Negocio.Nombre))
                .ForMember(dto => dto.Usuario, dtm => dtm.MapFrom(dtm => dtm.Usuario.Expresion))
                .ForMember(dto => dto.Permiso, dtm => dtm.MapFrom(dtm => dtm.Permiso.Nombre));
                CreateMap<PermisosPorTipoDto, PermisosPorTipoDtm>()
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore())
                .ForMember(dtm => dtm.Usuario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Permiso, dto => dto.Ignore());
            }
        }

        public GestorDePermisosPorTipo(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {

        }

        public static GestorDePermisosPorTipo Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePermisosPorTipo(contexto, mapeador);
        }

        protected override IQueryable<PermisosPorTipoDtm> AplicarJoins(IQueryable<PermisosPorTipoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Negocio);
            consulta = consulta.Include(p => p.Permiso);
            consulta = consulta.Include(p => p.Usuario);
            return consulta;
        }

        protected override IQueryable<PermisosPorTipoDtm> AplicarFiltros(IQueryable<PermisosPorTipoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.ToLower().Equals(nameof(PermisosPorTipoDtm.IdNegocio).ToLower())
                    && filtro.Criterio.Equals(enumCriteriosDeFiltrado.igual)
                    && NegociosDeSe.LeerNegocioPorId(filtro.Valor.ToString().Entero()).Enumerado.Equals(enumNegocio.Usuario.ToString())
                    )
                    filtro.Aplicado = true;
            }

            return consulta;
        }

        protected override void AntesDePersistir(PermisosPorTipoDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            var permiso = GestorDePermisos.LeerRegistroPorId(Contexto, registro.IdPermiso);

            if (!Contexto.DatosDeConexion.EsAdministrador)
                GestorDeErrores.Emitir("La gestión de permisos solo pueda hacerse por un administrador");

            if (parametros.Operacion == enumTipoOperacion.Eliminar && registro.Calculado)
                GestorDeErrores.Emitir($"Para borrar el permiso {permiso.Nombre} calculado, ha de modificar el rol que lo incluye");

            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                ApiDePermisos.ValidarQueElPermisoNoEstaOtorgado(Contexto, registro, permiso);

                if (PermisosPorTipoSql.EstaElPermisoOtorgado(Contexto, registro))
                    GestorDeErrores.Emitir($"El permisos {permiso.Nombre} ya está otorgado, o de manera directa o por haberse heredado al asignar uno de índole superior");
            }
            GestorDePermisos.ActualizarCachesDePermisos();
        }

        protected override void DespuesDeMapearElElemento(PermisosPorTipoDtm registro, PermisosPorTipoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            var gestor = NegociosDeSe.CrearGestor(Contexto, NegociosDeSe.ToEnumerado(registro.IdNegocio));
            elemento.Tipo = ((INombre)gestor.GestorDeTipos.LeerRegistroPorId(registro.IdTipo, false)).Nombre;
            elemento.ModoDeAcceso = registro.Calculado
            ? enumModoDeAccesoDeDatos.Consultor
            : Contexto.DatosDeConexion.EsAdministrador ? enumModoDeAccesoDeDatos.Gestor : enumModoDeAccesoDeDatos.Consultor;
        }
        protected override PermisosPorTipoDto DespuesDePersistirElementoDto(PermisosPorTipoDto elementoDto, PermisosPorTipoDtm registro, ParametrosDeNegocio parametros)
        {
            var nuevoDto = base.DespuesDePersistirElementoDto(elementoDto, registro, parametros);


            var permiso = GestorDePermisos.LeerRegistroPorId(Contexto, nuevoDto.IdPermiso, true);
            var tipo = ApiDeEnsamblados.ToEnumerado<enumModoDeAccesoDeDatos>(permiso.Tipo.Nombre);

            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                if (tipo == enumModoDeAccesoDeDatos.Consultor)
                    return nuevoDto;

                dynamic tipoDtm = NegociosDeSe.CrearGestor(Contexto, NegociosDeSe.ToEnumerado(nuevoDto.IdNegocio)).GestorDeTipos.LeerRegistroPorId(nuevoDto.IdTipo, false);

                OtorgarPermiso(nuevoDto, ((ITipoDtm)tipoDtm).IdPermisoDeConsultor);

                if (tipo == enumModoDeAccesoDeDatos.Administrador)
                {
                    if (tipoDtm.GetType().ImplementaPermisosDeInterventor()) OtorgarPermiso(nuevoDto, ((IPermisoDeInterventor)tipoDtm).IdPermisoInterventor);
                    OtorgarPermiso(nuevoDto, ((ITipoDtm)tipoDtm).IdPermisoDeGestor);
                }

                if (tipo == enumModoDeAccesoDeDatos.Interventor)
                {
                    OtorgarPermiso(nuevoDto, ((ITipoDtm)tipoDtm).IdPermisoDeGestor);
                }
            }
            return nuevoDto;

        }

        private void OtorgarPermiso(PermisosPorTipoDto elementoDto, int idPermiso)
        {
            var registro = new PermisosPorTipoDtm
            {
                IdTipo = elementoDto.IdTipo,
                IdNegocio = elementoDto.IdNegocio,
                IdPermiso = idPermiso,
                IdUsuario = elementoDto.IdUsuario,
                Calculado = false
            };
            var idPuesto = ApiDePermisos.PuestoDondeEsta(Contexto, registro);
            if (idPuesto == 0 && !PermisosPorTipoSql.EstaElPermisoOtorgado(Contexto, registro))
                registro.Insertar(Contexto);
            //PersistirRegistro(registro, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
        }

        private void QuitarPermiso(PermisosPorTipoDto elementoDto, int idPermiso)
        {
            var registro = new PermisosPorTipoDtm
            {
                IdTipo = elementoDto.IdTipo,
                IdNegocio = elementoDto.IdNegocio,
                IdPermiso = idPermiso,
                IdUsuario = elementoDto.IdUsuario,
                Calculado = false
            };
            var idPuesto = ApiDePermisos.PuestoDondeEsta(Contexto, registro);
            if (idPuesto == 0 && PermisosPorTipoSql.EstaElPermisoOtorgado(Contexto, registro))
                registro.Eliminar(Contexto);
            //PersistirRegistro(registro, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
        }
    }
}
