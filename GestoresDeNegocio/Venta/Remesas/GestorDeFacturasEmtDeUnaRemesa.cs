using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;
using Gestor.Errores;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;
using iText.Commons.Actions.Contexts;
using System;
using Azure;

namespace GestoresDeNegocio.Callejero
{

    public class GestorDeFacturasEmtDeUnaRemesa : GestorDeRelaciones<ContextoSe, FacturaEmtDeUnaRemesaDtm, FacturaEmtDeUnaRemesaDto>
    {
        public class ltrFacturasEmtDeUnaRemesa
        {
        }

        public class MapearFacturasEmtDeUnaRemesa : Profile
        {
            public MapearFacturasEmtDeUnaRemesa()
            {
                CreateMap<FacturaEmtDeUnaRemesaDtm, FacturaEmtDeUnaRemesaDto>()
                .ForMember(dto => dto.Elemento, dtm => dtm.MapFrom(dtm => dtm.Elemento.Expresion))
                .ForMember(dto => dto.Factura, dtm => dtm.MapFrom(dtm => dtm.Factura.Expresion));

                CreateMap<FacturaEmtDeUnaRemesaDto, FacturaEmtDeUnaRemesaDtm>()
                .ForMember(dtm => dtm.Factura, dto => dto.Ignore())
                .ForMember(dtm => dtm.CargadaEl, dto => dto.Ignore())
                .ForMember(dtm => dtm.FechaMaximaDeDevolucion, dto => dto.Ignore())
                .ForMember(dtm => dtm.Elemento, dto => dto.Ignore());

            }
        }

        public GestorDeFacturasEmtDeUnaRemesa(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {
        }
        internal static GestorDeFacturasEmtDeUnaRemesa Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeFacturasEmtDeUnaRemesa(contexto, mapeador);
        }


        protected override IQueryable<FacturaEmtDeUnaRemesaDtm> AplicarJoins(IQueryable<FacturaEmtDeUnaRemesaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(rp => rp.Elemento);
            consulta = consulta.Include(rp => rp.Factura);
            return consulta;
        }

        protected override void AntesDePersistir(FacturaEmtDeUnaRemesaDtm fr, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(fr, parametros);
            var remesa = (RemesaFaeDtm)fr.Elemento(Contexto);
            var factura = fr.Factura(Contexto);

            if (factura.Sociedad(Contexto).Id != remesa.Sociedad(Contexto).Id)
                GestorDeErrores.Emitir($"la remesa '{remesa.Referencia}' y la factura '{factura.Referencia}' han de ser de la misma sociedad");

            if ((parametros.Insertando || parametros.Eliminando) && !remesa.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeRemesasFae> { enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion }))
                GestorDeErrores.Emitir($"No se puede {(parametros.Insertando ? "incluir" : "excluir")}  " +
                    $"la factura '{factura.Referencia}' en " +
                    $"la remesa '{remesa.Referencia}' " +
                    $"por no estar en la etapa de '{enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion.Nombre(true)}'");

            if (parametros.Insertando)
            {
                ValidarQueLaFacturaNoEstaIncluidaEnOtraRemesaViva(remesa, factura);

                if (remesa.Clase == enumClaseDeRemesaFae.Devueltas && !factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Devuelta))
                    GestorDeErrores.Emitir($"No se puede incluir la factura '{factura.Referencia}' en la remesa '{remesa.Referencia}' por no estar la factura en la etapa de '{enumEtapasDeFacturasEmt.FAE_Etapa_Devuelta.Nombre(true)}' ya que la clase de la remesa es para '{remesa.Clase.Descripcion()}'");

                if (remesa.Clase == enumClaseDeRemesaFae.Emitidas && !factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida))
                    GestorDeErrores.Emitir($"No se puede incluir la factura '{factura.Referencia}' en la remesa '{remesa.Referencia}' por no estar la factura en la etapa de '{enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.Nombre(true)}' ya que la clase de la remesa es para '{remesa.Clase.Descripcion()}'");

                factura.Cliente(Contexto).CuentaDeCliente(Contexto, ServicioDeDatos.Contabilidad.enumClaseDeCuentaBancaria.Pago);

                if (factura.Bi(Contexto) == 0) GestorDeErrores.Emitir($"No puede emitir la factura '{factura.Referencia}' porque no tiene valor");

            }

            if (parametros.Modificando)
            {
                if (!remesa.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeRemesasFae> { enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion }))
                    GestorDeErrores.Emitir($"No se puede modificar la factura '{factura.Referencia}' en la remesa '{remesa.Referencia}' por no está '{enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion.Nombre(true)}'");

                #region Valido que si se ha modificado la fecha de devolución sea correcta
                if (fr.PropiedadCambiada<DateTime?>(nameof(FacturaEmtDeUnaRemesaDtm.DevueltoEl), parametros))
                {
                    if (fr.DevueltoEl.HasValue && ((FacturaEmtDeUnaRemesaDtm)parametros.registroEnBd).DevueltoEl.HasValue)
                        GestorDeErrores.Emitir($"la '{factura.Referencia}' de la remesa'{remesa.Referencia}' ya tiene una fecha de devolución, no puede modificarse, anule la devolución y vuelva a excluirla");

                    if (!fr.PropiedadCambiada<string>(nameof(FacturaEmtDeUnaRemesaDtm.Motivo), parametros))
                        GestorDeErrores.Emitir($"Debe indicar un motivo diferente al último indicado");

                    if (fr.DevueltoEl is not null)
                    {
                        if (!fr.CargadaEl.HasValue && !remesa.EsInterventor<TipoDeRemesaFaeDtm>(Contexto))
                            GestorDeErrores.Emitir($"No se puede devolver la factura '{factura.Referencia}' de la remesa  '{remesa.Referencia}' ya que aun no se ha cargado, para hacer esto necesita permisos de intervención");

                        if (remesa.PresentadaEl.Fecha() > fr.DevueltoEl)
                            GestorDeErrores.Emitir($"No se puede devolver la '{factura.Referencia}' con fecha anterior a la presentación de la remesa'{remesa.Referencia}'");

                        var fechaMaxima = remesa.FechaMaximaDeDevolucion(Contexto);
                        if (fechaMaxima < fr.DevueltoEl && !remesa.EsInterventor<TipoDeRemesaFaeDtm>(Contexto))
                            GestorDeErrores.Emitir($"En la remesa '{remesa.Referencia}' se ha indicado que la factura '{factura.Referencia}' se quiere devolver después de su fechá máxima de devolución '{fechaMaxima.ToShortDateString()}', para ello necesita permisos de intervención");

                        if (fr.Motivo.IsNullOrEmpty())
                            GestorDeErrores.Emitir($"En la remesa '{remesa.Referencia}' se ha indicado que la factura '{factura.Referencia}' se quiere devolver, es obligatorio el motivo");

                    }
                    else
                    {
                        if (fr.Motivo.IsNullOrEmpty())
                            GestorDeErrores.Emitir($"Si quiere anular la devolución de la factura '{factura.Referencia}' de la remesa '{remesa.Referencia}' ha de indicar el motivo");
                        fr.CargadaEl = remesa.CargadaEl;

                    }
                }
                #endregion

                #region Valido que si se ha modificado la fecha de cargo, sea correcta
                else if (fr.PropiedadCambiada<DateTime?>(nameof(FacturaEmtDeUnaRemesaDtm.CargadaEl), parametros))
                {
                    if (fr.CargadaEl.HasValue && ((FacturaEmtDeUnaRemesaDtm)parametros.registroEnBd).CargadaEl.HasValue)
                        GestorDeErrores.Emitir($"No se puede modificar la fecha '{((FacturaEmtDeUnaRemesaDtm)parametros.registroEnBd).CargadaEl.Fecha().ToShortDateString()}' de cargo de la factura '{factura.Referencia}' remesada en '{remesa.Referencia}', anúlelo o devuélvalo");

                    if (!fr.CargadaEl.HasValue && fr.DevueltoEl.HasValue)
                        GestorDeErrores.Emitir($"No se puede anular el cargo de la factura '{factura.Referencia}' remesada en '{remesa.Referencia}' ya que se devolvió el día '{fr.DevueltoEl.Fecha().ToShortDateString()}'");

                    if (fr.CargadaEl.HasValue && fr.DevueltoEl.HasValue)
                        GestorDeErrores.Emitir($"No se puede realizar el cargo de la factura '{factura.Referencia}' remesada en '{remesa.Referencia}' ya que se devolvió el día '{fr.DevueltoEl.Fecha().ToShortDateString()}', anule la devolución y cárguelo");

                }
                else
                    GestorDeErrores.Emitir($"De la factura '{factura.Referencia}' remesada en '{remesa.Referencia}', solo se puede modificar la fecha de devolución o cargo");

                #endregion
            }
        }

        private void ValidarQueLaFacturaNoEstaIncluidaEnOtraRemesaViva(RemesaFaeDtm remesa, FacturaEmtDtm factura)
        {
            var frs = Contexto.SeleccionarTodos<FacturaEmtDeUnaRemesaDtm>(nameof(FacturaEmtDeUnaRemesaDtm.IdFactura), factura.Id, aplicarJoin: true);
            foreach (var r in frs)
                if (r.Elemento.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeRemesasFae> { enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion, enumEtapasDeRemesasFae.REM_Etapa_Generada, enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion }))
                    GestorDeErrores.Emitir($"No se puede incluir la factura '{factura.Referencia}' en la remesa '{remesa.Referencia}' por estar en la remesa '{r.Elemento.Referencia}' que está en la etapa de '{r.Elemento.Etapa().Nombre(true)}'");
        }

        protected override void DespuesDePersistir(FacturaEmtDeUnaRemesaDtm fr, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(fr, parametros);
            var remesa = (RemesaFaeDtm)fr.Elemento(Contexto);
            var factura = fr.Factura(Contexto);

            if (parametros.Operacion == enumTipoOperacion.Insertar)
                factura.TransitarALaEtapa(Contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Remesada.EstadosDeLaEtapa(), new Dictionary<string, object>());

            if (parametros.Operacion == enumTipoOperacion.Eliminar)
                factura.TransitarALaEtapa(Contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.EstadosDeLaEtapa(), new Dictionary<string, object>());

            AlDevolverORetrocederUnaDevolucion(fr, parametros, remesa, factura);

            AlCargarOAnularCargoDeUnaRemesa(fr, parametros, factura);

        }

        protected override void EliminarCaches(FacturaEmtDeUnaRemesaDtm remesa, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(remesa, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Rem_Fae_Total, remesa.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Rem_Fae_Cobrado, remesa.Id.ToString());
        }

        private void AlDevolverORetrocederUnaDevolucion(FacturaEmtDeUnaRemesaDtm fr, ParametrosDeNegocio parametros, RemesaFaeDtm remesa, FacturaEmtDtm factura)
        {
            var continuar = parametros.AccionQueSeEjecuta == ltrDeUnaRemesaFae.Accion_DevolverFactura ||
                            parametros.AccionQueSeEjecuta == ltrDeUnaRemesaFae.Accion_AnularDevolucionDeFactura ||
                            parametros.EsUnaPeticion && parametros.Peticion == enumPeticion.epModificarRelacion;

            if (!continuar)
                return;

            if (!fr.PropiedadCambiada<DateTime?>(nameof(FacturaEmtDeUnaRemesaDtm.DevueltoEl), parametros))
                return;

            #region Si se indica que se devuelve y hay cargo, se devuelve, si no lo hay (por lo que sea aun no se ha cargado, ha de ser interventor) se transita a devuelta
            if (fr.DevueltoEl is not null)
            {
                var cobros = factura.Detalles<CobroDeFaeDtm>(Contexto);
                var cobroRemesado = cobros.FirstOrDefault(x => x.IdFacturaRemesada == fr.Id);
                if (cobroRemesado != null)
                {
                    cobroRemesado.Eliminar(Contexto, esUnaAccion: true, parametros: new Dictionary<string, object> { { nameof(VariableDeFacturasEmt.enumMotivoTransicion), VariableDeFacturasEmt.enumMotivoTransicion.DevolverPagoRemesado } });
                }
                else
                {
                    if (!remesa.EsInterventor<TipoDeRemesaFaeDtm>(Contexto))
                        GestorDeErrores.Emitir($"Para indicar que la factura '{factura.Referencia}' en la remesa '{remesa.Referencia}' ha sido devuelta, y el cargo aun no se ha indicado, ha de tener permisos de interventor");

                    factura = factura.TransitarALaEtapa(Contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Devuelta.EstadosDeLaEtapa(), new Dictionary<string, object>());
                    new TrazasDeUnaFacturaEmtDtm
                    {
                        IdElemento = fr.IdFactura,
                        Nombre = "Excluida de la remesa antes del cargo",
                        Descripcion = $"El usuario '{Contexto.DatosDeConexion.Login}' ha indicado que la factura incluida en la remesa '{remesa.Referencia}' no ha sido cargada en la cuenta del cliente, por el motivo indicado.{Environment.NewLine}Motivo:{Environment.NewLine}{fr.Motivo}",

                    }.InsertarTraza(Contexto);
                }
            }
            #endregion

            #region Si se anula la devolución y hay fecha de 'cargada el', se crea cobro en la factura (la devuelve a cobrada), si no hay, se devuelve a remesada           
            if (fr.DevueltoEl is null)
            {
                if (remesa.CargadaEl is null)
                {
                    if (!remesa.EsInterventor<TipoDeRemesaFaeDtm>(Contexto))
                        GestorDeErrores.Emitir($"Para indicar que la factura '{factura.Referencia}' de la remesa '{remesa.Referencia}' ha sido re-incluida cuando el cargo aun  no ha sido realizado, ha de tener permisos de interventor");

                    factura = factura.TransitarALaEtapa(Contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Remesada.EstadosDeLaEtapa(), new Dictionary<string, object>());
                    new TrazasDeUnaFacturaEmtDtm
                    {
                        IdElemento = fr.IdFactura,
                        Nombre = "Incluida la factura en la remesa antes del cargo",
                        Descripcion = $"El usuario '{Contexto.DatosDeConexion.Login}' ha indicado que la factura se incluya otra vez en la remesa '{remesa.Referencia}' para poder realizar el cargo, por el motivo indicado.{Environment.NewLine}Motivo:{Environment.NewLine}{fr.Motivo}",

                    }.InsertarTraza(Contexto);
                }
                else
                {
                    factura.Cobrar(Contexto, idFacturaRemesada: fr.Id);
                }
            }
            #endregion
        }

        private void AlCargarOAnularCargoDeUnaRemesa(FacturaEmtDeUnaRemesaDtm fr, ParametrosDeNegocio parametros, FacturaEmtDtm factura)
        {
            if (!(
                parametros.AccionQueSeEjecuta == ltrDeUnaRemesaFae.Accion_AnularCargoDeRemesa ||
                parametros.AccionQueSeEjecuta == ltrDeUnaRemesaFae.Accion_CargoDeRemesa
                ))
                return;

            if (fr.PropiedadCambiada<DateTime?>(nameof(FacturaEmtDeUnaRemesaDtm.CargadaEl), parametros))
            {
                if (fr.CargadaEl.HasValue) factura.Cobrar(Contexto, idFacturaRemesada: fr.Id);
                if (!fr.CargadaEl.HasValue)
                {
                    var cobros = factura.Detalles<CobroDeFaeDtm>(Contexto);
                    var cobroRemesado = cobros.FirstOrDefault(x => x.IdFacturaRemesada == fr.Id);
                    cobroRemesado.Eliminar(Contexto, esUnaAccion: true, parametros: new Dictionary<string, object> { { nameof(VariableDeFacturasEmt.enumMotivoTransicion), VariableDeFacturasEmt.enumMotivoTransicion.AnularPresentacionDeRemesa } });
                }
            }
        }

        protected override void DespuesDeMapearElElemento(FacturaEmtDeUnaRemesaDtm fr, FacturaEmtDeUnaRemesaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(fr, elemento, parametros);
            var remesa = (RemesaFaeDtm)fr.Elemento(Contexto);
            var factura = fr.Factura(Contexto);
            elemento.Cliente = factura.Cliente(Contexto).Expresion;
            elemento.Etapas = remesa.ListaDeEtapas();
            elemento.EstaCargada = fr.CargadaEl.HasValue;
            elemento.EsInterventor = remesa.EsInterventor(Contexto);
            elemento.Factura = $"[{factura.NumeroDeFactura}] {factura.Expresion}";
            if (elemento.ModoDeAcceso.SoyGestor() &&
                !remesa.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeRemesasFae> { enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion, enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion }))
                elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;

            if (remesa.EstaEnLaEtapa(enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion))
            {
                elemento.FechaMaximaDeDevolucion = remesa.FechaMaximaDeDevolucion(Contexto);
                if (fr.CargadaEl is null && parametros.Peticion == enumPeticion.epLeerPorId)
                    elemento.CargadaEl = remesa.CargarEl;
            }

            elemento.ImporteFactura = factura.APagar(Contexto);
        }

    }
}
