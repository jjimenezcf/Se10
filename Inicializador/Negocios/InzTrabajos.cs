using GestorDeElementos.Extensores;
using GestoresDeNegocio.Administracion;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Gastos;
using GestoresDeNegocio.Guarderias;
using GestoresDeNegocio.Juridico;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Ventas;
using ServicioDeDatos;

namespace SistemaDeElementos.Inicializador
{
    public static class InzTrabajos
    {
        public static void SometerTrabajos(ContextoSe contexto)
        {
            TrabajosDeEntorno.SometerEliminarCorreos(contexto);
            SometerTrabajosDeJuridico(contexto);
            SometerTrabajosDeGastos(contexto);
            SometerTrabajosDeVentas(contexto);
            if (ExtensorDeGuarderias.ModuloActivo(contexto)) SometerTrabajosDeGuarderia(contexto);
            SometerTrabajosDeExpedientes(contexto);
            SometerTrabajosDeFichadas(contexto);
        }

        private static void SometerTrabajosDeJuridico(ContextoSe contexto)
        {
            TrabajosDeContratos.SometerNotificarPorcentajeDeAvisoSobrepasado(contexto);
            TrabajosDeContratos.SometerMotorDeContratos(contexto);
        }

        private static void SometerTrabajosDeGastos(ContextoSe contexto)
        {
            TrabajosDePagos.SometerProcesosDePagos(contexto);
            TrabajosDeRemesasPag.SometerProcesosDeRemesasPag(contexto);
            TrabajosDeFacturasRec.SometerActualizacionDeFacturas(contexto);
        }
        private static void SometerTrabajosDeVentas(ContextoSe contexto)
        {
            TrabajosDePlfsDeVenta.SometerGenerarPlanificacionesDeVenta(contexto);
            TrabajosDeFacturasEmt.SometerVencerFacturasImpagadas(contexto);
            TrabajosDeRemesasFae.SometerProcesosDeRemesasFae(contexto);
            TrabajosDeFacturasEmt.SometerActualizacionDeFacturas(contexto);
        }

        private static void SometerTrabajosDeGuarderia(ContextoSe contexto)
        {
            TrabajosDeGuarderia.SometerAnularPermisosDeAgendas(contexto);
        }

        private static void SometerTrabajosDeExpedientes(ContextoSe contexto)
        {
            TrabajosDeExpedientes.SometerComunicarSubidaDeArchivosAExpedientes(contexto);
        }

        private static void SometerTrabajosDeFichadas(ContextoSe contexto)
        {
            TrabajosDeCircuitosDocumentales.SometerFicharEntrada(contexto);
            TrabajosDeCircuitosDocumentales.TrabajoDeCrearHistoricoDeFichadas(contexto);
            TrabajosDeCircuitosDocumentales.SometerCerrarFichadasAbiertas(contexto);
            TrabajosDeCircuitosDocumentales.SometerCerrarFichadasAntiguas(contexto);
        }


    }
}
