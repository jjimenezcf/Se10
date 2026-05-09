using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.TrabajosSometidos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using ModeloDeDto.Negocio;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.TrabajosSometidos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Utilidades;

namespace GestoresDeNegocio.SistemaDocumental
{

    public class GestorDeArchivadores : GestorDeElementos<ContextoSe, ArchivadorDtm, ArchivadorDto>, IImportadorDelCorreo
    {
        public class ltrDeUnArchivador
        {

        }

        public class MapearArchivador : Profile
        {
            public MapearArchivador()
            {
                CreateMap<ArchivadorDtm, ArchivadorDto>()
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Nombre))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion));
                CreateMap<ArchivadorDto, ArchivadorDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore());

                CreateMap<ArchivadorDto, ElementoMovilOutput>();
            }
        }

        public override enumNegocio Negocio => enumNegocio.Archivador;

        //public override TiposDelTipoDeElemento TiposDelTipo => Negocio.TiposDelTipo();
        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeArchivadores.Gestor(Contexto, Contexto.Mapeador);

        public GestorDeArchivadores(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeArchivadores Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeArchivadores(contexto, mapeador);
        }

        internal static JerarquiaDto LeerArchivadorComoRaiz(ContextoSe contexto, int id)
        {
            var archivador = (ArchivadorDto)enumNegocio.Archivador.LeerElemento(contexto, id, parametros: new Dictionary<string, object> { { ltrParametrosNeg.Peticion, enumPeticion.epLeerPorId } });
            var jerarquia = new JerarquiaDto();
            var nodoDtm = new NodoDtm();
            nodoDtm.Activo = true;
            nodoDtm.Id = archivador.Id;
            nodoDtm.IdPadre = null;
            nodoDtm.Nombre = archivador.Expresion;
            nodoDtm.TipoDtm = typeof(ArchivadorDtm).FullName;
            nodoDtm.modoAcceso = archivador.ModoDeAcceso;

            var nodoDto = new NodoDto(nodoDtm, enumNegocio.Archivador.ToNombre(), typeof(ArchivadorDto).FullName, nodoDtm.modoAcceso);
            var nodoDeJerarquia = new NodoDeJerarquiaDto(nodoDto);
            jerarquia.Ramas.Add(nodoDeJerarquia);

            return jerarquia;
        }

        internal static void HacerCopiaDeSeguridad(ContextoSe contexto, ArchivoDtm archivo)
        {
            ArchivadorDtm archivador = ObtenerArchivadorDelArchivo(contexto, archivo);
            var idUsuario = contexto.DatosDeConexion.IdUsuario;
            contexto.AsignarUsuario(contexto.Administrador());
            try
            {
                var idTipo = GestorDeTiposDeArchivadores.Cfg_Id_Tipo_De_Archivador_De_BackUp(contexto);

                var filtros = new Dictionary<string, object>();
                filtros[nameof(ArchivadorDtm.IdTipo)] = idTipo;
                filtros[nameof(ArchivadorDtm.IdCg)] = archivador.IdCg;
                filtros[nameof(ArchivadorDtm.Nombre)] = "Z_" + archivador.Nombre;
                var archivadores = contexto.SeleccionarTodos<ArchivadorDtm>(filtros);

                var backUp = archivadores.Count == 0 ?
                new ArchivadorDtm
                {
                    IdCg = archivador.IdCg,
                    IdTipo = idTipo,
                    Nombre = "Z_" + archivador.Nombre,
                    Descripcion = $"Copia de seguridad para el archivador {archivador.Referencia}",
                }.Insertar(contexto) :
                archivadores[0];

                var fichero = ServidorDocumental.DescargarArchivo(contexto, archivo.Id, false, true);
                ServidorDocumental.AnexarArchivo(contexto, enumNegocio.Archivador, backUp.Id, fichero, sanitizar: false);
            }
            finally
            {
                contexto.AsignarUsuario(contexto.SeleccionarPorId<UsuarioDtm>(idUsuario));
            }
        }

        private static ArchivadorDtm ObtenerArchivadorDelArchivo(ContextoSe contexto, ArchivoDtm archivo)
        {
            int idArchivador;
            var vinculados = VinculoSql.LeerVinculosAl(contexto, typeof(ArchivadorDtm), enumNegocio.Archivos, typeof(ArchivoDtm), archivo.Id, filtros: null);
            if (vinculados.Count == 0)
            {
                vinculados = VinculoSql.LeerVinculosAl(contexto, typeof(CarpetaDtm), enumNegocio.Archivos, typeof(ArchivoDtm), archivo.Id, filtros: null);
                if (vinculados.Count == 0)
                    GestorDeErrores.Emitir($"El archivo '{archivo.Nombre}' con id '{archivo.Id}' no está vinculado ni a un archivador ni carpeta");
                idArchivador = contexto.SeleccionarPorId<CarpetaDtm>(vinculados[0].idElemento1).IdArchivador;
            }
            else idArchivador = vinculados[0].idElemento1;
            var archivador = contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador);
            return archivador;
        }

        public static void SometerSincronizarArchivador(ContextoSe contexto, string parametros)
        {
            if (parametros.IsNullOrEmpty())
                GestorDeErrores.Emitir("se ha de indicar el archivador a exportar");

            Dictionary<string, object> dic = parametros.ToDiccionarioDeParametros();
            if (!dic.ContainsKey(nameof(CarpetaDto.IdArchivador).ToLower()))
                GestorDeErrores.Emitir("No se ha indicado el archivador que se quiere sincronizar");

            ApiDePermisos.ValidarPermisosDePersistencia(contexto, enumNegocio.Archivador
                , typeof(ArchivadorDtm)
                , new ParametrosDeNegocio(enumTipoOperacion.Modificar)
                , LeerRegistroPorId(contexto, (int)(long)dic[nameof(CarpetaDto.IdArchivador).ToLower()]));

            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(GestorDeArchivadores).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosSometidos.SincronizarArchivador, dll, clase, nameof(SometerSincronizarArchivador), comunicarFin: false);
            var tu = GestorDeTrabajosDeUsuario.Crear(contexto, ts, new Dictionary<string, object> { { nameof(TrabajoDeUsuarioDtm.Parametros), parametros } });
        }

        public static void SincronizarArchivador(EntornoDeTrabajo entornoDeTrabajo)
        {
            Dictionary<string, object> parametros = entornoDeTrabajo.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            if (!parametros.ContainsKey(nameof(CarpetaDto.IdArchivador).ToLower()))
                GestorDeErrores.Emitir("No se ha indicado el archivador que se quiere sincronizar");

            var archivador = LeerRegistroPorId(entornoDeTrabajo.contextoDelProceso, (int)(long)parametros[nameof(CarpetaDtm.IdArchivador).ToLower()]);
            SincronizarArchivador(entornoDeTrabajo.contextoDelProceso, archivador);
        }

        public static void SincronizarArchivador(ContextoSe contexto, ArchivadorDtm archivador)
        {
            ApiDePermisos.ValidarPermisosDePersistencia(contexto, enumNegocio.Archivador
                , typeof(ArchivadorDtm)
                , new ParametrosDeNegocio(enumTipoOperacion.Modificar)
                , LeerRegistroPorId(contexto, archivador.Id));

            var directorioCreado = false;
            if (!Directory.Exists(archivador.SincronizarCon))
            {
                directorioCreado = true;
                Directory.CreateDirectory(archivador.SincronizarCon);
            }

            var ficheros = Directory.EnumerateFiles(archivador.SincronizarCon);
            var importados = directorioCreado ? new List<int>() : ServidorDocumental.ImportarFicheros(contexto, ficheros, enumNegocio.Archivador, archivador.Id);
            var gestor = GestorDeArchivos.Gestor(contexto, contexto.Mapeador);

            ServidorDocumental.ExportarAnexado(gestor, enumNegocio.Archivador, archivador.Id, archivador.SincronizarCon, importados, directorioCreado);

            GestorDeCarpetas.SincronizarCarpetas(contexto, archivador, directorioCreado);
            //TODO: Eliminar carpetas si no existen los directorios
        }

        internal static void ExportarArchivo(ContextoSe contexto, int idArchivador, ArchivoDtm archivo)
        {
            var archivador = LeerRegistroPorId(contexto, idArchivador);
            if (!archivador.SincronizarCon.IsNullOrEmpty())
                ApiDeArchivos.ExportarArchivo(contexto, archivo, archivador.SincronizarCon);
        }

        protected override void DespuesDeMapearElRegistro(ArchivadorDto elemento, ArchivadorDtm registro, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(elemento, registro, opciones);
            if (opciones.Insertando) opciones.Parametros[nameof(ArchivadorDto.IdArchivoAlCrear)] = elemento.IdArchivoAlCrear;
        }

        protected override IQueryable<ArchivadorDtm> AplicarJoins(IQueryable<ArchivadorDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            registros = registros.Include(p => p.Cg);

            bool filtroPorTipo = false;
            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.Equals(nameof(ArchivadorDtm.IdTipo), StringComparison.CurrentCultureIgnoreCase) && filtro.Criterio == enumCriteriosDeFiltrado.igual)
                    filtroPorTipo = true;
            }
            if (!filtroPorTipo)
                filtros.Add(new ClausulaDeFiltrado(nameof(ArchivadorDtm.IdTipo), enumCriteriosDeFiltrado.diferente, CfgIdTipoBackUp));

            return registros;
        }

        protected override IQueryable<ArchivadorDtm> AplicarFiltros(IQueryable<ArchivadorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroDeVinculadosA(Contexto, filtros);
            consulta = consulta.FiltroDeVincularCon(Contexto, filtros);
            return consulta;
        }

        protected override void Persistir(ArchivadorDtm archivador, ParametrosDeNegocio parametros)
        {
            if (!parametros.Insertando)
            {
                if (SemaforoDeProcesoSql.HaySemaforoPara(Contexto, Negocio.IdNegocio(), archivador.Id, new List<enumOpercionesDeSemaforo> { enumOpercionesDeSemaforo.EARC, enumOpercionesDeSemaforo.IZIP }))
                    GestorDeErrores.Emitir($"Se está exportando o importando '{archivador.Referencia}', no puede ser modificado, inténtelo más tarde");
            }

            base.Persistir(archivador, parametros);
        }

        protected override void AlDarDeBaja(ArchivadorDtm archivador, ParametrosDeNegocio parametros)
        {
            base.AlDarDeBaja(archivador, parametros);
            var idTipoArchivadorDePreasiento = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Tipo_De_Archivador, valorPorDefecto: Literal.Cero).Valor.Entero();
            if (archivador.IdTipo == idTipoArchivadorDePreasiento)
            {
                archivador.AnularContabilizacion(Contexto);
            }
        }

        protected override void AlDarDeAlta(ArchivadorDtm archivador, ParametrosDeNegocio parametros)
        {
            base.AlDarDeAlta(archivador, parametros);
            var idTipoArchivadorDePreasiento = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Tipo_De_Archivador, valorPorDefecto: Literal.Cero).Valor.Entero();
            if (archivador.IdTipo == idTipoArchivadorDePreasiento)
            {
                GestorDeErrores.Emitir("Un archivador de preasientos, no se puede dar de alta");
            }
        }

        protected override void AntesDePersistir(ArchivadorDtm archivador, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(archivador, parametros);

            if (Contexto.DatosDeConexion.EsClienteWeb)
                GestorDeErrores.Emitir($"Un cliente web no puede '{parametros.Operacion}' un archivador");

            var tipoArchivador = Contexto.SeleccionarPorId<TipoDeArchivadorDtm>(archivador.IdTipo);

            if ((parametros.Modificando || parametros.Eliminando) && archivador.IdTipo == CfgIdTipoBackUp)
                GestorDeErrores.Emitir($"El archivado de copia de seguridad '{archivador.Nombre}' no es modificable ni se puede eliminar");

            if ((parametros.Insertando && !archivador.SincronizarCon.IsNullOrEmpty()) || (parametros.Modificando && ((ArchivadorDtm)parametros.registroEnBd).SincronizarCon != archivador.SincronizarCon))
            {
                var modo = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Archivador, archivador);
                if (!ModoDeAcceso.SoyAdministrador(modo))
                    GestorDeErrores.Emitir($"Para indicar o modificar el directorio de sincronización de un archivador necesita permisos de administrador y tiene permisos de {modo}");
            }

            if (!parametros.Eliminando && !archivador.SincronizarCon.IsNullOrEmpty())
            {
                if (!Directory.Exists(archivador.SincronizarCon))
                    Directory.CreateDirectory(archivador.SincronizarCon);
            }

            if (parametros.Insertando && parametros.EsUnaPeticion && tipoArchivador.DelSistema)
                GestorDeErrores.Emitir($"No se puede crear un archivador del tipo '{tipoArchivador.Nombre}' por ser del sistema");

            if (parametros.Modificando && parametros.EsUnaPeticion && tipoArchivador.DelSistema)
            {
                if (archivador.SeHaModificadoElCampo<string>(x => x.Name == nameof(INombre.Nombre), parametros))
                    GestorDeErrores.Emitir($"No se puede modificar el nombre del archivador '{archivador.Referencia}' por ser del sistema");
                if (archivador.SeHaModificadoElCampo<bool>(x => x.Name == nameof(ArchivadorDtm.Baja), parametros))
                    GestorDeErrores.Emitir($"El usuario no puede indicar si el archivador  '{archivador.Referencia}' esta de alta o baja por ser del sistema");
            }
        }

        protected override void DespuesDePersistir(ArchivadorDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            if (parametros.Operacion == enumTipoOperacion.Modificar && !registro.SincronizarCon.IsNullOrEmpty())
            {
                var ficheros = Directory.EnumerateFiles(registro.SincronizarCon);
                ServidorDocumental.ImportarFicheros(Contexto, ficheros, enumNegocio.Archivador, registro.Id);
            }

            if (parametros.Insertando)
            {
                var idArchivoAlCrear = parametros.Parametros.LeerValor<int?>(nameof(ArchivadorDto.IdArchivoAlCrear), null);
                if (idArchivoAlCrear.Entero() > 0)
                    IncluirArchivoTrasCrear(registro, idArchivo: idArchivoAlCrear.Entero());
            }
        }

        protected override IQueryable<ArchivadorDtm> AplicarSeguridad(IQueryable<ArchivadorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<ArchivadorDtm, TipoDeArchivadorDtm, PermisoDelArchivadorDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<ArchivadorDtm, PermisoDelArchivadorDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        private int? _cfgIdTipoBackUp = null;
        public int CfgIdTipoBackUp { get { if (_cfgIdTipoBackUp == null) _cfgIdTipoBackUp = GestorDeTiposDeArchivadores.Cfg_Id_Tipo_De_Archivador_De_BackUp(Contexto); return (int)_cfgIdTipoBackUp; } }

        private int? _cfgIdTipoZip = null;
        public int CfgIdTipoZip { get { if (_cfgIdTipoZip == null) _cfgIdTipoZip = GestorDeTiposDeArchivadores.Cfg_Id_Tipo_De_Archivador_Zip(Contexto); return (int)_cfgIdTipoZip; } }

        private int? _cfgIdTipoSii = null;
        public int CfgIdTipoSii { get { if (_cfgIdTipoSii == null) _cfgIdTipoSii = ExtensorDeArchivadores.Cfg_Id_Tipo_De_Archivador_Sii(Contexto); return (int)_cfgIdTipoSii; } }

        protected override void DespuesDeMapearElElemento(ArchivadorDtm archivadorDtm, ArchivadorDto archivadorDto, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(archivadorDtm, archivadorDto, parametros);
            var tipo = (TipoDeArchivadorDtm)parametros.Parametros[nameof(TipoDeElementoDtm)];
            archivadorDto.DelSistema = tipo.DelSistema;
            if (archivadorDto.DelSistema)
                archivadorDto.informacion = $"El archivador '{archivadorDtm.Referencia}' es del sistema, no todas sus propiedades son modificables";


            var incluirDetalles = parametros.Parametros.LeerValor(ltrParametrosNeg.IncluirDetalles, false);

            if (!parametros.CargarLista)
            {
                archivadorDto.ConCarpetas = archivadorDtm.HayCarpetas(Contexto);
                archivadorDto.Accion = !archivadorDto.ConCarpetas ? "Crear carpetas" : "Mostrar carpetas";

                if (incluirDetalles)
                {
                    archivadorDto.Cantidad = archivadorDtm.ObtenerTotalArchivosDeArchivador(Contexto);
                    // GestorDeVinculos.Cantidad(Contexto, Negocio, enumNegocio.Archivos, archivadorDtm.Id);
                }
                if (parametros.Peticion == enumPeticion.epLeerDatosParaElGrid)
                {
                    archivadorDto.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, Negocio, archivadorDtm.Id, parametros.Parametros);
                }
                if (ModoDeAcceso.SoyGestor(archivadorDto.ModoDeAcceso))
                {
                    if (archivadorDtm.IdTipo == CfgIdTipoBackUp)
                    {
                        archivadorDto.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                        archivadorDto.informacion = $"El archivador '{archivadorDtm.Referencia}' es de backUp, no es modificable";
                        return;
                    }

                    if (archivadorDtm.IdTipo == CfgIdTipoZip)
                    {
                        archivadorDto.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                        archivadorDto.informacion = $"El archivador '{archivadorDtm.Referencia}' es de Zip, no es modificable";
                        archivadorDto.Accion = "";
                        return;
                    }

                    if (archivadorDtm.IdTipo == CfgIdTipoSii)
                    {
                        archivadorDto.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                        archivadorDto.informacion = $"El archivador '{archivadorDtm.Referencia}' es del Sii, no es modificable";
                        archivadorDto.Accion = "";
                        return;
                    }
                    archivadorDto.EsUnCorreo = archivadorDtm.EsUnCoreo(Contexto);

                    if (enumPeticion.epLeerPorId == parametros.Peticion || enumPeticion.epLeerDatosParaElGrid == parametros.Peticion)
                    {
                        var resultado = archivadorDtm.DegradarPermisosDeGestor(Contexto, analizarBloqueo: false, analizarDelSistema: false);
                        if (resultado.Degradado)
                        {
                            archivadorDto.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                            archivadorDto.informacion = resultado.Mensaje;
                        }

                        if (parametros.LeerPorIdParaEditar)
                        {
                            var operaciones = SemaforoDeProcesoSql.ObtenerOperaciones(Contexto, Negocio.IdNegocio(), archivadorDtm.Id);
                            if (operaciones.Any(x => x == enumOpercionesDeSemaforo.IZIP))
                            {
                                archivadorDto.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                                archivadorDto.informacion = $"Se está importando un zip al archivador '{archivadorDtm.Referencia}', no puede ser modificado";
                            }
                            if (operaciones.Any(x => x == enumOpercionesDeSemaforo.EARC))
                            {
                                archivadorDto.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                                archivadorDto.informacion = $"Se está exportando un zip de los archivos, no puede ser modificado";
                            }
                        }
                    }

                    if (!ModoDeAcceso.SoyGestor(archivadorDto.ModoDeAcceso)) archivadorDto.Accion = "";
                }

            }
        }

        private void IncluirArchivoTrasCrear(ArchivadorDtm archivador, int idArchivo)
        {
            var archivo = Contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
            if (ApiDeArchivos.EsUnComprimido(archivo.Nombre))
            {
                TrabajosDelSistemaDocumental.SometerImportarZip(Contexto, archivador.Id, idArchivo, remplazar: true, renombrar: false, eliminarArchivo: false, eliminarCarpeta: false);
            }
            else
                GestorDeVinculos.Vincular(Contexto, Negocio, enumNegocio.Archivos, archivador.Id, archivo.Id);
        }


        public static void AntesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var idArchivador = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            if (entorno.Negocio == enumNegocio.Carpeta)
                idArchivador = entorno.Contexto.SeleccionarPorId<CarpetaDtm>(idArchivador).IdArchivador;


            var archivador = entorno.Contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador);

            if (archivador.Bloqueado)
                GestorDeErrores.Emitir(ltrDeTrazas.Error_De_Quitar_Vinculo.Replace("Referencia", archivador.Referencia).Replace("Bloqueador", archivador.Bloqueador(entorno.Contexto).Login));
        }


        public IUsaTipoConCG ImportarDelCorreo(int idCg, int idTipo, string nombre, string descripcion, Dictionary<string, object> parametros)
        {
            var archivador = (ArchivadorDtm)ExtensorDeElementosDeUnProceso.NuevoDtm(Negocio.TipoDtm(), idCg, idTipo, nombre, descripcion, parametros);
            return archivador;
        }

        public static void SometerImportarZip(ContextoSe contexto, int idArchivador, int idArchivoZip, bool remplazar, bool renombrar, bool eliminarArchivo, bool eliminarCarpeta)
        {
            var archivador = contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador);
            if (archivador.Baja || archivador.Bloqueado)
                GestorDeErrores.Emitir($"El archivador '{archivador.Referencia}' donde importar el fichero zip, no puede estár de baja o bloqueado");

            TrabajosDelSistemaDocumental.SometerImportarZip(contexto, archivador.Id, idArchivoZip, remplazar, renombrar, eliminarArchivo, eliminarCarpeta);
        }

        public static void SometerProcesarFarConIa(ContextoSe contexto, int idArchivador, int idCg, int idTipo, int? idProveedor, int? idCarpeta)
        {
            var archivador = contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador);
            if (archivador.Baja || archivador.Bloqueado)
                GestorDeErrores.Emitir($"El archivador '{archivador.Referencia}' donde importar el fichero zip, no puede estár de baja o bloqueado");

            TrabajosDelSistemaDocumental.SometerProcesarFarConIa(contexto, archivador.Id, idCg, idTipo, idProveedor, idCarpeta);
        }

        public static int CopiarArchivador(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (!parametros.ContieneClave(nameof(CopiarArchDto.IdElemento))) GestorDeErrores.Emitir("No se ha indicado el archivador a copiar");
            if (!parametros.ContieneClave(nameof(CopiarArchDto.IdTipo))) GestorDeErrores.Emitir("No se ha indicado el tipo del archivador");
            if (!parametros.ContieneClave(nameof(CopiarArchDto.IdCg))) GestorDeErrores.Emitir("No se ha indicado el cg del archivador");
            if (!parametros.ContieneClave(nameof(CopiarArchDto.Nombre))) GestorDeErrores.Emitir("No se ha indicado el asunto del archivador");
            if (!parametros.ContieneClave(nameof(CopiarArchDto.Descripcion))) GestorDeErrores.Emitir("No se ha indicado la descripción del archivador");

            var idcg = (int)parametros.LeerValor<long>(nameof(CopiarArchDto.IdCg));
            var idtipo = (int)parametros.LeerValor<long>(nameof(CopiarArchDto.IdTipo));
            var nombre = parametros.LeerValor<string>(nameof(CopiarArchDto.Nombre));
            var descripcion = parametros.LeerValor<string>(nameof(CopiarArchDto.Descripcion));
            var copiarCarpetas = parametros.LeerValor<bool>(nameof(CopiarArchDto.CopiarCarpetas));
            var copiarArchivos = parametros.LeerValor<bool>(nameof(CopiarArchDto.CopiarArchivos));
            var enlazarArchivos = parametros.LeerValor<bool>(nameof(CopiarArchDto.EnlazarArchivos));

            var tipo = contexto.SeleccionarPorId<TipoDeArchivadorDtm>(idtipo);
            if (tipo.DelSistema) GestorDeErrores.Emitir($"El tipo '{tipo.Nombre}' es del sistema, no lo puede crear");
            if (!tipo.PermiteCrear) GestorDeErrores.Emitir($"El tipo '{tipo.Nombre}' está marcado como no creable");
            if (copiarArchivos && enlazarArchivos) GestorDeErrores.Emitir($"No se puede solicitar copiar y enlazar simultaneamente");


            var arcOrigen = contexto.SeleccionarPorId<ArchivadorDtm>((int)(long)parametros[nameof(CopiarArchDto.IdElemento)]);


            return arcOrigen.Copiar(contexto, idcg, tipo, nombre, descripcion, copiarCarpetas, copiarArchivos, enlazarArchivos).Id;
        }

        public List<CualquierVinculo> CualquierVinculadoAl(ArchivadorDtm archivador)
        {
            var vinculadosAl = new List<CualquierVinculo>();
            var tiposVinculados = Negocio.VinculosCon(Contexto);
            foreach (var tipoDtm in tiposVinculados)
            {
                var vinculos = VinculoSql.LeerVinculosAl(Contexto, tipoDtm, Negocio, Negocio.TipoDtm(), archivador.Id, filtros: null);
                if (vinculos.Count == 0)
                    continue;
                var negocioVinculado = NegociosDeSe.NegocioDeUnDtm(tipoDtm);
                foreach (var vinculo in vinculos)
                {
                    vinculadosAl.Add(new CualquierVinculo
                    {
                        Id = vinculo.Id,
                        Negocio1 = negocioVinculado,
                        idElemento1 = vinculo.idElemento1,
                        Negocio2 = Negocio,
                        idElemento2 = archivador.Id
                    });
                }
            }
            return vinculadosAl;
        }

        public static void Descontabilizar(ContextoSe contexto, List<int> ids)
        {
            var idsDeUsuarios = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Usuarios_Con_Permiso_Para_Generar, valorPorDefecto: Literal.Cero).Valor.ToLista<int>();
            if (!idsDeUsuarios.Contains(contexto.DatosDeConexion.IdUsuario))
                GestorDeErrores.Emitir($"El usuario conectado no tiene permisos para realizar esta operación, incluyalo en '{enumParametrosDePreasiento.SPR_Usuarios_Con_Permiso_Para_Generar}'");

            var idTipoArchivadorDePreasiento = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Tipo_De_Archivador, valorPorDefecto: Literal.Cero).Valor.Entero();

            foreach (var id in ids)
            {
                var archivador = contexto.SeleccionarPorId<ArchivadorDtm>(id);
                if (archivador.IdTipo != idTipoArchivadorDePreasiento)
                {
                    GestorDeErrores.Emitir("Solo se pueden descontabilizar archivadores de preasientos");
                }
                archivador.Baja = true;
                var tran = contexto.IniciarTransaccion();
                try
                {
                    archivador.ModificarComoAdministrador(contexto, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarTrazaDeBloqueo, false } });
                    contexto.Commit(tran);
                }
                catch
                {
                    contexto.Rollback(tran);
                }
            }
        }
    }

}
