using GestorDeElementos;
using GestoresDeNegocio.Callejero;
using Inicializador.Expedientes;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Terceros;
using SistemaDeElementos.Inicializador.Acromur;
using GestorDeElementos.Extensores;
using System.Collections.Generic;
using Utilidades;
using ValidacionesBase;
using ServicioDeDatos.SistemaDocumental;
using GestoresDeNegocio.Expediente;
using System.Linq;
using Gestor.Errores;
using GestoresDeNegocio.SistemaDocumental;
using Inicializador.Presupuestos;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Contabilidad;
using Inicializador.Procesos;
using ServicioDeDatos.Tarea;
using System;

namespace ValidacionesDeRn
{
    class TestProcesoSprin
    {

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DefinirProcesoSprin()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                CrearProcesosDeSprin(contexto);
            }
            //ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void ProbarProcesoSprin()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                CrearProcesosDeSprin(contexto);

                //Abrir sprin
                var expediente = CrearExpedienteDeSprin(contexto);

                //intentar poner lo en ejecución, falla por no tener tareas evo o spc relacionadas
                ApiDeValidaciones.IntentarEjecutar(() => expediente.Transitar(contexto, InzProcesoSprim.n_tran_spr_iniciar), "no tiene tareas activas o terminadas");
                var tareaEvo = new TareaDtm
                {
                    IdCg = expediente.IdCg,
                    IdTipo = contexto.SeleccionarPorNombre<TipoDeTareaDtm>(InzTareasEvo.n_tipo_Evo).Id,
                    Nombre = "Tarea ....",
                    IdSolicitante = expediente.IdSolicitante
                }.Insertar(contexto);
                tareaEvo = tareaEvo.Vincular(contexto, expediente);

                //intentar transitar a iniciado, debe fallar por que las tareas del sprint no está planificadas
                ApiDeValidaciones.IntentarEjecutar(() => expediente.Transitar(contexto, InzProcesoSprim.n_tran_spr_iniciar), "ha de estar planificada");
                tareaEvo.Planificar(contexto, DateTime.Now.AddMinutes(1), DateTime.Now.AddDays(1));


                //intentar ponerlo en ejecución, debe fallar por no tener valoración
                ApiDeValidaciones.IntentarEjecutar(() => expediente.Transitar(contexto, InzProcesoSprim.n_tran_spr_iniciar), "No se puede iniciar el expediente");

                //asociarle valoración
                var valoracion =  ExtensorDePresupuestos.CrearValoracion(contexto, new ModeloDeDto.Presupuesto.ValoracionDto
                {
                    IdCg = expediente.IdCg,
                    IdTipo = contexto.SeleccionarPorNombre<TipoDePresupuestoDtm>(InzValoraciones.n_tipo_valoracion).Id,
                    Nombre = $"Valoración del esprin '{expediente.Referencia}'",
                    IdSolicitante = expediente.IdSolicitante,
                    Concepto = "esfuerzo semanal",
                    Clase = enumClaseUnitario.Servicio.ToString(),
                    IdNaturaleza = contexto.Set<NaturalezaDtm>().First(x => true).Id,
                    IdUnidad = contexto.Set<UnidadDtm>().First(x => true).Id,
                    Cantidad = 40 * 3M,
                    Precio = 25.6M,
                    Descuento = 2M,
                    IdIvaR = contexto.Set<IvaRepercutidoDtm>().First(x => true).Id,
                    idExpediente = expediente.Id
                });

                //cancelar la valoración
                valoracion.Cancelar(contexto);

                //intentar ponerlo en ejecución, debe fallar por tener valoración cancelada
                ApiDeValidaciones.IntentarEjecutar(() => expediente.Transitar(contexto, InzProcesoSprim.n_tran_spr_valorar), $"por no tener presupuestos asociados");

                //asociarle  una nueva valoración
                valoracion = ExtensorDePresupuestos.CrearValoracion(contexto, new ModeloDeDto.Presupuesto.ValoracionDto
                {
                    IdCg = expediente.IdCg,
                    IdTipo = contexto.SeleccionarPorNombre<TipoDePresupuestoDtm>(InzValoraciones.n_tipo_valoracion).Id,
                    Nombre = $"Valoración del esprin '{expediente.Referencia}'",
                    IdSolicitante = expediente.IdSolicitante,
                    Concepto = "esfuerzo semanal",
                    Clase = enumClaseUnitario.Servicio.ToString(),
                    IdNaturaleza = contexto.Set<NaturalezaDtm>().First(x => true).Id,
                    IdUnidad = contexto.Set<UnidadDtm>().First(x => true).Id,
                    Cantidad = 40 * 3M,
                    Precio = 25.6M,
                    Descuento = 2M,
                    IdIvaR = contexto.Set<IvaRepercutidoDtm>().First(x => true).Id,
                    idExpediente = expediente.Id
                });

                ////ponerlo en ejecución
                expediente = expediente.Transitar(contexto, InzProcesoSprim.n_tran_spr_iniciar);

                //intentar cerrar el expediente y no poder por tener las tareas en proceso
                ApiDeValidaciones.IntentarEjecutar(() => expediente.Transitar(contexto, InzProcesoSprim.n_tran_spr_cerrar), $"por tener {enumNegocio.Tarea.Plural(true)} en proceso");
                tareaEvo.IdResponsable = contexto.DatosDeConexion.IdUsuario;
                tareaEvo.Modificar(contexto);
                tareaEvo.Transitar(contexto, InzTareasEvo.n_tran_evo_asignar);
                tareaEvo.Transitar(contexto, InzTareasEvo.n_tran_evo_iniciar);
                tareaEvo.Transitar(contexto, InzTareasEvo.n_tran_evo_finalizar);
                tareaEvo.Transitar(contexto, InzTareasEvo.n_tran_evo_terminar);

                //intentar cerrarlo, no se puede por tener un presupuesto no cerrado
                ApiDeValidaciones.IntentarEjecutar(() => expediente.Transitar(contexto, InzProcesoSprim.n_tran_spr_cerrar), $"por tener {enumNegocio.Presupuesto.Plural(true)} en proceso");

                //intentar cerrar la valoración, debe fallar por no tener facturas o partes de trabajo
                ApiDeValidaciones.IntentarEjecutar(() => valoracion.Transitar(contexto, InzValoraciones.n_tran_cerrar), "por no tener ningún parte ni ninguna factura");
                valoracion.CrearPrefactura(contexto);
                valoracion.Transitar(contexto, InzValoraciones.n_tran_cerrar);


                //cerrar el expediente e intentar reabrilo
                expediente.Transitar(contexto, InzProcesoSprim.n_tran_spr_cerrar);
                ApiDeValidaciones.IntentarEjecutar(() => expediente.Transitar(contexto, InzProcesoSprim.n_tran_spr_reabrir), "está facturado");

            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------

        private static void CrearProcesosDeSprin(ContextoSe contexto)
        {
            InzTareasEvo.ModeloDeTareasEvo(contexto);
            InzTareasSpc.ModeloDeTareasSpc(contexto);
            InzValoraciones.ModeloDeValoracion(contexto);
            InzProcesoSprim.ProcesoDeSprin(contexto);
        }


        //-----------------------------------------------------------------------------------------------------------------------------------
        private static ExpedienteDtm CrearExpedienteDeSprin(ContextoSe contexto)
        {
            var sociedad = TestConTerceros.CrearSociedadConInterlocutorYCalleDto(contexto, enumCalificadorDireccion.fiscal);
            var solicitante = ((SociedadDtm)sociedad.MapearDtm(contexto)).Interlocutor(contexto);

            if (solicitante.Baja)
            {
                solicitante.Baja = false;
                solicitante = solicitante.Modificar(contexto);
            }

            var cliente = solicitante.Cliente(contexto, crearCliente: true);
            if (cliente.Baja)
            {
                cliente.Baja = false;
                cliente.Modificar(contexto);
            }

            var expediente = TestConObras.CrearExpediente(contexto,
                codigCg: contexto.Set<CentroGestorDtm>().First(x => true).Codigo,
                nombreTipo: InzProcesoSprim.n_exp_tipo_expediente_spr,
                enumClaseDeExpediente.DeCliente,
                solicitante.NIF(contexto),
                "mi primer expediente de Sprint");

            return expediente;
        }
    }
}
