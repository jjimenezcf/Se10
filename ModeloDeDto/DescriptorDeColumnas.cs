using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace ModeloDeDto
{
    public enum enumMostrarComo { Numero, Texto, Invisible }

    public class PropiedadDto
    {
        public string Titulo { get; set; }
        public string Propiedad { get; set; }
        public enumAliniacion Aliniacion { get; set; }

        public int Tamano { get; set; }
        public bool Mostrar { get; set; }
        public enumFormato? Formato { get; set; }
        public bool EsFecha { get; set; }
        public bool AutoAjustable { get; set; }
    }

    public class DescriptorDeColumnas
    {
        private List<PropiedadDto> _propiedades = new List<PropiedadDto>();

        public int Cantidad => _propiedades.Count();

        public PropiedadDto this[int indice] => _propiedades[indice];

        public string Id { get; }


        public DescriptorDeColumnas(string id)
        {
            Id = id;
        }

        public int Add(string titulo, string propiedad = null, enumAliniacion alineacion = enumAliniacion.izquierda, bool mostrar = true, int tamano = 0, enumFormato? formato = null, bool autoAjustable = false)
        {
            var esFecha = formato != null && (formato == enumFormato.Fecha || formato == enumFormato.FechaTiempo || formato == enumFormato.FechaHoraMinutos);
            if (formato != null) alineacion = enumAliniacion.derecha;
            if (propiedad.IsNullOrEmpty()) propiedad = titulo;

            var esnumero = formato != null && (formato == enumFormato.Moneda || formato == enumFormato.Porcentaje || formato == enumFormato.Numero_2 || formato == enumFormato.Numero_6);
            if (esnumero && tamano == 0 ) tamano = 150;


            var p = new PropiedadDto 
            { 
                Titulo = titulo, 
                Aliniacion = alineacion, 
                Propiedad = propiedad, 
                Mostrar = mostrar, 
                Tamano = tamano,
                EsFecha = esFecha,
                Formato = formato,
                AutoAjustable = autoAjustable
            };
            _propiedades.Add(p);
            return _propiedades.Count;
        }


    }
}
