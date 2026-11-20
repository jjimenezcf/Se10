using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Contabilidad
{
    // Cuadre de existencias de los almacenes que usan la variación de existencias (ver AlmacenDtm.UsaVariacionDeExistencias): sus compras van
    // al gasto y sus movimientos no se preasientan, así que antes de crear el lote de cada cierre contable de la sociedad se cuadra su cuenta
    // de existencias contra la de variación por lo que ha variado su inventario en el periodo que se cierra
    public static class CuadreDeExistencias
    {
        // Si la sociedad genera preasientos de almacén, sus almacenes con movimientos en el periodo han de tener la información contable
        // definida: sin ella no se sabe si se cuadran
        public static void ValidarAlmacenesAlCerrar(ContextoSe contexto, SociedadDtm sociedad, DateTime desde, DateTime cerrarHasta)
        {
            if (sociedad.Autonomo || !sociedad.UsaPreasientos(contexto, enumNegocio.Almacen))
                return;

            var sinDefinir = AlmacenesDeLaSociedad(contexto, sociedad)
                .Where(a => a.IdCuentaDeExistencias == null
                         && contexto.Set<MovimientoDeAlmacenDtm>().Any(m => m.IdAlmacen == a.Id && m.RealizadoEl >= desde.Date && m.RealizadoEl < cerrarHasta.Date))
                .Select(a => a.Referencia)
                .ToList();
            if (sinDefinir.Count > 0)
                GestorDeErrores.Emitir($"No se puede cerrar la sociedad '{sociedad.Expresion}': {(sinDefinir.Count == 1 ? "el almacén" : "los almacenes")} " +
                                       $"'{string.Join("', '", sinDefinir)}' {(sinDefinir.Count == 1 ? "tiene" : "tienen")} movimientos en el periodo y no {(sinDefinir.Count == 1 ? "tiene" : "tienen")} definida su información contable");
        }

        // Si al cerrar la sociedad hay algún almacén cuyas existencias se cuadran: genera preasientos de almacén y usa la variación de existencias
        public static bool CuadraAlCerrar(ContextoSe contexto, SociedadDtm sociedad) =>
            !sociedad.Autonomo && sociedad.UsaPreasientos(contexto, enumNegocio.Almacen) && AlmacenesQueCuadran(contexto, sociedad).Any();

        // Antes de crear el lote del cierre, un preasiento por almacén que usa la variación de existencias, con fecha contable el último día
        // cerrado para que entre en el lote, por la variación de su inventario en el periodo:
        //   inventario a la fecha de cierre (movimientos anteriores a ella)
        //   - inventario al último cierre (o al 1 de enero si no hay), que es lo que tiene la cuenta de existencias tras el cuadre anterior
        //   - inventarios de apertura del periodo, que ya están en la contabilidad como saldos de apertura (ver ValorDeLaAperturaEntre)
        // Si sube, al debe la cuenta de existencias y al haber la de variación; si baja, al revés
        public static void CuadrarExistenciasAlCerrar(ContextoSe contexto, SociedadDtm sociedad, DateTime desde, DateTime cerrarHasta, Action<string> anotar)
        {
            if (sociedad.Autonomo || !sociedad.UsaPreasientos(contexto, enumNegocio.Almacen))
                return;

            var fecha = cerrarHasta.Date.AddDays(-1);
            foreach (var almacen in AlmacenesQueCuadran(contexto, sociedad))
            {
                var anterior = almacen.ValorDelInventario(contexto, antesDe: desde.Date);
                var apertura = almacen.ValorDeLaAperturaEntre(contexto, desde.Date, cerrarHasta.Date);
                var actual = almacen.ValorDelInventario(contexto, antesDe: cerrarHasta.Date);
                var variacion = Math.Round(actual - anterior - apertura, 2);

                var concepto = $"Variación de existencias del almacén '{almacen.Referencia}': inventario a {fecha:dd/MM/yyyy} {actual.ToMoneda()} contra {anterior.ToMoneda()} a {desde.Date.AddDays(-1):dd/MM/yyyy}" +
                               (apertura == 0 ? "" : $" más {apertura.ToMoneda()} de inventarios de apertura");
                var cuadres = new List<(bool existenciasAlDebe, decimal importe, string concepto)> { (variacion > 0, Math.Abs(variacion), concepto) };
                var preasiento = almacen.PreasentarCuadreDeExistencias(contexto, sociedad, $"Cuadre de existencias al cerrar: almacén '{almacen.Referencia}'", cuadres, fecha);
                anotar(preasiento is null
                    ? $"Almacén '{almacen.Referencia}': el inventario no ha variado ({actual.ToMoneda()}), no se cuadran las existencias"
                    : $"Almacén '{almacen.Referencia}': preasiento nº {preasiento.Id} de variación de existencias por {Math.Abs(variacion).ToMoneda()} ({concepto})");
            }
        }

        private static IQueryable<AlmacenDtm> AlmacenesDeLaSociedad(ContextoSe contexto, SociedadDtm sociedad)
        {
            var cgsDeLaSociedad = contexto.Set<CentroGestorDtm>().Where(cg => cg.IdSociedad == sociedad.Id).Select(cg => cg.Id);
            return contexto.Set<AlmacenDtm>().Where(a => cgsDeLaSociedad.Contains(a.IdCg));
        }

        private static List<AlmacenDtm> AlmacenesQueCuadran(ContextoSe contexto, SociedadDtm sociedad) =>
            AlmacenesDeLaSociedad(contexto, sociedad).Where(a => a.IdCuentaDeExistencias != null && a.UsaVariacionDeExistencias).ToList();
    }
}
