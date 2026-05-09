using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.SistemaDocumental;
using ModeloDeDto.SistemaDocumental;
using GestoresDeNegocio.SistemaDocumental;
using Inicializador.SistemaDocumental;
using System;
using Utilidades;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Gastos;

namespace MVCSistemaDeElementos.Controllers
{
    public class ArchivadoresController : EntidadController<ContextoSe, ArchivadorDtm, ArchivadorDto>
    {
        public ArchivadoresController(GestorDeArchivadores gestorDeArchivadores, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeArchivadores,
           gestorDeErrores
         )
        {
        }

        protected override Dictionary<string, object> IndicadoresParaInicializarLaVistaMnt(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var indicadores = base.IndicadoresParaInicializarLaVistaMnt(contexto, parametros);
            indicadores.Add(IndArchivador.PermiteSincronizar, false);
            indicadores.Add(IndArchivador.IdTipoArchivadorDeFacturaRec, enumParametrosDeFacturasRec.FAR_Tipo_De_Archivador.Entero(errorSiNoDefinido: false));
            return indicadores;
        }

        public IActionResult CrudArchivadores()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeArchivadores).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.SistemaDocumental}/{nameof(CrudArchivadores)}";
                    return base.View(destino, new DescriptorDeArchivadores(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<ArchivadorDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeArchivadores(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        public IActionResult CrearArchivadoresEco()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.DatosDeConexion.EsAdministrador)
                    GestorDeErrores.Emitir("Esta opción sólo se permite a administradores");

                InzArchivadoresEcoFin.ArchivadoresEconomicoFinanciero(Contexto);
                ViewBag.Mensaje = "Tipos inicializados";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                return RenderMensaje($"No se ha podido inicializar los tipos.{Environment.NewLine}{GestorDeErrores.Detalle(e)}");
            }
            finally
            {
                ServicioDeCaches.EliminarTodas();
            }
            return VistaDelPanelDeControl(Contexto);
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            List<int> ids = null;
            switch (opcion)
            {
                case eventosDeMf.Arc_IrACapetas:
                    return null;
                case eventosDeMf.Arc_ImportarZip:
                    return null;
                case eventosDeMf.Arc_ProcesarFarConIa:
                    return null;
                case eventosDeMf.Arc_CopiarArc:
                    return null;
                case eventosDeMf.Arc_ExportarArchivador:
                    ids = parametros.LeerValor(ltrParametrosEp.ids, new List<int>());
                    TrabajosDelSistemaDocumental.SometerExportacionDeArchivadores(Contexto, ids);
                    return null;
                case eventosDeMf.Arc_Descontabilizar:
                    ids = parametros.LeerValor(ltrParametrosEp.ids, new List<int>());
                    GestorDeArchivadores.Descontabilizar(Contexto, ids);
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        public JsonResult epCopiarArchivador(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                r.Datos = GestorDeArchivadores.CopiarArchivador(Contexto, parametros);
                r.Consola = $"Archivador copiado";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al copiar el archivado.");
            }
            return new JsonResult(r);
        }

        public JsonResult epImportarZip(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idArchivador = (int)parametros.LeerValor<long>(nameof(ImportarZipDto.IdArchivador));
                var idArchivoZip = (int)parametros.LeerValor<long>(nameof(ImportarZipDto.IdArchivo));
                var remplazar = parametros.LeerValor<bool>(nameof(ImportarZipDto.Remplazar));
                var renombrar = parametros.LeerValor<bool>(nameof(ImportarZipDto.Renombrar));
                var eliminarArchivo = parametros.LeerValor<bool>(nameof(ImportarZipDto.EliminarArchivo));
                // var eliminarCarpeta = parametros.LeerValor<bool>(nameof(ImportarZipDto.EliminarCarpeta));

                if (idArchivoZip == 0)
                    GestorDeErrores.Emitir("Ha de indicar el archivo Zip que quiere importar");

                if (idArchivador == 0)
                    GestorDeErrores.Emitir("Ha de indicar el archivador donde quiere importar el archivo zip");

                if (remplazar && renombrar)
                    GestorDeErrores.Emitir("Debe indicar si renombro el archivo importado o remplazo el existente, pero no ambas cosas");

                GestorDeArchivadores.SometerImportarZip(Contexto, idArchivador, idArchivoZip, remplazar, renombrar, eliminarArchivo, false);

                r.Consola = $"Trabajo sometido para importar archivo Zip.";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al importar el archivo zip.");
            }
            return new JsonResult(r);
        }


        public JsonResult epProcesarFarConIa(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idArchivador = (int)parametros.LeerValor<long>(nameof(ProcesarFarConIaDto.IdArchivador));
                var idTipo = (int)parametros.LeerValor<long>(nameof(ProcesarFarConIaDto.IdTipoFarPropuesto));
                var idCg = (int)parametros.LeerValor<long>(nameof(ProcesarFarConIaDto.IdCgPropuesto));
                var idProveedor = (int?)parametros.LeerValor<long?>(nameof(ProcesarFarConIaDto.IdProveedor), valorPorDefecto: (long?)null);
                var idCarpeta = (int?)parametros.LeerValor<long?>(nameof(ProcesarFarConIaDto.IdCarpetaSeleccionada), valorPorDefecto: (long?)null);

                if (idArchivador == 0 || idCg == 0 || idTipo == 0)
                    GestorDeErrores.Emitir("Debe indicar el archivador, el centro gestor y el tipo de factura");

                GestorDeArchivadores.SometerProcesarFarConIa(Contexto, idArchivador, idCg, idTipo, idProveedor == 0 ? null : idProveedor, idCarpeta == 0 ? null : idCarpeta);

                r.Consola = $"Trabajo sometido para procesar facturas con IA.";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error alsometer el trabajo para procesar facturas con IA.");
            }
            return new JsonResult(r);
        }

    }
}
