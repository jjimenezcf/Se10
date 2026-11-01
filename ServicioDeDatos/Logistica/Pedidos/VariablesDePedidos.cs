using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    public enum enumEtapasDePedido
    {
        [Description("Ids de estados en los que un pedido se ha de cumplimentar")]
        PED_Etapa_De_Cumplimentacion,
        [Description("Ids de estados en los que un pedido se está aprobando")]
        PED_Etapa_De_Aprobacion,
        [Description("Ids de estados en los que un pedido se ha de solicitado")]
        PED_Etapa_De_Solicitud,
        [Description("Ids de estados en los que un pedido se esta entregando")]
        PED_Etapa_De_Recepcion,
        [Description("Ids de estados en los que un pedido está cerrado")]
        PED_Etapa_Cerrado,
        [Description("Ids de estados en los que un pedido es devuelto")]
        PED_Etapa_Devuelto,
        [Description("Ids de estados en los que un pedido es anulado")]
        PED_Etapa_Cancelado
    }

    public enum enumParametrosDePedidos
    {
        [Description("Indica el id de la unidad de medida por defecto")]
        PED_Unidad_Medida,
        [Description("Indica el id de la naturaleza por defecto")]
        PED_Naturaleza,
        [Description("Indica el tipo de línea por defecto")]
        PED_TipoDeLinea,
        [Description("Indica los datos por defecto para usar en la impresión de un pedido")]
        PED_DatosDeImpresion,
        [Description("Indica el incremento a aplicar a las líneas de pedido")]
        PED_IncrementarOrdenEn
    }

    public static class VariableDePedidos
    {
        internal static readonly string IA_Reglas_de_filtrado = @"
### R.Pedidos.1 · Pendientes de recepcionar (`FiltroPorEtapa` + `FiltroPorPendienteDeRecibir`)
- **Disparador:** Pedidos ""pendientes de recepcionar"", ""pendientes de recibir"", ""sin recibir"", ""que faltan por recibir"", ""pendientes de entrega"".
- **Acción:** Genera siempre estos dos objetos:
  1. `{""Clausula"": ""FiltroPorEtapa"", ""Criterio"": ""igual"", ""Valor"": ""PED_Etapa_De_Solicitud|PED_Etapa_De_Recepcion""}`
  2. `{""Clausula"": ""FiltroPorPendienteDeRecibir"", ""Criterio"": ""igual"", ""Valor"": ""5""}`
- **Nota técnica:** Un pedido pendiente de recepcionar es el que ya se ha solicitado al proveedor y aún no está cerrado: `PED_Etapa_De_Solicitud` (todavía no se ha entregado nada) o `PED_Etapa_De_Recepcion` (entregado parcialmente). Ambas etapas van en un único objeto separadas por `|` (unión). Además se exige que alguna de sus líneas tenga cantidad pendiente de recibir (`FiltroPorPendienteDeRecibir` = `5`), porque un pedido en recepción puede tener ya todas sus líneas recibidas. Con `6` se obtienen los que no tienen nada pendiente de recibir. No generes `quemostrar`: son pedidos en curso.

### R.Pedidos.2 · Importe del pedido (`FiltroPorImporte`)
- **Disparador:** Pedidos ""cuyo importe sea más de [X]"", ""por más de [X]€"", ""de entre [X] y [Y] euros"", ""de menos de [X]"", ""con un total de [X]"".
- **Acción:** Genera:
  1. `{""Clausula"": ""FiltroPorImporte"", ""Criterio"": ""entreImportes"", ""Valor"": ""min;max""}`
  2. `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""9""}` (Para incluir también cerrados/anulados).
- **Nota:** ""más de [X]"" → `X;` · ""menos de [X]"" → `0;X` · ""entre [X] y [Y]"" → `X;Y`. Es el importe del pedido sin IVA (lo pedido en sus líneas, con descuento). El valor es numérico, sin símbolo de moneda (""mil euros"" → `1000`).

### R.Pedidos.3 · Importe entregado (`FiltroPorImporteEntregado`)
- **Disparador:** Pedidos ""en donde se haya entregado ya [X] euros"", ""con entregas por más de [X]"", ""recepcionado/recibido más de [X]€"", ""sin nada entregado"".
- **Acción:** Genera:
  1. `{""Clausula"": ""FiltroPorImporteEntregado"", ""Criterio"": ""entreImportes"", ""Valor"": ""min;max""}`
  2. `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""9""}`
- **Nota:** Mismo formato de valor que R.Pedidos.2. ""Sin entregas"" / ""sin nada entregado"" → `0;0`. Es el importe de todas las entregas, sin restar lo devuelto. Si la cantidad se expresa en porcentaje (""el 50%""), usa R.Pedidos.4.

### R.Pedidos.4 · Porcentaje recepcionado (`FiltroPorPorcentajeEntregado`)
- **Disparador:** Pedidos ""que haya recepcionado ya el [P]%"", ""entregados al menos el [P]%"", ""recibidos al completo / al 100%"", ""con menos del [P]% recibido"".
- **Acción:** Genera:
  1. `{""Clausula"": ""FiltroPorPorcentajeEntregado"", ""Criterio"": ""entreRangos"", ""Valor"": ""min;max""}`
  2. `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""9""}`
- **Nota:** Porcentaje (de 0 a 100) del importe del pedido que ya se ha entregado. ""el [P]%"" / ""al menos [P]%"" / ""más de [P]%"" → `P;` · ""menos de [P]%"" → `0;P` · ""entre [P] y [Q]%"" → `P;Q` · ""completamente recibidos"" → `100;`. El valor va sin el símbolo %.

### R.Pedidos.5 · Devoluciones (`FiltroPorImporteDevuelto`)
- **Disparador:** Pedidos ""que tengan devoluciones"", ""con devoluciones"", ""con algo devuelto"", ""con devoluciones por más de [X]€"", ""sin devoluciones"".
- **Acción:** Genera:
  1. `{""Clausula"": ""FiltroPorImporteDevuelto"", ""Criterio"": ""entreImportes"", ""Valor"": ""0.01;""}`
  2. `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""9""}`
- **Casos especiales:** ""con devoluciones"" → `0.01;` · ""sin devoluciones"" → `0;0` · con importe concreto → como R.Pedidos.2 (`X;`, `0;X`, `X;Y`).
- **Nota:** No lo confundas con la etapa `PED_Etapa_Devuelto` (R13): si el usuario pide pedidos ""en etapa/estado devuelto"" usa R13; ""con devoluciones"" siempre se resuelve aquí.

### R.Pedidos.6 · Unitarios / materiales (`UnitariosDePedidos`)
- **Disparador:** Pedidos ""con el unitario [X]"", ""que lleven el material [X]"", ""con los materiales cuyas referencias son [X], [Y], [Z]"", ""con líneas del unitario [X]"".
- **Acción:** Genera:
  1. `{""Clausula"": ""UnitariosDePedidos"", ""Criterio"": ""contiene"", ""Valor"": ""texto1;texto2;texto3""}`
  2. `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""9""}`
- **Nota técnica:** Devuelve los pedidos con alguna línea de un unitario cuyo nombre o referencia contenga alguno de los valores (coincidencia parcial, sin distinguir mayúsculas). Separa los valores con `;` y consérvalos tal cual los dio el usuario: son referencias o nombres, no apliques la limpieza de palabras vacías.

### R.Pedidos.7 · Expediente
**R.Pedidos.7.1 · Por datos del expediente (`ExpedientesDePedidos`)**
- **Disparador:** Pedidos ""para el expediente [Nombre/Referencia]"", ""del expediente [X]"", ""imputados al expediente [X]"".
- **Acción:** Genera:
  1. `{""Clausula"": ""ExpedientesDePedidos"", ""Criterio"": ""contiene"", ""Valor"": ""nombre_o_referencia1;nombre_o_referencia2""}`
  2. `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""9""}`

**R.Pedidos.7.2 · Con o sin expediente (`AsociadaAUnExpediente`)**
- **Disparador:** ""con expediente"", ""imputados a algún expediente"" (Valor `5`) · ""sin expediente"", ""no imputados a ningún expediente"" (Valor `6`).
- **Acción:** Genera:
  1. `{""Clausula"": ""AsociadaAUnExpediente"", ""Criterio"": ""igual"", ""Valor"": ""5""}` (o `6` para ""sin expediente"")
  2. `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""9""}`

**Jerarquía:** Si se indica un nombre o referencia de expediente (R.Pedidos.7.1), no generes R.Pedidos.7.2.

### R.Pedidos.8 · Almacén
**R.Pedidos.8.1 · Por datos del almacén (`AlmacenesDePedidos`)**
- **Disparador:** Pedidos ""del almacén [Nombre/Referencia]"", ""con destino al almacén [X]"", ""para el almacén [X]"".
- **Acción:** Genera:
  1. `{""Clausula"": ""AlmacenesDePedidos"", ""Criterio"": ""contiene"", ""Valor"": ""nombre_o_referencia1;nombre_o_referencia2""}`
  2. `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""9""}`

**R.Pedidos.8.2 · Con o sin almacén (`AsociadaAUnAlmacen`)**
- **Disparador:** ""pedidos de almacén"", ""con destino a almacén"", ""que van a un almacén"" (Valor `5`) · ""sin almacén"", ""que no van a un almacén"", ""que van a obra"" (Valor `6`).
- **Acción:** Genera:
  1. `{""Clausula"": ""AsociadaAUnAlmacen"", ""Criterio"": ""igual"", ""Valor"": ""5""}` (o `6` para ""sin almacén"")
  2. `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""9""}`

**Jerarquía:** Si se indica un nombre o referencia de almacén (R.Pedidos.8.1), no generes R.Pedidos.8.2.

### R.Pedidos.9 · Entregados (`FiltroPorEtapa` + `FiltroPorPendienteDeRecibir`)
- **Disparador:** Pedidos ""entregados"", ""que se han entregado"", ""recibidos"", ""recepcionados"", ""ya recibidos"", ""con todo entregado"", ""sin nada pendiente de recibir"".
- **Acción:** Genera siempre estos tres objetos:
  1. `{""Clausula"": ""FiltroPorEtapa"", ""Criterio"": ""igual"", ""Valor"": ""PED_Etapa_De_Recepcion|PED_Etapa_Cerrado""}`
  2. `{""Clausula"": ""FiltroPorPendienteDeRecibir"", ""Criterio"": ""igual"", ""Valor"": ""6""}`
  3. `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""9""}` (Para incluir los cerrados).
- **Nota técnica:** Un pedido entregado es el que está en recepción o cerrado y no tiene ninguna línea con cantidad pendiente de recibir. No uses sólo `PED_Etapa_Cerrado`: un pedido en recepción puede tener ya todo recibido, y uno cerrado puede haberse cerrado sin recibirlo todo. No lo confundas con R.Pedidos.1 (""pendientes de entrega"", Valor `5`).
";

        private static string etapaDeCumplimentacion => enumNegocio.Pedido.Parametro(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion)?.Valor ?? null;
        private static string etapaDeAprobacion => enumNegocio.Pedido.Parametro(enumEtapasDePedido.PED_Etapa_De_Aprobacion)?.Valor ?? null;
        private static string etapaDeCobro => enumNegocio.Pedido.Parametro(enumEtapasDePedido.PED_Etapa_De_Solicitud)?.Valor ?? null;
        private static string etapaDePago => enumNegocio.Pedido.Parametro(enumEtapasDePedido.PED_Etapa_De_Recepcion)?.Valor ?? null;
        private static string etapaDeCierre => enumNegocio.Pedido.Parametro(enumEtapasDePedido.PED_Etapa_Cerrado)?.Valor ?? null;
        private static string etapaDeDevolucion => enumNegocio.Pedido.Parametro(enumEtapasDePedido.PED_Etapa_Devuelto)?.Valor ?? null;
        private static string etapaAnulada => enumNegocio.Pedido.Parametro(enumEtapasDePedido.PED_Etapa_Cancelado)?.Valor ?? null;

        public enum enumMotivoTransicion {  };
                

        public static string Estados(this enumEtapasDePedido etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDePedido.PED_Etapa_De_Cumplimentacion: estados =etapaDeCumplimentacion; break;
                case enumEtapasDePedido.PED_Etapa_De_Aprobacion: estados =etapaDeAprobacion; break;
                case enumEtapasDePedido.PED_Etapa_De_Solicitud: estados =etapaDeCobro; break;
                case enumEtapasDePedido.PED_Etapa_De_Recepcion: estados =etapaDePago; break;
                case enumEtapasDePedido.PED_Etapa_Cerrado: estados =etapaDeCierre; break;
                case enumEtapasDePedido.PED_Etapa_Devuelto: estados =etapaDeDevolucion; break;
                case enumEtapasDePedido.PED_Etapa_Cancelado: estados =etapaAnulada; break;

            }

            return estados.IsNullOrEmpty() ? enumNegocio.Pedido.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnLaEtapa(this PedidoDtm pedido, enumEtapasDePedido etapa) => etapa.Lista().Contains(pedido.IdEstado);

        public static bool ContieneLaEtapa(this List<enumEtapasDePedido> etapas, enumEtapasDePedido etapa) => etapas.Contains(etapa);

        public static bool EstaEnAlgunaDeLasEtapa(this PedidoDtm pedido, List<enumEtapasDePedido> etapas)
        {
            var etapasDeLaFactura = pedido.Etapas();
            foreach (var etapa in etapas)
                if (etapasDeLaFactura.Contains(etapa)) return true;
            return false;
        }

        public static (List<int> estados, enumEtapasDePedido etapa) EstadosDeLaEtapa(this enumEtapasDePedido etapa) => (etapa.Lista(), etapa);

        public static List<int> Lista(this enumEtapasDePedido etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static List<enumEtapasDePedido> Etapas(this PedidoDtm pedido)
        {
            var etapas = new List<enumEtapasDePedido>();
            if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion))
                etapas.Add(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion);
            if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Aprobacion))
                etapas.Add(enumEtapasDePedido.PED_Etapa_De_Aprobacion);
            if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Solicitud))
                etapas.Add(enumEtapasDePedido.PED_Etapa_De_Solicitud);
            if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Recepcion))
                etapas.Add(enumEtapasDePedido.PED_Etapa_De_Recepcion);
            if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_Cerrado))
                etapas.Add(enumEtapasDePedido.PED_Etapa_Cerrado);
            if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_Devuelto))
                etapas.Add(enumEtapasDePedido.PED_Etapa_Devuelto);
            if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_Cancelado))
                etapas.Add(enumEtapasDePedido.PED_Etapa_Cancelado);
            return etapas;
        }

        public static string CadenaDeEtapas(this PedidoDtm pedido) => string.Join(Simbolos.separadorDeEtapas, pedido.Etapas());

        public static List<string> ListaDeEtapas(this PedidoDtm pedido) => pedido.CadenaDeEtapas().ToLista<string>(Simbolos.separadorDeEtapas);

        public static enumEtapasDePedido Etapa(this PedidoDtm pedido)
        {
            var etapas = pedido.Etapas();
            if (etapas.Count == 0)
                throw new Exception($"No se ha definido la etapa del {enumNegocio.Pedido.Singular(true)}, " +
                    $"cuando éste está en el estado {pedido.Propiedad<EstadoDtm>(typeof(EstadoDeUnPedidoDtm)).Nombre}");
            if (etapas.Count > 1)
                throw new Exception($"El estado del pedido {enumNegocio.Pedido.Singular(true)} '{pedido.Referencia}' " +
                    $"se encuentra en las etapas {string.Join(',', etapas)} y sólo ha de estar en una");
            return etapas[0];
        }

        public static string Nombre(this enumEtapasDePedido etapa, bool minusculas = true)
        {
            switch (etapa)
            {
                case enumEtapasDePedido.PED_Etapa_De_Cumplimentacion: return minusculas ? "cumplimentación" : "Cumplimentación";
                case enumEtapasDePedido.PED_Etapa_De_Aprobacion: return minusculas ? "aprobación" : "Aprobada";
                case enumEtapasDePedido.PED_Etapa_De_Solicitud: return minusculas ? "solicitado" : "Solicitado";
                case enumEtapasDePedido.PED_Etapa_De_Recepcion: return minusculas ? "de entrega" : "De entrega";
                case enumEtapasDePedido.PED_Etapa_Cerrado: return minusculas ? "cerrado" : "Cerrado";
                case enumEtapasDePedido.PED_Etapa_Devuelto: return minusculas ? "devuelto" : "Devuelto";
                case enumEtapasDePedido.PED_Etapa_Cancelado: return minusculas ? "anulado" : "Anulado";
            }
            return etapa.ToString();
        }

    }

}
