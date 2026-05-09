using DocumentFormat.OpenXml.Packaging;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeReportes.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.SistemaDocumental
{


    public interface IServicioDeImpresion
    {
        public string Imprimir(enumClaseDePlantilla clase);
    }


    public class ServicioDeImpresion : IServicioDeImpresion
    {
        private ContextoSe Contexto { get; }
        private enumNegocio Negocio { get; }
        private int IdElemento { get; set; }

        private int IdPlantilla { get; set; }

        private ElementoDtm Elemento => Negocio.SeleccionarPorId(Contexto, IdElemento);
        public ServicioDeImpresion(ContextoSe contexto, enumNegocio negocio, int idElemento, int idPlantilla)
        {
            Contexto = contexto;
            Negocio = negocio;
            IdElemento = idElemento;
            IdPlantilla = idPlantilla;
        }

        public string Imprimir(enumClaseDePlantilla clase)
        {
            var plantilla = Negocio.LeerPlantillaPorId(Contexto, clase, IdPlantilla);
            if (plantilla == null)
                return null;

            var archivoPlantilla = Contexto.SeleccionarPorId<ArchivoDtm>(plantilla.IdArchivo.Entero());

            var pltPlantilla = archivoPlantilla.DescargarArchivo(enumRutas.RutaDePlantillas, usarCacheado: true, ponerTickAlNombre: false);
            var nombreFichero = ServidorDocumental.ProponerNombreDeArchivo(Contexto, Negocio, IdElemento, $"{plantilla.Nombre}_{Elemento.Referencia(Contexto)}.{enumExtensiones.docx}".NormalizarFichero());
            var pltResultado = Path.Combine(enumRutas.RutaDeDescarga, nombreFichero);

            var accion = Contexto.SeleccionarPorId<AccionDtm>(plantilla.IdAccion);

            if (accion.ClaseDeAccion != enumClaseDeAccion.PA.ToString())
                GestorDeErrores.Emitir($"No se ha implementado como tratar la acción '{accion.Nombre}' para obtener datos de la plantilla '{plantilla.Nombre}'");

            var datosDelPa = ExtensorDePlantillas.LeerDatos(Contexto, accion.Esquema, accion.Pa, Negocio.IdNegocio(), IdElemento);

            var elementoDto = Negocio.LeerElemento(Contexto, Elemento.Id);
            var datosDelObjeto = new Dictionary<string, Dictionary<string, object>> { { Elemento.GetType().Name.Replace("Dtm", ""), elementoDto.ToDictionary() } };

            var ampliaciones = Negocio.TiposDeAmpliaciones();
            foreach (var ampliacion in ampliaciones)
            {
                var gestor = ampliacion.CrearGestorDeUnaAmpliacion(Contexto);
                var elementos = ( (IEnumerable<object>) gestor.LeerElementos(0, -1, new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(
                    clausula: nameof(IDetalle.IdElemento),
                    criterio: enumCriteriosDeFiltrado.igual,
                    valor: Elemento.Id)}, orden: null,
                    opcionesDeMapeo: new Dictionary<string, object> { { ltrParametrosNeg.AplicarJoin, true } })).Cast<IElementoDto>().ToList();

                if (elementos.Count() == 0) continue;

                datosDelObjeto[ampliacion.Name.Replace("Dtm", "")] = elementos[0].ToDictionary();
            }

            var detalles = new DetallesDelObjeto(Contexto, Elemento);

            File.Copy(pltPlantilla, pltResultado, overwrite: true);
            try
            {
                GenerarDocumentoDocx(pltResultado, datosDelPa, datosDelObjeto, detalles);
                return pltResultado;
            }
            catch
            {
                if (File.Exists(pltResultado)) File.Delete(pltResultado);
                throw;
            }
        }

        private static void GenerarDocumentoDocx(string pltResultado
            , (Dictionary<string, string> formulas, Dictionary<string, Dictionary<string, string>> datos, Dictionary<string, List<Dictionary<string, string>>> filas, Dictionary<string, Dictionary<string, string>> mapeos) datos
            , Dictionary<string, Dictionary<string, object>> datosDelObjeto
            , DetallesDelObjeto detalles)
        {
            using (var fileStream = new FileStream(pltResultado, FileMode.Open, FileAccess.ReadWrite))
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(fileStream, true))
                {
                    var documento = wordDocument.MainDocumentPart;
                    if (documento != null)
                    {
                        var cuerpo = documento?.Document;
                        if (cuerpo != null)
                        {
                            ApiDePlantillas.ProcesarParte(cuerpo, datos.formulas, datos.datos, datosDelObjeto);
                            ApiDePlantillas.ProcesarMapeosDeTablasDelPa(cuerpo, datos.mapeos, datos.filas);
                            ApiDePlantillas.ProcesarMapeosDeDetalles(cuerpo, detalles);
                            ApiDePlantillas.ProcesarMapeosDeExtensiones(cuerpo, detalles.Hitos, enumEncabezadosDeTablas.Hitos);
                            ApiDePlantillas.ProcesarMapeosDeExtensiones(cuerpo, detalles.Observaciones, enumEncabezadosDeTablas.Observaciones);
                            ApiDePlantillas.ProcesarMapeosDeExtensiones(cuerpo, detalles.Direcciones, enumEncabezadosDeTablas.Direcciones);
                        }

                        var pie = documento?.FooterParts.FirstOrDefault();
                        if (pie != null) ApiDePlantillas.ProcesarParte(pie.Footer, datos.formulas, datos.datos, datosDelObjeto);
                    }
                }
            }
        }
    }
}
