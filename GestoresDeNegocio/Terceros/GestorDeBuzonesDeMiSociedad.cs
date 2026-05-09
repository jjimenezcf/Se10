using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using GestoresDeNegocio.Seguridad;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Entorno;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeBuzonesDeMiSociedad : GestorDeElementos<ContextoSe, BuzonDeMiSociedadDtm, BuzonDeMiSociedadDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class MapearBuzonesDeMiSociedad : Profile
        {
            public MapearBuzonesDeMiSociedad()
            {
                CreateMap<BuzonDeMiSociedadDtm, BuzonDeMiSociedadDto>();
                CreateMap<BuzonDeMiSociedadDto, BuzonDeMiSociedadDtm>()
                .ForMember(dtm => dtm.Elemento, x => x.Ignore());
            }
        }

        public static GestorDeBuzonesDeMiSociedad Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeBuzonesDeMiSociedad(contexto, mapeador);
        }

        public GestorDeBuzonesDeMiSociedad(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<BuzonDeMiSociedadDtm> AplicarSeguridad(IQueryable<BuzonDeMiSociedadDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            consulta = consulta.Where(permiso => Contexto.Set<UsuariosDeUnPermisoDtm>()
                                        .Any(permisos => permisos.IdUsuario == Contexto.DatosDeConexion.IdUsuario && permisos.IdPermiso == permiso.IdPermiso));
            return consulta;
        }

        protected override IQueryable<BuzonDeMiSociedadDtm> AplicarOrden(IQueryable<BuzonDeMiSociedadDtm> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return base.AplicarOrden(consulta, new List<ClausulaDeOrdenacion> { 
                new ClausulaDeOrdenacion(ordenarPor: nameof(BuzonDeMiSociedadDtm.Orden), modo: ModoDeOrdenancion.ascendente),
                new ClausulaDeOrdenacion(ordenarPor: nameof(BuzonDeMiSociedadDtm.Buzon), modo: ModoDeOrdenancion.ascendente) }
            );
        }

        protected override void AntesDePersistir(BuzonDeMiSociedadDtm buzon, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(buzon, parametros);
            if (parametros.Insertando)
            {
                buzon.IdPermiso = GestorDePermisos.CrearObtener(Contexto, $"{buzon.eMail}({buzon.Buzon})", enumClaseDePermiso.Buzon).Id;
            }
            if (!parametros.Eliminando)
                AsignarPermisoAlBuzon(buzon);
        }
        private void AsignarPermisoAlBuzon(BuzonDeMiSociedadDtm buzon)
        {
            var puesto = ExtensorDeSociedades.PuestoDeAdministrador(Contexto);
            new PermisosDirectosDtm
            {
                IdPuesto = puesto.Id,
                IdPermiso = buzon.IdPermiso
            }.InsertarComoAdministradorSiNoExiste(Contexto, new List<string> { nameof(PermisosDirectosDtm.IdPuesto), nameof(PermisosDirectosDtm.IdPermiso) });
            puesto.AsociarUsuario(Contexto, Contexto.Administrador().Id);
        }

        protected override void DespuesDePersistir(BuzonDeMiSociedadDtm buzon, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(buzon, parametros);

            if (parametros.Modificando && (buzon.SeHaModificadoElCampo<string>(x => x.Name == nameof(BuzonDeMiSociedadDtm.Buzon), parametros) ||
                buzon.SeHaModificadoElCampo<string>(x => x.Name == nameof(BuzonDeMiSociedadDtm.eMail), parametros)))
            {
                var permiso = Contexto.SeleccionarPorId<PermisoDtm>(((BuzonDeMiSociedadDtm)parametros.registroEnBd).IdPermiso);
                GestorDePermisos.ModificarPermisoFuncional(Contexto, Mapeador, permiso, $"{buzon.eMail}({buzon.Buzon})", enumClaseDePermiso.Buzon);
            }
        }

        protected override void EliminarCaches(BuzonDeMiSociedadDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Valores, ltrDeBuzonesDeMiSociedad.PermisosDeBuzones);
        }

        protected override void DespuesDeMapearElElemento(BuzonDeMiSociedadDtm buzon, BuzonDeMiSociedadDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(buzon, elemento, parametros);
            var sociedad = Contexto.SeleccionarPorId<SociedadDtm>(buzon.IdElemento);
            elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Sociedad, buzon.DetalleDe(Contexto));
            elemento.Elemento = sociedad.Expresion;
            elemento.Permiso = Contexto.SeleccionarPorId<PermisoDtm>(buzon.IdPermiso).Expresion;
        }

    }
}
