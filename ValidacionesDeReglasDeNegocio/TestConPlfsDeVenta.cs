using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using GestoresDeNegocio.Ventas;
using Inicializador.ContratosVnt;
using Inicializador.Ventas;
using ModeloDeDto.MaestrosTecnico;
using ModeloDeDto.Terceros;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using Utilidades;
using ValidacionesBase;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ValidacionesDeRn
{
    class TestConPlfsDeVenta
    {

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DefinirFlujoPlfsDeVenta()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                TestConContratos.CrearModeloDeDatosYMaestros(contexto);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }


        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void ProbarFlujoDePlfsDeVentas()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                CrearContratoYPlanificaciones(contexto);

            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        public static void CrearContratoYPlanificaciones(ContextoSe contexto, int duracionEnMeses = 1)
        {
            //creamos los flujos de contratos, planificaciones y partes de trabajo
            TestConContratos.CrearModeloDeDatosYMaestros(contexto);
            TestConMaestros.CrearUnitariosEjemplos(contexto);
            //Creo una solicitud de contrato para un solicitante
            var contrato1 = TestConContratos.CrearContrato(contexto,
                 contexto.Tipos<TipoDeContratoDtm>().Where(x => x.Nombre.Equals(InzContratosVnt.n_ctv_tipo_venta)).First().Id,
                 contexto.SeleccionarPorPropiedad<ClienteDtm>(nameof(ClienteDto.Expresion), "B73954455"),
                 $"Contrato nº: {DateTime.Now.Ticks}",
                 seIniciaEn: 0,
                 duracionInicial: duracionEnMeses,
                 prorrogarCada: 0,
                 prorrogas: 0,
                 enumClaseDeProrroga.noProrrogable);

            var planificador = CrearUnPlanificador(contexto, contrato1);

            TestConContratos.AsociarSaldos(contexto, contrato1, 2000000);
            contrato1 = contrato1.Recargar(contexto);
            contrato1 = contrato1.Transitar(contexto, nombreTransicion: InzContratosVnt.n_ctv_tran_iniciar);
            planificador = planificador.GenerarPlanificaciones(contexto);
            var planificaciones = contexto.Set<PlanificacionDeVentaDtm>().Where(x => x.IdPlanificador == planificador.Id).ToList();
            foreach (var plan in planificaciones)
            {
                plan.EjecutarEl = DateTime.Now.AddDays(-1);
                if (plan.EjecutarEl < planificador.Inicio) plan.EjecutarEl = planificador.Inicio.AddDays(+1);
                plan.Modificar(contexto);
            }

            TrabajosDePlfsDeVenta.SometerGenerarPlanificacionesDeVenta(contexto).EjecutarTrabajo(contexto);
        }

        private static PlanificadorDeVentaDtm CrearUnPlanificador(ContextoSe contexto, ContratoDtm contrato)
        {
            var planificador = new PlanificadorDeVentaDtm
            {
                Nombre = $"Planificador del contrato {contrato.Referencia}",
                IdContrato = contrato.Id,
                Inicio = DateTime.Now.AddDays(1),
                Hasta = contrato.Ampliacion<DatosDelContratoDtm>(contexto).FinContrato.Fecha(),
                IdCgDeLaPlanificacion = contrato.IdCg,
                IdLote = null,
                IdTipoDePlanificacion = contexto.SeleccionarPorNombre<TipoDePlanificacionDeVentaDtm>(InzPlanificacionesDeVenta.n_plg_tipo_general).Id,
                IdTipoDeParte = contexto.SeleccionarPorNombre<TipoDeParteTrDtm>(InzPartesTr.n_ptr_tipo_general).Id,
                RepetirCada = 1,
                Periodicidad = enumPeriodicidad.Mensual
            }.Insertar(contexto);
            TestConContratos.DefinirLineaDePlanificador(planificador, contexto, contexto.ElementoPorNombre<UnitarioDto, UnitarioDtm>("servicio 1"), 10).Insertar(contexto);
            TestConContratos.DefinirLineaDePlanificador(planificador, contexto, contexto.ElementoPorNombre<UnitarioDto, UnitarioDtm>("servicio 1"), 20).Insertar(contexto);
            TestConContratos.DefinirLineaDePlanificador(planificador, contexto, contexto.ElementoPorNombre<UnitarioDto, UnitarioDtm>("material 1"), 30).Insertar(contexto);
            TestConContratos.DefinirLineaDePlanificador(planificador, contexto, contexto.ElementoPorNombre<UnitarioDto, UnitarioDtm>("material 2"), 40).Insertar(contexto);

            return planificador;

        }
    }
}
