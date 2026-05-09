using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using System;
using Microsoft.EntityFrameworkCore;
using Gestor.Errores;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Ventas;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Guarderias;
using GestoresDeNegocio.Seguridad;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeTrabajadores : GestorDeElementos<ContextoSe, TrabajadorDtm, TrabajadorDto>
    {
        public override enumNegocio Negocio => enumNegocio.Trabajador;

        public class MapearTrabajadores : Profile
        {
            public MapearTrabajadores()
            {
                CreateMap<TrabajadorDtm, TrabajadorDto>()
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion));
                CreateMap<TrabajadorDto, TrabajadorDtm>()
                .ForMember(dtm => dtm.Usuario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdCuenta, dto => dto.Ignore());
            }
        }

        public GestorDeTrabajadores(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeTrabajadores Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTrabajadores(contexto, mapeador); ;
        }

        protected override IQueryable<TrabajadorDtm> AplicarJoins(IQueryable<TrabajadorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(e => e.Interlocutor);
            consulta = consulta.Include(e => e.Cg);
            return consulta;
        }

        protected override IQueryable<TrabajadorDtm> AplicarFiltros(IQueryable<TrabajadorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltrarPorSociedadDelTrabajador(filtros);
            consulta = consulta.FiltrarPorPersona(filtros);
            consulta = consulta.FiltrarPorExpresion(filtros);
            return consulta;
        }

        protected override IQueryable<TrabajadorDtm> AplicarSeguridad(IQueryable<TrabajadorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador) consulta = FiltrarPorSeguridad.DeNegocio(Contexto, Negocio, consulta);
            return consulta;
        }

        protected override void DespuesDeMapearElRegistro(TrabajadorDto elemento, TrabajadorDtm trabajador, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(elemento, trabajador, opciones);
            trabajador.IdCuenta = elemento.IdCuenta.Entero() == 0
            ? Contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Sueldos).Id
            : (int)elemento.IdCuenta;
        }

        protected override void AntesDePersistir(TrabajadorDtm trabajador, ParametrosDeNegocio parametros)
        {
            var sociedad = Contexto.SeleccionarPorId<SociedadDtm>(Contexto.SeleccionarPorId<CentroGestorDtm>(trabajador.IdCg).IdSociedad);
            trabajador.Nombre = $"{sociedad.CodigoFiscal}: {Contexto.SeleccionarPorId<InterlocutorDtm>(trabajador.IdInterlocutor).Expresion}";
            Contexto.Entry(trabajador).Property(x => x.Nombre).IsModified = true;
            base.AntesDePersistir(trabajador, parametros);
            if (trabajador.Cg(Contexto, aplicarJoin: true).Sociedad.CodigoFiscal.IsNullOrEmpty())
                GestorDeErrores.Emitir($"La sociedad '{trabajador.Cg(Contexto, aplicarJoin: true).Sociedad.Referencia}' debe tener indicado un código fiscal para poder dar de alta o modificar trabajadores");
            ValidarQueElTrabajadorNoEstaEnLaSociedad(trabajador, parametros);
        }

        protected override void DespuesDePersistir(TrabajadorDtm trabajador, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(trabajador, parametros);

            if (trabajador.PropiedadCambiada<int?>(nameof(TrabajadorDtm.IdUsuario), parametros))
            {
                var idUsuarioAnterior = ((TrabajadorDtm)parametros.registroEnBd).IdUsuario;
                var idUsuarioNuevo = trabajador.IdUsuario;
                if (idUsuarioAnterior != null || idUsuarioNuevo != null)
                {
                    var cursosDondeEsResponsable = Contexto.SeleccionarTodos<CursoDeGuarderiaDtm>(nameof(CursoDeGuarderiaDtm.IdTrabajador), trabajador.Id);
                    foreach (var curso in cursosDondeEsResponsable)
                    {
                        var puestoGestor = curso.PuestoDeGestor(Contexto);
                        if (idUsuarioAnterior != null) puestoGestor.DesasociarUsuario(Contexto, (int)idUsuarioAnterior);
                        if (idUsuarioNuevo != null) puestoGestor.AsociarUsuario(Contexto, (int)idUsuarioNuevo);
                    }
                    var cursosDondeEsAyudante = Contexto.SeleccionarTodos<ProfeDeCursoDeGuarderiaDtm>(nameof(ProfeDeCursoDeGuarderiaDtm.IdTrabajador), trabajador.Id);
                    foreach (var apoyoEn in cursosDondeEsAyudante)
                    {
                        var curso = apoyoEn.DetalleDe<CursoDeGuarderiaDtm>(Contexto);
                        var puestoConsultor = curso.PuestoDeConsultor(Contexto);
                        if (idUsuarioAnterior != null) puestoConsultor.DesasociarUsuario(Contexto, (int)idUsuarioAnterior);
                        if (idUsuarioNuevo != null) puestoConsultor.AsociarUsuario(Contexto, (int)idUsuarioNuevo);
                    }
                }
            }
            if (parametros.Modificando) trabajador.TrazarModificaciones(Contexto, (TrabajadorDtm)parametros.registroEnBd);
        }

        protected override void EliminarCaches(TrabajadorDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            ServicioDeCaches.EliminarCache(CacheDe.Pag_DatosDelPagoDto);
            ServicioDeCaches.EliminarElemento(CacheDe.Int_Trabajador, registro.IdInterlocutor.ToString());
        }

        protected override void AlDarDeAlta(TrabajadorDtm trabajador, ParametrosDeNegocio parametros)
        {
            base.AlDarDeAlta(trabajador, parametros);

            if (trabajador.Interlocutor(Contexto).Baja)
                GestorDeErrores.Emitir($"Debe dar de alta el interlocutor '{trabajador.Interlocutor(Contexto).Expresion(Contexto)}' antes de dar de alta el trabajador");
        }

        protected override void AlDarDeBaja(TrabajadorDtm trabajador, ParametrosDeNegocio parametros)
        {
            base.AlDarDeBaja(trabajador, parametros);

            var filtros = new Dictionary<string, object> {
                        { nameof(AsignacionDePtrDtm.IdTrabajador), trabajador.Id },
                        { nameof(AsignacionDePtrDtm.Finalizada), enumCriteriosDeFiltrado.noEsNulo }
                    };
            var asignaciones = Contexto.SeleccionarTodos<AsignacionDePtrDtm>(filtros);
            if (asignaciones.Count() > 0)
                GestorDeErrores.Emitir($"No se puede dar de baja el trabajador '{trabajador.Referencia(Contexto)}' con partes de trabajo pendientes");
        }

        protected override void DespuesDeMapearElElemento(TrabajadorDtm trabajador, TrabajadorDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(trabajador, elemento, parametros);
            if (parametros.Peticion == enumPeticion.epLeerPorId && trabajador.IdUsuario != default)
                elemento.Usuario = trabajador.Usuario(Contexto).Expresion;
        }
        private void ValidarQueElTrabajadorNoEstaEnLaSociedad(TrabajadorDtm trabajador, ParametrosDeNegocio parametros)
        {
            if (parametros.Modificando || parametros.Insertando)
            {
                var idSociedad = Contexto.SeleccionarPorId<CentroGestorDtm>(trabajador.IdCg).IdSociedad;
                var trabajadores = Contexto.SeleccionarTodos<TrabajadorDtm>(nameof(TrabajadorDtm.IdInterlocutor), trabajador.IdInterlocutor);
                foreach (var t in trabajadores)
                {
                    if (parametros.Modificando && t.Id == trabajador.Id)
                        continue;

                    var cgDelT = Contexto.SeleccionarPorId<CentroGestorDtm>(t.IdCg);
                    if (cgDelT.IdSociedad == idSociedad)
                        GestorDeErrores.Emitir($"No se puede dar de alta al trabajador {trabajador.Expresion} por estar ya dado de alta en el CG {cgDelT.Expresion}");
                }
            }
        }

        public static List<TrabajadorDtm> CrearTrabajadores(ContextoSe contexto, CentroGestorDtm cg, List<int> idsDeInter, int idCuenta)
        {
            var result = new List<TrabajadorDtm>();
            foreach (var idInter in idsDeInter)
            {
                result.Add(CrearTrabajador(contexto, cg.Id, idInter, idCuenta));
            }
            return result;
        }

        public static TrabajadorDtm CrearTrabajador(ContextoSe contexto, int idCg, string nif, int idCuenta)
        {
            var inter = ExtensorDeInterlocutores.CrearInterlocutor(contexto, nif);
            return CrearTrabajador(contexto, idCg, inter, idCuenta);
        }

        public static TrabajadorDtm CrearTrabajador(ContextoSe contexto, int idCg, int idInter, int idCuenta)
        =>
        CrearTrabajador(contexto, idCg, contexto.SeleccionarPorId<InterlocutorDtm>(idInter), idCuenta);

        public static TrabajadorDtm CrearTrabajador(ContextoSe contexto, int idCg, InterlocutorDtm inter, int idCuenta)
        {
            var ak = new Dictionary<string, object> { { nameof(TrabajadorDtm.IdCg), idCg }, { nameof(TrabajadorDtm.IdInterlocutor), inter.Id } };
            var Trabajador = contexto.SeleccionarPorAk<TrabajadorDtm>(ak, errorSiNoHay: false);

            if (Trabajador != null)
                return Trabajador;

            if (inter.Baja)
                GestorDeErrores.Emitir($"No se puede dar de alta un Trabajador por estar de baja el interlocutor");

            if (Trabajador == null)
            {
                Trabajador = new TrabajadorDtm();
                Trabajador.IdCg = idCg;
                Trabajador.IdInterlocutor = inter.Id;
                Trabajador.Nombre = inter.Expresion;
                Trabajador.Telefono = inter.Telefono;
                Trabajador.eMail = inter.eMail;
                Trabajador.IdCuenta = idCuenta;
                Trabajador = Trabajador.Insertar(contexto);
            }

            return Trabajador;
        }
    }


}
