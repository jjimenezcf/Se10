using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Gastos;
using System.Collections.Generic;
using System.Linq;

namespace ModeloDeDto.Gastos
{

    public class PagoRpt : IInformacionRpt<PagoDto>
    {
        public PagoDto Datos { get; set; }
        public SociedadDto Sociedad { get; set; }
        public InterlocutorDto Acreedor { get; set; }
        public CuentaBancariaDtm CtaAcreedora { get; set; }
        public CuentaBancariaDtm CtaDeudora { get; set; }
        public DireccionDto Direccion { get; set; }
        public bool esProveedor { get; set; }
        public bool esTrabajador { get; set; }
        public string Logo { get; set; }        

    }
}
