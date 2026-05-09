using System.Collections.Generic;
using ModeloDeDto;
using MVCSistemaDeElementos.Descriptores;

namespace UtilidadesParaIu
{
    public class FilaDelGrid
    {
        private List<CeldaDelGrid> Celdas = new List<CeldaDelGrid>();

        public string Id => $"{Datos.Id}_d_tr_{NumeroDeFila}";
        public string IdHtml => Id.ToLower();

        public string idHtmlCheckDeSeleccion => $"{IdHtml}.{ltrColumnasDelGrid.chksel}";
        public int NumeroDeCeldas => Celdas.Count;

        public dynamic Datos { get; set; }

        public int NumeroDeFila { get; set; }

        public FilaDelGrid(dynamic datos, ElementoDto elemento)
        {
            Datos = datos;
            var columna = datos.ObtenerColumna(ltrColumnasDelGrid.chksel);
            if (columna != null)
                AnadirCelda(new CeldaDelGrid(columna) { Valor = false });

            columna = datos.ObtenerColumna(nameof(elemento.Id));
            if (columna != null)
                AnadirCelda(new CeldaDelGrid(columna) { Valor = elemento.Id });

        }

        public void AnadirCelda(CeldaDelGrid celda)
        {
            foreach(var c in Celdas)
            {
                if (c.Propiedad == celda.Propiedad)
                    return;
            }

            celda.Fila = this;
            celda.NumeroCelda = Celdas.Count;
            Celdas.Add(celda);
        }

        public CeldaDelGrid ObtenerCelda(int i)
        {
            return Celdas[i];
        }
    }
}