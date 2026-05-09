using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ModeloDeDto.Ventas;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using GestorDeElementos.Extensores;
using Gestor.Errores;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;

namespace GestoresDeNegocio.Ventas
{

    public class GestorDePartesTr : GestorDeElementos<ContextoSe, ParteTrDtm, ParteTrDto>
    {
        public class MapearParteTr : Profile
        {
            public MapearParteTr()
            {
                CreateMap<ParteTrDtm, ParteTrDto>()
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Expresion))
                .ForMember(dto => dto.Cg, dtm => dtm.MapFrom(dtm => dtm.Cg.Expresion))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(dtm => dtm.Estado.Nombre))
                .ForMember(dto => dto.Contrato, dtm => dtm.MapFrom(dtm => dtm.Contrato.Expresion))
                .ForMember(dto => dto.FacturaEmt, dtm => dtm.MapFrom(dtm => dtm.FacturaEmt.Expresion))
                .ForMember(dto => dto.Presupuesto, dtm => dtm.MapFrom(dtm => dtm.Presupuesto.Expresion))
                .ForMember(dto => dto.Cliente, dtm => dtm.MapFrom(dtm => dtm.Cliente.Expresion));

                CreateMap<ParteTrDto, ParteTrDtm>()
                .ForMember(dtm => dtm.Cg, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tipo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.Ignore())
                .ForMember(dtm => dtm.Cliente, dto => dto.Ignore())
                .ForMember(dtm => dtm.Presupuesto, dto => dto.Ignore())
                .ForMember(dtm => dtm.FacturaEmt, dto => dto.Ignore())
                .ForMember(dtm => dtm.Contrato, dto => dto.Ignore())
                .ForMember(dtm => dtm.Total, dto => dto.Ignore());
            }
        }

        public override enumNegocio Negocio => enumNegocio.ParteDeTrabajo;
        
        public override IGestorDeTipos GestorDeTipos => GestorDeTiposDeParteTr.Gestor(Contexto, Contexto.Mapeador);

        public GestorDePartesTr(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDePartesTr Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePartesTr(contexto, mapeador);
        }

        protected override IQueryable<ParteTrDtm> AplicarJoins(IQueryable<ParteTrDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Cliente);
            consulta = consulta.Include(x => x.Contrato);
            consulta = consulta.Include(x => x.FacturaEmt);
            consulta = consulta.Include(x => x.Presupuesto);
            return consulta;
        }

        protected override IQueryable<ParteTrDtm> AplicarFiltros(IQueryable<ParteTrDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            parametros.AplicarFiltroQueMostrar = !filtros.OmitirFiltrosPorEstado(new List<string> { ltrDeUnParteTr.IdContrato, ltrDeUnParteTr.IdTarea, ltrDeUnParteTr.IdFacturaEmt, ltrDeUnParteTr.IdPlfDeVenta, ltrDeUnParteTr.IdPresupuesto });
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.AplicarFiltroPorFacturaEmt(Contexto,filtros);
            consulta = consulta.AplicarFiltroPorPpt(filtros);
            consulta = consulta.EjecutadoDeUnPpt(Contexto, filtros);
            foreach (var filtro in filtros)
            {
                if (filtro.Aplicado)
                    continue;

                consulta = consulta.FiltroSiHayDependenciaDe(ltrDeUnParteTr.DependeDePpt, filtro, nameof(ParteTrDtm.IdPresupuesto));

                consulta = consulta.AplicarFiltroPorUnitario(Contexto, filtro);
                consulta = consulta.AplicarFiltroPorContratos(filtro);
                consulta = consulta.AplicarFiltroPorTarea(Contexto, filtro);
                consulta = consulta.AplicarFiltroPorPlfDeVenta(Contexto, filtro);
            }

            //Si viene el filtro de vincularCon si es con una factura excluimos todos los partes tr que no estén pendientes
            //consulta = consulta.SeleccionarPartesTrPdtFacturar(filtros);
            //consulta = consulta.AplicarFiltroPorEtapas(filtros);

            return consulta;
        }

        protected override IQueryable<ParteTrDtm> AplicarSeguridad(IQueryable<ParteTrDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                consulta = FiltrarPorSeguridad.DeTipo<ParteTrDtm, TipoDeParteTrDtm, PermisoDelParteTrDtm>(Contexto, Negocio, consulta);
                consulta = FiltrarPorSeguridad.DeCg<ParteTrDtm, PermisoDelParteTrDtm>(Contexto, Negocio, consulta);
            }
            return consulta;
        }

        protected override void AntesDePersistir(ParteTrDtm ptr, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(ptr, parametros);
            ptr = ptr.InicializarDatosCliente(Contexto, parametros);
            ValidarAsociarPresupuesto(ptr, parametros);

            if (parametros.Modificando && !((ParteTrDtm)parametros.registroEnBd).Descripcion.Equals(ptr.Descripcion))
            {
                var asignaciones = ptr.Detalles<AsignacionDePtrDtm>(Contexto);
                foreach (var asignacion in asignaciones) asignacion.PersistirEvento(Contexto, parametros);
            }
        }

        protected override void DespuesDePersistir(ParteTrDtm ptr, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(ptr, parametros);

            if (parametros.Insertando && ptr.IdPresupuesto.Entero() > 0 && !parametros.Copiando)
                ptr.CopiarLineasDelPpt(Contexto);
        }

        protected override void EliminarCaches(ParteTrDtm ptr, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(ptr, parametros);

            if (!parametros.Insertando) ServicioDeCaches.EliminarElemento(CacheDe.PlvDeUnParteTr, ptr.Id.ToString());
            if (ptr.IdPresupuesto.Entero() > 0)
            {
                ServicioDeCaches.EliminarElemento(CacheDe.Ppt_Ejecutando, $"{ptr.IdPresupuesto}");
                ServicioDeCaches.EliminarElemento(CacheDe.Ppt_Ejecutado, $"{ptr.IdPresupuesto}");
                ServicioDeCaches.EliminarElemento(CacheDe.Ppt_Prefacturado, $"{ptr.IdPresupuesto}");
            }

            if (parametros.Modificando && ptr.PropiedadCambiada<int?>(nameof(FacturaEmtDtm.IdPresupuesto), parametros))
            {
                ServicioDeCaches.EliminarElemento(CacheDe.Ppt_TienePartesTr, $"{ptr.IdPresupuesto}");
                ServicioDeCaches.EliminarElemento(CacheDe.Ppt_TienePartesTr, $"{((FacturaEmtDtm)parametros.registroEnBd).IdPresupuesto}");
                ServicioDeCaches.EliminarElemento(CacheDe.Ppt_Ejecutando, $"{((FacturaEmtDtm)parametros.registroEnBd).IdPresupuesto}");
                ServicioDeCaches.EliminarElemento(CacheDe.Ppt_Ejecutado, $"{((FacturaEmtDtm)parametros.registroEnBd).IdPresupuesto}");
                ServicioDeCaches.EliminarElemento(CacheDe.Ppt_Prefacturado, $"{((FacturaEmtDtm)parametros.registroEnBd).IdPresupuesto}");
            }
        }

        protected override ParteTrDtm AntesDeTransitar(ParteTrDtm parte, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            parte = base.AntesDeTransitar(parte, transicion, parametros);

            if (transicion.EntreEtapas(enumEtapasDePartesTr.PTR_Etapa_Pendiente.Estados(), enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar.Estados()))
                parte.AntesDeDarPorRealizado(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar.Estados(), enumEtapasDePartesTr.PTR_Etapa_Facturado.Estados()))
                parte.AntesDePrefacturar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDePartesTr.PTR_Etapa_Facturado.Estados(), enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar.Estados()))
                parte = parte.AlCancelarPrefactura(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDePartesTr.PTR_Etapa_Pendiente.Estados(), enumEtapasDePartesTr.PTR_Etapa_Cancelado.Estados()))
                parte.AntesDeDarPorCancelada(Contexto, parametros);

            return parte;
        }

        protected override ParteTrDtm DespuesDeTransitar(ParteTrDtm parte, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            parte = base.DespuesDeTransitar(parte, transicion, parametros);

            if (transicion.EntreEtapas(enumEtapasDePartesTr.PTR_Etapa_Pendiente.Estados(), enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar.Estados()))
                parte.DespuesDeDarPorRealizado(Contexto);

            if (transicion.EntreEtapas(enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar.Estados(), enumEtapasDePartesTr.PTR_Etapa_Facturado.Estados()))
                parte = parte.DespuesDePrefacturar(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar.Estados(), enumEtapasDePartesTr.PTR_Etapa_Pendiente.Estados()))
                parte = parte.TrasDevolverAPendiente(Contexto, parametros);

            if (transicion.EntreEtapas(enumEtapasDePartesTr.PTR_Etapa_Pendiente.Estados(), enumEtapasDePartesTr.PTR_Etapa_Cancelado.Estados()))
                parte = parte.TrasCancelar(Contexto, parametros);

            return parte;
        }

        private void ValidarAsociarPresupuesto(ParteTrDtm parte, ParametrosDeNegocio parametros)
        {
            bool cambiandoDePpt = parametros.Modificando && parte.PropiedadCambiada<int?>((ParteTrDtm)parametros.registroEnBd, nameof(ParteTrDtm.IdPresupuesto));
            if (cambiandoDePpt)
                GestorDeErrores.Emitir($"No se puede modificar el {enumNegocio.Presupuesto.Singular(true)} de un {enumNegocio.ParteDeTrabajo.Singular(true)}, elimínelo o cancelelo y genere el correcto");

            if (parte.IdPresupuesto.Entero() == 0) return;

            var ppt = parte.Presupuesto(Contexto);

            if (ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Presupuesto, (int)parte.IdPresupuesto) == ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"No se puede modificar el parte por no tener acceso al presupuesto {ppt.Referencia}");

            if (Contexto.SeleccionarPorId<CentroGestorDtm>(ppt.IdCg).IdSociedad != Contexto.SeleccionarPorId<CentroGestorDtm>(parte.IdCg).IdSociedad)
                GestorDeErrores.Emitir($"Las sociedades del presupuesto {ppt.Referencia} y el parte {parte.Referencia} han de ser la misma");

            if (!parametros.EsUnaTransicion && !ppt.EstaEnLaEtapa(enumEtapasDePpts.PPT_Etapa_AsociarParteTr))
                GestorDeErrores.Emitir($"No se puede crear un {Negocio.Singular(true)} por no estar el {enumNegocio.Presupuesto.Singular(true)} en la etapa {enumEtapasDePpts.PPT_Etapa_AsociarParteTr.Nombre(true)}");
        }
            
        protected override void DespuesDeMapearElElemento(ParteTrDtm parteTr, ParteTrDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(parteTr, elemento, parametros);

            if (elemento.ModoDeAcceso.HayPermisosDe(enumModoDeAccesoDeDatos.Gestor) && !parteTr.EstaEnLaEtapa(enumEtapasDePartesTr.PTR_Etapa_Pendiente))
                elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;

            elemento.TotalSinIva = parteTr.Total(Contexto, conIva: false);
            elemento.TotalConIva = parteTr.Total(Contexto, conIva: true);

            if (parametros.Peticion == enumPeticion.epLeerPorId)
            {
                var plf = parteTr.Planificacion(Contexto);
                if (plf != null)
                {
                    elemento.IdPlfDeVenta = plf.Id;
                    elemento.PlfDeVenta = plf.Expresion;
                }
                elemento.Etapa = parteTr.Etapa();
            }
        }
    }
}
