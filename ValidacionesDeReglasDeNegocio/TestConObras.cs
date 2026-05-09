using Inicializador.Expedientes;
using Inicializador.Presupuestos;
using MVCSistemaDeElementos.Controllers;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Terceros;
using SistemaDeElementos.Inicializador.Acromur;
using ValidacionesBase;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Utilidades;
using GestoresDeNegocio.Callejero;
using System.Collections.Generic;
using System;
using Inicializador.Procesos;
using Inicializador.Ventas;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Ventas;
using ModeloDeDto.MaestrosTecnico;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.MaestrosTecnico;
using SistemaDeElementos.Inicializador.Datos;
using ModeloDeDto.Terceros;

namespace ValidacionesDeRn
{
    class TestConObras
    {
        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DefinirFlujoDeTalleres()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InicializadorController.InzProcesosDeExpedientes(contexto);
            }
            //ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DefinirFlujoDeProcesosJudiciales()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InicializadorController.InzProcedimientosJudiciales(contexto);
            }
            //ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DefinirFlujoDeObras()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                TestConMaestros.CrearMaestros(contexto);
                InicializadorController.InzProcesosDeExpedientes(contexto);
                InzPresupuestos.ModeloDePresupuestos(contexto);
                InzPartesTr.ModeloDePartesTr(contexto);
                TestConMaestros.CrearUnitariosEjemplos(contexto);
                var expediente = CrearExpedienteDeObra(contexto);
                var direccion = expediente.Direccion(contexto, enumCalificadorDireccion.contacto);
                //expediente.QuitarDireccion(contexto, direccion);
                //IntentarIniciarExp(contexto, expediente);
                //direccion.Id = 0;
                //expediente.AsociarDireccion(contexto, direccion);
                IntentarIniciarExp(contexto, expediente);

                var tarea = expediente.CrearTarea(contexto, InzTareasPlf.n_tipo_plf);
                IntentarCancelarExp(contexto, expediente);
                tarea.Cancelar(contexto);

                var presupuesto = expediente.CrearPresupuesto(contexto, InzPresupuestos.n_tipo_venta);
                IntentarCancelarExp(contexto, expediente);
                presupuesto.Cancelar(contexto);

                presupuesto = expediente.CrearPresupuesto(contexto, InzPresupuestos.n_tipo_venta);

                /* El expediente ya está iniciado porque lee quitado la validación de que antes de iniciar un expediente de obra tenga un ppt asociado
                //expediente = expediente.TransitarAl(contexto, InzExpeditesDeObra.n_estado_exp_obr_iniciado);
                */

                tarea = expediente.CrearTarea(contexto, InzTareasPlf.n_tipo_plf);
                IntentarCerrarExp(contexto, expediente);

                tarea.IdResponsable = contexto.DatosDeConexion.IdUsuario;
                tarea.Modificar(contexto);
                tarea.Planificar(contexto, DateTime.Now, DateTime.Now.AddDays(1));
                tarea.Transitar(contexto, InzTareasPlf.n_tran_plf_asignar);
                IntentarCerrarExp(contexto, expediente);

                tarea.Transitar(contexto, InzTareasPlf.n_tran_plf_iniciar);
                tarea.Transitar(contexto, InzTareasPlf.n_tran_plf_finalizar);
                tarea.Transitar(contexto, InzTareasPlf.n_tran_plf_terminar);
                IntentarCerrarExp(contexto, expediente);

                presupuesto.Transitar(contexto, InzPresupuestos.n_tran_cancelar);
                //IntentarReabrirExp(contexto, expediente);

                presupuesto = CrearPresupuesto(contexto, expediente);
                presupuesto.Transitar(contexto, InzPresupuestos.n_tran_entregar);
                presupuesto.Transitar(contexto, InzPresupuestos.n_tran_iniciar);
                IntentarCerrarExp(contexto, expediente);
                presupuesto.Transitar(contexto, InzPresupuestos.n_tran_no_ejecutar);

                var tipoPpt = contexto.SeleccionarPorNombre<TipoDePresupuestoDtm>(InzPresupuestos.n_tipo_venta);
                tipoPpt.IdTipoParteTr = contexto.SeleccionarPorNombre<TipoDeParteTrDtm>(InzPartesTr.n_ptr_tipo_general).Id;
                tipoPpt.Modificar(contexto);

                presupuesto = CrearPresupuesto(contexto, expediente);
                presupuesto.Transitar(contexto, InzPresupuestos.n_tran_entregar);
                presupuesto.Transitar(contexto, InzPresupuestos.n_tran_iniciar);
                var parte = presupuesto.CrearParteTr(contexto);
                IntentarCancelarEjecucion(contexto, presupuesto);
                parte.Cancelar(contexto);
                presupuesto.Transitar(contexto, InzPresupuestos.n_tran_no_ejecutar);

                CrearPptYParte(contexto, expediente, out presupuesto, out parte);
                IntentarCerrarExp(contexto, expediente);
                tarea = parte.CrearTarea(contexto, InzTareasPlf.n_tipo_plf);
                IntentarDarPorEjecutado(contexto, parte);
                tarea.Cancelar(contexto);
                var trabajador = TestConTerceros.CrearTrabajador(contexto, parte.Cg(contexto),idUsuario: null, "34798737J", "Elvira", "Fuster Marquina", "jjimenezcf@gmail.com", "600627237");
                var asignacion = parte.Asignalo(contexto, trabajador, DateTime.Now, DateTime.Now.AddDays(1));
                IntentarDarPorEjecutado(contexto, parte);
                asignacion.DarPorRealizada(contexto);
                parte.Transitar(contexto, InzPartesTr.n_tran_ptr_gen_realizar);
                presupuesto.Transitar(contexto, InzPresupuestos.n_tran_cerrar);
                expediente.Transitar(contexto, InzExpeditesDeObra.n_tran_exp_obr_cerrar);
                try
                {
                    parte.CrearPrefactura(contexto);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("debe tener una dirección 'Fiscal'"))
                       throw;
                }
                var cliente = parte.Cliente(contexto);
                if (cliente.Baja)
                {
                    cliente.Baja = false; ;
                    cliente = cliente.Modificar(contexto);
                }
                cliente.AsignarDireccion(contexto, cliente.Direccion(contexto, enumCalificadorDireccion.contacto), enumCalificadorDireccion.fiscal);
                var factura = parte.CrearPrefactura(contexto);
                factura.Transitar(contexto, InzFacturasEmt.n_tran_fae_anular);

                parte.Transitar(contexto, InzPartesTr.n_tran_ptr_gen_devolver);
                parte.Transitar(contexto, InzPartesTr.n_tran_ptr_gen_cancelar);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        private static void CrearPptYParte(ContextoSe contexto, ExpedienteDtm expediente, out PresupuestoDtm presupuesto, out ParteTrDtm parte)
        {
            presupuesto = CrearPresupuesto(contexto, expediente);
            presupuesto.Transitar(contexto, InzPresupuestos.n_tran_entregar);
            presupuesto.Transitar(contexto, InzPresupuestos.n_tran_iniciar);
            parte = presupuesto.CrearParteTr(contexto);
        }

        private static PresupuestoDtm CrearPresupuesto(ContextoSe contexto, ExpedienteDtm expediente)
        {
            PresupuestoDtm presupuesto = expediente.CrearPresupuesto(contexto, InzPresupuestos.n_tipo_venta);
            CrearLinea(presupuesto, contexto, contexto.ElementoPorNombre<UnitarioDto, UnitarioDtm>("servicio 3"), 10);
            CrearLinea(presupuesto, contexto, contexto.ElementoPorNombre<UnitarioDto, UnitarioDtm>("material 1"), 20);
            CrearAlzada(presupuesto, contexto, "alzada 1", 30);
            CrearAlzada(presupuesto, contexto, "alzada 2", 40);
            CrearComentario(presupuesto, contexto, "comenetario 1", 50);
            return presupuesto;
        }

        private static void IntentarCancelarExp(ContextoSe contexto, ExpedienteDtm expediente)
        {
            try
            {
                expediente.Cancelar(contexto);
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("No se puede cancelar el"))
                    throw;
            }
        }
        private static void IntentarIniciarExp(ContextoSe contexto, ExpedienteDtm expediente) => TransitarExp(contexto, expediente, InzExpeditesDeObra.n_estado_exp_obr_iniciado, "No se puede iniciar el expediente");

        private static void IntentarCerrarExp(ContextoSe contexto, ExpedienteDtm expediente) => TransitarExp(contexto, expediente, InzExpeditesDeObra.n_estado_exp_obr_cerrado, "No se puede cerrar el");

        private static void IntentarReabrirExp(ContextoSe contexto, ExpedienteDtm expediente) => TransitarExp(contexto, expediente, InzExpeditesDeObra.n_estado_exp_obr_iniciado, "No se puede reabrir el");

        private static void TransitarExp(ContextoSe contexto, ExpedienteDtm expediente, string estadoDestino, string mensaje)
        {
            try
            {
                expediente.TransitarAl(contexto, estadoDestino);
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains(mensaje))
                    throw;
            }
        }
        //No se puede anular la ejecución del
        private static void IntentarCancelarEjecucion(ContextoSe contexto, PresupuestoDtm presupuesto) => TransitarPpt(contexto, presupuesto, InzPresupuestos.n_estado_rechazado, "No se puede anular la ejecución del");
        private static void TransitarPpt(ContextoSe contexto, PresupuestoDtm presupuesto, string transicion, string mensaje)
        {
            try
            {
                presupuesto.TransitarAl(contexto, transicion);
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains(mensaje))
                    throw;
            }
        }

        private static void IntentarDarPorEjecutado(ContextoSe contexto, ParteTrDtm parte)
        =>
        TransitarParte(contexto, parte, InzPartesTr.n_estado_ptr_gen_realizado, new List<string> { "No se puede dar por realizado", "sin terminar" });

        private static void IntentarRechazarParte(ContextoSe contexto, ParteTrDtm parte)
        =>
        TransitarParte(contexto, parte, InzPartesTr.n_estado_ptr_gen_realizado, new List<string> { "No se puede dar por realizado", "sin terminar" });
            
        private static void TransitarParte(ContextoSe contexto, ParteTrDtm parte, string transicion, List<string> mensajes)
        {
            try
            {
                parte.TransitarAl(contexto, transicion);
            }
            catch (Exception ex)
            {
                foreach (string s in mensajes)
                    if (ex.Message.Contains(s))
                        return;
                throw;
            }
        }

        private static ExpedienteDtm CrearExpedienteDeObra(ContextoSe contexto)
        {
            var sociedad = TestConTerceros.CrearSociedadConInterlocutorYCalleDto(contexto, enumCalificadorDireccion.correspondencia);
            var solicitante = ((SociedadDtm)sociedad.MapearDtm(contexto)).Interlocutor(contexto);
            DireccionDtm d = new DireccionDtm();
            var calles = GestorDeCalles.Gestor(contexto, contexto.Mapeador).LeerRegistros(0, 1, new List<ClausulaDeFiltrado>(), parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, true));
            if (calles.Count > 0)
            {
                var c = calles[0];
                d.IdElemento = solicitante.Id;
                d.IdPais = c.Municipio.Provincia.IdPais;
                d.IdProvincia = c.Municipio.IdProvincia;
                d.IdMunicipio = c.IdMunicipio;
                d.IdCalle = c.Id;
                d.Calificador = enumCalificadorDireccion.contacto;

                GestorDeDirecciones.Gestor(contexto, enumNegocio.Interlocutor).PersistirRegistro(d, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            }

            var expediente = CrearExpediente(contexto,
                codigCg: InzAcromur.n_cg_acm_codigo,
                nombreTipo: InzExpeditesDeObra.n_exp_tipo_expediente_obra,
                enumClaseDeExpediente.ConValoracion,
                solicitante.NIF(contexto),
                "expediente de obra");

            var dirDelExp = expediente.Direccion(contexto, enumCalificadorDireccion.contacto);
            if (dirDelExp is null)
            {
                throw new Exception("El expediente debería tener una dirección de contacto");
            }
            var direccion = solicitante.Direccion(contexto, enumCalificadorDireccion.contacto);

            if (!dirDelExp.EsIgualA(direccion, compararCalificador: true))
            {
                throw new Exception("la dirección del expediente debería ser igual a la del solicitante");
            }

            return expediente;
        }

        public static ExpedienteDtm CrearExpediente(ContextoSe contexto, string codigCg, string nombreTipo, enumClaseDeExpediente clase, string nif, string nombreExpediente)
        {
            var tipo = contexto.SeleccionarPorNombre<TipoDeExpedienteDtm>(nombreTipo, aplicarJoin: true);
            var solicitantes =  contexto.SeleccionarTodos<InterlocutorDtm>(new Dictionary<string, object> {
                { nameof(InterlocutorDtm.Sociedad.NIF), nif },
                { ltrInterlocutor.BuscarPorContacto, false }
                });
            var exp = new ExpedienteDtm
            {
                IdCg = contexto.SeleccionarPorPropiedad<CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), codigCg).Id,
                ClaseDeExpediente = clase,
                IdSolicitante = solicitantes[0].Id,
                Nombre = nombreExpediente,
                Descripcion = "....",
                IdTipo = tipo.Id
            }.Insertar(contexto);
            return exp;
        }

        private static LineaDeUnPptDtm CrearLinea(PresupuestoDtm ppt, ContextoSe contexto, UnitarioDto unitario, int posicion)
        {
            return new LineaDeUnPptDtm
            {
                IdElemento = ppt.Id,
                Orden = posicion,
                TipoDeLinea = Enumerados.enumTipoDeLinea.Unitario,
                IdUnitario = unitario.Id,
                Cantidad = 10,
                Precio = unitario.Venta,
                IdIvaR = contexto.SeleccionarPorPropiedad<IvaRepercutidoDtm>(nameof(IvaRepercutidoDtm.Clase), "IRG").Id
            }.Insertar(contexto);
        }

        private static LineaDeUnPptDtm CrearAlzada(PresupuestoDtm ppt, ContextoSe contexto, string concepto, int posicion)
        {
            return new LineaDeUnPptDtm
            {
                IdElemento = ppt.Id,
                Orden = posicion,
                TipoDeLinea = Enumerados.enumTipoDeLinea.Alzada,
                Concepto = concepto,
                Cantidad = 10,
                Precio = (decimal)50.6,
                IdIvaR = contexto.SeleccionarPorPropiedad<IvaRepercutidoDtm>(nameof(IvaRepercutidoDtm.Clase), "IRG").Id,
                Anotacion = "Esto es una alzada",
                Clase = enumClaseUnitario.Servicio,
                IdNaturaleza = contexto.SeleccionarPorPropiedad<NaturalezaDtm>(nameof(NaturalezaDtm.Sigla), InzMaestros.n_natu_consultoria).Id,
                IdUnidad = contexto.SeleccionarPorPropiedad<UnidadDtm>(nameof(UnidadDtm.Sigla), InzMaestros.n_unidad_ud).Id
            }.Insertar(contexto);
        }

        private static LineaDeUnPptDtm CrearComentario(PresupuestoDtm ppt, ContextoSe contexto, string comentario, int posicion)
        {
            return new LineaDeUnPptDtm
            {
                IdElemento = ppt.Id,
                Orden = posicion,
                TipoDeLinea = Enumerados.enumTipoDeLinea.Comentario,
                Concepto = comentario,
                Anotacion = "Esto es un comentario"
            }.Insertar(contexto);
        }
    }
}
