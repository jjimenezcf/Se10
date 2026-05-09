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
using System.IO;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeCuentasDeTrabajador : GestorDeElementos<ContextoSe, CuentaDeTrabajadorDtm, CuentaDeTrabajadorDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrCuentasDeTrabajador
        {
        }

        public class MapearCuentasDeTrabajador : Profile
        {
            public MapearCuentasDeTrabajador()
            {
                CreateMap<CuentaDeTrabajadorDtm, CuentaDeTrabajadorDto>()
                .ForMember(dto => dto.Entidad, x => x.MapFrom(dtm => dtm.Cuenta.Entidad))
                .ForMember(dto => dto.Oficina, x => x.MapFrom(dtm => dtm.Cuenta.Oficina))
                .ForMember(dto => dto.DcCcc, x => x.MapFrom(dtm => dtm.Cuenta.DcCcc))
                .ForMember(dto => dto.Numero, x => x.MapFrom(dtm => dtm.Cuenta.Numero));
                CreateMap<CuentaDeTrabajadorDto, CuentaDeTrabajadorDtm>()
                .ForMember(dtm => dtm.Cuenta, x => x.Ignore())
                .ForMember(dtm => dtm.Elemento, x => x.Ignore());
            }
        }

        public GestorDeCuentasDeTrabajador(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<CuentaDeTrabajadorDtm> AplicarJoins(IQueryable<CuentaDeTrabajadorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Cuenta);
            return consulta;
        }

        public static GestorDeCuentasDeTrabajador Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCuentasDeTrabajador(contexto, mapeador);
        }

        protected override void DespuesDeMapearElRegistro(CuentaDeTrabajadorDto ctDto, CuentaDeTrabajadorDtm ctDtm, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(ctDto, ctDtm, opciones);
            if (opciones.Insertando)
                ctDtm.IdCuenta =
                    ExtensorDeCuentasBancarias.Leer(Contexto, ctDto.Iban.Substring(0, 2), ctDto.Iban.Substring(2, 2), ctDto.Entidad, ctDto.Oficina, ctDto.DcCcc, ctDto.Numero).Id;

        }

        protected override void AntesDePersistir(CuentaDeTrabajadorDtm cp, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(cp, parametros);
            if (parametros.Insertando) cp.Activa = true;
            if (parametros.Operacion == enumTipoOperacion.Modificar)
            {
                cp.IdCuenta = ((CuentaDeTrabajadorDtm)parametros.registroEnBd).IdCuenta;
                if (cp.IdArchivo is null)
                    cp.IdArchivo = ((CuentaDeTrabajadorDtm)parametros.registroEnBd).IdArchivo;

                if (cp.SeHaModificadoElCampo<bool>(x => x.Name == nameof(CuentaDeTrabajadorDtm.Activa), parametros))
                    cp.IdArchivo = ((CuentaDeTrabajadorDtm)parametros.registroEnBd).IdArchivo;

                cp.Cuenta(Contexto).Validar();

            }
        }


        protected override void DespuesDePersistir(CuentaDeTrabajadorDtm ct, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(ct, parametros);

            if (parametros.Insertando)
                ((TrabajadorDtm)ct.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha añadido una cuenta al trabajador", $"La cuenta {ct.Cuenta(Contexto).NumeroIban} ha sido añadido por el usuario {Contexto.DatosDeConexion.Login}");


            if (parametros.Modificando && ct.SeHaModificadoElCampo<bool>(x => x.Name == nameof(CuentaDeTrabajadorDtm.Activa), parametros))
                ((TrabajadorDtm)ct.DetalleDe(Contexto)).CrearTraza(Contexto, $"{(ct.Activa ? "Activada " : "Desactivada")} la cuenta del trabajador", $"La cuenta {ct.Cuenta(Contexto).NumeroIban} ha sido {(ct.Activa ? "activada" : "desactivada")} por el usuario {Contexto.DatosDeConexion.Login}");

            if (parametros.Eliminando)
                ((TrabajadorDtm)ct.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha eliminado la cuenta del trabajador", $"La cuenta {ct.Cuenta(Contexto).NumeroIban} ha sido eliminada por el usuario {Contexto.DatosDeConexion.Login}");

            if (ct.IdArchivo != default)
            {
                if (parametros.Insertando || (parametros.Modificando && !GestorDeVinculos.Existe(Contexto, enumNegocio.Trabajador, enumNegocio.Archivos, ct.IdElemento, (int)ct.IdArchivo)))
                {
                    ct.AsignarNombreAlCertificado(Contexto, enumNegocio.Trabajador, ct.IdElemento, parametros.Modificando ? (IUsaCuentaBancaria)parametros.registroEnBd : null);
                    ((TrabajadorDtm)ct.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha añadido el certificado de cuenta", $"El certificado de cuenta {ct.Cuenta(Contexto).NumeroIban} ha sido añadido por el usuario {Contexto.DatosDeConexion.Login}");
                }

                if (parametros.Eliminando && GestorDeVinculos.Existe(Contexto, enumNegocio.Trabajador, enumNegocio.Archivos, ct.IdElemento, (int)ct.IdArchivo))
                {
                    ((TrabajadorDtm)ct.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha eliminado la cuenta del trabajador", $"La cuenta {ct.Cuenta(Contexto).NumeroIban} ha sido eliminada por el usuario {Contexto.DatosDeConexion.Login}");
                    GestorDeVinculos.BorrarVinculo(Contexto, enumNegocio.Trabajador, enumNegocio.Archivos, ct.IdElemento, (int)ct.IdArchivo, new Dictionary<string, object>());
                }
            }
        }

        protected override void EliminarCaches(CuentaDeTrabajadorDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            ServicioDeCaches.EliminarCache(CacheDe.Pag_DatosDelPagoDto);
        }

        protected override void DespuesDeMapearElElemento(CuentaDeTrabajadorDtm ci, CuentaDeTrabajadorDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(ci, elemento, parametros);

            elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Trabajador, ci.DetalleDe(Contexto));
            ci.Cuenta = ci.Cuenta(Contexto);
            elemento.Iban = ci.Cuenta.IsoPais + ci.Cuenta.DcIban;
            elemento.NombreArchivo = ci.IdArchivo is null ? ltrCuentasBancarias.SinCertificado : Contexto.SeleccionarPorId<ArchivoDtm>((int)ci.IdArchivo).Expresion;
            elemento.Banco = ci.Cuenta.Banco(Contexto, errorSiNoHay: false).Expresion(Contexto);
            elemento.Elemento = Contexto.SeleccionarPorId<TrabajadorDtm>(ci.IdElemento).Expresion;
        }

    }
}
