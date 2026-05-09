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
    public class GestorDeCuentasDeProveedor : GestorDeElementos<ContextoSe, CuentaDeProveedorDtm, CuentaDeProveedorDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrCuentasDeProveedor
        {
        }

        public class MapearCuentasDeProveedor : Profile
        {
            public MapearCuentasDeProveedor()
            {
                CreateMap<CuentaDeProveedorDtm, CuentaDeProveedorDto>()
                .ForMember(dto => dto.Entidad, x => x.MapFrom(dtm => dtm.Cuenta.Entidad))
                .ForMember(dto => dto.Oficina, x => x.MapFrom(dtm => dtm.Cuenta.Oficina))
                .ForMember(dto => dto.DcCcc, x => x.MapFrom(dtm => dtm.Cuenta.DcCcc))
                .ForMember(dto => dto.Numero, x => x.MapFrom(dtm => dtm.Cuenta.Numero));
                CreateMap<CuentaDeProveedorDto, CuentaDeProveedorDtm>()
                .ForMember(dtm => dtm.Cuenta, x => x.Ignore())
                .ForMember(dtm => dtm.Elemento, x => x.Ignore());
            }
        }

        public GestorDeCuentasDeProveedor(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<CuentaDeProveedorDtm> AplicarJoins(IQueryable<CuentaDeProveedorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Cuenta);
            return consulta;
        }

        public static GestorDeCuentasDeProveedor Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCuentasDeProveedor(contexto, mapeador);
        }

        protected override void DespuesDeMapearElRegistro(CuentaDeProveedorDto ccDto, CuentaDeProveedorDtm ccDtm, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(ccDto, ccDtm, opciones);
            if (opciones.Insertando)
                ccDtm.IdCuenta =
                    ExtensorDeCuentasBancarias.Leer(Contexto, ccDto.Iban.Substring(0, 2), ccDto.Iban.Substring(2, 2), ccDto.Entidad, ccDto.Oficina, ccDto.DcCcc, ccDto.Numero).Id;
        }


        protected override void AntesDePersistir(CuentaDeProveedorDtm cp, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(cp, parametros);
            if (parametros.Insertando) cp.Activa = true;
            if (parametros.Operacion == enumTipoOperacion.Modificar)
            {
                cp.IdCuenta = ((CuentaDeProveedorDtm)parametros.registroEnBd).IdCuenta;
                if (cp.IdArchivo is null)
                    cp.IdArchivo = ((CuentaDeProveedorDtm)parametros.registroEnBd).IdArchivo;

                if (cp.SeHaModificadoElCampo<bool>(x => x.Name == nameof(CuentaDeProveedorDtm.Activa), parametros))
                    cp.IdArchivo = ((CuentaDeProveedorDtm)parametros.registroEnBd).IdArchivo;

                cp.Cuenta(Contexto).Validar();

            }
        }

        protected override void DespuesDePersistir(CuentaDeProveedorDtm cp, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(cp, parametros);

            if (parametros.Insertando)
                ((ProveedorDtm)cp.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha añadido una cuenta al proveedor", $"La cuenta {cp.Cuenta(Contexto).NumeroIban} ha sido añadido por el usuario {Contexto.DatosDeConexion.Login}");


            if (parametros.Modificando && cp.SeHaModificadoElCampo<bool>(x => x.Name == nameof(CuentaDeProveedorDtm.Activa), parametros))
                ((ProveedorDtm)cp.DetalleDe(Contexto)).CrearTraza(Contexto, $"{(cp.Activa ? "Activada " : "Desactivada")} la cuenta del proveedor", $"La cuenta {cp.Cuenta(Contexto).NumeroIban} ha sido {(cp.Activa ? "activada" : "desactivada")} por el usuario {Contexto.DatosDeConexion.Login}");

            if (parametros.Eliminando)
                ((ProveedorDtm)cp.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha eliminado la cuenta del proveedor", $"La cuenta {cp.Cuenta(Contexto).NumeroIban} ha sido eliminada por el usuario {Contexto.DatosDeConexion.Login}");

            if (cp.IdArchivo != default)
            {
                if (parametros.Insertando || (parametros.Modificando && !GestorDeVinculos.Existe(Contexto, enumNegocio.Proveedor, enumNegocio.Archivos, cp.IdElemento, (int)cp.IdArchivo)))
                {
                    cp.AsignarNombreAlCertificado(Contexto, enumNegocio.Proveedor, cp.IdElemento, parametros.Modificando ? (IUsaCuentaBancaria)parametros.registroEnBd : null);
                    ((ProveedorDtm)cp.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha añadido el certificado de cuenta", $"El certificado de cuenta {cp.Cuenta(Contexto).NumeroIban} ha sido añadido por el usuario {Contexto.DatosDeConexion.Login}");
                }

                if (parametros.Eliminando && GestorDeVinculos.Existe(Contexto, enumNegocio.Proveedor, enumNegocio.Archivos, cp.IdElemento, (int)cp.IdArchivo))
                {
                    ((ProveedorDtm)cp.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha eliminado la cuenta del proveedor", $"La cuenta {cp.Cuenta(Contexto).NumeroIban} ha sido eliminada por el usuario {Contexto.DatosDeConexion.Login}");
                    GestorDeVinculos.BorrarVinculo(Contexto, enumNegocio.Proveedor, enumNegocio.Archivos, cp.IdElemento, (int)cp.IdArchivo, new Dictionary<string, object>());
                }
            }
        }

        protected override void EliminarCaches(CuentaDeProveedorDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            ServicioDeCaches.EliminarCache(CacheDe.Pag_DatosDelPagoDto);
        }

        protected override void DespuesDeMapearElElemento(CuentaDeProveedorDtm cp, CuentaDeProveedorDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(cp, elemento, parametros);

            elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Proveedor, cp.DetalleDe(Contexto));
            cp.Cuenta = cp.Cuenta(Contexto);
            elemento.Iban = cp.Cuenta.IsoPais + cp.Cuenta.DcIban;
            elemento.NombreArchivo = cp.IdArchivo is null ? ltrCuentasBancarias.SinCertificado : Contexto.SeleccionarPorId<ArchivoDtm>((int)cp.IdArchivo).Expresion;
            elemento.Banco = cp.Cuenta.Banco(Contexto, errorSiNoHay: false).Expresion(Contexto);
            elemento.Elemento = Contexto.SeleccionarPorId<ProveedorDtm>(cp.IdElemento).Expresion;
        }

    }
}
