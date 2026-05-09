using System;
using System.Collections.Generic;
using System.Linq;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.RegistroEs;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.Terceros;
using Inicializador.Procesos;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using SistemaDeElementos.Inicializador;
using SistemaDeElementos.Inicializador.Acromur;
using SistemaDeElementos.Inicializador.AytoBeniel;
using Utilidades;
using ValidacionesBase;

namespace ValidacionesDeRn
{
    class TestConRegistrosEs
    {
        //ContextoSe contexto;
        //[SetUp]
        //public void Setup()
        //{
        //    contexto = Inicializaciones.CrearContexto();
        //}
        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void CrearEstadoDelRegistro()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                try
                {
                    ApiDeTest.CrearEstado(contexto, enumNegocio.Registro, "pendiente", true, false, false);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void CrearTransicionDelRegistro()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                try
                {
                    var e1 = ApiDeTest.CrearEstado(contexto, enumNegocio.Registro, "pendiente", true, false, false);
                    var e2 = ApiDeTest.CrearEstado(contexto, enumNegocio.Registro, "terminado", false, true, false);
                    var t1 = ApiDeTest.CrearTransicion(contexto, enumNegocio.Registro, e1, "pasar a", e2);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void CrearUnRegistroEs()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                var rEs = CrearRegistro(contexto, InzMaestrosBeniel.n_sociedad_beniel_cf, InzRegistroEs.n_tipo_ree, InzAcromur.n_nif_acromur, "prueba de una entrada", "el que la sigue la consigue");
                TestearArchivosVinculados(contexto, rEs);
                TestearBorrarVinculos(contexto, rEs);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void ProcesoDeUnRegistroEs_caso1()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                contexto.IniciarTraza(nameof(DefinirFlujoRee));
                //DefinirFlujoRee(contexto);
                var cg = enumNegocio.CentroGestor.SeleccionarPorPropiedad<CentroGestorDtm>(contexto, nameof(INombre.Nombre), "Ayto de Beniel");

                contexto.IniciarTraza(nameof(CrearRegistro));
                var registroEs = CrearRegistro(contexto, cg.Codigo, InzRegistroEs.n_tipo_ree, InzAcromur.n_nif_acromur, "prueba de una entrada", "el que la sigue la consigue");

                contexto.IniciarTraza("Asignar registro a Atc");
                registroEs = registroEs.Transitar(contexto, InzRegistroEs.n_tran_ree_asignar_atc);

                var tareas = registroEs.Vinculados<TareaDtm>(contexto);
                contexto.IniciarTraza("Iniciar las tareas de Atc");
                foreach (var tarea in tareas)
                    tarea.Transitar(contexto, InzTareasRre.n_tran_trr_iniciar);
                var tareaUno = tareas[0].Recargar(contexto);

                contexto.IniciarTraza("crear una tarea nueva de resolución y enlazarla");
                var tareaDos = TestConTareas.CrearTarea(contexto, tareaUno.Cg.Codigo, InzTareasRre.n_tipo_trr, InzAcromur.n_nif_acromur, "prueba dos tarea", "tarea añadida posterioremente");
                registroEs = registroEs.Vincular(contexto, tareaDos);

                contexto.IniciarTraza("crear una tarea tres de resolución, enlazarla y cancelarla");
                var tareaTres = TestConTareas.CrearTarea(contexto, tareaUno.Cg.Codigo, InzTareasRre.n_tipo_trr, InzAcromur.n_nif_acromur, "prueba tres tarea", "tarea añadida posterioremente");
                registroEs = registroEs.Vincular(contexto, tareaDos);
                tareaTres = tareaTres.Cancelar(contexto);

                contexto.IniciarTraza("iniciar la tarea creada");
                tareaDos = tareaDos.Transitar(contexto, InzTareasRre.n_tran_trr_iniciar);
                registroEs = registroEs.Recargar(contexto, true);

                contexto.IniciarTraza("resolver las dos tareas tarea");
                tareaUno = tareaUno.Transitar(contexto, InzTareasRre.n_tran_trr_resolver);
                tareaDos = tareaDos.Transitar(contexto, InzTareasRre.n_tran_trr_resolver);
                registroEs = registroEs.Recargar(contexto, true);

                contexto.IniciarTraza("dar por resuelto el registro");
                registroEs.Transitar(contexto, InzRegistroEs.n_tran_ree_cerrar_registro);
                registroEs = registroEs.Recargar(contexto, true);

                if (registroEs.Estado.Nombre != InzRegistroEs.n_estado_ree_resuelto)
                    GestorDeErrores.Emitir($"El registro debería estar en estado {InzRegistroEs.n_estado_ree_resuelto} y está en {registroEs.Estado.Nombre} ");

            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }


        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void ProcesoDeUnRegistroEs_Caso2()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                contexto.IniciarTraza(nameof(DefinirFlujoRee));
                DefinirFlujoRee(contexto);
                var cg = enumNegocio.CentroGestor.SeleccionarPorPropiedad<CentroGestorDtm>(contexto, nameof(CentroGestorDtm.Codigo), InzMaestrosBeniel.n_cg_beniel_codigo);

                contexto.IniciarTraza(nameof(CrearRegistro));
                var registroEs = CrearRegistro(contexto, cg.Codigo, InzRegistroEs.n_tipo_ree, InzAcromur.n_nif_acromur, "prueba de una entrada", "el que la sigue la consigue");

                contexto.IniciarTraza("Asignar registro a Atc");
                registroEs = registroEs.Transitar(contexto, InzRegistroEs.n_tran_ree_asignar_atc);

                //var gestorDeCorreos = GestorDeCorreos.Gestor(contexto, contexto.Mapeador);
                //gestorDeCorreos.EnviarCorreoPendientesAsync();

                contexto.IniciarTraza("Validamos que no puede devolver por estar la tarea de resolución creada");
                bool transitado = false;
                try
                {
                    registroEs = registroEs.TransitarAl(contexto, InzRegistroEs.n_estado_ree_pdt_asignar);
                    transitado = true;
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("has de cancelar la tarea"))
                        throw;
                }
                if (transitado) throw new Exception("Debía de no haber permitido transitar");

                contexto.IniciarTraza("cancelar tarea creada");
                var tareaUno = registroEs.Vinculados<TareaDtm>(contexto)[0];
                tareaUno = tareaUno.Cancelar(contexto);

                contexto.IniciarTraza("Devolver el registro a inicial");
                registroEs = registroEs.TransitarAl(contexto, InzRegistroEs.n_estado_ree_pdt_asignar);

                contexto.IniciarTraza("validamos que ha quitado los permisos a ATC");
                var filtrosPorAk = new Dictionary<string, object>();
                filtrosPorAk[nameof(PuestoDtm.IdCg)] = enumNegocio.CentroGestor.SeleccionarPorPropiedad<CentroGestorDtm>(contexto, nameof(CentroGestorDtm.Codigo), InzMaestrosBeniel.n_cg_beniel_codigo_atc).Id;
                filtrosPorAk[nameof(PuestoDtm.Nombre)] = InzSeguridadBeniel.n_pt_Administrativo;
                var administrativoAtc = contexto.SeleccionarPorAk<PuestoDtm>(filtrosPorAk);
                if (administrativoAtc.HayPermisosDirectosDe(contexto, registroEs, enumModoDeAccesoDeDatos.Consultor))
                    throw new Exception($"No debía terner permisos sobre el elemento {registroEs.Referencia}");


            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }



        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void ProcesoDeUnRegistroEs_Caso3()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                CacheDeVariable.Modificar(Variable.CFG_Debugar_Sqls, "N");
                contexto.IniciarTraza(nameof(DefinirFlujoRee));
                DefinirFlujoRee(contexto);

                contexto.IniciarTraza("Crea un registro el usuario de ventanilla");
                contexto.AsignarUsuario(InzUsuariosBeniel.n_usuario_ventanilla);
                var cg = enumNegocio.CentroGestor.SeleccionarPorPropiedad<CentroGestorDtm>(contexto, nameof(CentroGestorDtm.Codigo), InzMaestrosBeniel.n_cg_beniel_codigo);
                var registroEs = CrearRegistro(contexto, cg.Codigo, InzRegistroEs.n_tipo_ree, InzAcromur.n_nif_acromur, "prueba de una entrada", "el que la sigue la consigue");

                contexto.IniciarTraza("Asignar registro a Atc");
                registroEs = registroEs.Transitar(contexto, InzRegistroEs.n_tran_ree_asignar_atc);

                //var gestorDeCorreos = GestorDeCorreos.Gestor(contexto, contexto.Mapeador);
                //gestorDeCorreos.EnviarCorreoPendientesAsync();

                contexto.AsignarUsuario(InzUsuariosBeniel.n_usuario_administrativo);
                contexto.IniciarTraza("Validamos que no puede devolver por estar la tarea de resolución creada");
                bool transitado = false;
                //try
                //{
                //    registroEs = registroEs.TransitarAl(contexto, InzRegistroEs.n_estado_ree_pdt_asignar);
                //    transitado = true;
                //}
                //catch (Exception ex)
                //{
                //    if (!ex.Message.Contains("has de cancelar la tarea"))
                //        throw;
                //}
                //if (transitado) throw new Exception("Debía de no haber permitido transitar");

                contexto.IniciarTraza("cancelar tarea creada");
                var tareaUno = registroEs.Vinculados<TareaDtm>(contexto)[0];
                tareaUno = tareaUno.Cancelar(contexto);

                contexto.IniciarTraza("Devolver el registro a inicial");
                registroEs = registroEs.TransitarAl(contexto, InzRegistroEs.n_estado_ree_pdt_asignar);

                contexto.AsignarUsuario(ContextoSe.Login_Admin);
                contexto.IniciarTraza("validamos que ha quitado los permisos a ATC");
                var filtrosPorAk = new Dictionary<string, object>();
                filtrosPorAk[nameof(PuestoDtm.IdCg)] = enumNegocio.CentroGestor.SeleccionarPorPropiedad<CentroGestorDtm>(contexto, nameof(CentroGestorDtm.Codigo), InzMaestrosBeniel.n_cg_beniel_codigo_atc).Id;
                filtrosPorAk[nameof(PuestoDtm.Nombre)] = InzSeguridadBeniel.n_pt_Administrativo;
                var administrativoAtc = contexto.SeleccionarPorAk<PuestoDtm>(filtrosPorAk);
                //if (administrativoAtc.HayPermisosDirectosDe(contexto, registroEs, enumModoDeAccesoDeDatos.Consultor))
                //    throw new Exception($"No debía terner permisos sobre el elemento {registroEs.Referencia}");

                contexto.AsignarUsuario(InzUsuariosBeniel.n_usuario_ventanilla);
                contexto.IniciarTraza("Asignar registro a Atc");
                registroEs = registroEs.Transitar(contexto, InzRegistroEs.n_tran_ree_asignar_atc);

                contexto.AsignarUsuario(InzUsuariosBeniel.n_usuario_jefe_atc);
                contexto.IniciarTraza("crear una tarea nueva de resolución y enlazarla");
                var tareaDos = TestConTareas.CrearTarea(contexto, tareaUno.Cg.Codigo, InzTareasRre.n_tipo_trr, InzAcromur.n_nif_acromur, "prueba dos tarea", "tarea añadida posterioremente");
                registroEs = registroEs.Vincular(contexto, tareaDos);

                contexto.IniciarTraza("crear una tarea tres de resolución, enlazarla y cancelarla");
                var tareaTres = TestConTareas.CrearTarea(contexto, tareaUno.Cg.Codigo, InzTareasRre.n_tipo_trr, InzAcromur.n_nif_acromur, "prueba tres tarea", "tarea añadida posterioremente");
                registroEs = registroEs.Vincular(contexto, tareaDos);
                tareaTres = tareaTres.Cancelar(contexto);

                contexto.AsignarUsuario(InzUsuariosBeniel.n_usuario_administrativo);
                contexto.IniciarTraza("resolver las dos tareas tarea e intentar modificar la tercera");
                transitado = false;
                try
                {
                    tareaUno = tareaUno.Transitar(contexto, InzTareasRre.n_tran_trr_resolver);
                    transitado = true;
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("No hay parametrizada una transición llamada"))
                        throw;
                }
                if (transitado) throw new Exception("Se ha resuelto una tarea cancelada, y eso no es posible");


                tareaDos = tareaDos.Transitar(contexto, InzTareasRre.n_tran_trr_iniciar);
                tareaDos = tareaDos.Transitar(contexto, InzTareasRre.n_tran_trr_resolver);
                bool modificada = false;
                try
                {
                    tareaTres = tareaTres.Modificar(contexto);
                    modificada = true;
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("no es modificable"))
                        throw;
                }
                if (modificada) throw new Exception("Ha podido modificar una tarea y no está permitido");

                registroEs = registroEs.Recargar(contexto, true);
                var tareas = registroEs.Vinculados<TareaDtm>(contexto, new Dictionary<string, object> { { ltrParametrosNeg.AplicarJoin, true } });
                foreach (var tarea in tareas)
                {
                    if (tarea.Estado.Cancelado || tarea.Estado.Terminado)
                        continue;
                    if (tarea.Estado.Inicial)
                        tarea.Transitar(contexto, InzTareasRre.n_tran_trr_iniciar)
                             .Transitar(contexto, InzTareasRre.n_tran_trr_resolver);
                }

                registroEs = registroEs.Recargar(contexto);

                if (registroEs.Estado.Nombre != InzRegistroEs.n_estado_ree_pdt_respuesta)
                    throw new Exception($"El registro debía estar {InzRegistroEs.n_estado_ree_pdt_respuesta} y está en estado {registroEs.Estado.Nombre}");

                contexto.AsignarUsuario(InzUsuariosBeniel.n_usuario_responsable_ventanilla);
                registroEs.Transitar(contexto, InzRegistroEs.n_tran_ree_cerrar_registro);

            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }


        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DesvinculaUnRegistroDeUnaTareaSoloSiEstaCancelada()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                DefinirFlujoRee(contexto);
                var cg = enumNegocio.CentroGestor.SeleccionarPorPropiedad<CentroGestorDtm>(contexto, nameof(INombre.Nombre), "Ayto de Beniel");

                contexto.IniciarTraza(nameof(CrearRegistro));
                var registroEs = CrearRegistro(contexto, cg.Codigo, InzRegistroEs.n_tipo_ree, InzAcromur.n_nif_acromur, "prueba de una entrada", "el que la sigue la consigue");

                registroEs = registroEs.Transitar(contexto, InzRegistroEs.n_tran_ree_asignar_atc);

                var tareas = registroEs.Vinculados<TareaDtm>(contexto);

                //Solo se puede desvincular si la tarea si está cancelada
                try
                {
                    registroEs.Desvincular(contexto, tareas[0]);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("Para desvincular la tarea"))
                        throw;
                }

                var tarea = tareas[0].Cancelar(contexto);
                registroEs.Desvincular(contexto, tarea, new Dictionary<string, object> { { nameof(ParametrosDeNegocio.ValidarPermisosDePersistencia), false } });
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }



        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DefinirFlujoRee()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                DefinirFlujoRee(contexto);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void IntentarGrabarRegistroTerminado()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                try
                {
                    DefinirFlujoRee(contexto);
                    var cg = enumNegocio.CentroGestor.SeleccionarPorPropiedad<CentroGestorDtm>(contexto, nameof(INombre.Nombre), "Ayto de Beniel");
                    var registroEs = CrearRegistro(contexto, InzMaestrosBeniel.n_sociedad_beniel_cf, InzRegistroEs.n_tipo_ree, InzAcromur.n_nif_acromur, "prueba par no atender", "el que la sigue la consigue");
                    registroEs = registroEs.Transitar(contexto, InzRegistroEs.n_tran_ree_no_atender);
                    registroEs.Modificar(contexto);
                    throw new Exception("No se ha validado correctamente que el registro sea terminado");
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("no es modificable")) throw;
                }
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }


        private RegistroEsDtm CrearRegistro(ContextoSe contexto, string codigoCg, string nombreTipo, string dniSolicitante, string asunto, string descripcion)
        {
            var fCg = new ClausulaDeFiltrado(nameof(CentroGestorDtm.Codigo), enumCriteriosDeFiltrado.igual, codigoCg);
            var cg = GestorDeCentrosGestores.Gestor(contexto, contexto.Mapeador).LeerRegistro(new List<ClausulaDeFiltrado> { fCg }, new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo), true, true);

            var fTipo = new ClausulaDeFiltrado(nameof(TipoDeElementoDtm.Nombre), enumCriteriosDeFiltrado.igual, nombreTipo);
            var tipo = GestorDeTiposDeRegistroEs.Gestor(contexto, contexto.Mapeador).LeerRegistro(new List<ClausulaDeFiltrado> { fTipo }, new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo), true, true);

            var sociedad = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), dniSolicitante);
            var interlocutor = sociedad.CrearInterlocutor(contexto);

            //Cremos el registro y se le han de crear tres archivadore, con tres vínculos
            var registroSe = new RegistroEsDtm();
            registroSe.IdCg = cg.Id;
            registroSe.IdTipo = tipo.Id;
            registroSe.Nombre = asunto;
            registroSe.Descripcion = descripcion;
            registroSe.IdSolicitante = interlocutor.Id;

            return registroSe.Insertar(contexto);
        }

        private RegistroEsDtm AsignarloAtc(ContextoSe contexto, RegistroEsDtm registroEs)
        {
            TestearTareaVinculadaConArchivado(contexto, registroEs);

            TestearQueHaOtorgadoPermisos(contexto, registroEs);

            return registroEs;
        }


        private void TestearQueHaOtorgadoPermisos(ContextoSe contexto, RegistroEsDtm rEs)
        {
            var permiso = PermisosDelElementoSql.LeerPorIdElemento(contexto, enumNegocio.Registro.TablaDePermisos(), enumNegocio.Registro.TipoDtm(), rEs.Id);
            int numero = contexto.Set<PermisosDirectosDtm>().Where(x => x.IdPermiso == permiso.IdConsultor).Count();
            if (numero == 0)
                throw new Exception($"Debería de haberse otorgado permisos directos algun puesto de trabajo");
        }

        private int TestearTareaVinculadaConArchivado(ContextoSe contexto, RegistroEsDtm rEs)
        {
            var tareasDeUnRegistro = contexto.Set<TareasDeUnRegistroDtm>().Where(y => y.idElemento1 == rEs.Id);
            var tareas = NegociosDeSe.ElementosConCg(enumNegocio.Tarea, contexto).Cast<TareaDtm>().ToList();
            var numero = 0;
            foreach (var _ in tareas.Where(tarea => tarea.IdArchivador == rEs.IdArchivadorInterno).Select(tarea => new { }))
                numero = numero + 1;
            if (numero != 1)
                throw new Exception($"Debería de haberse una tarea y hay {numero}");
            return numero;
        }

        private RegistroEsDtm TestearBorrarVinculos(ContextoSe contexto, RegistroEsDtm registroEs)
        {
            try
            {
                GestorDeVinculos.BorrarVinculo(contexto, enumNegocio.Archivador, enumNegocio.Registro, (int)registroEs.IdArchivadorDeEntrada, registroEs.Id, new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("No puede quitar del registro"))
                    throw;
            }

            try
            {
                GestorDeVinculos.BorrarVinculo(contexto, enumNegocio.Registro, enumNegocio.Archivador, registroEs.Id, (int)registroEs.IdArchivadorDeSalida, new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("No puede quitar del registro"))
                    throw;
            }
            registroEs = GestorDeRegistrosEs.LeerRegistroPorId(contexto, registroEs.Id, true, true);

            if (registroEs.Vinculados<ArchivadorDtm>(contexto).Count != 3)
                throw new Exception("Debería de haberse los tres archivadores");

            return registroEs;
        }

        private void TestearArchivosVinculados(ContextoSe contexto, RegistroEsDtm registroEs)
        {

            foreach (var a in registroEs.Vinculados<ArchivadorDtm>(contexto))
            {
                var registros = GestorDeVinculos.RegistrosVinculados<RegistroEsDtm>(contexto, enumNegocio.Archivador, enumNegocio.Registro, a.Id, new Dictionary<string, object>());

                if (registros.Count != 1)
                    throw new Exception("Debería de haberse vinculado un registro");
            }
        }


        private static void DefinirFlujoRee(ContextoSe contexto)
        {
            TestDefinirFlujos.CrearFlujo(contexto);
        }
    }
}
