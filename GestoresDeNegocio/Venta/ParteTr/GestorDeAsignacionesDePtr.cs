using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ModeloDeDto.Ventas;
using ServicioDeDatos.Ventas;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Gestor.Errores;
using System;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Elemento;
using System.Text;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Seguridad;

namespace GestoresDeNegocio.Ventas
{
    public class GestorDeAsignacionesDePtr : GestorDeElementos<ContextoSe, AsignacionDePtrDtm, AsignacionDePtrDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrAsignacionesDePtrDeUnUnitarios
        {
        }

        public class MapearAsignacionesDePtrDeUnitarios : Profile
        {
            public MapearAsignacionesDePtrDeUnitarios()
            {
                CreateMap<AsignacionDePtrDtm, AsignacionDePtrDto>()
                .ForMember(dto => dto.Trabajador, dtm => dtm.MapFrom(dtm => dtm.Trabajador.Expresion))
                .ForMember(dto => dto.Elemento, dtm => dtm.MapFrom(dtm => dtm.Elemento.Expresion))
                .ForMember(dto => dto.LtrMedidoEn, dtm => dtm.MapFrom(dtm => dtm.MedidoEn.ToString()));
                CreateMap<AsignacionDePtrDto, AsignacionDePtrDtm>()
                .ForMember(dtm => dtm.Trabajador, dto => dto.Ignore())
                .ForMember(dtm => dtm.Elemento, dto => dto.Ignore());
            }
        }

        public GestorDeAsignacionesDePtr(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeAsignacionesDePtr Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeAsignacionesDePtr(contexto, mapeador);
        }

        protected override IQueryable<AsignacionDePtrDtm> AplicarJoins(IQueryable<AsignacionDePtrDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Trabajador);
            consulta = consulta.Include(p => p.Elemento);
            return consulta;
        }

        protected override IQueryable<AsignacionDePtrDtm> AplicarFiltros(IQueryable<AsignacionDePtrDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            if (parametros.Parametros.LeerValor(ltrParametrosEp.filtrarPara, "") == ltrDeUnaAsignacion.MostrarLasPendientes)
            {
                filtros.Add(new ClausulaDeFiltrado(nameof(AsignacionDePtrDtm.Finalizada), enumCriteriosDeFiltrado.esNulo));
            }

            consulta = consulta.FiltroPorTrabajador(filtros);

            var filtro = filtros.FirstOrDefault(x => x.Clausula == nameof(INombre.Nombre).ToLower());
            if (filtro != null) consulta = consulta.FiltroPorNombreDeTrabajador(Contexto, filtro);

            consulta = consulta.FiltroPorParteTr(filtros);
            consulta = consulta.FiltroPorCliente(Contexto, filtros);
            consulta = consulta.FiltroPorUnitario(Contexto, filtros);
            consulta = consulta.FiltroPorContrato(filtros);
            consulta = consulta.FiltroPorPresupuesto(filtros);
            consulta = consulta.FiltroPorEstadoDeLaAsignacion(filtros);

            return consulta;
        }

        protected override IQueryable<AsignacionDePtrDtm> AplicarSeguridad(IQueryable<AsignacionDePtrDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);

            if (Contexto.DatosDeConexion.EsAdministrador)
                return consulta;


            var idNegocioPtr = enumNegocio.ParteDeTrabajo.IdNegocio();

            //solo son visibles las asignaciones de partes de trabajo de las que de su tipo soy gestor
            consulta = consulta.Where(x => (Contexto.Set<PermisosPorTipoDtm>().Any(y => y.IdTipo.Equals(x.Elemento.IdTipo)
                                           && y.IdNegocio.Equals(idNegocioPtr)
                                           && y.IdUsuario.Equals(Contexto.DatosDeConexion.IdUsuario)
                                           && Contexto.Set<TipoDeParteTrDtm>().Any(t => t.IdPermisoDeGestor.Equals(y.IdPermiso)
                                                                                         && t.Id.Equals(x.Elemento.IdTipo)))

            //solo son visibles las asignaciones de trabajo de las que de su centro gestor para partes soy gestor
                                           && Contexto.Set<PermisosPorCgDtm>().Any(y => y.IdCg.Equals(x.Elemento.IdCg)
                                           && y.IdNegocio.Equals(idNegocioPtr)
                                           && y.IdUsuario.Equals(Contexto.DatosDeConexion.IdUsuario)
                                           && Contexto.Set<NegociosDeUnCgDtm>().Any(t => t.IdGestor.Equals(y.IdPermiso)
                                                                                && t.IdCg.Equals(x.Elemento.IdCg)
                                                                                && t.IdNegocio.Equals(idNegocioPtr))))
                                           ||
           //o los partes asignados a mi
                                           (
                                           Contexto.Set<TrabajadorDtm>().Any(tra => tra.IdUsuario == Contexto.DatosDeConexion.IdUsuario
                                                                             && tra.Id == x.IdTrabajador)
                                           ));

            return consulta;
        }

        protected override void ValidarPermisosDePersistencia(AsignacionDePtrDtm asignacion, ParametrosDeNegocio parametros)
        {
            //Si el parte está en la etapa de pendiente, y el usuario conectado corresponde con el trabajador asignado al parte
            //entonces no validamos los permisos
            var parte = (ParteTrDtm)asignacion.DetalleDe(Contexto);

            if (!parte.EstaEnLaEtapa(enumEtapasDePartesTr.PTR_Etapa_Pendiente))
                GestorDeErrores.Emitir($"Los datos de asignación del parte '{parte.Referencia}' no son modificables ya que se ha realizado");

            var trabajador = Contexto.SeleccionarPorId<TrabajadorDtm>(asignacion.IdTrabajador);
            if (parte.EstaEnLaEtapa(enumEtapasDePartesTr.PTR_Etapa_Pendiente) && trabajador.IdUsuario.Entero() == Contexto.DatosDeConexion.IdUsuario)
                parametros.ValidarPermisosDePersistencia = false;

            base.ValidarPermisosDePersistencia(asignacion, parametros);
        }

        protected override void AntesDePersistir(AsignacionDePtrDtm asignacion, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(asignacion, parametros);

            asignacion.Negocio.ValidarUsaDetalleDe(typeof(AsignacionDePtrDtm));

            var parte = asignacion.DetalleDe<ParteTrDtm>(Contexto);
            if (!parametros.Eliminando)
            {
                var trabajador = Contexto.SeleccionarPorId<TrabajadorDtm>(asignacion.IdTrabajador);
                if (Contexto.SeleccionarPorId<CentroGestorDtm>(parte.IdCg).IdSociedad != Contexto.SeleccionarPorId<CentroGestorDtm>(trabajador.IdCg).IdSociedad)
                    GestorDeErrores.Emitir("Solo puede asignar un parte a un trabajador de la misma sociedad que el parte");
            }

            if (parte.EstaEnLaEtapa(enumEtapasDePartesTr.PTR_Etapa_Facturado))
                GestorDeErrores.Emitir("Un parte facturado no permite modificaciones");

            if (parte.EstaEnLaEtapa(enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar) && !asignacion.Informada)
                GestorDeErrores.Emitir("Para grabar la asignación de un parte facturado ha de informar de todos sus datos");

            if (asignacion.PlfDeInicio != default && asignacion.PlfDeFin == default || asignacion.PlfDeInicio == default && asignacion.PlfDeFin != default)
                GestorDeErrores.Emitir("La planificación ha de ser completa, con fecha inicial y final");

            if (asignacion.PlfDeFin != default && asignacion.PlfDeInicio != default && asignacion.PlfDeInicio > asignacion.PlfDeFin)
                GestorDeErrores.Emitir("La planificación inicial ha de ser anterior a la final");

            if (asignacion.Finalizada != default && asignacion.Iniciada != default && asignacion.Iniciada > asignacion.Finalizada)
                GestorDeErrores.Emitir("La fecha de inicio ha de ser anterior a la de finalización");

            if (asignacion.Duracion.HasValue && !asignacion.MedidoEn.HasValue)
                GestorDeErrores.Emitir("Si indica la duración ha de indicar en que se mide");

            if (!asignacion.Duracion.HasValue && asignacion.MedidoEn.HasValue)
                GestorDeErrores.Emitir("Si no indica la duración no ha de indicar en que se mide");

        }

        protected override void DespuesDePersistir(AsignacionDePtrDtm asignacion, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(asignacion, parametros);
            asignacion.PersistirEvento(Contexto, parametros);
        }

        protected override void DespuesDeMapearElRegistro(AsignacionDePtrDto elemento, AsignacionDePtrDtm registro, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(elemento, registro, opciones);
            if (elemento.CalcularTiempo)
            {
                if (registro.Finalizada == default || registro.Iniciada == default)
                    GestorDeErrores.Emitir("La fecha de inicio y fin han de indicarse para poder calcular la duración");

                if (registro.Finalizada != default && registro.Iniciada != default && registro.Iniciada > registro.Finalizada)
                    GestorDeErrores.Emitir("La fecha de inicio ha de ser anterior a la de finalización");

                registro.Duracion = (decimal)((DateTime)elemento.Finalizada - (DateTime)elemento.Iniciada).TotalHours;
                registro.MedidoEn = ServicioDeDatos.Elemento.Enumerados.enumDurabilidad.Horas;
            }
        }

        protected override void DespuesDeMapearElElemento(AsignacionDePtrDtm asignacion, AsignacionDePtrDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(asignacion, elemento, parametros);
            var parte = asignacion.DetalleDe<ParteTrDtm>(Contexto, aplicarJoin: true);
            elemento.IdCg = parte.IdCg;
            elemento.Cg = parte.Cg.Expresion;
        }

        protected override void ObtenerModoDeAccesoAlElementoQueSeDevuelve(AsignacionDePtrDtm asignacion, AsignacionDePtrDto elemento, ParametrosDeNegocio parametros)
        =>
        elemento.ModoDeAcceso = ((ParteTrDtm)asignacion.DetalleDe(Contexto)).ModoDeAccesoAlParteTr(Contexto);

        public static StringBuilder DarPorRealizadasHoy(ContextoSe contexto, List<int> list)
        {
            StringBuilder resultado = new StringBuilder();
            foreach (var id in list)
            {
                var asignacion = contexto.SeleccionarPorId<AsignacionDePtrDtm>(id, aplicarJoin: true);
                if (asignacion.Iniciada is not null && asignacion.Finalizada is not null)
                {
                    resultado.AppendLine($"La asignación del parte '{asignacion.Elemento.Referencia}' al trabajador '{asignacion.Trabajador.NIF(contexto)}' ya estaba realizada");
                    continue;
                }
                asignacion.Finalizada = DateTime.Now;
                if (asignacion.Iniciada is null) asignacion.Iniciada = ((DateTime)asignacion.Finalizada).AddHours(-VariableDePartesTr.HorasDeJornada.Entero());
                var transaccion = contexto.IniciarTransaccion();
                try
                {
                    asignacion.Modificar(contexto);
                    resultado.AppendLine($"La asignación del parte '{asignacion.Elemento.Referencia}' al trabajador '{asignacion.Trabajador.NIF(contexto)}' se ha actualizado");
                    contexto.Commit(transaccion);
                }
                catch (Exception e)
                {
                    contexto.Rollback(transaccion);
                    resultado.AppendLine($"No se ha actualizado la asignación del parte '{asignacion.Elemento.Referencia}' al trabajador '{asignacion.Trabajador.NIF(contexto)}'{Environment.NewLine}{e.Message}");
                }
            }
            return resultado;
        }

        public static StringBuilder DarPorRealizadasSegunPlan(ContextoSe contexto, List<int> list)
        {
            StringBuilder resultado = new StringBuilder();
            foreach (var id in list)
            {
                var asignacion = contexto.SeleccionarPorId<AsignacionDePtrDtm>(id, aplicarJoin: true);
                if (asignacion.Iniciada is not null && asignacion.Finalizada is not null)
                {
                    resultado.AppendLine($"La asignación del parte '{asignacion.Elemento.Referencia}' al trabajador '{asignacion.Trabajador.NIF(contexto)}' ya estaba realizada");
                    continue;
                }
                if (asignacion.Iniciada is null) asignacion.Iniciada = asignacion.PlfDeInicio;
                asignacion.Finalizada = asignacion.PlfDeFin;
                var transaccion = contexto.IniciarTransaccion();
                try
                {
                    asignacion.Modificar(contexto);
                    resultado.AppendLine($"La asignación del parte '{asignacion.Elemento.Referencia}' al trabajador '{asignacion.Trabajador.NIF(contexto)}' se ha actualizado");
                    contexto.Commit(transaccion);
                }
                catch (Exception e)
                {
                    contexto.Rollback(transaccion);
                    resultado.AppendLine($"No se ha actualizado la asignación del parte '{asignacion.Elemento.Referencia}' al trabajador '{asignacion.Trabajador.NIF(contexto)}'{Environment.NewLine}{e.Message}");
                }
            }
            return resultado;
        }

        public static StringBuilder AplicarDatosDeEjecucion(ContextoSe contexto, List<int> ids, Dictionary<string, object> parametros)
        {
            StringBuilder resultado = new StringBuilder();
            var iniciada = parametros.LeerValor<DateTime>(nameof(FechasDeEjecucionDto.Iniciada));
            var finalizada = parametros.LeerValor(nameof(FechasDeEjecucionDto.Terminada), (DateTime?)null);

            if (finalizada is not null && ((DateTime)finalizada).Ticks - iniciada.Ticks <= 0)
                GestorDeErrores.Emitir($"La fecha y hora de inicio debe ser anterior a la de finalización");

            foreach (var id in ids)
            {
                var asignacion = contexto.SeleccionarPorId<AsignacionDePtrDtm>(id, aplicarJoin: true);
                if (asignacion.Iniciada is not null && asignacion.Finalizada is not null)
                {
                    resultado.AppendLine($"La asignación del parte '{asignacion.Elemento.Referencia}' al trabajador '{asignacion.Trabajador.NIF(contexto)}' ya estaba realizada");
                    continue;
                }
                if (asignacion.Iniciada is null)
                    asignacion.Iniciada = iniciada;
                asignacion.Finalizada = finalizada;
                var transaccion = contexto.IniciarTransaccion();
                try
                {
                    asignacion.Modificar(contexto);
                    resultado.AppendLine($"La asignación del parte '{asignacion.Elemento.Referencia}' al trabajador '{asignacion.Trabajador.NIF(contexto)}' se ha actualizado");
                    contexto.Commit(transaccion);
                }
                catch (Exception e)
                {
                    contexto.Rollback(transaccion);
                    resultado.AppendLine($"No se ha actualizado la asignación del parte '{asignacion.Elemento.Referencia}' al trabajador '{asignacion.Trabajador.NIF(contexto)}'{Environment.NewLine}{e.Message}");
                }
            }
            return resultado;
        }
    }
}
