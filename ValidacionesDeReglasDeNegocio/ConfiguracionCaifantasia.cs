
using ServicioDeDatos;
using NUnit.Framework;
using ValidacionesBase;
using Inicializador.ContratosVnt;
using Inicializador.Procesos;
using SistemaDeElementos.Inicializador;
using Inicializador.Presupuestos;
using Inicializador.Ventas;
using Inicializador.Gastos;
using GestoresDeNegocio.Juridico;
using GestorDeElementos.Extensores;
using Inicializador.Expedientes;
using Microsoft.IdentityModel.Tokens;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using Utilidades;

namespace ValidacionesDeRn
{
    class ConfiguracionCaifantasia
    {
        [Test]
        public void DefinirProcesos()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzAcciones.DefinirAcciones(contexto);
                InzSolicitudDeContrato.ModeloDeSolicitudesDeContrato(contexto);
                InzContratosVnt.ModeloDeContratosVnt(contexto);
                ExtensorDeContratos.InicializarEtapas(contexto);
                InzPresupuestos.ModeloDePresupuestos(contexto);
                InzPartesTr.ModeloDePartesTr(contexto);
                InzFacturasEmt.ModeloDeFacturasEmt(contexto);
                InzPlanificacionesDeVenta.ModeloDePlanificacionesDeVenta(contexto);
                InzRemesasFae.ModeloDeRemesasFae(contexto);
                InzPagos.ModeloDePagos(contexto);
                InzRemesasPag.ModeloDeRemesasPag(contexto);
                InzFacturasRec.ModeloDeFacturasRec(contexto);
                TrabajosDeContratos.SometerNotificarPorcentajeDeAvisoSobrepasado(contexto);
                TrabajosDeContratos.SometerMotorDeContratos(contexto);
            }
            //ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }


        [Test]
        public void DefinirProcesosAulas()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                ExtensorDeExpedientes.DefinirEtapasBasicas(contexto);
                var estados = enumNegocio.Expediente.Estados(contexto);
                var iniciales = "";
                var enCurso = "";
                foreach (EstadoDtm estado in estados)
                    if (estado.Inicial) iniciales = $"{(iniciales.IsNullOrEmpty() ? estado.Id.ToString() : $"{iniciales},{estado.Id}")}";

                foreach (EstadoDtm estado in estados)
                    if (!estado.Inicial && !estado.Terminado && !estado.Cancelado)
                        enCurso = $"{(enCurso.IsNullOrEmpty() ? estado.Id.ToString() : $"{enCurso},{estado.Id}")}";


                enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas, iniciales + "," + enCurso);
                enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos, ltrEstados.EstadoNulo);
                enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Ejecucion, ltrEstados.EstadoNulo);
                enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Compra, ltrEstados.EstadoNulo);
                enumNegocio.Expediente.IncluirEnParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Venta, ltrEstados.EstadoNulo);

            }
            //ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

    }
}

