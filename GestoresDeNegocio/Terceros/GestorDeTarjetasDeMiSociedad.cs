using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using GestorDeElementos.Extensores;
using Gestor.Errores;
using ServicioDeDatos.Elemento;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeTarjetasDeMiSociedad : GestorDeElementos<ContextoSe, TarjetaDeMiSociedadDtm, TarjetaDeMiSociedadDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrTarjetasDeMiSociedad
        {
        }

        public class MapearTarjetasDeMiSociedad : Profile
        {
            public MapearTarjetasDeMiSociedad()
            {
                CreateMap<TarjetaDeMiSociedadDtm, TarjetaDeMiSociedadDto>()
                .ForMember(dto => dto.CuentaDeCargo, x => x.MapFrom(dtm => dtm.CuentaDeCargo == null || dtm.CuentaDeCargo.Cuenta == null ? null : dtm.CuentaDeCargo.Cuenta.NumeroIban));
                CreateMap<TarjetaDeMiSociedadDto, TarjetaDeMiSociedadDtm>()
                .ForMember(dtm => dtm.CuentaDeCargo, x => x.Ignore())
                .ForMember(dtm => dtm.Expresion, x => x.Ignore())
                .ForMember(dtm => dtm.Elemento, x => x.Ignore());
            }
        }

        public GestorDeTarjetasDeMiSociedad(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeTarjetasDeMiSociedad Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTarjetasDeMiSociedad(contexto, mapeador);
        }

        protected override IQueryable<TarjetaDeMiSociedadDtm> AplicarJoins(IQueryable<TarjetaDeMiSociedadDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.CuentaDeCargo).ThenInclude(x => x.Cuenta);
            return consulta;
        }

        protected override IQueryable<TarjetaDeMiSociedadDtm> AplicarFiltros(IQueryable<TarjetaDeMiSociedadDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            var filtroPorCg = filtros.FirstOrDefault(x => x.Clausula.Equals(nameof(IUsaCg.Cg).ToLower(), System.StringComparison.CurrentCultureIgnoreCase) 
                 || x.Clausula.Equals(nameof(ProveedorDto.CgPropuesto).ToLower(), System.StringComparison.CurrentCultureIgnoreCase));
            if (filtroPorCg != null)
            {
                consulta = consulta.Where(x => x.IdElemento == Contexto.Set<CentroGestorDtm>().First(cg => cg.Id == filtroPorCg.Valor.Entero()).IdSociedad);
                filtroPorCg.Aplicado = true;
            }

            var filtroPorActiva = filtros.FirstOrDefault(x => x.Clausula.Equals(nameof(IUsaActiva.Activa).ToLower(), System.StringComparison.CurrentCultureIgnoreCase));
            if (filtroPorActiva != null)
            {
                consulta = consulta.Where(x => x.Activa == true && Contexto.Set<CuentaDeMiSociedadDtm>().Any(cuenta => cuenta.Activa == true && cuenta.Id == x.IdCuentaDeCargo));
                filtroPorActiva.Aplicado = true;
            }

            return consulta;
        }

        protected override void DefinirOrden(List<ClausulaDeOrdenacion> orden, ParametrosDeNegocio parametros)
        {
            base.DefinirOrden(orden, parametros);
            if (orden.Count() == 1 && orden.Any(o => o.OrdenarPor == nameof(RegistroDtm.Id) || o.OrdenarPor == nameof(IDetalle.IdElemento)))
            {
                orden.Clear();
                orden.Add(new ClausulaDeOrdenacion(nameof(TarjetaDeMiSociedadDtm.Activa), ModoDeOrdenancion.ascendente));
                orden.Add(new ClausulaDeOrdenacion(nameof(TarjetaDeMiSociedadDtm.Numero), ModoDeOrdenancion.ascendente));
            }
        }

        protected override void AntesDePersistir(TarjetaDeMiSociedadDtm tarjeta, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(tarjeta, parametros);
            if (parametros.Insertando)
            {
                if (!tarjeta.CuentaDeCargo(Contexto).Activa)
                    GestorDeErrores.Emitir($"la cuenta de cargo '{tarjeta.IbanDeCuentaDeCargo(Contexto)}' de la sociedad '{Contexto.SeleccionarPorId<SociedadDtm>(tarjeta.IdElemento).NIF}' no está activa");

                tarjeta.Activa = true;
                if (tarjeta.IdCuentaDeCargo == 0)
                    GestorDeErrores.Emitir($"Debe indicar la cuenta de cargo de la sociedad '{Contexto.SeleccionarPorId<SociedadDtm>(tarjeta.IdElemento).NIF}'");
            }

            if (parametros.EstaActivando(tarjeta) && !tarjeta.CuentaDeCargo(Contexto).Activa)
                GestorDeErrores.Emitir($"La cuenta de cargo '{tarjeta.IbanDeCuentaDeCargo(Contexto)}' de la sociedad '{Contexto.SeleccionarPorId<SociedadDtm>(tarjeta.IdElemento).NIF}' no está activa");

            if (parametros.Modificando)
            {
                tarjeta.IdCuentaDeCargo = ((TarjetaDeMiSociedadDtm)parametros.registroEnBd).IdCuentaDeCargo;

                if (parametros.EstaDesactivando(tarjeta) )
                    tarjeta.ValidarQueSonIguales((TarjetaDeMiSociedadDtm)parametros.registroEnBd,
                        excepto: new List<string> {
                            nameof(IUsaActiva.Activa),
                            nameof(TarjetaDeMiSociedadDtm.FechaModificacion),
                            nameof(TarjetaDeMiSociedadDtm.IdUsuaModi)
                        });
                else if (parametros.EstaActivando(tarjeta))
                    tarjeta.ValidarQueSonIguales((TarjetaDeMiSociedadDtm)parametros.registroEnBd,
                        excepto: new List<string> {
                            nameof(IUsaActiva.Activa),
                            nameof(TarjetaDeMiSociedadDtm.Alias),
                            nameof(TarjetaDeMiSociedadDtm.FechaModificacion),
                            nameof(TarjetaDeMiSociedadDtm.IdUsuaModi)
                        });
                else
                    tarjeta.ValidarQueSonIguales((TarjetaDeMiSociedadDtm)parametros.registroEnBd,
                        excepto: new List<string> {
                            nameof(TarjetaDeMiSociedadDtm.Alias),
                            nameof(TarjetaDeMiSociedadDtm.Numero),
                            nameof(TarjetaDeMiSociedadDtm.FechaModificacion),
                            nameof(TarjetaDeMiSociedadDtm.IdUsuaModi) });
            }

        }

        protected override void DespuesDePersistir(TarjetaDeMiSociedadDtm tarjeta, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(tarjeta, parametros);

            if (parametros.Insertando)
                ((SociedadDtm)tarjeta.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha añadido una tarjeta a la sociedad", $"La tarjeta {tarjeta.IbanDeCuentaDeCargo(Contexto)} ha sido añadido por el usuario {Contexto.DatosDeConexion.Login}");

            if (parametros.EstaActivando(tarjeta) || parametros.EstaDesactivando(tarjeta))
                ((SociedadDtm)tarjeta.DetalleDe(Contexto)).CrearTraza(Contexto, $"{(tarjeta.Activa ? "Activada " : "Desactivada")} la tarjeta de la sociedad", $"La tarjeta {tarjeta.IbanDeCuentaDeCargo(Contexto)} ha sido {(tarjeta.Activa ? "activada" : "desactivada")} por el usuario {Contexto.DatosDeConexion.Login}");

            if (parametros.Eliminando)
                ((SociedadDtm)tarjeta.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha eliminado la tarjeta de la sociedad", $"La tarjeta {tarjeta.IbanDeCuentaDeCargo(Contexto)} ha sido eliminada por el usuario {Contexto.DatosDeConexion.Login}");
        }

        protected override void EliminarCaches(TarjetaDeMiSociedadDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros); 
            ServicioDeCaches.EliminarCache(CacheDe.Pag_DatosDelPagoDto);
        }

        protected override void DespuesDeMapearElElemento(TarjetaDeMiSociedadDtm tarjeta, TarjetaDeMiSociedadDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(tarjeta, elemento, parametros);

            elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Sociedad, tarjeta.DetalleDe(Contexto));
            tarjeta.CuentaDeCargo = tarjeta.CuentaDeCargo(Contexto);
            elemento.Elemento = Contexto.SeleccionarPorId<SociedadDtm>(tarjeta.IdElemento).Expresion;
            elemento.Expresion = tarjeta.Expresion;

        }

    }
}
