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
using ModeloDeDto.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace GestoresDeNegocio.Seguridad
{

    public class GestorDePermisosPorTransicion : GestorDeElementos<ContextoSe, PermisosPorTransicionDtm, PermisosPorTransicionDto>
    {
        public class MapearPermisosPorTransicion : Profile
        {
            public MapearPermisosPorTransicion()
            {
                CreateMap<PermisosPorTransicionDtm, PermisosPorTransicionDto>()
                .ForMember(dto => dto.Negocio, dtm => dtm.MapFrom(dtm => dtm.Negocio.Nombre))
                .ForMember(dto => dto.Usuario, dtm => dtm.MapFrom(dtm => dtm.Usuario.Expresion))
                .ForMember(dto => dto.Permiso, dtm => dtm.MapFrom(dtm => dtm.Permiso.Nombre));
                CreateMap<PermisosPorTransicionDto, PermisosPorTransicionDtm>()
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore())
                .ForMember(dtm => dtm.Usuario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Permiso, dto => dto.Ignore());
            }
        }

        public GestorDePermisosPorTransicion(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {

        }

        public static GestorDePermisosPorTransicion Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePermisosPorTransicion(contexto, mapeador);
        }

        protected override IQueryable<PermisosPorTransicionDtm> AplicarJoins(IQueryable<PermisosPorTransicionDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Negocio);
            consulta = consulta.Include(p => p.Permiso);
            consulta = consulta.Include(p => p.Usuario);
            return consulta;
        }

        protected override IQueryable<PermisosPorTransicionDtm> AplicarFiltros(IQueryable<PermisosPorTransicionDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.ToLower().Equals(nameof(PermisosPorTransicionDtm.IdNegocio).ToLower())
                    && filtro.Criterio.Equals(enumCriteriosDeFiltrado.igual)
                    && NegociosDeSe.LeerNegocioPorId(filtro.Valor.ToString().Entero()).Enumerado.Equals(enumNegocio.Usuario.ToString())
                    )
                    filtro.Aplicado = true;
            }

            return consulta;
        }

        protected override void AntesDePersistir(PermisosPorTransicionDtm registro, ParametrosDeNegocio parametros)
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

                if (PermisosPorTransicionSql.EstaElPermisoOtorgado(Contexto, registro))
                    GestorDeErrores.Emitir($"El permisos {permiso.Nombre} ya está otorgado, o de manera directa o por haberse heredado al asignar uno de índole superior");
            }
            GestorDePermisos.ActualizarCachesDePermisos();
        }

        protected override void DespuesDeMapearElElemento(PermisosPorTransicionDtm registro, PermisosPorTransicionDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            var gestor = NegociosDeSe.CrearGestorDeTransiciones(Contexto, NegociosDeSe.ToEnumerado(registro.IdNegocio));
            elemento.Transicion = ((INombre)gestor.LeerRegistroPorId(registro.IdTransicion, false)).Nombre;
            elemento.ModoDeAcceso = registro.Calculado
            ? enumModoDeAccesoDeDatos.Consultor
            : Contexto.DatosDeConexion.EsAdministrador ? enumModoDeAccesoDeDatos.Gestor : enumModoDeAccesoDeDatos.Consultor;
        }

    }
}
