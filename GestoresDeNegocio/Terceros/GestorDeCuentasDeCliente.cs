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

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeCuentasDeCliente : GestorDeElementos<ContextoSe, CuentaDeClienteDtm, CuentaDeClienteDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrCuentasDeCliente
        {
        }

        public class MapearCuentasDeCliente : Profile
        {
            public MapearCuentasDeCliente()
            {
                CreateMap<CuentaDeClienteDtm, CuentaDeClienteDto>()
                .ForMember(dto => dto.Entidad, x => x.MapFrom(dtm => dtm.Cuenta.Entidad))
                .ForMember(dto => dto.Oficina, x => x.MapFrom(dtm => dtm.Cuenta.Oficina))
                .ForMember(dto => dto.DcCcc, x => x.MapFrom(dtm => dtm.Cuenta.DcCcc))
                .ForMember(dto => dto.Numero, x => x.MapFrom(dtm => dtm.Cuenta.Numero));
                CreateMap<CuentaDeClienteDto, CuentaDeClienteDtm>()
                .ForMember(dtm => dtm.Cuenta, x => x.Ignore())
                .ForMember(dtm => dtm.Elemento, x => x.Ignore());
            }
        }

        public GestorDeCuentasDeCliente(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<CuentaDeClienteDtm> AplicarJoins(IQueryable<CuentaDeClienteDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Cuenta);
            return consulta;
        }

        public static GestorDeCuentasDeCliente Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCuentasDeCliente(contexto, mapeador);
        }

        protected override IQueryable<CuentaDeClienteDtm> AplicarFiltros(IQueryable<CuentaDeClienteDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == nameof(ltrParametrosEp.negocio).ToLower());
            if (filtro !=  null)
            {
                filtro.Aplicado = true;
            }
            return consulta;
        }

        protected override void DespuesDeMapearElRegistro(CuentaDeClienteDto ccDto, CuentaDeClienteDtm ccDtm, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(ccDto, ccDtm, opciones);

            if (opciones.Insertando)
                ccDtm.IdCuenta = 
                    ExtensorDeCuentasBancarias.Leer(Contexto, ccDto.Iban.Substring(0, 2), ccDto.Iban.Substring(2, 2), ccDto.Entidad, ccDto.Oficina, ccDto.DcCcc, ccDto.Numero).Id;
        }

        protected override void AntesDePersistir(CuentaDeClienteDtm cc, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(cc, parametros);
            if (parametros.Insertando) cc.Activa = true;
            if (parametros.Operacion == enumTipoOperacion.Modificar)
            {
                cc.IdCuenta = ((CuentaDeClienteDtm)parametros.registroEnBd).IdCuenta;
                if (cc.IdArchivo is null)
                    cc.IdArchivo = ((CuentaDeClienteDtm)parametros.registroEnBd).IdArchivo;

                if (cc.SeHaModificadoElCampo<bool>(x => x.Name == nameof(CuentaDeClienteDtm.Activa), parametros))
                    cc.IdArchivo = ((CuentaDeClienteDtm)parametros.registroEnBd).IdArchivo;

                cc.CuentaBancaria(Contexto).Validar();

            }
        }

        protected override void DespuesDePersistir(CuentaDeClienteDtm cc, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(cc, parametros);

            if (parametros.Insertando)
                ((ClienteDtm)cc.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha añadido una cuenta al cliente", $"La cuenta {cc.CuentaBancaria(Contexto).NumeroIban} ha sido añadido por el usuario {Contexto.DatosDeConexion.Login}");


            if (parametros.Modificando && cc.SeHaModificadoElCampo<bool>(x => x.Name == nameof(CuentaDeClienteDtm.Activa), parametros))
                ((ClienteDtm)cc.DetalleDe(Contexto)).CrearTraza(Contexto, $"{(cc.Activa ? "Activada " : "Desactivada")} la cuenta del cliente", $"La cuenta {cc.CuentaBancaria(Contexto).NumeroIban} ha sido {(cc.Activa ? "activada" : "desactivada")} por el usuario {Contexto.DatosDeConexion.Login}");

            if (parametros.Eliminando)
                ((ClienteDtm)cc.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha eliminado la cuenta del cliente", $"La cuenta {cc.CuentaBancaria(Contexto).NumeroIban} ha sido eliminada por el usuario {Contexto.DatosDeConexion.Login}");

            if (cc.IdArchivo != default)
            {
                if (parametros.Insertando || (parametros.Modificando && !GestorDeVinculos.Existe(Contexto, enumNegocio.Cliente, enumNegocio.Archivos, cc.IdElemento, (int)cc.IdArchivo)))
                {
                    cc.AsignarNombreAlCertificado(Contexto, enumNegocio.Cliente, cc.IdElemento, parametros.Modificando ? (IUsaCuentaBancaria)parametros.registroEnBd : null);
                    ((ClienteDtm)cc.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha añadido el certificado de cuenta", $"El certificado de cuenta {cc.CuentaBancaria(Contexto).NumeroIban} ha sido añadido por el usuario {Contexto.DatosDeConexion.Login}");
                }

                if (parametros.Eliminando && GestorDeVinculos.Existe(Contexto, enumNegocio.Cliente, enumNegocio.Archivos, cc.IdElemento, (int)cc.IdArchivo))
                {
                    ((ClienteDtm)cc.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha eliminado la cuenta del cliente", $"La cuenta {cc.CuentaBancaria(Contexto).NumeroIban} ha sido eliminada por el usuario {Contexto.DatosDeConexion.Login}");
                    GestorDeVinculos.BorrarVinculo(Contexto, enumNegocio.Cliente, enumNegocio.Archivos, cc.IdElemento, (int)cc.IdArchivo, new Dictionary<string, object>());
                }
            }
        }

        protected override void EliminarCaches(CuentaDeClienteDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            ServicioDeCaches.EliminarCache(CacheDe.Pag_DatosDelPagoDto);
        }

        protected override void DespuesDeMapearElElemento(CuentaDeClienteDtm cc, CuentaDeClienteDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(cc, elemento, parametros);

            elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Cliente, cc.DetalleDe(Contexto));
            cc.Cuenta = cc.CuentaBancaria(Contexto);
            elemento.Iban = cc.Cuenta.IsoPais + cc.Cuenta.DcIban;
            elemento.Entidad = cc.Cuenta.Entidad;
            elemento.Oficina = cc.Cuenta.Oficina;
            elemento.DcCcc = cc.Cuenta.DcCcc;
            elemento.Numero = cc.Cuenta.Numero;
            elemento.NombreArchivo = cc.IdArchivo is null ? ltrCuentasBancarias.SinCertificado : Contexto.SeleccionarPorId<ArchivoDtm>((int)cc.IdArchivo).Expresion;
            elemento.Banco = cc.Cuenta.Banco(Contexto, errorSiNoHay: false).Expresion(Contexto);
            elemento.Elemento = Contexto.SeleccionarPorId<ClienteDtm>(cc.IdElemento).Expresion;

        }

    }
}
