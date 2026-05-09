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
using Azure;
using Gestor.Errores;
using Utilidades;

namespace GestoresDeNegocio.Ventas
{
    public class GeneradorDePagoRpt: IGeneradorRpt<PagoDto>
    {
        private ContextoSe Contexto { get; }
        private PagoDtm Pago { get; }

        public GeneradorDePagoRpt(ContextoSe contexto, PagoDtm pago)
        {
            Contexto = contexto;
            Pago = pago;
        }

        public IInformacionRpt<PagoDto> ObtenerInformacionDeRpt(string plantilla)
        {
            var informacionRpt = new PagoRpt();
            informacionRpt.Datos = Pago.MapearDto<PagoDto>(Contexto);
            
            informacionRpt.Sociedad = Contexto.SeleccionarDto<SociedadDto,SociedadDtm>(
                          id: Pago.Cg(Contexto).IdSociedad, 
                          parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.ObtenerDatosFiscales, true } }));
            
            informacionRpt.Acreedor = Contexto.SeleccionarDto<InterlocutorDto, InterlocutorDtm>(
                          id: Pago.IdSolicitante,
                          parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo));

            informacionRpt.Direccion = Pago.DireccionDePago(Contexto);
            
            if (Pago.Clase != enumClaseDePago.Contado)
            {
                if (Pago.IdCuentaDeAcreedor is null)
                    GestorDeErrores.Emitir($"Para emitir el justificante del pago '{Pago.Referencia}' ha de indicar la cuenta acreedora");

                var ca = Pago.CuentaDeAcreedor(Contexto);
                informacionRpt.Datos.Iban = ca.IsoPais + ca.DcIban;
                informacionRpt.Datos.Entidad = ca.Entidad;
                informacionRpt.Datos.Oficina = ca.Oficina;
                informacionRpt.Datos.DcCcc = ca.DcCcc;
                informacionRpt.Datos.Numero = ca.Numero;
            }

            if (Pago.Clase == enumClaseDePago.Transferencia || Pago.Clase == enumClaseDePago.Remesa)
            {
                if (Pago.IdCuentaDePago is null)
                    GestorDeErrores.Emitir($"Para emitir el justificante del pago '{Pago.Referencia}' ha de indicar la cuenta deudora");
                
                var cp = Pago.CuentaDePago(Contexto).Cuenta(Contexto);
                informacionRpt.Datos.CuentaDePago = cp.NumeroIban;
            }

            informacionRpt.Logo =  informacionRpt.Sociedad.IdArchivo is null
            ? ApiDeArchivos.FicheroNoEncontrado 
            : ServidorDocumental.DescargarArchivo(Contexto, (int)informacionRpt.Sociedad.IdArchivo, solicitadoPorLaCola: false, erroSiNoEstaEnLaruta: false);

            return informacionRpt;
        }


    }
}
