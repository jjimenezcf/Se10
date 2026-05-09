using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ModeloDeDto.Ventas;
using System.Linq;
using System.Collections.Generic;
using ServicioDeDatos.Ventas;
using GestorDeElementos.Extensores;
using Gestor.Errores;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using Microsoft.EntityFrameworkCore;
using System;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.SistemaDocumental;
using System.IO;
using ServicioXml;

namespace GestoresDeNegocio.Ventas
{

    public class GestorDeRemesasFae : GestorDeElementos<ContextoSe, RemesaFaeDtm, RemesaFaeDto>
    {
        public class MapearRemesaFae : Profile
        {
            public MapearRemesaFae()
            {
                CreateMap<RemesaFaeDtm, RemesaFaeDto>()
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Expresion))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado.Nombre))
                .ForMember(dto => dto.CuentaDeAbono, dtm => dtm.MapFrom(dtm => dtm.CuentaDeAbono.Cuenta.NumeroIban));

                CreateMap<RemesaFaeDto, RemesaFaeDtm>()
                .ForMember(dtm => dtm.CuentaDeAbono, dto => dto.Ignore())
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.RemesaFae;

        //public override TiposDelTipoDeElemento TiposDelTipo => Negocio.TiposDelTipo();

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeRemesaFae.Gestor(Contexto, Contexto.Mapeador);

        public GestorDeRemesasFae(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeRemesasFae Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeRemesasFae(contexto, mapeador);
        }

        protected override IQueryable<RemesaFaeDtm> AplicarJoins(IQueryable<RemesaFaeDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.CuentaDeAbono).ThenInclude(y => y.Cuenta);
            return consulta;
        }

        protected override IQueryable<RemesaFaeDtm> AplicarOrden(IQueryable<RemesaFaeDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return base.AplicarOrden(consulta, ordenacion);
        }

        protected override IQueryable<RemesaFaeDtm> AplicarFiltros(IQueryable<RemesaFaeDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroParaBuscarRemesasDeUnaFactura(Contexto, filtros);
            consulta = consulta.FiltroPorFechaDeCargo(filtros);
            consulta = consulta.FiltroPorFechaDeGeneracion(filtros);
            consulta = consulta.FiltroPorImporteDeRemesa(Contexto, filtros);
            return consulta;
        }

        protected override IQueryable<RemesaFaeDtm> AplicarSeguridad(IQueryable<RemesaFaeDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<RemesaFaeDtm, TipoDeRemesaFaeDtm, PermisoDeLaRemesaFaeDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<RemesaFaeDtm, PermisoDeLaRemesaFaeDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void AntesDePersistir(RemesaFaeDtm rem, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(rem, parametros);
            if (parametros.AccionQueSeEjecuta == ltrDeUnaRemesaFae.Accion_AsociarQ19)
                return;

            //Asigno nulo si inserto o retrocedo a cumplimentación, en el resto de casos lo que haya en BD
            rem.IdArchivo = parametros.Insertando
                ? null
                : parametros.Transitando && 
                  enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion.Lista().Contains(parametros.Parametros.LeerValor<EstadoDtm>(nameof(ltrParametrosNeg.EstadoDestino)).Id)
                ? null
                : ((RemesaFaeDtm)parametros.registroEnBd).IdArchivo;


            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                rem.PresentadaEl = null;
                rem.GeneradaEl = null;
                rem.CargarEl = null;
                rem.CargadaEl = null;
                if (rem.Presentador.IsNullOrEmpty()) rem.Presentador = rem.Acreedor;
                if (rem.NifDelPresentador.IsNullOrEmpty()) rem.Presentador = rem.NifDelPresentador;
                if (rem.SufijoPresentador.IsNullOrEmpty()) rem.Presentador = rem.SufijoPresentador;
                var cuenta = Contexto.SeleccionarPorId<CuentaDeMiSociedadDtm>(rem.IdCuentaDeAbono, aplicarJoin: true);
                rem.Entidad = cuenta.Cuenta.Entidad;
                rem.Oficina = cuenta.Cuenta.Oficina;
            }

            if (rem.CargarEl.HasValue && rem.CargarEl.Fecha() != rem.CargarEl.Fecha().Date)
                GestorDeErrores.Emitir($"No puede indicar hora en la fecha de cuando cargar la remesa '{rem.Referencia}'");

            if (rem.CargadaEl.HasValue && rem.CargadaEl.Fecha() != rem.CargadaEl.Fecha().Date)
                GestorDeErrores.Emitir($"No puede indicar hora en la fecha de cargo de la remesa '{rem.Referencia}'");

            if (parametros.Operacion == enumTipoOperacion.Modificar && !parametros.ProcesandoTransicion(rem) && !rem.EstaEnLaEtapa(enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion))
                GestorDeErrores.Emitir($"La remesa '{rem.Referencia}' no está en la etapa de {enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion.Nombre(true)} y por tanto no es modificable");

            if (rem.PropiedadCambiada<enumClaseDeRemesaFae>(nameof(RemesaFaeDtm.Clase), parametros) && rem.Detalles<FacturaEmtDeUnaRemesaDtm>(Contexto).Count() > 0)
                GestorDeErrores.Emitir($"No se puede modificar la clase de la remesa '{rem.Referencia}' por tener ya facturas seleccionadas");

            rem.Acreedor = rem.Sociedad(Contexto).Nombre;
            rem.NifDelAcreedor = rem.Sociedad(Contexto).NIF;
            if (rem.PropiedadCambiada<DateTime?>(nameof(RemesaFaeDtm.CargadaEl), parametros))
            {
                if (((RemesaFaeDtm)parametros.registroEnBd).CargadaEl.HasValue && rem.CargadaEl.HasValue)
                    GestorDeErrores.Emitir($"Para modificar la fecha de cargo anule el cargo de la remesa '{rem.Referencia}' y vuélvalo a crear");

                if (!rem.EstaEnLaEtapa(enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion))
                    GestorDeErrores.Emitir($"Para cargar o anular el cargo de la remesa '{rem.Referencia}' ha de estar en la etapa de '{enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion.Nombre(minusculas: true)}'");

                if (rem.CargadaEl.HasValue)
                {
                    if (rem.CargadaEl.Fecha() < rem.GeneradaEl.Fecha().Date)
                        GestorDeErrores.Emitir($"La fecha de cargo '{rem.CargadaEl.Fecha().ToShortDateString()}' de la remesa '{rem.Referencia}' no puede ser anterior a la de generación '{rem.GeneradaEl.Fecha().ToShortDateString()}', y debería ser posterior a la presentación '{rem.PresentadaEl.Fecha().ToShortDateString()}'");

                    if (rem.CargadaEl.Fecha().Date > DateTime.Today)
                        GestorDeErrores.Emitir($"La fecha del cargo '{rem.CargadaEl.Fecha().ToShortDateString()}' de la remesa '{rem.Referencia}' no puede ser mayor a la de hoy");
                }

                if (!rem.EsInterventor(Contexto))
                    GestorDeErrores.Emitir($"Para cargar o anular el cargo de la remesa '{rem.Referencia}' en una fecha '{rem.CargadaEl.Fecha().ToShortDateString()}' diferente a la parametrizada '{rem.CargarEl.Fecha().ToShortDateString()}' se necesita un perfil de intervención");
            }
        }

        protected override void DespuesDePersistir(RemesaFaeDtm rem, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(rem, parametros);

            if (rem.CargarEl is not null)
            {
                if (parametros.Insertando)
                    rem.CrearEventoDeCargo(Contexto);
                if (rem.PropiedadCambiada<DateTime?>(nameof(RemesaFaeDtm.CargarEl), parametros))
                    rem.PersistirEventoDeCargo(Contexto);
            }
            else rem.EliminarEventoDeCargo(Contexto);

            if (rem.PropiedadCambiada<DateTime?>(nameof(RemesaFaeDtm.CargadaEl), parametros))
            {
                if (rem.CargadaEl is null) rem.AnularCargo(Contexto, ((RemesaFaeDtm)parametros.registroEnBd).CargadaEl.Fecha());
                if (rem.CargadaEl is not null) rem.Cargar(Contexto, rem.CargadaEl.Fecha());
            }
        }

        protected override RemesaFaeDtm AntesDeTransitar(RemesaFaeDtm remesa, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            remesa.IdArchivo = Contexto.SeleccionarPorId<RemesaFaeDtm>(remesa.Id, usarLaCache: false).IdArchivo;
            remesa = base.AntesDeTransitar(remesa, transicion, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion.Estados(), enumEtapasDeRemesasFae.REM_Etapa_Cancelada.Estados()))
                remesa.AntesDeCancelar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion.Estados(), enumEtapasDeRemesasFae.REM_Etapa_Generada.Estados()))
                remesa.AntesDeGenerar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasFae.REM_Etapa_Generada.Estados(), enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion.Estados()))
                remesa.AntesDePresentar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion.Estados(), enumEtapasDeRemesasFae.REM_Etapa_De_Cierre.Estados()))
                remesa.AntesDeDarPorConciliada(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasFae.REM_Etapa_Generada.Estados(), enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion.Estados()))
                remesa = remesa.AntesDeAnularGeneracion(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion.Estados(), enumEtapasDeRemesasFae.REM_Etapa_Generada.Estados()))
                remesa = remesa.AntesDeAnularPresentacion(Contexto, parametros);

            return remesa;
        }

        protected override RemesaFaeDtm DespuesDeTransitar(RemesaFaeDtm remesa, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            remesa = base.DespuesDeTransitar(remesa, transicion, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion.Estados(), enumEtapasDeRemesasFae.REM_Etapa_Generada.Estados()))
                GenerarSepaQ19(Contexto, remesa);

            if (transicion.EntreEtapas(enumEtapasDeRemesasFae.REM_Etapa_Generada.Estados(), enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion.Estados()))
                remesa.DespuesDeAnularGeneracion(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion.Estados(), enumEtapasDeRemesasFae.REM_Etapa_Generada.Estados()))
                remesa.DespuesDeAnularPresentacion(Contexto, parametros);

            return remesa;
        }

        protected override void DespuesDeMapearElElemento(RemesaFaeDtm remesa, RemesaFaeDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(remesa, elemento, parametros);
            elemento.ImporteRemesa = remesa.Total(Contexto);
            elemento.Cobrado = remesa.Cobrado(Contexto);
            elemento.Pendiente = elemento.ImporteRemesa - elemento.Cobrado;
            elemento.Incluidas = remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(Contexto).Count();
            elemento.Devueltas = remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(Contexto).Where(x => x.DevueltoEl.HasValue).Count();
            elemento.Etapas = remesa.ListaDeEtapas();
        }

        private void GenerarSepaQ19(ContextoSe contexto, RemesaFaeDtm remesa)
        {
            var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, $"Rem-{remesa.Referencia}.xml".NormalizarFichero());
            remesa.GenerarSepaQ19(contexto, rutaConFichero);
            var idArchivo = ServidorDocumental.SubirArchivo(contexto, rutaConFichero, sanitizar: false);
            remesa.AsociarArchivo(contexto, idArchivo, ltrDeUnaRemesaFae.Accion_AsociarQ19);
        }

        public static void AntesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var idRemesa = entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var remesa = entorno.Contexto.SeleccionarPorId<RemesaFaeDtm>(idRemesa);

            if (vinculado == enumNegocio.Archivos)
            {
                if (remesa.IdArchivo.Entero() == entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado)))
                {
                    var archivo = entorno.Contexto.SeleccionarPorId<ArchivoDtm>(remesa.IdArchivo.Entero());
                    GestorDeErrores.Emitir($"No puede quitar de la {enumNegocio.RemesaFae.Singular(true)} '{remesa.Referencia}' el {enumNegocio.Archivos.Singular(true)} '{archivo.Nombre}' por ser el original de la emisión");
                }
            }
        }

        public static void Cargar(ContextoSe contexto, int idRemesa, DateTime cargadaEl)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                var remesa = contexto.SeleccionarPorId<RemesaFaeDtm>(idRemesa);
                remesa.CargadaEl = cargadaEl;
                remesa.Modificar(contexto, esUnaAccion: true);
                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran, e);
                throw;
            }
        }

        public static CargarRemesaDto InformacionDeCargar(ContextoSe contexto, int idRemesa)
        {
            contexto.IniciarTraza(nameof(InformacionDeCargar));
            try
            {
                var remesa = contexto.SeleccionarPorId<RemesaFaeDtm>(idRemesa);
                if (remesa.CargadaEl.HasValue)
                    GestorDeErrores.Emitir($"La remesa '{remesa.Referencia}' tiene fecha de cargo '{remesa.CargadaEl.Fecha().ToShortDateString()}', si quiere modificar la fecha de cargo, anúlela, y vuélvala a cargar");

                if (!remesa.EsInterventor(contexto))
                    GestorDeErrores.Emitir($"Para cargar la remesa '{remesa.Referencia}' manualmente, necesita permisos de intervención");

                var info = new CargarRemesaDto
                {
                    IdElemento = idRemesa,
                    Elemento = remesa.Expresion,
                    CargarEl = remesa.CargarEl,
                    CargadaEl = null,
                    Incluidas = remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto).Count(),
                    Devueltas = remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto).Where(x => x.DevueltoEl != null).Count(),
                    ImporteRemesa = remesa.Total(contexto),
                    Cobrado = remesa.Cobrado(contexto),
                };
                contexto.CerrarTraza();
                return info;
            }
            catch (Exception e)
            {
                contexto.CerrarTraza(e.Message);
                throw;
            }
        }

        public static void AnularCargo(ContextoSe contexto, int idRemesa)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                var remesa = contexto.SeleccionarPorId<RemesaFaeDtm>(idRemesa);
                remesa.CargadaEl = null;
                remesa.Modificar(contexto, esUnaAccion: true);
                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran, e);
                throw;
            }
        }

        public static AnularCargoRemesaDto InformacionDeAnularCargo(ContextoSe contexto, int idRemesa)
        {
            contexto.IniciarTraza(nameof(InformacionDeAnularCargo));
            try
            {
                var remesa = contexto.SeleccionarPorId<RemesaFaeDtm>(idRemesa);
                if (!remesa.CargadaEl.HasValue)
                    GestorDeErrores.Emitir($"La remesa '{remesa.Referencia}' aun no ha sido cargada, tiene fecha de planificación de cargo '{remesa.CargarEl.Fecha().ToShortDateString()}', no se puede anular su cargo");

                if (!remesa.EsInterventor(contexto))
                    GestorDeErrores.Emitir($"Para anular el cargo de la remesa '{remesa.Referencia}' manualmente, necesita permisos de intervención");

                var info = new AnularCargoRemesaDto
                {
                    IdElemento = idRemesa,
                    Elemento = remesa.Expresion,
                    CargarEl = remesa.CargarEl,
                    CargadaEl = remesa.CargadaEl,
                    Incluidas = remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto).Count(),
                    Devueltas = remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto).Where(x => x.DevueltoEl != null).Count(),
                    ImporteRemesa = remesa.Total(contexto),
                    Cobrado = remesa.Cobrado(contexto),
                };
                contexto.CerrarTraza();
                return info;
            }
            catch (Exception e)
            {
                contexto.CerrarTraza(e.Message);
                throw;
            }
        }
    }

}
