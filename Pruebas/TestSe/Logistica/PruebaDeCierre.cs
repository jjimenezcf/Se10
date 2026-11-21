using System;
using System.Linq;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Contabilidad;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;

namespace TestLogistica
{
    // Cierre contable de la sociedad tras una compra contra un almacén con o sin variación de existencias. Un cierre no puede ser posterior a
    // hoy y toma lo anterior a la fecha de cierre, así que la compra (inventario inicial, pedido, entregas y devolución) se hace con fecha de
    // ayer y se cierra hasta hoy.
    // El cierre se somete como desde la sociedad (TrabajosContables.SometerCrearCierreContable) y se ejecuta su trabajo
    // (TrabajosContables.CrearCierreContable) en la transacción de la prueba (ver EjecutorDeTrabajos): valida los almacenes, cuadra las
    // existencias de los que usan la variación, crea el lote con los preasientos pendientes y registra el cierre.
    //  - con variación: un preasiento de cuadre del almacén con fecha contable ayer por lo que ha variado el inventario sin contar el de
    //    apertura (= lo comprado menos lo devuelto), existencias al debe y variación al haber si sube
    //  - sin variación: el almacén no se cuadra, sus entregas y devoluciones ya están en la cuenta de existencias
    // En ambos casos los preasientos de la compra quedan contabilizados en el lote del cierre.
    // El fichero del lote queda en el servidor documental: el rollback sólo deshace su registro en la BD
    public class PruebaDeCierre
    {
        private readonly ConfigDeLogistica _config;
        private readonly bool _conVariacion;

        public string Nombre => $"Cierre contable {(_conVariacion ? "con" : "sin")} variación de existencias";

        public PruebaDeCierre(ConfigDeLogistica config, bool conVariacion)
        {
            _config = config;
            _conVariacion = conVariacion;
        }

        public void Ejecutar(ContextoSe contexto, Informe informe)
        {
            var ayer = DateTime.Today.AddDays(-1);
            var hoy = DateTime.Today;
            var sociedad = contexto.SeleccionarPorId<SociedadDtm>(_config.IdSociedad, usarLaCache: false);

            // la compra y el inventario inicial han de ser posteriores al último cierre
            var desde = GestorDeCierresContables.FechaDelUltimoCierre(contexto, sociedad.Id);
            if (!informe.Comprobar(ayer >= desde, $"el último cierre de la sociedad ({desde:dd/MM/yyyy}) permite comprar el {ayer:dd/MM/yyyy}"))
                return;

            var compras = new PruebaDeCompras(_config, _conVariacion, fechaDeLaCompra: ayer);
            compras.Ejecutar(contexto, informe);
            var almacen = compras.Almacen;
            var preasientosDeLaCompra = compras.PreasientosDeLaCompra();

            //---------------------------------------------------------------------------------------------------- someter
            informe.Paso($"Someter el cierre contable de la sociedad {sociedad.Id} hasta el {hoy:dd/MM/yyyy}");
            var (sistema, digitos) = sociedad.SistemaYDigitosParaContabilizar();
            if (!informe.Comprobar(sistema is not null, "la sociedad tiene sistema contable", $"configure '{enumParametrosDePreasiento.SPR_Contabilizar_En}'"))
                return;
            var trabajo = TrabajosContables.SometerCrearCierreContable(contexto, sociedad.Id, hoy, (enumSistemaContable)sistema, digitos);

            //---------------------------------------------------------------------------------------------------- ejecutar
            informe.Paso("Ejecutar el trabajo del cierre");
            var ultimoPreasiento = contexto.Set<PreasientoDtm>().Max(p => (int?)p.Id) ?? 0;
            var resultado = EjecutorDeTrabajos.Ejecutar(contexto, trabajo);
            foreach (var traza in resultado.Trazas)
                informe.Dato(traza);
            foreach (var error in resultado.Errores)
                informe.Comprobar(false, "error del trabajo", error);

            var cierre = contexto.Set<CierreContableDtm>().Where(c => c.IdSociedad == sociedad.Id && c.CerradaEl == hoy).OrderByDescending(c => c.Id).FirstOrDefault();
            if (!informe.Comprobar(cierre is not null, $"cierre registrado hasta el {hoy:dd/MM/yyyy}", Ultima(resultado)))
                return;
            var lote = contexto.SeleccionarPorId<CircuitoDocDtm>(cierre.IdLote, usarLaCache: false);
            informe.Dato($"cierre '{cierre.Nombre}', lote '{lote.Referencia}'");

            //---------------------------------------------------------------------------------------------------- cuadre
            informe.Paso(_conVariacion
                ? "Validar el cuadre de existencias del almacén (preasiento de variación de existencias)"
                : "Validar que el almacén no se cuadra");
            var marca = $"almacén '{almacen.Referencia}'";
            var cuadres = contexto.Set<PreasientoDtm>().Where(p => p.Id > ultimoPreasiento && p.Nombre.Contains(marca)).ToList();
            if (_conVariacion)
                ValidarElCuadre(contexto, informe, compras, cuadres, ayer);
            else
                informe.Comprobar(cuadres.Count == 0, $"ningún preasiento de cuadre del {marca}", $"hay {cuadres.Count}");

            //---------------------------------------------------------------------------------------------------- contabilizados
            informe.Paso("Validar que los preasientos de la compra están contabilizados");
            var contabilizados = preasientosDeLaCompra.Concat(cuadres.Select(c => c.Id)).ToList();
            informe.Comprobar(contabilizados.Count > 0, "la compra tiene preasientos");
            foreach (var id in contabilizados)
            {
                var preasiento = contexto.SeleccionarPorId<PreasientoDtm>(id, usarLaCache: false);
                informe.Comprobar(preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Contabilizado), $"preasiento {preasiento.Referencia} contabilizado",
                    $"está en '{preasiento.Estado(contexto).Nombre}'");
            }
        }

        private static void ValidarElCuadre(ContextoSe contexto, Informe informe, PruebaDeCompras compras, System.Collections.Generic.List<PreasientoDtm> cuadres, DateTime ayer)
        {
            var almacen = compras.Almacen;
            if (!informe.Comprobar(cuadres.Count == 1, $"un preasiento de cuadre del almacén '{almacen.Referencia}'", $"hay {cuadres.Count}"))
                return;

            var cuadre = cuadres[0];
            informe.Comprobar(cuadre.FechaContable.Date == ayer, $"preasiento {cuadre.Referencia} con fecha contable {ayer:dd/MM/yyyy}", $"es {cuadre.FechaContable:dd/MM/yyyy}");

            var apuntes = contexto.Set<ApunteDeUnPreasientoDtm>().Where(a => a.IdElemento == cuadre.Id).ToList();
            if (!informe.Comprobar(apuntes.Count == 2, $"preasiento {cuadre.Referencia} con 2 apuntes", $"tiene {apuntes.Count}"))
                return;

            // si el inventario sube, existencias al debe y variación al haber; si baja, al revés
            var existencias = almacen.CuentaDeExistencias(contexto).Codigo;
            var variacion = almacen.CuentaDeVariacion(contexto).Codigo;
            var esperado = compras.ImporteNetoDeLaCompra();
            var (alDebe, alHaber) = esperado >= 0 ? (existencias, variacion) : (variacion, existencias);
            var importe = Math.Abs(esperado);
            var debe = apuntes.First(a => a.Posicion == enumPosicionContable.Debe);
            var haber = apuntes.First(a => a.Posicion == enumPosicionContable.Haber);
            informe.Comprobar(debe.Cuenta == alDebe && debe.Importe == importe, $"debe {alDebe} {importe:N2}", $"es {debe.Cuenta} {debe.Importe:N2}");
            informe.Comprobar(haber.Cuenta == alHaber && haber.Importe == importe, $"haber {alHaber} {importe:N2}", $"es {haber.Cuenta} {haber.Importe:N2}");
        }

        // el trabajo termina sin error y sin cierre si no hay nada que cerrar o la fecha no es válida: lo dice su última traza
        private static string Ultima(EjecutorDeTrabajos.Resultado resultado) =>
            resultado.Trazas.Count == 0 ? "el trabajo no ha dejado trazas" : $"última traza del trabajo: {resultado.Trazas.Last()}";
    }
}
