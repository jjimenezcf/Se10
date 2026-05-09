using System;
using System.Collections.Generic;
using System.Linq;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Callejero;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Terceros;
using GestoresDeNegocio.TrabajosSometidos;
using GestoresDeNegocio.Ventas;
using Inicializador.ContratosVnt;
using Inicializador.Ventas;
using ModeloDeDto.Terceros;
using ModeloDeDto.Ventas;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using SistemaDeElementos.Inicializador.Acromur;
using SistemaDeElementos.Inicializador.Datos;
using Utilidades;
using ValidacionesBase;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ValidacionesDeRn
{
    class TestConRemesasFae
    {

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DefinirFlujoRemesasFae()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                try
                {
                    InzRemesasFae.ModeloDeRemesasFae(contexto);
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
        public void ProbarFlujoDeRemesasFae()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                var cliente = InzContratosVnt.PepeFuster.CrearCliente(contexto);

                //var cliente = contexto.SeleccionarPorPropiedad<ClienteDtm>(nameof(ClienteDto.Expresion), "B73954455");
                var calles = GestorDeCalles.Gestor(contexto, contexto.Mapeador).LeerRegistros(0, 1, new List<ClausulaDeFiltrado>(), parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, true));
                DireccionDtm direccion = calles[0].CrearDireccion(enumCalificadorDireccion.contacto);
                cliente.AsignarDireccion(contexto, direccion);
                cliente.AsignarDireccion(contexto, cliente.Direccion(contexto, enumCalificadorDireccion.contacto), enumCalificadorDireccion.fiscal);

                //cargar la parametrización de remesas
                InzRemesasFae.ModeloDeRemesasFae(contexto);
                var factura1 = CrearFactura(contexto, cliente, "factura 1");
                var factura2 = CrearFactura(contexto, cliente, "factura 2");
                var factura3 = CrearFactura(contexto, cliente, "factura 3");

                //emitir tres facturas
                factura1.Transitar(contexto, InzFacturasEmt.n_tran_fae_emitir);
                GestorDeFacturasEmt.EmitirPdfFactura(contexto, factura1.MapearDto<FacturaEmtDto>(contexto));
                factura2.Transitar(contexto, InzFacturasEmt.n_tran_fae_emitir);
                GestorDeFacturasEmt.EmitirPdfFactura(contexto, factura2.MapearDto<FacturaEmtDto>(contexto));

                //emitir y corregir, validar que quita el archivo y lo regenera
                factura3 = factura3.Transitar(contexto, InzFacturasEmt.n_tran_fae_emitir);
                GestorDeFacturasEmt.EmitirPdfFactura(contexto, factura3.MapearDto<FacturaEmtDto>(contexto));
                factura3 = factura3.Recargar(contexto);
                var idarchivo = factura3.IdArchivo;
                factura3 = factura3.Transitar(contexto, InzFacturasEmt.n_tran_fae_corregir, parametros: new Dictionary<string, object>
                {
                    {ltrParametrosEp.detalleAsunto, "para volver a generar" }
                });
                factura3 = factura3.Transitar(contexto, InzFacturasEmt.n_tran_fae_emitir);
                GestorDeFacturasEmt.EmitirPdfFactura(contexto, factura3.MapearDto<FacturaEmtDto>(contexto));
                factura3 = factura3.Recargar(contexto);
                if (idarchivo >= factura3.IdArchivo) throw new Exception("El archivo generado para la factura tres no es correcto");

                //crear una remesa
                var remesa1 = CrearRemesa(contexto, enumClaseDeRemesaFae.Emitidas, "Mi primera remesa");

                //incluir las tres facturas en la remesa
                IntentarIncluirFactura(contexto, remesa1, factura1, "no tiene ninguna cuenta bancaria activa");


                var cb = new CuentaBancariaDtm { IsoPais = ltrIsoPaises.Spain, DcIban = "48", Entidad = "0049", Oficina = "2518", DcCcc = "63", Numero = "2216397739" }
                         .CrearSiNoExiste(contexto);
                
                new CuentaDeClienteDtm
                {
                    IdElemento = cliente.Id,
                    IdCuenta = cb.Id,
                    Clase = enumClaseDeCuentaBancaria.Pago,
                    IdArchivo = contexto.Set<ArchivoDtm>().First().Id,
                    Alias = "Cuenta de prueba"
                }.Insertar(contexto);
                new FacturaEmtDeUnaRemesaDtm { IdElemento = remesa1.Id, IdFactura = factura1.Id }.Insertar(contexto);
                new FacturaEmtDeUnaRemesaDtm { IdElemento = remesa1.Id, IdFactura = factura2.Id }.Insertar(contexto);
                new FacturaEmtDeUnaRemesaDtm { IdElemento = remesa1.Id, IdFactura = factura3.Id }.Insertar(contexto);

                //crear una segunda remesa , intentar incluir la primera de ellas y ver que falla
                var remesa2 = CrearRemesa(contexto, enumClaseDeRemesaFae.Emitidas, "Mi segunda remesa");
                IntentarIncluirFactura(contexto, remesa2, factura2, "No se puede incluir la factura");

                //transitar la remesa a generada
                remesa1 = remesa1.TransitarALaEtapa(contexto, enumEtapasDeRemesasFae.REM_Etapa_Generada.EstadosDeLaEtapa(), new Dictionary<string, object>(), delSistema: false);

                //intentar borrar el xml generado y ver que falla
                remesa1.IdArchivo = null;
                IntentarEjecutar(() => remesa1.Modificar(contexto), "y por tanto no es modificable");
                remesa1 = remesa1.Transitar(contexto, InzRemesasFae.n_tran_rem_corregir, parametros: new Dictionary<string, object>
                {
                    {ltrParametrosEp.detalleAsunto, "para volver a generar" }
                });
                if (remesa1.IdArchivo != null) throw new Exception("No debería haber q19");

                //intentar cargar la remesa y ver que falla
                IntentarEjecutar(() => remesa1.Cargar(contexto, DateTime.Now), "No se puede cargar la remesa");

                //pasar la remesa a presentada                
                remesa1.Transitar(contexto, InzRemesasFae.n_tran_rem_generar).Transitar(contexto, InzRemesasFae.n_tran_rem_presentar);

                //ejecutar el trabajo sometido
                TrabajosDeRemesasFae.SometerProcesosDeRemesasFae(contexto).EjecutarTrabajo(contexto);

                //cargar manualmente la remesa por el interventor y ver que la carga
                remesa1 = remesa1.Recargar(contexto);
                remesa1.Cargar(contexto, DateTime.Now);
                var roles = contexto.LeerRoles().ToList();

                //montar seguridad por tipo, estados y cg
                var tipoRemesa = contexto.SeleccionarPorPropiedad<TipoDeRemesaFaeDtm>(nameof(INombre.Nombre), InzRemesasFae.n_rem_tipo_general);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), roles[0].Id, tipoRemesa.IdPermisoDeGestor);
                GestorDeTiposDeArchivadores.GenerarSeguridad(contexto);
                var tipoFactura = contexto.SeleccionarPorPropiedad<TipoDeFacturaEmtDtm>(nameof(INombre.Nombre), InzFacturasEmt.n_fae_tipo_general, aplicarJoin: true);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), roles[0].Id, tipoRemesa.IdPermisoInterventor);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), roles[0].Id, tipoFactura.IdPermisoDeConsultor);
                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), roles[0].Id, tipoFactura.IdPermisoDeGestor);
                GestorDeTiposDeArchivadores.GenerarSeguridad(contexto);

                contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), roles[0].Id, contexto.SeleccionarEstado<EstadoDeUnaRemesaFaeDtm>(InzRemesasFae.n_estado_rem_presentada).IdPermiso);
                GestorDeEstados.GenerarSeguridad(contexto);
 
                roles[0].AsignarAccesoAlCgDe(contexto, remesa1, enumModoDeAccesoDeDatos.Gestor);
                roles[0].AsignarAccesoAlCgDe(contexto, factura3, enumModoDeAccesoDeDatos.Gestor);
                GestorDeCentrosGestores.GenerarSeguridad(contexto);

                var usuario = contexto.AsignarUsuario(InzAcromur.n_usuario_contable);
                if (!PermisosPorTipoSql.UsuarioConAlgunPermiso(contexto, usuario.Id, new List<int> { tipoFactura.IdPermisoDeConsultor }))
                    throw new Exception($"El usuario {usuario.Login} debería tener el permiso {tipoFactura.PermisoDeConsultor.Nombre}");
                if (!PermisosPorTipoSql.UsuarioConAlgunPermiso(contexto, usuario.Id, new List<int> { tipoRemesa.IdPermisoInterventor }))
                    throw new Exception($"El usuario {usuario.Login} debería tener el permiso {tipoRemesa.PermisoDeInterventor.Nombre}");

                //anular cargo con el contable
                remesa1.AnularCargo(contexto, DateTime.Now);

                //sacar una factura de la remesa y comprobar que el estado de la factura es devuelta
                remesa1.DevolverFactura(contexto,factura3,DateTime.Now, "para probar");
                factura3 = factura3.Recargar(contexto);
                if (!factura3.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Devuelta)) throw new Exception("La factura debia estar devuelta");

                //Volverla a incluir anulando la fecha de devolución y comprobar que su estado es remesada
                remesa1.AnularDevolucionDeFactura(contexto, factura3, "para probar 2");
                factura3 = factura3.Recargar(contexto);
                if (!factura3.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Remesada)) throw new Exception("La factura debia estar remesada");

                //cargar la remesa 
                remesa1.Cargar(contexto, DateTime.Now);

                //verificar que las tres facturas están cobradas
                if (!factura1.Recargar(contexto).EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada)) throw new Exception("La factura 1 debía estar cobrada");
                if (!factura2.Recargar(contexto).EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada)) throw new Exception("La factura 2 debía estar cobrada");
                if (!factura3.Recargar(contexto).EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada)) throw new Exception("La factura 3 debía estar cobrada");

                //intentar borrar el cobro y verificar que no deja
                IntentarEjecutar(()=> factura3.UltimoCobro(contexto).Eliminar(contexto, new Dictionary<string, object> { { ltrParametrosNeg.EsUnaPeticion, true } }), "No se pueden crear o borrar cobros de remesas");

                //intentar borrar la factura de la remesa y comprobar que no deja
                IntentarEjecutar(() => contexto.EliminarPorId<FacturaEmtDeUnaRemesaDtm>(remesa1.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto)[0].Id), "por no estar en la etapa de");

            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        private static void IntentarEjecutar(Action accion, string mensaje)
        {
            try
            {
                accion();
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains(mensaje)) throw;
            }
        }

        private static void IntentarIncluirFactura(ContextoSe contexto, RemesaFaeDtm remesa, FacturaEmtDtm factura, string mensaje)
        {
            try
            {
                new FacturaEmtDeUnaRemesaDtm { IdElemento = remesa.Id, IdFactura = factura.Id }.Insertar(contexto);
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains(mensaje)) throw;
            }
        }

        private static RemesaFaeDtm CrearRemesa(ContextoSe contexto, enumClaseDeRemesaFae clase, string nombre)
        {
            var sociedad = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), InzAcromur.n_nif_acromur);
            var cuentaDeSociedad = contexto.SeleccionarPorFk<CuentaDeMiSociedadDtm>(nameof(CuentaDeMiSociedadDtm.IdElemento), sociedad.Id, errorSiMasDeuno: false);
            var tipo = contexto.SeleccionarPorPropiedad<TipoDeRemesaFaeDtm>(nameof(TipoDeRemesaFaeDtm.Nombre), InzRemesasFae.n_rem_tipo_general);
            var remesa = new RemesaFaeDtm
            {
                IdCg = contexto.SeleccionarDtoPorAk<CentroGestorDto, CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), InzAcromur.n_cg_acm_codigo).Id,
                IdTipo = tipo.Id,
                Clase = clase,
                Nombre = nombre,
                Acreedor = sociedad.Expresion,
                NifDelAcreedor = sociedad.NIF,
                SufijoAcreedor = "000",
                Presentador = "Juan Jiménez",
                NifDelPresentador = "27485405Z",
                SufijoPresentador = "000",
                IdCuentaDeAbono = cuentaDeSociedad.Id
            }.Insertar(contexto);

            return remesa;
        }

        private static FacturaEmtDtm CrearFactura(ContextoSe contexto, ClienteDtm cliente, string nombre)
        {
            var f = new FacturaEmtDtm
            {
                IdCg = contexto.SeleccionarDtoPorAk<CentroGestorDto, CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), InzAcromur.n_cg_acm_codigo).Id,
                IdTipo = contexto.SeleccionarPorPropiedad<TipoDeFacturaEmtDtm>(nameof(TipoDeFacturaEmtDtm.Nombre), InzFacturasEmt.n_fae_tipo_general).Id,
                Nombre = nombre,
                IdCliente = cliente.Id,
                Contacto = cliente.Expresion,
                Telefono = cliente.Telefono,
                eMail = cliente.eMail
            }.Insertar(contexto);
            CrearLinea(contexto, f);
            var periodo = f.Periodo(contexto);
            periodo.Inicio = DateTime.Now;
            periodo.Fin = DateTime.Now.AddDays(30);
            periodo.Modificar(contexto);
            return f;
        }

        private static LineaDeUnaFaeDtm CrearLinea(ContextoSe contexto, FacturaEmtDtm factura, int orden = 10)
        {
            var iva = contexto.SeleccionarPorNombre<IvaRepercutidoDtm>(InzMaestros.n_iva_general);
            return new LineaDeUnaFaeDtm
            {
                IdElemento = factura.Id,
                Orden = orden,
                TipoDeLinea = enumTipoDeLinea.Alzada,
                Concepto = "trabajito",
                Cantidad = 1,
                Precio = 100,
                Descuento = 10,
                IdIvaR = iva.Id,
                Iva = iva.Porcentaje,
                Clase = enumClaseUnitario.Servicio,
                IdUnidad = contexto.SeleccionarPorPropiedad<UnidadDtm>(nameof(UnidadDtm.Sigla), InzMaestros.n_unidad_ud).Id,
                IdNaturaleza = contexto.SeleccionarPorPropiedad<NaturalezaDtm>(nameof(NaturalezaDtm.Sigla), InzMaestros.n_natu_consultoria).Id
            }.Insertar(contexto);

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
