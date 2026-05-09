using GestoresDeNegocio.TrabajosSometidos;
using ServicioDeDatos.TrabajosSometidos;
using ServicioDeDatos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using ModeloDeDto;
using ServicioDeDatos.Expediente;
using System.Linq;
using Utilidades;
using ServicioDeDatos.Negocio;
using GestorDeElementos.Extensores;
using ServicioDeDatos.SistemaDocumental;
using ModeloDeDto.Expediente;
using GestorDeElementos;
using ServicioDeDatos.Entorno;

namespace GestoresDeNegocio.Administracion
{

    public enum enumTrabajosDeExpedientes
    {
        [Description("Comunicar documentación añadidad a un expediente")]
        ComunicarSubidaDeArchivosAExpedientes,
    }

    public class TrabajosDeExpedientes
    {
        public static TrabajoDeUsuarioDtm SometerComunicarSubidaDeArchivosAExpedientes(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeExpedientes).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeExpedientes.ComunicarSubidaDeArchivosAExpedientes.Descripcion(), dll, clase,
                nameof(enumTrabajosDeExpedientes.ComunicarSubidaDeArchivosAExpedientes), comunicarFin: false);
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400}
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void ComunicarSubidaDeArchivosAExpedientes(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(ComunicarSubidaDeArchivosAExpedientes));
            entorno.CrearTraza($"Inicio del proceso");
            var tran = contexto.IniciarTransaccion();
            try
            {
                var ultimaEjecucion = entorno.TrabajoSometido.UltimaEjecucion(contexto);

                var expendientesCandidatos = ExpedientesCandidatos(contexto, () => entorno.CrearTraza($"No hay definido tipos de expedientes a analizar, defina el parámetro {enumParametrosDeExpedientes.EXP_Tipos_Para_Comunicar_Responsable_Tras_Anexar}"));
                var responsables = expendientesCandidatos.Select(e => (int)e.IdResponsable).Distinct().ToList();
                foreach (var idResponsable in responsables)
                {
                    var expedientesPorComunicar = new List<TipoDtoElmento>();
                    var expedientesDeUnResponsable = expendientesCandidatos.Where(x => x.IdResponsable == idResponsable);
                    List<ExtensionDeArchivo> archivosExt = new List<ExtensionDeArchivo>();
                    foreach (var expediente in expedientesDeUnResponsable)
                    {
                        archivosExt.AddRange(expediente.ArchivosExt(contexto, recursivo: true, incluirOriginal: false));
                    }

                    var archivosNuevos = ultimaEjecucion.Iniciado != null ?
                        archivosExt.Where(archivo => (archivo.Archivo.FechaCreacion >= ultimaEjecucion.Iniciado)):
                        archivosExt.Where(archivo => (archivo.Archivo.FechaCreacion >= DateTime.Today)); 

                    if (archivosNuevos.Count() > 0)
                    {
                        var expedientes = archivosNuevos.Select(a => (ExpedienteDtm) a.Elemento).Distinct().ToList();
                        foreach(var e  in expedientes)
                        {
                            expedientesPorComunicar.Add(new TipoDtoElmento { IdElemento = e.Id, Referencia = e.Expresion, TipoDto = typeof(ExpedienteDto).FullName });
                        }
                    }

                    if (expedientesPorComunicar.Count > 0)
                    {
                        GestorDeCorreos.CrearCorreoPara(entorno.contextoDelProceso
                            , new List<string> { contexto.SeleccionarPorId<UsuarioDtm>(idResponsable).eMail }
                            , "Expedientes a los que se ha anexado documentación"
                            , "Se adjunta los enlaces a los expedientes a los que usuarios le han adjuntado documentación"
                            , expedientesPorComunicar //lista de expedientes afectados
                            , new List<string>()
                            );
                    }
                }
                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                entorno.AnotarError(e);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static List<ExpedienteDtm> ExpedientesCandidatos(ContextoSe contexto, Func<TrazaDeUnTrabajoDtm> traza)
        {
            var tipos = enumNegocio.Expediente.Parametro(enumParametrosDeExpedientes.EXP_Tipos_Para_Comunicar_Responsable_Tras_Anexar, crearParametro: true, valorPorDefecto:  Literal.Cero).Valor;
            var lista = tipos.ToLista<int>();

            if (lista.Count == 0 || ( lista.Count == 1 && lista[0] == 0))
            { 
                traza();
                return new List<ExpedienteDtm>();
            }

            var estados = enumNegocio.Expediente.Estados(contexto);
            var vivos = estados.Where(e => !e.Cancelado).Select(e => e.Id).ToList();
            var expedientes = contexto.Set<ExpedienteDtm>().Where(x => lista.Contains(x.IdTipo) && vivos.Contains(x.IdEstado) && x.IdResponsable != null).ToList();
            return expedientes;
        }
    }
}
