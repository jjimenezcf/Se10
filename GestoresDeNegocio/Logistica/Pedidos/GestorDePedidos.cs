using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using GestorDeElementos.Extensores;
using System;
using System.Text;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Ventas;
using static Gestor.Errores.GestorDeErrores;
using ServicioDeDatos.Expediente;
using System.Threading.Tasks;
using ServicioDeDatos.Logistica;
using ModeloDeDto.Logistica;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.TrabajosSometidos;
using ModeloDeDto;
using ServicioDeReportes.Logistica;
using QuestPDF.Fluent;
using System.IO;

namespace GestoresDeNegocio.Logistica
{

    public class GestorDePedidos : GestorDeElementos<ContextoSe, PedidoDtm, PedidoDto>, IEsImputable, ITotalizador<TotalesDePedidos>
    {
        public class MapearPedido : Profile
        {
            public MapearPedido()
            {
                CreateMap<PedidoDtm, PedidoDto>()
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Expresion))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado.Nombre))
                .ForMember(dto => dto.Expediente, dtm => dtm.MapFrom(dtm => dtm.Expediente == null ? null : dtm.Expediente.Expresion))
                .ForMember(dto => dto.Proveedor, dtm => dtm.MapFrom(dtm => dtm.Proveedor == null ? null : dtm.Proveedor.Expresion))
                .ForMember(dto => dto.Contrato, dtm => dtm.MapFrom(dtm => dtm.Contrato == null ? null : dtm.Contrato.Expresion))
                .ForMember(dto => dto.Almacen, dtm => dtm.MapFrom(dtm => dtm.Almacen == null ? null : dtm.Almacen.Expresion));

                CreateMap<PedidoDto, PedidoDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore())
                .ForMember(dtm => dtm.Proveedor, dto => dto.Ignore())
                .ForMember(dtm => dtm.Expediente, dto => dto.Ignore())
                .ForMember(dtm => dtm.Contrato, dto => dto.Ignore())
                .ForMember(dtm => dtm.Almacen, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.Pedido;

        //public override TiposDelTipoDeElemento TiposDelTipo => Negocio.TiposDelTipo();

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDePedido.Gestor(Contexto, Contexto.Mapeador);

        public GestorDePedidos(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDePedidos Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePedidos(contexto, mapeador);
        }

        // La cabecera sólo se modifica desde el Dto mientras se cumplimenta; las transiciones y acciones del sistema modifican el registro sin pasar por aquí
        protected override void DespuesDeMapearElRegistro(PedidoDto dto, PedidoDtm dtm, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(dto, dtm, opciones);

            if (!opciones.Modificando)
                return;

            var pedidoEnBd = Contexto.SeleccionarPorId<PedidoDtm>(dtm.Id, aplicarJoin: true);
                       

            if (!pedidoEnBd.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion))
                Emitir($"La cabecera del pedido '{pedidoEnBd.Referencia}' solo es modificable mientras se cumplimenta");
        }

        protected override IQueryable<PedidoDtm> AplicarJoins(IQueryable<PedidoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Proveedor);
            consulta = consulta.Include(x => x.Contrato);
            consulta = consulta.Include(x => x.Expediente);
            consulta = consulta.Include(x => x.Almacen);
            return consulta;
        }

        protected override IQueryable<PedidoDtm> AplicarOrden(IQueryable<PedidoDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override IQueryable<PedidoDtm> AplicarFiltros(IQueryable<PedidoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            parametros.AplicarFiltroQueMostrar = !filtros.OmitirFiltrosPorEstado(new List<string> { ltrDeUnPedido.IdContrato, ltrDeUnPedido.PedidosImputablesAlContrato, ltrDeUnPedido.IdExpediente, ltrDeUnPedido.PedidosImputablesAlExpediente, ltrDeUnPedido.IdAlmacen });
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroPorEjercicio(filtros);
            consulta = consulta.FiltroPorProveedor(filtros);
            consulta = consulta.FiltroPorAsuntoReferenciaPedido(Contexto, filtros);
            consulta = consulta.FiltroPorFechaPedido(filtros);
            consulta = consulta.FiltroPorFechaDeEntrega(filtros);
            consulta = consulta.FiltroPedidosPosiblesDelContrato(Contexto, filtros);
            consulta = consulta.FiltroPedidosPosiblesDeUnExpediente(Contexto, filtros, parametros);
            consulta = consulta.FiltroSiHayDependenciaDe(filtros, nameof(PedidoDtm.IdExpediente), ltrDeUnPedido.AsociadaAUnExpediente, parametros, aplicarFiltroDeEstado: false);
            consulta = consulta.FiltroSiHayDependenciaDe(filtros, nameof(PedidoDtm.IdContrato), ltrDeUnPedido.AsociadaAUnContrato, parametros, aplicarFiltroDeEstado: false);
            consulta = consulta.FiltroSiHayDependenciaDe(filtros, nameof(PedidoDtm.IdAlmacen), ltrDeUnPedido.AsociadaAUnAlmacen, parametros, aplicarFiltroDeEstado: false);

            if (parametros.Peticion == enumPeticion.epTotales)
                consulta = consulta.ExcluirLosNoTotalizables();

            return consulta;
        }

        protected override IQueryable<PedidoDtm> AplicarSeguridad(IQueryable<PedidoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<PedidoDtm, TipoDePedidoDtm, PermisoDelPedidoDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<PedidoDtm, PermisoDelPedidoDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void AntesDeMapearElRegistroParaInsertar(PedidoDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(elemento, opciones);
            if (elemento.Importe is not null)
            {
                if (elemento.IdNaturaleza.Entero() == 0)
                    Emitir("Si indica un importe de pedido, ha de indicar una naturaleza");
                opciones.Parametros.Add(nameof(PedidoDto.Importe), elemento.Importe);
                opciones.Parametros.Add(nameof(PedidoDto.IdNaturaleza), elemento.IdNaturaleza);
            }

            if (elemento.IdArchivoPedido is not null && elemento.Importe is null)
                Emitir("Si me indica un archivo de pedido externo, me ha de indicar su importe");

            opciones.Parametros[nameof(PedidoDto.IdArchivoPedido)] = elemento.IdArchivoPedido;
        }

        protected override void AntesDeMapearElRegistroParaModificar(PedidoDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaModificar(elemento, opciones);

            opciones.Parametros[nameof(PedidoDto.IdArchivoPedido)] = elemento.IdArchivoPedido;
        }

        protected override void AntesDePersistir(PedidoDtm pedido, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(pedido, parametros);

            pedido.InicializarDatosProveedor(Contexto, parametros, validarEnAlta: parametros.Insertando);
            if (parametros.Insertando)
            {
                AntesDeCrear(pedido, parametros);
            }
            else if (parametros.Modificando)
            {
                AntesDeModificar(pedido, parametros);
            }

            ValidarDatosNoModificablesSiNoEstaCumplimentandose(pedido, parametros);
        }

        private void AntesDeModificar(PedidoDtm pedido, ParametrosDeNegocio parametros)
        {
            var idArchivoPedido = parametros.Parametros.LeerValor<int?>(nameof(PedidoDto.IdArchivoPedido), null);
            pedido.Validar(Contexto, parametros);

            var pedidoEnBd = (PedidoDtm)parametros.registroEnBd;
            if (pedidoEnBd.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion) && idArchivoPedido != null)
            {
                pedido.IdArchivo = idArchivoPedido;
            }

            // El Dto no trae el archivo del pedido; en transiciones y acciones el registro viene de la BD y si no lo tiene es porque se ha quitado
            if (!parametros.EsUnaTransicion && !parametros.EstaEjecutandoUnaAccion && pedido.IdArchivo is null && pedidoEnBd.IdArchivo is not null)
                pedido.IdArchivo = pedidoEnBd.IdArchivo;
        }

        private void AntesDeCrear(PedidoDtm pedido, ParametrosDeNegocio parametros)
        {
            var idArchivoPedido = parametros.Parametros.LeerValor<int?>(nameof(PedidoDto.IdArchivoPedido), null);
            pedido.Validar(Contexto, parametros);
        }

        private void ValidarDatosNoModificablesSiNoEstaCumplimentandose(PedidoDtm far, ParametrosDeNegocio parametros)
        {
            if (far.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion))
                return;
        }

        protected override void DespuesDePersistir(PedidoDtm pedido, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(pedido, parametros);
            if (parametros.Insertando) DespuesDeCrear(pedido, parametros);
            if (parametros.Modificando)
            {
                var contratoCambiado = pedido.PropiedadCambiada<int?>(nameof(PedidoDtm.IdContrato), parametros);

                if (contratoCambiado)
                {
                    var anteriorId = ((PedidoDtm)parametros.registroEnBd).IdContrato.Entero();
                    var anterior = anteriorId == 0 ? null : Contexto.SeleccionarPorId<ContratoDtm>(anteriorId);

                    if (pedido.IdContrato is null)
                        pedido.DecrementarLoPlanificado(Contexto, anterior);

                    if (anterior is null)
                        pedido.IncrementarLoPlanificado(Contexto, pedido.Contrato(Contexto));
                }
            }
            // Sólo el archivo indicado por el usuario al cumplimentar; al cancelar la solicitud el pdf emitido se mantiene anexado y al quitar
            // el archivo del pedido el vínculo ya se ha borrado
            if (!parametros.EsUnaTransicion && !parametros.EstaEjecutandoUnaAccion && pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion))
                pedido.ProcesarArchivo(Contexto, parametros);
        }

        private void DespuesDeCrear(PedidoDtm pedido, ParametrosDeNegocio parametros)
        {
            if (pedido.IdArchivo is not null)
            {
                var importe = parametros.Parametros.LeerValor(nameof(PedidoDto.Importe), 0m);
                if (importe != 0) new LineaDeUnPedidoDtm
                {
                    TipoDeLinea = Enumerados.enumTipoDeLinea.Alzada,
                    IdElemento = pedido.Id,
                    Orden = Negocio.Parametro(enumParametrosDePedidos.PED_IncrementarOrdenEn, crearParametro: true, valorPorDefecto: 10).Valor.Entero(),
                    Concepto = pedido.Nombre,
                    IdNaturaleza = parametros.Parametros.LeerValor<int>(nameof(PedidoDto.IdNaturaleza)),
                    Precio = importe,
                    Cantidad = 1,
                    IdUnidad = enumNegocio.Pedido.Parametro(enumParametrosDePedidos.PED_Unidad_Medida, crearParametro: true, valorPorDefecto: Literal.Cero).Valor.Entero()
                }.Insertar(Contexto);
            }
        }



        protected override PedidoDtm AntesDeTransitar(PedidoDtm pedido, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            pedido = base.AntesDeTransitar(pedido, transicion, parametros);

            if (SeSolicita(transicion))
                pedido.AntesDeSolicitar(Contexto, parametros);

            if (SeCancelaLaSolicitud(transicion))
                pedido.AntesDeCancelarSolicitud(Contexto, parametros);

            // Al darlo por recibido la fecha de recepción de la cabecera es la de las entregas que se registran tras cerrarlo (ver DespuesDeTransitar)
            if (SeDaPorRecibido(transicion) && parametros != null)
                parametros[nameof(PedidoDtm.RecibidoEl)] = pedido.FechaDeRecepcionAlDarPorRecibido();

            if (SeRecibe(transicion))
                pedido.AntesDeRecibir(Contexto, parametros);

            if (SeAnulaLaRecepcion(transicion))
                pedido.AntesDeAnularRecepcion(Contexto, parametros);

            if (SeCierra(transicion))
                pedido.AntesDeCerrar(Contexto, parametros);

            if (SeReabre(transicion))
                pedido.AntesDeReabrir(Contexto, parametros);

            return pedido;
        }

        private static bool SeCierra(TransicionDtm transicion)
        =>
        transicion.DestinoEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_Cerrado.Estados()) &&
        !transicion.OrigenEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_Cerrado.Estados());

        private static bool SeReabre(TransicionDtm transicion)
        =>
        transicion.OrigenEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_Cerrado.Estados()) &&
        !transicion.DestinoEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_Cerrado.Estados());

        // De solicitado a la etapa de entrega o, con "Dar por recibido", directamente a cerrado
        private static bool SeRecibe(TransicionDtm transicion)
        =>
        transicion.OrigenEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Solicitud.Estados()) &&
        (transicion.DestinoEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Recepcion.Estados()) || transicion.DestinoEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_Cerrado.Estados()));

        // "Dar por recibido": de solicitado a cerrado sin pasar por recepción, se recepcionan todas las líneas
        private static bool SeDaPorRecibido(TransicionDtm transicion)
        =>
        transicion.OrigenEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Solicitud.Estados()) &&
        transicion.DestinoEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_Cerrado.Estados());

        private static bool SeAnulaLaRecepcion(TransicionDtm transicion)
        =>
        transicion.OrigenEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Recepcion.Estados()) &&
        transicion.DestinoEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Solicitud.Estados());

        // Permite al interventor corregir la cabecera fuera de la etapa de cumplimentación, en la que es de sólo consulta: el nombre, el correo
        // (y guardarlo como correo del proveedor), la fecha de entrega y, una vez recibido o cerrado, la de recepción (no posterior a hoy ni anterior
        // a la fecha en la que se creó el pedido) y la de cierre (no posterior a hoy). Los datos se toman de la edición, si desde que se leyó otro
        // usuario ha modificado el pedido no se guardan
        public static void ModificarCabecera(ContextoSe contexto, ModificarCabeceraDePedidoDto datos)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                var pedido = contexto.SeleccionarPorId<PedidoDtm>(datos.IdElemento, usarLaCache: false);
                pedido.ValidarRowId(contexto, datos.RowVersion, releer: false);

                if (!pedido.EsInterventor<TipoDePedidoDtm>(contexto))
                    Emitir($"Ha de ser interventor del pedido '{pedido.Referencia}' para modificar su cabecera");

                if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_Cancelado))
                    Emitir($"El pedido '{pedido.Referencia}' está anulado, no se puede modificar su cabecera");

                if (datos.Nombre.IsNullOrEmpty())
                    Emitir($"Ha de indicar el nombre del pedido '{pedido.Referencia}'");

                // lo que se modifica, con su valor anterior y el nuevo, para la traza
                var cambios = new List<string>();
                string Fecha(DateTime? fecha) => fecha is null ? "(vacía)" : ((DateTime)fecha).ToString("dd-MM-yyyy");
                string Texto(string texto) => texto.IsNullOrEmpty() ? "(vacío)" : $"'{texto}'";

                if ((pedido.Nombre ?? "") != (datos.Nombre ?? ""))
                    cambios.Add($"Nombre: {Texto(pedido.Nombre)} → {Texto(datos.Nombre)}");
                pedido.Nombre = datos.Nombre;

                if (!datos.eMail.IsNullOrEmpty() && !extCorreos.EsUnCorreo(datos.eMail))
                    Emitir(extCorreos.MensajeDeCorreoNoValido(datos.eMail));
                if ((pedido.eMail ?? "") != (datos.eMail ?? ""))
                    cambios.Add($"Correo: {Texto(pedido.eMail)} → {Texto(datos.eMail)}");
                pedido.eMail = datos.eMail;

                if (pedido.EntregarEl?.Date != datos.EntregarEl?.Date)
                    cambios.Add($"Entregar el: {Fecha(pedido.EntregarEl)} → {Fecha(datos.EntregarEl)}");
                pedido.EntregarEl = datos.EntregarEl;

                //if (datos.RecibidoEl?.Date != pedido.RecibidoEl?.Date)
                //{
                //    if (!pedido.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePedido> { enumEtapasDePedido.PED_Etapa_De_Recepcion, enumEtapasDePedido.PED_Etapa_Cerrado }))
                //        Emitir($"La fecha de recepción del pedido '{pedido.Referencia}' sólo se puede indicar una vez recibido");

                //    if (datos.RecibidoEl is null)
                //        Emitir($"Ha de indicar la fecha de recepción del pedido '{pedido.Referencia}'");

                //    var recibidoEl = ((DateTime)datos.RecibidoEl).Date;
                //    if (recibidoEl > DateTime.Today || recibidoEl < pedido.FechaCreacion.Date)
                //        Emitir($"La fecha de recepción del pedido '{pedido.Referencia}' ha de estar entre el '{pedido.FechaCreacion.ToString("dd-MM-yyyy")}', en el que se creó, y hoy");

                //    cambios.Add($"Recibido el: {Fecha(pedido.RecibidoEl)} → {Fecha(recibidoEl)}");
                //    pedido.RecibidoEl = recibidoEl;
                //}

                //if (datos.CerradoEl?.Date != pedido.CerradoEl?.Date)
                //{
                //    if (!pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_Cerrado))
                //        Emitir($"La fecha de cierre del pedido '{pedido.Referencia}' sólo se puede indicar una vez cerrado");

                //    if (datos.CerradoEl is null)
                //        Emitir($"Ha de indicar la fecha de cierre del pedido '{pedido.Referencia}'");

                //    var cerradoEl = ((DateTime)datos.CerradoEl).Date;
                //    if (cerradoEl > DateTime.Today)
                //        Emitir($"La fecha de cierre del pedido '{pedido.Referencia}' no puede ser posterior a hoy");

                //    // que no sea anterior a la de recepción lo valida ValidarFechas al modificar
                //    cambios.Add($"Cerrado el: {Fecha(pedido.CerradoEl)} → {Fecha(cerradoEl)}");
                //    pedido.CerradoEl = cerradoEl;
                //}

                var proveedor = pedido.Proveedor(contexto);
                var guardarEnElProveedor = datos.GuardarEnElProveedor && !datos.eMail.IsNullOrEmpty() && !datos.eMail.Equals(proveedor.eMail, StringComparison.CurrentCultureIgnoreCase);

                if (cambios.Count == 0 && !guardarEnElProveedor)
                    Emitir($"No ha modificado ningún dato de la cabecera del pedido '{pedido.Referencia}'");

                if (cambios.Count > 0)
                    pedido.Modificar(contexto, ltrDeUnPedido.Accion_ModificarCabecera);

                if (guardarEnElProveedor)
                {
                    cambios.Add($"Correo del proveedor '{proveedor.Nombre}': {Texto(proveedor.eMail)} → {Texto(datos.eMail)}");
                    proveedor.eMail = datos.eMail;
                    proveedor.Modificar(contexto, ltrDeUnPedido.Accion_ModificarCabecera);
                }

                // un dato modificado por línea, con su valor anterior y el nuevo
                pedido.CrearTraza(contexto, "Cabecera modificada",
                    $"El usuario '{contexto.DatosDeConexion.Login}' ha modificado la cabecera del pedido:{Environment.NewLine}{string.Join(Environment.NewLine, cambios)}");
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        protected override PedidoDtm DespuesDeTransitar(PedidoDtm pedido, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            pedido = base.DespuesDeTransitar(pedido, transicion, parametros);

            if (SeSolicita(transicion))
            {
                EmitirPdfPedido(Contexto, pedido);
                EnviarPedidoAlProveedor(Contexto, pedido);
            }

            // Se recepciona una vez cerrado: Recibir guarda las entregas y no ha de hacerlo en mitad de la transición
            if (SeDaPorRecibido(transicion))
                pedido.RecepcionarAlDarPorRecibido(Contexto);

            return pedido;
        }

        private static bool SeSolicita(TransicionDtm transicion)
        =>
        transicion.DestinoEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Solicitud.Estados()) &&
        (transicion.OrigenEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion.Estados()) || transicion.OrigenEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Aprobacion.Estados()));

        private static bool SeCancelaLaSolicitud(TransicionDtm transicion)
        =>
        transicion.OrigenEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Solicitud.Estados()) &&
        (transicion.DestinoEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion.Estados()) || transicion.DestinoEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Aprobacion.Estados()));

        // Al solicitar el pedido se emite su pdf, firmado si la sociedad tiene certificado, y queda como el archivo del pedido, salvo que el
        // usuario haya subido uno que lo sustituya mientras lo cumplimentaba o aprobaba
        public static void EmitirPdfPedido(ContextoSe contexto, PedidoDtm pedido)
        {
            if (pedido.IdArchivo is not null)
                return;

            var idArchivo = GenerarPdf(contexto, pedido);
            // AsociarArchivo relee el pedido ya transitado, guarda su IdArchivo (Modificar) y lo anexa; aquí sólo se refleja en el pedido en memoria
            pedido.AsociarArchivo(contexto, idArchivo, ltrDeUnPedido.Accion_AsociarArchivo);
            pedido.IdArchivo = idArchivo;
            FirmarPdf(contexto, pedido, idArchivo);
        }

        // Como en las facturas emitidas, se firma con el certificado de la sociedad si sólo tiene uno; si no se puede firmar queda la traza
        private static void FirmarPdf(ContextoSe contexto, PedidoDtm pedido, int idArchivo)
        {
            try
            {
                var certificados = GestorDeVinculos.RegistrosVinculados<CertificadoDtm>(contexto, enumNegocio.Sociedad, enumNegocio.Certificado, pedido.Cg(contexto).IdSociedad);
                if (certificados.Count != 1)
                    return;

                var password = ApiDeCertificados.LeerPasswordDeCertificado(contexto, certificados[0].Id);
                GestorDeArchivos.Gestor(contexto, contexto.Mapeador).FirmarAnexado(enumNegocio.Pedido, pedido.Id, idArchivo, certificados[0].Id, password,
                    new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
            }
            catch (Exception exc)
            {
                pedido.CrearTraza(contexto, "No se ha podido firmar el pedido", $"Se ha producido un error al firmar el pedido:{Environment.NewLine}{exc.Message}");
            }
        }

        // Si el pedido (o en su defecto su proveedor) tiene correo se le envía el archivo del pedido, el firmado si lo hay. El correo se encola en la
        // misma transacción, si la transición falla no se envía; si no se puede preparar el envío, el pedido queda solicitado y se anota la traza
        public static void EnviarPedidoAlProveedor(ContextoSe contexto, PedidoDtm pedido)
        {
            if (pedido.IdArchivo is null)
                return;
            var correo = pedido.eMail.IsNullOrEmpty() ? pedido.Proveedor(contexto).eMail : pedido.eMail;
            if (correo.IsNullOrEmpty())
            {
                pedido.CrearTraza(contexto, "Pedido no enviado por correo", $"Ni el pedido ni el proveedor tienen correo electrónico, ha de enviarle el pedido por otro medio");
                return;
            }
            try
            {
                var idArchivo = contexto.Set<FirmadoDtm>().FirstOrDefault(x => x.IdOriginal == (int)pedido.IdArchivo)?.IdFirmado ?? (int)pedido.IdArchivo;

                // El pdf va adjunto y, además, con un enlace de descarga válido 24 horas; si uno de los dos no se puede preparar el correo sale con el otro
                var adjunto = PrepararParaElCorreo(contexto, pedido, "adjuntar el pedido al correo", () => ServidorDocumental.FicheroParaAdjuntar(contexto, idArchivo));
                var enlace = PrepararParaElCorreo(contexto, pedido, "generar el enlace de descarga del pedido", () => contexto.SeleccionarPorId<ArchivoDtm>(idArchivo).HrefDeDescargaConGuid(contexto, horasDeValidez: CacheDeVariable.CFG_Horas_De_Validez_De_Enlaces_Archivos));
                if (adjunto is null && enlace is null)
                {
                    pedido.CrearTraza(contexto, "Pedido no enviado por correo", $"No se ha podido adjuntar el pedido ni generar su enlace de descarga, ha de enviárselo a '{correo}' por otro medio");
                    return;
                }

                var cuerpo = $"Le enviamos el pedido '{pedido.Referencia}' de {pedido.Sociedad(contexto).RazonSocial}: {pedido.Nombre}" +
                             (pedido.EntregarEl is null ? "" : $"{Environment.NewLine}Fecha de entrega solicitada: {((DateTime)pedido.EntregarEl).ToString("dd-MM-yyyy")}") +
                             (enlace is null ? "" : $"{Environment.NewLine}{Environment.NewLine}Puede descargarlo desde este enlace: {enlace}");

                GestorDeCorreos.CrearCorreoPara(contexto
                    , new List<string> { correo }
                    , $"Pedido {pedido.Referencia}"
                    , cuerpo
                    , new List<TipoDtoElmento>()
                    , adjunto is null ? new List<string>() : new List<string> { adjunto });

                pedido.CrearTraza(contexto, "Pedido enviado por correo", $"Se ha encolado el envío del pedido al correo '{correo}'");
            }
            catch (Exception exc)
            {
                pedido.CrearTraza(contexto, "No se ha podido enviar el pedido por correo", $"Se ha producido un error al preparar el envío a '{correo}':{Environment.NewLine}{exc.Message}");
            }
        }

        private static string PrepararParaElCorreo(ContextoSe contexto, PedidoDtm pedido, string queSePrepara, Func<string> preparar)
        {
            try
            {
                return preparar();
            }
            catch (Exception exc)
            {
                pedido.CrearTraza(contexto, $"No se ha podido {queSePrepara}", exc.Message);
                return null;
            }
        }

        public static void ImprimirPedido(ContextoSe contexto, PedidoDtm pedido)
        {
            var idArchivo = GenerarPdf(contexto, pedido);
            GestorDeVinculos.Vincular(contexto, enumNegocio.Pedido, enumNegocio.Archivos, pedido.Id, idArchivo);
        }

        private static int GenerarPdf(ContextoSe contexto, PedidoDtm pedido)
        {
            var nombrePropuesto = pedido.ProponerNombreDeArchivo(contexto, $"Ped-{pedido.Referencia}.pdf".NormalizarFichero());
            var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, nombrePropuesto);
            var pedidoRpt = new GeneradorDePedidoRpt(contexto, pedido).ObtenerInformacionDeRpt(plantilla: null);
            new ReporteDePedido(pedidoRpt).GeneratePdf(rutaConFichero);
            return ServidorDocumental.SubirArchivo(contexto, rutaConFichero, sanitizar: false);
        }


        protected override void DespuesDeMapearElElemento(PedidoDtm pedido, PedidoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(pedido, elemento, parametros);

            // La cabecera sólo es modificable mientras se cumplimenta, en el resto de etapas se consulta.
            // Solicitado y en recepción se mantiene el permiso para poder recepcionar (entregas de las líneas y "Dar por entregado"),
            // la cabecera se pone en consulta en el cliente (CrudEdicionPedido)
            if (elemento.ModoDeAcceso.HayPermisosDe(enumModoDeAccesoDeDatos.Gestor)
                && !pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion)
                && !pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Solicitud)
                && !pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Recepcion))
                elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;

            elemento.Importe = pedido.Importe(Contexto);
            if (parametros.LeerPorId)
            {
                var proveedor = pedido.Proveedor(Contexto);
                var idNaturalezaDelProveedor = proveedor?.IdNaturaleza;
                if (idNaturalezaDelProveedor.Entero() > 0)
                {
                    elemento.IdNaturaleza = idNaturalezaDelProveedor;
                }
                else
                {
                    var idNaturalezaDelIndicador = enumNegocio.Pedido.Parametro(enumParametrosDePedidos.PED_Naturaleza, crearParametro: true, valorPorDefecto: Literal.Cero).Valor.Entero();
                    elemento.IdNaturaleza = idNaturalezaDelIndicador > 0 ? idNaturalezaDelIndicador : null;
                }
                elemento.IdUnidadDelProveedor = proveedor?.IdUnidad;
                elemento.ConceptoDelProveedor = proveedor?.Concepto;
                elemento.BiPropuestoDelProveedor = proveedor?.BiPropuesto;
                elemento.UsaTarifaDelProveedor = proveedor != null && Contexto.Set<TarifaDtm>().Any(t => t.IdProveedor == proveedor.Id);
            }
        }


        // Como en las facturas, el archivo del pedido no se puede quitar una vez solicitado: sólo mientras se cumplimenta o se aprueba
        public static void AntesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var idPedido = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var pedido = entorno.Contexto.SeleccionarPorId<PedidoDtm>(idPedido);
            if (vinculado == enumNegocio.Archivos && pedido.IdArchivo.Entero() == entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado)))
            {
                if (pedido.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePedido> { enumEtapasDePedido.PED_Etapa_De_Cumplimentacion, enumEtapasDePedido.PED_Etapa_De_Aprobacion }))
                    return;

                var archivo = entorno.Contexto.SeleccionarPorId<ArchivoDtm>(pedido.IdArchivo.Entero());
                Emitir($"No puede quitar del {enumNegocio.Pedido.Singular(true)} '{pedido.Referencia}' el {enumNegocio.Archivos.Singular(true)} '{archivo.Nombre}' por ser el pedido, " +
                       $"sólo se puede quitar mientras está en la etapa de {enumEtapasDePedido.PED_Etapa_De_Cumplimentacion.Nombre()} o de {enumEtapasDePedido.PED_Etapa_De_Aprobacion.Nombre()}");
            }
        }

        // Si se ha quitado el archivo del pedido (sólo posible mientras se cumplimenta o se aprueba) el pedido deja de tenerlo
        public static void DespuesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var idPedido = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var pedido = entorno.Contexto.SeleccionarPorId<PedidoDtm>(idPedido);
            if (vinculado == enumNegocio.Archivos && pedido.IdArchivo.Entero() == entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado)))
            {
                pedido.IdArchivo = null;
                pedido.Modificar(entorno.Contexto, accionEjecutada: ApiDeEnsamblados.DespuesDeQuitarVinculo);
            }
        }

        public static void DespuesDeVincular(EntornoDeUnaAccion entorno)
        {
            var idPedido = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var pedido = entorno.Contexto.SeleccionarPorId<PedidoDtm>(idPedido);
            if (vinculado == enumNegocio.Archivos && pedido.IdArchivo.Entero() == 0)
            {

            }
        }

        public (bool EstabaSinImputar, string Mensaje) Imputar(int id, enumNegocio negocio, int idDondeImputar)
        {
            return negocio == enumNegocio.Contrato ? ImputarContrato(id, idDondeImputar) : ImputarExpediente(id, idDondeImputar);
        }

        public (bool EstabaSinImputar, string Mensaje) ImputarContrato(int id, int idDondeImputar)
        {
            var contrato = Contexto.SeleccionarPorId<ContratoDtm>(idDondeImputar);

            if (contrato.EstaEnLaEtapa(enumEtapasDeContratos.CTR_Etapa_Cancelado))
                Emitir($"El contrato '{contrato.Referencia}' está cancelado, no se le pueden imputar pedidos");

            if (!contrato.EsInterventor<TipoDeContratoDtm>(Contexto))
                Emitir($"Ha de ser interventor del contrato '{contrato.Referencia}' para poder imputarle pedidos");

            var pedido = Contexto.SeleccionarPorId<PedidoDtm>(id, aplicarJoin: true);

            if (pedido.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePedido> { enumEtapasDePedido.PED_Etapa_Cancelado, enumEtapasDePedido.PED_Etapa_Devuelto }))
                Emitir($"El pedido '{pedido.Referencia}' no puede estar en la etapa de '{enumEtapasDePedido.PED_Etapa_Cancelado.Nombre()} ni en la de '{enumEtapasDePedido.PED_Etapa_Devuelto.Nombre()}' para imputarla al contrato '{contrato.Referencia}'");

            if (pedido.IdContrato is null)
            {
                pedido.IdContrato = idDondeImputar;
                pedido.Modificar(Contexto, nameof(ImputarContrato));
                return (true, "");
            }

            return (false, $"El pedido '{pedido.Referencia}' ya estaba imputado al contrato '{pedido.Contrato(Contexto).Referencia}'");
        }

        public (bool EstabaSinImputar, string Mensaje) ImputarExpediente(int id, int idDondeImputar)
        {
            var expediente = Contexto.SeleccionarPorId<ExpedienteDtm>(idDondeImputar);

            if (expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Cancelada))
                Emitir($"El expediente '{expediente.Referencia}' está cancelado, no se le pueden imputar pedidos");

            if (!expediente.EsInterventor<TipoDeExpedienteDtm>(Contexto))
                Emitir($"Ha de ser interventor del expediente '{expediente.Referencia}' para poder imputarle pedidos");

            var pedido = Contexto.SeleccionarPorId<PedidoDtm>(id, aplicarJoin: true);

            if (pedido.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePedido> { enumEtapasDePedido.PED_Etapa_Cancelado, enumEtapasDePedido.PED_Etapa_Devuelto }))
                Emitir($"El pedido '{pedido.Referencia}' no puede estar en la etapa de '{enumEtapasDePedido.PED_Etapa_Cancelado.Nombre()} ni en la de '{enumEtapasDePedido.PED_Etapa_Devuelto.Nombre()}' para imputarla al expediente '{expediente.Referencia}'");

            if (pedido.IdExpediente is null)
            {
                pedido.IdExpediente = idDondeImputar;
                pedido.Modificar(Contexto, nameof(ImputarExpediente));
                return (true, "");
            }

            return (false, $"El pedido '{pedido.Referencia}' ya estaba imputado al expediente '{pedido.Expediente(Contexto).Referencia}'");
        }
        public void QuitarContrato(List<int> ids)
        {
            var trans = Contexto.IniciarTransaccion();
            try
            {
                foreach (int id in ids)
                {
                    var pedido = Contexto.SeleccionarPorId<PedidoDtm>(id);
                    if (pedido.IdContrato is null)
                        Emitir($"El pedido '{pedido.Referencia}' no está imputado a ningún contrato");

                    var contrato = Contexto.SeleccionarPorId<ContratoDtm>((int)pedido.IdContrato);
                    if (!contrato.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeContratos> { enumEtapasDeContratos.CTR_Etapa_Vigente }) && !contrato.EsInterventor(Contexto))
                        Emitir($"Ha de ser interventor del contrato '{contrato.Referencia}' para poder quitarle pedidos o el contrato a de estar vigente");

                    if (!pedido.EsInterventor(Contexto))
                        Emitir($"Ha de ser interventor de El pedido '{pedido.Referencia}' para poder qitarle el contrato");

                    pedido.IdContrato = null;
                    pedido.Modificar(Contexto, nameof(QuitarContrato));
                }
                Contexto.Commit(trans);
            }
            catch
            {
                Contexto.Rollback(trans);
                throw;
            }
        }
        public void QuitarExpediente(List<int> ids)
        {
            var trans = Contexto.IniciarTransaccion();
            try
            {
                foreach (int id in ids)
                {
                    var pedido = Contexto.SeleccionarPorId<PedidoDtm>(id);
                    if (pedido.IdExpediente is null)
                        Emitir($"El pedido '{pedido.Referencia}' no está imputado a ningún expediente");

                    var expediente = Contexto.SeleccionarPorId<ExpedienteDtm>((int)pedido.IdExpediente);

                    if (!expediente.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeExpedientes> { enumEtapasDeExpedientes.EXP_Etapa_Ejecucion, enumEtapasDeExpedientes.EXP_Etapa_Terminada }) && !expediente.EsInterventor(Contexto))
                        Emitir($"Ha de ser interventor del contrato '{expediente.Referencia}' para poder anular la imputación de pedidos o el expediente ha de estar en las etapas válidas");

                    if (!pedido.EsInterventor(Contexto))
                        Emitir($"Ha de ser interventor de El pedido '{pedido.Referencia}' para poder qitarle el expediente");

                    pedido.IdExpediente = null;
                    pedido.Modificar(Contexto, nameof(QuitarExpediente));
                }
                Contexto.Commit(trans);
            }
            catch
            {
                Contexto.Rollback(trans);
                throw;
            }
        }


        public async Task<TotalesDePedidos> ObtenerTotalesAsync(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            return await Task.Run(() => ObtenerTotales(filtros, posicion, cantidad));
        }

        public TotalesDePedidos ObtenerTotales(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            var pedidos = Contexto.SeleccionarTodos<PedidoDtm>(filtros, parametros: new Dictionary<string, object> {
                { ltrParametrosNeg.PosicionInicial, posicion},
                { ltrParametrosNeg.CantidadPorLeer, cantidad},
                { ltrParametrosNeg.Peticion, enumPeticion.epTotales}
            });
            var importes = pedidos.ToDictionary(p => p.Id, p => p.Importe(Contexto));
            var totales = new TotalesDePedidos();

            totales.Pendiente = Sumar(pedidos, EstadosDe(enumEtapasDePedido.PED_Etapa_De_Solicitud), importes);
            totales.Recibido = Sumar(pedidos, EstadosDe(enumEtapasDePedido.PED_Etapa_De_Recepcion, enumEtapasDePedido.PED_Etapa_Cerrado), importes);
            totales.TotalPedido = totales.Pendiente + totales.Recibido;
            totales.EnCumplimentacion = Sumar(pedidos, EstadosDe(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion), importes);
            totales.Devuelto = Sumar(pedidos, EstadosDe(enumEtapasDePedido.PED_Etapa_Devuelto), importes);

            totales.TotalesPorProveedor = FormatearTotalesPorProveedor(pedidos, importes);
            totales.TotalesPorNaturaleza = FormatearTotalesPorNaturaleza(pedidos);

            totales.Procesados = pedidos.Count();
            return totales;
        }

        private static HashSet<int> EstadosDe(params enumEtapasDePedido[] etapas) => etapas.SelectMany(etapa => etapa.Lista()).ToHashSet();

        private static decimal Sumar(IEnumerable<PedidoDtm> pedidos, HashSet<int> estados, Dictionary<int, decimal> importes)
        => pedidos.Where(p => estados.Contains(p.IdEstado)).Sum(p => importes[p.Id]);

        private string FormatearTotalesPorProveedor(List<PedidoDtm> pedidos, Dictionary<int, decimal> importes)
        {
            if (!pedidos.Any()) return string.Empty;

            var porSolicitar = EstadosDe(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion, enumEtapasDePedido.PED_Etapa_De_Aprobacion);
            var solicitado = EstadosDe(enumEtapasDePedido.PED_Etapa_De_Solicitud);
            var entregado = EstadosDe(enumEtapasDePedido.PED_Etapa_De_Recepcion, enumEtapasDePedido.PED_Etapa_Cerrado);

            var filas = pedidos
                .GroupBy(p => p.IdProveedor)
                .Select(g => (
                    nombre: g.First().Proveedor(Contexto).Nombre,
                    porSolicitar: Sumar(g, porSolicitar, importes),
                    solicitado: Sumar(g, solicitado, importes),
                    entregado: Sumar(g, entregado, importes)
                ))
                .Where(f => f.porSolicitar != 0 || f.solicitado != 0 || f.entregado != 0)
                .OrderByDescending(f => f.solicitado + f.entregado)
                .ToList();

            if (!filas.Any()) return string.Empty;

            const int anchoNombre = 40;
            const int anchoImporte = 16;

            string Linea(string nombre, decimal aSolicitar, decimal pedido, decimal servido)
            {
                if (nombre.Length > anchoNombre) nombre = nombre.Substring(0, anchoNombre - 1) + "…";
                return
                    $"{nombre.PadRight(anchoNombre)}" +
                    $"{aSolicitar.ToString("N2").PadLeft(anchoImporte)}" +
                    $"{pedido.ToString("N2").PadLeft(anchoImporte)}" +
                    $"{servido.ToString("N2").PadLeft(anchoImporte)}" +
                    $"{(pedido + servido).ToString("N2").PadLeft(anchoImporte)}" +
                    $"   ";
            }

            var separador = new string('-', anchoNombre + anchoImporte * 4);
            var sb = new StringBuilder();
            sb.AppendLine(
                $"{"Proveedor".PadRight(anchoNombre)}" +
                $"{"Por solicitar".PadLeft(anchoImporte)}" +
                $"{"Solicitado".PadLeft(anchoImporte)}" +
                $"{"Entregado".PadLeft(anchoImporte)}" +
                $"{"Total".PadLeft(anchoImporte)}" +
                $"   "
            );
            sb.AppendLine(separador);

            foreach (var fila in filas)
                sb.AppendLine(Linea(fila.nombre, fila.porSolicitar, fila.solicitado, fila.entregado));

            sb.AppendLine(separador);
            sb.AppendLine(Linea("Total", filas.Sum(f => f.porSolicitar), filas.Sum(f => f.solicitado), filas.Sum(f => f.entregado)));

            return sb.ToString();
        }

        private string FormatearTotalesPorNaturaleza(List<PedidoDtm> pedidos)
        {
            var excluidos = EstadosDe(enumEtapasDePedido.PED_Etapa_Cancelado, enumEtapasDePedido.PED_Etapa_Devuelto);
            var lineas = pedidos
                .Where(p => !excluidos.Contains(p.IdEstado))
                .SelectMany(p => p.Detalles<LineaDeUnPedidoDtm>(Contexto))
                .ToList();

            if (!lineas.Any()) return string.Empty;

            var filas = lineas
                .GroupBy(l => l.IdNaturaleza)
                .Select(g => (
                    nombre: g.Key is null ? "(sin naturaleza)" : Contexto.SeleccionarPorId<NaturalezaDtm>((int)g.Key).Expresion,
                    lineas: g.Count(),
                    importe: g.Sum(l => l.ImporteDeLinea)
                ))
                .OrderByDescending(f => f.importe)
                .ToList();

            var total = filas.Sum(f => f.importe);

            const int anchoNombre = 40;
            const int anchoNum = 10;
            const int anchoImporte = 16;

            string Linea(string nombre, int numLineas, decimal importe)
            {
                if (nombre.Length > anchoNombre) nombre = nombre.Substring(0, anchoNombre - 1) + "…";
                var porcentaje = total == 0 ? 0m : importe / total * 100;
                return
                    $"{nombre.PadRight(anchoNombre)}" +
                    $"{numLineas.ToString().PadLeft(anchoNum)}" +
                    $"{importe.ToString("N2").PadLeft(anchoImporte)}" +
                    $"{porcentaje.ToString("N2").PadLeft(anchoNum)}" +
                    $"   ";
            }

            var separador = new string('-', anchoNombre + anchoNum * 2 + anchoImporte);
            var sb = new StringBuilder();
            sb.AppendLine(
                $"{"Naturaleza".PadRight(anchoNombre)}" +
                $"{"Líneas".PadLeft(anchoNum)}" +
                $"{"Importe".PadLeft(anchoImporte)}" +
                $"{"%".PadLeft(anchoNum)}" +
                $"   "
            );
            sb.AppendLine(separador);

            foreach (var fila in filas)
                sb.AppendLine(Linea(fila.nombre, fila.lineas, fila.importe));

            sb.AppendLine(separador);
            sb.AppendLine(Linea("Total", lineas.Count, total));

            return sb.ToString();
        }

    }

}
