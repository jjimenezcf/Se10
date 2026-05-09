using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServicioDeDatos.Contabilidad;
using Utilidades;

namespace ModeloDeDto.Terceros
{
    public class CuentaDeAcreedorDto
    {
        public string Clase { get; set; }
        public string NumeroIban => $"{Iban}-{Entidad}-{Oficina}-{DcCcc}-{Numero}";
        public int IdCuenta { get; set; }
        public string Iban { get; set; }
        public string Entidad { get; set; }
        public string Oficina { get; set; }
        public string DcCcc { get; set; }
        public string Numero { get; set; }
        public bool Activa { get; set; }
        public string Alias { get; set; }
        public string Banco { get; set; }
        public bool EsDePoveedor { get; set; }
        public int? IdProveedor { get; set; }
        public string Proveedor { get; set; }
        public bool EsDeTrabajador { get; set; }
        public int? IdTrabajador { get; set; }
        public string Trabajador { get; set; }
        public bool EsDeAcreedor { get; set; }
    }
}
