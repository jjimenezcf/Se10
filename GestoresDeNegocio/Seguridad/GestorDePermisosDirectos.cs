using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestoresDeNegocio.Entorno;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace GestoresDeNegocio.Seguridad
{

    public class GestorDePermisosDirectos : GestorDeRelaciones<ContextoSe, PermisosDirectosDtm, PermisosDeUnPuestoDto>
    {

        public class MapearPermisosDeUnPuesto : Profile
        {
            public MapearPermisosDeUnPuesto()
            {
                CreateMap<PermisosDirectosDtm, PermisosDeUnPuestoDto>()
                    .ForMember(dto => dto.CgDelPuesto, dtm => dtm.MapFrom(dtm => dtm.Puesto.Cg.Expresion))
                    .ForMember(dto => dto.Puesto, dtm => dtm.MapFrom(dtm => dtm.Puesto.Nombre))
                    .ForMember(dto => dto.Permiso, dtm => dtm.MapFrom(dtm => dtm.Permiso.Nombre));

                CreateMap<PermisosDeUnPuestoDto, PermisosDirectosDtm>()
                    .ForMember(dtm => dtm.Puesto, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Permiso, dto => dto.Ignore());
            }
        }

        public GestorDePermisosDirectos(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }


        public static GestorDePermisosDirectos Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePermisosDirectos(contexto, mapeador);
        }

        public static void OtorgarPermisos(ContextoSe contexto, enumNegocio negocio, int idElemento, int idPt, enumModoDeAccesoDeDatos modo)
        {
            var parametros = new Dictionary<string, object>
            {
             {ltrParametrosNeg.EstaEjecutandoUnaAccion, true},
             {ltrParametrosNeg.ValidarPermisosDePersistencia, false },
             {ltrParametrosNeg.IdElemento, idElemento },
             {ltrParametrosNeg.Negocio, negocio }
            };

            var permisosDelElemento = GestorDePemisosDelElemento.Gestor(contexto, negocio).LeerRegistroPorId(idElemento, true, false, false, false);
            var permisoDirecto = new PermisosDirectosDtm { IdPuesto = idPt };

            if (modo.SoyConsultor())
            {
                permisoDirecto.IdPermiso = permisosDelElemento.IdConsultor;
                permisoDirecto.Insertar(contexto, parametros);
            }

            if (modo.SoyGestor())
            {
                permisoDirecto.Id = 0;
                permisoDirecto.IdPermiso = permisosDelElemento.IdGestor;
                permisoDirecto.Insertar(contexto, parametros);
            }
            if (modo.SoyAdministrador())
            {
                permisoDirecto.Id = 0;
                permisoDirecto.IdPermiso = permisosDelElemento.IdAdministrador;
                permisoDirecto.Insertar(contexto, parametros);
            }
        }

        public static void QuitarPermisos(ContextoSe contexto, enumNegocio negocio, int idElemento, int idPt, enumModoDeAccesoDeDatos modo)
        {
            var parametros = new Dictionary<string, object>
            {
             {ltrParametrosNeg.EstaEjecutandoUnaAccion, true},
             {ltrParametrosNeg.ValidarPermisosDePersistencia, false },
             {ltrParametrosNeg.IdElemento, idElemento },
             {ltrParametrosNeg.Negocio, negocio }
            };

            var permiso = GestorDePemisosDelElemento.Gestor(contexto, negocio).LeerRegistroPorId(idElemento, true, false, false, false);
            var filtrosPorAk = new Dictionary<string, object>();
            filtrosPorAk.Add(nameof(PermisosDirectosDtm.IdPuesto), idPt);
            if (ModoDeAcceso.SoyConsultor(modo))
            {
                filtrosPorAk.Add(nameof(PermisosDirectosDtm.IdPermiso), permiso.IdConsultor);
                contexto.EliminarPorAk<PermisosDirectosDtm>(filtrosPorAk, parametros);
            }

            if (ModoDeAcceso.SoyGestor(modo))
            {
                filtrosPorAk.Add(nameof(PermisosDirectosDtm.IdPermiso), permiso.IdGestor);
                contexto.EliminarPorAk<PermisosDirectosDtm>(filtrosPorAk, parametros);
            }

            if (ModoDeAcceso.SoyAdministrador(modo))
            {
                filtrosPorAk.Add(nameof(PermisosDirectosDtm.IdPermiso), permiso.IdAdministrador);
                contexto.EliminarPorAk<PermisosDirectosDtm>(filtrosPorAk, parametros);
            }
        }


        protected override IQueryable<PermisosDirectosDtm> AplicarJoins(IQueryable<PermisosDirectosDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(rp => rp.Permiso);
            consulta = consulta.Include(rp => rp.Puesto);
            consulta = consulta.Include(rp => rp.Puesto.Cg);
            return consulta;
        }

        protected override IQueryable<PermisosDirectosDtm> AplicarFiltros(IQueryable<PermisosDirectosDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            if (HayFiltroPorId(filtros))
                return consulta;

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Clausula.ToLower() == nameof(PermisosDirectosDtm.IdPuesto).ToLower())
                    consulta = consulta.Where(x => x.IdPuesto == filtro.Valor.Entero());

                if (filtro.Clausula.ToLower() == nameof(PermisosDirectosDtm.IdPermiso).ToLower())
                    consulta = consulta.Where(x => x.IdPermiso == filtro.Valor.Entero());

                if (filtro.Clausula.ToLower() == nameof(PermisosDirectosDtm.Permiso).ToLower())
                    consulta = consulta.Where(x => x.Permiso.Nombre.Contains(filtro.Valor));
            }
            return consulta;

        }

        protected override void AntesDePersistir(PermisosDirectosDtm registro, ParametrosDeNegocio parametros)
        {
            if (!parametros.EstaEjecutandoUnaAccion && !Contexto.DatosDeConexion.EsAdministrador)
                GestorDeErrores.Emitir($"Sólo el administrador puede gestionar los permisos de un puestos de trabajo");

            base.AntesDePersistir(registro, parametros);
        }


        protected override void DespuesDePersistir(PermisosDirectosDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            var idElemento = (int)parametros.Parametros.LeerValor(ltrParametrosNeg.IdElemento, 0);
            if (idElemento > 0)
            {
                var negocio = (enumNegocio)parametros.Parametros.LeerValor(ltrParametrosNeg.Negocio, enumNegocio.No_Definido);
                if (negocio == enumNegocio.No_Definido)
                    throw new System.Exception("Para asignar o quitar el permiso directo por elemento a los usuarios de un puesto, ha de indicar el negocio ");

                var usuariosDelPt = Contexto.SeleccionarTodos<PuestosDeUnUsuarioDtm>(new Dictionary<string, object> { { nameof(PuestosDeUnUsuarioDtm.IdPuesto), registro.IdPuesto } });
                if (parametros.Insertando) foreach (var usuario in usuariosDelPt)
                    {
                        PermisosPorElementoSql.Insertar(Contexto, negocio.IdNegocio(), idElemento, registro.IdPermiso, usuario.IdUsuario, calculado: false) ;
                    }
                if (parametros.Eliminando) foreach (var usuario in usuariosDelPt)
                    {
                        PermisosPorElementoSql.Eliminar(Contexto, negocio.IdNegocio(), idElemento, registro.IdPermiso, usuario.IdUsuario, calculado: false);
                    }
            }

            GestorDePermisos.ActualizarCachesDePermisos();
            var idsDeUsuarios = Contexto.SeleccionarTodos<PuestosDeUnUsuarioDtm>(new Dictionary<string, object> { { nameof(PermisosDirectosDtm.IdPuesto), registro.IdPuesto } }).Select(x => x.IdUsuario).ToList();
            TrabajosDeEntorno.SometerGenerarSeguridadParaLosUsuario(Contexto, idsDeUsuarios);
        }
    }

    public static class ExtensionDePermisosDirector
    {
        public static bool HayPermisosDirectosDe(this PuestoDtm puesto, ContextoSe contexto, RegistroDtm registro, enumModoDeAccesoDeDatos modo)
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(registro.GetType());
            var permisosDelElemento = GestorDePemisosDelElemento.Gestor(contexto, negocio).LeerElementoPorId(registro.Id);
            if (permisosDelElemento == null)
                return false;

            var filtros = new Dictionary<string, object>() { { nameof(PermisosDirectosDtm.IdPuesto), puesto.Id } };
            var permisosDirectos = contexto.SeleccionarTodos<PermisosDirectosDtm>(filtros);
            foreach (var permisoDirecto in permisosDirectos)
            {
                var permiso = contexto.SeleccionarPorId<PermisoDtm>(permisoDirecto.IdPermiso, aplicarJoin: true);
                if (permiso.Id != permisosDelElemento.IdGestor && permiso.Id != permisosDelElemento.IdConsultor && permiso.Id != permisosDelElemento.IdAdministrador)
                    continue;

                if (permiso.Tipo.Nombre.ToModoAcceso() == modo && modo.Gestor()) return true;
                if (permiso.Tipo.Nombre.ToModoAcceso() == modo && modo.Consultor()) return true;
                if (permiso.Tipo.Nombre.ToModoAcceso() == modo && modo.Administrador()) return true;
                if (permiso.Tipo.Nombre.ToModoAcceso() == modo && modo.Intervetor()) return true;
            }
            return false;
        }

        internal static void Copiar(ContextoSe contexto, int idPtOrigen, int idPtDestino)
        =>
        PermisosDirectosSql.Copiar(contexto, idPtOrigen, idPtDestino);
    }
}
