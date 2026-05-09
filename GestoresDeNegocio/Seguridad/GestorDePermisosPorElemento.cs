using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using ModeloDeDto.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace GestoresDeNegocio.Seguridad
{

    public class GestorDePermisosPorElemento : GestorDeElementos<ContextoSe, PermisosPorElementoDtm, PermisosPorElementoDto>
    {
        public class MapearPermisosPorElemento : Profile
        {
            public MapearPermisosPorElemento()
            {
                CreateMap<PermisosPorElementoDtm, PermisosPorElementoDto>()
                .ForMember(dto => dto.Negocio, dtm => dtm.MapFrom(dtm => dtm.Negocio.Nombre))
                .ForMember(dto => dto.Usuario, dtm => dtm.MapFrom(dtm => dtm.Usuario.Expresion))
                .ForMember(dto => dto.Permiso, dtm => dtm.MapFrom(dtm => dtm.Permiso.Nombre));
                CreateMap<PermisosPorElementoDto, PermisosPorElementoDtm>()
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore())
                .ForMember(dtm => dtm.Usuario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Permiso, dto => dto.Ignore());
            }
        }

        public GestorDePermisosPorElemento(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {

        }

        public static GestorDePermisosPorElemento Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePermisosPorElemento(contexto, mapeador);
        }

        protected override IQueryable<PermisosPorElementoDtm> AplicarJoins(IQueryable<PermisosPorElementoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Negocio);
            consulta = consulta.Include(p => p.Permiso);
            consulta = consulta.Include(p => p.Usuario);
            return consulta;
        }

        protected override IQueryable<PermisosPorElementoDtm> AplicarFiltros(IQueryable<PermisosPorElementoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.ToLower().Equals(nameof(PermisosPorElementoDtm.IdNegocio).ToLower())
                    && filtro.Criterio.Equals(enumCriteriosDeFiltrado.igual)
                    && NegociosDeSe.LeerNegocioPorId(filtro.Valor.ToString().Entero()).Enumerado.Equals(enumNegocio.Usuario.ToString())
                    )
                    filtro.Aplicado = true;
            }

            return consulta;
        }

        protected override void AntesDePersistir(PermisosPorElementoDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            var permiso = GestorDePermisos.LeerRegistroPorId(Contexto, registro.IdPermiso);
            var negocio = NegociosDeSe.ToEnumerado(registro.IdNegocio);
            registro.IdElemento = PermisosDelElementoSql.LeerElementoPorIdPermiso(Contexto, negocio.TablaDePermisos(), NegociosDeSe.TipoDtm(negocio), registro.IdPermiso).IdElemento;
            
            if (!Contexto.DatosDeConexion.EsAdministrador && parametros.Parametros.LeerValor(ltrParametrosNeg.ValidarPermisosDePersistencia, true))
                GestorDeErrores.Emitir("La gestión de permisos solo pueda hacerse por un administrador");

            if (parametros.Eliminando && registro.Calculado)
                GestorDeErrores.Emitir($"Para borrar el permiso {permiso.Nombre} calculado, ha de modificar el rol que lo incluye");

            if (parametros.Insertando)
            {
                ApiDePermisos.ValidarQueElPermisoNoEstaOtorgado(Contexto, registro, permiso);

                if (PermisosPorElementoSql.EstaElPermisoOtorgado(Contexto, registro))
                    GestorDeErrores.Emitir($"El permisos {permiso.Nombre} ya está otorgado, o de manera directa o por haberse heredado al asignar uno de índole superior");
            }
            GestorDePermisos.ActualizarCachesDePermisos();
        }

        protected override void DespuesDeMapearElElemento(PermisosPorElementoDtm registro, PermisosPorElementoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            var gestor = NegociosDeSe.CrearGestor(Contexto, NegociosDeSe.ToEnumerado(registro.IdNegocio));
            elemento.Elemento = ((INombre)gestor.LeerRegistroPorId(registro.IdElemento, false)).Nombre;
            elemento.ModoDeAcceso = registro.Calculado
            ? enumModoDeAccesoDeDatos.Consultor
            : Contexto.DatosDeConexion.EsAdministrador ? enumModoDeAccesoDeDatos.Gestor : enumModoDeAccesoDeDatos.Consultor;
        }

        protected override PermisosPorElementoDto DespuesDePersistirElementoDto(PermisosPorElementoDto elementoDto, PermisosPorElementoDtm registro, ParametrosDeNegocio parametros)
        {
            var nuevoDto = base.DespuesDePersistirElementoDto(elementoDto, registro, parametros);
            if (parametros.Operacion != enumTipoOperacion.Insertar)
                return nuevoDto;

            var permiso = GestorDePermisos.LeerRegistroPorId(Contexto, registro.IdPermiso, true);
            var tipo = ApiDeEnsamblados.ToEnumerado<enumModoDeAccesoDeDatos>(permiso.Tipo.Nombre);

            if (tipo == enumModoDeAccesoDeDatos.Consultor)
                return nuevoDto;

            var negocio = NegociosDeSe.ToEnumerado(nuevoDto.IdNegocio);
            var permisosDelElemento = PermisosDelElementoSql.LeerPorIdElemento(Contexto, negocio.TablaDePermisos(), NegociosDeSe.TipoDtm(negocio), nuevoDto.IdElemento);
            OtorgarPermiso(nuevoDto, permisosDelElemento.IdConsultor);
            if (tipo == enumModoDeAccesoDeDatos.Administrador) 
                OtorgarPermiso(nuevoDto, permisosDelElemento.IdGestor);


            GestorDePermisos.ActualizarCachesDePermisos();
            return nuevoDto;
        }

        private void OtorgarPermiso(PermisosPorElementoDto elementoDto, int idPermiso)
        {
            var registro = new PermisosPorElementoDtm
            {
                IdElemento = elementoDto.IdElemento,
                IdNegocio = elementoDto.IdNegocio,
                IdPermiso = idPermiso,
                IdUsuario = elementoDto.IdUsuario,
                Calculado = false
            };
            var idPuesto =  ApiDePermisos.PuestoDondeEsta(Contexto, registro);
            
            if (idPuesto==0 && !PermisosPorElementoSql.EstaElPermisoOtorgado(Contexto, registro)) 
               PersistirRegistro(registro, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
        }
    }
}
