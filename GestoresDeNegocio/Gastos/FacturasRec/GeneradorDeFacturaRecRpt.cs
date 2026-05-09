using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.SistemaDocumental;
using ModeloDeDto;
using ModeloDeDto.Terceros;
using ModeloDeDto.Gastos;
using ServicioDeDatos;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Gastos;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using ServicioDeDatos.Entorno;

namespace GestoresDeNegocio.Gastos
{

    public class GeneradorDeFacturaRecRpt : IGeneradorRpt<FacturaRecDto>
    {
        private ContextoSe Contexto { get; }
        private FacturaRecDtm Factura { get; }

        public GeneradorDeFacturaRecRpt(ContextoSe contexto, FacturaRecDtm factura)
        {
            Contexto = contexto;
            Factura = factura;
        }

        public IInformacionRpt<FacturaRecDto> ObtenerInformacionDeRpt(string plantilla)
        {
            var informacionRpt = new FacturaRecRpt();
            informacionRpt.Datos = Factura.MapearDto<FacturaRecDto>(Contexto);


            informacionRpt.Sociedad = Contexto.SeleccionarDto<SociedadDto, SociedadDtm>(
                          id: Factura.Cg(Contexto).IdSociedad,
                          parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.ObtenerDatosFiscales, true } }));

            informacionRpt.Proveedor = Contexto.SeleccionarDto<ProveedorDto, ProveedorDtm>(
                          id: Factura.IdProveedor,
                          parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.ObtenerDatosFiscales, true } }));

            informacionRpt.Direccion = Factura.DireccionFiscal(Contexto, errorSiNoHay: false);
            //if (informacionRpt.Direccion is null)
            //    GestorDeErrores.Emitir("La factura ha de tener una direción fical");

            informacionRpt.Logo = informacionRpt.Sociedad.IdArchivo is null
            ? ApiDeArchivos.FicheroNoEncontrado
            : ServidorDocumental.DescargarArchivo(Contexto, (int)informacionRpt.Sociedad.IdArchivo, solicitadoPorLaCola: false, erroSiNoEstaEnLaruta: false);

            //var filtro = new List<ClausulaDeFiltrado>
            //{
            //    new ClausulaDeFiltrado(nameof(LineaDeUnaFarDtm.IdElemento), enumCriteriosDeFiltrado.igual, Factura.Id),
            //};
            //informacionRpt.Lineas = GestorDeLineasDeUnaFar.Gestor(Contexto, Contexto.Mapeador).LeerElementos(0, -1, filtro,
            //new List<ClausulaDeOrdenacion> { new ClausulaDeOrdenacion(nameof(LineaDeUnaFarDtm.Orden), ModoDeOrdenancion.ascendente) },
            //new Dictionary<string, object>()
            //).ToList();
            informacionRpt.Lineas = new List<LineaDeUnaFarDto>();

            var esIntraComunitario = Factura.Proveedor(Contexto).EsIntraComunitario(Contexto);
            var esExtraComunitario = Factura.Proveedor(Contexto).EsExtraComunitario(Contexto);

            foreach (var linea in Factura.Detalles<LineaDeUnaFarDtm>(Contexto, aplicarJoin: true).OrderBy(x => x.Orden))
            {
                informacionRpt.Lineas.Add(linea.MapearDto<LineaDeUnaFarDto>(Contexto));
                informacionRpt.IncluirIva(linea, esExtraComunitario, esIntraComunitario);
            }

            informacionRpt.Observaciones = enumNegocio.FacturaRecibida.Observaciones(Contexto).Where(obs => obs.IdElemento == Factura.Id).ToList();

            informacionRpt.Trazas = enumNegocio.FacturaRecibida.Trazas(Contexto).Where(traza => traza.IdElemento == Factura.Id).ToList();

            informacionRpt.Hitos = enumNegocio.FacturaRecibida.Hitos(Contexto).Where(hito => hito.IdElemento == Factura.Id).ToList();

            informacionRpt.Historial = informacionRpt.Observaciones.Select(o => new HistorialRpt
            {
                CreadaEl = o.CreadaEl,
                Nombre = o.Nombre,
                Creador = Contexto.SeleccionarPorId<UsuarioDtm>(o.IdCreador).Login,
                Descripcion = o.Descripcion,
                Clase = enumClaseHistorialRpt.Observacion
            }).ToList();

            informacionRpt.Historial.AddRange(informacionRpt.Trazas.Select(t => new HistorialRpt
            {
                CreadaEl = t.CreadaEl,
                Nombre = t.Nombre,
                Creador = Contexto.SeleccionarPorId<UsuarioDtm>(t.IdCreador).Login,
                Descripcion = t.Descripcion,
                Clase = enumClaseHistorialRpt.Traza
            }));

            informacionRpt.Historial.AddRange(informacionRpt.Hitos.Select(h => new HistorialRpt
            {
                CreadaEl = h.Fecha,
                Nombre = Contexto.Estados<EstadoDeUnaFacturaRecDtm>().First(e => e.Id == h.IdEstado).Nombre,
                Creador = Contexto.SeleccionarPorId<UsuarioDtm>(h.IdUsuario).Login,
                Descripcion = h.IdTransicion is not null ? $"Transición aplicada: {Contexto.Transiciones<TransicionesDeUnaFacturaRecDtm>().First(t => t.Id == h.IdTransicion).Nombre}" : "",
                Clase = enumClaseHistorialRpt.Hito
            }));

            informacionRpt.Historial = informacionRpt.Historial.OrderBy(h => h.CreadaEl).ToList();

            return informacionRpt;
        }


    }
}
