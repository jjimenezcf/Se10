using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using GestoresDeNegocio.Terceros;
using GestoresDeNegocio.Entorno;
using ServicioDeDatos.Entorno;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Guarderias;
using Gestor.Errores;

namespace GestoresDeNegocio.Seguridad
{
    public class GestorDePuestosDeTrabajo : GestorDeElementos<ContextoSe, PuestoDtm, PuestoDto>
    {
        public override enumNegocio Negocio => enumNegocio.Puesto;

        public class MapearPuestoDeTrabajo : Profile
        {
            public MapearPuestoDeTrabajo()
            {
                CreateMap<PuestoDtm, PuestoDto>()
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion));
                CreateMap<PuestoDto, PuestoDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore());
            }
        }

        public GestorDePuestosDeTrabajo(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDePuestosDeTrabajo Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePuestosDeTrabajo(contexto, mapeador);
        }

        public static PuestoDtm PersistirPt(ContextoSe contexto, string nif, string nombreCg, string nombrePt, string descripcion)
        {
            var cg = GestorDeCentrosGestores.LeerCgPorNombre(contexto, nif, nombreCg);
            var filtrosPorAk = new Dictionary<string, object> { { nameof(PuestoDtm.IdCg), cg.Id.ToString() }, { nameof(PuestoDtm.Nombre), nombrePt } };
            var pt = contexto.SeleccionarPorAk<PuestoDtm>(filtrosPorAk, false);
            if (pt == null)
            {
                pt = new PuestoDtm();
                pt.IdCg = cg.Id;
                pt.Nombre = nombrePt;
                pt.Descripcion = descripcion;
                pt = pt.Insertar(contexto);
            }
            return pt;
        }

        public static PuestoDtm PersistirPt(ContextoSe contexto, string nif, string nombrePt, string descripcion)
        {
            var cg = GestorDeCentrosGestores.LeerCgPorCodigoFiscal(contexto, nif);
            var filtrosPorAk = new Dictionary<string, object> { { nameof(PuestoDtm.IdCg), cg.Id.ToString() }, { nameof(PuestoDtm.Nombre), nombrePt } };
            var pt = contexto.SeleccionarPorAk<PuestoDtm>(filtrosPorAk, false);
            if (pt == null)
            {
                pt = new PuestoDtm();
                pt.IdCg = cg.Id;
                pt.Nombre = nombrePt;
                pt.Descripcion = descripcion;
                pt = pt.Insertar(contexto);
            }
            return pt;
        }

        protected override IQueryable<PuestoDtm> AplicarJoins(IQueryable<PuestoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Cg);
            return consulta;
        }

        protected override IQueryable<PuestoDtm> AplicarFiltros(IQueryable<PuestoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            if (HayFiltroPorId(filtros))
                return consulta;

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Clausula.ToLower() == nameof(PuestosDeUnUsuarioDtm.IdUsuario).ToLower() && filtro.Criterio == enumCriteriosDeFiltrado.diferente)
                    consulta = consulta.AplicarPredicado(filtro, i => !i.Usuarios.Any(r => r.IdUsuario == filtro.Valor.Entero()));

                if (filtro.Clausula.ToLower() == nameof(RolesDeUnPuestoDtm.IdRol).ToLower() && filtro.Criterio == enumCriteriosDeFiltrado.diferente)
                    consulta = consulta.AplicarPredicado(filtro, i => !i.Roles.Any(r => r.IdRol == filtro.Valor.Entero()));

                if (filtro.Clausula.ToLower() == nameof(PuestosDeUnUsuarioDtm.IdUsuario).ToLower() && filtro.Criterio == enumCriteriosDeFiltrado.igual)
                    consulta = consulta.AplicarPredicado(filtro, i => i.Usuarios.Any(r => r.IdUsuario == filtro.Valor.Entero()));

                if ((filtro.Clausula.ToLower() == nameof(PuestoDtm.Nombre).ToLower() || filtro.Clausula.ToLower() == nameof(PuestoDto.Expresion).ToLower())
                    && filtro.Criterio == enumCriteriosDeFiltrado.contiene)
                    consulta = consulta.AplicarPredicado(filtro, i => i.Nombre.Contains(filtro.Valor) || i.Cg.Nombre.StartsWith(filtro.Valor) || i.Cg.Codigo.StartsWith(filtro.Valor));

                if (filtro.Clausula.ToLower() == nameof(RolesDeUnPuestoDto.IdRol).ToLower() && filtro.Criterio == enumCriteriosDeFiltrado.igual)
                {
                    consulta = consulta.Where(x => Contexto.Set<RolesDeUnPuestoDtm>().AsNoTracking().Where(r => r.IdRol == filtro.Valor.Entero()).Select(r => r.IdPuesto).Contains(x.Id));
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.ToLower() == nameof(PermisosDeUnPuestoDto.IdPermiso).ToLower() && filtro.Criterio.Equals(enumCriteriosDeFiltrado.igual))
                {
                    consulta = consulta.Where(x => Contexto.Set<PermisosDirectosDtm>().AsNoTracking().Where(x => x.IdPermiso == filtro.Valor.Entero()).Select(x => x.IdPuesto).Contains(x.Id)
                    || Contexto.Set<PermisosHeredadosDtm>().AsNoTracking().Where(x => x.IdPermiso == filtro.Valor.Entero()).Select(x => x.IdPuesto).Contains(x.Id));
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.ToLower() == nameof(PermisosDirectosDtm.IdPermiso).ToLower() && filtro.Criterio == enumCriteriosDeFiltrado.diferente)
                {
                    consulta = consulta.Where(i => !Contexto.Set<PermisosDirectosDtm>().AsNoTracking().Where(p => p.IdPermiso == filtro.Valor.Entero()).Select(x => x.IdPuesto).Contains(i.Id));
                    filtro.Aplicado = true;
                }


            }
            return consulta;
        }

        protected override void AntesDeMapearElRegistroParaModificar(PuestoDto puestoDto, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaModificar(puestoDto, opciones);
            if (puestoDto.IdRolesDeUnPuesto is not null) opciones.Parametros[ltrDePuestoTrabajo.CopiarRoles] = puestoDto.IdRolesDeUnPuesto;
            if (puestoDto.IdPermisosDirectos is not null) opciones.Parametros[ltrDePuestoTrabajo.CopiarPermisosDirectos] = puestoDto.IdPermisosDirectos;
        }

        protected override void AntesDePersistir(PuestoDtm puesto, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(puesto, parametros);
            if ((puesto.PropiedadCambiada<string>(nameof(PuestoDtm.Nombre), parametros) || puesto.PropiedadCambiada<int>(nameof(PuestoDtm.IdCg), parametros)) &&
                parametros.EsUnaPeticion &&
                puesto.CursoDeGuarderia(Contexto) != null)
                GestorDeErrores.Emitir("No se puede modificar el nombre del puesto de trabajo por estar asociado a un curso de guardería");

            if (!parametros.Insertando && ((PuestoDtm)parametros.registroEnBd).Nombre == ltrDePuestoTrabajo.PtDeAdministrador)
            {
                GestorDeErrores.Emitir($"No se puede modificar ni borra el pt '{puesto.Nombre}'");
            }
        }

        protected override void DespuesDePersistir(PuestoDtm puesto, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(puesto, parametros);
            var idRolesDeUnPuesto = parametros.Parametros.LeerValor(ltrDePuestoTrabajo.CopiarRoles, 0);
            if (idRolesDeUnPuesto > 0)
                GestorDeRolesDeUnPuesto.Copiar(Contexto, idRolesDeUnPuesto, puesto.Id);

            var idPermisosDirectos = parametros.Parametros.LeerValor(ltrDePuestoTrabajo.CopiarPermisosDirectos, 0);
            if (idPermisosDirectos > 0)
                ExtensionDePermisosDirector.Copiar(Contexto, idPermisosDirectos, puesto.Id);
        }

        protected override void DespuesDeMapearElElemento(PuestoDtm registro, PuestoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            elemento.ModoDeAcceso = Contexto.SePuedeParametrizar() ? enumModoDeAccesoDeDatos.Administrador : enumModoDeAccesoDeDatos.Consultor;
            if (registro.CursoDeGuarderia(Contexto) != null)
            {
                elemento.DelSistema = true;
                elemento.informacion = "El puesto de trabajo está asociado a un curso de guardería";
            }
        }

    }

    public static class ExtensionDePuestos
    {
        public static (PuestosDeUnUsuarioDtm relacio, bool existe) IncluirUsuario(this PuestoDtm puesto, ContextoSe contexto, string login, string nombre, string apellido)
        {
            var idUsuario = contexto.DatosDeConexion.IdUsuario;
            contexto.AsignarUsuario(contexto.Administrador());
            try
            {
                var usuario = GestorDeUsuarios.Crear(contexto, login, nombre, apellido);
                var gestor = GestorDePuestosDeUnUsuario.Gestor(contexto, contexto.Mapeador);
                return gestor.CrearRelacion(nameof(PuestosDeUnUsuarioDtm.IdUsuario), usuario.Id, puesto.Id, errorSiYaExiste: false);
            }
            finally
            {
                contexto.AsignarUsuario(contexto.SeleccionarPorId<UsuarioDtm>(idUsuario));
            }
        }

        public static void AsociarUsuario(this PuestoDtm puesto, ContextoSe contexto, int idUsuario)
        =>
        AsociarUsuarioAlPt(contexto, puesto.Id, idUsuario);

        internal static void AsociarUsuarioAlPt(ContextoSe contexto, int idPuesto, int idUsuario)
        {
            new PuestosDeUnUsuarioDtm
            {
                IdUsuario = idUsuario,
                IdPuesto = idPuesto
            }.
            InsertarComoAdministradorSiNoExiste(contexto, new List<string> { nameof(PuestosDeUnUsuarioDtm.IdUsuario), nameof(PuestosDeUnUsuarioDtm.IdPuesto) });
            TrabajosDeEntorno.SometerGenerarSeguridadParaElUsuario(contexto, idUsuario);
        }

        public static void DesasociarUsuario(this PuestoDtm puesto, ContextoSe contexto, int idUsuario)
        =>
        DesasociarUsuarioAlPt(contexto, puesto.Id, idUsuario);

        internal static void DesasociarUsuarioAlPt(ContextoSe contexto, int idPuesto, int idUsuario)
        {
            var registro = contexto.SeleccionarTodos<PuestosDeUnUsuarioDtm>(new Dictionary<string, object> {
                     { nameof(PuestosDeUnUsuarioDtm.IdPuesto), idPuesto },
                     { nameof(PuestosDeUnUsuarioDtm.IdUsuario), idUsuario }
            });
            if (registro.Count == 1)
            {
                registro[0].EliminarComoAdministrador(contexto);
                TrabajosDeEntorno.SometerGenerarSeguridadParaElUsuario(contexto, idUsuario);
            }
        }

        internal static CursoDeGuarderiaDtm CursoDeGuarderia(this PuestoDtm puesto, ContextoSe contexto)
        =>
        contexto.SeleccionarPorPropiedades<CursoDeGuarderiaDtm>(new List<string> { nameof(CursoDeGuarderiaDtm.IdConsultor), nameof(CursoDeGuarderiaDtm.IdGestor) }, puesto.Id, errorSiNoHay: false);

    }
}
