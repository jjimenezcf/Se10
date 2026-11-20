using System;
using System.Collections.Generic;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Callejero;
using GestoresDeNegocio.Gastos;
using GestoresDeNegocio.Terceros;
using GestoresDeNegocio.TrabajosSometidos;
using Inicializador.ContratosVnt;
using Inicializador.Gastos;
using ModeloDeDto.Gastos;
using ModeloDeDto.Terceros;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using SistemaDeElementos.Inicializador.Acromur;
using Utilidades;
using ValidacionesBase;

namespace ValidacionesDeRn
{
    class TestConRemesasPag
    {

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DefinirFlujoRemesasPag()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                try
                {
                    InzRemesasPag.ModeloDeRemesasPag(contexto);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }


        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void ProbarFlujoDeRemesasPag()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                var proveedor = ExtensorDeProveedores.CrearProveedor(contexto, InzContratosVnt.PepeFuster);
                var interlocutor = proveedor.Interlocutor(contexto);
                var calles = GestorDeCalles.Gestor(contexto, contexto.Mapeador).LeerRegistros(0, 1, new List<ClausulaDeFiltrado>(), parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, true));
                DireccionDtm direccion = calles[0].CrearDireccion(enumCalificadorDireccion.contacto);
                interlocutor.AsignarDireccion(contexto, direccion);
                interlocutor.AsignarDireccion(contexto, interlocutor.Direccion(contexto, enumCalificadorDireccion.contacto), enumCalificadorDireccion.fiscal);

                //InzPagos.ModeloDePagos(contexto);
                //InzRemesasPag.ModeloDeRemesasPag(contexto);

                //crear tres pagos diferente
                ApiDeValidaciones.IntentarEjecutar(() => CrearPago(contexto, interlocutor, "pago contado", enumClaseDePago.Contado, pagarEl: null, pagadoEL: DateTime.Now.Date, importe: 0), "por no tener un importe mayor");
                var contado = CrearPago(contexto, interlocutor, "pago contado", enumClaseDePago.Contado, pagarEl: null, pagadoEL: DateTime.Now.Date, importe: 200);
                var transferencia = CrearPago(contexto, interlocutor, "pago por transferencia", enumClaseDePago.Transferencia, null, null, importe: 300);
                var remesado1 = CrearPago(contexto, interlocutor, "pago remesado 1", enumClaseDePago.Remesa, null, null, importe: 300);

                //generar justificantes de pago y debe falla por estar terminado
                GestorDePagos.GenerarJustificante(contexto, contado.Id);

                //generar justificante y debe fallar por no tener las cuenta de acreedor y deudora definida
                ApiDeValidaciones.IntentarEjecutar(() => GestorDePagos.GenerarJustificante(contexto, transferencia.Id), "Para emitir el justificante del pago");

                //generar justificante y debe fallar por no tener las cuenta de acreedor definida
                ApiDeValidaciones.IntentarEjecutar(() => GestorDePagos.GenerarJustificante(contexto, remesado1.Id), "Para emitir el justificante del pago");

                //intentar transitar la transferencia a pagada,faltan las cuentas bancaria
                ApiDeValidaciones.IntentarEjecutar(
                () => transferencia.TransitarALaEtapa(contexto, enumEtapasDePagos.PAG_Etapa_Pagado.EstadosDeLaEtapa(), new Dictionary<string, object>(), delSistema: false),
                "por no tener indicada la cuenta");


                //añadir cuentas deudora y acreedoras y transitar a pagada
                transferencia.IdCuentaDePago = transferencia.Sociedad(contexto).CuentasDeMiSociedad(contexto, enumClaseDeCuentaBancaria.Pago, errorSiNoHay: true)[0].Id;
                ApiDeValidaciones.IntentarEjecutar(() => transferencia.AsignarCuentaAcreedora(contexto), "no tiene ninguna cuenta bancaria activa");
                var cuenta = transferencia.CuentaBancariaDePago(contexto);
                transferencia.Solicitante(contexto).Proveedor(contexto).AsociarCuenta(contexto, "Cuenta de ingreso", enumClaseDeCuentaBancaria.Ingreso, cuenta);
                transferencia.AsignarCuentaAcreedora(contexto);

                //intentamos transitar pero el día de pago es mayor al de hoy
                ApiDeValidaciones.IntentarEjecutar(() => transferencia.Transitar(contexto, InzPagos.n_tran_pag_pagar), "mayor");
                transferencia.PagadoEl = DateTime.Now.Date;
                transferencia = transferencia.Modificar(contexto);
                GestorDePagos.GenerarJustificante(contexto, transferencia.Id);

                //añadir cuenta acreedora al pago a remesar
                remesado1.Solicitante(contexto).Proveedor(contexto).AsociarCuenta(contexto, "Cuenta de ingreso y pagos", enumClaseDeCuentaBancaria.Ambas, cuenta);
                transferencia.Solicitante(contexto).Proveedor(contexto).AsociarCuenta(contexto, "Cuenta de ingreso", enumClaseDeCuentaBancaria.Ingreso, cuenta);
                remesado1.Solicitante(contexto).Proveedor(contexto).AsociarCuenta(contexto, "sólo pra ingresos", enumClaseDeCuentaBancaria.Ambas, cuenta);
                transferencia.AsignarCuentaAcreedora(contexto);
                remesado1.AsignarCuentaAcreedora(contexto);

                //crear dos remesas
                var remesa1 = CrearRemesa(contexto, enumClaseDeRemesaPag.Transferencias, "remesa 1");
                var remesa2 = CrearRemesa(contexto, enumClaseDeRemesaPag.Transferencias, "remesa 2");
                remesa1.PagarEl = DateTime.Now.Date.AddDays(-1);
                ApiDeValidaciones.IntentarEjecutar(() => remesa1.Modificar(contexto), "anterior al día de hoy");
                remesa1.PagarEl = DateTime.Now;
                ApiDeValidaciones.IntentarEjecutar(() => remesa2.Modificar(contexto), "puede indicar hora");
                remesa1.PagarEl = DateTime.Now.Date;
                remesa1 = remesa1.Modificar(contexto);
                remesa2.PagarEl = DateTime.Now.Date;
                remesa2 = remesa2.Modificar(contexto);

                //incluir el pago a remesar en una de ella, y si se vuelve hacer falla por ya estar incluido el la remesa
                var relacion = new PagoDeUnaRemesaDto();
                relacion.IdElemento = remesa1.Id;
                relacion.IdPago = remesado1.Id;
                var parametros = new ParametrosDeNegocio(enumTipoOperacion.Insertar);
                parametros.EsUnaPeticion = true;
                parametros.Parametros[ltrParametrosNeg.Peticion] = enumPeticion.epCrearRelacion;
                GestorDePagosDeUnaRemesa.Gestor(contexto, contexto.Mapeador).CrearRelacion(relacion, parametros, true);
                ApiDeValidaciones.IntentarEjecutar(() => GestorDePagosDeUnaRemesa.Gestor(contexto, contexto.Mapeador).CrearRelacion(relacion, parametros, true), "ya existe la relación");

                //debe fallar si se intenta modificar la fecha de pago de un pago remesado
                remesado1 = remesado1.Recargar(contexto);
                remesado1.PagarEl = DateTime.Now.AddDays(1);
                ApiDeValidaciones.IntentarEjecutar(() => remesado1.Modificar(contexto), "por no estar en la etapa de pendiente");
                remesado1 = remesado1.Recargar(contexto);
                if (remesado1.PagarEl != remesa1.PagarEl) throw new Exception("La fecha de cuando pagar el pago y la remesa ha de ser la misma");

                //incluir pago por transferencia y el de contado en la remesa, deben fallar
                ApiDeValidaciones.IntentarEjecutar(() => new PagoDeUnaRemesaDtm { IdElemento = remesa2.Id, IdPago = transferencia.Id }.Insertar(contexto), "por no ser remesable");
                transferencia = transferencia.TransitarALaEtapa(contexto, enumEtapasDePagos.PAG_Etapa_Pendiente.EstadosDeLaEtapa(), new Dictionary<string, object>(), delSistema: false);
                ApiDeValidaciones.IntentarEjecutar(() => new PagoDeUnaRemesaDtm { IdElemento = remesa2.Id, IdPago = transferencia.Id }.Insertar(contexto), "por no ser remesable");

                //intentar cambiar la clase del pago acualquier pago, debe fallar
                transferencia.Clase = enumClaseDePago.Remesa;
                ApiDeValidaciones.IntentarEjecutar(() => transferencia.Modificar(contexto), "modificar la clase del pago");

                //intentar incluir el pago de la remesa en otra, debe fallar
                ApiDeValidaciones.IntentarEjecutar(() => new PagoDeUnaRemesaDtm { IdElemento = remesa2.Id, IdPago = remesado1.Id }.Insertar(contexto), "no se puede incluir o excluir en la remesa");

                //crear un pago a remesar, Incluir el pago en la remesa
                var remesado2 = CrearPago(contexto, interlocutor, "pago remesado 2", enumClaseDePago.Remesa, null, null, importe: 200);
                remesado1.Solicitante(contexto).Proveedor(contexto).AsociarCuenta(contexto, "Cuenta de ingreso y pagos", enumClaseDeCuentaBancaria.Ambas, cuenta);
                new PagoDeUnaRemesaDtm { IdElemento = remesa1.Id, IdPago = remesado2.Id }.Insertar(contexto);

                //transitar la remesa a generada e intentar borrar el archivo sepa
                remesa1 = remesa1.TransitarALaEtapa(contexto, enumEtapasDeRemesasPag.REM_Etapa_Generada.EstadosDeLaEtapa(), new Dictionary<string, object>(), delSistema: false);

                ApiDeValidaciones.IntentarEjecutar(() => contexto.EliminarPorId<ArchivoDtm>((int)remesa1.IdArchivo), "ya que está siendo referenciado");
                new ArchivosDeUnaRemesaPagDtm { idElemento1 = remesa1.Id, idElemento2 = (int)remesa1.IdArchivo }.Insertar(contexto);

                //intentar eliminarlo con una AK
                ApiDeValidaciones.IntentarEjecutar(() => contexto.EliminarPorAk<ArchivosDeUnaRemesaPagDtm>(
                    new Dictionary<string, object> {
                        {nameof(ArchivosDeUnaRemesaPagDtm.idElemento1), remesa1.Id },
                        {nameof(ArchivosDeUnaRemesaPagDtm.idElemento2), remesa1.IdArchivo }
                    }), "por ser el original de la emisión");

                //intentar quitar el pago de la remesa
                ApiDeValidaciones.IntentarEjecutar(() => contexto.EliminarPorAk<PagoDeUnaRemesaDtm>(
                    new Dictionary<string, object> {
                        {nameof(PagoDeUnaRemesaDtm.IdElemento), remesa1.Id },
                        {nameof(PagoDeUnaRemesaDtm.IdPago), remesado2.Id}
                    }), "no acepta excluir el pago");

                //devolver la remesa y debe borrar el sepa
                remesa1 = remesa1.TransitarALaEtapa(contexto, enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion.EstadosDeLaEtapa(), new Dictionary<string, object>(), delSistema: false);
                if (remesa1.IdArchivo is not null) throw new Exception("La remesa no debería tener archivos");

                ////mandar la remesa a presentada
                remesa1 = remesa1.TransitarALaEtapa(contexto, enumEtapasDeRemesasPag.REM_Etapa_Generada.EstadosDeLaEtapa(), new Dictionary<string, object>(), delSistema: false);
                enumNegocio.RemesaPag.ResetearParametro(contexto, VariableDeRemesasPag.Parametro.REM_DiasDeTransferencia, "0");
                remesa1 = remesa1.TransitarALaEtapa(contexto, enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion.EstadosDeLaEtapa(), new Dictionary<string, object>(), delSistema: false);

                //darla por pagada, anular un pago y anular la anulación del pago, cancelar el pago
                remesa1.PagadaEl = DateTime.Now.Date;
                remesa1 = remesa1.Modificar(contexto, ltrDeUnaRemesaPag.Accion_PagoDeRemesa);
                remesa1.AnularPago(contexto, remesado2, DateTime.Now.Date, "no estaba bien la cuenta");
                remesa1.AnularAnulacionDePago(contexto, remesado2, "si lo estaba bien la cuenta");
                remesa1.PagadaEl = null;
                remesa1 = remesa1.Modificar(contexto, ltrDeUnaRemesaPag.Accion_AnularPagoDeRemesa);

                //intentar anular el pago
                ApiDeValidaciones.IntentarEjecutar(() => remesa1.AnularPago(contexto, remesado2, DateTime.Now.Date, "no estaba bien la cuenta"), "aun no ha sido pagada");

                //Intentar transitar a cerrada
                ApiDeValidaciones.IntentarEjecutar(() => remesa1.TransitarALaEtapa(contexto, enumEtapasDeRemesasPag.REM_Etapa_De_Cierre.EstadosDeLaEtapa(), new Dictionary<string, object>(), delSistema: false),
                    "no se ha cumplido la fecha de pago");

                //ejecutar el trabajo sometido
                TrabajosDeRemesasPag.SometerProcesosDeRemesasPag(contexto).EjecutarTrabajo(contexto);

                //dar por pagada la remesa y cerrarla
                remesa1.PagadaEl = DateTime.Now.Date;
                remesa1 = remesa1.Modificar(contexto, ltrDeUnaRemesaPag.Accion_PagoDeRemesa);
                remesa1.TransitarALaEtapa(contexto, enumEtapasDeRemesasPag.REM_Etapa_De_Cierre.EstadosDeLaEtapa(), new Dictionary<string, object>(), delSistema: false);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        private static RemesaPagDtm CrearRemesa(ContextoSe contexto, enumClaseDeRemesaPag clase, string nombre)
        {
            var sociedad = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), InzAcromur.n_nif_acromur);
            var cuentaDeSociedad = sociedad.CuentasDeMiSociedad(contexto, enumClaseDeCuentaBancaria.Pago, activa: true, errorSiNoHay: true)[0];
            var tipo = contexto.SeleccionarPorPropiedad<TipoDeRemesaPagDtm>(nameof(TipoDeRemesaPagDtm.Nombre), InzRemesasPag.n_rem_tipo_general);
            var remesa = new RemesaPagDtm
            {
                IdCg = contexto.SeleccionarDtoPorAk<CentroGestorDto, CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), InzAcromur.n_cg_acm_codigo).Id,
                IdTipo = tipo.Id,
                Clase = clase,
                Nombre = nombre,
                Deudor = sociedad.Expresion,
                NifDelDeudor = sociedad.NIF,
                SufijoDeudor = "000",
                Presentador = "Juan Jiménez",
                NifDelPresentador = "27485405Z",
                SufijoPresentador = "000",
                IdCuentaDePago = cuentaDeSociedad.Id
            }.Insertar(contexto);

            return remesa;
        }

        private static PagoDtm CrearPago(ContextoSe contexto, InterlocutorDtm interlocutor, string nombre, enumClaseDePago clase, DateTime? pagarEl, DateTime? pagadoEL, int importe)
        {
            var f = new PagoDtm
            {
                IdCg = contexto.SeleccionarDtoPorAk<CentroGestorDto, CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), InzAcromur.n_cg_acm_codigo).Id,
                IdTipo = contexto.SeleccionarPorPropiedad<TipoDePagoDtm>(nameof(TipoDePagoDtm.Nombre), InzPagos.n_pag_tipo_general).Id,
                Nombre = nombre,
                IdSolicitante = interlocutor.Id,
                Contacto = interlocutor.Expresion,
                Telefono = interlocutor.Telefono,
                eMail = interlocutor.eMail,
                Clase = clase,
                PagarEl = pagarEl,
                PagadoEl = pagadoEL,
                Importe = importe
            }.Insertar(contexto);
            return f;
        }

    }
}
