using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Dapper;
using Utilidades;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using ModeloDeDto.Negocio;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.SistemaDocumental;
using Gestor.Errores;
using ServicioDeDatos.Seguridad;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.SistemaDocumental
{
    public class GestorDeCarpetas : GestorDeElementos<ContextoSe, CarpetaDtm, CarpetaDto>
    {


        public override enumNegocio Negocio => enumNegocio.Carpeta;

        public class ltrCarpeta
        {
            public static readonly string mostrarJerarquia = nameof(mostrarJerarquia);
            public static string filtroPorArchivador => $"{nameof(CarpetaDtm.IdArchivador)}".ToLower();
        }

        public class MapearCarpetas : Profile
        {
            public MapearCarpetas()
            {
                CreateMap<CarpetaDtm, CarpetaDto>()
                .ForMember(dto => dto.Archivador, dtm => dtm.MapFrom(dtm => dtm.Archivador.Expresion))
                .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(dtm => dtm.Padre.Nombre))
                .ForMember(dto => dto.Expresion, dtm => dtm.MapFrom(dtm => $"{(dtm.Padre == null ? dtm.Nombre : $"{dtm.Padre.Nombre}.{dtm.Nombre}")}"));

                CreateMap<CarpetaDto, CarpetaDtm>()
                .ForMember(dtm => dtm.Archivador, dto => dto.Ignore())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }

        public GestorDeCarpetas(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeCarpetas Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCarpetas(contexto, mapeador);
        }

        public static JerarquiaDto LeerJerarquia(ContextoSe contexto, int? idPadre, string filtrosJson)
        {
            var filtros = filtrosJson.ToDiccionario();
            var gestor = Gestor(contexto, contexto.Mapeador);
            JerarquiaDto raiz = GestorDeArchivadores.LeerArchivadorComoRaiz(contexto, (int)filtros[ltrCarpeta.filtroPorArchivador]);
            var carpetasLeidosDtm = gestor.LeerJerarquiaDeCarpetas(idArchivador: raiz.Ramas[0].Dto.Id, filtros);
            var mostrarJerarquia = (bool)filtros[ltrCarpeta.mostrarJerarquia];
            List<NodoDtm> nodos = carpetasLeidosDtm.ToNodosDeJerarquia(mostrarNumero: true);

            if (mostrarJerarquia)
                ApiDeJerarquias.ApilarNodosComoJerarquiaEnRaiz(enumNegocio.Carpeta, raiz.Ramas[0], nodos);
            else
                ApiDeJerarquias.ApilarNodosComoHijosDeLaRaiz(enumNegocio.Carpeta, raiz.Ramas[0], nodos);

            return raiz;
        }


        internal static void ExportarArchivo(ContextoSe contexto, int idCarpeta, ArchivoDtm archivo)
        {
            var carpeta = LeerRegistroPorId(contexto, idCarpeta);
            var archivador = GestorDeArchivadores.LeerRegistroPorId(contexto, carpeta.IdArchivador);
            if (!archivador.SincronizarCon.IsNullOrEmpty())
            {
                var ruta = ObtenerRuta(contexto, archivador.SincronizarCon, carpeta);
                ApiDeArchivos.CrearDirectorio(contexto, ruta);
                ApiDeArchivos.ExportarArchivo(contexto, archivo, ruta);
            }
        }

        public List<NodoDeCarpetaDtm> LeerJerarquiaDeCarpetas(int idArchivador, Dictionary<string, object> filtros)
        {
            var archivador = Contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador);
            return archivador.LeerJerarquiaDeCarpetas(Contexto, filtros);
        }

        protected override IQueryable<CarpetaDtm> AplicarJoins(IQueryable<CarpetaDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            registros = registros.Include(x => x.Archivador).Include(x => x.Padre);
            return registros;
        }

        protected override IQueryable<CarpetaDtm> AplicarFiltros(IQueryable<CarpetaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {

            var filtroParaOperar = filtros.FirstOrDefault(x => x.Clausula.Equals(ltrFiltros.SeleccionarDestino, StringComparison.CurrentCultureIgnoreCase));
            if (filtroParaOperar != null)
            {
                var filtroCarpetaDiferente = filtros.First(x => x.Clausula.Equals(ltrFiltros.IdOrigenDiferente, StringComparison.CurrentCultureIgnoreCase));
                consulta = consulta.Where(x => x.IdArchivador == Contexto.Set<CarpetaDtm>().First(c => c.Id == filtroCarpetaDiferente.Valor.Entero()).IdArchivador && x.Id != filtroCarpetaDiferente.Valor.Entero());
                consulta = consulta.Where(x => x.Nombre.IndexOf(filtroParaOperar.Valor) > -1);
                filtroParaOperar.Aplicado = true;
                filtroCarpetaDiferente.Aplicado = true;
            }


            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            var filtroPorElemento = filtros.FirstOrDefault(x => x.Clausula.Equals(ltrParametrosNeg.Elemento, StringComparison.CurrentCultureIgnoreCase));
            if (filtroPorElemento != null && filtroPorElemento.Criterio == enumCriteriosDeFiltrado.igual && filtroPorElemento.Valor.Entero() > 0)
            {
                consulta = consulta.Where(x => x.IdArchivador == filtroPorElemento.Valor.Entero());
                filtroPorElemento.Aplicado = true;
            }
            var filtroPorIdelemento = filtros.FirstOrDefault(x => x.Clausula.Equals(ltrParametrosNeg.IdElemento, StringComparison.CurrentCultureIgnoreCase));
            if (filtroPorIdelemento != null)
            {
                consulta = consulta.Where(x => x.IdArchivador == filtroPorIdelemento.Valor.Entero());
                filtroPorIdelemento.Aplicado = true;
            }

            return consulta;
        }

        protected override void AntesDePersistir(CarpetaDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {
                var archivos = VinculoSql.LeerVinculosCon(Contexto, typeof(CarpetaDtm), enumNegocio.Archivos, ApiDeRegistroDtm.EsquemaTabla(typeof(ArchivoDtm)), registro.Id);
                if (archivos.Count > 0)
                    GestorDeErrores.Emitir($"La carpeta a suprimir tiene {archivos.Count} archivos anexados, quítelos primero");
            }
        }

        protected override void DespuesDePersistir(CarpetaDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Arc_HayCarpetas, registro.IdArchivador.ToString());
            ServicioDeCaches.EliminarCache(ServicioDeCaches.PorId(typeof(CarpetaDtm).FullName));
            ServicioDeCaches.EliminarCache(ServicioDeCaches.PorNombre(typeof(CarpetaDtm).FullName));
            ServicioDeCaches.EliminarCache(ServicioDeCaches.PorAk(typeof(CarpetaDtm).FullName));
            ServicioDeCaches.EliminarCache(ServicioDeCaches.PorPropiedad(typeof(CarpetaDtm).FullName));
        }

        internal static void SincronizarCarpetas(ContextoSe contexto, ArchivadorDtm archivador, bool primeraVez)
        {
            GestorDeCarpetas gestor = Gestor(contexto, contexto.Mapeador);
            List<int> importadas = ImportarDirectorios(gestor, archivador);
            var carpetas = ExportarCarpetas(gestor, archivador, importadas);
            foreach (var carpeta in carpetas)
            {
                var ruta = ObtenerRuta(archivador.SincronizarCon, carpetas, carpeta);
                SincronizarCarpeta(gestor, carpeta, ruta, primeraVez);
            }
        }

        private static void SincronizarCarpeta(GestorDeCarpetas gestor, NodoDtm carpeta, string ruta, bool primeraVez)
        {
            var directorioCreado = ApiDeArchivos.CrearDirectorio(gestor.Contexto, ruta);
            var ficheros = Directory.GetFiles(ruta);
            var importados = directorioCreado ? new List<int>() : ServidorDocumental.ImportarFicheros(gestor.Contexto, ficheros, enumNegocio.Carpeta, carpeta.Id);

            //Siempre paso false ya que puede ser que haya renombrado la carpeta, por tanto si existe sincronización ha de borrar los ficheros  de la antigua
            //y ponerlos en el nuevo rirectorio
            ServidorDocumental.ExportarAnexado(GestorDeArchivos.Gestor(gestor.Contexto, gestor.Contexto.Mapeador)
            , enumNegocio.Carpeta
            , carpeta.Id
            , ruta
            , importados
            , primeraVez);
        }

        private static List<int> ImportarDirectorios(GestorDeCarpetas gestor, ArchivadorDtm archivador)
        {
            IEnumerable<string> directorios = Directory.EnumerateDirectories(archivador.SincronizarCon);
            List<int> importados = new List<int>();
            foreach (var directorio in directorios)
            {
                importados = ImportarDirectorio(gestor, archivador.Id, idCarpetaPadre: 0, directorio, importados);
            }
            return importados;
        }

        private static List<NodoDtm> ExportarCarpetas(GestorDeCarpetas gestor, ArchivadorDtm archivador, List<int> importados)
        {
            IEnumerable<string> directorios = Directory.EnumerateDirectories(archivador.SincronizarCon);
            var carpetas = archivador.LeerJerarquiaDeCarpetas(gestor.Contexto, new Dictionary<string, object>());
            foreach (var carpeta in carpetas)
            {
                if (importados.Contains(carpeta.Id))
                    continue;

                var ruta = ObtenerRuta(archivador.SincronizarCon, carpetas.ToNodosDeJerarquia(mostrarNumero: false), carpeta);

                if (directorios.Contains(ruta))
                {
                    continue;
                }
                ApiDeArchivos.CrearDirectorio(gestor.Contexto, ruta);
            }
            return carpetas.ToNodosDeJerarquia(mostrarNumero: false);
        }

        private static List<int> ImportarDirectorio(GestorDeCarpetas gestor, int idArchivador, int idCarpetaPadre, string directorio, List<int> importados)
        {
            var info = new DirectoryInfo(directorio);
            if (!info.Attributes.HasFlag(FileAttributes.Normal))
                return importados;

            var idCarpeta = CrearObtenerCarpeta(gestor, idArchivador, idCarpetaPadre, directorio);
            importados.Add(idCarpeta);
            var subDirectorios = Directory.EnumerateDirectories(directorio);
            foreach (var subDirectorio in subDirectorios)
            {
                importados = ImportarDirectorio(gestor, idArchivador, idCarpeta, subDirectorio, importados);
            }

            return importados;
        }


        public static string ObtenerRuta(string rutaBase, List<NodoDtm> carpetas, NodoDtm carpeta)
        {
            var nombre = carpeta.Nombre;
            while (carpeta.IdPadre != null)
            {
                foreach (var nodo in carpetas)
                {
                    if (nodo.Id == carpeta.IdPadre)
                    {
                        nombre = $"{nodo.Nombre}\\{nombre}";
                        carpeta = nodo;
                        break;
                    }
                }
            }
            return $"{rutaBase}\\{nombre}";
        }
        internal static string ObtenerRuta(ContextoSe contexto, string rutaBase, CarpetaDtm carpeta)
        {
            var nombre = carpeta.Nombre;
            while (carpeta.IdPadre != null)
            {
                var padre = LeerRegistroPorId(contexto, (int)carpeta.IdPadre);
                nombre = $"{padre.Nombre}\\{nombre}";
                carpeta = padre;
            }
            return $"{rutaBase}\\{nombre}";
        }

        private static int CrearObtenerCarpeta(GestorDeCarpetas gestor, int idArchivador, int idCarpetaPadre, string directorio)
        {
            var filtros = new List<ClausulaDeFiltrado>();
            filtros.Add(new ClausulaDeFiltrado(nameof(CarpetaDtm.IdArchivador), enumCriteriosDeFiltrado.igual, idArchivador.ToString()));
            filtros.Add(new ClausulaDeFiltrado(nameof(CarpetaDtm.IdPadre)
                                               , idCarpetaPadre.Equals(0) ? enumCriteriosDeFiltrado.esNulo : enumCriteriosDeFiltrado.igual
                                               , idCarpetaPadre.ToString()));
            var carpetas = gestor.LeerRegistros(0, -1, filtros);
            var partes = directorio.Split(Path.DirectorySeparatorChar);
            var nombre = partes[partes.Length - 1];
            var idNuevoPadre = 0;
            var existe = false;
            foreach (var carpeta in carpetas.Where(carpeta => carpeta.Nombre.Equals(nombre)))
            {
                existe = true;
                idNuevoPadre = carpeta.Id;
                break;
            }
            if (!existe) idNuevoPadre = CrearCarpeta(gestor, idArchivador, idCarpetaPadre, nombre);
            return idNuevoPadre;
        }

        private static int CrearCarpeta(GestorDeCarpetas gestor, int idArchivador, int idCarpetaPadre, string nombre)
        {
            int idNuevoPadre;
            var carpeta = new CarpetaDtm();
            carpeta.Nombre = nombre;
            carpeta.IdArchivador = idArchivador;
            carpeta.IdPadre = idCarpetaPadre == 0 ? null : idCarpetaPadre;
            idNuevoPadre = gestor.PersistirRegistro(carpeta, new ParametrosDeNegocio(enumTipoOperacion.Insertar)).Id;
            return idNuevoPadre;
        }

        protected override void DespuesDeMapearElElemento(CarpetaDtm carpeta, CarpetaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(carpeta, elemento, parametros);
            elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Archivador, elemento.IdArchivador);
            if (ModoDeAcceso.HayPermisosDe(elemento.ModoDeAcceso, enumModoDeAccesoDeDatos.Gestor))
            {
                var archivador = Contexto.SeleccionarPorId<ArchivadorDtm>(elemento.IdArchivador);
                var degradar = archivador.DegradarPermisosDeGestor(Contexto);
                if (degradar.Degradado)
                {
                    elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                    elemento.informacion = degradar.Mensaje;
                }
            }

            if (parametros.CargarLista)
            {
                elemento.Expresion = carpeta.Referencia(Contexto);
            }
        }

        protected new List<CarpetaDto> OrdenarElementosLeidos(List<CarpetaDto> lista, ParametrosDeNegocio parametros)
        {
            return parametros.CargarLista
            ? lista.OrderBy(c => c.Expresion).ToList()
            : lista;
        }

        protected override void ValidarPermisosDePersistencia(CarpetaDtm carpeta, ParametrosDeNegocio parametros)
        {
            var archivador = (ArchivadorDtm)NegociosDeSe.CrearGestor(Contexto, enumNegocio.Archivador).LeerRegistroPorId(carpeta.IdArchivador, false);
            ApiDePermisos.ValidarPermisosDePersistencia(Contexto, enumNegocio.Archivador, typeof(ArchivadorDtm), new ParametrosDeNegocio(enumTipoOperacion.Modificar), archivador);
            var resultado = archivador.DegradarPermisosDeGestor(Contexto);
            if (resultado.Degradado)
                GestorDeErrores.Emitir(resultado.Mensaje);
        }
    }
}
