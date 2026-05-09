using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.TrabajosSometidos;
using ModeloDeDto.Gastos;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.TrabajosSometidos;
using ServicioDeDatos.Utilidades;
using Utilidades;

namespace GestoresDeNegocio.Gastos
{
    public enum enumTrabajosDeFacturasRec
    {
        [Description("Crear facturas con Ia")]
        CrearFarConIa,
        [Description("Actualización de facturas recibidas")]
        ActualizacionDeFacturas
    }

    public class TrabajosDeFacturasRec
    {
        public static TrabajoDeUsuarioDtm SometerCrearFarConIa(ContextoSe contexto, int idCg, int idTipo, int idArchivo, int? idProveedor)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeFacturasRec).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeFacturasRec.CrearFarConIa.Descripcion(), dll, clase, nameof(enumTrabajosDeFacturasRec.CrearFarConIa), comunicarFin: true);

            var parametrosEntrada = new Dictionary<string, object> {
                        { nameof(CrearFarConIaDto.IdCgPropuesto), idCg },
                        { nameof(CrearFarConIaDto.IdTipoFarPropuesto), idTipo },
                        { nameof(CrearFarConIaDto.IdArchivo), idArchivo },
                        { nameof(CrearFarConIaDto.IdProveedor), idProveedor }
             };

            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() },
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddMinutes(-1) },
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static TrabajoDeUsuarioDtm SometerActualizacionDeFacturas(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeFacturasRec).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeFacturasRec.ActualizacionDeFacturas.Descripcion(), dll, clase, nameof(enumTrabajosDeFacturasRec.ActualizacionDeFacturas), comunicarFin: false);

            var parametrosEntrada = new Dictionary<string, object> { };

            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() },
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1).Date.AddMinutes(1) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400}
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void ActualizacionDeFacturas(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            var facturas = contexto.SeleccionarTodos<FacturaRecDtm>(new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado {
                      Clausula = ltrDeUnaFacturaRec.FiltroPorEtapa,
                      Criterio = enumCriteriosDeFiltrado.igual,
                      Valor = enumEtapasDeFacturasRec.FAR_Etapa_De_Pago.ToString()}
                }, negocio: enumNegocio.FacturaRecibida);

            foreach (var factura in facturas)
            {
                if (factura.EstaPagada(contexto))
                {
                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        factura.TransitarALaEtapa(contexto, enumEtapasDeFacturasRec.FAR_Etapa_Pagada.EstadosDeLaEtapa());
                        entorno.CrearTraza($"Factura '{factura.Referencia}' transitada");
                        contexto.Commit(tran);
                    }
                    catch (Exception ex)
                    {
                        contexto.Rollback(tran);
                        entorno.AnotarError(ex);
                    }
                }
            }
        }

        public static void CrearFarConIa(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(enumTrabajosDeFacturasRec.CrearFarConIa));
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();

            var idArchivoZip = (int)parametros.LeerValor<long>(nameof(CrearFarConIaDto.IdArchivo));
            var remplazar = false;
            var renombrar = false;
            var eliminarArchivo = false;
            var eliminarCarpeta = false;
            var cg = contexto.SeleccionarPorId<CentroGestorDtm>((int)parametros.LeerValor<long>(nameof(CrearFarConIaDto.IdCgPropuesto)));
            var tipoFar = contexto.SeleccionarPorId<TipoDeFacturaRecDtm>((int)parametros.LeerValor<long>(nameof(CrearFarConIaDto.IdTipoFarPropuesto)));
            var idProveedor = (int?)parametros.LeerValor<long?>(nameof(CrearFarConIaDto.IdProveedor), valorPorDefecto: (long?)null);
            var proveedor = idProveedor.Entero() > 0 ? contexto.SeleccionarPorId<ProveedorDtm>((int)idProveedor) : null;

            enumParametrosDeFacturasRec.FAR_Unidad_Medida.ValidarQueEstaDefinido();
            enumParametrosDeFacturasRec.FAR_Naturaleza.ValidarQueEstaDefinido();

            var archivador = ExtensorDeFacturasRec.CrearArchivadorDeFacturas(contexto, cg, "Facturas a incorporar por IA", proveedor);
            var otorgado = entorno.Ejecutor.OtorgarAdministrador(contexto);
            try
            {
                TrabajosDelSistemaDocumental.ImportarZipInterno(contexto, archivador, idArchivoZip, remplazar, renombrar, eliminarArchivo, eliminarCarpeta);
                var rutas = Task.Run(() => CrearFarConIaInterno(entorno, new ProcesadorOcr.ProcesadorOcr(), archivador, tipoFar.Id, proveedor)).Result;
                foreach (var r in rutas)
                {
                    if (File.Exists(r)) File.Delete(r);
                }
            }
            catch (Exception e)
            {
                archivador.Baja = true;
                archivador.Modificar(contexto);
                entorno.AnotarError(e);
            }
            finally
            {
                if (otorgado) entorno.Ejecutor.AnularAdministrador(contexto, otorgado);
                contexto.CerrarTraza();
            }
        }

        public static void ProcesarArchivosFar(EntornoDeTrabajo entorno, ArchivadorDtm archivador, int IdCg, int idTipo, int? idProveedor, int? idCarpeta)
        {
            var procesadorOcr = new ProcesadorOcr.ProcesadorOcr();
            var tipoFar = entorno.contextoDelProceso.SeleccionarPorId<TipoDeFacturaRecDtm>(idTipo);
            var proveedor = idProveedor.Entero() > 0 ? entorno.contextoDelProceso.SeleccionarPorId<ProveedorDtm>((int)idProveedor) : null;
            var rutas = Task.Run(() => CrearFarConIaInterno(entorno, procesadorOcr, archivador, tipoFar.Id, proveedor, IdCg, idCarpeta)).Result;
            foreach (var r in rutas)
            {
                if (File.Exists(r)) File.Delete(r);
            }
        }

        private async static Task<List<string>> CrearFarConIaInterno(EntornoDeTrabajo entorno, ProcesadorOcr.ProcesadorOcr procesadorOcr, ArchivadorDtm archivador, int idTipo, ProveedorDtm proveedor, int? idCg = null, int? idCarpeta = null)
        {
            var archivosExt = archivador.ArchivosExt(entorno.contextoDelProceso, padre: null);
            List<string> rutas = new List<string>();
            List<ArchivoDtm> procesados = new List<ArchivoDtm>();
            var estadosVivos = enumNegocio.FacturaRecibida.Estados(entorno.contextoDelProceso).Where(e => !e.Cancelado).Select(e => e.Id).ToList();

            foreach (var archivoExt in archivosExt)
            {
                if (procesados.Any(procesado => procesado.Id == archivoExt.Archivo.Id))
                    continue;
                if (archivoExt.Archivo.Nombre.StartsWith(Simbolos.ArchivoCancelado))
                    continue;
                if (idCarpeta.Entero() > 0 && archivoExt.IdCarpeta.Entero() > 0 && archivoExt.IdCarpeta != idCarpeta)
                    continue;

                var facturasVinculada = GestorDeVinculos.RegistrosVinculados<FacturaRecDtm>(entorno.contextoDelProceso, enumNegocio.Archivos, enumNegocio.FacturaRecibida, archivoExt.Archivo.Id);

                var facturaVinculada = facturasVinculada.FirstOrDefault(fac => estadosVivos.Contains(fac.IdEstado));
                if (facturaVinculada != null)
                {
                    entorno.CrearTraza($"El archivo '{archivoExt.Archivo.Nombre}' ya está asociado a la factura '{facturaVinculada.Referencia}'");
                    continue;
                }

                var rutaArchivo = ApiDeArchivos.ObtenerRutaArchivo(archivoExt.Archivo);
                rutas.Add(rutaArchivo);
                procesados.Add(archivoExt.Archivo);
                var tran = entorno.contextoDelProceso.IniciarTransaccion();
                try
                {
                    var ia = ExtensorDeIa.CrearIa(ExtensorDeUsuarios.IaUsada(entorno.contextoDelProceso));
                    var jsonFactura = "";

                    try
                    {
                        jsonFactura = await ExtensorDeIa.ProcesarFactura(archivoExt.IdArchivo, rutaArchivo, ia, procesadorOcr);
                    }
                    catch (Exception ex)
                    {
                        entorno.AnotarError(ex);
                    }

                    if (jsonFactura.IsNullOrEmpty())
                        GestorDeErrores.Emitir($"No se ha podido extraer el contenido de la imagen del archivo factura '{archivoExt.Archivo.Nombre}' de '{archivoExt.Elemento}' ");

                    var factura = ExtensorDeFacturasRec.CrearFactura(entorno.contextoDelProceso, archivador, idTipo, jsonFactura, proveedor, archivoExt.Archivo, idCg);
                    if (factura != null)
                        entorno.CrearTraza($"factura '{factura.Expresion}' creada");
                    else
                        GestorDeErrores.Emitir($"el archivo '{archivoExt.Archivo.Nombre}' no se ha podido procesar");
                    entorno.contextoDelProceso.Commit(tran);
                }
                catch (Exception ex)
                {
                    entorno.contextoDelProceso.Rollback(tran);
                    if (ex.Message.StartsWith("ya existe la factura"))
                        entorno.AnotarError($"el archivo '{archivoExt.Archivo.Nombre}' ya existe", ex);
                    else if (ex.Message.StartsWith("El NIF "))
                        entorno.AnotarError($"el archivo '{archivoExt.Archivo.Nombre}' tiene Nif no válido", ex);
                    else
                        entorno.AnotarError($"el archivo '{archivoExt.Archivo.Nombre}' no se ha podido procesar", ex);
                    if (!archivoExt.Archivo.Nombre.Contains(ltrDeUnArchivo.PendienteProcesar))
                        archivoExt.Archivo.Nombre = (ltrDeUnArchivo.PendienteProcesar + ' ' + archivoExt.Archivo.Nombre).Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1);
                    archivoExt.Archivo.Recargar(entorno.contextoDelProceso).ModificarComoAdministrador(entorno.contextoDelProceso, accionQueSeEjecuta: ltrDeUnArchivo.Accion_Permitir_Modificar_Nombre);
                    continue;
                }
            }
            return rutas;
        }

    }
}
