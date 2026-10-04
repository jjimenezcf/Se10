using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.MaestrosTecnico;
using ServicioDeDatos;
using ServicioDeDatos.Negocio;
using Utilidades;

namespace SistemaDeElementos.Inicializador
{
    public static class InzMaestrosTecnicos
    {
        public static void AccionesMt(ContextoSe contexto)
        {
            contexto.DatosDeConexion.CreandoModelo = true;
            contexto.IniciarTraza(nameof(AccionesMt));
            var tran = contexto.IniciarTransaccion();
            try
            {
                ExportarMtParaProveedor(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
            finally
            {
                contexto.CerrarTraza();
                contexto.DatosDeConexion.CreandoModelo = false;
            }
        }

        /// <summary>
        /// Plantilla de exportación de los unitarios seleccionados para que un proveedor indique su referencia y su tarifa
        /// </summary>
        private static void ExportarMtParaProveedor(ContextoSe contexto)
        {
            var a = new PlantillaDeExportacionDtm();
            a.IdNegocio = enumNegocio.Unitario.IdNegocio();
            a.Nombre = ExportacionesDeUnitarios.N_ParaProveedor;
            a.Dll = $"{nameof(GestoresDeNegocio)}";
            a.Clase = $"{nameof(GestoresDeNegocio)}.{nameof(GestoresDeNegocio.MaestrosTecnico)}.{nameof(ExportacionesDeUnitarios)}";
            a.Metodo = nameof(ExportacionesDeUnitarios.ParaProveedor);
            a.InsertarSiNoExiste(contexto, new List<string> { nameof(PlantillaDeExportacionDtm.Dll), nameof(PlantillaDeExportacionDtm.Clase), nameof(PlantillaDeExportacionDtm.Metodo) });
        }
    }
}
