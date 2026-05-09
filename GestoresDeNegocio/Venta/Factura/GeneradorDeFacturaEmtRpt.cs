using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.SistemaDocumental;
using ModeloDeDto;
using ModeloDeDto.Reporte;
using ModeloDeDto.Terceros;
using ModeloDeDto.Ventas;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using Utilidades;

namespace GestoresDeNegocio.Ventas
{
    public class GeneradorDeFacturaEmtRpt : IGeneradorRpt<FacturaEmtDto>
    {
        private ContextoSe Contexto { get; }
        private FacturaEmtDtm Factura { get; }


        public GeneradorDeFacturaEmtRpt(ContextoSe contexto, FacturaEmtDtm factura)
        {
            Contexto = contexto;
            Factura = factura;
        }

        public IInformacionRpt<FacturaEmtDto> ObtenerInformacionDeRpt(string plantilla)
        {
            var informacionRpt = new FacturaEmtRpt();
            informacionRpt.Datos = Factura.MapearDto<FacturaEmtDto>(Contexto);
            informacionRpt.Lineas = new List<LineaDeUnaFaeDto>();
            informacionRpt.FacturaDtm = Factura;
            var lineas = Factura.Detalles<LineaDeUnaFaeDtm>(Contexto, aplicarJoin: true);
            foreach (var linea in lineas)
            {
                informacionRpt.Lineas.Add(linea.MapearDto<LineaDeUnaFaeDto>(Contexto));
                informacionRpt.IncluirIva(linea);
                informacionRpt.IncluirExento(linea);
            }

            if (Factura.TienenIrpf(Contexto, aplicarJoin: true))
                informacionRpt.IncluirIrpf(Factura.IrpfEmt);

            var cg = Factura.Cg(Contexto, aplicarJoin: true);
            informacionRpt.Sociedad = Contexto.SeleccionarDto<SociedadDto, SociedadDtm>(
                          id: cg.IdSociedad,
                          parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.ObtenerDatosFiscales, true } }));

            informacionRpt.Cliente = Contexto.SeleccionarDto<ClienteDto, ClienteDtm>(
                          id: Factura.IdCliente,
                          parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.ObtenerDatosFiscales, true } }));

            informacionRpt.Direccion = Factura.DireccionFiscal(Contexto);

            informacionRpt.Logo = informacionRpt.Sociedad.IdArchivo is null
            ? ApiDeArchivos.FicheroNoEncontrado
            : ServidorDocumental.DescargarArchivo(Contexto, (int)informacionRpt.Sociedad.IdArchivo, solicitadoPorLaCola: false, erroSiNoEstaEnLaruta: false);

            var datosDeFactura = (ParametrosDeMiSociedadDtm)Factura.Sociedad(Contexto).SeleccionarAmpliacion(Contexto, typeof(ParametrosDeMiSociedadDtm));

            informacionRpt.InformacionFiscal = Factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.Estados())
            ? Factura.InformacionFiscal.IsNullOrEmpty() 
            ? Factura.MotivosDeExeccion(lineas) 
            : Factura.InformacionFiscal
            : Factura.InformacionFiscal;

            informacionRpt.PieDeFactura = Factura.Descripcion.IsNullOrEmpty() 
            ? datosDeFactura.PieDeFactura
            : Factura.Descripcion + Environment.NewLine + datosDeFactura.PieDeFactura;

            if (Factura.EsRectificativa)
            {
                informacionRpt.PieDeFactura = $"Rectificada: '{Factura.RectificaA(Contexto).NumeroDeFactura}' " +
                    $"calse: '{Factura.ClaseRectificativa.Descripcion()} " +
                    $"y motivo: '{Factura.MotivoDeRectificacion.Descripcion()}'" +
                    $"{Environment.NewLine}{informacionRpt.PieDeFactura}";
            }

            informacionRpt.InscritoEn = datosDeFactura.InscritoEn;
            informacionRpt.BiConIva = Factura.BiConIva(Contexto);
            informacionRpt.UrlAeat = Factura.GenerarURLParaVerifactu(Contexto);
            informacionRpt.UrlSe = Factura.GenerarURLParaSe(Contexto);
            informacionRpt.LeyendaAeat = Factura.LeyendaVerifactu(Contexto);
            informacionRpt.LeyendaSe = Factura.LeyendaSe(Contexto);
            var datos = enumNegocio.FacturaEmitida.LeerCrearParametro(Contexto, enumParametrosDeFacturasEmt.FAE_DatosDeImpresion, valor: ltrParametrosRpt.ParametrosPorDefecto());
            informacionRpt.Parametros = JsonConvert.DeserializeObject<Dictionary<string, object>>(datos.Valor);
            if (!informacionRpt.VerificarVersionDeParametros())
            {
                informacionRpt.ActualizarVersionDeParametros(enumNegocio.FacturaEmitida.IdNegocio(), enumParametrosDeFacturasEmt.FAE_DatosDeImpresion);
            }
            return informacionRpt;
        }


    }
}
