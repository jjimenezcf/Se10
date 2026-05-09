using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.SistemaDocumental;
using ModeloDeDto;
using ModeloDeDto.Terceros;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System.Collections.Generic;

namespace GestoresDeNegocio.Ventas
{
    public class GeneradorDeParteTrRpt: IGeneradorRpt<ParteTrDto>
    {
        private ContextoSe Contexto { get; }
        private ParteTrDtm Parte { get; }

        public GeneradorDeParteTrRpt(ContextoSe contexto, ParteTrDtm parte)
        {
            Contexto = contexto;
            Parte = parte;
        }

        public IInformacionRpt<ParteTrDto> ObtenerInformacionDeRpt(string plantilla)
        {
            var parteTrRpt = new ParteTrRpt();
            parteTrRpt.Datos = Parte.MapearDto<ParteTrDto>(Contexto);
            parteTrRpt.Lineas = new List<LineaDeUnPtrDto>();
            var lineas = Parte.Detalles<LineaDeUnPtrDtm>(Contexto, aplicarJoin: true);
            foreach (var linea in lineas)
            {
                parteTrRpt.Lineas.Add(linea.MapearDto<LineaDeUnPtrDto>(Contexto));
            }            
            
            parteTrRpt.Sociedad = Contexto.SeleccionarDto<SociedadDto,SociedadDtm>(
                          id: Parte.Cg(Contexto).IdSociedad, 
                          parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.ObtenerDatosFiscales, true } }));
            
            parteTrRpt.Cliente = Contexto.SeleccionarDto<ClienteDto, ClienteDtm>(
                          id: Parte.IdCliente,
                          parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.ObtenerDatosFiscales, true } }));

            parteTrRpt.Direccion = Parte.DireccionDeEjecucion(Contexto);

            parteTrRpt.Logo =  parteTrRpt.Sociedad.IdArchivo is null
            ? ApiDeArchivos.FicheroNoEncontrado 
            : ServidorDocumental.DescargarArchivo(Contexto, (int)parteTrRpt.Sociedad.IdArchivo, solicitadoPorLaCola: false, erroSiNoEstaEnLaruta: false);

            return parteTrRpt;
        }


    }
}
