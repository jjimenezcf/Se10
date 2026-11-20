using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Callejero;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Ventas;
using Inicializador.Ventas;
using ModeloDeDto.Ventas;
using ModeloXml.eFactura.Facturae322;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Ventas;
using Utilidades;
using ValidacionesBase;
using static GestorDeElementos.Extensores.ExtensorDeParmetrosDeNegocio;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using GestoresDeNegocio.SistemaDocumental;

namespace ValidacionesDeRn
{
    class TestConFacturasEmt
    {

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DefinirFlujoFacturasEmt()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzFacturasEmt.ModeloDeFacturasEmt(contexto);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }


        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void ProbarFlujoDeFacturasEmt()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                FacturaEmtDtm factura = CrearFactura(contexto);
                var periodo = factura.Periodo(contexto);
                periodo.Inicio = DateTime.Now;
                periodo.Fin = DateTime.Now.AddDays(30);
                periodo.Modificar(contexto);
                factura = factura.Transitar(contexto, InzFacturasEmt.n_tran_fae_emitir);
                GestorDeFacturasEmt.EmitirPdfFactura(contexto, factura.MapearDto<FacturaEmtDto>(contexto));

                //Cobrarla totalmente
                var cobro = factura.Cobrar(contexto);
                cobro.Eliminar(contexto);
                factura = factura.Recargar(contexto);
                if (!factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida))
                    GestorDeErrores.Emitir("La factura debe estar en estado emitido");

                //Cobrar parcialemente
                factura.Cobrar(contexto, factura.APagar(contexto) - factura.APagar(contexto) / 2);
                factura = factura.Recargar(contexto);
                cobro = factura.Cobrar(contexto);
                factura = factura.Recargar(contexto);
                if (factura.Etapa() != enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada)
                    GestorDeErrores.Emitir("La factura debe estar en estado cobrada");
                cobro.Eliminar(contexto);
                cobro = factura.UltimoCobro(contexto);
                cobro.Eliminar(contexto);
                factura = factura.Recargar(contexto);
                if (!factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida))
                    GestorDeErrores.Emitir("La factura debe estar en estado emitido");

                //Transitar a vencida por el usuario
                try
                {
                    factura = factura.Transitar(contexto, InzFacturasEmt.n_tran_fae_enviar_a_vencida, delSistema: false);
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("No hay parametrizada una transición"))
                        throw;
                }

                //Transitar a vencida por el sistema y validar documento de reclamación
                var darPorVencida = factura.TransicionPosible(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Emitida, enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion.Lista());
                factura = factura.Transitar(contexto, darPorVencida.Id);
                var parametro = enumNegocio.FacturaEmitida.LeerParametro(contexto, enumParametrosDeFacturasEmt.FAE_TipoArchivadorDeReclamacion);

                var archivador = contexto.SeleccionarPorId<ArchivadorDtm>((int)factura.IdArchivadorParaLaReclamacion);
                if (parametro.Valor.Entero() != archivador.IdTipo)
                    GestorDeErrores.Emitir($"La factura debe tener un archivador del tipo {archivador.IdTipo}");

                //Cobrar una factura vencida con dos cobros y anularlos
                factura.Cobrar(contexto, factura.APagar(contexto) - factura.APagar(contexto) / 2);
                cobro = factura.Cobrar(contexto);
                factura = factura.Recargar(contexto);
                cobro.Eliminar(contexto);
                cobro = factura.UltimoCobro(contexto);
                cobro.Eliminar(contexto);

                //Transitar a vencida, y de ahí a reclamada
                factura = factura.Recargar(contexto);
                factura = factura.Transitar(contexto, darPorVencida.Id);
                var reclamacion = factura.ArchivadorParaLaReclamacion(contexto);
                var ruta = ApiDeValidaciones.CrearFicheroWindowsParaAnexar(GestorDeVariables.RutaBase, "reclamación.txt");
                var archivo = (ArchivoDtm)reclamacion.AnexarArchivo(contexto, ruta);
                if (archivo.Nombre != "reclamación.txt")
                    GestorDeErrores.Emitir($"Se ha asociado incorrectamente el archivo de reclamación a la factura");
                factura = factura.Transitar(contexto, InzFacturasEmt.n_tran_fae_iniciar_reclamacion);

                //Cobrar una factura reclamada con dos cobros y anularlos
                factura.Cobrar(contexto, factura.APagar(contexto) - factura.APagar(contexto) / 2);
                cobro = factura.Cobrar(contexto);
                factura = factura.Recargar(contexto);
                cobro.Eliminar(contexto);
                cobro = factura.UltimoCobro(contexto);
                cobro.Eliminar(contexto);

            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        private static FacturaEmtDtm CrearFactura(ContextoSe contexto)
        {
            InzFacturasEmt.ModeloDeFacturasEmt(contexto);
            TestConPlfsDeVenta.CrearContratoYPlanificaciones(contexto, duracionEnMeses: 12);
            var planificacion = contexto.SeleccionarUltimo<PlanificacionDeVentaDtm>(nameof(PlanificacionDeVentaDtm.FechaCreacion));
            planificacion.Transitar(contexto, InzPlanificacionesDeVenta.n_tran_plg_generar);

            //Generar una prefactura
            var cliente = planificacion.Cliente(contexto);
            var calles = GestorDeCalles.Gestor(contexto, contexto.Mapeador).LeerRegistros(0, 1, new List<ClausulaDeFiltrado>(), parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, true));
            DireccionDtm direccion = calles[0].CrearDireccion(enumCalificadorDireccion.contacto);
            cliente.AsignarDireccion(contexto, direccion);
            var parte = planificacion.ParteTr(contexto);
            parte.Transitar(contexto, InzPartesTr.n_tran_ptr_gen_realizar);
            cliente.AsignarDireccion(contexto, cliente.Direccion(contexto, enumCalificadorDireccion.contacto), enumCalificadorDireccion.fiscal);
            parte.Transitar(contexto, InzPartesTr.n_tran_ptr_gen_perfacturar);

            //Emitir prefactura
            return parte.FacturaEmt(contexto);
        }

        [Test]
        public void LeerFacturaElectronica()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                var ruta = Path.Combine("..", "..", "..", "..", "SistemaDeElementos", "Documentación", "Xmls");
                if (!Directory.Exists(ruta)) { Directory.CreateDirectory(ruta); }
                var rutaConFichero = Path.Combine(ruta, $"Test_eFactura_146.xml");
                var efactura = eFactura322.FromFile(rutaConFichero);
                //efactura.Validate();
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        [Test]
        public void GenerarFacturaElctronica32()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(63);
                new GeneradorDeFacturaEmtXml(contexto, factura, @"C:\Temp\Trazas\Secify\63-32.xml").Generar(enumClaseDeEmision.eFactura32);
                new GeneradorDeFacturaEmtXml(contexto, factura, @"C:\Temp\Trazas\Secify\63-322.xml").Generar(enumClaseDeEmision.eFactura322);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }


        [Test]
        public void EmitirFacturaElectronica()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {

                var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(146); // CrearFactura(contexto);

                var ruta = Path.Combine("..", "..", "..", "..", "SistemaDeElementos", "Documentación", "Xmls");
                if (!Directory.Exists(ruta)) { Directory.CreateDirectory(ruta); }
                var rutaConFichero = Path.Combine(ruta, $"Test_eFactura_{factura.Id}.xml");

                new GeneradorDeFacturaEmtXml(contexto, factura,rutaConFichero).Generar();
                //ServicioSepa.eFactura.ValidarFacturaE(rutaConFichero);
                var objeto = eFactura322.FromFile(rutaConFichero);

                var recibida = new FacturaRecDtm();
                recibida.IdTipo = contexto.SeleccionarPorId<TipoDeFacturaEmtDtm>(1).Id;
                recibida.IdCg = contexto.SeleccionarPorId<CentroGestorDtm>(1).Id;
                recibida.Numero = objeto.Invoices[0].InvoiceHeader.InvoiceNumber;
                recibida.Descripcion = objeto.Invoices[0].InvoiceIssueData.InvoiceDescription;
                var asunto = objeto.Invoices[0].InvoiceIssueData.InvoiceDescription.Left(250);
                recibida.Nombre = asunto.IsNullOrEmpty() ? "una factura más" : objeto.Invoices[0].InvoiceIssueData.InvoiceDescription.Left(250);
                objeto.Parties.SellerParty.TaxIdentification.TaxIdentificationNumber = "B53413258";
                recibida.IdProveedor = contexto.SeleccionarPorPropiedad<ProveedorDtm>(nameof(ltrProveedor.NIF), objeto.Parties.SellerParty.TaxIdentification.TaxIdentificationNumber).Id;
                recibida.FacturadaEl = objeto.Invoices[0].InvoiceIssueData.IssueDate;
                recibida.VenceEl = objeto.Invoices[0].PaymentDetails[0].InstallmentDueDate;
                recibida.BaseImponible = objeto.Invoices[0].InvoiceTotals.TotalGrossAmountBeforeTaxes.ToString().Decimal();
                recibida.TotalDelPago = objeto.Invoices[0].InvoiceTotals.InvoiceTotal.ToString().Decimal();

                var idArchivo = ServidorDocumental.SubirArchivo(contexto, rutaConFichero, sanitizar: false);
                recibida.IdArchivo = idArchivo;
                recibida.Insertar(contexto);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        [Test]
        public void CrearFacturaElectronica()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                var ruta = Path.Combine("..", "..", "..", "..", "SistemaDeElementos", "Documentación", "Xmls");
                if (!Directory.Exists(ruta)) 
                    Directory.CreateDirectory(ruta);

                var original = Path.Combine(ruta, $"epDescargarArchivo.xml");
                if (!File.Exists(original))
                    GestorDeErrores.Emitir($"Ha de crear el fichero '{original}' para hacer este test");

                var rutaConFichero = Path.Combine(ruta, $"epDescargarArchivo - copia.xml");
                File.Copy(original, rutaConFichero);

                var facturae = new eFactura322().Validate(rutaConFichero);

                var proveedor = contexto.SeleccionarPorPropiedad<ProveedorDtm>(nameof(ltrProveedor.NIF), facturae.Parties.SellerParty.TaxIdentification.TaxIdentificationNumber);
                var recibida = new FacturaRecDtm();
                recibida.IdTipo = contexto.SeleccionarPorId<TipoDeFacturaEmtDtm>(1).Id;
                recibida.IdCg = contexto.SeleccionarPorId<CentroGestorDtm>(1).Id;
                recibida.Numero = facturae.Invoices[0].InvoiceHeader.InvoiceNumber;
                var asunto = facturae.Invoices[0].InvoiceIssueData.InvoiceDescription.Left(250);
                recibida.Nombre = asunto.IsNullOrEmpty() ? "una factura más" : facturae.Invoices[0].InvoiceIssueData.InvoiceDescription.Left(250);
                recibida.Descripcion = facturae.Invoices[0].AdditionalData.InvoiceAdditionalInformation.Left(2000);
                recibida.IdProveedor = proveedor.Id;
                recibida.FacturadaEl = facturae.Invoices[0].InvoiceIssueData.IssueDate;
                recibida.VenceEl = facturae.Invoices[0].PaymentDetails[0].InstallmentDueDate;
                recibida.BaseImponible = facturae.Invoices[0].InvoiceTotals.TotalGrossAmountBeforeTaxes.ToString().Decimal();
                recibida.TotalDelPago = facturae.Invoices[0].InvoiceTotals.InvoiceTotal.ToString().Decimal();

                var idArchivo = ServidorDocumental.SubirArchivo(contexto, rutaConFichero, sanitizar: false);
                recibida.IdArchivo = idArchivo;
                recibida.RecibidaEl = DateTime.Now;
                recibida.Insertar(contexto);

                if (facturae.Invoices[0].InvoiceIssueData.InvoiceDescription.ToString().Length > 250)
                {
                    //registrar una observación con el nombre completo
                }
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

    }
}

/*

1. Devuelvo el cobro de una factura en estado Cobrada
   * Si no hay más cobros
      * si está vencida 
        * transito a etapa de reclamación: Cobrada --> Vencida
      * Si no está vencida 
        * transito a etapa de emitida: Cobrada --> Emitida
   * Si hay mas de un cobro
      * Si está vencida
         transito a etapa de reclamación: Cobrada --> Vencida
      * Si no está vencida 
         transito a parcialmentecobrada: Cobrada --> Parc. Cobrada (etapa pdt. pago- etapa de reclamación - etapa emitida)

2. Devuelvo el cobro de una factura en estado parcl. cobrada
   * Si no hay más cobros
      * si está vencida 
         transito a etapa de reclamación: Cobrada --> Vencida
      * Si no está vencida 
        * transito a etapa de emitida: Cobrada --> Emitida
   * Si hay mas de un cobro
      * Si está vencida
         transito a etapa de reclamación: Cobrada --> Vencida
      * Si no está vencida (sigo en Prcl. cobrada)

3. Devuelvo el cobro de una factura en estado reclamación o vencida 
   * No se hace nada

4. Pago de una factura emitida   
   * Si la salda
     transito a la etapa cobrada: emitida --> cobrada
   * Si no la salda
     Recalculo el vencimiento
	 transito a parcialmentecobrada: emitida a parcial

5. Pago de una factura parcial
   * Si la salda
     transito a la etapa cobrada: parcial --> cobrada
   * Si no la salda
     Recalculo vencimiento

6. Pago de una factura vencida
   * Si la salda
     transito a la etapa cobrada: vencida --> cobrada
   * Si no la salda
     Recalculo vencimiento
	 Transito a parcial cobrada: vencida --> parcial (etapa pdt. pago- etapa de reclamación - etapa emitida)
   
7. Pago de una factura en reclamación
   * Si la salda
     transito a la etapa cobrada: en reclamación --> cobrada
   * Si no la salda
     Recalculo vencimiento
	 Transito a parcial cobrada: en reclamación --> parcial (etapa pdt. pago- etapa de reclamación - etapa emitida)
   
*/
