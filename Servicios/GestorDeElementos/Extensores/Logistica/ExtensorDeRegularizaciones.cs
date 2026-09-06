using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
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
    }
}
