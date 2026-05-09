using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.Ventas;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Ventas;
using Utilidades;
using static ServicioDeDatos.Ventas.VariablesDePlfsDeVenta;

namespace Inicializador.Ventas
{
    public static class InzPartesTr
    {
        public static readonly string n_ptr = "GEN";

        public static void ModeloDePartesTr(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                DefinirEtapas(contexto);
                DefinirVariables(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public static readonly string n_estado_ptr_gen_pendiente = $"{n_ptr}: Pendiente";
        public static readonly string n_estado_ptr_gen_realizado = $"{n_ptr}: Realizado";
        public static readonly string n_estado_ptr_gen_prefacturado = $"{n_ptr}: Prefacturado";
        public static readonly string n_estado_ptr_gen_facturado = $"{n_ptr}: Facturado";
        public static readonly string n_estado_ptr_gen_cancelado = $"{n_ptr}: Cancelado";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados del expediente");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.ParteDeTrabajo, n_estado_ptr_gen_pendiente, inicial: true, terminado: false, cancelado: false, 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.ParteDeTrabajo, n_estado_ptr_gen_realizado, false, false, false, 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.ParteDeTrabajo, n_estado_ptr_gen_prefacturado, false, terminado: true, false, 30);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.ParteDeTrabajo, n_estado_ptr_gen_facturado, false, terminado: true, false, 40);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.ParteDeTrabajo, n_estado_ptr_gen_cancelado, false, false, true, 50);          
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var etapaDePendiente = contexto.SeleccionarEstado<EstadoDeUnParteTrDtm>(n_estado_ptr_gen_pendiente).Id.ToString();
            CacheDeVariable.BorrarVariable(contexto, enumEtapasDePartesTr.PTR_Etapa_Pendiente);
            enumNegocio.ParteDeTrabajo.IncluirEnParametro(contexto, enumEtapasDePartesTr.PTR_Etapa_Pendiente, etapaDePendiente);

            var etapaDeRealizado = contexto.SeleccionarEstado<EstadoDeUnParteTrDtm>(n_estado_ptr_gen_realizado).Id.ToString();
            CacheDeVariable.BorrarVariable(contexto, enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar);
            enumNegocio.ParteDeTrabajo.IncluirEnParametro(contexto, enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar, etapaDeRealizado);

            var etapaDeFacturacion = contexto.SeleccionarEstado<EstadoDeUnParteTrDtm>(n_estado_ptr_gen_prefacturado).Id.ToString() + ","
                                   + contexto.SeleccionarEstado<EstadoDeUnParteTrDtm>(n_estado_ptr_gen_facturado).Id.ToString();
            CacheDeVariable.BorrarVariable(contexto, enumEtapasDePartesTr.PTR_Etapa_Facturado);
            enumNegocio.ParteDeTrabajo.IncluirEnParametro(contexto, enumEtapasDePartesTr.PTR_Etapa_Facturado, etapaDeFacturacion);

            var etapaDeCancelado = contexto.SeleccionarEstado<EstadoDeUnParteTrDtm>(n_estado_ptr_gen_cancelado).Id.ToString();
            CacheDeVariable.BorrarVariable(contexto, enumEtapasDePartesTr.PTR_Etapa_Cancelado);
            enumNegocio.ParteDeTrabajo.IncluirEnParametro(contexto, enumEtapasDePartesTr.PTR_Etapa_Cancelado, etapaDeCancelado);
        }

        public static readonly string n_tran_ptr_gen_realizar = $"{n_ptr}: Realizar";
        public static readonly string n_tran_ptr_gen_perfacturar = $"{n_ptr}: Prefacturar";
        public static readonly string n_tran_ptr_gen_incluirlo_en_prefactura  = $"{n_ptr}: Incluirlo en prefactura";
        public static readonly string n_tran_ptr_gen_excluirlo_en_prefactura = $"{n_ptr}: Excluirlo en prefactura";
        public static readonly string n_tran_ptr_gen_al_facturar = $"{n_ptr}: Al facturar";
        public static readonly string n_tran_ptr_gen_al_anular_preftr = $"{n_ptr}: Al anular prefactura";
        public static readonly string n_tran_ptr_gen_devolver = $"{n_ptr}: Devolver";
        public static readonly string n_tran_ptr_gen_cancelar = $"{n_ptr}: Cancelar";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de expedientes");
            try
            {
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.ParteDeTrabajo, n_tran_ptr_gen_realizar, n_estado_ptr_gen_pendiente, n_estado_ptr_gen_realizado);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.ParteDeTrabajo, n_tran_ptr_gen_perfacturar, n_estado_ptr_gen_realizado, n_estado_ptr_gen_prefacturado);

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.ParteDeTrabajo, n_tran_ptr_gen_incluirlo_en_prefactura, n_estado_ptr_gen_realizado, n_estado_ptr_gen_prefacturado, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.ParteDeTrabajo, n_tran_ptr_gen_excluirlo_en_prefactura, n_estado_ptr_gen_prefacturado, n_estado_ptr_gen_realizado, delSistema: true);

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.ParteDeTrabajo, n_tran_ptr_gen_al_facturar, n_estado_ptr_gen_prefacturado, n_estado_ptr_gen_facturado, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.ParteDeTrabajo, n_tran_ptr_gen_al_anular_preftr, n_estado_ptr_gen_prefacturado, n_estado_ptr_gen_realizado, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.ParteDeTrabajo, n_tran_ptr_gen_devolver, n_estado_ptr_gen_realizado, n_estado_ptr_gen_pendiente);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.ParteDeTrabajo, n_tran_ptr_gen_cancelar, n_estado_ptr_gen_pendiente, n_estado_ptr_gen_cancelado, asunto: "Motivo de cancelación");
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }


        private static void DefinirVariables(ContextoSe contexto)
        {
            var alExcluirParte = new TransicionPorMotivo
            {
                Motivo = VariableDePartesTr.enumMotivoTransicion.EliminarParteDeUnaLineaDeFactura.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnParteTrDtm>(n_estado_ptr_gen_prefacturado).Id,
                IdTransicion = contexto.SeleccionarTransicion<TransicionesDeUnParteTrDtm>(nameof(TransicionDtm.Nombre),n_tran_ptr_gen_excluirlo_en_prefactura).Id
            };
            var alAnularFactura = new TransicionPorMotivo
            {
                Motivo = VariableDePartesTr.enumMotivoTransicion.AnularPrefactura.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnParteTrDtm>(n_estado_ptr_gen_prefacturado).Id,
                IdTransicion = contexto.SeleccionarTransicion<TransicionesDeUnParteTrDtm>(nameof(TransicionDtm.Nombre), n_tran_ptr_gen_al_anular_preftr).Id
            };
            var alFacturarElParteDeUnContrato = new TransicionPorMotivo
            {
                Motivo = VariableDePartesTr.enumMotivoTransicion.FacturarParteDeUnContrato.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnParteTrDtm>(n_estado_ptr_gen_realizado).Id,
                IdTransicion = contexto.SeleccionarTransicion<TransicionesDeUnParteTrDtm>(nameof(TransicionDtm.Nombre), n_tran_ptr_gen_perfacturar).Id
            };
            //enumVariablesDePartes.PTR_Aplicar_Transicion.Persistir(contexto, JsonConvert.SerializeObject(new List<TransicionPorMotivo> { alExcluirParte, alAnularFactura, alFacturarElParteDeUnContrato }));
            var transicionesPorMotivos = JsonConvert.SerializeObject(new List<TransicionPorMotivo> { alExcluirParte, alAnularFactura, alFacturarElParteDeUnContrato });
            CacheDeVariable.BorrarVariable(contexto, enumParametrosDePartes.PTR_Aplicar_Transicion);
            enumNegocio.ParteDeTrabajo.IncluirEnParametro(contexto, enumParametrosDePartes.PTR_Aplicar_Transicion, transicionesPorMotivos);
        }

        public static readonly string n_ptr_tipo_general = $"{n_ptr}: Parte";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de partes de trabajo");
            try
            {
                var ptr_estadoInicial = enumNegocio.ParteDeTrabajo.Estado(contexto, n_estado_ptr_gen_pendiente);
                GestorDeTiposDeParteTr.PersistirTipo(contexto, n_ptr_tipo_general, ptr_estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_ptr);            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
