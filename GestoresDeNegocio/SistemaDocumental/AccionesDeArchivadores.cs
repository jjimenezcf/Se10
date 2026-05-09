using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.SistemaDocumental;

namespace GestoresDeNegocio.SistemaDocumental
{
    public static class AccionesDeArchivadores
    {
        public const string N_DarDeBajaArchivadores = "Dar de baja archivadores vinculados";

        public static void DarDeBajaArchivadores(EntornoDeUnaAccion entorno)
        {
            var proceso = (IElementoDeProcesoDtm)entorno.Registro;

            var archivadores = proceso.Vinculados<ArchivadorDtm>(entorno.Contexto);
            foreach (var archivador in archivadores)
            {
                archivador.Baja = true;
                archivador.Modificar(entorno.Contexto, nameof(DarDeBajaArchivadores));
            }
        }
    }
}
