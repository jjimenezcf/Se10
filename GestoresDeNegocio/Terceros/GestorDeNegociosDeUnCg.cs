using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Terceros;
using System;
using GestoresDeNegocio.Seguridad;
using ServicioDeDatos.Seguridad;
using System.Collections.Generic;
using ModeloDeDto.Terceros;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Negocio;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeNegociosDeUnCg : GestorDeElementos<ContextoSe, NegociosDeUnCgDtm, NegociosDeUnCgDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrNegociosDeUnCg
        {
        }

        public class MapearNegociosDeUnCg : Profile
        {
            public MapearNegociosDeUnCg()
            {
                CreateMap<NegociosDeUnCgDtm, NegociosDeUnCgDto>()
               .ForMember(x => x.Negocio, y => y.MapFrom(y => y.Negocio.Nombre))
               .ForMember(x => x.Gestor, y => y.MapFrom(y => y.Gestor.Nombre))
               .ForMember(x => x.Consultor, y => y.MapFrom(y => y.Consultor.Nombre));
            }
        }

        public GestorDeNegociosDeUnCg(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        //public static GestorDeNegociosDeUnCg Gestor(ContextoSe contexto) => Gestor(contexto, () => new GestorDeNegociosDeUnCg(contexto, contexto.Mapeador));

        //public static GestorDeNegociosDeUnCg Gestor(ContextoSe contexto, IMapper mapeador)
        //{
        //    return Gestor(contexto); // new GestorDePermisos(contexto, mapeador);
        //}
        public static GestorDeNegociosDeUnCg Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeNegociosDeUnCg(contexto, mapeador);
        }

        internal static void CrearModificarNegocios(ContextoSe contexto, NegocioDtm negocio)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var parametros = new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo);
            parametros.IncluirBajas = true;
            var cgs = GestorDeCentrosGestores.Gestor(contexto, contexto.Mapeador).LeerRegistros(0, -1, parametros: parametros);
            foreach (var cg in cgs)  PersistirSeguridad(gestor, cg, negocio);
        }

        internal static void CrearModificarNegociosDeUnCg(ContextoSe contexto, CentroGestorDtm cg)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);

            foreach (var negocio in Enum.GetValues(typeof(enumNegocio)))
            {
                if (!NegociosDeSe.UsaPermisosPorCg((enumNegocio)negocio))
                    continue;

                var negocioDtm = NegociosDeSe.LeerNegocioPorNombre(((enumNegocio)negocio).ToNombre());               
                PersistirSeguridad(gestor, cg, negocioDtm);
            }
        }

        public static void AsignarPermisos(ContextoSe contexto, string codigoCg, enumNegocio negocio, int idPuesto,  enumModoDeAccesoDeDatos modo)
        {
            var gestorPermisos = GestorDePermisosDirectos.Gestor(contexto, contexto.Mapeador);
            var filtrosPorAk = new Dictionary<string, object>();
            filtrosPorAk.Add(nameof(NegociosDeUnCgDtm.IdNegocio), negocio.IdNegocio().ToString());
            filtrosPorAk[nameof(NegociosDeUnCgDtm.IdCg)] = contexto.SeleccionarPorPropiedad<CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), codigoCg).Id.ToString();

            if (ModoDeAcceso.SoyGestor(modo))
                gestorPermisos.CrearRelacion(nameof(PermisosDirectosDtm.IdPuesto), idPuesto, contexto.SeleccionarPorAk<NegociosDeUnCgDtm>(filtrosPorAk).IdGestor, errorSiYaExiste: false);

            gestorPermisos.CrearRelacion(nameof(PermisosDirectosDtm.IdPuesto), idPuesto, contexto.SeleccionarPorAk<NegociosDeUnCgDtm>(filtrosPorAk).IdConsultor, errorSiYaExiste: false);
        }

        private static void PersistirSeguridad(GestorDeNegociosDeUnCg gestor, CentroGestorDtm cg,  NegocioDtm negocio)
        {
            var porCg = new ClausulaDeFiltrado { Clausula = nameof(NegociosDeUnCgDtm.IdCg), Valor = cg.Id.ToString(), Criterio = enumCriteriosDeFiltrado.igual };
            var porNegocio = new ClausulaDeFiltrado { Clausula = nameof(NegociosDeUnCgDtm.IdNegocio), Valor = negocio.Id.ToString(), Criterio = enumCriteriosDeFiltrado.igual };

            var negocioPorCg = gestor.LeerRegistroCacheadoPoAk(new List<ClausulaDeFiltrado> { porNegocio, porCg }, true, false, true);

            var parametros = new ParametrosDeNegocio(negocioPorCg == null ? enumTipoOperacion.Insertar: enumTipoOperacion.Modificar);
            parametros.Parametros[nameof(NegociosDeUnCgDtm.Negocio)] = negocio;
            parametros.Parametros[nameof(NegociosDeUnCgDtm.CentroGestor)] = cg;

            if (negocioPorCg == null)
            {
                negocioPorCg = new NegociosDeUnCgDtm();
                negocioPorCg.IdCg = cg.Id;
                negocioPorCg.IdNegocio = negocio.Id;
            }
            gestor.PersistirRegistro(negocioPorCg, parametros);
        }

        protected override IQueryable<NegociosDeUnCgDtm> AplicarFiltros(IQueryable<NegociosDeUnCgDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            foreach (var filtro in filtros.Where(filtro => filtro.Clausula.Equals(nameof(NegociosDeUnCgDtm.IdNegocio), StringComparison.CurrentCultureIgnoreCase)
                                                        && NegociosDeSe.IdNegocio(enumNegocio.CentroGestor) == filtro.Valor.Entero()))
            {
                filtro.Aplicado = true;
            }

            return consulta;
        }

        protected override IQueryable<NegociosDeUnCgDtm> AplicarJoins(IQueryable<NegociosDeUnCgDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.CentroGestor);
            consulta = consulta.Include(x => x.Negocio);
            consulta = consulta.Include(x => x.Gestor);
            consulta = consulta.Include(x => x.Consultor);
            return consulta;
        }

        internal static void EliminarNegociosDeUnCg(ContextoSe contexto, int idCg)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            foreach (var negocio in Enum.GetValues(typeof(enumNegocio)))
            {
                if (!NegociosDeSe.UsaPermisosPorCg((enumNegocio)negocio))
                    continue;

                var negocioDtm = NegociosDeSe.LeerNegocioPorNombre(((enumNegocio)negocio).ToNombre());
                var porCg = new ClausulaDeFiltrado { Clausula = nameof(NegociosDeUnCgDtm.IdCg), Valor = idCg.ToString(), Criterio = enumCriteriosDeFiltrado.igual };
                var porNegocio = new ClausulaDeFiltrado { Clausula = nameof(NegociosDeUnCgDtm.IdNegocio), Valor = negocioDtm.Id.ToString(), Criterio = enumCriteriosDeFiltrado.igual };

                var negociosPorCg = gestor.LeerRegistroCacheadoPoAk(new List<ClausulaDeFiltrado> { porNegocio, porCg }, false, false, true);
                gestor.PersistirRegistro(negociosPorCg, new ParametrosDeNegocio(enumTipoOperacion.Eliminar));
            }
        }

        protected override void AntesDePersistir(NegociosDeUnCgDtm negocioDeUnCg, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(negocioDeUnCg, parametros);

            if (parametros.Insertando)
            {
                var negocio = (NegocioDtm) parametros.Parametros[nameof(NegociosDeUnCgDtm.Negocio)];
                var cg = (CentroGestorDtm) parametros.Parametros[nameof(NegociosDeUnCgDtm.CentroGestor)];
                negocioDeUnCg.IdConsultor = GestorDePermisos.CrearObtener(Contexto, enumNegocio.CentroGestor, $"{negocio.Nombre} - {cg.Expresion}", enumClaseDePermiso.CentroGestor, enumModoDeAccesoDeDatos.Consultor).Id;
                negocioDeUnCg.IdGestor = GestorDePermisos.CrearObtener(Contexto, enumNegocio.CentroGestor, $"{negocio.Nombre} - {cg.Expresion}", enumClaseDePermiso.CentroGestor, enumModoDeAccesoDeDatos.Gestor).Id;
             
            }
        }


        protected override void DespuesDePersistir(NegociosDeUnCgDtm negocioDeUnCg, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(negocioDeUnCg, parametros);
            if (parametros.Eliminando)
            {
                GestorDePermisos.EliminarRegistroPorId(Contexto, negocioDeUnCg.IdGestor, parametros: parametros.Parametros);
                GestorDePermisos.EliminarRegistroPorId(Contexto, negocioDeUnCg.IdConsultor, parametros: parametros.Parametros);
            }

            if (parametros.Operacion == enumTipoOperacion.Modificar)
            {
                var negocio = (NegocioDtm)parametros.Parametros[nameof(NegociosDeUnCgDtm.Negocio)];
                var cg = (CentroGestorDtm)parametros.Parametros[nameof(NegociosDeUnCgDtm.CentroGestor)];

                var permiso = GestorDePermisos.LeerRegistroPorId(Contexto, negocioDeUnCg.IdConsultor);
                GestorDePermisos.ModificarPermisoDeDatos(Contexto, enumNegocio.CentroGestor, permiso, $"{negocio.Nombre} - {cg.Expresion}", enumClaseDePermiso.CentroGestor, enumModoDeAccesoDeDatos.Consultor);
                
                permiso = GestorDePermisos.LeerRegistroPorId(Contexto, negocioDeUnCg.IdGestor);
                GestorDePermisos.ModificarPermisoDeDatos(Contexto, enumNegocio.CentroGestor, permiso, $"{negocio.Nombre} - {cg.Expresion}", enumClaseDePermiso.CentroGestor, enumModoDeAccesoDeDatos.Gestor);
            }

            var i = $"{NegociosDeSe.ToEnumerado(negocioDeUnCg.IdNegocio)}.{negocioDeUnCg.IdCg}";
            ServicioDeCaches.EliminarElemento(CacheDe.Permisos_CgPorNegocio,i);
            ServicioDeCaches.EliminarElementos(CacheDe.Permisos_CgsConGestion, $"{NegociosDeSe.ToEnumerado(negocioDeUnCg.IdNegocio)}-");

        }

    }
}
