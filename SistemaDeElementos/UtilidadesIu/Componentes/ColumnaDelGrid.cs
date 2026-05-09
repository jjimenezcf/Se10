using System;
using ModeloDeDto;
using Utilidades;

namespace UtilidadesParaIu
{
    public class HtmlDescriptorCabecera
    {
        public string id { get; set; }
        public string idHtml { get; set; }
        public string visible { get; set; }
        public string alineada { get; set; }
        public string valor { get; set; }
    }

    public class ltrColumnasDelGrid
    {
        public const string chksel = nameof(chksel);
    }

    public class ColumnaDelGrid
    {
        public dynamic ZonaDeDatos { get; set; }

        private enumAliniacion _alineada;
        private string _titulo;
        public string Propiedad { get; set; }
        public string Id => $"{ZonaDeDatos.IdHtml}_c_tr_0.{Propiedad}";
        public string IdHtml => Id.ToLower();
        public string Titulo { get { return _titulo == null ? Propiedad : _titulo; } set { _titulo = (value is null) ? "" : value; } }
        public string Ayuda { get; set; }
        public Type Tipo { get; set; } = typeof(string);
        public int PorAnchoMnt { get; set; } = 0;
        public string TamanoFijo { get; set; }
        public int PosicionEnElGrid { get; set; }

        private int _PorAnchoSel;
        public int PorAnchoSel { get { return _PorAnchoSel == 0 ? PorAnchoMnt : _PorAnchoSel; } set { _PorAnchoSel = value; } }
        public bool ConOrdenacion { get; set; } = false;
        public string TipoDeControl { get; set; }
        public bool EsFecha { get; set; }
        public bool EsAccion { get; set; }
        public string Accion { get; set; }
        public enumFormato Formato { get; set; }

        public enumCssOrdenacion cssOrdenacion { get; set; } = enumCssOrdenacion.SinOrden;
        public bool Visible { get; set; } = true;
        public bool Editable { get; set; } = false;
        public enumAliniacion Alineada
        {
            get
            {
                if (_alineada != enumAliniacion.no_definida)
                    return _alineada;

                if (Tipo == typeof(int) || Tipo == typeof(decimal))
                    return enumAliniacion.derecha;
                if (Tipo == typeof(DateTime))
                    return enumAliniacion.centrada;

                return enumAliniacion.izquierda;
            }

            set { _alineada = value; }
        }

        public string AlineacionCss => Alineada.Render();

        public HtmlDescriptorCabecera descriptor { get; set; }
        public string OrdenarPor { get; internal set; }

        public enumCssGrid CssDeLaColumna { get; set; } = enumCssGrid.Nulo;

        public ColumnaDelGrid()
        {
            descriptor = new HtmlDescriptorCabecera();
        }
    }
}