using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using GestorDeElementos.Extensores;
using System;
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
                .ForMember(dto => dto.Contrato, dtm => dtm.MapFrom(dtm => dtm.Contrato == null ? null : dtm.Contrato.Expresion));

                CreateMap<PedidoDto, PedidoDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore())
                .ForMember(dtm => dtm.Proveedor, dto => dto.Ignore())
                .ForMember(dtm => dtm.Expediente, dto => dto.Ignore())
                .ForMember(dtm => dtm.Contrato, dto => dto.Ignore());
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

        protected override void DespuesDeMapearElRegistro(PedidoDto dto, PedidoDtm dtm, ParametrosDeNegocio opciones)
        {

        }

        protected override IQueryable<PedidoDtm> AplicarJoins(IQueryable<PedidoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Proveedor);
            consulta = consulta.Include(x => x.Contrato);
            consulta = consulta.Include(x => x.Expediente);
            return consulta;
        }

        protected override IQueryable<PedidoDtm> AplicarOrden(IQueryable<PedidoDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override IQueryable<PedidoDtm> AplicarFiltros(IQueryable<PedidoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            parametros.AplicarFiltroQueMostrar = !filtros.OmitirFiltrosPorEstado(new List<string> { ltrDeUnPedido.IdContrato, ltrDeUnPedido.PedidosImputablesAlContrato, ltrDeUnPedido.IdExpediente, ltrDeUnPedido.PedidosImputablesAlExpediente });
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroPorEjercicio(filtros);
            consulta = consulta.FiltroPorProveedor(filtros);
            consulta = consulta.FiltroPorAsuntoReferenciaPedido(Contexto, filtros);
            consulta = consulta.FiltroPorFechaPedido(filtros);
            consulta = consulta.FiltroPorFechaDeEntrega(filtros);
            //consulta = consulta.FiltroPorEtapa(filtros);
            consulta = consulta.FiltroPedidosPosiblesDelContrato(Contexto, filtros);
            consulta = consulta.FiltroPedidosPosiblesDeUnExpediente(Contexto, filtros, parametros);
            consulta = consulta.FiltroSiHayDependenciaDe(filtros, nameof(PedidoDtm.IdExpediente), ltrDeUnPedido.AsociadaAUnExpediente, parametros, aplicarFiltroDeEstado: false);
            consulta = consulta.FiltroSiHayDependenciaDe(filtros, nameof(PedidoDtm.IdContrato), ltrDeUnPedido.AsociadaAUnContrato, parametros, aplicarFiltroDeEstado: false);

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
                if (elemento.IdNaturaleza.Entero() == 0 || elemento.ClaseDeLinea == null)
                    Emitir("Si indica un importe de pedido, ha de indicar una naturaleza y una clase de lo pedido");
                opciones.Parametros.Add(nameof(PedidoDto.Importe), elemento.Importe);
                opciones.Parametros.Add(nameof(PedidoDto.IdNaturaleza), elemento.IdNaturaleza);
                opciones.Parametros.Add(nameof(PedidoDto.ClaseDeLinea), elemento.ClaseDeLinea);
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
            pedido.Validar(Contexto);
            
            if (idArchivoPedido != null)
            {
                pedido.IdArchivo = idArchivoPedido;
            }

            if (pedido.IdArchivo is null && ((PedidoDtm)parametros.registroEnBd).IdArchivo is not null)
                pedido.IdArchivo = ((PedidoDtm)parametros.registroEnBd).IdArchivo;
        }

        private void AntesDeCrear(PedidoDtm pedido, ParametrosDeNegocio parametros)
        {
            var idArchivoPedido = parametros.Parametros.LeerValor<int?>(nameof(PedidoDto.IdArchivoPedido), null);
            pedido.Validar(Contexto);
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
            if (pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion))
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
                    Clase = parametros.Parametros.LeerValor<enumClaseUnitario>(nameof(PedidoDto.IdNaturaleza)),
                    Precio = importe,
                    Cantidad = 1,
                    IdUnidad = enumNegocio.Pedido.Parametro(enumParametrosDePedidos.PED_Unidad_Medida, crearParametro: true, valorPorDefecto: Literal.Cero).Valor.Entero()
                }.Insertar(Contexto);
            }
        }



        protected override PedidoDtm AntesDeTransitar(PedidoDtm pedido, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            pedido = base.AntesDeTransitar(pedido, transicion, parametros);

            if (transicion.DestinoEstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Solicitud.Estados()) &&
                pedido.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePedido> { enumEtapasDePedido.PED_Etapa_De_Aprobacion, enumEtapasDePedido.PED_Etapa_De_Cumplimentacion }))
                pedido.AntesDeSolicitar(Contexto, parametros);

            return pedido;
        }

        protected override PedidoDtm DespuesDeTransitar(PedidoDtm pedido, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            pedido = base.DespuesDeTransitar(pedido, transicion, parametros);
            return pedido;
        }


        protected override void DespuesDeMapearElElemento(PedidoDtm pedido, PedidoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(pedido, elemento, parametros);
            elemento.Importe = pedido.Importe(Contexto);
        }


        public static void AntesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var idPedido = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var pedido = entorno.Contexto.SeleccionarPorId<PedidoDtm>(idPedido);
            if (vinculado == enumNegocio.Archivos)
            {
                if (pedido.IdArchivo.Entero() == entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado)))
                {
                    var archivo = entorno.Contexto.SeleccionarPorId<ArchivoDtm>(pedido.IdArchivo.Entero());
                    var firmado = entorno.Contexto.Set<FirmadoDtm>().Where(x => x.IdOriginal == (int)pedido.IdArchivo).FirstOrDefault();
                    if (firmado != null)
                    {
                        if (firmado.IdOriginal == pedido.IdArchivo)
                            Emitir($"No puede quitar del {enumNegocio.Pedido.Singular(true)} '{pedido.Referencia}' el {enumNegocio.Archivos.Singular(true)} '{archivo.Nombre}' por ser el pedido");
                    }
                    else
                        Emitir($"No puede quitar del {enumNegocio.Pedido.Singular(true)} '{pedido.Referencia}' el {enumNegocio.Archivos.Singular(true)} '{archivo.Nombre}' por ser el pedido");
                }
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
            var totales = new TotalesDePedidos();

            totales.Procesados = pedidos.Count();
            return totales;
        }

    }

}
