using System;
using System.Collections.Generic;
using System.Reflection;
using Utilidades;
using ModeloDeDto;
using GestoresDeNegocio.Entorno;
using GestorDeElementos;
using ServicioDeDatos;
using Gestor.Errores;

namespace MVCSistemaDeElementos.Descriptores
{

    public class DescriptorDeControlDeLaTabla
    {
        internal PropertyInfo Descriptor { get; set; }

        internal string propiedad => Descriptor.Name.ToLower();

        public DescriptorDeFila Fila { get; set; }
        public short NumeroColumna { get; set; }

        internal string IdHtmlContenedor => $"{Fila.Tabla.IdHtml}-{Fila.NumeroFila}-{NumeroColumna}-crtl-{Atributos.Posicion}";
        internal string IdHtml => $"{Fila.Tabla.IdHtml}-{propiedad}";
        private IUPropiedadAttribute _CacheDeAtributos = null;
        internal IUPropiedadAttribute Atributos
        {
            get
            {
                if (_CacheDeAtributos == null)
                    _CacheDeAtributos = ApiDeAtributos.ObtenerAtributos(Descriptor, Fila.Tabla.PropiedadesJson);
                return _CacheDeAtributos;
            }
        }
    }

    public class DescriptorDeColumna
    {
        private Dictionary<short, DescriptorDeControlDeLaTabla> Controles = new Dictionary<short, DescriptorDeControlDeLaTabla>();

        public DescriptorDeFila Fila { get; private set; }
        public DescriptorDeTabla Tabla => Fila.Tabla;

        public short NumeroColumna { get; private set; }

        public short NumeroDeControles { get; private set; } = 0;
        public short PosicionMaxima { get; private set; } = 0;

        public short NumeroDeEtiquetasVisibles
        {
            get
            {
                short numero = 0;
                for (short i = 0; i <= PosicionMaxima; i++)
                {
                    var control = ObtenerControlEnLaPosicion(i);
                    if (control != null && control.Atributos.EsVisible(Tabla.ModoDeTrabajo) && !control.Atributos.Etiqueta.IsNullOrEmpty())
                        numero = (short)(numero + 1);
                }
                return numero;
            }
        }
        public short NumeroControlesVisibles
        {
            get
            {
                short numero = 0;
                for (short i = 0; i <= PosicionMaxima; i++)
                {
                    var control = ObtenerControlEnLaPosicion(i);

                    if (control != null && control.Atributos.EsVisible(Tabla.ModoDeTrabajo))
                        numero = (short)(numero + 1);
                }
                return numero;
            }
        }

        private int _conColSpan
        {
            get
            {
                for (short i = 0; i < NumeroDeControles; i++)
                {
                    var c = Controles[i];
                    if (c.Atributos.AutoSpan)
                        return AutoSpan;
                    if (c.Atributos.ColSpan > 0)
                        return c.Atributos.ColSpan > AutoSpan ? c.Atributos.ColSpan : AutoSpan;
                }
                return 0;
            }
        }

        private int? _AutoSpan = null;
        private int AutoSpan
        {
            get
            {
                if (_AutoSpan == null)
                {
                    var i = NumeroColumna;
                    var colSpan = 1;
                    while (i < Tabla.NumeroDeColumnas)
                    {
                        if (Fila.ColumnaDefinida((short)(i + 1)))
                        {
                            var siguienteColumna = Fila.ObtenerColumna((short)(i + 1));
                            if (siguienteColumna.NumeroControlesVisibles > 0)
                                return colSpan;
                        }
                        colSpan++;
                        i++;
                    }
                    _AutoSpan = colSpan;
                }
                return (int)_AutoSpan;
            }
        }

        public int ColSpan => _conColSpan;

        public string DisplaCss
        {
            get
            {
                short i = 0;
                while (i < NumeroDeControles)
                {
                    if (Controles.ContainsKey(i))
                    {
                        var c = Controles[i];
                        if (!c.Atributos.DisplaCssDelTd.IsNullOrEmpty())
                            return c.Atributos.DisplaCssDelTd;
                    }
                    i++;
                }
                return "";
            }
        }

        public enumCssDiv CssDelDivDeLaCelda
        {
            get
            {
                short i = 0;
                while (i < NumeroDeControles)
                {
                    if (Controles.ContainsKey(i))
                    {
                        var c = Controles[i];
                        if (c.Atributos.CssDelDivDeLaTd != enumCssDiv.Nulo)
                            return c.Atributos.CssDelDivDeLaTd;
                    }
                    i++;
                }
                return enumCssDiv.Nulo;
            }
        }

        public DescriptorDeColumna(DescriptorDeFila fila, short indice)
        {
            Fila = fila;
            NumeroColumna = indice;
        }

        public void AnadirControl(short pos, PropertyInfo descriptor)
        {

            if (pos > Controles.Count)
                //throw new Exception($"Para {descriptor.Name} se ha definido una posición mayor al nº de controles");
                pos = (short)(Controles.Count);

            if (!Controles.ContainsKey(pos))
            {
                Controles[pos] = new DescriptorDeControlDeLaTabla { Descriptor = descriptor, Fila = Fila, NumeroColumna = NumeroColumna };

                if (PosicionMaxima < pos)
                    PosicionMaxima = pos;

                NumeroDeControles = (short)(NumeroDeControles + 1);
            }
            else
                AnadirControl((short)(pos + 1), descriptor);

        }

        public DescriptorDeControlDeLaTabla ObtenerControlEnLaPosicion(short pos)
        {
            if (Controles.ContainsKey(pos))
                return Controles[pos];

            return null;
        }
        public List<DescriptorDeControlDeLaTabla> ObtenerControles()
        {
            var l = new List<DescriptorDeControlDeLaTabla>();
            for (var i = 0; i < NumeroDeControles; i++)
            {
                l.Add(ObtenerControlEnLaPosicion((short)i));
            }
            return l;
        }

    }

    public class DescriptorDeFila
    {
        private Dictionary<short, DescriptorDeColumna> Columnas = new Dictionary<short, DescriptorDeColumna>();

        public string IdHtml => $"{Tabla.IdHtml}-{NumeroFila}".ToLower();

        public short NumeroFila { get; set; }

        public DescriptorDeTabla Tabla { get; private set; }

        public DescriptorDeFila(DescriptorDeTabla descriptorDeTabla, short indice)
        {
            Tabla = descriptorDeTabla;
            NumeroFila = indice;
        }

        public short NumeroDeColumnas { get; private set; } = 0;

        internal bool ColumnaDefinida(short indice)
        {
            return Columnas.ContainsKey(indice);
        }

        private void DefinirColumna(short indice)
        {
            var celda = new DescriptorDeColumna(this, indice);
            Columnas[indice] = celda;
            if (NumeroDeColumnas <= indice)
            {
                NumeroDeColumnas = (short)(indice + 1);
            }
        }

        internal DescriptorDeColumna ObtenerColumna(short columna)
        {
            if (!ColumnaDefinida(columna))
                DefinirColumna(columna);

            return Columnas[columna];
        }
    }

    public class DescriptorDeTabla : IControlHtml
    {
        private Dictionary<short, DescriptorDeFila> Filas = new Dictionary<short, DescriptorDeFila>();
        public Type Tipo;
        public enumModoDeTrabajo ModoDeTrabajo { get; private set; }
        public short NumeroDeFilas { get; private set; } = 0;

        public short NumeroDeColumnas { get; private set; } = 0;

        public string Controlador { get; private set; }

        public string IdHtml => IdHtmlDeTabla(Tipo.Name, ModoDeTrabajo, PostfijoNombreDeTabla); // $"table-{Tipo.Name}-{ModoDeTrabajo}".ToLower();

        public string IdHtmlContenedor { get; }

        public string Id { get { return IdHtml; } }

        public string Etiqueta { get; }

        public string PostfijoNombreDeTabla { get; set; }

        public IControlHtml Padre { get; }
        public enumNegocio Negocio { get; }
        public bool EsVinculado { get; }
        public List<PropiedaJson> PropiedadesJson { get; }

        public List<string> NoRenderizar { get; }

        public DescriptorDeTabla(IControlHtml padre, Type tipo, enumModoDeTrabajo modoDeTrabajo, string controlador, string idHtmlContenedor, bool esVinculado = false, List<string> noRenderizar = null)
        {
            Tipo = tipo;
            Padre = padre;

            if (padre.GetType() == typeof(DescriptorDeExpansor) && ((DescriptorDeExpansor)padre).PostFijoNombreIdDeLaTabla != null)
                PostfijoNombreDeTabla = ((DescriptorDeExpansor)padre).PostFijoNombreIdDeLaTabla.ToString();

            EsVinculado = esVinculado;
            NoRenderizar = noRenderizar;
            Negocio = NegociosDeSe.NegocioDeUnDto(tipo);
            Controlador = controlador.Replace(ltrEndPoint.Controller, "");
            ModoDeTrabajo = ApiDeInterfaceDto.ImplementaRelacionDto(tipo) && modoDeTrabajo == enumModoDeTrabajo.Nuevo ? enumModoDeTrabajo.NuevaRelacion : modoDeTrabajo;
            IdHtmlContenedor = idHtmlContenedor;
            PropiedadesJson = ApiClasesComunes.ObtenerAtributosJson(tipo, GestorDeVariables.RutaDeJson, ServicioDeCaches.UsaCacheParaRenderizar);
            var propiedades = tipo.GetProperties();
            foreach (var p in propiedades)
                AnadirPropiedad(p);
        }
        private void DefinirFila(short indice)
        {
            var fila = new DescriptorDeFila(this, indice);
            Filas[indice] = fila;

            if (NumeroDeFilas <= indice)
            {
                NumeroDeFilas = (short)(indice + 1);
            }

        }


        private bool FilaDefinida(short indice)
        {
            return Filas.ContainsKey(indice);
        }
        public DescriptorDeFila ObtenerFila(short fila)
        {
            if (!FilaDefinida(fila))
                DefinirFila(fila);

            return Filas[fila];
        }

        private void AnadirPropiedad(PropertyInfo propiedad)
        {
            IUPropiedadAttribute atributos = ApiDeAtributos.ObtenerAtributos(propiedad, PropiedadesJson);
            if (atributos != null)
            {
                var descriptorColumna = ObtenerColumna(atributos.Fila, atributos.Columna);

                if (NumeroDeColumnas <= atributos.Columna)
                    NumeroDeColumnas = (short)(atributos.Columna + 1);

                descriptorColumna.AnadirControl(atributos.Posicion, propiedad);
            }
        }

        public DescriptorDeColumna ObtenerColumna(short fila, short columna)
        {
            var descriptorFila = ObtenerFila(fila);
            return descriptorFila.ObtenerColumna(columna);
        }

        public static string htmlRenderObjetoVacio(IControlHtml padre, Type dto, string controlador, string idHtmlCuerpo, string claseCss, enumModoDeTrabajo modo, List<string> noRenderizar = null)
        {
            var tabla = new DescriptorDeTabla(padre, dto, modo, controlador, idHtmlCuerpo, noRenderizar: noRenderizar);

            var negocio = NegociosDeSe.NegocioDeUnDto(dto);
            var htmlObjeto = @$"<div id='{tabla.IdHtml}' name='table_propiedad'  class='{enumCssDiv.Tabla.Render()} {claseCss}' id-negocio='{(negocio == enumNegocio.No_Definido ? 0 : negocio.IdNegocio())}'>
                                  <div class='{enumCssDiv.Tbody.Render()} {enumCssEdicion.Tbody.Render()}'>
                                     htmlFilas
                                  </div>
                                </div>
                               ";

            var htmlFilas = "";

            for (short i = 0; i < tabla.NumeroDeFilas; i++)
            {
                htmlFilas = htmlFilas + Environment.NewLine + RenderDto.RenderFilaParaElDto(tabla, i);
            }

            return htmlObjeto.Replace("htmlFilas", $"{htmlFilas}");
        }

        public static string IdHtmlDeTabla(string tipo, enumModoDeTrabajo modo, string postFijo)
        {
            return $"table-{tipo}-{modo}{postFijo}".ToLower();
        }

        public string RenderControl()
        {
            throw new Exception("Un descriptor de tabla no es renderizable, lo es su contenedor");
        }
    }

}
