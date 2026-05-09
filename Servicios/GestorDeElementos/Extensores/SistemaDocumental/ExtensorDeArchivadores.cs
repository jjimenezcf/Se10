using Dapper;
using Gestor.Errores;
using Microsoft.Build.Experimental.ProjectCache;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilidades;

namespace GestorDeElementos.Extensores
{

    public class ltrDeUnArchivador
    {
        public const string MostraArchivadoresRelacionados = nameof(MostraArchivadoresRelacionados);
        public const string IdArchivador = nameof(IdArchivador);
        public const string IdCarpeta = nameof(IdCarpeta);
        public const string Accion_GenerarZip = nameof(Accion_GenerarZip);
        public const string Accion_GenerarSii = nameof(Accion_GenerarSii);
    }
    public static class ExtensorDeArchivadores
    {
        public static IQueryable<VinculoDtm> Archivadores(this enumNegocio negocio, ContextoSe contexto)
        {
            switch (negocio)
            {
                case enumNegocio.Sociedad:
                    return contexto.Set<ArchivadoresDeUnaSociedadDtm>();
                case enumNegocio.Persona:
                    return contexto.Set<ArchivadoresDeUnaPersonaDtm>();
                case enumNegocio.Registro:
                    return contexto.Set<ArchivadoresDeUnRegistroEsDtm>();
                case enumNegocio.Tarea:
                    return contexto.Set<ArchivadoresDeUnaTareaDtm>();
                case enumNegocio.Expediente:
                    return contexto.Set<ArchivadoresDeUnExpedienteDtm>();
                case enumNegocio.Presupuesto:
                    return contexto.Set<ArchivadoresDeUnPresupuestoDtm>();
                case enumNegocio.Pleito:
                    return contexto.Set<ArchivadoresDeUnPleitoDtm>();
                case enumNegocio.Contrato:
                    return contexto.Set<ArchivadoresDeUnContratoDtm>();
                case enumNegocio.FacturaEmitida:
                    return contexto.Set<ArchivadoresDeUnaFacturaEmtDtm>();
                case enumNegocio.FacturaRecibida:
                    return contexto.Set<ArchivadoresDeUnaFacturaRecDtm>();
                case enumNegocio.Pedido:
                    return contexto.Set<ArchivadoresDeUnPedidoDtm>();
                case enumNegocio.Preasiento:
                    return contexto.Set<ArchivadoresDeUnPreasientoDtm>();
                case enumNegocio.Pago:
                    return contexto.Set<ArchivadoresDeUnPagoDtm>();
                case enumNegocio.RemesaFae:
                    return contexto.Set<ArchivadoresDeUnaRemesaFaeDtm>();
                case enumNegocio.RemesaPag:
                    return contexto.Set<ArchivadoresDeUnaRemesaPagDtm>();
                case enumNegocio.CircuitoDoc:
                    return contexto.Set<ArchivadoresDeUnCircuitoDocDtm>();
                case enumNegocio.ParteDeTrabajo:
                    return contexto.Set<ArchivadoresDeUnParteTrDtm>();
                case enumNegocio.Cliente:
                    return contexto.Set<ArchivadoresDeUnClienteDtm>();
                case enumNegocio.Infante:
                    return contexto.Set<ArchivadoresDeUnInfanteDtm>();
                case enumNegocio.CursoDeGuarderia:
                    return contexto.Set<ArchivadoresDeUnCursoDeGuarderiaDtm>();
            }

            throw new Exception($"Se debe indicar como obtener los archivos vinculados al negocio: {negocio}");
        }

        public static IQueryable<VinculoDtm> Archivos(this enumNegocio negocio, ContextoSe contexto)
        {
            switch (negocio)
            {
                case enumNegocio.Expediente:
                    return contexto.Set<ArchivosDeUnExpedienteDtm>();
                case enumNegocio.FacturaRecibida:
                    return contexto.Set<ArchivosDeUnaFacturaRecDtm>();
                case enumNegocio.Pedido:
                    return contexto.Set<ArchivosDeUnPedidoDtm>();
                case enumNegocio.Pago:
                    return contexto.Set<ArchivosDeUnPagoDtm>();
                case enumNegocio.RemesaPag:
                    return contexto.Set<ArchivosDeUnaRemesaPagDtm>();
                case enumNegocio.Contrato:
                    return contexto.Set<ArchivosDeUnContratoDtm>();
                case enumNegocio.Pleito:
                    return contexto.Set<ArchivosDeUnPleitoDtm>();
                case enumNegocio.Presupuesto:
                    return contexto.Set<ArchivosDeUnPresupuestoDtm>();
                case enumNegocio.Registro:
                    return contexto.Set<ArchivosDeUnRegistroEsDtm>();
                case enumNegocio.Archivador:
                    return contexto.Set<ArchivosDeUnArchivadorDtm>();
                case enumNegocio.Carpeta:
                    return contexto.Set<ArchivosDeUnaCarpetaDtm>();
                case enumNegocio.CircuitoDoc:
                    return contexto.Set<ArchivosDeUnCircuitoDocDtm>();
                case enumNegocio.Tarea:
                    return contexto.Set<ArchivosDeUnaTareaDtm>();
                case enumNegocio.Abogado:
                    return contexto.Set<ArchivosDeUnAbogadoDtm>();
                case enumNegocio.Cliente:
                    return contexto.Set<ArchivosDeUnClienteDtm>();
                case enumNegocio.Interlocutor:
                    return contexto.Set<ArchivosDeUnInterlocutorDtm>();
                case enumNegocio.Persona:
                    return contexto.Set<ArchivosDeUnaPersonaDtm>();
                case enumNegocio.Procurador:
                    return contexto.Set<ArchivosDeUnProcuradorDtm>();
                case enumNegocio.Proveedor:
                    return contexto.Set<ArchivosDeUnProveedorDtm>();
                case enumNegocio.Sociedad:
                    return contexto.Set<ArchivosDeUnaSociedadDtm>();
                case enumNegocio.Trabajador:
                    return contexto.Set<ArchivosDeUnTrabajadorDtm>();
                case enumNegocio.FacturaEmitida:
                    return contexto.Set<ArchivosDeUnaFacturaEmtDtm>();
                case enumNegocio.ParteDeTrabajo:
                    return contexto.Set<ArchivosDeUnParteTrDtm>();
                case enumNegocio.PlanificacionDeVenta:
                    return contexto.Set<ArchivosDeUnaPlanificacionDeVentaDtm>();
                case enumNegocio.RemesaFae:
                    return contexto.Set<ArchivosDeUnaRemesaFaeDtm>();
                case enumNegocio.Infante:
                    return contexto.Set<ArchivosDeUnInfanteDtm>();
                case enumNegocio.CursoDeGuarderia:
                    return contexto.Set<ArchivosDeUnCursoDeGuarderiaDtm>();
            }

            throw new Exception($"Se debe indicar como obtener los archivos anexados al negocio: {negocio}");
        }

        public static IQueryable<T> Archivos<T>(this IQueryable<T> consulta, ContextoSe contexto, enumNegocio negocio, ClausulaDeFiltrado filtro)
            where T : RegistroDtm
        {
            // Preparamos la lista de términos
            var nombresBuscados = filtro.Valor.Split(Simbolos.separadorDeCadenasDeFiltrado, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

            // Archivos directos del negocio
            var archivos = negocio.Archivos(contexto).Where(vin =>
                contexto.Set<ArchivoDtm>().Any(a =>
                    a.Id == vin.idElemento2 &&
                    nombresBuscados.Any(nb => a.Nombre.Contains(nb))
                )
            );

            // Archivos dentro de archivadores
            var archivosAnexadosAArchivadores = enumNegocio.Archivador.Archivos(contexto).Where(vin =>
                contexto.Set<ArchivoDtm>().Any(a =>
                    a.Id == vin.idElemento2 &&
                    nombresBuscados.Any(nb => a.Nombre.Contains(nb))
                )
            );

            consulta = consulta.Where(x =>
                negocio.Archivadores(contexto).Any(vinculados =>
                    vinculados.idElemento1 == x.Id &&
                    archivosAnexadosAArchivadores.Any(anexados => anexados.idElemento1 == vinculados.idElemento2)
                ) ||
                archivos.Any(a => a.idElemento1 == x.Id)
            );

            return consulta;
        }

        public static IQueryable<T> FiltrosPorArchivadores<T>(this IQueryable<T> consulta, ContextoSe Contexto, List<ClausulaDeFiltrado> filtros)
            where T : RegistroDtm
        {
            var negocio = typeof(T).NegocioDeUnDtm();
            if (negocio.UsaArchivadores())
            {
                ClausulaDeFiltrado filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrDeUnArchivador.IdArchivador.ToLower());
                if (filtro != null)
                {
                    consulta = consulta.Where(x => negocio.Archivadores(Contexto).Any(y => y.idElemento1 == x.Id && y.idElemento2 == filtro.Valor.Entero()));
                    filtro.Aplicado = true;

                    var filtroQueMostrar = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrParametrosNeg.QueMostrar.ToLower() && x.Criterio == enumCriteriosDeFiltrado.diferente && !x.Aplicado);
                    if (filtroQueMostrar != null)
                        filtroQueMostrar.Aplicado = true;

                    return consulta;
                }

                filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrDeUnArchivador.MostraArchivadoresRelacionados.ToLower());
                if (filtro != null)
                {
                    if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                        consulta = consulta.Where(x => negocio.Archivadores(Contexto).Any(y => y.idElemento1 == x.Id));
                    if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                        consulta = consulta.Where(x => !negocio.Archivadores(Contexto).Any(y => y.idElemento1 == x.Id));
                    filtro.Aplicado = true;
                }

            }
            return consulta;
        }

        public static ArchivadorDtm CrearArchivador(ContextoSe Contexto, enumNegocio negocio, IElementoDeProcesoDtm elemento, string tipo, int idTipo)
        {
            var nombre = $"Documentación {tipo} del {negocio.Singular().ToLower()}: {elemento.Referencia}";
            return elemento.CrearArchivador(Contexto, idTipo, nombre);
        }

        public static ArchivadorDtm AsociarArchivador(this IElementoDeProcesoDtm elemento, ContextoSe contexto, int idTipo, string nombre)
        {
            var archivador = elemento.CrearArchivador(contexto, idTipo, nombre);
            var negocioDtm = NegociosDeSe.LeerNegocioPorDtm(elemento.GetType().FullName);
            GestorDeVinculos.Vincular(contexto, NegociosDeSe.ToEnumerado(negocioDtm), enumNegocio.Archivador, elemento.Id, archivador.Id);
            return archivador;
        }

        public static ArchivadorDtm CrearArchivador(this IElementoDeProcesoDtm elemento, ContextoSe contexto, int idTipo, string nombre)
        {
            var archivador = new ArchivadorDtm();
            archivador.IdCg = elemento.IdCg;
            archivador.IdTipo = idTipo;
            archivador.Nombre = nombre;
            return archivador.Insertar(contexto, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
        }

        public static bool TieneArchivos(this ArchivadorDtm archivador, ContextoSe contexto)
        {
            var vinculos = VinculoSql.LeerVinculosCon(contexto, enumNegocio.Archivador.TipoDtm(), enumNegocio.Archivos, ApiDeRegistroDtm.EsquemaTabla(typeof(ArchivoDtm)), archivador.Id);
            return vinculos.Count() > 0;
        }

        public static (bool Degradado, string Mensaje) DegradarPermisosDeGestor(this ArchivadorDtm archivador, ContextoSe contexto, bool analizarBloqueo = true, bool analizarDelSistema = true)
        {
            foreach (var negocio in NegociosDeSe.VinculadosConArchivadores())
            {
                if (DegradarPermisosPorNegocio(contexto, archivador, negocio))
                    return (Degradado: true, Mensaje: $"El archivador '{archivador.Referencia}' se relaciona con elementos de baja, cancelado o en un estado a los que el usuario no tiene acceso, por tanto no es editable");
            }

            //if (SemaforoDeProcesoSql.HaySemaforoPara(contexto, enumNegocio.Archivador.IdNegocio(), archivador.Id, new List<enumOpercionesDeSemaforo> { enumOpercionesDeSemaforo.EARC, enumOpercionesDeSemaforo.IZIP }))
            //    return (Degradado: true, Mensaje: $"El archivador '{archivador.Referencia}' está bloqueado por un proceso de importación/exportación");

            if (analizarBloqueo && archivador.Bloqueado)
                return (Degradado: true, Mensaje: $"El archivador '{archivador.Referencia}' está bloqueado por el usuario {archivador.Bloqueador(contexto).Login}");

            if (analizarDelSistema && archivador.Tipo<TipoDeArchivadorDtm>(contexto).DelSistema)
                return (Degradado: true, Mensaje: $"El archivador '{archivador.Referencia}'es del sistema, sólo hay acceso en consulta");

            return (Degradado: false, Mensaje: null);
        }

        private static bool DegradarPermisosPorNegocio(ContextoSe contexto, ArchivadorDtm archivador, enumNegocio negocio)
        {
            var vinculados = VinculoSql.LeerVinculosAl(contexto, negocio.TipoDtm(), enumNegocio.Archivador, typeof(ArchivadorDtm), archivador.Id, filtros: null);
            foreach (var vinculado in vinculados)
            {
                if (DegradarPermisosPorVinculado(contexto, archivador, negocio, vinculado))
                    return true;
            }
            return false;
        }

        private static bool DegradarPermisosPorVinculado(ContextoSe contexto, ArchivadorDtm archivador, enumNegocio negocio, VinculoDtm vinculado)
        {
            var elementoVinculado = NegociosDeSe.CrearGestor(contexto, negocio).LeerRegistroPorId(vinculado.idElemento1, true, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });
            if (negocio.UsaBaja())
                if (((IUsaBaja)elementoVinculado).Baja)
                    return true;

            if (negocio.UsaEstado())
            {
                var estado = ((IElementoDeProcesoDtm)elementoVinculado).Estado(contexto);
                if (estado.Terminado || estado.Cancelado)
                {
                    return true;
                }

                if (!contexto.DatosDeConexion.EsAdministrador && !ApiDePermisos.HayPermisosDeEstado(contexto, negocio, estado))
                {
                    return true;
                }
            }
            return false;
        }

        public static void AuditarOperacion(this ArchivoDtm archivo, ContextoSe contexto, string mensaje)
        {
            new AuditoriaDeUnArchivoDtm
            {
                IdArchivo = archivo.Id,
                Auditoria = mensaje,
            }.Insertar(contexto);
        }

        public static void AuditarCopiar(this ArchivoDtm archivo, ContextoSe contexto, ArchivoDtm destino, string referenciaOrigen, string referenciaDestino)
        {
            var mensaje = ltrDeAuditoriaDeArchivo.Copiar.Replace("[0]", contexto.DatosDeConexion.Login)
                .Replace("[1]", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"))
                .Replace("[2]", $"Origen: '{referenciaOrigen}' Destino: '{referenciaDestino}'");
            destino.AuditarOperacion(contexto, mensaje);

            mensaje = ltrDeAuditoriaDeArchivo.Copiar.Replace("[0]", contexto.DatosDeConexion.Login)
                .Replace("[1]", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"))
                .Replace("[2]", $"Origen: '{referenciaOrigen}' Destino: '{referenciaDestino}'");
            archivo.AuditarOperacion(contexto, mensaje);
        }

        public static void AuditarEnlazar(this ArchivoDtm archivo, ContextoSe contexto, string referenciaOrigen)
        {
            var mensaje = ltrDeAuditoriaDeArchivo.Enlazar.Replace("[0]", contexto.DatosDeConexion.Login)
                .Replace("[1]", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"))
                .Replace("[2]", $"Origen: '{referenciaOrigen}'");
            archivo.AuditarOperacion(contexto, mensaje);
        }

        public static void AuditarDescarga(this ArchivoDtm archivo, ContextoSe contexto)
        {
            new AuditoriaDeUnArchivoDtm
            {
                IdArchivo = archivo.Id,
                Auditoria = ltrDeAuditoriaDeArchivo.Descargar.Replace("[0]", contexto.DatosDeConexion.Login).Replace("[1]", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"))
            }.Insertar(contexto);
        }
        public static string Auditoria(this ArchivoDtm archivo, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Arc_AuditoriaDeArchivo);
            if (!cache.ContainsKey(archivo.Id.ToString()))
            {
                var registros = contexto.Set<AuditoriaDeUnArchivoDtm>().Where(x => x.IdArchivo == archivo.Id).OrderByDescending(x => x.Id).ToList();
                var auditoria = "";
                foreach (var registro in registros)
                {
                    auditoria = auditoria + $"{(auditoria.IsNullOrEmpty() ? "" : Environment.NewLine)}" + registro.Auditoria;
                }
                cache[archivo.Id.ToString()] = auditoria;
            }
            return (string)cache[archivo.Id.ToString()];
        }

        public static string Referencia(this CarpetaDtm carpeta, ContextoSe contexto)
        {
            var aux = carpeta;
            var referencia = carpeta.Expresion;
            while (aux.IdPadre is not null)
            {
                aux = aux.Padre is null
                ? enumNegocio.Carpeta.SeleccionarPorId(contexto, aux.IdPadre.Entero())
                : aux.Padre;
                referencia = aux.Expresion + "." + referencia;
            }
            return referencia;
        }

        public static List<NodoDtm> ToNodosDeJerarquia(this List<NodoDeCarpetaDtm> carpetasLeidosDtm, bool mostrarNumero)
        {
            var nodos = new List<NodoDtm>();
            foreach (var carpeta in carpetasLeidosDtm)
            {
                nodos.Add(new NodoDtm
                {
                    Id = carpeta.Id,
                    IdPadre = carpeta.IdPadre,
                    Nombre = mostrarNumero ? $"{carpeta.Nombre}{Simbolos.DosPuntosConEspacio}{carpeta.NumeroDeArchivos}" : carpeta.Nombre,
                    Activo = carpeta.Activo,
                    TipoDtm = carpeta.TipoDtm,
                });
            }

            return nodos;
        }

        public static int Cfg_Id_Tipo_De_Archivador_Sii(ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Valores);
            if (!cache.ContainsKey(nameof(Cfg_Id_Tipo_De_Archivador_Sii)))
            {
                var idUsuario = contexto.DatosDeConexion.IdUsuario;
                contexto.AsignarUsuario(contexto.Administrador());
                try
                {
                    var tipo = new TipoDeArchivadorDtm
                    {
                        Nombre = CacheDeVariable.Cfg_Tipo_De_Archivador_Sii,
                        ClaseDeLibro = enumClaseDeLibro.POR_CG_TIPO,
                        Sigla = ltrSii.SiglaTipoArchivadorSii,
                        Visible = false,
                        DelSistema = true,
                        NombreModificable = false,
                        PermiteCrear = true,
                        Activo = true,
                    }.Persistir(contexto);
                    cache[nameof(Cfg_Id_Tipo_De_Archivador_Sii)] = tipo.Id;
                }
                finally
                {
                    contexto.AsignarUsuario(contexto.SeleccionarPorId<UsuarioDtm>(idUsuario));
                }
            }
            return (int)cache[nameof(Cfg_Id_Tipo_De_Archivador_Sii)];
        }

        public static ArchivadorDtm Copiar(this ArchivadorDtm archivadorOrigen, ContextoSe contexto, int idcg, TipoDeArchivadorDtm tipo, string nombre, string descripcion, bool copiarCarpetas, bool copiarArchivos, bool enlazarArchivos)
        {
            var archivadorDestino = new ArchivadorDtm
            {
                IdCg = idcg,
                IdTipo = tipo.Id,
                Nombre = nombre,
                Descripcion = descripcion,
                Baja = false,
                SincronizarCon = null,
                Bloqueado = false
            }.Insertar(contexto);

            if (copiarArchivos || enlazarArchivos)
            {
                var archivos = archivadorOrigen.Vinculados<ArchivoDtm>(contexto);
                ProcesarArchivos(contexto, archivos, archivadorDestino, copiarArchivos, enlazarArchivos, archivadorOrigen.Referencia);
            }

            if (copiarCarpetas)
            {
                var carpetas = archivadorOrigen.LeerJerarquiaDeCarpetas(contexto, new Dictionary<string, object>());
                foreach (var carpeta in carpetas.Where(c => c.IdPadre is null))
                {
                    var nueva = new CarpetaDtm { IdPadre = null, Nombre = carpeta.Nombre, IdArchivador = archivadorDestino.Id }.Insertar(contexto);
                    CopiarRama(contexto, carpetas, carpeta, nueva, copiarArchivos, enlazarArchivos, archivadorOrigen.Referencia);
                }
            }

            return archivadorDestino;
        }

        private static void CopiarRama(ContextoSe contexto, List<NodoDeCarpetaDtm> carpetas, NodoDeCarpetaDtm origen, CarpetaDtm destino, bool copiarArchivos, bool enlazarArchivos, string referenciaArchivador)
        {
            var archivos = GestorDeVinculos.RegistrosVinculados<ArchivoDtm>(contexto, enumNegocio.Carpeta, enumNegocio.Archivos, origen.Id);
            ProcesarArchivos(contexto, archivos, destino, copiarArchivos, enlazarArchivos, $"({referenciaArchivador}) {origen.Expresion}");

            foreach (var carpeta in carpetas.Where(c => c.IdPadre == origen.Id))
            {
                var nueva = new CarpetaDtm { IdPadre = destino.Id, Nombre = carpeta.Nombre, IdArchivador = destino.IdArchivador }.Insertar(contexto);
                CopiarRama(contexto, carpetas, carpeta, nueva, copiarArchivos, enlazarArchivos, referenciaArchivador);
            }
        }

        private static void ProcesarArchivos(ContextoSe contexto, List<ArchivoDtm> archivos, RegistroDtm destino, bool copiarArchivos, bool enlazarArchivos, string referenciaOrigen)
        {
            if (copiarArchivos || enlazarArchivos)
            {
                foreach (var archivo in archivos)
                {
                    var idArchivo = archivo.Id;
                    if (copiarArchivos)
                    {
                        var nuevoArchivo = ApiDeArchivos.SubirArchivoInterno(contexto, Path.Combine(archivo.AlmacenadoEn, $"{archivo.Id}.{enumExtensiones.se}"), sincronizar: false, nombreFicheroParaAlmacenar: archivo.Nombre, copiar: true, sanitizar: false);
                        idArchivo = nuevoArchivo.Id;
                        archivo.AuditarCopiar(contexto, nuevoArchivo, referenciaOrigen, (destino is CarpetaDtm) ? ((INombre)destino).Expresion : ((IUsaReferencia)destino).Referencia);
                    }
                    destino.Vincular(contexto, enumNegocio.Archivos, idArchivo);
                    if (enlazarArchivos)
                        archivo.AuditarEnlazar(contexto, referenciaOrigen);
                }
            }
        }

        public static List<NodoDeCarpetaDtm> LeerJerarquiaDeCarpetas(this ArchivadorDtm archivador, ContextoSe contexto, Dictionary<string, object> filtros)
        {
            var parametrosSql = new Dictionary<string, object>();
            var sentenciaSql = CarpetaSql.AplicarFiltros(CarpetaSql.JerarquiaDeCarpeta, archivador.Id, filtros, parametrosSql);

            var consulta = new ConsultaSql<NodoDeCarpetaDtm>(contexto, sentenciaSql);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
            return registros;
        }

        public static bool HayCarpetas(this ArchivadorDtm archivador, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Arc_HayCarpetas);
            if (!cache.ContainsKey(archivador.Id.ToString()))
            {
                cache[archivador.Id.ToString()] = contexto.Set<CarpetaDtm>().Any(carpeta => carpeta.IdArchivador == archivador.Id);
            }
            return (bool)cache[archivador.Id.ToString()];
        }


        public static int ObtenerTotalArchivosDeArchivador(this ArchivadorDtm archivador, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Arc_CantidadDeArchivos);
            if (!cache.ContainsKey(archivador.Id.ToString()))
            {

                var totalArchivos = contexto.Set<ArchivadorDtm>()
               .Where(a => a.Id == archivador.Id)
               .Select(a => new
               {
                   ArchivosDirectos = contexto.Set<ArchivosDeUnArchivadorDtm>()
                       .Count(aa => aa.idElemento1 == a.Id),
                   ArchivosCarpetas = contexto.Set<CarpetaDtm>()
                       .Where(c => c.IdArchivador == a.Id)
                       .Join(
                           contexto.Set<ArchivosDeUnaCarpetaDtm>(),
                           c => c.Id,
                           ca => ca.idElemento1,
                           (c, ca) => ca.idElemento2
                       )
                       .Count()
               })
               .AsEnumerable()
               .Select(result => result.ArchivosDirectos + result.ArchivosCarpetas)
               .FirstOrDefault();

                cache[archivador.Id.ToString()] = totalArchivos;
            }
            return (int)cache[archivador.Id.ToString()];
        }

        public static List<ExtensionDeArchivo> ArchivosExt(this CarpetaDtm carpeta, ContextoSe contexto, bool incluirOriginal = false)
        {
            // Primero, obtenemos la lista de archivos usando el método existente
            var archivos = enumNegocio.Carpeta.Archivos(contexto, carpeta.Id, incluirOriginal: false);
            // Luego, proyectamos esta lista a ArchivoExt
            var archivosExt = archivos.Select(archivo => new ExtensionDeArchivo
            {
                IdArchivo = archivo.Id,
                Negocio = enumNegocio.Archivador,
                IdElemento = carpeta.IdArchivador,
                IdArchivador = null,
                IdCarpeta = carpeta.Id,
                Archivo = archivo,
                Elemento = carpeta.Archivador,
            }).ToList();

            return archivosExt;
        }

        public static List<ExtensionDeArchivo> ArchivosExt(this ArchivadorDtm archivador, ContextoSe contexto, IElementoDtm padre)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Arc_ListaDeArchivosExt);
            if (!cache.ContainsKey(archivador.Id.ToString()))
            {
                var archivosExtEnumerable = contexto.Set<ArchivadorDtm>()
                           .Where(a => a.Id == archivador.Id)
                           .Select(a => new
                           {
                               ArchivosDirectos = contexto.Set<ArchivosDeUnArchivadorDtm>()
                                   .Where(aa => aa.idElemento1 == a.Id)
                                   .Select(aa => new ExtensionDeArchivo
                                   {
                                       IdArchivo = aa.idElemento2,
                                       Negocio = enumNegocio.Archivador,
                                       IdElemento = padre == null ? a.Id : padre.Id,
                                       IdArchivador = a.Id,
                                       IdCarpeta = (int?)null,
                                       Carpeta = null,
                                       Archivo = contexto.Set<ArchivoDtm>().FirstOrDefault(arch => arch.Id == aa.idElemento2),
                                       Elemento = padre == null ? archivador : padre
                                   }).ToList(),
                               ArchivosCarpetas = contexto.Set<CarpetaDtm>()
                                   .Where(c => c.IdArchivador == a.Id)
                                   .Join(
                                       contexto.Set<ArchivosDeUnaCarpetaDtm>(),
                                       c => c.Id,
                                       ca => ca.idElemento1,
                                       (c, ca) => new ExtensionDeArchivo
                                       {
                                           IdArchivo = ca.idElemento2,
                                           Negocio = enumNegocio.Archivador,
                                           IdElemento = padre == null ? a.Id : padre.Id,
                                           IdArchivador = a.Id,
                                           IdCarpeta = c.Id,
                                           Carpeta = contexto.Set<CarpetaDtm>().Include(x => x.Padre).FirstOrDefault(carp => carp.Id == ca.idElemento1),
                                           Archivo = contexto.Set<ArchivoDtm>().FirstOrDefault(arch => arch.Id == ca.idElemento2),
                                           Elemento = padre == null ? archivador : padre
                                       }
                                   ).ToList()
                           })
                           .AsEnumerable()
                           .SelectMany(result => result.ArchivosDirectos.Concat(result.ArchivosCarpetas))
                           .Distinct();
                var archivosExt = archivosExtEnumerable.ToList();

                cache[archivador.Id.ToString()] = archivosExt;
            }
            return (List<ExtensionDeArchivo>)cache[archivador.Id.ToString()];
        }

        public static List<ExtensionDeArchivo> Ordenar(this List<ExtensionDeArchivo> archivosExt, ContextoSe contexto, enumNegocio negocioPadre)
        {
            var esElPadreArchivador = negocioPadre == enumNegocio.Archivador;
            foreach (var archivoExt in archivosExt)
            {
                if (negocioPadre == enumNegocio.Carpeta)
                {
                    archivoExt.Expresion = archivoExt.Archivo.Nombre;
                    continue;
                }

                if (!archivoExt.EstaAnexadoAUnArchivador)
                {
                    archivoExt.Expresion = archivoExt.Archivo.Nombre;
                    continue;
                }

                if (esElPadreArchivador)
                {
                    if (archivoExt.EstaAnexadoAUnaCarpeta)
                    {
                        archivoExt.Expresion = archivoExt.Carpeta.Nombre + ": " + archivoExt.Archivo.Nombre;
                        continue;
                    }
                    archivoExt.Expresion = archivoExt.Archivo.Nombre;
                    continue;
                }

                var archivador = contexto.SeleccionarPorId<ArchivadorDtm>((int)archivoExt.IdArchivador);
                if (archivoExt.EstaAnexadoAUnaCarpeta)
                {
                    archivoExt.Expresion = archivador.Referencia + "." + archivoExt.Carpeta.Nombre + ": " + archivoExt.Archivo.Nombre;
                    continue;
                }
                archivoExt.Expresion = archivador.Referencia + ": " + archivoExt.Archivo.Nombre;
            }
            return archivosExt.OrderBy(x => x.Expresion).ToList();
        }

        public static ArchivadorDtm ObtenerArchivadorBlockChain(ContextoSe contexto, int ejercicio, string nif)
        {
            var cg = ExtensionCentrosGestores.Cfg_CG_De_Documentacion(contexto);
            var idtipo = Cfg_Id_Tipo_De_Archivador_Sii(contexto);
            var nombreArchivador = $"{ejercicio}-{nif}-BlockChain";
            var archivadores = contexto.SeleccionarTodos<ArchivadorDtm>(new Dictionary<string, object> {
                    { nameof(ArchivadorDtm.IdCg), cg.Id },
                    { nameof(ArchivadorDtm.IdTipo), idtipo },
                    { nameof(ArchivadorDtm.Nombre), nombreArchivador } });

            if (archivadores.Count == 1)
                return archivadores.First();

            if (archivadores.Count > 1)
                GestorDeErrores.Emitir($"Se han encontrado {archivadores.Count} archivadores de blockChain de Verifactu {ejercicio}");

            return new ArchivadorDtm
            {
                IdCg = ExtensionCentrosGestores.Cfg_CG_De_Documentacion(contexto).Id,
                IdTipo = idtipo,
                Nombre = nombreArchivador,
                Descripcion = $"Archivador de blockChain de Verifactu {ejercicio} y nif {nif}",
                Baja = false,
                SincronizarCon = null,
                Bloqueado = false
            }.InsertarComoAdministrador(contexto);
        }

        public static void Cancelar(this ArchivoDtm archivo, ContextoSe contexto, string auditoria)
        {
            archivo.Nombre = $"{Simbolos.ArchivoCancelado}{archivo.Nombre}";
            archivo.Modificar(contexto, accionEjecutada: ltrDeUnArchivo.Accion_Marcar_Cancelacion);
            new AuditoriaDeUnArchivoDtm
            {
                IdArchivo = archivo.Id,
                Auditoria = auditoria,
            }.InsertarComoAdministrador(contexto);
        }

        public static void Activar(this ArchivoDtm archivo, ContextoSe contexto, string auditoria)
        {
            if (archivo.Nombre.StartsWith(Simbolos.ArchivoCancelado))
            {
                new AuditoriaDeUnArchivoDtm
                {
                    IdArchivo = archivo.Id,
                    Auditoria = auditoria
                }.InsertarComoAdministrador(contexto);

                var vecesCancelado = contexto.Set<AuditoriaDeUnArchivoDtm>().Count(x => x.Auditoria.StartsWith(ltrDeAuditoriaDeArchivo.enuAccion.Cancelar.ToString()));
                var vecesReactivado = contexto.Set<AuditoriaDeUnArchivoDtm>().Count(x => x.Auditoria.StartsWith(ltrDeAuditoriaDeArchivo.enuAccion.Reactivar.ToString()));
                if (vecesCancelado <= vecesReactivado)
                {
                    archivo.Nombre = archivo.Nombre.Replace(Simbolos.ArchivoCancelado, "");
                    archivo.Modificar(contexto, accionEjecutada: ltrDeUnArchivo.Accion_Anular_Cancelacion);
                }
            }
        }

        public static bool EsUnCoreo(this ArchivadorDtm archivador, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Arc_EsCorreo);
            if (cache.ContainsKey(archivador.Id.ToString()))
            {
                return (bool)cache[archivador.Id.ToString()];
            }
            cache[archivador.Id.ToString()] = contexto.Existen<MiCorreoDtm>(new List<ClausulaDeFiltrado>
                    {
                    new ClausulaDeFiltrado{ Clausula = nameof(MiCorreoDtm.IdNegocio), Valor= enumNegocio.Archivador.IdNegocio().ToString()},
                    new ClausulaDeFiltrado{ Clausula = nameof(MiCorreoDtm.IdElemento), Valor= archivador.Id.ToString()}
                    });
            return (bool)cache[archivador.Id.ToString()];
        }
    }
}


