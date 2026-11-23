using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Logistica;
using ModeloDeDto.Logistica;
using System.IO;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.SistemaDocumental;
using QuestPDF.Fluent;
using ServicioDeReportes.Logistica;
using Gestor.Errores;
using ServicioDeDatos.Seguridad;

namespace GestoresDeNegocio.Logistica
{

    public class GestorDeVentas : GestorDeElementos<ContextoSe, VentaDtm, VentaDto>
    {
        public class MapearVenta : Profile
        {
            public MapearVenta()
            {
                CreateMap<VentaDtm, VentaDto>()
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Expresion))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado.Nombre))
                .ForMember(dto => dto.Cliente, dtm => dtm.MapFrom(dtm => dtm.Cliente.Expresion))
                .ForMember(dto => dto.Almacen, dtm => dtm.MapFrom(dtm => dtm.Almacen.Expresion))
                .ForMember(dto => dto.FacturaEmt, dtm => dtm.MapFrom(dtm => dtm.FacturaEmt == null ? null : dtm.FacturaEmt.Expresion))
                .ForMember(dto => dto.TotalSinIva, dtm => dtm.Ignore())
                .ForMember(dto => dto.TieneDevoluciones, dtm => dtm.Ignore());

                CreateMap<VentaDto, VentaDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore())
                .ForMember(dtm => dtm.Cliente, dto => dto.Ignore())
                .ForMember(dtm => dtm.Almacen, dto => dto.Ignore())
                .ForMember(dtm => dtm.FacturaEmt, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.Venta;

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeVenta.Gestor(Contexto, Contexto.Mapeador);

        public GestorDeVentas(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeVentas Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeVentas(contexto, mapeador);
        }

        protected override void DespuesDeMapearElRegistro(VentaDto dto, VentaDtm venta, ParametrosDeNegocio opciones)
        {
            venta.IdCg = venta.Almacen(Contexto).IdCg;
        }

        protected override IQueryable<VentaDtm> AplicarJoins(IQueryable<VentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Cliente)
                           .Include(x => x.Almacen)
                           .Include(x => x.FacturaEmt);
        }

        protected override IQueryable<VentaDtm> AplicarOrden(IQueryable<VentaDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override IQueryable<VentaDtm> AplicarFiltros(IQueryable<VentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            // TODO: completar los filtros propios del negocio de documentos de venta
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroPorFactura(filtros, parametros);
            consulta = consulta.FiltroPorUnitario(Contexto, filtros);
            consulta = consulta.FiltroPorImporte(Contexto, filtros);
            return consulta;
        }

        protected override IQueryable<VentaDtm> AplicarSeguridad(IQueryable<VentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<VentaDtm, TipoDeVentaDtm, PermisoDeLaVentaDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<VentaDtm, PermisoDeLaVentaDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void AntesDePersistir(VentaDtm venta, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(venta, parametros);
            ValidarQueEsModificable(venta, parametros);
            venta.InicializarDatosCliente(Contexto, parametros);

            // la fecha de servicio la indica el usuario mientras está solicitado; al transitar o ejecutar una acción la pone el sistema
            if ((parametros.Insertando || parametros.Modificando) && !parametros.EsUnaTransicion && !parametros.EstaEjecutandoUnaAccion)
            {
                if (venta.ServidoEl is not null)
                    venta.ServidoEl = ((System.DateTime)venta.ServidoEl).Date;
                venta.ValidarFechaDeServicio();
            }
        }

        // Una vez servido (y facturado o cancelado) el documento de venta no se puede modificar ni borrar: sólo en la etapa de solicitud.
        // Lo que cambia el propio sistema al transitar o al ejecutar una acción (fecha de servicio, prefactura...) sí se guarda
        private void ValidarQueEsModificable(VentaDtm venta, ParametrosDeNegocio parametros)
        {
            if (parametros.Operacion != enumTipoOperacion.Modificar && parametros.Operacion != enumTipoOperacion.Eliminar)
                return;
            if (parametros.EsUnaTransicion || parametros.EstaEjecutandoUnaAccion)
                return;

            var ventaEnBd = Contexto.SeleccionarPorId<VentaDtm>(venta.Id, usarLaCache: false);
            if (!ventaEnBd.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Solicitud))
                GestorDeErrores.Emitir($"El {enumNegocio.Venta.Singular(true)} '{ventaEnBd.Referencia}' no se puede {(parametros.Operacion == enumTipoOperacion.Eliminar ? "borrar" : "modificar")} por no estar en la etapa de {enumEtapasDeVenta.DSA_Etapa_Solicitud.Nombre(true)}");
        }

        protected override void DespuesDePersistir(VentaDtm venta, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(venta, parametros);
        }

        protected override VentaDtm AntesDeTransitar(VentaDtm venta, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            venta = base.AntesDeTransitar(venta, transicion, parametros);

            // lo que se cambia aquí en la venta se guarda al transitar (ver GestorDeHitos.Transitar)
            if (transicion.EntreEtapas(enumEtapasDeVenta.DSA_Etapa_Solicitud.Estados(), enumEtapasDeVenta.DSA_Etapa_Servicio.Estados()))
                venta.AntesDeServir(Contexto);
            else if (transicion.EntreEtapas(enumEtapasDeVenta.DSA_Etapa_Servicio.Estados(), enumEtapasDeVenta.DSA_Etapa_Solicitud.Estados()))
                venta.AntesDeDevolverASolicitado(Contexto);
            else if (transicion.EntreEtapas(enumEtapasDeVenta.DSA_Etapa_Facturacion.Estados(), enumEtapasDeVenta.DSA_Etapa_Servicio.Estados()))
                venta.AlCancelarPrefactura(Contexto, parametros);
            else if (transicion.DestinoEstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Facturacion.Estados()))
                venta.AntesDeFacturar(Contexto);
            else if (transicion.DestinoEstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Cancelacion.Estados()))
                venta.AntesDeCancelar(Contexto);

            return venta;
        }

        protected override VentaDtm DespuesDeTransitar(VentaDtm venta, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            venta = base.DespuesDeTransitar(venta, transicion, parametros);

            // al servir se crean las salidas del almacén de sus líneas (con su preasiento y el barrido de los posteriores)
            if (transicion.EntreEtapas(enumEtapasDeVenta.DSA_Etapa_Solicitud.Estados(), enumEtapasDeVenta.DSA_Etapa_Servicio.Estados()))
                venta.CrearSalidasDelAlmacen(Contexto);
            else if (transicion.DestinoEstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Facturacion.Estados()))
                venta.CrearPrefactura(Contexto);

            return venta;
        }

        // Oferta del documento de venta (ver ReporteDeOfertaDeVenta): se genera su pdf, se sube al servidor documental y se anexa a los
        // archivos del documento de venta, como la impresión del pedido (GestorDePedidos.ImprimirPedido)
        private static string NombreDeLaOferta(VentaDtm venta) => $"Oferta-{venta.Referencia}.pdf".NormalizarFichero();

        // Al modificar lo servido (ver ExtensorDeVentas.ModificarLineaServida): si la oferta ya estaba impresa y anexada (no se guarda su
        // id, se busca por su nombre) se renombra con un postfijo numérico, "Oferta-ref (1).pdf", "(2)"..., y se vuelve a imprimir con el
        // nombre original. Devuelve el nombre con el que ha quedado la anterior, o null si no estaba impresa
        public static string ReimprimirOferta(ContextoSe contexto, VentaDtm venta)
        {
            var nombre = NombreDeLaOferta(venta);
            var anexados = venta.LeerAnexados(contexto);
            var impresa = anexados.FirstOrDefault(a => a.Nombre.Equals(nombre, System.StringComparison.CurrentCultureIgnoreCase));
            if (impresa is null)
                return null;

            var sinExtension = Path.GetFileNameWithoutExtension(nombre);
            var extension = Path.GetExtension(nombre);
            var numero = 1;
            while (anexados.Any(a => a.Nombre.Equals($"{sinExtension} ({numero}){extension}", System.StringComparison.CurrentCultureIgnoreCase)))
                numero++;

            var renombrada = $"{sinExtension} ({numero}){extension}";
            impresa.Renombrar(contexto, renombrada);
            ImprimirOferta(contexto, venta);
            return renombrada;
        }

        public static void ImprimirOferta(ContextoSe contexto, VentaDtm venta)
        {
            var nombrePropuesto = venta.ProponerNombreDeArchivo(contexto, NombreDeLaOferta(venta));
            var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, nombrePropuesto);
            var ofertaRpt = new GeneradorDeOfertaDeVentaRpt(contexto, venta).ObtenerInformacionDeRpt(plantilla: null);
            new ReporteDeOfertaDeVenta(ofertaRpt).GeneratePdf(rutaConFichero);
            var idArchivo = ServidorDocumental.SubirArchivo(contexto, rutaConFichero, sanitizar: false);
            GestorDeVinculos.Vincular(contexto, enumNegocio.Venta, enumNegocio.Archivos, venta.Id, idArchivo);
        }

        protected override void DespuesDeMapearElElemento(VentaDtm venta, VentaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(venta, elemento, parametros);
            elemento.TotalSinIva = venta.TotalSinIva(Contexto);
            if (parametros.LeerPorId)
                elemento.TieneDevoluciones = venta.Detalles<LineaDeUnaVentaDtm>(Contexto).Any(l => l.CantidadDevuelta(Contexto) > 0);

            // sólo se edita en la etapa de solicitud (ver ValidarQueEsModificable): en el resto se consulta, aunque se puede transitar
            if (parametros.LeerPorId && elemento.ModoDeAcceso.HayPermisosDe(enumModoDeAccesoDeDatos.Gestor)
                && !venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Solicitud))
                elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
        }

    }

}
