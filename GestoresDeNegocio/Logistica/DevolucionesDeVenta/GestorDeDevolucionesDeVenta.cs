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
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Logistica;
using Gestor.Errores;
using System.IO;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.SistemaDocumental;
using QuestPDF.Fluent;
using ServicioDeReportes.Logistica;

namespace GestoresDeNegocio.Logistica
{

    public class GestorDeDevolucionesDeVenta : GestorDeElementos<ContextoSe, DevolucionDeVentaDtm, DevolucionDeVentaDto>
    {
        public class MapearDevolucionDeVenta : Profile
        {
            public MapearDevolucionDeVenta()
            {
                CreateMap<DevolucionDeVentaDtm, DevolucionDeVentaDto>()
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Expresion))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado.Nombre))
                .ForMember(dto => dto.Cliente, dtm => dtm.MapFrom(dtm => dtm.Cliente.Expresion))
                .ForMember(dto => dto.TotalSinIva, dtm => dtm.Ignore());

                CreateMap<DevolucionDeVentaDto, DevolucionDeVentaDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore())
                .ForMember(dtm => dtm.Cliente, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.DevolucionVenta;

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeDevolucionDeVenta.Gestor(Contexto, Contexto.Mapeador);

        public GestorDeDevolucionesDeVenta(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeDevolucionesDeVenta Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeDevolucionesDeVenta(contexto, mapeador);
        }

        protected override IQueryable<DevolucionDeVentaDtm> AplicarJoins(IQueryable<DevolucionDeVentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Cliente);
        }

        protected override IQueryable<DevolucionDeVentaDtm> AplicarFiltros(IQueryable<DevolucionDeVentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroEnTramitacion(filtros);
            consulta = consulta.FiltroPorCliente(filtros);
            return consulta;
        }

        protected override IQueryable<DevolucionDeVentaDtm> AplicarSeguridad(IQueryable<DevolucionDeVentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<DevolucionDeVentaDtm, TipoDeDevolucionDeVentaDtm, PermisoDeLaDevolucionDeVentaDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<DevolucionDeVentaDtm, PermisoDeLaDevolucionDeVentaDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void AntesDePersistir(DevolucionDeVentaDtm devolucion, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(devolucion, parametros);
            ValidarQueEsModificable(devolucion, parametros);
            devolucion.InicializarDatosCliente(Contexto, parametros);
        }

        // Una vez aceptada la devolución no se puede modificar ni borrar: sólo mientras se tramita. Lo que cambia el propio sistema al
        // transitar o al ejecutar una acción sí se guarda
        private void ValidarQueEsModificable(DevolucionDeVentaDtm devolucion, ParametrosDeNegocio parametros)
        {
            if (parametros.Insertando)
                return;
            if (parametros.EsUnaTransicion || parametros.EstaEjecutandoUnaAccion)
                return;

            var enBd = (DevolucionDeVentaDtm)parametros.registroEnBd;
            if (!enBd.EstaEnLaEtapa(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion))
                GestorDeErrores.Emitir($"La {enumNegocio.DevolucionVenta.Singular(true)} '{enBd.Referencia}' no se puede {(parametros.Operacion == enumTipoOperacion.Eliminar ? "borrar" : "modificar")} por no estar en la etapa de {enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion.Nombre(true)}");
        }

        protected override DevolucionDeVentaDtm AntesDeTransitar(DevolucionDeVentaDtm devolucion, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            devolucion = base.AntesDeTransitar(devolucion, transicion, parametros);

            // lo que se cambia aquí (la fecha de la devolución) se guarda al transitar (ver GestorDeHitos.Transitar)
            if (transicion.EntreEtapas(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion.Estados(), enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada.Estados()))
                devolucion.AntesDeAceptar(Contexto);

            return devolucion;
        }

        // Al aceptar: entradas en el almacén (con su preasiento y el barrido de los posteriores), regularización de las facturas de las
        // ventas devueltas (ver ExtensorDeDevolucionesDeVenta.RegularizarFacturas) y el documento de la devolución, que se anexa a sus archivos
        protected override DevolucionDeVentaDtm DespuesDeTransitar(DevolucionDeVentaDtm devolucion, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            devolucion = base.DespuesDeTransitar(devolucion, transicion, parametros);

            if (transicion.EntreEtapas(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion.Estados(), enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada.Estados()))
            {
                devolucion.AlAceptar(Contexto);
                ImprimirDevolucion(Contexto, devolucion);
            }

            return devolucion;
        }

        // Documento de la devolución (ver ReporteDeDevolucionDeVenta): se genera su pdf, se sube al servidor documental y se anexa a los
        // archivos de la devolución, como la oferta del documento de venta (GestorDeVentas.ImprimirOferta)
        public static void ImprimirDevolucion(ContextoSe contexto, DevolucionDeVentaDtm devolucion)
        {
            var nombrePropuesto = devolucion.ProponerNombreDeArchivo(contexto, $"Devolucion-{devolucion.Referencia}.pdf".NormalizarFichero());
            var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, nombrePropuesto);
            var devolucionRpt = new GeneradorDeDevolucionDeVentaRpt(contexto, devolucion).ObtenerInformacionDeRpt(plantilla: null);
            new ReporteDeDevolucionDeVenta(devolucionRpt).GeneratePdf(rutaConFichero);
            var idArchivo = ServidorDocumental.SubirArchivo(contexto, rutaConFichero, sanitizar: false);
            GestorDeVinculos.Vincular(contexto, enumNegocio.DevolucionVenta, enumNegocio.Archivos, devolucion.Id, idArchivo);
        }

        protected override void DespuesDeMapearElElemento(DevolucionDeVentaDtm devolucion, DevolucionDeVentaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(devolucion, elemento, parametros);
            elemento.TotalSinIva = devolucion.TotalSinIva(Contexto);

            // sólo se edita mientras se tramita (ver ValidarQueEsModificable): una vez aceptada se consulta
            if (parametros.LeerPorId && elemento.ModoDeAcceso.HayPermisosDe(enumModoDeAccesoDeDatos.Gestor)
                && !devolucion.EstaEnLaEtapa(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion))
                elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
        }

    }

}
