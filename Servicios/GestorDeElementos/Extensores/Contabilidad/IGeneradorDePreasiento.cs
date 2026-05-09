using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Terceros;
using System;
using System.ComponentModel;
using System.IO;
using Utilidades;

namespace GestorDeElementos.Extensores.Contabilidad
{
    public abstract class GeneradorDePreasiento
    {
        public SociedadDtm Sociedad { get; set; }
        public DateTime? FechaContable { get; set; }
        public int Ejercicio { get; set; }
        public ContextoSe Contexto { get; set; }

        public bool RespetarFechacontable { get; set; }

    }

    public abstract class GeneradorBaseNcs : GeneradorDePreasiento
    {

        public enum enumNcsTipoOperacion
        {
            [Description("Operaciones Interiores / Régimen General")]
            IV01,
            [Description("Adquisiciones intracomunitarias")]
            IV02,
            [Description("Importaciones")]
            IV03,
            [Description("Entregas intracomunitarias")]
            IV04,
            [Description("Exportaciones y operaciones Asimiladas")]
            IV05,
            [Description("Operaciones exentas sin derecho a deducción")]
            IV06,
            [Description("Operac. Sujetas con derecho a devolución")]
            IV07,
            [Description("Operac. No sujetas con derecho a deducción")]
            IV08,
            [Description("Regularización Inversiones")]
            IV09,
            [Description("Operaciones interiores bienes de inversión")]
            IV10,
            [Description("Importaciones bienes de inversión")]
            IV11,
            [Description("Adquisiciones Intracomunitarias bienes de inversión")]
            IV12,
            [Description("Arrendamientos locales de negocio")]
            IV13,
            [Description("Operaciones de seguros")]
            IV14,
            [Description("Compras públicas")]
            IV15,
            [Description("Subvenciones Empresariales no agrícolas")]
            IV16,
            [Description("Operaciones Sujetas por inversión del sujeto pasivo")]
            IV17,
            [Description("Imp. Percibido por transm. inmbil. Sujeta a IVA")]
            IV18,
            [Description("Prestación de Servicios intracomunitarios")]
            IV19,
            [Description("Adquisición de Servicios intracomunitarios")]
            IV20,
            [Description("Entr. intrac. bienes post. a import. exenta")]
            IV21,
            [Description("Entr. intrac. bienes post. import. ex. rep. fiscal")]
            IV22,
            [Description("Operaciones interiores bienes de inversión leasing")]
            IV23,
            [Description("Rectificación en concurso de acreedores")]
            IV24,
            [Description("Ingresos sujetos por inversión del sujeto pasivo")]
            IV31
        }

        public enum enumNcsPorcentageIva
        {
            [Description("Cuando se seleccione Ex en IVA, y no tenga recargo")]
            [Valor("EX")]
            Exento,
            [Description("Cuando se seleccione Ns en IVA y no tenga recargo")]
            [Valor("NS")]
            NoSujeto,
            [Description("Cuando se seleccione 4% de IVA y no tenga recargo")]
            [Valor("4")]
            CuatroPorciento,
            [Description("Cuando se seleccione 7% de IVA y no tenga recargo")]
            [Valor("7")]
            SietePorciento,
            [Description("Cuando se seleccione 8% IVA y no tenga recargo")]
            [Valor("8")]
            OchoPorciento,
            [Description("Cuando se seleccione 10% IVA y no tenga recargo")]
            [Valor("10")]
            DiezPorciento,
            [Description("Cuando se seleccione 16% IVA y no tenga recargo")]
            [Valor("16")]
            DieciseisPorciento,
            [Description("Cuando se seleccione 18% IVA y no tenga recargo")]
            [Valor("18")]
            DieciochoPorciento,
            [Description("Cuando se seleccione 21% IVA y no tenga recargo")]
            [Valor("21")]
            VeintiUnoPorciento,
            [Description("Cuando se seleccione 4% de IVA y 0,5% recargo")]
            [Valor("4R")]
            CuatroConRecargo,
            [Description("Cuando se seleccione 7% de IVA y 1% recargo")]
            [Valor("7R")]
            SieteConRecargo,
            [Description("Cuando se seleccione 8% de IVA y 1% recargo")]
            [Valor("8R")]
            OchoConRecargo,
            [Description("Cuando se seleccione 10% de IVA y 1,4% recargo")]
            [Valor("10R")]
            DiezConRecargo,
            [Description("Cuando se seleccione 16% de IVA y 4% recargo")]
            [Valor("16R")]
            DieciseisConRecargo,
            [Description("Cuando se seleccione 18% de IVA y 4% recargo")]
            [Valor("18R")]
            DieciochoConRecargo,
            [Description("Cuando se seleccione 21% de IVA y 5,2% recargo")]
            [Valor("21R")]
            VeintiUnoConRecargo,
            [Description("Cuando se selecciones el 6% de IVA")]
            [Valor("AG6")]
            AgrarioSeisPorciento,
            [Description("Cuando se seleccione 7,5% de recargo agrario")]
            [Valor("R7,5")]
            RecargoAgrarioSieteYMedio,
            [Description("Cuando se seleccione 9% de recargo agrario")]
            [Valor("R9")]
            RecargoAgrarioNueve,
            [Description("Cuando se seleccione 10% de recargo agrario")]
            [Valor("R10")]
            RecargoAgrarioDiez,
            [Description("Cuando se seleccione 8,5% de recargo agrario")]
            [Valor("R8,5")]
            RecargoAgrarioOchoYMedio,
            [Description("Cuando se seleccione 10,5% de recargo agrario")]
            [Valor("R10,5")]
            RecargoAgrarioDiezYMedio,
            [Description("Cuando se seleccione 12% de recargo agrario")]
            [Valor("R12")]
            RecargoAgrarioDoce
        }


        protected const string _xsi = "http://www.w3.org/2001/XMLSchema-instance";
        protected const string _xsd = "http://www.w3.org/2001/XMLSchema";

        public string DiarioXml { get; protected set; }
        public string TercerosXml { get; protected set; }

        public GeneradorBaseNcs(ContextoSe contexto, SociedadDtm sociedad, int ejercicio)
        {
            Contexto = contexto;
            Sociedad = sociedad;
            Ejercicio = ejercicio;
        }

        public void Deshacer()
        {
            if (File.Exists(DiarioXml)) File.Delete(DiarioXml);
            if (File.Exists(TercerosXml)) File.Delete(TercerosXml);
        }

        protected static enumNcsPorcentageIva PorcentajeDeIvaRepNcs(IvaRepercutidoDtm ivarep, enumNcsTipoOperacion tipoOperacion)
        {
            return ivarep.Clase == enumClasesDeIvaRep.NSJ || tipoOperacion == enumNcsTipoOperacion.IV31
                    ? enumNcsPorcentageIva.NoSujeto
                    : ivarep.Exento
                    ? enumNcsPorcentageIva.Exento
                    : EncontrarEnumeradoPorSegunPorcentaje(ivarep.Porcentaje, tipoOperacion);
        }

        protected static enumNcsPorcentageIva PorcentajeDeIvaSopNcs(IvaSoportadoDtm ivasop, enumNcsTipoOperacion tipoOperacion)
        {

            if (tipoOperacion == enumNcsTipoOperacion.IV17)
                return enumNcsPorcentageIva.VeintiUnoPorciento;

            return ivasop.Clase == enumClasesDeIvaSop.NSJ 
                    ? enumNcsPorcentageIva.NoSujeto
                    : ivasop.Exento
                    ? enumNcsPorcentageIva.Exento
                    : EncontrarEnumeradoPorSegunPorcentaje(ivasop.Porcentaje, tipoOperacion);
        }

        private static enumNcsPorcentageIva EncontrarEnumeradoPorSegunPorcentaje(decimal porcentaje, enumNcsTipoOperacion tipoOpercion)
        {
            if (tipoOpercion == enumNcsTipoOperacion.IV02 || tipoOpercion == enumNcsTipoOperacion.IV17)
                return enumNcsPorcentageIva.VeintiUnoPorciento;

            if (porcentaje == 0)
                return enumNcsPorcentageIva.Exento;

            foreach (enumNcsPorcentageIva enumValue in Enum.GetValues(typeof(enumNcsPorcentageIva)))
            {
                var valor = enumValue.Valor();

                if (valor.Contains("R"))
                    continue;

                if (valor.Entero() == 0)
                    continue;

                if (valor.Decimal() == porcentaje)
                {
                    return enumValue;
                }
            }

            throw new ArgumentException($"El enumerado para el Iva de Ncs con valor '{porcentaje}' no está dado de alta.");
        }

    }
}
