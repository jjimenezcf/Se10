using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Juridico;
using ModeloDeDto.Juridico;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using static Gestor.Errores.GestorDeErrores;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GestoresDeNegocio.Juridico
{
    public class GestorDeDatosDelContrato : GestorDeElementos<ContextoSe, DatosDelContratoDtm, DatosDelContratoDto>
    {

        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class MapearDatosJuridicos : Profile
        {
            public MapearDatosJuridicos()
            {
                CreateMap<DatosDelContratoDtm, DatosDelContratoDto>()
                .ForMember(dto => dto.Proveedor, dtm => dtm.MapFrom(dtm => dtm.Proveedor.Expresion))
                .ForMember(dto => dto.Cliente, dtm => dtm.MapFrom(dtm => dtm.Cliente.Expresion));
                CreateMap<DatosDelContratoDto, DatosDelContratoDtm>()
                .ForMember(dtm => dtm.Cliente, dto => dto.Ignore())
                .ForMember(dtm => dtm.Proveedor, dto => dto.Ignore());
            }
        }

        public GestorDeDatosDelContrato(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeDatosDelContrato Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeDatosDelContrato(contexto, mapeador);
        }

        protected override IQueryable<DatosDelContratoDtm> AplicarJoins(IQueryable<DatosDelContratoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Cliente);
            consulta = consulta.Include(x => x.Proveedor);
            return consulta;
        }

        protected override void DespuesDeMapearElElemento(DatosDelContratoDtm registro, DatosDelContratoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            var contrato = (ContratoDtm)registro.AmpliacionDe(Contexto);
            if (contrato.ClaseDeContrato == enumClaseDeContrato.Venta && elemento.IdCliente != null) elemento.Cliente = Contexto.SeleccionarPorId<ClienteDtm>((int)registro.IdCliente).Expresion;
            if (contrato.ClaseDeContrato == enumClaseDeContrato.Compra && elemento.IdProveedor != null) elemento.Cliente = Contexto.SeleccionarPorId<ProveedorDtm>((int)registro.IdProveedor).Expresion;
        }

        protected override void AntesDePersistir(DatosDelContratoDtm datos, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(datos, parametros);
            ContratoDtm contrato = (ContratoDtm)parametros.Parametros[nameof(ContratoDtm)];
            ValidacionesSobreLasFechasDeInicioYFinDeContrato(Contexto, datos, contrato, parametros);
            ValidacionesAntesDePersistirSobreLaFechaDeAvisoDeFinDeContrato(datos, parametros);
            if (contrato.ClaseDeContrato == enumClaseDeContrato.Venta)
                ValidacionesSiEsUnContratoDeVenta(datos, contrato, parametros);
            if (contrato.ClaseDeContrato == enumClaseDeContrato.Compra)
                ValidacioneSiEsUnContratoDeCompra(datos, contrato, parametros);

            contrato.SiHayAvalHayFechaDeFinDeContrato(Contexto);

        }

        private void ValidacioneSiEsUnContratoDeCompra(DatosDelContratoDtm datos, ContratoDtm contrato, ParametrosDeNegocio parametros)
        {
            datos.IdCliente = null;
            var anteriorProveedor = parametros.Insertando ? datos.IdProveedor : ((DatosDelContratoDtm)parametros.registroEnBd).IdProveedor;
            if (!contrato.EstaEnLaEtapa(VariablesDeContratos.etapaDeElaboracion) && datos.IdProveedor != anteriorProveedor) Emitir("No se puede modificar el proveedor de un contrato activo");
            if (!contrato.EstaEnLaEtapa(VariablesDeContratos.etapaDeElaboracion) && datos.IdProveedor == default) Emitir("Un contrato ya activo necesita un proveedor");
            if (datos.IdCliente is not null) Emitir("Un contrato de compra no puede tener un cliente");
            if (anteriorProveedor.Entero() > 0)
            {
                var proveedor = Contexto.SeleccionarPorId<ProveedorDtm>(anteriorProveedor.Entero());
                if (datos.Contacto.IsNullOrEmpty()) datos.Contacto = proveedor.Expresion;
                if (datos.Telefono.IsNullOrEmpty()) datos.Telefono = proveedor.Telefono;
                if (datos.eMail.IsNullOrEmpty()) datos.eMail = proveedor.eMail;
            }
        }

        private void ValidacionesSiEsUnContratoDeVenta(DatosDelContratoDtm datos, ContratoDtm contrato, ParametrosDeNegocio parametros)
        {
            datos.IdProveedor = null;
            var anteriorCliente = parametros.Insertando ? datos.IdCliente : ((DatosDelContratoDtm)parametros.registroEnBd).IdCliente;
            if (!contrato.EstaEnLaEtapa(VariablesDeContratos.etapaDeElaboracion) && datos.IdCliente != anteriorCliente) Emitir("No se puede modificar el cliente de un contrato activo");
            if (!contrato.EstaEnLaEtapa(VariablesDeContratos.etapaDeElaboracion) && datos.IdCliente == default) Emitir("Un contrato ya activo necesita un cliente");
            if (datos.IdProveedor is not null) Emitir("Un contrato de venta no puede tener un proveedor");
            if (anteriorCliente.Entero() > 0)
            {
                var cliente = Contexto.SeleccionarPorId<ClienteDtm>(anteriorCliente.Entero());
                if (datos.Contacto.IsNullOrEmpty()) datos.Contacto = cliente.Expresion;
                if (datos.Telefono.IsNullOrEmpty()) datos.Telefono = cliente.Telefono;
                if (datos.eMail.IsNullOrEmpty()) datos.eMail = cliente.eMail;
            }
        }

        private static void ValidacionesSobreLasFechasDeInicioYFinDeContrato(ContextoSe contexto, DatosDelContratoDtm datos, ContratoDtm contrato, ParametrosDeNegocio parametros)
        {
            if (datos.InicioContrato != default) datos.InicioContrato = datos.InicioContrato.Date;
            if (datos.FinContrato != default) datos.FinContrato = ((DateTime)datos.FinContrato).Date;

            var inicioContratoAnterior = parametros.Insertando ? datos.InicioContrato : ((DatosDelContratoDtm)parametros.registroEnBd).InicioContrato;
            var finContratoAnterior = parametros.Insertando ? datos.FinContrato : ((DatosDelContratoDtm)parametros.registroEnBd).FinContrato;

            if (!datos.InicioContrato.Equals(inicioContratoAnterior) && !contrato.EstaEnLaEtapa(VariablesDeContratos.etapaDeElaboracion)) Emitir("La fecha de inicio de contrato no es modificable una vez que está activo");
            if (!(bool)parametros.Parametros.LeerValor(ltrDatosDelContrato.EstoyProrrogando, false) &&
                !datos.FinContrato.Equals(finContratoAnterior) &&
                !contrato.EstaEnLaEtapa(VariablesDeContratos.etapaDeElaboracion))
                Emitir("La fecha de fin de contrato no es modificable una vez que está activo, adende una nueva fecha");
            var loteFueraDeRango = contexto.Set<LoteDeUnContratoDtm>().Where(x => x.IdContrato == datos.IdElemento).
                       Any(x => (x.VigenteDesde != null && x.VigenteDesde < datos.InicioContrato) ||
                       (datos.FinContrato != null && x.VigenteHasta != null &&
                       ((DateTime)x.VigenteHasta).Date > (DateTime)datos.FinContrato));
            if (loteFueraDeRango)
                Emitir($"Alguno de los lotes no está dentro del rango de fechas del contrato");
        }

        private static void ValidacionesAntesDePersistirSobreLaFechaDeAvisoDeFinDeContrato(DatosDelContratoDtm datos, ParametrosDeNegocio parametros)
        {
            if (datos.AvisarAntesDe.Entero() <= 0) datos.AvisarAntesDe = null;

            var datosDb = parametros.Insertando ? null : (DatosDelContratoDtm)parametros.registroEnBd;
            var AvisarAntesDeAnterior = parametros.Insertando ? null : datosDb.AvisarAntesDe;

            if (datos.FinContrato.Equals(default(DateTime?)) && datos.AvisarAntesDe != null)
                Emitir($"Ha indicado que se avise {datos.AvisarAntesDe} mes/meses antes de cumplir la fecha de fin de contato y no ha indicado la fecha de fin de contrato");

            if (!parametros.Insertando && !datos.RecordatorioEnviado.Equals(datosDb.RecordatorioEnviado) && parametros.EsUnaPeticion)
                Emitir("La modificación del valor de notificación se puede modificar con una petición");

            if (datos.FechaDeAvisoPrevio != default && (DateTime)datos.FechaDeAvisoPrevio <= datos.InicioContrato)
                Emitir($"La fecha de aviso, {datos.FechaDeAvisoPrevio}, ha de ser posterior a la de inicio de contrato, {datos.InicioContrato}");

            if (datos.AvisarAntesDe.Entero() - AvisarAntesDeAnterior.Entero() != 0)
                datos.RecordatorioEnviado = false;
        }

        protected override void DespuesDePersistir(DatosDelContratoDtm datos, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(datos, parametros);
            var datosBd = (DatosDelContratoDtm)parametros.registroEnBd;
            var contrato = (ContratoDtm)parametros.Parametros[nameof(ContratoDtm)];

            if (parametros.Modificando)
            {
                DespuesDeModificar(contrato, datos, datosBd, parametros);
                return;
            }
        }

        private void DespuesDeModificar(ContratoDtm contrato, DatosDelContratoDtm datos, DatosDelContratoDtm datosBd, ParametrosDeNegocio parametros)
        {
            contrato.PersistirEventoDeAvisoPrevio(Contexto, datos);
            if (datos.FechaDeAvisoPrevio == default && datosBd.FechaDeAvisoPrevio != default)
            {
                datos.Elemento<ContratoDtm>(Contexto).CrearTraza(Contexto, "Eliminar aviso previo",
                    $"El usuario {Contexto.DatosDeConexion.Login} ha eliminado el aviso previo con fecha {datos.FechaDeAvisoPrevio}");
            }

        }
    }
}
