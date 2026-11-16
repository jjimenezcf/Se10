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
    // Cambio del método de contabilización de las compras de almacén de una sociedad (parámetro SOCIEDAD_USA_VARIACION_DE_EXISTENCIA, se
    // aplica antes de grabarlo, ver GestorDeParametrosDeNegocio.AntesDePersistir):
    //  - con variación de existencias la entrega va al gasto (grupo 6) contra albaranes (4009) y la cuenta de existencias (311) sólo se toca
    //    a fin de ejercicio; sin ella la entrega va a la 311 contra la 4009 (ver ExtensorDePreasientos.PreasentarCompraDeAlmacen)
    //  - al cambiar se cuadra la 311 contra la de variación de existencias (611) con un preasiento por almacén de la sociedad, referenciado
    //    a la sociedad y con el tipo de preasiento de almacén, y se anota una traza en la sociedad
    //  - sólo se permite si todos los preasientos de la sociedad están contabilizados
    public static class CambioDeVariacionDeExistencias
    {
        public const string TrazaDeActivar = "Se activa la variación de existencias en la contabilización de las compras";
        public const string TrazaDeDesactivar = "Se desactiva la variación de existencias en la contabilización de las compras";

        // Valida y aplica el cambio a las sociedades que pasan a usar o dejan de usar la variación de existencias entre los dos json
        public static void Aplicar(ContextoSe contexto, string jsonAnterior, string jsonNuevo)
        {
            var antes = ParametrosDeSociedades.SociedadesQueUsanVariacionDeExistencias(jsonAnterior);
            var despues = ParametrosDeSociedades.SociedadesQueUsanVariacionDeExistencias(jsonNuevo);

            foreach (var idSociedad in antes.Except(despues))
                Cambiar(contexto, idSociedad, activar: false);

            foreach (var idSociedad in despues.Except(antes))
                Cambiar(contexto, idSociedad, activar: true);
        }

        private static void Cambiar(ContextoSe contexto, int idSociedad, bool activar)
        {
            var sociedad = contexto.SeleccionarPorId<SociedadDtm>(idSociedad);
            var cgsDeLaSociedad = contexto.Set<CentroGestorDtm>().Where(cg => cg.IdSociedad == idSociedad).Select(cg => cg.Id);

            ValidarQueEstaTodoContabilizado(contexto, sociedad, cgsDeLaSociedad, activar);

            var hoy = DateTime.Today;
            var detalle = new List<string>
            {
                activar
                ? "Las compras de almacén pasan a contabilizarse al gasto de la naturaleza contra albaranes (variación de existencias)"
                : $"Las compras de almacén pasan a contabilizarse contra la cuenta de existencias ({VariablesDeCuentas.Existencias})"
            };

            if (sociedad.Autonomo || !sociedad.UsaPreasientos(contexto, enumNegocio.Almacen))
                detalle.Add("La sociedad no genera preasientos de almacén: no se cuadran las existencias");
            else
                detalle.AddRange(CuadrarExistencias(contexto, sociedad, cgsDeLaSociedad, activar, hoy));

            sociedad.CrearTraza(contexto, activar ? TrazaDeActivar : TrazaDeDesactivar, string.Join(Environment.NewLine, detalle));
        }

        // Para cambiar de método la 311 ha de estar cuadrada con lo contabilizado: no puede quedar ningún preasiento de la sociedad pendiente
        private static void ValidarQueEstaTodoContabilizado(ContextoSe contexto, SociedadDtm sociedad, IQueryable<int> cgsDeLaSociedad, bool activar)
        {
            var pendientes = enumEtapasDePreasiento.SPR_Etapa_Pendiente.Lista();
            var cantidad = contexto.Set<PreasientoDtm>().Count(p => cgsDeLaSociedad.Contains(p.IdCg) && pendientes.Contains(p.IdEstado));
            if (cantidad > 0)
                GestorDeErrores.Emitir($"No se puede {(activar ? "activar" : "desactivar")} la variación de existencias de la sociedad '{sociedad.Expresion}': " +
                                       $"tiene {cantidad} preasiento{(cantidad == 1 ? "" : "s")} {enumEtapasDePreasiento.SPR_Etapa_Pendiente.Nombre()}, han de estar todos contabilizados");
        }

        // Un preasiento por almacén de la sociedad con existencias que cuadrar:
        //  - al activarla (de 311 a variación) la 311 tiene el inventario de hoy (Vi): se deja a 0, al debe la 611 y al haber la 311 por Vi;
        //    a fin de ejercicio se carga en la 311 el inventario final contra la 611
        //  - al desactivarla (de variación a 311) la 311 tiene las existencias iniciales del periodo con variación (E0): el inventario a 31/12
        //    del ejercicio anterior o 0 si se activó en este ejercicio (el cuadre de la activación la dejó a 0). Se dan de baja (al debe la 611
        //    y al haber la 311 por E0) y se carga el inventario de hoy (al debe la 311 y al haber la 611 por Vi): la 311 queda en Vi y la 611
        //    recoge la variación hasta hoy
        private static List<string> CuadrarExistencias(ContextoSe contexto, SociedadDtm sociedad, IQueryable<int> cgsDeLaSociedad, bool activar, DateTime hoy)
        {
            var inicioDelEjercicio = new DateTime(hoy.Year, 1, 1);
            var activadaEnElEjercicio = !activar && contexto.Set<TrazasDeUnaSociedadDtm>()
                .Any(t => t.IdElemento == sociedad.Id && t.Nombre == TrazaDeActivar && t.CreadaEl >= inicioDelEjercicio);

            var nombre = $"Cuadre de existencias al {(activar ? "activar" : "desactivar")} la variación de existencias";
            var detalle = new List<string>();
            foreach (var almacen in contexto.Set<AlmacenDtm>().Where(a => cgsDeLaSociedad.Contains(a.IdCg)).ToList())
            {
                var inventarioDeHoy = almacen.ValorDelInventario(contexto, antesDe: hoy.AddDays(1));
                var cuadres = new List<(bool existenciasAlDebe, decimal importe, string concepto)>();
                if (activar)
                    cuadres.Add((false, inventarioDeHoy, $"Inventario del almacén '{almacen.Referencia}' a {hoy:dd/MM/yyyy}"));
                else
                {
                    var existenciasIniciales = activadaEnElEjercicio ? 0 : almacen.ValorDelInventario(contexto, antesDe: inicioDelEjercicio);
                    cuadres.Add((false, existenciasIniciales, $"Baja de las existencias iniciales del almacén '{almacen.Referencia}' a {inicioDelEjercicio.AddDays(-1):dd/MM/yyyy}"));
                    cuadres.Add((true, inventarioDeHoy, $"Inventario del almacén '{almacen.Referencia}' a {hoy:dd/MM/yyyy}"));
                }

                var preasiento = almacen.PreasentarCuadreDeExistencias(contexto, sociedad, $"{nombre}: almacén '{almacen.Referencia}'", cuadres);
                detalle.Add(preasiento is null
                    ? $"Almacén '{almacen.Referencia}': sin existencias que cuadrar"
                    : $"Almacén '{almacen.Referencia}': preasiento nº {preasiento.Id} ({string.Join(", ", cuadres.Where(c => c.importe != 0).Select(c => $"{(c.existenciasAlDebe ? "debe 311 / haber 611" : "debe 611 / haber 311")} por {c.importe.ToMoneda()}"))})");
            }

            if (detalle.Count == 0)
                detalle.Add("La sociedad no tiene almacenes: no se cuadran las existencias");
            else if (activar)
                detalle.Add("A fin de ejercicio se ha de cargar en la cuenta de existencias el inventario final contra la de variación de existencias, cuadrándola contra su saldo real");
            return detalle;
        }
    }
}
