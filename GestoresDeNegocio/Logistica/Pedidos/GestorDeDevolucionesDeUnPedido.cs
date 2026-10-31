using AutoMapper;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using ModeloDeDto.Logistica;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Entorno;
using ServicioDeReportes.Logistica;
using QuestPDF.Fluent;
using System.IO;

namespace GestoresDeNegocio.Logistica
{
    // Devoluciones de las entregas de un pedido para el grid de devoluciones de su edición: se filtran por el pedido (IdElemento).
    // Se valoran al precio y descuento de la entrega. Devolver una entrega es insertar una devolución y cancelarla es eliminarla; las
    // reglas están en ExtensorDeDevolucionesDePedido. No se modifican: para cambiar una devolución se cancela y se vuelve a devolver
    public class GestorDeDevolucionesDeUnPedido : GestorDeElementos<ContextoSe, DevolucionDeEntregaDtm, DevolucionDeUnPedidoDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class MapearDevolucionesDeUnPedido : Profile
        {
            public MapearDevolucionesDeUnPedido()
            {
                CreateMap<DevolucionDeEntregaDtm, DevolucionDeUnPedidoDto>()
                .ForMember(dto => dto.IdElemento, x => x.MapFrom(dtm => dtm.LineaDeRecepcion.Linea.IdElemento))
                .ForMember(dto => dto.Concepto, x => x.MapFrom(dtm => dtm.LineaDeRecepcion.Linea.Concepto))
                .ForMember(dto => dto.Precio, x => x.MapFrom(dtm => dtm.LineaDeRecepcion.Precio))
                .ForMember(dto => dto.Descuento, x => x.MapFrom(dtm => dtm.LineaDeRecepcion.Descuento))
                .ForMember(dto => dto.IdAlmacen, x => x.MapFrom(dtm => dtm.LineaDeRecepcion.IdAlmacen))
                .ForMember(dto => dto.Almacen, x => x.MapFrom(dtm => dtm.LineaDeRecepcion.Almacen.Expresion))
                .ForMember(dto => dto.IdExpediente, x => x.MapFrom(dtm => dtm.LineaDeRecepcion.IdExpediente))
                .ForMember(dto => dto.Expediente, x => x.MapFrom(dtm => dtm.LineaDeRecepcion.Expediente.Expresion))
                .ForMember(dto => dto.Importe, x => x.Ignore())
                .ForMember(dto => dto.Elemento, x => x.Ignore())
                .ForMember(dto => dto.Titulo, x => x.Ignore())
                .ForMember(dto => dto.Contabilizada, x => x.Ignore());

                CreateMap<DevolucionDeUnPedidoDto, DevolucionDeEntregaDtm>()
                .ForMember(dtm => dtm.LineaDeRecepcion, dto => dto.Ignore());
            }
        }

        public GestorDeDevolucionesDeUnPedido(ContextoSe contexto, IMapper mapeador) : base(contexto, mapeador)
        {
        }

        public static GestorDeDevolucionesDeUnPedido Gestor(ContextoSe contexto, IMapper mapeador) => new GestorDeDevolucionesDeUnPedido(contexto, mapeador);

        // Devuelve parte (o todo) lo entregado en la línea, ver AntesDePersistir y DespuesDePersistir
        public static string DevolverEntrega(ContextoSe contexto, int idRecepcion, string rowVersion, decimal cantidad, DateTime devueltaEl, string motivo)
        {
            var devolucion = new DevolucionDeEntregaDtm
            {
                IdLineaDeRecepcion = idRecepcion,
                DevueltaEl = devueltaEl.Date,
                Cantidad = cantidad,
                Motivo = motivo?.Trim(),
                IdUsuaCrea = contexto.DatosDeConexion.IdUsuario,
                FechaCreacion = DateTime.Now
            }.Insertar(contexto, new Dictionary<string, object> { { ltrDevolucionesDeUnPedido.RowVersionDeLaEntrega, rowVersion } });

            var recepcion = devolucion.Recepcion(contexto);
            var linea = contexto.SeleccionarPorId<LineaDeUnPedidoDtm>(recepcion.IdLinea);
            return $"Devueltas {cantidad} de la línea {linea.Orden} el {devolucion.DevueltaEl:dd/MM/yyyy}, importe {devolucion.Importe(recepcion):N2}"
                 + (recepcion.VaAlAlmacen ? "; se ha registrado su salida del almacén" : "");
        }

        // Cancela la devolución desde el grid de devoluciones, ver AntesDePersistir y DespuesDePersistir
        public static string CancelarDevolucion(ContextoSe contexto, int idDevolucion, string rowVersion)
        {
            var devolucion = contexto.SeleccionarPorId<DevolucionDeEntregaDtm>(idDevolucion, usarLaCache: false);
            devolucion.ValidarRowId(contexto, rowVersion, releer: false);
            var linea = contexto.SeleccionarPorId<LineaDeUnPedidoDtm>(devolucion.Recepcion(contexto).IdLinea);
            // sin la navegación, para que sólo se borre la devolución
            devolucion.LineaDeRecepcion = null;
            devolucion.Eliminar(contexto);
            return $"Cancelada la devolución de {devolucion.Cantidad} de la línea {linea.Orden}";
        }

        // Imprime todas las devoluciones del pedido (ver GeneradorDeDevolucionDePedidoRpt) y anexa el pdf a sus archivos con el nombre
        // "Devolucion-[referencia del pedido]-[nº de impresión]"
        public static string ImprimirDevoluciones(ContextoSe contexto, int idPedido)
        {
            var pedido = contexto.SeleccionarPorId<PedidoDtm>(idPedido, usarLaCache: false);
            var hayDevoluciones = contexto.Set<DevolucionDeEntregaDtm>().Any(devolucion => devolucion.LineaDeRecepcion.Linea.IdElemento == pedido.Id);
            if (!hayDevoluciones)
                Emitir($"El pedido '{pedido.Referencia}' no tiene devoluciones que imprimir");

            // el nº de impresión es uno más que los impresos de devoluciones ya anexados al pedido
            var prefijo = $"Devolucion-{pedido.Referencia}-".NormalizarFichero();
            var impresion = pedido.LeerAnexados(contexto).Count(archivo => archivo.Nombre.StartsWith(prefijo)) + 1;
            var nombre = pedido.ProponerNombreDeArchivo(contexto, $"{prefijo}{impresion}.pdf");

            var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, nombre);
            var devolucionRpt = new GeneradorDeDevolucionDePedidoRpt(contexto, pedido).ObtenerInformacionDeRpt(plantilla: null);
            new ReporteDeDevolucionDePedido(devolucionRpt).GeneratePdf(rutaConFichero);
            var idArchivo = ServidorDocumental.SubirArchivo(contexto, rutaConFichero, sanitizar: false);
            GestorDeVinculos.Vincular(contexto, enumNegocio.Pedido, enumNegocio.Archivos, pedido.Id, idArchivo);

            return $"Impresas las devoluciones del pedido '{pedido.Referencia}' en el archivo '{nombre}'";
        }

        protected override IQueryable<DevolucionDeEntregaDtm> AplicarJoins(IQueryable<DevolucionDeEntregaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.LineaDeRecepcion).ThenInclude(x => x.Linea);
            consulta = consulta.Include(x => x.LineaDeRecepcion).ThenInclude(x => x.Almacen);
            consulta = consulta.Include(x => x.LineaDeRecepcion).ThenInclude(x => x.Expediente);
            return consulta;
        }

        // el restrictor del grid es el pedido, del que son las líneas cuyas entregas se han devuelto
        protected override IQueryable<DevolucionDeEntregaDtm> AplicarFiltros(IQueryable<DevolucionDeEntregaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            var filtro = filtros.FirstOrDefault(x => !x.Aplicado && x.Clausula.Equals(nameof(IDetalleDto.IdElemento), StringComparison.CurrentCultureIgnoreCase));
            if (filtro != null)
            {
                var idPedido = filtro.Valor.Entero();
                consulta = consulta.Where(x => x.LineaDeRecepcion.Linea.IdElemento == idPedido);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        // Al devolver: la entrega es la que vio el usuario, quien devuelve gestiona el pedido (en recepción o cerrado) y la cantidad,
        // la fecha y el motivo son válidos (ver ExtensorDeDevolucionesDePedido.ValidarDevolucion). Al cancelar: lo mismo del pedido y, si
        // la entrega fue a un almacén, se borra antes su salida, que referencia a la devolución
        protected override void AntesDePersistir(DevolucionDeEntregaDtm devolucion, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(devolucion, parametros);

            if (parametros.Modificando)
                Emitir("Una devolución no se modifica: cancélela y vuelva a devolver la entrega");

            var recepcion = parametros.Insertando
                ? ExtensorDeDevolucionesDePedido.LeerEntregaParaDevolver(Contexto, devolucion.IdLineaDeRecepcion, parametros.Parametros.LeerValor<string>(ltrDevolucionesDeUnPedido.RowVersionDeLaEntrega, null))
                : Contexto.SeleccionarPorId<RecepcionDeLineaDtm>(devolucion.IdLineaDeRecepcion, usarLaCache: false);
            var (linea, pedido) = recepcion.LeerPedidoParaDevolver(Contexto);

            if (parametros.Insertando)
                recepcion.ValidarDevolucion(Contexto, pedido, linea, devolucion.Cantidad, devolucion.DevueltaEl, devolucion.Motivo);

            if (parametros.Eliminando)
                devolucion.ValidarCancelacion(Contexto, recepcion, linea)?.AnularSalidaPorDevolucion(Contexto);

            parametros.Parametros[ltrDevolucionesDeUnPedido.Entrega] = recepcion;
            parametros.Parametros[ltrDevolucionesDeUnPedido.Linea] = linea;
            parametros.Parametros[ltrDevolucionesDeUnPedido.Pedido] = pedido;
        }

        // Al devolver, si la entrega fue a un almacén se saca lo devuelto (el gestor de movimientos barre los posteriores); en ambos casos
        // queda una traza en el pedido
        protected override void DespuesDePersistir(DevolucionDeEntregaDtm devolucion, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(devolucion, parametros);

            var recepcion = (RecepcionDeLineaDtm)parametros.Parametros[ltrDevolucionesDeUnPedido.Entrega];
            var linea = (LineaDeUnPedidoDtm)parametros.Parametros[ltrDevolucionesDeUnPedido.Linea];
            var pedido = (PedidoDtm)parametros.Parametros[ltrDevolucionesDeUnPedido.Pedido];

            if (parametros.Insertando && recepcion.VaAlAlmacen)
                devolucion.SalidaPorDevolucion(Contexto, recepcion, linea);

            var descripcion = devolucion.DescripcionParaLaTraza(Contexto, recepcion, linea, devolucion.Importe(recepcion));
            if (parametros.Insertando)
                pedido.CrearTraza(Contexto, $"Devolución de la línea {linea.Orden}", descripcion);
            else if (parametros.Eliminando)
                pedido.CrearTraza(Contexto, $"Devolución de la línea {linea.Orden} cancelada", $"Cancelada por {Contexto.Usuario.Expresion} el {DateTime.Now:dd/MM/yyyy HH:mm}{Environment.NewLine}{descripcion}");
        }

        // Lo devuelto de la entrega se recalcula: se llama antes y después de persistir y también si falla, así no queda en la caché
        // lo de una devolución que se ha deshecho
        protected override void EliminarCaches(DevolucionDeEntregaDtm devolucion, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(devolucion, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Pedido_Devuelto, devolucion.IdLineaDeRecepcion.ToString());
        }

        protected override void DespuesDeMapearElElemento(DevolucionDeEntregaDtm devolucion, DevolucionDeUnPedidoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(devolucion, elemento, parametros);
            elemento.Importe = devolucion.Importe(devolucion.Recepcion(Contexto));
            elemento.Contabilizada = devolucion.DevolucionContabilizada(Contexto);
        }
    }
}
