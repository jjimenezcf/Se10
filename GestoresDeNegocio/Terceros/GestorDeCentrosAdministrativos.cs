using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using ModeloDeDto.Ventas;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeCentrosAdministrativos : GestorDeElementos<ContextoSe, CentroAdministrativoDtm, CentroAdministrativoDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrCentrosAdministrativo
        {
        }

        public class MapearCentrosAdministrativo : Profile
        {
            public MapearCentrosAdministrativo()
            {
                CreateMap<CentroAdministrativoDtm, CentroAdministrativoDto>();
                CreateMap<CentroAdministrativoDto, CentroAdministrativoDtm>()
                .ForMember(dtm => dtm.Elemento, x => x.Ignore())
                .ForMember(dtm => dtm.Expresion, x => x.Ignore())
                .ForMember(dtm => dtm.Contacto, x => x.Ignore());
            }
        }

        public GestorDeCentrosAdministrativos(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<CentroAdministrativoDtm> AplicarJoins(IQueryable<CentroAdministrativoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            //consulta = consulta.Include(x => x.Contacto);
            return consulta;
        }

        public static GestorDeCentrosAdministrativos Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCentrosAdministrativos(contexto, mapeador);
        }

        protected override IQueryable<CentroAdministrativoDtm> AplicarFiltros(IQueryable<CentroAdministrativoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            var modo = parametros.Parametros.LeerValor("modo", "");
            if (modo == "editando")
                filtros.Add(new ClausulaDeFiltrado(nameof(CentroAdministrativoDtm.Activa), enumCriteriosDeFiltrado.igual, true));

            return base.AplicarFiltros(consulta, filtros, parametros);
        }

        protected override void AntesDePersistir(CentroAdministrativoDtm ca, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(ca, parametros);
            if (parametros.Insertando) ca.Activa = true;
            if (parametros.Operacion == enumTipoOperacion.Modificar)
            {
            }
        }

        protected override void DespuesDePersistir(CentroAdministrativoDtm ca, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(ca, parametros);

        }

        protected override void DespuesDeMapearElElemento(CentroAdministrativoDtm ca, CentroAdministrativoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(ca, elemento, parametros);

            elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Cliente, ca.DetalleDe(Contexto));
            elemento.Elemento = Contexto.SeleccionarPorId<ClienteDtm>(ca.IdElemento).Expresion;
            elemento.Contacto = Contexto.SeleccionarPorId<InterlocutorDtm>(ca.IdContacto).Expresion;
        }

    }
}
