using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Utilidades
{
    /// <summary>
    /// Para procesar ficheros CSV fácilmente.
    /// Permite hacer un foreach sobre las líneas del fichero.
    /// </summary>
    public class FicheroCsv : IEnumerable<CsvFila>
    {
        private readonly StreamReader _stream;

        public FicheroCsv(string ruta)
        {
            _stream = new StreamReader(ruta, Encoding.Default);
        }

        public FicheroCsv(Stream stream)
        {
            _stream = new StreamReader(stream, Encoding.Default);
        }

        public IEnumerator<CsvFila> GetEnumerator()
        {
            return new EnumeradorDelCsv(_stream);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// El enumerador
    /// </summary>
    public class EnumeradorDelCsv : IEnumerator<CsvFila>
    {
        private readonly StreamReader _fichero;
        public CsvFila Current { get; private set; }
        object IEnumerator.Current
        {
            get { return Current; }
        }

        internal EnumeradorDelCsv(StreamReader ficheroCsv)
        {
            _fichero = ficheroCsv;
        }

        public void Dispose()
        {
            _fichero.Dispose();
        }

        public bool MoveNext()
        {
            var lineaCsv = _fichero.ReadLine();
            if (lineaCsv == null)
            {
                return false;
            }
            else
            {
                Current = new CsvFila(lineaCsv);
                return true;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

    }

    /// <summary>
    /// Representa un fila de un CSV como una colección de campos de texto
    /// </summary>
    public class CsvFila
    {
        private string[] _campos;

        internal CsvFila(string[] campos)
        {
            _campos = campos;
        }

        internal CsvFila(string fila) : this(fila.Split(';'))
        {
        }

        /// <summary>
        /// Permite acceder a un campo usando como índice la columna Excel (p.e. A, B, AB)
        /// </summary>
        /// <param name="columna">Columna</param>
        /// <returns>Contenido de la columna</returns>
        public string this[string columna]
        {
            get { return this[IndiceDeColumna(columna)]; }
        }

        /// <summary>
        /// Accede a una columna.
        /// </summary>
        /// <param name="indice"></param>
        /// <returns></returns>
        public string this[int indice]
        {
            get { return _campos.Length > indice ? _campos[indice] : null; }
        }

        /// <summary>
        /// True si todos los campos de la columna están en blanco
        /// </summary>
        public bool EnBlanco
        {
            get { return _campos.All(c => c == ""); }
        }

        public int Columnas
        {
            get { return _campos.Length; }
        }

        /// <summary>
        /// Calcula el índice de una columna en base a la nomenclatura Excel
        /// </summary>
        /// <param name="col">Columna en nomenclatura Excel (p.e. A, B, AB)</param>
        /// <returns>Índice de la columna</returns>
        private static int IndiceDeColumna(string col)
        {
            return col.Select((c, i) => ((c - 'A' + 1) * ((int)Math.Pow(26, col.Length - i - 1)))).Sum() - 1;
        }
    }
}
