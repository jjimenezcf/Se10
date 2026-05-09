using AutoMapper;
using DocumentFormat.OpenXml.Math;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.TrabajosSometidos;
using GestoresDeNegocio.Ventas;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using ModeloDeDto.Presupuesto;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilidades;

namespace GestoresDeNegocio.Presupuesto
{

    public class GestorDePresupuestos : GestorDeElementos<ContextoSe, PresupuestoDtm, PresupuestoDto>, ITotalizador<TotalesDePresupuestos>
    {
        public class MapearPresupuesto : Profile
        {
            public MapearPresupuesto()
            {
                CreateMap<PresupuestoDtm, PresupuestoDto>()
                .DtmToDto()
                .ForMember(dto => dto.Expediente, dtm => dtm.MapFrom(dtm => dtm.IdExpediente.Entero() > 0
                ? dtm.Expediente == null ? null : dtm.Expediente.Expresion
                : "Sin expediente"));

                CreateMap<PresupuestoDto, PresupuestoDtm>()
                .DtoToDtm()
                .ForMember(dtm => dtm.Total, dto => dto.Ignore())
                .ForMember(dtm => dtm.Expediente, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.Presupuesto;

        //public override TiposDelTipoDeElemento TiposDelTipo => Negocio.TiposDelTipo();

        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDePresupuesto.Gestor(Contexto, Contexto.Mapeador);

        public GestorDePresupuestos(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDePresupuestos Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePresupuestos(contexto, mapeador);
        }

        protected override IQueryable<PresupuestoDtm> AplicarJoins(IQueryable<PresupuestoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Solicitante)
                .Include(x => x.Responsable)
                .Include(x => x.Expediente);
            return consulta;
        }

        protected override IQueryable<PresupuestoDtm> AplicarFiltros(IQueryable<PresupuestoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            if (filtros.Exists(x => (x.Clausula.Equals(ltrDeUnPresupuesto.FiltroPorConOSinFacturaEmt, StringComparison.CurrentCultureIgnoreCase) &&
                                    (x.Valor.Entero() == ltrParametrosNeg.ConRelacion || x.Valor.Entero() == ltrParametrosNeg.SinRelacion)) ||
                                     x.Clausula.Equals(ltrDeUnPresupuesto.IdFacturaEmt, StringComparison.CurrentCultureIgnoreCase) ||
                                    (x.Clausula.Equals(ltrDeUnPresupuesto.FiltroPorConOSinParteTr, StringComparison.CurrentCultureIgnoreCase) &&
                                    (x.Valor.Entero() == ltrParametrosNeg.ConRelacion || x.Valor.Entero() == ltrParametrosNeg.SinRelacion)) ||
                                     x.Clausula.Equals(ltrDeUnPresupuesto.IdParteTr, StringComparison.CurrentCultureIgnoreCase)
                ))
                parametros.AplicarFiltroQueMostrar = false;

            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroPorFactura(Contexto, filtros);
            consulta = consulta.FiltroPorPartes(Contexto, filtros);
            consulta = consulta.FiltroPorPartesPrefacturados(Contexto, filtros);
            consulta = consulta.FiltroParaImputarFactura(Contexto, filtros);

            var filtroMismaSociedad = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrFiltros.VincularCon);
            if (filtroMismaSociedad is not null)
            {
                var vincularCon = NegociosDeSe.ToEnumerado(filtroMismaSociedad.IdNegocio);
                if (vincularCon.UsaCg())
                {
                    var elemento = (IElementoDeProcesoDtm)vincularCon.LeerRegistro(Contexto, filtroMismaSociedad.IdElemento);
                    var idsoci = elemento.Cg(Contexto).IdSociedad;
                    consulta = consulta.Where(ppt => ppt.Cg.IdSociedad == idsoci);
                }
            }

            foreach (var filtro in filtros)
            {
                if (filtro.Aplicado) continue;
                consulta = consulta.FiltroSiHayDependenciaDe(ltrDeUnPresupuesto.DependeDeExpediente, filtro, nameof(PresupuestoDtm.IdExpediente));

                if (filtro.Clausula.Equals(ltrDeUnPresupuesto.PptDeUnExpediente, StringComparison.CurrentCultureIgnoreCase) ||
                    filtro.Clausula.Equals(ltrDeUnPresupuesto.PptDeUnaFactura, StringComparison.CurrentCultureIgnoreCase) ||
                    filtro.Clausula.Equals(ltrDeUnPresupuesto.PptDeUnPartTr, StringComparison.CurrentCultureIgnoreCase) ||
                    filtro.Clausula.Equals(ltrDeUnPresupuesto.PptDeUnaAsignacionPtr, StringComparison.CurrentCultureIgnoreCase))
                {
                    filtro.Criterio = enumCriteriosDeFiltrado.porReferencia;
                    consulta = consulta.AplicarFiltroDeCadena(filtro);
                }

            }
            return consulta;
        }


        protected override IQueryable<PresupuestoDtm> AplicarSeguridad(IQueryable<PresupuestoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<PresupuestoDtm, TipoDePresupuestoDtm, PermisoDelPresupuestoDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<PresupuestoDtm, PermisoDelPresupuestoDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void ValidarPermisosDePersistencia(PresupuestoDtm ppt, ParametrosDeNegocio parametros)
        {
            base.ValidarPermisosDePersistencia(ppt, parametros);

            if (parametros.Insertando)
                return;

            if (parametros.EsUnaTransicion) return;

            if (parametros.AccionQueSeEjecuta != ltrDeUnElemento.Accion_Renombrar && parametros.AccionQueSeEjecuta != ltrDeUnPresupuesto.AsociarExpediente)
            {
                if (!ppt.Etapas().Contains(enumEtapasDePpts.PPT_Etapa_Elaboracion))
                    GestorDeErrores.Emitir($"No se puede modificar el presupuesto '{ppt.Referencia}' ya que no está en la etapa de  '{enumEtapasDePpts.PPT_Etapa_Elaboracion}'");
            }
            else
            {
                if (parametros.AccionQueSeEjecuta == ltrDeUnElemento.Accion_Renombrar)
                {
                    if (!ppt.EsInterventor(Contexto))
                    {
                        GestorDeErrores.Emitir($@"Para renombra el presupuesto '{ppt.Referencia}' necesita permisos de intervención");
                    }

                    if (!ppt.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePpts> { enumEtapasDePpts.PPT_Etapa_Pendiente, enumEtapasDePpts.PPT_Etapa_Aceptado, enumEtapasDePpts.PPT_Etapa_PermiteFacturar, enumEtapasDePpts.PPT_Etapa_AsociarParteTr }))
                    {
                        GestorDeErrores.Emitir($@"El presupuesto '{ppt.Referencia}' solo se puede renombrar en las etapas: '{enumEtapasDePpts.PPT_Etapa_Pendiente.Nombre(minusculas: false)}', '{enumEtapasDePpts.PPT_Etapa_Aceptado.Nombre(minusculas: false)}', '{enumEtapasDePpts.PPT_Etapa_PermiteFacturar.Nombre(minusculas: false)}', '{enumEtapasDePpts.PPT_Etapa_AsociarParteTr.Nombre(minusculas: false)}'");
                    }
                }

                if (parametros.AccionQueSeEjecuta == ltrDeUnPresupuesto.AsociarExpediente)
                {
                    if (!ppt.EsInterventor(Contexto))
                    {
                        GestorDeErrores.Emitir($@"Para asociar el presupuesto '{ppt.Referencia}' necesita permisos de intervención");
                    }
                    if (ppt.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePpts> { enumEtapasDePpts.PPT_Etapa_Cancelado }))
                    {
                        GestorDeErrores.Emitir($@"El presupuesto '{ppt.Referencia}' está en la etapa '{enumEtapasDePpts.PPT_Etapa_Cancelado.Nombre(minusculas: false)}', no se le puede asociar un expediente");
                    }

                }
            }
        }

        protected override void AntesDePersistir(PresupuestoDtm ppt, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(ppt, parametros);
            ppt.ClaseDePresupuesto = ((TipoDePresupuestoDtm)parametros.TipoConFujo).ClaseDePresupuesto;

            ValidarAsociarExpediente(ppt, parametros);
        }

        protected override void DespuesDePersistir(PresupuestoDtm ppt, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(ppt, parametros);

            if (parametros.Insertando && ppt.ClaseDePresupuesto == enumClaseDePresupuesto.venta)
            {
                var direccion = ppt.IdExpediente.Entero() > 0 ? ppt.CopiarDireccionDelExpediente(Contexto, enumCalificadorDireccion.contacto) : null;
                if (direccion is null)
                    ppt.CopiarDireccionDelSolicitante(Contexto, enumCalificadorDireccion.contacto);
            }

            if (!parametros.Parametros.LeerValor(ltrDeUnPresupuesto.TrazaDelCambioDeExpediente, "").IsNullOrEmpty())
                new TrazasDeUnPresupuestoDtm
                {
                    IdElemento = ppt.Id,
                    Nombre = parametros.Modificando ? "Modificación del presupuesto" : "Copia de un Presupuesto",
                    Descripcion = (string)parametros.Parametros[ltrDeUnPresupuesto.TrazaDelCambioDeExpediente]
                }
                .Insertar(Contexto);

            TrazaDeAsignacionDeResponsable(ppt, parametros);
        }

        protected override void EliminarCaches(PresupuestoDtm presupuesto, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(presupuesto, parametros);
            if (presupuesto.PropiedadCambiada<int?>(nameof(PresupuestoDtm.IdExpediente), parametros))
            {
                var indice = presupuesto.IdExpediente is null ? ((PresupuestoDtm)parametros.registroEnBd).IdExpediente.ToString() : presupuesto.IdExpediente.ToString();
                ServicioDeCaches.EliminarElemento(CacheDe.Exp_TienePresupuestos, indice);
                ServicioDeCaches.EliminarElemento(CacheDe.Exp_Ingresos, indice);
                ServicioDeCaches.EliminarElemento(CacheDe.Exp_Presupuestado, indice);
            }
        }


        protected override PresupuestoDtm AntesDeTransitar(PresupuestoDtm ppt, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            ppt = base.AntesDeTransitar(ppt, transicion, parametros);
            var destino = (EstadoDtm)parametros[ltrParametrosNeg.EstadoDestino];
            if (destino.Terminado && !enumEtapasDePpts.PPT_Etapa_Rechazo.Lista().Contains(destino.Id))
                ppt.AntesDeCerrar(Contexto);

            if (((EstadoDtm)parametros[ltrParametrosNeg.EstadoDestino]).Cancelado)
                ppt.AntesDeCanelar(Contexto);

            if (transicion.EntreEtapas(enumEtapasDePpts.PPT_Etapa_Elaboracion.Estados(), enumEtapasDePpts.PPT_Etapa_Pendiente.Estados()))
                ppt.AntesDeEntregarElPpt(Contexto);

            if (transicion.EntreEtapas(enumEtapasDePpts.PPT_Etapa_Aceptado.Estados(), enumEtapasDePpts.PPT_Etapa_Elaboracion.Estados()) &&
                transicion.EntreEtapas(enumEtapasDePpts.PPT_Etapa_PermiteFacturar.Estados(), enumEtapasDePpts.PPT_Etapa_Elaboracion.Estados()))
                ppt.AntesDeDevolverUnPptFacturable(Contexto);

            if (transicion.EntreEtapas(enumEtapasDePpts.PPT_Etapa_Aceptado.Estados(), enumEtapasDePpts.PPT_Etapa_Elaboracion.Estados()) &&
                transicion.EntreEtapas(enumEtapasDePpts.PPT_Etapa_AsociarParteTr.Estados(), enumEtapasDePpts.PPT_Etapa_Elaboracion.Estados()))
                ppt.AntesDeDevolverUnPptEjecutable(Contexto);

            if (transicion.EntreEtapas(enumEtapasDePpts.PPT_Etapa_Aceptado.Estados(), enumEtapasDePpts.PPT_Etapa_Rechazo.Estados()) &&
                transicion.EntreEtapas(enumEtapasDePpts.PPT_Etapa_PermiteFacturar.Estados(), enumEtapasDePpts.PPT_Etapa_Rechazo.Estados()))
                ppt.AntesDeDevolverUnPptFacturable(Contexto);

            if (transicion.EntreEtapas(enumEtapasDePpts.PPT_Etapa_Aceptado.Estados(), enumEtapasDePpts.PPT_Etapa_Rechazo.Estados()) &&
                transicion.EntreEtapas(enumEtapasDePpts.PPT_Etapa_AsociarParteTr.Estados(), enumEtapasDePpts.PPT_Etapa_Rechazo.Estados()))
                ppt.AntesDeDevolverUnPptEjecutable(Contexto);

            return ppt;
        }

        protected override PresupuestoDtm DespuesDeTransitar(PresupuestoDtm ppt, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            ppt = base.DespuesDeTransitar(ppt, transicion, parametros);

            if (transicion.EntreEtapas(enumEtapasDePpts.PPT_Etapa_Pendiente.Estados(), enumEtapasDePpts.PPT_Etapa_Aceptado.Estados()))
                ppt.TrasAceptarElPresupuesto(Contexto);

            return ppt;
        }

        protected override void DespuesDeMapearElElemento(PresupuestoDtm ppt, PresupuestoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(ppt, elemento, parametros);
            elemento.TotalSinIva = ppt.Total(Contexto, conIva: false);
            elemento.TotalConIva = ppt.Total(Contexto, conIva: true);

            if (parametros.LeerDatosParaElGridOParaExportar)
            {
                elemento.Direcciones = string.Join($"{Simbolos.Coma} ", ppt.Direcciones(Contexto).Where(x => x.Activo).Select(x => x.Calle));
                elemento.IrAlExpediente = ppt.IdExpediente is null ? "" : ppt.Expediente(Contexto).Referencia;
            }

            if (parametros.Peticion == enumPeticion.epLeerPorId ||
                (parametros.Filtros is not null && parametros.Filtros.Exists(x => x.Clausula.ToLower() == nameof(PresupuestoDtm.IdExpediente).ToLower())))
            {
                if (parametros.Parametros.LeerValor(nameof(PresupuestoDto.IdTipoFacturaPorDefecto), false))
                {
                    var tipoPpt = ppt.Tipo<TipoDePresupuestoDtm>(Contexto);
                    if (tipoPpt.IdTipoFacturaEmt != default)
                    {
                        var tipoFactura = Contexto.SeleccionarPorId<TipoDeFacturaEmtDtm>((int)tipoPpt.IdTipoFacturaEmt);
                        elemento.IdTipoFacturaPorDefecto = tipoFactura.Id;
                        elemento.TipoFacturaPorDefecto = tipoFactura.Nombre;
                    }
                }

                if (parametros.Parametros.LeerValor<bool>(nameof(PresupuestoDto.IdTipoPartePorDefecto), false))
                {
                    var tipoPpt = ppt.Tipo<TipoDePresupuestoDtm>(Contexto);
                    if (tipoPpt.IdTipoParteTr != default)
                    {
                        var tipoParte = Contexto.SeleccionarPorId<TipoDeParteTrDtm>((int)tipoPpt.IdTipoParteTr);
                        elemento.IdTipoPartePorDefecto = tipoParte.Id;
                        elemento.TipoPartePorDefecto = tipoParte.Nombre;
                    }
                }

                elemento.Etapas = ppt.ListaDeEtapas();
                if (!elemento.Etapas.Contains(enumEtapasDePpts.PPT_Etapa_Elaboracion.ToString()))
                {
                    elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                    if (elemento.Etapas.Contains(enumEtapasDePpts.PPT_Etapa_PermiteFacturar.ToString()) && !ApiDePermisos.EsInterventorDelTipo(Contexto, enumNegocio.Presupuesto, ppt))
                        elemento.Etapas.Remove(enumEtapasDePpts.PPT_Etapa_PermiteFacturar.ToString());
                }
                elemento.Ejecutado = ppt.Ejecutado(Contexto);
                elemento.Ejecutando = ppt.Ejecutando(Contexto);
                elemento.Prefacturado = ppt.Prefacturado(Contexto);
                elemento.Facturado = ppt.Facturado(Contexto);
            }

            if (parametros.LeerDatosParaElGridOParaExportar)
            {
                if (parametros.ColumnasDelGrid.Contains(nameof(PresupuestoDto.Ejecutado).ToLower()))
                    elemento.Ejecutado = ppt.Ejecutado(Contexto);

                if (parametros.ColumnasDelGrid.Contains(nameof(PresupuestoDto.Ejecutando).ToLower()))
                    elemento.Ejecutando = ppt.Ejecutando(Contexto);

                if (parametros.ColumnasDelGrid.Contains(nameof(PresupuestoDto.Prefacturado).ToLower()))
                    elemento.Prefacturado = ppt.Prefacturado(Contexto);

                if (parametros.ColumnasDelGrid.Contains(nameof(PresupuestoDto.Facturado).ToLower()))
                    elemento.Facturado = ppt.Facturado(Contexto);
            }

        }

        private void TrazaDeAsignacionDeResponsable(PresupuestoDtm ppt, ParametrosDeNegocio parametros)
        {
            TrazasDeUnPresupuestoDtm traza = null;
            if (parametros.Insertando && ppt.IdResponsable != null)
            {
                traza = new TrazasDeUnPresupuestoDtm
                {
                    IdElemento = ppt.Id,
                    Nombre = "Asignación de responsable",
                    Descripcion = $"El usuario {Contexto.DatosDeConexion.Login} le ha asignado " +
                    $"como responsable a {Contexto.SeleccionarPorId<UsuarioDtm>((int)ppt.IdResponsable).Expresion}"

                };
            }

            if (parametros.Modificando)
            {
                if (ppt.PropiedadCambiada<int?>((PresupuestoDtm)parametros.registroEnBd, nameof(PresupuestoDtm.IdResponsable)))
                {
                    traza = (PresupuestoDtm)parametros.registroEnBd == null
                        ? TrazaDeAsignacion(ppt)
                        : ppt.IdResponsable == null
                        ? TrazaDeDesasignacion(ppt)
                        : TrazaDeReasignacion(ppt, parametros);
                }
                else
                if (ppt.AsignarValor<int?>((PresupuestoDtm)parametros.registroEnBd, nameof(PresupuestoDtm.IdResponsable)) &&
                    ppt.IdResponsable != Contexto.DatosDeConexion.IdUsuario)
                {
                    traza = TrazaDeAsignacion(ppt);
                }
                else
                if (ppt.QuitarValor<int?>((PresupuestoDtm)parametros.registroEnBd, nameof(PresupuestoDtm.IdResponsable)) &&
                    ppt.IdResponsable != Contexto.DatosDeConexion.IdUsuario)
                {
                    traza = TrazaDeDesasignacion(ppt);
                }
            }

            if (traza != null)
            {
                traza.Insertar(Contexto);
                var email = ppt.IdResponsable == null
                    ? GestorDeUsuarios.LeerUsuario(Contexto, (int)((PresupuestoDtm)parametros.registroEnBd).IdResponsable).eMail
                    : GestorDeUsuarios.LeerUsuario(Contexto, (int)ppt.IdResponsable).eMail;
                var adjunto = new TipoDtoElmento { TipoDto = Negocio.TipoDto().FullName, IdElemento = ppt.Id, Referencia = ppt.Referencia };

                if (Contexto.DatosDeConexion.IdUsuario == ppt.IdResponsable.Entero())
                    return;

                GestorDeCorreos.CrearCorreoPara(Contexto
                    , new List<string> { email }
                    , "Asignación de presupuesto"
                    , "Le informamos que se le ha asignado un presupuesto para su elaboración"
                    , new List<TipoDtoElmento>() { adjunto }
                    , new List<string>());
            }
        }

        private TrazasDeUnPresupuestoDtm TrazaDeDesasignacion(PresupuestoDtm ppt)
        {
            return new TrazasDeUnPresupuestoDtm
            {
                IdElemento = ppt.Id,
                Nombre = "Anulación como responsable",
                Descripcion = $"El usuario {Contexto.DatosDeConexion.Login} le ha desasignado como responsable del presupuesto"
            };
        }

        private TrazasDeUnPresupuestoDtm TrazaDeAsignacion(PresupuestoDtm ppt)
        {
            return new TrazasDeUnPresupuestoDtm
            {
                IdElemento = ppt.Id,
                Nombre = "Asignación de responsable",
                Descripcion = $"El usuario {Contexto.DatosDeConexion.Login} le ha asignado como responsable del presupuesto"
            };
        }

        private TrazasDeUnPresupuestoDtm TrazaDeReasignacion(PresupuestoDtm ppt, ParametrosDeNegocio parametros)
        {
            return new TrazasDeUnPresupuestoDtm
            {
                IdElemento = ppt.Id,
                Nombre = "Reasignación de responsable",
                Descripcion = $"El usuario {Contexto.DatosDeConexion.Login} a desasignado al responsable" +
                                    $"{Contexto.SeleccionarPorId<UsuarioDtm>((int)((PresupuestoDtm)parametros.registroEnBd).IdResponsable).Expresion}" +
                                    $"y ha asignado a {Contexto.SeleccionarPorId<UsuarioDtm>((int)ppt.IdResponsable).Expresion}"

            };
        }

        private void ValidarAsociarExpediente(PresupuestoDtm ppt, ParametrosDeNegocio parametros)
        {
            if (ppt.IdExpediente.Entero() == 0)
            {
                ppt.IdExpediente = null;
                return;
            }

            if (ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Expediente, (int)ppt.IdExpediente) == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"No se puede persistir el {Negocio.Singular(true)} por no tener acceso al {enumNegocio.Expediente.Singular(true)} indicado");

            bool cambiandoExpediente = parametros.Modificando
                  && ppt.PropiedadCambiada<int?>((PresupuestoDtm)parametros.registroEnBd, nameof(PresupuestoDtm.IdExpediente));

            if (parametros.Insertando || cambiandoExpediente || parametros.Eliminando)
            {
                if (Contexto.SeleccionarPorId<CentroGestorDtm>(Contexto.SeleccionarPorId<ExpedienteDtm>((int)ppt.IdExpediente).IdCg).IdSociedad
                    != Contexto.SeleccionarPorId<CentroGestorDtm>(ppt.IdCg).IdSociedad)
                    GestorDeErrores.Emitir("Las sociedades del expediente y el presupuesto han de ser la misma");

                if (!ExtensorDeElementos.EstaEnEtapa(Contexto, enumNegocio.Expediente, (int)ppt.IdExpediente, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos.Estados()))
                    GestorDeErrores.Emitir("No se puede asociar el presupuesto por no estar el expediente a asociar en la etapa de asociar un presupuesto");
            }

            if (cambiandoExpediente && ((PresupuestoDtm)parametros.registroEnBd).IdExpediente != null)
            {
                var modoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Presupuesto, ppt.Id);
                if (!ModoDeAcceso.HayPermisosDe(modoDeAcceso, enumModoDeAccesoDeDatos.Interventor))
                    GestorDeErrores.Emitir("Para un cambio de expediente se necesitan permisos de intervención");

                var idAnterior = (int)((PresupuestoDtm)parametros.registroEnBd).IdExpediente;

                if (ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Expediente, idAnterior) == enumModoDeAccesoDeDatos.SinPermiso)
                    GestorDeErrores.Emitir("No se puede modificar el presupuesto por no tener acceso al expediente asociado");

                if (!ExtensorDeElementos.EstaEnEtapa(Contexto, enumNegocio.Expediente, idAnterior, enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos.Estados()))
                    GestorDeErrores.Emitir("No se puede desasociar el presupuesto por no estar el expediente asociado en la etapa de desasociar un presupuesto");

                parametros.Parametros[ltrDeUnPresupuesto.TrazaDelCambioDeExpediente] = $"Se ha susutituido el expediente {Contexto.SeleccionarPorId<ExpedienteDtm>(idAnterior).Referencia} por el expediente {Contexto.SeleccionarPorId<ExpedienteDtm>((int)ppt.IdExpediente).Referencia}";
            }
        }

        public static int CopiarPpt(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (!parametros.ContieneClave(nameof(CopiarPptDto.IdElemento))) GestorDeErrores.Emitir("No se ha indicado el presupuesto a copiar");
            if (!parametros.ContieneClave(nameof(CopiarPptDto.IdTipo))) GestorDeErrores.Emitir("No se ha indicado el tipo del nuevo presupuesto");
            if (!parametros.ContieneClave(nameof(CopiarPptDto.IdCg))) GestorDeErrores.Emitir("No se ha indicado el cg del nuevo presupuesto");
            if (!parametros.ContieneClave(nameof(CopiarPptDto.IdSolicitante))) GestorDeErrores.Emitir("No se ha indicado el solicitante del nuevo presupuesto");
            if (!parametros.ContieneClave(nameof(CopiarPptDto.Nombre))) GestorDeErrores.Emitir("No se ha indicado el nombre del nuevo presupuesto");
            if (!parametros.ContieneClave(nameof(CopiarPptDto.Descripcion))) GestorDeErrores.Emitir("No se ha indicado la descripción del nuevo presupuesto");
            var pptOrigen = contexto.SeleccionarPorId<PresupuestoDtm>((int)(long)parametros[nameof(CopiarPptDto.IdElemento)]);
            return pptOrigen.Copiar(contexto, parametros).Id;
        }

        public Task<TotalesDePresupuestos> ObtenerTotalesAsync(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            return Task.Run(() => ObtenerTotales(filtros, posicion, cantidad));
        }

        public TotalesDePresupuestos ObtenerTotales(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad)
        {
            var presupuestos = Contexto.SeleccionarTodos<PresupuestoDtm>(filtros, parametros: new Dictionary<string, object> {
                { ltrParametrosNeg.PosicionInicial, posicion},
                { ltrParametrosNeg.CantidadPorLeer, cantidad},
                { ltrParametrosNeg.Peticion, enumPeticion.epTotales},
            });
            var totales = new TotalesDePresupuestos();
            totales.Bi = presupuestos.Sum(ppt => ppt.Total(Contexto, conIva: false));
            totales.Iva = presupuestos.Sum(ppt => ppt.Total(Contexto, conIva: true)) - totales.Bi;
            totales.Presupuestado = totales.Bi + totales.Iva;

            var pptsEnCumplimentacion = presupuestos.Where(ppt => ppt.EstaEnLaEtapa(enumEtapasDePpts.PPT_Etapa_Elaboracion)).ToList();
            var pptsPdtDelCliente = presupuestos.Where(ppt => ppt.EstaEnLaEtapa(enumEtapasDePpts.PPT_Etapa_Pendiente)).ToList();

            totales.Facturado = presupuestos.Sum(ppt => ppt.Facturado(Contexto));
            totales.PptSinFacturas = presupuestos.Where(ppt => !ppt.TieneFacturas(Contexto)
                   && !pptsEnCumplimentacion.Any(p => p.Id == ppt.Id)
                   && !pptsPdtDelCliente.Any(p => p.Id == ppt.Id)
            ).Sum(ppt => ppt.Total(Contexto, conIva: false));

            totales.EnElaboracion = pptsEnCumplimentacion.Sum(ppt => ppt.Total(Contexto, conIva: false));
            totales.PdtDelCliente = pptsPdtDelCliente.Sum(ppt => ppt.Total(Contexto, conIva: false));


            totales.Facturable = totales.Bi - totales.EnElaboracion - totales.PdtDelCliente - totales.Facturado;

            totales.Procesados = presupuestos.Count();
            return totales;
        }

    }
}
