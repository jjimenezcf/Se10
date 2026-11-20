using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.MaestrosTecnico;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeRegularizaciones
    {
        public static void ValidarQueNoHayaRegularizacionViva(this AlmacenDtm almacen, ContextoSe contexto)
        {

            if (!almacen.EstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_Activo))
                GestorDeErrores.Emitir($"No se puede crear la regularización porque el almacén '{almacen.Referencia}' no está activo");

            var estadosIniciada = enumEtapasDeRegularizacion.RAL_Inicial.Lista();
            var estadosRecontando = enumEtapasDeRegularizacion.RAL_Recontando.Lista();

            if (!estadosIniciada.Any())
                GestorDeErrores.Emitir($"Debe definir el parámetro '{enumEtapasDeRegularizacion.RAL_Inicial}' del negocio de '{enumNegocio.Regularizacion}'");

            if (!estadosRecontando.Any())
                GestorDeErrores.Emitir($"Debe definir el parámetro '{enumEtapasDeRegularizacion.RAL_Recontando}' del negocio de '{enumNegocio.Regularizacion}'");

            var estadosVivos = estadosIniciada.Union(estadosRecontando).ToList();
            var hayRegularizacionViva = contexto.Set<RegularizacionDtm>().Any(x => x.IdAlmacen == almacen.Id && estadosVivos.Contains(x.IdEstado));
            if (hayRegularizacionViva)
                GestorDeErrores.Emitir($"No se puede crear la regularización porque el almacén '{almacen.Referencia}' ya tiene una regularización en curso");

        }

        public static int IncrementarOrdenEn(ContextoSe contexto)
        {
            var incremento = enumNegocio.Regularizacion.LeerCrearParametro(contexto, enumParametrosDeRegularizaciones.RAL_IncrementarOrdenEn, "10");
            if (incremento is null || incremento.Valor.Entero() == 0)
                GestorDeErrores.Emitir($"Ha de definir el parámetro '{enumParametrosDeRegularizaciones.RAL_IncrementarOrdenEn}' del negocio de '{enumNegocio.Regularizacion.Singular()}' con un valor mayor de 0");
            return incremento.Valor.Entero();
        }

        public static AlmacenDtm Almacen(this RegularizacionDtm regularizacion, ContextoSe contexto)
        {
            if (regularizacion.Almacen is not null && regularizacion.IdAlmacen == regularizacion.Almacen.Id)
            {
                return regularizacion.Almacen;
            }

            return regularizacion.Almacen = contexto.SeleccionarPorId<AlmacenDtm>(regularizacion.IdAlmacen);
        }

        public static void AntesDeIniciarElRecuento(this RegularizacionDtm regularizacion, ContextoSe contexto)
        {
            var almacen = regularizacion.Almacen(contexto);

            if (!almacen.EstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_Activo))
                GestorDeErrores.Emitir($"No se puede iniciar un recuento/inventario si el almacén no está en la etapa {enumEtapasDeAlmacen.ALM_Etapa_Activo.Nombre()}");

            almacen.TransitarALaEtapa(contexto, enumEtapasDeAlmacen.ALM_Etapa_En_Inventario.EstadosDeLaEtapa(), delSistema: true);
        }

        // El recuento no se aplica al transitar: es un trabajo en segundo plano (ver TrabajosParaRegularizaciones.AplicarRecuento) que, al
        // terminar, transita la regularización a aplicada. Sólo se deja pasar la transición del propio trabajo
        public static void AntesDeCerrarElRecuento(this RegularizacionDtm regularizacion, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (!parametros.LeerValor(ltrDeUnaRegularizacion.AplicandoElRecuento, false))
                GestorDeErrores.Emitir($"El recuento de la regularización '{regularizacion.Referencia}' no se aplica con esta transición: utilice la opción 'Aplicar recuento' del menú, que lo aplica en segundo plano y, al terminar, pasa la regularización a aplicada");
        }

        // Validaciones previas a someter el trabajo que aplica el recuento
        public static void ValidarQueSePuedeAplicarElRecuento(this RegularizacionDtm regularizacion, ContextoSe contexto)
        {
            if (!regularizacion.EstaEnLaEtapa(enumEtapasDeRegularizacion.RAL_Recontando))
                GestorDeErrores.Emitir($"La regularización '{regularizacion.Referencia}' ha de estar {enumEtapasDeRegularizacion.RAL_Recontando.Nombre()} para aplicar el recuento");

            var fechas = contexto.Set<LineasDeUnaRegularizacionDtm>().Where(x => x.IdElemento == regularizacion.Id).Select(x => x.Fecha).ToList();
            if (fechas.Count == 0)
                GestorDeErrores.Emitir($"La regularización '{regularizacion.Referencia}' no tiene líneas de recuento que aplicar");

            foreach (var fecha in fechas.Distinct())
                regularizacion.ValidarLaFechaDelRecuento(contexto, fecha);
        }

        // La fecha de una línea no puede ser posterior a hoy ni anterior al último cierre contable de la sociedad del almacén (ya no se
        // pueden generar preasientos en un periodo cerrado)
        public static void ValidarLaFechaDelRecuento(this RegularizacionDtm regularizacion, ContextoSe contexto, DateTime fecha)
        {
            if (fecha.Date == default)
                GestorDeErrores.Emitir("Debe indicar la fecha del recuento");

            if (fecha.Date > DateTime.Today)
                GestorDeErrores.Emitir($"La fecha del recuento '{fecha:dd/MM/yyyy}' no puede ser posterior a hoy");

            var sociedad = regularizacion.Almacen(contexto).Sociedad(contexto);
            var ultimoCierre = ExtensorDeSociedades.UltimoCierreContable(contexto, sociedad.Id)?.CerradaEl.Date ?? new DateTime(DateTime.Now.Year, 1, 1);
            if (fecha.Date < ultimoCierre)
                GestorDeErrores.Emitir($"La fecha del recuento '{fecha:dd/MM/yyyy}' es anterior al último cierre contable de la sociedad '{sociedad.Expresion}' ({ultimoCierre:dd/MM/yyyy})");
        }

        public static void ValidarQueNoHayaLineasDeRecuento(this RegularizacionDtm regularizacion, ContextoSe contexto)
        {
            var hayLineas = contexto.Set<LineasDeUnaRegularizacionDtm>().Any(x => x.IdElemento == regularizacion.Id);
            if (hayLineas)
                GestorDeErrores.Emitir($"No se puede devolver la regularización '{regularizacion.Referencia}' a la etapa inicial porque tiene líneas de recuento");
        }

        // Genera los movimientos de almacén de las líneas de la regularización, cada uno a las 00:00 de la fecha de su línea (el almacén está
        // cerrado mientras se cuenta, ese día no hubo movimientos). Cada alta barre los movimientos posteriores del material. Debe llamarse dentro de
        // una transacción, que el llamador deshace si falla alguna línea. Devuelve el nº de movimientos creados
        //  - inventario inicial: un stock inicial por línea
        //  - recuento: la cantidad contada es el stock del material a esa fecha, se genera un incremento o un decremento si difiere del que
        //    había y, si el precio de la línea difiere del precio medio, un ajuste de precio (dos movimientos si cambian ambos); si el material
        //    no tenía movimientos, se crea como stock inicial. Los materiales que no están en el recuento no se tocan
        //  - ajuste de precio: sólo el ajuste del precio medio al de la línea
        public static int AplicarElRecuento(this RegularizacionDtm regularizacion, ContextoSe contexto, Action<string> traza = null)
        {
            var clase = ((TipoDeRegularizacionDtm)regularizacion.Tipo(contexto)).ClaseDeRegularizacion;
            var lineas = contexto.Set<LineasDeUnaRegularizacionDtm>()
                .Where(x => x.IdElemento == regularizacion.Id)
                .OrderBy(x => x.Fecha).ThenBy(x => x.Orden)
                .ToList();
            if (lineas.Count == 0)
                GestorDeErrores.Emitir($"La regularización '{regularizacion.Referencia}' no tiene líneas de recuento que aplicar");

            var tipos = new Dictionary<enumTiposDeMovimiento, int>();
            int Tipo(enumTiposDeMovimiento tipo)
            {
                if (!tipos.ContainsKey(tipo)) tipos[tipo] = tipo.LeerTipoDeMovimiento(contexto).Id;
                return tipos[tipo];
            }

            var creados = 0;
            void Crear(LineasDeUnaRegularizacionDtm linea, enumTiposDeMovimiento tipo, decimal cantidad, decimal precio)
            {
                new MovimientoDeAlmacenDtm
                {
                    IdAlmacen = regularizacion.IdAlmacen,
                    IdUnitario = linea.IdUnitario,
                    IdTipoMovimiento = Tipo(tipo),
                    Cantidad = cantidad,
                    Precio = precio,
                    RealizadoEl = linea.Fecha.Date,
                    IdLineaInventario = linea.Id
                }.InsertarComoAdministrador(contexto);
                creados++;
            }

            foreach (var linea in lineas)
            {
                try
                {
                    regularizacion.ValidarLaFechaDelRecuento(contexto, linea.Fecha);
                    var anterior = ExtensorDeAlmacenes.UltimoMovimientoHasta(contexto, regularizacion.IdAlmacen, linea.IdUnitario, linea.Fecha.Date);

                    switch (clase)
                    {
                        case enumRegularizacionAlm.Inicial:
                            Crear(linea, enumTiposDeMovimiento.StockInicial, linea.Cantidad, linea.Precio);
                            break;

                        case enumRegularizacionAlm.Recuento:
                            if (anterior is null)
                            {
                                if (linea.Cantidad != 0)
                                    Crear(linea, enumTiposDeMovimiento.StockInicial, linea.Cantidad, linea.Precio);
                                break;
                            }
                            if (linea.Cantidad != anterior.Stock)
                                Crear(linea, linea.Cantidad > anterior.Stock ? enumTiposDeMovimiento.Incremento : enumTiposDeMovimiento.Decremento, linea.Cantidad, anterior.Valor);
                            if (!MismoPrecio(linea.Precio, anterior.Valor))
                                Crear(linea, enumTiposDeMovimiento.AjustePrecio, 0, linea.Precio);
                            break;

                        case enumRegularizacionAlm.AjusteDePrecio:
                            if (anterior is null)
                                GestorDeErrores.Emitir("el material no tiene movimientos anteriores a esa fecha en el almacén, no se puede ajustar su precio");
                            if (!MismoPrecio(linea.Precio, anterior.Valor))
                                Crear(linea, enumTiposDeMovimiento.AjustePrecio, 0, linea.Precio);
                            break;
                    }
                }
                catch (Exception e)
                {
                    var unitario = contexto.SeleccionarPorId<UnitarioDtm>(linea.IdUnitario);
                    throw new Exception($"Línea nº {linea.Orden} (material '{unitario.Referencia}', {linea.Fecha:dd/MM/yyyy}, regularización de clase '{clase.Descripcion()}'): {e.Message}", e);
                }
            }

            traza?.Invoke($"Movimientos de almacén generados: {creados}");
            return creados;
        }

        // los precios del Excel pasan por double: se comparan con 4 decimales
        private static bool MismoPrecio(decimal a, decimal b) => Math.Round(a, 4) == Math.Round(b, 4);

    }
}
