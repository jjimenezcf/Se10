using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Inicializador.ContratosVnt;
using Inicializador.Procesos;
using ModeloDeDto.Terceros;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Terceros;
using SistemaDeElementos.Inicializador;
using SistemaDeElementos.Inicializador.Acromur;
using SistemaDeElementos.Inicializador.Mra;
using ValidacionesBase;

namespace ValidacionesDeRn
{
    class TestConSc
    {

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DefinirFlujoSc()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzSolicitudDeContrato.ModeloDeSolicitudesDeContrato(contexto);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }


        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void ProbarFlujoDeSc()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                //creamos la tarea de desasociar contratos
                InzAcciones.DefinirAcciones(contexto);
                InzContratosVnt.ModeloDeContratosVnt(contexto);
                InzSolicitudDeContrato.ModeloDeSolicitudesDeContrato(contexto);


                var sc = TestConObras.CrearExpediente(contexto,
                    codigCg: InzAcromur.n_cg_acm_codigo,
                    nombreTipo: InzSolicitudDeContrato.n_exp_tipo_expediente_scv,
                    enumClaseDeExpediente.solicitudContrato,
                    InzMra.n_nif_mra,
                    "mi primer SC");

      
                //Creo un contrato de venta para el solicitante y se lo asocio y da error ya que no cumple la etapa
                var numero = DateTime.Now.Ticks;
                var solicitante = contexto.SeleccionarDto<InterlocutorDto, InterlocutorDtm>(sc.IdSolicitante);
                var cuenta = contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Clientes);
                var cliente = solicitante.CrearCliente(contexto, cuenta.Id);

                var contrato1 = TestConContratos.CrearContrato(contexto,
                     contexto.Tipos<TipoDeContratoDtm>().Where(x => x.Nombre.Equals(InzContratosVnt.n_ctv_tipo_venta)).First().Id,
                     contexto.SeleccionarPorId<ClienteDtm>(cliente.Id),
                     $"Contrato nº: {numero + 1}",
                     seIniciaEn: 0,
                     duracionInicial: 1,
                     prorrogarCada: 0,
                     prorrogas: 0,
                     enumClaseDeProrroga.noProrrogable);

                contrato1.IdExpediente = sc.Id;
                try
                {
                    contrato1.Modificar(contexto);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("No se puede asociar el contrato")) throw;
                }
                sc = sc.Transitar(contexto, InzSolicitudDeContrato.n_tran_scv_iniciar);
                contrato1.Modificar(contexto);

                //Transito la sc de elaborado a no contratado, y debe dar error ya que tiene un contrato asociado y este no está cancelado
                try
                {
                    sc = sc.Transitar(contexto, InzSolicitudDeContrato.n_tran_scv_no_contratada);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("La Solicitud de contrato no puede transitarse por tener el contrato")) 
                        throw;
                }

                //Transito la sc de elaborado a contratado y debe permitirlo por tener un contrato asociado no cancelado
                sc = sc.Transitar(contexto, InzSolicitudDeContrato.n_tran_scv_contratar);

                //Cancelo el contrato y si esté tiene una SC con solo este contrato el sistema devuelve la SC a en elaboración
                contrato1 = contrato1.Transitar(contexto,InzContratosVnt.n_ctv_tran_cancelar);
                sc = sc.Recargar(contexto);
                if (sc.Estado.Nombre != InzSolicitudDeContrato.n_estado_scv_en_elaboracion)
                    GestorDeErrores.Emitir($"La solicitud de contrato '{sc.Referencia}' debería estar en elaboración");

                //Activo de nuevo el contrato y devuelvo la solicitud a contratada
                contrato1 = contrato1.Transitar(contexto, InzContratosVnt.n_ctv_tran_reactivar);
                contrato1.IdExpediente = sc.Id;
                contrato1 = contrato1.Modificar(contexto);
                sc = sc.Transitar(contexto, InzSolicitudDeContrato.n_tran_scv_contratar);

                //Devuelvo la SC a En Elaboración, esto desasocia el contrato, lo recargo, y lo vuelvo a asociar
                sc = sc.Transitar(contexto, InzSolicitudDeContrato.n_tran_scv_desasociar);
                contrato1 = contrato1.Recargar(contexto);
                if (contrato1.IdExpediente>0)
                    GestorDeErrores.Emitir($"El contrato '{contrato1.Referencia}' debería estar sin expediente");
                contrato1.IdExpediente = sc.Id;
                contrato1.Modificar(contexto);


                //Intento transitar a "sc no contratada" y debe dar error por tener un contrato activo
                try
                {
                    sc = sc.Transitar(contexto, InzSolicitudDeContrato.n_tran_scv_no_contratada);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("La Solicitud de contrato no puede transitarse por tener el contrato"))
                        throw;
                }

                //Por tanto la transito la SC a contratada, cancelo el contrato, esto la devuelve a en elaboración
                sc = sc.Transitar(contexto, InzSolicitudDeContrato.n_tran_scv_contratar);
                contrato1 = contrato1.Transitar(contexto, InzContratosVnt.n_ctv_tran_cancelar);
                sc = sc.Recargar(contexto);

                //y ya la puedo transitar la SC a no contratada
                sc = sc.Transitar(contexto, InzSolicitudDeContrato.n_tran_scv_no_contratada);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

    }
}
