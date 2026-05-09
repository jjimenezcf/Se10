using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.RegistroEs;
using GestoresDeNegocio.TrabajosSometidos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using ModeloDeDto.Juridico;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Utilidades;
using static GestoresDeNegocio.Juridico.GestorDelPlanificadorDeVentas;
using static ServicioDeDatos.Ventas.VariablesDePlfsDeVenta;

namespace GestoresDeNegocio.Juridico
{

    public class GestorDeContratos : GestorDeElementos<ContextoSe, ContratoDtm, ContratoDto>
    {

        public class MapearContrato : Profile
        {
            public MapearContrato()
            {
                CreateMap<ContratoDtm, ContratoDto>()
                .DtmToDto();

                CreateMap<ContratoDto, ContratoDtm>()
                .DtoToDtm()
                .ForMember(dtm => dtm.Expediente, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.Contrato;

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeContrato.Gestor(Contexto, Contexto.Mapeador);

        public GestorDeContratos(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeContratos Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeContratos(contexto, mapeador);
        }

        protected override IQueryable<ContratoDtm> AplicarJoins(IQueryable<ContratoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Responsable);
            if (parametros.AmpliacionesSolicitadas.Contains(nameof(DatosDelContratoDtm)))
            {
                consulta = consulta.Include(x => x.Datos);
                consulta = consulta.Include(x => x.MatriculaDeGuarderia).ThenInclude(x => x.Curso);
            }
            if (parametros.ColumnasDelGrid.Contains(nameof(ContratoDto.Proveedor))) consulta = consulta.Include(x => x.Datos).ThenInclude(x => x.Proveedor);
            if (parametros.ColumnasDelGrid.Contains(nameof(ContratoDto.Cliente))) consulta = consulta.Include(x => x.Datos).ThenInclude(x => x.Cliente);
            if (parametros.ColumnasDelGrid.Contains(nameof(ContratoDto.Importe))) consulta = consulta.Include(x => x.Saldos);
            if (parametros.ColumnasDelGrid.Contains(nameof(ContratoDto.ImporteAval))) consulta = consulta.Include(x => x.Aval);
            return consulta;
        }

        protected override IQueryable<ContratoDtm> AplicarFiltros(IQueryable<ContratoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            parametros.AplicarFiltroQueMostrar = !filtros.OmitirFiltrosPorEstado(new List<string> { ltrDeUnContrato.IdFacturaEmt, ltrDeUnContrato.IdParteTr, ltrDeUnContrato.IdPlfDeVenta, ltrDeUnContrato.IdExpediente })
                                              || filtros.Select(x => x.Clausula == ltrDeUnContrato.FiltrosPorEvolucion && x.Valor == ltrAvalesSolicitados.AvalDevuelto).Count() != 1;

            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            var hayFiltroPorClase = false;
            var esNecesarioIndicarLaClase = parametros.EstaEjecutandoUnaAccion ? false : parametros.Parametros.LeerValor(ltrDeUnContrato.NecesitaFiltrarPorClase, true);

            AjustarFiltros(filtros, parametros);

            if (filtros.Any(filtro => filtro.Clausula.Equals(ltrDeUnContrato.SelectorParaUnPedido, StringComparison.InvariantCultureIgnoreCase)) &&
                !filtros.Any(filtro => filtro.Clausula.Equals(nameof(ContratoDtm.ClaseDeContrato), StringComparison.InvariantCultureIgnoreCase)))
            {
                filtros.Add(new ClausulaDeFiltrado { Clausula = nameof(ContratoDtm.ClaseDeContrato), Criterio = enumCriteriosDeFiltrado.igual, Valor = enumClaseDeContrato.Compra.ToString() });
            }

            var filtrosPorEvolucion = filtros.Where(x => x.Clausula.Equals(ltrDeUnContrato.FiltrosPorEvolucion, StringComparison.CurrentCultureIgnoreCase));
            foreach (var filtro in filtrosPorEvolucion) consulta = consulta.AplicarFiltroPorEvolucion(Contexto, filtro);
            consulta = consulta.AplicarFiltroPorEtapas(filtros);
            consulta = consulta.AplicarFiltroPorEtapaDePartesTr(Contexto, filtros);
            consulta = consulta.AplicarFiltroPorEtapaDePlanificaciones(Contexto, filtros);
            foreach (var filtro in filtros)
            {
                if (filtro.Aplicado) continue;
                consulta = consulta.RegistrosEsRelacionados(Contexto, filtro);
                consulta = consulta.AplicarFiltroPorClaseDeContrato(filtro, ref hayFiltroPorClase);
                consulta = consulta.AplicarFiltroDeCliente(Contexto, filtro);
                consulta = consulta.AplicarFiltroDeProveedor(Contexto, filtro);
                consulta = consulta.AplicarFiltroDeCabecera(Contexto, filtro);
                consulta = consulta.AplicarFiltroDeImporte(Contexto, filtro);
                consulta = consulta.ContratosConExpediente(filtro, ref esNecesarioIndicarLaClase);
                consulta = consulta.AplicarFiltroPorPlfVenta(Contexto, filtro, ref esNecesarioIndicarLaClase);
                consulta = consulta.AplicarFiltroPorFacturaEmt(Contexto, filtro, ref esNecesarioIndicarLaClase);
                consulta = consulta.AplicarFiltroPorParteTr(Contexto, filtro, ref esNecesarioIndicarLaClase);
                consulta = consulta.AplicarFiltroPorUnitario(Contexto, filtro);
            }

            if (esNecesarioIndicarLaClase && !hayFiltroPorClase && !parametros.HayFiltroPorTipo && !parametros.FiltroPorId && !parametros.FiltroPorNombreIgual)
            {
                if (!parametros.Parametros.ContainsKey(nameof(ContratoDtm.ClaseDeContrato)) && parametros.EsUnaPeticion)
                    GestorDeErrores.Emitir("Ha de indicar la clases de contratos que se quiere obtener");

                var clase = parametros.Parametros[nameof(ContratoDtm.ClaseDeContrato)] is enumClaseDeContrato ?
                     (enumClaseDeContrato)parametros.Parametros[nameof(ContratoDtm.ClaseDeContrato)] :
                     ApiDeEnsamblados.ToEnumerado<enumClaseDeContrato>((string)parametros.Parametros[nameof(ContratoDtm.ClaseDeContrato)]);
                consulta = consulta.Where(x => x.ClaseDeContrato.Equals(clase));
            }
            return consulta;
        }

        protected override IQueryable<ContratoDtm> AplicarSeguridad(IQueryable<ContratoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<ContratoDtm, TipoDeContratoDtm, PermisoDelContratoDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<ContratoDtm, PermisoDelContratoDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void DespuesDeMapearElRegistro(ContratoDto elemento, ContratoDtm registro, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(elemento, registro, opciones);

            if (opciones.Insertando && registro.ClaseDeContrato == enumClaseDeContrato.MatriculaDeGuarderia)
            {
                var curso = Contexto.SeleccionarPorId<CursoDeGuarderiaDtm>(elemento.IdCurso);
                var infante = Contexto.SeleccionarPorId<InfanteDtm>(elemento.IdInfante);
                registro.Nombre = $"Curso {curso.Inicio.Year}-{curso.Fin.Year}: {infante.Nombre}";
                opciones.Parametros[nameof(CursoDeGuarderiaDtm)] = curso;
                opciones.Parametros[nameof(InfanteDtm)] = infante;
            }

            if (opciones.Transitando)
                registro.IdAgenda = LeerRegistroPorId(Contexto, elemento.Id).IdAgenda;
        }

        protected override void AntesDePersistir(ContratoDtm contrato, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(contrato, parametros);
            var claseDelTipo = ((TipoDeContratoDtm)parametros.TipoConFujo).ClaseDeContrato;
            if (contrato.ClaseDeContrato != claseDelTipo)
                GestorDeErrores.Emitir($"La clase de contrato '{contrato.ClaseDeContrato}', debe coincidir con la del tipo '{((TipoDeContratoDtm)parametros.TipoConFujo).Nombre}', y ésta es '{((TipoDeContratoDtm)parametros.TipoConFujo).ClaseDeContrato}'");
            if (parametros.Insertando)
            {
                contrato.IdAgenda = new AgendaDtm() { Nombre = contrato.MiAgenda }.InsertarComoAdministrador(Contexto).Id;
            }

            if (parametros.Modificando)
            {
                contrato.IdAgenda = ((ContratoDtm)parametros.registroEnBd).IdAgenda;

                if (contrato.EstaEnLaEtapa(VariablesDeContratos.etapaVigente) && contrato.IdResponsable.Entero() == 0)
                    GestorDeErrores.Emitir($"El contrato {contrato.Expresion}, por estar vigente, ha de tener un responsable");
            }

            ValidarAsociarExpediente(contrato, parametros);
        }

        protected override void DespuesDePersistir(ContratoDtm contrato, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(contrato, parametros);
            if (parametros.Insertando)
            {
                var tipo = (TipoDeContratoDtm)parametros.Parametros[nameof(TipoConFlujoDtm)];
                var nombreArchivador = $"Información {contrato.DeLaClaseDeContrato}: {contrato.Referencia}";
                contrato.AsociarArchivador(Contexto, tipo.IdTipoArchivador, nombreArchivador);

                if (contrato.ClaseDeContrato == enumClaseDeContrato.MatriculaDeGuarderia)
                {
                    DespuesDePersistirMatricula(contrato, parametros);
                }
                else if (contrato.ClaseDeContrato == enumClaseDeContrato.Compra || contrato.ClaseDeContrato == enumClaseDeContrato.Venta)
                {
                    DespuesDePersistirCompraVenta(contrato, parametros);
                }

                if (contrato.IdResponsable.Entero() > 0) EnviarMensajeDeResponsable(contrato);
            }

            if (parametros.Modificando && SeHaCambiadoElResponsable(contrato, (ContratoDtm)parametros.registroEnBd))
                EnviarMensajeDeResponsable(contrato, (ContratoDtm)parametros.registroEnBd);

        }

        private void DespuesDePersistirMatricula(ContratoDtm contrato, ParametrosDeNegocio parametros)
        {
            new MatriculaDeGuarderiaDtm
            {
                IdElemento = contrato.Id,
                IdCurso = parametros.Parametros.LeerValor<CursoDeGuarderiaDtm>(nameof(CursoDeGuarderiaDtm)).Id,
                IdInfante = parametros.Parametros.LeerValor<InfanteDtm>(nameof(InfanteDtm)).Id,
            }
            .Insertar(Contexto, parametros: parametros.Parametros);
        }
        private void DespuesDePersistirCompraVenta(ContratoDtm contrato, ParametrosDeNegocio parametros)
        {
            new AvanceDtm { IdElemento = contrato.Id, Planificado = 0, Realizado = 0, Facturado = 0, Cobrado = 0 }.Insertar(Contexto, parametros: parametros.Parametros);
            new ProrrogaDtm { IdElemento = contrato.Id, ClaseDeProrroga = enumClaseDeProrroga.noProrrogable, FechaUltimaProrroga = null, Meses = null }.Insertar(Contexto, parametros: parametros.Parametros);
            new DatosDelContratoDtm { IdElemento = contrato.Id, InicioContrato = contrato.FechaCreacion, FinContrato = null, AvisarAntesDe = null, RecordatorioEnviado = false }.Insertar(Contexto, parametros: parametros.Parametros);
            new AvalSolicitadoDtm { IdElemento = contrato.Id, ImporteAval = 0, MesesDeAval = null, AvisoEnviado = false }.Insertar(Contexto, parametros: parametros.Parametros);

            new SaldosDelContratoDtm
            {
                IdElemento = contrato.Id,
                Importe = 0,
                Adendado = 0,
                Aviso = VariablesDeContratos.porcentageAviso.Decimal(),
                Bloqueo = VariablesDeContratos.porcentageBloqueo.Decimal(),
                Notificado = false
            }.Insertar(Contexto, parametros: parametros.Parametros);
        }

        protected override ContratoDtm AntesDeTransitar(ContratoDtm contrato, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            contrato = base.AntesDeTransitar(contrato, transicion, parametros);
            if (contrato.EstaEnLaEtapa(VariablesDeContratos.etapaDeElaboracion) && transicion.DestinoEstaEnLaEtapa(VariablesDeContratos.etapaVigente))
                contrato.AntesDeIniciarUnContrato(Contexto);

            if (transicion.DestinoEstaEnLaEtapa(VariablesDeContratos.etapaCancelado))
                contrato.AntesDeCancelarUnContrato(Contexto);

            if (transicion.DestinoEstaEnLaEtapa(VariablesDeContratos.etapaDeElaboracion))
                contrato.AntesDeDevolverAEnElaboracion(Contexto);

            if (transicion.DestinoEstaEnLaEtapa(VariablesDeContratos.etapaDerogado))
                contrato.AntesDeDerogarUnContrato(Contexto);

            return contrato;
        }

        protected override ContratoDtm DespuesDeTransitar(ContratoDtm contrato, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            contrato = base.DespuesDeTransitar(contrato, transicion, parametros);

            if (transicion.EntreEtapas(enumEtapasDeContratos.CTR_Etapa_Vigente.Estados(), enumEtapasDeContratos.CTR_Etapa_Pdt_Prorroga.Estados()))
                contrato.TrasPasarAProrrogar(Contexto);

            if (transicion.DestinoEstaEnLaEtapa(enumEtapasDeContratos.CTR_Etapa_En_Elaboracion.Estados()))
                contrato.TrasPasarAElaboracion(Contexto);

            return contrato;
        }

        protected override void DespuesDeMapearElElemento(ContratoDtm ctr, ContratoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(ctr, elemento, parametros);
            if (parametros.Parametros.ContainsKey(ltrParametrosNeg.Peticion) && (enumPeticion)parametros.Parametros[ltrParametrosNeg.Peticion] == enumPeticion.epLeerPorId)
            {
                elemento.Etapa = ctr.Etapa();
                elemento.Expediente = ctr.Expediente(Contexto, errorSiNoHay: false)?.Expresion;
            }

            if (ctr.ClaseDeContrato == enumClaseDeContrato.Venta || ctr.ClaseDeContrato == enumClaseDeContrato.Compra)
            {
                DespuesDeMapearElElementoDeCompraVenta(ctr, elemento, parametros);
            }
            else if (ctr.ClaseDeContrato == enumClaseDeContrato.MatriculaDeGuarderia)
            {
                DespuesDeMapearElElementoDeMatriculaDeGuarderia(ctr, elemento, parametros);
            }

            if (ctr.UsaLaAmpliacionDe(Contexto, typeof(MatriculaDeGuarderiaDtm)))
            {
                elemento.InicioContrato = ctr?.MatriculaDeGuarderia(Contexto)?.Curso?.Inicio;
                elemento.FinContrato = ctr?.MatriculaDeGuarderia(Contexto)?.Curso?.Fin;
            }
            else
            if (ctr.UsaLaAmpliacionDe(Contexto, typeof(DatosDelContratoDtm)))
            {
                elemento.InicioContrato = ctr.Datos(Contexto)?.InicioContrato;
                elemento.FinContrato = ctr.Datos(Contexto)?.FinContrato;
            }
        }

        private static void AjustarFiltros(List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            var flt = filtros.Where(x => x.Clausula.Equals(ltrDeUnContrato.SelectorParaUnParteTr, StringComparison.CurrentCultureIgnoreCase) ||
                                         x.Clausula.Equals(ltrDeUnContrato.SelectorParaUnaAsignacionDeParte, StringComparison.CurrentCultureIgnoreCase) ||
                                         x.Clausula.Equals(ltrDeUnContrato.SelectorParaFiltratFacturasEmt, StringComparison.CurrentCultureIgnoreCase) ||
                                         x.Clausula.Equals(ltrDeUnContrato.SelectorParaUnaPlvDeVenta, StringComparison.CurrentCultureIgnoreCase) ||
                                         x.Clausula.Equals(ltrDeUnContrato.SelectorParaFiltrarFacturasRec, StringComparison.CurrentCultureIgnoreCase) ||
                                         x.Clausula.Equals(ltrDeUnContrato.SelectorParaUnaFacturaRec, StringComparison.CurrentCultureIgnoreCase) ||
                                         x.Clausula.Equals(ltrDeUnContrato.SelectorParaUnaFacturaEmt, StringComparison.CurrentCultureIgnoreCase)
                                    ).FirstOrDefault();
            if (flt != null)
            {
                flt.Aplicado = true;
                parametros.Parametros[nameof(ContratoDtm.ClaseDeContrato)] =
                flt.Clausula.Equals(ltrDeUnContrato.SelectorParaFiltrarFacturasRec, StringComparison.CurrentCultureIgnoreCase) ||
                flt.Clausula.Equals(ltrDeUnContrato.SelectorParaUnaFacturaRec, StringComparison.CurrentCultureIgnoreCase)
                ? enumClaseDeContrato.Compra
                : enumClaseDeContrato.Venta;
               
                if (flt.Clausula.Equals(ltrDeUnContrato.SelectorParaUnaFacturaRec, StringComparison.CurrentCultureIgnoreCase))
                    filtros.Add(new ClausulaDeFiltrado(clausula: ltrDeUnContrato.FiltroPorEtapas, criterio: enumCriteriosDeFiltrado.esAlgunoDe, valor: flt.Valor.Replace(Simbolos.PuntoComa, Simbolos.separadorDeEtapas)));

                if (flt.Clausula.Equals(ltrDeUnContrato.SelectorParaFiltratFacturasEmt, StringComparison.CurrentCultureIgnoreCase))
                    filtros.Add(new ClausulaDeFiltrado(clausula: ltrDeUnContrato.FiltroPorEtapas, criterio: enumCriteriosDeFiltrado.esAlgunoDe, valor: flt.Valor.Replace(Simbolos.PuntoComa, Simbolos.separadorDeEtapas)));
            }
        }

        private void DespuesDeMapearElElementoDeCompraVenta(ContratoDtm ctr, ContratoDto elemento, ParametrosDeNegocio parametros)
        {
            if (parametros.ColumnasDelGrid.Contains(nameof(ContratoDto.Cliente), StringComparer.InvariantCultureIgnoreCase))
                elemento.Cliente = ctr.Ampliacion<DatosDelContratoDtm>(Contexto, errorSiNoHay: false, aplicarJoin: true)?.Cliente?.Expresion ?? "pdt. de definir";

            if (parametros.ColumnasDelGrid.Contains(nameof(ContratoDto.Proveedor), StringComparer.InvariantCultureIgnoreCase))
                elemento.Proveedor = ctr.Ampliacion<DatosDelContratoDtm>(Contexto, errorSiNoHay: false, aplicarJoin: true)?.Proveedor?.Expresion ?? "pdt. de definir";

            if (parametros.ColumnasDelGrid.Contains(nameof(ContratoDto.Importe), StringComparer.InvariantCultureIgnoreCase))
                elemento.Importe = ctr.Ampliacion<SaldosDelContratoDtm>(Contexto, errorSiNoHay: false)?.Importe ?? 0;

            if (parametros.ColumnasDelGrid.Contains(nameof(ContratoDto.ImporteAval), StringComparer.InvariantCultureIgnoreCase))
                elemento.ImporteAval = ctr.Ampliacion<AvalSolicitadoDtm>(Contexto, errorSiNoHay: false)?.ImporteAval ?? 0;

        }

        private void DespuesDeMapearElElementoDeMatriculaDeGuarderia(ContratoDtm ctr, ContratoDto elemento, ParametrosDeNegocio parametros)
        {
            if (parametros.ColumnasDelGrid.Contains(nameof(ContratoDto.Cliente), StringComparer.InvariantCultureIgnoreCase))
                elemento.Cliente = ctr.Ampliacion<MatriculaDeGuarderiaDtm>(Contexto, errorSiNoHay: false, aplicarJoin: true)?.Cliente?.Expresion ?? "pdt. de definir";
        }

        private void EnviarMensajeDeResponsable(ContratoDtm contrato, ContratoDtm anterior = null)
        {
            if (contrato.IdResponsable.Entero() > 0) EnviarMensajeDeAsignacionDeResponsable(contrato);
            if (anterior != null && anterior.IdResponsable.Entero() > 0)
                EnviarMensajeDeQuitarResponsable(anterior);
        }

        private void EnviarMensajeDeAsignacionDeResponsable(ContratoDtm contrato)
        {
            if (Contexto.DatosDeConexion.IdUsuario == (int)contrato.IdResponsable)
                return;

            GestorDeCorreos.CrearCorreoPara(Contexto,
                new List<string> { Contexto.SeleccionarPorId<UsuarioDtm>((int)contrato.IdResponsable).eMail },
                $"Se le ha asignado la responsabilidad de controla el contrato {contrato.Referencia}",
                "Ud. es el encargado de controlar la progresión del contrato adjunto",
                new List<ModeloDeDto.TipoDtoElmento> {
                    new ModeloDeDto.TipoDtoElmento {
                       TipoDto  = typeof(ContratoDto).FullName,
                       IdElemento = contrato.Id,
                       Referencia = contrato.Expresion}
                    }, null);
        }

        private void EnviarMensajeDeQuitarResponsable(ContratoDtm contrato)
        {
            if (Contexto.DatosDeConexion.IdUsuario == (int)contrato.IdResponsable)
                return;

            GestorDeCorreos.CrearCorreoPara(Contexto,
                new List<string> { Contexto.SeleccionarPorId<UsuarioDtm>((int)contrato.IdResponsable).eMail },
                $"Se le ha quitado la responsabilidad de controla el contrato {contrato.Referencia}",
                "Ud. ya no es el encargado de controlar la progresión del contrato adjunto",
                new List<ModeloDeDto.TipoDtoElmento> {
                    new ModeloDeDto.TipoDtoElmento {
                       TipoDto  = typeof(ContratoDto).FullName,
                       IdElemento = contrato.Id,
                       Referencia = contrato.Expresion}
                    }, null);
        }

        private bool SeHaCambiadoElResponsable(ContratoDtm contrato, ContratoDtm anterior) => contrato.IdResponsable.Entero() != anterior.IdResponsable.Entero();

        private void ValidarAsociarExpediente(ContratoDtm contrato, ParametrosDeNegocio parametros)
        {
            if (contrato.IdExpediente.Entero() == 0)
            {
                contrato.IdExpediente = null;
                return;
            }

            var expediente = Contexto.SeleccionarPorId<ExpedienteDtm>((int)contrato.IdExpediente, aplicarJoin: true);
            var tipo = expediente.Valor<TipoDeExpedienteDtm>(x => x.Name == nameof(IUsaTipo.Tipo));

            if (ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Expediente, (int)contrato.IdExpediente) == ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir("No se puede modificar el contrato por no tener acceso al expediente indicado");

            bool cambiandoExpediente = parametros.Modificando
                  && contrato.PropiedadCambiada<int?>((ContratoDtm)parametros.registroEnBd, nameof(ContratoDtm.IdExpediente));

            if (parametros.Insertando || cambiandoExpediente || parametros.Eliminando)
            {
                if (Contexto.SeleccionarPorId<CentroGestorDtm>(expediente.IdCg).IdSociedad != Contexto.SeleccionarPorId<CentroGestorDtm>(contrato.IdCg).IdSociedad)
                    GestorDeErrores.Emitir("Las sociedades del expediente y el contrato han de ser las mismas");


                if (tipo.ScDeVenta && !expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Venta))
                    GestorDeErrores.Emitir($"No se puede asociar el contrato '{contrato.Referencia}' al expediente '{expediente.Referencia}' por no estar en la etapa de asociación de contratos");

                if (tipo.ScDeCompra && !expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Compra))
                    GestorDeErrores.Emitir($"No se puede asociar el contrato '{contrato.Referencia}' al expediente '{expediente.Referencia}' por no estar en la etapa de asociación de contratos");

            }

            if (cambiandoExpediente && ((ContratoDtm)parametros.registroEnBd).IdExpediente != null)
            {
                var modoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Contrato, contrato.Id);
                if (ModoDeAcceso.HayPermisosDe(modoDeAcceso, enumModoDeAccesoDeDatos.Gestor))
                    GestorDeErrores.Emitir("Para un cambio de expediente se necesitan permisos de intervención");

                var idAnterior = (int)((ContratoDtm)parametros.registroEnBd).IdExpediente;

                if (ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Expediente, idAnterior) == enumModoDeAccesoDeDatos.SinPermiso)
                    GestorDeErrores.Emitir("No se puede modificar el contrato por no tener acceso al expediente asociado");

                if (tipo.ScDeVenta && !ExtensorDeElementos.EstaEnEtapa(Contexto, enumNegocio.Expediente, idAnterior, enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Venta.Estados()))
                    GestorDeErrores.Emitir("No se puede desasociar el contrato por no estar el expediente asociado en la etapa de desasociar un contrato de venta");

                if (tipo.ScDeCompra && !ExtensorDeElementos.EstaEnEtapa(Contexto, enumNegocio.Expediente, idAnterior, enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Compra.Estados()))
                    GestorDeErrores.Emitir("No se puede desasociar el contrato por no estar el expediente asociado en la etapa de desasociar un contrato de compra");

                parametros.Parametros[ltrDeUnPresupuesto.TrazaDelCambioDeExpediente] = $"Se ha susutituido el expediente " +
                    $"{Contexto.SeleccionarPorId<ExpedienteDtm>(idAnterior).Referencia} por el expediente " +
                    $"{Contexto.SeleccionarPorId<ExpedienteDtm>((int)contrato.IdExpediente).Referencia}";
            }
        }

        public static void ValidarPuedeImputarFacturas(ContextoSe contexto, List<int> ids)
        {
            if (ids.Count != 1) GestorDeErrores.Emitir(ids.Count == 0 ? "Ha de indicar el contrato al que se quiere imputar las facturas" : $"Solo se pueden imputar facturas a un contrato, ha seleccionado {ids.Count} contratos");
            var contrato = contexto.SeleccionarPorId<ContratoDtm>(ids[0]);

            if (!contrato.EsInterventor<TipoDeContratoDtm>(contexto))
                GestorDeErrores.Emitir($"No ha de ser interventor del contrato '{contrato.Referencia}' para poder imputarle facturas");

            if (contrato.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeContratos> { enumEtapasDeContratos.CTR_Etapa_En_Elaboracion, enumEtapasDeContratos.CTR_Etapa_Cancelado }))
                GestorDeErrores.Emitir($"El contrato '{contrato.Referencia}' no puede estar ni en la etapa de '{enumEtapasDeContratos.CTR_Etapa_En_Elaboracion.Nombre()}' ni en la de '{enumEtapasDeContratos.CTR_Etapa_Cancelado.Nombre()}' para poder imputarle facturas");
        }

        public static IEnumerable<ContratoDtm> ValidarExistenPlanificadoresPendientes(ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var claseContrato = filtros.FirstOrDefault(x => x.Clausula.Equals(nameof(ContratoDtm.ClaseDeContrato), StringComparison.CurrentCultureIgnoreCase));

            if (claseContrato == null) GestorDeErrores.Emitir($"Debe indicar la Clase de Contrato de la que se quiere generar planificaciones");

            if (!filtros.Exists(x => x.Clausula.Equals(ltrDeUnContrato.FiltrosPorEvolucion, StringComparison.CurrentCultureIgnoreCase)))
                filtros.Add(new ClausulaDeFiltrado(ltrDeUnContrato.FiltrosPorEvolucion, enumCriteriosDeFiltrado.igual, ltrPlanificadorDeVentas.PlanificadoresPdts));

            if (!filtros.Exists(x => x.Clausula.Equals(ltrDeUnContrato.FiltroPorEtapas, StringComparison.CurrentCultureIgnoreCase)))
                filtros.Add(new ClausulaDeFiltrado(ltrDeUnContrato.FiltroPorEtapas, enumCriteriosDeFiltrado.igual, enumEtapasDeContratos.CTR_Etapa_Vigente));

            var contratos = enumNegocio.Contrato.SeleccionarPorFiltro<ContratoDtm>(contexto, filtros);

            if (contratos.Count() == 0)
            {
                var clase = ApiDeEnsamblados.ToEnumerado<enumClaseDeContrato>(claseContrato.Valor);
                GestorDeErrores.Emitir($"No hay {(clase == enumClaseDeContrato.Venta ? "contratos vigentes" : "matriculas activas")} con planificadores pendientes de generar");
            }

            return contratos;
        }

        public static IEnumerable<ContratoDtm> ValidarExistenPlanificaciones(ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            if (!filtros.Exists(x => x.Clausula.Equals(nameof(ContratoDtm.ClaseDeContrato), StringComparison.CurrentCultureIgnoreCase)))
                filtros.Add(new ClausulaDeFiltrado(nameof(ContratoDtm.ClaseDeContrato), enumCriteriosDeFiltrado.igual, enumClaseDeContrato.Venta));

            if (!filtros.Exists(x => x.Clausula.Equals(ltrDeUnContrato.FiltroPorEtapaDePlfsDeVenta, StringComparison.CurrentCultureIgnoreCase)))
                filtros.Add(new ClausulaDeFiltrado(ltrDeUnContrato.FiltroPorEtapaDePlfsDeVenta, enumCriteriosDeFiltrado.igual, enumEtapasDePlfsDeVenta.PLF_Etapa_Pendiente));

            if (!filtros.Exists(x => x.Clausula.Equals(ltrDeUnContrato.FiltroPorEtapas, StringComparison.CurrentCultureIgnoreCase)))
                filtros.Add(new ClausulaDeFiltrado(ltrDeUnContrato.FiltroPorEtapas, enumCriteriosDeFiltrado.igual, enumEtapasDeContratos.CTR_Etapa_Vigente));

            var contratos = enumNegocio.Contrato.SeleccionarPorFiltro<ContratoDtm>(contexto, filtros);

            if (contratos.Count() == 0)
                GestorDeErrores.Emitir("No hay contratos vigentes con planificaciones pendientes");

            return contratos;
        }

        public static IEnumerable<ContratoDtm> ValidarExistenPartesRealizados(ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            if (!filtros.Exists(x => x.Clausula.Equals(nameof(ContratoDtm.ClaseDeContrato), StringComparison.CurrentCultureIgnoreCase)))
                filtros.Add(new ClausulaDeFiltrado(nameof(ContratoDtm.ClaseDeContrato), enumCriteriosDeFiltrado.igual, enumClaseDeContrato.Venta));

            if (!filtros.Exists(x => x.Clausula.Equals(ltrDeUnContrato.FiltroPorEtapaDePartesTr, StringComparison.CurrentCultureIgnoreCase)))
                filtros.Add(new ClausulaDeFiltrado(ltrDeUnContrato.FiltroPorEtapaDePartesTr, enumCriteriosDeFiltrado.igual, enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar));

            if (!filtros.Exists(x => x.Clausula.Equals(ltrDeUnContrato.FiltroPorEtapas, StringComparison.CurrentCultureIgnoreCase)))
                filtros.Add(new ClausulaDeFiltrado(ltrDeUnContrato.FiltroPorEtapas, enumCriteriosDeFiltrado.igual, enumEtapasDeContratos.CTR_Etapa_Vigente));

            var contratos = enumNegocio.Contrato.SeleccionarPorFiltro<ContratoDtm>(contexto, filtros);

            if (contratos.Count() == 0)
                GestorDeErrores.Emitir("No hay contratos vigentes con partes de trabajo disponibles a facturar");

            return contratos;
        }

        public static void PrefacturarContratos(ContextoSe contexto, List<ClausulaDeFiltrado> filtros, DateTime fechaDesde, DateTime fechaHasta)
        {
            var contratos = ValidarExistenPartesRealizados(contexto, filtros);
            var prefacturas = 0;
            foreach (var contrato in contratos)
            {
                var filtrosDePtr = new Dictionary<string, object>
                {
                    { nameof(ParteTrDtm.IdContrato), contrato.Id },
                    { ltrFiltros.FiltroPorEtapa, enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar }
                };
                var partes = contexto.SeleccionarTodos<ParteTrDtm>(filtrosDePtr);
                if (partes.Count() == 0)
                    continue;

                FacturaEmtDtm factura = null;
                foreach (var ptr in partes)
                {
                    var hito = ptr.UltimoHito(contexto);
                    if (fechaDesde <= hito.Fecha && hito.Fecha < fechaHasta)
                    {
                        if (factura == null) factura = contrato.CrearPrefactura(contexto, $"Factura del contrato '{contrato.Referencia}'. Periodo: {fechaDesde.ToShortDateString()} -- {fechaHasta.ToShortDateString()}");
                        ptr.IncluirParteEnFactura(contexto, factura.Id, new Dictionary<string, object>());
                        prefacturas++;
                    }
                }
            }
            if (prefacturas == 0)
                GestorDeErrores.Emitir("Con las fechas proporcionadas no se han generado ninguna prefactura, aun habiendo partes ya realizados");
        }

        public static void PrefacturarPartesTr(ContextoSe contexto, List<ClausulaDeFiltrado> filtros, DateTime fechaDesde, DateTime fechaHasta)
        {
            var contratos = ValidarExistenPartesRealizados(contexto, filtros);
            var prefacturas = 0;
            foreach (var contrato in contratos)
            {
                var filtrosDePtr = new Dictionary<string, object>
                {
                    { nameof(ParteTrDtm.IdContrato), contrato.Id },
                    { ltrFiltros.FiltroPorEtapa, enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar }
                };
                var partes = contexto.SeleccionarTodos<ParteTrDtm>(filtrosDePtr);
                foreach (var ptr in partes)
                {
                    var hito = ptr.UltimoHito(contexto);
                    if (fechaDesde <= hito.Fecha && hito.Fecha < fechaHasta)
                    {
                        var transicion = ptr.IntentarAplicarTransicion(contexto, TransicionAplicable.Transiciones(VariableDePartesTr.TransicionesPorMotivo, VariableDePartesTr.enumMotivoTransicion.FacturarParteDeUnContrato, errorSiNoHay: true), errorSiNoSeAplica: false);
                        if (transicion.aplicada != null)
                            prefacturas++;
                    }
                }
            }
            if (prefacturas == 0)
                GestorDeErrores.Emitir("Con las fechas proporcionadas no se han generado ninguna prefactura, aun habiendo partes ya realizados");
        }

        public static void PrepararPartesDeTrabajo(ContextoSe contexto, List<ClausulaDeFiltrado> filtros, DateTime fechaDesde, DateTime fechaHasta)
        {
            var contratos = ValidarExistenPlanificaciones(contexto, filtros);
            var partesGenerados = 0;
            DateTime fechaCandidataMin = fechaDesde;
            DateTime fechaCandidataMax = fechaHasta;
            var partesAnteriores = 0;
            var partesPosteriores = 0;
            foreach (var contrato in contratos)
            {
                var filtrosDePlv = new Dictionary<string, object>
                {
                    { nameof(PlanificacionDeVentaDtm.IdContrato), contrato.Id },
                    { ltrFiltros.FiltroPorEtapa, enumEtapasDePlfsDeVenta.PLF_Etapa_Pendiente }
                };
                var plfDeVenta = contexto.SeleccionarTodos<PlanificacionDeVentaDtm>(filtrosDePlv);
                foreach (var plv in plfDeVenta)
                {
                    if (fechaDesde <= plv.EjecutarEl && plv.EjecutarEl < fechaHasta)
                    {
                        plv.TransitarALaEtapa(contexto, enumEtapasDePlfsDeVenta.PLF_Etapa_Generada.EstadosDeLaEtapa(), new Dictionary<string, object>());
                        partesGenerados++;
                    }
                    else
                    {
                        if (plv.EjecutarEl < fechaDesde)
                        {
                            if (plv.EjecutarEl > fechaCandidataMin || partesPosteriores == 0) fechaCandidataMin = plv.EjecutarEl;
                            partesAnteriores++;
                        }
                        if (plv.EjecutarEl >= fechaHasta)
                        {
                            if (fechaCandidataMax > plv.EjecutarEl || partesPosteriores == 0) fechaCandidataMax = plv.EjecutarEl;
                            partesPosteriores++;
                        }
                    }
                }
            }
            if (partesGenerados == 0)
            {
                var postmensajeAnt = $"{(partesAnteriores > 0 ? $"Hay {partesAnteriores} planificaciones a ejecutar anteriores a {fechaCandidataMin.ToShortDateString()}" : "No hay planificaciones anteriores")}";
                var postmensajePos = $"{(partesPosteriores > 0 ? $"hay {partesPosteriores} planificaciones a ejecutar a partir del {fechaCandidataMax.ToShortDateString()}" : "no hay planificaciones posterirores")}";
                GestorDeErrores.Emitir($"Con las fechas proporcionadas {fechaDesde.ToShortDateString()},{fechaHasta.ToShortDateString()} no se han generado ningún parte de trabajo. {postmensajeAnt} y {postmensajePos}");
            }
        }

        public static void GenerarPlanificadores(ContextoSe contexto, List<ClausulaDeFiltrado> filtros, DateTime fechaDeInicio, DateTime fechaDeFin)
        {
            var contratos = ValidarExistenPlanificadoresPendientes(contexto, filtros);
            var planificadoresGenerados = 0;
            foreach (var contrato in contratos)
            {
                var planificadores = contexto.SeleccionarTodos<PlanificadorDeVentaDtm>(nameof(PlanificadorDeVentaDtm.IdContrato), contrato.Id);
                foreach (var planificador in planificadores)
                {
                    if (planificador.Generado || planificador.Inicio >= fechaDeFin || planificador.Hasta <= fechaDeInicio)
                        continue;
                    DateTime fecha1 = planificador.Inicio > fechaDeInicio ? planificador.Inicio : fechaDeInicio;
                    DateTime fecha2 = planificador.Hasta < fechaDeFin ? planificador.Hasta : fechaDeFin;
                    if (!planificador.GenerarPlanificaciones(contexto, fecha1, fecha2))
                        continue;
                    planificadoresGenerados++;
                }
            }
            if (planificadoresGenerados == 0)
                GestorDeErrores.Emitir("Con las fechas proporcionadas no se han generado ningún planificador, aun habiendo pendientes");
        }

        public static void CopiarPlanificadores(ContextoSe contexto, int idContrato, CopiarPlfDeVentaDto copiarPlfDeVenta)
        {
            var contrato = contexto.SeleccionarPorId<ContratoDtm>(idContrato);
            var ids = new List<int>();
            var tran = contexto.IniciarTransaccion();
            try
            {
                if (copiarPlfDeVenta.IdPlanificador != null && copiarPlfDeVenta.IdPlanificador > 0)
                {
                    var planificador = contexto.SeleccionarPorId<PlanificadorDeVentaDtm>((int)copiarPlfDeVenta.IdPlanificador);
                    planificador.Copiar(contexto, contrato, copiarPlfDeVenta.Inicio, copiarPlfDeVenta.Hasta);
                    ids.Add(planificador.Id);
                }
                else
                {
                    var origen = contexto.SeleccionarPorId<ContratoDtm>(copiarPlfDeVenta.IdContrato);
                    var planificadores = origen.PlanificadoresDeVenta(contexto);
                    if (planificadores.Count == 0)
                        GestorDeErrores.Emitir($"El contrato '{contrato.Referencia}' no tiene planificadores definidos");

                    foreach (var planificador in planificadores)
                    {
                        planificador.Copiar(contexto, contrato, copiarPlfDeVenta.Inicio, copiarPlfDeVenta.Hasta);
                        ids.Add(planificador.Id);
                    }
                }
                contrato.CrearTraza(contexto, "Planificadores copiados", $"El usuario '{contexto.DatosDeConexion.Login}' ha copiado del contrato '{contrato.Referencia}' los planificadores con Id '{string.Join(",", ids)}'");
                contexto.Commit(tran);
            }
            catch (Exception ex)
            {
                contexto.Rollback(tran, ex);
                throw;
            }
        }
    }

}
