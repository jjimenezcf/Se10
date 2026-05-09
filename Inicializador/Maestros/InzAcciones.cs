using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Administracion;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Expediente;
using GestoresDeNegocio.Gastos;
using GestoresDeNegocio.Juridico;
using GestoresDeNegocio.RegistroEs;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Tarea;
using GestoresDeNegocio.TrabajosSometidos;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using Utilidades;

namespace SistemaDeElementos.Inicializador
{
    public static class InzAcciones
    {
        public static void DefinirExportaciones(ContextoSe contexto)
        {
            using (var gestor = GestorDeAcciones.Gestor(contexto, contexto.Mapeador))
            {
                gestor.Contexto.DatosDeConexion.CreandoModelo = true;
                gestor.Contexto.IniciarTraza(nameof(DefinirAcciones));
                var tran = gestor.Contexto.IniciarTransaccion();
                try
                {
                    //CrearExportacionesDeRemesas(gestor);
                    CrearExportacionesDeFactura(gestor);
                    //CrearExportacionesDeExpedientes(gestor);
                    gestor.Contexto.Commit(tran);
                }
                catch
                {
                    gestor.Contexto.Rollback(tran);
                    throw;
                }
                finally
                {
                    gestor.Contexto.CerrarTraza();
                    gestor.Contexto.DatosDeConexion.CreandoModelo = false;
                }
            }
        }


        public static void DefinirAcciones(ContextoSe contexto)
        {
            using (var gestor = GestorDeAcciones.Gestor(contexto, contexto.Mapeador))
            {
                gestor.Contexto.DatosDeConexion.CreandoModelo = true;
                gestor.Contexto.IniciarTraza(nameof(DefinirAcciones));
                var tran = gestor.Contexto.IniciarTransaccion();
                try
                {
                    CrearAccionesDeTs(gestor);
                    CrearAccionesDeSeguridad(gestor);
                    CrearAccionesDeRegistro(gestor);
                    CrearAccionesDeTareas(gestor);
                    CrearAccionesDeContratos(gestor);
                    CrearAccionesDeSolicitudesDeContratos(gestor);
                    CrearAccionesDeArchivadores(gestor);
                    gestor.Contexto.Commit(tran);
                }
                catch
                {
                    gestor.Contexto.Rollback(tran);
                    throw;
                }
                finally
                {
                    gestor.Contexto.CerrarTraza();
                    gestor.Contexto.DatosDeConexion.CreandoModelo = false;
                }
            }
        }

        private static void CrearAccionesDeRegistro(GestorDeAcciones gestor)
        {
            var a = new AccionDtm();
            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.RegistroEs)}.{nameof(AccionesDelRegistro)}";

            #region Crear tarea de registro
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDelRegistro.N_CrearTareaDeRegistroEs;
            a.Descripcion = @"Crear y vincula una tarea para la resolución del registro
Parámetros:
[
   {{
     ""parametro"": ""IdCg"",
     ""valor"": x
   }},
   {{
     ""parametro"": ""IdTipo"",
     ""valor"": y
   }}
]";
            a.Metodo = nameof(AccionesDelRegistro.CrearTareaDeRegistroEs);
            PersistirAccion(gestor, a);
            #endregion

            #region Validar que no hay tareas en circuiro
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDelRegistro.N_ValidarTareasNoEnCircuito;
            a.Descripcion = @"Valida que no hay tareas vinculadas en circuito. No hay parámetros";
            a.Metodo = nameof(AccionesDelRegistro.ValidarTareasNoEnCircuito);
            PersistirAccion(gestor, a);
            #endregion

            #region Validar que las tareas están canceladas
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDelRegistro.N_ValidarTareasCanceladas;
            a.Descripcion = AccionesDelRegistro.D_ValidarTareasCanceladas;
            a.Metodo = nameof(AccionesDelRegistro.ValidarTareasCanceladas);
            PersistirAccion(gestor, a);
            #endregion

            #region Vincular archivos del registro con el objeto indicado
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDelRegistro.N_VincularArchivadores;
            a.Descripcion = @"Vincula los archivadores del registro con el objeto pasado. No hay parámetros";
            a.Metodo = nameof(AccionesDelRegistro.VincularArchivadores);
            PersistirAccion(gestor, a);
            #endregion

            #region Desvincular archivos del registro con el objeto indicado
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDelRegistro.N_DesvincularArchivadores;
            a.Descripcion = @"Desvincula los archivadores del registro con el objeto pasado. No hay parámetros";
            a.Metodo = nameof(AccionesDelRegistro.DesvincularArchivadores);
            PersistirAccion(gestor, a);
            #endregion

            #region Validar al desvincular una tarea de un registro que está cancelada
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDelRegistro.N_ValidarDesvincularUnaTareaDeUnRegistro;
            a.Descripcion = AccionesDelRegistro.D_ValidarDesvincularUnaTareaDeUnRegistro;
            a.Metodo = nameof(AccionesDelRegistro.ValidarDesvincularUnaTareaDeUnRegistro);
            PersistirAccion(gestor, a);
            #endregion
        }

        private static void CrearAccionesDeTareas(GestorDeAcciones gestor)
        {
            var a = new AccionDtm();
            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.Tarea)}.{nameof(AccionesDeTareas)}";

            #region Al cerrar todas las tareas de un RegistroEs
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDeTareas.N_AlCerraTodasLasTareaDelRegistroEs;
            a.Descripcion = @"Al cerrar todas las tareas vinculadas a un registro, aplicar la transición
Parámetros:
[
   {{
     ""parametro"": ""IdsDeTransiciones"",
     ""valor"": int
   }}
]";
            a.Metodo = nameof(AccionesDeTareas.AlCerraTodasLasTareaDelRegistroEs);
            PersistirAccion(gestor, a);
            #endregion

            #region Al cancelar todas las tareas de un RegistroEs
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDeTareas.N_AlCancelarTodasLasTareasDelRegistroEs;
            a.Descripcion = @"Al cancelar todas las tareas vinculadas a un registro, aplicar la transición
Parámetros:
[
   {{
     ""parametro"": ""IdsDeTransiciones"",
     ""valor"": List<int>
   }}
]";
            a.Metodo = nameof(AccionesDeTareas.AlCancelarTodasLasTareasDelRegistroEs);
            PersistirAccion(gestor, a);
            #endregion

            #region Al iniciar una de las tareas de un RegistroEs
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDeTareas.N_AlIniciarUnaTareaDelRegistroEs;
            a.Descripcion = @"Al iniciar una de las tareas vinculadas a un registro, aplicar la transición
Parámetros:
[
   {{
     ""parametro"": ""IdsDeTransiciones"",
     ""valor"": int
   }}
]";
            a.Metodo = nameof(AccionesDeTareas.AlIniciarUnaTareaDelRegistroEs);
            PersistirAccion(gestor, a);
            #endregion
        }

        private static void CrearAccionesDeTs(GestorDeAcciones gestor)
        {
            var a = new AccionDtm();
            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.TrabajosSometidos)}.{nameof(AccionesDeTs)}";
            #region Enviar mensaje a un PT
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDeTs.N_CrearCorreoParaPt;
            a.Descripcion = $@"Envía un mensaje a un Puesto de trabajo con los siguientes parámetros
[
  {{
    'parametro': 'IdPuesto',
    'valor': x
  }},
  {{
    'parametro': 'Asunto',
    'valor':'Registro de entrada pendiente'
  }},
  {{
    'parametro': 'Cuerpo',
    'valor':'Por favor se le ha asignado la tarea de resolución adjunta al registro indicado, realícenla y al finalizar nos lo comunican, gracias.'
  }}
]";
            a.Metodo = nameof(AccionesDeTs.CrearCorreoParaPt);
            PersistirAccion(gestor, a);
            #endregion
        }

        private static void CrearAccionesDeSeguridad(GestorDeAcciones gestor)
        {
            var a = new AccionDtm();
            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.Seguridad)}.{nameof(AccionesDeSeguridad)}";

            #region Otorgar permiso de consultor
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDeSeguridad.N_OtorgarPermisosDeConsultorAlPt;
            a.Descripcion = $@"Otorga permiso de consulta al id del elemento del negocio (enumerado) pasado
[
  {{
    'parametro': 'IdPuesto',
    'valor': int
  }},
  {{
    'parametro': 'Negocio',
    'valor':'enumNegocio'
  }}
]";
            a.Metodo = nameof(AccionesDeSeguridad.OtorgarPermisosDeConsultorAlPt);
            PersistirAccion(gestor, a);
            #endregion

            #region Otorgar permiso de gestor
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDeSeguridad.N_OtorgarPermisosDeGestorAlPt;
            a.Descripcion = $@"Otorga permiso de gestión al id del elemento del negocio (enumerado) pasado
[
  {{
    'parametro': 'IdPuesto',
    'valor': int
  }},
  {{
    'parametro': 'Negocio',
    'valor':'enumNegocio'
  }}
]";
            a.Metodo = nameof(AccionesDeSeguridad.OtorgarPermisosDeGestorAlPt);
            PersistirAccion(gestor, a);
            #endregion

            #region Quitar permiso de consultor
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDeSeguridad.N_QuitarPermisosDeConsultorAlPt;
            a.Descripcion = $@"Quitar permiso de consulta al id del elemento del negocio (enumerado) pasado
[
  {{
    'parametro': 'IdPuesto',
    'valor': int
  }},
  {{
    'parametro': 'Negocio',
    'valor':'enumNegocio'
  }}
]";
            a.Metodo = nameof(AccionesDeSeguridad.QuitarPermisosDeConsultorAlPt);
            PersistirAccion(gestor, a);
            #endregion

            #region Quitar permiso de gestor
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDeSeguridad.N_QuitarPermisosDeGestorAlPt;
            a.Descripcion = $@"Quita los permiso de gestión al id del elemento del negocio (enumerado) pasado
[
  {{
    'parametro': 'IdPuesto',
    'valor': int
  }},
  {{
    'parametro': 'Negocio',
    'valor':'enumNegocio'
  }}
]";
            a.Metodo = nameof(AccionesDeSeguridad.QuitarPermisosDeGestorAlPt);
            PersistirAccion(gestor, a);
            #endregion

        }

        internal static void CrearAccionesDeContratos(GestorDeAcciones gestor)
        {
            var a = new AccionDtm();
            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.Juridico)}.{nameof(AccionesDeContratos)}";

            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();

            #region Transitar solicitud si todos sus contratos están cancelados
            a.Nombre = AccionesDeContratos.N_TransitarSolicitudSiTodosSusContratosVntCancelados;
            a.Descripcion = "Transita la solicitud asociado al contrato si todos los contratos de dicha solicitud están cancelados";
            a.Metodo = nameof(AccionesDeContratos.TransitarSolicitudSiTodosSusContratosVntCancelados);
            PersistirAccion(gestor, a);
            #endregion

            #region Validar fecha de inicio mayor que hoy
            a.Nombre = AccionesDeContratos.N_ValidarFechaInicioMayorQueHoy;
            a.Descripcion = "Valida que la fecha de inicio de contrato es mayor a hoy";
            a.Metodo = nameof(AccionesDeContratos.ValidarFechaInicioMayorQueHoy);
            PersistirAccion(gestor, a);
            #endregion

            #region Validar que no hay planificaciones asociadas al contrato
            a.Nombre = AccionesDeContratos.N_ValidarQueNoHayPlanificacionesDeVenta;
            a.Descripcion = AccionesDeContratos.N_ValidarQueNoHayPlanificacionesDeVenta;
            a.Metodo = nameof(AccionesDeContratos.ValidarQueNoHayPlanificacionesDeVenta);
            PersistirAccion(gestor, a);
            #endregion
        }

        private static void CrearAccionesDeSolicitudesDeContratos(GestorDeAcciones gestor)
        {
            var a = new AccionDtm();
            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.Expediente)}.{nameof(AccionesDeSolicitudes)}";

            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDeSolicitudes.N_DesasociarContratosDeLaSolicitud;
            a.Descripcion = "Desasocia los contratos asociados a la solicitud, si hay alguno vigente o terminado, lo impide";
            a.Metodo = nameof(AccionesDeSolicitudes.DesasociarContratosDeLaSolicitud);
            PersistirAccion(gestor, a);

            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDeSolicitudes.N_ValidarQueNoHayaContratos;
            a.Descripcion = "Valida que la solicitud de contratos no tiene ningún contratos asociados";
            a.Metodo = nameof(AccionesDeSolicitudes.ValidarQueNoHayaContratos);
            PersistirAccion(gestor, a);

            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDeSolicitudes.N_ValidarQueSiHayContratosEstanCancelados;
            a.Descripcion = "Valida que si la solicitud tiene algún contrato, éste está cancelado";
            a.Metodo = nameof(AccionesDeSolicitudes.ValidarQueSiHayContratosEstanCancelados);
            PersistirAccion(gestor, a);

            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDeSolicitudes.N_ValidarQueHayAlMenosUnContratoAsociadoNoCancelado;
            a.Descripcion = "Valida que la solicitud tenga almenos un contratos asociado y no esté cancelado";
            a.Metodo = nameof(AccionesDeSolicitudes.ValidarQueHayAlMenosUnContratoAsociadoNoCancelado);
            PersistirAccion(gestor, a);
        }

        private static void CrearExportacionesDeRemesas(GestorDeAcciones gestor)
        {
            var a = new AccionDtm();
            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.Gastos)}.{nameof(ExportacionesDeRemesas)}";
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = ExportacionesDeRemesas.N_RemesasConSusPagos;
            a.Descripcion = @"Crear un excel con una hoja por remesa donde se muestren los pagos";
            a.Metodo = nameof(ExportacionesDeRemesas.RemesasConSusPagos);
            PersistirAccion(gestor, a);
        }

        private static void CrearExportacionesDeFactura(GestorDeAcciones gestor)
        {
            var a = new PlantillaDeExportacionDtm();
            a.IdNegocio = enumNegocio.FacturaRecibida.IdNegocio();
            a.Nombre = ExportacionesDeFacturasRec.N_EnviarAContabilizar.ToString();
            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.Gastos)}.{nameof(ExportacionesDeFacturasRec)}";
            a.Metodo = nameof(ExportacionesDeFacturasRec.EnviarAContabilizar);
            a.InsertarSiNoExiste(gestor.Contexto, new List<string> { nameof(PlantillaDeExportacionDtm.Dll), nameof(PlantillaDeExportacionDtm.Clase) , nameof(PlantillaDeExportacionDtm.Metodo) });

            //var b = new PlantillaDeExportacionDtm();
            //b.IdNegocio = enumNegocio.FacturaRecibida.IdNegocio();
            //b.Nombre = ExportacionesDeFacturasRec.N_ReporteParaContabilidad.ToString();
            //b.Dll = $"{nameof(GestoresDeNegocio)}";
            //b.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.Gastos)}.{nameof(ExportacionesDeFacturasRec)}";
            //b.Metodo = nameof(ExportacionesDeFacturasRec.ReporteParaContabilidad);
            //b.InsertarSiNoExiste(gestor.Contexto, new List<string> { nameof(PlantillaDeExportacionDtm.Dll), nameof(PlantillaDeExportacionDtm.Clase), nameof(PlantillaDeExportacionDtm.Metodo) });

        }


        private static void CrearExportacionesDeExpedientes(GestorDeAcciones gestor)
        {
            var a = new AccionDtm();
            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.Administracion)}.{nameof(ExportacionDeTareasRealizadas)}";
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = ExportacionDeTareasRealizadas.N_TareasRealizadas;
            a.Descripcion = @"Crear un excel con una hoja por expediente donde se muestren las tareas y horas realizadas";
            a.Metodo = nameof(ExportacionDeTareasRealizadas.ExpedientesConSusTareas);
            PersistirAccion(gestor, a);
        }

        internal static void CrearAccionesDeArchivadores(GestorDeAcciones gestor)
        {
            var a = new AccionDtm();
            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.SistemaDocumental)}.{nameof(AccionesDeArchivadores)}";

            #region Dar de baja los archivadores
            a.ClaseDeAccion = enumClaseDeAccion.DLL.ToString();
            a.Nombre = AccionesDeArchivadores.N_DarDeBajaArchivadores;
            a.Descripcion = $@"Da de baja los archivadores vinculados a un proceso";
            a.Metodo = nameof(AccionesDeArchivadores.DarDeBajaArchivadores);
            PersistirAccion(gestor, a);
            #endregion
        }

        private static void PersistirAccion(GestorDeAcciones gestor, AccionDtm accion) => accion.PersistirAccion(gestor.Contexto);


    }
}
