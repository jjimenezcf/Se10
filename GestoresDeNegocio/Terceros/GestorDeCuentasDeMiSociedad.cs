using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Contabilidad;
using GestorDeElementos.Extensores;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Ventas;
using ModeloDeDto;
using Gestor.Errores;
using ServicioDeDatos.Elemento;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeCuentasDeMiSociedad : GestorDeElementos<ContextoSe, CuentaDeMiSociedadDtm, CuentaDeMiSociedadDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public static class ltrCuentasDeMiSociedad
        {
            public static string DarDeBajaCuenta = nameof(DarDeBajaCuenta);
        }

        public class MapearCuentasDeMiSociedad : Profile
        {
            public MapearCuentasDeMiSociedad()
            {
                CreateMap<CuentaDeMiSociedadDtm, CuentaDeMiSociedadDto>()
                .ForMember(dto => dto.Entidad, x => x.MapFrom(dtm => dtm.Cuenta.Entidad))
                .ForMember(dto => dto.Oficina, x => x.MapFrom(dtm => dtm.Cuenta.Oficina))
                .ForMember(dto => dto.DcCcc, x => x.MapFrom(dtm => dtm.Cuenta.DcCcc))
                .ForMember(dto => dto.Numero, x => x.MapFrom(dtm => dtm.Cuenta.Numero));
                CreateMap<CuentaDeMiSociedadDto, CuentaDeMiSociedadDtm>()
                .ForMember(dtm => dtm.Cuenta, x => x.Ignore())
                .ForMember(dtm => dtm.Elemento, x => x.Ignore());
            }
        }

        public GestorDeCuentasDeMiSociedad(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeCuentasDeMiSociedad Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCuentasDeMiSociedad(contexto, mapeador);
        }

        protected override IQueryable<CuentaDeMiSociedadDtm> AplicarJoins(IQueryable<CuentaDeMiSociedadDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Cuenta);
            return consulta;
        }

        protected override IQueryable<CuentaDeMiSociedadDtm> AplicarFiltros(IQueryable<CuentaDeMiSociedadDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            var filtroPorSociedadCg = filtros.FirstOrDefault(x => x.Clausula.Equals(nameof(ElementoDeUnProcesoDto.IdSociedadDelCg).ToLower(), System.StringComparison.CurrentCultureIgnoreCase));
            if (filtroPorSociedadCg != null)
            {
                consulta = consulta.Where(x => x.IdElemento == filtroPorSociedadCg.Valor.Entero() && x.Activa == true);
                filtroPorSociedadCg.Aplicado = true;
            }

            var filtroPorCg = filtros.FirstOrDefault(x => x.Clausula.Equals(nameof(ProveedorDto.CgPropuesto).ToLower(), System.StringComparison.CurrentCultureIgnoreCase));
            if (filtroPorCg != null)
            {
                consulta = consulta.Where(x => x.IdElemento == Contexto.Set<CentroGestorDtm>().First(cg => cg.Id == filtroPorCg.Valor.Entero()).IdSociedad);
                filtroPorCg.Aplicado = true;
            }

            var filtroPorNegocio = filtros.FirstOrDefault(x => x.Clausula.Equals(ltrParametrosNeg.Negocio, System.StringComparison.CurrentCultureIgnoreCase));
            if (filtroPorNegocio != null)
            {
                filtroPorNegocio.Aplicado = true;
                if (ApiDeEnsamblados.ToEnumerado<enumNegocio>(filtroPorNegocio.Valor) == enumNegocio.FacturaEmitida)
                {
                    var fltFactura = filtros.FirstOrDefault(x => x.Clausula.Equals(ltrParametrosEp.idElemento, System.StringComparison.CurrentCultureIgnoreCase));
                    if (fltFactura != null)
                    {
                        fltFactura.Aplicado = true;
                        var factura = Contexto.SeleccionarPorId<FacturaEmtDtm>(fltFactura.Valor.Entero());
                        var idSociedad = factura.Cg(Contexto).IdSociedad;
                        consulta = consulta.Where(x => x.IdElemento.Equals(idSociedad) && x.Activa == true);
                    }
                }
            }

            var filtroPorClase = filtros.FirstOrDefault(x => x.Clausula.Equals(nameof(CuentaDeMiSociedadDtm.Clase).ToLower(), System.StringComparison.CurrentCultureIgnoreCase));
            if (filtroPorClase != null)
            {
                if (filtroPorClase.Valor != enumClaseDeCuentaBancaria.Ambas.ToString())
                    consulta = consulta.Where(x => x.Clase == enumClaseDeCuentaBancaria.Ambas || x.Clase == ApiDeEnsamblados.ToEnumerado<enumClaseDeCuentaBancaria>(filtroPorClase.Valor));
                filtroPorClase.Aplicado = true;
            }

            return consulta;
        }

        protected override void DespuesDeMapearElRegistro(CuentaDeMiSociedadDto csDto, CuentaDeMiSociedadDtm csDtm, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(csDto, csDtm, opciones);
            if (opciones.Insertando)
                csDtm.IdCuenta =
                    ExtensorDeCuentasBancarias.Leer(Contexto, csDto.Iban.Substring(0, 2), csDto.Iban.Substring(2, 2), csDto.Entidad, csDto.Oficina, csDto.DcCcc, csDto.Numero).Id;
        }

        protected override void AntesDePersistir(CuentaDeMiSociedadDtm cc, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(cc, parametros);
            if (parametros.Insertando)
            {
                cc.Activa = true;
                if (cc.IdCuenta == 0)
                    GestorDeErrores.Emitir($"Debe indicar un número de cuenta para la sociedad '{Contexto.SeleccionarPorId<SociedadDtm>(cc.IdElemento).NIF}'");
                cc.Cuenta(Contexto).Validar();
            }

            if (parametros.Operacion == enumTipoOperacion.Modificar)
            {
                cc.IdCuenta = ((CuentaDeMiSociedadDtm)parametros.registroEnBd).IdCuenta;
                if (parametros.EstaDesactivando(cc))
                    cc.ValidarQueSonIguales((CuentaDeMiSociedadDtm)parametros.registroEnBd, excepto: new List<string> { nameof(IUsaActiva.Activa), nameof(CuentaDeMiSociedadDtm.FechaModificacion), nameof(CuentaDeMiSociedadDtm.IdUsuaModi) });
                else if (parametros.EstaActivando(cc))
                    cc.ValidarQueSonIguales((CuentaDeMiSociedadDtm)parametros.registroEnBd, excepto: new List<string> { nameof(CuentaDeMiSociedadDtm.IdArchivo), nameof(CuentaDeMiSociedadDtm.Alias), nameof(IUsaActiva.Activa), nameof(CuentaDeMiSociedadDtm.FechaModificacion), nameof(CuentaDeMiSociedadDtm.IdUsuaModi) });
                else
                    cc.ValidarQueSonIguales((CuentaDeMiSociedadDtm)parametros.registroEnBd, excepto: new List<string> { nameof(CuentaDeMiSociedadDtm.IdArchivo), nameof(CuentaDeMiSociedadDtm.Alias), nameof(CuentaDeMiSociedadDtm.FechaModificacion), nameof(CuentaDeMiSociedadDtm.IdUsuaModi) });
            }

        }


        protected override void DespuesDePersistir(CuentaDeMiSociedadDtm cs, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(cs, parametros);

            if (parametros.Insertando)
                ((SociedadDtm)cs.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha añadido una cuenta a la sociedad", $"La cuenta {cs.Cuenta(Contexto).NumeroIban} ha sido añadido por el usuario {Contexto.DatosDeConexion.Login}");

            if (parametros.EstaActivando(cs) || parametros.EstaDesactivando(cs))
                ((SociedadDtm)cs.DetalleDe(Contexto)).CrearTraza(Contexto, $"{(cs.Activa ? "Activada " : "Desactivada")} la cuenta de la sociedad", $"La cuenta {cs.Cuenta(Contexto).NumeroIban} ha sido {(cs.Activa ? "activada" : "desactivada")} por el usuario {Contexto.DatosDeConexion.Login}");

            if (parametros.EstaDesactivando(cs))
            {
                foreach(var tarjeta in Contexto.SeleccionarTodos<TarjetaDeMiSociedadDtm>(nameof(TarjetaDeMiSociedadDtm.IdElemento), cs.IdElemento).Where(tarjeta => tarjeta.Activa == true).ToList())
                {
                    tarjeta.Activa = false;
                    tarjeta.Modificar(Contexto, accionEjecutada: ltrCuentasDeMiSociedad.DarDeBajaCuenta);
                }
            }

            if (parametros.Eliminando)
                ((SociedadDtm)cs.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha eliminado la cuenta de la sociedad", $"La cuenta {cs.Cuenta(Contexto).NumeroIban} ha sido eliminada por el usuario {Contexto.DatosDeConexion.Login}");

            if (cs.IdArchivo != default)
            {
                if (parametros.Insertando || (parametros.Modificando && !GestorDeVinculos.Existe(Contexto, enumNegocio.Sociedad, enumNegocio.Archivos, cs.IdElemento, (int)cs.IdArchivo)))
                {
                    cs.AsignarNombreAlCertificado(Contexto, enumNegocio.Sociedad , cs.IdElemento, parametros.Modificando ? (IUsaCuentaBancaria)parametros.registroEnBd : null);
                    ((SociedadDtm)cs.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha añadido el certificado de cuenta", $"El certificado de cuenta {cs.Cuenta(Contexto).NumeroIban} ha sido añadido por el usuario {Contexto.DatosDeConexion.Login}");
                }

                if (parametros.Eliminando && GestorDeVinculos.Existe(Contexto, enumNegocio.Sociedad, enumNegocio.Archivos, cs.IdElemento, (int)cs.IdArchivo))
                {
                    ((SociedadDtm)cs.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha eliminado la cuenta de la sociedad", $"La cuenta {cs.Cuenta(Contexto).NumeroIban} ha sido eliminada por el usuario {Contexto.DatosDeConexion.Login}");
                    GestorDeVinculos.BorrarVinculo(Contexto, enumNegocio.Sociedad, enumNegocio.Archivos, cs.IdElemento, (int)cs.IdArchivo, new Dictionary<string, object>());
                }
            }
        }

        protected override void EliminarCaches(CuentaDeMiSociedadDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            ServicioDeCaches.EliminarCache(CacheDe.Pag_DatosDelPagoDto);
        }

        protected override void DespuesDeMapearElElemento(CuentaDeMiSociedadDtm ctaDeSociedad, CuentaDeMiSociedadDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(ctaDeSociedad, elemento, parametros);

            if (parametros.CargarListaDeElementos || parametros.LeerPorId)
                elemento.Expresion = ctaDeSociedad.Expresion(Contexto);

            elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Sociedad, ctaDeSociedad.DetalleDe(Contexto));
            ctaDeSociedad.Cuenta = ctaDeSociedad.Cuenta(Contexto);
            elemento.Iban = ctaDeSociedad.Cuenta.IsoPais + ctaDeSociedad.Cuenta.DcIban;
            elemento.NombreArchivo = ctaDeSociedad.IdArchivo is null ? ltrCuentasBancarias.SinCertificado : Contexto.SeleccionarPorId<ArchivoDtm>((int)ctaDeSociedad.IdArchivo).Expresion;
            var banco = ctaDeSociedad.Cuenta.Banco(Contexto, errorSiNoHay: false);
            elemento.Banco = banco is not null ? banco.Expresion : ltrBanco.BancoNoDefinido;
            elemento.Elemento = Contexto.SeleccionarPorId<SociedadDtm>(ctaDeSociedad.IdElemento).Expresion;
        }

    }
}
