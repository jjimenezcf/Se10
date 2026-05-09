using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;
using Gestor.Errores;
using System.Collections.Generic;
using ServicioDeDatos.Elemento;
using System.Linq;
using ServicioDeDatos.Seguridad;
using System.IO;
using ServicioDeDatos.Entorno;
using GestoresDeNegocio.Entorno;
using System;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Logistica;
using ModeloDeDto;

namespace GestoresDeNegocio.SistemaDocumental
{

    public class GestorDeArchivos : GestorDeElementos<ContextoSe, ArchivoDtm, ArchivoDto>
    {

        public class MapearArchivos : Profile
        {
            public MapearArchivos()
            {
                CreateMap<ArchivoDtm, ArchivoDto>()
                .ForMember(dto => dto.IdOriginal, dtm => dtm.Ignore())
                .ForMember(dto => dto.Original, dtm => dtm.Ignore());
                CreateMap<ArchivoDto, ArchivoDtm>();
                CreateMap<ArchivoDto, ArchivoMovilOutput>();
            }
        }

        public override enumNegocio Negocio => enumNegocio.Archivos;

        public GestorDeArchivos(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeArchivos Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeArchivos(contexto, mapeador);
        }

        protected override void AntesDePersistir(ArchivoDtm archivo, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(archivo, parametros);

            ApiDeArchivos.ValidarNombre(archivo.Nombre);

            if (parametros.Insertando && archivo.Nombre.StartsWith(Simbolos.ArchivoCancelado))
                GestorDeErrores.Emitir($"El nombre del archivo no puede comenzar por '{Simbolos.ArchivoCancelado}', este símbolo se usa para indicar que un archivo está asociada a un elemento cancelado");

            if (parametros.Modificando)
            {
                var anterior = (ArchivoDtm)parametros.registroEnBd;

                if ((archivo.Nombre.StartsWith(Simbolos.ArchivoCancelado) || anterior.Nombre.StartsWith(Simbolos.ArchivoCancelado)) &&
                    parametros.AccionQueSeEjecuta != ltrDeUnArchivo.Accion_Marcar_Cancelacion && parametros.AccionQueSeEjecuta != ltrDeUnArchivo.Accion_Anular_Cancelacion &&
                    parametros.AccionQueSeEjecuta != ltrDeUnArchivo.Accion_Permitir_Modificar_Nombre)
                {
                    GestorDeErrores.Emitir($"El indicador de cancelación '{Simbolos.ArchivoCancelado}' no se puede añadir ni quitar del nombre del archivo");
                }

                if (parametros.AccionQueSeEjecuta != ltrDeUnArchivo.Accion_Marcar_Cancelacion && parametros.AccionQueSeEjecuta != ltrDeUnArchivo.Accion_Anular_Cancelacion && parametros.AccionQueSeEjecuta != ltrDeUnArchivo.Accion_Permitir_Modificar_Nombre)
                {
                    var bloqueo = Contexto.SeleccionarPorFk<BloqueoDeUnArchivoDtm>(nameof(BloqueoDeUnArchivoDtm.IdArchivo), archivo.Id, errorSiNoHay: false);
                    if (bloqueo != null && bloqueo.Bloqueado)
                        GestorDeErrores.Emitir("No se puede modificar un archivo bloqueado");

                    if (!archivo.SeHaModificadoElCampo<string>(x => x.Name == nameof(ArchivoDtm.Nombre), parametros))
                        GestorDeErrores.Emitir("no ha modificado el nombre del archivo");

                    ServidorDocumental.ValidarNombreDuplicado(Contexto, archivo, excepcionSiNoVinculado: parametros.AccionQueSeEjecuta != enumAccionesSistemaDocumental.RenombrarArchivo
                        && parametros.AccionQueSeEjecuta != ltrDeUnArchivo.Accion_Permitir_Modificar_Nombre);
                }
                var nuevaExtension = ApiDeArchivos.Extension(archivo.Nombre);
                if (nuevaExtension != ApiDeArchivos.Extension(anterior.Nombre))
                {
                    if (!nuevaExtension.IsNullOrEmpty()) GestorDeErrores.Emitir("No se puede modificar la extensión de un archivo");
                    archivo.Nombre = archivo.Nombre + ApiDeArchivos.Extension(anterior.Nombre);
                }

                if (parametros.AccionQueSeEjecuta != ltrDeUnArchivo.Accion_Permitir_Modificar_Nombre &&
                    parametros.AccionQueSeEjecuta != ltrDeUnArchivo.Accion_Marcar_Cancelacion &&
                    parametros.AccionQueSeEjecuta != ltrDeUnArchivo.Accion_Anular_Cancelacion)
                {
                    var facturaRec = Contexto.SeleccionarPorPropiedad<FacturaRecDtm>(nameof(FacturaRecDtm.IdArchivo), archivo.Id, errorSiNoHay: false);
                    if (facturaRec != null)
                        GestorDeErrores.Emitir($"No se puede modificar el nombre del archivo, ya que es el de la factura '{facturaRec.Referencia}'");

                    var pedido = Contexto.SeleccionarPorPropiedad<PedidoDtm>(nameof(PedidoDtm.IdArchivo), archivo.Id, errorSiNoHay: false);
                    if (pedido != null)
                        GestorDeErrores.Emitir($"No se puede modificar el nombre del archivo, ya que es el del pedido '{pedido.Referencia}'");
                }

            }

            if (parametros.Eliminando)
            {
                var bloqueo = Contexto.SeleccionarPorFk<BloqueoDeUnArchivoDtm>(nameof(BloqueoDeUnArchivoDtm.IdArchivo), archivo.Id, errorSiNoHay: false);
                if (bloqueo != null)
                    GestorDeErrores.Emitir("No se puede eliminar un archivo con auditoría de bloqueos");

                Contexto.EliminarTodos<AuditoriaDeUnArchivoDtm>(new Dictionary<string, object> { { nameof(AuditoriaDeUnArchivoDtm.IdArchivo), archivo.Id } });

                if ((bool)parametros.Parametros.LeerValor(ltrParametrosNeg.CopiaSeguridad, false))
                    GestorDeArchivadores.HacerCopiaDeSeguridad(Contexto, archivo);

                var sinc = ArchivoSincronizadoSql.Leer(Contexto, archivo.Id, errorSiNoHay: false);
                if (sinc != null)
                {
                    parametros.Parametros[nameof(ArchivoSincronizadoDtm.Ruta)] = sinc.Ruta;
                    ArchivoSincronizadoSql.Quitar(Contexto, archivo.Id);
                }

            }
        }

        protected override void ValidarPermisosDePersistencia(ArchivoDtm archivo, ParametrosDeNegocio parametros)
        {

            if (parametros.AccionQueSeEjecuta == ltrDeUnArchivo.Accion_Marcar_Cancelacion || parametros.AccionQueSeEjecuta == ltrDeUnArchivo.Accion_Anular_Cancelacion ||
                 parametros.AccionQueSeEjecuta == ltrDeUnArchivo.Accion_Permitir_Modificar_Nombre)
                return;
            if (parametros.Parametros.LeerValor(ltrParametrosNeg.ValidarPermisosDePersistencia, true))
            {
                var lista = enumNegocio.Archivos.VinculosCon(Contexto);
                foreach (var tipoDtm in lista)
                {
                    var vinculos = VinculoSql.LeerVinculosAl(Contexto, tipoDtm, enumNegocio.Archivos, typeof(ArchivoDtm), archivo.Id, filtros: null);
                    var negocioVinculado = NegociosDeSe.NegocioDeUnDtm(tipoDtm);
                    var gestor = NegociosDeSe.CrearGestor(Contexto, negocioVinculado == enumNegocio.Carpeta ? enumNegocio.Archivador : negocioVinculado);
                    foreach (var vinculo in vinculos)
                    {
                        if (negocioVinculado == enumNegocio.Carpeta)
                            gestor.ValidarPermisosDePersistencia(parametros.Operacion, enumNegocio.Archivador, GestorDeCarpetas.LeerRegistroPorId(Contexto, vinculo.idElemento1).IdArchivador);
                        else
                            gestor.ValidarPermisosDePersistencia(parametros.Operacion, negocioVinculado, vinculo.idElemento1);
                    }
                }
            }
        }

        protected override void DespuesDePersistir(ArchivoDtm archivo, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(archivo, parametros);

            if (parametros.Operacion.Equals(enumTipoOperacion.Eliminar))
                DespuesDeEliminarArchivo(archivo, parametros);
            else
            if (parametros.Operacion.Equals(enumTipoOperacion.Modificar))
            {
                var nombreAnterior = ((ArchivoDtm)parametros.registroEnBd).Nombre;
                if (nombreAnterior != archivo.Nombre)
                {
                    archivo.AuditarOperacion(Contexto, ltrDeAuditoriaDeArchivo.Renombrar.Replace("[0]", Contexto.DatosDeConexion.Login).Replace("[1]", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")).Replace("[2]", nombreAnterior));
                }
                if (HayQueSincronizar(archivo.Id))
                    DespuesDeModificarArchivo(archivo, parametros);
            }
            else
            if (parametros.Operacion.Equals(enumTipoOperacion.Insertar))
                DespuesDeCrearArchivo(archivo, parametros);
        }

        protected override void EliminarCaches(ArchivoDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            VinculoSql.BlanquearCacheDeAnexados();
        }

        protected override IQueryable<ArchivoDtm> AplicarFiltros(IQueryable<ArchivoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            if (!(bool)parametros.Parametros.LeerValor(ltrParametrosNeg.IncluirOriginales, false))
            {
                if (!parametros.FiltroPorId)
                    consulta = consulta.Where(x => !Contexto.Set<FirmadoDtm>().Any(y => y.IdOriginal == x.Id));
            }

            return consulta;
        }

        private void DespuesDeEliminarArchivo(ArchivoDtm archivo, ParametrosDeNegocio parametros)
        {
            if (File.Exists($@"{archivo.AlmacenadoEn}\{archivo.Id}.se"))
                ServidorDocumental.EliminarArchivo(Contexto, archivo.Id, $"{archivo.AlmacenadoEn}");

            if (!((string)parametros.Parametros.LeerValor(nameof(ArchivoSincronizadoDtm.Ruta), "")).IsNullOrEmpty())
                ServidorDocumental.EliminarFichero(Contexto, (string)parametros.Parametros[nameof(ArchivoSincronizadoDtm.Ruta)]);
        }

        public FirmadoDto LeerDatosDeFirma(enumNegocio negocio, int idElemento, int idArchivo)
        {
            var gestor = NegociosDeSe.CrearGestor(Contexto, negocio);
            gestor.ValidarPermisosDePersistencia(enumTipoOperacion.Eliminar, negocio, idElemento);

            ApiDeArchivos.ValidarQueEstaAnexado(Contexto, negocio, idElemento, idArchivo);
            var firmadoDtm = Contexto.Set<FirmadoDtm>().Where(x => x.IdFirmado == idArchivo).FirstOrDefault();
            if (firmadoDtm == null)
                GestorDeErrores.Emitir("El archivo solicitado no está firmado");

            var modo = ApiDePermisos.LeerModoDeAcceso(Contexto, negocio, idElemento);
            modo = modo.HayPermisosDe(enumModoDeAccesoDeDatos.Gestor) && firmadoDtm.IdUsuario == Contexto.DatosDeConexion.IdUsuario
            ? enumModoDeAccesoDeDatos.Administrador
            : enumModoDeAccesoDeDatos.Consultor;

            return new FirmadoDto
            {
                Id = firmadoDtm.Id,
                Firmado = Contexto.SeleccionarPorId<ArchivoDtm>(firmadoDtm.IdFirmado).Expresion,
                Original = Contexto.SeleccionarPorId<ArchivoDtm>(firmadoDtm.IdOriginal).Expresion,
                Usuario = Contexto.SeleccionarPorId<UsuarioDtm>(firmadoDtm.IdUsuario).Expresion,
                Certificado = Contexto.SeleccionarPorId<CertificadoDtm>(firmadoDtm.IdCertificado).Expresion,
                FirmadoEl = firmadoDtm.FirmadoEl,
                Motivo = firmadoDtm.Motivo,
                ModoDeAcceso = modo
            };
        }

        private void DespuesDeCrearArchivo(ArchivoDtm archivo, ParametrosDeNegocio parametros)
        {
            if (parametros.Parametros.ContainsKey(ltrDeUnArchivo.Sincronizar) && (bool)parametros.Parametros[ltrDeUnArchivo.Sincronizar])
            {
                var info = ApiDeArchivos.ObtenerInformacionDelFichero(archivo.Id, (string)parametros.Parametros[ltrDeUnArchivo.rutaDelArchivo]);
                ArchivoSincronizadoSql.Crear(Contexto, info);
            }
        }

        private void DespuesDeModificarArchivo(ArchivoDtm archivo, ParametrosDeNegocio parametros)
        {
            var nuevaRuta = SincronizarCon(archivo.Id);
            if (nuevaRuta.IsNullOrEmpty())
                return;

            if (ParametroDeNegocioSql.Parametro(enumNegocio.Archivador, enumParametrosDeArchivadores.ARC_Sincronizacion_Habilitada, emitirError: false, crearParametro: true, valorPorDefecto: false).Valor.EsTrue())
            {
                var sinc = ArchivoSincronizadoSql.Leer(Contexto, archivo.Id, errorSiNoHay: false);
                if (sinc != null) ServidorDocumental.EliminarFichero(Contexto, sinc.Ruta);

                ArchivoSincronizadoSql.Quitar(Contexto, archivo.Id);
                ApiDeArchivos.ExportarArchivo(Contexto, archivo, nuevaRuta);
            }

        }

        private bool HayQueSincronizar(int idArchivo)
        {
            return !SincronizarCon(idArchivo).IsNullOrEmpty();
        }

        private string SincronizarCon(int idArchivo)
        {
            var vinculos = VinculoSql.LeerVinculosAl(Contexto, typeof(ArchivadorDtm), enumNegocio.Archivos, typeof(ArchivoDtm), idArchivo, filtros: null);
            if (vinculos.Count == 1)
            {
                var archivador = GestorDeArchivadores.LeerRegistroPorId(Contexto, vinculos[0].idElemento1);
                return archivador.SincronizarCon;
            }

            vinculos = VinculoSql.LeerVinculosAl(Contexto, typeof(CarpetaDtm), enumNegocio.Archivos, typeof(ArchivoDtm), idArchivo, filtros: null);
            if (vinculos.Count == 1)
            {
                var carpeta = GestorDeCarpetas.LeerRegistroPorId(Contexto, vinculos[0].idElemento1, true);
                return carpeta.Archivador.SincronizarCon == null ? null : GestorDeCarpetas.ObtenerRuta(Contexto, carpeta.Archivador.SincronizarCon, carpeta);
            }

            return null;
        }

        public void QuitarAnexado(enumNegocio negocio, int idElemento, int idArchivo, enumPeticion peticion)
        {
            var elementos = ExtensorDeElementos.ObtenerRegistrosAnexadosAlArchivo(Contexto, negocio, idArchivo, incluirBajas: true, incluirCancelados: true);
            var anexadoMasDeUno = elementos.Count() > 1;
            var sonCarpetas = true;
            if (anexadoMasDeUno)
            {
                foreach (var elemento in elementos)
                {
                    if (elemento.GetType() != typeof(CarpetaDtm))
                    {
                        sonCarpetas = false;
                        break;
                    }
                }
            }

            var gestor = NegociosDeSe.CrearGestor(Contexto, negocio);
            var registro = gestor.LeerRegistroPorId(idElemento, aplicarJoin: false);

            if (negocio == enumNegocio.Archivador && ((ArchivadorDtm)registro).Tipo<TipoDeArchivadorDtm>(Contexto).DelSistema)
            {
                GestorDeErrores.Emitir($"No se puede eliminar el archivos del archivador '{((ArchivadorDtm)registro).Referencia}' por ser del sistema");
            }

            var archivo = Contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);

            var validarPermisos = !negocio.UsaFlujo() || !((IElementoDeProcesoDtm)registro).Estado(Contexto).Terminado || !((IElementoDeProcesoDtm)registro).PermitirDesvincularArchivoAlEstarTerminado(Contexto);
            if (validarPermisos)
                ServidorDocumental.ValidarPermisosDeOperacion(Contexto, archivo, negocio, (RegistroConNombreDtm)registro, operacion: "eliminar");

            if (anexadoMasDeUno && sonCarpetas)
            {
                VinculoSql.QuitarVinculo(Contexto, typeof(CarpetaDtm), enumNegocio.Archivos, ((IRegistro)registro).Id, archivo.Id);
                archivo.AuditarOperacion(Contexto, ltrDeAuditoriaDeArchivo.Desenlazar.Replace("[0]", Contexto.DatosDeConexion.Login).Replace("[1]", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")).Replace("[2]", $"Carpeta: {((INombre)registro).Nombre}"));
                return;
            }

            if (!validarPermisos && negocio.UsaFlujo() && negocio.Estado(Contexto, ((IElementoDeProcesoDtm)registro).IdEstado).Terminado)
            {
                HacerCopiaDeSeguridad(Contexto, negocio, (IElementoDeProcesoDtm)registro, archivo);
                GestorDeVinculos.BorrarVinculo(Contexto, negocio, enumNegocio.Archivos, ((IRegistro)registro).Id, archivo.Id, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
                return;
            }
            var auditoria = archivo.Auditoria(Contexto);
            if (!auditoria.IsNullOrEmpty() && peticion == enumPeticion.epQuitarAnexado && (
                auditoria.Contains(ltrDeAuditoriaDeArchivo.enuAccion.Bloquear.ToString(), StringComparison.CurrentCultureIgnoreCase) ||
                auditoria.Contains(ltrDeAuditoriaDeArchivo.enuAccion.Bloqueado.ToString(), StringComparison.CurrentCultureIgnoreCase)))
            {
                GestorDeErrores.Emitir($"No se puede eliminar el archivo por tener auditoría de bloqueo");
            }

            ApiDeArchivos.QuitarAnexado((IRegistro)registro, Contexto, idArchivo, validarPermisos, QuitarDeRestoDeAnexados: false);
        }

        public void FirmarAnexado(enumNegocio negocio, int idElemento, int idArchivo, int idCertificado, string password, Dictionary<string, object> parametros)
        {
            var gestor = NegociosDeSe.CrearGestor(Contexto, negocio == enumNegocio.Carpeta ? enumNegocio.Archivador : negocio);

            var archivo = Contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
            var registro = gestor.LeerRegistroPorId(idElemento, aplicarJoin: false);
            ServidorDocumental.ValidarPermisosDeOperacion(Contexto, archivo, negocio, (RegistroConNombreDtm)registro, operacion: "firmar");

            if (negocio == enumNegocio.Carpeta)
                gestor.ValidarPermisosDePersistencia(enumTipoOperacion.Modificar, enumNegocio.Archivador, GestorDeCarpetas.LeerRegistroPorId(Contexto, idElemento).IdArchivador);
            else
                gestor.ValidarPermisosDePersistencia(enumTipoOperacion.Modificar, negocio, idElemento, parametros);

            ApiDeArchivos.ValidarQueEstaAnexado(Contexto, negocio, idElemento, idArchivo);
            ApiDeArchivos.ValidarQueNoEstaFirmado(Contexto, idArchivo);

            var fichero = ServidorDocumental.DescargarArchivo(Contexto, idArchivo, false, true);

            var ficheroFirmado = ApiDeCertificados.Firmar(Contexto, fichero, idCertificado, password, visible: true);

            new FirmadoDtm
            {
                IdOriginal = idArchivo,
                IdFirmado = ServidorDocumental.AnexarArchivo(Contexto, negocio, idElemento, ficheroFirmado, sanitizar: false).Id,
                FirmadoEl = DateTime.Now,
                IdCertificado = idCertificado,
                Motivo = "firmado desde la interface",
                IdUsuario = Contexto.DatosDeConexion.IdUsuario,
            }.Insertar(Contexto);

            var traza = negocio == enumNegocio.Carpeta ? enumNegocio.Archivador.NuevaTrazaDtm<TrazaDtm>() : negocio.NuevaTrazaDtm<TrazaDtm>();
            traza.IdElemento = negocio == enumNegocio.Carpeta ? GestorDeCarpetas.LeerRegistroPorId(Contexto, idElemento).IdArchivador : idElemento;
            traza.Nombre = $"Archivo Firmado";
            traza.Descripcion = $"El usuario {Contexto.SeleccionarPorId<UsuarioDtm>(Contexto.DatosDeConexion.IdUsuario).Expresion} " +
                $"ha firmado el archivo '{Contexto.Set<ArchivoDtm>().Where(x => x.Id == idArchivo).First().Nombre}' " +
                $"con el certificado '{Contexto.SeleccionarPorId<CertificadoDtm>(idCertificado).Expresion}'";
            traza.Insertar(Contexto);
        }

        public void AnularFirma(enumNegocio negocio, int idElemento, int idArchivo)
        {
            var gestor = NegociosDeSe.CrearGestor(Contexto, negocio == enumNegocio.Carpeta ? enumNegocio.Archivador : negocio);
            if (negocio == enumNegocio.Carpeta)
                gestor.ValidarPermisosDePersistencia(enumTipoOperacion.Modificar, enumNegocio.Archivador, GestorDeCarpetas.LeerRegistroPorId(Contexto, idElemento).IdArchivador);
            else
                gestor.ValidarPermisosDePersistencia(enumTipoOperacion.Modificar, negocio, idElemento);

            ApiDeArchivos.ValidarQueEstaAnexado(Contexto, negocio, idElemento, idArchivo);
            var firmado = ApiDeArchivos.ValidarQueEstaFirmadoPorElUsuarioConectado(Contexto, idArchivo);

            var traza = negocio == enumNegocio.Carpeta ? enumNegocio.Archivador.NuevaTrazaDtm<TrazaDtm>() : negocio.NuevaTrazaDtm<TrazaDtm>();
            traza.IdElemento = negocio == enumNegocio.Carpeta ? GestorDeCarpetas.LeerRegistroPorId(Contexto, idElemento).IdArchivador : idElemento;
            traza.Nombre = $"Se ha suprimido un archivo firmado";
            traza.Descripcion = $"El usuario {Contexto.SeleccionarPorId<UsuarioDtm>(Contexto.DatosDeConexion.IdUsuario).Expresion} " +
                $"ha suprimido el archivo '{Contexto.Set<ArchivoDtm>().Where(x => x.Id == idArchivo).First().Nombre}'";
            traza.Insertar(Contexto);

            Contexto.EliminarPorId<FirmadoDtm>(firmado.Id);

            QuitarAnexado(negocio, idElemento, idArchivo, enumPeticion.epAnularFirma);
        }


        public List<CertificadosDisponiblesDto> LeerCertificadosDisponibles(enumNegocio negocio, int idElemento, int idArchivo)
        {
            var gestor = NegociosDeSe.CrearGestor(Contexto, negocio);
            gestor.ValidarPermisosDePersistencia(enumTipoOperacion.Modificar, negocio, idElemento);

            ApiDeArchivos.ValidarQueEstaAnexado(Contexto, negocio, idElemento, idArchivo);
            //ApiDeArchivos.ValidarQueNoEstaFirmado(Contexto, idArchivo);

            var certificados = GestorDeCertificados.Gestor(Contexto, Contexto.Mapeador).LeerRegistros(0, -1, new List<ClausulaDeFiltrado>());
            var disponibles = new List<CertificadosDisponiblesDto>();

            foreach (var certificado in certificados)
                disponibles.Add(new CertificadosDisponiblesDto
                {
                    Certificado = certificado.Nombre,
                    Id = certificado.Id
                });

            return disponibles;

        }

        public List<ArchivoDtm> LeerAnexadosDtm(enumNegocio negocio, int idRegistro, Dictionary<string, object> parametros)
        {
            List<ArchivoDtm> archivosDtm;
            archivosDtm = LeerAnexadosAlRegistro(negocio, idRegistro, parametros);

            if (negocio.UsaArchivadores())
            {
                var archivadores = GestorDeVinculos.RegistrosVinculados<ArchivadorDtm>(Contexto, negocio, enumNegocio.Archivador, idRegistro);
                foreach (var archivador in archivadores)
                {
                    var archivos = LeerAnexadosDtm(enumNegocio.Archivador, archivador.Id, parametros);
                    foreach (var archivo in archivos)
                    {
                        var existente = archivosDtm.FirstOrDefault(x => x.Id == archivo.Id);
                        if (existente is not null)
                        {
                            existente.AnexadoAUnArchivador = true;
                            continue;
                        }
                        var nuevo = new ArchivoDtm();
                        archivo.CopiarEn(nuevo);
                        nuevo.Nombre = $"{archivador.Referencia}: {archivo.Nombre}";
                        nuevo.AnexadoAUnArchivador = true;
                        archivosDtm.Add(nuevo);
                    }
                }
            }

            return archivosDtm;
        }

        public List<ArchivoDtm> LeerAnexadosAlRegistro(enumNegocio negocio, int idRegistro, Dictionary<string, object> parametros)
        {
            ValidarPermisosSobreElAnexado(negocio, idRegistro);
            var cache = ServicioDeCaches.Obtener(CacheDe.Arc_AnexadosDtm);
            var indice = $"{negocio.TipoDtm()}-{idRegistro}";
            if (!cache.ContainsKey(indice))
            {
                cache[indice] = negocio.Archivos(Contexto, idRegistro, parametros.LeerValor(ltrParametrosNeg.IncluirOriginales, false));
            }

            return (List<ArchivoDtm>)cache[indice];
        }

        public List<ArchivoDto> LeerAnexados(enumNegocio negocio, int idElemento, int posicion, int cantidad, string guid, Dictionary<string, object> parametros)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Arc_AnexadosDto_PorGuid);
            var indice = $"{guid}-{idElemento}";
            if (posicion == 0 || !cache.Keys.Contains(indice))
            {
               cache[indice] = new List<ArchivoDto>();
            }
            var cacheados = 0;

            var archivosDto = new List<ArchivoDto>();
            var elemento = negocio.LeerElemento(Contexto, idElemento);
            var registro = negocio.LeerRegistro(Contexto, idElemento);
            List<ArchivoDtm> archivosDtm = new List<ArchivoDtm>();

            List<ExtensionDeArchivo> archivosExt = enumNegocio.Carpeta == negocio ?
            ExtensorDeArchivadores.ArchivosExt((CarpetaDtm)registro, Contexto, incluirOriginal: false) :
            ((IElementoDtm)registro).ArchivosExt(Contexto, recursivo: true, incluirOriginal: false);

            var permisosDe = ValidarPermisosSobreElAnexado(negocio, idElemento);

            if (!cache.Keys.Contains(indice))
            {
                cache[indice] = new List<ArchivoDto>();
            }

            var idsArchivosDtoCacheados = ((List<ArchivoDto>)cache[indice]).Select(a => a.Id).ToList();

            var padreBloqueado = negocio == enumNegocio.Carpeta
            ? Contexto.SeleccionarPorId<ArchivadorDtm>(((CarpetaDto)elemento).IdArchivador).Bloqueado
            : elemento.GetType().ImplementaUsaBloqueoDto() && ((IUsaBloqueoDto)elemento).Bloqueado;

            var archivosPorExpresion = archivosExt.Ordenar(Contexto, negocio);
            foreach (var archivoExt in archivosPorExpresion)
            {
                if (cacheados >= cantidad)
                    break;
                if (idsArchivosDtoCacheados.Contains(archivoExt.Archivo.Id))
                    continue;
                var archivodto = archivoExt.Archivo.MapearDto<ArchivoDto, ArchivoDtm>(Contexto,
                    parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo,
                    parametros: new Dictionary<string, object> {
                                 {ltrParametrosNeg.Peticion, enumPeticion.epLeerAnexados },
                                 {ltrParametrosNeg.ModoAccesoLeido, permisosDe},
                                 {ltrParametrosNeg.Negocio,negocio }
                    },
                    aplicarJoin: false));

                archivodto.EsDeUnArchivadorVinculado = negocio == enumNegocio.Carpeta || (archivoExt.Negocio != enumNegocio.Archivador && archivoExt.IdArchivador is null) ?
                false :
                negocio != enumNegocio.Archivador && archivoExt.Negocio == enumNegocio.Archivador ?
                true :
                negocio == enumNegocio.Archivador && archivoExt.IdCarpeta == null ?
                false :
                true;

                archivodto.DelSistema = elemento.DelSistema;
                archivodto.PadreBloqueado = padreBloqueado || archivodto.EsDeUnArchivadorVinculado;
                archivodto.EstaCancelada = elemento.EstaCancelada || archivoExt.Archivo.Nombre.StartsWith(Simbolos.ArchivoCancelado);
                archivodto.ModoDeAcceso = archivoExt.Archivo.AnexadoAUnArchivador || archivodto.EstaCancelada ? enumModoDeAccesoDeDatos.Consultor : permisosDe;

                ((List<ArchivoDto>)cache[indice]).Add(archivodto);
                archivodto.Nombre = archivoExt.Expresion;
                archivosDto.Add(archivodto);
                cacheados++;
            }

            return archivosDto;
        }

        public List<ArchivoDto> LeerAnexados(enumNegocio negocio, int idElemento, Dictionary<string, object> parametros)
        {
            var posicion = parametros.LeerValor(ltrDeUnArchivo.Posicion, -1);
            var permisosDe = ValidarPermisosSobreElAnexado(negocio, idElemento);
            var archivosDto = new List<ArchivoDto>();

            var cache = ServicioDeCaches.Obtener(CacheDe.Arc_AnexadosDto);
            var indice = $"{negocio.TipoDtm()}-{idElemento}";
            if (!cache.ContainsKey(indice))
            {
                var archivosDtm = negocio.Archivos(Contexto, idElemento, parametros.LeerValor(ltrParametrosNeg.IncluirOriginales, false));
                foreach (var archivo in archivosDtm)
                {
                    archivosDto.Add(archivo.MapearDto<ArchivoDto, ArchivoDtm>(Contexto,
                        parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo,
                        parametros: new Dictionary<string, object> {
                                 {ltrParametrosNeg.Peticion, enumPeticion.epLeerAnexados },
                                 {ltrParametrosNeg.ModoAccesoLeido, permisosDe},
                                 {ltrParametrosNeg.Negocio,negocio }
                        },
                        aplicarJoin: false)));
                }
                cache[indice] = archivosDto;
            }
            else
            {
                archivosDto = (List<ArchivoDto>)cache[indice];
            }

            if (negocio.UsaArchivadores())
            {
                var archivadores = GestorDeVinculos.RegistrosVinculados<ArchivadorDtm>(Contexto, negocio, enumNegocio.Archivador, idElemento);
                foreach (var archivador in archivadores)
                {
                    var archivos = LeerAnexadosDeUnArchivador(enumNegocio.Archivador, archivador.Id, parametros);
                    foreach (var archivo in archivos)
                    {
                        var existente = archivosDto.FirstOrDefault(x => x.Id == archivo.Id);
                        if (existente is not null)
                        {
                            existente.EsDeUnArchivadorVinculado = true;
                            continue;
                        }
                        var nuevo = new ArchivoDto();
                        archivo.CopiarEn(nuevo);
                        nuevo.Nombre = $"{archivador.Referencia}: {archivo.Nombre}";
                        nuevo.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                        nuevo.EsDeUnArchivadorVinculado = true;
                        archivosDto.Add(nuevo);
                    }
                }
            }

            return archivosDto;
        }

        public List<ArchivoDto> LeerAnexadosDeUnArchivador(enumNegocio negocio, int idElemento, Dictionary<string, object> parametros)
        {
            var permisosDe = ValidarPermisosSobreElAnexado(negocio, idElemento);
            var cacheVcl = ServicioDeCaches.Obtener(CacheDe.Arc_AnexadosDeUnArchivadorVinculado);
            var cacheDto = ServicioDeCaches.Obtener(CacheDe.Arc_AnexadosDto);
            var ind = $"{NegociosDeSe.TipoDtm(negocio)}-{idElemento}";
            var archivosDto = new List<ArchivoDto>();
            if (!cacheVcl.ContainsKey(ind))
            {
                var archivosDtm = negocio.Archivos(Contexto, idElemento, parametros.LeerValor(ltrParametrosNeg.IncluirOriginales, false));
                foreach (var archivo in archivosDtm)
                {
                    archivosDto.Add(archivo.MapearDto<ArchivoDto, ArchivoDtm>(Contexto,
                        parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo,
                        parametros: new Dictionary<string, object> {
                                 {ltrParametrosNeg.Peticion, enumPeticion.epLeerAnexados },
                                 {ltrParametrosNeg.ModoAccesoLeido, permisosDe},
                                 {ltrParametrosNeg.Negocio,negocio }
                        },
                        aplicarJoin: false)));
                }
                cacheVcl[ind] = archivosDto;
                cacheDto[ind] = archivosDto;
            }
            archivosDto = (List<ArchivoDto>)cacheVcl[ind];
            return archivosDto;
        }


        public enumModoDeAccesoDeDatos ValidarPermisosSobreElAnexado(enumNegocio negocio, int idElemento)
        {
            enumModoDeAccesoDeDatos permisosDe;
            ElementoDtm elemento;
            if (negocio == enumNegocio.Carpeta)
            {
                var carpeta = Contexto.SeleccionarPorId<CarpetaDtm>(idElemento);
                elemento = Contexto.SeleccionarPorId<ArchivadorDtm>(carpeta.IdArchivador);
            }
            else
            {
                elemento = (ElementoDtm)negocio.SeleccionarPorId(Contexto, idElemento);
            }
            if (!elemento.EsConsultor(Contexto))
                GestorDeErrores.Emitir($"No tiene acceso a los anexos del elemento '{elemento.Referencia(Contexto)}'");

            permisosDe = elemento.PermisosDe(Contexto);
            return permisosDe;
        }

        protected override void DespuesDeMapearElElemento(ArchivoDtm archivo, ArchivoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(archivo, elemento, parametros);

            var firmado = Contexto.SeleccionarPorAk<FirmadoDtm>(new Dictionary<string, object> { { nameof(FirmadoDtm.IdFirmado), elemento.Id } }, errorSiNoHay: false);
            if (firmado != null)
            {
                var original = Contexto.Set<ArchivoDtm>().Where(y => y.Id == firmado.IdOriginal).First();
                elemento.IdOriginal = original.Id;
                elemento.Original = original.Nombre;
            }

            if (parametros.Peticion == enumPeticion.epLeerAnexados || parametros.Peticion == enumPeticion.epLeerPorId)
            {
                var datosDeAsociados = parametros.Peticion == enumPeticion.epLeerPorId
                ? LeerDatosDeLosAsociados(archivo)
                : (modoDeAcceso: parametros.Parametros.LeerValor<enumModoDeAccesoDeDatos>(ltrParametrosNeg.ModoAccesoLeido), negocio: (enumNegocio)parametros.Parametros[ltrParametrosNeg.Negocio]);

                var bloqueado = Contexto.SeleccionarPorFk<BloqueoDeUnArchivoDtm>(nameof(BloqueoDeUnArchivoDtm.IdArchivo), archivo.Id, errorSiNoHay: false);
                if (bloqueado != null)
                {
                    elemento.EstaBloqueado = bloqueado.Bloqueado;
                    elemento.ModoDeAcceso = elemento.EstaBloqueado ? enumModoDeAccesoDeDatos.Consultor : datosDeAsociados.modoDeAcceso;
                }
                else
                    elemento.ModoDeAcceso = datosDeAsociados.modoDeAcceso;

                if (parametros.Peticion == enumPeticion.epLeerPorId)
                {
                    var cantidad = ExtensorDeElementos.ContarAnexadosAlArchivo(Contexto, datosDeAsociados.negocio, archivo.Id);
                    elemento.EstaEnlazado = cantidad > 1;
                    if (elemento.EstaEnlazado)
                    {
                        var elementosAsociados = ExtensorDeElementos.ObtenerRegistrosAnexadosAlArchivo(Contexto, datosDeAsociados.negocio, archivo.Id, incluirBajas: true, incluirCancelados: true);
                        elemento.EnlazadoA = string.Join(",", elementosAsociados.OfType<ElementoDtm>().Select(x => x.Referencia()));
                    }
                    else
                    {
                        elemento.EnlazadoA = "Sólo asociado al elemento editado";
                    }

                    elemento.Auditoria = archivo.Auditoria(Contexto);
                }

            }

        }

        private (enumModoDeAccesoDeDatos modoDeAcceso, enumNegocio negocio) LeerDatosDeLosAsociados(ArchivoDtm archivo)
        {
            var tiposVinculados = enumNegocio.Archivos.VinculosCon(Contexto);
            var modo = archivo.IdUsuaCrea == Contexto.DatosDeConexion.IdUsuario
            ? enumModoDeAccesoDeDatos.Administrador
            : enumModoDeAccesoDeDatos.Gestor;

            var negocioVinculado = enumNegocio.No_Definido;
            foreach (var tipoDtm in tiposVinculados)
            {
                var vinculos = VinculoSql.LeerVinculosAl(Contexto, tipoDtm, enumNegocio.Archivos, typeof(ArchivoDtm), archivo.Id, filtros: null);
                if (vinculos.Count == 0)
                    continue;
                negocioVinculado = NegociosDeSe.NegocioDeUnDtm(tipoDtm);
                var gestor = NegociosDeSe.CrearGestor(Contexto, negocioVinculado == enumNegocio.Carpeta ? enumNegocio.Archivador : negocioVinculado);
                foreach (var vinculo in vinculos)
                {
                    modo = negocioVinculado == enumNegocio.Carpeta
                    ? ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Archivador, GestorDeCarpetas.LeerRegistroPorId(Contexto, vinculo.idElemento1).IdArchivador)
                    : ApiDePermisos.LeerModoDeAcceso(Contexto, negocioVinculado, vinculo.idElemento1);

                    if (modo == enumModoDeAccesoDeDatos.Consultor)
                        return (modoDeAcceso: modo, negocio: negocioVinculado);
                }
            }
            return (modoDeAcceso: modo, negocio: negocioVinculado);
        }

        public List<CualquierVinculo> CualquierVinculadoAl(ArchivoDtm archivo, bool detalleDelVinculado, bool excluirCancelados)
        {
            var vinculadosAl = new List<CualquierVinculo>();
            var tiposVinculados = enumNegocio.Archivos.VinculosCon(Contexto);
            foreach (var tipoDtm in tiposVinculados)
            {
                var vinculos = VinculoSql.LeerVinculosAl(Contexto, tipoDtm, enumNegocio.Archivos, typeof(ArchivoDtm), archivo.Id, filtros: null);
                if (vinculos.Count == 0)
                    continue;
                var negocioVinculado = NegociosDeSe.NegocioDeUnDtm(tipoDtm);
                foreach (var vinculo in vinculos)
                {
                    var elemento = detalleDelVinculado || excluirCancelados ? (INombre)negocioVinculado.LeerRegistro(Contexto, vinculo.idElemento1) : null;
                    if (elemento != null)
                    {
                        if (excluirCancelados && negocioVinculado.UsaBaja() && ((IUsaBaja)elemento).Baja) continue;
                        if (excluirCancelados && negocioVinculado.UsaFlujo() && ((IElementoDeProcesoDtm)elemento).Estado(Contexto).Cancelado) continue;
                    }

                    vinculadosAl.Add(new CualquierVinculo
                    {
                        Id = vinculo.Id,
                        Negocio1 = negocioVinculado,
                        idElemento1 = vinculo.idElemento1,
                        elemento1 = detalleDelVinculado ? elemento : null,
                        Negocio2 = enumNegocio.Archivos,
                        idElemento2 = archivo.Id,
                        elemento2 = detalleDelVinculado ? archivo : null,
                    });
                }
            }
            return vinculadosAl;
        }

        public bool ExisteAlgunVinculoAl(ArchivoDtm archivo)
        {
            var vinculadosAl = new List<CualquierVinculo>();
            var tiposVinculados = enumNegocio.Archivos.VinculosCon(Contexto);
            foreach (var tipoDtm in tiposVinculados)
            {
                var cantidad = ExtensorDeElementos.ContarAnexadosAlArchivo(Contexto, NegociosDeSe.NegocioDeUnDtm(tipoDtm), archivo.Id);
                if (cantidad > 0)
                    return true;
            }
            return false;
        }

        public static void AntesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var contexto = entorno.Contexto;
            if (entorno.Entrada.LeerValor(nameof(ltrDeUnArchivo.QuitarTodosLosAnexados), false))
            {
                var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
                var idArchivo = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
                var idVinculado = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado));

                var lista = enumNegocio.Archivos.VinculosCon(contexto);
                foreach (var tipoDtm in lista)
                {
                    if (tipoDtm.NegocioDeUnDtm() == vinculado)
                        VinculoSql.QuitarVinculosAlExcepto(contexto, tipoDtm, enumNegocio.Archivos, idArchivo, idVinculado);
                    else
                        VinculoSql.QuitarVinculosAl(contexto, tipoDtm, enumNegocio.Archivos, idArchivo);
                }
            }
        }

        public static void DespuesDeVincular(EntornoDeUnaAccion entorno)
        {
            BlanquearAnexadosDeUnArchivador(entorno);
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            if (vinculado != enumNegocio.Archivador && vinculado != enumNegocio.Carpeta)
            {
                var idVinculado = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado));
                if (vinculado.UsaArchivadores())
                {
                    var archivadores = vinculado.Archivadores(entorno.Contexto).Where(x => x.idElemento1 == idVinculado).ToList();
                    foreach (var archivador in archivadores)
                    {
                        VinculoSql.BlanquearCacheDeAnexados(entorno.Contexto, enumNegocio.Archivador.TipoDtm(), archivador.idElemento2);
                    }
                }
            }
        }

        public static void DespuesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var contexto = entorno.Contexto;
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var idVinculado = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado));

            VinculoSql.BlanquearCacheDeAnexados(entorno.Contexto, vinculado.TipoDtm(), idVinculado);
        }

        private static void HacerCopiaDeSeguridad(ContextoSe contexto, enumNegocio vinculado, IElementoDeProcesoDtm registro, ArchivoDtm archivo)
        {
            var idTipoArchivador = GestorDeTiposDeArchivadores.Cfg_Id_Tipo_De_Archivador_De_BackUp(contexto);
            var nombreArchivador = $"{vinculado.Singular()}: {registro.Referencia}";
            var usuarioDeConexion = contexto.SeleccionarPorId<UsuarioDtm>(contexto.DatosDeConexion.IdUsuario);
            var otorgado = usuarioDeConexion.OtorgarAdministrador(contexto);
            try
            {
                var archivadores = contexto.SeleccionarTodos<ArchivadorDtm>(new Dictionary<string, object> { { nameof(ArchivadorDtm.IdTipo), idTipoArchivador }, { nameof(ArchivadorDtm.Nombre), nombreArchivador } });
                var archivador = archivadores.Count > 0 ? archivadores[0] : new ArchivadorDtm
                {
                    IdCg = registro.IdCg,
                    IdTipo = idTipoArchivador,
                    Nombre = nombreArchivador
                }.InsertarComoAdministrador(contexto);

                GestorDeVinculos.Vincular(contexto, enumNegocio.Archivador, enumNegocio.Archivos, archivador.Id, archivo.Id, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
                ((IUsaTraza)registro).CrearTraza(contexto, "Copia de seguridad del archivo", $"Se ha eliminado el archivo '{archivo.Nombre}' y se ha hecho una copia de seguridad en el archivador '{nombreArchivador}'");
            }
            finally
            {
                usuarioDeConexion.AnularAdministrador(contexto, otorgado);
            }
        }

        private static void BlanquearAnexadosDeUnArchivador(EntornoDeUnaAccion entorno)
        {
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var idVinculado = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado));
            VinculoSql.BlanquearCacheDeAnexados(entorno.Contexto, vinculado.TipoDtm(), idVinculado);
        }

        public string RegistrarDescargaConGuid(int id, DateTime? caducaEl, int? maximoDeDescargas, bool auditar = true)
        {
            var archivo = Contexto.SeleccionarPorId<ArchivoDtm>(id);
            return archivo.RegistrarDescargaConGuid(Contexto, caducaEl, maximoDeDescargas, auditar);
        }

        public void ValidarDescargaConGuid(string guid, int idArchivo)
        {
            Guid guidParsed = Guid.Parse(guid);
            var descarga = Contexto.Set<DescargaConGuidDtm>().FirstOrDefault(x => x.Guid == guidParsed);
            if (descarga.IdArchivo != idArchivo)
            {
                GestorDeErrores.Emitir($"El id de archivo no corresponde con el guid proporcionado");
            }
            if (descarga.CaducaEl == null && descarga.DescargadoEl is not null && descarga.MaximoDescargas is null)
            {
                GestorDeErrores.Emitir($"Este archivo no se puede descargar ya que se descargo el '{descarga.DescargadoEl.Fecha().ToString("yyy-MM-dd")}', solicite un nuevo enlace");
            }
            if (descarga.CaducaEl != null && (DateTime)descarga.CaducaEl < DateTime.Now)
            {
                GestorDeErrores.Emitir($"Este archivo no se puede descargar ya que caducó el '{descarga.CaducaEl.Fecha().ToString("yyy-MM-dd HH:mm")}', solicite un nuevo enlace");
            }
            if (descarga.MaximoDescargas != null && (int)descarga.Descargas > (int)descarga.MaximoDescargas)
            {
                GestorDeErrores.Emitir($"Este archivo no se puede descargar ya que se ha descargado '{descarga.Descargas}' {(descarga.Descargas == 1 ? "vez" : "veces")}, solicite un nuevo enlace");
            }
            descarga.DescargadoEl = DateTime.Now;
            descarga.Descargas = descarga.Descargas.Entero() + 1;
            descarga.Modificar(Contexto);
            new AuditoriaDeUnArchivoDtm
            {
                IdArchivo = idArchivo,
                Auditoria = ltrDeAuditoriaDeArchivo.DescargaRealizada.Replace("[0]", guid.ToString())
            }
            .Insertar(Contexto);
        }
    }
}
