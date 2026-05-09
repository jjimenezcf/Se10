using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.RegistroEs;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.TrabajosSometidos;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using SistemaDeElementos.Inicializador.AytoBeniel;
using Utilidades;

namespace Inicializador.Procesos
{

    public static class InzRegistroEs
    {
        internal static readonly string n_ree = "REE";

        public static void ModeloDeRegistroEs(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public static readonly string n_estado_ree_pdt_asignar = $"{n_ree}: Pendiente de asignar";
        public static readonly string n_estado_ree_asignado_atc = $"{n_ree}: Asignado a ATC";
        public static readonly string n_estado_ree_asignado_urb = $"{n_ree}: Asignado a URB";
        public static readonly string n_estado_ree_en_resolucion = $"{n_ree}: En resolución";
        public static readonly string n_estado_ree_pdt_respuesta = $"{n_ree}: Pdt de respuesta";
        public static readonly string n_estado_ree_resuelto = $"{n_ree}: Resuelto";
        public static readonly string n_estado_ree_cancelado = $"{n_ree}: Cancelado";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados del registro de entrada de Beniel");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Registro, n_estado_ree_pdt_asignar, true, false, false, 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Registro, n_estado_ree_asignado_atc, false, false, false, 15);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Registro, n_estado_ree_asignado_urb, false, false, false, 15);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Registro, n_estado_ree_en_resolucion, false, false, false, 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Registro, n_estado_ree_pdt_respuesta, false, false, false, 25);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Registro, n_estado_ree_resuelto, false, true, false, 40);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Registro, n_estado_ree_cancelado, false, false, true, 50);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }


        public static readonly string n_tran_ree_asignar_atc = $"{n_ree}: Asignar a ATC";
        public static readonly string n_tran_ree_asignar_urb = $"{n_ree}: Asignar a URB";
        public static readonly string n_tran_ree_no_atender = $"{n_ree}: No atender";
        public static readonly string n_tran_ree_cerrar_registro = $"{n_ree}: Cerrar registro";
        public static readonly string n_tran_ree_cancelar_registro = $"{n_ree}: Cancelar registro";
        public static readonly string n_tran_ree_devolver_pendiente = $"{n_ree}: Devolver a pendiente";
        public static readonly string n_tran_ree_iniciado_por_atc = $"{n_ree}: Iniciado por Atc";
        public static readonly string n_tran_ree_iniciado_por_urb = $"{n_ree}: Iniciado por Urb";
        public static readonly string n_tran_ree_registro_resuelto = $"{n_ree}: Resuelto";
        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones del registro de entrada");
            try
            {
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Registro, n_tran_ree_asignar_atc, n_estado_ree_pdt_asignar, n_estado_ree_asignado_atc, true, false, false, null);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Registro, n_tran_ree_asignar_urb, n_estado_ree_pdt_asignar, n_estado_ree_asignado_urb, true, false, false, null);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Registro, n_tran_ree_no_atender, n_estado_ree_pdt_asignar, n_estado_ree_resuelto, true, false, false, null);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Registro, n_tran_ree_cancelar_registro, n_estado_ree_pdt_asignar, n_estado_ree_cancelado, true, true, false, "Motivo de cancelación");

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Registro, n_tran_ree_iniciado_por_atc, n_estado_ree_asignado_atc, n_estado_ree_en_resolucion, true, false, true, null);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Registro, n_tran_ree_devolver_pendiente, n_estado_ree_asignado_atc, n_estado_ree_pdt_asignar, true, true, false, "Motivo de devolució");

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Registro, n_tran_ree_iniciado_por_urb, n_estado_ree_asignado_urb, n_estado_ree_en_resolucion, true, false, true, null);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Registro, n_tran_ree_devolver_pendiente, n_estado_ree_asignado_urb, n_estado_ree_pdt_asignar, true, true, false, "Motivo de devolució");

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Registro, n_tran_ree_registro_resuelto, n_estado_ree_en_resolucion, n_estado_ree_pdt_respuesta, true, false, true, null);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Registro, n_tran_ree_cerrar_registro, n_estado_ree_pdt_respuesta, n_estado_ree_resuelto, true, false, false, null);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_tipo_ree = $"{n_ree}: General";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos del registro de entrada");
            try
            {
                var estadoInicial = enumNegocio.Registro.Estado(contexto, n_estado_ree_pdt_asignar);
                GestorDeTiposDeRegistroEs.PersistirTipo(contexto, enumClaseDeRegistroEs.E, n_tipo_ree, estadoInicial.Id, enumClaseDeLibro.POR_CG, n_ree);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }


        private static readonly string jsonMensaje = @"[
  {
    ""parametro"": ""IdPuesto"",
    ""valor"": [IDPUESTO]
  },
  {
    ""parametro"": ""Asunto"",
    ""valor"": ""Registro de entrada pendiente""
  },
  {
    ""parametro"": ""Cuerpo"",
    ""valor"": ""Por favor se le ha asignado la tarea de resolución adjunta al registro indicado, realícenla y al finalizar nos lo comunican, gracias.""
  }
]
";

        public static void AccionesAlTransitar(ContextoSe contexto)
        {
            contexto.IniciarTraza("Acciones de las transiciones del registro");
            try
            {
                AsignarATC(contexto);
                AsignarUrb(contexto);
                PasarAPdtRespuesta(contexto);
                DevolverAPendiente(contexto, n_estado_ree_asignado_atc, n_estado_ree_pdt_asignar, InzMaestrosBeniel.n_cg_beniel_codigo_atc, InzSeguridadBeniel.n_pt_Administrativo);
                DevolverAPendiente(contexto, n_estado_ree_asignado_urb, n_estado_ree_pdt_asignar, InzMaestrosBeniel.n_cg_beniel_codigo_urb, InzSeguridadBeniel.n_pt_Tecnico);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }


        private static void DevolverAPendiente(ContextoSe contexto, string origen, string destino, string codigoCg, string nombrePt)
        {
            var transicion = enumNegocio.Registro.Transicion(contexto, origen, destino);

            var filtrosPorAk = new Dictionary<string, object>();
            filtrosPorAk[nameof(PuestoDtm.IdCg)] = enumNegocio.CentroGestor.SeleccionarPorPropiedad<CentroGestorDtm>(contexto, nameof(CentroGestorDtm.Codigo), codigoCg).Id;
            filtrosPorAk[nameof(PuestoDtm.Nombre)] = nombrePt;
            var puestoTrabajo = contexto.SeleccionarPorAk<PuestoDtm>(filtrosPorAk);

            var jsonQuitaPermisoAUnpt = $@"[{{""parametro"": ""IdPuesto"",""valor"": {puestoTrabajo.Id}}},{{""parametro"": ""Negocio"",""valor"": ""Registro""}}]";

            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Registro, transicion, AccionesDelRegistro.N_ValidarTareasCanceladas, enumMomentoDeEjecucion.A, null, 10, AccionesDelRegistro.D_ValidarTareasCanceladas);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Registro, transicion, AccionesDeSeguridad.N_QuitarPermisosDeConsultorAlPt, enumMomentoDeEjecucion.A, jsonQuitaPermisoAUnpt, 15, "Quita los permisos otorgados");
        }


        private static void PasarAPdtRespuesta(ContextoSe contexto)
        {
            var transicion = enumNegocio.Registro.Transicion(contexto,n_estado_ree_en_resolucion, n_estado_ree_pdt_respuesta);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Registro, transicion, AccionesDelRegistro.N_ValidarTareasNoEnCircuito, enumMomentoDeEjecucion.A, null, 10, "Valida que las tareas relacionadas o están canceladas o terminadas");
        }

        private static void AsignarATC(ContextoSe contexto)
        {
            var transicion = enumNegocio.Registro.Transicion(contexto, n_tran_ree_asignar_atc);

            var tipoTrr = contexto.SeleccionarPorPropiedad<TipoDeTareaDtm>(nameof(TipoDeTareaDtm.Nombre), InzTareasRre.n_tipo_trr);
            var sociedad = enumNegocio.Sociedad.SeleccionarPorPropiedad<SociedadDtm>(contexto, nameof(SociedadDtm.NIF), InzMaestrosBeniel.n_nif_beniel);
            var f1 = new ClausulaDeFiltrado(nameof(CentroGestorDtm.IdSociedad), enumCriteriosDeFiltrado.igual, sociedad.Id);
            var f2 = new ClausulaDeFiltrado(nameof(CentroGestorDtm.Nombre), enumCriteriosDeFiltrado.igual, InzMaestrosBeniel.n_cg_beniel_nombre_atc);
            var cgs = enumNegocio.CentroGestor.SeleccionarPorFiltro<CentroGestorDtm>(contexto, new List<ClausulaDeFiltrado> { f1, f2 });

            var filtrosPorAk = new Dictionary<string, object>();
            filtrosPorAk[nameof(PuestoDtm.IdCg)] = enumNegocio.CentroGestor.SeleccionarPorPropiedad<CentroGestorDtm>(contexto, nameof(CentroGestorDtm.Codigo), InzMaestrosBeniel.n_cg_beniel_codigo_atc).Id;
            filtrosPorAk[nameof(PuestoDtm.Nombre)] = InzSeguridadBeniel.n_pt_Administrativo;
            var administrativoDeAtc = contexto.SeleccionarPorAk<PuestoDtm>(filtrosPorAk);

            var jsonCrearTarea = $@"[{{""parametro"": ""IdCg"",""valor"": {cgs[0].Id}}},{{""parametro"": ""IdTipo"",""valor"": {tipoTrr.Id}}}]";
            var jsonOtorgarPermisoAUnpt = $@"[{{""parametro"": ""IdPuesto"",""valor"": {administrativoDeAtc.Id}}},{{""parametro"": ""Negocio"",""valor"": ""Registro""}}]";
            var jsonMensajAlAdministrativoAtc = jsonMensaje.Replace("[IDPUESTO]", administrativoDeAtc.Id.ToString());

            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Registro, transicion, AccionesDeSeguridad.N_OtorgarPermisosDeConsultorAlPt, enumMomentoDeEjecucion.A, jsonOtorgarPermisoAUnpt, 10, "Otorga al Pt permisos de consultor");
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Registro, transicion, AccionesDelRegistro.N_CrearTareaDeRegistroEs, enumMomentoDeEjecucion.D, jsonCrearTarea, 10, "Crea una tarea de resolución de registro");
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Registro, transicion, AccionesDeTs.N_CrearCorreoParaPt, enumMomentoDeEjecucion.D, jsonMensajAlAdministrativoAtc, 90, "Envía un mensaje a los usuarios de un puesto");
        }


        private static void AsignarUrb(ContextoSe contexto)
        {
            var transicion = enumNegocio.Registro.Transicion(contexto, n_tran_ree_asignar_urb);

            var tipoTrr = contexto.SeleccionarPorPropiedad<TipoDeTareaDtm>(nameof(TipoDeTareaDtm.Nombre), InzTareasRre.n_tipo_trr);
            var sociedad = enumNegocio.Sociedad.SeleccionarPorPropiedad<SociedadDtm>(contexto, nameof(SociedadDtm.NIF), InzMaestrosBeniel.n_nif_beniel);
            var f1 = new ClausulaDeFiltrado(nameof(CentroGestorDtm.IdSociedad), enumCriteriosDeFiltrado.igual, sociedad.Id);
            var f2 = new ClausulaDeFiltrado(nameof(CentroGestorDtm.Nombre), enumCriteriosDeFiltrado.igual, InzMaestrosBeniel.n_cg_beniel_nombre_urb);
            var cgs = enumNegocio.CentroGestor.SeleccionarPorFiltro<CentroGestorDtm>(contexto, new List<ClausulaDeFiltrado> { f1, f2 });


            var jsonCrearTarea = $@"[{{""parametro"": ""IdCg"",""valor"": {cgs[0].Id}}},{{""parametro"": ""IdTipo"",""valor"": {tipoTrr.Id}}}]";

            var filtrosPorAk = new Dictionary<string, object>();
            filtrosPorAk[nameof(PuestoDtm.IdCg)] = enumNegocio.CentroGestor.SeleccionarPorPropiedad<CentroGestorDtm>(contexto, nameof(CentroGestorDtm.Codigo), InzMaestrosBeniel.n_cg_beniel_codigo_urb).Id;
            filtrosPorAk[nameof(PuestoDtm.Nombre)] = InzSeguridadBeniel.n_pt_Tecnico;
            var tecnicoDeUrbanismo = contexto.SeleccionarPorAk<PuestoDtm>(filtrosPorAk);

            var jsonOtorgarPermisoAUnpt = $@"[{{""parametro"": ""IdPuesto"",""valor"": {tecnicoDeUrbanismo.Id}}},{{""parametro"": ""Negocio"",""valor"": ""Registro""}}]";
            var jsonMensajAlTecnico = jsonMensaje.Replace("[IDPUESTO]", tecnicoDeUrbanismo.Id.ToString());

            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Registro, transicion, AccionesDeSeguridad.N_OtorgarPermisosDeConsultorAlPt, enumMomentoDeEjecucion.A, jsonOtorgarPermisoAUnpt, 10, "Otorga al Pt permisos de consultor");
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Registro, transicion, AccionesDelRegistro.N_CrearTareaDeRegistroEs, enumMomentoDeEjecucion.D, jsonCrearTarea, 10, "Crea una tarea de resolución de registro");
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Registro, transicion, AccionesDeTs.N_CrearCorreoParaPt, enumMomentoDeEjecucion.D, jsonMensajAlTecnico, 90, "Envía un mensaje a los usuarios de un puesto");
        }

    }
}


