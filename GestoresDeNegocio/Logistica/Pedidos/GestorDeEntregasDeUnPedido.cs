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
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestoresDeNegocio.Logistica
{
    // Entregas (recepciones) de las líneas de un pedido para el grid de entregas de su edición: se filtran por el pedido (IdElemento).
    // Entregar es insertar una recepción y cancelarla es eliminarla; las reglas están en ExtensorDeRecepciones. No se modifican: para
    // cambiar una entrega se cancela y se vuelve a entregar. Sus devoluciones las gestiona GestorDeDevolucionesDeUnPedido
    public class GestorDeEntregasDeUnPedido : GestorDeElementos<ContextoSe, RecepcionDeLineaDtm, EntregaDeUnPedidoDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class MapearEntregasDeUnPedido : Profile
        {
            public MapearEntregasDeUnPedido()
            {
                CreateMap<RecepcionDeLineaDtm, EntregaDeUnPedidoDto>()
                .ForMember(dto => dto.IdElemento, x => x.MapFrom(dtm => dtm.Linea.IdElemento))
                .ForMember(dto => dto.Concepto, x => x.MapFrom(dtm => dtm.Linea.Concepto))
                .ForMember(dto => dto.Almacen, x => x.MapFrom(dtm => dtm.Almacen.Expresion))
                .ForMember(dto => dto.Expediente, x => x.MapFrom(dtm => dtm.Expediente.Expresion))
                .ForMember(dto => dto.Elemento, x => x.Ignore())
                .ForMember(dto => dto.Titulo, x => x.Ignore())
                .ForMember(dto => dto.CantidadDevuelta, x => x.Ignore())
                .ForMember(dto => dto.Contabilizada, x => x.Ignore());

                CreateMap<EntregaDeUnPedidoDto, RecepcionDeLineaDtm>()
                .ForMember(dtm => dtm.Linea, dto => dto.Ignore())
                .ForMember(dtm => dtm.Almacen, dto => dto.Ignore())
                .ForMember(dtm => dtm.Expediente, dto => dto.Ignore());
            }
        }

        public GestorDeEntregasDeUnPedido(ContextoSe contexto, IMapper mapeador) : base(contexto, mapeador)
        {
        }

        public static GestorDeEntregasDeUnPedido Gestor(ContextoSe contexto, IMapper mapeador) => new GestorDeEntregasDeUnPedido(contexto, mapeador);

        //---------------------------------------------------------------------------------------------------------------------------
        // Operaciones de recepción, se ejecutan dentro de la transacción del llamador. Al acabar, bien o mal, olvidan las cachés de
        // recepciones de las líneas tocadas, para que si la transacción se deshace no queden con datos que no se guardaron
        //---------------------------------------------------------------------------------------------------------------------------

        // Entrega parcial de una línea con lo indicado en la modal de recepción; si no se indica destino va al de la cabecera.
        // Si el pedido está solicitado pasa a recepción
        public static string RecepcionarLinea(ContextoSe contexto, int idLineaPedido, string rowVersion, DateTime recepcionadaEl, decimal cantidad, decimal precio, decimal? descuento, int? idAlmacen, int? idExpediente)
        {
            PedidoDtm pedido = null;
            try
            {
                LineaDeUnPedidoDtm linea;
                (linea, pedido) = ExtensorDeRecepciones.LeerLineaParaRecepcionar(contexto, idLineaPedido, rowVersion, esUnaEntrega: true);

                var destino = idAlmacen is null && idExpediente is null ? pedido.DestinoDeLasEntregas() : (idAlmacen, idExpediente);
                var estabaSolicitado = pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Solicitud);
                pedido = pedido.PasarARecepcion(contexto, recepcionadaEl);
                var recepcion = Entregar(contexto, linea, pedido, recepcionadaEl, cantidad, precio, descuento, destino, exigirRecepcion: true);

                return $"Línea {linea.Orden} recibida el {recepcion.RecepcionadaEl:dd/MM/yyyy} {(recepcion.VaAlAlmacen ? "en el almacén" : "en la obra del expediente")}: cantidad {cantidad}, precio {precio}, descuento {descuento}, importe entregado {recepcion.Importe:N2}"
                     + (estabaSolicitado ? $"; el pedido '{pedido.Referencia}' pasa a {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()}" : "");
            }
            finally
            {
                ExtensorDeRecepciones.OlvidarRecepcion(idLineaPedido, pedido?.Id ?? 0);
            }
        }

        // Cancela una entrega desde el grid de entregas (ver AntesDePersistir); la entrega ha de ser la versión que el usuario vio en el grid
        public static string CancelarEntrega(ContextoSe contexto, int idRecepcion, string rowVersion)
        {
            LineaDeUnPedidoDtm linea = null;
            try
            {
                var recepcion = contexto.SeleccionarPorId<RecepcionDeLineaDtm>(idRecepcion, usarLaCache: false);
                recepcion.ValidarRowId(contexto, rowVersion, releer: false);
                linea = contexto.SeleccionarPorId<LineaDeUnPedidoDtm>(recepcion.IdLinea, usarLaCache: false);

                recepcion.Eliminar(contexto, new Dictionary<string, object> { { ltrEntregasDeUnPedido.Linea, linea } });
                return $"Cancelada la entrega de {recepcion.Cantidad} de la línea {linea.Orden} recibida el {recepcion.RecepcionadaEl:dd/MM/yyyy}";
            }
            finally
            {
                if (linea is not null)
                    linea.OlvidarRecepcion();
            }
        }

        // "Dar por entregado": entrega lo que queda pendiente de cada línea del pedido; si no se indica destino va al de la cabecera.
        // Si el pedido está solicitado pasa a recepción
        public static string RecepcionarPedido(ContextoSe contexto, int idPedido, string rowVersion, DateTime recepcionadoEl, int? idAlmacen, int? idExpediente)
        {
            var idsDeLineas = new List<int>();
            try
            {
                var pedido = contexto.SeleccionarPorId<PedidoDtm>(idPedido, usarLaCache: false);
                pedido.ValidarRowId(contexto, rowVersion, releer: false);
                pedido.ValidarQueEsGestor(contexto);

                var destino = idAlmacen is null && idExpediente is null ? pedido.DestinoDeLasEntregas() : (idAlmacen, idExpediente);

                if (!pedido.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePedido> { enumEtapasDePedido.PED_Etapa_De_Solicitud, enumEtapasDePedido.PED_Etapa_De_Recepcion }))
                    Emitir($"El pedido '{pedido.Referencia}' solo se puede recepcionar {enumEtapasDePedido.PED_Etapa_De_Solicitud.Nombre()} o {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()}");

                pedido = pedido.PasarARecepcion(contexto, recepcionadoEl);
                var (lineas, importe) = RecibirTodasLasLineas(contexto, pedido, recepcionadoEl, destino, idsDeLineas, estaEnRecepcion: true);

                return $"Pedido '{pedido.Referencia}' recepcionado el {recepcionadoEl:dd/MM/yyyy} {(destino.idAlmacen != null ? "en el almacén" : "en la obra del expediente")}: lo pendiente de {lineas} líneas por un importe de {importe:N2}";
            }
            finally
            {
                foreach (var idLinea in idsDeLineas)
                    ExtensorDeRecepciones.OlvidarRecepcion(idLinea, idPedido);
                ExtensorDeRecepciones.OlvidarRecepciones(idPedido);
            }
        }

        // "Dar por recibido": una vez cerrado el pedido se recepcionan todas sus líneas con lo pendiente, en la fecha de recepción de la
        // cabecera (ver GestorDePedidos.DespuesDeTransitar) y al destino de la cabecera. Un pedido sin líneas que recibir se cierra igualmente
        public static void RecepcionarAlDarPorRecibido(ContextoSe contexto, PedidoDtm pedido)
        {
            var idsDeLineas = new List<int>();
            try
            {
                var recibidoEl = pedido.RecibidoEl ?? pedido.FechaDeRecepcionAlDarPorRecibido();
                RecibirTodasLasLineas(contexto, pedido, recibidoEl, pedido.DestinoDeLasEntregas(), idsDeLineas, estaEnRecepcion: false);
            }
            finally
            {
                foreach (var idLinea in idsDeLineas)
                    ExtensorDeRecepciones.OlvidarRecepcion(idLinea, pedido.Id);
                ExtensorDeRecepciones.OlvidarRecepciones(pedido.Id);
            }
        }

        // Entrega lo que queda pendiente de cada línea, al precio y descuento de la línea.
        // estaEnRecepcion: entrega completa del pedido en recepción; si no, se da por recibido al cerrarlo y no es error que no haya nada pendiente
        private static (int lineas, decimal importe) RecibirTodasLasLineas(ContextoSe contexto, PedidoDtm pedido, DateTime recepcionadoEl, (int? idAlmacen, int? idExpediente) destino, List<int> idsDeLineas, bool estaEnRecepcion)
        {
            // los comentarios no tienen cantidad ni precio, no se reciben
            var pendientes = pedido.Detalles<LineaDeUnPedidoDtm>(contexto)
                .Where(linea => linea.TipoDeLinea != enumTipoDeLinea.Comentario && linea.Cantidad > 0 && linea.Precio != null)
                .OrderBy(linea => linea.Orden)
                .Select(linea => (linea, pendiente: linea.CantidadPendienteDeEntregar(contexto)))
                .Where(x => x.pendiente > 0)
                .ToList();
            if (pendientes.Count == 0 && estaEnRecepcion)
                Emitir($"El pedido '{pedido.Referencia}' no tiene nada pendiente de entregar");

            idsDeLineas.AddRange(pendientes.Select(x => x.linea.Id));
            var importe = 0m;
            foreach (var (linea, pendiente) in pendientes)
                importe += Entregar(contexto, linea, pedido, recepcionadoEl, pendiente, (decimal)linea.Precio, linea.Descuento, destino, exigirRecepcion: estaEnRecepcion).Importe;
            return (pendientes.Count, importe);
        }

        // Inserta la entrega de la línea (ver AntesDePersistir y DespuesDePersistir)
        private static RecepcionDeLineaDtm Entregar(ContextoSe contexto, LineaDeUnPedidoDtm linea, PedidoDtm pedido, DateTime recepcionadaEl, decimal cantidad, decimal precio, decimal? descuento, (int? idAlmacen, int? idExpediente) destino, bool exigirRecepcion)
            => new RecepcionDeLineaDtm
            {
                IdLinea = linea.Id,
                Cantidad = cantidad,
                Precio = precio,
                Descuento = descuento,
                RecepcionadaEl = recepcionadaEl.Date,
                IdAlmacen = destino.idAlmacen,
                IdExpediente = destino.idExpediente,
                IdUsuaCrea = contexto.DatosDeConexion.IdUsuario,
                FechaCreacion = DateTime.Now
            }.Insertar(contexto, new Dictionary<string, object> { { ltrEntregasDeUnPedido.Linea, linea }, { ltrEntregasDeUnPedido.Pedido, pedido }, { ltrEntregasDeUnPedido.ExigirRecepcion, exigirRecepcion } });

        private static T Leido<T>(ParametrosDeNegocio parametros, string clave) where T : class
            => parametros.Parametros.TryGetValue(clave, out var valor) ? valor as T : null;

        // Al entregar: el pedido en recepción (salvo al darlo por recibido) y la entrega válida (ver ExtensorDeRecepciones.ValidarEntrega).
        // Al cancelar: quien cancela gestiona el pedido, que ha de estar en recepción, y la entrega no tiene devoluciones; si fue a un
        // almacén se borra antes su entrada de compra, que referencia a la entrega (ver ExtensorDeRecepciones.AnularEntradaDeCompra)
        protected override void AntesDePersistir(RecepcionDeLineaDtm recepcion, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(recepcion, parametros);

            if (parametros.Modificando)
                Emitir("Una entrega no se modifica: cancélela y vuelva a entregar la línea");

            var linea = Leido<LineaDeUnPedidoDtm>(parametros, ltrEntregasDeUnPedido.Linea) ?? Contexto.SeleccionarPorId<LineaDeUnPedidoDtm>(recepcion.IdLinea, usarLaCache: false);
            PedidoDtm pedido;
            if (parametros.Insertando)
            {
                pedido = Leido<PedidoDtm>(parametros, ltrEntregasDeUnPedido.Pedido) ?? Contexto.SeleccionarPorId<PedidoDtm>(linea.IdElemento, usarLaCache: false);
                if (parametros.Parametros.LeerValor(ltrEntregasDeUnPedido.ExigirRecepcion, true) && !pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Recepcion))
                    Emitir($"Para recepcionar la línea {linea.Orden} el pedido '{pedido.Referencia}' ha de estar {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()}");
                recepcion.ValidarEntrega(Contexto, linea, pedido);
            }
            else
            {
                pedido = recepcion.ValidarEntregaParaCancelar(Contexto, linea);
                parametros.Parametros[ltrEntregasDeUnPedido.Descripcion] = recepcion.DescripcionParaLaTraza(Contexto, linea);
                recepcion.AnularEntradaDeCompra(Contexto);
            }

            parametros.Parametros[ltrEntregasDeUnPedido.Linea] = linea;
            parametros.Parametros[ltrEntregasDeUnPedido.Pedido] = pedido;
        }

        // Al entregar en un almacén se registra su entrada de compra (el gestor de movimientos la preasienta y barre los posteriores) y,
        // con el pedido en recepción, la fecha de recepción de la cabecera pasa a ser la de la primera entrega. Al cancelar queda una
        // traza en el pedido con quién la ha cancelado y los datos que tenía la entrega
        protected override void DespuesDePersistir(RecepcionDeLineaDtm recepcion, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(recepcion, parametros);

            var linea = Leido<LineaDeUnPedidoDtm>(parametros, ltrEntregasDeUnPedido.Linea);
            var pedido = Leido<PedidoDtm>(parametros, ltrEntregasDeUnPedido.Pedido);
            if (parametros.Insertando)
            {
                if (recepcion.VaAlAlmacen)
                    recepcion.EntradaDeCompra(Contexto, linea);
                if (parametros.Parametros.LeerValor(ltrEntregasDeUnPedido.ExigirRecepcion, true))
                    pedido.AdelantarLaFechaDeRecepcion(Contexto, recepcion.RecepcionadaEl);
            }
            else if (parametros.Eliminando)
                pedido.CrearTraza(Contexto, $"Entrega de la línea {linea.Orden} cancelada", (string)parametros.Parametros[ltrEntregasDeUnPedido.Descripcion]);
        }

        // Lo entregado de la línea y si el pedido tiene entregas se recalculan; se llama antes y después de persistir y también si falla
        protected override void EliminarCaches(RecepcionDeLineaDtm recepcion, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(recepcion, parametros);
            ExtensorDeRecepciones.OlvidarRecepcion(recepcion.IdLinea, Leido<LineaDeUnPedidoDtm>(parametros, ltrEntregasDeUnPedido.Linea)?.IdElemento ?? 0);
            if (recepcion.Id > 0)
                ServicioDeCaches.EliminarElemento(CacheDe.Pedido_LineaPedidoPorRecepcion, recepcion.Id.ToString());
        }

        protected override IQueryable<RecepcionDeLineaDtm> AplicarJoins(IQueryable<RecepcionDeLineaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Linea);
            consulta = consulta.Include(x => x.Almacen);
            consulta = consulta.Include(x => x.Expediente);
            return consulta;
        }

        // el restrictor del grid es el pedido, del que son las líneas entregadas
        protected override IQueryable<RecepcionDeLineaDtm> AplicarFiltros(IQueryable<RecepcionDeLineaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            var filtro = filtros.FirstOrDefault(x => !x.Aplicado && x.Clausula.Equals(nameof(IDetalleDto.IdElemento), StringComparison.CurrentCultureIgnoreCase));
            if (filtro != null)
            {
                var idPedido = filtro.Valor.Entero();
                consulta = consulta.Where(x => x.Linea.IdElemento == idPedido);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        protected override void DespuesDeMapearElElemento(RecepcionDeLineaDtm recepcion, EntregaDeUnPedidoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(recepcion, elemento, parametros);
            elemento.CantidadDevuelta = recepcion.CantidadDevuelta(Contexto);
            elemento.Contabilizada = recepcion.EntregaContabilizada(Contexto);
        }
    }
}
