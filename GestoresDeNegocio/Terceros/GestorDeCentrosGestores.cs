using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeCentrosGestores : GestorDeElementos<ContextoSe, CentroGestorDtm, CentroGestorDto>
    {
        public override enumNegocio Negocio => enumNegocio.CentroGestor;

        public class ltrCentrosGestores
        {
            public static readonly string mostrarJerarquia = nameof(mostrarJerarquia);
            public static string filtroPorTiposNoActivo => CentroGestorSql.Filtro.TiposNoActivos;
            public static string filtroPorUsuario => CentroGestorSql.Filtro.PorUsuario;
            public static string filtroPorPuesto => CentroGestorSql.Filtro.PorPuesto;
            public static string filtroPorRol => CentroGestorSql.Filtro.PorRol;
            public static string filtroPorNegocio => CentroGestorSql.Filtro.PorNegocio;
            public static string filtroPorTipoPermiso => CentroGestorSql.Filtro.PorTipoPermiso;
            public static string filtroPorSociedad => CentroGestorSql.Filtro.PorSociedad;

            public static string DandoDeBajaLaSociedad = nameof(DandoDeBajaLaSociedad);
        }

        public class MapearCentrosGestores : Profile
        {
            public MapearCentrosGestores()
            {
                CreateMap<CentroGestorDtm, CentroGestorDto>()
                .ForMember(dto => dto.Sociedad, dtm => dtm.MapFrom(dtm => dtm.Sociedad.Expresion))
                .ForMember(dto => dto.Responsable, dtm => dtm.MapFrom(dtm => dtm.Responsable.Expresion))
                .ForMember(dto => dto.CgPadre, dtm => dtm.MapFrom(dtm => dtm.CgPadre.Expresion));

                CreateMap<CentroGestorDto, CentroGestorDtm>()
                .ForMember(dtm => dtm.Sociedad, dto => dto.Ignore())
                .ForMember(dtm => dtm.CgPadre, dto => dto.Ignore())
                .ForMember(dtm => dtm.Responsable, dto => dto.Ignore())
                .ForMember(dtm => dtm.Archivo, dto => dto.Ignore());
            }
        }

        public GestorDeCentrosGestores(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeCentrosGestores Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCentrosGestores(contexto, mapeador); ;
        }

        public static JerarquiaDto LeerJerarquia(ContextoSe contexto, int? idPadre, Dictionary<string, object> filtros)
        {
            var jerarquia = GestorDeSociedades.JerarquiaDeSociedades(contexto, filtros);
            foreach (var raiz in jerarquia.Ramas)
            {
                var cgsLeidosDtm = CentroGestorSql.LeerJerarquiaDeCgsPorSociedad(contexto, idSociedad: raiz.Dto.Id, filtros);
                var mostrarJerarquia = (bool)filtros[ltrCentrosGestores.mostrarJerarquia];
                if (mostrarJerarquia)
                    ApiDeJerarquias.ApilarNodosComoJerarquiaEnRaiz(enumNegocio.CentroGestor, raiz, cgsLeidosDtm);
                else
                    ApiDeJerarquias.ApilarNodosComoHijosDeLaRaiz(enumNegocio.CentroGestor, raiz, cgsLeidosDtm);
            }
            return jerarquia;
        }

        public static CentroGestorDto PersistirCgJson(ContextoSe contexto, string cgJson, ParametrosDeNegocio parametros)
        {
            var cgDto = JsonConvert.DeserializeObject<CentroGestorDto>(cgJson);
            var gestor = Gestor(contexto, contexto.Mapeador);
            return gestor.PersistirElementoDto(cgDto, parametros);
        }

        internal static void DarDeBajaLosCgsDeUnaSociedad(ContextoSe contexto, SociedadDtm Sociedad)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var cgs = gestor.LeerRegistros(0, -1, new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado
                {
                Clausula = nameof(CentroGestorDtm.IdSociedad) ,
                Criterio = enumCriteriosDeFiltrado.igual,
                Valor = Sociedad.Id.ToString()
                }
            });
            var parametros = new ParametrosDeNegocio(enumTipoOperacion.Modificar);
            parametros.Parametros[ltrCentrosGestores.DandoDeBajaLaSociedad] = true;
            foreach (var cg in cgs)
            {
                if (cg.IdCgPadre != null || cg.Baja) continue;
                cg.Baja = true;
                gestor.PersistirRegistro(cg, parametros);
            }
        }

        public static CentroGestorDtm LeerCgPorNombre(ContextoSe contexto, string nif, string nombreCg)
        {
            var sociedad = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), nif);
            var f1 = new ClausulaDeFiltrado(nameof(CentroGestorDtm.IdSociedad), enumCriteriosDeFiltrado.igual, sociedad.Id);
            var f2 = new ClausulaDeFiltrado(nameof(CentroGestorDtm.Nombre), enumCriteriosDeFiltrado.igual, nombreCg);
            var cgs = enumNegocio.CentroGestor.SeleccionarPorFiltro<CentroGestorDtm>(contexto, new List<ClausulaDeFiltrado> { f1, f2 });
            return cgs[0];
        }

        public static CentroGestorDtm LeerCgPorCodigoFiscal(ContextoSe contexto, string nif)
        {
            var sociedad = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), nif);
            var f1 = new ClausulaDeFiltrado(nameof(CentroGestorDtm.IdSociedad), enumCriteriosDeFiltrado.igual, sociedad.Id);
            var f2 = new ClausulaDeFiltrado(nameof(CentroGestorDtm.Codigo), enumCriteriosDeFiltrado.igual, sociedad.CodigoFiscal);
            var cgs = enumNegocio.CentroGestor.SeleccionarPorFiltro<CentroGestorDtm>(contexto, new List<ClausulaDeFiltrado> { f1, f2 });
            return cgs[0];
        }

        public static void GenerarSeguridadParaElUsuario(ContextoSe contexto, int idUsuario, Action<string> logger = null)
        {
            PermisosPorCgSql.QuitarPermisos(contexto, idUsuario, calculado: true);
            var cgsAccedidos = ApiDePermisos.PermisosCalculadosDe(contexto, idUsuario, enumClaseDePermiso.CentroGestor);
            foreach (var permiso in cgsAccedidos)
            {
                var negocioDeUnCg = contexto.Set<NegociosDeUnCgDtm>().First(x => x.IdGestor == permiso.IdPermiso || x.IdConsultor == permiso.IdPermiso);
                var tipoPermiso = enumModoDeAccesoDeDatos.Parse<enumModoDeAccesoDeDatos>(permiso.Tipo, true);
                OtorgarSeguridadDeAUnUsuarioPorNegocioYCg(contexto, negocioDeUnCg, tipoPermiso, idUsuario, calculado: true);
            }

            PermisosPorCgSql.QuitarPermisos(contexto, idUsuario, calculado: false);
            var permisosDirectos = ApiDePermisos.PermisosDirectosDe(contexto, idUsuario, enumClaseDePermiso.CentroGestor);
            foreach (var permiso in permisosDirectos)
            {
                var negocioDeUnCg = contexto.Set<NegociosDeUnCgDtm>().First(x => x.IdGestor == permiso.IdPermiso || x.IdConsultor == permiso.IdPermiso);
                var tipoPermiso = enumModoDeAccesoDeDatos.Parse<enumModoDeAccesoDeDatos>(permiso.Tipo, true);
                OtorgarSeguridadDeAUnUsuarioPorNegocioYCg(contexto, negocioDeUnCg, tipoPermiso, idUsuario, calculado: false);
            }
        }

        public static void GenerarSeguridad(ContextoSe contexto, Action<string> logger = null)
        {
            var filtros = new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado(ltrParametrosNeg.QueMostrar.ToString(), enumCriteriosDeFiltrado.igual, ltrParametrosNeg.MostrarTodos.ToString())
            };
            var gCg = Gestor(contexto, contexto.Mapeador);
            var gncg = GestorDeNegociosDeUnCg.Gestor(contexto, contexto.Mapeador);
            var cgs = gCg.LeerRegistros(0, -1, filtros);
            var filtro = new ClausulaDeFiltrado(nameof(NegociosDeUnCgDtm.IdCg), enumCriteriosDeFiltrado.igual);
            filtros.Add(filtro);
            PermisosPorCgSql.EliminarTodos(contexto);
            foreach (var cg in cgs)
            {
                logger?.Invoke($"Procesando seguridad del CG: {cg.Expresion}");
                filtro.Valor = cg.Id.ToString();
                var negociosDeUnCg = gncg.LeerRegistros(0, -1, filtros);
                OtorgarSeguridadParaLosNegocioDeUnCg(contexto, cg, negociosDeUnCg, enumModoDeAccesoDeDatos.Gestor);
                OtorgarSeguridadParaLosNegocioDeUnCg(contexto, cg, negociosDeUnCg, enumModoDeAccesoDeDatos.Consultor);
            }
        }

        protected override IQueryable<CentroGestorDtm> AplicarJoins(IQueryable<CentroGestorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Sociedad).Include(x => x.CgPadre);
            consulta = consulta.Include(x => x.Responsable);
            return consulta;
        }

        protected override IQueryable<CentroGestorDtm> AplicarFiltros(IQueryable<CentroGestorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.ToLower().Equals(nameof(CentroGestorDtm.Nombre).ToLower()) && filtro.Criterio.Equals(enumCriteriosDeFiltrado.contiene) && !filtro.Valor.IsNullOrEmpty())
                {
                    // Crear el patrón para Contains: %valor%
                    string valorContains = $"%{filtro.Valor}%";

                    // Crear el patrón para StartsWith: valor%
                    string valorStartsWith = $"{filtro.Valor}%";

                    // Usar la función Like de EF.Functions para asegurar la traducción literal del patrón.
                    consulta = consulta.Where(x => EF.Functions.Like(x.Nombre, valorContains) || EF.Functions.Like(x.Codigo, valorStartsWith));

                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.ToLower().Equals(nameof(ElementoDeUnProcesoDto.IdSociedadDelCg).ToLower()) && filtro.Valor.Entero() > 0)
                {
                    consulta = consulta.Where(x => x.IdSociedad == filtro.Valor.Entero());
                    filtro.Aplicado = true;
                }
            }
            consulta = consulta.FiltrarLosCgsSeleccionables(Contexto, filtros);

            return consulta;
        }

        protected override void AntesDePersistir(CentroGestorDtm cg, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(cg, parametros);
            ValidarSociedadActiva(cg, parametros);

            if (parametros.Modificando && cg.Sigla != ((CentroGestorDtm)parametros.registroEnBd).Sigla)
                ValidarNoHayElementosReferenciados(cg, parametros.Operacion);

            if (!parametros.Eliminando)
                ValidarCgsDuplicados(cg, parametros);

            if (parametros.Eliminando)
            {
                ValidarNoHayElementosReferenciados(cg, parametros.Operacion);
                GestorDeNegociosDeUnCg.EliminarNegociosDeUnCg(Contexto, cg.Id);
            }

            if (parametros.Insertando)
            {
                CrearPermisosDeUnCg(cg);
            }
            else
            {
                cg.IdGestor = ((CentroGestorDtm)parametros.registroEnBd).IdGestor;
                cg.IdConsultor = ((CentroGestorDtm)parametros.registroEnBd).IdConsultor;
            }
        }

        protected override void DespuesDePersistir(CentroGestorDtm cg, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(cg, parametros);

            ServicioDeCaches.EliminarElemento(nameof(CentroGestorSql.LeerCgPorId), cg.Id.ToString());
            ServicioDeCaches.EliminarElemento(nameof(CentroGestorSql.CentrosGestoresDependientes), cg.Id.ToString());

            if (cg.PropiedadCambiada<string>(nameof(CentroGestorDtm.Nombre), parametros))  ModificarPermisosDeUnCg(cg);

            if (parametros.Modificando)
            {
                if (parametros.EstaDandoDeBaja(cg))
                    DarDeBajaLosCgsDependientes(cg);

                if (parametros.EstaDandoDeAlta(cg) && cg.IdCgPadre.Entero() > 0)
                    DarDeAltaLosCgsPadre(cg);
            }

            if (parametros.Insertando || parametros.Modificando)
                GestorDeNegociosDeUnCg.CrearModificarNegociosDeUnCg(Contexto, cg);

            ServicioDeCaches.EliminarCache(CacheDe.Valores);
            ServicioDeCaches.EliminarCache(ServicioDeCaches.PorId(typeof(CentroGestorDtm).FullName));
            ServicioDeCaches.EliminarCache(ServicioDeCaches.PorNombre(typeof(CentroGestorDtm).FullName));
            ServicioDeCaches.EliminarCache(ServicioDeCaches.PorAk(typeof(CentroGestorDtm).FullName));
            ServicioDeCaches.EliminarCache(ServicioDeCaches.PorPropiedad(typeof(CentroGestorDtm).FullName));
        }

        private void ValidarSociedadActiva(CentroGestorDtm cg, ParametrosDeNegocio parametros)
        {
            if ((bool)parametros.Parametros.LeerValor(ltrCentrosGestores.DandoDeBajaLaSociedad, false))
                return;

            var sociedad = GestorDeSociedades.LeerRegistroPorId(Contexto, cg.IdSociedad);
            if (sociedad.Baja)
                GestorDeErrores.Emitir($"No se puede operar con los CGs de la sociedad {sociedad.Expresion} por no estar activa");
        }

        private void DarDeAltaLosCgsPadre(CentroGestorDtm cg)
        {
            var cgPadre = LeerRegistros(0, -1, new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado
                {
                Clausula = nameof(CentroGestorDtm.Id) ,
                Criterio = enumCriteriosDeFiltrado.igual,
                Valor = cg.IdCgPadre.ToString()
                }
            })[0];

            if (cgPadre.Baja)
            {
                cgPadre.Baja = false;
                PersistirRegistro(cgPadre, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }

        }

        private void DarDeBajaLosCgsDependientes(CentroGestorDtm registro)
        {
            var cgs = LeerRegistros(0, -1, new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado
                {
                Clausula = nameof(CentroGestorDtm.IdCgPadre) ,
                Criterio = enumCriteriosDeFiltrado.igual,
                Valor = registro.Id.ToString()
                }
            });
            foreach (var cg in cgs)
            {
                if (cg.Baja) continue;
                cg.Baja = true;
                PersistirRegistro(cg, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
        }

        private void ValidarCgsDuplicados(CentroGestorDtm registro, ParametrosDeNegocio parametros)
        {
            var porSociedad = new ClausulaDeFiltrado { Clausula = nameof(CentroGestorDtm.IdSociedad), Valor = registro.IdSociedad.ToString(), Criterio = enumCriteriosDeFiltrado.igual };

            var porCodigo = new ClausulaDeFiltrado { Clausula = nameof(CentroGestorDtm.Codigo), Valor = registro.Codigo, Criterio = enumCriteriosDeFiltrado.igual };
            var registrosPorCodigo = LeerRegistros(0, 1, new List<ClausulaDeFiltrado> { porCodigo, porSociedad });

            var porNombre = new ClausulaDeFiltrado { Clausula = nameof(CentroGestorDtm.Nombre), Valor = registro.Nombre, Criterio = enumCriteriosDeFiltrado.igual };
            var registrosPorNombre = LeerRegistros(0, 1, new List<ClausulaDeFiltrado> { porNombre, porSociedad });

            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                if (registrosPorCodigo.Count > 0)
                    GestorDeErrores.Emitir($"Ya existe un CG con el mismo código para la sociedad indicada");

                if (registrosPorNombre.Count > 0)
                    GestorDeErrores.Emitir($"Ya existe un CG con el mismo nombre para la sociedad indicada");
            }

            if (parametros.Operacion == enumTipoOperacion.Modificar)
            {
                if (registrosPorCodigo.Count == 1 && registrosPorCodigo[0].Id != registro.Id)
                    GestorDeErrores.Emitir($"Ya existe un CG con el mismo código para la sociedad indicada");

                if (registrosPorNombre.Count == 1 && registrosPorNombre[0].Id != registro.Id)
                    GestorDeErrores.Emitir($"Ya existe un CG con el mismo nombre para la sociedad indicada");
            }
        }

        private void ValidarNoHayElementosReferenciados(CentroGestorDtm registro, enumTipoOperacion operacion)
        {
            var elementosPorCg = new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado { Clausula = nameof(ICamposCG.IdCg), Criterio = enumCriteriosDeFiltrado.igual, Valor = registro.Id.ToString() }
            };
            foreach (var negocio in Enum.GetValues(typeof(enumNegocio)))
            {
                if (!NegociosDeSe.EsNegocioDeBD((enumNegocio)negocio))
                    continue;

                if (operacion == enumTipoOperacion.Modificar && enumNegocio.Puesto == (enumNegocio)negocio)
                    continue;

                if (!ApiDeInterfaceDtm.ImplementaUsaCg(NegociosDeSe.TipoDtm((enumNegocio)negocio)))
                    continue;

                var gestor = NegociosDeSe.CrearGestor(Contexto, (enumNegocio)negocio);

                if (!gestor.Existen(elementosPorCg,
                    (enumNegocio)negocio == enumNegocio.Contrato
                    ? new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrDeUnContrato.NecesitaFiltrarPorClase, false } })
                    : null))
                    continue;

                GestorDeErrores.Emitir($"No se puede {(operacion == enumTipoOperacion.Modificar ? "modificar" : "eliminar")} el CG por tener elementos del negocio {NegociosDeSe.ToNombre((enumNegocio)negocio)} asociados");
            }
        }

        private void CrearPermisosDeUnCg(CentroGestorDtm cg)
        {
            cg.Consultor = CrearPermiso(cg, enumModoDeAccesoDeDatos.Consultor);
            cg.Gestor = CrearPermiso(cg, enumModoDeAccesoDeDatos.Gestor);
            cg.IdGestor = cg.Gestor.Id;
            cg.IdConsultor = cg.Consultor.Id;
        }

        private void ModificarPermisosDeUnCg(CentroGestorDtm cg)
        {
            var consultor = cg.Consultor(Contexto);
            var gestor = cg.Gestor(Contexto);

            consultor.Nombre = cg.NombreDelPermisoDe(enumModoDeAccesoDeDatos.Consultor);
            gestor.Nombre = cg.NombreDelPermisoDe(enumModoDeAccesoDeDatos.Gestor);

            consultor.ModificarComoAdministrador(Contexto);
            gestor.ModificarComoAdministrador(Contexto);
        }

        private PermisoDtm CrearPermiso(CentroGestorDtm cg, enumModoDeAccesoDeDatos modo)
        {
            var clase = Contexto.SeleccionarPorNombre<ClasePermisoDtm>(enumClaseDePermiso.Elemento.Descripcion(), errorSiNoHay: false);
            if (clase is null)
            {
                clase = new ClasePermisoDtm {  Nombre = enumClaseDePermiso.Elemento.Descripcion() }.InsertarComoAdministrador(Contexto);
            }

            var tipoPermiso = Contexto.SeleccionarPorNombre<TipoPermisoDtm>(modo == enumModoDeAccesoDeDatos.Consultor ? enumTipoPermiso.Consultor.Descripcion() : enumTipoPermiso.Gestor.Descripcion(), errorSiNoHay: false);
            if (tipoPermiso is null)
            {
                tipoPermiso = new TipoPermisoDtm { Nombre = modo == enumModoDeAccesoDeDatos.Consultor ? enumTipoPermiso.Consultor.Descripcion() : enumTipoPermiso.Gestor.Descripcion() }.InsertarComoAdministrador(Contexto);
            }

            return new PermisoDtm
            {
                IdClase = clase.Id,
                IdTipo = tipoPermiso.Id,
                Nombre = cg.NombreDelPermisoDe(modo)
            }.InsertarComoAdministrador(Contexto);
        }

        private static void OtorgarSeguridadParaLosNegocioDeUnCg(ContextoSe contexto, CentroGestorDtm cg, List<NegociosDeUnCgDtm> negociosDeUnCg, enumModoDeAccesoDeDatos tipoPermiso)
        {
            var filtro = new ClausulaDeFiltrado(nameof(PermisosDeUnRolDtm.IdPermiso), enumCriteriosDeFiltrado.igual);
            foreach (var negocioDeUnCg in negociosDeUnCg)
                OtorgarSeguridadPorNegocioDeUnCg(contexto, filtro, negocioDeUnCg, tipoPermiso);
        }

        private static void OtorgarSeguridadPorNegocioDeUnCg(ContextoSe contexto, ClausulaDeFiltrado filtro, NegociosDeUnCgDtm negocioDeUnCg, enumModoDeAccesoDeDatos tipoPermiso)
        {
            filtro.Valor = tipoPermiso.Equals(enumModoDeAccesoDeDatos.Gestor)
                ? negocioDeUnCg.IdGestor.ToString()
                : negocioDeUnCg.IdConsultor.ToString();
            var usuariosConElPermiso = GestorDePermisosDeUnUsuario.Gestor(contexto, contexto.Mapeador).LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro });
            foreach (var usuario in usuariosConElPermiso)
            {
                OtorgarSeguridadDeAUnUsuarioPorNegocioYCg(contexto, negocioDeUnCg, tipoPermiso, usuario.IdUsuario, calculado: true);
            }
            PermisosPorCgSql.EliminarCachesDePermisosPorCg();
        }

        private static void OtorgarSeguridadDeAUnUsuarioPorNegocioYCg(ContextoSe contexto, NegociosDeUnCgDtm negocioDeUnCg, enumModoDeAccesoDeDatos tipoPermiso, int idUsuario, bool calculado)
        {
            if (tipoPermiso.Equals(enumModoDeAccesoDeDatos.Gestor))
            {
                PermisosPorCgSql.Otorgar(contexto, negocioDeUnCg.IdNegocio, negocioDeUnCg.IdCg, idUsuario, negocioDeUnCg.IdGestor, calculado);
                PermisosPorCgSql.Otorgar(contexto, negocioDeUnCg.IdNegocio, negocioDeUnCg.IdCg, idUsuario, negocioDeUnCg.IdConsultor, calculado);
            }
            else
            {
                PermisosPorCgSql.Otorgar(contexto, negocioDeUnCg.IdNegocio, negocioDeUnCg.IdCg, idUsuario, negocioDeUnCg.IdConsultor, calculado);
            }
        }
    }
}

